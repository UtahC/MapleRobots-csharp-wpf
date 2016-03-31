using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class BottingGoby : BottingBase
    {
        internal static void bottingGoby()
        {
            object outX = -1, outY = -1;
            int lastItemX, lastItemY, tempX, tempY;
            //Hack.GetClientRectangle(MainWindow.process.MainWindowHandle, 800, 600, out windowX, out windowY, out tempX, out tempY);
            //Debug.WriteLine(windowX + "," + windowY);
            //Hack.ShowMessageBox(windowX + "," + windowY);
            lastItemX = 757;
            lastItemY = 236;
            Debug.WriteLine("lastItemX = " + lastItemX + ", lastItemY = " + lastItemY);
            _threadOfTraining = new Thread(bottingGobyTraining);
            _threadOfTraining.Start();
            /*while (true)
            {
                if (!(Hack.CompareColor(WindowHwnd, lastItemX, lastItemY, "DDDDDD", "222222") &&
                      Hack.CompareColor(WindowHwnd, lastItemX + 5, lastItemY - 5, "DDDDDD", "222222") &&
                      Hack.CompareColor(WindowHwnd, lastItemX + 10, lastItemY - 10, "DDDDDD", "222222")))
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
                    AutoKey.mre_PickUp.Reset();
                    if (counter % 6 == 0)
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        GoToLocationInWater(-585, 882, 20, true, false, 0);
                        for (int i = 0; i < 10; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1500);
                    }
                    AutoKey.mre_PickUp.Set();
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
                    Hack.KeyPress(WindowHwnd, MainWindow.keyDoor);
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
            AutoKey.mre_PickUp.Reset();
            // Aquarium -> Aquarium Store
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230000002)
            {
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230000000)
                {
                    GoToLocationInWater(190, 11, 5, true, true, 230000002);
                }
            }
            // Selling
            while (!Hack.CompareColor(WindowHwnd, 580, 174, "EE8844", "111111"))
            {
                int screenX, screenY;
                Hack.ClientToScreen(WindowHwnd, 267, 65, out screenX, out screenY);
                Hack.MoveTo(screenX, screenY);//seller of Aqua store
                Hack.LeftDoubleClick();
            }
            selling();
            // Aquarium Store -> Aquarium
            while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) != 230000000)
            {
                Hack.KeyDown(WindowHwnd, Keys.Up);
                while (Hack.ReadInt(MainWindow.process, MainWindow.MapIDBaseAdr, MainWindow.MapIDOffset) == 230000002)
                {
                    GoToX(-349, 0, false, true, 230000000);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    GoToX(-351, 0, false, true, 230000000);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
                    GoToX(-347, 0, false, true, 230000000);
                    Hack.KeyDown(WindowHwnd, Keys.Up);
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
            AutoKey.mre_PickUp.Set();
        }

        internal static void selling()
        {
            int firstItemIconScreenX, firstItemIconScreenY;
            int firstItemScreenX, firstItemScreenY;
            int sureSellScreenX, sureSellScreenY;
            Hack.ClientToScreen(WindowHwnd, 421, 274, out firstItemIconScreenX, out firstItemIconScreenY);
            Hack.ClientToScreen(WindowHwnd, 540, 274, out firstItemScreenX, out firstItemScreenY);
            Hack.ClientToScreen(WindowHwnd, 297, 271, out sureSellScreenX, out sureSellScreenY);
            while (!(Hack.CompareColor(IntPtr.Zero, firstItemIconScreenX, firstItemIconScreenY, "DDDDDD", "222222") &&
                    (Hack.CompareColor(IntPtr.Zero, firstItemIconScreenX + 10, firstItemIconScreenY - 10, "DDDDDD", "222222"))))
            {
                Hack.MoveTo(firstItemScreenX, firstItemScreenY);
                Hack.LeftDoubleClick();
                //sureSellColor1 = dmBotting.DM.GetColor(297 + windowX, 271 + windowY);
                //sureSellColor2 = dmBotting.DM.GetColor(481 + windowX, 271 + windowY);
                if (Hack.CompareColor(IntPtr.Zero, sureSellScreenX, sureSellScreenY, "4488BB", "000000"))
                    Hack.KeyPress(WindowHwnd, Keys.Enter);
            }
            while (Hack.CompareColor(WindowHwnd, 580, 174, "EE8844", "111111"))
                Hack.KeyPress(WindowHwnd, Keys.Enter);
        }
    }
}
