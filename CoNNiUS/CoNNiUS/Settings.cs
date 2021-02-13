using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connius
{
    static class Settings
    {
        public static string GPU
        {
            get;
            set;
        }
        public static bool ThreadingEnabled
        {
            get;
            set;
        }
        public static string PythonDir
        {
            get;
            set;
        }
        public static string ESRGANDir
        {
            get;
            set;
        }
        public static string LastAccessedFolder
        {
            get;
            set;
        }
        public static bool PythonInstalled
        {
            get;
            set;
        }
        public static bool TorchInstalled
        {
            get;
            set;
        }
        public static bool Cv2Installed
        {
            get;
            set;
        }
        public static bool NumpyInstalled
        {
            get;
            set;
        }
        public static bool IntelGpuAvailable
        {
            get;
            set;
        }
        public static bool NVIDIAGPUAvailable
        {
            get;
            set;
        }
        public static bool AMDGPUAvailable
        {
            get;
            set;
        }
    }
}
