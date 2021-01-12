using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace DBHelper
{
    public class StrategyLogs
    {
        static string docPath = "logConsoleServerApp.txt";
        protected string constr = ConfigurationManager.ConnectionStrings["SchoolConnectionString"].ConnectionString;
        public async Task SaveStrategyLogInfo(string instruction, int stid, string status, string machinemac,int equipid)
        {
            try
            {
                using (var context = new organisationdatabaseEntities())
                {
                    var classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                    var newLog = new strategylog()
                    {
                        StrategyDescId = stid,
                        MachineMac = classid,
                        ExecutionTime =Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm")),
                        Instruction = instruction,
                        Status = status,
                        EquipmentId=equipid
                    };
                    context.strategylogs.Add(newLog);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString()
                    + " " + DateTime.Now.ToLongTimeString() + "exception in stratrgy logs: "
                    + ex.StackTrace + " error message " + ex.InnerException);

            }

        }
        public async Task<int> UpdateStrategyStatus(string instruction, string machinemac, int stid, string status)
        {
            int r = 0;
            try
            {
                using (var context = new organisationdatabaseEntities())
                {
                    var classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                    if (context.strategylogs.Any(x => x.Instruction == instruction && x.MachineMac == classid
                    && x.StrategyDescId == stid))
                    {
                        var log = context.strategylogs.Where(x => x.Instruction == instruction && x.MachineMac == classid && x.StrategyDescId == stid);
                        foreach (var l in log)
                        {
                            if (DateTime.Now.Subtract(l.ExecutionTime).TotalSeconds < 180)
                            {
                                l.Status = status;
                                break;
                            }
                        }
                    }
                    r = await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() +
                    " " + DateTime.Now.ToLongTimeString() + "exception in stratrgy logs: " +
                    ex.InnerException + " error message " + ex.Message);
            }
            return r;
        }
        public async Task SaveMachineLogs(string type,string data, string machinemac)
        {            
            int r = 0;
            try
            {
                using (var context = new organisationdatabaseEntities())
                {
                    var classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                    if (classid != 0)
                    {
                        var machineop = new machineoperationlog()
                        {
                            Operation = data,                  //JsonSerializer.Serialize(data),
                            Location = classid,
                            Type=type,
                            ExecutionTime = DateTime.Now
                        };
                        context.machineoperationlogs.Add(machineop);
                    }
                    r = await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString()
                    + " " + DateTime.Now.ToLongTimeString() + "exception in recording machine logs: "
                    + ex.StackTrace + " error message " + ex.InnerException);
            }
        }
    }
     
}
