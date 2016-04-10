using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingSkele : BottingBase
    {
        private static int nowFloor = 0;
        private static int CharacterX;
        internal static void botting()
        {
            if (hit == 1)
                _threadOfTraining = new Thread(training1hit);
            else
                _threadOfTraining = new Thread(training2hit);
            _threadOfTraining.Start();
            while (true)
            {
                CharacterX = getCharacterX();
                int CharacterY = getCharacterY();
                if (CharacterY <= 530)
                    nowFloor = 1;
                else if (CharacterY > 530 && CharacterY <= 920)
                    nowFloor = 2;
                else if (CharacterY > 920 && CharacterY <= 1260)
                    nowFloor = 3;
                else if (CharacterY > 1260)
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
            if (isOnRope() && getCharacterX() != -58)
                RopeExiting(true);
            //training start
            while (true)
            {
                if (counter % 8 == 0)
                {
                    //go to position start
                    while (getCharacterStatus() < 14 || getCharacterStatus() > 17 || CharacterX != -58 || getCharacterY() > 979)
                    {
                        if (getCharacterStatus() >= 14 && getCharacterStatus() <= 17 && CharacterX != -58)
                            RopeExiting(true);
                        GoToFloor(3);
                        if (CharacterX < 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, 60, -60);
                        else if (CharacterX > 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, -75, 75);
                    }
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    GoToFloor(2);
                }
                else if (counter % 4 == 0)
                {
                    //go to position start
                    while (getCharacterStatus() < 14 || getCharacterStatus() > 17 || CharacterX != -58 || getCharacterY() > 979)
                    {
                        if (getCharacterStatus() >= 14 && getCharacterStatus() <= 17 && CharacterX != -58)
                            RopeExiting(true);
                        GoToFloor(3);
                        if (CharacterX < 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, 60, -60);
                        else if (CharacterX > 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, -75, 75);
                    }
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 50; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    AutoKey.mre_PickUp.Set();
                    GoToFloor(2);
                }
                else
                {
                    while (nowFloor != 2)
                        GoToFloor(2);
                }
                GoToX(180);
                Attack(1);
                GoToX(650);
                GoToX(878, 20, false, false, 0);
                Attack(1);
                if (CharacterX >= 773)
                    GoToX(878);
                GoToX(380);
                while (getCharacterY() > 1140 && nowFloor == 3)
                    JumpingOver(350, true);
                GoToX(25);
                Attack(1);
                /* if (counter % 6 == 0 && counter > 0)
                 {
                     GoToFloor(1);
                     GoToX(-183);
                 }*/


                counter++;
            }
        }
        static void training2hit()
        {
            int counter = 0;
            Thread.Sleep(500);
            if (isOnRope() && getCharacterX() != -58)
                RopeExiting(true);
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    while (getCharacterStatus() < 14 || getCharacterStatus() > 17 || CharacterX != -58 || getCharacterY() > 979)
                    {
                        if (getCharacterStatus() >= 14 && getCharacterStatus() <= 17 && CharacterX != -58)
                            RopeExiting(true);
                        GoToFloor(3);
                        if (CharacterX < 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, 60, -60);
                        else if (CharacterX > 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, -75, 75);
                    }
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    GoToFloor(2);
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    while (getCharacterStatus() < 14 || getCharacterStatus() > 17 || CharacterX != -58 || getCharacterY() > 979)
                    {
                        if (getCharacterStatus() >= 14 && getCharacterStatus() <= 17 && CharacterX != -58)
                            RopeExiting(true);
                        GoToFloor(3);
                        if (CharacterX < 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, 60, -60);
                        else if (CharacterX > 0)
                            SpecialRopeClimbing(-58, false, 979, 1180, -75, 75);
                    }
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 50; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    AutoKey.mre_PickUp.Set();
                    GoToFloor(2);
                }
                else
                {
                    while (nowFloor != 2)
                        GoToFloor(2);
                }
                GoToX(180);
                Attack(2);
                GoToX(650);
                GoToX(878, 20, false, false, 0);
                Attack(2);
                if (CharacterX >= 773)
                    GoToX(878);
                GoToX(380);
                while (getCharacterY() > 1140 && nowFloor == 3)
                    JumpingOver(350, true);
                GoToX(25);
                Attack(2);
               /* if (counter % 6 == 0 && counter > 0)
                {
                    GoToFloor(1);
                    GoToX(-183);
                }*/


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
                    GoToX(35, 8, true, false, 0);
                    if (originFloor == nowFloor)
                    {
                        Hack.KeyDown(WindowHwnd, Keys.Down);
                        Thread.Sleep(50);
                        Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                        Thread.Sleep(30);
                        Hack.KeyUp(WindowHwnd, Keys.Down);
                    }
                }
            }
            else if (targetFloor - nowFloor == -1)
            {
                int originFloor = nowFloor;
                while (originFloor == nowFloor)
                {
                    if (targetFloor == 3)
                    {
                        RopeClimbing(-316, true, 1247, 1480, 60, 60);
                        if (nowFloor == 3)
                            upstairTeleport(1098);
                    }
                    else if (targetFloor == 2)
                    {
                        while (getCharacterY() > 875 && nowFloor == 3)
                        {
                            if (CharacterX < 0)
                                SpecialRopeClimbing(-58, true, 875, 1257, 60, -60);
                            else if (CharacterX >= 0)
                                SpecialRopeClimbing(-58, true, 875, 1257, -75, 75);
                        }
                        if (nowFloor == 2)
                            upstairTeleport(774);
                    }
                    else if (targetFloor == 1)
                        RopeClimbing(-21, true, 481, 780, 60, 60);
                }
            }
        }
        internal static void SpecialRopeClimbing(int coorX, bool isClimbToTop, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            int CharacterX, CharacterY, CharacterStatus;
            CharacterStatus = Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
            if (isStand())
            {
                GoToNearX(coorX, leftDistance, rightDistance);
                if (nowFloor == 3)
                {
                    Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                    GoToXInAir(coorX, 4, true, true);
                }
            }

            CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
            CharacterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
            CharacterStatus = Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
            if (CharacterY > floorY)
            {
                return;
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
        
        }
    }
}
