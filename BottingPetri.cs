using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingPetri : BottingBase
    {
        private static int nowFloor = 0;
        private static int CharacterX;
        internal static void botting()
        {
            if (hit == 1)
            {
                _threadOfTraining = new Thread(training1hit);
                _threadOfTraining.Name = "Petri1hit";
            }
            else
            {
                _threadOfTraining = new Thread(training2hit);
                _threadOfTraining.Name = "Petri2hit";
            }
            _threadOfTraining.Start();
            int CharacterY;
            while (true)
            {
                CharacterX = getCharacterX();
                CharacterY = getCharacterY();

                if (CharacterY <= -616)
                    nowFloor = 1;
                else if (CharacterY > -616 && CharacterY <= -376)
                    nowFloor = 2;
                else if (CharacterY > -376 && CharacterY <= -136)
                    nowFloor = 3;
                else if (CharacterY > -136 && CharacterY <= 104)
                    nowFloor = 4;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        static void training1hit()
        {
            int counter = 0;
            Thread.Sleep(500);
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    GoToFloor(2);
                    GoToX(-35);
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
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToFloor(2);
                    GoToX(-35);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    AutoKey.mre_PickUp.Set();
                }
                if (nowFloor == 2)
                {
                    GoToX(-155);
                    Attack(1);
                }
                if (CharacterX > -155)
                    GoToX(-155);

                GoToFloor(3);

                GoToX(-273);
                Attack(1);
                GoToX(572);
                Attack(1);

                GoToFloor(2);

                GoToX(572);
                Attack(1);


                counter++;
            }
        }
        static void training2hit()
        {
            int counter = 0;
            Thread.Sleep(500);
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    GoToFloor(2);
                    GoToX(-35);
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
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToFloor(2);
                    GoToX(-35);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    AutoKey.mre_PickUp.Set();
                }
                if (nowFloor == 2)
                {
                    GoToX(-155);
                    Attack(1);
                }
                if (nowFloor == 2)
                { 
                    if (CharacterX < -155)
                        GoToX(-155);
                    Attack(1);
                }
                if (CharacterX > -155)
                    GoToX(-155);

                GoToFloor(3);

                GoToX(-273);
                Attack(1);
                if (CharacterX < -273)
                    GoToX(-273);
                Attack(1);
                GoToX(572);
                Attack(2);

                GoToFloor(2);

                GoToX(572);
                Attack(2);


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
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
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
                        SpecialRopeClimbing(631, true, -136, 104, 60, 60, DateTime.Now);
                    else if (targetFloor == 2)
                        SpecialRopeClimbing(765, true, -376, -136, 60, 60, DateTime.Now);
                    else if (targetFloor == 1)
                        SpecialRopeClimbing(572, true, 572, -510, 60, 60, DateTime.Now);
                }
            }
        }
        static void SpecialRopeClimbing(int coorX, bool isClimbToTop, int topBoundary, int floorY, int leftDistance, int rightDistance, DateTime time_start)
        {
            int CharacterX, CharacterY, CharacterStatus;
            CharacterStatus = Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
            DateTime time_end = DateTime.Now;//計時結束 取得目前時間
                                             //後面的時間減前面的時間後 轉型成TimeSpan即可印出時間差
            double result = ((time_end - time_start)).TotalMilliseconds;
            if (result >= 5000 && isStand())
            {
                if (_threadOfTraining.Name == "Petri1hit")
                    Attack(1);
                else
                    Attack(2);
                time_start = DateTime.Now;
            }
            if (CharacterStatus < 14 || CharacterStatus > 17)
            {
                GoToNearX(coorX, leftDistance, rightDistance);
                Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                GoToXInAir(coorX, 4, true, true);
            }
            while (true)
            {
                CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                CharacterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                CharacterStatus = Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
                if (CharacterY > floorY)
                {
                    break;
                }
                else if ((isClimbToTop && (CharacterY <= topBoundary) && ((CharacterStatus < 14) || (CharacterStatus > 17))) ||
                    (isClimbToTop == false) && (CharacterY <= topBoundary))
                {
                    Thread.Sleep(1);
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    Hack.KeyUp(WindowHwnd, Keys.Right);
                    Hack.KeyUp(WindowHwnd, Keys.Left);
                    return;
                }
                else if (CharacterX >= coorX - 20 && CharacterX <= coorX + 20 && CharacterY <= floorY)
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                else
                    RopeClimbing(coorX, isClimbToTop, topBoundary, floorY, leftDistance, rightDistance);
            }
        }
    }
}
