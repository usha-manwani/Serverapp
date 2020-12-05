using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.Json;
using DBHelper;
using System.Text;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    class StrategyExec
    {

        public List<FinalResult> GetData(string time)
        {
            List<FinalResult> ff = new List<FinalResult>();
            Strategy st = new Strategy();
            var inscode = "";
            
            try
            {
                var dayOfMonth = DateTime.Now.Day;
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                var toDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                var db = st.GetData(time);
                foreach (var s in db)
                {
                    var numbers = s.Location.Split(',').Select(int.Parse).ToList();
                    List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);

                    if (Convert.ToBoolean(s.ServiceConfig["isActive"].ToString()))
                    {

                        switch (s.StrategyTimeFrame1)
                        {
                            case "Monthly":
                                var day = s.StrategyTimeFrame2.Split(',').Select(int.Parse).ToList();
                                if (day.Contains(dayOfMonth))
                                {

                                    inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                    foreach (LocationsMac l in locationsmac)
                                    {
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
                                    }
                                }
                                break;
                            case "Weekly":
                                var dayno = s.StrategyTimeFrame2.Split(',').Select(int.Parse).ToList();
                                if (dayno.Contains(dayOfWeek))
                                {
                                    inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                    foreach (LocationsMac l in locationsmac)
                                    {
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
                                    }
                                }
                                break;
                            case "Daily":
                                inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                foreach (LocationsMac l in locationsmac)
                                {
                                    ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
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
                                    inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                    foreach (LocationsMac l in locationsmac)
                                    {
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
                                    }
                                }
                                
                                break;
                            case "TestTime":
                                var tempdate1 = Convert.ToDateTime(s.StrategyTimeFrame2).ToString("yyyy-MM-dd");
                                if (tempdate1 == toDate)
                                {
                                    inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                    foreach (LocationsMac l in locationsmac)
                                    {
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
                                    }
                                }

                                break;
                            case "CheckTime":
                                var tempdate2 = Convert.ToDateTime(s.StrategyTimeFrame2).ToString("yyyy-MM-dd");
                                if (tempdate2 == toDate)
                                {
                                    inscode = CheckEquipmentCode1(s.EquipmentId, s.ServiceConfig);
                                    foreach (LocationsMac l in locationsmac)
                                    {
                                        ff.Add(new FinalResult { Instruction = inscode, Ccmac = l.CCMac, Deskmac = l.DeskMac });
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
                System.Windows.Forms.MessageBox.Show(ex.Message +" Debug "+ ex.StackTrace);
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
                            instruction = ins.GetValues("SystemOn").ToString();
                        else
                            instruction = ins.GetValues("SystemOff").ToString();
                        break;
                    case 2:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ProjectorOn").ToString();
                        else
                            instruction = ins.GetValues("ProjectorOff").ToString();
                        break;
                    case 3:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("CurtainOpen").ToString();
                        else
                            instruction = ins.GetValues("CurtainClose").ToString();
                        break;
                    case 4:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ComputerOn").ToString();
                        else
                            instruction = ins.GetValues("ComputerOff").ToString();
                        break;
                    case 5:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("SystemLock").ToString();
                        else
                            instruction = ins.GetValues("SystemUnlock").ToString();
                        break;
                    case 6:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ProjectorPowerOn").ToString();
                        else
                            instruction = ins.GetValues("ProjectorPowerOff").ToString();
                        break;
                    case 7:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ComputerPowerOn").ToString();
                        else
                            instruction = ins.GetValues("ComputerPowerOff").ToString();
                        break;
                    case 8:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("AmplifierPowerOn").ToString();
                        else
                            instruction = ins.GetValues("AmplifierPowerOff").ToString();
                        break;
                    case 9:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("OtherPowerOn").ToString();
                        else
                            instruction = ins.GetValues("OtherPowerOff").ToString();
                        break;
                    case 10:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("PodiumLightOn").ToString();
                        else
                            instruction = ins.GetValues("PodiumLightOff").ToString();
                        break;
                    case 11:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ClassroomLightOn").ToString();
                        else
                            instruction = ins.GetValues("ClassroomLightOff").ToString();
                        break;
                    case 12:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("PodiumCurtainOn").ToString();
                        else
                            instruction = ins.GetValues("PodiumCurtainOff").ToString();
                        break;
                    case 13:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ClassroomCurtainOn").ToString();
                        else
                            instruction = ins.GetValues("ClassroomCurtainOff").ToString();
                        break;
                    case 14:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("ExhaustFanOn").ToString();
                        else
                            instruction = ins.GetValues("ExhaustFanOff").ToString();
                        break;
                    case 15:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("FreshAirSystemOn").ToString();
                        else
                            instruction = ins.GetValues("FreshAirSystemOff").ToString();
                        break;
                    case 16:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("Ac1On").ToString();
                        else
                            instruction = ins.GetValues("Ac1Off").ToString();
                        break;
                    case 17:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("Ac2On").ToString();
                        else
                            instruction = ins.GetValues("Ac2Off").ToString();
                        break;
                    case 18:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("Ac3On").ToString();
                        else
                            instruction = ins.GetValues("Ac3Off").ToString();
                        break;
                    case 19:
                        if (c["Stat"].ToString() == "On")
                            instruction = ins.GetValues("Ac4On").ToString();
                        else
                            instruction = ins.GetValues("Ac4Off").ToString();
                        break;
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
                        instruction = "SystemOnS";
                    else
                        instruction = "SystemOffS";
                    break;
                case 2:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ProjectorOnS";
                    else
                        instruction = "ProjectorOffS";
                    break;
                case 3:
                    if (c["Stat"].ToString() == "On")
                        instruction = "CurtainOpenS";
                    else
                        instruction = "CurtainCloseS";
                    break;
                case 4:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ComputerOnS";
                    else
                        instruction = "ComputerOffS";
                    break;
                case 5:
                    if (c["Stat"].ToString() == "On")
                        instruction = "SystemLockS";
                    else
                        instruction = "SystemUnlockS";
                    break;
                case 6:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ProjectorPowerOnS";
                    else
                        instruction = "ProjectorPowerOffS";
                    break;
                case 7:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ComputerPowerOnS";
                    else
                        instruction = "ComputerPowerOffS";
                    break;
                case 8:
                    if (c["Stat"].ToString() == "On")
                        instruction = "AmplifierPowerOnS";
                    else
                        instruction = "AmplifierPowerOffS";
                    break;
                case 9:
                    if (c["Stat"].ToString() == "On")
                        instruction = "OtherPowerOnS";
                    else
                        instruction = "OtherPowerOffS";
                    break;
                case 10:
                    if (c["Stat"].ToString() == "On")
                        instruction = "PodiumLightOnS";
                    else
                        instruction = "PodiumLightOffS";
                    break;
                case 11:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ClassroomLightOnS";
                    else
                        instruction = "ClassroomLightOffS";
                    break;
                case 12:
                    if (c["Stat"].ToString() == "On")
                        instruction = "PodiumCurtainOnS";
                    else
                        instruction = "PodiumCurtainOffS";
                    break;
                case 13:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ClassroomCurtainOnS";
                    else
                        instruction = "ClassroomCurtainOffS";
                    break;
                case 14:
                    if (c["Stat"].ToString() == "On")
                        instruction = "ExhaustFanOnS";
                    else
                        instruction = "ExhaustFanOffS";
                    break;
                case 15:
                    if (c["Stat"].ToString() == "On")
                        instruction = "FreshAirSystemOnS";
                    else
                        instruction = "FreshAirSystemOffS";
                    break;
                case 16:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac1OnS";
                    else
                        instruction = "Ac1OffS";
                    break;
                case 17:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac2OnS";
                    else
                        instruction = "Ac2OffS";
                    break;
                case 18:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac3OnS";
                    else
                        instruction ="Ac3OffS";
                    break;
                case 19:
                    if (c["Stat"].ToString() == "On")
                        instruction = "Ac4OnS";
                    else
                        instruction = "Ac4OffS";
                    break;
                default:
                    break;
            }
            return instruction;
        }
        public List<FinalResult> GetStrategyBySchedule(string time)
        {
            var ff = new List<FinalResult>();
            Strategy st = new Strategy();
            Dictionary<string, string> output = new Dictionary<string, string>();
            var ss = st.GetStrategyBySchedule(time);
            var section = Convert.ToInt32(ss["section"]);
            var dt = ss["datatable"] as DataTable;
            var sectiontime = ss["sectiontime"].ToString();
            List<LocationsMac> temp = new List<LocationsMac>();
            foreach(DataRow dr in dt.Rows)
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
                ff = GetStrategyBySection(section,sectiontime);            
                
                var data = st.GetStrByScheduleorSection("Schedule");
                if (dt.Rows.Count > 0)
                {
                    foreach (StrategyDesc dec in data)
                    {
                        if (Convert.ToBoolean(dec.ServiceConfig["isActive"].ToString()))
                        {
                            var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                            List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
                            string instruction = CheckEquipmentCode1(dec.EquipmentId, dec.ServiceConfig);
                            var pp = (from x in temp
                                      join y in locationsmac on x.ClassId equals y.ClassId
                                      select y).ToList();
                            foreach (LocationsMac l in pp)
                            {
                                if (sectiontime == "stop")
                                {
                                    if (instruction.Contains("Off"))
                                        ff.Add(new FinalResult() { Ccmac = l.CCMac, Instruction = instruction, Deskmac = l.DeskMac });
                                }
                                else if (sectiontime == "start")
                                {
                                    if (instruction.Contains("On"))
                                        ff.Add(new FinalResult() { Ccmac = l.CCMac, Instruction = instruction, Deskmac = l.DeskMac });
                                }
                            }
                        }                        
                    }
                }                
            }
            return ff;
        }

        public List<FinalResult> GetStrategyBySection(int section, string sectiontime)
        {
            List<FinalResult> ff = new List<FinalResult>();
            Strategy st = new Strategy();
            var data = st.GetStrByScheduleorSection("Section");
            if (data.Count > 0)
            {                
                foreach(StrategyDesc dec in data)
                {
                    if (Convert.ToBoolean(dec.ServiceConfig["isActive"].ToString()))
                    {
                        var numbers = dec.Location.Split(',').Select(int.Parse).ToList();
                        List<LocationsMac> locationsmac = st.GetLocationsMac(numbers);
                        string instruction = CheckEquipmentCode1(dec.EquipmentId, dec.ServiceConfig);
                        foreach (LocationsMac l in locationsmac)
                        {
                            if (sectiontime =="stop")
                            {
                                if (instruction.Contains("Off"))
                                    ff.Add(new FinalResult() { Ccmac = l.CCMac, Instruction = instruction, Deskmac = l.DeskMac });
                            }
                            else if (sectiontime == "start")
                            {
                                if (instruction.Contains("On"))
                                    ff.Add(new FinalResult() { Ccmac = l.CCMac, Instruction = instruction, Deskmac = l.DeskMac });
                            }
                        }
                    }
                }
            }
            return ff;
        }
    }
    public class FinalResult
    {
        public string Instruction { get; set; }
        public string Ccmac { get; set; }
        public string Deskmac { get; set; }
    }
}
