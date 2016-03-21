using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace MapleRobots
{
    public abstract class Hack
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
          byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

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

        public static int ReadInt(Process process, string address)
        {
            List<int> list = GetAddressAndOffset(address);

            int value;
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            if (list.Count == 2)
            {
                ReadProcessMemory((int)process.Handle, list[0], buffer, buffer.Length, ref bytesRead);
                int baseAdr = Convert.ToInt32(buffer);
                baseAdr = baseAdr + list[1];
                ReadProcessMemory((int)process.Handle, baseAdr, buffer, buffer.Length, ref bytesRead);
                value = Convert.ToInt32(buffer);
            }
            else if (list.Count == 1)
            {
                ReadProcessMemory((int)process.Handle, list[0], buffer, buffer.Length, ref bytesRead);
                value = Convert.ToInt32(buffer);
            }
            else
                value = -1;
            return value;
        }

        public static void WriteInt(Process process, string address, int value)
        {
            List<int> list = GetAddressAndOffset(address);

            int bytesRead = 0;
            int bytesWritten = 0;
            byte[] buffer = new byte[4];
            buffer = BitConverter.GetBytes(value);

            if (list.Count == 2)
            {
                ReadProcessMemory((int)process.Handle, list[0], buffer, buffer.Length, ref bytesRead);
                int baseAdr = Convert.ToInt32(buffer);
                baseAdr = baseAdr + list[1];
                WriteProcessMemory((int)process.Handle, baseAdr, buffer, buffer.Length, ref bytesWritten);
                value = Convert.ToInt32(buffer);
            }
            else if (list.Count == 1)
            {
                WriteProcessMemory((int)process.Handle, list[0], buffer, buffer.Length, ref bytesWritten);
                value = Convert.ToInt32(buffer);
            }
            
        }

        public static List <int> GetAddressAndOffset(string address)
        {
            List<int> list = new List<int>();
            char[] addr = address.ToCharArray();
            if (addr[0] == '[')
            {
                address.IndexOf(']');
                string str = address.Substring(1, 8);
                list.Add(int.Parse(str));
                str = address.Substring(address.IndexOf(']') + 1);
                list.Add(int.Parse(str));
            }
            else
                list.Add(int.Parse(address));
            return list;
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