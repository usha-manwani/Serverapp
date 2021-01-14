using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using DBHelper;
using System.Text;
using System.IO;
using System.Text.Json;

namespace TcpServerListener
{
    class StrategyExec
    {
        static string docPath = "logConsoleServerApp.txt";
        public List<FinalResult> GetData(string time)
        {
            List<FinalResult> ff = new List<FinalResult>();
            Strategy st = new Strategy();
            var inscode = "";            
            try
            {
                var dayOfMonth = DateTime.Now.Day;
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                var toDate = DateTime.Now.ToString("yyyy-MM-dd");
                var db = st.GetData(time);
                foreach (var s in db)
                {
                    var numbers = s.Location.Split(',').Select(int.Parse).ToList();
                    List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
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
                                        ff.Add(new FinalResult { Instruction = inscode,
                                            Ccmac = l.CCMac.ToUpper(),
                                            Deskmac = l.DeskMac.ToUpper(),
                                            StrategyDescId =s.StrategyDescId,StrategyId=s.StrategyId,
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
                                        ff.Add(new FinalResult { Instruction = inscode,
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
                                    ff.Add(new FinalResult { Instruction = inscode,
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
                                        ff.Add(new FinalResult { Instruction = inscode,
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
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac.ToUpper(), Deskmac = l.DeskMac.ToUpper(),
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
                                        ff.Add(new FinalResult { Instruction = inscode,
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
                ff.AddRange(GetStrategyBySchedule(time));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Debug " + ex.StackTrace);
            }            
            return ff;
        }

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
        public string CheckEquipmentCode1(int id, Dictionary<string, object> c)
        {
            var instruction = "";


            switch (id)
            {
                case 1:
                    if (c["Stat"].ToString() == "On")
                        instruction = "SystemOnStrategy";
                    else
                        instruction = "SystemOffStrategy";
                    break;
                case 2:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ProjectorOnStrategy";
                    else
                        instruction = "ProjectorOffStrategy";
                    break;
                case 3:
                    if (c["Stat"].ToString() == "On")
                        instruction = "CurtainOpenStrategy";
                    else
                        instruction = "CurtainCloseStrategy";
                    break;
                case 4:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ComputerOnStrategy";
                    else
                        instruction = "ComputerOffStrategy";
                    break;
                case 5:
                    if (c["Stat"].ToString() == "On")
                        instruction = "SystemLockStrategy";
                    else
                        instruction = "SystemUnlockStrategy";
                    break;
                case 6:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ProjectorPowerOnStrategy";
                    else
                        instruction = "ProjectorPowerOffStrategy";
                    break;
                case 7:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ComputerPowerOnStrategy";
                    else
                        instruction = "ComputerPowerOffStrategy";
                    break;
                case 8:
                    if (c["Stat"].ToString() == "On")
                        instruction = "AmplifierPowerOnStrategy";
                    else
                        instruction = "AmplifierPowerOffStrategy";
                    break;
                case 9:
                    if (c["Stat"].ToString() == "On")
                        instruction = "OtherPowerOnStrategy";
                    else
                        instruction = "OtherPowerOffStrategy";
                    break;
                case 10:
                    if (c["Stat"].ToString() == "On")
                        instruction = "PodiumLightOnStrategy";
                    else
                        instruction = "PodiumLightOffStrategy";
                    break;
                case 11:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ClassroomLightOnStrategy";
                    else
                        instruction = "ClassroomLightOffStrategy";
                    break;
                case 12:
                    if (c["Stat"].ToString() == "On")
                        instruction = "PodiumCurtainOnStrategy";
                    else
                        instruction = "PodiumCurtainOffStrategy";
                    break;
                case 13:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ClassroomCurtainOnStrategy";
                    else
                        instruction = "ClassroomCurtainOffStrategy";
                    break;
                case 14:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ExhaustFanOnStrategy";
                    else
                        instruction = "ExhaustFanOffStrategy";
                    break;
                case 15:
                    if (c["Stat"].ToString() == "On")
                        instruction = "FreshAirSystemOnStrategy";
                    else
                        instruction = "FreshAirSystemOffStrategy";
                    break;
                case 16:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac1OnStrategy";
                    else
                        instruction = "Ac1OffStrategy";
                    break;
                case 17:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac2OnStrategy";
                    else
                        instruction = "Ac2OffStrategy";
                    break;
                case 18:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac3OnStrategy";
                    else
                        instruction = "Ac3OffStrategy";
                    break;
                case 19:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac4OnStrategy";
                    else
                        instruction = "Ac4OffStrategy";
                    break;
                default:
                    break;
            }
            return instruction;
        }
        public List<FinalResult> GetStrategyBySchedule(string time)
        {
           
            var ff = new List<FinalResult>();
            try
            {
                Strategy st = new Strategy();
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
                    ff = GetStrategyBySection(section, sectiontime);

                    var data = st.GetStrByScheduleorSection("Schedule");
                    if (dt.Rows.Count > 0)
                    {
                        foreach (StrategyDesc dec in data)
                        {
                            //dec.ServiceConfig.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
                            if (dec.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                            {
                                var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                                List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
                                string instruction = CheckEquipmentCode(dec.EquipmentId, dec.ServiceConfig);
                                var pp = (from x in temp
                                          join y in locationsmac on x.ClassId equals y.ClassId
                                          select y).ToList();
                                foreach (LocationsMac l in pp)
                                {
                                    if (sectiontime == "stop")
                                    {
                                        if (instruction.Contains("Off") || instruction.Contains("CloseStrategy"))
                                            ff.Add(new FinalResult() { Ccmac = l.CCMac.ToUpper(),
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
                                            ff.Add(new FinalResult() { Ccmac = l.CCMac.ToUpper(), Instruction = instruction,
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
                Console.WriteLine("strategy error: "+ex.StackTrace);
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "strategy error: "+ ex.StackTrace);
                // Log("error in strategy exex : " + ex.StackTrace);
            }
            
            return ff;
        }
        private  void Log(string logMessage)
        {
            StringBuilder sb = new StringBuilder(DateTime.Now.ToLongTimeString());
            sb.AppendLine("  " + DateTime.Now.ToLongDateString());
            sb.AppendLine(logMessage);
            sb.AppendLine("---------------------------------------------");
            string docPath = "logConsoleServerApp.txt";
            try
            {
                using (var oStreamWriter = new StreamWriter(docPath, true))
                {
                    oStreamWriter.WriteLine(sb);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public List<FinalResult> GetStrategyBySection(int section, string sectiontime)
        {
            List<FinalResult> ff = new List<FinalResult>();
            Strategy st = new Strategy();
            var data = st.GetStrByScheduleorSection("Section");
            if (data.Count > 0)
            {
                foreach (StrategyDesc dec in data)
                {
                    if (dec.ServiceConfig["isActive"].ToString().ToUpper() == "TRUE")
                    {
                        var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                        List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
                        string instruction = CheckEquipmentCode(dec.EquipmentId, dec.ServiceConfig);
                        foreach (LocationsMac l in locationsmac)
                        {
                            if (sectiontime == "stop")
                            {
                                if(instruction.Contains("Off")|| instruction.Contains("CloseStrategy"))
                                    ff.Add(new FinalResult() {
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
                                    ff.Add(new FinalResult() {
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
        public List<TestTimes> GetTestTimeData()
        {
            List<int> tempid = new List<int>();
            List<TestTimes> result = new List<TestTimes>();
            var t= DateTime.Now.AddMinutes(5).ToString("HH:mm:00");
            var starttime = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:00");
            Strategy st = new Strategy();
            var a = st.GetTestTimeData(t);
            foreach (var s in a)
            {
                var numbers = s.Location.Split(',').Select(int.Parse).ToList();
                List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
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
                                        PublishText = s.ServiceConfig["publishText"].ToString(),
                                        PublishTitle = s.ServiceConfig["publishTitle"].ToString(),
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
            return result;
        }
    }
    public class FinalResult
    {
        public string Instruction { get; set; }
        public string Ccmac { get; set; }
        public string Deskmac { get; set; }
        public int StrategyId { get; set; }
        public int StrategyDescId { get; set; }
        public int Equipmentid { get; set; }
    }

    public class TestTimes
    {
        public string EndTime { get; set; }
        public string StartTime { get; set; }
        public string PublishTitle { get; set; }
        public string CCmac { get; set; }
        public string Code { get; set; }
        public string PublishText { get; set; }        
        public string Subject { get; set; }
        public string Deskmac { get; set; }
    }
}
