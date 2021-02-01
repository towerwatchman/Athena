using System;
using System.Diagnostics;
using System.IO;

namespace PythonWrapper
{
    public class Wrapper
    {
        public string PythonDir
        {
            get;
            set;
        }

        private string LocalDirectory = Directory.GetCurrentDirectory();

        public void CheckPythonVersion()
        {

        }

        public void CheckForCV2()
        {

        }
        public void RemoveAlhpaChannel(string src, string dst)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\RemoveTransparency.py";

            string args = "--input \"" + src + "\" --output \"" + dst + "\"";

            runScript(cmd, args);
        }

        public void HelloWorld()
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\HelloWorld.py";
            string args = "";
            runScript(cmd, args);
        }

        public void Esrgan(string src, string dst, string model, string gpu)
        {
            string cmd = @"C:\ctp\esrgan\run.py";
            //string cmd = LocalDirectory + @"\Resources\Scripts\cv2\Run.py";
            string args = "";
            gpu = "KK";

            if (gpu == "NVIDIA")
            {
                args = "--input " + src + @" --output " + dst + @" C:\ctp\esrgan\models\" + model;
            }
            else //AMD
            {
                args = "--input " + src + @" --output " + dst + @" C:\ctp\esrgan\models\" + model;
            }

            runScript(cmd, args);
        }

        public void Test(string src, string dst, string model, string gpu)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\ESRGAN_Upscale.py";
            string args = "";
            //string cmd = LocalDirectory + @"\Resources\Scripts\cv2\Run.py";
            if (gpu == "NVIDIA")
            {
                args = "--input " + src + " --output " + dst + " --model " + @" C:\ctp\esrgan\models\4x_xbrz_90k.pth" + " --model2 " + @" C:\ctp\esrgan\models\4x_FArtDIV3_Blend.pth";
            }

            runScript(cmd, args);
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

            runScript(cmd, args);
        }

        public void AddTransparency(string ori, string src, string dst)
        {
            string cmd = LocalDirectory + @"\Resources\Scripts\RestoreTransparency.py";

            string args = "--original " + ori + " --input " + src + " --output " + dst;

            runScript(cmd, args);
        }

        public void GenerateBitmaps(string Image, string DestinationFolder, string Type)
        {
            if (Type == "Dolphin")
            {
                string cmd = LocalDirectory + @"\Resources\Scripts\GenerateMipmaps.py";

                string args = "--input " + Image + " --output " + DestinationFolder;

                runScript(cmd, args);
            }
        }

        public string runScript(string cmd, string args)
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
