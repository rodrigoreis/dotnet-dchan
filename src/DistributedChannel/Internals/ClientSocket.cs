using System;
using System.Net.Sockets;
using System.Threading;
using DistributedChannel.Extensions;

namespace DistributedChannel.Internals
{
    internal class ClientSocket
    {
        private volatile bool _stopRequested;
        private bool _running;

        private readonly ConnectionHandler _connectionHandler;
        private readonly LogHandler _logHandler;

        protected int ConnectionAttemptRetryDelayMs { get; }
        protected BindingAddress Host { get; }
        
        public virtual bool StopRequested
        {
            get => _stopRequested;
            set => _stopRequested = value;
        }

        public ClientSocket(ConnectionHandler connectionHandler,
                            string hostIpv4 = "127.0.0.1",
                            int port = 1983,
                            int connectionAttemptRetryDelayMs = 2000,
                            LogHandler logHandler = default)
        {
            _connectionHandler = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            _logHandler = logHandler;

            Host = new BindingAddress(hostIpv4 ?? throw new ArgumentNullException(nameof(hostIpv4)), port);
            ConnectionAttemptRetryDelayMs = connectionAttemptRetryDelayMs;
        }

        public virtual void Start()
        {
            if (_running)
                return;

            _running = true;
            _stopRequested = false;

            while (!_stopRequested)
                WaitForServerConnection();

            _running = false;
        }

        protected virtual void WaitForServerConnection()
        {
            WriteLog($"Attempting server connection on Thread {Thread.CurrentThread.ManagedThreadId}");

            // TODO: create TcpClient factory.
            using var client = new TcpClient();

            try
            {
                client.Connect(Host.ToIpAddress(), Host.Port);
            }
            catch(SocketException sockEx)
            {
                WriteLog($"{sockEx.Message}. Waiting {ConnectionAttemptRetryDelayMs}ms to retry");
                Thread.Sleep(ConnectionAttemptRetryDelayMs);
                return;
            }
            catch(Exception ex)
            {
                WriteLog($"{ex.Message}. This fatal error will terminate the process with {ex.HResult}");
                Environment.Exit(ex.HResult);
            }

            if (!client.Connected)
                return;

            using var networkStream = client.GetStream();
            _connectionHandler.Invoke(new AutoDisposedNetworkStream(networkStream));
        }

        private void WriteLog(string message)
        {
            _logHandler?.Invoke(message);
        }
    }
}