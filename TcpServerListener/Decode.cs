// ***********************************************************************
// Assembly         : TcpServerListener
// Author           : admin
// Created          : 04-02-2021
//
// Last Modified By : admin
// Last Modified On : 04-14-2021
// ***********************************************************************
// <copyright file="Decode.cs" company="">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;

using MySql.Data.MySqlClient;

using System.Configuration;

namespace TcpServerListener
{
    /// <summary>
    /// Class Decode.
    /// </summary>
    class Decode
    {
         /// <summary>
        /// The key codes
        /// </summary>
        Dictionary<byte, string> keyCodes = new Dictionary<byte, string>();
        /// <summary>
        /// Decode the response bytes into string key value format
        /// </summary>
        /// <param name="macaddress">The mac address of machine.</param>
        /// <param name="received">The received bytes.</param>
        /// <param name="Status">The status of devices.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.
        /// Response in form of key value pair string</returns>
        public Dictionary<string, object> Decoded(string macaddress, byte[] received, string[] Status)
        {
            Dictionary<string, object> statdata = new Dictionary<string, object>();
            Dictionary<string, string> objdata = new Dictionary<string, string>();
            Dictionary<string, object> result = new Dictionary<string, object>();
            byte[] data = null;
            
            string log = "";
            int length = 0;
            try
            {
                ///this line is to confirm the data is in the correct format
                if (received[0] == Convert.ToByte(0x8B) && received[1] == Convert.ToByte(0xB9))
                {
                    
                    length = 4 + (256 * received[2]) + received[3];
                    data = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = received[i];
                    }

                   

                    if (data[4] == Convert.ToByte(0x01))
                    {
                        statdata.Add("Command", "Reader");

                       
                        if (data[6] == Convert.ToByte(0xc4))
                        {
                            
                            statdata.Add("InstructionStatus", "Success");
                            switch (data[5])
                            {
                                case 1:
                                    statdata.Add("Type", "CardRegister");
                                   
                                    byte[] cardbytes = new byte[4];
                                    int count = 0;
                                    for(int k=7; k< data.Length - 1;)
                                    {
                                        count += 1;
                                        cardbytes[0] = data[k + 3]; 
                                        cardbytes[1] = data[k + 2]; 
                                        cardbytes[2] = data[k + 1];
                                        cardbytes[3] = data[k];
                                        objdata.Add("CardValue" + count, HexEncoding.ToStringfromHEx(cardbytes));
                                        k += 4;
                                    }
                                    //for (int i = 7; i <= 10; i++)
                                    //{
                                    //    cardbytes[i - 7] = data[i];
                                    //    //MessageArray[2] = MessageArray[2] +" "+ data[i];
                                    //}
                                    //objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes));
                                    statdata.Add("Log", "CardRegister");
                                    

                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
                                    statdata.Add("Type", "NewCardRegister");

                                   
                                    byte[] cardbytes1 = new byte[4];
                                    for (int i = 10; i >= 7; i--)
                                    {
                                        cardbytes1[10-i] = data[i];
                                        //MessageArray[2] = MessageArray[2] +" "+ data[i];
                                    }
                                   
                                    objdata.Add("CardValue", HexEncoding.ToStringfromHEx(cardbytes1));
                                    statdata.Add("Log", "NewCardRegister");
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
                                  
                                    byte[] cardbytes3 = new byte[4];
                                    for (int i = 10; i >= 7; i--)
                                    {
                                        cardbytes3[10 - i] = data[i];
                                    }
                                    var tempcardid1 = HexEncoding.ToStringfromHEx(cardbytes3);
                                    objdata.Add("CardValue", tempcardid1);
                                    statdata.Add("Log", tempcardid1);
                                    break;
                                case 12:
                                    statdata.Add("Type", "ReaderLogOff");
                                   
                                    byte[] cardbytes4 = new byte[4];
                                    for (int i = 10; i >= 7; i--)
                                    {
                                        cardbytes4[10 - i] = data[i];
                                    }
                                    var tempcardid = HexEncoding.ToStringfromHEx(cardbytes4);
                                    objdata.Add("CardValue", tempcardid);
                                    statdata.Add("Log", tempcardid);
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
                            
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x02))
                    {

                        statdata.Add("Command", "PanelControl");
                        
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
                                            
                                            Status[5] = "On";
                                            break;
                                        case 193:
                                            objdata.Add("WorkStatus", "Close");
                                            log = "SystemOff";
                                           
                                            Status[5] = "Off";
                                            break;

                                        case 86:
                                            objdata.Add("Screen", "Down");
                                            log = "ScreenDown";
                                           
                                            break;
                                        case 102:
                                            objdata.Add("Screen", "Stop");
                                            log = "ScreenStop";
                                            
                                            break;
                                        case 118:
                                            objdata.Add("Screen", "Up");
                                            log = "ScreenRise";
                                            
                                            break;
                                        case 44:
                                            objdata.Add("IsSystemLock", "True");
                                            log = "SystemLock";
                                           
                                            break;
                                        case 45:
                                            objdata.Add("IsSystemLock", "False");
                                            log = "SystemUnlock";
                                           
                                            break;
                                        case 46:
                                            objdata.Add("IsPodiumLock", "True");
                                            log = "PodiumLock";
                                            
                                            break;
                                        case 47:
                                            objdata.Add("IsPodiumLock", "False");
                                            log = "PodiumUnlock";
                                            
                                            break;
                                        case 95:
                                            objdata.Add("IsClassLock", "True");
                                            log = "ClassLock";
                                           
                                            break;
                                        case 96:
                                            objdata.Add("IsClassLock", "False");
                                            log = "ClassUnlock";
                                           
                                            break;
                                        case 32:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                          
                                            break;
                                        case 33:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            
                                            break;
                                        case 34:
                                            objdata.Add("Volume", data[8].ToString());
                                            log = "Volume";
                                            
                                            break;
                                        case 35:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                           
                                            break;
                                        case 36:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            
                                            break;
                                        case 37:
                                            objdata.Add("WiredMicVolume", data[8].ToString());
                                            log = "WiredMicVolume";
                                            
                                            break;
                                        case 115:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                           
                                            break;
                                        case 116:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            
                                            break;
                                        case 117:
                                            objdata.Add("WirelessMicVolume", data[8].ToString());
                                            log = "WirelessMicVolume";
                                            
                                            break;
                                        case 146:
                                            objdata.Add("Recording", "Start");
                                            
                                            break;
                                        case 147:
                                            objdata.Add("Recording", "Stop");
                                            
                                            break;
                                        case 48:
                                            objdata.Add("DVD", "Play");
                                           
                                            break;
                                        case 49:
                                            objdata.Add("DVD", "WareHouse");
                                            
                                            break;
                                        case 54:
                                            objdata.Add("DVD", "Power");
                                            
                                            break;
                                        case 55:
                                            objdata.Add("DVD", "Pause");
                                          
                                            break;
                                        case 56:
                                            objdata.Add("DVD", "Stop");
                                           
                                            break;
                                        case 50:
                                            objdata.Add("DVD", "Forward");
                                           
                                            break;
                                        case 64:
                                            objdata.Add("DVD", "Rewind");
                                           
                                            break;
                                        case 65:
                                            objdata.Add("DVD", "Previous");
                                            
                                            break;
                                        case 66:
                                            objdata.Add("DVD", "Next");
                                            
                                            break;
                                        case 160:
                                            objdata.Add("TV", "Power");
                                           
                                            break;
                                        case 161:
                                            objdata.Add("TV", "");
                                            
                                            break;
                                        case 162:
                                            objdata.Add("TV", "ChannelPlus");
                                            
                                            break;
                                        case 163:
                                            objdata.Add("TV", "ChannelMinus");
                                            
                                            break;
                                        case 164:
                                            objdata.Add("TV", "VolumePlus");
                                            
                                            break;
                                        case 165:
                                            objdata.Add("TV", "VolumeMinus");
                                            
                                            break;
                                        case 166:
                                            objdata.Add("TV", "Menu");
                                           
                                            break;
                                        case 167:
                                            objdata.Add("TV", "Ok");
                                            
                                            break;
                                        case 168:
                                            objdata.Add("TV", "Exit");
                                           
                                            break;
                                        case 51:
                                            objdata.Add("ProjectorStatus", "On");
                                            log = "ProjectorOn";
                                            
                                            Status[1] = "On";
                                            break;
                                        case 67:
                                            objdata.Add("ProjectorStatus", "Off");
                                            log = "ProjectorOff";
                                            
                                            Status[1] = "Off";
                                            break;
                                        case 52:
                                            objdata.Add("Projector", "Hdmi");
                                            log = "ProjectorHDMI";
                                            
                                            break;
                                        case 53:
                                            objdata.Add("Projector", "Video");
                                            log = "ProjectorVideo";
                                            
                                            break;
                                        case 68:
                                            objdata.Add("Projector", "Vga");
                                            log = "ProjectorVGA";
                                            
                                            break;
                                        case 69:
                                            statdata.Add("Projector", "Sleep");
                                            log = "ProjectorSleep";
                                            
                                            break;
                                        case 119:
                                            objdata.Add("Curtain1", "Open");
                                            log = "CurtainOpen";
                                            
                                            break;
                                        case 87:
                                            objdata.Add("Curtain1", "Close");
                                            log = "CurtainClose";
                                            
                                            break;
                                        case 103:
                                            objdata.Add("Curtain1", "Stop");
                                            log = "CurtainStop";
                                            
                                            break;
                                        case 99:
                                            objdata.Add("Curtain2", "Open");
                                            
                                            break;
                                        case 100:
                                            objdata.Add("Curtain2", "Close");
                                            
                                            break;
                                        case 101:
                                            objdata.Add("Curtain2", "Stop");
                                            
                                            break;
                                        case 70:
                                            objdata.Add("Curtain3", "Open");
                                            
                                            break;
                                        case 71:
                                            objdata.Add("Curtain3", "Close");
                                            
                                            break;
                                        case 72:
                                            objdata.Add("Curtain3", "Stop");
                                            
                                            break;
                                        case 73:
                                            objdata.Add("Curtain4", "Open");
                                            
                                            break;
                                        case 74:
                                            objdata.Add("Curtain4", "Close");
                                            
                                            break;
                                        case 75:
                                            objdata.Add("Curtain4", "Stop");
                                            
                                            break;
                                        case 120:
                                            objdata.Add("Light", "light1");
                                           
                                            break;
                                        case 104:
                                            objdata.Add("Light", "light2");
                                            
                                            break;
                                        case 88:
                                            objdata.Add("Light", "light3");
                                           
                                            break;
                                        case 83:
                                            objdata.Add("Light", "light4");
                                            
                                            break;
                                        case 84:
                                            objdata.Add("Light", "light5");
                                           
                                            break;
                                        case 85:
                                            objdata.Add("Light", "light6");
                                            
                                            break;
                                        case 76:
                                            objdata.Add("Light", "light7");
                                            
                                            break;
                                        case 77:
                                            objdata.Add("Light", "light8");
                                            
                                            break;
                                        case 176:
                                            objdata.Add("BlueRayDVD", "Play");
                                            
                                            break;
                                        case 177:
                                            objdata.Add("BlueRayDVD", "Warehouse");
                                           
                                            break;
                                        case 178:
                                            objdata.Add("BlueRayDVD", "Power");
                                           
                                            break;
                                        case 179:
                                            objdata.Add("BlueRayDVD", "Pause");
                                            
                                            break;
                                        case 180:
                                            objdata.Add("BlueRayDVD", "Stop");
                                           
                                            break;
                                        case 181:
                                            objdata.Add("BlueRayDVD", "Forward");
                                            
                                            break;
                                        case 182:
                                            objdata.Add("BlueRayDVD", "Rewind");
                                            
                                            break;
                                        case 183:
                                            objdata.Add("BlueRayDVD", "Previous");
                                            
                                            break;
                                        case 184:
                                            objdata.Add("BlueRayDVD", "Next");
                                           
                                            break;

                                        default:
                                            objdata.Add("NoData", "NoData");
                                            
                                            break;
                                    }
                                    statdata.Add("Data", objdata);
                                    statdata.Add("Log", log);
                                    break;

                                case 05:
                                    statdata.Add("Type", "LedIndicator");
                                    int re = -1;
                                    int p = 256 * data[7] + data[8];
                                    if ((p & 256) == 256)
                                    {
                                        objdata.Add("WorkStatus", "Open");
                                        
                                    }
                                    else
                                    {
                                        objdata.Add("WorkStatus", "Closed");
                                        
                                    }
                                    if ((p & 128) == 128)
                                    {
                                        objdata.Add("PcStatus", "On");
                                       
                                    }
                                    else
                                    {
                                        objdata.Add("PcStatus", "Off");
                                        
                                    }
                                    // int p = Convert.ToInt32(r); 
                                    int[] compare = new int[] { 1, 2, 4, 8, 16, 32, 64, 512, 1024, 2048,
                                            4096, 8192, 16384 };
                                    for (int i = 0; i < compare.Length; i++)
                                    {
                                        re = p & compare[i];
                                        if (re == compare[i])
                                        {
                                          
                                            switch (re)
                                            {
                                                case 1:
                                                    objdata.Add("MediaSignal", "Desktop");
                                                    
                                                    break;
                                                case 2:
                                                    objdata.Add("MediaSignal", "Laptop");
                                                    
                                                    break;
                                                case 4:
                                                    objdata.Add("MediaSignal", "DigitalBooth");
                                                    
                                                    break;
                                                case 8:
                                                    objdata.Add("MediaSignal", "DigitalEquipment");
                                                    
                                                    break;
                                                case 16:
                                                    objdata.Add("MediaSignal", "DVD");
                                                   
                                                    break;
                                                case 32:
                                                    objdata.Add("MediaSignal", "TV");
                                                    
                                                    break;
                                                case 64:
                                                    objdata.Add("MediaSignal", "VideoCamera");
                                                    
                                                    break;

                                                case 512:
                                                    objdata.Add("MediaSignal", "RecordingSystem");
                                                    
                                                    break;
                                                case 1024:
                                                    objdata.Add("MediaSignal", "BluRayDVD");
                                                    
                                                    break;
                                                case 2048:
                                                    objdata.Add("MediaSignal", "ExternalHD");
                                                    
                                                    break;
                                                case 4096:
                                                    objdata.Add("IsSystemLock", "");
                                                   
                                                    break;
                                                case 8192:
                                                    objdata.Add("IsPodiumLock", "");
                                                   
                                                    break;
                                                case 16384:
                                                    objdata.Add("IsClassLock", "");
                                                    
                                                    break;
                                            }
                                        }
                                    }
                                    if ((p & 4096) == 4096)
                                    {
                                        objdata.Add("IsSystemLock", "True");
                                       
                                    }
                                    else
                                    {
                                        objdata.Add("IsSystemLock", "False");
                                        
                                    }
                                    if ((p & 8192) == 8192)
                                    {
                                        objdata.Add("IsPodiumLock", "True");
                                        
                                    }
                                    else
                                    {
                                        objdata.Add("IsPodiumLock", "False");
                                        
                                    }

                                    if ((p & 16384) == 16384)
                                    {
                                        statdata.Add("IsClassLock", "True");
                                        
                                    }
                                    else
                                    {
                                        statdata.Add("IsClassLock", "False");
                                       
                                    }
                                    statdata.Add("Data", objdata);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            statdata.Add("InstructionStatus", "Fail");
                          
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x03))
                    {
                        statdata.Add("Command", "ProjectorConfig");
                        
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
                            case 17:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjectorOnCode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjectorOnCode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                
                                break;
                            case 18:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjectorOffCode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjectorOffCode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            case 19:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjComCode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjComCode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                
                                break;
                            case 20:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjVideoCode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjVideoCode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }

                                break;
                            case 21:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjHDMICode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjHDMICode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }

