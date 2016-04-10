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
            while (MainWindow.keyWantToPress != System.Windows.Forms.Keys.None)
            {
                Hack.KeyDown(MainWindow.WindowHwnd, MainWindow.keyWantToPress);
                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }
        internal static void Skill1()
        {
            while (MainWindow.keySkill1 != System.Windows.Forms.Keys.None && MainWindow.delaySkill1 > 0 && MainWindow.timeSkill1 > 0)
            { 
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Thread.Sleep(500);
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill1);
                Thread.Sleep(MainWindow.delaySkill1);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill1*1000 - MainWindow.delaySkill1);
            }
        }
        internal static void Skill2()
        {
            while (MainWindow.keySkill2 != System.Windows.Forms.Keys.None && MainWindow.delaySkill2 > 0 && MainWindow.timeSkill2 > 0)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Thread.Sleep(500);
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill2);
                Thread.Sleep(MainWindow.delaySkill2);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill2*1000 - MainWindow.delaySkill2);
            }
        }
        internal static void Skill3()
        {
            while (MainWindow.keySkill3 != System.Windows.Forms.Keys.None && MainWindow.delaySkill3 > 0 && MainWindow.timeSkill3 > 0)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Thread.Sleep(500);
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill3);
                Thread.Sleep(MainWindow.delaySkill3);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill3 * 1000 - MainWindow.delaySkill3);
            }
        }
        internal static void Skill4()
        {
            while (MainWindow.keySkill4 != System.Windows.Forms.Keys.None && MainWindow.delaySkill4 > 0 && MainWindow.timeSkill4 > 0)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Thread.Sleep(500);
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill4);
                Thread.Sleep(MainWindow.delaySkill4);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill4 * 1000 - MainWindow.delaySkill4);
            }
        }
        internal static void Skill5()
        {
            while (MainWindow.keySkill5 != System.Windows.Forms.Keys.None && MainWindow.delaySkill5 > 0 && MainWindow.timeSkill5 > 0)
            {
                mre_KeyPresser.Reset();
                mre_PickUp.Reset();
                Thread.Sleep(500);
                Hack.KeyPress(MainWindow.WindowHwnd, MainWindow.keySkill5);
                Thread.Sleep(MainWindow.delaySkill5);
                mre_KeyPresser.Set();
                mre_PickUp.Set();
                Thread.Sleep(MainWindow.timeSkill5 * 1000 - MainWindow.delaySkill5);
            }
        }
    }
}
