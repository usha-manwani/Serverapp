using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;

using MySql.Data.MySqlClient;

using System.Configuration;

namespace TcpServerListener
{
    class Decode
    {
        //public static readonly string constr = ConfigurationManager.ConnectionStrings["CresijCamConnectionString"].ConnectionString;
        //public static readonly string constr = "Integrated Security=SSPI;Persist Security Info=False;" +
        //    "Data Source=WFJ-20190418TVO\\SQLEXPRESS;Initial Catalog=CresijCam";

        List<int> time = new List<int>();
#pragma warning disable CS0414 // The field 'Decode.projectorStatus' is assigned but its value is never used
        string projectorStatus = "Closed";
#pragma warning restore CS0414 // The field 'Decode.projectorStatus' is assigned but its value is never used
#pragma warning disable CS0414 // The field 'Decode.computer' is assigned but its value is never used
        static string computer = "Offline";
#pragma warning restore CS0414 // The field 'Decode.computer' is assigned but its value is never used
        // static string[] Status = new string[7];

        Dictionary<byte, string> keyCodes = new Dictionary<byte, string>();
        public Dictionary<string, object> Decoded(string ip, byte[] received, string[] Status)
        {
            Dictionary<string, object> statdata = new Dictionary<string, object>();
            Dictionary<string, string> objdata = new Dictionary<string, string>();
            Dictionary<string, object> result = new Dictionary<string, object>();
            byte[] data = null;
            string[] MessageArray = null;
            string log = "";
            int length = 0;
            try
            {
                if (received[0] == Convert.ToByte(0x8B) && received[1] == Convert.ToByte(0xB9))
                {
                    //Status[0] = ip;
                    //for (int i = 1; i < 7; i++)
                    //{
                    //    Status[i] = "Off";
                    //}

                    length = 4 + (256 * received[2]) + received[3];
                    data = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = received[i];
                    }

                    MessageArray = new string[data.Length - 1];
                    for (int i = 0; i < MessageArray.Length; i++)
                    {
                        MessageArray[i] = "--";
                    }

                    if (data[4] == Convert.ToByte(0x01))
                    {
                        statdata.Add("Command", "Reader");

                        MessageArray[0] = "Reader";
                        if (data[6] == Convert.ToByte(0xc4))
                        {
                            statdata.Add("InstructionStatus", "Success");
                            switch (data[5])
                            {
                                case 1:
                                    statdata.Add("Type", "Registered");

                                    MessageArray[1] = "registered";
                                    MessageArray[2] = data[7].ToString();
                                    byte[] cardbytes = new byte[4];
                                    for (int i = 7; i <= 10; i++)
                                    {
                                        cardbytes[i - 7] = data[i];
                                        //MessageArray[2] = MessageArray[2] +" "+ data[i];
                                    }
                                    objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes));

                                    MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes);

                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
                                    statdata.Add("Type", "ToRegister");

                                    MessageArray[1] = "Toregister";
                                    MessageArray[2] = data[7].ToString();
                                    byte[] cardbytes1 = new byte[4];
                                    for (int i = 7; i <= 10; i++)
                                    {
                                        cardbytes1[i - 7] = data[i];
                                        //MessageArray[2] = MessageArray[2] +" "+ data[i];
                                    }
                                    MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes1);
                                    objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes1));

