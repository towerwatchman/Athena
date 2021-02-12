using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using PythonWrapper;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management;

namespace Connius
{
    public partial class MainWindow : Window
    {

        private Wrapper pythonWrapper = new Wrapper();
        private SettingsParser settingsParser = new SettingsParser();
        private LoadingScreen Screen = new LoadingScreen();

        //Global Variables
        private string RBGDir = Directory.GetCurrentDirectory() + @"\Resources\Temp\RGB";
        private string AlphaDir = Directory.GetCurrentDirectory() + @"\Resources\Alpha";
        private string ESRGANOUTDIR = Directory.GetCurrentDirectory() + @"\Resources\Temp\ESRGAN_RGB";
        private string ImageDir = Directory.GetCurrentDirectory() + @"\Resources\Images";
        private string CompleteDIR = Directory.GetCurrentDirectory() + @"\Complete";
        private string CurrDir = Directory.GetCurrentDirectory();
        private string GPU = "";
        private int fileCount = 0;
        private static List<string> ImageExtensions = new List<string> { ".PNG", ".JPG" };
        bool ModelTestMode = false;

        //Threading Stuff
        private int count = 0;
        private int AlphaComplete = 0;
        private int ESRGANComplete = 0;
        private int FinalComplete = 0;
        private string CurrentModel = "";

        private int ActiveTasks = 0;

        public MainWindow()
        {
            //Screen.Show(); //show loading screen
            Init();
            InitializeComponent();
            //Screen.Hide();
            PopulateWPF();
        }

        private void PopulateWPF()
        {

            #region DISABLE GPU LABELS
            lbNVIDIA.Visibility = Visibility.Hidden;
            //lbAMD.Visibility = Visibility.Hidden;
            //lbIntel.Visibility = Visibility.Hidden;
            #endregion
            if (Settings.LastAccessedFolder != "")
            {
                ImageDir = CurrDir + @"\Resources\images";
            }

            //list all files in image folder folder        
            GetImageFiles(ImageDir);
            tb_Input_Directory.Text = ImageDir;

            //list all models in esrgan folder
            GetModels(CurrDir + @"\Resources\models");

            //Find GPU information
            if (Settings.GPU != "")
            {

            }
            else
            {
                GetGPUInfo();
            }

            if (Settings.NVIDIAGPUAvailable == true)
            {
                lbNVIDIA.Visibility = Visibility.Visible;
                lbNVIDIA.Foreground = Brushes.Green;
                Settings.GPU = "NVIDIA";
            }

            lbPytorch.Foreground = Settings.TorchInstalled == true ? Brushes.Green : Brushes.Red;
            lbPytorch.Content = Settings.TorchInstalled == true ? "Installed" : "Not Installed";
            lbCV2.Foreground = Settings.Cv2Installed == true ? Brushes.Green : Brushes.Red;
            lbCV2.Content = Settings.Cv2Installed == true ? "Installed" : "Not Installed";
            lbPython.Foreground = Settings.PythonInstalled == true ? Brushes.Green : Brushes.Red;
            lbPython.Content = Settings.PythonInstalled == true ? "Installed" : "Not Installed";
        }

        private void Init()
        {
            Screen.pb_loading.Value = 0;

            #region LOAD SETTINGS
            //lOAD SETTINGS
            settingsParser.ReadSettings();
            #endregion
            Screen.pb_loading.Value = 10;

            #region CREATE FOLDER STRUCTURE

            //Check if temporary folders exist. If it does not then create them
            string Path = Directory.GetCurrentDirectory();

            //Create Models directory
            if (!Directory.Exists(Path + @"\Resources\Models"))
                Directory.CreateDirectory(Path + @"\Resources\Models");

            //Create Alpha Images directory
            if (!Directory.Exists(Path + @"\Resources\Alpha"))
                Directory.CreateDirectory(Path + @"\Resources\Alpha");

            if (!Directory.Exists(Path + @"\Resources\Temp"))
                Directory.CreateDirectory(Path + @"\Resources\Temp");
            //Create Alpha Images directory
            if (!Directory.Exists(Path + @"\Resources\Temp\RGB"))
                Directory.CreateDirectory(Path + @"\Resources\Temp\RGB");

            //Create Alpha Images directory
            if (!Directory.Exists(Path + @"\Resources\Temp\ESRGAN_RGB"))
                Directory.CreateDirectory(Path + @"\Resources\Temp\ESRGAN_RGB");

            //Base folder where images files will be stored
            if (!Directory.Exists(Path + @"\Resources\Images"))
                Directory.CreateDirectory(Path + @"\Resources\Images");



            //Base folder where Completed files will be stored
            if (!Directory.Exists(Path + @"\Complete"))
                Directory.CreateDirectory(Path + @"\Complete");



            #endregion

            Screen.pb_loading.Value = 20;

            #region CHECK PYTHON DIRECTORY
            Settings.PythonInstalled = Directory.Exists(Settings.PythonDir) ? true : false;
            pythonWrapper.PythonDir = Settings.PythonDir;
            #endregion

            Screen.pb_loading.Value = 30;

            #region CHECK PYTORCH
            Settings.TorchInstalled = Directory.Exists(Settings.PythonDir + @"\Lib\site-packages\torch") ? true : false;
            #endregion

            Screen.pb_loading.Value = 40;

            #region CHECK CV2
            Settings.Cv2Installed = Directory.Exists(Settings.PythonDir + @"\Lib\site-packages\cv2") ? true : false;
            #endregion

            #region DELETE FILES IN FOLDERS

            //DELTE ALL FILES IN TEMP FOLDERS
            DirectoryInfo directory = new DirectoryInfo(AlphaDir);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }

