using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_GUI
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
    }
}
