using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MapleRobots
{
    class Bossing : BottingBase
    {
        internal static int bossingFaceTo { set; get; }
        internal static void bossing()
        {
            int CharacterX, CharacterY, CharacterStatus;
            int originX = getCharacterX();
            int originY = getCharacterY();
            while (true)
            {
                CharacterX = getCharacterX();
                CharacterY = getCharacterY();
                CharacterStatus = getCharacterStatus();
                if (Distance(0, CharacterY, 0, originY) > 100)
                {
                    SystemSounds.Beep.Play();
                    //Thread.Sleep(200);
                    break;
                }
                AutoKey.mre_KeyPresser.Reset();
                if (Distance(CharacterX, CharacterY, originX, originY) > 50)
                    GoToX(originX, 8, false, false, 0);
                if (bossingFaceTo == 1)
                {
                    while (CharacterStatus % 2 != 1)
                    {
                        CharacterStatus = getCharacterStatus();
                        Hack.KeyDown(WindowHwnd, Keys.Left);
                        Thread.Sleep(20);
                        Hack.KeyUp(WindowHwnd, Keys.Left);
                        Thread.Sleep(100);
                    }
                }
                else if (bossingFaceTo == 2)
                {
                    while (CharacterStatus % 2 != 0)
                    {
                        CharacterStatus = getCharacterStatus();
                        Hack.KeyDown(WindowHwnd, Keys.Right);
                        Thread.Sleep(20);
                        Hack.KeyUp(WindowHwnd, Keys.Right);
                        Thread.Sleep(100);
                    }
                }
                AutoKey.mre_KeyPresser.Set();
                Thread.Sleep(1);
            }
        }
        
    }
}
