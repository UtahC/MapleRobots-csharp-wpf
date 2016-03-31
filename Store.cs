using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapleRobots
{
    class Store
    {
        internal static void selling()
        {
            int clientX = 544, clientY = 434;
            int screenX, screenY, nowX, nowY, sureSellScreenX, sureSellScreenY;
            Hack.ClientToScreen(MainWindow.WindowHwnd, clientX, clientY, out screenX, out screenY);
            nowX = screenX;
            nowY = screenY;
            Hack.MoveTo(screenX, screenY);
            Hack.ClientToScreen(MainWindow.WindowHwnd, 297, 271, out sureSellScreenX, out sureSellScreenY);
            while (nowX == screenX && nowY == screenY)
            {
                Hack.GetMousePosition(out nowX, out nowY);
                Hack.LeftDoubleClick();
                if (Hack.CompareColor(IntPtr.Zero, sureSellScreenX, sureSellScreenY, "4488BB", "000000"))
                    Hack.KeyPress(MainWindow.WindowHwnd, Keys.Enter);
            }
        }
    }
}
