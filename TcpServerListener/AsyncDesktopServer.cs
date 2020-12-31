using DBHelper;
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

namespace TcpServerListener
{
    public class WaitListClass
    {
        public DateTime TimeofExec { get; set; }
        public int Strategyid { get; set; }
        public int StrategyDescId { get; set; }
        public string Command { get; set; }
        public string Ccmac { get; set; }
        public int EquipmentId { get; set; }
        public byte[] Inst { get; set; }
    }
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer;
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public string MacAddress = "";
    }

    public class AsyncDesktopServer
    {
        
        static string docPath = "logConsoleServerApp.txt";
        public static readonly string constr = "Integrated Security=SSPI;Persist Security Info=False;" +
            "Data Source=WIN-OTVR1M4I567\\SQLEXPRESS;Initial Catalog=CresijCam";

        public static Dictionary<string, WaitListClass> WaitList = new Dictionary<string, WaitListClass>();
        public static Dictionary<string, int> IPStatus = new Dictionary<string, int>();
        public static int connectedClient = 0;
        public static Dictionary<Socket, StateObject> Clients = new Dictionary<Socket, StateObject>();
        public static Dictionary<string, string> DesktopList = new Dictionary<string, string>();
        //private static System.Timers.Timer IPStatusTimer;
        private static int totalClients = 0;

        private static Timer _timer;
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
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        //public AsyncDesktopServer()
        //{
        //    var current = DateTime.Now.TimeOfDay;
        //    SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(10), CheckShutdownInstruction);
        //}

        public static void StartListening()
        {
            var current = DateTime.Now.TimeOfDay;
            SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(25), CheckShutdownInstruction);
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            //// running the listener is "host.contoso.com".                       
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
                Console.WriteLine(e.ToString());
            }
        }

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
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "connected desktop client total : "+connectedClient);
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + 
                    DateTime.Now.ToLongTimeString() + "connected desktop client Ip Address : " + ip);
                Console.WriteLine(" Total Connected desktop client : " + connectedClient);
                Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
                DatatoSend.Add("Type", "Reply");
                DatatoSend.Add("Status", "Connected");
                var s = JsonSerializer.Serialize(DatatoSend);
                byte[] b = Encoding.ASCII.GetBytes(s);
                Send(handler, b);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                //Log("Error in Accept call back: " + ex.StackTrace);
                // Console.WriteLine(ex.Message);
            }
        }

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
                        var received = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
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
                                        File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " Socket exception, disconnected desktop client  " + c.Value.MacAddress);
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
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private static async Task<int> DecodeDataDesktop(Dictionary<string, string> data, Socket sock)
        {
           
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            KeyValuePair<string, string> r = new KeyValuePair<string, string>();
            try
            {
                if (data["Type"] == "MacAddress")
                {
                    //Log("mac received " + data["value"]);
                    var mac = data["value"].Split(',');
                    GetMacAddress gt = new GetMacAddress();
                    r = gt.GetMac(mac);
                    if (r.Key != null)
                    {
                        Console.WriteLine("mac from database: "+r.Key +"mac : "+ r.Value);
                        if (!DesktopList.Contains(r))
                        {
                            DesktopList.Add(r.Key, r.Value);
                        }
                        var client = new List<KeyValuePair<Socket,StateObject>>();
                        if (Clients.Any(x => x.Value.MacAddress == r.Key.ToUpper()))
                        {
                            client =Clients.Where(x => x.Value.MacAddress == r.Key.ToUpper()).ToList();
                        }
                        // Console.WriteLine("total clients by same mac addresses: " + client.Count());
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
                        Console.WriteLine("total clients Desktop CLients: " + Clients.Count());
                        File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "mac from desktop key: " + r.Key + " value: " + r.Value);
                    }
                    else
                    {
                        File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "mac from database key: " + r.Key + " value: " + r.Value);
                        DatatoSend.Add("Type", "Reply");
                        DatatoSend.Add("Status", "404");

                        var s = JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.ASCII.GetBytes(s);
                        Send(sock, b);
                        ClearSocketDesktop(sock);
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
                        //Instructions ins = new Instructions();
                        //d = "CloseStrategy";

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
                            var t = JsonSerializer.Serialize(DatatoSend);
                            
                            byte[] b = Encoding.ASCII.GetBytes(t);
                            Send(sock, b);
                            //Console.WriteLine(DateTime.Now.ToLongTimeString() + " data sent to desktop client: " + t);
                            //File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "received " + code + " for machine with mac " + ccmac);
                            //File.AppendAllText(docPath, Environment.NewLine + " " + DateTime.Now.ToLongTimeString() + " data sent to desktop client: " + t);
                        }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                        catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                        {
                            File.AppendAllText(docPath, Environment.NewLine + " " + DateTime.Now.ToLongTimeString() + 
                                " Decode Desktop Data exception under send data to machine " + " " + ex.Message + " " + ex.StackTrace);
                            // Console.WriteLine(ex.Message);
                        }
                    }
                }

                Console.WriteLine(JsonSerializer.Serialize(data.ToList()));
            }
            catch (Exception ex)
            {
                //Log("Error in Decode Desktop data: " + ex.StackTrace);
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " Decode Desktop Data exception 2 " + " " + ex.Message + " " + ex.StackTrace);
                //  ClearSocketDesktop(ip, ((IPEndPoint)sock.RemoteEndPoint).Port);
            }
            return 1;
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
               File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in isCLientConnected Desktop: " + ex.StackTrace);
                Console.WriteLine("CLient Connection lost with exception message   " + ex.Message);
            }
            return status;
        }

        public static int SendToDesktop(string ccmac, string deskmac, string instruction,int strategyid,int strategydeskid, byte[] inst, int equipid)
        {
            int result = 0;
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            DatatoSend.Add("Type", "Command");
            DatatoSend.Add("Code", instruction);
            DatatoSend.Add("CCmac", ccmac.ToUpper());
            DatatoSend.Add("Deskmac", deskmac.ToUpper());
            //DatatoSend.Add("isDev", "on");
            var s = JsonSerializer.Serialize(DatatoSend);
            Console.WriteLine("data sending to desktop client: " + s);
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
              //  File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Sendtodesktop: " + ex.StackTrace);
            }
            return result;
        }
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
               File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Send Desktop: " + socex.StackTrace);
                ClearSocketDesktop(handler);
                Console.WriteLine(socex.Message);
            }
        }

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
                    Console.WriteLine("Sent {0} bytes to desktop client.", bytesSent);
                    //File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() +
                   //     " " + DateTime.Now.ToLongTimeString() + "Bytes sent to client : " + bytesSent);
                    Clients.Where(x => x.Key == handler).Select(y => y.Value.MacAddress).ToList().ForEach(Console.WriteLine);
                }
               
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString() +
                          " " + DateTime.Now.ToLongTimeString() + "Error in sending data to desktop client "+ e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }
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
                File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " clear socket exception 1 " + " " + ex.Message + " " + ex.StackTrace);
            }
        }

        private static async Task CheckShutdownInstruction()
        {
            //List<string> machineMacList = new List<string>();
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
                        //DatatoSend.Add("isDev", "on");
                        var t = JsonSerializer.Serialize(DatatoSend);
                        byte[] b = Encoding.ASCII.GetBytes(t);
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
               File.AppendAllText(docPath, Environment.NewLine+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in CheckShutdownInstruction() : " + ex.StackTrace);
            }
            
        }
    }
}
