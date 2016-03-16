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
        QfDm dm = new QfDm();
        internal static int WindowHwnd;
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
                label_AttackCount.Content = "攻擊次數: " + dm.DM.ReadInt(WindowHwnd, MainWindow.AttackCountAdr, 0);
                label_HpAlert.Content = "HP警告: " + dm.DM.ReadInt(WindowHwnd, MainWindow.HpAlarmAdr, 0);
                label_MpAlert.Content = "MP警告: " + dm.DM.ReadInt(WindowHwnd, MainWindow.MpAlarmAdr, 0);
                label_HpValue.Content = "HP值: " + dm.DM.ReadInt(WindowHwnd, MainWindow.HpValueAdr, 0);
                label_MpValue.Content = "MP值: " + dm.DM.ReadInt(WindowHwnd, MainWindow.MpValueAdr, 0);
                label_Exp.Content = "經驗值: " + Math.Round(dm.DM.ReadDouble(WindowHwnd, MainWindow.ExpPercentAdr), 2) + "%";
                label_PlayerCount.Content = "地圖玩家數量: " + dm.DM.ReadInt(WindowHwnd, MainWindow.PlayerCountAdr, 0);
                label_MobCount.Content = "地圖怪物數量: " + dm.DM.ReadInt(WindowHwnd, MainWindow.MobCountAdr, 0);
                label_CharacterStates.Content = "角色狀態: " + dm.DM.ReadInt(WindowHwnd, MainWindow.CharacterStatusAdr, 0);
                label_CharacterX.Content = "角色X座標: " + dm.DM.ReadInt(WindowHwnd, MainWindow.CharacterXAdr, 0);
                label_CharacterY.Content = "角色Y座標: " + dm.DM.ReadInt(WindowHwnd, MainWindow.CharacterYAdr, 0);
                label_MapID.Content = "地圖編號: " + dm.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            dm.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
