namespace DistributedChannel.Internals
{
    public readonly struct BindingAddress
    {
        public string IpV4 { get; }
        public int Port { get; }

        public BindingAddress(string ipv4, int port)
        {
            IpV4 = ipv4;
            Port = port;
        }
    }
}