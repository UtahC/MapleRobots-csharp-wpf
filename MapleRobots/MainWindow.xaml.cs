
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

        private const string AttackCountAdr = "[00978358]+1F04";
        private const string HpAlarmAdr = "[00978134]+74";
        private const string MpAlarmAdr = "[00978134]+78";
        private const string HpValueAdr = "[00978360]+C80";
        private const string MpValueAdr = "[00978360]+C84";
        private const string ExpPercentAdr = "[00978360]+B48";
        private const string PlayerCountAdr = "[00978140]+18";
        private const string MobCountAdr = "[0097813C]+10";
        private const string CharacterStatusAdr = "[00978358]+52C";
        private const string CharacterXAdr = "[00979268]+59C";
        private const string CharacterYAdr = "[00979268]+5A0";
        private const string MapIDAdr = "[00979268]+62C";
        private const string BreathAdr = "[00979358]+528";


        private static ManualResetEvent mre_BottingGoby = new ManualResetEvent(false);
        private static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);

        private String WindowTitle;
        private Keys HpPotKey, MpPotKey, KeyWantToPress, HotKeyAutoAttack;
        private Thread _threadOfKeyPresser, _threadOfBotting, _threadOfPickUp;
        private int intTemp, HpBelow, MpBelow, dm_ret;
        private double doubleTemp;
        private int WindowHwnd;  
        private uint RunTimer = 0;
        private string filename = "data.ini";
        CDmSoft dm = new CDmSoft();


        public MainWindow()
        {
            InitializeComponent();
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
            //RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, 0x77); //CTRL + CAPS_LOCK
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
            WritePrivateProfileString("Setting", "KeyAutoAttack", textBox_KeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString("Setting", "HotKeyAutoAttack", textBox_HotKeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString("Setting", "HpValue", textBox_HpValue.Text, ".\\" + filename);
            WritePrivateProfileString("Setting", "HotKeyHp", textBox_HotKeyHp.Text, ".\\" + filename);
            WritePrivateProfileString("Setting", "MpValue", textBox_MpValue.Text, ".\\" + filename);
            WritePrivateProfileString("Setting", "HotKeyMp", textBox_HotKeyMp.Text, ".\\" + filename);
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
                _threadOfKeyPresser = new Thread(KeyPresser);
                _threadOfKeyPresser.Start();
            }
            mre_KeyPresser.Set();
        }

        private void checkBox_PressKey_Unchecked(object sender, RoutedEventArgs e)
        {
            mre_KeyPresser.Reset();
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

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfBotting == null)
            {
                _threadOfBotting = new Thread(BottingGoby);
                _threadOfBotting.Start();
            }
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _threadOfBotting.Abort();
            _threadOfBotting = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((bool)checkBox_UnlimitedAttack.IsChecked)
                dm_ret = dm.WriteInt(WindowHwnd, AttackCountAdr, 0, 0);
            dm_ret = dm.WriteInt(WindowHwnd, HpAlarmAdr, 0, 20);
            dm_ret = dm.WriteInt(WindowHwnd, MpAlarmAdr, 0, 20);
            if (HpPotKey != Keys.None && HpBelow > 0)
                if (dm.ReadInt(WindowHwnd, HpValueAdr, 0) < HpBelow)
                    Hack.KeyPress((IntPtr)WindowHwnd, HpPotKey);
            if (MpPotKey != Keys.None && MpBelow > 0)
                if (dm.ReadInt(WindowHwnd, MpValueAdr, 0) < MpBelow)
                    Hack.KeyPress((IntPtr)WindowHwnd, MpPotKey);
            //intTemp = Hack.ReadInt(AttackCountBaseAdr, AttackCountOffset, WindowTitle);
            //if (intTemp >= 0)
                label_AttackCount.Content = "攻擊次數: " + dm.ReadInt(WindowHwnd, AttackCountAdr, 0);
            //intTemp = Hack.ReadInt(HpAlarmBaseAdr, HpAlarmOffset, WindowTitle);
            //if (intTemp >= 0)
                label_HpAlert.Content = "HP警告: " + dm.ReadInt(WindowHwnd, HpAlarmAdr, 0);
            //intTemp = Hack.ReadInt(MpAlarmBaseAdr, MpAlarmOffset, WindowTitle);
            //if (intTemp >= 0)
                label_MpAlert.Content = "MP警告: " + dm.ReadInt(WindowHwnd, MpAlarmAdr, 0);
            //intTemp = Hack.ReadInt(HpValueBaseAdr, HpValueOffset, WindowTitle);
            //if (intTemp >= 0)
                label_HpValue.Content = "HP值: " + dm.ReadInt(WindowHwnd, HpValueAdr, 0);
            //intTemp = Hack.ReadInt(MpValueBaseAdr, MpValueOffset, WindowTitle);
            //if (intTemp >= 0)
                label_MpValue.Content = "MP值: " + dm.ReadInt(WindowHwnd, MpValueAdr, 0);
            //doubleTemp = Hack.ReadDouble(ExpPercentBaseAdr, ExpPercentOffset, WindowTitle);
            //if (doubleTemp > 0)
                label_Exp.Content = "經驗值: " + Math.Round(dm.ReadDouble(WindowHwnd, ExpPercentAdr),2) + "%";
            //intTemp = Hack.ReadInt(RedCountBaseAdr, RedCountOffset, WindowTitle);
            //if (intTemp >= 0)
                label_PlayerCount.Content = "地圖玩家數量: " + dm.ReadInt(WindowHwnd, PlayerCountAdr, 0);
            //intTemp = Hack.ReadInt(MobCountBaseAdr, MobCountOffset, WindowTitle);
            //if (intTemp >= 0)
                label_MobCount.Content = "地圖怪物數量: " + dm.ReadInt(WindowHwnd, MobCountAdr, 0);
            //intTemp = Hack.ReadInt(CharacterStatusBaseAdr, CharacterStatusOffset, WindowTitle);
            //if (intTemp >= 0)
                label_CharacterStates.Content = "角色狀態: " + dm.ReadInt(WindowHwnd, CharacterStatusAdr, 0);
            //intTemp = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
            //if (intTemp >= 0)
                label_CharacterX.Content = "角色X座標: " + dm.ReadInt(WindowHwnd, CharacterXAdr, 0);
            //intTemp = Hack.ReadInt(CharacterYBaseAdr, CharacterYOffset, WindowTitle);
            //if (intTemp >= 0)
            label_CharacterY.Content = "角色Y座標: " + dm.ReadInt(WindowHwnd, CharacterYAdr, 0);
            //intTemp = Hack.ReadInt(MapIDBaseAdr, MapIDOffset, WindowTitle);
            //if (intTemp >= 0)
                label_MapID.Content = "地圖編號: " + dm.ReadInt(WindowHwnd, MapIDAdr, 0);

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (dm.ReadInt(WindowHwnd, BreathAdr, 0) > 0)
            {
                RunTimer += 1;
                //if (RunTimer > 3600)
                //label16.Text = RunTimer / 3600 + ":" + RunTimer % 3600 / 60 + ":" + RunTimer % 60;
                //else if (RunTimer > 60)
                //label16.Text = RunTimer / 60 + ":" + RunTimer % 60;
                //else
                //label16.Text = RunTimer.ToString();
            }
        }

        private void BindWindow()
        {
            WindowHwnd = dm.GetForegroundWindow();
            WindowTitle = dm.GetWindowTitle(WindowHwnd);
            dm_ret = dm.SetWindowState(WindowHwnd, 1);
            dm_ret = dm.BindWindow(WindowHwnd, "normal", "normal", "normal", 0);
            /*
            WindowHwnd = Hack.GetForegroundWindow();
            WindowTitle = Hack.GetWindowTitle(WindowHwnd);
            */
            if (WindowHwnd == 0 || WindowTitle == null)
            {
                System.Windows.MessageBox.Show("bind failed");
                return;
            }
            label_BindWindow.Content = ("已綁定視窗: " + WindowTitle);
            checkBox_PressKey.IsEnabled = true;

            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 500;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
            timer2.Interval = 1000;
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Start();

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
                    GetPrivateProfileString("Setting", "KeyAutoAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
                    {
                        textBox_KeyAutoAttack.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_KeyAutoAttack.Text, out KeyWantToPress);
                    }
                    else
                        textBox_KeyAutoAttack.Text = "攻擊鍵";
                    GetPrivateProfileString("Setting", "HotKeyAutoAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
                    {
                        textBox_HotKeyAutoAttack.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyAutoAttack.Text, out HotKeyAutoAttack);
                        RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)HotKeyAutoAttack);
                    }
                    else
                        textBox_HotKeyAutoAttack.Text = "開關熱鍵";
                    GetPrivateProfileString("Setting", "HpValue", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
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
                    GetPrivateProfileString("Setting", "HotKeyHp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
                    {
                        textBox_HotKeyHp.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_HotKeyHp.Text, out HpPotKey);
                    }
                    else
                        textBox_HotKeyHp.Text = "補血熱鍵";
                    GetPrivateProfileString("Setting", "MpValue", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
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
                    GetPrivateProfileString("Setting", "HotKeyMp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$"))
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
        }

        private void KeyPresser()
        {
            while (true)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, KeyWantToPress);
                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }

        private void BottingGoby()
        {
            //RopeClimbing(-45, 1425, 1515, 40, 40);
            //RopeClimbing(259, 1185, 1425, 70, -70);
            //RopeClimbing(-205, 975, 1185, 40, 40);
            /*  -147,91
                -316,454
                -446,470
                -585,1032
                -428,1120
                -316,1576*/
            Random random = new Random(Guid.NewGuid().GetHashCode());
            while (true)
            {
                GoToLocationInWater(-446, 500, 20, true, false);
                Debug.WriteLine("到達第 1 點");
                Attack(2);
                GoToLocationInWater(-585, 1032, 20, true, false);
                Debug.WriteLine("到達第 2 點");
                Attack(1);
                GoToLocationInWater(-428, 1220, 20, true, false);
                Debug.WriteLine("到達第 3 點");
                Attack(1);
                GoToLocationInWater(-316, 1596, 20, true, false);
                Debug.WriteLine("到達第 4 點");
                //for (int i = 0; i < 5 && dm.ReadInt(WindowHwnd, MobCountAdr, 0) >= 15; i++)
                Attack(1);
                do
                {
                    Attack(1);
                } while (dm.ReadInt(WindowHwnd, MobCountAdr, 0) > 21);
                Debug.WriteLine("arrived.");
            }
        }

        public void Attack(int times)
        {
            times = times * 50;
            while (times >= 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.C);
                Thread.Sleep(50);
                times--;
            }
        }

        public void PickUp()
        {
            while (true)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Z);
                Thread.Sleep(50);
            }
        }
        public int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public void GoToNearX(int coorX, int leftDistance, int rightDistance)
        {
            int CharacterX = dm.ReadInt(WindowHwnd, CharacterXAdr, 0);
            if (Distance(CharacterX, 0, coorX - 60, 0) < Distance(CharacterX, 0, coorX + 60, 0))
            {
                GoToX(coorX - leftDistance, 8, true, false);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Thread.Sleep(1);
            }
            else
            {
                GoToX(coorX + rightDistance, 8, true, false);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                Thread.Sleep(1);
            }
        }

        public void GoToLocationInWater(int coorX, int coorY, int deviation, bool isTeleport, bool isWithUp)
        {
            int CharacterX, CharacterY;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int upBoundary = coorY - deviation * 2;
            int downBoundary = coorY;
            while (true)
            {
                CharacterX = dm.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dm.ReadInt(WindowHwnd, CharacterYAdr, 0); ;
                Debug.WriteLine("trying to get " + coorX + " , " + coorY + " and now at " + CharacterX + " , " + CharacterY);
                if (CharacterX >= leftBoundary && CharacterX <= rightBoundary && CharacterY >= upBoundary && CharacterY <= downBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Debug.WriteLine("arrive");
                    return;
                }
                else if (CharacterX < leftBoundary || CharacterX > rightBoundary)
                {
                    GoToX(coorX, deviation, isTeleport, isWithUp);

                }
                else if (CharacterY > downBoundary + 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Menu);
                    //Debug.WriteLine("往上");
                }
                else if (CharacterY < upBoundary - 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Menu);
                    //Debug.WriteLine("往下");
                }

            }

        }

        public void GoToX(int coorX, int deviation, bool isTeleport, bool isWithUp)
        {
            int CharacterX;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int leftFarBoundary = coorX - 150;
            int rightFarBoundary = coorX + 150;
            while (true)
            {
                CharacterX = dm.ReadInt(WindowHwnd, CharacterXAdr, 0);
                if (CharacterX >= leftBoundary && CharacterX <= rightBoundary)
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
                        Hack.KeyPress((IntPtr)WindowHwnd, Keys.ShiftKey);
                }
                else if (CharacterX > rightFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                    if (isTeleport)
                        Hack.KeyPress((IntPtr)WindowHwnd, Keys.ShiftKey);
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

        public void RopeClimbing(int coorX, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            int CharacterX, CharacterY, CharacterStatus;
            GoToNearX(coorX, leftDistance, rightDistance);
            Hack.KeyPress((IntPtr)WindowHwnd, Keys.Menu);
            GoToX(coorX, 4, true, true);
            while (true)
            {
                CharacterX = dm.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dm.ReadInt(WindowHwnd, CharacterYAdr, 0);
                CharacterStatus = dm.ReadInt(WindowHwnd, CharacterStatusAdr, 0);
                if (CharacterY <= topBoundary && CharacterStatus != 14 && CharacterStatus != 15)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (CharacterX >= coorX - 5 && CharacterX <= coorX + 5 && CharacterY <= floorY)
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                else
                    RopeClimbing(coorX, topBoundary, floorY, leftDistance, rightDistance);
            }
        }


    }
}