                                    break;
                                case 5:
                                    break;
                                case 6:
                                    break;
                                case 7:
                                    break;
                                case 8:
                                    break;
                                case 9:
                                    break;
                                case 10:
                                    break;
                                case 11:
                                    statdata.Add("Type", "ReaderLogOn");
                                    MessageArray[1] = "readerlog";
                                    MessageArray[2] = data[7].ToString();
                                    byte[] cardbytes3 = new byte[4];
                                    for (int i = 7; i <= 10; i++)
                                    {
                                        cardbytes3[i - 7] = data[i];
                                    }
                                    MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes3);
                                    objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes3));

                                    break;
                                case 12:
                                    statdata.Add("Type", "ReaderLogOff");
                                    MessageArray[1] = "readerlog";
                                    MessageArray[2] = data[7].ToString();
                                    byte[] cardbytes4 = new byte[4];
                                    for (int i = 7; i <= 10; i++)
                                    {
                                        cardbytes4[i - 7] = data[i];
                                    }
                                    MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes4);
                                    objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes4));

                                    break;
                                case 13:
                                    break;
                                case 14:
                                    break;
                                case 15:
                                    break;
                                case 17:
                                    break;
                                default:
                                    break;
                            }
                            statdata.Add("Data", objdata);
                        }
                        else
                        {
                            statdata.Add("InstructionStatus", "Fail");
                            MessageArray[1] = "Unsuccessful";
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x02))
                    {

                        statdata.Add("Command", "PanelControl");
                        MessageArray[0] = "PanelControl";
                        if (data[6] == Convert.ToByte(0xc4))
                        {
                            statdata.Add("InstructionStatus", "Success");
                            switch (data[5])
                            {
                                case 04:
                                    statdata.Add("Type", "Panel");
                                    //MessageArray[1] = "KeyValue";
                                    switch (Convert.ToByte(data[7]))
                                    {
                                        case 16:
                                            objdata.Add("MediaSignal", "Desktop");
                                            log = "Desktop";
                                            break;
                                        case 17:
                                            objdata.Add("MediaSignal", "Laptop");
                                            log = "Laptop";
                                            break;
                                        case 18:
                                            objdata.Add("MediaSignal", "DigitalBooth");
                                            log = "DigitalBooth";
                                            break;
                                        case 19:
                                            objdata.Add("MediaSignal", "DigitalEquipment");
                                            log = "DigitalEquipment";
                                            break;
                                        case 29:
                                            objdata.Add("PcStatus", "On");
                                            log = "ComputerOn";
                                            break;
                                        case 30:

                                            objdata.Add("PcStatus", "Off");
                                            log = "ComputerOff";
                                            //Status[5] = "Off";
                                            break;
                                        case 192:
                                            objdata.Add("WorkStatus", "Open");
                                            log = "SystemOn";
                                            MessageArray[2] = "SystemOn";
                                            Status[5] = "On";
                                            break;
                                        case 193:
                                            objdata.Add("WorkStatus", "Close");
                                            log = "SystemOff";
                                            MessageArray[2] = "SystemOff";
                                            Status[5] = "Off";
                                            break;

                                        case 86:
                                            objdata.Add("Screen", "Down");
                                            log = "ScreenDown";
                                            MessageArray[2] = "DSDown";
                                            break;
                                        case 102:
                                            objdata.Add("Screen", "Stop");
                                            log = "ScreenStop";
                                            MessageArray[2] = "DSStop";
                                            break;
                                        case 118:
                                            objdata.Add("Screen", "Up");
                                            log = "ScreenRise";
                                            MessageArray[2] = "DSUp";
                                            break;
                                        case 44:
                                            objdata.Add("IsSystemLock", "True");
                                            log = "SystemLock";
                                            MessageArray[2] = "syslock";
                                            break;
                                        case 45:
                                            objdata.Add("IsSystemLock", "False");
                                            log = "SystemUnlock";
                                            MessageArray[2] = "sysunlock";
                                            break;
                                        case 46:
                                            objdata.Add("IsPodiumLock", "True");
                                            log = "PodiumLock";
                                            MessageArray[2] = "podiumlock";
                                            break;
                                        case 47:
                                            objdata.Add("IsPodiumLock", "False");
                                            log = "PodiumUnlock";
                                            MessageArray[2] = "podiumunlock";
                                            break;
                                        case 95:
                                            objdata.Add("IsClassLock", "True");
                                            log = "ClassLock";
                                            MessageArray[2] = "classlock";
                                            break;
                                        case 96:
                                            objdata.Add("IsClassLock", "False");
                                            log = "ClassUnlock";
                                            MessageArray[2] = "classunlock";
                                            break;
                                        case 32:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            MessageArray[2] = "volplus";
                                            break;
                                        case 33:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            MessageArray[2] = "volminus";
                                            break;
                                        case 34:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            MessageArray[2] = "mute";
                                            break;
                                        case 35:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredvolplus";
                                            break;
                                        case 36:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredvolminus";
                                            break;
                                        case 37:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredmute";
                                            break;
                                        case 115:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            MessageArray[2] = "wirelessvolplus";
                                            break;
                                        case 116:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            MessageArray[2] = "wirelessvolminus";
                                            break;
                                        case 117:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            MessageArray[2] = "wirelessmute";
                                            break;
                                        case 146:
                                            objdata.Add("Recording", "Start");
                                            MessageArray[2] = "startrec";
                                            break;
                                        case 147:
                                            objdata.Add("Recording", "Stop");
                                            MessageArray[2] = "stoprec";
                                            break;
                                        case 48:
                                            objdata.Add("DVD", "Play");
                                            MessageArray[2] = "playdvd";
                                            break;
                                        case 49:
                                            objdata.Add("DVD", "WareHouse");
                                            MessageArray[2] = "warehousedvd";
                                            break;
                                        case 54:
                                            objdata.Add("DVD", "Power");
                                            MessageArray[2] = "powerdvd";
                                            break;
                                        case 55:
                                            objdata.Add("DVD", "Pause");
                                            MessageArray[2] = "pausedvd";
                                            break;
                                        case 56:
                                            objdata.Add("DVD", "Stop");
                                            MessageArray[2] = "stopdvd";
                                            break;
                                        case 50:
                                            objdata.Add("DVD", "Forward");
                                            MessageArray[2] = "forwarddvd";
                                            break;
                                        case 64:
                                            objdata.Add("DVD", "Rewind");
                                            MessageArray[2] = "rewinddvd";
                                            break;
                                        case 65:
                                            objdata.Add("DVD", "Previous");
                                            MessageArray[2] = "previousdvd";
                                            break;
                                        case 66:
                                            objdata.Add("DVD", "Next");
                                            MessageArray[2] = "nextdvd";
                                            break;
                                        case 160:
                                            objdata.Add("TV", "Power");
                                            MessageArray[2] = "powertv";
                                            break;
                                        case 161:
                                            objdata.Add("TV", "");
                                            MessageArray[2] = "tvsignal";
                                            break;
                                        case 162:
                                            objdata.Add("TV", "ChannelPlus");
                                            MessageArray[2] = "channelplustv";
                                            break;
                                        case 163:
                                            objdata.Add("TV", "ChannelMinus");
                                            MessageArray[2] = "channelminustv";
                                            break;
                                        case 164:
                                            objdata.Add("TV", "VolumePlus");
                                            MessageArray[2] = "volplustv";
                                            break;
                                        case 165:
                                            objdata.Add("TV", "VolumeMinus");
                                            MessageArray[2] = "volminustv";
                                            break;
                                        case 166:
                                            objdata.Add("TV", "Menu");
                                            MessageArray[2] = "menutv";
                                            break;
                                        case 167:
                                            objdata.Add("TV", "Ok");
                                            MessageArray[2] = "oktv";
                                            break;
                                        case 168:
                                            objdata.Add("TV", "Exit");
                                            MessageArray[2] = "exittv";
                                            break;
                                        case 51:
                                            objdata.Add("ProjectorStatus", "On");
                                            log = "ProjectorOn";
                                            MessageArray[2] = "projopen";
                                            Status[1] = "On";
                                            break;
                                        case 67:
                                            objdata.Add("ProjectorStatus", "Off");
                                            log = "ProjectorOff";
                                            MessageArray[2] = "projoff";
                                            Status[1] = "Off";
                                            break;
                                        case 52:
                                            objdata.Add("Projector", "Hdmi");
                                            log = "ProjectorHDMI";
                                            MessageArray[2] = "hdmi";
                                            break;
                                        case 53:
                                            objdata.Add("Projector", "Video");
                                            log = "ProjectorVideo";
                                            MessageArray[2] = "video";
                                            break;
                                        case 68:
                                            objdata.Add("Projector", "Vga");
                                            log = "ProjectorVGA";
                                            MessageArray[2] = "vga";
                                            break;
                                        case 69:
                                            statdata.Add("Projector", "Sleep");
                                            log = "ProjectorSleep";
                                            MessageArray[2] = "sleep";
                                            break;
                                        case 119:
                                            objdata.Add("Curtain1", "Open");
                                            log = "CurtainOpen";
                                            MessageArray[2] = "curtain1open";
                                            break;
                                        case 87:
                                            objdata.Add("Curtain1", "Close");
                                            log = "CurtainClose";
                                            MessageArray[2] = "curtain1close";
                                            break;
                                        case 103:
                                            objdata.Add("Curtain1", "Stop");
                                            log = "CurtainStop";
                                            MessageArray[2] = "curtain1stop";
                                            break;
                                        case 99:
                                            objdata.Add("Curtain2", "Open");
                                            MessageArray[2] = "curtain2open";
                                            break;
                                        case 100:
                                            objdata.Add("Curtain2", "Close");
                                            MessageArray[2] = "curtain2close";
                                            break;
                                        case 101:
                                            objdata.Add("Curtain2", "Stop");
                                            MessageArray[2] = "curtain2stop";
                                            break;
                                        case 70:
                                            objdata.Add("Curtain3", "Open");
                                            MessageArray[2] = "curtain3open";
                                            break;
                                        case 71:
                                            objdata.Add("Curtain3", "Close");
                                            MessageArray[2] = "curtain3close";
                                            break;
                                        case 72:
                                            objdata.Add("Curtain3", "Stop");
                                            MessageArray[2] = "curtain3stop";
                                            break;
                                        case 73:
                                            objdata.Add("Curtain4", "Open");
                                            MessageArray[2] = "curtain4open";
                                            break;
                                        case 74:
                                            objdata.Add("Curtain4", "Close");
                                            MessageArray[2] = "curtain4close";
                                            break;
                                        case 75:
                                            objdata.Add("Curtain4", "Stop");
                                            MessageArray[2] = "curtain4stop";
                                            break;
                                        case 120:
                                            objdata.Add("Light", "light1");
                                            MessageArray[2] = "light1";
                                            break;
                                        case 104:
                                            objdata.Add("Light", "light2");
                                            MessageArray[2] = "light2";
                                            break;
                                        case 88:
                                            objdata.Add("Light", "light3");
                                            MessageArray[2] = "light3";
                                            break;
                                        case 83:
                                            objdata.Add("Light", "light4");
                                            MessageArray[2] = "light4";
                                            break;
                                        case 84:
                                            objdata.Add("Light", "light5");
                                            MessageArray[2] = "light5";
                                            break;
                                        case 85:
                                            objdata.Add("Light", "light6");
                                            MessageArray[2] = "light6";
                                            break;
                                        case 76:
                                            objdata.Add("Light", "light7");
                                            MessageArray[2] = "light7";
                                            break;
                                        case 77:
                                            objdata.Add("Light", "light8");
                                            MessageArray[2] = "light8";
                                            break;
                                        case 176:
                                            objdata.Add("BlueRayDVD", "Play");
                                            MessageArray[2] = "playbludvd";
                                            break;
                                        case 177:
                                            objdata.Add("BlueRayDVD", "Warehouse");
                                            MessageArray[2] = "warehousebludvd";
                                            break;
                                        case 178:
                                            objdata.Add("BlueRayDVD", "Power");
                                            MessageArray[2] = "powerbludvd";
                                            break;
                                        case 179:
                                            objdata.Add("BlueRayDVD", "Pause");
                                            MessageArray[2] = "pausebludvd";
                                            break;
                                        case 180:
                                            objdata.Add("BlueRayDVD", "Stop");
                                            MessageArray[2] = "stopbludvd";
                                            break;
                                        case 181:
                                            objdata.Add("BlueRayDVD", "Forward");
                                            MessageArray[2] = "forwardbludvd";
                                            break;
                                        case 182:
                                            objdata.Add("BlueRayDVD", "Rewind");
                                            MessageArray[2] = "rewindbludvd";
                                            break;
                                        case 183:
                                            objdata.Add("BlueRayDVD", "Previous");
                                            MessageArray[2] = "previousbludvd";
                                            break;
                                        case 184:
                                            objdata.Add("BlueRayDVD", "Next");
                                            MessageArray[2] = "nextbludvd";
                                            break;

                                        default:
                                            objdata.Add("NoData", "NoData");
                                            MessageArray[2] = "Nochange";
                                            break;
                                    }
                                    statdata.Add("Data", objdata);
                                    statdata.Add("Log", log);
                                    break;

                                case 05:
                                    statdata.Add("Type", "LedIndicator");

                                    MessageArray[1] = "LEDIndicator";
                                    // int result = 0;
                                    int re = -1;

                                    //if (data[7] == Convert.ToByte(0x00))
                                    //{
                                    //    MessageArray[2] = "SystemSwitchOff";
                                    //    //Status[1] = "Off";
                                    //}
                                    //else /*if (data[7] == Convert.ToByte(0x01))*/
                                    //{
                                    int p = 256 * data[7] + data[8];
                                    if ((p & 256) == 256)
                                    {
                                        objdata.Add("WorkStatus", "Open");
                                        MessageArray[2] = "SystemSwitchOn";
                                    }
                                    else
                                    {
                                        objdata.Add("WorkStatus", "Closed");
                                        MessageArray[2] = "SystemSwitchOff";
                                    }
                                    if ((p & 128) == 128)
                                    {
                                        objdata.Add("PcStatus", "On");
                                        MessageArray[4] = "Computer";
                                    }
                                    else
                                    {
                                        objdata.Add("PcStatus", "Off");
                                        MessageArray[4] = "ComputerOff";
                                    }
                                    // int p = Convert.ToInt32(r); 
                                    int[] compare = new int[] { 1, 2, 4, 8, 16, 32, 64, 512, 1024, 2048,
                                            4096, 8192, 16384 };
                                    for (int i = 0; i < compare.Length; i++)
                                    {
                                        re = p & compare[i];
                                        if (re == compare[i])
                                        {
                                            // result = compare[i];
                                            //break;
                                            switch (re)
                                            {
                                                case 1:
                                                    objdata.Add("MediaSignal", "Desktop");
                                                    MessageArray[3] = "Desktop";
                                                    break;
                                                case 2:
                                                    objdata.Add("MediaSignal", "Laptop");
                                                    MessageArray[3] = "Laptop";
                                                    break;
                                                case 4:
                                                    objdata.Add("MediaSignal", "DigitalBooth");
                                                    MessageArray[3] = "DigitalCurtain";
                                                    break;
                                                case 8:
                                                    objdata.Add("MediaSignal", "DigitalEquipment");
                                                    MessageArray[3] = "DigitalScreen";
                                                    break;
                                                case 16:
                                                    objdata.Add("MediaSignal", "DVD");
                                                    MessageArray[3] = "DVD";
                                                    break;
                                                case 32:
                                                    objdata.Add("MediaSignal", "TV");
                                                    MessageArray[3] = "TV";
                                                    break;
                                                case 64:
                                                    objdata.Add("MediaSignal", "VideoCamera");
                                                    MessageArray[3] = "VideoCamera";
                                                    break;

                                                case 512:
                                                    objdata.Add("MediaSignal", "RecordingSystem");
                                                    MessageArray[3] = "RecordingSystem";
                                                    break;
                                                case 1024:
                                                    objdata.Add("MediaSignal", "BluRayDVD");
                                                    MessageArray[3] = "Blu-RayDVD";
                                                    break;
                                                case 2048:
                                                    objdata.Add("MediaSignal", "ExternalHD");
                                                    MessageArray[3] = "ExternalHD";
                                                    break;
                                                case 4096:
                                                    objdata.Add("IsSystemLock", "");
                                                    MessageArray[5] = "CentralLock";
                                                    break;
                                                case 8192:
                                                    objdata.Add("IsPodiumLock", "");
                                                    MessageArray[6] = "PodiumLock";
                                                    break;
                                                case 16384:
                                                    objdata.Add("IsClassLock", "");
                                                    MessageArray[7] = "ClassLock";
                                                    break;
                                            }
                                        }
                                    }
                                    if ((p & 4096) == 4096)
                                    {
                                        objdata.Add("IsSystemLock", "True");
                                        MessageArray[5] = "CentralLock";
                                    }
                                    else
                                    {
                                        objdata.Add("IsSystemLock", "False");
                                        MessageArray[5] = "CentralLockoff";
                                    }
                                    if ((p & 8192) == 8192)
                                    {
                                        objdata.Add("IsPodiumLock", "True");
                                        MessageArray[6] = "PodiumLock";
                                    }
                                    else
                                    {
                                        objdata.Add("IsPodiumLock", "False");
                                        MessageArray[6] = "PodiumLockoff";
                                    }

                                    if ((p & 16384) == 16384)
                                    {
                                        statdata.Add("IsClassLock", "True");
                                        MessageArray[7] = "ClassLock";
                                    }
                                    else
                                    {
                                        statdata.Add("IsClassLock", "False");
                                        MessageArray[7] = "ClassLockoff";
                                    }
                                    //}
                                    statdata.Add("Data", objdata);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            statdata.Add("InstructionStatus", "Fail");
                            MessageArray[1] = "Unsuccessful";
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x03))
                    {
                        statdata.Add("Command", "ProjectorConfig");
                        if (data[5] == 64)
                        {
                            if (data[6] == Convert.ToByte(0xc4))
                                MessageArray[1] = "MacSuccess";
                            else
                            {
                                MessageArray[1] = "MacFailure";
                            }
                        }

                        MessageArray[0] = "config";
                        switch (data[5])
                        {
                            case 1:
                                break;
                            case 2:
                                statdata.Add("Type", "ReadConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjectorOffDelayMinute", data[7].ToString().PadLeft(2, '0'));
                                    objdata.Add("ScreenAutoDrop", data[8].ToString().PadLeft(2, '0'));
                                    objdata.Add("ProjectorAutoOn", data[9].ToString().PadLeft(2, '0'));
                                    objdata.Add("ProjectorAutoOff", data[10].ToString().PadLeft(2, '0'));
                                    objdata.Add("ComputerAutoOn", data[11].ToString().PadLeft(2, '0'));
                                    objdata.Add("ComputerAutoOff", data[12].ToString().PadLeft(2, '0'));
                                    objdata.Add("ProjectorSwitchAuto", data[13].ToString().PadLeft(2, '0'));
                                    objdata.Add("ScreenLinkageOn", data[14].ToString().PadLeft(2, '0'));
                                    objdata.Add("ScreenLinkageOff", data[15].ToString().PadLeft(2, '0'));
                                    objdata.Add("VolumeMemoryOn", data[16].ToString().PadLeft(2, '0'));
                                    objdata.Add("BuzzerOn", data[17].ToString().PadLeft(2, '0'));
                                    objdata.Add("IODetectionOff", data[18].ToString().PadLeft(2, '0'));
                                    objdata.Add("IODetectionOn", data[19].ToString().PadLeft(2, '0'));
                                    objdata.Add("Projector232Signal", data[20].ToString().PadLeft(2, '0'));
                                    objdata.Add("projector2Infrared", data[21].ToString().PadLeft(2, '0'));
                                    objdata.Add("SwipeOn", data[22].ToString().PadLeft(2, '0'));
                                    objdata.Add("SwipeOff", data[23].ToString().PadLeft(2, '0'));
                                    objdata.Add("FingerPrintOn", data[24].ToString().PadLeft(2, '0'));
                                    objdata.Add("FingerPrintOff", data[25].ToString().PadLeft(2, '0'));
                                    objdata.Add("ProjectorOnDelaySecond", data[26].ToString().PadLeft(2, '0'));
                                    objdata.Add("ComputerLinkageOff", data[27].ToString().PadLeft(2, '0'));
                                    objdata.Add("HdmiAudio", data[28].ToString().PadLeft(2, '0'));
                                    objdata.Add("SystemAlarm", data[29].ToString().PadLeft(2, '0'));
                                    statdata.Add("Data", objdata);
                                    statdata.Add("Log", "ReadProjectorConfig");
                                    //MessageArray[2] = data[8].ToString();
                                    //MessageArray[3] = data[9].ToString();
                                    //MessageArray[4] = data[10].ToString();
                                    //MessageArray[5] = data[11].ToString();
                                    //MessageArray[6] = data[12].ToString();
                                    //MessageArray[7] = data[13].ToString();
                                    //MessageArray[8] = data[14].ToString();
                                    //MessageArray[9] = data[15].ToString();
                                    //MessageArray[10] = data[16].ToString();
                                    //MessageArray[11] = data[17].ToString();
                                    //MessageArray[12] = data[18].ToString();
                                    //MessageArray[13] = data[19].ToString();
                                    //MessageArray[14] = data[20].ToString();
                                    //MessageArray[15] = data[21].ToString();
                                    //MessageArray[16] = data[22].ToString();
                                    //MessageArray[17] = data[23].ToString();
                                    //MessageArray[18] = data[24].ToString();
                                    //MessageArray[19] = data[25].ToString();
                                    //MessageArray[20] = data[26].ToString();
                                    //MessageArray[21] = data[27].ToString();
                                    //MessageArray[22] = data[28].ToString();
                                    //MessageArray[23] = data[29].ToString();
                                    //MessageArray[24] = data[30].ToString();
                                    //MessageArray[25] = data[31].ToString();
                                    //MessageArray[26] = data[32].ToString();
                                    //MessageArray[27] = data[33].ToString();
                                }
                                else
                                    statdata.Add("InstructionStatus", "Fail");
                                break;
                            case 3:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("SetConfig", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("SetConfig", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            case 4:
                                break;
                            case 5:
                                break;
                            case 6:
                                break;
                            case 7:
                                break;

                            default:
                                break;
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x04))
                    {
                        MessageArray[0] = "Temp";
                        if (data[6] == Convert.ToByte(0xc9))
                        {
                            MessageArray[1] = "Unsuccessful";
                        }
                        else
                        {
                            switch (data[5])
                            {
                                case 1:
                                    if (data[6] == Convert.ToByte(0x00))
                                    {
                                        MessageArray[1] = data[7] + "." + data[8].ToString("D2");
                                    }
                                    else
                                    {
                                        MessageArray[1] = "-" + data[7] + "." + data[8].ToString("D2");
                                    }
                                    MessageArray[2] = data[9] + "." + data[10].ToString("D2");
                                    MessageArray[3] = (256 * data[11]) + data[12] + "." + data[13].ToString("D2");
                                    MessageArray[4] = (256 * data[14]) + data[15] + "." + data[16].ToString("D2");

                                    if (length > 18)
                                    {
                                        //voltage
                                        MessageArray[5] = data[17].ToString();

                                        //current
                                        MessageArray[6] = ((256 * data[18]) + data[19]).ToString();
                                        MessageArray[7] = ((256 * data[20]) + data[21]).ToString();
                                        MessageArray[8] = ((256 * data[22]) + data[23]).ToString();
                                        MessageArray[9] = ((256 * data[24]) + data[25]).ToString();
                                        //co2
                                        MessageArray[10] = ((256 * data[26]) + data[27]).ToString();
                                        //formaldehyde
                                        MessageArray[11] = data[28] + data[29] + "." + data[30].ToString("D2");
                                        //volatile gases
                                        MessageArray[12] = data[31] + data[32] + "." + data[33].ToString("D2");
                                        //light intensity
                                        MessageArray[13] = ((256 * data[34]) + data[35]).ToString();

                                    }

                                    break;
                                case 2:

                                    break;
                                case 3:

                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x05))
                    {
                        statdata.Add("Command", "NetworkControl");
                        MessageArray[0] = "NetworkControl";
                        switch (data[5])
                        {
                            case 01:
                                statdata.Add("Type", "Heartbeat");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    {
                                        //machine status
                                        MessageArray[2] = "在线";
                                        //work status
                                        if (data[8] == Convert.ToByte(0x00))
                                        {
                                            MessageArray[3] = "待机";//CLOSED
                                            objdata.Add("WorkStatus", "Closed");
                                            Status[5] = "Off";
                                        }
                                        else
                                        {
                                            MessageArray[3] = "运行中";//OPEN
                                            objdata.Add("WorkStatus", "Open");
                                            Status[5] = "On";
                                        }
                                        //system lock status
                                        if (data[12] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsSystemLock", "True");
                                            MessageArray[12] = "解锁";//locked
                                        }
                                        else
                                        {
                                            objdata.Add("IsSystemLock", "False");
                                            MessageArray[12] = "锁定";//unlocked
                                        }

                                        //class lock status
                                        if (data[9] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsClassLock", "True");
                                            MessageArray[13] = "解锁";//locked
                                        }
                                        else
                                        {
                                            objdata.Add("IsClassLock", "False");
                                            MessageArray[13] = "锁定";//unlocked
                                        }

                                        //podium lock status
                                        if (data[10] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsPodiumLock", "True");
                                            MessageArray[14] = "解锁";//locked
                                        }
                                        else
                                        {
                                            objdata.Add("IsPodiumLock", "False");
                                            MessageArray[14] = "锁定";//unlocked

                                        }

                                        objdata.Add("Volume", data[19].ToString());// main volume
                                        objdata.Add("WiredMicVolume", data[20].ToString());//wired mic volume
                                        objdata.Add("WirelessMicVolume", data[21].ToString());// wireless mic volume
                                                                                              //media signal
                                        switch (Convert.ToInt32(data[11]))
                                        {
                                            case 1:
                                                objdata.Add("MediaSignal", "Desktop");
                                                // MessageArray[11] = "台式电脑";//desktop
                                                break;
                                            case 2:
                                                objdata.Add("MediaSignal", "Laptop");
                                                // MessageArray[11] = "手提电脑";//laptop
                                                break;
                                            case 3:
                                                objdata.Add("MediaSignal", "DigitalBooth");
                                                // MessageArray[11] = "数码展台";//digital booth(curtain)
                                                break;
                                            case 4:
                                                objdata.Add("MediaSignal", "DigitalEquipment");
                                                // MessageArray[11] = "数码设备";//digital equipment(Screen)
                                                break;
                                            case 5:
                                                objdata.Add("MediaSignal", "Dvd");
                                                // MessageArray[11] = "DVD";//dvd
                                                break;
                                            case 6:
                                                objdata.Add("MediaSignal", "BluRayDvd");
                                                // MessageArray[11] = "蓝光DVD";//"Blu-Ray DVD"
                                                break;
                                            case 7:
                                                objdata.Add("MediaSignal", "Tv");
                                                // MessageArray[11] = "电视机"; //TV
                                                break;
                                            case 8:
                                                objdata.Add("MediaSignal", "VideoCamera");
                                                // MessageArray[11] = "摄像机";//Video Camera
                                                break;
                                            case 9:
                                                objdata.Add("MediaSignal", "RecordingSystem");
                                                // MessageArray[11] = "录播"; //Recording System
                                                break;
                                            default:
                                                objdata.Add("MediaSignal", "None");
                                                MessageArray[11] = "无信号"; //No system
                                                break;
                                        }


                                        MessageArray[4] = "--";//timer service

                                        //pc status
                                        if (data[7] == Convert.ToByte(0x00))
                                        {
                                            MessageArray[5] = "已关机";//Off
                                            objdata.Add("PcStatus", "Off");
                                            Status[2] = "Off";
                                        }
                                        else
                                        {
                                            MessageArray[5] = "已开机";//On
                                            objdata.Add("PcStatus", "On");
                                            Status[2] = "On";
                                        }
                                        //projector status
                                        if (data[13] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("ProjectorStatus", "Off");
                                            MessageArray[6] = "已关机";
                                            Status[1] = "Off";
                                        }
                                        else
                                        {
                                            objdata.Add("ProjectorStatus", "On");
                                            MessageArray[6] = "已开机";
                                            Status[1] = "On";
                                        }
                                        MessageArray[7] = "--";//projector hours
                                                               //Curtain Status窗帘
                                        switch (Convert.ToInt32(data[15]))
                                        {
                                            case 1:
                                                MessageArray[8] = "开";//Open
                                                objdata.Add("Curtain", "Open");
                                                break;
                                            case 2:
                                                objdata.Add("Curtain", "Close");
                                                MessageArray[8] = "关";//Close
                                                break;
                                            case 0:
                                                objdata.Add("Curtain", "Stop");
                                                MessageArray[8] = "停";//Stop
                                                break;
                                        }
                                        //Screen status屏幕
                                        switch (Convert.ToInt32(data[14]))
                                        {
                                            case 1:

                                                MessageArray[9] = "升";//Rise Up
                                                objdata.Add("Screen", "Up");
                                                Status[6] = "Off";
                                                break;
                                            case 2:
                                                MessageArray[9] = "降";//Down
                                                objdata.Add("Screen", "Down");
                                                Status[6] = "On";
                                                break;
                                            case 0:
                                                MessageArray[9] = "停";//Stop
                                                objdata.Add("Screen", "Stop");
                                                Status[6] = "Off";
                                                break;
                                        }
                                        //light status
                                        if (data[16] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("LightStatus", "Off");
                                            MessageArray[10] = "关";//Off
                                        }
                                        else
                                        {
                                            objdata.Add("LightStatus", "On");
                                            MessageArray[10] = "开";//On 
                                        }

                                        objdata.Add("Temperature", data[17].ToString());

                                        objdata.Add("Humidity", data[18].ToString());

                                        objdata.Add("Voltage", data[22].ToString());
                                        MessageArray[21] = (256 * data[24] + data[23]).ToString();
                                        objdata.Add("Power", (256 * data[24] + data[23]).ToString());

                                        var tempbit = Convert.ToString(data[25], 2).PadLeft(4, '0');

                                        if (tempbit[3] == '1')
                                            objdata.Add("ProjectorPowerStatus", "On");
                                        else
                                            objdata.Add("ProjectorPowerStatus", "Off");
                                        if (Convert.ToByte(tempbit[2]) == '1')
                                            objdata.Add("ComputerPowerStatus", "On");
                                        else
                                            objdata.Add("ComputerPowerStatus", "Off");

                                        if (Convert.ToByte(tempbit[1]) == '1')
                                            objdata.Add("AmplifierPowerStatus", "On");
                                        else
                                            objdata.Add("AmplifierPowerStatus", "Off");
                                        if (Convert.ToByte(tempbit[0]) == '1')
                                            objdata.Add("OtherPowerStatus", "On");
                                        else
                                            objdata.Add("OtherPowerStatus", "Off");
                                    }
                                }
                                else
                                {
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            case 02:
                                statdata.Add("Type", "ControlExecution");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    switch (Convert.ToByte(data[7]))
                                    {
                                        case 16:
                                            objdata.Add("MediaSignal", "Desktop");
                                            log = "Desktop";
                                            MessageArray[3] = "Desktop";
                                            break;
                                        case 17:
                                            objdata.Add("MediaSignal", "Laptop");
                                            log = "Laptop";
                                            MessageArray[3] = "Laptop";
                                            break;
                                        case 18:
                                            //DigitalBooth = DigitalCurtain
                                            objdata.Add("MediaSignal", "DigitalBooth");
                                            log = "DigitalBooth";
                                            MessageArray[3] = "DigitalCurtain";
                                            break;
                                        case 19:
                                            // DigitalEquipment=DigitalScreen
                                            objdata.Add("MediaSignal", "DigitalEquipment");
                                            log = "DigitalEquipment";
                                            MessageArray[3] = "DigitalScreen";
                                            break;
                                        case 20:
                                            objdata.Add("MediaSignal", "DVD");
                                            log = "Dvd";
                                            MessageArray[3] = "DVD";
                                            break;
                                        case 21:
                                            objdata.Add("MediaSignal", "TV");
                                            log = "TV";
                                            MessageArray[3] = "TV";
                                            break;
                                        case 22:
                                            objdata.Add("MediaSignal", "VideoCamera");
                                            log = "VideoCamera";
                                            MessageArray[3] = "VideoCamera";
                                            break;

                                        case 25:
                                            objdata.Add("MediaSignal", "RecordingSystem");
                                            log = "RecordingDevice";
                                            MessageArray[3] = "RecordingSystem";
                                            break;
                                        case 26:
                                            objdata.Add("MediaSignal", "BluRayDVD");
                                            log = "BlurayDvd";
                                            MessageArray[3] = "Blu-RayDVD";
                                            break;
                                        //case 29:
                                        //    objdata.Add("MediaSignal", "ExternalHD");
                                        //    MessageArray[3] = "ExternalHD";
                                        //    break;
                                        case 192:

                                            objdata.Add("WorkStatus", "Open");
                                            log = "SystemOn";
                                            MessageArray[2] = "SystemON";
                                            // Status[5] = "On";

                                            break;
                                        case 193:
                                            objdata.Add("WorkStatus", "Closed");
                                            log = "SystemOff";
                                            MessageArray[2] = "SystemOff";
                                            //Status[5] = "Off";
                                            break;
                                        case 29:

                                            objdata.Add("pcStatus", "On");
                                            log = "ComputerOn";
                                            MessageArray[2] = "ComputerOn";

                                            break;
                                        case 30:

                                            objdata.Add("PcStatus", "Off");
                                            log = "ComputerOff";
                                            MessageArray[2] = "ComputerOff";

                                            //Status[5] = "Off";
                                            break;
                                        case 86:
                                            objdata.Add("Screen", "Down");
                                            log = "ScreenDown";
                                            MessageArray[2] = "DSDown";
                                            break;
                                        case 102:
                                            objdata.Add("Screen", "Stop");
                                            log = "ScreenStop";
                                            MessageArray[2] = "DSStop";
                                            break;
                                        case 118:
                                            objdata.Add("Screen", "Up");
                                            log = "ScreenRise";
                                            MessageArray[2] = "DSUp";
                                            break;
                                        case 44:
                                            objdata.Add("IsSystemLock", "True");
                                            log = "SystemLock";
                                            MessageArray[2] = "syslock";
                                            break;
                                        case 45:
                                            objdata.Add("IsSystemLock", "False");
                                            log = "SystemUnlock";
                                            MessageArray[2] = "sysunlock";
                                            break;
                                        case 46:
                                            objdata.Add("IsPodiumLock", "True");
                                            log = "PodiumLock";
                                            MessageArray[2] = "podiumlock";
                                            break;
                                        case 47:
                                            objdata.Add("IsPodiumLock", "False");
                                            log = "PodiumUnlock";
                                            MessageArray[2] = "podiumunlock";
                                            break;
                                        case 95:
                                            objdata.Add("IsClassLock", "True");
                                            log = "ClassLock";
                                            MessageArray[2] = "classlock";
                                            break;
                                        case 96:
                                            objdata.Add("IsClassLock", "False");
                                            log = "ClassUnlock";
                                            MessageArray[2] = "classunlock";
                                            break;
                                        case 32:
                                            objdata.Add("Volume", "Increase");
                                            log = "Volume";
                                            MessageArray[2] = "volplus";
                                            break;
                                        case 33:
                                            objdata.Add("Volume", "Decrease");
                                            log = "Volume";
                                            MessageArray[2] = "volminus";
                                            break;
                                        case 34:
                                            objdata.Add("Volume", "Mute");
                                            log = "Volume";
                                            MessageArray[2] = "mute";
                                            break;
                                        case 35:
                                            objdata.Add("WiredMicVolume", "Increase");
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredvolplus";
                                            break;
                                        case 36:
                                            objdata.Add("WiredMicVolume", "Decrease");
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredvolminus";
                                            break;
                                        case 37:
                                            objdata.Add("WiredMicVolume", "Mute");
                                            log = "WiredMicVolume";
                                            MessageArray[2] = "wiredmute";
                                            break;
                                        case 115:
                                            objdata.Add("WirelessMicVolume", "Increase");
                                            log = "WirlessMicVolume";
                                            MessageArray[2] = "wirelessvolplus";
                                            break;
                                        case 116:
                                            objdata.Add("WirelessMicVolume", "Decrease");
                                            log = "WirlessMicVolume";
                                            MessageArray[2] = "wirelessvolminus";
                                            break;
                                        case 117:
                                            objdata.Add("WirelessMicVolume", "Mute");
                                            log = "WirlessMicVolume";
                                            MessageArray[2] = "wirelessmute";
                                            break;
                                        case 146:
                                            objdata.Add("Recording", "Start");
                                            MessageArray[2] = "startrec";
                                            break;
                                        case 147:
                                            objdata.Add("Recording", "Stop");
                                            MessageArray[2] = "stoprec";
                                            break;
                                        case 48:
                                            objdata.Add("DVD", "Play");
                                            MessageArray[2] = "playdvd";
                                            break;
                                        case 49:
                                            objdata.Add("DVD", "WareHouse");
                                            MessageArray[2] = "warehousedvd";
                                            break;
                                        case 54:
                                            objdata.Add("DVD", "Power");
                                            MessageArray[2] = "powerdvd";
                                            break;
                                        case 55:
                                            objdata.Add("DVD", "Pause");
                                            MessageArray[2] = "pausedvd";
                                            break;
                                        case 56:
                                            objdata.Add("DVD", "Stop");
                                            MessageArray[2] = "stopdvd";
                                            break;
                                        case 50:
                                            objdata.Add("DVD", "Forward");
                                            MessageArray[2] = "forwarddvd";
                                            break;
                                        case 64:
                                            objdata.Add("DVD", "Rewind");
                                            MessageArray[2] = "rewinddvd";
                                            break;
                                        case 65:
                                            objdata.Add("DVD", "Previous");
                                            MessageArray[2] = "previousdvd";
                                            break;
                                        case 66:
                                            objdata.Add("DVD", "Next");
                                            MessageArray[2] = "nextdvd";
                                            break;
                                        case 160:
                                            objdata.Add("TV", "Power");
                                            MessageArray[2] = "powertv";
                                            break;
                                        case 161:
                                            objdata.Add("TV", "");
                                            MessageArray[2] = "tvsignal";
                                            break;
                                        case 162:
                                            objdata.Add("TV", "ChannelPlus");
                                            MessageArray[2] = "channelplustv";
                                            break;
                                        case 163:
                                            objdata.Add("TV", "ChannelMinus");
                                            MessageArray[2] = "channelminustv";
                                            break;
                                        case 164:
                                            objdata.Add("TV", "VolumePlus");
                                            MessageArray[2] = "volplustv";
                                            break;
                                        case 165:
                                            objdata.Add("TV", "VolumeMinus");
                                            MessageArray[2] = "volminustv";
                                            break;
                                        case 166:
                                            objdata.Add("TV", "Menu");
                                            MessageArray[2] = "menutv";
                                            break;
                                        case 167:
                                            objdata.Add("TV", "Ok");
                                            MessageArray[2] = "oktv";
                                            break;
                                        case 168:
                                            objdata.Add("TV", "Exit");
                                            MessageArray[2] = "exittv";
                                            break;
                                        case 51:
                                            objdata.Add("ProjectorStatus", "On");
                                            log = "ProjectorOn";
                                            MessageArray[2] = "projopen";
                                            Status[1] = "On";
                                            break;
                                        case 67:
                                            objdata.Add("ProjectorStatus", "Off");
                                            log = "ProjectorOff";
                                            MessageArray[2] = "projoff";
                                            Status[1] = "Off";
                                            break;
                                        case 52:
                                            objdata.Add("ProjectorStatus", "Hdmi");
                                            log = "ProjectorHDMI";
                                            MessageArray[2] = "hdmi";
                                            break;
                                        case 53:
                                            objdata.Add("ProjectorStatus", "Video");
                                            log = "ProjectorVideo";
                                            MessageArray[2] = "video";
                                            break;
                                        case 68:
                                            statdata.Add("ProjectorStatus", "Vga");
                                            log = "ProjectorVGA";
                                            MessageArray[2] = "vga";
                                            break;
                                        case 69:
                                            objdata.Add("ProjectorStatus", "Sleep");
                                            log = "ProjectorSleep";
                                            MessageArray[2] = "sleep";
                                            break;
                                        case 119:
                                            objdata.Add("Curtain1", "Open");
                                            log = "CurtainOpen";
                                            MessageArray[2] = "curtain1open";
                                            break;
                                        case 87:
                                            objdata.Add("Curtain1", "Close");
                                            log = "CurtainClose";
                                            MessageArray[2] = "curtain1close";
                                            break;
                                        case 103:
                                            objdata.Add("Curtain1", "Stop");
                                            log = "CurtainStop";
                                            MessageArray[2] = "curtain1stop";
                                            break;
                                        case 99:
                                            objdata.Add("Curtain2", "Open");
                                            MessageArray[2] = "curtain2open";
                                            break;
                                        case 100:
                                            objdata.Add("Curtain2", "Close");
                                            MessageArray[2] = "curtain2close";
                                            break;
                                        case 101:
                                            objdata.Add("Curtain2", "Stop");
                                            MessageArray[2] = "curtain2stop";
                                            break;
                                        case 70:
                                            objdata.Add("Curtain3", "Open");
                                            MessageArray[2] = "curtain3open";
                                            break;
                                        case 71:
                                            objdata.Add("Curtain3", "Close");
                                            MessageArray[2] = "curtain3close";
                                            break;
                                        case 72:
                                            objdata.Add("Curtain3", "Stop");
                                            MessageArray[2] = "curtain3stop";
                                            break;
                                        case 73:
                                            objdata.Add("Curtain4", "Open");
                                            MessageArray[2] = "curtain4open";
                                            break;
                                        case 74:
                                            objdata.Add("Curtain4", "Close");
                                            MessageArray[2] = "curtain4close";
                                            break;
                                        case 75:
                                            objdata.Add("Curtain4", "Stop");
                                            MessageArray[2] = "curtain4stop";
                                            break;
                                        case 120:
                                            objdata.Add("Light", "light1");
                                            MessageArray[2] = "light1";
                                            break;
                                        case 104:
                                            objdata.Add("Light", "light2");
                                            MessageArray[2] = "light2";
                                            break;
                                        case 88:
                                            objdata.Add("Light", "light3");
                                            MessageArray[2] = "light3";
                                            break;
                                        case 83:
                                            objdata.Add("Light", "light4");
                                            MessageArray[2] = "light4";
                                            break;
                                        case 84:
                                            statdata.Add("Light", "light5");
                                            MessageArray[2] = "light5";
                                            break;
                                        case 85:
                                            objdata.Add("Light", "light6");
                                            MessageArray[2] = "light6";
                                            break;
                                        case 76:
                                            objdata.Add("Light", "light7");
                                            MessageArray[2] = "light7";
                                            break;
                                        case 77:
                                            objdata.Add("Light", "light8");
                                            MessageArray[2] = "light8";
                                            break;
                                        case 176:
                                            objdata.Add("BlueRayDVD", "Play");
                                            MessageArray[2] = "playbludvd";
                                            break;
                                        case 177:
                                            objdata.Add("BlueRayDVD", "Warehouse");
                                            MessageArray[2] = "warehousebludvd";
                                            break;
                                        case 178:
                                            objdata.Add("BlueRayDVD", "Power");
                                            MessageArray[2] = "powerbludvd";
                                            break;
                                        case 179:
                                            objdata.Add("BlueRayDVD", "Pause");
                                            MessageArray[2] = "pausebludvd";
                                            break;
                                        case 180:
                                            objdata.Add("BlueRayDVD", "Stop");
                                            MessageArray[2] = "stopbludvd";
                                            break;
                                        case 181:
                                            objdata.Add("BlueRayDVD", "Forward");
                                            MessageArray[2] = "forwardbludvd";
                                            break;
                                        case 182:
                                            objdata.Add("BlueRayDVD", "Rewind");
                                            MessageArray[2] = "rewindbludvd";
                                            break;
                                        case 183:
                                            objdata.Add("BlueRayDVD", "Previous");
                                            MessageArray[2] = "previousbludvd";
                                            break;
                                        case 184:
                                            objdata.Add("BlueRayDVD", "Next");
                                            MessageArray[2] = "nextbludvd";
                                            break;
                                        default:
                                            objdata.Add("NoData", "NoData");
                                            MessageArray[2] = "Nochange";
                                            break;
                                    }
                                }
                                else
                                {
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            case 04:
                                break;

                            case 07:
                                statdata.Add("Type", "ControlExecution");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");

                                    switch (data[7])
                                    {
                                        case 01:
                                            MessageArray[1] = "PowerSupply";
                                            switch (Convert.ToByte(data[8]))
                                            {
                                                case 01:
                                                    if (Convert.ToByte(data[9]) == 1)
                                                    {
                                                        objdata.Add("ProjectorPowerStatus", "On");
                                                        log = "ProjectorPowerOn";
                                                    }

                                                    else
                                                    {
                                                        objdata.Add("ProjectorPowerStatus", "Off");
                                                        log = "ProjectorPowerOff";
                                                    }

                                                    break;
                                                case 02:
                                                    if (Convert.ToByte(data[9]) == 1)
                                                    {
                                                        objdata.Add("ComputerPowerStatus", "On");
                                                        log = "ComputerPowerOn";
                                                    }

                                                    else
                                                    {
                                                        objdata.Add("ComputerPowerStatus", "Off");
                                                        log = "ComputerPowerOff";
                                                    }

                                                    break;
                                                case 03:
                                                    if (Convert.ToByte(data[9]) == 1)
                                                    {
                                                        objdata.Add("AmplifierPowerStatus", "On");
                                                        log = "AmplifierPowerOn";
                                                    }

                                                    else
                                                    {
                                                        objdata.Add("AmplifierPowerStatus", "Off");
                                                        log = "AmplifierPowerOff";
                                                    }

                                                    break;
                                                case 04:
                                                    if (Convert.ToByte(data[9]) == 1)
                                                    {
                                                        objdata.Add("OtherPowerStatus", "On");
                                                        log = "OtherPowerOn";
                                                    }

                                                    else
                                                    {
                                                        objdata.Add("OtherPowerStatus", "Off");
                                                        log = "OtherPowerOff";
                                                    }

                                                    break;
                                            }
                                            break;
                                        case 02:
                                            switch (Convert.ToByte(data[8]))
                                            {
                                                case 07:
                                                    objdata.Add("SudentAudio", "On");
                                                    log = "StudentAudio";
                                                    break;
                                                case 08:
                                                    objdata.Add("TeacherAudio", "On");
                                                    log = "TeacherAudio";
                                                    break;
                                            }
                                            break;
                                        case 06:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            break;
                                        case 07:

                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            break;
                                        case 08:

                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            break;
                                    }
                                }
                                else
                                {
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            case 08:
                                statdata.Add("Type", "MacAddress");
                                string macadd = HexEncoding.ToStringfromHex(new byte[] { data[7], data[8], data[9], data[10], data[11], data[12] });
                                //var temo =HexEncoding.GetBytes(macadd,out int discard);

                                objdata.Add("MacAddress", macadd);
                                break;
                            case 09:
                                {
                                    //8B B9 00 07 05 09 C4 02 00 4F 2A
                                    statdata.Add("Type", "Strategy");
                                    int strategyID = (data[8] << 8) | data[9];
                                    statdata.Add("StrategyId", strategyID);
                                    MessageArray[1] = "StrategyInstruction";
                                    if (data[6] == Convert.ToByte(0xc4))
                                    {
                                        statdata.Add("InstructionStatus", "Success");
                                    }
                                    else
                                    {
                                        statdata.Add("InstructionStatus", "Fail");
                                    }
                                    switch (Convert.ToByte(data[7]))
                                        {
                                            case 01:
                                                objdata.Add("WorkStatus", "Closed");
                                                statdata.Add("Device", "CloseStrategy");
                                                log = "SystemOff";
                                                break;
                                            case 02:
                                                objdata.Add("WorkStatus", "Open");
                                                statdata.Add("Device", "SystemOn");
                                                log = "SystemOn";
                                                break;
                                            case 03:
                                                objdata.Add("ProjectorStatus", "On");
                                                statdata.Add("Device", "ProjectorOn");
                                                log = "projectorOn";
                                                break;
                                            case 04:
                                                objdata.Add("PcStatus", "On");
                                                statdata.Add("Device", "ComputerOn");
                                                log = "ComputerOn";
                                                break;
                                            case 05:
                                                objdata.Add("Sound", "On");
                                                statdata.Add("Device", "SoundOn");
                                                log = "Volume";
                                                break;
                                            case 06:
                                                objdata.Add("ElectricPort", "On");
                                                statdata.Add("Device", "PortPowerOn");
                                                log = "PortPowerOn";
                                                break;

                                            case 44:
                                                objdata.Add("IsSystemLock", "True");
                                                statdata.Add("Device", "PanelOff");
                                                log = "SystemLock";
                                                break;
                                            case 45:
                                                objdata.Add("IsSystemLock", "False");
                                                statdata.Add("Device", "PanelOn");
                                                log = "SystemUnlock";
                                                break;
                                            case 86:
                                                objdata.Add("Screen", "Down");
                                                statdata.Add("Device", "ScreenOn");
                                                log = "ScreenDown";
                                                break;
                                            case 118:
                                                objdata.Add("Screen", "Up");
                                                statdata.Add("Device", "ScreenOff");
                                                log = "ScreenRise";
                                                break;
                                    }
                                }
                                break;
                            case 10:
                                {
                                    //8B B9 00 07 05 09 C4 02 00 4F 2A
                                    statdata.Add("Type", "Reservation");
                                    MessageArray[1] = "ReserveInstruction";
                                    switch (Convert.ToByte(data[7]))
                                    {
                                        case 01:
                                            objdata.Add("WorkStatus", "Closed");
                                            statdata.Add("Device", "CloseReserve");
                                            log = "SystemOff";
                                            break;
                                        case 02:
                                            objdata.Add("WorkStatus", "Open");
                                            statdata.Add("Device", "SystemOn");
                                            log = "SystemOn";
                                            break;
                                        case 03:
                                            objdata.Add("ProjectorStatus", "On");
                                            statdata.Add("Device", "ProjectorOn");
                                            log = "projectorOn";
                                            break;
                                        case 04:
                                            objdata.Add("PcStatus", "On");
                                            statdata.Add("Device", "ComputerOn");
                                            log = "ComputerOn";
                                            break;
                                        case 05:
                                            objdata.Add("Sound", "On");
                                            statdata.Add("Device", "SoundOn");
                                            log = "Volume";
                                            break;
                                        case 06:
                                            objdata.Add("ElectricPort", "On");
                                            statdata.Add("Device", "PortPowerOn");
                                            log = "PortPowerOn";
                                            break;

                                        case 44:
                                            objdata.Add("IsSystemLock", "True");
                                            statdata.Add("Device", "PanelOff");
                                            log = "SystemLock";
                                            break;
                                        case 45:
                                            objdata.Add("IsSystemLock", "False");
                                            statdata.Add("Device", "PanelOn");
                                            log = "SystemUnlock";
                                            break;
                                        case 86:
                                            objdata.Add("Screen", "Down");
                                            statdata.Add("Device", "ScreenOn");
                                            log = "ScreenDown";
                                            break;
                                        case 118:
                                            objdata.Add("Screen", "Up");
                                            statdata.Add("Device", "ScreenOff");
                                            log = "ScreenRise";
                                            break;
                                    }
                                    int reserveID = (data[8] << 8) | data[9];
                                    statdata.Add("ReserveId", reserveID);

                                }
                                break;
                            case 11:
                                statdata.Add("Type", "Heartbeat");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    var tempbits = Convert.ToString(data[9], 2).PadLeft(4, '0');
                                    if (tempbits[3] == '1')
                                        objdata.Add("ProjectorPowerStatus", "On");
                                    else
                                        objdata.Add("ProjectorPowerStatus", "Off");
                                    if (Convert.ToByte(tempbits[2]) == '1')
                                        objdata.Add("ComputerPowerStatus", "On");
                                    else
                                        objdata.Add("ComputerPowerStatus", "Off");

                                    if (Convert.ToByte(tempbits[1]) == '1')
                                        objdata.Add("AmplifierPowerStatus", "On");
                                    else
                                        objdata.Add("AmplifierPowerStatus", "Off");
                                    if (Convert.ToByte(tempbits[0]) == '1')
                                        objdata.Add("OtherPowerStatus", "On");
                                    else
                                        objdata.Add("OtherPowerStatus", "Off");

                                    var statbits = Convert.ToString(data[7], 2).PadLeft(8, '0');
                                    var systembit = statbits.Substring(0, 2);
                                    if (systembit == "00")
                                        objdata.Add("WorkStatus", "Closed");
                                    else if (systembit == "01")
                                        objdata.Add("WorkStatus", "Opening");
                                    else if (systembit == "10") objdata.Add("WorkStatus", "Open");
                                    else objdata.Add("WorkStaus", "Closing");

                                    if (statbits[2] == '1') objdata.Add("IsSystemLock", "False");
                                    else objdata.Add("IsSystemLock", "True");

                                    //class lock status
                                    if (statbits[3] == '0')
                                    {
                                        objdata.Add("IsClassLock", "True");

                                    }
                                    else
                                    {
                                        objdata.Add("IsClassLock", "False");
                                    }

                                    //podium lock status
                                    if (statbits[4] == '0')
                                    {
                                        objdata.Add("IsPodiumLock", "True");
                                    }
                                    else
                                    {
                                        objdata.Add("IsPodiumLock", "False");
                                    }
                                    if (statbits[5] == '1') objdata.Add("PcStatus", "On");
                                    else objdata.Add("PcStatus", "Off");
                                    if (statbits[6] == '1') objdata.Add("ProjectorStatus", "On");
                                    else objdata.Add("ProjectorStatus", "Off");
                                    if (statbits[7] == '1') objdata.Add("LightStatus", "On");
                                    else objdata.Add("LightStatus", "Off");

                                    var tempbit1 = Convert.ToString(data[8], 2).PadLeft(8, '0');
                                    var screenbit = tempbit1.Substring(0, 2);
                                    if (screenbit == "00") objdata.Add("Screen", "Stop");
                                    else if (screenbit == "01") objdata.Add("Screen", "Up");
                                    else if (screenbit == "10") objdata.Add("Screen", "Down");
                                    var curtainbit = tempbit1.Substring(2, 2);
                                    if (curtainbit == "00") objdata.Add("Curtain", "Stop");
                                    else if (curtainbit == "01") objdata.Add("Curtain", "Open");
                                    else if (curtainbit == "10") objdata.Add("Curtain", "Close");
                                    var mediabit = tempbit1.Substring(4, 4);
                                    switch (mediabit)
                                    {
                                        case "0001":
                                            objdata.Add("MediaSignal", "Desktop");
                                            // MessageArray[11] = "台式电脑";//desktop
                                            break;
                                        case "0010":
                                            objdata.Add("MediaSignal", "Laptop");
                                            // MessageArray[11] = "手提电脑";//laptop
                                            break;
                                        case "0011":
                                            objdata.Add("MediaSignal", "DigitalBooth");
                                            // MessageArray[11] = "数码展台";//digital booth(curtain)
                                            break;
                                        case "0100":
                                            objdata.Add("MediaSignal", "DigitalEquipment");
                                            // MessageArray[11] = "数码设备";//digital equipment(Screen)
                                            break;
                                        case "0101":
                                            objdata.Add("MediaSignal", "Dvd");
                                            // MessageArray[11] = "DVD";//dvd
                                            break;
                                        case "0110":
                                            objdata.Add("MediaSignal", "BluRayDvd");
                                            // MessageArray[11] = "蓝光DVD";//"Blu-Ray DVD"
                                            break;
                                        case "0111":
                                            objdata.Add("MediaSignal", "Tv");
                                            // MessageArray[11] = "电视机"; //TV
                                            break;
                                        case "1000":
                                            objdata.Add("MediaSignal", "VideoCamera");
                                            // MessageArray[11] = "摄像机";//Video Camera
                                            break;
                                        case "1001":
                                            objdata.Add("MediaSignal", "RecordingSystem");
                                            // MessageArray[11] = "录播"; //Recording System
                                            break;
                                        default:
                                            objdata.Add("MediaSignal", "None");
                                            MessageArray[11] = "无信号"; //No system
                                            break;
                                    }
                                    objdata.Add("Volume", data[10].ToString());// main volume
                                    objdata.Add("WiredMicVolume", data[11].ToString());//wired mic volume
                                    objdata.Add("WirelessMicVolume", data[12].ToString());// wireless mic volume
                                }
                                else
                                {
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            default:
                                break;



                        }
                        statdata.Add("Data", objdata);
                        statdata.Add("Log", log);

                    }
                    else if (data[4] == Convert.ToByte(0x06))
                    {
                        if (data[6] == Convert.ToByte(0xc4))
                        {
                            switch (data[5])
                            {
                                case 1:
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
                                    break;
                                case 5:
                                    break;
                                case 6:
                                    break;
                                case 7:
                                    break;
                                default:
                                    break;
                            }

                        }
                        else
                        {
                            statdata.Add("InstructionStatus", "Fail");
                            MessageArray[1] = "Unsuccessful";
                        }

                    }
                    else
                    {
                        if (data[6] == Convert.ToByte(0xc4))
                        {
                            statdata.Add("InstructionStatus", "Success");
                            switch (data[5])
                            {
                                case 1:
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
                                    break;
                                case 5:
                                    break;
                                case 6:
                                    break;
                                case 7:
                                    break;
                                default:
                                    break;
                            }

                        }
                        else
                        {
                            statdata.Add("InstructionStatus", "Fail");
                            MessageArray[1] = "Unsuccessful";
                        }
                    }


                }
                //}
                //Console.WriteLine(" received byte " + received[8]);
                //Console.WriteLine(" ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(received.Length + "  " + data.Length);
                received.ToList().ForEach(x => Console.Write(" " + x));
                data.ToList().ForEach(x => Console.Write(" " + x));
                Console.WriteLine(ex.StackTrace);
            }
            //SaveStatus1(ip, MessageArray);
            //  result.Add(MessageArray.ToList());
            result.Add("data", statdata);
            result.Add("status", Status.ToList());
            return result;
        }

        private void KeyValueIndicator(string[] messageArray, byte[] data)
        {
            keyCodes.Add(10, "DeskTop");
        }

        public Dictionary<string, object> OfflineMessage()
        {

            Dictionary<string, object> result = new Dictionary<string, object>
            {
                { "Type", "MachineStatus" },
                { "InstructionStatus", "Fail" }
            };
            Dictionary<string, string> objdata = new Dictionary<string, string> { { "Status", "Offline" } };
            result.Add("Data", objdata);
            return result;
        }
        ////        public static void SaveStatus1(string ip, string[] status)
        ////        {
        ////            if (status[1] == "Heartbeat")
        ////            {
        ////                string s = "";
        ////                if (status[2] == "在线")
        ////                    s = "Online";
        ////                else
        ////                    s = "Offline";
        ////                string t = "";
        ////                if (status[3] == "运行中")
        ////                    t = "OPEN";
        ////                else if (status[3] == "待机")
        ////                    t = "CLOSED";
        ////                string u = "";
        ////                if (status[5] == "已关机")
        ////                    u = "Off";
        ////                else if (status[5] == "已开机")
        ////                    u = "On";

        ////                MySqlConnection con = new MySqlConnection(constr);
        ////                using (MySqlCommand cmd = new MySqlCommand("sp_UpdateStatus", con))
        ////                {
        ////                    cmd.CommandType = CommandType.StoredProcedure;
        ////                    cmd.Parameters.AddWithValue("@ip", ip);

        ////                    cmd.Parameters.AddWithValue("@mstat", s);

        ////                    cmd.Parameters.AddWithValue("@wstat", t);

        ////                    cmd.Parameters.AddWithValue("@cstat", u);

        ////                    try
        ////                    {
        ////                        if (con.State != ConnectionState.Open)
        ////                        {
        ////                            con.Open();
        ////                        }
        ////                        cmd.ExecuteNonQuery();
        ////                    }
        ////#pragma warning disable CS0168 // The variable 'ex' is declared but never used
        ////                    catch (Exception ex)
        ////#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        ////                    {
        ////                        // Console.WriteLine(ex.Message);
        ////                    }
        ////                    finally
        ////                    {
        ////                        con.Close();
        ////                    }
        ////                }
        ////            }
        ////        }
    }
}
