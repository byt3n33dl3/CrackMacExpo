using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeScammer.API
{
    public static class Kernel
    {
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
