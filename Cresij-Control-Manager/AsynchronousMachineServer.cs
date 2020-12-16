using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    public class MachineStateObject
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
    class AsynchronousMachineServer
    {
        protected Instructions inst = new Instructions();
        private static IHubProxy proxy;
        private static HubConnection con;
        public static Dictionary<string, StateObject> Clients = new Dictionary<string, StateObject>();
        public static int connectedClient = 0;
        private static int totalClients = 0;
        private static Dictionary<string, DateTime> ClientWaitList = new Dictionary<string, DateTime>();
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void ReceiveMacFromDesktop(string ccmac,string instruction)
        {
            
        }
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
       
        public static void StartListening()
        {
            ConnectToHub();
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
                Console.WriteLine("CLient Connection lost with exception message   " + ex.Message);
            }
            return status;
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

                Console.WriteLine(" Total Connected client : " + totalClients);
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                Instructions inst = new Instructions();
                var insdata = inst.GetValues("MacAddress");
                Send(handler, insdata);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
            if (handler.Connected)
            {
                //Console.WriteLine("ip   " + ip);
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


                        DecodeData(handler, bytes, bytesRead);
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
                                foreach (KeyValuePair<string, StateObject> c in Clients)
                                {
                                    if (c.Value.workSocket == handler)
                                    {
                                        Clients.Remove(c.Key);
                                        Console.WriteLine("Client automatically disconnected");
                                        Decode dr = new Decode();
                                        string m = dr.offlineMessage();
                                        //SendMessage(ip, m);
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
                        byte[] datatoDecode = new byte[4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3]];
                        for (int k = 0; k < datatoDecode.Length; k++)
                        {
                            datatoDecode[k] = receiveBytes[k + j];
                        }
                       
                        status = new string[7];
                       
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
                           object obj = re["data"];
                            final = obj as Dictionary<string, object>;

                            if (final.ContainsKey("MacAddress"))
                            {
                                if (Clients.Any(x => x.Value.workSocket == sock))
                                {
                                    var cc = Clients.Where(x => x.Value.workSocket == sock).Select(x => x.Value).FirstOrDefault();
                                    if (cc != null)
                                    {
                                        var temp = final["Data"] as Dictionary<string, string>;
                                        cc.MacAddress = temp["MacAddress"];
                                    }
                                }
                                mac = Clients.Where(y => y.Value.workSocket == sock).Select(y => y.Value.MacAddress).FirstOrDefault();
                                SendMessage(mac, final);

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
                Console.WriteLine(DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToLongDateString() + "  exception in Handle message  " + ex.Message + " stack trace " + ex.StackTrace + " "
                 + ex.GetError() + "from mac : " + mac);
                //string msg = con.State.ToString();
            }
            return 0;
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

        #region connection to website
        //connect to website
        public static void ConnectToHub()
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
                    //Console.WriteLine("server called SendControl");
                    //Console.WriteLine(ip + " data for IP "+data);
                    //byte[] dataBytes = HexEncoding.GetBytes(data, out int i);
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
                proxy.On<int>("Counts", i =>
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
            //message1.Add("test", "success");
            try
            {
                if (con.State != ConnectionState.Connected)
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
            proxy.Invoke("CountMachines",Clients.Count);
        }
        #endregion
    }
}
