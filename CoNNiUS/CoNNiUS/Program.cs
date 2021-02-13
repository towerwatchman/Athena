using PythonWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Connius.Class;

namespace Connius
{
    class Program
    {
        #region PRIVATE
        //private Class designations
        private Parser settingsParser = new Parser();
        //private Wrapper pythonWrapper = new Wrapper();

        //private variables
        private static List<string> ImageExtensions = new List<string> { ".PNG", ".JPG" };
        #endregion

        #region PUBLIC
        //public variables
        public int ImageFileCount = 0;
        #endregion

        public void Init()
        {
            //LoadingScreen loadingScreen = new LoadingScreen();
            //loadingScreen.Show();

            //REFACTOR
            #region LOAD SETTINGS
            //lOAD SETTINGS
            settingsParser.ReadSettings();
            #endregion

            #region INITIAIZE FOLDER LOCATIONS
            Settings.Directory.ImageDirectory = Directory.GetCurrentDirectory() + @"\Resources\Images";
            Settings.Directory.ModelDirectory = Directory.GetCurrentDirectory() + @"\Resources\Models";
            Settings.Directory.OutputDirectory = Directory.GetCurrentDirectory() + @"\Complete";
            #endregion

            #region CREATE FOLDER STRUCTURE

            //Check if temporary folders exist. If it does not then create them
            string Path = Directory.GetCurrentDirectory();

            //Create Models directory
            if (!Directory.Exists(Path + @"\Resources\Models"))
                Directory.CreateDirectory(Path + @"\Resources\Models");

            //Base folder where images files will be stored
            if (!Directory.Exists(Path + @"\Resources\Images"))
                Directory.CreateDirectory(Path + @"\Resources\Images");

            //Base folder where Completed files will be stored
            if (!Directory.Exists(Path + @"\Complete"))
                Directory.CreateDirectory(Path + @"\Complete");

            #endregion

            #region CHECK PYTHON DIRECTORY
            Settings.ResourceChecker.IsPythonInstalled = Directory.Exists(Settings.Directory.PythonDirectory) ? true : false;
            //pythonWrapper.PythonDir = Settings.Directory.PythonDirectory;
            #endregion

            #region CHECK PYTORCH
            Settings.ResourceChecker.IsTorchInstalled = Directory.Exists(Settings.Directory.PythonDirectory + @"\Lib\site-packages\torch") ? true : false;
            #endregion

            #region CHECK CV2
            Settings.ResourceChecker.IsCv2Installed = Directory.Exists(Settings.Directory.PythonDirectory + @"\Lib\site-packages\cv2") ? true : false;
            #endregion

            #region CHECK NUMPY
            Settings.ResourceChecker.IsNumpyInstalled = Directory.Exists(Settings.Directory.PythonDirectory + @"\Lib\site-packages\numpy") ? true : false;
            #endregion

            #region CHECK PNGQUANT
            Settings.ResourceChecker.IsPngQuantInstalled = Directory.Exists(Settings.Directory.PythonDirectory + @"\Lib\site-packages\pngquant") ? true : false;
            #endregion

            #region CHECK WIND
            Settings.ResourceChecker.IsWandInstalled = Directory.Exists(Settings.Directory.PythonDirectory + @"\Lib\site-packages\wand") ? true : false;
            #endregion

            //Check for available Video Cards and enable either the NVIDIA or Intel
            if (Settings.GPU.ActiveGPU == "")
            {
                GetGPUInfo();
            }
        }
        public void GetGPUInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["Name"].ToString().Contains("NVIDIA"))
                    {
                        Settings.GPU.IsNVIDIAGPUAvailable = true;
                        Settings.GPU.NvidiaGPUName = obj["Name"].ToString();
                    }
                    else if (obj["Name"].ToString().Contains("AMD") || obj["Name"].ToString().Contains("Radeon"))
                    {
                        Settings.GPU.IsAMDGPUAvailable = true;
                        Settings.GPU.AMDGPUName = obj["Name"].ToString();
                    }
                    else if (obj["Name"].ToString().Contains("Intel"))
                    {
                        Settings.GPU.IsIntelGpuAvailable = true;
                        Settings.GPU.IntelGPUName = obj["Name"].ToString();
                    }

