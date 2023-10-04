import argparse
from image_resizer import ImageResizer


resizer = ImageResizer()

# the point of using argparser was to make type check on parameters, but i didn't quite figure out how to use that option

parser = argparse.ArgumentParser()
parser.add_argument("path")
# parser.add_argument("-p", help="path to your image file goes here", action='store_true')
parser.add_argument("width")
# parser.add_argument("-x", help="desired witdh goes here", action='store_true', type=int)
parser.add_argument("height")
# parser.add_argument("-y", help="desired witdh goes here", action='store_true', type=int)
print("yay")
args = parser.parse_args()





args = parser.parse_args()
print(args)
img_path = args.path
img_height = int(args.height)
img_width = int(args.width)
resizer.resize(img_path=img_path, height=img_height, width=img_height)