// ***********************************************************************
// Assembly         : TcpServerListener
// Author           : admin
// Created          : 04-02-2021
//
// Last Modified By : admin
// Last Modified On : 04-08-2021
// ***********************************************************************
// <copyright file="AsyncDesktopServer.cs" company="">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************
using DBHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog;		   

namespace TcpServerListener
{
    /// <summary>
    /// Class WaitListClass.
    /// contains details of strategy and ccmac 
    /// </summary>
    public class WaitListClass
    {
        /// <summary>
        /// Gets or sets the timeof execute.
        /// </summary>
        /// <value>The timeof execute.</value>
        public DateTime TimeofExec { get; set; }
        /// <summary>
        /// Gets or sets the strategyid.
        /// </summary>
        /// <value>The strategyid.</value>
        public int Strategyid { get; set; }
        /// <summary>
        /// Gets or sets the strategy desc identifier.
        /// </summary>
        /// <value>The strategy desc identifier.</value>
        public int StrategyDescId { get; set; }
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; set; }
        /// <summary>
        /// Gets or sets the ccmac.
        /// </summary>
        /// <value>The ccmac.</value>
        public string Ccmac { get; set; }
        /// <summary>
        /// Gets or sets the equipment identifier.
        /// </summary>
        /// <value>The equipment identifier.</value>
        public int EquipmentId { get; set; }
        /// <summary>
        /// Gets or sets the inst.
        /// </summary>
        /// <value>The inst.</value>
        public byte[] Inst { get; set; }
    }
    /// <summary>
    /// Class StateObject.
    /// </summary>
    public class StateObject
    {
        // Client  socket.  
        /// <summary>
        /// The work socket
        /// </summary>
        public Socket workSocket = null;
        // Size of receive buffer.  
        /// <summary>
        /// The buffer size
        /// </summary>
        public const int BufferSize = 512;
        // Receive buffer.  
        /// <summary>
        /// The buffer
        /// </summary>
        public byte[] buffer;
        // Received data string.  
        /// <summary>
        /// The sb
        /// </summary>
        public StringBuilder sb = new StringBuilder();
        /// <summary>
        /// The mac address
        /// </summary>
        public string MacAddress = "";
    }
    /// <summary>
    /// Class AsyncDesktopServer.
    /// </summary>
    public class AsyncDesktopServer
    {
        /// <summary>
        /// The logger file
        /// </summary>
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The dictionary of WaitListClass with key as central control machine mac(ccmac)
        /// </summary>
        public static Dictionary<string, WaitListClass> WaitList = new Dictionary<string, WaitListClass>();
       
        /// <summary>
        /// The connected client
        /// </summary>
        public static int connectedClient = 0;
        /// <summary>
        /// The list of desktop clients connected
        /// </summary>
        public static Dictionary<Socket, StateObject> Clients = new Dictionary<Socket, StateObject>();
        /// <summary>
        /// The list of desktop mac as key and ccmac as value
        /// </summary>
        public static Dictionary<string, string> DesktopList = new Dictionary<string, string>();
        //private static System.Timers.Timer IPStatusTimer;
        /// <summary>
        /// The total clients
        /// </summary>
        private static int totalClients = 0;
        /// <summary>
        /// The test timer
        /// </summary>
        private static Timer _testTimer;
        /// <summary>
        /// The timer
        /// </summary>
        private static Timer _timer;
        /// <summary>
        /// Sets the timer.
        /// </summary>
        /// <param name="starTime">The star time.</param>
        /// <param name="every">The every.</param>
        /// <param name="action">The action.</param>
        private static void SetTimer(TimeSpan starTime, TimeSpan every, Func<Task> action)
        {
            var current = DateTime.Now;
            var timeToGo = starTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                return;
            }
            _timer = new Timer(x =>
            {
                action.Invoke();
            }, null, timeToGo, every);
        }
        /// <summary>
        /// Starts the test timer.
        /// </summary>
        private static void StartTestTimer()
        {
            int delayStart = (60 - DateTime.Now.Second) * 1000;

            _testTimer = new Timer(new TimerCallback(RunTestTimer), null, delayStart, 60000);
        }
        /// <summary>
        /// Runs the test timer.
        /// THis timer is for strategymode='test'
        /// </summary>
        /// <param name="state">The state.</param>
        private static void RunTestTimer(object state)
        {
            StrategyExec se = new StrategyExec();
            var data =se.GetTestTimeData();
            
            if (data.Count > 0)
            {
                loggerFile.Debug("total desktop client connected when test mode is running: " 
                    + Clients.Count);
                try
                {
                    foreach (var s in data)
                    {
                        Dictionary<string, object> DatatoSend = new Dictionary<string, object>();
                        DatatoSend.Add("Type", "Command");
                        DatatoSend.Add("CCmac", s.CCmac.ToUpper());
                        DatatoSend.Add("Deskmac", s.Deskmac.ToUpper());
                        DatatoSend.Add("publishTexts", s.PublishText);
                        DatatoSend.Add("publishTitle", s.PublishTitle);
                        DatatoSend.Add("startTime", s.StartTime);
                        DatatoSend.Add("endTime", s.EndTime);
                        DatatoSend.Add("Code", s.Code);
                        DatatoSend.Add("Subject", s.Subject);
                        var da = JsonConvert.SerializeObject(DatatoSend);
                        var sock = Clients.Where(x => x.Value.MacAddress == s.Deskmac.ToUpper())
                            .Select(x => x.Value.workSocket).FirstOrDefault();
                        if (sock != null)
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(da);
                            loggerFile.Debug("deskmac: " + s.Deskmac + " data: " +
                                da);
                            Send(sock, bytes);
                        }
                        DatatoSend.Clear();
                    }
                }
                catch(Exception ex)
                {
                    loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " 
                        + DateTime.Now.ToLongTimeString() + "Error in test mode sending data to desktop  " 
                        + ex.Message +" stack trace :"+ex.StackTrace+" Inner exception: "+ ex.InnerException);
                }
            }
        }
        // Thread signal.  
        /// <summary>
        /// All done
        /// </summary>
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// Starts the tcp server listener to listen to desktop clients.
        /// </summary>
        public static void StartListening()
        {
            var current = DateTime.Now.TimeOfDay;
            SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(25), CheckShutdownInstruction);
            StartTestTimer();
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer                                    
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 10008);
            // Create a TCP/IP socket.  
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(200);
                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a Desktop connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
              //  Log("Error in Desktop server listening: " + e.StackTrace);
                loggerFile.Debug(e.Message);
            }
        }

        /// <summary>
        /// Accepts new connections.
        /// </summary>
        /// <param name="ar">contains the state of socket</param>
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                
                // Signal the main thread to continue.  
                allDone.Set();
                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                listener.ReceiveTimeout = 3500;
                string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();

                // Create the state object.  
                StateObject state = new StateObject();
                state.buffer = new byte[StateObject.BufferSize];
                lock (Clients)
                {
                    if (!Clients.ContainsKey(handler))
                    {
                        
                        Clients.Add(handler, state);
                        totalClients = Clients.Count;
                        connectedClient++;
                    }                        
                }
                state.workSocket = handler;
                loggerFile.Debug("connected desktop client total : "+connectedClient);
               loggerFile.Debug("connected desktop client Ip Address : " + ip);
                Console.WriteLine(" Total Connected desktop client : " + connectedClient);
                Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
                DatatoSend.Add("Type", "Reply");
                DatatoSend.Add("Status", "Connected");
                var s = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
                byte[] b = Encoding.ASCII.GetBytes(s);
                Send(handler, b);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                //Log("Error in Accept call back: " + ex.StackTrace);
               loggerFile.Debug(ex);
            }
        }

        /// <summary>
        /// Reads the bytes from socket
        /// </summary>
        /// <param name="ar">The ar.</param>
        public static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead;
            // Read data from the client socket.   

            int i = 0;
            //tring ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
            if (handler.Connected)
            {
                try
                {
                    bytesRead = handler.EndReceive(ar);
                    byte[] bytes = new byte[bytesRead];
                    if (bytesRead > 0)
                    {
                        for (i = 0; i < bytesRead; i++)
                        {
                            bytes[i] = state.buffer[i];
                        }
                        var data = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                        var received = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(data);
                        DecodeDataDesktop(received, handler);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
                catch (SocketException se)
                {
                    //Log("Error in ReadCallback : " + se.StackTrace+" details: "+se.InnerException.TargetSite);
                    if (se.ErrorCode == 10054 || ((se.ErrorCode != 10004) && (se.ErrorCode != 10053)))
                    {
                        handler.Close();
                        lock (Clients)
                        {
                            if (Clients.Count > 0)
                            {
                                foreach (KeyValuePair<Socket, StateObject> c in Clients)
                                {
                                    if (c.Key == handler)
                                    {
                                        Clients.Remove(c.Key);
                                        connectedClient--;
                                        loggerFile.Debug( " Socket exception, disconnected desktop client  " + c.Value.MacAddress);
                                        Console.WriteLine("Client automatically disconnected");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    loggerFile.Debug(ex);
                }
            }
        }
        /// <summary>
        /// Decodes the data desktop.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="sock">The sock.</param>
        /// please look into the method for data reference
        /// <returns>System.Int32.</returns>
        private static async Task<int> DecodeDataDesktop(Dictionary<string, string> data, Socket sock)
        {
            GetMacAddress gt = new GetMacAddress();
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            KeyValuePair<string, string> r = new KeyValuePair<string, string>();
            try
            {
                if (data["Type"] == "MacAddress")
                {
                    //Log("mac received " + data["value"]);
                    var mac = data["value"].Split(',');
                   
                    r = gt.GetMac(mac);
                    if (r.Key != null)
                    {
                        loggerFile.Debug("mac from database: "+r.Key +" mac of machine : "+ r.Value);
                        if (!DesktopList.Contains(r))
                        {
                            DesktopList.Add(r.Key, r.Value);
                        }
                        var client = new List<KeyValuePair<Socket,StateObject>>();
                        if (Clients.Any(x => x.Value.MacAddress == r.Key.ToUpper()))
                        {
                            client =Clients.Where(x => x.Value.MacAddress == r.Key.ToUpper()).ToList();
                        }
                        // loggerFile.Debug("total clients by same mac addresses: " + client.Count());
                        if (client.Count() > 0)
                        {
                            foreach (var c in client)
                            {
                                ClearSocketDesktop(c.Value.workSocket);
                            }
                        }
                        if (Clients.Any(x => x.Value.workSocket == sock))
                        {
                            var cc = Clients.Where(x => x.Value.workSocket == sock).FirstOrDefault();
                            cc.Value.MacAddress = r.Key.ToUpper();
                        }
                        DatatoSend.Add("Type", "MacAddress");
                        DatatoSend.Add("Deskmac", r.Key);
                        var s = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.ASCII.GetBytes(s);
                        Send(sock, b);
                        loggerFile.Debug("total clients Desktop CLients: " + Clients.Count());
                        
                           loggerFile.Debug("mac from desktop key: " + r.Key + " value: " + r.Value);
                        
                    }
                    else
                    {
                        DatatoSend.Add("Type", "Reply");
                        DatatoSend.Add("Status", "404");
                        var s = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.ASCII.GetBytes(s);
                        Send(sock, b);
                        ClearSocketDesktop(sock);
                        try
                        {
                            loggerFile.Debug( Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                            DateTime.Now.ToLongTimeString() + " desktop mac from database key: " + 
                            r.Key + " value: " + r.Value);
                        }
                        catch (Exception) { }
                    }
                    //SendToDesktop(r.Key, r.Value, "Shutdown");
                }
                if (data["Type"] == "Command")
                {
                    var code = data["Code"];
                    var ccmac = data["CCmac"].ToUpper();
                    var deskmac = data["Deskmac"].ToUpper();
                    
                    if (code == "Shutdown")
                    {
                        try
                        {
                            if (WaitList.ContainsKey(ccmac))
                            {
                                var d = WaitList.Where(x => x.Key == ccmac).Select(x => x.Value).FirstOrDefault();
                                if (d != null)
                                {
                                    var stid = d.Strategyid;
                                    var stdescid = d.StrategyDescId;
                                    var instr = d.Inst;
                                    var equipid = d.EquipmentId;
                                  await  AsyncTcpListener.ReceiveMacFromDesktop(ccmac, "CloseStrategy", stid, stdescid, instr,equipid);
                                }
                                WaitList.Remove(ccmac);
                            }
                            
                            DatatoSend.Clear();
                            DatatoSend.Add("Type", "Command");
                            DatatoSend.Add("Code", "ExeShutdown");
                            DatatoSend.Add("CCmac", ccmac.ToUpper());
                            DatatoSend.Add("Deskmac", deskmac.ToUpper());
                            //DatatoSend.Add("isDev","on");
                            var t = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
                            
                            byte[] b = Encoding.ASCII.GetBytes(t);
                            Send(sock, b);
                            //loggerFile.Debug(DateTime.Now.ToLongTimeString() + " data sent to desktop client: " + t);
                            //loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "received " + code + " for machine with mac " + ccmac);
                            //loggerFile.Debug(Environment.NewLine + " " + DateTime.Now.ToLongTimeString() + " data sent to desktop client: " + t);
                        }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                        catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                        {
                            loggerFile.Debug(Environment.NewLine + " " + DateTime.Now.ToLongTimeString() + 
                                " Decode Desktop Data exception under send data to machine " + " " + ex.Message + " " + ex.StackTrace);
                            // Console.WriteLine(ex.Message);
                        }
                    }
                }

                if (data["Type"] == "DesktopEvent")
                {
                    var deskmac = data["Deskmac"].ToString();
                    var action = data["Action"].ToString();
                    int affectedRows= await gt.SaveInactiveDesktopAsync(deskmac, action);
                    var mes = new Dictionary<string, string>
                    {
                        { "Action", action },{ "Type","DesktopEvent"}
                    };
                    AsyncTcpListener.SendDesktopEventToWebsocket(deskmac,mes);
                }
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data.ToList()));
            }
            catch (Exception ex)
            {
                //Log("Error in Decode Desktop data: " + ex.StackTrace);
                loggerFile.Debug(Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " Decode Desktop Data exception 2 " + " " + ex.Message + " " + ex.StackTrace);
                //  ClearSocketDesktop(ip, ((IPEndPoint)sock.RemoteEndPoint).Port);
            }
            return 1;
        }
        /// <summary>
        /// Determines whether [is client connected] [the specified handler].
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns><c>true</c> if [is client connected] [the specified handler]; otherwise, <c>false</c>.</returns>
        public static bool isClientConnected(Socket handler)
        {
            bool status = false;
            try
            {
                status = handler.Connected;
            }
            catch (Exception ex)
            {
               loggerFile.Debug( Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in isCLientConnected Desktop: " + ex.StackTrace);
                Console.WriteLine("CLient Connection lost with exception message   " + ex.Message);
            }
            return status;
        }

        /// <summary>
        /// Method to Send data to desktop.
        /// </summary>
        /// <param name="ccmac">The ccmac.</param>
        /// <param name="deskmac">The deskmac.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="strategyid">The strategyid.</param>
        /// <param name="strategydeskid">The strategydeskid.</param>
        /// <param name="inst">The inst.</param>
        /// <param name="equipid">The equipid.</param>
        /// <returns>System.Int32.</returns>
        public static int SendToDesktop(string ccmac, string deskmac, string instruction,int strategyid,int strategydeskid, byte[] inst, int equipid)
        {
            int result = 0;
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            DatatoSend.Add("Type", "Command");
            DatatoSend.Add("Code", instruction);
            DatatoSend.Add("CCmac", ccmac.ToUpper());
            DatatoSend.Add("Deskmac", deskmac.ToUpper());
            //DatatoSend.Add("isDev", "on");
            var s = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
            loggerFile.Debug("data sending to desktop client: " + s);
            try
            {                
                var obj = Clients.Where(x => x.Value.MacAddress == deskmac.ToUpper()).Select(x => x.Value.workSocket).FirstOrDefault();
                if (obj != null)
                {
                    byte[] b = Encoding.ASCII.GetBytes(s);
                    Send(obj, b);
                    if (!WaitList.ContainsKey(ccmac.ToUpper()))
                    {
                        var w = new WaitListClass()
                        {
                            StrategyDescId = strategydeskid,
                            Strategyid = strategyid, TimeofExec = DateTime.Now,
                            Command = instruction,
                            Ccmac = ccmac.ToUpper(),
                            Inst=inst,
                            EquipmentId=equipid
                        };
                        WaitList.Add(ccmac.ToUpper(), w);
                    }
                        
                    result = 1;
                }
                else { result = 0; }
            }
            catch(Exception ex)
            {
                if (!WaitList.ContainsKey(ccmac.ToUpper()))
                {
                    var w = new WaitListClass()
                    {
                        StrategyDescId = strategydeskid,
                        Strategyid = strategyid,
                        TimeofExec = DateTime.Now,
                        Command = instruction,
                        Ccmac=ccmac.ToUpper(),
                        Inst = inst,
                        EquipmentId=equipid
                    };
                    WaitList.Add(ccmac.ToUpper(), w);
                }
              //  loggerFile.Debug( Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Sendtodesktop: " + ex.StackTrace);
            }
            return result;
        }
        /// <summary>
        /// Sends the bytes to specified handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="byteData">The byte data.</param>
        private static void Send(Socket handler, byte[] byteData)
        {
            try
            {
                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (SocketException socex)
            {
               loggerFile.Debug( Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Send Desktop: " + socex.StackTrace);
                ClearSocketDesktop(handler);
                loggerFile.Debug(socex.Message);
            }
        }

        /// <summary>
        /// call back function for sending the bytes to client
        /// </summary>
        /// <param name="ar">The ar.</param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device. 
                if (handler.Connected)
                {
                    int bytesSent = handler.EndSend(ar);
                    loggerFile.Debug("Sent {0} bytes to desktop client.", bytesSent);
                    //File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() +
                   //     " " + DateTime.Now.ToLongTimeString() + "Bytes sent to client : " + bytesSent);
                    Clients.Where(x => x.Key == handler).Select(y => y.Value.MacAddress).ToList().ForEach(Console.WriteLine);
                }
               
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                loggerFile.Debug( Environment.NewLine + DateTime.Now.ToLongDateString() +
                          " " + DateTime.Now.ToLongTimeString() + "Error in sending data to desktop client "+ e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// Disconnect the socket desktop client.
        /// </summary>
        /// <param name="sock">The sock.</param>
        private static void ClearSocketDesktop(Socket sock)
        {
            try
            {
                lock (Clients)
                {
                    if (Clients.ContainsKey(sock))
                    {
                        var client = Clients[sock];
                        var deskmac = client.MacAddress;
                        if (DesktopList.ContainsKey(deskmac))
                            DesktopList.Remove(deskmac);
                        Clients.Remove(sock);
                        connectedClient--;                        
                        sock.Shutdown(SocketShutdown.Both);
                    }
                }

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
              //  Log("Error in ClearSocket Desktop: " + ex.StackTrace);
                loggerFile.Debug( Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " clear socket exception 1 " + " " + ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Checks if the shutdwon instruction is sent to desktop client or not
        /// </summary>
        private static async Task CheckShutdownInstruction()
        {
            WaitListClass data = null;
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            try
            {
                lock (WaitList)
                {
                    var val = WaitList.Select(x => x.Value);
                   
                    var machineMacList = WaitList.Where(x => DateTime.Now.Subtract(x.Value.TimeofExec).TotalSeconds >= 35).Select(x => x.Key).ToList();
                    foreach (var m in machineMacList)
                    {
                        data = WaitList.Where(x => x.Key == m).Select(x=>x.Value).FirstOrDefault();
                        WaitList.Remove(m);
                        var deskmac = DesktopList.Where(x => x.Value == m).Select(x => x.Key).FirstOrDefault();
                        DatatoSend.Add("Type", "Command");
                        DatatoSend.Add("Code", "ExeShutdown");//
                        DatatoSend.Add("CCmac", m.ToUpper());
                        DatatoSend.Add("Deskmac", deskmac.ToUpper());
                        var t = System.Text.Json.JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.UTF8.GetBytes(t);
                        var sock = Clients.Where(x => x.Value.MacAddress == deskmac).Select(x => x.Value.workSocket).FirstOrDefault();
                        Send(sock, b);                       
                    }
                }
                if (data != null)
                {
                    var stid = data.Strategyid;
                    var stdescid = data.StrategyDescId;
                    var instr = data.Inst;
                    var equipid = data.EquipmentId;
                    await AsyncTcpListener.ReceiveMacFromDesktop(data.Ccmac, "CloseStrategy", stid, stdescid,instr,equipid);
                }
            }
            catch (Exception ex)
            {
               loggerFile.Debug( Environment.NewLine+ DateTime.Now.ToLongDateString() + " " 
                   + DateTime.Now.ToLongTimeString() + " Error Message in CheckShutdownInstruction() : " +
                   ex.Message+"  inner details : "+ex.InnerException+" stack trace: " + ex.StackTrace);
            }
            
        }
    }
}
