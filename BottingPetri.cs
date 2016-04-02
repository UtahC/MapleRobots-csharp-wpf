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
                _threadOfTraining = new Thread(training1hit);
            else
                _threadOfTraining = new Thread(training2hit);
            _threadOfTraining.Start();
            while (true)
            {
                CharacterX = getCharacterX();
                int CharacterY = getCharacterY();
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
                    for (int i = 0; i < 50; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
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
                    //for (int i = 0; i < 25; i++)
                    Hack.KeyDown(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1500);
                    Hack.KeyUp(WindowHwnd, MainWindow.keySkill);
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
                        RopeClimbing(631, true, -136, 104, 60, 60);
                    else if (targetFloor == 2)
                        RopeClimbing(765, true, -376, -136, 60, 60);
                    else if (targetFloor == 1)
                        RopeClimbing(572, true, 572, -510, 60, 60);
                }
            }
        }
    }
}
