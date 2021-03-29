using System.Net.Sockets;

namespace DistributedChannel
{
    public class AutoDisposedNetworkStream
    {
        public NetworkStream NetworkStream { get; }

        public AutoDisposedNetworkStream(NetworkStream networkStream)
        {
            NetworkStream = networkStream;
        }
    }
}