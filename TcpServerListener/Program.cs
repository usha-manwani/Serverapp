using System;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServerListener
{
    class Program
    {
        static Timer expiretimer;
        public static Thread DesktopSocketThread;
        static void Main(string[] args)
        {
            try
            {

                CheckExpiryTimer();
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
        static void CheckExpiryTimer()
        {
            expiretimer = new  Timer(new TimerCallback(CheckExpire), null, 1000, 12*60*60*1000);
        }
        private static void CheckExpire(object state)
        {
            var startdate = Convert.ToDateTime("2021-02-25");

            if (Math.Abs(Math.Round(DateTime.Now.Date.Subtract(startdate).TotalDays)) >=90)
            {

                Console.WriteLine("Application exeded the allowed time. Exiting operation!!");
                Console.WriteLine("Application Closing!!!");
                
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }
    }
}
