using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingMasterChronos : BottingBase
    {
        static int nowFloor = 0;
        internal static void bottingPlatoonChronos()//妖魔隊長
        {
            _threadOfTraining = new Thread(bottingMasterChronosTraining);
            _threadOfTraining.Start();
            while (true)
            {
                int CharacterY = getCharacterY();
                if (CharacterY >= 167 && CharacterY <= 312)
                    nowFloor = 1;
                else if (CharacterY >= 354 && CharacterY <= 552)
                    nowFloor = 2;
                else if (CharacterY >= 553 && CharacterY <= 792)
                    nowFloor = 3;
                else if (CharacterY >= 910 && CharacterY <= 1032)
                    nowFloor = 4;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        internal static void bottingMasterChronosTraining()
        {
            int counter = 0;
            Thread.Sleep(500);
            //training start
            while (true)
            {
                if (counter % 8 == 0)
                {
                    //go to position start
                    GoToFloor(4);
                    RopeClimbing(-604, false, 826, 1032, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(-604, true, 732, 1032, 60, 60);
                }
                else if (counter % 4 == 0)
                {
                    //go to position start
                    GoToFloor(4);
                    RopeClimbing(-604, false, 826, 1032, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(-604, true, 732, 1032, 60, 60);
                }
                GoToFloor(2);
                GoToX(-415);
                Attack(1);
                GoToX(-271);
                JumpingOver(-271, false);
                GoToX(114);
                Attack(1);
                GoToX(416);
                GoToFloor(4);
                GoToX(314);
                Attack(1);
                GoToX(-604);
                Attack(1);

                counter++;
            }
        }
        static void GoToFloor(int targetFloor)//腳踩地才能CALL
        {
            if (targetFloor - nowFloor == 0)
                return;
            else if (targetFloor - nowFloor > 1)
                GoToFloor(targetFloor - 1);
            else if (targetFloor - nowFloor < -1)
                GoToFloor(targetFloor + 1);
            if (targetFloor - nowFloor == 1)
            {
                int originFloor = nowFloor;
                while (originFloor == nowFloor)
                {
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
            }
            else if (targetFloor - nowFloor == -1)
            {
                int originFloor = nowFloor;
                while (originFloor == nowFloor)
                {
                    if (targetFloor == 3)
                        RopeClimbing(-604, true, 732, 1032, 60, 60);
                    else if (targetFloor == 2)
                        RopeClimbing(-415, true, 492, 732, 60, 60);
                    else if (targetFloor == 1)
                        RopeClimbing(-128, true, 252, 432, 60, 60);
                }
            }
        }
    }
}

