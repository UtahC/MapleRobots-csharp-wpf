﻿
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
using System.Reflection;
using System.Data;

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
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(int hWnd, StringBuilder title, int size);

        Version programVersion;
        bool isOldVersion = false, autoUpdated = true;

        private const int HOTKEY_ID = 9000;

        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_CAPITAL = 0x14; //CAPS LOCK

        internal static int AttackCountBaseAdr { get { return 0x00978358; } }
        internal static int AttackCountOffset { get { return 0x1f04; } }
        internal static int HpAlarmBaseAdr { get { return 0x00978134; } }
        internal static int HpAlarmOffset { get { return 0x74; } }
        internal static int MpAlarmBaseAdr { get { return 0x00978134; } }
        internal static int MpAlarmOffset { get { return 0x78; } }
        internal static int HpValueBaseAdr { get { return 0x00978360; } }
        internal static int HpValueOffset { get { return 0xC80; } }
        internal static int MpValueBaseAdr { get { return 0x00978360; } }
        internal static int MpValueOffset { get { return 0xC84; } }
        internal static int ExpPercentBaseAdr { get { return 0x00978360; } }
        internal static int ExpPercentOffset { get { return 0xB48; } }
        internal static int PlayerCountBaseAdr { get { return 0x00978140; } }
        internal static int PlayerCountOffset { get { return 0x18; } }
        internal static int MobCountBaseAdr { get { return 0x0097813C; } }
        internal static int MobCountOffset { get { return 0x10; } }
        internal static int CharacterStatusBaseAdr { get { return 0x978358; } }
        internal static int CharacterStatusOffset { get { return 0x52C; } }
        internal static int CharacterXBaseAdr { get { return 0x00979268; } }
        internal static int CharacterXOffset { get { return 0x59C; } }
        internal static int CharacterYBaseAdr { get { return 0x00979268; } }
        internal static int CharacterYOffset { get { return 0x5A0; } }
        internal static int MapIDBaseAdr { get { return 0x00979268; } }
        internal static int MapIDOffset { get { return 0x62C; } }
        internal static int BreathBaseAdr { get { return 0x00978358; } }
        internal static int BreathOffset { get { return 0x528; } }
        internal static int CharacterNameBaseAdr { get { return 0x0097E4B0; } }
        internal static int CharacterNameOffset { get { return 0x4; } }
        internal static int DoorXBaseAdr { get { return 0x0097EF4C; } }
        internal static int DoorYBaseAdr { get { return 0x0097EF50; } }
        internal static IntPtr WindowHwnd { get; set; }
        internal static bool isBind { get; set; }

        internal static int delayComboKey1, delayComboKey2, attackParam;
        internal static int delaySkill1, delaySkill2, delaySkill3, delaySkill4, delaySkill5;
        internal static int timeSkill1, timeSkill2, timeSkill3, timeSkill4, timeSkill5;
        internal static Keys keyTeleport, keyAttack, keyDoor, keyPickUp, keySkill, keyCombo1, keyCombo2, keyJump;
        internal static Keys keyWantToPress, keySkill1, keySkill2, keySkill3, keySkill4, keySkill5;
        internal static string InGameName;
        internal static WindowHotKey windowhotkey;
        internal static Process process;
        private String WindowTitle;
        private Keys HpPotKey, MpPotKey, HotKeyAutoAttack, HotKeyBotting, HotKeyBossing;
        private Thread _threadOfKeyPresser, _threadOfBotting, _threadOfPickUp, _threadOfAlarmForPlayer;
        private Thread _threadOfSkill1, _threadOfSkill2, _threadOfSkill3, _threadOfSkill4, _threadOfSkill5;
        private Thread _threadOfBossing, _threadOfSelling;
        private int HpBelow, MpBelow, PlayerCountAlarm, TrialAvailable = 0;
        private string filename = "data.ini";

        private ManualResetEvent mre_AlarmForPlayer = new ManualResetEvent(true);
        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timer3 = new System.Windows.Forms.Timer();


        public MainWindow()
        {
            Version serverVersion;
            string strVersion = "";
            using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                var cmd = conn.CreateCommand();
                try
                {
                    conn.Open();
                }
                catch
                {
                    Hack.ShowMessageBox("無法連接伺服器");
                    this.Close();
                }
                try
                {
                    cmd.CommandText = @"
                    SELECT NowVersion
                    FROM dbo.RobotsVersion";

                    strVersion = (string)cmd.ExecuteScalar();

                }
                catch
                {

                }
            }
            programVersion = Assembly.GetEntryAssembly().GetName().Version;
            serverVersion = new Version(strVersion);
            if (programVersion.CompareTo(serverVersion) >= 0)
            {
                File.Delete("_MapleRobots.exe");
            }
            else
            {
                isOldVersion = true;
                DownLoadFile();
            }

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
                        case HOTKEY_ID + 2:
                            vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == (int)HotKeyBossing)
                            {
                                if ((bool)checkBox_Bossing.IsChecked)
                                    checkBox_Bossing.IsChecked = false;
                                else
                                    checkBox_Bossing.IsChecked = true;
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
            if (_threadOfKeyPresser != null && _threadOfKeyPresser.IsAlive)
                _threadOfKeyPresser.Abort();
            if (_threadOfBotting != null && _threadOfBotting.IsAlive)
                _threadOfBotting.Abort();
            if (_threadOfPickUp != null && _threadOfPickUp.IsAlive)
                _threadOfPickUp.Abort();
            if (_threadOfBossing != null && _threadOfBossing.IsAlive)
                _threadOfBossing.Abort();
            if (_threadOfAlarmForPlayer != null && _threadOfAlarmForPlayer.IsAlive)
                _threadOfAlarmForPlayer.Abort();
            if (BottingBase._threadOfTraining != null && BottingBase._threadOfTraining.IsAlive)
                BottingBase._threadOfTraining.Abort();
            if (windowhotkey != null)
                windowhotkey.Close();
            Hack.KeyUp(WindowHwnd, Keys.Up);
            Hack.KeyUp(WindowHwnd, Keys.Down);
            Hack.KeyUp(WindowHwnd, Keys.Left);
            Hack.KeyUp(WindowHwnd, Keys.Right);
            WritePrivateProfileString(InGameName, "KeyAutoAttack", textBox_KeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyAutoAttack", textBox_HotKeyAutoAttack.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyBossing", textBox_HotKeyBossing.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "KeySkill1", textBox_KeySkill1.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill1Delay", textBox_SkillDelay1.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill1Time", textBox_SkillTime1.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "KeySkill2", textBox_KeySkill2.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill2Delay", textBox_SkillDelay2.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill2Time", textBox_SkillTime2.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "KeySkill3", textBox_KeySkill3.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill3Delay", textBox_SkillDelay3.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill3Time", textBox_SkillTime3.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "KeySkill4", textBox_KeySkill4.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill4Delay", textBox_SkillDelay4.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill4Time", textBox_SkillTime4.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "KeySkill5", textBox_KeySkill5.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill5Delay", textBox_SkillDelay5.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "Skill5Time", textBox_SkillTime5.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyBotting", textBox_HotKeyBotting.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HpValue", textBox_HpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyHp", textBox_HotKeyHp.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "MpValue", textBox_MpValue.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "HotKeyMp", textBox_HotKeyMp.Text, ".\\" + filename);
            WritePrivateProfileString(InGameName, "PlayerCountAlarm", textBox_PlayerAlarm.Text, ".\\" + filename);
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            UnregisterHotKey(_windowHandle, HOTKEY_ID + 1);
            UnregisterHotKey(_windowHandle, HOTKEY_ID + 2);
            base.OnClosed(e);
        }

        private void textBox_HotKeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotKeyAutoAttack = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyAutoAttack.Text = HotKeyAutoAttack.ToString();
            //UnregisterHotKey(_windowHandle, HOTKEY_ID);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)HotKeyAutoAttack);
        }

        private void textBox_HotKeyBossing_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotKeyBossing = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyBossing.Text = HotKeyBossing.ToString();
            //UnregisterHotKey(_windowHandle, HOTKEY_ID);
            RegisterHotKey(_windowHandle, HOTKEY_ID + 2, MOD_NONE, (uint)HotKeyBossing);
        }

        private void textBox_KeyAutoAttack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.System)
                keyWantToPress = Keys.Menu;
            else
                keyWantToPress = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeyAutoAttack.Text = keyWantToPress.ToString();
        }

        private void textBox_HotKeyHp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HpPotKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_HotKeyHp.Text = HpPotKey.ToString();
        }

        private void textBox_KeySkill1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keySkill1 = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeySkill1.Text = keySkill1.ToString();
        }

        private void textBox_KeySkill2_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keySkill2 = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeySkill2.Text = keySkill2.ToString();
        }

        private void textBox_KeySkill3_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keySkill3 = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeySkill3.Text = keySkill3.ToString();
        }

        private void textBox_KeySkill4_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keySkill4 = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeySkill4.Text = keySkill4.ToString();
        }

        private void textBox_KeySkill5_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keySkill5 = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_KeySkill5.Text = keySkill5.ToString();
        }

        private void textBox_SkillDelay1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillDelay1.Text == "延遲" || textBox_SkillDelay1.Text == "")
                return;
            else
            {
                try
                {
                    delaySkill1 = int.Parse(textBox_SkillDelay1.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillDelay2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillDelay2.Text == "延遲" || textBox_SkillDelay2.Text == "")
                return;
            else
            {
                try
                {
                    delaySkill2 = int.Parse(textBox_SkillDelay2.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillDelay3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillDelay3.Text == "延遲" || textBox_SkillDelay3.Text == "")
                return;
            else
            {
                try
                {
                    delaySkill3 = int.Parse(textBox_SkillDelay3.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillDelay4_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillDelay4.Text == "延遲" || textBox_SkillDelay4.Text == "")
                return;
            else
            {
                try
                {
                    delaySkill4 = int.Parse(textBox_SkillDelay4.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillDelay5_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillDelay5.Text == "延遲" || textBox_SkillDelay5.Text == "")
                return;
            else
            {
                try
                {
                    delaySkill5 = int.Parse(textBox_SkillDelay5.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillTime1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillTime1.Text == "秒/次" || textBox_SkillTime1.Text == "")
                return;
            else
            {
                try
                {
                    timeSkill1 = int.Parse(textBox_SkillTime1.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillTime2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillTime2.Text == "秒/次" || textBox_SkillTime2.Text == "")
                return;
            else
            {
                try
                {
                    timeSkill2 = int.Parse(textBox_SkillTime2.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillTime3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillTime3.Text == "秒/次" || textBox_SkillTime3.Text == "")
                return;
            else
            {
                try
                {
                    timeSkill3 = int.Parse(textBox_SkillTime3.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillTime4_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillTime4.Text == "秒/次" || textBox_SkillTime4.Text == "")
                return;
            else
            {
                try
                {
                    timeSkill4 = int.Parse(textBox_SkillTime4.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_SkillTime5_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_SkillTime5.Text == "秒/次" || textBox_SkillTime5.Text == "")
                return;
            else
            {
                try
                {
                    timeSkill5 = int.Parse(textBox_SkillTime5.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void checkBox_PressKeySkill1_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill1 == null)
            {
                _threadOfSkill1 = new Thread(AutoKey.Skill1);
                _threadOfSkill1.Start();
            }
        }

        private void checkBox_PressKeySkill1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill1 != null)
            {
                _threadOfSkill1.Abort();
                _threadOfSkill1 = null;
            }
        }

        private void checkBox_PressKeySkill2_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill2 == null)
            {
                _threadOfSkill2 = new Thread(AutoKey.Skill2);
                _threadOfSkill2.Start();
            }
        }

        private void checkBox_PressKeySkill2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill2 != null)
            {
                _threadOfSkill2.Abort();
                _threadOfSkill2 = null;
            }
        }

        private void checkBox_PressKeySkill3_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill3 == null)
            {
                _threadOfSkill3 = new Thread(AutoKey.Skill3);
                _threadOfSkill3.Start();
            }
        }

        private void checkBox_PressKeySkill3_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill3 != null)
            {
                _threadOfSkill3.Abort();
                _threadOfSkill3 = null;
            }
        }

        private void checkBox_PressKeySkill4_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill4 == null)
            {
                _threadOfSkill4 = new Thread(AutoKey.Skill4);
                _threadOfSkill4.Start();
            }
        }

        private void checkBox_PressKeySkill4_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill4 != null)
            {
                _threadOfSkill4.Abort();
                _threadOfSkill4 = null;
            }
        }

        private void checkBox_PressKeySkill5_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill5 == null)
            {
                _threadOfSkill5 = new Thread(AutoKey.Skill5);
                _threadOfSkill5.Start();
            }
        }

        private void checkBox_PressKeySkill5_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfSkill5 != null)
            {
                _threadOfSkill5.Abort();
                _threadOfSkill5 = null;
            }
        }

        private void button_Selling_Click(object sender, RoutedEventArgs e)
        {
            _threadOfSelling = new Thread(Store.selling);
            _threadOfSelling.Start();
        }

        private void textBox_SkillDelay2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay2.Text == "")
                textBox_SkillDelay2.Text = "延遲";
        }

        private void textBox_SkillDelay2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay2.Text == "延遲")
                textBox_SkillDelay2.Text = "";
        }

        private void textBox_SkillDelay1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay1.Text == "")
                textBox_SkillDelay1.Text = "延遲";
        }

        private void textBox_SkillDelay1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay1.Text == "延遲")
                textBox_SkillDelay1.Text = "";
        }

        private void textBox_SkillDelay3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay3.Text == "")
                textBox_SkillDelay3.Text = "延遲";
        }

        private void textBox_SkillDelay3_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay3.Text == "延遲")
                textBox_SkillDelay3.Text = "";
        }

        private void textBox_SkillDelay4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay4.Text == "")
                textBox_SkillDelay4.Text = "延遲";
        }

        private void textBox_SkillDelay4_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay4.Text == "延遲")
                textBox_SkillDelay4.Text = "";
        }

        private void textBox_SkillDelay5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay5.Text == "")
                textBox_SkillDelay5.Text = "延遲";
        }

        private void textBox_SkillDelay5_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillDelay5.Text == "延遲")
                textBox_SkillDelay5.Text = "";
        }

        private void textBox_SkillTime1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime1.Text == "")
                textBox_SkillTime1.Text = "秒/次";
        }

        private void textBox_SkillTime1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime1.Text == "秒/次")
                textBox_SkillTime1.Text = "";
        }

        private void textBox_SkillTime2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime2.Text == "")
                textBox_SkillTime2.Text = "秒/次";
        }

        private void button_Referees_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = Hack.ShowMessageBoxYesNo("推薦人將會獲得30分鐘額度，確定你的推薦人UID是 '" + textBox_Referees.Text + "' 嗎?(不可修改)");
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                int referees = 0;
                int.TryParse(textBox_Referees.Text, out referees);
                using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
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
                    try
                    {
                        conn.Open();
                    }
                    catch
                    {
                        Hack.ShowMessageBox("無法連接伺服器");
                        this.Close();
                    }
                    try
                    {
                        cmd.CommandText = @"
                        SELECT Id
                        FROM dbo.RobotsUser
                        WHERE MAC = @MAC;";
                        cmd.Parameters.AddWithValue("@MAC", macList[0].ToString());
                        int UserID = (int)cmd.ExecuteScalar();
                        if (referees == UserID)
                        {
                            Hack.ShowMessageBox("推薦人不可以是自己");
                            return;
                        }
                        cmd.CommandText = @"
                        SELECT Point
                        FROM dbo.RobotsUser
                        WHERE Id = @Referees;
                        UPDATE dbo.RobotsUser
                        SET Referees = @Referees
                        WHERE MAC = @MAC;";
                        cmd.Parameters.AddWithValue("@Referees", referees);
                        int Referees_Point = (int)cmd.ExecuteScalar();
                        cmd.CommandText = @"
                        UPDATE dbo.RobotsUser
                        SET Point = @Referees_Point, RefereesCount = RefereesCount + 1
                        WHERE Id = @Referees;";
                        cmd.Parameters.AddWithValue("@Referees_Point", Referees_Point + 180);
                        cmd.ExecuteScalar();

                        textBox_Referees.IsEnabled = false;
                        button_Referees.IsEnabled = false;
                    }
                    catch
                    {
                        Hack.ShowMessageBox("查無此ID");
                    }
                }
            }
        }

        private void textBox_SkillTime2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime2.Text == "秒/次")
                textBox_SkillTime2.Text = "";
        }

        private void label_Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.facebook.com/MapleRobots/");
        }

        private void button_Trial_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result;
            result = Hack.ShowMessageBoxYesNo("確定現在要試用嗎?(每人只能試用一次)");
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
                {
                    int id = -1;
                    int referees = -1;
                    int point = 0;
                    bool isTrialAvailable = false;
                    var cmd = conn.CreateCommand();
                    string localMAC = Hack.getMacAddress();
                    string localIP = Hack.getIPAddress();
                    try
                    {
                        conn.Open();
                    }
                    catch
                    {
                        Hack.ShowMessageBox("無法連接伺服器");
                        Close();
                    }
                    try
                    {
                        cmd.CommandText = @"
                        SELECT Id, Referees, Point, TrialAvailable
                        FROM dbo.RobotsUser
                        WHERE MAC = @MAC;";
                        cmd.Parameters.AddWithValue("@MAC", localMAC);
                        //using (SqlDataReader sdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                id = (int)sdr.GetSqlInt32(0);
                                referees = (int)sdr.GetSqlInt32(1);
                                point = (int)sdr.GetSqlInt32(2);
                                isTrialAvailable = sdr.GetBoolean(3);
                                //this.TextBox1.Text = "號碼: " + dr.GetSqlInt32(0).Value + " 名稱: " + dr.GetSqlString(1).Value;
                            }
                        }
                        if (!isTrialAvailable)
                        {
                            Hack.ShowMessageBox("你已經試用過了");
                            return;
                        }
                        cmd.CommandText = @"
                        UPDATE dbo.RobotsUser
                        SET TrialAvailable = @notAble, Point = @Point
                        WHERE Id = @Id;
                        SELECT Point
                        FROM dbo.RobotsUser
                        WHERE Id = @Id;";
                        cmd.Parameters.AddWithValue("@notAble", 0);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Point", point + 180);
                        point = (int)cmd.ExecuteScalar();
                        labelPoint.Content = "剩餘: " + (point / 6 / 60) + "時 " + (point / 6 % 60) + "分";
                        if (point > 0)
                        {
                            checkBox_Botting.IsEnabled = true;
                            textBox_HotKeyBotting.IsEnabled = true;
                            button_keySetting.IsEnabled = true;
                            comboBox_BottingCase.IsEnabled = true;
                            checkBox_PlayerAlarm.IsEnabled = true;
                            textBox_PlayerAlarm.IsEnabled = true;
                            button_Selling.IsEnabled = true;
                            checkBox_PickUp.IsEnabled = true;
                            button_Trial.IsEnabled = false;
                        }
                    }
                    catch
                    {
                        Hack.ShowMessageBox("無法取得資料");
                    }
                }
            }
        }

        private void textBox_Referees_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((int)e.Key < (int)Key.D0) || (((int)e.Key > (int)Key.D9) && ((int)e.Key < (int)Key.NumPad0)) || ((int)e.Key > (int)Key.NumPad9))
            {
                e.Handled = true;
                Hack.ShowMessageBox("請輸入數字");
            }
        }

        private void textBox_SkillTime3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime3.Text == "")
                textBox_SkillTime3.Text = "秒/次";
        }

        private void textBox_SkillTime3_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime3.Text == "秒/次")
                textBox_SkillTime3.Text = "";
        }

        private void textBox_SkillTime4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime4.Text == "")
                textBox_SkillTime4.Text = "秒/次";
        }

        private void textBox_SkillTime4_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime4.Text == "秒/次")
                textBox_SkillTime4.Text = "";
        }

        private void textBox_SkillTime5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime5.Text == "")
                textBox_SkillTime5.Text = "秒/次";
        }

        private void textBox_SkillTime5_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_SkillTime5.Text == "秒/次")
                textBox_SkillTime5.Text = "";
        }

        private void checkBox_Bossing_Checked(object sender, RoutedEventArgs e)
        {
            if (radioButton_BossingFaceBoth.IsChecked == true)
                Bossing.bossingFaceTo = 0;
            else if (radioButton_BossingFaceLeft.IsChecked == true)
                Bossing.bossingFaceTo = 1;
            else if (radioButton_BossingFaceRight.IsChecked == true)
                Bossing.bossingFaceTo = 2;
            if (_threadOfBossing == null)
            {
                Hack.SetForegroundWindow(WindowHwnd);
                _threadOfBossing = new Thread(Bossing.bossing);
                _threadOfBossing.Start();
            }
        }

        private void checkBox_Bossing_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfBossing != null)
            {
                _threadOfBossing.Abort();
                _threadOfBossing = null;
                Hack.KeyUp(WindowHwnd, Keys.Left);
                Hack.KeyUp(WindowHwnd, Keys.Right);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isOldVersion)
            {
                if (autoUpdated)
                    Process.Start("MapleRobots.exe");
                Close();
            }
        }

        private void comboBox_BottingCase_Loaded(object sender, RoutedEventArgs e)
        {
            List <string> data = new List <string> ();

            data.Add("尚未選擇");
            data.Add("籃水靈");
            data.Add("黑肥肥");
            data.Add("發條熊");
            data.Add("小幽靈(一號線)");
            data.Add("進化妖魔(時間之路)");
            data.Add("妖魔隊長(時間之路)");
            data.Add("大幽靈(一號線)");
            data.Add("GS1");
            data.Add("GS5");
            data.Add("GS2");
            data.Add("WS");
            data.Add("魚窩");
            data.Add("ULU1");
            data.Add("ULU2");
            data.Add("烏賊");
            data.Add("石頭人");
            data.Add("骨龍");
            comboBox_BottingCase.ItemsSource = data;
            // Make the first item selected.
            comboBox_BottingCase.SelectedIndex = 0;
        }

        private void comboBox_BottingCase_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBox_BottingCase.SelectedItem.ToString() == "ULU2" ||
              comboBox_BottingCase.SelectedItem.ToString() == "石頭人" ||
              comboBox_BottingCase.SelectedItem.ToString() == "骨龍" ||
              comboBox_BottingCase.SelectedItem.ToString() == "烏賊")
            {
                comboBox_BottingHits.Visibility = Visibility.Visible;
            }
            else
            {
                comboBox_BottingHits.Visibility = Visibility.Hidden;
                comboBox_BottingHits.SelectedIndex = 0;
            }
                
        }

        private void comboBox_BottingHits_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("1hit");
            data.Add("2hit");
            comboBox_BottingHits.ItemsSource = data;
            // Make the first item selected.
            comboBox_BottingHits.SelectedIndex = 0;
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
                    string ign = Hack.ReadString(theprocess, CharacterNameBaseAdr, CharacterNameOffset, 15);
                    ign = ign.Replace("\0", "");
                    if (ign == "")
                        ign = "-未登入-";
                    else
                        data.Add(ign);
                    
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
                        string ign = Hack.ReadString(theprocess, CharacterNameBaseAdr, CharacterNameOffset, 15);
                        ign = ign.Replace("\0", "");
                        if (ign == "")
                            ign = "-未登入-";
                        else
                            data.Add(ign);
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
            StringBuilder title = new StringBuilder(256);
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Contains("MapleRoyals") || theprocess.ProcessName.Contains(".tmp"))
                {
                    string ign = Hack.ReadString(theprocess, CharacterNameBaseAdr, CharacterNameOffset, 15);
                    ign = ign.Replace("\0", "");
                    if (ign == comboBox_Process.SelectedItem.ToString())
                    {
                        BindWindow(theprocess);
                        process = theprocess;
                        comboBox_Process.IsEnabled = false;
                    }
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
            if (textBox_HpValue.Text == "")
                return;
            else
            {
                try
                {
                    HpBelow = int.Parse(textBox_HpValue.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_MpValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_MpValue.Text == "")
                return;
            else
            {
                try
                {
                    MpBelow = int.Parse(textBox_MpValue.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void textBox_PlayerAlarm_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBox_PlayerAlarm.Text == "")
                return;
            else
            {
                try
                {
                    PlayerCountAlarm = int.Parse(textBox_PlayerAlarm.Text);
                }
                catch
                {
                    Hack.ShowMessageBox("請輸入數字");
                }
            }
        }

        private void checkBox_PressKey_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfKeyPresser == null)
            {
                _threadOfKeyPresser = new Thread(AutoKey.KeyPresser);
                _threadOfKeyPresser.Start();
            }
            AutoKey.mre_KeyPresser.Set();
        }

        private void checkBox_PressKey_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfKeyPresser != null)
            {
                _threadOfKeyPresser.Abort();
                _threadOfKeyPresser = null;
            }
            AutoKey.mre_KeyPresser.Reset();
        }

        private void checkBox_PickUp_Checked(object sender, RoutedEventArgs e)
        {
            if (_threadOfPickUp == null)
            {
                _threadOfPickUp = new Thread(AutoKey.PickUp);
                _threadOfPickUp.Start();
            }
        }

        private void checkBox_PickUp_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_threadOfPickUp != null)
            {
                _threadOfPickUp.Abort();
                _threadOfPickUp = null;
            }
        }

        private void checkBox_Botting_Checked(object sender, RoutedEventArgs e)
        {
            if (keyTeleport == Keys.None || keyPickUp == Keys.None || keyAttack == Keys.None
                || keyJump == Keys.None || windowhotkey != null || attackParam <= 0 || keySkill == Keys.None
                || keyCombo1 == Keys.None || keyCombo2 == Keys.None || delayComboKey1 < 0 || delayComboKey2 < 0)
            {
                checkBox_Botting.IsChecked = false;
                Hack.ShowMessageBox("請先完成掛機設定");
                return;
            }
            else
            {
                timer3.Interval = 2000;
                timer3.Tick += new EventHandler(timer3_Tick);
                timer3.Start();
                checkBox_Botting.Visibility = Visibility.Hidden;
                UnregisterHotKey(_windowHandle, HOTKEY_ID + 1);
                if (_threadOfBotting == null && getPointFromDB(InGameName, 0) > 0)
                {
                    int mapID = Hack.ReadInt(process, MapIDBaseAdr, MapIDOffset);
                    timer2.Start();
                    if (comboBox_BottingHits.SelectedItem.ToString() == "2hit")
                        BottingBase.hit = 2;

                    if (checkMap(comboBox_BottingCase.SelectedItem.ToString()))
                    {
                        if (comboBox_BottingCase.SelectedItem.ToString() == "魚窩")
                            _threadOfBotting = new Thread(BottingGoby.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "籃水靈")
                            _threadOfBotting = new Thread(BottingBubbling.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "黑肥肥")
                            _threadOfBotting = new Thread(BottingWildBoar.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "發條熊")
                            _threadOfBotting = new Thread(BottingTeddy.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "小幽靈(一號線)")
                            _threadOfBotting = new Thread(BottingJrWraith.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "進化妖魔(時間之路)")
                            _threadOfBotting = new Thread(BottingPlattonChronos.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "妖魔隊長(時間之路)")
                            _threadOfBotting = new Thread(BottingMasterChronos.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "大幽靈(一號線)")
                            _threadOfBotting = new Thread(BottingWraith.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "GS1")
                            _threadOfBotting = new Thread(BottingGhostShip1.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "GS5")
                            _threadOfBotting = new Thread(BottingGhostShip5.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "GS2")
                            _threadOfBotting = new Thread(BottingGhostShip2.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "WS")
                            _threadOfBotting = new Thread(BottingWolfSpider.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "ULU1")
                            _threadOfBotting = new Thread(BottingULU1.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "ULU2")
                            _threadOfBotting = new Thread(BottingULU2.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "烏賊")
                            _threadOfBotting = new Thread(BottingSquid.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "石頭人")
                            _threadOfBotting = new Thread(BottingPetri.botting);
                        else if (comboBox_BottingCase.SelectedItem.ToString() == "骨龍")
                            _threadOfBotting = new Thread(BottingSkele.botting);
                    }
                    else
                    {
                        timer2.Stop();
                        checkBox_Botting.IsChecked = false;
                        if (comboBox_BottingCase.SelectedItem.ToString() != "尚未選擇")
                            Hack.ShowMessageBox("請先前往該地圖");
                        return;
                    }
                    Hack.SetForegroundWindow(WindowHwnd);
                    _threadOfBotting.Start();
                    button_keySetting.IsEnabled = false;
                    comboBox_BottingCase.IsReadOnly = true;
                    checkBox_PlayerAlarm.IsEnabled = false;
                    textBox_PlayerAlarm.IsEnabled = false;
                    button_Selling.IsEnabled = false;
                    checkBox_PickUp.IsEnabled = false;
                    return;
                }
                else
                {
                    int point = getPointFromDB(InGameName, 0);
                    labelPoint.Content = "剩餘: " + (point / 6 / 60) + "時 " + (point / 6 % 60) + "分";
                    Hack.ShowMessageBox("點數不足");
                    checkBox_Botting.IsChecked = false;
                    checkBox_Botting.IsEnabled = false;
                    textBox_HotKeyBotting.IsEnabled = false;
                    button_keySetting.IsEnabled = false;
                    comboBox_BottingCase.IsEnabled = false;
                    checkBox_PlayerAlarm.IsEnabled = false;
                    textBox_PlayerAlarm.IsEnabled = false;
                    button_Selling.IsEnabled = false;
                    checkBox_PickUp.IsEnabled = false;
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
            if (BottingBase._threadOfTraining != null)
            {
                BottingBase._threadOfTraining.Abort();
                BottingBase._threadOfTraining = null;
            }
            timer2.Stop();
            button_keySetting.IsEnabled = true;
            comboBox_BottingCase.IsReadOnly = false;
            checkBox_PlayerAlarm.IsEnabled = true;
            textBox_PlayerAlarm.IsEnabled = true;
            button_Selling.IsEnabled = true;
            checkBox_PickUp.IsEnabled = true;
            Hack.KeyUp(WindowHwnd, Keys.Up);
            Hack.KeyUp(WindowHwnd, Keys.Left);
            Hack.KeyUp(WindowHwnd, Keys.Right);
            Hack.KeyUp(WindowHwnd, Keys.Down);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((bool)checkBox_UnlimitedAttack.IsChecked)
                Hack.WriteInt(process, AttackCountBaseAdr, AttackCountOffset, 0);
            if ((bool)checkBox_Potions.IsChecked)
            {
                if (Hack.ReadInt(process, HpAlarmBaseAdr, HpAlarmOffset) != 20)
                    Hack.WriteInt(process, HpAlarmBaseAdr, HpAlarmOffset, 20);
                if (Hack.ReadInt(process, MpAlarmBaseAdr, MpAlarmOffset) != 20)
                    Hack.WriteInt(process, MpAlarmBaseAdr, MpAlarmOffset, 20);
                if (HpPotKey != Keys.None && HpBelow > 0)
                    if (Hack.ReadInt(process, HpValueBaseAdr, HpValueOffset) < HpBelow)
                        Hack.KeyPress((IntPtr)WindowHwnd, HpPotKey);
                if (MpPotKey != Keys.None && MpBelow > 0)
                    if (Hack.ReadInt(process, MpValueBaseAdr, MpValueOffset) < MpBelow)
                        Hack.KeyPress((IntPtr)WindowHwnd, MpPotKey);
            }
            if ((bool)checkBox_Botting.IsChecked)
            {
                if (!checkMap(comboBox_BottingCase.SelectedItem.ToString()))
                {
                    checkBox_Botting.IsChecked = false;
                    Hack.ShowMessageBox("請先前往該地圖");
                }
                if ((bool)checkBox_PlayerAlarm.IsChecked && 
                  Hack.ReadInt(process, PlayerCountBaseAdr, PlayerCountOffset) > PlayerCountAlarm)
                {
                    if (_threadOfAlarmForPlayer == null)
                    {
                        _threadOfAlarmForPlayer = new Thread(alarmForPlayer);
                        _threadOfAlarmForPlayer.Start();
                    }
                    mre_AlarmForPlayer.Set();
                }
                else
                {
                    mre_AlarmForPlayer.Reset();
                }
            }
            else
            {
                mre_AlarmForPlayer.Reset();
            }
            if (Hack.GetForegroundWindowHwnd() != WindowHwnd)
            {
                checkBox_Bossing.IsChecked = false;
                checkBox_Botting.IsChecked = false;
            }
            if (Hack.ReadString(process, CharacterNameBaseAdr, CharacterNameOffset, 15) != InGameName)
            {
                this.Close();
            }
            if (checkBox_Bossing.IsChecked == true && _threadOfBossing.IsAlive == false)
                checkBox_Bossing.IsChecked = false;
            //if (checkBox_Botting.IsEnabled == false)
                //checkBox_Botting.IsEnabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //Hack.ShowMessageBox("start");
            int point = getPointFromDB(InGameName, -1);
            labelPoint.Content = "剩餘: " + (point / 6 / 60) + "時 " + (point / 6 % 60) + "分";
            if (point <= 0)
            {
                checkBox_Botting.IsChecked = false;
                Hack.ShowMessageBox("點數不足");
                checkBox_Botting.IsEnabled = false;
                textBox_HotKeyBotting.IsEnabled = false;
                button_keySetting.IsEnabled = false;
                comboBox_BottingCase.IsEnabled = false;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (checkBox_Botting.Visibility == Visibility.Hidden)
            {
                checkBox_Botting.Visibility = Visibility.Visible;
                RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, (uint)HotKeyBotting);
            }
            timer3.Stop();
        }

        private bool getInitialValueFromDB(string IGN, out int id, out int referees, out int point, out bool isTrialAvailable)
        {
            using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                id = -1;
                referees = -1;
                point = 0;
                isTrialAvailable = false;
                var cmd = conn.CreateCommand();
                string localMAC = Hack.getMacAddress();
                string localIP = Hack.getIPAddress();
                try
                {
                    conn.Open();
                }
                catch
                {
                    Hack.ShowMessageBox("無法連接伺服器");
                    Close();
                }
                try
                {
                    cmd.CommandText = @"
                    SELECT Id, Referees, Point, TrialAvailable
                    FROM dbo.RobotsUser
                    WHERE MAC = @MAC;
                    INSERT INTO dbo.RobotsLogin (InGameName, Time, MAC, IP, Version)
                    VALUES (@InGameName, CURRENT_TIMESTAMP, @MAC, @IP, @Version);";
                    cmd.Parameters.AddWithValue("@InGameName", InGameName);
                    cmd.Parameters.AddWithValue("@MAC", localMAC);
                    cmd.Parameters.AddWithValue("@IP", localIP);
                    cmd.Parameters.AddWithValue("@Version", programVersion.ToString());
                    cmd.Parameters.AddWithValue("@Point", 0);
                    
                    //using (SqlDataReader sdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            id = (int)sdr.GetSqlInt32(0);
                            referees = (int)sdr.GetSqlInt32(1);
                            point = (int)sdr.GetSqlInt32(2);
                            isTrialAvailable = sdr.GetBoolean(3);
                            //this.TextBox1.Text = "號碼: " + dr.GetSqlInt32(0).Value + " 名稱: " + dr.GetSqlString(1).Value;
                            return true;
                        }
                    }
                }
                catch
                {
                    Hack.ShowMessageBox("something wrong.");
                }
                if (id == -1)
                {
                    try
                    {
                        cmd.CommandText = @"
                        SELECT Referees
                        FROM dbo.RobotsUser
                        WHERE InGameName = @InGameName;";
                        referees = (int)cmd.ExecuteScalar();
                        cmd.CommandText = @"
                        INSERT INTO dbo.RobotsUser (InGameName, Point, LastestTime, MAC, Referees, TrialAvailable, RefereesCount)
                        OUTPUT INSERTED.Id, INSERTED.Referees, INSERTED.Point, INSERTED.TrialAvailable
                        VALUES (@InGameName, @Point, CURRENT_TIMESTAMP, @MAC, @Referees, @TrialAvailable, @RefereesCount);";
                        cmd.Parameters.AddWithValue("@Referees", referees);
                        cmd.Parameters.AddWithValue("@TrialAvailable", TrialAvailable);
                        cmd.Parameters.AddWithValue("@RefereesCount", 0);

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                id = (int)sdr.GetSqlInt32(0);
                                referees = (int)sdr.GetSqlInt32(1);
                                point = (int)sdr.GetSqlInt32(2);
                                isTrialAvailable = sdr.GetBoolean(3);
                                //this.TextBox1.Text = "號碼: " + dr.GetSqlInt32(0).Value + " 名稱: " + dr.GetSqlString(1).Value;
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        TrialAvailable = 1;
                        cmd.CommandText = @"
                        INSERT INTO dbo.RobotsUser (InGameName, Point, LastestTime, MAC, Referees, TrialAvailable, )
                        OUTPUT INSERTED.Id, INSERTED.Referees, INSERTED.Point, INSERTED.TrialAvailable
                        VALUES (@InGameName, @Point, CURRENT_TIMESTAMP, @MAC, @Referees, @TrialAvailable);";

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                id = (int)sdr.GetSqlInt32(0);
                                referees = (int)sdr.GetSqlInt32(1);
                                point = (int)sdr.GetSqlInt32(2);
                                isTrialAvailable = sdr.GetBoolean(3);
                                //this.TextBox1.Text = "號碼: " + dr.GetSqlInt32(0).Value + " 名稱: " + dr.GetSqlString(1).Value;
                                return true;
                            }
                        }
                    }
                } 
            }
            return false;
        }

        private int getPointFromDB (string IGN, int deltaPoint)
        {
            using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                int userPoint, updatedUserPoint;
                var cmd = conn.CreateCommand();
                string localMAC = Hack.getMacAddress();
                string localIP = Hack.getIPAddress();
                try
                {
                    conn.Open();
                }
                catch
                {
                    Hack.ShowMessageBox("無法連接伺服器");
                    this.Close();
                }
                try
                {
                    cmd.CommandText = @"
                    SELECT Point
                    FROM dbo.RobotsUser
                    WHERE MAC = @MAC;";
                    cmd.Parameters.AddWithValue("@MAC", localMAC);
                    cmd.Parameters.AddWithValue("@InGameName", IGN);
                    cmd.Parameters.AddWithValue("@DeltaPoint", deltaPoint);
                    cmd.Parameters.AddWithValue("@IP", localIP);
                    cmd.Parameters.AddWithValue("@Version", programVersion.ToString());
                    cmd.Parameters.AddWithValue("@MAP", comboBox_BottingCase.SelectedItem.ToString());
                    userPoint = (int)cmd.ExecuteScalar();
                    updatedUserPoint = userPoint + deltaPoint;
                }
                catch
                {
                    /*try
                    {
                        cmd.CommandText = @"
                        SELECT Point
                        FROM dbo.RobotsUser
                        WHERE InGameName = @InGameName;";
                        userPoint = (int)cmd.ExecuteScalar();
                        updatedUserPoint = userPoint + deltaPoint;
                    }
                    catch
                    {*/
                        updatedUserPoint = 0;
                    /*}*/
                }
                try
                {
                    cmd.CommandText = @"
                    SELECT Point
                    FROM dbo.RobotsUserLog
                    WHERE InGameName = @InGameName
                      AND MAC = @MAC
                      AND IP = @IP
                      AND Version = @Version
                      AND MAP = @MAP
                      AND CONVERT (date, Time) = CONVERT (date, CURRENT_TIMESTAMP);

                    UPDATE dbo.RobotsUserLog
                    SET DeltaPoint = DeltaPoint + @DeltaPoint, Point = @updatedPoint
                    WHERE InGameName = @InGameName
                      AND MAC = @MAC
                      AND IP = @IP
                      AND Version = @Version
                      AND MAP = @MAP
                      AND CONVERT (date, Time) = CONVERT (date, CURRENT_TIMESTAMP);

                    UPDATE dbo.RobotsUser
                    SET Point = @updatedPoint, LastestTime = CURRENT_TIMESTAMP
                    WHERE InGameName = @InGameName OR MAC = @MAC;";
                    cmd.Parameters.AddWithValue("@updatedPoint", updatedUserPoint);
                    int temp = (int)cmd.ExecuteScalar();
                    Debug.WriteLine(temp.ToString());
                }
                catch
                {
                    Debug.WriteLine("catch");
                    cmd.CommandText = @"
                    INSERT INTO dbo.RobotsUserLog ( InGameName, DeltaPoint, Point, Time ,MAC, IP, Version, MAP)
                    OUTPUT INSERTED.Point
                    VALUES (@InGameName, @DeltaPoint, @updatedPoint, CURRENT_TIMESTAMP, @MAC, @IP, @Version, @MAP);
                    UPDATE dbo.RobotsUser
                    SET Point = @updatedPoint, LastestTime = CURRENT_TIMESTAMP
                    WHERE InGameName = @InGameName OR MAC = @MAC;";
                    int temp = (int)cmd.ExecuteScalar();
                }
                return updatedUserPoint;
            }
        }
        
        private bool checkBanned()
        {
            using (var conn = new SqlConnection("Server=tcp:mssql02.qsh.eu,1481;Database=db1012457-maplerobots;User ID=db1012457-maplerobots;Password=753951;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                var cmd = conn.CreateCommand();
                string localMAC = Hack.getMacAddress();
                string localIP = Hack.getIPAddress();
                conn.Open();
                //Hack.ShowMessageBox("ign = \"" + InGameName + "\"");
                //Hack.ShowMessageBox("ip = \"" + localIP + "\"");
                //Hack.ShowMessageBox("mac = \"" + macList[0].ToString()+"\"");
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
                        cmd.Parameters.AddWithValue("@MAC", localMAC);
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
                        cmd.Parameters.AddWithValue("@MAC", localMAC);
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
            WindowHwnd = process.MainWindowHandle;
            BottingBase.WindowHwnd = WindowHwnd;
            WindowReadData.WindowHwnd = WindowHwnd;
            WindowTitle = process.MainWindowTitle;
            Hack.SetForegroundWindow(WindowHwnd);
            if (WindowHwnd == IntPtr.Zero || WindowTitle == null)
            {
                Hack.ShowMessageBox("視窗綁定失敗");
                Close();
            }
            else if (!WindowTitle.Contains("MapleRoyals"))
            {
                Hack.ShowMessageBox("此程式僅適用於MapleRoyals");
                Close();
            }

            InGameName = Hack.ReadString(process, CharacterNameBaseAdr, CharacterNameOffset, 15);
            if (InGameName == null || InGameName == "")
            {
                Hack.ShowMessageBox("請先進入遊戲");
                Close();
            }

            if (checkBanned())//檢查是否被BAN
            {
                Hack.ShowMessageBox("伺服器拒絕存取");
                Close();
                return;
            }

            if (File.Exists(".\\" + filename))
            {
                try
                {
                    Keys key;
                    string temp;

                    textBox_KeyAutoAttack.Text = Hack.iniReader(".\\" + filename, InGameName, 
                      "KeyAutoAttack", false, "攻擊鍵", out keyWantToPress);

                    textBox_HotKeyAutoAttack.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HotKeyAutoAttack", false, "開關熱鍵", out HotKeyAutoAttack);
                    RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, (uint)HotKeyAutoAttack);//註冊熱鍵

                    textBox_KeySkill1.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "KeySkill1", false, "技能熱鍵", out keySkill1);

                    textBox_SkillDelay1.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill1Delay", true, "延遲", out key);
                    int.TryParse(textBox_SkillDelay1.Text, out delaySkill1);

                    textBox_SkillTime1.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill1Time", true, "秒/次", out key);
                    int.TryParse(textBox_SkillTime1.Text, out timeSkill1);

                    textBox_KeySkill2.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "KeySkill2", false, "技能熱鍵", out keySkill2);

                    textBox_SkillDelay2.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill2Delay", true, "延遲", out key);
                    int.TryParse(textBox_SkillDelay2.Text, out delaySkill2);

                    textBox_SkillTime2.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill2Time", true, "秒/次", out key);
                    int.TryParse(textBox_SkillTime2.Text, out timeSkill2);

                    textBox_KeySkill3.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "KeySkill3", false, "技能熱鍵", out keySkill3);

                    textBox_SkillDelay3.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill3Delay", true, "延遲", out key);
                    int.TryParse(textBox_SkillDelay3.Text, out delaySkill3);

                    textBox_SkillTime3.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill3Time", true, "秒/次", out key);
                    int.TryParse(textBox_SkillTime3.Text, out timeSkill3);

                    textBox_KeySkill4.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "KeySkill4", false, "技能熱鍵", out keySkill4);

                    textBox_SkillDelay4.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill4Delay", true, "延遲", out key);
                    int.TryParse(textBox_SkillDelay4.Text, out delaySkill4);

                    textBox_SkillTime4.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill4Time", true, "秒/次", out key);
                    int.TryParse(textBox_SkillTime4.Text, out timeSkill4);

                    textBox_KeySkill5.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "KeySkill5", false, "技能熱鍵", out keySkill5);

                    textBox_SkillDelay5.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill5Delay", true, "延遲", out key);
                    int.TryParse(textBox_SkillDelay5.Text, out delaySkill5);

                    textBox_SkillTime5.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "Skill5Time", true, "秒/次", out key);
                    int.TryParse(textBox_SkillTime5.Text, out timeSkill5);

                    textBox_HotKeyBotting.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HotKeyBotting", false, "開關熱鍵", out HotKeyBotting);
                    RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, (uint)HotKeyBotting);//註冊熱鍵

                    textBox_HpValue.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HpValue", true, "0", out key);
                    int.TryParse(textBox_HpValue.Text, out HpBelow);

                    textBox_HotKeyHp.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HotKeyHp", false, "補血熱鍵", out HpPotKey);

                    textBox_MpValue.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "MpValue", true, "0", out key);
                    int.TryParse(textBox_MpValue.Text, out MpBelow);

                    textBox_HotKeyMp.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HotKeyMp", false, "補魔熱鍵", out MpPotKey);

                    textBox_PlayerAlarm.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "PlayerCountAlarm", true, "0", out key);
                    int.TryParse(textBox_PlayerAlarm.Text, out PlayerCountAlarm);

                    textBox_HotKeyBossing.Text = Hack.iniReader(".\\" + filename, InGameName,
                      "HotKeyBossing", false, "開關熱鍵", out HotKeyBossing);
                    RegisterHotKey(_windowHandle, HOTKEY_ID + 2, MOD_NONE, (uint)HotKeyBossing);//註冊熱鍵

                    Hack.iniReader(".\\" + filename, InGameName, "KeyTeleport", false, "", out keyTeleport);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyPickUp", false, "", out keyPickUp);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyAttack", false, "", out keyAttack);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyJump", false, "", out keyJump);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyDoor", false, "", out keyDoor);
                    Hack.iniReader(".\\" + filename, InGameName, "KeySkill", false, "", out keySkill);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyCombo1", false, "", out keyCombo1);
                    Hack.iniReader(".\\" + filename, InGameName, "KeyCombo2", false, "", out keyCombo2);

                    temp = Hack.iniReader(".\\" + filename, InGameName,
                      "KeyCombo1Delay", true, "3000", out key);
                    int.TryParse(temp, out delayComboKey1);

                    temp = Hack.iniReader(".\\" + filename, InGameName,
                      "KeyCombo2Delay", true, "3000", out key);
                    int.TryParse(temp, out delayComboKey2);

                    temp = Hack.iniReader(".\\" + filename, InGameName,
                      "AttackParam", true, "25", out key);
                    int.TryParse(temp, out attackParam);
                }
                catch
                {
                    Hack.ShowMessageBox("設定檔錯誤，請刪除data.ini後再嘗試一次");
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
            checkBox_PressKeySkill1.IsEnabled = true;
            textBox_KeySkill1.IsEnabled = true;
            textBox_SkillDelay1.IsEnabled = true;
            textBox_SkillTime1.IsEnabled = true;
            checkBox_PressKeySkill2.IsEnabled = true;
            textBox_KeySkill2.IsEnabled = true;
            textBox_SkillDelay2.IsEnabled = true;
            textBox_SkillTime2.IsEnabled = true;
            checkBox_PressKeySkill3.IsEnabled = true;
            textBox_KeySkill3.IsEnabled = true;
            textBox_SkillDelay3.IsEnabled = true;
            textBox_SkillTime3.IsEnabled = true;
            checkBox_PressKeySkill4.IsEnabled = true;
            textBox_KeySkill4.IsEnabled = true;
            textBox_SkillDelay4.IsEnabled = true;
            textBox_SkillTime4.IsEnabled = true;
            checkBox_PressKeySkill5.IsEnabled = true;
            textBox_KeySkill5.IsEnabled = true;
            textBox_SkillDelay5.IsEnabled = true;
            textBox_SkillTime5.IsEnabled = true;
            checkBox_Bossing.IsEnabled = true;
            radioButton_BossingFaceBoth.IsEnabled = true;
            radioButton_BossingFaceLeft.IsEnabled = true;
            radioButton_BossingFaceRight.IsEnabled = true;
            tabControl.IsEnabled = true;
            textBox_HotKeyBossing.IsEnabled = true;
            int id, referees, point;
            bool isTrialAvailable = false;
            if (!getInitialValueFromDB(InGameName, out id, out referees, out point, out isTrialAvailable))
            {
                Hack.ShowMessageBox("無法取得資料..");
                Close();
            }
            if (isTrialAvailable)
                button_Trial.IsEnabled = true;
            label_ID.Content = "你的UID: " + id;
            if (referees < 0)
            {
                textBox_Referees.IsReadOnly = false;
                textBox_Referees.IsEnabled = true;
                button_Referees.IsEnabled = true;
            }
            else
            {
                textBox_Referees.Text = referees.ToString();
                button_Referees.IsEnabled = false;
            }
            labelPoint.Content = "剩餘: " + (point / 6 / 60) + "時 " + (point / 6 % 60) + "分";
            if (point > 0)
            {
                checkBox_Botting.IsEnabled = true;
                textBox_HotKeyBotting.IsEnabled = true;
                button_keySetting.IsEnabled = true;
                comboBox_BottingCase.IsEnabled = true;
                checkBox_PlayerAlarm.IsEnabled = true;
                textBox_PlayerAlarm.IsEnabled = true;
                button_Selling.IsEnabled = true;
                checkBox_PickUp.IsEnabled = true;
            }
            isBind = true;
        }
        private void alarmForPlayer()
        {
            while (true)
            {
                SystemSounds.Beep.Play();
                Thread.Sleep(800);
                mre_AlarmForPlayer.WaitOne();
            }
        }

        private bool checkMap(string mapName)
        {
            int mapID = Hack.ReadInt(process, MapIDBaseAdr, MapIDOffset);
            if (comboBox_BottingCase.SelectedItem.ToString() == "魚窩")
            {
                if (mapID != 230040100)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "籃水靈")
            {
                if (mapID != 103000101)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "黑肥肥")
            {
                if (mapID != 101040001)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "發條熊")
            {
                if (mapID != 220010500)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "小幽靈(一號線)")
            {
                if (mapID != 103000103)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "進化妖魔(時間之路)")
            {
                if (mapID != 220040000)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "妖魔隊長(時間之路)")
            {
                if (mapID != 220040400)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "大幽靈(一號線)")
            {
                if (mapID != 103000105)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "GS1")
            {
                if (mapID != 541010000)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "GS5")
            {
                if (mapID != 541010040)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "GS2")
            {
                if (mapID != 541010010)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "WS")
            {
                if (mapID != 600020300)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "ULU1")
            {
                if (mapID != 541020100)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "ULU2")
            {
                if (mapID != 541020200)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "烏賊")
            {
                if (mapID != 230040300)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "石頭人")
            {
                if (mapID != 541020500)
                    return false;
                else
                    return true;
            }
            else if (comboBox_BottingCase.SelectedItem.ToString() == "骨龍")
            {
                if (mapID != 240040511)
                    return false;
                else
                    return true;
            }
            else
            {
                Hack.ShowMessageBox("請先選擇模組");
                return false;
            }
        }

        private void DownLoadFile()
        {
            //lbProgress.Content = "正在更新中..";

            string UName = "user", UPWord = "user";
            FtpWebRequest ftpReq;
            int filesize;
            Uri utt = new Uri("ftp://www.utt.game.tw/MapleRoyals/MapleRobots.exe");
            Debug.WriteLine(utt.Port);
            try
            {
                //宣告FTP連線
                ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://www.utt.game.tw/MapleRoyals/MapleRobots.exe"));
                //ftpReq.EnableSsl = true;
                //取得欲下載檔案的大小(位元)存至 fiesize
                ftpReq.Method = WebRequestMethods.Ftp.GetFileSize;
                //認證
                ftpReq.Credentials = new NetworkCredential(UName, UPWord);
                filesize = (int)ftpReq.GetResponse().ContentLength;
            }
            catch
            {
                Hack.ShowMessageBox("自動更新失敗,請至粉絲專頁下載最新版");
                Process.Start("https://www.facebook.com/MapleRobots/");
                autoUpdated = false;
                return;
            }
            try
            {
                //宣告FTP連線
                ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://www.utt.game.tw/MapleRoyals/MapleRobots.exe"));
                //ftpReq.EnableSsl = true;
                //下載
                ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
                //認證
                ftpReq.Credentials = new NetworkCredential(UName, UPWord);
            }
            catch
            {
                Hack.ShowMessageBox("自動更新失敗,請至粉絲專頁下載最新版");
                Process.Start("https://www.facebook.com/MapleRobots/");
                autoUpdated = false;
                return;
            }

            //binaary
            ftpReq.UseBinary = true;

            //支援續傳
            FileInfo fi = new FileInfo(".\\MapleRobots.exe");
            FileStream fs = null;
            //檢測是否已有相同檔名的存在於client端
            if (fi.Exists)
            {
                File.Delete("_MapleRobots.exe");
                File.Move("MapleRobots.exe", "_MapleRobots.exe");
                FileInfo fi2 = new FileInfo(".\\_MapleRobots.exe");
                fi2.Attributes = FileAttributes.Hidden;
            }
            //client端無檔案 則重新建立新檔
            fs = new FileStream(".\\MapleRobots.exe", FileMode.Create);
            //建立ftp連線
            FtpWebResponse ftpResp = (FtpWebResponse)ftpReq.GetResponse();

            bool bfinish = false;
            try
            {
                //取得下載用的stream物件
                //ftpResp.GetResponseStream()--->擷取包含從FTP server傳送之回應資料的資料流
                using (Stream stm = ftpResp.GetResponseStream())
                {
                    //以block方式多批寫入
                    byte[] buff = new byte[5];
                    //讀data
                    int len = 0;

                    while (fs.Length < filesize)
                    {
                        //取得長度
                        len = stm.Read(buff, 0, buff.Length);
                        fs.Write(buff, 0, len);

                        //傳完
                        if (fs.Length == filesize)
                        {
                            //File.Delete("_MapleRobots.exe");
                            //lbProgress.Content = "更新已完成";

                        }
                    }

                    fs.Flush();
                    //傳完，bfinish = true
                    //清除資料流的緩衝區
                    bfinish = (fs.Length == filesize);
                    fs.Close();
                    stm.Close();
                }
            }
            catch (WebException we)
            {
                //若未傳完才要觸發 exception
                if (!bfinish)
                    throw we;
            }
            ftpResp.Close();
            Hack.ShowMessageBox("已更新完畢，即將重啟程式");
        }
    }
}
