using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bjd.net;
using Bjd.util;

namespace Bjd.sock
{
    public class SockServerTcp : SockObj
    {

        public ProtocolKind ProtocolKind { get; private set; }
        private Socket _socket;
        private Ip _bindIp;
        private int _bindPort;
        private object Lock = new object();

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
            System.Diagnostics.Trace.TraceInformation($"SockServer..ctor{protocolKind.ToString()}");
            ProtocolKind = protocolKind;
            _ssl = ssl;
        }

        public override void Close()
        {
            if (_socket != null)
            {
                _socket.Dispose();
            }
            SetError("close()");
        }


        //TCP用
        public bool Bind(Ip bindIp, int port, int listenMax)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Bind TCP Start {bindIp.ToString()} {port.ToString()} {listenMax.ToString()}");
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
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }

                Set(SockState.Bind, (IPEndPoint)_socket.LocalEndPoint, null);

                //受信開始
                BeginReceiveTcp();

                return true;

            }
            finally
            {
                System.Diagnostics.Trace.TraceInformation("SockServer.Bind End");
            }
        }


        //受信開始
        void BeginReceiveTcp()
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.BeginReceiveTcp");
            // TCP
            //_socket.BeginAccept(AcceptFunc, this);
            var tTcp = _socket.AcceptAsync();
            tTcp.ContinueWith(_ => this.Accept(_), Kernel.CancelToken);
        }

        void Accept(Task<Socket> taskResult)
        {
            if (taskResult.IsCanceled)
                return;
            if (taskResult.IsCompleted)
            {
                try
                {
                    var client = new SockTcp(Kernel, _ssl, taskResult.Result);
                    lock (Lock)
                    {
                        sockQueue.Enqueue(client);
                        WaitSelect.Set();
                    }
                }
                catch { }
            }
            //受信開始
            BeginReceiveTcp();
        }

        Queue<sock.SockTcp> sockQueue = new Queue<sock.SockTcp>();
        ManualResetEventSlim WaitSelect = new ManualResetEventSlim(false);

        public SockTcp Select(ILife iLife)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Select");

            while (iLife.IsLife())
            {
                lock (Lock)
                {
                    if (sockQueue.Count > 0)
                    {
                        return sockQueue.Dequeue();
                    }
                    else
                    {
                        WaitSelect.Reset();
                    }
                }
                //Ver5.8.1
                //Thread.Sleep(0);
                //Thread.Sleep(1);
                WaitSelect.Wait();
            }
            SetError("isLife()==false");
            return null;
        }


        //指定したアドレス・ポートで待ち受けて、接続されたら、そのソケットを返す
        //失敗した時nullが返る
        //Ver5.9.2 Java fix
        //public static SockTcp CreateConnection(Kernel kernel,Ip ip, int port,ILife iLife){
        public static SockTcp CreateConnection(Kernel kernel, Ip ip, int port, Ssl ssl, ILife iLife)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.CreateConnection");
            //Ver5.9.2 Java fix
            //var sockServer = new SockServer(kernel,ProtocolKind.Tcp);
            var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, ssl);
            if (sockServer.SockState != SockState.Error)
            {
                const int listenMax = 1;
                if (sockServer.Bind(ip, port, listenMax))
                {
                    while (iLife.IsLife())
                    {
                        var child = (SockTcp)sockServer.Select(iLife);
                        if (child == null)
                        {
                            break;
                        }
                        //sockServer.Close(); //これ大丈夫？
                        return child;
                    }
                }
            }
            sockServer.Close();
            return null;
        }

        //bindが可能かどうかの確認
        public static bool IsAvailable(Kernel kernel, Ip ip, int port)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.IsAvailable");
            var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, null);
            if (sockServer.SockState != SockState.Error)
            {
                const int listenMax = 1;
                if (sockServer.Bind(ip, port, listenMax))
                {
                    sockServer.Close();
                    return true;
                }
            }
            sockServer.Close();
            return false;
        }

    }

}
