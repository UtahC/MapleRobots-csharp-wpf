using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingULU1 : BottingBase
    {
        static int nowFloor = 0;
        internal static void botting()
        {
            _threadOfTraining = new Thread(training);
            _threadOfTraining.Start();
            while (true)
            {
                int CharacterX = getCharacterX();
                int CharacterY = getCharacterY();
                if (CharacterY <= -727)
                    nowFloor = 1;
                else if (CharacterY > -727 && CharacterY <= -586)
                    nowFloor = 2;
                else if (CharacterY > -586 && CharacterY <= -286 && CharacterX <= 693)
                    nowFloor = 3;
                else if (CharacterY > -424 && CharacterY <= -214 && CharacterX > 693)
                    nowFloor = 4;
                else if (CharacterY > -286 && CharacterY <= 14)
                    nowFloor = 4;
                else if (CharacterY > 14)
                    nowFloor = 5;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        internal static void training()
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && nowFloor != 4 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(4);
                    RopeClimbing(869, false, -574, -346, -60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(869, true, -727, -346, -60, 60);
                    GoToX(750);
                    Attack(1);
                    gotoFloor(2);
                    Attack(1);
                    gotoFloor(3);
                    GoToX(533);
                    Attack(1);
                }
                else if (counter % 3 == 0)
                {
                    int CharacterStatus = getCharacterStatus();
                    if (nowFloor != 3 && nowFloor != 4 && (CharacterStatus < 14 || CharacterStatus > 17))
                        gotoFloor(4);
                    RopeClimbing(869, false, -574, -346, -60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    AutoKey.mre_PickUp.Set();
                    RopeClimbing(869, true, -727, -346, -60, 60);
                    GoToX(750);
                    Attack(1);
                    gotoFloor(2);
                    Attack(1);
                    gotoFloor(3);
                    GoToX(533);
                    Attack(1);
                }
                gotoFloor(4);
                GoToX(490);
                Attack(1);
                GoToX(-128);
                Attack(1);
                GoToX(490);
                Attack(1);
                gotoFloor(3);
                GoToX(500);
                Attack(1);
                gotoFloor(2);
                Attack(1);
                gotoFloor(3);
                GoToX(500);
                Attack(1);

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
                    GoToX(-39);
                }
                else if (targetFloor == 3 && nowFloor == 2)
                {
                    GoToX(-50);
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
                else if (targetFloor == 4 && nowFloor == 3)
                {
                    GoToX(533);
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
                else if (targetFloor == 5 && nowFloor == 4)
                {
                    GoToX(-128);
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                }
                else if (targetFloor == 4 && nowFloor == 5)
                {
                    GoToX(-128);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                }
                else if (targetFloor == 3 && nowFloor == 4)
                {
                    GoToX(615);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                }
                else if (targetFloor == 2 && nowFloor == 3)
                {
                    GoToX(-50);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    Thread.Sleep(50);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    Thread.Sleep(30);
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                }
                else if (targetFloor == 1 && nowFloor == 2)
                {
                    int CharacterY = getCharacterY();
                    if (CharacterY > -616)
                    {
                        GoToX(-50);
                        Hack.KeyDown(WindowHwnd, Keys.Up);
                        Thread.Sleep(50);
                        Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                        Thread.Sleep(30);
                        Hack.KeyUp(WindowHwnd, Keys.Up);
                    }
                    GoToX(290);
                }
            }
        }
    }
}
