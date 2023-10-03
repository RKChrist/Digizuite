import os, sys
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
            except IOError:
                print("cannot create thumbnailfor '{infile}'")
