// ***********************************************************************
// Assembly         : TcpServerListener
// Author           : admin
// Created          : 04-02-2021
//
// Last Modified By : admin
// Last Modified On : 04-22-2021
// ***********************************************************************
// <copyright file="StrategyExec.cs" company="">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using DBHelper;
using System.Text;
using NLog;
using System.Text.Json;
using System.Configuration;

namespace TcpServerListener
{
    /// <summary>
    /// Class StrategyExec.
    /// </summary>
    class StrategyExec
    {
        /// <summary>
        /// The object of NLog to write the logs in file
        /// </summary>
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This method is called by the strategy timer to get the instructions from
        /// database
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>List of StrategyDetailExecStruct</returns>
        public List<StrategyDetailExecStruct> GetData(string time)
        {
            List<StrategyDetailExecStruct> ff = new List<StrategyDetailExecStruct>();
            Strategy st = new Strategy();
            var inscode = "";
            
                var dayOfMonth = DateTime.Now.Day;
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                var toDate = DateTime.Now.ToString("yyyy-MM-dd");
                foreach (ConnectionStringSettings n in ConfigurationManager.ConnectionStrings)
                {
                try
                {
                    var dbname = n.Name;

                    if (!dbname.Contains("Entities"))
                    {
                        var dbnameEntities = dbname + "Entities";
                        var db = st.GetData(time, dbnameEntities);
                        foreach (var s in db)
                        {
                            var numbers = s.Location.Split(',').Select(int.Parse).ToList();
                            List<LocationsMac> locationsmac = st.GetLocationsMac(numbers, dbnameEntities);
                            if (s.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                            {
                                switch (s.StrategyTimeFrame1)
                                {
                                    case "Monthly":
                                        var day = s.StrategyTimeFrame2.Split(',').Select(int.Parse).ToList();
                                        if (day.Contains(dayOfMonth))
                                        {

                                            inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                            foreach (LocationsMac l in locationsmac)
                                            {
                                                ff.Add(new StrategyDetailExecStruct
                                                {
                                                    Instruction = inscode,
                                                    Ccmac = l.CCMac.ToUpper(),
                                                    Deskmac = l.DeskMac.ToUpper(),
                                                    StrategyDescId = s.StrategyDescId,
                                                    StrategyId = s.StrategyId,
                                                    Equipmentid = s.EquipmentId
                                                });
                                            }
                                        }
                                        break;
                                    case "Weekly":
                                        var dayno = s.StrategyTimeFrame2.Split(',').Select(int.Parse).ToList();
                                        if (dayno.Contains(dayOfWeek))
                                        {
                                            inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                            foreach (LocationsMac l in locationsmac)
                                            {
                                                ff.Add(new StrategyDetailExecStruct
                                                {
                                                    Instruction = inscode,
                                                    Ccmac = l.CCMac.ToUpper(),
                                                    Deskmac = l.DeskMac.ToUpper(),
                                                    StrategyDescId = s.StrategyDescId,
                                                    StrategyId = s.StrategyId,
                                                    Equipmentid = s.EquipmentId
                                                });
                                            }
                                        }
                                        break;
                                    case "Daily":
                                        inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                        foreach (LocationsMac l in locationsmac)
                                        {
                                            ff.Add(new StrategyDetailExecStruct
                                            {
                                                Instruction = inscode,
                                                Ccmac = l.CCMac.ToUpper(),
                                                Deskmac = l.DeskMac.ToUpper(),
                                                StrategyDescId = s.StrategyDescId,
                                                StrategyId = s.StrategyId,
                                                Equipmentid = s.EquipmentId
                                            });
                                        }
                                        break;
                                    //case "Schedule":
                                    //    GetStrategyBySchedule(s.EquipmentId, time, s.ServiceConfig, s.Location.Split(','));
                                    //    break;
                                    //case "Section":
                                    //    CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                    //    break;
                                    case "Date":
                                        var tempdate = Convert.ToDateTime(s.StrategyTimeFrame2).ToString("yyyy-MM-dd");
                                        if (tempdate == toDate)
                                        {
                                            inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                            foreach (LocationsMac l in locationsmac)
                                            {
                                                ff.Add(new StrategyDetailExecStruct
                                                {
                                                    Instruction = inscode,
                                                    Ccmac = l.CCMac.ToUpper(),
                                                    Deskmac = l.DeskMac.ToUpper(),
                                                    StrategyDescId = s.StrategyDescId,
                                                    StrategyId = s.StrategyId,
                                                    Equipmentid = s.EquipmentId
                                                });
                                            }
                                        }
                                        break;
                                    case "TestTime":
                                        var tempdate1 = Convert.ToDateTime(s.StrategyTimeFrame2).ToString("yyyy-MM-dd");
                                        if (tempdate1 == toDate)
                                        {
                                            inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                            foreach (LocationsMac l in locationsmac)
                                            {
                                                ff.Add(new StrategyDetailExecStruct
                                                {
                                                    Instruction = inscode,
                                                    Ccmac = l.CCMac.ToUpper(),
                                                    Deskmac = l.DeskMac.ToUpper(),
                                                    StrategyDescId = s.StrategyDescId,
                                                    StrategyId = s.StrategyId,
                                                    Equipmentid = s.EquipmentId
                                                });
                                            }
                                        }
                                        break;
                                    case "CheckTime":
                                        var tempdate2 = Convert.ToDateTime(s.StrategyTimeFrame2).ToString("yyyy-MM-dd");
                                        if (tempdate2 == toDate)
                                        {
                                            inscode = CheckEquipmentCode(s.EquipmentId, s.ServiceConfig);
                                            foreach (LocationsMac l in locationsmac)
                                            {
                                                ff.Add(new StrategyDetailExecStruct
                                                {
                                                    Instruction = inscode,
                                                    Ccmac = l.CCMac.ToUpper(),
                                                    Deskmac = l.DeskMac.ToUpper(),
                                                    StrategyDescId = s.StrategyDescId,
                                                    StrategyId = s.StrategyId,
                                                    Equipmentid = s.EquipmentId
                                                });
                                            }
                                        }
                                        break;
                                    default:
                                        Console.WriteLine("testc");
                                        break;
                                }
                            }
                        }
                        ff.AddRange(GetStrategyBySchedule(time, dbname));
                    }
                }
                catch (Exception ex)
                {
                    loggerFile.Debug(ex.Message + " Debug " + ex.StackTrace);
                }
                ///this is called because of multiple database

                ///this can be directly called for single database and change the method signature
                //ff.AddRange(GetStrategyBySchedule(time));
            }
                     
            return ff;
        }

        /// <summary>
        /// this method is use to get the Instruction Keyword from Instructions.cs file
        /// based on the configuration instructions from database
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="c">The c.</param>
        /// <returns>Instruction keyword</returns>
        public string CheckEquipmentCode(int id, Dictionary<string, object> c)
        {
            var instruction = "None";
            Instructions ins = new Instructions();
            if (c.ContainsKey("Stat"))
            {
                switch (id)
                {
                    case 1:
                        if (c["Stat"].ToString() == "On")
                            instruction = "SystemOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 2:
                        if (c["Stat"].ToString() == "On")
                            instruction = "ProjectorOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 3:
                        if (c["Stat"].ToString() == "On")
                            instruction = "ScreenOn";
                        else
                            instruction = "ScreenOff";
                        break;
                    case 4:
                        if (c["Stat"].ToString() == "On")
                            instruction = "ComputerOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 5:
                        if (c["Stat"].ToString() == "Off")
                            instruction = "PanelOff";//lock
                        else
                            instruction = "PanelOn";//unlock
                        break;
                    case 6:
                        if (c["Stat"].ToString() == "On")
                            instruction = "PortPowerOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 7:
                        if (c["Stat"].ToString() == "On")
                            instruction = "PortPowerOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 8:
                        if (c["Stat"].ToString() == "On")
                            instruction = "PortPowerOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    case 9:
                        if (c["Stat"].ToString() == "On")
                            instruction = "PortPowerOn";
                        else
                            instruction = "CloseStrategy";
                        break;
                    //case 10:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("PodiumLightOn").ToString();
                    //    else
                    //        instruction = ins.GetValues("PodiumLightOff").ToString();
                    //    break;
                    //case 11:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("ClassroomLightOn").ToString();
                    //    else
                    //        instruction = ins.GetValues("ClassroomLightOff").ToString();
                    //    break;
                    //case 12:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("PodiumCurtainOn").ToString();
                    //    else
                    //        instruction = ins.GetValues("PodiumCurtainOff").ToString();
                    //    break;
                    //case 13:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = "CurtainStrategy";
                    //    else
                    //        instruction = "CloseStrategy";
                    //    break;
                    //case 14:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("ExhaustFanOn").ToString();
                    //    else
                    //        instruction = ins.GetValues("ExhaustFanOff").ToString();
                    //    break;
                    //case 15:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("FreshAirSystemOn").ToString();
                    //    else
                    //        instruction = ins.GetValues("FreshAirSystemOff").ToString();
                    //    break;
                    //case 16:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("Ac1On").ToString();
                    //    else
                    //        instruction = ins.GetValues("Ac1Off").ToString();
                    //    break;
                    //case 17:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("Ac2On").ToString();
                    //    else
                    //        instruction = ins.GetValues("Ac2Off").ToString();
                    //    break;
                    //case 18:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("Ac3On").ToString();
                    //    else
                    //        instruction = ins.GetValues("Ac3Off").ToString();
                    //    break;
                    //case 19:
                    //    if (c["Stat"].ToString() == "On")
                    //        instruction = ins.GetValues("Ac4On").ToString();
                    //    else
                    //        instruction = ins.GetValues("Ac4Off").ToString();
                    //    break;
                    default:
                        break;
                }
            }

            return instruction;
        }

        /// <summary>
        ///this method is part of strategy timer and is called from GetData(time) method in this same class.
        /// Gets the strategy by schedule.
        /// Get the strategy details, configurations when the timeframe is selected as Schedule
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List of StrategyDetailExecStruct.</returns>
        public List<StrategyDetailExecStruct> GetStrategyBySchedule(string time, string dbname)
        {
            string dbnameEntities = dbname + "Entities";
            var ff = new List<StrategyDetailExecStruct>();
            try
            {
                
                Strategy st = new Strategy(dbname);
                Dictionary<string, string> output = new Dictionary<string, string>();
                var ss = st.GetStrategyBySchedule(time);
                var section = Convert.ToInt32(ss["section"]);
                var dt = ss["datatable"] as DataTable;
                var sectiontime = ss["sectiontime"].ToString();
                List<LocationsMac> temp = new List<LocationsMac>();
                foreach (DataRow dr in dt.Rows)
                {
                    temp.Add(new LocationsMac
                    {
                        ClassId = Convert.ToInt32(dr["classid"]),
                        CCMac = dr["ccmac"].ToString(),
                        DeskMac = dr["deskmac"].ToString()
                    });
                }
                if (section > 0)
                {
                    ff = GetStrategyBySection(section, sectiontime,dbname);

                    var data = st.GetStrByScheduleorSection("Schedule",dbnameEntities);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (StrategyDesc dec in data)
                        {
                            //dec.ServiceConfig.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
                            if (dec.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                            {
                                var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                                List<LocationsMac> locationsmac = st.GetLocationsMac(numbers,dbnameEntities);
                                string instruction = CheckEquipmentCode(dec.EquipmentId, dec.ServiceConfig);
                                var pp = (from x in temp
                                          join y in locationsmac on x.ClassId equals y.ClassId
                                          select y).ToList();
                                foreach (LocationsMac l in pp)
                                {
                                    if (sectiontime == "stop")
                                    {
                                        if (instruction.Contains("Off") || instruction.Contains("CloseStrategy"))
                                            ff.Add(new StrategyDetailExecStruct() { Ccmac = l.CCMac.ToUpper(),
                                                Instruction = instruction,
                                                Deskmac = l.DeskMac.ToUpper(),
                                                StrategyDescId = dec.StrategyDescId,
                                                StrategyId = dec.StrategyId,
                                                Equipmentid = dec.EquipmentId
                                            });
                                    }
                                    else if (sectiontime == "start")
                                    {
                                        if (instruction.Contains("On"))
                                            ff.Add(new StrategyDetailExecStruct() { Ccmac = l.CCMac.ToUpper(), Instruction = instruction,
                                                Deskmac = l.DeskMac.ToUpper(),
                                                StrategyDescId = dec.StrategyDescId,
                                                StrategyId = dec.StrategyId,
                                                Equipmentid = dec.EquipmentId
                                            });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                
                loggerFile.Debug( Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "strategy error: "+ ex.StackTrace);
                
            }
            
            return ff;
        }

        /// <summary>
        ///this method is part of strategy timer and is called from "GetStrategyBySchedule(param1,param2)" method in this same class.
        /// Gets the strategy by Section.
        /// Get the strategy details, configurations when the timeframe is selected as Section
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="sectiontime">The sectiontime.</param>
        /// <param name="dbname">The dbname.</param>
        /// <returns>List&lt;StrategyDetailExecStruct&gt;.</returns>
        public List<StrategyDetailExecStruct> GetStrategyBySection(int section, string sectiontime,string dbname)
        {
            string dbnameEntities = dbname + "Entities";
            List<StrategyDetailExecStruct> ff = new List<StrategyDetailExecStruct>();
           
            Strategy st = new Strategy();
            ///GetStrByScheduleorSection used EF connection string
            var data = st.GetStrByScheduleorSection("Section",dbnameEntities);
            if (data.Count > 0)
            {
                foreach (StrategyDesc dec in data)
                {
                    if (dec.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                    {
                        var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                        ///GetLocationsMac uses EF connection string
                        List<LocationsMac> locationsmac = st.GetLocationsMac(numbers,dbnameEntities);
                        string instruction = CheckEquipmentCode(dec.EquipmentId, dec.ServiceConfig);
                        foreach (LocationsMac l in locationsmac)
                        {
                            if (sectiontime == "stop")
                            {
                                if(instruction.Contains("Off")|| instruction.Contains("CloseStrategy"))
                                    ff.Add(new StrategyDetailExecStruct() {
                                        Ccmac = l.CCMac.ToUpper(),
                                        Instruction = instruction,
                                        Deskmac = l.DeskMac.ToUpper(),
                                        StrategyDescId = dec.StrategyDescId,
                                        StrategyId = dec.StrategyId,
                                        Equipmentid = dec.EquipmentId
                                    });
                            }
                            else if (sectiontime == "start")
                            {
                                if (instruction.Contains("On"))
                                    ff.Add(new StrategyDetailExecStruct() {
                                        Ccmac = l.CCMac.ToUpper(),
                                        Instruction = instruction,
                                        Deskmac = l.DeskMac.ToUpper(),
                                        StrategyDescId = dec.StrategyDescId,
                                        StrategyId = dec.StrategyId,
                                        Equipmentid= dec.EquipmentId
                                    });
                            }
                        }
                    }
                }
            }
            return ff;
        }
        /// <summary>
        /// This method is called from AsyncDesktopServer.cs to find the strategy that has a time frame for
        /// test so that a desktop client can be informed 5 minutes prior to test
        /// </summary>
        /// <returns>List&lt;TestTimes&gt;.</returns>
        public List<TestTimes> GetTestTimeData()
        {
            List<int> tempid = new List<int>();
            List<TestTimes> result = new List<TestTimes>();
            var t= DateTime.Now.AddMinutes(5).ToString("HH:mm:00");
            var starttime = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:00");
            foreach (ConnectionStringSettings n in ConfigurationManager.ConnectionStrings)
            {
                Strategy st = new Strategy();
                var dbname = n.Name;
                if (dbname.Contains("Entities"))
                {
                    var a = st.GetTestTimeData(t, dbname);
                    foreach (var s in a)
                    {
                        var numbers = s.Location.Split(',').Select(int.Parse).ToList();
                        List<LocationsMac> locationsmac = st.GetLocationsMac(numbers,dbname);
                        if (s.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                        {
                            if (!tempid.Contains(s.StrategyId))
                            {
                                if (s.ServiceConfig.ContainsKey("starttime"))
                                {
                                    tempid.Add(s.StrategyId);
                                    if (s.ServiceConfig["starttime"].ToString() == starttime)
                                    {
                                        foreach (var l in locationsmac)
                                        {
                                            var testtime = new TestTimes()
                                            {
                                                EndTime = s.ServiceConfig["endtime"].ToString(),
                                                StartTime = s.ServiceConfig["starttime"].ToString(),
                                                PublishText = s.ServiceConfig["publishText"].ToObject<int[]>(),
                                                PublishTitle = s.ServiceConfig["publishTitle"].ToObject<int[]>(),
                                                CCmac = l.CCMac,
                                                Deskmac = l.DeskMac,
                                                Subject = s.ServiceConfig["subject"].ToString(),
                                                Code = "TestStart"
                                            };
                                            result.Add(testtime);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

      
    }

    /// <summary>
    /// Class StrategyDetailExecStruct.
    /// </summary>
    public class StrategyDetailExecStruct
    {
        /// <summary>
        /// Gets or sets the instruction.
        /// </summary>
        /// <value>The instruction.</value>
        public string Instruction { get; set; }
        /// <summary>
        /// Gets or sets the ccmac.
        /// </summary>
        /// <value>The ccmac.</value>
        public string Ccmac { get; set; }
        /// <summary>
        /// Gets or sets the deskmac.
        /// </summary>
        /// <value>The deskmac.</value>
        public string Deskmac { get; set; }
        /// <summary>
        /// Gets or sets the strategy identifier.
        /// </summary>
        /// <value>The strategy identifier.</value>
        public int StrategyId { get; set; }
        /// <summary>
        /// Gets or sets the strategy desc identifier.
        /// </summary>
        /// <value>The strategy desc identifier.</value>
        public int StrategyDescId { get; set; }
        /// <summary>
        /// Gets or sets the equipmentid.
        /// </summary>
        /// <value>The equipmentid.</value>
        public int Equipmentid { get; set; }
    }

    /// <summary>
    /// Class TestTimes.
    /// </summary>
    public class TestTimes
    {
        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        public string EndTime { get; set; }
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public string StartTime { get; set; }
        /// <summary>
        /// Gets or sets the publish title.
        /// </summary>
        /// <value>The publish title.</value>
        public int[] PublishTitle { get; set; }
        /// <summary>
        /// Gets or sets the c cmac.
        /// </summary>
        /// <value>The c cmac.</value>
        public string CCmac { get; set; }
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }
        /// <summary>
        /// Gets or sets the publish text.
        /// </summary>
        /// <value>The publish text.</value>
        public int[] PublishText { get; set; }
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; set; }
        /// <summary>
        /// Gets or sets the deskmac.
        /// </summary>
        /// <value>The deskmac.</value>
        public string Deskmac { get; set; }
    }
}
