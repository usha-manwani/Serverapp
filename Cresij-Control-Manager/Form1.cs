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
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Cresij_Control_Manager
{
    public partial class Form1 : Form
    {
        List<Pools> Pool_list = new List<Pools>();
        // private static Dictionary<string, Socket> Clients = new Dictionary<string, Socket>();
        private static Dictionary<string, List<string>> currentstatus = new Dictionary<string, List<string>>();
        public static Dictionary<string, DateTime> times = new Dictionary<string, DateTime>();
        //Dictionary<string, DateTime> HeartbeatTime = new Dictionary<string, DateTime>();
        public static int connectedClient = 0;
        private static IHubProxy proxy;
        private static HubConnection con;
        delegate void UpdateGrid(Control grid);
        private static System.Threading.Timer _timer;
        private static System.Threading.Timer timer;
        private static System.Threading.Timer statusTimer;
        private static System.Threading.Timer StrategyTimer;
        private static Dictionary<string, string> fixedip = new Dictionary<string, string>();
        private static Dictionary<string, Socket> dynamicip = new Dictionary<string, Socket>();
        Dictionary<string, WorkingHours> workingHours = new Dictionary<string, WorkingHours>();
        delegate void UpdateGridCallback(DataTable dt);
        private delegate Dictionary<string, string> UpdateGridCallback1(Dictionary<string, string> data, string ip);
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
            notifyIcon1.BalloonTipText = "Server App Started";
            notifyIcon1.BalloonTipTitle = "Server App Notifier";
            notifyIcon1.ShowBalloonTip(2000);
            closeform.Click += Closeform_Click;
            viewdevices.Click += Viewdevices_Click;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            if (!EventLog.SourceExists("CresijServerLogTest"))
            {
                EventLog.CreateEventSource("CresijServerLogTest", "Application");
            }
            // Set the source name for writing log entries.
            eventLog.Source = "CresijServerLogTest";
            StartListen(IPAddress.Any.ToString(), 1200);
            StartListenDesktop(IPAddress.Any.ToString(), 10008);
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
        static int eventID = 9;

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
                    ClearSocket(sokConnection);
                    //dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去.
                    //if (!Clients.ContainsKey(_ip))
                    //{
                    //    Clients.Add(_ip, sokConnection);
                    //}
                    bool ipexists;
                    //string mac = "";
                    //ipexists = CallGridforMac(_ip);

                    //if (ipexists)
                    //{
                    // byte[] d = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 }; 
                    Log("Socket Connected asking for mac address from ip : " + iep.Address.ToString() + " port: " + iep.Port);
                    Send(sokConnection, "MacAddress");
                    //Send(sokConnection, d);
                    Thread thr = new Thread(RecMsg)
                    {
                        IsBackground = true
                    };
                    SetControl(sokConnection);//设置心跳包
                    thr.Start(sokConnection);
                    
                    Pools pl = new Pools(_ip, sokConnection, thr, DateTime.Now);
                    UpdatePool(pl);

                    //if (!workingHours.ContainsKey(_ip))
                    //{
                    //    WorkingHours working = new WorkingHours(_ip);
                    //    workingHours.Add(_ip, working);
                    //}
                    // }
                }
                catch (Exception ex)
                {
                    Log("error in watch connecting method " + ex.StackTrace);
                    //MessageBox.Show("Error in tcp listening: " + ex.Message);
                }
            }
        }

        delegate void IpExists(string _ip, out bool ipexists);

        private void ClearSocket(Socket soc)
        {
            try
            {
                for (int i = Pool_list.Count - 1; i > 0; i--)
                {
                    Pools _p = Pool_list[i];
                    if (_p != null && _p._sock == soc)
                    {
                        Pool_list.RemoveAt(i);
                        if (dynamicip.ContainsKey(_p._Mac))
                            dynamicip.Remove(_p._Mac);
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
                Log("error in Clear socket " + ex.StackTrace);
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
                        HandleCommand(sokClient, receiveBytes, length);
                       // Task.Factory.StartNew(() => HandleCommand(sokClient, receiveBytes, length));
                    }
#pragma warning disable CS0168 // The variable 'se' is declared but never used
                    catch (SocketException se)
#pragma warning restore CS0168 // The variable 'se' is declared but never used
                    {
                        RemovePool(iep.Address.ToString());
                        workingHours.Remove(iep.Address.ToString());
                        //Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "Receive Message exception from " + iep.Address.ToString() + "  Message : " + se.Message +"source " + se.StackTrace);
                        break;
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        RemovePool(iep.Address.ToString());
                        workingHours.Remove(iep.Address.ToString());
                        //Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + " Message exception  " + iep.Address.ToString() + "  Message : " + ex.Message + "source " + ex.StackTrace);
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
                //Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + " Object disposed exception Message: " + ex.Message + " stack trace " + ex.StackTrace + " "
                // + ex.GetError() +" exception instance "+ ex.InnerException +"socket connection for IP :"+ ((IPEndPoint)((Socket)sokConnectionparn).RemoteEndPoint).Address.ToString());
                return;//如果主界面已经退出了，那线程也退出好了
            }
        }
        /// <summary>
        /// 更新连接池
        /// </summary>
        /// <param name="_pls"></param>
        private void UpdatePool(Pools _pls)
        {
            try
            {
                if (!Pool_list.Contains(_pls))
                {
                    Pool_list.Add(_pls);
                    Log(" Updated Pool " + _pls._ip + " " + _pls._Port);
                }

                if (onlinemachinelbl.InvokeRequired)
                {
                    Updatelabel updatelabel = new Updatelabel(UpdatemachineLabel);
                    this.Invoke(updatelabel);
                }
            }
            catch (Exception ex)
            {
                Log("error in Update Pool " + ex.StackTrace);
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
                        var _iep = _x._ip + "_" + _x._Port;
                        if (_x._sock != null)
                        {
                            _x._sock.Close();
                        }
                        Pool_list.Remove(_x);
                        Log(" Removed Pool " + _iep);
                        if (dynamicip.ContainsKey(_x._Mac))
                            dynamicip.Remove(_x._Mac);
                        if (onlinemachinelbl.InvokeRequired)
                        {
                            Updatelabel updatelabel = new Updatelabel(UpdatemachineLabel);
                            this.Invoke(updatelabel);
                        }
                        Decode dr = new Decode();
                        string m = dr.offlineMessage();
                        // SendMessage(_IPKey,new Dictionary<string,string>());
                        dr = null;
                        if (_x._thr.IsAlive)
                            _x._thr.Abort();
                        _x.Destory();

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("error in Remove Pool " + ex.StackTrace);
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
            catch (Exception ex) { Log("error in Check Socket Connection " + ex.StackTrace); }
        }

        private static void Send(Socket workSocket, string keyword) //byte[] data,
        {
            var some = new int[] { };

            Instructions inst = new Instructions();

            var cc = inst.GetValues(keyword);
            Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + " IP : " +
                  ((IPEndPoint)workSocket.RemoteEndPoint).Address.ToString() + " instruction bytes : " +
                  cc);
            try
            {
                if (cc != null || cc.Length > 0)
                    if (!workSocket.Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。                                                                                  //   -或 - 如果有数据可供读取，则为 true。-或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
                    {
                        Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "Check before send. Socket Connected : " +
                 ((IPEndPoint)workSocket.RemoteEndPoint).Address.ToString() + " instruction bytes : " +
                 cc.Select(x => x.ToString()));
                        workSocket.Send(cc);
                    }
                    else

                        Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "  socket not connected " + ((IPEndPoint)workSocket.RemoteEndPoint).Address.ToString());
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "  sent to  " +
                  ((IPEndPoint)workSocket.RemoteEndPoint).Address.ToString() + "  data bytes : "
                  + cc + " Error occured " + ex.Message + " stack trace " + ex.StackTrace);
            }
        }
        #endregion

        #region--处理下位机信息--
        private int HandleCommand(Socket sock, byte[] receiveBytes, int length)
        {
            var mac = "";
            try
            {
                
               
                Decode dd = new Decode();
                Dictionary<string, string> final;
                string[] status;
                Dictionary<string, object> re = new Dictionary<string, object>();
                for (int j = 0; j < length;)
                {
                    if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
                    {
                        byte[] datatoDecode = new byte[4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3]];
                        for (int k = 0; k < datatoDecode.Length; k++)
                        {
                            datatoDecode[k] = receiveBytes[k + j];
                        }
                        //if (currentstatus.ContainsKey(iep))
                        //{
                        //    status = currentstatus[iep].ToArray();
                        //}
                        //else
                        //{
                        status = new string[7];
                        if (dynamicip.ContainsValue(sock))
                        {
                            mac = dynamicip.FirstOrDefault(x => x.Value == sock).Key;
                        }
                        status[0] = mac;
                        for (int i = 1; i < 7; i++)
                        {
                            status[i] = "Off";
                        }
                        //}
                        re = dd.Decoded("", datatoDecode, status);
                        // dd = null;
                        if (re.Count == 2)
                        {
                            if (mac != "")
                            {
                                var statustemp = (re["status"] as IEnumerable<string>).ToArray();
                                for (int i = 0; i < 6; i++)
                                {
                                    status[i + 1] = statustemp[i];
                                }

                                if (currentstatus.ContainsKey(mac))
                                {
                                    currentstatus[mac] = status.ToList();
                                }
                                else
                                {
                                    currentstatus.Add(mac, status.ToList());
                                }
                            }

                            object obj = re["data"];
                            final = obj as Dictionary<string, string>;

                            if (final.ContainsKey("MacAddress"))
                            {
                                mac = final["MacAddress"];
                                IsMAcAdd(fixedip, final, sock);
                            }
                            if (mac != "")
                            {
                                if (final["Type"]=="Heartbeat")
                                {
                                    final = UpdateipGridOnHeartBeat(final, mac);
                                    lock (times)
                                    {
                                        if (times.ContainsKey(mac)) times[mac] = DateTime.Now;
                                        else times.Add(mac, DateTime.Now);
                                    }
                                }
                                
                            }
                            
                           // SendMessage(mac, final);
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
                Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "  exception in Handle message  " + ex.Message + " stack trace " + ex.StackTrace + " "
                 + ex.GetError() + "from mac : " + mac);
                //string msg = con.State.ToString();
            }
            return 0;
        }

        
        private void IsMAcAdd(Dictionary<string, string> databaseip, Dictionary<string, string> message, Socket sock)
        {

            try
            {
                var tt = message["MacAddress"];
                lock (times)
                {
                    if (times.ContainsKey(tt))
                    {
                        times[tt] = DateTime.Now;
                    }
                    else
                    {
                        times.Add(tt, DateTime.Now);
                    }
                }
                if (databaseip.ContainsKey(tt))
                {
                    if (!dynamicip.ContainsKey(tt))
                        dynamicip.Add(tt, sock);

                    if (!workingHours.ContainsKey(tt))
                    {
                        WorkingHours working = new WorkingHours(tt);
                        workingHours.Add(tt, working);
                    }
                    foreach (Pools p in Pool_list)
                    {
                        if (p._sock == sock)
                        {
                            p.SetMac(tt);
                        }
                    }
                    if (onlinemachinelbl.InvokeRequired)
                    {
                        Updatelabel updatelabel = new Updatelabel(UpdatemachineLabel);
                        this.Invoke(updatelabel);
                    }
                    else UpdatemachineLabel();
                }
            }
            catch (Exception ex) { Log("error in IsMacAdd " + ex.StackTrace); }

        }

        protected Dictionary<string, string> UpdateGridonheartbeat(Dictionary<string, string> final, string mac)
        {
            if (dynamicip.ContainsKey(mac))
            {

                try
                {

                    for (int i = 0; i < ipGrid.Rows.Count; i++)
                    {
                        if (ipGrid["MacAddress", i].Value.ToString() == mac && final["Type"] == "Heartbeat")
                        {
                            ipGrid["Status", i].Value = "在线";//Online
                            ipGrid["WorkStat", i].Value = final["WorkStatus"] == "Open" ? "运行中" : "待机";
                            ipGrid["PCStat", i].Value = final["PcStatus"] == "Off" ? "已关机" : "已开机";
                            ipGrid["ProjectorStat", i].Value = final["ProjectorStatus"] == "Off" ? "已关机" : "已开机";
                            if (final["CurtainStatus"] == "Up") ipGrid["CurtainStat", i].Value = "升";
                            else if (final["CurtainStatus"] == "Down") ipGrid["CurtainStat", i].Value = "降";
                            else ipGrid["CurtainStat", i].Value = "停";

                            if (final["ScreenStatus"] == "Open") ipGrid["ScreenStat", i].Value = "升";
                            else if (final["ScreenStatus"] == "Close") ipGrid["ScreenStat", i].Value = "关";
                            else ipGrid["ScreenStat", i].Value = "停";
                            ipGrid["LightStat", i].Value = final["LightStatus"] == "On" ? "开" : "关";
                            switch (final["MediaSignal"])
                            {
                                case "Desktop":
                                    ipGrid["Media", i].Value = "台式电脑";//desktop;
                                    break;
                                case "Laptop":
                                    ipGrid["Media", i].Value = "手提电脑";//laptop"";
                                    break;
                                case "DigitalBooth":
                                    ipGrid["Media", i].Value = "数码展台";//digital booth(curtain)"";
                                    break;
                                case "DigitalCurtain":
                                    ipGrid["Media", i].Value = "数码设备";//digital equipment(Screen)"";
                                    break;
                                case "DVD":
                                    ipGrid["Media", i].Value = "DVD";//dvd"";
                                    break;
                                case "BluRayDVD":
                                    ipGrid["Media", i].Value = "蓝光DVD";//"Blu-Ray DVD" "";
                                    break;
                                case "TV":
                                    ipGrid["Media", i].Value = "电视机"; //TV"";
                                    break;
                                case "VideoCamera":
                                    ipGrid["Media", i].Value = "摄像机";//Video Camera
                                    break;
                                case "RecordingSystem":
                                    ipGrid["Media", i].Value = "录播"; //Recording System"";
                                    break;
                                case "ExternalHardDisk":
                                    ipGrid["Media", i].Value = "";
                                    break;

                            }

                            ipGrid["CentralLock", i].Value = final["IsSystemLock"] == "True" ? "锁定" : "解锁";
                            ipGrid["PodiumLock", i].Value = final["IsPodiumLock"] == "True" ? "锁定" : "解锁";
                            ipGrid["ClassLock", i].Value = final["IsClassLock"] == "True" ? "锁定" : "解锁";
                            final["ProjectorHour"] = ipGrid["ProjHour", i].ToString();
                            // final["TimerService"] = ipGrid["Timer", i].ToString();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("error in Update Grid " + ex.StackTrace);
                }
            }

            //for (int i = 0; i < ipGrid.Rows.Count; i++)
            //{
            //    if (ipGrid[0, i].Value.ToString() == iep && final["type"] == "Heartbeat")
            //    {
            //        for (int m = 2; m < ipGrid.ColumnCount; m++)
            //        {
            //            if (m != 4 && m != 7)
            //            {
            //                ipGrid[m, i].Value = "";// final[m];
            //            }
            //            else if (m == 7)
            //            {
            //                final["ProjectorHours"] = ipGrid[m, i].Value.ToString();
            //            }
            //            else if (m == 4)
            //            {
            //                final["TimerService"] = ipGrid[m, i].Value.ToString();
            //                //ipGrid[m, i].Value = final[m];
            //            }
            //        }
            //        break;
            //    }
            //}
            return final;
        }

        private Dictionary<string, string> UpdateipGridOnHeartBeat(Dictionary<string, string> final, string mac)
        {
            try
            {
                if (this.ipGrid.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    UpdateGridCallback1 d = new UpdateGridCallback1(UpdateGridonheartbeat);
                    this.Invoke
                    (d, new object[] { final, mac });
                }
                else
                {
                    UpdateGridonheartbeat(final, mac);
                }
            }
            catch (Exception ex) { Log("error in Update Grid on Heartbeat  " + ex.StackTrace); }
            return final;
        }
        #endregion
        //Fill Grid from database
        private void FillForm(DataTable dt)
        {
            try
            {
                totalmachinelabel.Text = dt.Rows.Count.ToString();
                offlinemachinelbl.Text = (Convert.ToInt32(totalmachinelabel.Text) - Convert.ToInt32(onlinemachinelbl.Text)).ToString();
                //DataTable dt = GridData(); 
                if (ipGrid.DataSource is IDisposable old)
                {
                    old.Dispose();
                }
                ipGrid.Rows.Clear();
                fixedip.Clear();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        ipGrid.Rows.Add();
                        for (int i = 0; i < ipGrid.Columns.Count; i++)
                        {
                            ipGrid.Rows[ipGrid.Rows.Count - 1].Cells[i].Value = dataRow[i].ToString();

                        }
                        fixedip.Add(dataRow["ccmac"].ToString(), dataRow["ip"].ToString());
                    }
                    //DataTable dt1 = GetProjHour();
                    //if (ipGrid.Rows.Count > 0)
                    //{
                    //    if (dt1.Rows.Count > 0)
                    //    {
                    //        foreach (DataRow row in dt1.Rows)
                    //        {
                    //            for (int k = 0; k < ipGrid.RowCount; k++)
                    //            {
                    //                if (ipGrid[0, k].Value.ToString() == row[0].ToString())
                    //                {
                    //                    ipGrid[7, k].Value = row[1].ToString();
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
                    //try
                    //{
                    //    foreach (Pools p in Pool_list)
                    //    {
                    //        Log("mac address of machine where status query to send is : " + p._Mac + " from method FllForm");
                    //        Send(p._sock, "MacAddress"); //data
                    //    }
                    //}
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
//                    catch (Exception ex)
//#pragma warning restore CS0168 // The variable 'ex' is declared but never used
//                    {
//                        Log("error in Fill Form " + ex.StackTrace);
//                        // Console.WriteLine(ex.Message);
//                    }
//                    finally
//                    {
//                        // dt1.Dispose();
//                        dt.Dispose();
//                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in Fill Form 1 " + ex.StackTrace);
            }
            finally
            {
                dt.Dispose();
            }
        }
            public DataTable GridData()
        {
            GetIPStatus getIPStatus = new GetIPStatus();
            string query = "select distinct(ccmac) as ccmac,ccequipip as ip, cd.classname ,'离线' as status,'' as workstatus, " +
                "'' as pcstatus,'' as projectorstatus,'' as projHour,'' as curtain,'' as screen,'' as light,'' as mediasignal, " +
                "'' as centrallock,'' as podiumlock,'' as classlock, classid from " +
                " classdetails cd group by ccmac order by ccequipip";
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
                con = new HubConnection("http://localhost:8090/");
                // con.TraceLevel = TraceLevels.All;
                // con.TraceWriter = Console.Out;
                proxy = con.CreateHubProxy("myHub");
                // MessageBox.Show("create proxy hub called");
                proxy.On<int>("SendToMachine", i =>
                {
                    //byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
                    //Console.WriteLine("SignalR Hub called Console Application");
                    //Console.WriteLine();
                    try
                    {
                        foreach (Pools p in Pool_list)
                        {
                            Log("mac address of machine where status query to send is : " + p._Mac + " from method SendToMachine from COnnectTO Hub");
                            Send(p._sock, "MacAddress"); //data
                        }
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        // Console.WriteLine(ex.Message);
                    }
                });
                proxy.On<string, string>("SendControl", (mac, data) =>
                {
                    //Console.WriteLine("server called SendControl");
                    //Console.WriteLine(ip + " data for IP "+data);
                    //byte[] dataBytes = HexEncoding.GetBytes(data, out int i);
                    try
                    {
                        lock (Pool_list)
                        {
                            if (Pool_list.Count > 0)
                            {
                                for (int k = 0; k < Pool_list.Count; k++)
                                {
                                    if (Pool_list[k]._Mac == mac)
                                    {
                                        Log("mac address of machine where status query to send is : " + Pool_list[k]._Mac +
                                            " from method sendtocontrol from connect to hub");
                                        Socket soc = Pool_list[k]._sock;
                                        Send(soc, data); //dataBytes
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
                proxy.On<string>("RefreshStatus", (mac) =>
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
                                    if (Pool_list[k]._Mac == mac)
                                    {
                                        Log("mac address of machine where status query to send is : " + Pool_list[k]._Mac + " from method RefreshStatus from COnnecttoHUb");
                                        Socket soc = Pool_list[k]._sock;
                                        Send(soc, "Status"); //data
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
        public static void SendMessage(string sender, Dictionary<string, string> message)
        {


            Dictionary<string, string> message1 = new Dictionary<string, string>();
            message1.Add("test", "success");
            try
            {
                if (con.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                {
                    // Console.WriteLine("connecting to server");
                    con.Start().Wait();
                    // Console.WriteLine("connected");
                }
                proxy.Invoke("SendMessage", sender, message1);
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
            StrategyTimer = new System.Threading.Timer(new TimerCallback(RunStrategyTimer), null, 10000, 60000);
        }
        private void RunStrategyTimer(object state)
        {
            var tt = DateTime.Now.ToString("HH:mm") + ":00";
            StrategyExec strategyExec = new StrategyExec();
            var ff = strategyExec.GetData(tt);

            foreach (FinalResult f in ff)
            {
                try
                {

                    if (f.Instruction == "SystemOffS")
                    {
                        int r = SendToDesktop(f.Deskmac, f.Ccmac, "Shutdown");
                        if (r == 0)
                        {
                            lock (Pool_list)
                            {
                                if (Pool_list.Count > 0)
                                {
                                    for (int k = 0; k < Pool_list.Count; k++)
                                    {
                                        if (Pool_list[k]._Mac == f.Ccmac)
                                        {
                                            Log("mac address of machine where status query to send is : " + Pool_list[k]._Mac +
                                                 " from method : RunStrategyTimer");
                                            Socket soc = Pool_list[k]._sock;
                                            Send(soc, f.Instruction); //dataBytes

                                            break;
                                        }
                                    }

                                }
                            }
                        }

                    }
                    else
                    {

                        lock (Pool_list)
                        {
                            if (Pool_list.Count > 0)
                            {
                                for (int k = 0; k < Pool_list.Count; k++)
                                {
                                    if (Pool_list[k]._Mac == f.Ccmac)
                                    {
                                        Log("mac address of machine where status query to send is : " + Pool_list[k]._Mac +
                                                " from method : RunStrategyTimer 2nd part");
                                        Socket soc = Pool_list[k]._sock;
                                        Send(soc, f.Instruction); //dataBytes
                                        Log("sent instruction " + f.Instruction + " to " + Pool_list[k]._Mac + " using strategy at time " + DateTime.Now);
                                        break;
                                    }
                                }

                            }
                        }

                    }
                }
                catch (Exception ex) { }
            }
        }
        public void UpdatemachineLabel()
        {
            try
            {
                if (ipGrid.Rows.Count > 0)
                {
                    totalmachinelabel.Text = ipGrid.Rows.Count.ToString();
                    onlinemachinelbl.Text = dynamicip.Count.ToString();
                    offlinemachinelbl.Text = (Convert.ToInt32(totalmachinelabel.Text) - Convert.ToInt32(onlinemachinelbl.Text)).ToString();
                    int compcount = GetPcOnCount();
                    int projcount = GetProjectorOnCount();
                    string counts = compcount.ToString() + ',' + projcount.ToString() + ',' + totalmachinelabel.Text;
                    proxy.Invoke("CountOfMachines", counts);
                }
            }
            catch (Exception ex)
            {
                Log("Exception in update machine label " + ex.StackTrace);
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
                Log("Exception in update machine Status " + ex.StackTrace);
            }
            finally
            {
                //keys.Clear();
                keys = null;
            }
        }

        private void UpdateipGrid()
        {
            try
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
            catch (Exception ex) { Log("Exception in update ip Grid " + ex.StackTrace); }

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
                Log("Exception in Check Machine " + ex.StackTrace);
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
                Log("Exception in isClientConnected " + ex.StackTrace);
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
        static EventLog eventLog = new EventLog();
        public static void Log(string logMessage)
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
                eventLog.WriteEntry("Error in stream writer " + ex.Message, EventLogEntryType.Warning, eventID);
            }

        }

        #region Desktop Server
        Thread threadWatch1 = null; // 负责监听客户端连接请求的 线程;
        Socket socketWatch1 = null;
        List<Pools> PoolList_Desktop = new List<Pools>();
        Dictionary<string, string> DesktopList = new Dictionary<string, string>();
        Dictionary<string, string> DesktopIpDynamic = new Dictionary<string, string>();
        private void StartListenDesktop(string _IpAddress, int _Ports)
        {
            socketWatch1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socketWatch1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            IPAddress address = IPAddress.Parse(_IpAddress);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _Ports);
            try
            {

                // 将负责监听的套接字绑定到唯一的ip和端口上;
                socketWatch1.Bind(endPoint);
                //MessageBox.Show("Tcp Connection done ");
            }
            catch (SocketException se)
            {
                Log(" Socket connection " + " " + se.Message + " " + se.StackTrace);
                //MessageBox.Show("Error in tcp : " + se.Message);
                return;
            }
            // 设置监听队列的长度;  
            socketWatch1.Listen(100);
            // 创建负责监听的线程;
            threadWatch1 = new Thread(WatchConnectingDesktop)
            {
                IsBackground = true
            };
            threadWatch1.Start();
        }

        void WatchConnectingDesktop()
        {
            while (true)  // 持续不断的监听客户端的连接请求;
            {
                try
                {
                    // socketWatch.BeginAccept(new AsyncCallback(AcceptCallback), socketWatch);
                    Socket sokConnection = socketWatch1.Accept();

                    // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字  
                    // 将与客户端连接的 套接字 对象添加到集合中;
                    //要判断当前连接是否已经在集合中  
                    IPEndPoint iep = (IPEndPoint)sokConnection.RemoteEndPoint;
                    string _ip = iep.Address.ToString();
                    int clientport = iep.Port;
                    ClearSocketDesktop(_ip, iep.Port);

                    Thread thr = new Thread(RecMsg_Desktop)
                    {
                        IsBackground = true
                    };

                    thr.Start(sokConnection);

                    Pools pl = new Pools(_ip, sokConnection, thr, DateTime.Now, clientport);
                    UpdatePoolDesktop(pl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in Desktop listening: " + ex.Message);
                }
            }
        }

        private void UpdatePoolDesktop(Pools _pls)
        {
            if (!PoolList_Desktop.Contains(_pls))
            {
                PoolList_Desktop.Add(_pls);
                Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
                DatatoSend.Add("Type", "Reply");
                DatatoSend.Add("Status", "Connected");
                var s = JsonSerializer.Serialize(DatatoSend);
                byte[] b = Encoding.ASCII.GetBytes(s);
                _pls._sock.Send(b);
            }
        }
        private void RecMsg_Desktop(object sokConnectionparn)
        {
            string data = "";
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
                        data = Encoding.ASCII.GetString(receiveBytes, 0, length);
                        var received = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
                        DecodeDataDesktop(iep.Address.ToString(), received, sokClient);
                    }
#pragma warning disable CS0168 // The variable 'se' is declared but never used
                    catch (SocketException se)
#pragma warning restore CS0168 // The variable 'se' is declared but never used
                    {
                        Log(" Receive message exception 1 " + " " + se.Message + " " + se.StackTrace);
                        ClearSocketDesktop(iep.Address.ToString(), iep.Port);
                        break;
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        Log(" Receive message exception 2 " + " " + ex.Message + " " + ex.StackTrace);
                        ClearSocketDesktop(iep.Address.ToString(), iep.Port);
                        //Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + " Message exception  " + iep.Address.ToString() + "  Message : " + ex.Message + "source " + ex.StackTrace);
                        break;
                    }

                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (ObjectDisposedException ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                Log(" Receive message exception 3 " + " " + ex.Message + " " + ex.StackTrace);
                //Log(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + " Object disposed exception Message: " + ex.Message + " stack trace " + ex.StackTrace + " "
                // + ex.GetError() +" exception instance "+ ex.InnerException +"socket connection for IP :"+ ((IPEndPoint)((Socket)sokConnectionparn).RemoteEndPoint).Address.ToString());
                return;//如果主界面已经退出了，那线程也退出好了
            }

        }
        private int DecodeDataDesktop(string ip, Dictionary<string, string> data, Socket sock)
        {
            string d = "";
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            KeyValuePair<string, string> r = new KeyValuePair<string, string>();
            try
            {
                if (data["Type"] == "MacAddress")
                {
                    Log("mac received " + data["value"]);
                    var mac = data["value"].Split(',');
                    GetMacAddress gt = new GetMacAddress();
                    r = gt.GetMac(mac);
                    DatatoSend.Add("FromDatabaseMac", r.Key);
                    if (r.Key != null)
                    {
                        if (!DesktopList.Contains(r))
                        {
                            DesktopList.Add(r.Key, r.Value);
                        }
                        lock (PoolList_Desktop)
                        {
                            for (int i = PoolList_Desktop.Count - 1; i >= 0; i--)
                            {
                                Pools _p = PoolList_Desktop[i];
                                if (_p._sock == sock)
                                    _p._Mac = r.Key;
                            }
                        }
                    }
                    else
                    {
                        Log("mac from database key: " + r.Key + " value: " + r.Value);
                        DatatoSend.Add("Type", "Reply");
                        DatatoSend.Add("Status", "404");

                        var s = JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.ASCII.GetBytes(s);
                        sock.Send(b);
                        ClearSocketDesktop(ip, ((IPEndPoint)sock.RemoteEndPoint).Port);
                    }
                    //SendToDesktop(r.Key, r.Value, "Shutdown");
                }
                if (data["Type"] == "Command")
                {
                    var code = data["Code"];
                    var ccmac = data["CCmac"];
                    Log("received " + code + " for machine with mac " + ccmac);
                    if (code == "Shutdown")
                    {
                        Instructions ins = new Instructions();
                        d = "SystemOffS";
                    }
                    try
                    {
                        lock (Pool_list)
                        {
                            if (Pool_list.Count > 0)
                            {
                                for (int k = 0; k < Pool_list.Count; k++)
                                {
                                    if (Pool_list[k]._Mac == ccmac)
                                    {
                                        Log("mac address of machine where status query to send is : " + Pool_list[k]._Mac +
                                        " from method : Decode Data Desktop");
                                        Socket soc = Pool_list[k]._sock;
                                        Send(soc, d); //dataBytes
                                        Log("sent " + code + " to machine with mac " + ccmac);
                                        break;
                                    }
                                    else
                                    {
                                        Log(" machine connected with mac " + Pool_list[k]._Mac);
                                    }
                                }
                            }
                        }
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        Log(" Decode Desktop Data exception under send data to machine " + " " + ex.Message + " " + ex.StackTrace);
                        // Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(" Decode Desktop Data exception 2 " + " " + ex.Message + " " + ex.StackTrace);
                ClearSocketDesktop(ip, ((IPEndPoint)sock.RemoteEndPoint).Port);
            }
            return 1;
        }
        private int SendToDesktop(string deskmac, string ccmac, string cmd)
        {
            var result = 0;
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            DatatoSend.Add("Type", "Command");
            DatatoSend.Add("Code", cmd);
            DatatoSend.Add("CCmac", ccmac.ToUpper());
            DatatoSend.Add("Deskmac", deskmac.ToUpper());
            var s = JsonSerializer.Serialize(DatatoSend);

            try
            {
                lock (PoolList_Desktop)
                {
                    for (int i = PoolList_Desktop.Count - 1; i >= 0; i--)
                    {
                        var _p = PoolList_Desktop[i];
                        if (_p._Mac.ToUpper() == deskmac.ToUpper())
                        {
                            var sokConnection = _p._sock;
                            byte[] b = Encoding.ASCII.GetBytes(s);
                            if (CheckConnectionDesktop(sokConnection))
                            {
                                sokConnection.Send(b);
                                Log("Instruction for shutdown sent to " + deskmac);
                                result = 1;
                            }
                            else
                            {
                                Log("Instruction for shutdown to desktop couldn't send " +
                                    "because the client wasnt connected with desktop mac as " + deskmac);
                                ClearSocketDesktop(((IPEndPoint)sokConnection.RemoteEndPoint).Address.ToString(), ((IPEndPoint)sokConnection.RemoteEndPoint).Port);
                                result = 0;
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(" can not send to desktop " + " " + ex.Message + " " + ex.StackTrace);
                result = 0;
            }
            return result;
        }
        private bool CheckConnectionDesktop(Socket _sock)
        {
            try
            {
                string sendStr = "Server Information";
                byte[] bs = Encoding.ASCII.GetBytes(sendStr);
                if (_sock.Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。                                                                                  //   -或 - 如果有数据可供读取，则为 true。-或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
                {
                    return false;
                }
                else return true;
            }
            catch (Exception ex) { return false; }
        }
        private void ClearSocketDesktop(string _ip, int port)
        {
            Log("ip " + _ip);
            try
            {
                lock (PoolList_Desktop)
                {
                    for (int i = PoolList_Desktop.Count - 1; i >= 0; i--)
                    {
                        Pools _p = PoolList_Desktop[i];
                        if (_p != null && _p._ip == _ip && _p._Port == port)
                        {
                            string mac = _p._Mac;
                            PoolList_Desktop.RemoveAt(i);
                            if (mac != null || mac != "")
                                DesktopList.Remove(mac);
                            //DesktopIpDynamic.Remove(_ip);

                            if (_p._sock != null && _p._sock.Connected)
                            {
                                _p._sock.Shutdown(SocketShutdown.Both);
                                _p._sock.Close();
                            }
                            if (_p._thr != null) _p._thr.Abort();
                        }
                    }
                }

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                Log(" clear socket exception 1 " + " " + ex.Message + " " + ex.StackTrace);
            }
        }
        #endregion
    }
}
