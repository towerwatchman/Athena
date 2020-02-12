using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using PythonWrapper;
using System.Windows.Controls;
using System.Windows.Data;

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
        private string CurrDir = Directory.GetCurrentDirectory();

        private string GPU = "";

        public MainWindow()
        {
            InitializeComponent();

            #region CREATE FOLDER STRUCTURE
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
            #endregion

            #region INIT
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
            #endregion
        }
        private void GetImageFiles(string folder)
        {
            //clear existing images in list
            lv_FileList.Items.Clear();

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


            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FileInfo[] files = directoryInfo.GetFiles();

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
                }
                else //for all other types add to the array of items
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(file.FullName);
                    this.lv_FileList.Items.Add(new Image { Name = f_name, ImgSize = img.Width + "x" + img.Height, mipmap = "No" });
                    //lv_FileList.Items.Add(file.Name);
                }
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
                    cv2.GenerateBitmaps(file.FullName.ToString(), CurrDir + @"\Complete\Mipmaps", "Dolphin");
                }
            }
            
        }
    }
}
