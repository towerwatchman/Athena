using System;
using System.Diagnostics;
using System.IO;

namespace PythonWrapper
{
    /// <summary>
    /// Arch
    /// Numpy
    /// CV2
    /// Torch
    /// Pip
    /// </summary>
    public class Wrapper
    {
        public string PythonDir
        {
            get;
            set;
        }

        private string LocalDirectory = Directory.GetCurrentDirectory();

        //Installer for missing Packages
        public void IsntallPackages()
        {
            //Upgrade Pip
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/k " +
                "pip install --upgrade pip & " +
                "pip install numpy & " +
                "pip install opencv-python & " +
                "pip install torch & " +
                "pip install torchvision & " +
                "pip install torchaudio & " +
                "timeout 10 & " +
                "exit";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.StartInfo.Verb = "runas";
            process.Start();
            process.WaitForExit();
        }


        public void Esrgan(string src, string dst, string model, string gpu, bool TestMode)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\ESRGAN.py";
            //string cmd = LocalDirectory + @"\Resources\Scripts\run.py";
            //string cmd2 = LocalDirectory + @"\Resources\Scripts\TestMode.py";
            string args = "";

            if (gpu == "NVIDIA")
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model;

            }
            else //AMD
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model + " --cpu";
            }

            if (TestMode == false)
            {
                RunScriptHidden(cmd, args);
            }
            else
            {
                //RunScriptHidden(cmd2, args);
            }

        }

        public void AlphaBRZ(string org, string src, string dst, string model, string gpu, string alpha, string tempdir)
        {
            //pythonWrapper.AlphaBRZ(ImageDir, file.FullName.ToString(), CompleteDIR, "4x_xbrz_90k.pth", Settings.GPU, AlphaDir, ESRGANOUTDIR);
            string cmd = LocalDirectory + @"\Resources\Scripts\TransparencyUpscale.py";
            string args = "";
            //string cmd = LocalDirectory + @"\Resources\Scripts\cv2\Run.py";
            if (gpu == "NVIDIA")
            {
                args = "--input " + src + " --output " + dst + " --original " + org + " --model " + @" C:\ctp\esrgan\models\4x_FArtDIV3_Fine.pth" + " --tempdir " + alpha + " --rgbdir " + tempdir;
            }

            RunScriptHidden(cmd, args);
        }

        public void GenerateBitmaps(string Image, string DestinationFolder, string Type)
        {
            if (Type == "Dolphin")
            {
                string cmd = LocalDirectory + @"\Resources\Scripts\GenerateMipmaps.py";

                string args = "--input " + Image + " --output " + DestinationFolder;

                RunScriptHidden(cmd, args);
            }
        }
        public string RunScriptHidden(string cmd, string args)
        {
            string stderr = "";
            string result = "";

            Process process = new Process();
            process.StartInfo.FileName = PythonDir + @"\python.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = string.Concat(cmd, " ", args);
            process.Start();
            using (StreamReader reader = process.StandardOutput)
            {
                stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")

                Console.Out.WriteLine(stderr);
                Console.Out.WriteLine(result);
            }
            if (stderr.Length > 0)
            {
                return stderr; //return Error to console
            }
            else
            {
                return result; //return Complete
            }
        }      
    }
}
