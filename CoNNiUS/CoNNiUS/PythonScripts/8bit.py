import sys
import os.path
import glob
import cv2
import numpy as np
from numpngw import write_png
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--input', default='LR', help='Input Image')
parser.add_argument('--output', default='results', help='Output folder')

# Example 4
#
# Create an 8-bit indexed RGB image that uses a palette.

img = cv2.imread(path, cv2.IMREAD_UNCHANGED)

img_width = 300
img_height = 200
img = np.zeros((img_height, img_width, 3), dtype=np.uint8)

height, width, channels = img.shape
img_width
img_height

np.random.seed(222)
for _ in range(40):
    width = np.random.randint(5, img_width // 5)
    height = np.random.randint(5, img_height // 5)
    row = np.random.randint(5, img_height - height - 5)
    col = np.random.randint(5, img_width - width - 5)
    color = np.random.randint(80, 256, size=2)
    img[row:row+height, col:col+width, 1:] = color

write_png('example4.png', img, use_palette=True)