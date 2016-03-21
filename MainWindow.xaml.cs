
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
using System.Media;
using System.Linq;

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
        internal static string BreathAdr { get { return "[00978358]+528"; } }
        internal static string CharacterNameAdr { get { return "[0097E4B0]+4"; } }
        internal static string DoorXAdr { get { return "0097EF4C"; } }
        internal static string DoorYAdr { get { return "0097EF50"; } }
        internal static Keys KeyWantToPress { get; set; }
        internal static int WindowHwnd { get; set; }
        internal static bool isBind { get; set; }

        private static int windowX, windowY;
        internal static int delayComboKey1, delayComboKey2;
        internal static Keys keyTeleport, keyAttack, keyDoor, keyPickUp, keySkill, keyCombo1, keyCombo2, keyJump;
        internal static string InGameName;
        internal static WindowHotKey windowhotkey;
        private String WindowTitle;
        private Keys HpPotKey, MpPotKey, HotKeyAutoAttack, HotKeyBotting;
        private Thread _threadOfKeyPresser, _threadOfBotting, _threadOfPickUp, _threadOfAlarmForPlayer;
        private Thread _threadOfTraining;
        private int HpBelow, MpBelow, dm_ret;
        private string filename = "data.ini";
        private QfDm dm, dmBotting;

        internal static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);
        internal static ManualResetEvent mre_PickUp = new ManualResetEvent(true);


        private ManualResetEvent mre_AlarmForPlayer = new ManualResetEvent(true);
        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();


        public MainWindow()
        {
            InitializeComponent();
            //WindowReadData windowreaddata = new WindowReadData();
            //windowreaddata.Show();
        }

        private IntPtr _windowHandle;
        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)Keys.F8); //CTRL + CAPS_LOCK

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
                            /*if (vkey == (int)Keys.F8)
                                BindWindow();*/
                            if (vkey == (int)HotKeyAutoAttack)
                            {
                                if ((bool)checkBox_PressKey.IsChecked)
                                    checkBox_PressKey.IsChecked = false;
                                else
                                    checkBox_PressKey.IsChecked = true;
                            }
                            handled = true;
                            break;
                        case HOTKEY_ID + 1:
                            vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == (int)HotKeyBotting)
                            {
                                if ((bool)checkBox_Botting.IsChecked)
                                    checkBox_Botting.IsChecked = false;
                                else
                                    checkBox_Botting.IsChecked = true;
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
            if (_threadOfAlarmForPlayer != null && _threadOfAlarmForPlayer.IsAlive)
                _threadOfAlarmForPlayer.Abort();
            if (Botting._threadOfTraining != null && Botting._threadOfTraining.IsAlive)
                Botting._threadOfTraining.Abort();
            if (windowhotkey != null)
                windowhotkey.Close();
            
            WritePrivateProfileString(InGameName, "KeyAutoAttack", textBox_KeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyAutoAttack", textBox_HotKeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyBotting", textBox_HotKeyBotting.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HpValue", textBox_HpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyHp", textBox_HotKeyHp.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "MpValue", textBox_MpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyMp", textBox_HotKeyMp.Text, ".\\" + filename);
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            UnregisterHotKey(_windowHandle, HOTKEY_ID + 1);
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
            if (e.Key == System.Windows.Input.Key.System)
                KeyWantToPress = Keys.Menu;
            else
                KeyWantToPress = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeyAutoAttack.Text = KeyWantToPress.ToString();
        }

        private void textBox_HotKeyHp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HpPotKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyHp.Text = HpPotKey.ToString();
        }

        private void comboBox_BottingCase_Loaded(object sender, RoutedEventArgs e)
        {
            List <string> data = new List <string> ();
            data.Add("魚窩");
            data.Add("籃水靈");
            comboBox_BottingCase.ItemsSource = data;
            // Make the first item selected.
            comboBox_BottingCase.SelectedIndex = 0;
        }

        private void comboBox_Process_Loaded(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            List<string> data = new List<string>();
            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                //Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                if (theprocess.ProcessName.Contains(".tmp"))
                {
                    data.Add(theprocess.ProcessName);
                    counter++;
                }
            }
            if (counter <= 0)
            {
                foreach (Process theprocess in processlist)
                {
                    //Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                    if (theprocess.ProcessName.Contains("MapleRoyals"))
                    {
                        data.Add(theprocess.ProcessName);
                    }

                }
            }
            comboBox_Process.ItemsSource = data;
            // Make the first item selected.
            comboBox_Process.SelectedIndex = -1;
        }

        private void comboBox_Process_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                //Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                if (theprocess.ProcessName == comboBox_Process.SelectedItem.ToString())
                {
                    BindWindow(theprocess);
                    comboBox_Process.IsEnabled = false;
                }
            }
        }

        private void textBox_HotKeyBotting_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotKeyBotting = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyBotting.Text = HotKeyBotting.ToString();
            //UnregisterHotKey(_windowHandle, HOTKEY_ID);
            RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, (uint)HotKeyBotting);
        }

        private void button_keySetting_Click(object sender, RoutedEventArgs e)
        {
            windowhotkey = new WindowHotKey();
            windowhotkey.Show();
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
                System.Windows.Forms.MessageBox.Show("請輸入數字");
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
                System.Windows.Forms.MessageBox.Show("請輸入數字");
            }
        }

        private void checkBox_PressKey_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfKeyPresser == null)
            {
                _threadOfKeyPresser = new Thread(KeyPresser);
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
                _threadOfPickUp = new Thread(PickUp);
                _threadOfPickUp.Start();
            }
        }

        private void checkBox_PickUp_Unchecked(object sender, RoutedEventArgs e)
        {
            _threadOfPickUp.Abort();
            _threadOfPickUp = null;
        }

        private void checkBox_Botting_Checked(object sender, RoutedEventArgs e)
        {
            if (keyTeleport == Keys.None || keyPickUp == Keys.None || keyAttack == Keys.None
                || keyJump == Keys.None || windowhotkey != null)
            {
                checkBox_Botting.IsChecked = false;
                System.Windows.Forms.MessageBox.Show("請先完成設定熱鍵");
                return;
            }
            else
            {
                if (_threadOfBotting == null && getPointFromDB(InGameName, 0) > 0)
                {
                    timer2.Start();
                    if (comboBox_BottingCase.SelectedIndex == -1)
                    {
                        timer2.Stop();
                        checkBox_Botting.IsChecked = false;
                        System.Windows.Forms.MessageBox.Show("請先選擇練功地點");
                        return;
                    }
                    else if (comboBox_BottingCase.SelectedItem.ToString() == "魚窩")
                    {
                        //Botting.dmBotting = new QfDm();
                        _threadOfBotting = new Thread(bottingGoby);
                    }
                    _threadOfBotting.Start();
                    button_keySetting.IsEnabled = false;
                    comboBox_BottingCase.IsReadOnly = true;
                }
                else
                {
                    int point = getPointFromDB(InGameName, 0);
                    labelPoint.Content = "點數: " + point;
                    System.Windows.Forms.MessageBox.Show("點數不足");
                    checkBox_Botting.IsChecked = false;
                    checkBox_Botting.IsEnabled = false;
                    textBox_HotKeyBotting.IsEnabled = false;
                    button_keySetting.IsEnabled = false;
                    comboBox_BottingCase.IsEnabled = false;
                    return;
                }
            }
        }
        private void checkBox_Botting_Unchecked(object sender, RoutedEventArgs e)
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
            button_keySetting.IsEnabled = true;
            comboBox_BottingCase.IsReadOnly = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((bool)checkBox_UnlimitedAttack.IsChecked)
                dm_ret = dm.DM.WriteInt(WindowHwnd, AttackCountAdr, 0, 0);
            if ((bool)checkBox_Potions.IsChecked)
            {
                dm_ret = dm.DM.WriteInt(WindowHwnd, HpAlarmAdr, 0, 20);
                dm_ret = dm.DM.WriteInt(WindowHwnd, MpAlarmAdr, 0, 20);
                if (HpPotKey != Keys.None && HpBelow > 0)
                    if (dm.DM.ReadInt(WindowHwnd, HpValueAdr, 0) < HpBelow)
                        Hack.KeyPress((IntPtr)WindowHwnd, HpPotKey);
                if (MpPotKey != Keys.None && MpBelow > 0)
                    if (dm.DM.ReadInt(WindowHwnd, MpValueAdr, 0) < MpBelow)
                        Hack.KeyPress((IntPtr)WindowHwnd, MpPotKey);
            }
            if ((bool)checkBox_NoBreath.IsChecked)
                dm_ret = dm.DM.WriteInt(WindowHwnd, BreathAdr, 0, 0);
            if ((bool)checkBox_Botting.IsChecked)
            {
                if (dm.DM.ReadInt(WindowHwnd, PlayerCountAdr, 0) > 0)
                {
                    if (_threadOfAlarmForPlayer == null)
                    {
                        _threadOfAlarmForPlayer = new Thread(alarmForPlayer);
                        _threadOfAlarmForPlayer.Start();
                    }
                    mre_AlarmForPlayer.Set();
                }
            }
            else
            {
                mre_AlarmForPlayer.Reset();
            }
            //if (dm.DM.GetForegroundWindow() != WindowHwnd)
                //checkBox_Botting.IsChecked = false;
            if (dm.DM.ReadString(WindowHwnd, CharacterNameAdr, 0, 20) != InGameName)
            {
                this.Close();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("start");
            int point = getPointFromDB(InGameName, -1);
            labelPoint.Content = "點數: " + point;
            if (point <= 0)
            {
                checkBox_Botting.IsChecked = false;
                System.Windows.Forms.MessageBox.Show("點數不足");
                checkBox_Botting.IsEnabled = false;
                textBox_HotKeyBotting.IsEnabled = false;
                button_keySetting.IsEnabled = false;
                comboBox_BottingCase.IsEnabled = false;
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
                try
                {
                    conn.Open();
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("無法連接伺服器");
                    this.Close();
                }
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

        private bool checkBanned()
        {
            using (var conn = new SqlConnection("Server=tcp:MapleRobots.no-ip.org,1433;Database=MapleRobots;User ID=sa;Password=bstking9g6k;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"))
            {
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
                //System.Windows.Forms.MessageBox.Show("ign = \"" + InGameName + "\"");
                //System.Windows.Forms.MessageBox.Show("ip = \"" + localIP + "\"");
                //System.Windows.Forms.MessageBox.Show("mac = \"" + macList[0].ToString()+"\"");
                try
                {
                    cmd.CommandText = @"
                    SELECT InGameName
                    FROM dbo.RobotsBannedUser
                    WHERE InGameName = @InGameName;";
                    cmd.Parameters.AddWithValue("@InGameName", InGameName);
                    string temp = (string)cmd.ExecuteScalar();
                    if (temp == null)
                    {
                        cmd.CommandText = @"
                        SELECT MAC
                        FROM dbo.RobotsBannedUser
                        WHERE MAC = @MAC;";
                        cmd.Parameters.AddWithValue("@MAC", macList[0].ToString());
                        temp = (string)cmd.ExecuteScalar();
                        if (temp == null)
                        {
                            cmd.CommandText = @"
                            SELECT IP
                            FROM dbo.RobotsBannedUser
                            WHERE IP = @IP;";
                            cmd.Parameters.AddWithValue("@IP", localIP);
                            temp = (string)cmd.ExecuteScalar();
                            if (temp == null)
                            {
                                return false;
                            }
                        }
                    }
                }
                catch
                {
                    try
                    {
                        cmd.CommandText = @"
                        SELECT MAC
                        FROM dbo.RobotsBannedUser
                        WHERE MAC = @MAC;";
                        cmd.Parameters.AddWithValue("@MAC", macList[0].ToString());
                        string temp = (string)cmd.ExecuteScalar();
                    }
                    catch
                    {
                        try
                        {
                            cmd.CommandText = @"
                            SELECT IP
                            FROM dbo.RobotsBannedUser
                            WHERE IP = @IP;";
                            cmd.Parameters.AddWithValue("@IP", localIP);
                            string temp = (string)cmd.ExecuteScalar();
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        private void BindWindow(Process process)
        {
            dm = new QfDm();
            WindowHwnd = (int)process.MainWindowHandle;
            Botting.WindowHwnd = WindowHwnd;
            WindowReadData.WindowHwnd = WindowHwnd;
            WindowTitle = process.MainWindowTitle;
            dm_ret = dm.DM.SetWindowState(WindowHwnd, 1);
            dm_ret = dm.DM.BindWindow(WindowHwnd, "normal", "normal", "normal", 0);
            /*
            WindowHwnd = Hack.GetForegroundWindow();
            WindowTitle = Hack.GetWindowTitle(WindowHwnd);
            */
            if (WindowHwnd == 0 || WindowTitle == null)
            {
                System.Windows.Forms.MessageBox.Show("視窗綁定失敗");
                return;
            }
            else if (!WindowTitle.Contains("MapleRoyals"))
            {
                System.Windows.Forms.MessageBox.Show("此程式僅適用於MapleRoyals");
                return;
            }

            InGameName = dm.DM.ReadString(WindowHwnd, CharacterNameAdr, 0, 20);
            if (InGameName == null || InGameName == "")
            {
                System.Windows.Forms.MessageBox.Show("請先進入遊戲");
                return;
            }

            if (checkBanned())//檢查是否被BAN
            {
                System.Windows.MessageBox.Show("伺服器拒絕存取");
                this.Close();
                return;
            }

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
                    GetPrivateProfileString(InGameName, "HotKeyBotting", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        textBox_HotKeyBotting.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyBotting.Text, out HotKeyBotting);
                        RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, (uint)HotKeyBotting);
                    }
                    else
                        textBox_HotKeyAutoAttack.Text = "開關熱鍵";
                    stringBuilder.Clear();
                    GetPrivateProfileString(InGameName, "HpValue", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
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
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
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
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyTeleport", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        keyTeleport = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyPickUp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyPickUp = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyAttack = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyJump", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyJump = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyDoor", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyDoor = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeySkill", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keySkill = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo1", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyCombo1 = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo2", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        Enum.TryParse(stringBuilder.ToString(), out key);
                        MainWindow.keyCombo2 = key;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo1Delay", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
                    {
                        int delay;
                        delay = int.Parse(stringBuilder.ToString());
                        MainWindow.delayComboKey1 = delay;
                    }
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo2Delay", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
                    {
                        int delay;
                        delay = int.Parse(stringBuilder.ToString());
                        MainWindow.delayComboKey2 = delay;
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("設定檔錯誤，請刪除data.ini後再嘗試一次");
                }
            }
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
            checkBox_Potions.IsEnabled = true;
            checkBox_NoBreath.IsEnabled = true;
            checkBox_PickUp.IsEnabled = true;

            int point = getPointFromDB(InGameName, 0);
            labelPoint.Content = "點數: " + point;
            if (point > 0)
            {
                checkBox_Botting.IsEnabled = true;
                textBox_HotKeyBotting.IsEnabled = true;
                button_keySetting.IsEnabled = true;
                comboBox_BottingCase.IsEnabled = true;
            }
            isBind = true;
        }
        private void alarmForPlayer()
        {
            //dm = new QfDm();
            //dm.DM.SetPath(".\\data");
            while (true)
            {
                //int ret = dm.DM.Play("alarm.mp3");
                //if (ret == 0)
                //    System.Windows.MessageBox.Show("播放失敗");
                SystemSounds.Beep.Play();
                Thread.Sleep(800);
                mre_AlarmForPlayer.WaitOne();
            }
        }

        internal void PickUp()
        {
            while (true)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, MainWindow.keyPickUp);
                Thread.Sleep(50);
                mre_PickUp.WaitOne();
            }
        }
        internal void KeyPresser()
        {
            while (true)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, KeyWantToPress);
                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }
        internal void Attack(int times)
        {
            times = times * 50;
            while (times >= 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyAttack);
                Thread.Sleep(50);
                times--;
            }
        }
        internal int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        internal void GoToNearX(int coorX, int leftDistance, int rightDistance)
        {
            //dmBotting = new QfDm();
            int CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
            if (Distance(CharacterX, 0, coorX - 60, 0) < Distance(CharacterX, 0, coorX + 60, 0))
            {
                GoToX(coorX - leftDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Thread.Sleep(1);
            }
            else
            {
                GoToX(coorX + rightDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                Thread.Sleep(1);
            }
        }

        internal void GoToLocationInWater(int coorX, int coorY, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
        {
            //dmBotting = new QfDm();
            int CharacterX, CharacterY;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int upBoundary = coorY - deviation * 2;
            int downBoundary = coorY;
            Debug.WriteLine("trying to get to " + coorX + " , " + coorY);
            if (isWithUp)
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
            while (true)
            {
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dmBotting.DM.ReadInt(WindowHwnd, CharacterYAdr, 0);
                //Debug.WriteLine("trying to get " + coorX + " , " + coorY + " and now at " + CharacterX + " , " + CharacterY);
                if (dmBotting.DM.ReadInt(WindowHwnd, MapIDAdr, 0) == targetMapID)
                {
                    Debug.WriteLine("arrive " + targetMapID);
                    return;
                }
                else if (CharacterX >= leftBoundary && CharacterX <= rightBoundary && CharacterY >= upBoundary && CharacterY <= downBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Debug.WriteLine("arrive location");
                    return;
                }
                else if (CharacterX < leftBoundary || CharacterX > rightBoundary)
                {
                    if (isWithUp && Distance(coorX, coorY, CharacterX, CharacterY) < 200)
                        GoToX(coorX, deviation, isTeleport, isWithUp, targetMapID);
                    else
                        GoToX(coorX, deviation, isTeleport, false, targetMapID);
                }
                else if (CharacterY > downBoundary + 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
                }
                else if (CharacterY < upBoundary - 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Menu);
                }

            }

        }

        internal void GoToX(int coorX, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
        {
            //dmBotting = new QfDm();
            int CharacterX;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int leftFarBoundary = coorX - 150;
            int rightFarBoundary = coorX + 150;
            while (true)
            {
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                if (dmBotting.DM.ReadInt(WindowHwnd, MapIDAdr, 0) == targetMapID)
                {
                    Debug.WriteLine("arrive " + targetMapID);
                    return;
                }
                else if (CharacterX >= leftBoundary && CharacterX <= rightBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    return;
                }
                else if (CharacterX < leftFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                    if (isTeleport)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > rightFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                    if (isTeleport)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > leftFarBoundary && CharacterX < leftBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                    if (isWithUp)
                        Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
                else if (CharacterX > rightBoundary && CharacterX < rightFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                    if (isWithUp)
                        Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
            }
        }

        internal void RopeClimbing(bool isClimbToTop, int coorX, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            //dmBotting = new QfDm();
            int CharacterX, CharacterY, CharacterStatus;
            GoToNearX(coorX, leftDistance, rightDistance);
            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
            GoToX(coorX, 4, true, true, 0);
            while (true)
            {
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dmBotting.DM.ReadInt(WindowHwnd, CharacterYAdr, 0);
                CharacterStatus = dmBotting.DM.ReadInt(WindowHwnd, CharacterStatusAdr, 0);
                if (isClimbToTop && CharacterY <= topBoundary && CharacterStatus != 14 && CharacterStatus != 15)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (!isClimbToTop && (CharacterStatus == 14 || CharacterStatus == 15))
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (CharacterX >= coorX - 5 && CharacterX <= coorX + 5 && CharacterY <= floorY)
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                else
                    RopeClimbing(isClimbToTop, coorX, topBoundary, floorY, leftDistance, rightDistance);
            }
        }
        internal void bottingBubbling()
        {
            //dmBotting = new QfDm();

        }
        internal void bottingGoby()
        {
            dmBotting = new QfDm();
            object outX = -1, outY = -1;
            int lastItemX, lastItemY;
            string lastItemColor, lastItemColor2;
            dmBotting.DM.BindWindow(WindowHwnd, "normal", "normal", "normal", 0);
            dmBotting.DM.SetPath(".\\data");
            object windowX1, windowX2, windowY1, windowY2;
            dmBotting.DM.GetWindowRect(WindowHwnd, out windowX1, out windowY1, out windowX2, out windowY2);
            Debug.WriteLine(windowX1 + "," + windowY1 + "," + windowX2 + "," + windowY2);
            windowX = ((int)windowX2 - 800) / 2;
            windowY = ((int)windowY2 - 600) - windowX;
            Debug.WriteLine(windowX + "," + windowY);
            //MessageBox.Show(windowX + "," + windowY);
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "ItemInventory.bmp", "000000", 0.9, 0, out outX, out outY) < 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.I);
                Thread.Sleep(1000);
            }
            //MessageBox.Show("ItemInventoryX = " + outX + ", ItemInventoryY = " + outY);
            lastItemX = (int)outX + 123 + windowX;
            lastItemY = (int)outY + 225 + windowY;
            Debug.WriteLine("lastItemX = " + lastItemX + ", lastItemY = " + lastItemY);
            _threadOfTraining = new Thread(bottingGobyTraining);
            _threadOfTraining.Start();
            lastItemColor = dmBotting.DM.GetColor(lastItemX, lastItemY);
            while (true)
            {
                lastItemColor = dmBotting.DM.GetColor(lastItemX, lastItemY);
                char[] array1 = lastItemColor.ToArray();
                lastItemColor2 = dmBotting.DM.GetColor(lastItemX + 10, lastItemY - 10);
                char[] array2 = lastItemColor2.ToArray();
                int sum1 = 0, sum2 = 0;
                for (int i = 0; i < 6; i++)
                {
                    sum1 = sum1 + array1[i];
                    sum2 = sum2 + array2[i];
                }
                if (sum1 < 594 || sum1 > 606 || sum2 < 594 || sum2 > 606)
                {
                    //MessageBox.Show("color = " + lastItemColor + ", color2 = " + lastItemColor2);
                    Debug.WriteLine("color = " + lastItemColor + ", color2 = " + lastItemColor2);
                    _threadOfTraining.Abort();
                    _threadOfTraining = null;
                    bottingGobyShopping();
                    _threadOfTraining = new Thread(bottingGobyTraining);
                    _threadOfTraining.Start();
                }
                Thread.Sleep(2000);
            }
        }

        internal void bottingGobyTraining()
        {
            dmBotting = new QfDm();
            //Random random = new Random(Guid.NewGuid().GetHashCode());
            int counter = 0;
            while (true)
            {
                GoToLocationInWater(-446, 520, 20, true, false, 0);
                Attack(2);
                if (counter % 3 == 0)
                {
                    mre_PickUp.Reset();
                    //GoToX(-689, 20, false, false, 0);
                    if (counter % 6 == 0)
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 10; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1500);
                    }
                    mre_PickUp.Set();
                }
                else if (counter % 3 == 1)
                    GoToX(-265, 20, false, false, 0);
                GoToLocationInWater(-585, 1032, 20, true, false, 0);
                Attack(1);
                GoToLocationInWater(-428, 1220, 20, true, false, 0);
                Attack(1);
                if (counter % 3 == 1)
                    GoToLocationInWater(-83, 1582, 20, false, false, 0);
                else
                    GoToLocationInWater(-316, 1596, 20, true, false, 0);
                Attack(1);
                do
                {
                    Attack(1);
                } while (dmBotting.DM.ReadInt(WindowHwnd, MobCountAdr, 0) > 21);
                if (counter % 3 == 0)
                    GoToLocationInWater(-829, 875, 20, true, false, 0);
                else if (counter % 3 == 1)
                    GoToLocationInWater(-83, 450, 20, false, false, 0);
                counter++;
            }
        }

        internal void bottingGobyShopping()
        {
            //dmBotting = new QfDm();
            // Goby -> Aquarium
            dmBotting.DM.SetPath(".\\data");
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000000)
            {
                int doorX, doorY, tempDoorX, tempDoorY;
                doorX = dmBotting.DM.ReadInt(WindowHwnd, DoorXAdr, 0);
                doorY = dmBotting.DM.ReadInt(WindowHwnd, DoorYAdr, 0);
                tempDoorX = doorX;
                tempDoorY = doorY;
                while (doorX == tempDoorX && doorY == tempDoorY)
                {
                    doorX = dmBotting.DM.ReadInt(WindowHwnd, DoorXAdr, 0);
                    doorY = dmBotting.DM.ReadInt(WindowHwnd, DoorYAdr, 0);
                    Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyDoor);
                    Thread.Sleep(1000);
                }
                DateTime time_start = DateTime.Now;
                double result = 0;
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230040100 && result < 8000)
                {
                    GoToLocationInWater(doorX, doorY, 5, true, true, 230000000);
                    DateTime time_end = DateTime.Now;//計時結束 取得目前時間
                                                     //後面的時間減前面的時間後 轉型成TimeSpan即可印出時間差
                    result = ((TimeSpan)(time_end - time_start)).TotalMilliseconds;
                }
            }
            mre_PickUp.Reset();
            // Aquarium -> Aquarium Store
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000002)
            {
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000000)
                {
                    GoToLocationInWater(190, 11, 5, true, true, 230000002);
                }
            }
            // Selling
            object outX, outY;
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY) < 0)
            {
                dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellerInAquarium.bmp", "000000", 0.9, 0, out outX, out outY);
                if ((int)outX > 0 && (int)outY > 0)
                {
                    dmBotting.DM.MoveTo((int)outX, (int)outY);
                    dmBotting.DM.LeftDoubleClick();
                }
            }
            selling();
            // Aquarium Store -> Aquarium
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000000)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000002)
                {
                    GoToX(-349, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                    GoToX(-351, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                    GoToX(-347, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
            }
            // Aquarium -> Goby
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230040100)
            {
                GoToLocationInWater(195, 340, 5, true, true, 230040100);
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000000)
                {
                    GoToLocationInWater(674, 340, 5, true, true, 230040100);
                }
            }
            mre_PickUp.Set();
        }

        internal void selling()
        {
            //dmBotting = new QfDm();
            object outX, outY;
            dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY);
            int Xi = (int)outX;
            int Yi = (int)outY + 104;
            Debug.WriteLine("SellItemX = " + outX + ", SellItemY = " + outY);
            string sureSellColor1, sureSellColor2;
            sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
            while (dmBotting.DM.FindPic(403 + windowX, 256 + windowY, 439 + windowX, 292 + windowY, "EmptyEqu.bmp", "000000", 0.7, 0, out outX, out outY) < 0)
            {
                dmBotting.DM.MoveTo(Xi, Yi);
                dmBotting.DM.LeftClick();
                dmBotting.DM.LeftClick();
                dmBotting.DM.LeftClick();
                sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
                sureSellColor2 = dmBotting.DM.GetColor(481 + windowX, 271 + windowY);
                if (sureSellColor1 == "4488bb" && sureSellColor2 == "4488bb")
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
            }
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY) >= 0)
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
        }
    }
}

