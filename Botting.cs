using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class Botting
    {
        private static int windowX, windowY;
        internal static IntPtr WindowHwnd;
        internal Keys KeyWantToPress;
        internal static Thread _threadOfTraining;

        internal static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);
        internal static ManualResetEvent mre_PickUp = new ManualResetEvent(true);

        public Botting()
        {
            
        }
        internal void PickUp()
        {
            while (true)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, MainWindow.keyPickUp);
                Thread.Sleep(50);
                mre_PickUp.WaitOne();
            }
        }
        internal void KeyPresser()
        {
            while (true)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, KeyWantToPress);
                Thread.Sleep(50);
                mre_KeyPresser.WaitOne();
            }
        }
        internal static void Attack(int times)
        {
            times = times * 50;
            while (times >= 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyAttack);
                Thread.Sleep(50);
                times--;
            }
        }
        internal static int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        internal static void GoToNearX(int coorX, int leftDistance, int rightDistance)
        {
            int CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
            int originX = CharacterX;
            if (Distance(CharacterX, 0, coorX - 60, 0) < Distance(CharacterX, 0, coorX + 60, 0))
            {
                GoToX(coorX - leftDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
            }
            else
            {
                GoToX(coorX + rightDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
            }
            Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
            Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
            CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
            if (CharacterX < coorX)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                if (originX - CharacterX > 0)
                    Thread.Sleep(200);
            }
            else
            { 
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                if(originX - CharacterX < 0)
                    Thread.Sleep(200);
            }
            
        }

        internal static void GoToLocationInWater(int coorX, int coorY, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
        {
            //dmBotting = new QfDm();
            int CharacterX, CharacterY;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int upBoundary = coorY - deviation * 2;
            int downBoundary = coorY;
            Debug.WriteLine("trying to get to " + coorX + " , " + coorY);
            if (isWithUp)
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
            while (true)
            {
                CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                CharacterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                //Debug.WriteLine("trying to get " + coorX + " , " + coorY + " and now at " + CharacterX + " , " + CharacterY);
                if (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == targetMapID)
                {
                    Debug.WriteLine("arrive " + targetMapID);
                    return;
                }
                else if (CharacterX >= leftBoundary && CharacterX <= rightBoundary && CharacterY >= upBoundary && CharacterY <= downBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Debug.WriteLine("arrive location");
                    return;
                }
                else if (CharacterX < leftBoundary || CharacterX > rightBoundary)
                {
                    if (isWithUp && Distance(coorX, coorY, CharacterX, CharacterY) < 200)
                        GoToX(coorX, deviation, isTeleport, isWithUp, targetMapID);
                    else
                        GoToX(coorX, deviation, isTeleport, false, targetMapID);
                }
                else if (CharacterY > downBoundary + 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
                }
                else if (CharacterY < upBoundary - 20)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Menu);
                }
                Thread.Sleep(10);
            }

        }
        internal static void GoToX(int coorX)
        {
            GoToX(coorX, 20, true, false, 0);
        }
        internal static void GoToX(int coorX, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
        {
            //dmBotting = new QfDm();
            int CharacterX;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int leftFarBoundary = coorX - 150;
            int rightFarBoundary = coorX + 150;
            while (true)
            {
                CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                if (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == targetMapID)
                {
                    Debug.WriteLine("arrive " + targetMapID);
                    return;
                }
                else if (CharacterX >= leftBoundary && CharacterX <= rightBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Down);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    return;
                }
                else if (CharacterX < leftFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                    if (isTeleport)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > rightFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                    if (isTeleport)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyTeleport);
                }
                else if (CharacterX > leftFarBoundary && CharacterX < leftBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                    if (isWithUp)
                        Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
                else if (CharacterX > rightBoundary && CharacterX < rightFarBoundary)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                    if (isWithUp)
                        Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
                Thread.Sleep(10);
            }
        }
        internal static void RopeExiting(bool isLeft)
        {
            int originX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
            int originY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
            int CharacterX = originX;
            int CharacterY = originY;
            while (CharacterX == originX || CharacterY == originY)
            {
                CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                CharacterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                if (isLeft)
                    Hack.KeyDown(WindowHwnd, Keys.Left);
                else
                    Hack.KeyDown(WindowHwnd, Keys.Right);
                Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
            }
            Hack.KeyUp(WindowHwnd, Keys.Left);
            Hack.KeyUp(WindowHwnd, Keys.Right);
        }
        internal static void RopeClimbing(int coorX, bool isClimbToTop, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            //dmBotting = new QfDm();
            int CharacterX, CharacterY, CharacterStatus;
            CharacterStatus = Hack.ReadInt(MainWindow.process, MainWindow.CharacterStatusBaseAdr, MainWindow.CharacterStatusOffset);
            if (CharacterStatus < 14 || CharacterStatus > 17)
            {
                GoToNearX(coorX, leftDistance, rightDistance);
                Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
                GoToX(coorX, 4, true, true, 0);
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
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (CharacterX >= coorX - 5 && CharacterX <= coorX + 5 && CharacterY <= floorY)
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                else
                    RopeClimbing(coorX, isClimbToTop, topBoundary, floorY, leftDistance, rightDistance);
            }
        }
        internal static void JumpingOver(int coorX, bool isLeft)
        {
            GoToX(coorX);
            if (isLeft)
                Hack.KeyDown(WindowHwnd, Keys.Left);
            else
                Hack.KeyDown(WindowHwnd, Keys.Right);
            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
        }
        internal static void upstairTeleport(int upstairY)
        {
            int needToDownY = upstairY - 40;
            while (true)
            {
                int characterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                if (characterY <= upstairY && characterY >= needToDownY)
                {
                    Hack.KeyUp(WindowHwnd, Keys.Up);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                    break;
                }
                else if (characterY < needToDownY)
                {
                    Hack.KeyDown(WindowHwnd, Keys.Down);
                    Thread.Sleep(1);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyJump);
                }
                else
                {
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    Hack.KeyPress(WindowHwnd, MainWindow.keyTeleport);
                }
            }
        }
        internal static void bottingBubbling()
        {
            int counter = 0;
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
                    mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    mre_PickUp.Set();
                    RopeExiting(true);
                }
                else if (counter % 3 == 0)
                {
                    //go to position start
                    GoToX(2800);
                    upstairTeleport(73);
                    RopeClimbing(2859, false, -38, 73, 60, 60);
                    //go to position end
                    mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    mre_PickUp.Set();
                    RopeExiting(true);
                }
                GoToX(2801);
                upstairTeleport(73);
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
        internal static void bottingWildBoar()//黑肥肥
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 3 ==0)
                {
                    //go to position start
                    if (Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset) > 2055)
                        RopeClimbing(1380, true, 2055, 2205, -60, 60);
                    RopeClimbing(834, false, 1845, 2205, 60, 60);
                    //go to position end
                    mre_PickUp.Reset();
                    if (counter % 6 == 0)
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1000);
                    }
                    mre_PickUp.Set();
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
        internal static void bottingTeddy()//發條熊
        {
            int CharacterX, CharacterY, counter = 0;
            //training start
            while (true)
            {
                if (counter % 5 == 0)
                {
                    //go to position start
                    do
                    {
                        GoToX(-179);
                        upstairTeleport(6);
                        RopeClimbing(-320, false, -213, 6, -70, 70);
                        CharacterX = Hack.ReadInt(MainWindow.process, MainWindow.CharacterXBaseAdr, MainWindow.CharacterXOffset);
                        CharacterY = Hack.ReadInt(MainWindow.process, MainWindow.CharacterYBaseAdr, MainWindow.CharacterYOffset);
                    } while (CharacterX != -320 && CharacterY > -213);
                    

                    //go to position end
                    mre_PickUp.Reset();
                    if (counter % 10 == 0)
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1000);
                    }
                    mre_PickUp.Set();
                    RopeExiting(true);
                }
                GoToX(-607);
                Attack(1);
                GoToX(-80);
                Attack(1);
                GoToX(598);
                Attack(1);
                GoToX(-80);
                Attack(1);

                counter++;
            }
        }
        internal static void bottingWraith()//大幽靈
        {
            int counter = 0;
            //training start
            while (true)
            {
                if (counter % 6 == 0)
                {
                    //go to position start
                    RopeClimbing(-92, false, 36, 198, 60, 60);
                    //go to position end
                    mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    mre_PickUp.Set();
                    RopeExiting(false);
                }

                else if (counter % 3 == 0)
                {
                    //go to position start
                    RopeClimbing(-92, false, 36, 198, 60, 60);
                    //go to position end
                    mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    mre_PickUp.Set();
                    RopeExiting(false);
                }
                GoToX(438);
                upstairTeleport(73);
                Attack(1);
                upstairTeleport(-47);
                GoToX(1214);
                upstairTeleport(73);
                Attack(1);
                upstairTeleport(-17);
                GoToX(1732);
                Attack(1);
                JumpingOver(1800, false);
                GoToX(2428);
                upstairTeleport(73);
                Attack(1);
                GoToX(1648);
                Attack(1);
                GoToX(904);
                upstairTeleport(73);
                Attack(1);


                counter++;
            }
        }
        internal static void bottingGoby()
        {
            object outX = -1, outY = -1;
            int lastItemX, lastItemY, tempX, tempY;
            Hack.GetClientRectangle(MainWindow.process.MainWindowHandle, 800, 600, out windowX, out windowY, out tempX, out tempY);
            Debug.WriteLine(windowX + "," + windowY);
            //Hack.ShowMessageBox(windowX + "," + windowY);
            lastItemX = 757;
            lastItemY = 236;
            Debug.WriteLine("lastItemX = " + lastItemX + ", lastItemY = " + lastItemY);
            _threadOfTraining = new Thread(bottingGobyTraining);
            _threadOfTraining.Start();
            /*while (true)
            {
                if (!(Hack.CompareColor(MainWindow.WindowHwnd, lastItemX, lastItemY, "DDDDDD", "222222") &&
                      Hack.CompareColor(MainWindow.WindowHwnd, lastItemX + 5, lastItemY - 5, "DDDDDD", "222222") &&
                      Hack.CompareColor(MainWindow.WindowHwnd, lastItemX + 10, lastItemY - 10, "DDDDDD", "222222")))
                {
                    _threadOfTraining.Abort();
                    _threadOfTraining = null;
                    bottingGobyShopping();
                    _threadOfTraining = new Thread(bottingGobyTraining);
                    _threadOfTraining.Start();
                }
                Thread.Sleep(2000);
            }*/
        }

        internal static void bottingGobyTraining()
        {
            //Random random = new Random(Guid.NewGuid().GetHashCode());
            int counter = 0;
            while (true)
            {
                GoToLocationInWater(-446, 520, 20, true, false, 0);
                Attack(2);
                if (counter % 3 == 0)
                {
                    mre_PickUp.Reset();
                    if (counter % 6 == 0)
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 10; i++)
                            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1500);
                    }
                    mre_PickUp.Set();
                }
                else if (counter % 3 == 1)
                    GoToX(-265, 20, false, false, 0);
                GoToLocationInWater(-585, 1032, 20, true, false, 0);
                Attack(1);
                GoToLocationInWater(-428, 1220, 20, true, false, 0);
                Attack(1);
                if (counter % 3 == 1)
                    GoToLocationInWater(-83, 1582, 20, false, false, 0);
                else
                    GoToLocationInWater(-316, 1596, 20, true, false, 0);
                Attack(1);
                do
                {
                    Attack(1);
                } while (Hack.ReadInt(MainWindow.process, MainWindow.MobCountBaseAdr, MainWindow.MobCountOffset) > 21);
                if (counter % 3 == 0)
                    GoToLocationInWater(-829, 875, 20, true, false, 0);
                else if (counter % 3 == 1)
                    GoToLocationInWater(-83, 450, 20, false, false, 0);
                counter++;
            }
        }

        internal static void bottingGobyShopping()
        {
            // Goby -> Aquarium
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230000000)
            {
                int doorX, doorY, tempDoorX, tempDoorY;
                doorX = Hack.ReadInt(MainWindow.process, MainWindow.DoorXBaseAdr);
                doorY = Hack.ReadInt(MainWindow.process, MainWindow.DoorYBaseAdr);
                tempDoorX = doorX;
                tempDoorY = doorY;
                while (doorX == tempDoorX && doorY == tempDoorY)
                {
                    doorX = Hack.ReadInt(MainWindow.process, MainWindow.DoorXBaseAdr);
                    doorY = Hack.ReadInt(MainWindow.process, MainWindow.DoorYBaseAdr);
                    Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyDoor);
                    Thread.Sleep(1000);
                }
                DateTime time_start = DateTime.Now;
                double result = 0;
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230040100 && result < 8000)
                {
                    GoToLocationInWater(doorX, doorY, 5, true, true, 230000000);
                    DateTime time_end = DateTime.Now;//計時結束 取得目前時間
                                                     //後面的時間減前面的時間後 轉型成TimeSpan即可印出時間差
                    result = ((TimeSpan)(time_end - time_start)).TotalMilliseconds;
                }
            }
            mre_PickUp.Reset();
            // Aquarium -> Aquarium Store
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230000002)
            {
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230000000)
                {
                    GoToLocationInWater(190, 11, 5, true, true, 230000002);
                }
            }
            // Selling
            while (!Hack.CompareColor(MainWindow.WindowHwnd, 580, 174, "EE8844", "111111"))
            {
                int screenX, screenY;
                Hack.ClientToScreen(MainWindow.WindowHwnd, 267, 65, out screenX, out screenY);
                Hack.MoveTo(screenX, screenY);//seller of Aqua store
                Hack.LeftDoubleClick();
            }
            selling();
            // Aquarium Store -> Aquarium
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230000000)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230000002)
                {
                    GoToX(-349, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                    GoToX(-351, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                    GoToX(-347, 0, false, true, 230000000);
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                }
            }
            // Aquarium -> Goby
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230040100)
            {
                GoToLocationInWater(195, 340, 5, true, true, 230040100);
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230000000)
                {
                    GoToLocationInWater(674, 340, 5, true, true, 230040100);
                }
            }
            mre_PickUp.Set();
        }

        internal static void selling()
        {
            int firstItemIconScreenX, firstItemIconScreenY;
            int firstItemScreenX, firstItemScreenY;
            int sureSellScreenX, sureSellScreenY;
            Hack.ClientToScreen(MainWindow.WindowHwnd, 421, 274, out firstItemIconScreenX, out firstItemIconScreenY);
            Hack.ClientToScreen(MainWindow.WindowHwnd, 540, 274, out firstItemScreenX, out firstItemScreenY);
            Hack.ClientToScreen(MainWindow.WindowHwnd, 297, 271, out sureSellScreenX, out sureSellScreenY);
            while (!(Hack.CompareColor(IntPtr.Zero, firstItemIconScreenX, firstItemIconScreenY, "DDDDDD", "222222") &&
                    (Hack.CompareColor(IntPtr.Zero, firstItemIconScreenX + 10, firstItemIconScreenY - 10, "DDDDDD", "222222"))))
            {
                Hack.MoveTo(firstItemScreenX, firstItemScreenY);
                Hack.LeftDoubleClick();
                //sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
                //sureSellColor2 = dmBotting.DM.GetColor(481 + windowX, 271 + windowY);
                if (Hack.CompareColor(IntPtr.Zero, sureSellScreenX, sureSellScreenY, "4488BB", "000000"))
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
            }
            while (Hack.CompareColor(MainWindow.WindowHwnd, 580, 174, "EE8844", "111111"))
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
        }
    }
}
