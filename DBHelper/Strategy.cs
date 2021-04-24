// ***********************************************************************
// Assembly         : DBHelper
// Author           : admin
// Created          : 04-02-2021
//
// Last Modified By : admin
// Last Modified On : 04-22-2021
// ***********************************************************************
// <copyright file="Strategy.cs" company="Microsoft">
//     Copyright © Microsoft 2019
// </copyright>
// <summary></summary>
// ***********************************************************************
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
namespace DBHelper
{
    /// <summary>
    /// Class Strategy.
    /// </summary>
    public class Strategy
    {
        /// <summary>
        /// The constr
        /// </summary>
        protected string constr = ConfigurationManager.ConnectionStrings["OrganisationDatabase"].ConnectionString;
        /// <summary>
        /// The logger file
        /// </summary>
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Initializes a new instance of the <see cref="Strategy"/> class.
        /// </summary>
        public Strategy()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Strategy"/> class.
        /// </summary>
        /// <param name="con">The con.</param>
        public Strategy(string con)
        {
            constr = ConfigurationManager.ConnectionStrings[con].ConnectionString;
        }
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="query">Any sql query.</param>
        /// <returns>DataTable.</returns>
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
                { loggerFile.Debug(Environment.NewLine + "error in method ExecuteCmd " + ex.Message); }
                finally
                { con.Close(); con.Dispose(); }
            }
            return dt;
        }
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List&lt;StrategyDesc&gt;.</returns>
        public List<StrategyDesc> GetData(string time, string dbname)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();

            var dayOfMonth = DateTime.Now.Day;
            var dayOfWeek = (int)DateTime.Now.DayOfWeek;
            var toDate = DateTime.Now.Date;
            try
            {
                using (var context = new organisationdatabaseEntities(dbname))
                {
                    try
                    {
                        var temp = (from p in context.strategydescriptions
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
                                        StrategyId = s.strategyId
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
                                        StrategyId = x.StrategyId
                                    }).ToList();
                        if (temp.Count > 0)
                        {
                            st.AddRange(temp);
                        }
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
            }
            catch (Exception ex)
            {
                loggerFile.Debug(Environment.NewLine + "error in method GetData for strategy: " + ex.Message);
            }

            return st;
        }

        /// <summary>
        /// Gets the test time data.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List&lt;StrategyDesc&gt;.</returns>
        public List<StrategyDesc> GetTestTimeData(string time, string dbname)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();

            var dayOfMonth = DateTime.Now.Day;
            var dayOfWeek = (int)DateTime.Now.DayOfWeek;
            var toDate = DateTime.Now.Date;

            using (var context = new organisationdatabaseEntities(dbname))
            {
                try
                {
                    var temp = (from p in context.strategydescriptions
                                join e in context.strategyequipments
                                on p.Equipmentid equals e.id
                                join s in context.strategymanagements on p.StrategyRefId equals s.strategyId

                                where s.CurrentStatus != 0 && p.strategyTime == time && p.Equipmentid == 1
                                && p.StrategyTimeFrame1 == "TestTime"
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
                    if (temp.Count > 0)
                    {
                        st.AddRange(temp);
                    }
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
        /// <summary>
        /// Gets the strategy by schedule.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
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
                catch (Exception)
                { }
                finally
                { con.Close(); con.Dispose(); }
            }
            return result;
        }

        /// <summary>
        /// Gets the string by scheduleor section.
        /// </summary>
        /// <param name="timeFrameType">Type of the time frame.</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List&lt;StrategyDesc&gt;.</returns>
        public List<StrategyDesc> GetStrByScheduleorSection(string timeFrameType, string dbname)
        {
            List<StrategyDesc> st = new List<StrategyDesc>();

            using (var context = new organisationdatabaseEntities(dbname))
            {
                var temp = (from p in context.strategydescriptions
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
                                StrategyId = s.strategyId
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
                                StrategyId = x.StrategyId

                            }).ToList();
                if (temp.Count > 0)
                {
                    st.AddRange(temp);
                }
               

            }

            return st;
        }

        /// <summary>
        /// Gets the machine mac, desktop mac address from class ids list
        /// </summary>
        /// <param name="loc">list of class ids</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List&lt;LocationsMac&gt;.</returns>
        public List<LocationsMac> GetLocationsMac(List<int> loc, string dbname)
        {
            List<LocationsMac> cc = new List<LocationsMac>();

            using (var context = new organisationdatabaseEntities(dbname))
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

    /// <summary>
    /// Class LocationsMac.
    /// </summary>
    public class LocationsMac
    {
        /// <summary>
        /// Gets or sets the class identifier.
        /// </summary>
        /// <value>The class identifier.</value>
        public int ClassId { get; set; }
        /// <summary>
        /// Gets or sets the cc mac.
        /// </summary>
        /// <value>The cc mac.</value>
        public string CCMac { get; set; }
        /// <summary>
        /// Gets or sets the desk mac.
        /// </summary>
        /// <value>The desk mac.</value>
        public string DeskMac { get; set; }
    }
    /// <summary>
    /// Class StrategyDesc.
    /// </summary>
    public class StrategyDesc
    {
        /// <summary>
        /// Gets or sets the strategy desc identifier.
        /// </summary>
        /// <value>The strategy desc identifier.</value>
        public int StrategyDescId { get; set; }
        /// <summary>
        /// Gets or sets the equipment identifier.
        /// </summary>
        /// <value>The equipment identifier.</value>
        public int EquipmentId { get; set; }
        /// <summary>
        /// Gets or sets the name of the equipment.
        /// </summary>
        /// <value>The name of the equipment.</value>
        public string EquipmentName { get; set; }
        /// <summary>
        /// Gets or sets the service configuration.
        /// </summary>
        /// <value>The service configuration.</value>
        public dynamic ServiceConfig { get; set; }
        /// <summary>
        /// Gets or sets the strategy time frame1.
        /// </summary>
        /// <value>The strategy time frame1.</value>
        public string StrategyTimeFrame1 { get; set; }
        /// <summary>
        /// Gets or sets the strategy time.
        /// </summary>
        /// <value>The strategy time.</value>
        public string StrategyTime { get; set; }
        /// <summary>
        /// Gets or sets the strategy time frame2.
        /// </summary>
        /// <value>The strategy time frame2.</value>
        public string StrategyTimeFrame2 { get; set; }
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string Location { get; set; }
        /// <summary>
        /// Gets or sets the strategy identifier.
        /// </summary>
        /// <value>The strategy identifier.</value>
        public int StrategyId { get; set; }
    }
}
