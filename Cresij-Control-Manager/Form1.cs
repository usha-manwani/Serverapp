using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private static Dictionary<string, StateObject> Clients = new Dictionary<string, StateObject>();
        public static Dictionary<string, DateTime> times = new Dictionary<string, DateTime>();
        public static int connectedClient = 0;
        private static IHubProxy proxy;
        private static HubConnection con;
        private static System.Threading.Timer _timer;
        private static System.Threading.Timer timer;
        private static int totalClients = 0;
        public Form1()
        {
            InitializeComponent();
        }
      
        private void Form1_Load(object sender, EventArgs e)
        {
            //Fill IP grid on the form with the initial status
            FillForm();
            ConnectToHub();
            StartListen(IPAddress.Any.ToString(), 1200);
        }
        #region---启用端口，接受下位机连接,监测下位机状态池---
        Thread threadWatch = null; // 负责监听客户端连接请求的 线程；
        Socket socketWatch = null;
        
        private void StartListen(string _IpAddress, int _Ports)
        {
            // 创建负责监听的套接字，注意其中的参数；
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //初始化的时候，让socket可以进行端口复用,防止服务线程卡死
            socketWatch.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // 获得文本框中的IP对象；
            IPAddress address = IPAddress.Parse(_IpAddress);
            // 创建包含ip和端口号的网络节点对象;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _Ports);
            try
            {
                // 将负责监听的套接字绑定到唯一的ip和端口上；                 
                 socketWatch.Bind(endPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("Error: " + se.Message);
                return;
            }
            // 设置监听队列的长度；  
            socketWatch.Listen(100);
            // 创建负责监听的线程；  
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
        }
        void WatchConnecting()
        {
            while (true)  // 持续不断的监听客户端的连接请求；  
            {
                // 开始监听客户端连接请求，Accept方法会阻断当前的线程；  
                Socket sokConnection = socketWatch.Accept();
                // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字  
                // 将与客户端连接的 套接字 对象添加到集合中；
                //要判断当前连接是否已经在集合中  
                IPEndPoint iep = (IPEndPoint)sokConnection.RemoteEndPoint;
                string _ip = iep.Address.ToString();
                ClearSocket(_ip);
                
                Thread thr = new Thread(RecMsg)
                {
                    IsBackground = true
                };                
                setControl(sokConnection);//设置心跳包
                thr.Start(sokConnection);
                //dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去。
                Pools pl = new Pools(iep.Address.ToString(), sokConnection, thr, DateTime.Now);
                UpdatePool(pl);
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
            catch (ArgumentOutOfRangeException ex)
            {
              //  Console.WriteLine(ex.Message.ToString());
            }
        }
        private void setControl(Socket client)
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
                    byte[] receiveBytes = new byte[1024];
                    // 将接受到的数据存入到输入  arrMsgRec中；  
                    int length = -1;
                    try
                    {                        
                        length = sokClient.Receive(receiveBytes); // 接收数据，并返回数据的长度； 
                        //MessageBox.Show(length.ToString());
                        //string data = "";
                        //for (int j = 0; j < length;)
                        //{
                        //    if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
                        //    {
                        //        byte[] datatoDecode = new byte[4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3]];
                        //        for (int k = 0; k < datatoDecode.Length; k++)
                        //        {
                        //            datatoDecode[k] = receiveBytes[k + j];
                        //        }
                        //        for (int l = 0; l < datatoDecode.Length; l++)
                        //        {
                        //            data = data + datatoDecode[l] + ",";
                        //        }
                        //    }
                        //}
                        //MessageBox.Show(data);
                    }
                    catch (SocketException se)
                    {
                        RemovePool(iep.Address.ToString());
                        break;
                    }
                    catch
                    {
                        RemovePool(iep.Address.ToString());
                        break;
                    }
                   
                      
                    //string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串； 
                    //将Byte数组解析成字符串并添加到历史记录列表
                    //string Command = "";
                    //for (int i = 0; i < length; i++)
                    //    Command += arrMsgRec[i].ToString("X2") + " ";
                    HandleCommand(iep.Address.ToString(), receiveBytes,length);
                }
            }
            catch (ObjectDisposedException)
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
            Pool_list.Add(_pls);
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
                        _x.Destory();
                        _x._thr.Abort();
                        break;
                    }
                }
            }
            catch
            {
                return;
            }
        }
        /// <summary>
        /// 检查Socket是否断开连接
        /// </summary>
        private void CheckSocketConnection()
        {
            if (Pool_list.Count > 0)
            {
                for (int i = Pool_list.Count - 1; i >= 0; i--)
                {
                    string sendStr = "Server Information";
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);
                    if (Pool_list[i]._sock.Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。
                                                                              //   -或 - 如果有数据可供读取，则为 true。-或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
                    {
                        Pool_list[i]._sock.Close();//关闭socket
                        Pool_list[i].Destory();
                        Pool_list.RemoveAt(i);//从列表中删除断开的socke
                        continue;
                    }
                }
            }
        }
        #endregion
        private static void Send(Socket workSocket, byte[] data)
        {
            try
            {
                workSocket.Send(data);               
            }
            catch (Exception ex)
            {

            }
        }
        #region--处理下位机信息--
        private void HandleCommand(string iep, byte[] receiveBytes, int length)
        {
            try
            {
                Decode dd = new Decode();
                string[] final;
                
                for (int j = 0; j < length;)
                {
                    if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
                    {
                        byte[] datatoDecode = new byte[4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3]];
                        for (int k = 0; k < datatoDecode.Length; k++)
                        {
                            datatoDecode[k] = receiveBytes[k + j];
                        }
                        final = dd.Decoded(iep, datatoDecode);
                        for(int i=0; i < ipGrid.Rows.Count; i++)
                        {
                            if (ipGrid[0, i].Value.ToString() == iep && final[1]=="Heartbeat")
                            {
                                for(int m =2; m< ipGrid.ColumnCount; m++)
                                {
                                    ipGrid[m, i].Value = final[m];
                                }
                                break;
                            }
                        }
                        string data = "";
                        for (int l = 0; l < final.Length; l++)
                        {
                            data = data + final[l] + ",";
                        }                        
                        SendMessage(iep, data);
                        j = j + datatoDecode.Length;
                    }
                    else
                    {
                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                //string msg = con.State.ToString();                
            }
        }
        #endregion
        //Fill Grid from database
        private void FillForm()
        {
            GetIPStatus getIPStatus = new GetIPStatus();
            string query = "select * from temp_centralcontrol";
            DataTable dt= getIPStatus.ExecuteCmd(query);
            if (dt.Rows.Count > 0)
            {
                foreach(DataRow dataRow in dt.Rows)
                {
                    ipGrid.Rows.Add();
                    for(int i=0; i < ipGrid.Columns.Count; i++)
                    {
                        ipGrid.Rows[ipGrid.Rows.Count - 1].Cells[i].Value = dataRow[i].ToString();
                    }
                }
            }            
        }

        #region cnnection to website
        //connect to website
        public  void ConnectToHub()
        {
            try
            {
                con = new HubConnection("http://localhost:17263/");
                // con.TraceLevel = TraceLevels.All;
                // con.TraceWriter = Console.Out;                
                proxy = con.CreateHubProxy("myHub");
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
                        lock (Clients)
                        {
                            if (Clients.Count > 0)
                            {
                                foreach (StateObject Client in Clients.Values)
                                {
                                    if (isClientConnected(Client.workSocket))
                                    {
                                        Send(Client.workSocket, data);
                                        MessageBox.Show(((IPEndPoint)Client.workSocket.RemoteEndPoint).ToString());
                                    }
                                    //.Address.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
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
                        lock (Clients)
                        {
                            if (Clients.Count > 0)
                            {
                                if (Clients.TryGetValue(ip, out StateObject state))
                                {
                                    if (isClientConnected(state.workSocket))
                                    {
                                        Send(state.workSocket, dataBytes);
                                        // Console.WriteLine("sent to " + ip);
                                    }
                                    else
                                    {
                                       // SaveStatus(ip, "Offline");
                                    }
                                }
                                else
                                {
                                   // SaveStatus(ip, "Offline");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.Message);
                    }
                });
                proxy.On<string>("RefreshStatus", (ip) =>
                {
                    byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
                    try
                    {
                        lock (Clients)
                        {
                            if (Clients.Count > 0)
                            {
                                if (Clients.TryGetValue(ip, out StateObject state))
                                {
                                    if (isClientConnected(state.workSocket))
                                    {
                                        Send(state.workSocket, data);
                                        // Console.WriteLine("sent to " + ip);
                                    }
                                    else
                                    {
                                       // SaveStatus(ip, "Offline");
                                    }
                                }
                                else
                                {
                                   // SaveStatus(ip, "Offline");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.Message);
                    }
                });

                proxy.On<int>("Counts", i =>
                {
                    SendCounts();
                });
                con.Start().ContinueWith(task => {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error opening the connection:{0}",
                                          task.Exception.GetBaseException());
                    }
                    //else{MessageBox.Show("Connected");}

                }).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
        private static void StartTimer()
        {
            timer = new System.Threading.Timer(new TimerCallback(CheckMachine), null, 60000, 30000);
        }
        private static void CheckMachine(object state)
        {
            List<string> ips = new List<string>();
            DateTime now = DateTime.Now;
            try
            {
                if (times.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> p in times)
                    {
                        TimeSpan span = now - p.Value;
                        if (span.Minutes > 1)
                        {
                          //  SaveStatus(p.Key, "Offline");
                            ips.Add(p.Key);
                        }
                        else
                        {
                           // SaveStatus(p.Key, "Online");
                        }
                    }
                    if (ips.Count > 0)
                    {
                        lock (times)
                        {
                            foreach (string s in ips)
                            {
                                if (times.ContainsKey(s))
                                {
                                    times.Remove(s);
                                    Decode dr = new Decode();
                                    string m = dr.offlineMessage();
                                    SendMessage(s, m);
                                }
                            }
                        }
                        SendCounts();
                    }
                    ips.Clear();
                }
            }
            catch (Exception ex)
            {

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

        public static void RemoveCLient(string ip, Socket handler)
        {
            handler.Close();
            List<string> ips = new List<string>();
            try
            {
                lock (Clients)
                {
                    if (Clients.Count > 0)
                    {
                        foreach (KeyValuePair<string, StateObject> c in Clients)
                        {
                            if (c.Value.workSocket == handler)
                            {
                                //SaveStatus(ip, "Offline");
                                ips.Add(c.Key);
                                SendCounts();

                                Decode dr = new Decode();
                                string m = dr.offlineMessage();
                                SendMessage(ip, m);
                                break;
                            }
                        }
                        if (ips.Count > 0)
                        {
                            foreach (string s in ips)
                            {
                                Clients.Remove(s);

                            }

                        }
                        ips.Clear();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static bool isClientConnected(Socket handler)
        {
            bool status = false;
            try
            {
                status = handler.Connected;
            }
            catch (Exception ex)
            {
                //SaveStatus(((IPEndPoint)handler.RemoteEndPoint).Address.ToString(), "Offline");
            }
            return status;
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
    }
}
