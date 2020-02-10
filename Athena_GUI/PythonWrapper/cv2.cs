using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonWrapper
{
    //This is parts from the OpenCV Python Class
    public class cv2Wrapper
    {
        private string LocalDirectory = Directory.GetCurrentDirectory();
        ///Color Space Conversions
        ///
        public void CvtColor(string src, string dst, int code, int dstCn)
        {

        }

        public void RemoveAlhpaChannel(string src, string dst)
        {
            string cmd = LocalDirectory + @"\PythonScripts\cv2\cvtColor.py";

            string args = "--input " + src + " --output " + dst;

            runScript(cmd, args);
        }

        public void HelloWorld()
        {
            string cmd = LocalDirectory + @"\PythonScripts\cv2\HelloWorld.py";
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
                args = "--input " + src + @" --output " + dst + @"C:\ctp\esrgan\models\" + model;
            }
            else //AMD
            {
                args = "--input " + src + @" --output " + dst + @" --cpu C:\ctp\esrgan\models\" + model;
            }

            runScript(cmd, args);
        }

        public void AddTransparency(string ori, string src, string dst)
        {
            string cmd = LocalDirectory + @"\PythonScripts\cv2\RestoreTransparency.py";

            string args = "--original " +ori+ " --input " + src + " --output " + dst;

            runScript(cmd, args);
        }
        ///Geometric Image Transformations
        ///

        ///Image Filtering
        ///

        public void runScript(string cmd, string args)
        {
            Process process = new Process();
            process.StartInfo.FileName = Properties.Settings.Default.PYTHON_DIR + @"\python.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = string.Concat(cmd, " ", args);
            process.Start();
            using (StreamReader reader = process.StandardOutput)
            {
                string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")

                Console.Out.WriteLine(stderr);
                Console.Out.WriteLine(result);
            }

        }

       
    }
}
