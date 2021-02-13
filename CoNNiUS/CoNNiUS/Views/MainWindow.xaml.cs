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
        private Program project = new Program();
        private Wrapper PythonWrapper = new Wrapper();

        //public strings for keeping track of Images
        int errors = 0;
        int complete = 0;
        int progress = 0;

        public MainWindow()
        {
            InitializeComponent();
            Console.Out.TextBox = rtbConsole;
            project.Init();
            PythonWrapper.PythonDir = Settings.Directory.PythonDirectory; // This has to be set or not scripts will run
            
            
            PopulateWPF();            
        }

        private void PopulateWPF()
        {           
            //list all files in image folder folder        
            project.GetImageFiles(lv_FileList, Settings.Directory.ImageDirectory);
            label_fileN.Content = project.ImageFileCount;
            tb_Input_Directory.Text = Settings.Directory.ImageDirectory;

            //list all models in esrgan folder
            project.GetModels(cb_Model, Settings.Directory.ModelDirectory);

            //Population Labels
            lbGPU.Content = Settings.GPU.ActiveGPU;
            lbGPU.Foreground = Settings.GPU.IsCudaEnabled == true ? Brushes.Green: Brushes.Red;
            img_GPU.Source = Settings.GPU.IsCudaEnabled == true ? new BitmapImage(new Uri("pack://application:,,,/Images/Nvidia_logo.png")) : new BitmapImage( new Uri("pack://application:,,,/Images/Intel.png")) ;
            lbPytorch.Foreground = Settings.ResourceChecker.IsTorchInstalled == true ? Brushes.Green : Brushes.Red;
            lbPytorch.Content = Settings.ResourceChecker.IsTorchInstalled == true ? "Installed" : "Not Installed";
            lbCV2.Foreground = Settings.ResourceChecker.IsCv2Installed == true ? Brushes.Green : Brushes.Red;
            lbCV2.Content = Settings.ResourceChecker.IsCv2Installed == true ? "Installed" : "Not Installed";
            lbPython.Foreground = Settings.ResourceChecker.IsPythonInstalled == true ? Brushes.Green : Brushes.Red;
            lbPython.Content = Settings.ResourceChecker.IsPythonInstalled == true ? "Installed" : "Not Installed";
            lbNumpy.Foreground = Settings.ResourceChecker.IsNumpyInstalled == true ? Brushes.Green : Brushes.Red;
            lbNumpy.Content = Settings.ResourceChecker.IsNumpyInstalled == true ? "Installed" : "Not Installed";
            lbPngquant.Foreground = Settings.ResourceChecker.IsPngQuantInstalled == true ? Brushes.Green : Brushes.Red;
            lbPngquant.Content = Settings.ResourceChecker.IsPngQuantInstalled == true ? "Installed" : "Not Installed";
            lbWand.Foreground = Settings.ResourceChecker.IsWandInstalled == true ? Brushes.Green : Brushes.Red;
            lbWand.Content = Settings.ResourceChecker.IsWandInstalled == true ? "Installed" : "Not Installed";

            //progress bar
            pbImages.Maximum = 1;
            pbImages.Value = 0;

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

        public void UpdateProgressBar()
        {
            pbImages.Dispatcher.BeginInvoke((Action)(()=> pbImages.Value = progress));
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
                    string path = dialog.SelectedPath;
                    tb_Input_Directory.Text = path;
                    Settings.Directory.ImageDirectory = path;
                    project.GetImageFiles(lv_FileList, path);
                    label_fileN.Content = project.ImageFileCount;
                }
            }
        }
                
        private void Btn_ReloadModels_Click(object sender, RoutedEventArgs e)
        {
            project.GetModels(cb_Model, Settings.Directory.ModelDirectory);
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
            Console.Out.WriteLine("Processing Images");
            complete = 0;
            errors = 0;
            progress = 0;
            lbImageComplete.Content = 0;
            lbImageFail.Content = 0;
            pbImages.Maximum = (Int32)label_fileN.Content * 2;
            pbImages.Value = 0;

            btn_ESRGAN.IsEnabled = false;//disable button so you cant press it again

            DirectoryInfo directoryInfo = new DirectoryInfo(Settings.Directory.ImageDirectory);
            FileInfo[] files = directoryInfo.GetFiles();
            string model = cb_Model.SelectedItem.ToString();
            Task.Run(() =>
            {
                foreach (var file in files)
                {
                    progress ++;
                    UpdateProgressBar();
                    string status = PythonWrapper.Esrgan(file.FullName.ToString(), Settings.Directory.OutputDirectory, model, Settings.GPU.IsCudaEnabled);

                    errors += status == "Error" ? 1 : 0;
                    complete += status == "Complete" ? 1 : 0;

                    lbImageComplete.Dispatcher.BeginInvoke((Action)(() => lbImageComplete.Content = complete));
                    lbImageFail.Dispatcher.BeginInvoke((Action)(() => lbImageFail.Content = errors));

                    progress++;
                    UpdateProgressBar();
                }
            }).ContinueWith(t => btn_ESRGAN.Dispatcher.BeginInvoke((Action)(() => btn_ESRGAN.IsEnabled = true)));
            
        }

        private void btn_InstallPackages_Click(object sender, RoutedEventArgs e)
        {
            PythonWrapper.IsntallPackages();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btn_InstallPython_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.python.org/downloads/");
        }

        private void btn_InstallPytorch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://pytorch.org/get-started/locally/");
        }
    }
}
