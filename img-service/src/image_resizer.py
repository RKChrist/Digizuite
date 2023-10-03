import os, sys, io
from PIL import Image


class ImageResizer:

    def __init__(self) -> None:
        pass

    def resize(self, img_path, height, width) -> None:
        
        size = height, width

    
        outfile = os.path.splitext(img_path)[0] + ".jpeg"
        if img_path != outfile:
            try:
                im = Image.open(img_path)
                im.thumbnail(size, Image.Resampling.LANCZOS)
                im.save(outfile, "JPEG")
            except IOError as error:
                print("cannot create thumbnailfor '{img_path}'")
                print(error)
    
    def resize_from_bytes(self, img_data, height, width):
        size = height, width
        im = Image.open(io.BytesIO(img_data))
        height = im.height
        width = im.width
        print(f'image width: {width} - image height: {height}')
        
        im.thumbnail(size, Image.Resampling.LANCZOS)
        im.save("image.jpg", "jpeg")
        return im

