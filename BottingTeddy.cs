using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MapleRobots
{
    class BottingTeddy : BottingBase
    {
        internal static void botting()//發條熊
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
                    AutoKey.mre_PickUp.Reset();
                    if (counter % 10 == 0)
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo1);
                        Thread.Sleep(MainWindow.delayComboKey1);
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keyCombo2);
                        Thread.Sleep(MainWindow.delayComboKey2);
                    }
                    else
                    {
                        for (int i = 0; i < 25; i++)
                            Hack.KeyPress(WindowHwnd, MainWindow.keySkill);
                        Thread.Sleep(1000);
                    }
                    AutoKey.mre_PickUp.Set();
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
    }
}
