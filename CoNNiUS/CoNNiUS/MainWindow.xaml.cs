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
using System.Windows.Media.Imaging;

namespace Connius
{
    public partial class MainWindow : Window
    {
        private Project project = new Project();
        //private PythonWrapper PythonWrapper;

        public MainWindow()
        {
            project.Init();
            InitializeComponent();
            PopulateWPF();            
        }

        private void PopulateWPF()
        {           
            //list all files in image folder folder        
            project.GetImageFiles(lv_FileList, project.ImageDir);
            label_fileN.Content = project.ImageFileCount;
            tb_Input_Directory.Text = project.ImageDir;

            //list all models in esrgan folder
            project.GetModels(cb_Model, project.ModelDir);

            //Population Labels
            lbGPU.Content = Settings.GPU;
            lbGPU.Foreground = Settings.GPU == "NVIDIA" ? Brushes.Green: Brushes.Red;
            img_GPU.Source = Settings.GPU == "NVIDIA" ? new BitmapImage( new Uri(Resources.["  "])) : new BitmapImage( new Uri(@"\Images\Nvidia.jpg")) ;
            lbPytorch.Foreground = Settings.TorchInstalled == true ? Brushes.Green : Brushes.Red;
            lbPytorch.Content = Settings.TorchInstalled == true ? "Installed" : "Not Installed";
            lbCV2.Foreground = Settings.Cv2Installed == true ? Brushes.Green : Brushes.Red;
            lbCV2.Content = Settings.Cv2Installed == true ? "Installed" : "Not Installed";
            lbPython.Foreground = Settings.PythonInstalled == true ? Brushes.Green : Brushes.Red;
            lbPython.Content = Settings.PythonInstalled == true ? "Installed" : "Not Installed";
            lbNumpy.Foreground = Settings.NumpyInstalled == true ? Brushes.Green : Brushes.Red;
            lbNumpy.Content = Settings.NumpyInstalled == true ? "Installed" : "Not Installed";

            //clear existing images in list
            lv_FileList.Items.Clear();

            #region GridView Bindings
            GridView gridView = new GridView();
            GridViewColumn nameColumn = new GridViewColumn();
            nameColumn.DisplayMemberBinding = new Binding("Name");
            nameColumn.Header = "Name";
            nameColumn.Width = 350;
            gridView.Columns.Add(nameColumn);

            GridViewColumn sizeColumn = new GridViewColumn();
            sizeColumn.DisplayMemberBinding = new Binding("ImgSize");
            sizeColumn.Header = "Size";
            sizeColumn.Width = 80;
            gridView.Columns.Add(sizeColumn);

            GridViewColumn mipmapColumn = new GridViewColumn();
            mipmapColumn.DisplayMemberBinding = new Binding("mipmap");
            mipmapColumn.Header = "mipmap";
            mipmapColumn.Width = 60;
            gridView.Columns.Add(mipmapColumn);

            lv_FileList.View = gridView;
            #endregion

        }

        #region INVOKE METHODS FOR GUI THREAD
        public void UpdateLabel(Label label, string text)
        {
            label.Dispatcher.BeginInvoke((Action)(() => label.Content = text));
        }

        public void UpdateRTB(TextBox textBox, string text)
        {
            textBox.Dispatcher.BeginInvoke((Action)(() => textBox.Text += text + "\n"));
            textBox.Dispatcher.BeginInvoke((Action)(() => textBox.ScrollToEnd()));
        }
        #endregion

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
                    string path = dialog.SelectedPath;
                    tb_Input_Directory.Text = path;
                    project.ImageDir = path;
                    project.GetImageFiles(lv_FileList, path);
                }
            }
        }
                
        private void Btn_ReloadModels_Click(object sender, RoutedEventArgs e)
        {
            project.GetModels(cb_Model, project.ModelDir);
            //list all files in image folder folder        
            //GetImageFiles(tb_Input_Directory.Text);
            //ImageDir = tb_Input_Directory.Text;
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
            /*if (!Directory.Exists(CurrDir + @"\Complete\Mipmaps"))
                Directory.CreateDirectory(CurrDir + @"\Complete\Mipmaps");

            DirectoryInfo directoryInfo = new DirectoryInfo(CurrDir + @"\Complete");
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                if (file.Name.Contains("_m_"))
                {
                    pythonWrapper.GenerateBitmaps(file.FullName.ToString(), CurrDir + @"\Complete\Mipmaps", "Dolphin");
                }
            }*/

        }

        private void Btn_ESRGAN_Click(object sender, RoutedEventArgs e)
        {
            btn_ESRGAN.IsEnabled = false;
            DirectoryInfo directoryInfo = new DirectoryInfo(project.ImageDir);
            FileInfo[] files = directoryInfo.GetFiles();
            //ESRGANComplete = 0;
            string model = cb_Model.SelectedItem.ToString();
            Task.Run(() =>
            {
                foreach (var file in files)
                {
                    project.pythonWrapper.Esrgan(file.FullName.ToString(), project.OutputDir, model, Settings.GPU, false);
                    //ESRGANComplete++;
                    //UpdateLabel(lbESRGAN, ESRGANComplete.ToString());
                    UpdateRTB(rtbConsole, "Processed Image with ESRGAN: " + file.Name);
                }
            }).ContinueWith(t => btn_ESRGAN.Dispatcher.BeginInvoke((Action)(() => btn_ESRGAN.IsEnabled = true)));
            
        }

        private void btn_InstallPackages_Click(object sender, RoutedEventArgs e)
        {
            project.pythonWrapper.IsntallPackages();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
