# CoNNiUS - Convolutional Neural Network Image UpScaler
* Development Stage: Alpha 
* Current Version: 0.6
* [Releases](https://github.com/towerwatchman/CoNNiUS/releases)

## Description
CoNNiUS is Windows only GUI that uses a Python wrapper for [ESRGAN](https://github.com/xinntao/ESRGAN) with additional features.

## Features
* Uses Python libraries to upscale images using ESRGAN scripts. `COMPLETE` 
* Threading for batch image processing `COMPLETE` 
* Suports images with alpha channels. `IN PROGRESS`
* Supports image compression png compression (8bit). `NOT STARTED`
* Can output to the DDS (BC1-BC7) file format. `NOT STARTED`
* Accepts Jpeg files. `NOT STARTED`
* Parse images exported from Dolphin for upscale `IN PROGRESS`
* Parse images exported from PCSX2 `NOT STARTED`
* Ability to install missing packages from the GUI `IN PROGRESS`

## Requirements
- [Python 3.7.x](https://www.python.org/downloads/)
- [Pytourch](https://pytorch.org/get-started/locally/)
- OpenCV (Can be installed via GUI)
- Numpy (Can be installed via GUI)
- Windows 8 or 10
- NVIDA or Intel Graphics Card. AMD not supported currently.

## How To Uses
Once all requiremnts are installed, open the Settings.ini file and add the folder for where Python was installed. If you would like to overide the GPU, add Nvidia or Intel to the "GPU=" line