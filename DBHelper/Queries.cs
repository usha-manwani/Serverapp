using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public class Queries
    {
        public string InsertWorkingHour(string mac, double pjhour, double comphour, double recorder, double ac, double cc, double screen)
        {
            string query = "Insert into MachineWorkingHours values ('"+mac+"','"+DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + pjhour + "','" + comphour + "','" + recorder + 
                "','" + ac + "','" + cc + "','" + screen + "')";
            return query;
        }
    }
}
