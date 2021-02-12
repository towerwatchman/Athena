import sys
import os.path
import cv2
import numpy as np
import argparse
 
parser = argparse.ArgumentParser()
parser.add_argument('--input', default='C:\ctp\esrgan\LR', help='Input folder')
parser.add_argument('--output', default='C:\ctp\esrgan\results', help='Output folder')
args = parser.parse_args()

imgsrc_path = args.input
img_name = os.path.basename(os.path.splitext(args.input)[0])
dst_path = args.output

img = cv2.imread(imgsrc_path, cv2.IMREAD_UNCHANGED)
img_width = img.shape[0]
count = 1
while img_width > 1:
	scale_percent = 50 # percent of original size
	width = int(img.shape[1] * scale_percent / 100)
	height = int(img.shape[0] * scale_percent / 100)	
	#check for size of 2
	if width == 1:
		height = 1
	if height == 1:
		width = 1
	dim = (width, height)
	# resize image
	img = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
	img_width = width
	#cv2.imwrite(os.path.join(dst_path, img_name), resized)
	cv2.imwrite(os.path.join(dst_path, img_name + "_mip" + str(count) + ".png"), img)
	print("Generated mipmap: " + img_name + "_mip" + str(count) + ".png")
	count = count + 1

	




