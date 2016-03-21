using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace MapleRobots
{
    public abstract class Hack
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;

        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory
        (
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        IntPtr lpBuffer,
        int nSize,
        IntPtr lpNumberOfBytesRead
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

        //获取窗体的进程标识ID
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
        public static int ReadMemoryValue(int baseAddress, string processName)
        {
            try
            {
                byte[] buffer = new byte[4];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0); //获取缓冲区地址
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName));
                ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 4, IntPtr.Zero); //将制定内存中的值读入缓冲区
                CloseHandle(hProcess);
                int value = Marshal.ReadInt32(byteAddress);
                return value;
            }
            catch(AccessViolationException e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
                return -1;
            }
        }
        public static double ReadMemoryValueDouble(int baseAddress, string processName)
        {
            try
            {
                byte[] buffer = new byte[8];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0); //获取缓冲区地址
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName));
                ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 8, IntPtr.Zero); //将制定内存中的值读入缓冲区
                CloseHandle(hProcess);


                return BitConverter.ToDouble(buffer,0); 
            }
            catch
            {
                return -1;
            }
        }
        //将值写入指定内存地址中
        public static void WriteMemoryValue(int baseAddress, string processName, int value)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName)); //0x1F0FFF 最高权限
            WriteProcessMemory(hProcess, (IntPtr)baseAddress, new int[] { value }, 4, IntPtr.Zero);
            CloseHandle(hProcess);
        }
        public static int ReadInt(int baseAddress, int offset, string processName)
        {
            int address = ReadMemoryValue(baseAddress, processName);
            if (address >= 0)
            {
                address += offset;
                int value = ReadMemoryValue(address, processName);
                return value;
            }
            else
                return -1;
        }
        public static double ReadDouble(int baseAddress, int offset, string processName)
        {
            int address = ReadMemoryValue(baseAddress, processName);
            if (address >= 0)
            {
                address += offset;
                double value = ReadMemoryValueDouble(address, processName);
                return value;
            }
            else
                return -1;
        }
        public static void WriteInt(int baseAddress, int offset, string processName, int value)
        {
            int address = ReadMemoryValue(baseAddress, processName);
            if (address >= 0)
            {
                address += offset;
                WriteMemoryValue(address, processName, value);
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
        public static void KeyPress(IntPtr hwnd, Keys key)
        {
            PostMessage(hwnd, WM_KEYDOWN, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYDOWN));
            Thread.Sleep(1);
            PostMessage(hwnd, WM_KEYUP, (IntPtr)key, MakeKeyLparam((int)key, WM_KEYDOWN)); 
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
        
        
        
    }
}