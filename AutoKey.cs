using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class AutoKey
    {
        internal static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);
        internal static ManualResetEvent mre_PickUp = new ManualResetEvent(true);

        internal static void PickUp()
        {
            while (true)
            {
                Hack.KeyDown(MainWindow.WindowHwnd, MainWindow.keyPickUp);
                Thread.Sleep(50);
                mre_PickUp.WaitOne();
            }
        }
        internal static void KeyPresser()
        {
            while (true)
            {
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keyWantToPress);
                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }
        internal static void Skill1()
        {
            while (true)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill1);
                Thread.Sleep(MainWindow.delaySkill1);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill1*1000 - MainWindow.delaySkill1);
            }
        }
        internal static void Skill2()
        {
            while (true)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill2);
                Thread.Sleep(MainWindow.delaySkill2);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill2*1000 - MainWindow.delaySkill2);
            }
        }
    }
}
