using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingWildBoar : BottingBase
    {
        internal static void bottingWildBoar()//黑肥肥
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 3 == 0)
                {
                    //go to position start
                    if (Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset) > 2055)
                        RopeClimbing(1380, true, 2055, 2205, -60, 60);
                    RopeClimbing(834, false, 1845, 2205, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    if (counter % 6 == 0)
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1000);
                    }
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(834, true, 1695, 2205, 60, 60);
                    GoToX(750);
                    Attack(1);
                }
                GoToX(-164);
                Attack(1);
                GoToX(653);
                Attack(1);
                GoToX(1585);
                Attack(1);
                GoToX(653);
                Attack(1);

                counter++;
            }
        }
    }
}
