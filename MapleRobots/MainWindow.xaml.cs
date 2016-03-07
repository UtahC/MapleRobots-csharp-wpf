
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using System.Diagnostics;

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

        private const int HOTKEY_ID = 9000;
    
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_CAPITAL = 0x14; //CAPS LOCK

        private const int AttackCountBaseAdr = 0x00978358;
        private const int AttackCountOffset = 0x1f04;
        private const int HpAlarmBaseAdr = 0x00978134;
        private const int HpAlarmOffset = 0x74;
        private const int MpAlarmBaseAdr = 0x00978134;
        private const int MpAlarmOffset = 0x78;
        private const int HpValueBaseAdr = 0x00978360;
        private const int HpValueOffset = 0xC80;
        private const int MpValueBaseAdr = 0x00978360;
        private const int MpValueOffset = 0xC84;
        private const int ExpPercentBaseAdr = 0x00978360;
        private const int ExpPercentOffset = 0xB48;
        private const int RedCountBaseAdr = 0x00978140;
        private const int RedCountOffset = 0x18;
        private const int MobCountBaseAdr = 0x0097813C;
        private const int MobCountOffset = 0x10;
        private const int CharacterStatusBaseAdr = 0x978358;
        private const int CharacterStatusOffset = 0x52C;
        private const int CharacterXBaseAdr = 0x00979268;
        private const int CharacterXOffset = 0x59C;
        private const int CharacterYBaseAdr = 0x00979268;
        private const int CharacterYOffset = 0x5A0;
        private const int MapIDBaseAdr = 0x00979268;
        private const int MapIDOffset = 0x62C;
        private const int BreathBaseAdr = 0x00978358;
        private const int BreathOffset = 0x528;

        private static ManualResetEvent mre_Main = new ManualResetEvent(false);
        private static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);

        private String KeyWantToPress, WindowTitle, HpPotKey, MpPotKey;
        private Thread _threadOfKeyPresser;
        private int intTemp, HpBelow, MpBelow;
        private double doubleTemp;
        private IntPtr WindowHwnd;  
        private uint RunTimer = 0;

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
                            if (vkey == 0x77)
                            {
                                BindWindow();

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
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void textBox_HotKeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Forms.Keys keys = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyAutoAttack.Text = keys.ToString();
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)keys.GetHashCode());
        }

        private void textBox_KeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Forms.Keys keys = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            KeyWantToPress = keys.ToString();
            textBox_KeyAutoAttack.Text = KeyWantToPress;
        }

        private void textBox_HotKeyHp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Forms.Keys keys = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            HpPotKey = keys.ToString();
            textBox_HotKeyHp.Text = HpPotKey;
        }

        private void textBox_HotKeyMp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Forms.Keys keys = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            MpPotKey = keys.ToString();
            textBox_HotKeyMp.Text = MpPotKey;
        }

        private void textBox_HpValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HpBelow = int.Parse(textBox_HpValue.Text);
        }

        private void textBox_MpValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MpBelow = int.Parse(textBox_MpValue.Text);
        }

        private void button_Click(object sender, RoutedEventArgs e)
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
            GoToLocationInWater(-446, 500, 20, true, false);
            Debug.WriteLine("到達第 1 點");
            GoToLocationInWater(-585, 1032, 20, true, false);
            Debug.WriteLine("到達第 2 點");
            GoToLocationInWater(-428, 1220, 20, true, false);
            Debug.WriteLine("到達第 3 點");
            GoToLocationInWater(-316, 1596, 20, true, false);
            Debug.WriteLine("到達第 4 點");
            Debug.WriteLine("arrived.");
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((bool)checkBox_UnlimitedAttack.IsChecked)
                Hack.WriteInt(AttackCountBaseAdr, AttackCountOffset, WindowTitle, 0);
            Hack.WriteInt(HpAlarmBaseAdr, HpAlarmOffset, WindowTitle, 20);
            Hack.WriteInt(MpAlarmBaseAdr, MpAlarmOffset, WindowTitle, 20);
            if (HpPotKey != null && HpBelow > 0)
                if (Hack.ReadInt(HpValueBaseAdr, HpValueOffset, WindowTitle) < HpBelow)
                    Hack.KeyPress(WindowHwnd, HpPotKey);
            if (MpPotKey != null && MpBelow > 0)
                if (Hack.ReadInt(MpValueBaseAdr, MpValueOffset, WindowTitle) < MpBelow)
                    Hack.KeyPress(WindowHwnd, MpPotKey);
            intTemp = Hack.ReadInt(AttackCountBaseAdr, AttackCountOffset, WindowTitle);
            if (intTemp >= 0)
                label_AttackCount.Content = "攻擊次數: " + intTemp;
            intTemp = Hack.ReadInt(HpAlarmBaseAdr, HpAlarmOffset, WindowTitle);
            if (intTemp >= 0)
                label_HpAlert.Content = "HP警告: " + intTemp;
            intTemp = Hack.ReadInt(MpAlarmBaseAdr, MpAlarmOffset, WindowTitle);
            if (intTemp >= 0)
                label_MpAlert.Content = "MP警告: " + intTemp;
            intTemp = Hack.ReadInt(HpValueBaseAdr, HpValueOffset, WindowTitle);
            if (intTemp >= 0)
                label_HpValue.Content = "HP值: " + intTemp;
            intTemp = Hack.ReadInt(MpValueBaseAdr, MpValueOffset, WindowTitle);
            if (intTemp >= 0)
                label_MpValue.Content = "MP值: " + intTemp;
            doubleTemp = Hack.ReadDouble(ExpPercentBaseAdr, ExpPercentOffset, WindowTitle);
            if (doubleTemp > 0)
                label_Exp.Content = "經驗值: " + Math.Round(doubleTemp,2) + "%";
            intTemp = Hack.ReadInt(RedCountBaseAdr, RedCountOffset, WindowTitle);
            if (intTemp >= 0)
                label_PlayerCount.Content = "地圖玩家數量: " + intTemp;
            intTemp = Hack.ReadInt(MobCountBaseAdr, MobCountOffset, WindowTitle);
            if (intTemp >= 0)
                label_MobCount.Content = "地圖怪物數量: " + intTemp;
            intTemp = Hack.ReadInt(CharacterStatusBaseAdr, CharacterStatusOffset, WindowTitle);
            if (intTemp >= 0)
                label_CharacterStates.Content = "角色狀態: " + intTemp;
            intTemp = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
            if (intTemp >= 0)
                label_CharacterX.Content = "角色X座標: " + intTemp;
            intTemp = Hack.ReadInt(CharacterYBaseAdr, CharacterYOffset, WindowTitle);
            if (intTemp >= 0)
                label_CharacterY.Content = "角色Y座標: " + intTemp;
            intTemp = Hack.ReadInt(MapIDBaseAdr, MapIDOffset, WindowTitle);
            if (intTemp >= 0)
                label_MapID.Content = "地圖編號: " + intTemp;

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (Hack.ReadInt(BreathBaseAdr, BreathOffset, WindowTitle) > 0)
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
            WindowHwnd = Hack.GetForegroundWindow();
            WindowTitle = Hack.GetWindowTitle(WindowHwnd);
            if (WindowHwnd == null || WindowTitle == null)
            {
                System.Windows.MessageBox.Show("bind failed");
                return;
            }
            label_BindWindow.Content = ("已綁定視窗: " + WindowTitle);
            checkBox_PressKey.IsEnabled = true;

            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 1000;
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
        }

        private void KeyPresser()
        {
            while (true)
            {
                Hack.KeyPress(WindowHwnd, KeyWantToPress);
                //dm_ret = dm.KeyPressChar(KeyWantToPress);

                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }

        public int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public void GoToNearX(int coorX, int leftDistance, int rightDistance)
        {
            int CharacterX = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
            if (Distance(CharacterX, 0, coorX - 60, 0) < Distance(CharacterX, 0, coorX + 60, 0))
            {
                GoToX(coorX - leftDistance, 8, true, false);
                Hack.KeyDown(WindowHwnd, "Right");
                Thread.Sleep(1);
            }
            else
            {
                GoToX(coorX + rightDistance, 8, true, false);
                Hack.KeyDown(WindowHwnd, "Left");
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
                CharacterX = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
                CharacterY = Hack.ReadInt(CharacterYBaseAdr, CharacterYOffset, WindowTitle);
                Debug.WriteLine("trying to get " + coorX + " , " + coorY + " and now at " + CharacterX + " , " + CharacterY);
                if (CharacterX >= leftBoundary && CharacterX <= rightBoundary && CharacterY >= upBoundary && CharacterY <= downBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Up");
                    Hack.KeyUp(WindowHwnd, "Down");
                    Hack.KeyUp(WindowHwnd, "Left");
                    Hack.KeyUp(WindowHwnd, "Right");
                    Hack.KeyUp(WindowHwnd, "Menu");
                    Debug.WriteLine("arrive");
                    return;
                }
                else if (CharacterX < leftBoundary || CharacterX > rightBoundary)
                {
                    GoToX(coorX, deviation, isTeleport, isWithUp);

                }
                else if (CharacterY > downBoundary + 20)
                {
                    Hack.KeyUp(WindowHwnd, "Up");
                    Hack.KeyUp(WindowHwnd, "Down");
                    Hack.KeyPress(WindowHwnd, "Menu");
                    //Debug.WriteLine("往上");
                }
                else if (CharacterY < upBoundary - 20)
                {
                    Hack.KeyUp(WindowHwnd, "Up");
                    Hack.KeyUp(WindowHwnd, "Down");
                    Hack.KeyDown(WindowHwnd, "Down");
                    Hack.KeyPress(WindowHwnd, "Menu");
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
                CharacterX = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
                if (CharacterX >= leftBoundary && CharacterX <= rightBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Up");
                    Hack.KeyUp(WindowHwnd, "Left");
                    Hack.KeyUp(WindowHwnd, "Right");
                    return;
                }
                else if (CharacterX < leftFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Left");
                    if (isTeleport)
                        Hack.KeyPress(WindowHwnd, "ShiftKey");//可自訂
                    Hack.KeyDown(WindowHwnd, "Right");
                }
                else if (CharacterX > rightFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Right");
                    if (isTeleport)
                        Hack.KeyPress(WindowHwnd, "ShiftKey");//可自訂
                    Hack.KeyDown(WindowHwnd, "Left");
                }
                else if (CharacterX > leftFarBoundary && CharacterX < leftBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Left");
                    Hack.KeyDown(WindowHwnd, "Right");
                    if (isWithUp)
                        Hack.KeyDown(WindowHwnd, "Up");
                }
                else if (CharacterX > rightBoundary && CharacterX < rightFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, "Right");
                    Hack.KeyDown(WindowHwnd, "Left");
                    if (isWithUp)
                        Hack.KeyDown(WindowHwnd, "Up");
                }
            }
        }

        public void RopeClimbing(int coorX, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            int CharacterX, CharacterY, CharacterStatus;
            GoToNearX(coorX, leftDistance, rightDistance);
            Hack.KeyPress(WindowHwnd, "Menu");
            GoToX(coorX, 4, true, true);
            while (true)
            {
                CharacterX = Hack.ReadInt(CharacterXBaseAdr, CharacterXOffset, WindowTitle);
                CharacterY = Hack.ReadInt(CharacterYBaseAdr, CharacterYOffset, WindowTitle);
                CharacterStatus = Hack.ReadInt(CharacterStatusBaseAdr, CharacterStatusOffset, WindowTitle);
                if (CharacterY <= topBoundary && CharacterStatus != 14 && CharacterStatus != 15)
                {
                    Hack.KeyUp(WindowHwnd, "Up");
                    Hack.KeyUp(WindowHwnd, "Right");
                    Hack.KeyUp(WindowHwnd, "Left");
                    return;
                }
                else if (CharacterX >= coorX - 5 && CharacterX <= coorX + 5 && CharacterY <= floorY)
                    Hack.KeyDown(WindowHwnd, "Up");
                else
                    RopeClimbing(coorX, topBoundary, floorY, leftDistance, rightDistance);
            }
        }


    }
}
