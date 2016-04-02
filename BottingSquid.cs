using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingSquid : BottingBase
    {
        internal static void botting()
        {

            if (hit == 1)
                _threadOfTraining = new Thread(training1hit);
            else
                _threadOfTraining = new Thread(training2hit);
            _threadOfTraining.Start();

        }
        static void training1hit()
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    GoToLocationInWater(294, 920, 8, true, true, 0);
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
                    GoToLocationInWater(50, 887);
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToLocationInWater(294, 920, 8, true, true, 0);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    //for (int i = 0; i < 25; i++)
                    Hack.KeyDown(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    Hack.KeyUp(WindowHwnd, MainWindow.keySkill);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                    GoToLocationInWater(50, 887);
                }
                GoToLocationInWater(-50, -110);
                Attack(1);
                GoToLocationInWater(-50, 185);
                Attack(1);
                GoToLocationInWater(-50, 561);
                Attack(1);
                GoToLocationInWater(-50, 776);
                Attack(1);
                GoToLocationInWater(-50, 996);
                Attack(1);
                GoToLocationInWater(-200, 1225);
                Attack(2);

                counter++;
            }
        }
        static void training2hit()
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    GoToLocationInWater(294, 920, 8, true, true, 0);
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
                    GoToLocationInWater(50, 887);
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToLocationInWater(294, 920, 8, true, true, 0);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    //for (int i = 0; i < 25; i++)
                    Hack.KeyDown(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    Hack.KeyUp(WindowHwnd, MainWindow.keySkill);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                    GoToLocationInWater(50, 887);
                }
                GoToLocationInWater(-141, -141);
                Attack(1);
                GoToLocationInWater(-50, -110);
                Attack(1);
                GoToLocationInWater(-50, 185);
                Attack(2);
                GoToLocationInWater(-50, 561);
                Attack(1);
                GoToLocationInWater(-50, 776);
                Attack(1);
                GoToLocationInWater(-50, 996);
                Attack(1);
                GoToLocationInWater(-200, 1225);
                Attack(3);

                counter++;
            }
        }
    }
}
