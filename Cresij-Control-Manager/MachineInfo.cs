using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    /// <summary>
    /// 继电器的状态
    /// </summary>
    public enum RelayState
    {
        Open,
        Close
    }
    /// <summary>
    /// 视频输入选择
    /// </summary>
    public enum VGAInput
    {
        VGA1, VGA2, VGA3, VGA4
    }

    /// <summary>
    /// 音频输入选择MIC或LINE
    /// </summary>
    public enum AudioLine
    {
        MIC1, MIC2, LINE
    }

    public enum SerioParam
    {
        BaudRate, DataBits, StopBits, Parity
    }
    /// <summary>
    /// 返回值信息
    /// </summary>
    public struct ResultHex
    {
        public bool isAvail;
        public string AvailHex;
        public string ReplyHex;
        public int HexLen;

    }
    /// <summary>
    /// 命令类型，包括发送的命令，返回的命令，及ID号
    /// </summary>
    public struct CommStru
    {
        public int ID;
        public string SendCommand;
        public string ReplyCommand;
    }

    /// <summary>
    /// 网络搜索主机参数
    /// </summary>
    public struct MachineParam
    {
        public string IPAddress;
        public string Mac;
        public string MachineID;
        public string SubMask;
        public string NetGate;
        public string MachineType;
        public int LocalPort;
        public int RemotePort;
        public DateTime ReplayTime;
        public bool Online;
    }

    /// <summary>
    /// 设置电源的参数
    /// </summary>
    public struct PowerParam
    {
        public bool Warning { set; get; }
        public int MaximalVoltage { set; get; }

        public int MinimumVoltage { set; get; }

        public int MaximalCurrent { set; get; }

        public int MinimumCurrent { set; get; }

    }

    /// <summary>
    /// 时序电源的信息
    /// </summary>
    public struct PowerPort
    {
        public RelayState States { set; get; }
        public float Voltage { set; get; }
        public float Current { set; get; }

        public float Power { set; get; }

        public float TotalPower { set; get; }

        public float Discharge { set; get; }
        //单位伏特
        public int MaximalVoltage { set; get; }
        //单位伏特
        public int MinimumVoltage { set; get; }
        //单位安培
        public int MaximalCurrent { set; get; }
        //单位安培
        public int MinimumCurrent { set; get; }

        public bool Warning { set; get; }
    }

    public class MachineInfo
    {
        public MachineInfo(string FormatStr)
        {
            string[] StatesList = FormatStr.Split(' ');
            int L = StatesList.Length;
            if (L == 216)
            {
                MachineID = Transformation.HextoInt(StatesList[1] + StatesList[2] + StatesList[3]).ToString() + "-" + Transformation.HextoInt(StatesList[4] + StatesList[5]).ToString();
                ///电源的开关状态
                RefrshPowerState(StatesList[6], StatesList[7], StatesList[15], StatesList[16]);
                ///主机部分的信息
                RefreshState(StatesList[8], StatesList[9]);



                LockCardAllCount = Transformation.HextoInt(StatesList[10] + StatesList[11]);
                Temperature = Transformation.HextoInt(StatesList[12]);
                AudioMICVolumns = Transformation.HextoInt(StatesList[13]);
                AudioLineVolumns = Transformation.HextoInt(StatesList[14]);
                //17,18预留
                //D19D20D21当前正在使用时序中控的用户编号
                UserID = Transformation.HextoInt(StatesList[19] + StatesList[20] + StatesList[21]);
                //00(卡长) 00 00 00 00 00 00 00 00 00 00 00 00当前正在使用时序中控的用户的卡号D22-D34
                string[] temp = new string[15];
                for (int i = 22; i <= 34; i++)
                {
                    temp[i - 22] = StatesList[i];
                }
                UseingCardID = CardID(temp);
                //D35-D37投影机时间
                ProjectorTime = Transformation.HextoInt(StatesList[35] + StatesList[36] + StatesList[37]);
                ClassTime = Transformation.HextoInt(StatesList[38] + StatesList[39]);
                int BaseStart = 50;
                float zg = 0;
                for (int j = 0; j < 11; j++)
                {
                    float _temp = Transformation.HextoInt(StatesList[BaseStart + j * 14] + StatesList[BaseStart + j * 14 + 1]) / 100;
                    double V = Math.Round(_temp, 1);
                    //电压值
                    PowerPortList[j].Voltage = (float)V;
                    //电流值
                    _temp = Transformation.HextoInt(StatesList[BaseStart + j * 14 + 2] + StatesList[BaseStart + j * 14 + 3]) / 100;
                    V = Math.Round(_temp, 1);
                    PowerPortList[j].Current = (float)V;
                    //功率值
                    _temp = Transformation.HextoInt(StatesList[BaseStart + j * 14 + 4] + StatesList[BaseStart + j * 14 + 5]) / 100;
                    V = Math.Round(_temp, 1);
                    PowerPortList[j].Power = (float)V;
                    //电量
                    _temp = Transformation.HextoInt(StatesList[BaseStart + j * 14 + 6] + StatesList[BaseStart + j * 14 + 7] + StatesList[BaseStart + j * 14 + 8] + StatesList[BaseStart + j * 14 + 9]) / 3200;
                    V = Math.Round(_temp, 2);
                    zg += _temp;
                    PowerPortList[j].TotalPower = (float)V;
                    _temp = Transformation.HextoInt(StatesList[BaseStart + j * 14 + 10] + StatesList[BaseStart + j * 14 + 11] + StatesList[BaseStart + j * 14 + 12] + StatesList[BaseStart + j * 14 + 13]) / 1000;
                    V = Math.Round(_temp, 2);
                    PowerPortList[j].Discharge = (float)V;
                }
                TotalPower = zg;

            }
        }

        /// <summary>
        /// 组织用户卡号
        /// </summary>
        /// <param name="CardList"></param>
        /// <returns></returns>
        private string CardID(string[] CardList)
        {
            int l = Transformation.HextoInt(CardList[0]);
            if (l > CardList.Length) l = CardList.Length - 1;
            string _Result = "";
            for (int i = 0; i < l; i++)
            {
                _Result += " " + CardList[i + 1];
            }
            return _Result.Trim();
        }




        /// <summary>
        /// 十一部电源的状态
        /// </summary>
        /// <param name="HightHex"></param>
        /// <param name="LowHex"></param>
        private void RefrshPowerState(string HightHex, string LowHex, string WarningHigh, string WarningLow)
        {
            string BinaryStr = Transformation.HexToBin(HightHex) + Transformation.HexToBin(LowHex);
            string BinaryWarn = Transformation.HexToBin(WarningHigh) + Transformation.HexToBin(WarningLow);
            for (int i = 10; i >= 0; i--)
            {
                if (BinaryStr[i].ToString() == "1")
                    PowerPortList[i].States = RelayState.Open;
                else
                    PowerPortList[i].States = RelayState.Close;
                if (BinaryWarn == "1")
                    PowerPortList[i].Warning = false;
                else
                    PowerPortList[i].Warning = true;
            }
        }


        /// <summary>
        /// 根据两个字节的数据分析主机的状态信息
        /// </summary>
        /// <param name="HightHex"></param>
        /// <param name="LowHex"></param>
        private void RefreshState(string HightHex, string LowHex)
        {
            string BinaryHigh = Transformation.HexToBin(HightHex);
            string BinaryLow = Transformation.HexToBin(LowHex);
            for (int i = 0; i < 8; i++)
            {
                string Hchar = BinaryHigh[i].ToString();
                string Lchar = BinaryLow[i].ToString();
                switch (i)
                {
                    case 0:
                        if (Hchar == "1")
                        {
                            ClassState = RelayState.Open;
                        }
                        else
                            ClassState = RelayState.Close;
                        if (Lchar == "1") MagnetismLock = RelayState.Open; else MagnetismLock = RelayState.Close;
                        break;
                    case 1:
                        if (Lchar == "1") LockState = RelayState.Open; else LockState = RelayState.Close;
                        break;
                    case 2:
                        if (Hchar == "1") BackLight = RelayState.Open; else BackLight = RelayState.Close;
                        break;
                    case 3:
                        if (Hchar == "1") FrontLight = RelayState.Open; else FrontLight = RelayState.Close;
                        break;
                    case 4:
                        if (Hchar == "1") ComputerState = RelayState.Open; else ComputerState = RelayState.Close;
                        break;
                    case 5:
                        if (Hchar == "1") Curtains = RelayState.Open; else Curtains = RelayState.Close;
                        break;
                    case 6:
                        if (Hchar == "1") ElectricCurtains = RelayState.Open; else ElectricCurtains = RelayState.Close;
                        break;
                    case 7:
                        if (Hchar == "1") ProjectorState = RelayState.Open; else ProjectorState = RelayState.Close;
                        break;

                }



                ///分析音视频输入
                switch (BinaryLow[5].ToString() + BinaryLow[6].ToString())
                {
                    case "01":
                        VideoState = VGAInput.VGA1;
                        break;
                    case "10":
                        VideoState = VGAInput.VGA2;
                        break;
                    case "11":
                        VideoState = VGAInput.VGA3;
                        break;
                }
                switch (BinaryLow[3].ToString() + BinaryLow[4].ToString())
                {
                    case "01":
                        AudioInput = AudioLine.MIC1;
                        break;
                    case "10":
                        AudioInput = AudioLine.MIC2;
                        break;
                    case "11":
                        AudioInput = AudioLine.LINE;
                        break;
                }
            }

        }



        #region 主机状态信息
        /// <summary>
        /// 机器序列号
        /// </summary>
        public string MachineID
        {
            set; get;
        }
        /// <summary>
        /// 前排灯
        /// </summary>
        public RelayState FrontLight
        {
            set; get;
        }
        /// <summary>
        /// 后排灯
        /// </summary>
        public RelayState BackLight
        {
            set; get;
        }
        /// <summary>
        /// 窗帘
        /// </summary>
        public RelayState Curtains
        {
            set; get;
        }


        public float TotalPower
        {
            set; get;
        }
        /// <summary>
        /// 电脑幕布
        /// </summary>
        public RelayState ElectricCurtains
        {
            set; get;
        }/// <summary>
         /// 电脑主机
         /// </summary>
        public RelayState ComputerState
        {
            set; get;
        }
        /// <summary>
        /// 视频输入
        /// </summary>
        public VGAInput VideoState
        {
            set; get;
        }
        /// <summary>
        /// 上课的状态
        /// </summary>
        public RelayState ClassState
        {
            set; get;
        }
        /// <summary>
        /// 上课的时间
        /// </summary>
        public int ClassTime
        {
            set; get;
        }
        /// <summary>
        /// 音频输入口
        /// </summary>
        public AudioLine AudioInput
        {
            set; get;
        }
        /// <summary>
        /// MIC音量0-85db
        /// </summary>
        public int AudioMICVolumns
        {
            set; get;
        }
        /// <summary>
        ///线路输入音量
        /// </summary>
        public int AudioLineVolumns
        {
            set; get;
        }
        /// <summary>
        /// 投影机的状态
        /// </summary>
        public RelayState ProjectorState
        {
            set; get;
        }
        /// <summary>
        /// 投影机的开机时间
        /// </summary>
        public int ProjectorTime
        {
            set; get;
        }
        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature
        {
            set; get;
        }


        #endregion
        #region 智能锁信息
        /// <summary>
        /// 智能锁可以容纳卡的总量
        /// </summary>
        public int LockCardAllCount
        {
            set; get;
        }

        /// <summary>
        ///锁电池电压
        /// </summary>
        public float Voltage
        {
            set; get;
        }
        /// <summary>
        /// 卡用户数
        /// </summary>
        public int CardUserCount
        {
            set; get;
        }
        /// <summary>
        /// 密码用户
        /// </summary>
        public int PasswordUserCount
        {
            set; get;
        }
        /// <summary>
        /// 磁力锁
        /// </summary>
        public RelayState MagnetismLock
        {
            set; get;
        }
        /// <summary>
        /// 当前使用的卡号
        /// </summary>
        public string UseingCardID
        {
            set; get;
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            set; get;
        }
        /// <summary>
        /// 智能门锁的状态
        /// </summary>
        public RelayState LockState
        {
            set; get;
        }

        #endregion
        #region 时序状态
        public PowerPort[] PowerPortList = new PowerPort[11];

        #endregion
    }
}
