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
print(img_path)
img_name = os.path.basename(args.input);
print(img_name)
#output_folder = os.path.join(args.output, os.path.basename(args.input))
dst_img = os.path.join(args.output, os.path.basename(args.input))
print("Opening Image");
print(dst_img)
img = cv2.imread(img_path, cv2.IMREAD_COLOR)
cv2.cvtColor(img, cv2.COLOR_RGBA2RGB);
print("Removeing Alpha Channel")
cv2.imwrite(dst_img, img)
print("Saving Image")