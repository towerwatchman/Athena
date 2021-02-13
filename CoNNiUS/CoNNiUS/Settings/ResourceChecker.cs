using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connius.Settings
{
    class ResourceChecker
    {
        public static bool IsPythonInstalled
        {
            get;
            set;
        }
        public static bool IsTorchInstalled
        {
            get;
            set;
        }
        public static bool IsCv2Installed
        {
            get;
            set;
        }
        public static bool IsNumpyInstalled
        {
            get;
            set;
        }
        public static bool IsWandInstalled
        {
            get;
            set;
        }
        public static bool IsPngQuantInstalled
        {
            get;
            set;
        }
    }
}
