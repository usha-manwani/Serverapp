using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cresij_Control_Manager
{
    class Models
    {
    }
    public class Pools
    {
        public string _ip;
        public Socket _sock;
        public Thread _thr;
        public DateTime _dt;
        public Pools(string ip, Socket sock, Thread thr, DateTime dt)
        {
            _ip = ip;
            _sock = sock;
            _thr = thr;
            _dt = dt;
        }
        public void Destory()
        {
            _sock = null;
            _thr = null;
        }
    }

    public class MachineState
    {
        public string IPAddress;
        public bool Isline = false;
        public bool ClassOn = false;
        public int VGA = 1;
        public bool Curtains = false;
        public bool ProjectionScreen = false;
        public bool FrontLight = false;
        public bool BackLight = false;
        public bool Projection = false;
        public bool Computer = false;
        public bool IsChange = false;
        public bool Lock = false;
        public string OldState = "";
        public long Tick = 0;
        public DateTime OldTime;
        public MachineState(string _IP, string StringState)
        {
            //如果返回的值不同，则更新当前的状态
            if (OldState != StringState)
            {
                OldState = StringState;
                IsChange = true;
            }
            Isline = true;
            OldTime = DateTime.Now;
            Tick = 0;
            IPAddress = _IP;
            string BinaryChar = Transformation.HexToBin(StringState.Split(' ')[0]);
            if (BinaryChar.Substring(0, 1) == "1") ClassOn = true; else ClassOn = false;
            if (BinaryChar.Substring(1, 1) == "1") Lock = true; else Lock = false;
            if (BinaryChar.Substring(2, 1) == "1") BackLight = true; else BackLight = false;
            if (BinaryChar.Substring(3, 1) == "1") FrontLight = true; else FrontLight = false;
            if (BinaryChar.Substring(4, 1) == "1") Computer = true; else Computer = false;
            if (BinaryChar.Substring(5, 1) == "1") Curtains = true; else Curtains = false;
            if (BinaryChar.Substring(6, 1) == "1") ProjectionScreen = true; else ProjectionScreen = false;
            if (BinaryChar.Substring(6, 1) == "1") Projection = true; else Projection = false;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(IPAddress);
            if (Isline)
                str.Append(" 在线");
            else
                str.Append(" 离线");
            if (ClassOn)
                str.Append("，上课");
            else
                str.Append("，下课");
            if (Curtains)
                str.Append("，窗帘收起");
            else
                str.Append("，窗帘下降");
            if (Curtains)
                str.Append("，投影幕收起");
            else
                str.Append("，投影幕下降");
            if (Computer)
                str.Append("，主机启动");
            else
                str.Append("，主机关机");
            if (Projection)
                str.Append("，投影开");
            else
                str.Append("，投影关");
            if (ProjectionScreen)
                str.Append("，投影幕降");
            else
                str.Append("，投影幕升");
            str.Append("，VGA" + VGA.ToString());
            if (FrontLight)
                str.Append("，前排灯开");
            else
                str.Append("，前排灯关");
            str.Append("，更新时间mm:" + Tick.ToString());
            return str.ToString();
        }
    }
}
