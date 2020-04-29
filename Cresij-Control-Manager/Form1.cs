using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBHelper;
using Microsoft.AspNet.SignalR.Client;

namespace Cresij_Control_Manager
{
  public partial class Form1 : Form
  {
    List<Pools> Pool_list = new List<Pools>();
    // private static Dictionary<string, Socket> Clients = new Dictionary<string, Socket>();
    private static Dictionary<string, List<string>> currentstatus = new Dictionary<string, List<string>>();
    public static Dictionary<string, DateTime> times = new Dictionary<string, DateTime>();
    public static int connectedClient = 0;
    private static IHubProxy proxy;
    private static HubConnection con;
    delegate void UpdateGrid(Control grid);
    private static System.Threading.Timer _timer;
    private static System.Threading.Timer timer;
    private static System.Threading.Timer statusTimer;
    Dictionary<string, WorkingHours> workingHours = new Dictionary<string, WorkingHours>();
    delegate void UpdateGridCallback(DataTable dt);
    private delegate string[] UpdateGridCallback1(string[] data, string ip);
    delegate void Updatelabel();

    private void OnMinimize()
    {
      this.Visible = false;   // Do your stuff
    }
    protected override void WndProc(ref Message m)
    {
      // Trap WM_SYSCOMMAND, SC_MINIMIZE
      if (m.Msg == 0x112 && m.WParam.ToInt32() == 0xf020)
      {
        OnMinimize();
        return;        // NOTE: delete if you still want the default behavior
      }
      base.WndProc(ref m);
    }

