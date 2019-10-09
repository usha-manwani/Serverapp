using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cresij_Control_Manager
{
    class Models
    {
    }
    public class Pools
    {
        public string _ip;
        public Socket _sock;
        public Thread _thr;
        public DateTime _dt;
        public Pools(string ip, Socket sock, Thread thr, DateTime dt)
        {
            _ip = ip;
            _sock = sock;
            _thr = thr;
            _dt = dt;
        }

        public void Destory()
        {
            _sock = null;
            _thr = null;
        }

    }
}
