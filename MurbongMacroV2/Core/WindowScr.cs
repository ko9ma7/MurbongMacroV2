using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfScreenHelper;

namespace MurbongMacroV2.Core
{
    /// <summary>
    /// class_DD를 윈도우10에서 멀티모니터로 사용할 시 나타나는 증상을 해결해주는 클래스.
    /// </summary>
    public class WindowScr
    {
        public Point Virtual;
        public Point Real;
        public List<Screen> Screens;
        public Screen TopLeftScreen;
        public Rect WorkArea;
        public Point Ratio;
        public WindowScr()
        {
            Virtual = new Point(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            Real = new Point(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            Screens = Screen.AllScreens.ToList();
            WorkArea = SystemInformation.WorkingArea;
            TopLeftScreen = GetLeftScreen();
            Ratio = new Point(Virtual.X / Real.X, Virtual.Y / Real.Y);
        }
        

        public Point GetMousePoint()
        {
            return MouseHelper.MousePosition;
        }
        /// <summary>
        /// 윈도우10에서는 맨 왼쪽 모니터를 기준으로 한다.
        /// </summary>
        /// <returns></returns>
        public Screen GetLeftScreen()
        {
            Screen res = Screens[0];
            double min = res.WorkingArea.TopLeft.X + res.WorkingArea.TopLeft.Y;
            foreach (Screen s in Screens)
            {
                double offset = s.WorkingArea.TopLeft.X + s.WorkingArea.TopLeft.Y;
                if (offset < min)
                {
                    res = s;
                }
            }
            return res;
        }

    }
}
