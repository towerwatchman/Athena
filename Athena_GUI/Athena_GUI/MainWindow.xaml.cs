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
        private string UpdateDir = Directory.GetCurrentDirectory() + @"\Updated";
        private string resultsDir = Directory.GetCurrentDirectory() + @"\Final";
        private string OriginalDir = Directory.GetCurrentDirectory() + @"\Original";

        public MainWindow()
        {
            InitializeComponent();

            //Check if a results path exist. If it does not then create it
            string Path = Directory.GetCurrentDirectory();

            if (!Directory.Exists(Path + @"\Results"))
                Directory.CreateDirectory(Path + @"\Results");

            if (!Directory.Exists(Path + @"\Temp"))
                Directory.CreateDirectory(Path + @"\Temp");

            //list all files in image folder folder
            GetImageFiles(Properties.Settings.Default.IMAGE_FOLDER);
            tb_Input_Directory.Text = Properties.Settings.Default.IMAGE_FOLDER;

            //list all models in esrgan folder
            GetModels(Properties.Settings.Default.ESRGAN_FOLDER + @"\models");


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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                //System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)  //check for OK...they might press cancel, so don't do anything if they did.
                {
                    var path = dialog.SelectedPath;
                    tb_Input_Directory.Text = path;
                    //do something with path
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetModels(Properties.Settings.Default.ESRGAN_FOLDER + @"\models");
        }

        private void Btn_alpha_test_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Properties.Settings.Default.IMAGE_FOLDER);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                //.HelloWorld();
                cv2.RemoveAlhpaChannel(file.FullName.ToString(), tempDir);
            }
        }

        private void Btn_ESRGAN_Click(object sender, RoutedEventArgs e)
        {
            cv2.Esrgan(tempDir, cb_Model.SelectedItem.ToString());
        }

        private void Btn_MAlpha_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(UpdateDir);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                //.HelloWorld();
                cv2.AddTransparency(OriginalDir, file.FullName.ToString(), resultsDir);
            }
        }
    }
}
