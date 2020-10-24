using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MurbongMacroV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// DelayWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DelayWindow : MetroWindow
    {

        public DelayMacro macro;
        public DelayWindow()
        {
            InitializeComponent();
        }

        public DelayWindow(int delay, int rand1, int rand2) : this()
        {

            txt_Delay.Text = delay+"";
            txt_Param1.Text = rand1 + "";
            txt_Param2.Text = rand2 + "";

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[-]?\d*$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Text == "-" && (!string.IsNullOrEmpty((sender as TextBox).Text.Trim()) || (sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }
        private void Edit(object sender, RoutedEventArgs e)
        {

            int Delay;
            string DelayStr = "Delay/";
            int Param1 = 0;
            int Param2 = 0;
            try
            {
                Delay = Convert.ToInt32(txt_Delay.Text);
                DelayStr += txt_Delay.Text + "ms";
                if (txt_Param1.Text != "" && txt_Param2.Text != "")
                {
                    Param1 = Convert.ToInt32(txt_Param1.Text);
                    Param2 = Convert.ToInt32(txt_Param2.Text);
                    DelayStr += "/" + txt_Param1.Text + "~" + txt_Param2.Text + "ms";
                }
            }
            catch
            {
                Delay = 0;
                DelayStr += Delay + "ms";
            }

            if (Delay < 0 || Delay + Param1 < 0 || Param2 < Param1)
            {
                this.ShowMessageAsync("오류", "잘못된 명령입니다.", MessageDialogStyle.Affirmative);
            }
            else
            {
                macro = new DelayMacro()
                {
                    Control = "Delay",
                    Action = DelayStr,
                    MS = Delay,
                    RandParam1 = Param1,
                    RandParam2 = Param2
                };
                DialogResult = true;
                Close();
            }
        }
    }
}
