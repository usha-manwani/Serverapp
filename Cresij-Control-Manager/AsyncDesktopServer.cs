using DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    class AsyncDesktopServer
    {
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

    public class AsyncDesktopListener
    {
        public static readonly string constr = "Integrated Security=SSPI;Persist Security Info=False;" +
            "Data Source=WIN-OTVR1M4I567\\SQLEXPRESS;Initial Catalog=CresijCam";

        public static Dictionary<string,DateTime> WaitList = new Dictionary<string,DateTime>();
        public static Dictionary<string, int> IPStatus = new Dictionary<string, int>();
        public static int connectedClient = 0;
        public static Dictionary<Socket, StateObject> Clients = new Dictionary<Socket, StateObject>();
        public static Dictionary<string, string> DesktopList = new Dictionary<string, string>();
        private static System.Timers.Timer IPStatusTimer;
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

        public AsyncDesktopListener()
        {
            var current = DateTime.Now.TimeOfDay;
            SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(10), CheckShutdownInstruction);
        }

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".                       
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
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }           
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                connectedClient++;
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
                    if(Clients.ContainsKey(handler))
                    Clients.Add(handler, state);
                    totalClients = Clients.Count;
                }
                state.workSocket = handler;
                Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
                DatatoSend.Add("Type", "Reply");
                DatatoSend.Add("Status", "Connected");
                var s = JsonSerializer.Serialize(DatatoSend);
                byte[] b = Encoding.ASCII.GetBytes(s);
                Send(handler,b);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
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
                        var data = Encoding.ASCII.GetString(bytes, 0,bytesRead);
                        var received = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
                        DecodeDataDesktop(received, handler);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
                catch (SocketException se)
                {
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
        private static int DecodeDataDesktop(Dictionary<string, string> data, Socket sock)
        {
            string d = "";
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
                        if (!DesktopList.Contains(r))
                        {
                            DesktopList.Add(r.Key, r.Value);
                        }
                        lock (Clients)
                        {
                            if (Clients.ContainsKey(sock))
                            {
                                var client = Clients[sock];
                                client.MacAddress = data["value"].ToUpper();
                            }                            
                        }
                    }
                    else
                    {
                       // Log("mac from database key: " + r.Key + " value: " + r.Value);
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
                    var ccmac = data["CCmac"];
                   // Log("received " + code + " for machine with mac " + ccmac);
                    if (code == "Shutdown")
                    {
                        Instructions ins = new Instructions();
                        d = "SystemOffS";
                    }
                    try
                    {
                        AsynchronousMachineServer.ReceiveMacFromDesktop(ccmac, d);
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        //Log(" Decode Desktop Data exception under send data to machine " + " " + ex.Message + " " + ex.StackTrace);
                        // Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
               // Log(" Decode Desktop Data exception 2 " + " " + ex.Message + " " + ex.StackTrace);
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
                Console.WriteLine("CLient Connection lost with exception message   " + ex.Message);
            }
            return status;
        }

        public static int SendToDesktop(string ccmac, string deskmac, string instruction)
        {
            int result = 0;
            Dictionary<string, string> DatatoSend = new Dictionary<string, string>();
            DatatoSend.Add("Type", "Command");
            DatatoSend.Add("Code", instruction);
            DatatoSend.Add("CCmac", ccmac.ToUpper());
            DatatoSend.Add("Deskmac", deskmac.ToUpper());
            var s = JsonSerializer.Serialize(DatatoSend);
            try
            {
                Instructions ins = new Instructions();
                var obj = Clients.Where(x => x.Value.MacAddress == deskmac.ToUpper()).Select(x => x.Value.workSocket).FirstOrDefault();
                if (obj != null)
                {
                   Send(obj, ins.GetValues(instruction));
                    if (!WaitList.ContainsKey(ccmac.ToUpper()))
                        WaitList.Add(ccmac.ToUpper(), DateTime.Now);
                    result = 1;
                }
                else { result = 0; }
            }
            catch {
                if (!WaitList.ContainsKey(ccmac.ToUpper()))
                    WaitList.Add(ccmac.ToUpper(), DateTime.Now);
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
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
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
                        sock.Shutdown(SocketShutdown.Both);
                    }
                }

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
               // Log(" clear socket exception 1 " + " " + ex.Message + " " + ex.StackTrace);
            }
        }

        private static async Task CheckShutdownInstruction()
        {
            List<string> machineMacList = new List<string>();
            try
            {
                lock (WaitList)
                {                    
                    machineMacList =  WaitList.Where(x => DateTime.Now.Subtract(x.Value).Minutes > 2).Select(x => x.Key).ToList();
                    foreach (string m in machineMacList)
                    {
                        AsynchronousMachineServer.ReceiveMacFromDesktop(m, "SystemOff");
                        WaitList.Remove(m);
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
