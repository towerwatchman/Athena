using System;
using System.Diagnostics;
using System.IO;

namespace PythonWrapper
{
    public class Wrapper
    {

        private readonly string LocalDirectory = Directory.GetCurrentDirectory();
        public string PythonDir
        {
            get;
            set;
        }

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
                "pip install pngquant & " +
                "pip install Wand & " +
                "timeout 10 & " +
                "exit";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.StartInfo.Verb = "runas";
            process.Start();
            process.WaitForExit();
        }


        public string Esrgan(string src, string dst, string model, bool IsCudaEnabled)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\ESRGAN.py";
            string args;
            if (IsCudaEnabled == true)
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model;

            }
            else //Intel
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model + " --cpu";
            }

            return RunScriptHidden(cmd, args);

        }

        public string EsrganPreview(string src, string dst, string model, bool IsCudaEnabled)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\ESRGANpreview.py";
            string args;
            if (IsCudaEnabled == true)
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model;

            }
            else //Intel
            {
                args = "--input " + src + @" --output " + dst + " --model " + LocalDirectory + @"\Resources\Models\" + model + " --cpu";
            }
            
            return RunScriptHidden(cmd, args);

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
            process.StartInfo.CreateNoWindow = true;
            try
            {
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
                    return "Error"; //return Error to console
                }
                else
                {
                    return "Complete"; //return Complete
                }
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
                return "";
            }
            
        }
    }
}
