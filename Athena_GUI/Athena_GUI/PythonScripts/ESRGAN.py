import sys
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
cv2.cvtColor(img, cv2.COLOR_RGBA2RGB);

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
#------------------------------------------------------------------------------------------------------------------------
#scale = args.scale
print ("Attempting to Upscale Alpha Channel")
ori_img = cv2.imread(path,-1)
dst_img = cv2.imread(path,-1)

height, width, channels = ori_img.shape
height2, width2, channels2 = dst_img.shape
dst_img[:,:,:] = ori_img[:,:,:]
dst_img[:,:,0] = ori_img[:,:,3]
dst_img[:,:,1] = ori_img[:,:,3]
dst_img[:,:,2] = ori_img[:,:,3]

dst_img = cv2.cvtColor(dst_img, cv2.COLOR_RGBA2GRAY)
#cv2.imwrite(tempdir,dst_img)
print("Alpha Channel Loaded")
cv2.imwrite(os.path.join(output_folder, os.path.basename('alpha.png')), dst_img)
#ESRGAN
#path = tempdir
#model_path = args.model
#device = torch.device('cpu' if args.cpu else 'cuda')

#output_folder = os.path.normpath(args.output)

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
img = dst_img
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

alphaImage = model(img_LR).data.squeeze(0).float().cpu().clamp_(0, 1).numpy()
if alphaImage.shape[0] == 3:
    alphaImage = alphaImage[[2, 1, 0], :, :]
elif alphaImage.shape[0] == 4:
    alphaImage = alphaImage[[2, 1, 0, 3], :, :]
alphaImage = np.transpose(alphaImage, (1, 2, 0))
alphaImage = (alphaImage * 255.0).round()
#print(os.path.join(output_folder, os.path.basename(path)))
#cv2.imwrite(tempdir, alphaImage)
print("Finished Processing Alpha Channel")

#rgb_image = # oriignal converted image

#alpha_image = output
#output_folder = os.path.join(args.output, os.path.basename(args.input))

#rgb_image = cv2.imread(rgb_image,-1)
#alpha_image = cv2.imread(alpha_image,-1)

cv2.imwrite(os.path.join(output_folder, os.path.basename('color_Upscaled.png')), esrganImage)

esrganImage = cv2.cvtColor(esrganImage, cv2.COLOR_RGB2RGBA)

height, width, channels = alphaImage.shape
height2, width2, channels2 = esrganImage.shape      
esrganImage[:,:,3] = alphaImage[:,:,2]

alphaImage = cv2.convertScaleAbs(alphaImage, alpha=2.0, beta=50)

cv2.imwrite(os.path.join(output_folder, os.path.basename('alpha_Upscaled.png')), alphaImage)
cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), esrganImage)

#------------------------------------------------------------------------------------------------------------------------



#cv2.imwrite(os.path.join(output_folder, os.path.basename(path)), esrganImage)
print("Complete")
