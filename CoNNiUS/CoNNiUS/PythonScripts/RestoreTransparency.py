import cv2
import numpy
import os.path
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--input', default='C:\ctp\esrgan\LR', help='Input folder')
parser.add_argument('--output', default='C:\ctp\esrgan\results', help='Output folder')
parser.add_argument('--original', default='', help='')
parser.add_argument('--scale', default=4, help='Scale for output')
args = parser.parse_args()

scale = args.scale
ori_img = os.path.join(args.original,os.path.basename(args.input))
#print(ori_img)
dst_img = args.input
#print(dst_img)
output_folder = os.path.join(args.output, os.path.basename(args.input))
#print(output_folder)

ori_img = cv2.imread(ori_img,-1)
dst_img = cv2.imread(dst_img,-1)
dst_img = cv2.cvtColor(dst_img, cv2.COLOR_RGB2RGBA)
height, width, channels = ori_img.shape
height2, width2, channels2 = dst_img.shape
ori_img = cv2.resize(ori_img,(width*scale,height*scale))       
dst_img[:,:,3] = ori_img[:,:,3]
cv2.imwrite(output_folder,dst_img)
print("Complete")
