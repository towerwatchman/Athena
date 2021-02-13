using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connius
{
    class Parser
    {
        private string CurrDir = Directory.GetCurrentDirectory();

        public string ReadSettings()
        {
            //Attempt to open settings file. If no file is available then use default settings
            try
            {
                using (StreamReader reader = new StreamReader(CurrDir + @"\Settings.ini"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] temp = line.Split('=');
                        switch (temp[0])
                        {
                            case "PythonDir":
                                Settings.Directory.PythonDirectory = temp[1];
                                break;                           
                            case "GPU":
                                Settings.GPU.ActiveGPU = temp[1];
                                break;
                            case "Threading":
                                Settings.Threading.IsEnabled = Convert.ToBoolean(temp[1]);
                                break;
                            default:
                                //Console.WriteLine("Default case");
                                break;
                        }
                    }
                }
                return "Settings Successfully Loaded";
            }

            catch
            {
                return "Error Loading Settings";
            }
        }

        public void WriteSettings()
        {

        }
    }
}
