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

        private string filename = "data.ini";

        public WindowHotKey()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WritePrivateProfileString(MainWindow.InGameName, "KeyAttack", textBox_Attack.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "AttackParam", textBox_AttackParam.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyTeleport", textBox_Teleport.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyPickUp", textBox_PickUp.Text, ".\\" + filename);
            WritePrivateProfileString(MainWindow.InGameName, "KeyJump", textBox_Jump.Text, ".\\" + filename);
            //WritePrivateProfileString(MainWindow.InGameName, "KeyDoor", textBox_Door.Text, ".\\" + filename);
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
                    Keys key;

                    textBox_Attack.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyAttack", false, "", out MainWindow.keyAttack);
                    textBox_Teleport.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyTeleport", false, "", out MainWindow.keyTeleport);
                    textBox_PickUp.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyPickUp", false, "", out MainWindow.keyPickUp);
                    textBox_Jump.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyJump", false, "", out MainWindow.keyJump);
                    //textBox_Door.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyDoor", false, "", out MainWindow.keyDoor);
                    textBox_Skill.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeySkill", false, "", out MainWindow.keySkill);
                    textBox_Combo1.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyCombo1", false, "", out MainWindow.keyCombo1);
                    textBox_Combo2.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName, "KeyCombo2", false, "", out MainWindow.keyCombo2);

                    textBox_Combo1Delay.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName,
                      "KeyCombo1Delay", true, "3000", out key);
                    int.TryParse(textBox_Combo1Delay.Text, out MainWindow.delayComboKey1);

                    textBox_Combo2Delay.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName,
                      "KeyCombo2Delay", true, "3000", out key);
                    int.TryParse(textBox_Combo2Delay.Text, out MainWindow.delayComboKey2);

                    textBox_AttackParam.Text = Hack.iniReader(".\\" + filename, MainWindow.InGameName,
                      "AttackParam", true, "50", out key);
                    int.TryParse(textBox_AttackParam.Text, out MainWindow.attackParam);
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

        /*private void textBox_Door_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWindow.keyDoor = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            textBox_Door.Text = MainWindow.keyDoor.ToString();
        }*/

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
            if (textBox_Combo1Delay.Text == "")
                return;
            else
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
        }

        private void textBox_Combo2Delay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_Combo2Delay.Text == "")
                return;
            else
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

        private void textBox_AttackParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_Combo2Delay.Text == "")
                return;
            else
            {
                try
                {
                    MainWindow.attackParam = int.Parse(textBox_AttackParam.Text);
                }
                catch
                {
                    System.Windows.MessageBox.Show("請輸入數字");
                }
            }
        }
    }
}
