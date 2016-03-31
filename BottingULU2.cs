using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingULU2 : BottingBase
    {
        static int nowFloor = 0;
        internal static void botting()
        {

            if (hit == 1)
                _threadOfTraining = new Thread(training1hit);
            else
                _threadOfTraining = new Thread(training2hit);
            _threadOfTraining.Start();
            while (true)
            {
                int CharacterY = getCharacterY();
                if (CharacterY <= -496)
                    nowFloor = 1;
                else if (CharacterY > -496 && CharacterY <= -196)
                    nowFloor = 2;
                else if (CharacterY > 0 && CharacterY <= 104)
                    nowFloor = 3;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        static void training1hit()
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 8 == 0)
                {
                    //go to position start
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(3);
                    RopeClimbing(-330, false, -46, 104, 60, 60);
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
                    /*
                    RopeClimbing(-330, false, -196, 104, 60, 60);
                    GoToX(117, 20, false, false, 0);
                    //Attack(1);
                    GoToX(738, 20, false, false, 0);
                    gotoFloor(3);
                    Attack(1);
                    GoToX(117);
                    Attack(2);
                    */
                }
                else if (counter % 4 == 0)
                {
                    //go to position start
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(3);
                    RopeClimbing(-330, false, -46, 104, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                    /*
                    RopeClimbing(-330, false, -196, 104, 60, 60);
                    GoToX(117, 20, false, false, 0);
                    //Attack(1);
                    GoToX(738, 20, false, false, 0);
                    gotoFloor(3);
                    Attack(1);
                    GoToX(117);
                    Attack(2);
                    */
                }
                GoToX(-494);
                Attack(1);
                GoToX(117);
                Attack(1);
                GoToX(738);
                Attack(1);
                GoToX(117);
                Attack(1);

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
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(3);
                    RopeClimbing(-330, false, -46, 104, 60, 60);
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
                    /*
                    RopeClimbing(-330, false, -196, 104, 60, 60);
                    GoToX(117, 20, false, false, 0);
                    //Attack(1);
                    GoToX(738, 20, false, false, 0);
                    gotoFloor(3);
                    Attack(1);
                    GoToX(117);
                    Attack(2);
                    */
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(3);
                    RopeClimbing(-330, false, -46, 104, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                    /*
                    RopeClimbing(-330, false, -196, 104, 60, 60);
                    GoToX(117, 20, false, false, 0);
                    //Attack(1);
                    GoToX(738, 20, false, false, 0);
                    gotoFloor(3);
                    Attack(1);
                    GoToX(117);
                    Attack(2);
                    */
                }
                GoToX(-494);
                Attack(2);
                GoToX(117);
                Attack(2);
                GoToX(738);
                Attack(2);
                GoToX(117);
                Attack(2);

                counter++;
            }
        }
        static void gotoFloor(int targetFloor)
        {
            while (nowFloor != targetFloor)
            {
                if (nowFloor == 0)
                    return;
                else if (targetFloor - nowFloor > 1)
                {
                    gotoFloor(targetFloor - 1);
                }
                else if (targetFloor - nowFloor < -1)
                {
                    gotoFloor(targetFloor + 1);
                }
                else if (targetFloor == 2 && nowFloor == 1)
                {
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
                else if (targetFloor == 3 && nowFloor == 2)
                {
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
                else if (targetFloor == 2 && nowFloor == 3)
                {
                    RopeClimbing(-330, false, -196, 104, 60, 60);
                }
            }
        }
    }
}
