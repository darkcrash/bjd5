﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bjd.net;
using Bjd.util;

namespace Bjd.sock
{
    public class SockServer : SockObj
    {

        public ProtocolKind ProtocolKind { get; private set; }
        private Socket _socket;
        byte[] _udpBuf;
        ArraySegment<byte> _udpBufSegment;
        private Ip _bindIp;
        private int _bindPort;

        private readonly Ssl _ssl;

        public SockServer(Kernel kernel, ProtocolKind protocolKind, Ssl ssl) : base(kernel)
        {
            System.Diagnostics.Trace.WriteLine($"SockServer..ctor{protocolKind.ToString()}");
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
            System.Diagnostics.Trace.WriteLine($"SockServer.Bind Start {bindIp.ToString()} {port.ToString()} {listenMax.ToString()}");
            try
            {
                if (ProtocolKind != ProtocolKind.Tcp)
                    Util.RuntimeException("use udp version bind()");
                try
                {
                    _socket = new Socket((bindIp.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }

                try
                {
                    _socket.Bind(new IPEndPoint(bindIp.IPAddress, port));
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }
                try
                {
                    _socket.Listen(listenMax);
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }

                Set(SockState.Bind, (IPEndPoint)_socket.LocalEndPoint, null);

                //受信開始
                BeginReceive();

                return true;

            }
            finally
            {
                System.Diagnostics.Trace.WriteLine("SockServer.Bind End");
            }
        }


        //UDP用
        public bool Bind(Ip bindIp, int port)
        {
            _bindIp = bindIp;
            _bindPort = port;
            if (ProtocolKind != ProtocolKind.Udp)
                Util.RuntimeException("use tcp version bind()");

            try
            {
                _socket = new Socket((bindIp.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            }
            catch (Exception e)
            {
                SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                return false;
            }

            try
            {
                _socket.Bind(new IPEndPoint(bindIp.IPAddress, port));
            }
            catch (Exception e)
            {
                SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                return false;
            }

            Set(SockState.Bind, (IPEndPoint)_socket.LocalEndPoint, null);

            _udpBuf = new byte[1600]; //１パケットの最大サイズで受信待ちにする
            _udpBufSegment = new ArraySegment<byte>(_udpBuf);

            //受信開始
            BeginReceive();

            return true;
        }

        //受信開始
        void BeginReceive()
        {
            System.Diagnostics.Trace.WriteLine($"SockServer.BeginReceive");
            switch (ProtocolKind)
            {
                case ProtocolKind.Udp:
                    // UDP
                    var retry = 10;
                    again:
                    var ep = (EndPoint)new IPEndPoint((_bindIp.InetKind == InetKind.V4) ? IPAddress.Any : IPAddress.IPv6Any, _bindPort);
                    try
                    {
                        //_socket.BeginReceiveFrom(_udpBuf, 0, _udpBuf.Length, SocketFlags.None, ref ep, AcceptFunc, this);
                        var tUdp = _socket.ReceiveFromAsync(_udpBufSegment, SocketFlags.None, ep);
                        tUdp.ContinueWith(_ => this.Receive(_));

                    }
                    catch (Exception)
                    {
                        //Logger.Set(LogKind.Error, null, 9000008, detailInfomation);//BeginReceiveFrom()でエラーが発生しました[UDP]
                        Thread.Sleep(100);
                        retry--;
                        if (0 <= retry)
                        {
                            goto again;
                        }
                        Util.RuntimeException(string.Format("retry={0}", retry));
                    }
                    break;
                case ProtocolKind.Tcp:
                    // TCP
                    //_socket.BeginAccept(AcceptFunc, this);
                    var tTcp = _socket.AcceptAsync();
                    tTcp.ContinueWith(_ => this.Accept(_));

                    break;
            }
        }

        void Receive(Task<SocketReceiveFromResult> taskResult)
        {
            if (taskResult.IsCompleted)
            {
                try
                {
                    SocketReceiveFromResult srfr = taskResult.Result;
                    //int len = _socket.EndReceiveFrom(ar, ref ep);
                    SockUdp sockUdp = new SockUdp(Kernel, _socket, _udpBuf, srfr.ReceivedBytes, (IPEndPoint)srfr.RemoteEndPoint); //ACCEPT
                    sockQueue.Enqueue(sockUdp);
                }
                catch (Exception) { }
            }
            //受信開始
            BeginReceive();
        }

        void Accept(Task<Socket> taskResult)
        {
            if (taskResult.IsCompleted)
            {
                try
                {
                    sockQueue.Enqueue(new SockTcp(Kernel, _ssl, taskResult.Result));
                }
                catch (Exception) { }
            }
            //受信開始
            BeginReceive();
        }



        Queue<sock.SockObj> sockQueue = new Queue<sock.SockObj>();
        //void AcceptFunc(IAsyncResult ar)
        //{
        //    System.Diagnostics.Trace.WriteLine($"SockServer.AcceptFunc");
        //    sockQueue.Enqueue(ar);
        //}

        public SockObj Select(ILife iLife)
        {
            System.Diagnostics.Trace.WriteLine($"SockServer.Select");

            while (iLife.IsLife())
            {
                if (sockQueue.Count > 0)
                {
                    return sockQueue.Dequeue();

                    //IAsyncResult ar = sockQueue.Dequeue();

                    //if (ProtocolKind == ProtocolKind.Udp)
                    //{

                    //    SockUdp sockUdp = null;
                    //    var ep = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                    //    try
                    //    {
                    //        int len = _socket.EndReceiveFrom(ar, ref ep);
                    //        sockUdp = new SockUdp(Kernel, _socket, _udpBuf, len, (IPEndPoint)ep); //ACCEPT

                    //    }
                    //    catch (Exception)
                    //    {
                    //        sockUdp = null;
                    //    }
                    //    //受信開始
                    //    BeginReceive();
                    //    return sockUdp;
                    //}
                    //else {
                    //    //自分自身を複製するため、いったん別のSocketで受け取る必要がある
                    //    var newSocket = _socket.EndAccept(ar); //ACCEPT

                    //    //受信開始
                    //    BeginReceive();

                    //    //Ver5.9.2 Java fix
                    //    //return new SockTcp(Kernel, newSocket);
                    //    return new SockTcp(Kernel, _ssl, newSocket);
                    //}
                }
                //Ver5.8.1
                //Thread.Sleep(0);
                Thread.Sleep(1);
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
            System.Diagnostics.Trace.WriteLine($"SockServer.CreateConnection");
            //Ver5.9.2 Java fix
            //var sockServer = new SockServer(kernel,ProtocolKind.Tcp);
            var sockServer = new SockServer(kernel, ProtocolKind.Tcp, ssl);
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
                        sockServer.Close(); //これ大丈夫？
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
            System.Diagnostics.Trace.WriteLine($"SockServer.IsAvailable");
            var sockServer = new SockServer(kernel, ProtocolKind.Tcp, null);
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
