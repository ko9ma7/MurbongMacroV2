using MahApps.Metro.Controls;
using MurbongMacroV2.Core;
using System.Windows;

namespace MurbongMacroV2.Window
{
    /// <summary>
    /// MouseWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MouseWindow : MetroWindow
    {
        public MouseMacro macro;

        public MouseWindow()
        {
            InitializeComponent();
        }
        private string GetMouseBtnText()
        {
            string pr_pl = cmb_Mouse.Text;
            string mouse = "";
            mouse += pr_pl;
            mouse += "/";

            if (LButton.IsChecked == true)
            {
                mouse += "LB";
            }
            else if (RButton.IsChecked == true)
            {
                mouse += "RB";
            }
            else if (MButton.IsChecked == true)
            {
                mouse += "MB";
            }
            else if (E4Button.IsChecked == true)
            {
                mouse += "E4B";
            }
            else if (E5Button.IsChecked == true)
            {
                mouse += "E5B";
            }
            else if (MWheelDown.IsChecked == true)
            {
                mouse = "MWDown";
            }
            else if (MWheelUp.IsChecked == true)
            {
                mouse = "MWUp";
            }
            else
            {
                return "Err";
            }
            return mouse;
        }
        private int GetMouseBtn()
        {
            string pr_pl = cmb_Mouse.Text;
            int mouse;

            if (LButton.IsChecked == true)
            {
                mouse = MouseBtn.LB;
            }
            else if (RButton.IsChecked == true)
            {
                mouse = MouseBtn.RB;
            }
            else if (MButton.IsChecked == true)
            {
                mouse = MouseBtn.MB;
            }
            else if (E4Button.IsChecked == true)
            {
                mouse = MouseBtn.E4B;
            }
            else if (E5Button.IsChecked == true)
            {
                mouse = MouseBtn.E5B;
            }
            else if (MWheelDown.IsChecked == true)
            {
                mouse = 2052;//2052 = MWheelDown
            }
            else if (MWheelUp.IsChecked == true)
            {
                mouse = 2051;//2050 = MWheelUp
            }
            else
            {
                return -1;
            }
            mouse = pr_pl == "Press" ? mouse : mouse << 1;

            return mouse;
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            macro = new MouseMacro()
            {
                Control = "Mouse",
                Action = GetMouseBtnText(),
                Status = 0, // MouseClick
                MouseButton = GetMouseBtn()
            };
            DialogResult = true;
            Close();
        }
    }
}