            directory = new DirectoryInfo(RBGDir);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }

            directory = new DirectoryInfo(ESRGANOUTDIR);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }

            #endregion

            Screen.pb_loading.Value = 50;
        }

        private void GetGPUInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["Name"].ToString().Contains("NVIDIA"))
                    {
                        Settings.NVIDIAGPUAvailable = true;
                    }
                    else if (obj["Name"].ToString().Contains("AMD"))
                    {
                        Settings.AMDGPUAvailable = true;
                    }
                    else if (obj["Name"].ToString().Contains("Intel"))
                    {
                        Settings.IntelGpuAvailable = true;
                    }
                }
            }
        }
        private void GetImageFiles(string folder)
        {
            //clear existing images in list

            lv_FileList.Items.Clear();

            #region GridView Bindings
            GridView gridView = new GridView();
            GridViewColumn nameColumn = new GridViewColumn();
            nameColumn.DisplayMemberBinding = new Binding("Name");
            nameColumn.Header = "Name";
            nameColumn.Width = 300;
            gridView.Columns.Add(nameColumn);

            GridViewColumn sizeColumn = new GridViewColumn();
            sizeColumn.DisplayMemberBinding = new Binding("ImgSize");
            sizeColumn.Header = "Size";
            sizeColumn.Width = 60;
            gridView.Columns.Add(sizeColumn);

            GridViewColumn mipmapColumn = new GridViewColumn();
            mipmapColumn.DisplayMemberBinding = new Binding("mipmap");
            mipmapColumn.Header = "mipmap";
            mipmapColumn.Width = 60;
            gridView.Columns.Add(mipmapColumn);

            lv_FileList.View = gridView;
            #endregion

            #region Get Images from Folder
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                FileInfo[] files = directoryInfo.GetFiles();
                System.Drawing.Image img;

                fileCount = 0; //Reset File Count

                foreach (var file in files)
                {
                    string f_name = file.Name;
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

                            this.lv_FileList.Items.Add(new Image { Name = f_name, ImgSize = temp[1], mipmap = "Yes" });
                        }
                        else
                        {
                            this.lv_FileList.Items.Add(new Image { Name = f_name, ImgSize = temp[1], mipmap = "No" });
                        }
                        fileCount++; //increment file count
                    }
                    else //for all other types add to the array of items
                    {
                        /// *** Need to add check to image file type as listed in settings file.
                        /// *** Right now this will only work with *.png files.
                        /// *** REFACTOR 

                        if (ImageExtensions.Contains(Path.GetExtension(file.FullName).ToUpperInvariant()))
                        {
                            img = System.Drawing.Image.FromFile(file.FullName);
                            this.lv_FileList.Items.Add(new Image { Name = f_name, ImgSize = img.Width + "x" + img.Height, mipmap = "No" });
                        }
                    }
                }
                label_fileN.Content = files.Count();
                fileCount = files.Count();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex);
            }
            #endregion
        }

        private void GetModels(string folder)
        {
            //clear existing models
            cb_Model.Items.Clear();
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                FileInfo[] files = directoryInfo.GetFiles("*.pth");

                foreach (var file in files)
                {
                    cb_Model.Items.Add(file.Name);
                }
                cb_Model.SelectedIndex = 0;//show first item in list
            }
            catch
            {
                MessageBox.Show("Unable to Load Models");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_btn_OpenDIR(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)  //check for OK...they might press cancel, so don't do anything if they did.
                {
                    var path = dialog.SelectedPath;
                    tb_Input_Directory.Text = path;
                    ImageDir = path;
                    GetImageFiles(path);
                }
            }
        }

        #region Unused Buttons
        private void Button_btn_RemoveAlpha(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();
            int count = 0;

            foreach (var file in files)
            {
                Console.Out.WriteLine(count);
                count++;
                pythonWrapper.RemoveAlhpaChannel(file.FullName.ToString(), AlphaDir);
            }
        }

        private void Btn_ESRGAN_Click(object sender, RoutedEventArgs e)
        {
            btn_ESRGAN.IsEnabled = false;
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();
            ESRGANComplete = 0;
            CurrentModel = cb_Model.SelectedItem.ToString();
            Task.Run(() =>
            {
                foreach (var file in files)
                {
                    pythonWrapper.Esrgan(file.FullName.ToString(), CompleteDIR, CurrentModel, Settings.GPU, false);
                    ESRGANComplete++;
                    UpdateLabel(lbESRGAN, ESRGANComplete.ToString());
                    UpdateRTB(rtbConsole, "Processed Image with ESRGAN: " + file.Name);
                }
            }).ContinueWith(t => btn_ESRGAN.Dispatcher.BeginInvoke((Action)(() => btn_ESRGAN.IsEnabled = true)));
            
        }

        private void Btn_MAlpha_Click(object sender, RoutedEventArgs e)
        {
            /*
            DirectoryInfo directoryInfo = new DirectoryInfo(ESRGANOUTDIR);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                //.HelloWorld();
                pythonWrapper.AddTransparency(ImageDir, file.FullName.ToString(), CompleteDIR);
            }
            */
        }
        #endregion
        private void Btn_ReloadModels_Click(object sender, RoutedEventArgs e)
        {
            GetModels(CurrDir + @"\Resources\models");
            //list all files in image folder folder        
            GetImageFiles(tb_Input_Directory.Text);
            ImageDir = tb_Input_Directory.Text;
            //tb_Input_Directory.Text = Properties.Settings.Default.IMAGE_FOLDER;
        }

        private void NvidiaMenu_Click(object sender, RoutedEventArgs e)
        {
            /*lbGPU.Content = "NVIDIA";
            lbGPU.Foreground = Brushes.Green;
            GPU = "NVIDIA";*/

        }

        private void AMDMenu_Click(object sender, RoutedEventArgs e)
        {
            /*   lbGPU.Content = "AMD";
               lbGPU.Foreground = Brushes.Red;
               GPU = "AMD";*/
        }

        private void BtnMipmaps_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(CurrDir + @"\Complete\Mipmaps"))
                Directory.CreateDirectory(CurrDir + @"\Complete\Mipmaps");

            DirectoryInfo directoryInfo = new DirectoryInfo(CurrDir + @"\Complete");
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                if (file.Name.Contains("_m_"))
                {
                    pythonWrapper.GenerateBitmaps(file.FullName.ToString(), CurrDir + @"\Complete\Mipmaps", "Dolphin");
                }
            }

        }

        private void BtnAuto_ClickAsync(object sender, RoutedEventArgs e)
        {
            //disable buttons so they cant be pressed more than once
            btn_ESRGAN.IsEnabled = false;
            btnAuto.IsEnabled = false;
            CurrentModel = cb_Model.SelectedItem.ToString();
            //Run a new task that will perform all background operations to process Images
            Task.Run(() =>
            {
                //processImage();

                processImageThreaded();//Run if threading is true in settings
            })
            .ContinueWith(t => Console.WriteLine("done"));
        }

        public void processImageThreaded()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();
            //Remove Alpha Channel
            UpdateRTB(rtbConsole, "Removing Aplha Channel from Images");
            var taskA = Task.Run(() =>
            {
                RemoveAlpha(files);
            });
            taskA.Wait();
            System.Threading.Thread.Sleep(1000);//wait for all processes to complete
            UpdateRTB(rtbConsole, "All Alpha Channels Removed");


            //Process ESRGAN for Images
            directoryInfo = new DirectoryInfo(AlphaDir);
            files = directoryInfo.GetFiles();
            UpdateRTB(rtbConsole, "Processing Images with ESRGAN");
            var taskB = Task.Run(() =>
            {
                Esrgan(files);
            });
            taskB.Wait();
            System.Threading.Thread.Sleep(1000);//wait for all processes to complete
            System.Threading.Thread.Sleep(1000);//wait for all processes to complete
            UpdateRTB(rtbConsole, "All Images Processed with ESRGAN");

            //Process ESRGAN for Images
            directoryInfo = new DirectoryInfo(ESRGANOUTDIR);
            files = directoryInfo.GetFiles();

            UpdateRTB(rtbConsole, "Processing Images with Alpha");
            var taskC = Task.Run(() =>
            {
                if (ModelTestMode == false)
                {
                    RestoreAlpha(files);
                }
            });
            taskC.Wait();
            System.Threading.Thread.Sleep(1000);//wait for all processes to complete
            UpdateRTB(rtbConsole, "All Images Processed with Alpha");

            btn_ESRGAN.Dispatcher.BeginInvoke((Action)(() => btn_ESRGAN.IsEnabled = true));
            btnAuto.Dispatcher.BeginInvoke((Action)(() => btnAuto.IsEnabled = true));

        }

        public void RemoveAlpha(FileInfo[] files)
        {
            ///REFACTOR
            ///Run in a max of 5 threds
            ///Need to eventually change this based on settings file

            ActiveTasks = 0;
            UpdateLabel(lbAlpha, count.ToString());

            foreach (var file in files)
            {
                var taskD = Task.Run(() =>
                {
                    pythonWrapper.RemoveAlhpaChannel(file.FullName.ToString(), AlphaDir);
                    count++;
                    UpdateRTB(rtbConsole, "Removed alpha channel from: " + file.Name);
                    UpdateLabel(lbAlpha, count.ToString());
                }).ContinueWith(t => ActiveTasks--);
                ActiveTasks++; //Increment Task

            ThreadLoop:
                if (ActiveTasks >= 5)
                {
                    System.Threading.Thread.Sleep(100);
                    goto ThreadLoop;
                }
            }
        }

        public void Esrgan(FileInfo[] files)
        {
            foreach (var file in files)
            {
                pythonWrapper.Esrgan(file.FullName.ToString(), ESRGANOUTDIR, CurrentModel, Settings.GPU, ModelTestMode);
                ESRGANComplete++;
                UpdateLabel(lbESRGAN, ESRGANComplete.ToString());
                UpdateRTB(rtbConsole, "Processed Image with ESRGAN: " + file.Name);
            }
        }

        //## ADD MODEL FOR ALPHA CHANGE
        public void RestoreAlpha(FileInfo[] files)
        {

            foreach (var file in files)
            {
                var taskE = Task.Run(() =>
                {
                    //pythonWrapper.AlphaBRZ(ImageDir, file.FullName.ToString(), CompleteDIR, "4x_xbrz_90k.pth", Settings.GPU, AlphaDir, ESRGANOUTDIR);
                    pythonWrapper.AddTransparency(ImageDir, file.FullName.ToString(), CompleteDIR);
                    UpdateRTB(rtbConsole, "Processed Image with Alpha: " + file.Name);
                    FinalComplete++;
                    UpdateLabel(lbComplete, FinalComplete.ToString());
                }).ContinueWith(t => ActiveTasks--);
                ActiveTasks++; //Increment Task

            ThreadLoop:
                if (ActiveTasks >= 1)
                {
                    System.Threading.Thread.Sleep(100);
                    goto ThreadLoop;
                }
            }
        }

        public void UpdateLabel(Label label, string text)
        {
            label.Dispatcher.BeginInvoke((Action)(() => label.Content = text));
        }

        public void UpdateRTB(TextBox textBox, string text)
        {
            textBox.Dispatcher.BeginInvoke((Action)(() => textBox.Text += text + "\n"));
            textBox.Dispatcher.BeginInvoke((Action)(() => textBox.ScrollToEnd()));
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            ModelTestMode = true;

            //Run a new task that will perform all background operations to process Images
            Task.Run(() =>
            {
                foreach (var model in cb_Model.Items)
                {
                    CurrentModel = model.ToString();
                    processImageThreaded();//Run if threading is true in settings
                    System.Threading.Thread.Sleep(3000);
                }
                ModelTestMode = false;
            })
            .ContinueWith(t => Console.WriteLine("Test Mode Complete"));



        }
    }
}
