using System;
using System.Configuration;
using NLog;
using System.Linq;
using System.Threading.Tasks;


namespace DBHelper
{
    public class StrategyLogs
    {
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();
        
        public async Task SaveStrategyLogInfo(string instruction, int stid, string status, string machinemac, int equipid)
        {
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    try 
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {                          
                            var classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                            if (classid != 0)
                            {
                                found = true;
                                var newLog = new strategylog()
                                {
                                    StrategyDescId = stid,
                                    MachineMac = classid,
                                    ExecutionTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm")),
                                    Instruction = instruction,
                                    Status = status,
                                    EquipmentId = equipid
                                };
                                context.strategylogs.Add(newLog);
                                await context.SaveChangesAsync();
                            }

                        }

                    }

                    catch (Exception ex)
                    {
                        loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString()
                            + " " + DateTime.Now.ToLongTimeString() + "exception in stratrgy logs: "
                            + ex.StackTrace + " error message " + ex.InnerException);

                    }

                }
                else { break; }
            }
        }
        public async Task<int> UpdateStrategyStatus(string instruction, string machinemac, int stid, string status)
        {
            int r = 0;
            var classid = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    try
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                            if (context.strategylogs.Any(x => x.Instruction == instruction && x.MachineMac == classid
                            && x.StrategyDescId == stid))
                            {
                                found = true;
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
                        loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() +
                            " " + DateTime.Now.ToLongTimeString() + "exception in stratrgy logs: " +
                            ex.InnerException + " error message " + ex.Message + " data: " +
                            instruction + " " + machinemac + " " + stid + " " + status);
                    }
                }
                else { break; }
            }
            return r;
        }
        public async Task SaveMachineLogs(string type, string data, string machinemac)
        {
            int r = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    try
                    {
                        using (var context = new organisationdatabaseEntities(c.Name))
                        {
                            var classid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                            if (classid != 0)
                            {
                                found = true;
                                var machineop = new machineoperationlog()
                                {
                                    Operation = data,      //JsonSerializer.Serialize(data),
                                    Location = classid,
                                    Type = type,
                                    ExecutionTime = DateTime.Now
                                };
                                context.machineoperationlogs.Add(machineop);
                            }
                            r = await context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString()
                            + " " + DateTime.Now.ToLongTimeString() + "exception in recording machine logs: "
                            + ex.StackTrace + " error message " + ex.InnerException);
                    }
                }
                else { break; }
            }
        }

        public async Task<int> UpdateMachineStatus(string machinemac, string status)
        {
            int r = 0;
            var found = false;
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!found)
                {
                    using (var context = new organisationdatabaseEntities(c.Name))
                    {
                        var cid = context.classdetails.Where(x => x.ccmac == machinemac).Select(x => x.classID).FirstOrDefault();
                        if (cid != 0)
                        {
                            found = true;
                            if (context.temp_machinestatus.Any(x => x.classid == cid))
                            {
                                var row = context.temp_machinestatus.Where(x => x.classid == cid).FirstOrDefault();
                                row.machineStatus = status;
                            }
                            else
                            {
                                var newrow = new temp_machinestatus { classid = cid, machineStatus = status };
                                context.temp_machinestatus.Add(newrow);
                            }
                            r = await context.SaveChangesAsync();
                        }
                        
                    }
                }
                else { break; }
            }
            return r;
        }
    }

}
