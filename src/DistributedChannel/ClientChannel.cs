using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DistributedChannel.Extensions;
using DistributedChannel.Internals;

namespace DistributedChannel
{
    public class ClientChannel
    {
        private readonly ClientSocket _socket;
        private readonly Thread _thread;
        private readonly LogHandler _logHandler;

        public ClientChannel(LogHandler logHandler = default)
        {
            _logHandler = logHandler;
            _socket = new ClientSocket(ConnectionHandler, logHandler: logHandler);
            _thread = new Thread(_socket.Start);
        }

        public virtual void Open()
        {
            _thread.Start();
            WriteLog($"Client channel opened on Thread {_thread.ManagedThreadId}");
        }

        public virtual void Close()
        {
            _socket.StopRequested = true;
            WriteLog("Stop was requested to client socket thread");
            
            WriteLog("Joining client socket thread");
            _thread.Join();
            
            WriteLog("client socket thread has stopped gracefully");
        }
        
        protected virtual void ConnectionHandler(AutoDisposedNetworkStream stream)
        {
            if (! stream.CanReadAndWrite())
                return;

            var writer = stream.GetWriter();
            var reader = stream.GetReader();

            var sw = Stopwatch.StartNew();
            var i = 0;
            
            // TODO: Tight network message-loop (optional), check this.
            while (!_socket.StopRequested)
            {
                string ack;
                try
                {
                    //Synchronously send some JSON to the connected server
                    writer.WriteLine($"{Guid.NewGuid()}");
                    //Synchronously wait for a response from the connected server
                    ack = reader.ReadLine();
                }
                catch(IOException)
                {
                    return;
                }

                //TODO: Put breakpoint here to inspec the string returned by the connected client
                Console.WriteLine(ack);
                _ = ack;
                

                ++i;
                var elapsed = sw.ElapsedMilliseconds;
                
                if (sw.ElapsedMilliseconds < 1000) continue;
                
                WriteLog($"Thread {Thread.CurrentThread.ManagedThreadId}: {i} messages per second");
                i = 0;
                sw.Restart();
            }
        }
        
        private void WriteLog(string message)
        {
            _logHandler?.Invoke(message);
        }
    }
}