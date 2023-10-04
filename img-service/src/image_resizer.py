import os, sys, io
from PIL import Image


class ImageResizer:

    def __init__(self) -> None:
        pass


    def resize_from_bytes(self, img_data, height, width, file_extension):
        size = height, width
        im = Image.open(io.BytesIO(img_data))
        height = im.height
        width = im.width
        print(f'image width: {width} - image height: {height}')
        
        im.thumbnail(size, Image.Resampling.LANCZOS)
        im.save(f"/usr/output_images/image.{file_extension}", "jpeg")
        return im


