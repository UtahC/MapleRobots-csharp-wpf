using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingWolfSpider : BottingBase
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
                if (CharacterY <= -84 && CharacterX <= 0)
                    nowFloor = 1;
                else if (CharacterY > -84 && CharacterY <= 216 && CharacterX <= 0)
                    nowFloor = 2;
                else if (CharacterY > 216)
                    nowFloor = 3;
                else if (CharacterY > -84 && CharacterY <= 216 && CharacterX >= 0)
                    nowFloor = 4;
                else if (CharacterY <= -84 && CharacterX >= 0)
                    nowFloor = 5;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        internal static void training()
        {
            int counter = 0;
            Thread.Sleep(500);
            //training start
            while (true)
            {
                if (counter % 4 == 0)
                {
                    //go to position start
                    gotoFloor(3);
                    GoToX(-325, 0, true, false, 0);
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
                    gotoFloor(3);
                    GoToX(-325, 0, true, false, 0);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    AutoKey.mre_PickUp.Set();
                }
                gotoFloor(2);
                Attack(1);
                gotoFloor(1);
                GoToX(-1300);
                Attack(1);
                GoToX(-810);
                Attack(1);
                GoToX(-345);
                Attack(1);
                GoToX(0);
                Attack(2);
                gotoFloor(4);
                Attack(1);
                gotoFloor(5);
                GoToX(1300);
                Attack(1);
                GoToX(810);
                Attack(1);
                GoToX(345);
                Attack(1);
                GoToX(0);
                Attack(2);

                counter++;
            }
        }
        static void gotoFloor(int targetFloor)
        {
            while (nowFloor != targetFloor)
            {
                if (targetFloor == 3)
                {
                    while (nowFloor != 3)
                    {
                        GoToX(0);
                        Hack.KeyDown(WindowHwnd, Keys.Down);
                        Thread.Sleep(50);
                        Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                        Thread.Sleep(30);
                        Hack.KeyUp(WindowHwnd, Keys.Down);
                    }
                }
                if (Math.Abs(targetFloor - nowFloor) > 1)
                {
                    gotoFloor(targetFloor - 1);
                    gotoFloor(targetFloor + 1);
                }
                else if (targetFloor == 2 && nowFloor == 3)
                {
                    GoToX(-290, 0, true, true, 2);
                }
                else if (targetFloor == 4 && nowFloor == 3)
                {
                    GoToX(290, 0, true, true, 4);
                }
                else if (targetFloor == 1 && nowFloor == 2)
                {
                    GoToX(-825, 0, true, true, 1);
                }
                else if (targetFloor == 5 && nowFloor == 4)
                {
                    GoToX(770, 0, true, true, 5);
                }
            }
        }
        internal static new void GoToX(int coorX, int deviation, bool isTeleport, bool isWithUp, int targetFloor)
        {
            int CharacterX;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int leftFarBoundary = coorX - 150;
            int rightFarBoundary = coorX + 150;
            while (true)
            {
                CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                if (nowFloor == targetFloor)
                {
                    return;
                }
                else if (CharacterX >= leftBoundary && CharacterX <= rightBoundary)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    Hack.KeyUp(WindowHwnd, Keys.Down);
                    Hack.KeyUp(WindowHwnd, Keys.Left);
                    Hack.KeyUp(WindowHwnd, Keys.Right);
                    return;
                }
                else if (CharacterX < leftFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Left);
                    Hack.KeyDown(WindowHwnd, Keys.Right);
                    if (isTeleport)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > rightFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Right);
                    Hack.KeyDown(WindowHwnd, Keys.Left);
                    if (isTeleport)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > leftFarBoundary && CharacterX < leftBoundary)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Left);
                    Hack.KeyDown(WindowHwnd, Keys.Right);
                    if (isWithUp)
                        Hack.KeyDown(WindowHwnd, Keys.Up);
                }
                else if (CharacterX > rightBoundary && CharacterX < rightFarBoundary)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Right);
                    Hack.KeyDown(WindowHwnd, Keys.Left);
                    if (isWithUp)
                        Hack.KeyDown(WindowHwnd, Keys.Up);
                }
                Thread.Sleep(10);
            }
        }
    }
}
