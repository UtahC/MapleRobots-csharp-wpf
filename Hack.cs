using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
//using System.Windows.Shapes;
//using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace MapleRobots
{
    public abstract class Hack
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        const uint MOUSEEVENTF_LEFTUP = 0x04;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        const uint MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory
        (
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        int lpNumberOfBytesRead
        );
        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess
        (
        int dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId
        );
        [DllImport("kernel32.dll")]
        private static extern void CloseHandle
        (
        IntPtr hObject
        );
        //写内存
        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory
        (
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        int[] lpBuffer,
        int nSize,
        IntPtr lpNumberOfBytesWritten
        );

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")] 
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll")]
	    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal,
        int size, string filePath);
        public static int GetPid(string windowTitle)
        {
            int rs = 0;
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.MainWindowTitle.IndexOf(windowTitle) != -1)
                {
                    rs = p.Id;
                    break;
                }
            }
            return rs;
        }
        //根据进程名获取PID
        public static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            foreach (Process p in arrayProcess)
            {
                return p.Id;
            }
            return 0;
        }
        //根据窗体标题查找窗口句柄（支持模糊匹配）
        public static IntPtr FindWindow(string title)
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.MainWindowTitle.IndexOf(title) != -1)
                {
                    return p.MainWindowHandle;
                }
            }
            return IntPtr.Zero;
        }
        //读取内存中的值
        public static int ReadInt(Process process, int baseAddress)
        {
            try
            {
                byte[] buffer = new byte[4];
                int byteRead = 0;
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id);
                ReadProcessMemory(hProcess, (IntPtr)baseAddress, buffer, 4, byteRead);
                CloseHandle(hProcess);
                int value = BitConverter.ToInt32(buffer, 0);
                
                return value;
            }
            catch(AccessViolationException e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
                return -1;
            }
        }
        public static int ReadInt(Process process, int baseAddress, int offset)
        {
            try
            {
                byte[] buffer = new byte[4];
                int byteRead = 0;
                int secondAddress = ReadInt(process, baseAddress);
                secondAddress = secondAddress + offset;
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id);
                ReadProcessMemory(hProcess, (IntPtr)secondAddress, buffer, 4, byteRead);
                CloseHandle(hProcess);
                int value = BitConverter.ToInt32(buffer, 0);
                return value;
            }
            catch (AccessViolationException e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
                return -1;
            }
        }
        public static double ReadDouble(Process process, int baseAddress, int offset)
        {
            try
            {
                byte[] buffer = new byte[8];
                int byteRead = 0;
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id);
                int secondAddress = ReadInt(process, baseAddress);
                secondAddress += offset;
                ReadProcessMemory(hProcess, (IntPtr)secondAddress, buffer, 8, byteRead); //将制定内存中的值读入缓冲区
                CloseHandle(hProcess);
                return BitConverter.ToDouble(buffer,0); 
            }
            catch
            {
                return -1;
            }
        }
        public static double ReadFloat(Process process, int baseAddress, int offset)
        {
            try
            {
                byte[] buffer = new byte[4];
                int byteRead = 0;
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id);
                int secondAddress = ReadInt(process, baseAddress);
                secondAddress += offset;
                ReadProcessMemory(hProcess, (IntPtr)secondAddress, buffer, 4, byteRead); //将制定内存中的值读入缓冲区
                CloseHandle(hProcess);
                return BitConverter.ToDouble(buffer, 0);
            }
            catch
            {
                return -1;
            }
        }
        public static string ReadString(Process process, int baseAddress, int offset, int words)
        {
            try
            {
                byte[] buffer = new byte[words];
                int byteRead = 0;
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id);
                int secondAddress = ReadInt(process, baseAddress);
                secondAddress += offset;
                ReadProcessMemory(hProcess, (IntPtr)secondAddress, buffer, words, byteRead); //将制定内存中的值读入缓冲区
                CloseHandle(hProcess);
                string result = System.Text.Encoding.UTF8.GetString(buffer);
                return result;
            }
            catch
            {
                return "";
            }
        }
        //将值写入指定内存地址中
        public static void WriteInt(Process process, int baseAddress, int value)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, process.Id); //0x1F0FFF 最高权限
            WriteProcessMemory(hProcess, (IntPtr)baseAddress, new int[] { value }, 4, IntPtr.Zero);
            CloseHandle(hProcess);
        } 
        public static void WriteInt(Process process, int baseAddress, int offset, int value)
        {
            int secondAddress = ReadInt(process, baseAddress);
            if (secondAddress >= 0)
            {
                secondAddress += offset;
                WriteInt(process, secondAddress, value);
            }
            else
                return ;
        }
        public static IntPtr GetForegroundWindowHwnd()
        {
            return GetForegroundWindow();
        }
        public static string GetWindowTitle(IntPtr hwnd)
        {
            HandleRef handle = new HandleRef(null, hwnd);
            int capacity = GetWindowTextLength(handle) * 2;
            StringBuilder stringBuilder = new StringBuilder(capacity);
            GetWindowText(handle, stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }
        public static void GetWindowRectangle(IntPtr hwnd, out int left, out int top, out int right, out int bottom)
        {
            RECT rct = new RECT();
            GetWindowRect(hwnd, ref rct);
            left = rct.Left;
            top = rct.Top;
            right = rct.Right;
            bottom = rct.Bottom;
        }
        public static void GetClientRectangle(IntPtr hwnd, int clientWidth, int clientHeight,
            out int left, out int top, out int right, out int bottom, out int leftBoundary, out int topBoundary)
        {
            GetWindowRectangle(hwnd, out left, out top, out right, out bottom);
            leftBoundary = ((right - clientWidth - left) / 2);
            left = left + leftBoundary;
            topBoundary = bottom - top - clientHeight - leftBoundary;
            top = top + topBoundary;
            right = left + clientWidth;
            bottom = top + clientHeight;
        }
        public static void ClientToScreen(IntPtr hwnd, int clientX, int clientY, out int screenX, out int screenY)
        {// for MapleStory only
            int left, top, right, bottom, leftBoundary, topBoundary;
            GetClientRectangle(hwnd, 800, 600, out left, out top, out right, out bottom, out leftBoundary, out topBoundary);
            screenX = clientX + left;
            screenY = clientY + top;
        }
        public static void ScreenToClient(IntPtr hwnd, int screenX, int screenY, out int clientX, out int clientY)
        {// for MapleStory only
            int left, top, right, bottom, leftBoundary, topBoundary;
            GetClientRectangle(hwnd, 800, 600, out left, out top, out right, out bottom, out leftBoundary, out topBoundary);
            clientX = screenX - left;
            clientY = screenY - top;
        }
        public static void KeyPress(IntPtr hwnd, Keys key)
        {
            PostMessage(hwnd, WM_KEYDOWN, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYDOWN));
            Thread.Sleep(1);
            PostMessage(hwnd, WM_KEYUP, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYUP)); 
        }
        public static void KeyPress(IntPtr hwnd, string keyString)
        {
            Keys key;
            Enum.TryParse(keyString, out key);
            int keyCode = key.GetHashCode();
            PostMessage(hwnd, WM_KEYDOWN, (IntPtr)keyCode, MakeKeyLparam(keyCode, WM_KEYDOWN));
            //SendMessage(hwnd, WM_KEYDOWN, (IntPtr)keyCode, IntPtr.Zero);
            PostMessage(hwnd, WM_KEYUP, (IntPtr)keyCode, MakeKeyLparam(keyCode, WM_KEYUP));
            //SendMessage(hwnd, WM_KEYUP, (IntPtr)keyCode, IntPtr.Zero);
        }
        public static void KeyDown(IntPtr hwnd, Keys key)
        {
            if (key.ToString() == "Up" || key.ToString() == "Down" || key.ToString() == "Left" || key.ToString() == "Right")
                keybd_event((byte)key, 0, 0, 0);
            else
                PostMessage(hwnd, WM_KEYDOWN, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYDOWN));
        }
        public static void KeyDown(IntPtr hwnd, string keyString)
        {
            Keys key;
            Enum.TryParse(keyString, out key);
            int keyCode = key.GetHashCode();
            if (keyString == "Up" || keyString == "Down" || keyString == "Left" || keyString == "Right")
                keybd_event((byte)keyCode, 0, 0, 0);
            else
                PostMessage(hwnd, WM_KEYDOWN, (IntPtr)keyCode, MakeKeyLparam(keyCode, WM_KEYDOWN));
        }
        public static void KeyUp(IntPtr hwnd, Keys key)
        {
            if (key.ToString() == "Up" || key.ToString() == "Down" || key.ToString() == "Left" || key.ToString() == "Right")
                keybd_event((byte)key, 0, 2, 0);
            PostMessage(hwnd, WM_KEYUP, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYUP));
        }
        public static void KeyUp(IntPtr hwnd, string keyString)
        {
            Keys key;
            Enum.TryParse(keyString, out key);
            int keyCode = key.GetHashCode();
            if (keyString == "Up" || keyString == "Down" || keyString == "Left" || keyString == "Right")
                keybd_event((byte)keyCode, 0, 2, 0);
            else
                PostMessage(hwnd, WM_KEYUP, (IntPtr)keyCode, MakeKeyLparam(keyCode, WM_KEYUP));
        }
        public static IntPtr MakeKeyLparam(int keyCode, int flag)
        {
            string s;
            string Firstbyte;
            if (flag == WM_KEYDOWN)
                Firstbyte = "00";
            else
                Firstbyte = "C0";
            int Scancode;
            Scancode = (int)MapVirtualKey((uint)keyCode, 0);
            string Secondbyte;
            Secondbyte = Convert.ToString(Scancode, 16);
            s = Firstbyte + Secondbyte + "0001";
            return (IntPtr)Convert.ToInt32(s, 16);
        }
        public static string GetColor(IntPtr hwnd, int x, int y)
        {
            Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            Point location = new Point(x, y);
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(hwnd))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            Color color = screenPixel.GetPixel(0, 0);
            return String.Format("{0:X}{1:X}{2:X}", color.R, color.G, color.B);
        }
        public static bool CompareColor(IntPtr hwnd, int x, int y, string color, string deltaColor)
        {
            string pixelColor = GetColor(hwnd, x, y);
            if (pixelColor.Length < 6)
                for (; pixelColor.Length < 6; )
                    pixelColor = "0" + pixelColor;
            int[] pixelColorArray = {Convert.ToInt32(pixelColor.Substring(0,2), 16) | 0x000000,
                                     Convert.ToInt32(pixelColor.Substring(2,2), 16) | 0x000000,
                                     Convert.ToInt32(pixelColor.Substring(4,2), 16) | 0x000000};
            int[] colorArray = {Convert.ToInt32(color.Substring(0,2), 16) | 0x000000,
                                Convert.ToInt32(color.Substring(2,2), 16) | 0x000000,
                                Convert.ToInt32(color.Substring(4,2), 16) | 0x000000};
            int[] deltaColorArray = {Convert.ToInt32(deltaColor.Substring(0,2), 16) | 0x000000,
                                     Convert.ToInt32(deltaColor.Substring(2,2), 16) | 0x000000,
                                     Convert.ToInt32(deltaColor.Substring(4,2), 16) | 0x000000};

            if (Math.Abs(pixelColorArray[0] - colorArray[0]) <= deltaColorArray[0])
                if (Math.Abs(pixelColorArray[1] - colorArray[1]) <= deltaColorArray[1])
                    if (Math.Abs(pixelColorArray[2] - colorArray[2]) <= deltaColorArray[2])
                        return true;
            return false;
        }
        public static void MoveTo(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }
        public static void GetMousePosition(out int x, out int y)
        {
            Point point = new Point();
            point = Cursor.Position;
            x = point.X;
            y = point.Y;
        }
        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, (UIntPtr)0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
        }
        public static void LeftDoubleClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, (UIntPtr)0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
        }
        public static void Rightclick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, (UIntPtr)0);
        }
        public static void RightDoubleClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, (UIntPtr)0);
            Thread.Sleep(150);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, (UIntPtr)0);
        }
        public static void LeftDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, (UIntPtr)0);
        }
        public static void LeftUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
        }
        public static void ShowMessageBox(string text)
        {
            MessageBox.Show(text, "系統提示", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                 MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        public static string iniReader (string file, string sectionName, string keyName, bool isNumOnly, 
          string defaultValue, out Keys key)
        {
            StringBuilder stringBuilder = new StringBuilder(30);
            string valueFormat, result;
            if (isNumOnly)
                valueFormat = "^[0-9]*$";
            else
                valueFormat = "^[a-zA-Z0-9]*$";
            GetPrivateProfileString(sectionName, keyName, "", stringBuilder, 30, file);
            if (Regex.IsMatch(stringBuilder.ToString(), valueFormat) && stringBuilder.ToString() != "")
            {
                result = stringBuilder.ToString();
                bool ret = Enum.TryParse(result, out key);
                if (!ret)
                    key = Keys.None;
                return result;
            }
            else
            {
                key = Keys.None;
                return defaultValue;
            }
        }
        /*
        static public bool FindPic(IntPtr hwnd, string path, int X1, int Y1, int X2, int Y2,
          string deltaColor, double sim, out int picX, out int picY)
        //if hwnd == IntPtr.Zero then it will not find the picture in background
        {
            double rate;
            using (Bitmap pic = new Bitmap(System.IO.Directory.GetCurrentDirectory() + path))
            {
                rate = (pic.Height * pic.Width) * (1 - sim);
                using (Bitmap screen = new Bitmap(pic.Width, pic.Height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics gdest = Graphics.FromImage(screen))
                    {
                        using (Graphics gsrc = Graphics.FromHwnd(hwnd))
                        {
                            IntPtr hDC = gdest.GetHdc();
                            IntPtr hSrcDC = gsrc.GetHdc();
                            for (int x = X1; x < X2 - pic.Width; x++)
                            {
                                for (int y = Y1; y < Y2 - pic.Height; y++)
                                {
                                    int retval = BitBlt(hDC, 0, 0, pic.Width, pic.Height, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                                    int sameCount = 0;
                                    int differentCount = 0;
                                    if (pic.GetHashCode() == screen.GetHashCode())
                                        sameCount = 0;
                                    for (int i = 0; i < pic.Width && (double)differentCount < rate; i++)
                                    {
                                        for (int j = 0; j < pic.Height && (double)differentCount < rate; j++)
                                        {
                                            if (screen.GetPixel(i, j).Equals(pic.GetPixel(i, j)))
                                            {
                                                sameCount++;
                                            }
                                            else
                                            {
                                                differentCount++;
                                                break;
                                            }
                                            if (sameCount == (pic.Width * pic.Height) - differentCount)
                                            {
                                                picX = x;
                                                picY = y;
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                            gdest.ReleaseHdc();
                            gsrc.ReleaseHdc();
                        }
                    }
                }
            }
            picX = -1;
            picY = -1;
            return false;
        }
        */
    }
}