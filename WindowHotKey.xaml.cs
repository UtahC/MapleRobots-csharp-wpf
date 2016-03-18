using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapleRobots
{
    /// <summary>
    /// WindowHotKey.xaml 的互動邏輯
    /// </summary>
    public partial class WindowHotKey : Window
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal,
        int size, string filePath);

        private string filename = "data.ini";

        public WindowHotKey()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WritePrivateProfileString(MainWindow.InGameName, "KeyTeleport", textBox_Teleport.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyPickUp", textBox_PickUp.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyAttack", textBox_Attack.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyJump", textBox_Jump.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyDoor", textBox_Door.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeySkill", textBox_Skill.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyCombo1", textBox_Combo1.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyCombo2", textBox_Combo2.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyCombo1Delay", textBox_Combo1Delay.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyCombo2Delay", textBox_Combo2Delay.Text, ".\\" + filename);
            MainWindow.windowhotkey = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(".\\" + filename))
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder(30);
                    GetPrivateProfileString(MainWindow.InGameName, "KeyTeleport", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Teleport.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Teleport.Text, out key);
                        MainWindow.keyTeleport = key;
                    }
                    else
                        textBox_Teleport.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyPickUp", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_PickUp.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_PickUp.Text, out key);
                        MainWindow.keyPickUp = key;
                    }
                    else
                        textBox_PickUp.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyAttack", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Attack.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Attack.Text, out key);
                        MainWindow.keyAttack = key;
                    }
                    else
                        textBox_Attack.Text = "ex.天怒";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyJump", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Jump.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Jump.Text, out key);
                        MainWindow.keyJump = key;
                    }
                    else
                        textBox_Jump.Text = "ex.天怒";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyDoor", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Door.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Door.Text, out key);
                        MainWindow.keyDoor = key;
                    }
                    else
                        textBox_Door.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeySkill", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Skill.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Skill.Text, out key);
                        MainWindow.keySkill = key;
                    }
                    else
                        textBox_Skill.Text = "ex.祈禱";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo1", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Combo1.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Combo1.Text, out key);
                        MainWindow.keyCombo1 = key;
                    }
                    else
                        textBox_Combo1.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo2", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[a-zA-Z0-9]*$") && stringBuilder.ToString() != "")
                    {
                        Keys key;
                        textBox_Combo2.Text = stringBuilder.ToString();
                        Enum.TryParse(textBox_Combo2.Text, out key);
                        MainWindow.keyCombo2 = key;
                    }
                    else
                        textBox_Combo2.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo1Delay", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
                    {
                        int delay;
                        textBox_Combo1Delay.Text = stringBuilder.ToString();
                        delay = int.Parse(textBox_Combo1Delay.Text);
                        MainWindow.delayComboKey1 = delay;
                    }
                    else
                        textBox_Combo1Delay.Text = "";
                    stringBuilder.Clear();
                    GetPrivateProfileString(MainWindow.InGameName, "KeyCombo2Delay", "", stringBuilder, 30, ".\\" + filename);
                    if (Regex.IsMatch(stringBuilder.ToString(), "^[0-9]*$") && stringBuilder.ToString() != "")
                    {
                        int delay;
                        textBox_Combo2Delay.Text = stringBuilder.ToString();
                        delay = int.Parse(textBox_Combo2Delay.Text);
                        MainWindow.delayComboKey2 = delay;
                    }
                    else
                        textBox_Combo2Delay.Text = "";
                }
                catch
                {
                    System.Windows.MessageBox.Show("設定檔錯誤，請刪除data.ini後再嘗試一次");
                }
            }
        }

        private void textBox_Teleport_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyTeleport = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Teleport.Text = MainWindow.keyTeleport.ToString();
        }

        private void textBox_PickUp_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.System)
                MainWindow.keyJump = Keys.Menu;
            else
                MainWindow.keyPickUp = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_PickUp.Text = MainWindow.keyPickUp.ToString();
        }

        private void textBox_Attack_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyAttack = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Attack.Text = MainWindow.keyAttack.ToString();
        }

        private void textBox_Jump_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.System)
                MainWindow.keyJump = Keys.Menu;
            else
                MainWindow.keyJump = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Jump.Text = MainWindow.keyJump.ToString();
        }

        private void textBox_Door_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyDoor = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Door.Text = MainWindow.keyDoor.ToString();
        }

        private void textBox_Skill_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keySkill = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Skill.Text = MainWindow.keySkill.ToString();
        }

        private void textBox_Combo1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyCombo1 = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Combo1.Text = MainWindow.keyCombo1.ToString();
        }

        private void textBox_Combo2_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyCombo2 = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Combo2.Text = MainWindow.keyCombo2.ToString();
        }

        private void textBox_Combo1Delay_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                MainWindow.delayComboKey1 = int.Parse(textBox_Combo1Delay.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("請輸入數字");
            }
        }

        private void textBox_Combo2Delay_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                MainWindow.delayComboKey2 = int.Parse(textBox_Combo1Delay.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("請輸入數字");
            }
        }

        
    }
}
