﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingGhostShip5 : BottingBase
    {
        static int nowFloor = 0;
        internal static void bottingGS5()
        {
            _threadOfTraining = new Thread(bottingGhostShip5Training);
            _threadOfTraining.Start();
            while (true)
            {
                int CharacterY = getCharacterY();
                if (CharacterY >= 77 && CharacterY <= 155)
                    nowFloor = 1;
                else if (CharacterY >= 317 && CharacterY <= 395)
                    nowFloor = 2;
                else
                    nowFloor = 0;
                Thread.Sleep(1);
            }
        }
        internal static void bottingGhostShip5Training()
        {
            int counter = 0;
            Thread.Sleep(500);
            //training start
            while (true)
            {
                if (counter % 10 == 0)
                {
                    //go to position start
                    RopeClimbing(1314, false, -1686, -1585, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                    Thread.Sleep(MainWindow.delayComboKey1);
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                    Thread.Sleep(MainWindow.delayComboKey2);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                }
                else if (counter % 5 == 0)
                {
                    //go to position start
                    RopeClimbing(1314, false, -1686, -1585, 60, 60);
                    //go to position end
                    AutoKey.mre_PickUp.Reset();
                    for (int i = 0; i < 25; i++)
                        Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                    Thread.Sleep(1000);
                    AutoKey.mre_PickUp.Set();
                    RopeExiting(true);
                }
                GoToX(1022);
                waitForRespawn();
                GoToX(1022);
                Attack(1);
                GoToX(383);
                Attack(1);
                GoToX(383);
                waitForRespawn();
                GoToX(383);
                Attack(1);
                GoToX(1022);
                Attack(1);

                counter++;
            }
        }
        static void waitForRespawn()
        {
            while (Hack.ReadInt(MainWindow.process, MainWindow.MobCountBaseAdr, MainWindow.MobCountOffset) < 10)
            {
                Thread.Sleep(1);
            }
        }
    }
}