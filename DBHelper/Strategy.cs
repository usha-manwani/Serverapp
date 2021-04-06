using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace DBHelper
{
    public class Strategy
    {
        protected string constr = ConfigurationManager.ConnectionStrings["SchoolConnectionString"].ConnectionString;

        public DataTable ExecuteCmd(string query)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                { }
                finally
                { con.Close(); con.Dispose(); }
            }
            return dt;
        }
        public List<StrategyDesc> GetData(string time)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();

            var dayOfMonth = DateTime.Now.Day;
            var dayOfWeek = (int)DateTime.Now.DayOfWeek;
            var toDate = DateTime.Now.Date;
            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    st = (from p in context.strategydescriptions
                          join e in context.strategyequipments
                          on p.Equipmentid equals e.id
                          join s in context.strategymanagements on p.StrategyRefId equals s.strategyId
                          where s.CurrentStatus != 0 && p.strategyTime == time
                          select new
                          {
                              StrategyDescId = p.id,
                              StrategyTimeFrame1 = p.StrategyTimeFrame1,
                              StrategyTimeFrame2 = p.StrategyTimeFrame2,
                              EquipmentId = e.id,
                              EquipmentName = e.EquipmentsNames,
                              ServiceConfig = p.Config ?? "",
                              StrategyTime = p.strategyTime.ToString(),
                              Location = s.StrategyLocation,
                              StrategyId=s.strategyId
                          }).AsEnumerable().Select(x => new StrategyDesc
                          {
                              StrategyDescId = x.StrategyDescId,
                              StrategyTimeFrame1 = x.StrategyTimeFrame1,
                              StrategyTimeFrame2 = x.StrategyTimeFrame2,
                              EquipmentId = x.EquipmentId,
                              EquipmentName = x.EquipmentName,
                              ServiceConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, dynamic>>(x.ServiceConfig),
                              StrategyTime = x.StrategyTime,
                              Location = x.Location,
                              StrategyId=x.StrategyId
                          }).ToList();
                    // var data = context.strategydescriptions.Where(x => x.strategyTime == time );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    context.Dispose();
                }
            }
            return st;
        }

        public List<StrategyDesc> GetTestTimeData(string time)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();

            var dayOfMonth = DateTime.Now.Day;
            var dayOfWeek = (int)DateTime.Now.DayOfWeek;
            var toDate = DateTime.Now.Date;
            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    st = (from p in context.strategydescriptions
                          join e in context.strategyequipments
                          on p.Equipmentid equals e.id
                          join s in context.strategymanagements on p.StrategyRefId equals s.strategyId
                           
                          where s.CurrentStatus != 0 && p.strategyTime == time && p.Equipmentid==1
                          && p.StrategyTimeFrame1 =="TestTime"
                          select new
                        {
                            StrategyDescId = p.id,
                          StrategyTimeFrame1 = p.StrategyTimeFrame1,
                          StrategyTimeFrame2 = p.StrategyTimeFrame2,
                          EquipmentId = e.id,
                          EquipmentName = e.EquipmentsNames,
                          ServiceConfig = p.Config ?? "",
                          StrategyTime = p.strategyTime.ToString(),
                          Location = s.StrategyLocation,
                          StrategyId = s.strategyId
                        }).AsEnumerable().Select(x => new StrategyDesc
                          {
                              StrategyDescId = x.StrategyDescId,                              
                              StrategyTimeFrame2 = x.StrategyTimeFrame2,
                              EquipmentId = x.EquipmentId,
                              EquipmentName = x.EquipmentName,
                              ServiceConfig = JsonConvert.DeserializeObject(x.ServiceConfig),
                              StrategyTime = x.StrategyTime,
                              Location = x.Location,
                              StrategyId = x.StrategyId
                          }).ToList();
                    // var data = context.strategydescriptions.Where(x => x.strategyTime == time );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    context.Dispose();
                }
            }
            return st;
        }
        public Dictionary<string, object> GetStrategyBySchedule(string time)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            DataTable dt = new DataTable();

            int section = 0;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetStrategyScheduleTimer", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("timer", time);
                        cmd.Parameters.Add("sectionno", MySqlDbType.Int32);
                        cmd.Parameters["sectionno"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("sectiontime", MySqlDbType.VarChar);
                        cmd.Parameters["sectiontime"].Direction = ParameterDirection.Output;
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        da.Fill(dt);
                        section = Convert.ToInt32(cmd.Parameters["sectionno"].Value);
                        var sectiontime = cmd.Parameters["sectiontime"].Value;
                        result.Add("datatable", dt);
                        result.Add("section", section);
                        result.Add("sectiontime", sectiontime);
                    }

                }
                catch (Exception ex)
                { }
                finally
                { con.Close(); con.Dispose(); }
            }
            return result;
        }

        public List<StrategyDesc> GetStrByScheduleorSection(string timeFrameType)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();
           
            using (var context = new organisationdatabaseEntities())
            {
                st = (from p in context.strategydescriptions
                      join e in context.strategyequipments
                      on p.Equipmentid equals e.id
                      join s in context.strategymanagements on p.StrategyRefId equals s.strategyId
                      where s.CurrentStatus != 0 && p.StrategyTimeFrame1 == timeFrameType
                      select new
                      {
                          StrategyDescId = p.id,
                          StrategyTimeFrame1 = p.StrategyTimeFrame1,
                          StrategyTimeFrame2 = p.StrategyTimeFrame2,
                          EquipmentId = e.id,
                          EquipmentName = e.EquipmentsNames,
                          ServiceConfig = p.Config ?? "",
                          StrategyTime = p.strategyTime.ToString(),
                          Location = s.StrategyLocation,
                          StrategyId=s.strategyId
                      }).AsEnumerable().Select(x => new StrategyDesc
                      {
                          StrategyDescId = x.StrategyDescId,
                          StrategyTimeFrame1 = x.StrategyTimeFrame1,
                          StrategyTimeFrame2 = x.StrategyTimeFrame2,
                          EquipmentId = x.EquipmentId,
                          EquipmentName = x.EquipmentName,
                          ServiceConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(x.ServiceConfig),
                          StrategyTime = x.StrategyTime,
                          Location = x.Location,
                          StrategyId =x.StrategyId
                          
                      }).ToList();
                // var data = context.strategydescriptions.Where(x => x.strategyTime == time );

            }
            
            return st;
        }

        public List<LocationsMac> GetLocationsMac(List<int> loc)
        {
            List<LocationsMac> cc = new List<LocationsMac>();
            
            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    cc = (from c in context.classdetails
                          where loc.Contains(c.classID)
                          select new
                          {
                              cid = c.classID,
                              ccmac = c.ccmac,
                              deskmac = c.deskmac
                          }).AsEnumerable().Select(x => new LocationsMac
                          {
                              ClassId = x.cid,
                              CCMac = x.ccmac,
                              DeskMac = x.deskmac
                          }).ToList();

                }
                finally
                {
                    context.Dispose();
                }

            }
            return cc;
        }
    }

    public class LocationsMac
    {
        public int ClassId { get; set; }
        public string CCMac { get; set; }
        public string DeskMac { get; set; }
    }
    public class StrategyDesc
    {
        public int StrategyDescId { get; set; }
        public int EquipmentId { get; set; }
             public string EquipmentName { get; set; }
        public dynamic ServiceConfig { get; set; }
        public string StrategyTimeFrame1 { get; set; }
        public string StrategyTime { get; set; }
        public string StrategyTimeFrame2 { get; set; }
        public string Location { get; set; }
        public int StrategyId { get; set; }
    }
}
