Program Details
    - Open a folder with images and upscale them with ESRGAN + model

** Image Types

** Buttons

** Functions

** Specific functions
* Gamecube

* Playstation

* Other

** TODO
    - Need to refactor python to have one script to remove alpha, convert image with esrgan, upscale alpha, and merge the two. also need to use an 8-bit image compressor to reduce file size.
    - pngquant Image Reducer
    - numpngw 0.0.8


    from wand import image
with image.Image(filename="white_rect_dxt3.dds") as img:
    img.compression = "no"
    img.save(filename="white_rect_dxt3.png")
And same from .png to .dds

from wand import image
with image.Image(filename='white_rect.png') as img:
    img.compression = "dxt3"
    img.save(filename='white_rect_dxt3.dds')