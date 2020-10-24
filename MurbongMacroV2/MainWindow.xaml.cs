//using AutoHotkey.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using MurbongMacroV2.Core;
using MurbongMacroV2.Window;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfApplicationHotKey.WinApi;
using HotKey = WpfApplicationHotKey.WinApi.HotKey;
using Point = System.Windows.Point;

namespace MurbongMacroV2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private HotKey RunHotkey;
        private HotKey PosHotkey;
        private HotKey ParamHotkey;
        private HotKey CaptureHotkey;
        public struct TempIndeHotkey
        {
            public Keys key;
            public ModifierKeys modifier;

        }
        private TempIndeHotkey tempKey;
        private readonly JsonSerializerSettings settings;
        private readonly List<Macro> MacroList;
        private readonly List<Preset> PresetList;
        private readonly List<Independent> IndeList;
        private readonly List<Macro> Collection;
        private readonly List<string> ImageNameList;
        private readonly Class_DD DD;
        private readonly WindowScr Scr;
        //private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private readonly Listener server;
        private int keycode;
        private bool isRunning;
        private bool isWindow7;
        private bool SInputMode;
        private bool debugging = false;
        private CancellationTokenSource cts;

        /// <summary>
        /// 클라이언트 ->서버 통신하거나 매크로 실행시 설정창에서 나오는 디버깅용 창
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sendServer"></param>
        private void Debugging(string str, bool sendServer = true)
        {
            if (!debugging)
                return;
            string send = DateTime.Now + " : " + str + "\r\n";
            DebugPanel.Invoke(new Action(delegate ()
            {
                DebugPanel.AppendText(send);
                DebugPanel.Select(DebugPanel.Text.Length, 0);
                DebugPanel.ScrollToEnd();
            }));
            if (str.Equals("실행"))//클라이언트 ->서버에서 들어온 입력이 실행일때.
            {
                this.Invoke(() =>
                RunAsync(true));
            }
            else if (str.Equals("정지"))//클라이언트 ->서버에서 들어온 입력이 정지일때.
            {
                this.Invoke(() =>
                RunAsync(false));
            }
            if (sendServer && server.connectedClient != null && server.isConnected == true)
            {
                server.BeginSend(send);//클라이언트로 정보를 전송한다.
            }

        }
        public BitmapImage ConvertBitmap(Bitmap bitmap)//Bitmap을 imageSource로 바꿔주는 메소드.
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }
        public void CaptureBitmap()
        {

            try//Exception이 있을땐 실행하지 않는다.
            {
                int X_Pos = Convert.ToInt32(txt_x_image.Text);
                int Y_Pos = Convert.ToInt32(txt_y_image.Text);
                int Width = Convert.ToInt32(txt_w_image.Text);
                int Height = Convert.ToInt32(txt_h_image.Text);

                Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CopyFromScreen(X_Pos, Y_Pos, 0, 0, bmp.Size);
                }

                SaveFileDialog saveFile = new SaveFileDialog
                {
                    Title = "캡쳐저장하기",
                    DefaultExt = "bmp",
                    Filter = "bmp(*.bmp)|*.bmp",
                    InitialDirectory = Directory.GetCurrentDirectory() + @"\images"
                };
                if (saveFile.ShowDialog() == true)
                {
                    bmp.Save(saveFile.FileName);
                    bmp.Dispose();

                }
            }
            catch
            {

            }
            Refresh();
        }

        /// <summary>
        /// 윈도우 7, over 8일때 마우스 위치의 변화
        /// </summary>
        /// <param name="isWin7"></param>
        private void SetMousePos(bool isWin7 = false)
        {
            if (isWin7)
            {

                Class_DD.GetCursorPos(out POINT pos);

                //Point Pos = Scr.GetMousePoint();
                txt_x_pos.Text = pos.x.ToString();
                txt_y_pos.Text = pos.y.ToString();
                /*
                txt_x_image.Text = Pos.X.ToString();
                txt_y_image.Text = Pos.Y.ToString();
                */
            }
            else
            {
                Point Pos = Scr.GetMousePoint();
                Vector MPos = Pos - Scr.TopLeftScreen.WorkingArea.TopLeft;
                txt_x_pos.Text = MPos.X.ToString();
                txt_y_pos.Text = MPos.Y.ToString();

                txt_x_image.Text = Pos.X.ToString();
                txt_y_image.Text = Pos.Y.ToString();

                ocr_x_image.Text = Pos.X.ToString();
                ocr_y_image.Text = Pos.Y.ToString();
            }

        }

        /// <summary>
        /// 랜덤 인자를 만든다.
        /// </summary>
        private void SetFactorPos()
        {
            if (radio_None.IsChecked == true)
            {

            }
            else if (radio_Circle.IsChecked == true)
            {

                Vector Pos;
                if (isWindow7)
                {
                    Pos = (Vector)Scr.GetMousePoint();
                }
                else
                {
                    Pos = Scr.GetMousePoint() - Scr.TopLeftScreen.WorkingArea.TopLeft;
                }
                double X = 0.0;
                double Y = 0.0;
                try
                {
                    X = Pos.X - Convert.ToInt32(txt_x_pos.Text);
                    Y = Pos.Y - Convert.ToInt32(txt_y_pos.Text);
                }
                catch
                {

                }
                double Radius = Math.Sqrt(X * X + Y * Y);

                txtBox_MoveFactor.Text = Math.Round(Radius) + "";

            }
            else if (radio_Rect.IsChecked == true)
            {
                Vector Pos;
                if (isWindow7)
                {
                    Pos = (Vector)Scr.GetMousePoint();
                }
                else
                {
                    Pos = Scr.GetMousePoint() - Scr.TopLeftScreen.WorkingArea.TopLeft;
                }
                double X = 0.0;
                double Y = 0.0;
                try
                {
                    X = Pos.X - Convert.ToInt32(txt_x_pos.Text);
                    Y = Pos.Y - Convert.ToInt32(txt_y_pos.Text);
                }
                catch
                {

                }
                txtBox_MoveFactor.Text = X + ":" + Y;
            }

            if (isWindow7)
            {
                Vector Pos1 = (Vector)Scr.GetMousePoint();
                double X1 = Pos1.X - Convert.ToInt32(txt_x_pos.Text);
                double Y1 = Pos1.Y - Convert.ToInt32(txt_y_pos.Text);
                txt_w_image.Text = X1.ToString();
                txt_h_image.Text = Y1.ToString();
            }
            else
            {
                Vector Pos1 = Scr.GetMousePoint() - Scr.TopLeftScreen.WorkingArea.TopLeft;

                double X1 = Pos1.X - Convert.ToInt32(txt_x_pos.Text);
                double Y1 = Pos1.Y - Convert.ToInt32(txt_y_pos.Text);
                txt_w_image.Text = X1.ToString();
                txt_h_image.Text = Y1.ToString();

                ocr_w_image.Text = X1.ToString();
                ocr_h_image.Text = Y1.ToString();
            }
        }

        /// <summary>
        /// MainWindow가 실행될 때, 초기화하는 변수들 모음집
        /// </summary>
        public MainWindow()
        {


            DD = new Class_DD();
            Scr = new WindowScr();
            settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
            int state = 0;

            isWindow7 = false;
            isRunning = false;
            SInputMode = false;
            //ahk.ExecRaw("CoordMode,Mouse ,Screen");

            if (Environment.Is64BitProcess)
            {
                state = DD.Load(@".\dll\x64\DD64.dll");
            }
            else
            {
                state = DD.Load(@".\dll\x86\DD32.dll");
            }
            if (state == -2)
            {
                MessageBox.Show("DLL 오류이므로 오토핫키 모드로 변경됩니다 (온라인게임 사용 금지).");
                SInputMode = true;
            }
            MacroList = new List<Macro>();
            PresetList = new List<Preset>();
            Collection = new List<Macro>();
            IndeList = new List<Independent>();
            ImageNameList = new List<string>();
            server = new Listener(52323);
            server.receiving += Debugging;


            string sDirPath = @".\preset";
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)// Preset 디렉토리가 없으면 새로 만듭니다.
            {
                di.Create();
            }

            sDirPath = @".\images";
            di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)// images 디렉토리가 없으면 새로 만듭니다.
            {
                di.Create();
            }
        }

        /// <summary>
        /// 서버포트는 52323이 기본인데수.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (ServerSwitch.IsChecked.Value)
            {
                server.StartServer(52323);
                //await Task.Delay(500);
                Debugging("서버가 실행되었습니다.", false);
            }
            else
            {
                Debugging("서버가 종료되었습니다.");
                server.StopServer();
            }

        }


        /// <summary>
        /// MainWindow 로딩 끝난 뒤, 초기화하는 변수들 모음(핫키 등등)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Initializing(object sender, RoutedEventArgs e)
        {
            DebugPanel.AppendText("\n가상 모니터 크기 : " + Scr.Virtual + "\n");
            DebugPanel.AppendText("실제 모니터 크기 : " + Scr.Real + "\n");
            DebugPanel.AppendText("가장 왼쪽의 모니터 위치 : " + Scr.TopLeftScreen.WorkingArea.TopLeft + "\n");
            DebugPanel.AppendText("현재 / 가상 모니터 비율 : " + (Scr.Ratio.X + " , " + Scr.Ratio.Y) + "\n");

            Version v = Environment.OSVersion.Version;

            if (5 == v.Major && v.Minor > 0)
            {
                DebugPanel.AppendText("@ OS버전 : " + "Windows XP" + "\r\n");
            }

            else if (6 == v.Major && v.Minor == 0)
            {
                DebugPanel.AppendText("@ OS버전 : " + "Windows VISTA" + "\r\n");
            }

            else if (6 == v.Major && v.Minor == 1)
            {
                DebugPanel.AppendText("@ OS버전 : " + "Windows 7 " + "\r\n");
                isWindow7 = true;
            }
            else if (6 == v.Major && v.Minor == 2)
            {
                DebugPanel.AppendText("@ OS버젼 : " + "Over Windows 8 " + "\r\n");
            }


            if (Scr.Screens.Count > 1)
            {
                DebugPanel.AppendText("멀티윈도우 모드 가동중 \n");
            }

            Refresh();

            RunHotkey = new HotKey(ModifierKeys.None, Keys.F8, Process.GetCurrentProcess().MainWindowHandle);
            RunHotkey.HotKeyPressed += (k) => RunAsync();
            PosHotkey = new HotKey(ModifierKeys.None, Keys.F9, this);
            PosHotkey.HotKeyPressed += (k) => SetMousePos(isWindow7);
            ParamHotkey = new HotKey(ModifierKeys.None, Keys.F10, this);
            ParamHotkey.HotKeyPressed += (k) => SetFactorPos();
            CaptureHotkey = new HotKey(ModifierKeys.None, Keys.F11, this);
            CaptureHotkey.HotKeyPressed += (k) => CaptureBitmap();

            tempKey.key = Keys.F5;

        }

        /// <summary>
        /// Macro 클래스와 상속받는 하위 클래스를 분석하는 메소드.
        /// </summary>
        /// <param name="Macros">매크로 리스트</param>
        /// <param name="Token">시간 토큰</param>
        /// <param name="Time">몇번 반복할지 확인</param>
        /// <param name="Sub">지금 돌리는 매크로가 메인인지, 서브인지 구분함</param>
        private async Task ParseList(List<Macro> Macros, CancellationToken Token, int Time = 1, bool Sub = false)
        {
            int count = Macros.Count;
            DoubleAnimation anim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.2)
            };
            int LoopCount = 0;
            int EndCount = 0;
            int DelayCount = 0;
            bool Collect = false;
            int LoopTime = 0;
            anim.To = 0;
            M_ProgressBar.BeginAnimation(RangeBase.ValueProperty, anim);

            for (int i = 0; ((i < Time) || (isRunning && Time == -1)); i++)//Time만큼의 루프를 돌린다. 메인프리셋의 루프는 1이므로 서브루프일때 루프가 작동한다.
            {

                if (Token.IsCancellationRequested)
                {
                    return;//토큰(매크로 정지)가 캔슬되면 모든 루프를 중단한다.
                }

                int index = 1;

                foreach (Macro M in Macros)
                {
                    index++;
                    anim.To = index * 1.0 / count;

                    M_ProgressBar.BeginAnimation(RangeBase.ValueProperty, anim);
                    string control = M.Control;

                    switch (control)
                    {
                        case "Keyboard":

                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.
                            KeyboardMacro Keyboard = M as KeyboardMacro;
                            Debugging("키보드 " + Keyboard.Action);
                            if (SInputMode == false)
                            {
                                DD.key(DD.todc(Keyboard.Code), Keyboard.Btn);
                            }
                            else
                            {
                                string updown = Keyboard.Btn == 1 ? "down" : "up";
                                // ahk.ExecRaw("Send,{vk" + String.Format("{0:X}", Keyboard.Code) + " " + updown + "}");
                            }

                            break;
                        case "Mouse":
                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.
                            MouseMacro Mouse = M as MouseMacro;
                            Debugging("마우스 " + Mouse.Action);
                            if (Mouse.Status == 0)
                            {
                                if (SInputMode == false)
                                {
                                    if (Mouse.MouseButton >= 2049)//마우스휠은 2050+1,2로 해두었기 때문에 이렇게 해준다...
                                    {
                                        DD.whl(Mouse.MouseButton - 2050);
                                    }
                                    else
                                    {
                                        DD.btn(Mouse.MouseButton);
                                    }
                                }
                                else
                                {
                                    switch (Mouse.MouseButton)
                                    {
                                        case 1:
                                            //  ahk.ExecRaw("click down left");
                                            break;
                                        case 2:
                                            //ahk.ExecRaw("click up left");
                                            break;
                                        case 4:
                                            //ahk.ExecRaw("click down right");
                                            break;
                                        case 8:
                                            //ahk.ExecRaw("click up right");
                                            break;
                                        case 16:
                                            //ahk.ExecRaw("click down middle");
                                            break;
                                        case 32:
                                            //ahk.ExecRaw("click up middle");
                                            break;
                                        case 2051:
                                            //  ahk.ExecRaw("click wheelup");
                                            break;
                                        case 2052:
                                            //  ahk.ExecRaw("click wheeldown");
                                            break;
                                    }
                                }
                            }
                            else if (Mouse.Status == 1)
                            {
                                Random rands = new Random((int)DateTime.Now.Ticks);
                                int randX = 0;
                                int randY = 0;
                                int MovX = 0;
                                int MovY = 0;
                                if (Mouse.RandStatus == 0)
                                {
                                    MovX = (int)Mouse.Coordinate.X;
                                    MovY = (int)Mouse.Coordinate.Y;
                                }
                                else if (Mouse.RandStatus == 1)
                                {
                                    int Radius = (int)Mouse.Parameter.X;
                                    int Rad = rands.Next(0, Radius);
                                    int Degree = rands.Next(0, 360);

                                    randX = (int)(Math.Cos(Math.PI * Degree / 180) * Rad);
                                    randY = (int)(Math.Sin(Math.PI * Degree / 180) * Rad);

                                    MovX = (int)Mouse.Coordinate.X + randX;
                                    MovY = (int)Mouse.Coordinate.Y + randY;


                                }
                                else if (Mouse.RandStatus == 2)
                                {
                                    randX = rands.Next(0, (int)Mouse.Parameter.X);
                                    randY = rands.Next(0, (int)Mouse.Parameter.Y);

                                    MovX = (int)Mouse.Coordinate.X + randX;
                                    MovY = (int)Mouse.Coordinate.Y + randY;
                                }

                                if (SInputMode == false)
                                {
                                    if (isWindow7)
                                    {
                                        DD.mov(MovX, MovY);
                                    }
                                    else
                                    {
                                        DD.mov((int)(MovX * Scr.Ratio.X), (int)(MovY * Scr.Ratio.Y)); //마우스 위치  window10일때 컨버팅해준다.
                                    }
                                }
                                else
                                {
                                    //ahk.ExecRaw("mousemove," + MovX + "," + MovY);
                                }
                            }
                            break;

                        case "Delay":

                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.
                            DelayCount++;
                            DelayMacro Delay = M as DelayMacro;

                            int ms = Delay.MS;
                            int rand1 = Delay.RandParam1;
                            int rand2 = Delay.RandParam2;

                            Random random = new Random((int)DateTime.Now.Ticks);

                            int rand = ms + random.Next(rand1, rand2);
                            Debugging("지연시간 " + rand + " 밀리초");
                            try
                            {
                                await Task.Delay(rand, Token); // 작업이 취소된다면 return
                            }
                            catch
                            {
                                TextToolTip("Macro Has Been Canceled While Delay");
                                return;
                            }
                            break;

                        case "Loop":

                            ControlMacro Control = M as ControlMacro;

                            if (Control.Signal == 1)//Loop
                            {
                                LoopCount++;

                                if (Collect == true)// Loop 내의 Loop까지 컬렉션에 집어넣는다.
                                {
                                    Collection.Add(M);
                                }
                                else
                                {
                                    Collection.Clear();// Loop 내부가 아닌 첫 Loop일때 하는 일.
                                    LoopTime = Control.Parameter;
                                    Collect = true;

                                }
                            }
                            else if (Control.Signal == 0)//EndLoop
                            {
                                EndCount++;
                                if (LoopCount == EndCount)//루프의 끝이 맞아떨어지면..
                                {
                                    Collect = false;
                                    LoopCount = 0;
                                    EndCount = 0;
                                    await ParseList(Collection.ToList(), Token, LoopTime, true);//컬렉션을 parse한다.
                                }
                                else//loop 내의 endloop도 컬렉션에 집어넣는다.
                                {
                                    Collection.Add(M);
                                }
                            }
                            break;
                        case "Preset":
                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.
                            PresetMacro PM = M as PresetMacro;
                            Debugging("프리셋 " + PM.Name + " 실행");
                            foreach (Preset p in PresetList)
                            {
                                if (p.Name == PM.Name)
                                {
                                    await ParseList(p.Data, Token, 1, true);
                                    break;

                                }
                            }
                            break;
                        case "OCR":
                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.

                            OCRMacro OM = M as OCRMacro;

                            int OWidth = (int)OM.Area.X;
                            int OHeight = (int)OM.Area.Y;
                            int OX_Pos = (int)OM.Position.X;
                            int OY_Pos = (int)OM.Position.Y;

                            Bitmap obmp = new Bitmap(OWidth, OHeight, PixelFormat.Format24bppRgb);

                            using (Graphics gr = Graphics.FromImage(obmp))
                            {
                                gr.CopyFromScreen(OX_Pos, OY_Pos, 0, 0, obmp.Size);
                            }

                            Mat src = obmp.ToMat();
                            Mat dst = new Mat();
                            float[] data = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
                            Mat kernel = new Mat(3, 3, MatType.CV_32F, data);
                            Cv2.Filter2D(src, dst, src.Type(), kernel);
                            
                            string ocrfind = await OCRManager.ExtractTextAsync(dst.ToBitmap(), OM.EngKor == true ? "ko" : "en-US");

                            src.Dispose();
                            dst.Dispose();
                            kernel.Dispose();

                            try
                            {
                                Regex regex = new Regex(OM.RGX);

                                var match = regex.Match(ocrfind);

                                TextToolTip(ocrfind + " : " + match.Success,false);

                                if (match.Success)
                                {
                                    if (OM.SuccessPreset == "!STOP")
                                    {
                                        TextToolTip("End Macro");
                                        btnRun.Content = "실행";
                                        ChangeBool(true);
                                        return;
                                    }
                                    else if (OM.SuccessPreset == "!BREAK")
                                    {
                                        TextToolTip("Break", false);
                                        Time = 0;
                                        break;
                                    }
                                    else if (OM.SuccessPreset == "!CONTINUE")
                                    {
                                        TextToolTip("Continue", false);
                                        continue;
                                    }
                                    else
                                    {
                                        foreach (Preset p in PresetList)
                                        {
                                            if (p.Name == OM.SuccessPreset)
                                            {
                                                await ParseList(p.Data, Token, 1, true);
                                                break;

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (OM.FailPreset == "!STOP")
                                    {
                                        TextToolTip("End Macro");
                                        btnRun.Content = "실행";
                                        ChangeBool(true);
                                        return;
                                    }
                                    else if (OM.FailPreset == "!BREAK")
                                    {
                                        TextToolTip("Break", false);
                                        Time = 0;
                                        break;
                                    }
                                    else if (OM.FailPreset == "!CONTINUE")
                                    {
                                        TextToolTip("Continue", false);
                                        continue;
                                    }
                                    else
                                    {
                                        foreach (Preset p in PresetList)
                                        {
                                            if (p.Name == OM.FailPreset)
                                            {
                                                await ParseList(p.Data, Token, 1, true);
                                                break;

                                            }
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                MessageBox.Show("정규식 오류!!");
                            }

                            break;
                        case "Image":
                            if (Collect == true) { Collection.Add(M); continue; }//루프 내부에 있다면 컬렉션에 넣는다.
                            ImageSearchMacro IM = M as ImageSearchMacro;
                            Debugging("이미지서칭 " + IM.ImageName + " " + IM.Tolerance + "유사도로 찾는중.");
                            int Width = (int)IM.Area.X;
                            int Height = (int)IM.Area.Y;
                            int X_Pos = (int)IM.Position.X;
                            int Y_Pos = (int)IM.Position.Y;
                            string ImageName = IM.ImageName;
                            int Tolerance = IM.Tolerance;
                            bool MouseMode = IM.MouseMode;

                            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

                            using (Graphics gr = Graphics.FromImage(bmp))
                            {
                                gr.CopyFromScreen(X_Pos, Y_Pos, 0, 0, bmp.Size);
                            }

                            FileStream f = File.Open(@".\images\" + ImageName, FileMode.Open);
                            Bitmap find = new Bitmap(f);

                            f.Dispose();
                            Mat SourceMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
                            Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find);
                            Mat res = SourceMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed);
                            Cv2.MinMaxLoc(res, out double minval, out double maxval, out OpenCvSharp.Point minloc, out OpenCvSharp.Point maxloc);
                            SourceMat.Dispose();
                            FindMat.Dispose();
                            res.Dispose();
                            lbl_similarity.Content = "유사도 : " + maxval;

                            img_Source.Source = ConvertBitmap(bmp);
                            img_Find.Source = ConvertBitmap(find);
                            bmp.Dispose();
                            find.Dispose();//쓸모없어진 bmp와 find를 제거한다.

                            if (Tolerance < maxval * 100)
                            {
                                if (MouseMode == true)
                                {
                                    Debugging("이미지가 있는곳으로 마우스 이동.");
                                    double XPos = (X_Pos + maxloc.X - Scr.TopLeftScreen.WorkingArea.TopLeft.X);
                                    double YPos = (Y_Pos + maxloc.Y - Scr.TopLeftScreen.WorkingArea.TopLeft.Y);

                                    DD.mov((int)((XPos + find.Width / 2) * Scr.Ratio.X), (int)((YPos + find.Height / 2) * Scr.Ratio.Y));
                                }
                                else
                                {
                                    Debugging("이미지 발견");
                                    if (IM.SuccessPreset == "!STOP")
                                    {
                                        TextToolTip("End Macro");
                                        btnRun.Content = "실행";
                                        ChangeBool(true);
                                        return;
                                    }
                                    else if (IM.SuccessPreset == "!BREAK")
                                    {
                                        TextToolTip("Break", false);
                                        Time = 0;
                                        break;
                                    }
                                    else if (IM.SuccessPreset == "!CONTINUE")
                                    {
                                        TextToolTip("Continue", false);
                                        continue;
                                    }
                                    else
                                    {
                                        foreach (Preset p in PresetList)
                                        {
                                            if (p.Name == IM.SuccessPreset)
                                            {
                                                await ParseList(p.Data, Token, 1, true);
                                                break;

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debugging("이미지 발견 실패");
                                if (IM.FailPreset == "!STOP")
                                {
                                    TextToolTip("End Macro");
                                    btnRun.Content = "실행";
                                    ChangeBool(true);
                                    return;
                                }
                                else if (IM.FailPreset == "!BREAK")
                                {
                                    TextToolTip("Break", false);
                                    Time = 0;
                                    break;
                                }
                                else if (IM.FailPreset == "!CONTINUE")
                                {
                                    TextToolTip("Continue", false);
                                    continue;
                                }
                                else
                                {
                                    foreach (Preset p in PresetList)
                                    {
                                        if (p.Name == IM.FailPreset)
                                        {
                                            await ParseList(p.Data, Token, 1, true);
                                            break;

                                        }
                                    }
                                }
                            }
                            break;

                    }//Switch End
                }

                if (Time == -1 && DelayCount <= 0)//절대 delay없이 무한루프를 돌릴 수는 없다.
                {
                    Debugging("인터럽트 발생(무한루프)");
                    break;
                }
            }

            if (Sub == false)//메인루프일시 매크로를 정지한다.
            {
                Debugging("매크로 정지");
                TextToolTip("End Macro");
                btnRun.Content = "실행";
                ChangeBool(true);
                return;
            }
            else
            {
                return;
            }

        }

        #region MAINTAB

        /// <summary>
        /// Main 탭의 버튼을 비활성화, 활성화 시킨다.
        /// </summary>
        /// <param name="Bool"></param>
        private void ChangeBool(bool Bool)
        {
            btnAdd.IsEnabled = Bool;
            btnCopy.IsEnabled = Bool;
            btnDown.IsEnabled = Bool;
            btnUp.IsEnabled = Bool;
            btnSave.IsEnabled = Bool;
            btnLoad.IsEnabled = Bool;
            btnRemove.IsEnabled = Bool;
            isRunning = !Bool;
        }
        private void Btn_Run(object sender, RoutedEventArgs e)
        {
            RunAsync();
        }
        private async void RunAsync(bool run = true)
        {
            if (MacroList.Count == 0)
            {
                MetroDialogSettings mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "오케이",
                    NegativeButtonText = "Cancel",
                    AnimateShow = true,
                    AnimateHide = true
                };
                await this.ShowMessageAsync("오류", "아무것도 없는데?", MessageDialogStyle.Affirmative, mySettings);

                return;
            }
            if (isRunning == false && run == true)//실행을 누르면
            {
                Debugging("매크로 실행");
                btnRun.Content = "정지";
                ChangeBool(false);
                cts = new CancellationTokenSource();
                await ParseList(MacroList, cts.Token);
            }
            else//정지를 누르면
            {
                Debugging("매크로 정지");
                btnRun.Content = "실행";
                ChangeBool(true);
                cts.Cancel();
                cts.Dispose();
            }
        }
        #endregion 
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
            List<Macro> clone = M_DataGrid.SelectedItems.Cast<Macro>().ToList();
            // M_DataGrid.SelectedIndex = -1;

            string json = JsonConvert.SerializeObject(clone, settings);
            clone = JsonConvert.DeserializeObject<List<Macro>>(json, settings);

            MacroList.AddRange(clone);
            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
            TextToolTip("Copy " + clone.Count + " Data");

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
                    string load = stream.ReadToEnd();
                    stream.Dispose();
                    stream.Close();


                    Preset Import = JsonConvert.DeserializeObject<Preset>(load, settings);

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
                TextToolTip("닫았습니다.");
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

                Refresh();
            }
            else
            {
                TextToolTip("닫았습니다.");
            }
        }
        private void UpData(object sender, RoutedEventArgs e)
        {

            if (M_DataGrid.SelectedItems.Count > 0)
            {

                Macro select = M_DataGrid.SelectedItems[0] as Macro;
                int index = MacroList.IndexOf(select);



                if (index != 0)
                {
                    MacroList.Remove(select);
                    MacroList.Insert(index - 1, select);
                    TextToolTip(index + 1 + " Move to Up");
                }

                M_DataGrid.ItemsSource = MacroList;
                M_DataGrid.Items.Refresh();
            }
        }
        private void DownData(object sender, RoutedEventArgs e)
        {
            if (M_DataGrid.SelectedItems.Count > 0)
            {
                Macro select = M_DataGrid.SelectedItems[0] as Macro;
                int index = MacroList.IndexOf(select);


                if (index != MacroList.Count - 1)
                {
                    MacroList.Remove(select);
                    MacroList.Insert(index + 1, select);
                    TextToolTip(index + 1 + " Move to Down");
                }


                M_DataGrid.ItemsSource = MacroList;
                M_DataGrid.Items.Refresh();
            }
        }
        #endregion
        #region Mousebtn definition
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
        /// <summary>
        /// 프리셋 탭 -> 메인으로 보내는 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_CopyToMain(object sender, RoutedEventArgs e)
        {
            if (P_DataGrid.SelectedItems.Count > 0)
            {
                Preset select = P_DataGrid.SelectedItem as Preset;
                MacroList.AddRange(select.Data);

                M_DataGrid.ItemsSource = MacroList;
                M_DataGrid.Items.Refresh();
            }
        }
        private void Btn_SaveAll(object sender, RoutedEventArgs e)
        {

            foreach (Preset p in PresetList)
            {

                string str = JsonConvert.SerializeObject(p, settings);

                File.WriteAllText(@".\preset\" + p.Name + ".json", str);

            }
        }
        /// <summary>
        /// 폴더 안에 있는 Preset 새로고침
        /// </summary>
        private void Refresh()
        {
            string sDirPath = @".\preset";

            DirectoryInfo di = new DirectoryInfo(sDirPath);
            PresetList.Clear();
            ImageNameList.Clear();
            foreach (FileInfo file in di.GetFiles())
            {
                string str = File.ReadAllText(file.FullName);
                Preset Import = JsonConvert.DeserializeObject<Preset>(str, settings);

                PresetList.Add(Import);

            }
            ocr_fail.ItemsSource = ocr_success.ItemsSource = cmb_fail.ItemsSource = cmb_success.ItemsSource = cmb_Preset.ItemsSource = cmb_IndePreset.ItemsSource = P_DataGrid.ItemsSource = PresetList;

            cmb_IndePreset.DisplayMemberPath = cmb_Preset.DisplayMemberPath = cmb_Preset.SelectedValuePath = "Name";
            cmb_fail.DisplayMemberPath = cmb_fail.SelectedValuePath = "Name";
            cmb_success.DisplayMemberPath = cmb_success.SelectedValuePath = "Name";

            ocr_fail.DisplayMemberPath = ocr_fail.SelectedValuePath = "Name";
            ocr_success.DisplayMemberPath = ocr_success.SelectedValuePath = "Name";


            P_DataGrid.Items.Refresh();
            cmb_IndePreset.Items.Refresh();
            cmb_Preset.Items.Refresh();
            cmb_fail.Items.Refresh();
            cmb_success.Items.Refresh();
            ocr_fail.Items.Refresh();
            ocr_success.Items.Refresh();

            sDirPath = @".\images";

            di = new DirectoryInfo(sDirPath);

            foreach (FileInfo file in di.GetFiles())
            {

                string Image = file.Name;

                ImageNameList.Add(Image);

            }

            cmb_image.ItemsSource = ImageNameList;
            cmb_image.Items.Refresh();


        }
        /// <summary>
        /// 누르는 키 -> Vkcode로 바꾸는 메소드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 누르는키가 값이 숫자일 때만 핸들
        /// </summary>
        /// <param name="sender">텍스트박스</param>
        /// <param name="e">핸들</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            Regex regex = new Regex(@"^[-]?\d*$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Text == "-" && (!string.IsNullOrEmpty((sender as TextBox).Text.Trim()) || (sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 프리셋 추가하기 3갈래
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddPreset(object sender, RoutedEventArgs e)
        {
            TabItem tab = tab_Macro.SelectedItem as TabItem;
            string tabname = tab.Header.ToString();
            switch (tabname)
            {
                #region Keyboard definition
                case "키보드":
                    if (txt_Keyboard.Text != "")
                    {
                        int pr_pl = cmb_Key.Text == "Press" ? 1 : 2;
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
                        MetroDialogSettings mySettings = new MetroDialogSettings()
                        {
                            AffirmativeButtonText = "알겠어요",
                            NegativeButtonText = "Cancel",
                            AnimateShow = true,
                            AnimateHide = true
                        };
                        await this.ShowMessageAsync("오류", "키보드 입력 안넣어?", MessageDialogStyle.Affirmative, mySettings);

                    }
                    break;

                #endregion

                #region Mouse definition

                case "마우스":

                    TabItem tabMouse = tab_Mouse.SelectedItem as TabItem;
                    string tabMouseString = tabMouse.Header.ToString();

                    switch (tabMouseString)
                    {
                        case "MouseClick":
                            int button = GetMouseBtn();

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

                            MacroList.Add(new MouseMacro()
                            {
                                Control = "Mouse",
                                Action = MouseAction,
                                Status = 1,//MouseMove
                                Coordinate = MousePoint,
                                RandStatus = RandStatus,
                                Parameter = MouseFactor

                            });

                            break;

                    }

                    break;

                #endregion

                #region Control definition
                case "제어문":

                    TabItem tabControl = tab_Control.SelectedItem as TabItem;
                    string tabControlString = tabControl.Header.ToString();

                    switch (tabControlString)
                    {

                        #region Delay definition
                        case "Delay":
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
                                await this.ShowMessageAsync("오류", "잘못된 명령입니다.", MessageDialogStyle.Affirmative);
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


                        #region Loop definition

                        case "Loop":

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

                            if (M_DataGrid.SelectedItems.Count <= 0)
                            {

                                MacroList.Add(new ControlMacro()
                                {
                                    Control = "Loop",
                                    Action = "Loop # " + loopstr,
                                    Signal = 1,
                                    Parameter = loop

                                });
                                MacroList.Add(new ControlMacro()
                                {
                                    Control = "Loop",
                                    Action = "EndLoop",
                                    Signal = 0,
                                    Parameter = 0
                                });
                            }
                            else
                            {
                                List<Macro> temp = M_DataGrid.SelectedItems.Cast<Macro>().ToList();

                                int first = MacroList.IndexOf(temp[0]);//처음

                                int last = MacroList.IndexOf(temp[temp.Count - 1]) + 1;//끝

                                if (first > last)
                                {
                                    int swap = first;
                                    first = last;
                                    last = swap;

                                }

                                MacroList.Insert(last, new ControlMacro() //Last부터 하는 이유는 순서차이때문이다 시발거;;
                                {
                                    Control = "Loop",
                                    Action = "EndLoop",
                                    Signal = 0,
                                    Parameter = 0,
                                });
                                MacroList.Insert(first, new ControlMacro()
                                {
                                    Control = "Loop",
                                    Action = "Loop # " + loopstr,
                                    Signal = 1,
                                    Parameter = loop


                                });


                                TextToolTip(first + " : " + last);


                            }


                            break;

                        #endregion

                        case "Preset":

                            if (cmb_Preset.SelectedItem != null)
                            {


                                Preset SelectPreset = cmb_Preset.SelectedItem as Preset;

                                MacroList.Add(new PresetMacro()
                                {
                                    Control = "Preset",
                                    Action = "Preset Execute / " + SelectPreset.Name,
                                    Description = SelectPreset.Description,
                                    Name = SelectPreset.Name
                                });

                            }
                            break;
                        case "OCR":

                            int OX_Pos, OY_Pos;
                            int OW, OH;

                            OX_Pos = 0;
                            OY_Pos = 0;
                            OW = 0;
                            OH = 0;
                            string rex = ocr_image_regex.Text;
                            string OSuccName, OFailName;

                            try
                            {
                                OX_Pos = Convert.ToInt32(txt_x_image.Text);
                                OY_Pos = Convert.ToInt32(txt_y_image.Text);
                                OW = Convert.ToInt32(txt_w_image.Text);
                                OH = Convert.ToInt32(txt_h_image.Text);
                            }
                            catch
                            {

                            }

                            if (ocr_success.SelectedItem != null)
                            {
                                OSuccName = (cmb_success.SelectedItem as Preset).Name;
                            }
                            else
                            {
                                OSuccName = "";
                            }

                            if (ocr_fail.SelectedItem != null)
                            {
                                OFailName = (cmb_fail.SelectedItem as Preset).Name;
                            }
                            else
                            {
                                OFailName = "";
                            }

                            Point OImagePosition = new Point(OX_Pos, OY_Pos);
                            Point OImageArea = new Point(OW, OH);

                            MacroList.Add(new OCRMacro
                            {
                                Control = "OCR",
                                Action = "OCR / " + OX_Pos + " : " + OY_Pos + ", " + OW + " : " + OH + " Find " + rex + (ocr_engkor.IsChecked==true?" ko":" en-US"),
                                Position = OImagePosition,
                                Area = OImageArea,
                                RGX = rex,
                                SuccessPreset = OSuccName,
                                FailPreset = OFailName,
                                EngKor = ocr_engkor.IsChecked==true
                            });


                            break;
                        case "ImageSearch":


                            int X_Pos, Y_Pos;
                            int Width, Height;
                            int Tolerance;

                            string ImageName = cmb_image.Text;
                            string SuccName, FailName;



                            X_Pos = 0;
                            Y_Pos = 0;
                            Width = 0;
                            Height = 0;
                            Tolerance = 70;
                            try
                            {
                                X_Pos = Convert.ToInt32(txt_x_image.Text);
                                Y_Pos = Convert.ToInt32(txt_y_image.Text);
                                Width = Convert.ToInt32(txt_w_image.Text);
                                Height = Convert.ToInt32(txt_h_image.Text);
                                Tolerance = Convert.ToInt32(txt_image_tolerance.Text);
                            }
                            catch
                            {

                            }

                            if (cmb_success.SelectedItem != null)
                            {
                                SuccName = (cmb_success.SelectedItem as Preset).Name;
                            }
                            else
                            {
                                SuccName = "";
                            }

                            if (cmb_fail.SelectedItem != null)
                            {
                                FailName = (cmb_fail.SelectedItem as Preset).Name;
                            }
                            else
                            {
                                FailName = "";
                            }

                            Point ImagePosition = new Point(X_Pos, Y_Pos);
                            Point ImageArea = new Point(Width, Height);

                            bool MouseMode = chk_mousemove.IsChecked == true;

                            string str;
                            if (MouseMode == true)
                            {
                                str = " Mousemove";
                            }
                            else
                            {
                                str = " S : " + SuccName + " F : " + FailName;
                            }

                            MacroList.Add(new ImageSearchMacro()
                            {
                                Control = "Image",
                                Action = "ImageSearch / " + X_Pos + " : " + Y_Pos + ", " + Width + " : " + Height + " Find " + ImageName + " Tol : " + Tolerance + str,
                                Position = ImagePosition,
                                Area = ImageArea,
                                ImageName = ImageName,
                                Tolerance = Tolerance,
                                SuccessPreset = SuccName,
                                FailPreset = FailName,
                                MouseMode = MouseMode

                            });


                            cmb_success.SelectedIndex = cmb_fail.SelectedIndex = -1;

                            break;

                    }
                    break;
                    #endregion
            }
            M_DataGrid.ItemsSource = MacroList;
            M_DataGrid.Items.Refresh();
        }
        private async void AddIndePreset(object sender, RoutedEventArgs e)
        {
            if (cmb_IndePreset.SelectedItem != null)
            {
                try
                {
                    Independent inde = new Independent(new HotKey(tempKey.modifier, tempKey.key, this), cmb_IndePreset.SelectedItem as Preset, Txt_IndeHotkey.Text);
                    inde.Hotkey.HotKeyPressed += (k) => RunInde(inde);
                    IndeList.Add(inde);
                    IndeGrid.ItemsSource = IndeList;
                    IndeGrid.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MetroDialogSettings mySettings = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "오케이",
                        NegativeButtonText = "Cancel",
                        AnimateShow = true,
                        AnimateHide = true
                    };
                    await this.ShowMessageAsync("오류", ex.Message, MessageDialogStyle.Affirmative, mySettings);
                }
            }
        }
        private async void RunInde(Independent inde)
        {
            int i = IndeList.IndexOf(inde);
            if (inde.Activation == false)
            {
                inde.cts = new CancellationTokenSource();
                inde.Activation = true;
                IndeGrid.Items.Refresh();

                await ParseList(inde.Preset.Data, inde.cts.Token, 1, true).ContinueWith((arg) =>
                {
                    if (arg.IsCompleted)
                    {
                        inde.Activation = false;
                    }
                }
                );
            }
            else
            {
                inde.cts.Cancel();
                inde.cts.Dispose();
                inde.Activation = false;
            }

            IndeGrid.Items.Refresh();
        }
        /// <summary>
        /// 디버그용 툴팁(밑에 있는 Murbong Idle)
        /// </summary>
        /// <param name="txt">사용되는 텍스트</param>
        /// <param name="idle">Murbong Idle로 돌아가는지 선택</param>
        /// <param name="delay">Idle이 true일때만 사용</param>
        private async void TextToolTip(string txt, bool idle = true, int delay = 1000)// 별거 없을시에 다시 Murbong Idle로 돌아옴
        {
            TxtBlock.Text = txt;

            if (idle)
            {
                await Task.Delay(delay);
                TxtBlock.Text = "Murbong Idle";
            }

        }
        private void Txt_Hotkey_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            e.Handled = true;
            ModifierKeys Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) ? ModifierKeys.Control : ModifierKeys.None;
            ModifierKeys Shift = Keyboard.IsKeyDown(Key.LeftShift) ? ModifierKeys.Shift : ModifierKeys.None;
            ModifierKeys Alt = Keyboard.IsKeyDown(Key.LeftAlt) ? ModifierKeys.Alt : ModifierKeys.None;

            if (e.Key != Key.LeftCtrl && e.Key != Key.LeftShift && e.Key != Key.LeftAlt && e.Key != Key.System)
            {
                string s = "";

                if (Ctrl != ModifierKeys.None)
                {
                    s += "Ctrl + ";
                }

                if (Shift != ModifierKeys.None)
                {
                    s += "Shift + ";
                }

                if (Alt != ModifierKeys.None)
                {
                    s += "Alt + ";
                }

                s += e.Key.ToString();
                textbox.Text = s;

                if (textbox.Name == "Txt_RunHotkey")
                {
                    if (RunHotkey.Key != Keys.None)
                    {
                        RunHotkey.Dispose();
                        Keys num = Keys.None;
                        num = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
                        RunHotkey = new HotKey(Ctrl | Shift | Alt, num, this);
                        RunHotkey.HotKeyPressed += (k) => RunAsync();
                    }
                }
                else if (textbox.Name == "Txt_MouseHotkey")
                {
                    if (PosHotkey.Key != Keys.None)
                    {
                        PosHotkey.Dispose();
                        Keys num = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
                        PosHotkey = new HotKey(Ctrl | Shift | Alt, num, this);
                        PosHotkey.HotKeyPressed += (k) => SetMousePos(isWindow7);
                    }
                }
                else if (textbox.Name == "Txt_FactorHotkey")
                {
                    if (ParamHotkey.Key != Keys.None)
                    {
                        ParamHotkey.Dispose();
                        Keys num = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
                        ParamHotkey = new HotKey(Ctrl | Shift | Alt, num, this);
                        ParamHotkey.HotKeyPressed += (k) => SetFactorPos();
                    }
                }
                else if (textbox.Name == "Txt_CaptureHotkey")
                {
                    if (CaptureHotkey.Key != Keys.None)
                    {
                        CaptureHotkey.Dispose();
                        Keys num = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
                        CaptureHotkey = new HotKey(Ctrl | Shift | Alt, num, this);
                        CaptureHotkey.HotKeyPressed += (k) => CaptureBitmap();
                    }
                }
                else if (textbox.Name == "Txt_IndeHotkey")
                {

                    Keys num = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
                    tempKey.modifier = (Ctrl | Shift | Alt);
                    tempKey.key = num;

                }
            }
        }
        private void RadioCheck(object sender, RoutedEventArgs e)
        {
            txtBox_MoveFactor.Text = "";
        }
        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;

            if (toggle.Name == "Win7Switch")
            {
                isWindow7 = toggle.IsChecked.Value;
            }
            else if (toggle.Name == "SoftSwitch")
            {
                SInputMode = toggle.IsChecked.Value;
            }
        }
        private void Chk_mousemove_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_mousemove.IsChecked == true)
            {
                cmb_success.IsEnabled = false;
                cmb_fail.IsEnabled = false;
            }
            else
            {
                cmb_success.IsEnabled = true;
                cmb_fail.IsEnabled = true;
            }
        }
        private void IndeGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DataGrid grid = (DataGrid)sender;

                if (grid.SelectedItems.Count > 0)
                {
                    foreach (object row in grid.SelectedItems)
                    {
                        Independent inde = row as Independent;
                        inde.Hotkey.Dispose();
                        IndeList.Remove(inde);
                    }
                }
            }
            IndeGrid.Items.Refresh();
        }
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void EditCell(object sender, MouseButtonEventArgs e)
        {
            DataGrid data = sender as DataGrid;

            try
            {

                Macro cell = data.SelectedCells[0].Item as Macro;

                if (cell.Control == "Keyboard")
                {
                    KeyboardMacro keyb = cell as KeyboardMacro;
                    KeyboardWindow windo = new KeyboardWindow(keyb.Action.Split('/')[1], keyb.Code, keyb.Btn)
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    bool? result = windo.ShowDialog();
                    if (result == true)
                    {
                        keyb.Action = windo.macro.Action;
                        keyb.Btn = windo.macro.Btn;
                        keyb.Code = windo.macro.Code;
                    }
                }
                else if (cell.Control == "Mouse")
                {
                    MouseMacro mou = cell as MouseMacro;
                    if (mou.Status == 0)
                    {
                        MouseWindow windo = new MouseWindow
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        bool? result = windo.ShowDialog();
                        if (result == true)
                        {
                            mou.Action = windo.macro.Action;
                            mou.MouseButton = windo.macro.MouseButton;
                        }
                    }
                    else
                    {
                        MouseWindow2 windo = new MouseWindow2
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        bool? result = windo.ShowDialog();
                        if (result == true)
                        {
                            mou.Action = windo.macro.Action;
                            mou.Coordinate = windo.macro.Coordinate;
                            mou.Parameter = windo.macro.Parameter;
                            mou.RandStatus = windo.macro.RandStatus;
                        }
                    }
                }
                else if (cell.Control == "Delay")
                {
                    DelayMacro del = cell as DelayMacro;
                    DelayWindow windo = new DelayWindow(del.MS, del.RandParam1, del.RandParam2)
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    bool? result = windo.ShowDialog();
                    if (result == true)
                    {
                        del.Action = windo.macro.Action;
                        del.MS = windo.macro.MS;
                        del.RandParam1 = windo.macro.RandParam1;
                        del.RandParam2 = windo.macro.RandParam2;
                    }
                }
                else if (cell.Control == "Loop")
                {
                    ControlMacro contrl = cell as ControlMacro;
                    if (contrl.Signal == 1)
                    {
                        LoopWindow windo = new LoopWindow(contrl.Parameter)
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        bool? result = windo.ShowDialog();
                        if (result == true)
                        {
                            contrl.Action = windo.macro.Action;
                            contrl.Parameter = windo.macro.Parameter;
                            contrl.Signal = windo.macro.Signal;
                        }
                    }
                }
                M_DataGrid.ItemsSource = MacroList;
                M_DataGrid.Items.Refresh();
                Refresh();
            }
            catch
            {

            }
        }
    }
}
