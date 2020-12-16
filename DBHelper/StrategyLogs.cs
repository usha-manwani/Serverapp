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
        public void SaveStrategyLogInfo(string instruction, int stid, int descid,string status, string machinemac)
        {
            try
            {
                using(var context = new organisationdatabaseEntities())
                {
                    var newLog = new strategylog()
                    {
                        StrategyDescId = descid,
                        StrategyId=stid,
                        MachineMac=machinemac,
                        ExecutionTime=DateTime.Now,
                        Instruction=instruction,
                        Status=status                        
                    };
                    context.strategylogs.Add(newLog);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(docPath,"exception in stratrgy logs: "+ ex.StackTrace);
            }
            
        }
    }
}
