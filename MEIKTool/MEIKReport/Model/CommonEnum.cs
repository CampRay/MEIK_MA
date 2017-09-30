using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEIKReport.Model
{
    public enum PageSize
    {
        Letter,
        A4
    }

	public enum TransferMode
    {
        Email,
        CloudServer
    }
	
    public enum DeviceType
    {
        Technician=1,
        Doctor=2,
        Admin=3
    }
}
