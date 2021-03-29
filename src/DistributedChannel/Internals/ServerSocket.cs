using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DistributedChannel.Extensions;

namespace DistributedChannel.Internals
{
    internal class ServerSocket
    {
        private volatile bool _stopRequested;
        private bool _running;

        private readonly IList<Task> _concurrentClients;
        private readonly ConnectionHandler _connectionHandler;
        private readonly LogHandler _logHandler;

        protected int AwaiterTimeoutMs { get; }
        protected BindingAddress AdvertisedAddress { get; }
        protected int MaxConcurrentClientsPerThread { get; }
        protected TcpListener Listener { get; }

        public virtual bool StopRequested
        {
            get => _stopRequested;
            set => _stopRequested = value;
        }

        private ServerSocket()
        {
            _concurrentClients = new List<Task>();
        }

        public ServerSocket(ConnectionHandler connectionHandler,
                            string ipv4 = "0.0.0.0",
                            int port = 1983,
                            int maxConcurrentClientsPerThread = 10,
                            int awaiterTimeoutMs = 500,
                            LogHandler logHandler = default) : this()
        {
            _connectionHandler = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            _logHandler = logHandler;

            AdvertisedAddress = new BindingAddress(ipv4, port);
            MaxConcurrentClientsPerThread = maxConcurrentClientsPerThread;
            AwaiterTimeoutMs = awaiterTimeoutMs;
            Listener = new TcpListener(AdvertisedAddress.ToIpAddress(), AdvertisedAddress.Port);
        }

        public virtual void Start()
        {
            if (_running)
                return;

            Listener.Start();

            _running = true;
            _stopRequested = false;

            while (!_stopRequested)
                WaitForClientsConnections();

            _running = false;
        }

        protected virtual void WaitForClientsConnections()
        {
            while (_concurrentClients.Count < MaxConcurrentClientsPerThread)
            {
                var client = Task.Run(async () =>
                {
                    WriteLog($"Listening on Thread {Thread.CurrentThread.ManagedThreadId}");
                    AcceptClient(await Listener.AcceptTcpClientAsync());
                });

                _concurrentClients.Add(client);
            }

            HandleConcurrentClients();
        }

        protected virtual void AcceptClient(TcpClient client)
        {
            using (client)
            {
                if (!client.Connected)
                    return;

                WriteLog($"Client connected on Thread {Thread.CurrentThread.ManagedThreadId}");

                using var networkStream = client.GetStream();
                _connectionHandler.Invoke(new AutoDisposedNetworkStream(networkStream));
            }
        }

        private void HandleConcurrentClients()
        {
            var i = Task.WaitAny(_concurrentClients.ToArray(), AwaiterTimeoutMs);
            
            if (i < 0) return;
            
            _concurrentClients.RemoveAt(i);
            WriteLog($"Client disconnected, Recycling thread index {i}");
        }

        private void WriteLog(string message)
        {
            _logHandler?.Invoke(message);
        }
    }
}