                                break;
                            case 22:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("ProjSleepCode", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("ProjSleepCode", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }

                                break;
                            case 23:
                                statdata.Add("Type", "SetConfig");
                                statdata.Add("Log", "SetProjectorConfig");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    objdata.Add("SetBaudRate", "True");
                                    statdata.Add("Data", objdata);

                                }
                                else
                                {
                                    objdata.Add("SetBaudRate", "False");
                                    statdata.Add("Data", objdata);
                                    statdata.Add("InstructionStatus", "Fail");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x04))
                    {
                        if (data[6] == Convert.ToByte(0xc9))
                        {
                           
                        }
                        else
                        {
                            
                        }
                    }
                    else if (data[4] == Convert.ToByte(0x05))
                    {
                        statdata.Add("Command", "NetworkControl");
                        
                        switch (data[5])
                        {
                            case 01:
                                statdata.Add("Type", "Heartbeat");
                                if (data[6] == Convert.ToByte(0xc4))
                                {
                                    statdata.Add("InstructionStatus", "Success");
                                    {
                                        
                                        //work status
                                        if (data[8] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("WorkStatus", "Closed");
                                            Status[5] = "Off";
                                        }
                                        else
                                        {
                                            objdata.Add("WorkStatus", "Open");
                                            Status[5] = "On";
                                        }
                                        //system lock status
                                        if (data[12] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsSystemLock", "True");//locked
                                            
                                        }
                                        else
                                        {
                                            objdata.Add("IsSystemLock", "False");
                                            //unlocked
                                        }

                                        //class lock status
                                        if (data[9] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsClassLock", "True");
                                            //locked
                                        }
                                        else
                                        {
                                            objdata.Add("IsClassLock", "False");
                                            //unlocked
                                        }

                                        //podium lock status
                                        if (data[10] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("IsPodiumLock", "True");
                                            //locked
                                        }
                                        else
                                        {
                                            objdata.Add("IsPodiumLock", "False");
                                            //unlocked

                                        }

                                        objdata.Add("Volume", data[19].ToString());// main volume
                                        objdata.Add("WiredMicVolume", data[20].ToString());//wired mic volume
                                        objdata.Add("WirelessMicVolume", data[21].ToString());// wireless mic volume
                                                                                              //media signal
                                        switch (Convert.ToInt32(data[11]))
                                        {
                                            case 1:
                                                objdata.Add("MediaSignal", "Desktop");
                                                
                                                break;
                                            case 2:
                                                objdata.Add("MediaSignal", "Laptop");
                                               
                                                break;
                                            case 3:
                                                objdata.Add("MediaSignal", "DigitalBooth");
                                                break;
                                            case 4:
                                                objdata.Add("MediaSignal", "DigitalEquipment");
                                                break;
                                            case 5:
                                                objdata.Add("MediaSignal", "Dvd");
                                                break;
                                            case 6:
                                                objdata.Add("MediaSignal", "BluRayDvd");
                                                break;
                                            case 7:
                                                objdata.Add("MediaSignal", "Tv");
                                                break;
                                            case 8:
                                                objdata.Add("MediaSignal", "VideoCamera");
                                                break;
                                            case 9:
                                                objdata.Add("MediaSignal", "RecordingSystem");
                                                break;
                                            default:
                                                objdata.Add("MediaSignal", "None");
                                                break;
                                        }                                        

                                        //pc status
                                        if (data[7] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("PcStatus", "Off");
                                            Status[2] = "Off";
                                        }
                                        else
                                        {
                                            objdata.Add("PcStatus", "On");
                                            Status[2] = "On";
                                        }
                                        //projector status
                                        if (data[13] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("ProjectorStatus", "Off");
                                            Status[1] = "Off";
                                        }
                                        else
                                        {
                                            objdata.Add("ProjectorStatus", "On");
                                            Status[1] = "On";
                                        }
                                        
                                        //Curtain Status窗帘
                                        switch (Convert.ToInt32(data[15]))
                                        {
                                            case 1:
                                                //Open
                                                objdata.Add("Curtain", "Open");
                                                break;
                                            case 2:
                                                objdata.Add("Curtain", "Close");
                                                //Close
                                                break;
                                            case 0:
                                                objdata.Add("Curtain", "Stop");
                                                //Stop
                                                break;
                                        }
                                        //Screen status屏幕
                                        switch (Convert.ToInt32(data[14]))
                                        {
                                            case 1:

                                                //Rise Up
                                                objdata.Add("Screen", "Up");
                                                Status[6] = "Off";
                                                break;
                                            case 2:
                                                //Down
                                                objdata.Add("Screen", "Down");
                                                Status[6] = "On";
                                                break;
                                            case 0:
                                               
                                                objdata.Add("Screen", "Stop");
                                                Status[6] = "Off";
                                                break;
                                        }
                                        //light status
                                        if (data[16] == Convert.ToByte(0x00))
                                        {
                                            objdata.Add("LightStatus", "Off");
                                            
                                        }
                                        else
                                        {
                                            objdata.Add("LightStatus", "On");
                                            
                                        }

                                        objdata.Add("Temperature", data[17].ToString());

                                        objdata.Add("Humidity", data[18].ToString());

                                        objdata.Add("Voltage", data[22].ToString());
                                        
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
                                            break;
                                        case 17:
                                            objdata.Add("MediaSignal", "Laptop");
                                            log = "Laptop";
                                            break;
                                        case 18:
                                            //DigitalBooth = DigitalCurtain
                                            objdata.Add("MediaSignal", "DigitalBooth");
                                            log = "DigitalBooth";
                                            break;
                                        case 19:
                                            // DigitalEquipment=DigitalScreen
                                            objdata.Add("MediaSignal", "DigitalEquipment");
                                            log = "DigitalEquipment";
                                            break;
                                        case 20:
                                            objdata.Add("MediaSignal", "DVD");
                                            log = "Dvd";
                                            break;
                                        case 21:
                                            objdata.Add("MediaSignal", "TV");
                                            log = "TV";
                                            break;
                                        case 22:
                                            objdata.Add("MediaSignal", "VideoCamera");
                                            log = "VideoCamera";
                                            break;

                                        case 25:
                                            objdata.Add("MediaSignal", "RecordingSystem");
                                            log = "RecordingDevice";
                                            break;
                                        case 26:
                                            objdata.Add("MediaSignal", "BluRayDVD");
                                            log = "BlurayDvd";
                                            break;
                                        
                                        case 192:

                                            objdata.Add("WorkStatus", "Open");
                                            log = "SystemOn";
                                            break;
                                        case 193:
                                            objdata.Add("WorkStatus", "Closed");
                                            log = "SystemOff";
                                            //Status[5] = "Off";
                                            break;
                                        case 29:

                                            objdata.Add("pcStatus", "On");
                                            log = "ComputerOn";
                                            break;
                                        case 30:

                                            objdata.Add("PcStatus", "Off");
                                            log = "ComputerOff";
                                            
                                            //Status[5] = "Off";
                                            break;
                                        case 86:
                                            objdata.Add("Screen", "Down");
                                            log = "ScreenDown";
                                            break;
                                        case 102:
                                            objdata.Add("Screen", "Stop");
                                            log = "ScreenStop";
                                            break;
                                        case 118:
                                            objdata.Add("Screen", "Up");
                                            log = "ScreenRise";
                                            break;
                                        case 44:
                                            objdata.Add("IsSystemLock", "True");
                                            log = "SystemLock";
                                            break;
                                        case 45:
                                            objdata.Add("IsSystemLock", "False");
                                            log = "SystemUnlock";
                                            break;
                                        case 46:
                                            objdata.Add("IsPodiumLock", "True");
                                            log = "PodiumLock";
                                            break;
                                        case 47:
                                            objdata.Add("IsPodiumLock", "False");
                                            log = "PodiumUnlock";
                                            break;
                                        case 95:
                                            objdata.Add("IsClassLock", "True");
                                            log = "ClassLock";
                                            break;
                                        case 96:
                                            objdata.Add("IsClassLock", "False");
                                            log = "ClassUnlock";
                                            break;
                                        case 32:
                                            objdata.Add("Volume", "Increase");
                                            log = "Volume";
                                            break;
                                        case 33:
                                            objdata.Add("Volume", "Decrease");
                                            log = "Volume";
                                            break;
                                        case 34:
                                            objdata.Add("Volume", "Mute");
                                            log = "Volume";
                                            break;
                                        case 35:
                                            objdata.Add("WiredMicVolume", "Increase");
                                            log = "WiredMicVolume";
                                            break;
                                        case 36:
                                            objdata.Add("WiredMicVolume", "Decrease");
                                            log = "WiredMicVolume";
                                            break;
                                        case 37:
                                            objdata.Add("WiredMicVolume", "Mute");
                                            log = "WiredMicVolume";
                                            break;
                                        case 115:
                                            objdata.Add("WirelessMicVolume", "Increase");
                                            log = "WirlessMicVolume";
                                            break;
                                        case 116:
                                            objdata.Add("WirelessMicVolume", "Decrease");
                                            log = "WirlessMicVolume";
                                            break;
                                        case 117:
                                            objdata.Add("WirelessMicVolume", "Mute");
                                            log = "WirlessMicVolume";
                                            break;
                                        case 146:
                                            objdata.Add("Recording", "Start");
                                            break;
                                        case 147:
                                            objdata.Add("Recording", "Stop");
                                            break;
                                        case 48:
                                            objdata.Add("DVD", "Play");
                                            break;
                                        case 49:
                                            objdata.Add("DVD", "WareHouse");
                                            break;
                                        case 54:
                                            objdata.Add("DVD", "Power");
                                            break;
                                        case 55:
                                            objdata.Add("DVD", "Pause");
                                            break;
                                        case 56:
                                            objdata.Add("DVD", "Stop");
                                            break;
                                        case 50:
                                            objdata.Add("DVD", "Forward");
                                            break;
                                        case 64:
                                            objdata.Add("DVD", "Rewind");
                                            break;
                                        case 65:
                                            objdata.Add("DVD", "Previous");
                                            break;
                                        case 66:
                                            objdata.Add("DVD", "Next");
                                            break;
                                        case 160:
                                            objdata.Add("TV", "Power");
                                            break;
                                        case 161:
                                            objdata.Add("TV", "");
                                            break;
                                        case 162:
                                            objdata.Add("TV", "ChannelPlus");
                                            break;
                                        case 163:
                                            objdata.Add("TV", "ChannelMinus");
                                           break;
                                        case 164:
                                            objdata.Add("TV", "VolumePlus");
                                            break;
                                        case 165:
                                            objdata.Add("TV", "VolumeMinus");
                                            break;
                                        case 166:
                                            objdata.Add("TV", "Menu");
                                            break;
                                        case 167:
                                            objdata.Add("TV", "Ok");
                                            break;
                                        case 168:
                                            objdata.Add("TV", "Exit");
                                            break;
                                        case 51:
                                            objdata.Add("ProjectorStatus", "On");
                                            log = "ProjectorOn";
                                            Status[1] = "On";
                                            break;
                                        case 67:
                                            objdata.Add("ProjectorStatus", "Off");
                                            log = "ProjectorOff";
                                            Status[1] = "Off";
                                            break;
                                        case 52:
                                            objdata.Add("ProjectorStatus", "Hdmi");
                                            log = "ProjectorHDMI";
                                            break;
                                        case 53:
                                            objdata.Add("ProjectorStatus", "Video");
                                            log = "ProjectorVideo";
                                            break;
                                        case 68:
                                            statdata.Add("ProjectorStatus", "Vga");
                                            log = "ProjectorVGA";
                                            break;
                                        case 69:
                                            objdata.Add("ProjectorStatus", "Sleep");
                                            log = "ProjectorSleep";
                                            break;
                                        case 119:
                                            objdata.Add("Curtain1", "Open");
                                            log = "CurtainOpen";
                                            break;
                                        case 87:
                                            objdata.Add("Curtain1", "Close");
                                            log = "CurtainClose";
                                            break;
                                        case 103:
                                            objdata.Add("Curtain1", "Stop");
                                            log = "CurtainStop";
                                            break;
                                        case 99:
                                            objdata.Add("Curtain2", "Open");
                                            break;
                                        case 100:
                                            objdata.Add("Curtain2", "Close");
                                            break;
                                        case 101:
                                            objdata.Add("Curtain2", "Stop");
                                            break;
                                        case 70:
                                            objdata.Add("Curtain3", "Open");
                                            break;
                                        case 71:
                                            objdata.Add("Curtain3", "Close");
                                            break;
                                        case 72:
                                            objdata.Add("Curtain3", "Stop");
                                            break;
                                        case 73:
                                            objdata.Add("Curtain4", "Open");
                                            break;
                                        case 74:
                                            objdata.Add("Curtain4", "Close");
                                            break;
                                        case 75:
                                            objdata.Add("Curtain4", "Stop");
                                            break;
                                        case 120:
                                            objdata.Add("Light", "light1");
                                            break;
                                        case 104:
                                            objdata.Add("Light", "light2");
                                            break;
                                        case 88:
                                            objdata.Add("Light", "light3");
                                            break;
                                        case 83:
                                            objdata.Add("Light", "light4");
                                            break;
                                        case 84:
                                            statdata.Add("Light", "light5");
                                            break;
                                        case 85:
                                            objdata.Add("Light", "light6");
                                            break;
                                        case 76:
                                            objdata.Add("Light", "light7");
                                            break;
                                        case 77:
                                            objdata.Add("Light", "light8");
                                            break;
                                        case 176:
                                            objdata.Add("BlueRayDVD", "Play");
                                            break;
                                        case 177:
                                            objdata.Add("BlueRayDVD", "Warehouse");
                                            break;
                                        case 178:
                                            objdata.Add("BlueRayDVD", "Power");
                                            break;
                                        case 179:
                                            objdata.Add("BlueRayDVD", "Pause");
                                            break;
                                        case 180:
                                            objdata.Add("BlueRayDVD", "Stop");
                                            break;
                                        case 181:
                                            objdata.Add("BlueRayDVD", "Forward");
                                            break;
                                        case 182:
                                            objdata.Add("BlueRayDVD", "Rewind");
                                            break;
                                        case 183:
                                            objdata.Add("BlueRayDVD", "Previous");
                                            break;
                                        case 184:
                                            objdata.Add("BlueRayDVD", "Next");
                                            break;
                                        default:
                                            objdata.Add("NoData", "NoData");
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
                                objdata.Add("MacAddress", macadd);
                                break;
                            case 09:
                                {
                                    //8B B9 00 07 05 09 C4 02 00 4F 2A
                                    statdata.Add("Type", "Strategy");
                                    int strategyID = (data[8] << 8) | data[9];
                                    statdata.Add("StrategyId", strategyID);
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
                            case 11:   //8B B9 00 0A 05 0B C4 A6 A1 0F 5A 5A 3C 24
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
                                    else objdata.Add("WorkStatus", "Closing");

                                    if (statbits[2] == '1') objdata.Add("IsSystemLock", "True");
                                    else objdata.Add("IsSystemLock", "False");

                                    //class lock status
                                    if (statbits[3] == '1')
                                    {
                                        objdata.Add("IsClassLock", "True");

                                    }
                                    else
                                    {
                                        objdata.Add("IsClassLock", "False");
                                    }

                                    //podium lock status
                                    if (statbits[4] == '1')
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
                                            break;
                                        case "0010":
                                            objdata.Add("MediaSignal", "Laptop");
                                            break;
                                        case "0011":
                                            objdata.Add("MediaSignal", "DigitalBooth");
                                            break;
                                        case "0100":
                                            objdata.Add("MediaSignal", "DigitalEquipment");
                                            break;
                                        case "0101":
                                            objdata.Add("MediaSignal", "Dvd");
                                            break;
                                        case "0110":
                                            objdata.Add("MediaSignal", "BluRayDvd");
                                            break;
                                        case "0111":
                                            objdata.Add("MediaSignal", "Tv");
                                            break;
                                        case "1000":
                                            objdata.Add("MediaSignal", "VideoCamera");
                                            break;
                                        case "1001":
                                            objdata.Add("MediaSignal", "RecordingSystem");
                                            break;
                                        default:
                                            objdata.Add("MediaSignal", "None");
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
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(received.Length + "  " + data.Length);
                received.ToList().ForEach(x => Console.Write(" " + x));
                data.ToList().ForEach(x => Console.Write(" " + x));
                Console.WriteLine(ex.StackTrace);
            }
            
            result.Add("data", statdata);
            result.Add("status", Status.ToList());
            return result;
        }

        /// <summary>
        /// Keys the value indicator.
        /// </summary>
        /// <param name="messageArray">The message array.</param>
        /// <param name="data">The data.</param>
        private void KeyValueIndicator(string[] messageArray, byte[] data)
        {
            keyCodes.Add(10, "DeskTop");
        }

        /// <summary>
        /// Offlines the message.
        /// </summary>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
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
