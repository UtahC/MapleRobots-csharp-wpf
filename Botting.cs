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
        internal static int WindowHwnd;
        internal Keys KeyWantToPress;
        internal static string CharacterXAdr = MainWindow.CharacterXAdr;
        internal static string CharacterYAdr = MainWindow.CharacterYAdr;
        internal static string CharacterStatusAdr = MainWindow.CharacterStatusAdr;
        internal static string MapIDAdr = MainWindow.MapIDAdr;
        internal static string MobCountAdr = MainWindow.MobCountAdr;
        internal static string DoorXAdr = MainWindow.DoorXAdr;
        internal static string DoorYAdr = MainWindow.DoorYAdr;
        internal static Thread _threadOfTraining;
        private QfDm dmBotting;

        internal static ManualResetEvent mre_KeyPresser = new ManualResetEvent(true);
        internal static ManualResetEvent mre_PickUp = new ManualResetEvent(true);

        public Botting()
        {
            dmBotting = new QfDm();
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
        internal void Attack(int times)
        {
            times = times * 50;
            while (times >= 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyAttack);
                Thread.Sleep(50);
                times--;
            }
        }
        internal int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        internal void GoToNearX(int coorX, int leftDistance, int rightDistance)
        {
            //dmBotting = new QfDm();
            int CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
            if (Distance(CharacterX, 0, coorX - 60, 0) < Distance(CharacterX, 0, coorX + 60, 0))
            {
                GoToX(coorX - leftDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Right);
                Thread.Sleep(1);
            }
            else
            {
                GoToX(coorX + rightDistance, 8, true, false, 0);
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Left);
                Thread.Sleep(1);
            }
        }

        internal void GoToLocationInWater(int coorX, int coorY, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
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
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dmBotting.DM.ReadInt(WindowHwnd, CharacterYAdr, 0);
                //Debug.WriteLine("trying to get " + coorX + " , " + coorY + " and now at " + CharacterX + " , " + CharacterY);
                if (dmBotting.DM.ReadInt(WindowHwnd, MapIDAdr, 0) == targetMapID)
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

            }

        }

        internal void GoToX(int coorX, int deviation, bool isTeleport, bool isWithUp, int targetMapID)
        {
            //dmBotting = new QfDm();
            int CharacterX;
            int leftBoundary = coorX - deviation;
            int rightBoundary = coorX + deviation;
            int leftFarBoundary = coorX - 150;
            int rightFarBoundary = coorX + 150;
            while (true)
            {
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                if (dmBotting.DM.ReadInt(WindowHwnd, MapIDAdr, 0) == targetMapID)
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
            }
        }

        internal void RopeClimbing(bool isClimbToTop, int coorX, int topBoundary, int floorY, int leftDistance, int rightDistance)
        {
            //dmBotting = new QfDm();
            int CharacterX, CharacterY, CharacterStatus;
            GoToNearX(coorX, leftDistance, rightDistance);
            Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyJump);
            GoToX(coorX, 4, true, true, 0);
            while (true)
            {
                CharacterX = dmBotting.DM.ReadInt(WindowHwnd, CharacterXAdr, 0);
                CharacterY = dmBotting.DM.ReadInt(WindowHwnd, CharacterYAdr, 0);
                CharacterStatus = dmBotting.DM.ReadInt(WindowHwnd, CharacterStatusAdr, 0);
                if (isClimbToTop && CharacterY <= topBoundary && CharacterStatus != 14 && CharacterStatus != 15)
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (!isClimbToTop && (CharacterStatus == 14 || CharacterStatus == 15))
                {
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Up);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Right);
                    Hack.KeyUp((IntPtr)WindowHwnd, Keys.Left);
                    return;
                }
                else if (CharacterX >= coorX - 5 && CharacterX <= coorX + 5 && CharacterY <= floorY)
                    Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                else
                    RopeClimbing(isClimbToTop, coorX, topBoundary, floorY, leftDistance, rightDistance);
            }
        }
        internal void bottingBubbling()
        {
            //dmBotting = new QfDm();

        }
        internal void bottingGoby()
        {
            dmBotting = new QfDm();
            object outX = -1, outY = -1;
            int lastItemX, lastItemY; 
            string lastItemColor, lastItemColor2;
            dmBotting.DM.BindWindow(WindowHwnd, "normal", "normal", "normal", 0);
            dmBotting.DM.SetPath(".\\data"); 
            object windowX1, windowX2, windowY1, windowY2;
            dmBotting.DM.GetWindowRect(WindowHwnd, out windowX1, out windowY1, out windowX2, out windowY2);
            Debug.WriteLine(windowX1 + "," + windowY1 + "," + windowX2 + "," + windowY2);
            windowX = ((int)windowX2 - 800) / 2;
            windowY = ((int)windowY2 - 600) - windowX;
            Debug.WriteLine(windowX + "," + windowY);
            //MessageBox.Show(windowX + "," + windowY);
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "ItemInventory.bmp", "000000", 0.9, 0, out outX, out outY) < 0)
            {
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.I);
                Thread.Sleep(1000);
            }
            //MessageBox.Show("ItemInventoryX = " + outX + ", ItemInventoryY = " + outY);
            lastItemX = (int)outX + 123 + windowX;
            lastItemY = (int)outY + 225 + windowY;
            Debug.WriteLine("lastItemX = " + lastItemX + ", lastItemY = " + lastItemY);
            _threadOfTraining = new Thread(bottingGobyTraining);
            _threadOfTraining.Start();
            lastItemColor = dmBotting.DM.GetColor(lastItemX, lastItemY);
            while (true)
            {
                lastItemColor = dmBotting.DM.GetColor(lastItemX, lastItemY);
                char[] array1 = lastItemColor.ToArray();
                lastItemColor2 = dmBotting.DM.GetColor(lastItemX + 10, lastItemY - 10);
                char[] array2 = lastItemColor2.ToArray();
                int sum1 = 0, sum2 = 0;
                for (int i = 0; i < 6; i++)
                {
                    sum1 = sum1 + array1[i];
                    sum2 = sum2 + array2[i];
                }
                if (sum1 < 594 || sum1 > 606 || sum2 < 594 || sum2 > 606)
                {
                    //MessageBox.Show("color = " + lastItemColor + ", color2 = " + lastItemColor2);
                    Debug.WriteLine("color = " + lastItemColor + ", color2 = " + lastItemColor2);
                    _threadOfTraining.Abort();
                    _threadOfTraining = null;
                    bottingGobyShopping();
                    _threadOfTraining = new Thread(bottingGobyTraining);
                    _threadOfTraining.Start();
                }
                Thread.Sleep(2000);
            }
        }

        internal void bottingGobyTraining()
        {
            dmBotting = new QfDm();
            //Random random = new Random(Guid.NewGuid().GetHashCode());
            int counter = 0;
            while (true)
            {
                GoToLocationInWater(-446, 520, 20, true, false, 0);
                Attack(2);
                if (counter % 3 == 0)
                {
                    mre_PickUp.Reset();
                    //GoToX(-689, 20, false, false, 0);
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
                } while (dmBotting.DM.ReadInt(WindowHwnd, MobCountAdr, 0) > 21);
                if (counter % 3 == 0)
                    GoToLocationInWater(-829, 875, 20, true, false, 0);
                else if (counter % 3 == 1)
                    GoToLocationInWater(-83, 450, 20, false, false, 0);
                counter++;
            }
        }

        internal void bottingGobyShopping()
        {
            //dmBotting = new QfDm();
            // Goby -> Aquarium
            dmBotting.DM.SetPath(".\\data");
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000000)
            {
                int doorX, doorY, tempDoorX, tempDoorY;
                doorX = dmBotting.DM.ReadInt(WindowHwnd, DoorXAdr, 0);
                doorY = dmBotting.DM.ReadInt(WindowHwnd, DoorYAdr, 0);
                tempDoorX = doorX;
                tempDoorY = doorY;
                while (doorX == tempDoorX && doorY == tempDoorY)
                {
                    doorX = dmBotting.DM.ReadInt(WindowHwnd, DoorXAdr, 0);
                    doorY = dmBotting.DM.ReadInt(WindowHwnd, DoorYAdr, 0);
                    Hack.KeyPress((IntPtr)WindowHwnd, MainWindow.keyDoor);
                    Thread.Sleep(1000);
                }
                DateTime time_start = DateTime.Now;
                double result = 0;
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230040100 && result < 8000)
                {
                    GoToLocationInWater(doorX, doorY, 5, true, true, 230000000);
                    DateTime time_end = DateTime.Now;//計時結束 取得目前時間
                                                     //後面的時間減前面的時間後 轉型成TimeSpan即可印出時間差
                    result = ((TimeSpan)(time_end - time_start)).TotalMilliseconds;
                }
            }
            mre_PickUp.Reset();
            // Aquarium -> Aquarium Store
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000002)
            {
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000000)
                {
                    GoToLocationInWater(190, 11, 5, true, true, 230000002);
                }
            }
            // Selling
            object outX, outY;
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY) < 0)
            {
                dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellerInAquarium.bmp", "000000", 0.9, 0, out outX, out outY);
                if ((int)outX > 0 && (int)outY > 0)
                {
                    dmBotting.DM.MoveTo((int)outX, (int)outY);
                    dmBotting.DM.LeftDoubleClick();
                }
            }
            selling();
            // Aquarium Store -> Aquarium
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230000000)
            {
                Hack.KeyDown((IntPtr)WindowHwnd, Keys.Up);
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000002)
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
            while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) != 230040100)
            {
                GoToLocationInWater(195, 340, 5, true, true, 230040100);
                while (dmBotting.DM.ReadInt(WindowHwnd, MainWindow.MapIDAdr, 0) == 230000000)
                {
                    GoToLocationInWater(674, 340, 5, true, true, 230040100);
                }
            }
            mre_PickUp.Set();
        }

        internal void selling()
        {
            //dmBotting = new QfDm();
            object outX, outY;
            dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY);
            int Xi = (int)outX;
            int Yi = (int)outY + 104;
            Debug.WriteLine("SellItemX = " + outX + ", SellItemY = " + outY);
            string sureSellColor1, sureSellColor2;
            sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
            while (dmBotting.DM.FindPic(403 + windowX, 256 + windowY, 439 + windowX, 292 + windowY, "EmptyEqu.bmp", "000000", 0.7, 0, out outX, out outY) < 0)
            {
                dmBotting.DM.MoveTo(Xi, Yi);
                dmBotting.DM.LeftClick();
                dmBotting.DM.LeftClick();
                dmBotting.DM.LeftClick();
                sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
                sureSellColor2 = dmBotting.DM.GetColor(481 + windowX, 271 + windowY);
                if (sureSellColor1 == "4488bb" && sureSellColor2 == "4488bb")
                    Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
            }
            while (dmBotting.DM.FindPic(windowX, windowY, 800 + windowX, 600 + windowY, "SellItem.bmp", "000000", 0.9, 0, out outX, out outY) >= 0)
                Hack.KeyPress((IntPtr)WindowHwnd, Keys.Enter);
        }
    }
}
