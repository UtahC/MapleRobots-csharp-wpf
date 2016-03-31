using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingWraith : BottingBase
    {
        internal static void bottingWraith()//大幽靈
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    RopeClimbing(-92, false, 36, 198, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(false);
                }

                else if (counter % 3 == 0)
                {
                    //go to position start
                    RopeClimbing(-92, false, 36, 198, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(false);
                }
                GoToX(438);
                upstairTeleport(73);
                Attack(1);
                upstairTeleport(-47);
                GoToX(1214);
                upstairTeleport(73);
                Attack(1);
                upstairTeleport(-17);
                GoToX(1732);
                Attack(1);
                JumpingOver(1800, false);
                GoToX(2428);
                upstairTeleport(73);
                Attack(1);
                GoToX(1648);
                Attack(1);
                GoToX(904);
                upstairTeleport(73);
                Attack(1);


                counter++;
            }
        }
    }
}
