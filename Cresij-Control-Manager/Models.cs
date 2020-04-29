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
    public byte[] buffer;
    private Socket sokConnection;
    private DateTime now;

    public Pools(string ip, Socket sokConnection, DateTime now)
    {
      _ip = ip;
      this.sokConnection = sokConnection;
      this.now = now;
      buffer = new byte[256];
    }

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

  public class WorkingHours
  {
    public string Ipaddress { get; set; }

    public Thread th { get; set; }
    public WorkingHours(string ip)
    {
      Ipaddress = ip;
      SetState();
    }
    Dictionary<string, KeyValuePair<string, DateTime>> Timestatus = new Dictionary<string, KeyValuePair<string, DateTime>>();
    public void SetState()
    {
      Timestatus.Add("Projector", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
      Timestatus.Add("Computer", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
      Timestatus.Add("Recorder", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
      Timestatus.Add("AC", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
      Timestatus.Add("CentralControl", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
      Timestatus.Add("Screen", new KeyValuePair<string, DateTime>("Off", DateTime.Now));
    }
    public void StatusTimeUpdate(string[] data)
    {

      //projector hour
      try
      {
        if (this.Ipaddress == data[0])
        {
          double projectorhour = 0, computerhour = 0, recorderhour = 0, achour = 0, cchour = 0, screenhour = 0;
          Timestatus.TryGetValue("Projector", out KeyValuePair<string, DateTime> val);
          if (val.Key != data[1])
          {
            if (val.Key == "On")
            {
              projectorhour = DateTime.Now.Subtract(val.Value).TotalMinutes;
              val = new KeyValuePair<string, DateTime>(data[1], DateTime.Now);
              Timestatus.Remove("Projector");
              Timestatus.Add("Projector", val);
            }
            else
            {
              val = new KeyValuePair<string, DateTime>(data[1], DateTime.Now);
              Timestatus.Remove("Projector");
              Timestatus.Add("Projector", val);
            }
          }

          //Computer hours
          Timestatus.TryGetValue("Computer", out KeyValuePair<string, DateTime> val1);
          if (val1.Key != data[2])
          {
            if (val1.Key == "On")
            {
              computerhour = DateTime.Now.Subtract(val1.Value).TotalMinutes;
              Timestatus.Remove("Computer");
              val1 = new KeyValuePair<string, DateTime>(data[2], DateTime.Now);
              Timestatus.Add("Computer", val1);
            }
            else
            {
              val1 = new KeyValuePair<string, DateTime>(data[2], DateTime.Now);
              Timestatus.Remove("Computer");
              Timestatus.Add("Computer", val1);
            }
          }

          //recorder hours
          Timestatus.TryGetValue("Recorder", out KeyValuePair<string, DateTime> val2);
          if (val2.Key != data[3])
          {
            if (val2.Key == "On")
            {
              recorderhour = DateTime.Now.Subtract(val2.Value).TotalMinutes;
              val2 = new KeyValuePair<string, DateTime>(data[3], DateTime.Now);
              Timestatus.Remove("Recorder");
              Timestatus.Add("Recorder", val2);
              //Timestatus["Recorder"] = new KeyValuePair<string, DateTime>(data[3], DateTime.Now);
            }
            else
            {
              val2 = new KeyValuePair<string, DateTime>(data[3], DateTime.Now);
              Timestatus.Remove("Recorder");
              Timestatus.Add("Recorder", val2);
            }
          }


          //ac hours
          Timestatus.TryGetValue("AC", out KeyValuePair<string, DateTime> val3);
          if (val3.Key != data[4])
          {
            if (val3.Key == "On")
            {
              achour = DateTime.Now.Subtract(val3.Value).TotalMinutes;
              KeyValuePair<string, DateTime> tt = val3;
              val3 = new KeyValuePair<string, DateTime>(data[4], DateTime.Now);
              Timestatus.Remove("AC");
              Timestatus.Add("AC", val3);
            }
            else
            {
              val3 = new KeyValuePair<string, DateTime>(data[4], DateTime.Now);
              Timestatus.Remove("AC");
              Timestatus.Add("AC", val3);
            }
          }


          //CC hours
          Timestatus.TryGetValue("CentralControl", out KeyValuePair<string, DateTime> val4);
          if (val4.Key != data[5])
          {
            if (val4.Key == "On")
            {
              cchour = DateTime.Now.Subtract(val4.Value).TotalMinutes;
              val4 = new KeyValuePair<string, DateTime>(data[5], DateTime.Now);
              Timestatus.Remove("CentralControl");
              Timestatus.Add("CentralControl", val4);
            }
            else
            {
              val4 = new KeyValuePair<string, DateTime>(data[5], DateTime.Now);
              Timestatus.Remove("CentralControl");
              Timestatus.Add("CentralControl", val4);
            }
          }

          //Screen hours
          Timestatus.TryGetValue("Screen", out KeyValuePair<string, DateTime> val5);
          if (val5.Key != data[6])
          {
            if (val5.Key == "On")
            {
              screenhour = DateTime.Now.Subtract(val5.Value).TotalMinutes;
              val5 = new KeyValuePair<string, DateTime>(data[6], DateTime.Now);
              Timestatus.Remove("Screen");
              Timestatus.Add("Screen", val5);
              //Timestatus["Screen"] = new KeyValuePair<string, DateTime>(data[6], DateTime.Now);

            }
            else
            {
              val5 = new KeyValuePair<string, DateTime>(data[6], DateTime.Now);
              Timestatus.Remove("Screen");
              Timestatus.Add("Screen", val5);
            }
          }
          if (projectorhour > 2 || computerhour > 2 || recorderhour > 2 || achour > 2 || cchour > 2 || screenhour > 2)
          {
            DBHelper.Queries query = new DBHelper.Queries();
            string q = query.InsertWorkingHour(this.Ipaddress, Math.Round(projectorhour / 60, 2),
              Math.Round(computerhour / 60, 2), Math.Round(recorderhour / 60, 2),
                Math.Round(achour / 60, 2), Math.Round(cchour / 60, 2), Math.Round(screenhour / 60, 2));
            DBHelper.GetIPStatus status = new DBHelper.GetIPStatus();

            status.ExecuteAnyCommand(q);
          }

        }
      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {

      }
    }

    public void Destroy()
    {

    }
  }
}
