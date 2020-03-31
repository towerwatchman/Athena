import cv2
import numpy
import os.path
import argparse
from PIL import Image

parser = argparse.ArgumentParser()
parser.add_argument('--input', default='C:\ctp\esrgan\LR', help='Input folder')
parser.add_argument('--output', default='C:\ctp\esrgan\results', help='Output folder')
parser.add_argument('--original', default='', help='')
parser.add_argument('--scale', default=4, help='Scale for output')
args = parser.parse_args()

scale = args.scale
ori = os.path.join(args.original,os.path.basename(args.input))
print(ori)
dst = args.input
print(dst)
output_folder = os.path.join(args.output, os.path.basename(args.input))
print(output_folder)
#print(output_folder)

ori_img = Image.open(ori).convert('RGBA')
dst_img = Image.open(dst).convert('RGBA')

width, height = ori_img.size
for j in range(0, height):
    for i in range(0, width):
        p_ori = ori_img.getpixel((i, j))

        for i_s in range(i * 4, i * 4 + 4):
            for j_s in range(j * 4, j*4 + 4):
                p_dst = dst_img.getpixel((i_s, j_s))  
				
                # copy alpha from orignal
                p_dst = list (p_dst)
                p_ori = list(p_ori)
                #print(p_dst)
                p_dst[3] = p_ori[3]
                p_dst = tuple(p_dst)
                dst_img.putpixel((i_s, j_s), p_dst)

dst_img.save(output_folder)
print("Complete")
