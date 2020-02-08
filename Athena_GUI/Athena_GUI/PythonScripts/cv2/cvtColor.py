import sys
import os.path
import cv2
import numpy as np
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--input', default='C:\ctp\esrgan\LR', help='Input folder')
parser.add_argument('--output', default='C:\ctp\esrgan\results', help='Output folder')
args = parser.parse_args()

img_path = os.path.join(os.path.normpath(args.input))
img_name = os.path.basename(args.input);
print(img_path)
#output_folder = os.path.join(args.output, os.path.basename(args.input))
dst_img = os.path.join(args.output, os.path.basename(args.input))
print(dst_img)
img = cv2.imread(img_path, cv2.IMREAD_COLOR)

cv2.cvtColor(img, cv2.COLOR_RGBA2RGB);

cv2.imwrite(dst_img, img)
