using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer;
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        private static Timer timer;
        private static void StartTimer()
        {
            timer = new Timer(new TimerCallback(CheckMachine), null, 60000, 30000);
        }

        private static void CheckMachine(object state)
        {
            
        }
        private Constants constants;
        public StateObject()
        {
            constants = new Constants();
        }        
    }
}
