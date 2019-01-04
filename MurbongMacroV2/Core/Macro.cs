using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MurbongMacroV2.Core
{
    public class Macro
    {
        public string Control { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Control + Action + Description;
        }
    }

    public class KeyboardMacro : Macro
    {
        public int Code { get; set; }
        public int Btn { get; set; }

    }

    public class MouseMacro : Macro
    {
        public int Status { get; set; }
        public Point Coordinate { get; set; }
        public int RandStatus { get; set; }
        public Point Parameter { get; set; }
        public int MouseButton { get; set; }



    }

    public class DelayMacro : Macro
    {
        public int MS { get; set; }
        public int RandParam1 { get; set; }
        public int RandParam2 { get; set; }

    }

    public class ImageSearchMacro : Macro
    {
        public Image Images { get; set; }
        public Point Position { get; set; }
        public int Tolerance { get; set; }

    }

    public class LoopMacro : Macro
    {
        public Preset LoopPreset { get; set; }
        public int LoopCount { get; set; }
    }


}
