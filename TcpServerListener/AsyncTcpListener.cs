using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DBHelper;
using System.IO;

namespace TcpServerListener
{
    public class AsyncTcpListener
    {
        static string docPath = "logConsoleServerApp.txt";
        protected Instructions inst = new Instructions();
        private static IHubProxy proxy;
        private static HubConnection con;
       
        public static List<StateObject> Machines = new List<StateObject>();
        public static int connectedClient = 0;
       // private static int totalClients = 0;
        private static Timer _timer;
        //private static Timer ValidMachines;
        private static Timer Machinetimer;
        private static Timer StrategyTimer;
        private static Dictionary<string, DateTime> ClientWaitList = new Dictionary<string, DateTime>();
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static async Task ReceiveMacFromDesktop(string ccmac, string instruction,int stid,int stdescid,byte[] instruction1)
        {
            try
            {
                if (Machines.Any(x => x.MacAddress == ccmac))
                {
                    var temp = Machines.Where(x => x.MacAddress == ccmac).FirstOrDefault();
                   
                    StrategyLogs stlogs = new StrategyLogs();
                    if (temp.workSocket.Connected)
                    {
                       
                        Console.WriteLine("---------------------------- sent bytes  " +
                                                            HexEncoding.ToStringfromHex(instruction1) +
                                                            "  Instruction to Mac: " + ccmac + " ----------------------------");

                        // stlogs.SaveStrategyLogInfo(instruction, stid, stdescid, "1", ccmac);
                        Send(temp.workSocket, instruction1);
                        await stlogs.SaveStrategyLogInfo(instruction, stdescid, "Pending", ccmac);
                        File.AppendAllText(docPath, Environment.NewLine + "---------------------------- sent bytes  " +
                                                            HexEncoding.ToStringfromHex(instruction1) +
                                                            "  Instruction to Mac: " + ccmac + " ----------------------------");
                    }
                    else
                        await stlogs.SaveStrategyLogInfo(instruction, stdescid, "Fail", ccmac);
                }
            }
            catch(Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "ReceiveMacFromDesktop : " + ex.StackTrace);
            }
           
        }
        private static void StartTimer()
        {
            Machinetimer = new Timer(new TimerCallback(CheckMachine), null, 60000, 60000);
            int delayStart = (60-DateTime.Now.Second) * 1000;
            
            StrategyTimer = new Timer(new TimerCallback(RunStrategyTimer), null, delayStart, 60000);           
        }
        
