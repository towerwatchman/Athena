﻿import sys
import os.path
import glob
import cv2
import numpy as np
import torch
import architecture as arch
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('model')
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
model_path = args.model
device = torch.device('cpu' if args.cpu else 'cuda')

output_folder = os.path.normpath(args.output)

state_dict = torch.load(model_path)

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

#print('Model path {:s}. \nTesting...'.format(model_path))

# read image
img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
print(img.shape)
img = img * 1. / np.iinfo(img.dtype).max

if img.ndim == 2:
    img = np.tile(np.expand_dims(img, axis=2), (1, 1, min(in_nc, 3)))
    print('00'
if img.shape[2] > in_nc: # remove extra channels
    print('01')
    img = img[:, :, :in_nc]
elif img.shape[2] == 3 and in_nc == 4: # pad with solid alpha channel
    img = np.dstack((img, np.full(img.shape[:-1], 1.)))
    print('11')
