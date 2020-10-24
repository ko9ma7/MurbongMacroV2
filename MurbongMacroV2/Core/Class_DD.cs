using System;
using System.Runtime.InteropServices;

namespace MurbongMacroV2
{
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public static class MouseBtn
    {
        public static readonly int LB = 1,
        RB = 4,
        MB = 16,
        E4B = 64,
        E5B = 256;
    }
    public struct POINT
    {
        public int x;
        public int y;
    }
    public class Class_DD
    {
        [DllImport("user32")] public static extern int GetCursorPos(out POINT pt);

        [DllImport("Kernel32")]
        private static extern IntPtr LoadLibrary(string dllfile);
        [DllImport("Kernel32")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("Kernel32")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32")]
        public static extern void Beep(int freq, int duration);

        public delegate int DD_btn(int btn);
        public delegate int DD_whl(int whl);
        public delegate int DD_key(int ddcode, int flag);
        public delegate int DD_mov(int x, int y);
        public delegate int DD_movR(int dx, int dy);
        public delegate int DD_str(string str);
        public delegate int DD_todc(int vkcode);

        public DD_btn btn;
        public DD_whl whl;
        public DD_mov mov;
        public DD_movR movR;
        public DD_key key;
        public DD_str str;
        public DD_todc todc;

        public delegate int DD_MouseMove(IntPtr hwnd, int x, int y);
        public delegate int DD_SnapPic(IntPtr hwnd, int x, int y, int w, int h);
        public delegate int DD_PickColor(IntPtr hwnd, int x, int y, int mode);
        public delegate IntPtr DD_GetActiveWindow();

        public DD_MouseMove MouseMove;
        public DD_SnapPic SnapPic;
        public DD_PickColor PickColor;
        public DD_GetActiveWindow GetActiveWindow;

        private IntPtr hModule;

        public int Load(string dllfile)
        {
            hModule = LoadLibrary(dllfile);
            if (hModule.Equals(IntPtr.Zero))
            {
                return -2;
            }
            else
            {
                return GetDDfunAddress(hModule);
            }

        }

        ~Class_DD()
        {
            if (!hModule.Equals(IntPtr.Zero))
            {
                FreeLibrary(hModule);
            }

        }


        private int GetDDfunAddress(IntPtr hwnd)
        {
            IntPtr pFuncAddr;

            pFuncAddr = GetProcAddress(hwnd, "DD_btn");
            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            btn = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_btn)) as DD_btn;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_whl");
            whl = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_whl)) as DD_whl;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_mov");
            mov = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_mov)) as DD_mov;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_key");
            key = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_key)) as DD_key;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_movR");
            movR = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_movR)) as DD_movR;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_str");
            str = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_str)) as DD_str;

            if (pFuncAddr.Equals(IntPtr.Zero)) { return -1; }
            pFuncAddr = GetProcAddress(hwnd, "DD_todc");
            todc = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_todc)) as DD_todc;

            //Improvement
            pFuncAddr = GetProcAddress(hwnd, "DD_MouseMove");
            if (!pFuncAddr.Equals(IntPtr.Zero))
            {
                MouseMove = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_MouseMove)) as DD_MouseMove;
            }

            pFuncAddr = GetProcAddress(hwnd, "DD_SnapPic");
            if (!pFuncAddr.Equals(IntPtr.Zero))
            {
                SnapPic = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_SnapPic)) as DD_SnapPic;
            }

            pFuncAddr = GetProcAddress(hwnd, "DD_PickColor");
            if (!pFuncAddr.Equals(IntPtr.Zero))
            {
                PickColor = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_PickColor)) as DD_PickColor;
            }

            pFuncAddr = GetProcAddress(hwnd, "DD_GetActiveWindow");
            if (!pFuncAddr.Equals(IntPtr.Zero))
            {
                GetActiveWindow = Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DD_GetActiveWindow)) as DD_GetActiveWindow;
            }

            if (MouseMove == null || SnapPic == null || PickColor == null || GetActiveWindow == null)
            {
                return 0;
            }

            return 1;
        }
    }

}
