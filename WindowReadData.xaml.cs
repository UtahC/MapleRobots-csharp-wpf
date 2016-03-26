using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapleRobots
{
    /// <summary>
    /// WindowReadData.xaml 的互動邏輯
    /// </summary>
    public partial class WindowReadData : Window
    {
        internal static IntPtr WindowHwnd;
        public WindowReadData()
        {
            InitializeComponent();
            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 500;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MainWindow.isBind)
            {
                label_AttackCount.Content = "攻擊次數: " + Hack.ReadInt(MainWindow.process, MainWindow.AttackCountBaseAdr, MainWindow.AttackCountOffset);
                label_HpAlert.Content = "HP警告: " + Hack.ReadInt(MainWindow.process, MainWindow.HpAlarmBaseAdr, MainWindow.HpAlarmOffset);
                label_MpAlert.Content = "MP警告: " + Hack.ReadInt(MainWindow.process, MainWindow.MpAlarmBaseAdr, MainWindow.MpAlarmOffset);
                label_HpValue.Content = "HP值: " + (double)Hack.ReadInt(MainWindow.process, MainWindow.HpValueBaseAdr, MainWindow.HpValueOffset)/Hack.ReadInt(MainWindow.process, MainWindow.HpAlarmBaseAdr, MainWindow.HpAlarmOffset)*20;
                label_MpValue.Content = "MP值: " + (double)Hack.ReadInt(MainWindow.process, MainWindow.MpValueBaseAdr, MainWindow.MpValueOffset)/ Hack.ReadInt(MainWindow.process, MainWindow.MpAlarmBaseAdr, MainWindow.MpAlarmOffset)*20;
                label_Exp.Content = "經驗值: " + Math.Round(Hack.ReadDouble(MainWindow.process, MainWindow.ExpPercentBaseAdr, MainWindow.ExpPercentOffset), 2) + "%";
                label_PlayerCount.Content = "地圖玩家數量: " + Hack.ReadInt(MainWindow.process, MainWindow.PlayerCountBaseAdr, MainWindow.PlayerCountOffset);
                label_MobCount.Content = "地圖怪物數量: " + Hack.ReadInt(MainWindow.process, MainWindow.MobCountBaseAdr, MainWindow.MobCountOffset);
                label_CharacterStates.Content = "角色狀態: " + Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
                label_CharacterX.Content = "角色X座標: " + Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                label_CharacterY.Content = "角色Y座標: " + Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                label_MapID.Content = "地圖編號: " + Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
