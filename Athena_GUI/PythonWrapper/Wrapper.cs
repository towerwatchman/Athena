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
              
        public void RemoveAlhpaChannel(string src, string dst)
        {
            string cmd = LocalDirectory + @"\PythonScripts\RemoveTransparency.py";

            string args = "--input " + src + " --output " + dst;

            runScript(cmd, args);
        }

        public void HelloWorld()
        {
            string cmd = LocalDirectory + @"\PythonScripts\HelloWorld.py";
            string args = "";
            runScript(cmd, args);
        }

        public void Esrgan(string src, string dst, string model, string gpu)
        {
            string cmd = @"C:\ctp\esrgan\run.py";
            //string cmd = LocalDirectory + @"\PythonScripts\cv2\Run.py";
            string args = "";

            if (gpu == "NVIDIA")
            {
                args = "--input " + src + @" --output " + dst + @" C:\ctp\esrgan\models\" + model;
            }
            else //AMD
            {
                args = "--input " + src + @" --output " + dst + @" --cpu C:\ctp\esrgan\models\" + model;
            }

            runScript(cmd, args);
        }

        public void Test(string ori, string src, string dst)
        {
            string cmd = LocalDirectory + @"\PythonScripts\TransparencyTest.py";
            //string cmd = LocalDirectory + @"\PythonScripts\cv2\Run.py";
            string args = "--original " + ori + " --input " + src + " --output " + dst;

            runScript(cmd, args);
        }

        public void AddTransparency(string ori, string src, string dst)
        {
            string cmd = LocalDirectory + @"\PythonScripts\RestoreTransparency.py";

            string args = "--original " +ori+ " --input " + src + " --output " + dst;

            runScript(cmd, args);
        }

        public void GenerateBitmaps(string Image, string DestinationFolder, string Type)
        {
            if (Type == "Dolphin")
            {
                string cmd = LocalDirectory + @"\PythonScripts\GenerateMipmaps.py";

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
            if(stderr.Length >0)
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
