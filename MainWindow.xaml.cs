
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace MapleRobots
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal,
        int size, string filePath);

        private const int HOTKEY_ID = 9000;

        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_CAPITAL = 0x14; //CAPS LOCK

        internal static string AttackCountAdr { get { return "[00978358]+1F04"; } }
        internal static string HpAlarmAdr { get { return "[00978134]+74"; } }
        internal static string MpAlarmAdr { get { return "[00978134]+78"; } }
        internal static string HpValueAdr { get { return "[00978360]+C80"; } }
        internal static string MpValueAdr { get { return "[00978360]+C84"; } }
        internal static string ExpPercentAdr { get { return "[00978360]+B48"; } }
        internal static string PlayerCountAdr { get { return "[00978140]+18"; } }
        internal static string MobCountAdr { get { return "[0097813C]+10"; } }
        internal static string CharacterStatusAdr { get { return "[00978358]+52C"; } }
        internal static string CharacterXAdr { get { return "[00979268]+59C"; } }
        internal static string CharacterYAdr { get { return "[00979268]+5A0"; } }
        internal static string MapIDAdr { get { return "[00979268]+62C"; } }
        internal static string BreathAdr { get { return "[00979358]+528"; } }
        internal static string CharacterNameAdr { get { return "[0097E4B0]+4"; } }
        internal static string DoorXAdr { get { return "0097EF4C"; } }
        internal static string DoorYAdr { get { return "0097EF50"; } }
        internal static Keys KeyWantToPress { get; set; }
        internal static int WindowHwnd { get; set; }
        internal static bool isBind { get; set; }
        private String WindowTitle;
        private Keys HpPotKey, MpPotKey, HotKeyAutoAttack;
        private Thread _threadOfKeyPresser, _threadOfBotting, _threadOfPickUp;
        private int HpBelow, MpBelow, dm_ret;
        private uint RunTimer = 0;
        private string filename = "data.ini";
        private string InGameName;
        private QfDm dm;
        Botting botting;

        private static ManualResetEvent mre_BottingGoby = new ManualResetEvent(false);

        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();


        public MainWindow()
        {
            InitializeComponent();
            WindowReadData windowreaddata = new WindowReadData();
            windowreaddata.Show();
        }

        private IntPtr _windowHandle;
        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)Keys.F8); //CTRL + CAPS_LOCK

            isBind = false;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == (int)Keys.F8)
                                BindWindow();
                            else if (vkey == (int)HotKeyAutoAttack)
                            {
                                if ((bool)checkBox_PressKey.IsChecked)
                                    checkBox_PressKey.IsChecked = false;
                                else
                                    checkBox_PressKey.IsChecked = true;
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            dm.Dispose();
            if (_threadOfKeyPresser != null && _threadOfKeyPresser.IsAlive)
                _threadOfKeyPresser.Abort();
            if (_threadOfBotting != null && _threadOfBotting.IsAlive)
                _threadOfBotting.Abort();
            if (_threadOfPickUp != null && _threadOfPickUp.IsAlive)
                _threadOfPickUp.Abort();
            WritePrivateProfileString(InGameName, "KeyAutoAttack", textBox_KeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyAutoAttack", textBox_HotKeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HpValue", textBox_HpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyHp", textBox_HotKeyHp.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "MpValue", textBox_MpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyMp", textBox_HotKeyMp.Text, ".\\" + filename);
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void textBox_HotKeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotKeyAutoAttack = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyAutoAttack.Text = HotKeyAutoAttack.ToString();
            //UnregisterHotKey(_windowHandle, HOTKEY_ID);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)HotKeyAutoAttack);
        }

        private void textBox_KeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeyWantToPress = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            botting.KeyWantToPress = KeyWantToPress;
            textBox_KeyAutoAttack.Text = KeyWantToPress.ToString();
        }

        private void textBox_HotKeyHp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HpPotKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyHp.Text = HpPotKey.ToString();
        }

        private void textBox_HotKeyMp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MpPotKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyMp.Text = MpPotKey.ToString();
        }

        private void textBox_HpValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                HpBelow = int.Parse(textBox_HpValue.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("請輸入數字");
            }
        }

        private void textBox_MpValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                MpBelow = int.Parse(textBox_MpValue.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("請輸入數字");
            }
        }

        private void checkBox_PressKey_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfKeyPresser == null)
            {
                _threadOfKeyPresser = new Thread(botting.KeyPresser);
                _threadOfKeyPresser.Start();
            }
            Botting.mre_KeyPresser.Set();
        }

        private void checkBox_PressKey_Unchecked(object sender, RoutedEventArgs e)
        {
            Botting.mre_KeyPresser.Reset();
        }

        private void checkBox_PickUp_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfPickUp == null)
            {
                _threadOfPickUp = new Thread(botting.PickUp);
                _threadOfPickUp.Start();
            }
        }

        private void checkBox_PickUp_Unchecked(object sender, RoutedEventArgs e)
        {
            _threadOfPickUp.Abort();
            _threadOfPickUp = null;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfBotting == null && getPointFromDB(InGameName, 0) > 0)
            {
                
                timer2.Start();
                _threadOfBotting = new Thread(Botting.bottingGoby);
                _threadOfBotting.Start();
            }
            else
            {
                int point = getPointFromDB(InGameName, 0);
                labelPoint.Content = "目前點數: " + point;
                System.Windows.MessageBox.Show("點數不足");
                checkBox.IsChecked = false;
            }
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfBotting != null)
            {
                _threadOfBotting.Abort();
                _threadOfBotting = null;
            }
            if (Botting._threadOfTraining != null)
            {
                Botting._threadOfTraining.Abort();
                Botting._threadOfTraining = null;
            }
            timer2.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((bool)checkBox_UnlimitedAttack.IsChecked)
                dm_ret = dm.DM.WriteInt(WindowHwnd, AttackCountAdr, 0, 0);
            dm_ret = dm.DM.WriteInt(WindowHwnd, HpAlarmAdr, 0, 20);
            dm_ret = dm.DM.WriteInt(WindowHwnd, MpAlarmAdr, 0, 20);
            if (HpPotKey != Keys.None && HpBelow > 0)
                if (dm.DM.ReadInt(WindowHwnd, HpValueAdr, 0) < HpBelow)
                    Hack.KeyPress((IntPtr)WindowHwnd, HpPotKey);
            if (MpPotKey != Keys.None && MpBelow > 0)
                if (dm.DM.ReadInt(WindowHwnd, MpValueAdr, 0) < MpBelow)
                    Hack.KeyPress((IntPtr)WindowHwnd, MpPotKey);
            if (dm.DM.ReadString(WindowHwnd, CharacterNameAdr, 0, 20) != InGameName)
            {
                this.Close();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //System.Windows.MessageBox.Show("start");
            int point = getPointFromDB(InGameName, -1);
            labelPoint.Content = "目前點數: " + point;
            if (point <= 0)
            {
                checkBox.IsChecked = false;
                System.Windows.MessageBox.Show("點數不足");
            }
        }

        private int getPointFromDB (string IGN, int deltaPoint)
        {
            using (var conn = new SqlConnection("Server=tcp:MapleRobots.no-ip.org,1433;Database=MapleRobots;User ID=sa;Password=bstking9g6k;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                int userPoint, updatedUserPoint;
                var cmd = conn.CreateCommand();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                List<string> macList = new List<string>();
                foreach (var nic in nics)
                {
                    // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，
                    // 我只需要 Ethernet 網卡的 MAC
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        macList.Add(nic.GetPhysicalAddress().ToString());
                    }
                }
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                    }
                }
                conn.Open();
                try
                {
                    cmd.CommandText = @"
                    SELECT Point
                    FROM dbo.RobotsUser
                    WHERE InGameName = @InGameName;";
                    cmd.Parameters.AddWithValue("@InGameName", IGN);
                    userPoint = (int)cmd.ExecuteScalar();
                    updatedUserPoint = userPoint + deltaPoint;
                    cmd.CommandText = @"
                    INSERT INTO dbo.RobotsUserLog ( InGameName, DeltaPoint, Point, Time ,MAC, IP)
                    OUTPUT INSERTED.Point
                    VALUES (@InGameName, @DeltaPoint, @updatedPoint, CURRENT_TIMESTAMP, @MAC, @IP);
                    UPDATE dbo.RobotsUser
                    SET Point = @updatedPoint, LastestTime = CURRENT_TIMESTAMP
                    WHERE InGameName = @InGameName;";
                    cmd.Parameters.AddWithValue("@DeltaPoint", deltaPoint);
                    cmd.Parameters.AddWithValue("@updatedPoint", updatedUserPoint);
                    cmd.Parameters.AddWithValue("@MAC", macList[0].ToString());
                    cmd.Parameters.AddWithValue("@IP", localIP);
                    userPoint = (int)cmd.ExecuteScalar();
                }
                catch
                {
                    updatedUserPoint = 0;
                }
                return updatedUserPoint;
            }
        }

        private void BindWindow()
        {
            dm = new QfDm();
            WindowHwnd = dm.DM.GetForegroundWindow();
            botting = new Botting();
            Botting.WindowHwnd = WindowHwnd;
            WindowReadData.WindowHwnd = WindowHwnd;
            WindowTitle = dm.DM.GetWindowTitle(WindowHwnd);
            dm_ret = dm.DM.SetWindowState(WindowHwnd, 1);
            dm_ret = dm.DM.BindWindow(WindowHwnd, "normal", "normal", "normal", 0);
            /*
            WindowHwnd = Hack.GetForegroundWindow();
            WindowTitle = Hack.GetWindowTitle(WindowHwnd);
            */
            if (WindowHwnd == 0 || WindowTitle == null)
            {
                System.Windows.MessageBox.Show("視窗綁定失敗");
                return;
            }

            InGameName = dm.DM.ReadString(WindowHwnd, CharacterNameAdr, 0, 20);
            if (InGameName == null || InGameName == "")
            {
                System.Windows.MessageBox.Show("請先進入遊戲");
                return;
            }

            label_BindWindow.Content = ("已綁定視窗: " + WindowTitle);
            checkBox_PressKey.IsEnabled = true;

            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 500;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            timer2.Interval = 10000;
            timer2.Tick += new EventHandler(timer2_Tick);
            textBox_HotKeyAutoAttack.IsEnabled = true;
            textBox_HotKeyHp.IsEnabled = true;
            textBox_HotKeyMp.IsEnabled = true;
            textBox_HpValue.IsEnabled = true;
            textBox_KeyAutoAttack.IsEnabled = true;
            textBox_MpValue.IsEnabled = true;
            checkBox_PressKey.IsEnabled = true;
            checkBox_UnlimitedAttack.IsEnabled = true;
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            if (File.Exists(".\\" + filename))
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder(30);
                    GetPrivateProfileString(InGameName, "KeyAutoAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_KeyAutoAttack.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_KeyAutoAttack.Text, out key);
                        KeyWantToPress = key;
                        botting.KeyWantToPress = KeyWantToPress;
                    }
                    else
                        textBox_KeyAutoAttack.Text = "攻擊鍵";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "HotKeyAutoAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        textBox_HotKeyAutoAttack.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyAutoAttack.Text, out HotKeyAutoAttack);
                        RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)HotKeyAutoAttack);
                    }
                    else
                        textBox_HotKeyAutoAttack.Text = "開關熱鍵";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "HpValue", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        try
                        {
                            textBox_HpValue.Text = stringBuilder.ToString();
                            HpBelow = Convert.ToInt32(textBox_HpValue.Text);
                        }
                        catch
                        {
                            textBox_HpValue.Text = "";
                            HpBelow = 0;
                        }
                    }
                    else
                        textBox_HpValue.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "HotKeyHp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        textBox_HotKeyHp.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyHp.Text, out HpPotKey);
                    }
                    else
                        textBox_HotKeyHp.Text = "補血熱鍵";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "MpValue", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        try
                        {
                            textBox_MpValue.Text = stringBuilder.ToString();
                            MpBelow = Convert.ToInt32(textBox_MpValue.Text);
                        }
                        catch
                        {
                            textBox_MpValue.Text = "";
                            MpBelow = 0;
                        }
                    }
                    else
                        textBox_MpValue.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "HotKeyMp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        textBox_HotKeyMp.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyMp.Text, out MpPotKey);
                    }
                    else
                        textBox_HotKeyMp.Text = "補魔熱鍵";
                }
                catch
                {
                    System.Windows.MessageBox.Show("設定檔錯誤，請刪除data.ini後再嘗試一次");
                }
            }
            isBind = true;
        }
    }
}
