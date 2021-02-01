using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using PythonWrapper;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Athena_GUI
{
    public partial class MainWindow : Window
    {

        private Wrapper pythonWrapper = new Wrapper();
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private SettingsParser settingsParser = new SettingsParser();

        //Global Variables
        private string tempDir = Directory.GetCurrentDirectory() + @"\ResourTemp";
        private string AlphaDir = Directory.GetCurrentDirectory() + @"\Resources\Alpha";
        //private string ESRGANOUTDIR = Directory.GetCurrentDirectory() + @"\Temp\ESRGAN_RGB";
        private string ImageDir = Directory.GetCurrentDirectory() + @"\Resources\Images"; // need to add something to change this
        private string CompleteDIR = Directory.GetCurrentDirectory() + @"\Complete";
        private string CurrDir = Directory.GetCurrentDirectory();
        private string GPU = "";
        private int fileCount = 0;
        private static List<string> ImageExtensions = new List<string> { ".PNG", ".JPG" };

        //Threading Stuff
        private int count = 0;
        private int AlphaComplete = 0;
        private int ESRGANComplete = 0;
        private int FinalComplete = 0;
        private string CurrentModel = "";

        private int ActiveTasks = 0;


        public MainWindow()
        {
            InitializeComponent();

            #region CREATE FOLDER STRUCTURE
            //Check if temporary folders exist. If it does not then create them
            string Path = Directory.GetCurrentDirectory();

            //Create Models directory
            if (!Directory.Exists(Path + @"\Resources\Models"))
                Directory.CreateDirectory(Path + @"\Resources\Models");

            //Create Alpha Images directory
            if (!Directory.Exists(Path + @"\Resources\Alpha"))
                Directory.CreateDirectory(Path + @"\Resources\Alpha");

            //Base folder where images files will be stored
            if (!Directory.Exists(Path + @"\Resources\Images"))
                Directory.CreateDirectory(Path + @"\Resources\Images");

            //Base folder where Completed files will be stored
            if (!Directory.Exists(Path + @"\Complete"))
                Directory.CreateDirectory(Path + @"\Complete");
            #endregion

            #region INIT

            //DELTE ALL FILES IN TEMP FOLDERS
            /*DirectoryInfo directory = new DirectoryInfo(RBGDir);
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
            }*/

            //lOAD SETTINGS
            settingsParser.ReadSettings();

            if (Settings.LastAccessedFolder != "")
            {
                ImageDir = CurrDir + @"\Resources\images";
            }

            //list all files in image folder folder        
            GetImageFiles(ImageDir);
            tb_Input_Directory.Text = ImageDir;

            //list all models in esrgan folder
            GetModels(CurrDir + @"\Resources\models");

            //Set GPU Type
            if (Settings.GPU == "NVIDIA")
            {
                lbGPU.Content = "NVIDIA";
                lbGPU.Foreground = Brushes.Green;
                GPU = "NVIDIA";
            }
            else
            {
                lbGPU.Content = "AMD";
                lbGPU.Foreground = Brushes.Red;
                GPU = "AMD";
            }

            //Set pythondir for Python Wrapper
            if (Settings.PythonDir == "")
            {
                MessageBox.Show("Python Not Installed Correctly");
            }
            else
            {
                pythonWrapper.PythonDir = Settings.PythonDir;
            }
            #endregion
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
            /*DirectoryInfo directoryInfo = new DirectoryInfo(RBGDir);
            FileInfo[] files = directoryInfo.GetFiles();
            int count = 0;

            foreach (var file in files)
            {
                Console.Out.WriteLine(count);
                count++;
                pythonWrapper.Esrgan(file.FullName.ToString(), ESRGANOUTDIR, cb_Model.SelectedItem.ToString(), lbGPU.Content.ToString());
            }*/
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
            lbGPU.Content = "NVIDIA";
            lbGPU.Foreground = Brushes.Green;
            GPU = "NVIDIA";

        }

        private void AMDMenu_Click(object sender, RoutedEventArgs e)
        {
            lbGPU.Content = "AMD";
            lbGPU.Foreground = Brushes.Red;
            GPU = "AMD";
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

            /*
            //Process ESRGAN for Images
            directoryInfo = new DirectoryInfo(RBGDir);
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
                RestoreAlpha(files);
            });
            taskC.Wait();
            System.Threading.Thread.Sleep(1000);//wait for all processes to complete
            UpdateRTB(rtbConsole, "All Images Processed with Alpha");
            
            btn_ESRGAN.Dispatcher.BeginInvoke((Action)(() => btn_ESRGAN.IsEnabled = true));
            btnAuto.Dispatcher.BeginInvoke((Action)(() => btnAuto.IsEnabled = true));
            */
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
                //pythonWrapper.Esrgan(file.FullName.ToString(), ESRGANOUTDIR, CurrentModel, Settings.GPU);
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
                    //pythonWrapper.AddTransparency(ImageDir, file.FullName.ToString(), CompleteDIR);
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
            int count = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                pythonWrapper.Test(file.FullName.ToString(), CompleteDIR, cb_Model.SelectedItem.ToString(), Settings.GPU);
                count++;
                Console.Out.WriteLine("Updated Count: " + count);
            }
        }

    }
}
