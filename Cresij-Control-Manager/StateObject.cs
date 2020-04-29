using System.Net.Sockets;

namespace Cresij_Control_Manager
{
  internal class StateObject
  {
    internal byte[] buffer;
    internal Socket workSocket;

    public static int BufferSize { get; internal set; }
  }
}
