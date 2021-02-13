using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connius.Settings
{
    public static class GPU
    {
        public static string ActiveGPU
        {
            get;
            set;
        }
        public static bool IsIntelGpuAvailable
        {
            get;
            set;
        }
        public static bool IsNVIDIAGPUAvailable
        {
            get;
            set;
        }
        public static bool IsAMDGPUAvailable
        {
            get;
            set;
        }
        public static string IntelGPUName
        {
            get;
            set;
        }
        public static string NvidiaGPUName
        {
            get;
            set;
        }
        public static string AMDGPUName
        {
            get;
            set;
        }
        public static bool IsCudaEnabled
        {
            get;
            set;
        }
    }
}
