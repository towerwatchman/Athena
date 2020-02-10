# Athena
Esrgan GUI for Windows 10 (.NET 4.5)

This program is meant to create a more seamless workflow when using an ESRGAN package. The GUI interfaces a DLL that has mutiple OpenCV command built in.

If an image has an alpha channel the program will remove the alpha channel before using ESRGAN and then adds the scalled alpha channel back to the new image once completed.

Currently using the ESRGAN GitHub Repo (fork from BlueAmulet) but I plan to move to https://github.com/open-mmlab/mmsr eventually when I have time

In order to use this progarm you will need the following already installed:
- Python 3.7 or 3.8
- Pytourch
- OpenCV
- The following folder structure "C:\etp\esrgan"
- A copy of ESRGAN I am currently using the ESRGAN GitHub Repo (fork from BlueAmulet).
- Once downloaded copy run.py to the "C:\etp\esrgan" folder. I will fix this eventually.

I recommend following this guide to installing all of the items listed above.
https://upscale.wiki/wiki/ESRGAN_Installation_Guide_for_Windows




