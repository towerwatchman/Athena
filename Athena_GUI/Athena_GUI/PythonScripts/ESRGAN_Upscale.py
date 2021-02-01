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
parser.add_argument('--model')
parser.add_argument('--model2')
parser.add_argument('--cpu', action='store_true', help='Use CPU instead of CUDA')
args = parser.parse_args()

img_path = os.path.join(os.path.normpath(args.input))
#print(img_path)
img_name = os.path.basename(args.input);
#print(img_name)
#output_folder = os.path.join(args.output, os.path.basename(args.input))
dst_img = os.path.join(args.output, os.path.basename(args.input))
#print("Opening Image");
#print(dst_img)
img = cv2.imread(img_path, cv2.IMREAD_COLOR)
cv2.cvtColor(img, cv2.COLOR_RGBA2RGB);
#print("Removeing Alpha Channel")
#cv2.imwrite(dst_img, img)
print("Alppha Channel Removed")

#ESRGAN 1st ITTERATION
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

#img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
print(img.dtype)
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
#cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), output)
print("ESRGAN Complete #1")

height, width, channels = output.shape
width = width*.5
height = height *.5
print(height)
print(width)
img = cv2.resize(output,(int(width),int(height)))
#cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), img)
img = cv2.convertScaleAbs(img)
print("Image Scaled #1")

#ESRGAN 2nd ITTERATION
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
model_path = args.model2
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

#img = cv2.imread(img, cv2.IMREAD_UNCHANGED)
print(img.dtype)

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
#cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), output)
print("ESRGAN Complete #2")

#------------------------Image downscale #2
height, width, channels = output.shape
width = width*.5
height = height *.5
print(height)
print(width)
img = cv2.resize(output,(int(width),int(height)))
img = cv2.convertScaleAbs(img)
print("Image Scaled #2")

#-------------------------Alpha Upscale
ori_img = os.path.join(os.path.normpath(args.input))
print(ori_img)
dst_img = os.path.join(os.path.normpath(args.input))
print(dst_img)
output_folder = os.path.join(args.output, os.path.basename(args.input))
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

#ESRGAN
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
dst_img = dst_img * 1. / np.iinfo(dst_img.dtype).max

if dst_img.ndim == 2:
    dst_img = np.tile(np.expand_dims(dst_img, axis=2), (1, 1, min(in_nc, 3)))
if dst_img.shape[2] > in_nc: # remove extra channels
    #print('Warning: Truncating image channels')
    dst_img = dst_img[:, :, :in_nc]
elif dst_img.shape[2] == 3 and in_nc == 4: # pad with solid alpha channel
    dst_img = np.dstack((dst_img, np.full(dst_img.shape[:-1], 1.)))

if dst_img.shape[2] == 3:
    dst_img = dst_img[:, :, [2, 1, 0]]
elif dst_img.shape[2] == 4:
    dst_img = dst_img[:, :, [2, 1, 0, 3]]
dst_img = torch.from_numpy(np.transpose(dst_img, (2, 0, 1))).float()
img_LR = dst_img.unsqueeze(0)
img_LR = img_LR.to(device)

output = model(img_LR).data.squeeze(0).float().cpu().clamp_(0, 1).numpy()
if output.shape[0] == 3:
    output = output[[2, 1, 0], :, :]
elif output.shape[0] == 4:
    output = output[[2, 1, 0, 3], :, :]
output = np.transpose(output, (1, 2, 0))
output = (output * 255.0).round()
#print(os.path.join(output_folder, os.path.basename(path)))
print("Finished Processing Alpha Channel")

#------------------------Image downscale #3
height, width, channels = output.shape
width = width*.5
height = height *.5
print(height)
print(width)
alpha_image = cv2.resize(output,(int(width),int(height)))
alpha_image = cv2.convertScaleAbs(alpha_image)
print("Image Scaled #3")

rgb_image = img
alpha_image = output
output_folder = os.path.join(args.output, os.path.basename(args.input))

rgb_image = cv2.cvtColor(rgb_image, cv2.COLOR_RGB2RGBA)

height, width, channels = alpha_image.shape
height2, width2, channels2 = rgb_image.shape      
rgb_image[:,:,3] = alpha_image[:,:,2]
cv2.imwrite(output_folder,rgb_image)








