using System;
using System.Collections.Generic;
using System.Configuration;
using NLog;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public class GetMacAddress
    {
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();
      
        public KeyValuePair<string, string> GetMac(string[] mac)
        {
            KeyValuePair<string, string> result = new KeyValuePair<string, string>();
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            try
                            {
                                foreach (string m in mac)
                                {
                                    var temp = m.Replace(':', ' ').ToUpper();
                                    if (context.classdetails.Any(x => x.deskmac.ToUpper() == temp))
                                    {
                                        var d = context.classdetails.Where(x => x.deskmac.ToUpper() == temp).Select(y => y.ccmac).SingleOrDefault();
                                        found = true;
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
                    }
                    
                }
                else { break; }
            }
            return result;
        }

        public async Task<int> SaveInactiveDesktopAsync(string deskmac, string action)
        {
            var res = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            try
                            {
                                var id = context.classdetails.Where(x => x.deskmac.ToUpper() == deskmac.ToUpper())
                                                    .Select(x => x.classID).FirstOrDefault();
                                if (id != 0)
                                {
                                    found = true;
                                    temp_desktopevents tmp = new temp_desktopevents()
                                    {
                                        Action = action,
                                        ActionTime = DateTime.Now,
                                        Deskmac = deskmac,
                                        classid = id
                                    };
                                    context.temp_desktopevents.Add(tmp);
                                    res = await context.SaveChangesAsync();
                                }

                            }
                            catch (Exception ex)
                            {
                                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString()
                                    + " " + DateTime.Now.ToLongTimeString() + "exception in recording machine logs: "
                                    + ex.StackTrace + " error message " + ex.InnerException);
                            }
                        }
                    }
                }
                else { break; }
            }
            return res;
        }

        public int GetClassID(string mac)
        {
            int id = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            try
                            {
                                id = context.classdetails.Where(x => x.ccmac.ToUpper() == mac.ToUpper())
                                                    .Select(x => x.classID).FirstOrDefault();
                                if (id > 0) found = true;
                            }
                            catch (Exception ex)
                            {
                                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString()
                                    + " " + DateTime.Now.ToLongTimeString() + "exception in getting classid from machine mac : "
                                    + ex.StackTrace + " error message " + ex.InnerException);
                            }
                        }
                    }
                }
                else { break; }
            }
            return id;
        }

        public int UpdateStatCardReg(string stat, string mac, string cid)
        {
            int result = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            var classid = context.classdetails.Where(x => x.ccmac == mac).Select(x => x.classID).FirstOrDefault();
                            if (context.card_registration.Any(x => x.calssId == classid && x.OneCardId == cid))
                            {

                                var row = context.card_registration.Where(x => x.calssId == classid && x.OneCardId == cid)
                                    .Select(x => x).FirstOrDefault();

                                if (row != null)
                                {
                                    found = true;
                                    Console.WriteLine("status: " + row.Status);
                                    row.Status = stat;
                                    row.UpdateTime = DateTime.Now;
                                }
                                result = context.SaveChanges();
                            }
                            Console.WriteLine("UpdatedRows:" + result);
                        }
                    }
                }
                else { break; }

            }
            return result;
        }

        public int SaveReaderLog(string message, string mac, string cardid)
        {
            int result = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        try
                        {
                            using (var context = new organisationdatabaseEntities(c.Name))
                            {
                                var classid = context.classdetails.Where(x => x.ccmac == mac).Select(x => x.classID).FirstOrDefault();
                                if (classid != 0)
                                {
                                    found = true;
                                    cardlog cardlog = new cardlog()
                                    {
                                        Message = message,
                                        ClassId = classid,
                                        cardId = cardid,
                                        ActionTime = DateTime.Now
                                    };
                                    context.cardlogs.Add(cardlog);
                                    result = context.SaveChanges();
                                }


                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                else { break; }
            }
            return result;
        }

        public int SavePowerUsageInfo(int pow, string mac, string typ)
        {
            int r = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        try
                        {
                            using (var context = new organisationdatabaseEntities(c.Name))
                            {
                                var cid = context.classdetails.Where(x => x.ccmac == mac)
                                    .Select(x => x.classID).FirstOrDefault();
                                if (cid > 0)
                                {
                                    found = true;
                                    machineusagelogs_minute m = new machineusagelogs_minute()
                                    {
                                        classid = cid,
                                        value = pow,
                                        attribute = typ,
                                        recordtime = DateTime.Now
                                    };
                                    context.machineusagelogs_minute.Add(m);
                                    r = context.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else { break; }

            }
            return r;
        }
        public string GetPowerUsageInfo(string mac, string typ)
        {
            int r = 0;
            var data = "";
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        try
                        {
                            using (var context = new organisationdatabaseEntities(c.Name))
                            {
                                var cid = context.classdetails.Where(x => x.ccmac == mac)
                                    .Select(x => x.classID).FirstOrDefault();
                                if (cid > 0)
                                {
                                    found = true;
                                    data = context.Database.ExecuteSqlCommand("SELECT sum(value) as " +
                                        "value from organisationdatabase.machineusagelogs_minute WHERE " +
                                         "date(recordtime)= date(now())  and attribute = 'Power'" +
                                         " and classid =" + cid).ToString();

                                    r = context.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else { break; }

            }
            return data;
        }

        public int MachineCount(List<string> machinemac)
        {
            int r = 0;
            var data = "";
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        try
                        {
                            using (var context = new organisationdatabaseEntities(c.Name))
                            {
                                r = context.classdetails.Where(x => machinemac.Contains(x.ccmac)).Count();
                                if (r > 0) found = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else { break; }

            }
            return r;
        }

        public int UpdateProjectorConfig(string mac, string val)
        {
            int r = 0; string stat = "";
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    if (c.Name.Contains("Entities"))
                    {
                        if (val == "True")
                        {
                            stat = "Registered";
                        }
                        else stat = "Pending";
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            var classid = context.classdetails.Where(x => x.ccmac == mac).Select(x => x.classID).FirstOrDefault();
                            if (classid != 0)
                            {
                                found = true;
                                var d = context.projectorconfiginfoes.Where(x => x.Classid == classid).Select(x => x).FirstOrDefault();
                                if (d != null)
                                {
                                    d.status = stat;
                                }
                            }

                            r = context.SaveChanges();
                        }
                    }
                }
                else { break; }

            }
            return r;
        }
    }
}
