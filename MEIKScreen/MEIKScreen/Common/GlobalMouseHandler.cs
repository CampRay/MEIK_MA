using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEIKScreen.Common
{
    public class GlobalMouseHandler:IMessageFilter
    {
        private const int WM_LBUTTONDOWN = 0x201;
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                // Do stuffs
                return true;
            }
            return false;
        }
    }
}
