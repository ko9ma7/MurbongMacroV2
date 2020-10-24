using MahApps.Metro.Controls;
using MurbongMacroV2.Core;
using System;
using System.Windows;

namespace MurbongMacroV2.Window
{
    /// <summary>
    /// MouseWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MouseWindow2 : MetroWindow
    {
        public MouseMacro macro;

        public MouseWindow2()
        {
            InitializeComponent();
        }

        private int GetRandStatus()
        {
            if (radio_None.IsChecked == true)
            {
                return 0;
            }
            else if (radio_Circle.IsChecked == true)
            {
                return 1;
            }
            else if (radio_Rect.IsChecked == true)
            {
                return 2;
            }
            return -1;
        }
        private void RadioCheck(object sender, RoutedEventArgs e)
        {
            txtBox_MoveFactor.Text = "";
        }
        private void Edit(object sender, RoutedEventArgs e)
        {
            int x, y, rx, ry;
            int RandStatus = GetRandStatus();
            try
            {
                x = Convert.ToInt32(txt_x_pos.Text);
                y = Convert.ToInt32(txt_y_pos.Text);
                if (RandStatus == 1)
                {
                    rx = Convert.ToInt32(txtBox_MoveFactor.Text);
                    ry = 0;
                }
                else if (RandStatus == 2)
                {
                    rx = Convert.ToInt32(txtBox_MoveFactor.Text.Split(':')[0]);
                    ry = Convert.ToInt32(txtBox_MoveFactor.Text.Split(':')[1]);
                }
                else
                {
                    rx = 0;
                    ry = 0;
                }
            }
            catch
            {
                x = 0;
                y = 0;
                rx = 0;
                ry = 0;
            }

            Point MouseFactor = new Point(rx, ry);
            Point MousePoint = new Point(x, y);

            string MouseAction = "Move/" + MousePoint.ToString();

            if (RandStatus == 1)
            {
                MouseAction += "/C/" + MouseFactor.X.ToString();
            }
            else if (RandStatus == 2)
            {
                MouseAction += "/R/" + MouseFactor.ToString();
            }

            macro = new MouseMacro()
            {
                Control = "Mouse",
                Action = MouseAction,
                Status = 1,//MouseMove
                Coordinate = MousePoint,
                RandStatus = RandStatus,
                Parameter = MouseFactor


            };
            DialogResult = true;
            Close();
        }
    }
}
