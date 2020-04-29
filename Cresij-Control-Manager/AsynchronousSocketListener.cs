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
    public class AsynchronousSocketListener
    {
    // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void StartListening()
        {
      
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "localhost:1200".                       
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
                  // Console.WriteLine("Waiting for a connection...");
                  listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                  // Wait until a connection is made before continuing.  
                  allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
              // Console.WriteLine(e.ToString());
            }
          // Console.WriteLine("\nPress ENTER to continue...");            
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

            
              state.workSocket = handler;
              handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                  new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
              // Console.WriteLine(ex.Message);
            }
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   

            int i = 0;
            string ip = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();

            if (handler.Connected)
            {
        
            }
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
              //  Console.WriteLine(socex.Message);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                if (isClientConnected((Socket)ar.AsyncState))
                {
                  // Complete sending the data to the remote device.  
                    int bytesSent = handler.EndSend(ar);
                    Console.WriteLine("Sent {0} bytes to client{1}.", bytesSent, ((IPEndPoint)handler.RemoteEndPoint).Address.ToString());
                }
                else
                {
                    SaveStatus(((IPEndPoint)handler.RemoteEndPoint).Address.ToString(), "Offline");
                }
              
            }
            catch (Exception ex)
            {
                SaveStatus(((IPEndPoint)handler.RemoteEndPoint).Address.ToString(), "Offline");
            }
        }

    }
}
