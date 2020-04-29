using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

using System.Configuration;

namespace Cresij_Control_Manager
{
  class Decode
  {
    public static readonly string constr = ConfigurationManager.ConnectionStrings["CresijCamConnectionString"].ConnectionString;
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
    public List<List<string>> Decoded(string ip, byte[] received, string[] Status)
    {
      List<List<string>> result = new List<List<string>>();
      byte[] data = null;
      string[] MessageArray = null;
      int length = 0;
      try
      {
        if (received[0] == Convert.ToByte(0x8B) && received[1] == Convert.ToByte(0xB9))
        {
          //Status[0] = ip;
          //for (int i=1; i<7;i++) {
          //    Status[i] = "Off";
          //}
          int high = 256 * received[2];
          int low = received[3];
          length = 4 + high + low;
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

          //int checksum = -2;
          //for (int i = 2; i < length - 1; i++)
          //{
          //    checksum = checksum + Convert.ToByte(data[i]);
          //}
          //checksum = checksum & Convert.ToByte(0xFF);

          //if (checksum == data[length - 1]) 
          //{
          if (data[4] == Convert.ToByte(0x01))
          {
            MessageArray[0] = "Reader";
            if (data[6] == Convert.ToByte(0xc4))
            {
              switch (data[5])
              {
                case 1:

                  MessageArray[1] = "registered";
                  MessageArray[2] = data[7].ToString();
                  byte[] cardbytes = new byte[4];
                  for (int i = 7; i <= 10; i++)
                  {
                    cardbytes[i - 7] = data[i];
                    //MessageArray[2] = MessageArray[2] +" "+ data[i];
                  }
                  MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes);

                  break;
                case 2:
                  break;
                case 3:
                  break;
                case 4:
                  MessageArray[1] = "Toregister";
                  MessageArray[2] = data[7].ToString();
                  byte[] cardbytes1 = new byte[4];
                  for (int i = 7; i <= 10; i++)
                  {
                    cardbytes1[i - 7] = data[i];
                    //MessageArray[2] = MessageArray[2] +" "+ data[i];
                  }
                  MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes1);
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
                  MessageArray[1] = "readerlog";
                  MessageArray[2] = data[7].ToString();
                  byte[] cardbytes3 = new byte[4];
                  for (int i = 7; i <= 10; i++)
                  {
                    cardbytes3[i - 7] = data[i];
                  }
                  MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes3);
                  break;
                case 12:
                  MessageArray[1] = "readerlog";
                  MessageArray[2] = data[7].ToString();
                  byte[] cardbytes4 = new byte[4];
                  for (int i = 7; i <= 10; i++)
                  {
                    cardbytes4[i - 7] = data[i];
                  }
                  MessageArray[2] = HexEncoding.ToStringfromHEx(cardbytes4);
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
            }
            else
            {
              MessageArray[1] = "Unsuccessful";
            }
          }
          else if (data[4] == Convert.ToByte(0x02))
          {
            MessageArray[0] = "PanelControl";
            if (data[6] == Convert.ToByte(0xc4))
            {
              switch (data[5])
              {
                case 04:
                  MessageArray[1] = "KeyValue";
                  switch (Convert.ToByte(data[7]))
                  {
                    case 01:
                      MessageArray[2] = "SystemON";
                      Status[5] = "On";
                      break;
                    case 02:
                      MessageArray[2] = "SystemOff";
                      Status[5] = "Off";
                      break;

                    case 86:
                      MessageArray[2] = "DSDown";
                      break;
                    case 102:
                      MessageArray[2] = "DSStop";
                      break;
                    case 118:
                      MessageArray[2] = "DSUp";
                      break;
                    case 44:
                      MessageArray[2] = "syslock";
                      break;
                    case 45:
                      MessageArray[2] = "sysunlock";
                      break;
                    case 46:
                      MessageArray[2] = "podiumlock";
                      break;
                    case 47:
                      MessageArray[2] = "podiumunlock";
                      break;
                    case 95:
                      MessageArray[2] = "classlock";
                      break;
                    case 96:
                      MessageArray[2] = "classunlock";
                      break;
                    case 32:
                      MessageArray[2] = "volplus";
                      break;
                    case 33:
                      MessageArray[2] = "volminus";
                      break;
                    case 34:
                      MessageArray[2] = "mute";
                      break;
                    case 35:
                      MessageArray[2] = "wiredvolplus";
                      break;
                    case 36:
                      MessageArray[2] = "wiredvolminus";
                      break;
                    case 37:
                      MessageArray[2] = "wiredmute";
                      break;
                    case 115:
                      MessageArray[2] = "wirelessvolplus";
                      break;
                    case 116:
                      MessageArray[2] = "wirelessvolminus";
                      break;
                    case 117:
                      MessageArray[2] = "wirelessmute";
                      break;
                    case 146:
                      MessageArray[2] = "startrec";
                      break;
                    case 147:
                      MessageArray[2] = "stoprec";
                      break;
                    case 48:
                      MessageArray[2] = "playdvd";
                      break;
                    case 49:
                      MessageArray[2] = "warehousedvd";
                      break;
                    case 54:
                      MessageArray[2] = "powerdvd";
                      break;
                    case 55:
                      MessageArray[2] = "pausedvd";
                      break;
                    case 56:
                      MessageArray[2] = "stopdvd";
                      break;
                    case 50:
                      MessageArray[2] = "forwarddvd";
                      break;
                    case 64:
                      MessageArray[2] = "rewinddvd";
                      break;
                    case 65:
                      MessageArray[2] = "previousdvd";
                      break;
                    case 66:
                      MessageArray[2] = "nextdvd";
                      break;
                    case 160:
                      MessageArray[2] = "powertv";
                      break;
                    case 161:
                      MessageArray[2] = "tvsignal";
                      break;
                    case 162:
                      MessageArray[2] = "channelplustv";
                      break;
                    case 163:
                      MessageArray[2] = "channelminustv";
                      break;
                    case 164:
                      MessageArray[2] = "volplustv";
                      break;
                    case 165:
                      MessageArray[2] = "volminustv";
                      break;
                    case 166:
                      MessageArray[2] = "menutv";
                      break;
                    case 167:
                      MessageArray[2] = "oktv";
                      break;
                    case 168:
                      MessageArray[2] = "exittv";
                      break;
                    case 51:
                      MessageArray[2] = "projopen";
                      Status[1] = "On";
                      break;
                    case 67:
                      MessageArray[2] = "projoff";
                      Status[1] = "Off";
                      break;
                    case 52:
                      MessageArray[2] = "hdmi";
                      break;
                    case 53:
                      MessageArray[2] = "video";
                      break;
                    case 68:
                      MessageArray[2] = "vga";
                      break;
                    case 69:
                      MessageArray[2] = "sleep";
                      break;
                    case 119:
                      MessageArray[2] = "curtain1open";
                      break;
                    case 87:
                      MessageArray[2] = "curtain1close";
                      break;
                    case 103:
                      MessageArray[2] = "curtain1stop";
                      break;
                    case 99:
                      MessageArray[2] = "curtain2open";
                      break;
                    case 100:
                      MessageArray[2] = "curtain2close";
                      break;
                    case 101:
                      MessageArray[2] = "curtain2stop";
                      break;
                    case 70:
                      MessageArray[2] = "curtain3open";
                      break;
                    case 71:
                      MessageArray[2] = "curtain3close";
                      break;
                    case 72:
                      MessageArray[2] = "curtain3stop";
                      break;
                    case 73:
                      MessageArray[2] = "curtain4open";
                      break;
                    case 74:
                      MessageArray[2] = "curtain4close";
                      break;
                    case 75:
                      MessageArray[2] = "curtain4stop";
                      break;
                    case 120:
                      MessageArray[2] = "light1";
                      break;
                    case 104:
                      MessageArray[2] = "light2";
                      break;
                    case 88:
                      MessageArray[2] = "light3";
                      break;
                    case 83:
                      MessageArray[2] = "light4";
                      break;
                    case 84:
                      MessageArray[2] = "light5";
                      break;
                    case 85:
                      MessageArray[2] = "light6";
                      break;
                    case 76:
                      MessageArray[2] = "light7";
                      break;
                    case 77:
                      MessageArray[2] = "light8";
                      break;
                    case 176:
                      MessageArray[2] = "playbludvd";
                      break;
                    case 177:
                      MessageArray[2] = "warehousebludvd";
                      break;
                    case 178:
                      MessageArray[2] = "powerbludvd";
                      break;
                    case 179:
                      MessageArray[2] = "pausebludvd";
                      break;
                    case 180:
                      MessageArray[2] = "stopbludvd";
                      break;
                    case 181:
                      MessageArray[2] = "forwardbludvd";
                      break;
                    case 182:
                      MessageArray[2] = "rewindbludvd";
                      break;
                    case 183:
                      MessageArray[2] = "previousbludvd";
                      break;
                    case 184:
                      MessageArray[2] = "nextbludvd";
                      break;
                    default:
                      MessageArray[2] = "Nochange";
                      break;
                  }
                  break;
                case 05:
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
                    MessageArray[2] = "SystemSwitchOn";
                  }
                  else
                  {
                    MessageArray[2] = "SystemSwitchOff";
                  }
                  if ((p & 128) == 128)
                  {
                    MessageArray[4] = "Computer";
                  }
                  else
                  {
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
                          MessageArray[3] = "Desktop";
                          break;
                        case 2:
                          MessageArray[3] = "Laptop";
                          break;
                        case 4:
                          MessageArray[3] = "DigitalCurtain";
                          break;
                        case 8:
                          MessageArray[3] = "DigitalScreen";
                          break;
                        case 16:
                          MessageArray[3] = "DVD";
                          break;
                        case 32:
                          MessageArray[3] = "TV";
                          break;
                        case 64:
                          MessageArray[3] = "VideoCamera";
                          break;

                        case 512:
                          MessageArray[3] = "RecordingSystem";
                          break;
                        case 1024:
                          MessageArray[3] = "Blu-RayDVD";
                          break;
                        case 2048:
                          MessageArray[3] = "ExternalHD";
                          break;
                        case 4096:
                          MessageArray[5] = "CentralLock";
                          break;
                        case 8192:
                          MessageArray[6] = "PodiumLock";
                          break;
                        case 16384:
                          MessageArray[7] = "ClassLock";
                          break;
                      }
                    }
                  }
                  if ((p & 4096) == 4096)
                  {
                    MessageArray[5] = "CentralLock";
                  }
                  else
                  {
                    MessageArray[5] = "CentralLockoff";
                  }
                  if ((p & 8192) == 8192)
                  {
                    MessageArray[6] = "PodiumLock";
                  }
                  else
                  {
                    MessageArray[6] = "PodiumLockoff";
                  }

                  if ((p & 16384) == 16384)
                  {
                    MessageArray[7] = "ClassLock";
                  }
                  else
                  {
                    MessageArray[7] = "ClassLockoff";
                  }
                  //}
                  break;
                default:
                  break;
              }
            }
            else
            {
              MessageArray[1] = "Unsuccessful";
            }
          }
          else if (data[4] == Convert.ToByte(0x03))
          {
            if (data[5] == 64)
            {
              if (data[6] == Convert.ToByte(0xc4))
                MessageArray[1] = "MacSuccess";
              else
              {
                MessageArray[1] = "MacFailure";
              }
            }
            if (data[6] == Convert.ToByte(0xc4))
            {
              MessageArray[0] = "config";
              switch (data[5])
              {
                case 1:
                  break;
                case 2:
                  MessageArray[1] = data[7].ToString();
                  MessageArray[2] = data[8].ToString();
                  MessageArray[3] = data[9].ToString();
                  MessageArray[4] = data[10].ToString();
                  MessageArray[5] = data[11].ToString();
                  MessageArray[6] = data[12].ToString();
                  MessageArray[7] = data[13].ToString();
                  MessageArray[8] = data[14].ToString();
                  MessageArray[9] = data[15].ToString();
                  MessageArray[10] = data[16].ToString();
                  MessageArray[11] = data[17].ToString();
                  MessageArray[12] = data[18].ToString();
                  MessageArray[13] = data[19].ToString();
                  MessageArray[14] = data[20].ToString();
                  MessageArray[15] = data[21].ToString();
                  MessageArray[16] = data[22].ToString();
                  MessageArray[17] = data[23].ToString();
                  MessageArray[18] = data[24].ToString();
                  MessageArray[19] = data[25].ToString();
                  MessageArray[20] = data[26].ToString();
                  MessageArray[21] = data[27].ToString();
                  MessageArray[22] = data[28].ToString();
                  MessageArray[23] = data[29].ToString();
                  MessageArray[24] = data[30].ToString();
                  MessageArray[25] = data[31].ToString();
                  MessageArray[26] = data[32].ToString();
                  MessageArray[27] = data[33].ToString();
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
              MessageArray[1] = "Unsuccessful";
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
            MessageArray[0] = "NetworkControl";
            switch (data[5])
            {
              case 01:
                MessageArray[1] = "Heartbeat";
                if (data[6] == Convert.ToByte(0xc4))
                {
                  //machine status
                  MessageArray[2] = "在线";
                  //work status
                  if (data[8] == Convert.ToByte(0x00))
                  {
                    MessageArray[3] = "待机";//CLOSED
                    Status[5] = "Off";
                  }
                  else
                  {
                    MessageArray[3] = "运行中";//OPEN
                    Status[5] = "On";
                  }
                  MessageArray[4] = "--";//timer service

                  //pc status
                  if (data[7] == Convert.ToByte(0x00))
                  {
                    MessageArray[5] = "已关机";//Off
                    Status[2] = "Off";
                  }
                  else
                  {
                    MessageArray[5] = "已开机";//On
                    Status[2] = "On";
                  }
                  //projector status
                  if (data[13] == Convert.ToByte(0x00))
                  {
                    MessageArray[6] = "已关机";
                    Status[1] = "Off";
                  }
                  else
                  {
                    MessageArray[6] = "已开机";
                    Status[1] = "On";
                  }
                  MessageArray[7] = "--";//projector hours
                                         //Curtain Status
                  switch (Convert.ToInt32(data[15]))
                  {
                    case 1:
                      MessageArray[8] = "开";//Open
                      break;
                    case 2:
                      MessageArray[8] = "关";//Close
                      break;
                    case 0:
                      MessageArray[8] = "停";//Stop
                      break;
                  }
                  //Screen status
                  switch (Convert.ToInt32(data[14]))
                  {
                    case 1:
                      MessageArray[9] = "升";//Up
                      Status[6] = "Off";
                      break;
                    case 2:
                      MessageArray[9] = "降";//Down
                      Status[6] = "On";
                      break;
                    case 0:
                      MessageArray[9] = "停";//Stop
                      Status[6] = "Off";
                      break;
                  }
                  //light status
                  if (data[16] == Convert.ToByte(0x00))
                  {
                    MessageArray[10] = "关";//Off
                  }
                  else
                    MessageArray[10] = "开";//On
                                           //media signal
                  switch (Convert.ToInt32(data[11]))
                  {
                    case 1:
                      MessageArray[11] = "台式电脑";//desktop
                      break;
                    case 2:
                      MessageArray[11] = "手提电脑";//laptop
                      break;
                    case 3:
                      MessageArray[11] = "数码展台";//digital booth(curtain)
                      break;
                    case 4:
                      MessageArray[11] = "数码设备";//digital equipment(Screen)
                      break;
                    case 5:
                      MessageArray[11] = "DVD";//dvd
                      break;
                    case 6:
                      MessageArray[11] = "蓝光DVD";//"Blu-Ray DVD"
                      break;
                    case 7:
                      MessageArray[11] = "电视机"; //TV
                      break;
                    case 8:
                      MessageArray[11] = "摄像机";//Video Camera
                      break;
                    case 9:
                      MessageArray[11] = "录播"; //Recording System
                      break;
                    default:
                      MessageArray[11] = "无信号"; //No system
                      break;
                  }
                  //system lock status
                  if (data[12] == Convert.ToByte(0x00))
                  {
                    MessageArray[12] = "解锁";//unlocked
                  }
                  else
                    MessageArray[12] = "锁定";//locked
                                            //class lock status
                  if (data[10] == Convert.ToByte(0x00))
                  {
                    MessageArray[13] = "解锁";//unlocked
                  }
                  else
                    MessageArray[13] = "锁定";//locked
                                            //podium lock status
                  if (data[9] == Convert.ToByte(0x00))
                  {
                    MessageArray[14] = "解锁";//unlocked
                  }
                  else
                    MessageArray[14] = "锁定";//locked
                  MessageArray[15] = "--";
                  MessageArray[16] = "--";
                  MessageArray[17] = "--";
                  MessageArray[18] = "--";
                }
                else
                {
                  MessageArray[2] = "离线";//Offline
                                         // Status[0] = "Offline";
                  for (int i = 3; i < MessageArray.Length; i++)
                  {
                    MessageArray[i] = "--";
                  }
                }
                break;
              case 02:
                MessageArray[1] = "PanelKey";
                if (data[6] == 23)
                {
                  MessageArray[2] = "PCON";
                  Status[2] = "On";
                }
                else if (data[6] == 25)
                {
                  MessageArray[2] = "PCOFF";
                  Status[2] = "Off";
                }
                break;
              case 04:
                break;
              default:
                break;
            }

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
              MessageArray[1] = "Unsuccessful";
            }

          }
          else
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
        Console.WriteLine(ex.Message);
      }
      //SaveStatus1(ip, MessageArray);
      result.Add(MessageArray.ToList());
      result.Add(Status.ToList());
      return result;
    }

    private void KeyValueIndicator(string[] messageArray, byte[] data)
    {
      keyCodes.Add(10, "DeskTop");
    }

    public string offlineMessage()
    {
      string[] m = new string[19];
      m[0] = "--";
      m[1] = "--";
      m[2] = "Offline";
      string data = m[0] + "," + m[1] + "," + m[2];
      for (int i = 3; i < m.Length; i++)
      {
        data = data + "," + "--";
      }
      return data;
    }
    public static void SaveStatus1(string ip, string[] status)
    {
      if (status[1] == "Heartbeat")
      {
        string s = "";
        if (status[2] == "在线")
          s = "Online";
        else
          s = "Offline";
        string t = "";
        if (status[3] == "运行中")
          t = "OPEN";
        else if (status[3] == "待机")
          t = "CLOSED";
        string u = "";
        if (status[5] == "已关机")
          u = "Off";
        else if (status[5] == "已开机")
          u = "On";

        MySqlConnection con = new MySqlConnection(constr);
        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateStatus", con))
        {
          cmd.CommandType = CommandType.StoredProcedure;
          cmd.Parameters.AddWithValue("@ip", ip);

          cmd.Parameters.AddWithValue("@mstat", s);

          cmd.Parameters.AddWithValue("@wstat", t);

          cmd.Parameters.AddWithValue("@cstat", u);

          try
          {
            if (con.State != ConnectionState.Open)
            {
              con.Open();
            }
            cmd.ExecuteNonQuery();
          }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
          catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
          {
            // Console.WriteLine(ex.Message);
          }
          finally
          {
            con.Close();
          }
        }
      }
    }
  }
}
