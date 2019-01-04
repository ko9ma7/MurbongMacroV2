using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfScreenHelper;

namespace MurbongMacroV2.Core
{


    class WindowScr
    {

        public double VirtualX;
        public double VirtualY;
        public double RealX;
        public double RealY;


        public WindowScr()
        {
            VirtualX = SystemInformation.VirtualScreen.Width;
            VirtualY = SystemInformation.VirtualScreen.Height;
            RealX = SystemParameters.VirtualScreenWidth;
            RealY = SystemParameters.VirtualScreenHeight;

        }

    }
}
