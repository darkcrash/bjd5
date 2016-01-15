using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Bjd.net;

namespace Bjd.sock
{
    public class SockUdp : SockObj
    {
        private readonly Ssl _ssl;

        private readonly SockKind _sockKind;

        private readonly Socket _socket;

        private readonly byte[] _recvBuf = new byte[0];

        //***************************************************************************
        //パラメータのKernelはSockObjにおけるTrace()のためだけに使用されているので、
        //Traceしない場合は削除することができる
        //***************************************************************************

        protected SockUdp(Kernel kernel) : base(kernel)
        {
            //隠蔽する
        }


        //ACCEPT
        public SockUdp(Kernel kernel, Socket s, byte[] buf, int len, IPEndPoint ep) : base(kernel)
        {
            _sockKind = SockKind.ACCEPT;

            _socket = s;
            _recvBuf = new byte[len];
            Buffer.BlockCopy(buf, 0, _recvBuf, 0, len);

            //************************************************
            //selector/channel生成
            //************************************************
            Set(SockState.Connect, (IPEndPoint)s.LocalEndPoint, ep);

        }

        //CLIENT
        public SockUdp(Kernel kernel, Ip ip, int port, Ssl ssl, byte[] buf) : base(kernel)
        {
            //SSL通信を使用する場合は、このオブジェクトがセットされる 通常の場合は、null
            _ssl = ssl;

            _sockKind = SockKind.CLIENT;

            _socket = new Socket((ip.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);

            Set(SockState.Connect, null, new IPEndPoint(ip.IPAddress, port));

            //************************************************
            //送信処理
            //************************************************
            Send(buf);
        }

        public byte[] Recv(int sec)
        {
            _socket.ReceiveTimeout = sec * 1000;
            try
            {
                EndPoint ep = RemoteAddress;
                var tmp = new byte[1620];
                var l = _socket.ReceiveFrom(tmp, ref ep);
                //_recvBuf = new byte[l];
                //Buffer.BlockCopy(tmp, 0, _recvBuf, 0, l);
                var buf = new byte[l];
                Buffer.BlockCopy(tmp, 0, buf, 0, l);
                Set(SockState.Connect, LocalAddress, (IPEndPoint)ep);

                return buf;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        //ACCEPTの場合は、既に受信できているので、こちらでアクセスする
        public int Length()
        {
            return _recvBuf.Length;
        }
        //ACCEPTの場合は、既に受信できているので、こちらでアクセスする
        public byte[] RecvBuf
        {
            get { return _recvBuf; }
            //set { throw new NotImplementedException(); }
        }

        //ACCEPTのみで使用する　CLIENTは、コンストラクタで送信する
        public int Send(byte[] buf)
        {
            if (buf.Length == 0)
            {
                return 0;
            }
            if (RemoteAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                //警告 GetAddressBytes() を使用してください。
                //if (RemoteEndPoint.Address.Address == 0xffffffff) {
                var addrBytes = RemoteAddress.Address.GetAddressBytes();
                if (addrBytes[0] == 0xff && addrBytes[1] == 0xff && addrBytes[2] == 0xff && addrBytes[3] == 0xff)
                {
                    // ブロードキャストはこのオプション設定が必要
                    try
                    {
                        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    }
                    catch
                    {
                        return -1;
                    }
                }
                //IPv4
                return _socket.SendTo(buf, SocketFlags.None, RemoteAddress);
            } //IPv6
            return _socket.SendTo(buf, SocketFlags.None, RemoteAddress);
        }

        public override void Close()
        {
            //ACCEPT
            if (_sockKind == SockKind.ACCEPT)
            {
                return;
            }
            //_socket.Close();
            _socket.Dispose();
            SetError("close()");
        }
    }
}
