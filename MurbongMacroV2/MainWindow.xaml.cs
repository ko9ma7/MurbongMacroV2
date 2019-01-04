using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfScreenHelper;
using Newtonsoft.Json;
using Microsoft.Win32;
using MurbongMacroV2.Core;
using WpfApplicationHotKey.WinApi;
using System.Threading;
using System.Windows.Media.Animation;

namespace MurbongMacroV2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly JsonSerializerSettings settings;
        private WpfApplicationHotKey.WinApi.HotKey _hotkey;


        private readonly List<Macro> MacroList;
        private readonly List<Preset> PresetList;
        private readonly Class_DD DD;
        private int keycode;
        private CancellationTokenSource cts;



        public MainWindow()
        {

            DD = new Class_DD();
            settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var state = 0;


            if (Environment.Is64BitProcess)
            {
                state = DD.Load(@".\dll\x64\DD85590.64.dll");
            }
            else
            {
                state = DD.Load(@".\dll\x64\DD85590.32.dll");
            }


            if (state == -2)
            {

                MessageBox.Show("DLL 오류");
                Application.Current.Shutdown();
            }


            MacroList = new List<Macro>();
            PresetList = new List<Preset>();



            var sDirPath = @".\preset";
            var di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)// Preset 디렉토리가 없으면 새로 만듭니다.
            {
                di.Create();
            }
        }



        private async void ParseList(List<Macro> Macros, bool Sub = false)
        {
            cts = new CancellationTokenSource();
            var index = 1.0;
            var count = Macros.Count;
            var anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(1)
            };
            M_ProgressBar.Value = 0;
            foreach (Macro M in Macros)
            {



                if (btnRun.Content.Equals("정지"))
                {

                    anim.To = index++ / count;
                    M_ProgressBar.BeginAnimation(RangeBase.ValueProperty, anim);
                    var contrl = M.Control;
                    switch (contrl)
                    {
                        case "Keyboard":

                            var Keyboard = M as KeyboardMacro;

                            DD.key(DD.todc(Keyboard.Code), Keyboard.Btn);

                            TextToolTip(Keyboard.Action, false);

                            break;

                        case "Mouse":

                            var Mouse = M as MouseMacro;

                            if (Mouse.Status == 0)
                            {

                                if (Mouse.MouseButton >= 2049)
                                {
                                    DD.whl(Mouse.MouseButton - 2050);
                                }
                                else
                                {
                                    DD.btn(Mouse.MouseButton);
                                }

                            }
                            else if (Mouse.Status == 1)
                            {
                                if (Mouse.RandStatus == 0)
                                {
                                    DD.mov((int)Mouse.Coordinate.X, (int)Mouse.Coordinate.Y);
                                }
                            }

                            TextToolTip(Mouse.Action, false);
                            break;

                        case "Delay":

                            var Delay = M as DelayMacro;

                            var ms = Delay.MS;
                            var rand1 = Delay.RandParam1;
                            var rand2 = Delay.RandParam2;

                            Random random = new Random();

                            var rand = ms + random.Next(rand1, rand2);

                            //TextToolTip(rand + " ms delayed",false);
                            try
                            {
                                await Task.Delay(rand, cts.Token); // 작업이 취소된다면 return
                            }
                            catch
                            {
                                TextToolTip("Macro Has Been Canceled");
                                return;
                            }
                            break;

                    }

                }
                else
                {
                    TextToolTip("Macro Has Been Canceled");
                    return;
                }


            }

            if (Sub == false)
            {

                TextToolTip("End Macro", false);
                RunAsync();
            }

        }

        #region MAINTAB

        private void ChangeBool(bool Bool)
        {
            btnAdd.IsEnabled = Bool;
            btnCopy.IsEnabled = Bool;
            btnDown.IsEnabled = Bool;
            btnUp.IsEnabled = Bool;
            btnSave.IsEnabled = Bool;
            btnLoad.IsEnabled = Bool;
            btnRemove.IsEnabled = Bool;
        }
        private void Btn_Run(object sender, RoutedEventArgs e)
        {
            RunAsync();
        }
        private async void RunAsync()
        {
            if (MacroList.Count == 0)
            {
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "오케이",
                    NegativeButtonText = "Cancel",
                    AnimateShow = true,
                    AnimateHide = true
                };
                var res = await this.ShowMessageAsync("오류", "아무것도 없는데?", MessageDialogStyle.Affirmative, mySettings);

                return;
            }
            if (btnRun.Content.Equals("실행"))//실행을 누르면
            {
                btnRun.Content = "정지";
                ChangeBool(false);
                ParseList(MacroList);

            }
            else if (btnRun.Content.Equals("정지"))//정지를 누르면
            {
                btnRun.Content = "실행";
                ChangeBool(true);
                cts.Cancel(); ;
            }
        }

        #endregion

        private async void Initializing(object sender, RoutedEventArgs e)
        {
            var mySettings = new LoginDialogSettings()
            {
                AffirmativeButtonText = "로그인!"
            };
            var login = await this.ShowLoginAsync("로그인하세요", "빨리하세요", mySettings);

            Refresh();

            _hotkey = new WpfApplicationHotKey.WinApi.HotKey(ModifierKeys.Control | ModifierKeys.Shift, Keys.F8, this);
            _hotkey.HotKeyPressed += (k) => RunAsync();
        }

        #region Mousebtn definition
        private int GetMouseBtn()
        {
            var pr_pl = cmb_Mouse.Text;
            var mouse = 0;

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
        private string GetMouseBtnText()
        {
            var pr_pl = cmb_Mouse.Text;
            var mouse = "";
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
        #endregion
        private void Btn_Refresh(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            var sDirPath = @".\preset";

            var di = new DirectoryInfo(sDirPath);
            PresetList.Clear();

            foreach (FileInfo file in di.GetFiles())
            {
                var str = File.ReadAllText(file.FullName);
                var Import = JsonConvert.DeserializeObject<Preset>(str);

                PresetList.Add(Import);

            }
            P_DataGrid.ItemsSource = P_DataGrid2.ItemsSource = PresetList;
            P_DataGrid.Items.Refresh();
            P_DataGrid2.Items.Refresh();
        }
        private void KeyToCode(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;


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

            TextToolTip("You Selected " + DD.todc(keycode) + " Vk Code.");

            e.Handled = true;

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            Regex regex = new Regex(@"^[-]?\d*$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Text == "-" && (!string.IsNullOrEmpty((sender as TextBox).Text.Trim()) || (sender as TextBox).Text.IndexOf('-') > -1)) e.Handled = true;
        }

        #region DataControl
        private void RemoveData(object sender, RoutedEventArgs e)
        {
            List<Macro> temp = M_DataGrid.SelectedItems.Cast<Macro>().ToList();
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                MacroList.Remove(temp[i]);
            }
            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();

            TextToolTip("Remove " + temp.Count + " Data");

        }
        private void CopyData(object sender, RoutedEventArgs e)
        {
            List<Macro> temp = M_DataGrid.SelectedItems.Cast<Macro>().ToList();
            M_DataGrid.SelectedIndex = -1;
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                MacroList.Add(temp[i]);
            }
            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
            TextToolTip("Copy " + temp.Count + " Data");

        }
        private void ImportData(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Title = "불러오기",
                DefaultExt = "json",
                Filter = "Json(*.json)|*.json",
                InitialDirectory = Directory.GetCurrentDirectory() + @"\preset"
            };
            if (openFile.ShowDialog() == true)
            {
                try
                {
                    StreamReader stream = new StreamReader(openFile.FileName);
                    var load = stream.ReadToEnd();
                    stream.Dispose();
                    stream.Close();


                    var Import = JsonConvert.DeserializeObject<Preset>(load, settings);

                    MacroList.AddRange(Import.Data);



                    M_DataGrid.ItemsSource = MacroList;
                    M_DataGrid.Items.Refresh();

                    TextToolTip(Import.Name + " Has Imported");

                }
                catch
                {
                    TextToolTip("오류가 발생했습니다.");
                }
            }
            else
            {
                TextToolTip("이씨발새끼야!!!");
            }
        }
        private void ExportData(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFile = new SaveFileDialog
            {
                Title = "저장하기",
                DefaultExt = "json",
                Filter = "Json(*.json)|*.json",
                InitialDirectory = Directory.GetCurrentDirectory() + @"\preset"

            };
            if (saveFile.ShowDialog() == true)
            {
                Preset preset = new Preset
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(saveFile.FileName),
                    Data = MacroList
                };


                string result = JsonConvert.SerializeObject(preset, settings);
                File.WriteAllText(saveFile.FileName, result);

            }
            else
            {
                TextToolTip("이씨발새끼야!!!");
            }
        }
        private void UpData(object sender, RoutedEventArgs e)
        {

            Macro select = M_DataGrid.SelectedItems[0] as Macro;
            var index = MacroList.IndexOf(select);


            if (index != 0)
            {
                MacroList.Remove(select);
                MacroList.Insert(index - 1, select);
                TextToolTip(index + 1 + " Move to Up");
            }

            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
        }
        private void DownData(object sender, RoutedEventArgs e)
        {

            Macro select = M_DataGrid.SelectedItems[0] as Macro;
            var index = MacroList.IndexOf(select);


            if (index != MacroList.Count - 1)
            {
                MacroList.Remove(select);
                MacroList.Insert(index + 1, select);
                TextToolTip(index + 1 + " Move to Down");
            }

            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
        }
        #endregion

        private async void AddPreset(object sender, RoutedEventArgs e)
        {
            TextToolTip("Wait");
            var tab = tab_Macro.SelectedItem as TabItem;
            var tabname = tab.Header.ToString();

            #region Keyboard definition

            switch (tabname)
            {
                case "키보드":
                    if (txt_Keyboard.Text != "")
                    {
                        var pr_pl = cmb_Key.Text == "Press" ? 1 : 2;
                        MacroList.Add(new KeyboardMacro()
                        {
                            Control = "Keyboard",
                            Action = cmb_Key.Text + "/" + txt_Keyboard.Text,
                            Code = keycode,
                            Btn = pr_pl

                        });
                    }
                    else
                    {
                        var mySettings = new MetroDialogSettings()
                        {
                            AffirmativeButtonText = "알겠어요",
                            NegativeButtonText = "Cancel",
                            AnimateShow = true,
                            AnimateHide = true
                        };
                        var res = await this.ShowMessageAsync("오류", "키보드 입력 안넣어?", MessageDialogStyle.Affirmative, mySettings);

                    }
                    break;

                #endregion

                #region Mouse definition

                case "마우스":

                    var tabMouse = tab_Mouse.SelectedItem as TabItem;
                    var tabMouseString = tabMouse.Header.ToString();

                    switch (tabMouseString)
                    {
                        case "MouseClick":
                            var button = GetMouseBtn();

                            MacroList.Add(new MouseMacro()
                            {
                                Control = "Mouse",
                                Action = GetMouseBtnText(),
                                Status = 0, // MouseClick
                                MouseButton = button
                            });
                            break;

                        case "MouseMove":

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

                            var MouseFactor = new Point(rx, ry);
                            var MousePoint = new Point(x, y);

                            var MouseAction = "Move/" + MousePoint.ToString();

                            if (RandStatus == 1)
                            {
                                MouseAction += "/C/" + MouseFactor.X.ToString();
                            }
                            else if (RandStatus == 2)
                            {
                                MouseAction += "/R/" + MouseFactor.ToString();
                            }

                            MacroList.Add(new MouseMacro()
                            {
                                Control = "Mouse",
                                Action = MouseAction,
                                Status = 1,//MouseMove
                                Coordinate = MousePoint,
                                RandStatus = GetRandStatus(),
                                Parameter = MouseFactor


                            });

                            break;

                    }

                    break;

                #endregion

                #region Control definition
                case "제어문":

                    var tabControl = tab_Control.SelectedItem as TabItem;
                    var tabControlString = tabControl.Header.ToString();
                    var Delay = 0;
                    var DelayStr = "Delay/";
                    var Param1 = 0;
                    var Param2 = 0;
                    switch (tabControlString)
                    {

                        #region Delay definition
                        case "Delay":

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
                                var res = await this.ShowMessageAsync("오류", "씨발놈아 제대로좀 해봐;", MessageDialogStyle.Affirmative);
                            }
                            else
                            {
                                MacroList.Add(new DelayMacro()
                                {
                                    Control = "Delay",
                                    Action = DelayStr,
                                    MS = Delay,
                                    RandParam1 = Param1,
                                    RandParam2 = Param2
                                });
                            }
                            break;

                        #endregion

                        case "LoopPreset":



                            break;
                    }

                    break;
                    #endregion
            }
            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
        }


        private async void TextToolTip(String txt, bool idle = true, int delay = 1000)// 별거 없을시에 다시 Murbong Idle로 돌아옴
        {
            TxtBlock.Text = txt;

            if (idle)
            {
                await Task.Delay(delay);
                TxtBlock.Text = "Murbong Idle";
            }

        }

    }
}