                    //Console.Out.WriteLine(obj["Name"].ToString());
                }
            }

            //Set the GPU based on information from VideoController
            if (Settings.GPU.IsNVIDIAGPUAvailable == true)
            {
                Settings.GPU.ActiveGPU = Settings.GPU.NvidiaGPUName;
                Settings.GPU.IsCudaEnabled = true;
            }
            else if (Settings.GPU.IsIntelGpuAvailable == true && Settings.GPU.IsNVIDIAGPUAvailable == false)
            {
                Settings.GPU.ActiveGPU = Settings.GPU.IntelGPUName;
                Settings.GPU.IsCudaEnabled = false;
            }
            else //if nothing found, assume AMD
            {
                Settings.GPU.ActiveGPU = Settings.GPU.AMDGPUName;
                Settings.GPU.IsCudaEnabled = false;
            }
        }

        public void GetImageFiles(ListView listView, string folder)
        {
            #region Get Images from Folder
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                FileInfo[] files = directoryInfo.GetFiles();
                System.Drawing.Image img;

                foreach (var file in files)
                {
                    string f_name = file.Name;
                    img = System.Drawing.Image.FromFile(file.FullName);
                    string ImageSize = "";
                    if (f_name.Contains("tex1"))//for Dolphin Textures
                    {
                        string[] temp = f_name.Split('_');

                        //add check for dolphin
                        if (temp[2] == "m")//check for mipmaps in dolphin texture
                        {
                            if (f_name.Contains("mip"))
                            {
                                if (!Directory.Exists(folder + @"\Mipmap"))
                                    Directory.CreateDirectory(folder + @"\Mipmap");

                                File.Move(file.FullName, folder + @"\Mipmap\" + f_name);
                            }

                            ImageSize = img.Width + "x" + img.Height;
                            listView.Dispatcher.BeginInvoke((Action)(() =>
                                listView.Items.Add(new ImageClass { Name = f_name, ImgSize = ImageSize, mipmap = "Yes" })));
                        }
                        /*else if(temp[temp.Count() -1].Contains("6")) //Check for video frames from dolphin texture
                        {
                            if (!Directory.Exists(folder + @"\Video"))
                                Directory.CreateDirectory(folder + @"\Video");

                            File.Move(file.FullName, folder + @"\Video\" + f_name);
                        }*/
                        else
                        {

                            ImageSize = img.Width + "x" + img.Height;
                            listView.Dispatcher.BeginInvoke((Action)(() => listView.Items.Add(new ImageClass { Name = f_name, ImgSize = ImageSize, mipmap = "No" })));
                        }
                    }
                    else //for all other types add to the array of items
                    {
                        /// *** Need to add check to image file type as listed in settings file.
                        /// *** Right now this will only work with *.png files.
                        /// *** REFACTOR 

                        ImageSize = img.Width + "x" + img.Height;
                        if (ImageExtensions.Contains(Path.GetExtension(file.FullName).ToUpperInvariant()))
                        {
                            listView.Dispatcher.BeginInvoke((Action)(() =>
                                listView.Items.Add(new ImageClass { Name = f_name, ImgSize = ImageSize, mipmap = "No" })));
                        }
                    }
                }
                ImageFileCount = files.Count();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
            #endregion
        }

        public void GetModels(ComboBox comboBox, string folder)
        {
            //clear existing models
            comboBox.Dispatcher.BeginInvoke((Action)(() => comboBox.Items.Clear()));

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                FileInfo[] files = directoryInfo.GetFiles("*.pth");

                foreach (var file in files)
                {
                    comboBox.Dispatcher.BeginInvoke((Action)(() => comboBox.Items.Add(file.Name)));
                }
                comboBox.Dispatcher.BeginInvoke((Action)(() => comboBox.SelectedIndex = 0));//show first item in list
            }
            catch
            {
                MessageBox.Show("Unable to Load Models");
            }
        }
    }
}
