using System;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServerListener
{
    class Program
    {
        
        public static Thread DesktopSocketThread;
        static void Main(string[] args)
        {
            try
            {
                DesktopSocketThread = new Thread(AsyncDesktopServer.StartListening)
                {
                    IsBackground = true
                };
                DesktopSocketThread.Start();
                AsyncTcpListener.StartListening();

            }
            catch (Exception ex)
            {
                // Log the exception.
                Console.Write(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
