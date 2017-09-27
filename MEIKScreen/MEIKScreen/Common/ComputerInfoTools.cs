using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MEIKScreen.Common
{
    public class ComputerInfoTools
    {
        /// <summary>
        /// 取得设备网卡的MAC地址
        /// </summary>
        public static string GetNetCardMacAddress()
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            string str = "";
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                    str = mo["MacAddress"].ToString();
            }
            return str;
        }

        /// <summary>
        /// 取得CPU序列ID
        /// </summary>
        public static string GetCPUId()
        {
            ManagementClass mc = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mc.GetInstances();
            string strID = null;
            foreach (ManagementObject mo in moc)
            {
                strID = mo.Properties["ProcessorId"].Value.ToString();
                break;
            }
            return strID;
        }        

    }
}