    private const int CP_NOCLOSE_BUTTON = 0x200;
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams myCp = base.CreateParams;
        myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
        return myCp;
      }
    }
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      FillForm(GridData());
      this.Visible = false;
      this.MaximizeBox = false;
      notifyIcon1.BalloonTipText = "Cresij Server App Started";
      notifyIcon1.BalloonTipTitle = "Cresij Server App Notifier";
      notifyIcon1.ShowBalloonTip(2000);
      closeform.Click += Closeform_Click;
      viewdevices.Click += Viewdevices_Click;
      this.ShowInTaskbar = false;
      this.WindowState = FormWindowState.Minimized;
      StartListen(IPAddress.Any.ToString(), 1200);
      ConnectToHub();
      StartTimer();
    }

    private void Viewdevices_Click(object sender, EventArgs e)
    {
      this.Visible = true;
      this.Show();
      this.WindowState = FormWindowState.Normal;

    }

    private void Closeform_Click(object sender, EventArgs e)
    {
      this.Close();
      this.Dispose();
    }

    #region---启用端口，接受下位机连接,监测下位机状态池---
    Thread threadWatch = null; // 负责监听客户端连接请求的 线程;
    Socket socketWatch = null;

    private void StartListen(string _IpAddress, int _Ports)
    {
      // 创建负责监听的套接字，注意其中的参数;
      socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      //初始化的时候，让socket可以进行端口复用,防止服务线程卡死
      socketWatch.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
      // 获得文本框中的IP对象;
      IPAddress address = IPAddress.Parse(_IpAddress);
      // 创建包含ip和端口号的网络节点对象;
      IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _Ports);
      try
      {
        
        // 将负责监听的套接字绑定到唯一的ip和端口上;
        socketWatch.Bind(endPoint);
        //MessageBox.Show("Tcp Connection done ");
      }
      catch (SocketException se)
      {
        //MessageBox.Show("Error in tcp : " + se.Message);
        return;
      }
      // 设置监听队列的长度;  
      socketWatch.Listen(100);
      // 创建负责监听的线程;
      threadWatch = new Thread(WatchConnecting)
      {
        IsBackground = true
      };
      threadWatch.Start();
    }

    void WatchConnecting()
    {
      while (true)  // 持续不断的监听客户端的连接请求;
      {
        try
        {
          // socketWatch.BeginAccept(new AsyncCallback(AcceptCallback), socketWatch);
          Socket sokConnection = socketWatch.Accept();
          
          // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字  
          // 将与客户端连接的 套接字 对象添加到集合中;
          //要判断当前连接是否已经在集合中  
          IPEndPoint iep = (IPEndPoint)sokConnection.RemoteEndPoint;
          string _ip = iep.Address.ToString();
         // MessageBox.Show("machine connected " + _ip);
          ClearSocket(_ip);
          //dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去.
          //if (!Clients.ContainsKey(_ip))
          //{
          //    Clients.Add(_ip, sokConnection);
          //}
          bool ipexists;
          ipexists = CallGridforIp(_ip);
          
          if (ipexists)
          {
            byte[] d = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
            Send(sokConnection, d);
            Thread thr = new Thread(RecMsg)
            {
              IsBackground = true
            };
            SetControl(sokConnection);//设置心跳包
            thr.Start(sokConnection);
            lock (times)
            {
              if (!times.ContainsKey(_ip))
                times.Add(iep.Address.ToString(), DateTime.Now);
            }
            Pools pl = new Pools(_ip, sokConnection, thr, DateTime.Now);
            UpdatePool(pl);

            if (!workingHours.ContainsKey(_ip))
            {
              WorkingHours working = new WorkingHours(_ip);
              workingHours.Add(_ip, working);
            }
          }
        }
        catch (Exception ex) {
          //MessageBox.Show("Error in tcp listening: " + ex.Message);
        }
      }
    }

    delegate void IpExists(string _ip, out bool ipexists);
    private bool CallGridforIp(string _ip)
    {
      bool ipexists = false;
      object[] arguments = new object[] { _ip, null };
      if (this.ipGrid.InvokeRequired)
      {
        IpExists checkexists = new IpExists(CheckIPInGrid);
        this.Invoke
       (checkexists, arguments);
        ipexists = (bool)arguments[1];
      }
      else
      {
        CheckIPInGrid(_ip, out ipexists);
      }

      return ipexists;
    }
    private void CheckIPInGrid(string _ip, out bool ipexists)
    {
      ipexists = false;
      for (int i = 0; i < ipGrid.RowCount; i++)
      {
        if (ipGrid[0, i].Value.ToString() == _ip)
        {
          ipexists = true;
          break;
        }
      }
    }

    private void ClearSocket(string _ip)
    {
      try
      {
        for (int i = Pool_list.Count - 1; i > 0; i--)
        {
          Pools _p = Pool_list[i];
          if (_p != null && _p._ip == _ip)
          {
            Pool_list.RemoveAt(i);
            Application.DoEvents();
            if (_p._sock != null && _p._sock.Connected)
            {
              _p._sock.Shutdown(SocketShutdown.Both);
              _p._sock.Close();
            }
            if (_p._thr != null) _p._thr.Abort();
          }
        }
      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {
        //  Console.WriteLine(ex.Message.ToString());
      }
    }
    private void SetControl(Socket client)
    {
      byte[] inOptionValues = new byte[4 * 3];
      BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
      BitConverter.GetBytes((uint)15000).CopyTo(inOptionValues, 4);
      BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, 8);
      client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
    }
    void RecMsg(object sokConnectionparn)
    {
      try
      {
        if (sokConnectionparn == null) return;
        Socket sokClient = sokConnectionparn as Socket;
        IPEndPoint iep = (IPEndPoint)sokClient.RemoteEndPoint;
        while (true)
        {
          // 定义一个2M的缓存区；  
          byte[] receiveBytes = new byte[256];
          // 将接受到的数据存入到输入  arrMsgRec中；  
          int length = -1;
          try
          {
            length = sokClient.Receive(receiveBytes); // 接收数据，并返回数据的长度； 
            lock (times)
            {
              if (times.ContainsKey(iep.Address.ToString()))
              {
                times[iep.Address.ToString()] = DateTime.Now;
              }
            }
            // Thread.Sleep(50);
            //new Thread(() => { HandleCommand(iep.Address.ToString(), receiveBytes, length); }).Start();                       
            Task.Factory.StartNew(() => HandleCommand(iep.Address.ToString(), receiveBytes, length));
            //  HandleCommand(iep.Address.ToString(), receiveBytes, length);             
            //
            //HandleCommand(iep.Address.ToString(), receiveBytes, length);
          }
#pragma warning disable CS0168 // The variable 'se' is declared but never used
          catch (SocketException se)
#pragma warning restore CS0168 // The variable 'se' is declared but never used
          {
            RemovePool(iep.Address.ToString());
            workingHours.Remove(iep.Address.ToString());
            break;
          }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
          catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
          {
            RemovePool(iep.Address.ToString());
            workingHours.Remove(iep.Address.ToString());
            break;
          }
          //string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串； 
          //将Byte数组解析成字符串并添加到历史记录列表
          //string Command = "";
          //for (int i = 0; i < length; i++)
          //    Command += arrMsgRec[i].ToString("X2") + " ";
        }
      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (ObjectDisposedException ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {
        return;//如果主界面已经退出了，那线程也退出好了
      }
    }
    /// <summary>
    /// 更新连接池
    /// </summary>
    /// <param name="_pls"></param>
    private void UpdatePool(Pools _pls)
    {
      if (!Pool_list.Contains(_pls))
        Pool_list.Add(_pls);
      if (onlinemachinelbl.InvokeRequired)
      {
        Updatelabel updatelabel = new Updatelabel(UpdatemachineLabel);
        this.Invoke(updatelabel);
      }
    }
    //移除实例变量
    private void RemovePool(string _IPKey)
    {
      try
      {
        for (int i = 0; i < Pool_list.Count; i++)
        {
          Pools _x = Pool_list[i];
          if (_x._ip == _IPKey)
          {
            if (_x._sock != null)
            {
              _x._sock.Close();
            }
            Pool_list.Remove(_x);
            if (onlinemachinelbl.InvokeRequired)
            {
              Updatelabel updatelabel = new Updatelabel(UpdatemachineLabel);
              this.Invoke(updatelabel);
            }
            Decode dr = new Decode();
            string m = dr.offlineMessage();
            SendMessage(_IPKey, m);
            dr = null;
            _x.Destory();
            if (_x._thr.IsAlive)
              _x._thr.Abort();
            break;
          }
        }
      }
      catch (Exception ex)
      {
        return;
      }
      finally
      {

      }
    }
    /// <summary>
    /// 检查Socket是否断开连接
    /// </summary>
    private void CheckSocketConnection()
    {
      try
      {
        if (Pool_list.Count > 0)
        {
          for (int i = Pool_list.Count - 1; i >= 0; i--)
          {
            string sendStr = "Server Information";
            byte[] bs = Encoding.ASCII.GetBytes(sendStr);
            if (Pool_list[i]._sock.Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。                                                                                  //   -或 - 如果有数据可供读取，则为 true。-或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
            {
              Pool_list[i]._sock.Close();//关闭socket
              Pool_list[i].Destory();
              Pool_list.RemoveAt(i);//从列表中删除断开的socke
              continue;
            }
          }
        }
      }
      catch (Exception ex) { }
    }

    private static void Send(Socket workSocket, byte[] data)
    {
      try
      {
        workSocket.Send(data);
      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {
      }
    }
    #endregion

    #region--处理下位机信息--
    private int HandleCommand(string iep, byte[] receiveBytes, int length)
    {
      try
      {
        Decode dd = new Decode();
        string[] final;
        string[] status;
        List<List<string>> re = new List<List<string>>();
        for (int j = 0; j < length;)
        {
          if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
          {
            byte[] datatoDecode = new byte[4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3]];
            for (int k = 0; k < datatoDecode.Length; k++)
            {
              datatoDecode[k] = receiveBytes[k + j];
            }
            if (currentstatus.ContainsKey(iep))
            {
              status = currentstatus[iep].ToArray();
            }
            else
            {
              status = new string[7];
              status[0] = iep;
              for (int i = 1; i < 7; i++)
              {
                status[i] = "Off";
              }
            }
            re = dd.Decoded(iep, datatoDecode, Status: status);
            dd = null;
            if (re.Count == 2)
            {
              status = re[1].ToArray();
              if (currentstatus.ContainsKey(iep))
              {
                currentstatus[iep] = status.ToList();
              }
              else
              {
                currentstatus.Add(iep, status.ToList());
              }
              final = re[0].ToArray();
              final = UpdateipGridOnHeartBeat(final, iep);
              string data = "";
              for (int l = 0; l < final.Length; l++)
              {
                data = data + final[l] + ",";
              }
              SendMessage(iep, data);
            }
            j = j + datatoDecode.Length;
          }
          else
          {
            j++;
          }
        }
        //done = true;

      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {
        //string msg = con.State.ToString();
      }
      return 0;
    }

    protected string[] UpdateGridonheartbeat(string[] final, string iep)
    {

      for (int i = 0; i < ipGrid.Rows.Count; i++)
      {
        if (ipGrid[0, i].Value.ToString() == iep && final[1] == "Heartbeat")
        {
          for (int m = 2; m < ipGrid.ColumnCount; m++)
          {
            if (m != 4 && m != 7)
            {
              ipGrid[m, i].Value = final[m];
            }
            else if (m == 7)
            {
              final[m] = ipGrid[m, i].Value.ToString();
            }
            else if (m == 4)
            {
              final[m] = ipGrid[m, i].Value.ToString();
              //ipGrid[m, i].Value = final[m];
            }
          }
          break;
        }
      }
      return final;
    }

    private string[] UpdateipGridOnHeartBeat(string[] final, string iep)
    {
      if (this.ipGrid.InvokeRequired)
      {
        // It's on a different thread, so use Invoke.
        UpdateGridCallback1 d = new UpdateGridCallback1(UpdateGridonheartbeat);
        this.Invoke
        (d, new object[] { final, iep });
      }
      else
      {
        UpdateGridonheartbeat(final, iep);
      }
      return final;
    }
    #endregion
    //Fill Grid from database
    private void FillForm(DataTable dt)
    {
      totalmachinelabel.Text = dt.Rows.Count.ToString();
      //DataTable dt = GridData(); 
      if (ipGrid.DataSource is IDisposable old)
      {
        old.Dispose();
      }
      ipGrid.Rows.Clear();
      if (dt.Rows.Count > 0)
      {
        foreach (DataRow dataRow in dt.Rows)
        {
          ipGrid.Rows.Add();
          for (int i = 0; i < ipGrid.Columns.Count; i++)
          {
            ipGrid.Rows[ipGrid.Rows.Count - 1].Cells[i].Value = dataRow[i].ToString();
          }
        }
        DataTable dt1 = GetProjHour();
        if (ipGrid.Rows.Count > 0)
        {
          if (dt1.Rows.Count > 0)
          {
            foreach (DataRow row in dt1.Rows)
            {
              for (int k = 0; k < ipGrid.RowCount; k++)
              {
                if (ipGrid[0, k].Value.ToString() == row[0].ToString())
                {
                  ipGrid[7, k].Value = row[1].ToString();
                  break;
                }
              }
            }
          }
        }

        byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
        try
        {
          foreach (Pools p in Pool_list)
          {
            Send(p._sock, data);
          }
        }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
        catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        {
          // Console.WriteLine(ex.Message);
        }
        finally
        {
          dt1.Dispose();
          dt.Dispose();
        }
      }
    }
    public DataTable GridData()
    {
      GetIPStatus getIPStatus = new GetIPStatus();
      string query = "select cc.ccip as ip, cd.classname , status,workstatus, timer, " +
          "pcstatus,projectorstatus, projHour, curtain, screen, light, mediasignal, " +
          " centrallock, podiumlock, classlock from centralcontrol cc " +
          "join class_details cd on cc.location = cd.id join temp_centralcontrol tc " +
          " on cc.ccip = tc.ip COLLATE utf8mb4_unicode_ci order by tc.ip";
      DataTable dt = getIPStatus.ExecuteCmd(query);
      return dt;
    }
    public DataTable GetProjHour()
    {
      GetIPStatus getIPStatus = new GetIPStatus();
      string query = "select ip, sum(projectorHour) from machineworkinghours group by ip order by ip";
      DataTable dt = getIPStatus.ExecuteCmd(query);
      return dt;
    }

    #region connection to website
    //connect to website
    public void ConnectToHub()
    {
      try
      {
        con = new HubConnection("http://localhost:8080/");
        
        // con.TraceLevel = TraceLevels.All;
        // con.TraceWriter = Console.Out;
        proxy = con.CreateHubProxy("myHub");
       // MessageBox.Show("create proxy hub called");
        proxy.On<int>("SendToMachine", i =>
        {
          byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
          //Console.WriteLine("SignalR Hub called Console Application");
          //Console.WriteLine();
          try
          {
            foreach (Pools p in Pool_list)
            {
              Send(p._sock, data);
            }
          }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
          catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
          {
            // Console.WriteLine(ex.Message);
          }
        });
        proxy.On<string, string>("SendControl", (ip, data) =>
        {
          //Console.WriteLine("server called SendControl");
          //Console.WriteLine(ip + " data for IP "+data);
          byte[] dataBytes = HexEncoding.GetBytes(data, out int i);
          try
          {
            lock (Pool_list)
            {
              if (Pool_list.Count > 0)
              {
                for (int k = 0; k < Pool_list.Count; k++)
                {
                  if (Pool_list[k]._ip == ip)
                  {
                    Socket soc = Pool_list[k]._sock;
                    Send(soc, dataBytes);
                    break;
                  }
                }

              }
            }

          }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
          catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
          {
            // Console.WriteLine(ex.Message);
          }
        });
        proxy.On<string>("RefreshStatus", (ip) =>
        {
          byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
          try
          {
            lock (Pool_list)
            {
              if (Pool_list.Count > 0)
              {
                for (int k = 0; k < Pool_list.Count; k++)
                {
                  if (Pool_list[k]._ip == ip)
                  {
                    Socket soc = Pool_list[k]._sock;
                    Send(soc, data);
                    break;
                  }
                }

              }
            }
          }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
          catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
          {
            // Console.WriteLine(ex.Message);
          }
        });
        proxy.On<int>("Counts", i =>
        {
          SendCounts();
        });
        con.Start().ContinueWith(task =>
        {
          if (task.IsFaulted)
          {
            MessageBox.Show("There was an error opening the connection with WebClient");
          }
          //else{MessageBox.Show("Connected to signalR");}
        }).Wait();
        
      }
      catch (Exception ex)
      {
        
        MessageBox.Show("not connected to WebClient " + ex.Message);
        con.StateChanged += Con_StateChanged;
      }
    }

    private static async Task StartCon()
    {
      await con.Start();
    }
    private static void Con_StateChanged(StateChange obj)
    {
      if (obj.OldState == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
      {
        // Console.WriteLine("State changed inside");
        var current = DateTime.Now.TimeOfDay;
        SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(10), StartCon);
        // Console.WriteLine("State changed inside done");
      }
      else
      {
        // Console.WriteLine("State changed else inside");
        if (_timer != null)
          _timer.Dispose();
      }
    }

    private static void Con_Closed()
    {
      //Console.WriteLine("connection closed");
      con.Start().Wait();
    }
    public static void SendMessage(string sender, string message)
    {
      try
      {
        if (con.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
        {
          // Console.WriteLine("connecting to server");
          con.Start().Wait();
          // Console.WriteLine("connected");
        }
        proxy.Invoke("SendMessage", sender, message);
        //Console.WriteLine("Sent to signalR server by" + sender);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        //  Console.WriteLine("connecting to server");
        con.Start().Wait();
        // Console.WriteLine("connected");
      }
    }

    public static void SendCounts()
    {
      proxy.Invoke("CountMachines", times.Count);
    }
    #endregion

    //General App task
    private void StartTimer()
    {
      timer = new System.Threading.Timer(new TimerCallback(CheckMachine), null, 60000, 60000);
      statusTimer = new System.Threading.Timer(new TimerCallback(UpdateMachineStatus), null, 2000, 60000);
    }

    public void UpdatemachineLabel()
    {
      try
      {
        if (ipGrid.Rows.Count > 0)
        {
          totalmachinelabel.Text = ipGrid.Rows.Count.ToString();
          onlinemachinelbl.Text = Pool_list.Count.ToString();
          offlinemachinelbl.Text = (Convert.ToInt32(totalmachinelabel.Text) - Convert.ToInt32(onlinemachinelbl.Text)).ToString();
          int compcount = GetPcOnCount();
          int projcount = GetProjectorOnCount();
          string counts = compcount.ToString() + ',' + projcount.ToString() + ',' + totalmachinelabel.Text;
          proxy.Invoke("CountOfMachines", counts);
        }
      }
      catch (Exception ex)
      {

      }

    }
    private void UpdateMachineStatus(object state)
    {
      //if (offlinemachinelbl.InvokeRequired)
      //{
      //  Updatelabel d = new Updatelabel(UpdatemachineLabel);
      //  this.Invoke(d);
      //}
      List<string> keys;
      try
      {
        keys = new List<string>(currentstatus.Keys);
        lock (currentstatus)
        {
          lock (workingHours)
          {
            foreach (string s in keys)
            {
              if (workingHours.ContainsKey(s))
              {
                workingHours[s].StatusTimeUpdate(currentstatus[s].ToArray());
              }
            }
          }
        }
      }
      catch (Exception ex)
      {

      }
      finally
      {
        //keys.Clear();
        keys = null;
      }
    }

    private void UpdateipGrid()
    {
      DataTable dt = GridData();
      if (this.ipGrid.InvokeRequired)
      {
        // It's on a different thread, so use Invoke.
        UpdateGridCallback d = new UpdateGridCallback(FillForm);
        this.Invoke
            (d, new object[] { dt });
      }
      else
      {
        // It's on the same thread, no need for Invoke
        FillForm(dt);
      }
      dt.Dispose();

    }
    private void CheckMachine(object state)
    {
      List<string> todel = new List<string>();
      List<string> timekeys = null;
      try
      {
        UpdateipGrid();

        timekeys = new List<string>(times.Keys);
        foreach (string s in timekeys)
        {
          if ((DateTime.Now).Subtract(times[s]).Minutes > 1)
          {
            todel.Add(s);
          }
        }
        if (todel.Count > 0)
        {
          foreach (string s in todel)
          {
            lock (times)
            {
              times.Remove(s);
            }
            lock (currentstatus)
            {
              currentstatus.Remove(s);
            }
            //RemovePool(s);
            WorkingHours hours = null;
            if (workingHours.ContainsKey(s))
            {
              hours = workingHours[s];
            }
            if (hours != null)
            {
              string[] stat = new string[7];
              stat[0] = s;
              for (int k = 1; k < 7; k++)
              {
                stat[k] = "Off";
              }
              hours.StatusTimeUpdate(stat);
              workingHours.Remove(s);
            }

          }
        }

      }
      catch (Exception ex)
      {

      }
      finally
      {
        todel = null;
        timekeys = null;
      }
    }
    private static void SetTimer(TimeSpan starTime, TimeSpan every, Func<Task> action)
    {
      var current = DateTime.Now;
      var timeToGo = starTime - current.TimeOfDay;
      if (timeToGo < TimeSpan.Zero)
      {
        return;
      }
      _timer = new System.Threading.Timer(x =>
      {
        action.Invoke();
      }, null, timeToGo, every);
    }

    public static bool IsClientConnected(Socket handler)
    {
      bool status = false;
      try
      {
        status = handler.Connected;
      }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
      catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
      {
        //SaveStatus(((IPEndPoint)handler.RemoteEndPoint).Address.ToString(), "Offline");
      }
      return status;
    }
    private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      this.Visible = true;
      this.WindowState = FormWindowState.Normal;
    }

    private void Button1_Click(object sender, EventArgs e)
    {
      ipGrid.Rows.Clear();
      FillForm(GridData());
    }
    //onlien Computer Counts
    private int GetPcOnCount()
    {
      int pcCount = 0;
      foreach (DataGridViewRow row in ipGrid.Rows)
      {
        if (row.Cells[5].Value.ToString() == "已开机")
        {
          pcCount = pcCount + 1;
        }
      }
      return pcCount;
    }
    //Online PC COUnt
    private int GetProjectorOnCount()
    {
      int projCount = 0;
      foreach (DataGridViewRow row in ipGrid.Rows)
      {
        if (row.Cells[6].Value.ToString() == "已开机")
        {
          projCount = projCount + 1;
        }
      }
      return projCount;
    }
  }
}
