﻿import sys
import os.path
import glob
import cv2
import numpy as np
import torch
import architecture as arch
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--model')
parser.add_argument('--input', default='LR', help='Input Image')
parser.add_argument('--output', default='results', help='Output folder')
parser.add_argument('--cpu', action='store_true', help='Use CPU instead of CUDA')
args = parser.parse_args()

if not os.path.exists(args.model):
    print('Error: Model [{:s}] does not exist.'.format(args.model))
    sys.exit(1)
elif not os.path.isfile(args.input):
    print('Error: File [{:s}] does not exist.'.format(args.input))
    sys.exit(1)
elif os.path.isfile(args.output):
    print('Error: Folder [{:s}] is a file.'.format(args.output))
    sys.exit(1)
elif not os.path.exists(args.output):
    os.mkdir(args.output)

path = args.input
print('Loaded',os.path.basename(path),'for Conversion');
model_path = args.model
device = torch.device('cpu' if args.cpu else 'cuda')
print('Using',device,'for Torch');

output_folder = os.path.normpath(args.output)

state_dict = torch.load(model_path)

# Remove Alpha Channel
print('Removing Alpha Channel');
img = cv2.imread(path, cv2.IMREAD_COLOR)
img = cv2.cvtColor(img, cv2.COLOR_RGBA2RGB);

if 'conv_first.weight' in state_dict:
    print('Error: Attempted to load a new-format model')
    sys.exit(1)

# extract model information
scale2 = 0
max_part = 0
for part in list(state_dict):
    parts = part.split('.')
    n_parts = len(parts)
    if n_parts == 5 and parts[2] == 'sub':
        nb = int(parts[3])
    elif n_parts == 3:
        part_num = int(parts[1])
        if part_num > 6 and parts[2] == 'weight':
            scale2 += 1
        if part_num > max_part:
            max_part = part_num
            out_nc = state_dict[part].shape[0]
upscale = 2 ** scale2
in_nc = state_dict['model.0.weight'].shape[1]
nf = state_dict['model.0.weight'].shape[0]

model = arch.RRDB_Net(in_nc, out_nc, nf, nb, gc=32, upscale=upscale, norm_type=None, act_type='leakyrelu', \
                        mode='CNA', res_scale=1, upsample_mode='upconv')
model.load_state_dict(state_dict, strict=True)
del state_dict
model.eval()
for k, v in model.named_parameters():
    v.requires_grad = False
model = model.to(device)

print('Using Model: {:s} \nConverting...'.format(os.path.basename(model_path)))

# read image
#img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
img = img * 1. / np.iinfo(img.dtype).max

if img.ndim == 2:
    img = np.tile(np.expand_dims(img, axis=2), (1, 1, min(in_nc, 3)))
if img.shape[2] > in_nc: # remove extra channels
    print('Warning: Truncating image channels')
    img = img[:, :, :in_nc]
elif img.shape[2] == 3 and in_nc == 4: # pad with solid alpha channel
    img = np.dstack((img, np.full(img.shape[:-1], 1.)))

if img.shape[2] == 3:
    img = img[:, :, [2, 1, 0]]
elif img.shape[2] == 4:
    img = img[:, :, [2, 1, 0, 3]]
img = torch.from_numpy(np.transpose(img, (2, 0, 1))).float()
img_LR = img.unsqueeze(0)
img_LR = img_LR.to(device)

esrganImage = model(img_LR).data.squeeze(0).float().cpu().clamp_(0, 1).numpy()
if esrganImage.shape[0] == 3:
    esrganImage = esrganImage[[2, 1, 0], :, :]
elif esrganImage.shape[0] == 4:
    esrganImage = esrganImage[[2, 1, 0, 3], :, :]
esrganImage = np.transpose(esrganImage, (1, 2, 0))
esrganImage = (esrganImage * 255.0).round()

print("Image Upscaled");

#esrganImage = cv2.cvtColor(esrganImage, cv2.COLOR_RGB2RGBA)
   
cv2.imwrite(os.path.join(output_folder, "preview.png"), esrganImage)

#------------------------------------------------------------------------------------------------------------------------
#cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), esrganImage)
print("Complete")
