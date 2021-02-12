import sys
import os.path
import glob
import cv2
import numpy as np
import torch
import architecture as arch
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--input', default='C:\ctp\esrgan\LR', help='Input folder')
parser.add_argument('--output', default='C:\ctp\esrgan\results', help='Output folder')
parser.add_argument('--original', default='', help='')
parser.add_argument('--scale', default=4, help='Scale for output')
parser.add_argument('--model')
parser.add_argument('--tempdir')
parser.add_argument('--cpu', action='store_true', help='Use CPU instead of CUDA')
parser.add_argument('--rgbdir')
args = parser.parse_args()

scale = args.scale
ori_img = os.path.join(args.original,os.path.basename(args.input))
print(ori_img)
dst_img = os.path.join(args.original,os.path.basename(args.input))
print(dst_img)
output_folder = os.path.join(args.output, os.path.basename(args.input))
tempdir = os.path.join(args.tempdir, os.path.basename(args.input))
#print(output_folder)

ori_img = cv2.imread(ori_img,-1)
dst_img = cv2.imread(dst_img,-1)

height, width, channels = ori_img.shape
height2, width2, channels2 = dst_img.shape
#ori_img = cv2.resize(ori_img,(width*scale,height*scale))
dst_img[:,:,:] = ori_img[:,:,:]
dst_img[:,:,0] = ori_img[:,:,3]
dst_img[:,:,1] = ori_img[:,:,3]
dst_img[:,:,2] = ori_img[:,:,3]

dst_img = cv2.cvtColor(dst_img, cv2.COLOR_RGBA2RGB)
ori_img = cv2.cvtColor(ori_img, cv2.COLOR_RGBA2RGB)
#print(tempdir)
cv2.imwrite(tempdir,dst_img)
print("Removed Alpha Channel")

#ESRGAN
path = tempdir
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
img = img * 1. / np.iinfo(img.dtype).max

if img.ndim == 2:
    img = np.tile(np.expand_dims(img, axis=2), (1, 1, min(in_nc, 3)))
if img.shape[2] > in_nc: # remove extra channels
    #print('Warning: Truncating image channels')
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

output = model(img_LR).data.squeeze(0).float().cpu().clamp_(0, 1).numpy()
if output.shape[0] == 3:
    output = output[[2, 1, 0], :, :]
elif output.shape[0] == 4:
    output = output[[2, 1, 0, 3], :, :]
output = np.transpose(output, (1, 2, 0))
output = (output * 255.0).round()
#print(os.path.join(output_folder, os.path.basename(path)))
cv2.imwrite(tempdir, output)
print("Finished Processing Alpha Channel")

rgb_image = os.path.join(args.rgbdir,os.path.basename(args.input))
print(rgb_image)
alpha_image = tempdir
output_folder = os.path.join(args.output, os.path.basename(args.input))

rgb_image = cv2.imread(rgb_image,-1)
alpha_image = cv2.imread(alpha_image,-1)

rgb_image = cv2.cvtColor(rgb_image, cv2.COLOR_RGB2RGBA)

height, width, channels = alpha_image.shape
height2, width2, channels2 = rgb_image.shape      
rgb_image[:,:,3] = alpha_image[:,:,2]
cv2.imwrite(output_folder,rgb_image)


