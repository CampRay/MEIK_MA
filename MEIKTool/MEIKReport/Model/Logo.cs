using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEIKReport.Model
{
    public class Logo
    {
        private string device = "";        
        private string address = "";

        public string Device
        {
            get { return device; }
            set { device = value; }
        }        

        public string Address
        {
            get { return address; }
            set { address = value; }
        }
    }
}
