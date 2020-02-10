using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using PythonWrapper;

namespace Athena_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private cv2Wrapper cv2 = new cv2Wrapper();

        private string tempDir = Directory.GetCurrentDirectory() + @"\Temp";
        private string RBGDir = Directory.GetCurrentDirectory() + @"\Temp\RGB";
        private string ESRGANDIR = Directory.GetCurrentDirectory() + @"\Temp\ESRGAN_RGB";
        private string ImageDir = Properties.Settings.Default.IMAGE_FOLDER; // need to add something to change this
        private string CompleteDIR = Directory.GetCurrentDirectory() + @"\Complete";

        private string GPU = "";

        public MainWindow()
        {
            InitializeComponent();

            //Check if temporary folders exist. If it does not then create them
            string Path = Directory.GetCurrentDirectory();

            //Base folder where all temporary files will be stored
            if (!Directory.Exists(Path + @"\Temp"))
                Directory.CreateDirectory(Path + @"\Temp");

            //Folder for files that no longer have an alpha channel
            if (!Directory.Exists(Path + @"\Temp\RGB"))
                Directory.CreateDirectory(Path + @"\Temp\RGB");

            if (!Directory.Exists(Path + @"\Temp\ESRGAN_RGB"))
                Directory.CreateDirectory(Path + @"\Temp\ESRGAN_RGB");

            //Base folder where all temporary files will be stored
            if (!Directory.Exists(Path + @"\Complete"))
                Directory.CreateDirectory(Path + @"\Complete");



            //list all files in image folder folder
            GetImageFiles(Properties.Settings.Default.IMAGE_FOLDER);
            tb_Input_Directory.Text = Properties.Settings.Default.IMAGE_FOLDER;

            //list all models in esrgan folder
            GetModels(Properties.Settings.Default.ESRGAN_FOLDER + @"\models");

            //Set GPU Type
            if (Properties.Settings.Default.GPU == "NVIDIA")
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


        }

        private void GetImageFiles(string folder)
        {
            //clear existing images
            lv_FileList.Items.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                lv_FileList.Items.Add(file.Name);
            }
            label_fileN.Content = files.Count();
        }

        private void GetModels(string folder)
        {
            //clear existing models
            cb_Model.Items.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FileInfo[] files = directoryInfo.GetFiles("*.pth");

            foreach (var file in files)
            {
                cb_Model.Items.Add(file.Name);
            }
            cb_Model.SelectedIndex = 0;//show first item in list
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

        private void Button_btn_RemoveAlpha(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                cv2.RemoveAlhpaChannel(file.FullName.ToString(), RBGDir);
            }
        }

        private void Btn_ESRGAN_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(RBGDir);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                cv2.Esrgan(file.FullName.ToString(), ESRGANDIR, cb_Model.SelectedItem.ToString(), lbGPU.Content.ToString());
            }
        }

        private void Btn_MAlpha_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ESRGANDIR);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                //.HelloWorld();
                cv2.AddTransparency(ImageDir, file.FullName.ToString(), CompleteDIR);
            }
        }

        private void Btn_ReloadModels_Click(object sender, RoutedEventArgs e)
        {
            GetModels(Properties.Settings.Default.ESRGAN_FOLDER + @"\models");
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
    }
}
