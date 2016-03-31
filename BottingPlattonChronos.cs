using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingPlattonChronos : BottingBase
    {
        static int nowFloor = 0;
        internal static void bottingPlatoonChronos()//進化妖魔
        {
            _threadOfTraining = new Thread(bottingPlatoonChronosTraining);
            _threadOfTraining.Start();
            while (true)
            {
                int CharacterY = getCharacterY();
                if (CharacterY >= -6 && CharacterY <= 72)
                    nowFloor = 1;
                else if (CharacterY >= 229 && CharacterY <= 312)
                    nowFloor = 2;
                else if (CharacterY >= 474 && CharacterY <= 552)
                    nowFloor = 3;
                else if (CharacterY >= 714 && CharacterY <= 792)
                    nowFloor = 4;
                else if (CharacterY >= 949 && CharacterY <= 1032)
                    nowFloor = 5;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        internal static void bottingPlatoonChronosTraining()
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 10 == 0)
                {
                    //go to position start
                    GoToFloor(4);
                    RopeClimbing(-766, false, 656, 792, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(-766, true, 552, 792, 60, 60);
                }
                else if (counter % 5 == 0)
                {
                    //go to position start
                    GoToFloor(4);
                    RopeClimbing(-766, false, 656, 792, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(-766, true, 552, 792, 60, 60);
                }
                GoToFloor(3);
                GoToX(-498);
                Attack(1);
                GoToX(-54);
                Attack(1);
                GoToFloor(4);
                GoToX(226);
                Attack(1);
                GoToX(-498);
                Attack(1);

                counter++;
            }
        }
        static void GoToFloor (int targetFloor)//腳踩地才能CALL
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
                    Hack.KeyPress(WindowHwnd, Keys.Menu);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
            }
            else if (targetFloor - nowFloor == -1)
            {
                int originFloor = nowFloor;
                while (originFloor == nowFloor)
                {
                    if (targetFloor == 4)
                        RopeClimbing(-498, true, 792, 1032, 60, 60);
                    else if (targetFloor == 3)
                        RopeClimbing(-766, true, 552, 792, 60, 60);
                    else if (targetFloor == 2)
                        RopeClimbing(-498, true, 312, 552, 60, 60);
                    else if (targetFloor == 1)
                        RopeClimbing(-766, true, 72, 312, 60, 60);
                }
            }
        }
    }
}