        private static void CheckMachine(object state)
        {
            List<string> MachinetoDel = new List<string>();
            try
            {
                lock (ClientWaitList)
                {
                    MachinetoDel = ClientWaitList.Where(x => DateTime.Now.Subtract(x.Value).Minutes > 1).Select(x => x.Key).ToList();
                    foreach (var s in MachinetoDel)
                    {
                        if (Machines.Any(x => x.MacAddress == s))
                        {
                            var temp = Machines.Where(x => x.MacAddress == s).Select(x => x).FirstOrDefault();
                            if (temp != null)
                            {
                                var sock = temp.workSocket;
                                sock.Shutdown(SocketShutdown.Both);
                                Machines.Remove(temp);
                                connectedClient--;
                                ClientWaitList.Remove(s);
                            }
                        }
                    }
                }

                lock (Machines)
                {
                    var AbsoleteSocket = Machines.Where(x => x.MacAddress == "").ToList();
                    foreach(var s in AbsoleteSocket)
                    {
                        var sock = s.workSocket;
                        sock.Shutdown(SocketShutdown.Both);
                        Machines.Remove(s);
                    }
                    var SameMac = Machines.GroupBy(x => x.MacAddress).Select(g => new {Mac=g.Key, count=g.Count() });
                    foreach(var s in SameMac)
                    {
                        if (s.count > 1)
                        {
                            var itemtoRemove = Machines.Where(x => x.MacAddress == s.Mac);
                            foreach(var t in itemtoRemove)
                            {
                                var sock = t.workSocket;
                                sock.Shutdown(SocketShutdown.Both);
                                Machines.Remove(t);
                                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                    DateTime.Now.ToLongTimeString() + "Removed machine because of same mac address : " + s.Mac);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString()+" "+ DateTime.Now.ToLongTimeString() +"Error in Checkmachine : " + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }

        private static void RunStrategyTimer(object state)
        {
            File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToLongTimeString() + "Strategy Timer running at this time");
            Machines.ForEach(x=>Console.WriteLine("ip: "+ ((IPEndPoint)x.workSocket.RemoteEndPoint).Address.ToString()+" Mac Address: "+x.MacAddress));
            StringBuilder record = new StringBuilder();
            foreach (var s in Machines)
            {
                record.AppendLine(JsonSerializer.Serialize("Mac: "+s.MacAddress+ " ip: " + ((IPEndPoint)s.workSocket.RemoteEndPoint).Address.ToString()));
            }
            //Console.WriteLine(DateTime.Now.ToLongDateString() + " " +
            //                    DateTime.Now.ToLongTimeString() + " Machines Connected {0}", record.ToString());
             var tt = DateTime.Now.ToString("HH:mm") + ":00";
           
            try
            {
                StrategyExec strategyExec = new StrategyExec();
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " Connected machines details:  " + record.ToString());

                var ff = strategyExec.GetData(tt);
                
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + "rows from strategy count: "+ ff.Count);
                if (ff.Count > 0)
                {
                    StrategyLogs strategyLogs = new StrategyLogs();
                    Instructions inst = new Instructions();
                    foreach (FinalResult f in ff)
                    {
                        try
                        {
                            var tempIns = inst.GetValues(f.Instruction);
                            var bytes = new byte[2];
                            bytes[0] = (byte)(f.StrategyDescId >> 8);
                            bytes[1] = (byte)f.StrategyDescId;
                            var instruction = new byte[10];
                            for (int k = 0; k < 7; k++)
                            {
                                instruction[k] = tempIns[k];
                            }

                            instruction[7] = bytes[0]; instruction[8] = bytes[1];
                            var checksum = 0;
                            for (int i = 2; i < 9; i++)
                            {
                                checksum = checksum + Convert.ToByte(instruction[i]);
                            }
                            instruction[9] = Convert.ToByte(checksum);
                            File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() +
                                " Strategy Command : " + DateTime.Now.ToLongTimeString() + HexEncoding.ToStringfromHex(instruction));
                            if (f.Instruction == "CloseStrategy")
                            {
                                int r = AsyncDesktopServer.SendToDesktop(f.Ccmac, f.Deskmac, "Shutdown",f.StrategyDescId,f.StrategyId,instruction);
                                if (r == 0)
                                {
                                    Socket t = null;
                                    lock (Machines)
                                    {
                                        if (Machines.Count > 0)
                                        {
                                            if (Machines.Any(x => x.MacAddress == f.Ccmac))
                                            {
                                                t = Machines.Where(x => x.MacAddress == f.Ccmac).Select(x => x.workSocket).FirstOrDefault();

                                            }
                                        }
                                    }
                                    if (t != null)
                                    {
                                        if (isClientConnected(t))
                                        {

                                            strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Pending", f.Ccmac);

                                            Console.WriteLine("---------------------------- sent bytes  " +
                                                HexEncoding.ToStringfromHex(instruction) +
                                                "  Instruction to Mac: " + f.Ccmac + " ----------------------------");

                                            Send(t, instruction);
                                        }
                                        else
                                        {
                                            strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Fail", f.Ccmac);
                                        }
                                    }
                                   
                                    else
                                    {
                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Fail", f.Ccmac);
                                    }
                                }
                            }
                            else
                            {
                                Socket t = null;
                                lock (Machines)
                                {
                                    if (Machines.Count > 0)
                                    {
                                        if (Machines.Any(x => x.MacAddress == f.Ccmac))
                                        {
                                            t = Machines.Where(x => x.MacAddress == f.Ccmac).Select(x => x.workSocket).FirstOrDefault();

                                        }
                                    }
                                }
                                if (t != null)
                                {
                                    if (isClientConnected(t))
                                    {

                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Pending", f.Ccmac);

                                        Console.WriteLine("---------------------------- sent bytes  " +
                                            HexEncoding.ToStringfromHex(instruction) +
                                            "  Instruction to Mac: " + f.Ccmac + " ----------------------------");

                                        Send(t, instruction);
                                    }
                                    else
                                    {
                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Fail", f.Ccmac);
                                    }
                                }
                                else
                                {
                                    strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyDescId, "Fail", f.Ccmac);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToLongTimeString() + "Error in RunStrategyTimer sending instruction to machine : " + ex.StackTrace);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in RunStrategyTimer: " + ex.StackTrace);
                Console.WriteLine(ex.Message);
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
            _timer = new Timer(x =>
            {
                action.Invoke();
            }, null, timeToGo, every);
        }

        public static void StartListening()
        {
            ConnectToHub();
            StartTimer();
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".                       
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1200);
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
                    Console.WriteLine("Waiting for a Machine connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            
            catch (Exception e)
            {
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ".............Error in TCP Listerner start : " + e.StackTrace);
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            // Console.Read();
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
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in is client conencted : " + ex.StackTrace);
                Console.WriteLine("CLient Connection lost with exception message   " + ex.Message);
            }
            return status;
        }       

        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket handler;
            try
            {
                connectedClient++;
                // Signal the main thread to continue.  
                allDone.Set();
                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                handler = listener.EndAccept(ar);
                listener.ReceiveTimeout = 3500;
                string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();

                // Create the state object.  
                StateObject state = new StateObject();
                state.buffer = new byte[StateObject.BufferSize];
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ip +" connected");
                Console.WriteLine(" Total Connected Machine client : " + connectedClient);
                state.workSocket = handler;
                if (!Machines.Contains(state))
                {
                    Machines.Add(state);
                }
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Added new machine in AcceptCallBack: ");
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                Instructions inst = new Instructions();
                var insdata = inst.GetValues("MacAddress");
                Send(handler, insdata);
            }
            
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in AcceptCallBack: " + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            Socket handler = null;
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                handler = state.workSocket;

            int bytesRead;
            // Read data from the client socket.   

            int i = 0;
            //string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
            if (handler!=null && handler.Connected)
            {
                //Console.WriteLine("ip   " + ip);
                
                    bytesRead = handler.EndReceive(ar);
                    byte[] bytes = new byte[bytesRead];
                    if (bytesRead > 0)
                    {
                        for (i = 0; i < bytesRead; i++)
                        {
                            bytes[i] = state.buffer[i];
                        }
                        DecodeData(handler, bytes, bytesRead);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
                
            }
            catch (SocketException se)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Socket error in ReadCallBack: " + se.StackTrace);
                if (se.ErrorCode == 10054 || ((se.ErrorCode != 10004) && (se.ErrorCode != 10053)))
                {
                    var temp = Machines.Where(x => x.workSocket == handler).FirstOrDefault();
                    handler.Close();
                    if (temp != null)
                    {
                        lock (Machines)
                        {
                            Console.WriteLine("Client automatically disconnected");
                            Decode dr = new Decode();
                            Dictionary<string, object> result = dr.OfflineMessage();
                            SendMessage(temp.MacAddress, result);
                            Machines.Remove(temp);
                            connectedClient--;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in ReadCallback: " + ex.StackTrace);
            }
        }

        private static int DecodeData(Socket sock, byte[] receiveBytes, int length)
        {
            var mac = "";
            try
            {
                Decode dd = new Decode();
                Dictionary<string, object> final;
                string[] status;
                Dictionary<string, object> re = new Dictionary<string, object>();
                for (int j = 0; j < length;)
                {
                    if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
                    {
                        int len = 4+ (256 * receiveBytes[j + 2]) + receiveBytes[j + 3];
                        byte[] datatoDecode = receiveBytes.Skip(j).Take(len).ToArray();
                        
                        status = new string[7];

                        status[0] = mac;
                        for (int i = 1; i < 7; i++)
                        {
                            status[i] = "Off";
                        }
                        //}
                        re = dd.Decoded("", datatoDecode, status);
                         
                            Console.WriteLine("------------------" + DateTime.Now.ToLongDateString() + " " +
                                   DateTime.Now.ToLongTimeString() + "bytes Received from: " + mac +
                                   " message: " + HexEncoding.ToStringfromHex(datatoDecode));
                            Console.WriteLine();
                        
                        // dd = null;
                        if (re.Count == 2)
                        {
                            object obj = re["data"];
                            final = obj as Dictionary<string, object>;
                            if (final.ContainsKey("Type"))
                            {
                                if (final["Type"].ToString() == "MacAddress")
                                {
                                    var temp = final["Data"] as Dictionary<string, string>;
                                    if (Machines.Any(x => x.workSocket == sock))
                                    {
                                        var MachineListobj = Machines.Where(x => x.workSocket == sock).Select(x => x).FirstOrDefault();
                                        MachineListobj.MacAddress = temp["MacAddress"];
                                    }

                                }
                               
                            }
                            mac = Machines.Where(x => x.workSocket == sock).Select(x => x.MacAddress).FirstOrDefault();
                            if (mac != "" && mac != null)
                            {
                                lock (ClientWaitList)
                                {
                                    if (!ClientWaitList.ContainsKey(mac))
                                        ClientWaitList.Add(mac, DateTime.Now);
                                    else
                                        ClientWaitList[mac] = DateTime.Now;
                                }
                                if (final["Type"].ToString() == "Strategy")
                                {                                   
                                    StrategyLogs strategyLogs = new StrategyLogs();
                                    strategyLogs.UpdateStrategyStatus(final["Device"].ToString(), mac, Convert.ToInt32(final["StrategyDescId"]),"Success");
                                }
                            }
                            
                               
                            Console.WriteLine(DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToLongTimeString() + "Decoded version of Message Received from: " + mac +
                                " message: " + JsonSerializer.Serialize(final));
                            Console.WriteLine("---------------------------");
                            Console.WriteLine();
                            SendMessage(mac, final);
                            if (final["Type"].ToString() != "Heartbeat")
                            {
                                try
                                {
                                    File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToLongTimeString() + " Message Received from: " + mac +
                                " message: " + JsonSerializer.Serialize(final));
                                }
                                catch (Exception ex)
                                {

                                    File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                               DateTime.Now.ToLongTimeString() + "Error in writing the file: " + mac +
                               " message: " + JsonSerializer.Serialize(final));
                                }

                            }
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
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in DecodeData form machin : " + ex.StackTrace);
                Console.WriteLine(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "  exception in Handle message  " + ex.Message + " stack trace " + ex.StackTrace + " "
                 + ex.GetError() + "from mac : " + mac);
                //string msg = con.State.ToString();
            }
            return 0;
        }

        private static void Send(Socket handler, byte[] byteData)
        {
            try
            {
                Console.WriteLine("machine instruction sent to machine ip : " + ((IPEndPoint)handler.RemoteEndPoint).Address.ToString() );
                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (SocketException socex)
            {
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Send: " + socex.StackTrace); 
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {

                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in SendCallBack: " + e.StackTrace);
            }
        }

        #region connection to website
        //connect to website
        public static void ConnectToHub()
        {
            try
            {
                con = new HubConnection("http://localhost/");
                // con.TraceLevel = TraceLevels.All;
                // con.TraceWriter = Console.Out;
                proxy = con.CreateHubProxy("myHub");
                // MessageBox.Show("create proxy hub called");
                proxy.On<int>("SendToMachine", i =>
                {
                    try
                    {

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
                    try
                    {
                        File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                           DateTime.Now.ToLongTimeString() + "---------------------------- from web client :  " +
                                                 data +
                                                 "  Instruction to Mac: " + mac + " ----------------------------");
                       Instructions inst = new Instructions();
                        byte[] ins = inst.GetValues(data);
                        if (Machines.Any(x => x.MacAddress == mac))
                        {
                            var sock = Machines.Where(x => x.MacAddress == mac).Select(x => x.workSocket).FirstOrDefault();
                            if (isClientConnected(sock))
                            {
                                Console.WriteLine("---------------------------- sent bytes  " +
                                                 HexEncoding.ToStringfromHex(inst.GetValues(data)) +
                                                 "  Instruction to Mac: " + mac + " ----------------------------");
                                Send(sock, ins);
                            }
                        }
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + 
                            DateTime.Now.ToLongTimeString() + "Error in SendControl to Machine : " + 
                            ex.StackTrace+ " data from website "+data+" to mac: "+mac);
                        // Console.WriteLine(ex.Message);
                    }
                });
                proxy.On<string>("RefreshStatus", (mac) =>
                {
                    byte[] data = new byte[] { 0x8B, 0xB9, 0x00, 0x03, 0x05, 0x01, 0x09 };
                    try
                    {

                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        // Console.WriteLine(ex.Message);
                    }
                });
                proxy.On<int>("CountsMachines", i =>
                {
                    SendCounts();
                });
                con.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error opening the connection with WebClient");
                    }
                    //else{MessageBox.Show("Connected to signalR");}
                }).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("not connected to WebClient " + ex.Message);
                con.StateChanged += Con_StateChanged;
            }
        }

        private static async Task StartCon()
        {
            await con.Start();
        }
        private static void Con_StateChanged(StateChange obj)
        {
            if (obj.OldState == ConnectionState.Disconnected)
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
        public static void SendMessage(string sender, Dictionary<string, object> message)
        {
            Dictionary<string, object> message1 = new Dictionary<string, object>();
            var d= JsonSerializer.Serialize(message);
            //message1.Add("test", "success");
            try
            {
                if (con.State != ConnectionState.Connected)
                {
                    // Console.WriteLine("connecting to server");
                    con.Start().Wait();
                    // Console.WriteLine("connected");
                }
                proxy.Invoke("SendMessage", sender, d);
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
            proxy.Invoke("CountMachines", Machines.Count);
        }
        #endregion
      
    }

    
}
