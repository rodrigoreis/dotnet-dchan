using System.IO;

namespace DistributedChannel.Extensions
{
    public static class AutoDisposedNetworkStreamExtensions
    {
        public static bool IsValid(this AutoDisposedNetworkStream autoDisposedNetworkStream)
        {
            return autoDisposedNetworkStream.NetworkStream is not null;
        }

        public static bool CanReadAndWrite(this AutoDisposedNetworkStream autoDisposedNetworkStream)
        {
            return autoDisposedNetworkStream.IsValid() &&
                   autoDisposedNetworkStream.NetworkStream.CanRead &&
                   autoDisposedNetworkStream.NetworkStream.CanWrite;
        }

        public static StreamWriter GetWriter(this AutoDisposedNetworkStream autoDisposedNetworkStream, bool autoFlush = true)
        {
            if (autoDisposedNetworkStream.IsValid() && autoDisposedNetworkStream.NetworkStream.CanWrite)
            {
                return new StreamWriter(autoDisposedNetworkStream.NetworkStream) { AutoFlush = autoFlush };
            }

            return default;
        }
        
        public static StreamReader GetReader(this AutoDisposedNetworkStream autoDisposedNetworkStream)
        {
            if (autoDisposedNetworkStream.IsValid() && autoDisposedNetworkStream.NetworkStream.CanRead)
            {
                return new StreamReader(autoDisposedNetworkStream.NetworkStream);
            }

            return default;
        }
    }
}