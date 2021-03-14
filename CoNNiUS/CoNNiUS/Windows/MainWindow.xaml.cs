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
        private Wrapper PythonWrapper = new Wrapper();
        public MainWindow()
        {
            InitializeComponent();
            uiMainFrame.Source = new Uri("../Pages/ImagePage.xaml", UriKind.RelativeOrAbsolute);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btn_InstallPackages_Click(object sender, RoutedEventArgs e)
        {
            PythonWrapper.IsntallPackages();
        }


        private void btn_InstallPython_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.python.org/downloads/");
        }

        private void btn_InstallPytorch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://pytorch.org/get-started/locally/");
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
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btn_InstallImagick_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://imagemagick.org/script/download.php#windows");
        }

        private void btn_DownloadModels_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://upscale.wiki/wiki/Model_Database");
        }
    }
}
