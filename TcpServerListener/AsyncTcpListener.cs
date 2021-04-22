// ***********************************************************************
// Assembly         : TcpServerListener
// Author           : admin
// Created          : 04-02-2021
//
// Last Modified By : admin
// Last Modified On : 04-21-2021
// ***********************************************************************
// <copyright file="AsyncTcpListener.cs" company="">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************
using DBHelper;
using Microsoft.AspNet.SignalR.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServerListener
{
    /// <summary>
    /// Class AsyncTcpListener.
    /// </summary>
    public class AsyncTcpListener
    {
        #region tcp server methods
        /// <summary>
        /// The logger file
        /// </summary>
        private static Logger loggerFile = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The instructions object
        /// </summary>
        protected Instructions inst = new Instructions();
        /// <summary>
        /// SignalR Hub Proxy
        /// </summary>
        private static IHubProxy proxy;
        /// <summary>
        /// The Connection object for SignalR
        /// </summary>
        private static HubConnection con;

        /// <summary>
        /// The list of connected machines state object
        /// </summary>
        public static List<StateObject> Machines = new List<StateObject>();
        /// <summary>
        /// The count of connected client
        /// </summary>
        public static int connectedClient = 0;

        /// <summary>
        /// The timer
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// The machinetimer
        /// </summary>
        private static Timer Machinetimer;
        /// <summary>
        /// The strategy timer
        /// </summary>
        private static Timer StrategyTimer;
        /// <summary>
        /// The client wait list
        /// </summary>
        private static Dictionary<string, DateTime> ClientWaitList = new Dictionary<string, DateTime>();
        // Thread signal.  
        /// <summary>
        /// Reset event All done
        /// </summary>
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        /// <summary>
        /// this function is called from AsyncDesktopServer.cs class
        /// this gives the response from desktop class after the message related to system shutdown is sent
        /// to its connected desktop
        /// After receiving this we send the shutdown instruction to client machine.
        /// </summary>
        /// <param name="ccmac">The ccmac.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="stid">The stid.</param>
        /// <param name="stdescid">The stdescid.</param>
        /// <param name="instruction1">The instruction1.</param>
        /// <param name="equipid">The equipid.</param>
        public static async Task ReceiveMacFromDesktop(string ccmac, string instruction, int stid, int stdescid, byte[] instruction1, int equipid)
        {
            try
            {
                if (Machines.Any(x => x.MacAddress == ccmac))
                {
                    var temp = Machines.Where(x => x.MacAddress == ccmac).FirstOrDefault();
                    StrategyLogs stlogs = new StrategyLogs();
                    if (temp.workSocket.Connected)
                    {
                        loggerFile.Debug("sent bytes  " +
                                         HexEncoding.ToStringfromHex(instruction1) +
                                         "  Instruction to Mac: " + ccmac + " ");
                        Send(temp.workSocket, instruction1);
                        await stlogs.SaveStrategyLogInfo(instruction, stid, "Pending", ccmac, equipid);
                        loggerFile.Debug(Environment.NewLine + " sent bytes  " +
                                        HexEncoding.ToStringfromHex(instruction1) +
                                        "  Instruction to Mac: " + ccmac + "");
                    }
                    else
                    {
                        await stlogs.SaveStrategyLogInfo(instruction, stid, "Fail", ccmac, equipid);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "ReceiveMacFromDesktop : " + ex.StackTrace);
            }

        }
        /// <summary>
        /// this method is called to set the timer for checkmachine and strategy to run every minute
        /// </summary>
        private static void StartTimer()
        {
            Machinetimer = new Timer(new TimerCallback(CheckMachine), null, 60000, 60000);
            int delayStart = (60 - DateTime.Now.Second) * 1000;
            StrategyTimer = new Timer(new TimerCallback(RunStrategyTimer), null, delayStart, 60000);
        }

        /// <summary>
        /// this is a function thats called through timer to check the clients in the machine list
        /// if they are still connected or became obsolete and disconnect the obsolete one
        /// </summary>
        /// <param name="state">The state.</param>
        private static void CheckMachine(object state)
        {
            List<string> MachinetoDel = new List<string>();
            try
            {
                lock (ClientWaitList)
                {
                    MachinetoDel = ClientWaitList.Where(x => DateTime.Now.Subtract(x.Value).TotalSeconds > 40).Select(x => x.Key).ToList();
                    foreach (var s in MachinetoDel)
                    {
                        if (Machines.Any(x => x.MacAddress == s))
                        {
                            var temp = Machines.Where(x => x.MacAddress == s).Select(x => x).FirstOrDefault();
                            if (temp != null)
                            {
                                var sock = temp.workSocket;
                                sock.Shutdown(SocketShutdown.Both);
                                Decode dr = new Decode();
                                Dictionary<string, object> result = dr.OfflineMessage();
                                SendMessage(temp.MacAddress, result);
                                loggerFile.Debug("Offline message " + JsonSerializer.Serialize(result));
                                Machines.Remove(temp);
                                connectedClient--;
                                ClientWaitList.Remove(s);
                                SendCounts();
                            }
                        }
                    }
                }
                lock (Machines)
                {
                    var ObsoleteSocket = Machines.Where(x => x.MacAddress == "").ToList();
                    foreach (var s in ObsoleteSocket)
                    {
                        var sock = s.workSocket;
                        loggerFile.Debug("Removed Machine because of Empty MAc Address: " +
                            ((IPEndPoint)s.workSocket.RemoteEndPoint).Address);
                        sock.Shutdown(SocketShutdown.Both);
                        Machines.Remove(s);
                        connectedClient--;
                    }
                    var SameMac = Machines.GroupBy(x => x.MacAddress).Select(g => new { Mac = g.Key, count = g.Count() });
                    foreach (var s in SameMac)
                    {
                        if (s.count > 1)
                        {
                            var itemtoRemove = Machines.Where(x => x.MacAddress == s.Mac);
                            foreach (var t in itemtoRemove)
                            {
                                var sock = t.workSocket;

                                sock.Shutdown(SocketShutdown.Both);
                                Machines.Remove(t);
                                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                    DateTime.Now.ToLongTimeString() +
                                    "Removed machine because of same mac address : " + s.Mac);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggerFile.Debug(ex.Message);
            }
        }
        /// <summary>
        /// this is the method that runs every minute to check for the strategy instructions
        /// if needed to be executed
        /// </summary>
        /// <param name="state">The state.</param>
        private static void RunStrategyTimer(object state)
        {
            try
            {
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " +
                                DateTime.Now.ToLongTimeString() + "Strategy Timer running at this time");
                Machines.ForEach(x => loggerFile.Debug("ip: " + ((IPEndPoint)x.workSocket.RemoteEndPoint).Address.ToString() + " Mac Address: " + x.MacAddress));
                StringBuilder record = new StringBuilder();
                foreach (var s in Machines)
                {
                    record.AppendLine(JsonSerializer.Serialize("Mac: " + s.MacAddress + " ip: " + ((IPEndPoint)s.workSocket.RemoteEndPoint).Address.ToString()));
                }
                
                var tt = DateTime.Now.ToString("HH:mm") + ":00";
                
                StrategyExec strategyExec = new StrategyExec();
                
                var ff = strategyExec.GetData(tt);

                loggerFile.Debug("rows from strategy count: " + ff.Count);
                if (ff.Count > 0)
                {
                    StrategyLogs strategyLogs = new StrategyLogs();
                    Instructions inst = new Instructions();
                    foreach (FinalResult f in ff)
                    {
                        var instruction = new byte[10];
                        ///this here is to create the full instruction set because instruction needs the strategy id
                        try
                        {
                            var tempIns = inst.GetValues(f.Instruction);
                            var bytes = new byte[2];
                            bytes[0] = (byte)(f.StrategyId >> 8);
                            bytes[1] = (byte)f.StrategyId;
                            instruction = new byte[10];
                            for (int k = 0; k < 7; k++)
                            {
                                instruction[k] = tempIns[k];
                            }

                            instruction[7] = bytes[0]; instruction[8] = bytes[1];
                            var checksum = 0;
                            for (int i = 2; i < 9; i++)
                            {
                                checksum = checksum + instruction[i];
                            }
                            //loggerFile.Debug("checksum :{0}", checksum);
                            instruction[9] = Convert.ToByte(checksum & 0xff);
                            loggerFile.Debug(" Instruction query  : " + HexEncoding.ToStringfromHEx(instruction));
                            
                            if (f.Instruction == "CloseStrategy")
                            {
                                int r = AsyncDesktopServer.SendToDesktop(f.Ccmac, f.Deskmac, "Shutdown", f.StrategyDescId, f.StrategyId, instruction, f.Equipmentid);
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

                                            strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Pending", f.Ccmac, f.Equipmentid);

                                            loggerFile.Debug("---------------------------- sent bytes  " +
                                                HexEncoding.ToStringfromHex(instruction) +
                                                "  Instruction to Mac: " + f.Ccmac + " ----------------------------");

                                            Send(t, instruction);
                                        }
                                        else
                                        {
                                            strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Fail", f.Ccmac, f.Equipmentid);
                                        }
                                    }

                                    else
                                    {
                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Fail", f.Ccmac, f.Equipmentid);
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

                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Pending", f.Ccmac, f.Equipmentid);

                                        loggerFile.Debug("---------------------------- sent bytes  " +
                                            HexEncoding.ToStringfromHex(instruction) +
                                            "  Instruction to Mac: " + f.Ccmac + " ----------------------------");

                                        Send(t, instruction);
                                    }
                                    else
                                    {
                                        strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Fail", f.Ccmac, f.Equipmentid);
                                    }
                                }
                                else
                                {
                                    strategyLogs.SaveStrategyLogInfo(f.Instruction, f.StrategyId, "Fail", f.Ccmac, f.Equipmentid);
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            loggerFile.Debug(ex + " " + f);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //loggerFile.Debug(  Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in RunStrategyTimer: " + ex.StackTrace);
                loggerFile.Debug(ex.Message);
            }
        }
        /// <summary>
        /// this timer is set to try connecting to web socket in case it gets disconnected
        /// </summary>
        /// <param name="starTime">The star time.</param>
        /// <param name="every">time interval</param>
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
        /// This starts the tcp socket server to listen to incoming clients
        /// </summary>
        public static void StartListening()
        {
            ConnectToHub();
            StartTimer();
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer                                     
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
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ".............Error in TCP Listerner start : " + e.StackTrace);
                Console.WriteLine(e.ToString());
            }
            loggerFile.Debug("\nPress ENTER to continue...");
            // Console.Read();
        }
        /// <summary>
        /// this is to check if a socket client is connected
        /// </summary>
        /// <param name="handler">socket object.</param>
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
                loggerFile.Debug(ex);
            }
            return status;
        }
        /// <summary>
        /// this is to accept the new connections async
        /// </summary>
        /// <param name="ar">contains state of related object</param>
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
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ip + " connected");
                loggerFile.Debug(ip + " connected,  Total Connected Machine client : " + connectedClient);
                state.workSocket = handler;
                if (!Machines.Contains(state))
                {
                    Machines.Add(state);
                }
                loggerFile.Debug("Added new machine in AcceptCallBack: ");
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                Instructions inst = new Instructions();
                var insdata = inst.GetValues("MacAddress");
                Send(handler, insdata);
            }

            catch (Exception ex)
            {
                loggerFile.Debug(ex);
            }
        }
        /// <summary>
        /// this is the call back function for reading the data from socket async
        /// </summary>
        /// <param name="ar">contains state of related object</param>
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


                //string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                if (handler != null && handler.Connected)
                {
                    bytesRead = handler.EndReceive(ar);
                    byte[] bytes = new byte[bytesRead];
                    if (bytesRead > 0)
                    {
                        for (int i = 0; i < bytesRead; i++)
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

                if (se.ErrorCode == 10054 || ((se.ErrorCode != 10004) && (se.ErrorCode != 10053)))
                {
                    var temp = Machines.Where(x => x.workSocket == handler).FirstOrDefault();
                    handler.Close();
                    if (temp != null)
                    {
                        lock (Machines)
                        {
                            loggerFile.Debug("Client automatically disconnected");
                            Decode dr = new Decode();
                            Dictionary<string, object> result = dr.OfflineMessage();
                            SendMessage(temp.MacAddress, result);

                            loggerFile.Debug("Offline message " + JsonSerializer.Serialize(result));
                            Machines.Remove(temp);
                            connectedClient--;
                            SendCounts();
                        }
                    }
                }
                loggerFile.Debug(se);
            }
            catch (Exception ex)
            {
                loggerFile.Debug(ex);
            }
        }
        /// <summary>
        /// This method is used to call Decode.cs  to interpret the bytes response into readable form of list of
        /// Dictionary
        /// </summary>
        /// <param name="sock">the socket object from where data is received</param>
        /// <param name="receiveBytes">bytes received</param>
        /// <param name="length">count of bytes received</param>
        private static void DecodeData(Socket sock, byte[] receiveBytes, int length)
        {
            Dictionary<string, object> re = new Dictionary<string, object>();
            Decode dd = new Decode();
            Dictionary<string, object> final = new Dictionary<string, object>();
            var mac = "";
            try
            {
                string[] status = new string[7];

                for (int j = 0; j < length;)
                {
                    // 多组不定长度数据集合，确定分组标记
                    if (receiveBytes[j] == Convert.ToByte(0x8B) && receiveBytes[j + 1] == Convert.ToByte(0xB9))
                    {
                        int len = 4 + (256 * receiveBytes[j + 2]) + receiveBytes[j + 3];
                        byte[] datatoDecode = receiveBytes.Skip(j).Take(len).ToArray();

                        status = new string[7];

                        status[0] = mac;
                        for (int i = 1; i < 7; i++)
                        {
                            status[i] = "Off";
                        }
                        /// function call to get the bytes of response to a readable format
                        re = dd.Decoded("", datatoDecode, status);


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
                            loggerFile.Debug("bytes Received from: " + mac +
                               " message: " + HexEncoding.ToStringfromHex(datatoDecode));
                            if (mac != "" && mac != null)
                            {
                                lock (ClientWaitList)
                                {
                                    if (!ClientWaitList.ContainsKey(mac))
                                    {
                                        ClientWaitList.Add(mac, DateTime.Now);
                                    }
                                    else
                                    {
                                        ClientWaitList[mac] = DateTime.Now;
                                    }
                                }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                if (final["Type"].ToString() == "Strategy")
                                {
                                    StrategyLogs strategyLogs = new StrategyLogs();
                                    strategyLogs.UpdateStrategyStatus(final["Device"].ToString(), mac,
                                    Convert.ToInt32(final["StrategyId"]), final["InstructionStatus"].ToString());
                                }
                                else if (final["Type"].ToString() == "CardRegister" && final["InstructionStatus"].ToString() == "Success")
                                {

                                    var temp1 = final["Data"] as Dictionary<string, string>;
                                    var macobj1 = new GetMacAddress();
                                    var cardvalues = temp1.Where(x => x.Key.Contains("CardValue")).Select(x => x.Value);
                                    foreach (var s in cardvalues)
                                    {
                                        macobj1.UpdateStatCardReg("Registered", mac, s);
                                    }

                                }
                            }

                            SendMessage(mac, final);
                            if (final["Type"].ToString() != "Heartbeat")
                            {
                                var macobj2 = new GetMacAddress();
                                //var t = final["Log"].ToString();
                                var type = final["Type"].ToString();
                                if (final.ContainsKey("Log") && !string.IsNullOrEmpty(final["Log"].ToString()))
                                {
                                    StrategyLogs st = new StrategyLogs();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    st.SaveMachineLogs(type, final["Log"].ToString(), mac);
                                    if (final["Log"].ToString() == "SystemOn" || final["Log"].ToString() == "SystemOff")
                                    {
                                        st.UpdateMachineStatus(mac, final["Log"].ToString());
                                    }
                                    else if (final["Type"].ToString().Contains("ReaderLog"))
                                    {
                                        var temp2 = final["Data"] as Dictionary<string, string>;

                                        macobj2.SaveReaderLog(final["Type"].ToString(), mac, temp2["CardValue"]);
                                    }

                                    var temp3 = final["Data"] as Dictionary<string, string>;
                                    if (temp3.ContainsKey("ProjectorOnCode") || temp3.ContainsKey("ProjectorOffCode")
                                        || temp3.ContainsKey("SetBaudRate"))
                                    {
                                        var d = "False";
                                        if (temp3.ContainsKey("ProjectorOnCode"))
                                        {
                                            d = temp3["ProjectorOnCode"];
                                        }
                                        else if (temp3.ContainsKey("ProjectorOffCode"))
                                        {
                                            d = temp3["ProjectorOffCode"];
                                        }
                                        else if (temp3.ContainsKey("SetBaudRate"))
                                        {
                                            d = temp3["SetBaudRate"];
                                        }

                                        macobj2.UpdateProjectorConfig(mac, d);
                                    }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                    loggerFile.Debug(" Message Received from: " + mac +
                                     " message: " + JsonSerializer.Serialize(final));

                                }
                            }
                            else if (final["Type"].ToString() == "Heartbeat")
                            {
                                int error = 0;
                                var fnal = final["Data"] as Dictionary<string, string>;
                                var macobj = new GetMacAddress();
                                var d = new Dictionary<string, object>();
                                if (fnal["WorkStatus"].ToString() == "Closed")
                                {
                                    if (fnal["ProjectorPowerStatus"].ToString() == "On" ||
                                        fnal["ComputerPowerStatus"].ToString() == "On" ||
                                        fnal["AmplifierPowerStatus"].ToString() == "On" ||
                                        fnal["OtherPowerStatus"].ToString() == "On")
                                    {
                                        error = 1;
                                        d.Add("ErrorCode", error);
                                    }
                                    else if (fnal["Screen"].ToString() == "Down")
                                    {
                                        error = 2;
                                        d.Add("ErrorCode", error);
                                    }
                                    else if (fnal["ProjectorStatus"].ToString() == "On")
                                    {
                                        error = 6;
                                        d.Add("ErrorCode", error);
                                    }
                                }
                                else if (fnal["WorkStatus"].ToString() == "Open")
                                {
                                    if (fnal["Screen"].ToString() == "Up")
                                    {
                                        error = 3;
                                        d.Add("ErrorCode", error);
                                    }
                                    else if (fnal["Volume"].ToString() == "0")
                                    {
                                        error = 4;
                                        d.Add("ErrorCode", error);
                                    }
                                }
                                if (error != 0)
                                {
                                    int classid = macobj.GetClassID(mac);
                                    d.Add("ClassId", classid);
                                    SendMachineExceptionToWebsocket(classid, d);
                                }
                                if (fnal.ContainsKey("Power"))
                                {
                                    if (Convert.ToInt32(fnal["Power"]) > 0)
                                    {
                                        macobj.SavePowerUsageInfo(Convert.ToInt32(fnal["Power"]), mac, "Power");

                                    }

                                }
                                //fnal["Power"] = macobj.GetPowerUsageInfo(mac, "Power");
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
                loggerFile.Debug(ex + " from mac : " + mac);
                //string msg = con.State.ToString();
            }
        }
        /// <summary>
        /// this is to send the bytes to machine clients
        /// </summary>
        /// <param name="handler">socket object.</param>
        /// <param name="byteData">The byte data to send to machine.</param>
        private static void Send(Socket handler, byte[] byteData)
        {
            try
            {
                loggerFile.Debug("machine instruction sent to machine ip : " + ((IPEndPoint)handler.RemoteEndPoint).Address.ToString());
                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
                loggerFile.Debug("instructions send : " + HexEncoding.ToStringfromHex(byteData));
            }
            catch (SocketException socex)
            {
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "Error in Send: " + socex.StackTrace);
            }
        }
        /// <summary>
        /// call back function for sending the bytes to machine
        /// </summary>
        /// <param name="ar">contains state of related object</param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                loggerFile.Debug("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                loggerFile.Debug(Environment.NewLine + DateTime.Now.ToLongDateString()
                    + " " + DateTime.Now.ToLongTimeString() + "Error in SendCallBack: " + e.StackTrace);
            }
        }
        /// <summary>
        /// this method is to create the instruction set from the string keywords
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="value">The value.</param>
        /// <returns>instruction bytes</returns>
        private static byte[] CreateInstruction(string keyword, int value)
        {
            Instructions ins = new Instructions();
            var bytes = ins.GetValues(keyword);
            byte[] data = new byte[9];
            for (int i = 0; i < bytes.Count(); i++)
            {
                data[i] = bytes[i];
            }
            data[7] = Convert.ToByte(value);
            var checksum = 0;
            for (int k = 2; k < 9; k++)
            {
                checksum = checksum + data[k];
            }

            data[8] = Convert.ToByte(checksum & 0xff);
            return data;
        }
        /// <summary>
        /// this is to create the instruction set from the string keyword
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] ProjectorConfigInstruction(Dictionary<string, string> data)
        {
            StringBuilder bytes = new StringBuilder();
            bytes.Append("8bb9001E0303");
            bytes.Append(Convert.ToByte(data["ProjectorOffDelayMinute"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ScreenAutoDrop"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ProjectorAutoOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ProjectorAutoOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ComputerAutoOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ComputerAutoOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ProjectorSwitchAuto"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ScreenLinkageOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ScreenLinkageOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["VolumeMemoryOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["BuzzerOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["IODetectionOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["IODetectionOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["Projector232Signal"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["projector2Infrared"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["SwipeOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["SwipeOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["FingerPrintOn"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["FingerPrintOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ProjectorOnDelaySecond"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["ComputerLinkageOff"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["HdmiAudio"]).ToString("X2"));
            bytes.Append(Convert.ToByte(data["SystemAlarm"]).ToString("X2"));
            bytes.Append("00000000");

            List<byte> b = HexEncoding.GetBytes(bytes.ToString(), out int discard).ToList();
            int ch = 0;
            for (int i = 2; i < b.Count; i++)
            {
                ch = ch + b[i];
            }
            b.Add(Convert.ToByte(ch & 0xff));
            return b.ToArray();
        }
        #endregion
        #region connection to website
        //connect to website
        /// <summary>
        /// Connects to SignalR hub.
        /// </summary>
        public static void ConnectToHub()
        {
            try
            {
                var proxyUrl = ConfigurationManager.AppSettings.Get("proxyUrl").ToString();
                loggerFile.Debug("Proxy url: " + proxyUrl);
                con = new HubConnection(proxyUrl);
                // con.TraceLevel = TraceLevels.All;
                // con.TraceWriter = Console.Out;
                proxy = con.CreateHubProxy("myHub");
                // MessageBox.Show("create proxy hub called");
               
                proxy.On<List<string>, Dictionary<string, string>>("SetProjectorConfiguration", (mac, data) =>
                {
                    byte[] inst = ProjectorConfigInstruction(data);
                    loggerFile.Debug("bytes send " + HexEncoding.ToStringfromHex(inst));
                    foreach (var m in mac)
                    {
                        if (Machines.Any(x => x.MacAddress == m))
                        {
                            var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                            if (sock != null && isClientConnected(sock))
                            {
                                Send(sock, inst);
                            }
                        }
                    }
                }

                );
                proxy.On<List<string>, Dictionary<string, string>>("SetProjectorConfiguration1", (mac, data) =>
                {
                    var ins = new Instructions();

                    if (data.ContainsKey("ProjectorOnCode"))
                    {
                        var temp = HexEncoding.GetBytes(data["ProjectorOnCode"], out int discard);
                        var length = Convert.ToByte(temp.Length + 3);
                        var inslength = temp.Length + 7;
                        var inss = new byte[inslength];
                        var templen = length.ToString("X").PadLeft(4, '0');
                        var l1 = templen.Substring(0, 2);
                        var l2 = templen.Substring(2, 2);
                        var v = "8B B9 " + l1 + " " + l2 + " 03 11 " + data["ProjectorOnCode"];
                        List<byte> b = HexEncoding.GetBytes(v, out int dis2).ToList();
                        int ch = 0;
                        for (int i = 2; i < b.Count; i++)
                        {
                            ch = ch + b[i];
                        }
                        b.Add(Convert.ToByte(ch & 0xff));
                        var instruction = b.ToArray();
                        foreach (var m in mac)
                        {
                            if (Machines.Any(x => x.MacAddress == m))
                            {
                                var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                                if (sock != null && isClientConnected(sock))
                                {
                                    Send(sock, instruction);
                                }
                            }
                        }
                    }
                    if (data.ContainsKey("ProjectorOffCode"))
                    {
                        var temp = HexEncoding.GetBytes(data["ProjectorOffCode"], out int discard);
                        var length = Convert.ToByte(temp.Length + 3);
                        var inslength = temp.Length + 7;
                        var inss = new byte[inslength];
                        var templen = length.ToString("X").PadLeft(4, '0');
                        var l1 = templen.Substring(0, 2);
                        var l2 = templen.Substring(2, 2);
                        var v = "8B B9 " + l1 + " " + l2 + " 03 12 " + data["ProjectorOffCode"];
                        List<byte> b = HexEncoding.GetBytes(v, out int dis2).ToList();
                        int ch = 0;
                        for (int i = 2; i < b.Count; i++)
                        {
                            ch = ch + b[i];
                        }
                        b.Add(Convert.ToByte(ch & 0xff));
                        var instruction = b.ToArray();

                        foreach (var m in mac)
                        {
                            if (Machines.Any(x => x.MacAddress == m))
                            {
                                var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                                if (sock != null && isClientConnected(sock))
                                {
                                    Send(sock, instruction);
                                }
                            }
                        }
                    }

                    byte[] b1 = HexEncoding.GetBytes("8B B9 00 04 03 13 00 1A", out int discard1);
                    foreach (var m in mac)
                    {
                        if (Machines.Any(x => x.MacAddress == m))
                        {
                            var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                            if (sock != null && isClientConnected(sock))
                            {
                                Send(sock, b1);
                            }
                        }
                    }
                    var b2 = HexEncoding.GetBytes("8B B9 00 04 03 14 00 1B", out int discard2);
                    foreach (var m in mac)
                    {
                        if (Machines.Any(x => x.MacAddress == m))
                        {
                            var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                            if (sock != null && isClientConnected(sock))
                            {
                                Send(sock, b2);
                            }
                        }
                    }
                    var b3 = HexEncoding.GetBytes("8B B9 00 04 03 15 00 1C", out int discard3);
                    foreach (var m in mac)
                    {
                        if (Machines.Any(x => x.MacAddress == m))
                        {
                            var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                            if (sock != null && isClientConnected(sock))
                            {
                                Send(sock, b3);
                            }
                        }
                    }
                    var b4 = HexEncoding.GetBytes("8B B9 00 04 03 16 00 1D", out int discard4);
                    foreach (var m in mac)
                    {
                        if (Machines.Any(x => x.MacAddress == m))
                        {
                            var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                            if (sock != null && isClientConnected(sock))
                            {
                                Send(sock, b4);
                            }
                        }
                    }
                    if (data.ContainsKey("BaudRate"))
                    {
                        string parity = "";
                        string baud = ins.GetBaud(Convert.ToInt32(data["BaudRate"]));
                        if (data.ContainsKey("Parity"))
                        {
                            parity = data["Parity"];
                        }
                        var tempstring = "8B B9 00 05 03 17 " + baud + " " + parity.PadLeft(2, '0');
                        List<byte> b = HexEncoding.GetBytes(tempstring, out int discard).ToList();
                        int ch = 0;
                        for (int i = 2; i < b.Count; i++)
                        {
                            ch = ch + b[i];
                        }
                        b.Add(Convert.ToByte(ch & 0xff));
                        var instruction = b.ToArray();

                        foreach (var m in mac)
                        {
                            if (Machines.Any(x => x.MacAddress == m))
                            {
                                var sock = Machines.Where(x => x.MacAddress == m).Select(x => x.workSocket).FirstOrDefault();
                                if (sock != null && isClientConnected(sock))
                                {
                                    Send(sock, instruction);
                                }
                            }
                        }

                    }
                });
                proxy.On<string, string, string>("SendControl", (mac, data, value) =>
                 {
                     List<byte> dd = new List<byte>();
                     try
                     {

                         loggerFile.Debug("from web client :  " +
                                                   data +
                                                   "  Instruction to Mac: " + mac);
                         if (data.Contains("Volume") && !data.Contains("Mute"))
                         {
                             dd = CreateInstruction(data, Convert.ToInt32(value)).ToList();
                         }
                         else
                         {
                             Instructions inst = new Instructions();
                             dd = inst.GetValues(data).ToList();
                         }

                         if (Machines.Any(x => x.MacAddress == mac))
                         {
                             var sock = Machines.Where(x => x.MacAddress == mac).Select(x => x.workSocket).FirstOrDefault();
                             if (isClientConnected(sock))
                             {
                                 loggerFile.Debug("sent bytes  " +
                                                  HexEncoding.ToStringfromHex(dd.ToArray()) +
                                                  "  Instruction to Mac: " + mac);
                                 Send(sock, dd.ToArray());
                             }
                         }
                     }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                     catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                     {
                         loggerFile.Debug("Error in SendControl to Machine : " +
                              ex.StackTrace + " data from website " + data + " to mac: " + mac);
                         // loggerFile.Debug( ex.Message);
                     }
                 });
                proxy.On<List<string>, string>("RegisterCard", (macaddress, card) =>
                {
                    var temp = HexEncoding.GetBytesReverse(card, out int discard);

                    var length = Convert.ToByte(temp.Length + 3);
                    var inslength = temp.Length + 7;
                    var inss = new byte[inslength];
                    var checksum = length + 01 + 01;
                    string v = "8B B9 00 " + length.ToString("x2").PadLeft(2, '0') + " 01 01 ";
                    foreach (var s in temp)
                    {
                        checksum += s;
                        v += s.ToString("X2") + " ";
                    }
                    v += Convert.ToByte(checksum & 0xff).ToString("X2");
                    var b = v.Split(' ');
                    try
                    {
                        for (int i = 0; i < inss.Length; i++)
                        {
                            inss[i] = byte.Parse(b[i], System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerFile.Debug(ex);
                    }
                    //var ins = HexEncoding.GetBytes(v,out int d);

                    foreach (var mac in macaddress)
                    {
                        foreach (var s in Machines)
                        {
                            if (s.MacAddress == mac)
                            {
                                loggerFile.Debug("instruc: " + HexEncoding.ToStringfromHex(inss));
                                Send(s.workSocket, inss);
                                break;
                            }
                        }
                    }

                });

                proxy.On<List<string>, List<string>>("RegisterMultipleCard", (macaddress, cardids) =>
                 {
                     //for (int a = 0; a <= cardids.Count;)
                     //{
                     var cardbytes = new List<byte>();
                     string bytestoencode = "";
                     for (int c = 0; c < cardids.Count; c++)
                     {
                         if (c < cardids.Count)
                         {
                             bytestoencode += cardids[c];
                         }
                         else
                         {
                             break;
                         }
                     }
                     var temp = HexEncoding.GetBytesReverse(bytestoencode, out int dis);
                     cardbytes.AddRange(temp);
                     var length = cardbytes.Count + 3;
                     var inslength = cardbytes.Count + 7;
                     var inss = new byte[inslength];
                     //var checksum = length + 01 + 01;
                     var templen = length.ToString("X").PadLeft(4, '0');
                     var l1 = templen.Substring(0, 2);
                     var l2 = templen.Substring(2, 2);
                     int checksum = 0;
                     // var checksum = byte.Parse(l1, System.Globalization.NumberStyles.HexNumber) +
                     //byte.Parse(l2, System.Globalization.NumberStyles.HexNumber) + 01 + 01;
                     string v = "8B B9 " + l1 + " " + l2 + " 01 01 ";
                     cardbytes.ForEach(s => v += s.ToString("X2") + " ");
                     //foreach (var s in cardbytes)
                     //{
                     //    checksum += s;
                     //    v += s.ToString("X2") + " ";
                     //}
                     //v += Convert.ToByte(checksum & 0xff).ToString("X2");
                     var b = v.Split(' ');
                     try
                     {
                         for (int i = 0; i < inss.Length - 1; i++)
                         {
                             inss[i] = byte.Parse(b[i], System.Globalization.NumberStyles.HexNumber);
                             if (i > 1)
                             {
                                 checksum += inss[i];
                             }
                         }
                         //for(int j = 2; j < inss.Length; j++)
                         // {
                         //     checksum += j;
                         // }
                         var btcheck = Convert.ToByte(checksum & 0xff);
                         inss[inss.Length - 1] = Convert.ToByte(checksum & 0xff);
                         foreach (var mac in macaddress)
                         {
                             foreach (var s in Machines)
                             {
                                 if (s.MacAddress == mac)
                                 {
                                     loggerFile.Debug("instruc: " + HexEncoding.ToStringfromHex(inss));
                                     //loggerFile.Debug("bytes sending: " + HexEncoding.ToStringfromHex(inss));
                                     Send(s.workSocket, inss);
                                     break;
                                 }
                             }
                         }
                         Thread.Sleep(100);//wait for machine's response before sending more instructions
                     }
                     catch (Exception ex)
                     {
                         loggerFile.Debug(ex);
                     }
                     //a = a + 5;
                     // }
                 });
                proxy.On<string>("RefreshStatus", (d) =>
                {
                    Instructions inst = new Instructions();
                    byte[] b = inst.GetValues("Status");
                    try
                    {
                        foreach (var m in Machines)
                        {
                            try
                            {
                                if (isClientConnected(m.workSocket))
                                {
                                    Send(m.workSocket, b);
                                }
                            }
                            catch (Exception ex)
                            {
                                loggerFile.Debug(ex);
                            }
                        }
                    }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    {
                        loggerFile.Debug(ex);
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
                        loggerFile.Debug("There was an error opening the connection with WebClient");
                    }
                    //else{MessageBox.Show("Connected to signalR");}
                }).Wait();
            }
            catch (Exception ex)
            {
                loggerFile.Debug(" not connected to WebClient " + ex.Message);
                con.StateChanged += Con_StateChanged;
            }
        }

        /// <summary>
        /// Starts the con to the hub.
        /// </summary>
        private static async Task StartCon()
        {
            await con.Start();
        }
        /// <summary>
        /// called when the connection state of signalR changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void Con_StateChanged(StateChange obj)
        {
            if (obj.OldState == ConnectionState.Disconnected)
            {
                // loggerFile.Debug("State changed inside");
                var current = DateTime.Now.TimeOfDay;
                SetTimer(current.Add(TimeSpan.FromSeconds(30)), TimeSpan.FromSeconds(10), StartCon);
                // CloggerFile.Debug("State changed inside done");
            }
            else
            {
                // loggerFile.Debug("State changed else inside");
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
        }

        /// <summary>
        /// close the signalRconnection
        /// </summary>
        private static void Con_Closed()
        {
            //loggerFile.Debug("connection closed");
            con.Start().Wait();
        }
        /// <summary>
        /// Sends the message to signalR.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        public static void SendMessage(string sender, Dictionary<string, object> message)
        {
            Dictionary<string, object> message1 = new Dictionary<string, object>();
            var d = JsonSerializer.Serialize(message);
            //message1.Add("test", "success");
            try
            {
                if (con.State != ConnectionState.Connected)
                {
                    // loggerFile.Debug("connecting to server");
                    con.Start().Wait();
                    // loggerFile.Debug("connected");
                }
                proxy.Invoke("SendMessage", sender, d);
                //loggerFile.Debug("Sent to signalR server by" + sender);

            }
            catch (Exception ex)
            {
                loggerFile.Debug(ex.Message);
                //  loggerFile.Debug("connecting to server");
                con.Start().Wait();
                // loggerFile.Debug("connected");
            }
        }
        /// <summary>
        /// Sends the desktop event to web socket.
        /// </summary>
        /// <param name="sender">The Desktop mac.</param>
        /// <param name="message">The message.</param>
        public static void SendDesktopEventToWebsocket(string sender, Dictionary<string, string> message)
        {

            var d = JsonSerializer.Serialize(message);
            //message1.Add("test", "success");
            try
            {
                if (con.State != ConnectionState.Connected)
                {
                    // loggerFile.Debug("connecting to server");
                    con.Start().Wait();
                    // loggerFile.Debug("connected");
                }
                proxy.Invoke("ReceiveDesktopEvent", sender, d);
                //loggerFile.Debug("Sent to signalR server by" + sender);

            }
            catch (Exception ex)
            {
                loggerFile.Debug(ex.Message);
                //  loggerFile.Debug("connecting to server");
                con.Start().Wait();
                // loggerFile.Debug("connected");
            }
        }

        /// <summary>
        /// Sends the machine exception to web socket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="errorno">The errorno.</param>
        public static void SendMachineExceptionToWebsocket(int sender, Dictionary<string, object> errorno)
        {

            var d = JsonSerializer.Serialize(errorno);
            try
            {
                if (con.State != ConnectionState.Connected)
                {
                    // loggerFile.Debug("connecting to server");
                    con.Start().Wait();
                    // loggerFile.Debug("connected");
                }
                proxy.Invoke("ReceiveMachineException", sender, d);
                //loggerFile.Debug("Sent to signalR server by" + sender);

            }
            catch (Exception ex)
            {
                loggerFile.Debug(ex.Message);
                //  loggerFile.Debug("connecting to server");
                con.Start().Wait();
                // loggerFile.Debug("connected");
            }
        }

        /// <summary>
        /// Sends the counts of connected machine to signalR.
        /// </summary>
        public static void SendCounts()
        {
            GetMacAddress getMac = new GetMacAddress();
            int count = getMac.MachineCount(Machines.Select(x => x.MacAddress).ToList());
            proxy.Invoke("CountMachines", count);
        }
        #endregion

    }
}
