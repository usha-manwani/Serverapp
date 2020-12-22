using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBHelper
{
    public class StrategyLogs
    {
        static string docPath = "logConsoleServerApp.txt";
        protected string constr = ConfigurationManager.ConnectionStrings["SchoolConnectionString"].ConnectionString;
        public async Task SaveStrategyLogInfo(string instruction, int descid,string status, string machinemac)
        {            
            try
            {
                using(var context = new organisationdatabaseEntities())
                {
                    var newLog = new strategylog()
                    {
                        StrategyDescId = descid,
                        
                        MachineMac=machinemac,
                        ExecutionTime=DateTime.Now,
                        Instruction=instruction,
                        Status=status                        
                    };
                   context.strategylogs.Add(newLog);
                   await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(docPath,"exception in stratrgy logs: "+ ex.StackTrace);
            }
            
        }
        public async Task<int> UpdateStrategyStatus(string instruction,string machinemac,int descid,string status)
        {
            int r = 0;
            try
            {
                using (var context = new organisationdatabaseEntities())
                {
                    if(context.strategylogs.Any(x=>x.Instruction==instruction && x.MachineMac==machinemac 
                    && x.StrategyDescId == descid))
                    {
                        var log = context.strategylogs.Where(x => x.Instruction == instruction && x.MachineMac == machinemac && x.StrategyDescId == descid);
                        foreach(var l in log)
                        {
                            if (DateTime.Now.Subtract(l.ExecutionTime).TotalSeconds < 20)
                            {
                                l.Status = status;
                                break;
                            }
                        }
                    }
                  r= await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, "exception in stratrgy logs: " + ex.StackTrace);
            }
            return r;
        }
        public async Task SaveMachineLogs()
        {

        }
    }
}
