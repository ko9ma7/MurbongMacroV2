using MahApps.Metro.Controls;
using MurbongMacroV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MurbongMacroV2.Window
{
    /// <summary>
    /// LoopWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoopWindow : MetroWindow
    {
        public ControlMacro macro;
        public LoopWindow()
        {
            InitializeComponent();
        }

        public LoopWindow(int count) : this()
        {
            Nud_Loop.Value = count;
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            int loop;
            string loopstr;
            try
            {
                loop = (int)Nud_Loop.Value;
            }
            catch
            {
                loop = 1;
            }

            if (loop == -1)
            {
                loopstr = "infinity";
            }
            else if (loop == 0)
            {
                return;
            }
            else
            {
                loopstr = loop + "";
            }
            macro = new ControlMacro()
            {
                Control = "Loop",
                Action = "Loop # " + loopstr,
                Signal = 1,
                Parameter = loop

            };

            DialogResult = true;
            Close();
        }
    }
}
