using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public class GetMacAddress
    {
        public KeyValuePair<string, string> GetMac(string[] mac)
        {
            KeyValuePair<string, string> result = new KeyValuePair<string, string>();

            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    foreach (string m in mac)
                    {
                        var temp = m.Replace(':', ' ').ToUpper();
                        if (context.classdetails.Any(x => x.deskmac.ToUpper() == temp))
                        {
                            var d = context.classdetails.Where(x => x.deskmac.ToUpper() == temp).Select(y => y.ccmac).SingleOrDefault();

                            result = new KeyValuePair<string, string>(temp, d.ToUpper());
                            break;
                        }
                    }
                }
                finally
                {
                    context.Dispose();
                }
            }



            return result;
        }
       
    }
}
