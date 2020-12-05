using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StrategyExecution
{
    class StrategyExec
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
                { con.Close(); }
            }
            return dt;
        }
        public void GetData(string time)
        {
            try
            {
                var dayOfMonth = DateTime.Now.Day;
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                var toDate = DateTime.Now.Date;
                using (var context = new organisationdatabaseEntities())
                {
                    var db = (from p in context.strategydescriptions
                              join e in context.strategyequipments
                              on p.Equipmentid equals e.id
                              join s in context.strategymanagements on p.StrategyRefId equals s.strategyId
                              where s.CurrentStatus != 0 && p.strategyTime == time
                              select new
                              {
                                  StrategyDescId = p.id,
                                  st1 = p.StrategyTimeFrame1,
                                  st2 = p.StrategyTimeFrame2,
                                  EquipmentId = e.id,
                                  EquipmentName = e.EquipmentsNames,
                                  ServiceConfig = p.Config ?? "",
                                  StrategyTime = p.strategyTime.ToString(),
                                  location = s.StrategyLocation
                              }).AsEnumerable().ToList();
                    // var data = context.strategydescriptions.Where(x => x.strategyTime == time );
                    foreach (var s in db)
                    {
                        switch (s.st1)
                        {
                            case "Monthly":
                                if (s.st2 == dayOfMonth.ToString())
                                {
                                    CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                }
                                break;
                            case "Weekly":
                                if (s.st2 == dayOfWeek.ToString())
                                {
                                    CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                }
                                break;
                            case "Daily":
                                CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                break;
                            case "Schedule":
                                GetStrategyBySchedule(s.EquipmentId, time, s.ServiceConfig, s.location.Split(','));
                                break;
                            case "Section":
                                CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                break;
                            case "Date":
                                CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                break;
                            default:
                                Console.WriteLine("testc");
                                break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string CheckEquipmentCode(int id, string config)
        {
            var instruction = "";
            Instructions ins = new Instructions();
            var c = JsonSerializer.Deserialize<Dictionary<string,object>>(config);
            switch (id)
            {
                case 1:
                    if (c["stat"].ToString() == "On")
                        instruction= ins.GetValues("SystemOn").ToString();
                    else
                        instruction = ins.GetValues("SystemOff").ToString();
                     break;
                case 2:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ProjectorOn").ToString();
                    else
                        instruction = ins.GetValues("ProjectorOff").ToString();
                    break;
                case 3:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("CurtainOpen").ToString();
                    else
                        instruction = ins.GetValues("CurtainClose").ToString();
                    break;
                case 4:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ComputerOn").ToString();
                    else
                        instruction = ins.GetValues("ComputerOff").ToString();
                    break;
                case 5:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("SystemLock").ToString();
                    else
                        instruction = ins.GetValues("SystemUnlock").ToString();
                    break;
                case 6:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ProjectorPowerOn").ToString();
                    else
                        instruction = ins.GetValues("ProjectorPowerOff").ToString();
                    break;
                case 7:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ComputerPowerOn").ToString();
                    else
                        instruction = ins.GetValues("ComputerPowerOff").ToString();
                    break;
                case 8:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("AmplifierPowerOn").ToString();
                    else
                        instruction = ins.GetValues("AmplifierPowerOff").ToString();
                    break;
                case 9:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("OtherPowerOn").ToString();
                    else
                        instruction = ins.GetValues("OtherPowerOff").ToString();
                    break;
                case 10:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("PodiumLightOn").ToString();
                    else
                        instruction = ins.GetValues("PodiumLightOff").ToString();
                    break;
                case 11:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ClassroomLightOn").ToString();
                    else
                        instruction = ins.GetValues("ClassroomLightOff").ToString();
                    break;
                case 12:                    
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("PodiumCurtainOn").ToString();
                    else
                        instruction = ins.GetValues("PodiumCurtainOff").ToString();
                    break;
                case 13:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ClassroomCurtainOn").ToString();
                    else
                        instruction = ins.GetValues("ClassroomCurtainOff").ToString();
                    break;
                case 14:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("ExhaustFanOn").ToString();
                    else
                        instruction = ins.GetValues("ExhaustFanOff").ToString();
                    break;
                case 15:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("FreshAirSystemOn").ToString();
                    else
                        instruction = ins.GetValues("FreshAirSystemOff").ToString();
                    break;
                case 16:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("Ac1On").ToString();
                    else
                        instruction = ins.GetValues("Ac1Off").ToString();
                    break;
                case 17:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("Ac2On").ToString();
                    else
                        instruction = ins.GetValues("Ac2Off").ToString();
                    break;
                case 18:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("Ac3On").ToString();
                    else
                        instruction = ins.GetValues("Ac3Off").ToString();
                    break;
                case 19:
                    if (c["stat"].ToString() == "On")
                        instruction = ins.GetValues("Ac4On").ToString();
                    else
                        instruction = ins.GetValues("Ac4Off").ToString();
                    break;
                default:
                    break;
            }
            return instruction;
        }
        public void GetAcInstruction()
        {

        }
        public void GetStrategyBySchedule(int equipmentid, string time, string config, string[] locations)
        {
            var loc = locations.ToList();
            DataTable dt = new DataTable();
            Dictionary<string, string> output = new Dictionary<string, string>();
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
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        da.Fill(dt);
                        section =  cmd.Parameters["sectionno"].Value == null ? 0 : Convert.ToInt32( cmd.Parameters["sectionno"].Value);
                    }
                    if(section != 0)
                    {
                        foreach(DataRow dr in dt.Rows)
                        {
                            if (loc.Contains(dr["classid"].ToString()))
                            {
                                output.Add(dr["ccmac"].ToString(), dr["deskmac"].ToString());
                            }
                        }   
                        if(section % 2 == 0)
                        {
                            //Off Instructions
                            
                        }
                        if(section%2 == 1)
                        {
                            //On Instructions
                            
                        }
                    }
                }
                catch (Exception ex)
                { }
                finally
                { con.Close(); }
            }
        }

    }
}
