using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Utils;
using Bjd.Threading;

namespace Bjd.Net.Sockets
{
    public class SockServerTcp : SockObj
    {

        public ProtocolKind ProtocolKind { get; private set; }
        private Socket _socket;
        private Ip _bindIp;
        private int _bindPort;
        private readonly Ssl _ssl;

        private AddressFamily Family
        {
            get
            {
                return (_bindIp.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            }
        }


        public SockServerTcp(Kernel kernel, ProtocolKind protocolKind, Ssl ssl) : base(kernel)
        {
            Kernel.Logger.TraceInformation($"SockServerTcp..ctor{protocolKind.ToString()}");
            ProtocolKind = protocolKind;
            _ssl = ssl;
            var pool = SockQueuePool.Instance;
            pool = null;
        }


        public override void Close()
        {
            this.Cancel();
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
            SetError("close()");
        }


        //TCP用
        public bool Bind(Ip bindIp, int port, int listenMax)
        {
            Kernel.Logger.TraceInformation($"SockServerTcp.Bind TCP Start {bindIp.ToString()} {port.ToString()} {listenMax.ToString()}");
            _bindIp = bindIp;
            _bindPort = port;
            try
            {
                if (ProtocolKind != ProtocolKind.Tcp)
                    Util.RuntimeException("use udp version bind()");
                try
                {
                    _socket = new Socket(this.Family, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(bindIp.IPAddress, port));
                    _socket.Listen(listenMax);
                    _socket.NoDelay = true;
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }

                Set(SockState.Idle, (IPEndPoint)_socket.LocalEndPoint, null);

                return true;

            }
            finally
            {
                Kernel.Logger.TraceInformation("SockServerTcp.Bind End");
            }
        }

        public Task<SockTcp> SelectAsync(ILife iLife)
        {
            Kernel.Logger.DebugInformation($"SockServerTcp.Select");
            try
            {
                this.SockState = SockState.Bind;
                //while (true)
                //{
                //    if (_socket == null) return null;
                //    if (this.IsCancel) return null;
                //    if (!iLife.IsLife()) return null;

                //    if (_socket.Poll(1000000, SelectMode.SelectRead))
                //    {
                //        if (_socket == null) return null;
                //        if (this.IsCancel) return null;
                //        //var tTcp = _socket.AcceptAsync();
                //        //return tTcp.ContinueWith<SockTcp>((t) => new SockTcp(Kernel, _ssl, t.Result), this.CancelToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                //        return new Task<SockTcp>((t) => new SockTcp(Kernel, _ssl, _socket.Accept()), this.CancelToken, TaskCreationOptions.LongRunning);
                //    }
                //}
                return new Task<SockTcp>(() =>
                {
                    try
                    {
                        return new SockTcp(Kernel, _ssl, _socket.Accept());
                    }
                    catch (Exception ex)
                    {
                        if (IsCancel) return null;
                        Kernel.Logger.TraceError(ex.Message);
                        Kernel.Logger.TraceError(ex.StackTrace);
                    }
                    return null;
                }, this.CancelToken, TaskCreationOptions.LongRunning);
            }
            catch (OperationCanceledException)
            {
                Kernel.Logger.TraceInformation("SockServerTcp.Select OperationCanceledException");
            }
            catch (Exception ex)
            {
                Kernel.Logger.TraceError(ex.Message);
                Kernel.Logger.TraceError(ex.StackTrace);
            }
            return null;
        }

        public SockTcp Select(ILife iLife)
        {
            Kernel.Logger.DebugInformation($"SockServerTcp.Select");
            try
            {
                this.SockState = SockState.Bind;
                while (true)
                {
                    if (_socket == null) return null;
                    if (this.IsCancel) return null;
                    if (!iLife.IsLife()) return null;

                    if (_socket.Poll(1000000, SelectMode.SelectRead))
                    {
                        if (_socket == null) return null;
                        if (this.IsCancel) return null;
                        var tTcp = _socket.Accept();
                        return new SockTcp(Kernel, _ssl, tTcp);
                    }

                }
            }
            catch (OperationCanceledException)
            {
                Kernel.Logger.TraceInformation("SockServerTcp.Select OperationCanceledException");
            }
            catch (Exception ex)
            {
                Kernel.Logger.TraceError(ex.Message);
                Kernel.Logger.TraceError(ex.StackTrace);
            }
            return null;
        }

        private SocketAsyncEventArgs AcceptEventargs;
        private Action<object> AcceptCallback;

        public void AcceptAsync(Action<SockTcp> callback, ILife iLife)
        {
            Kernel.Logger.DebugInformation($"SockServerTcp.Select");
            try
            {
                if (AcceptCallback == null)
                {
                    AcceptCallback = (o) =>
                    {
                        callback(new SockTcp(Kernel, _ssl, (Socket)o));
                    };
                }
                AcceptEventargs = new SocketAsyncEventArgs();
                //AcceptEventargs.Completed += AcceptEventargs_Completed;
                AcceptEventargs.Completed += (sender, e) =>
                {
                    if (IsCancel) return;

                    var sock = e.AcceptSocket;
                    e.AcceptSocket = null;
                    _socket.AcceptAsync(AcceptEventargs);

                    if (e.SocketError == SocketError.Success)
                    {
                        //callback(new SockTcp(Kernel, _ssl, sock));
                        //System.Threading.ThreadPool.QueueUserWorkItem(AcceptCallback, sock);
                        var t = new Task(AcceptCallback, sock, TaskCreationOptions.LongRunning);
                        t.Start();
                    }
                };

                _socket.AcceptAsync(AcceptEventargs);

                this.SockState = SockState.Bind;

            }
            catch (OperationCanceledException)
            {
                Kernel.Logger.TraceInformation("SockServerTcp.Select OperationCanceledException");
            }
            catch (Exception ex)
            {
                Kernel.Logger.TraceError(ex.Message);
                Kernel.Logger.TraceError(ex.StackTrace);
            }
        }


    }

}
