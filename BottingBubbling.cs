using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingBubbling : BottingBase
    {
        internal static void botting()
        {
            int counter = 0;
            if (isOnRope() && getCharacterX() != 2859)
                RopeExiting(true);
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    GoToX(2800);
                    upstairTeleport(73);
                    RopeClimbing(2859, false, -38, 73, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToX(2800);
                    upstairTeleport(73);
                    RopeClimbing(2859, false, -38, 73, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                }
                GoToX(2801);
                //upstairTeleport(73);
                Attack(1);
                GoToX(2027);
                upstairTeleport(73);
                Attack(1);
                GoToX(1247);
                Attack(1);
                GoToX(431);
                Attack(1);
                GoToX(341);
                upstairTeleport(73);
                Attack(1);
                GoToX(1247);
                Attack(1);
                GoToX(2027);
                upstairTeleport(73);
                Attack(1);

                counter++;
            }
        }
    }
}
