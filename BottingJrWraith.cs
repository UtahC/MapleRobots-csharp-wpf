using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingJrWraith : BottingBase
    {
        internal static void botting()//小幽靈
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 4 == 0)
                {
                    //go to position start
                    upstairTeleport(198);
                    GoToX(315);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                }
                else if (counter % 2 == 0)
                {
                    //go to position start
                    upstairTeleport(198);
                    GoToX(315);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    AutoKey.mre_PickUp.Set();
                }
                GoToX(315);
                Attack(1);
                GoToX(315);
                upstairTeleport(-47);
                GoToX(602);
                Attack(1);
                GoToX(1280);
                Attack(1);
                GoToX(1948);
                Attack(1);
                GoToX(2804);
                Attack(1);
                GoToX(2804);
                upstairTeleport(73);
                GoToX(2218);
                Attack(1);
                GoToX(1280);
                Attack(1);

                counter++;
            }
        }
    }
}
