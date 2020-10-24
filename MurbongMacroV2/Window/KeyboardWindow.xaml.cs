using MahApps.Metro.Controls;
using MurbongMacroV2.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApplicationHotKey.WinApi;

namespace MurbongMacroV2.Window
{
    /// <summary>
    /// KeyboardWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyboardWindow : MetroWindow
    {

        public int keycode;
        public KeyboardMacro macro;
        public KeyboardWindow()
        {
            InitializeComponent();
        }

        public KeyboardWindow(string key,int code, int btn) : this()
        {
            txt_Keyboard.Text = key;
            cmb_Key.SelectedIndex = btn - 1;
            keycode = code;
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            int pr_pl = cmb_Key.Text == "Press" ? 1 : 2;
            macro = new KeyboardMacro()
            {
                Control = "Keyboard",
                Action = cmb_Key.Text + "/" + txt_Keyboard.Text,
                Code = keycode,
                Btn = pr_pl

            };
            DialogResult = true;
            Close();
        }

        private void KeyToCode(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.LeftCtrl)
            {
                textBox.Text = Keys.ControlKey.ToString();
                keycode = (int)Keys.ControlKey;
            }
            else if (e.Key == Key.LeftShift)
            {
                textBox.Text = Keys.ShiftKey.ToString();
                keycode = (int)Keys.ShiftKey;
            }
            else if (e.Key == Key.System)
            {
                textBox.Text = Keys.Menu.ToString();
                keycode = (int)Keys.Menu;
            }
            else
            {
                textBox.Text = e.Key.ToString();
                keycode = KeyInterop.VirtualKeyFromKey(e.Key);
            }
            e.Handled = true;

        }
    }
}
