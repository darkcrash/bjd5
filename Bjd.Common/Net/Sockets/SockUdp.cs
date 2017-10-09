using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Bjd.Net;
using Bjd.Memory;
using Bjd.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public class SockUdp : SockObj, ISocket
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
            Send(buf, 0, buf.Length);
        }

        public byte[] Recv(int len, int sec, ILife iLife)
        {
            _socket.ReceiveTimeout = sec * 1000;
            try
            {
                EndPoint ep = RemoteAddress;
                var tmp = new byte[len];
                var l = _socket.ReceiveFrom(tmp, 0, len, SocketFlags.None, ref ep);
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

        //public byte[] Recv(int sec)
        //{
        //    return Recv(1620, sec, null);
        //    //_socket.ReceiveTimeout = sec * 1000;
        //    //try
        //    //{
        //    //    EndPoint ep = RemoteAddress;
        //    //    var tmp = new byte[1620];
        //    //    var l = _socket.ReceiveFrom(tmp, ref ep);
        //    //    var buf = new byte[l];
        //    //    Buffer.BlockCopy(tmp, 0, buf, 0, l);
        //    //    Set(SockState.Connect, LocalAddress, (IPEndPoint)ep);

        //    //    return buf;
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return new byte[0];
        //    //}
        //}

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





        public async ValueTask<BufferData> BufferRecvAsync(int len, int sec)
        {
            _socket.ReceiveTimeout = sec * 1000;
            try
            {
                EndPoint ep = RemoteAddress;
                var tmp = BufferPool.Get(len);
                var result  = await _socket.ReceiveFromAsync(new ArraySegment<byte>(tmp.Data, 0, tmp.Length), SocketFlags.None, ep);
                tmp.DataSize = result.ReceivedBytes;
                Set(SockState.Connect, LocalAddress, (IPEndPoint)ep);

                return tmp;
            }
            catch (Exception)
            {
                return BufferData.Empty;
            }
        }

        public byte[] LineRecv(int sec, ILife iLife)
        {
            throw new NotImplementedException();
        }

        public BufferData LineBufferRecv(int sec, ILife iLife)
        {
            throw new NotImplementedException();
        }

        public ValueTask<BufferData> LineBufferRecvAsync(int timeoutSec)
        {
            throw new NotImplementedException();
        }



        public string LastLineSend => throw new NotImplementedException();


        //ACCEPTのみで使用する　CLIENTは、コンストラクタで送信する
        public int Send(byte[] buf, int offset, int length)
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
            } //IPv6
            return _socket.SendTo(buf, offset, length, SocketFlags.None, RemoteAddress);
        }


        public async ValueTask<bool> SendAsync(BufferData buf)
        {
            if (buf.Length == 0)
            {
                //return 0;
                return true;
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
                        //return -1;
                        return false;
                    }
                }
                //IPv4
            } //IPv6
            await _socket.SendToAsync(buf.GetSegment(), SocketFlags.None, RemoteAddress);
            return true;
        }


        public int Send(IList<ArraySegment<byte>> buffers)
        {
            var cnt = 0;
            foreach (var buf in buffers)
            {
                cnt += Send(buf.Array, buf.Offset, buf.Count);
            }
            return cnt;
        }

        public int Send(BufferData buf)
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
            } //IPv6
            return _socket.SendTo(buf.Data, buf.DataSize, SocketFlags.None, RemoteAddress);
        }

        public int SendNoTrace(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length);
        }

        public int SendNoTrace(ArraySegment<byte> buffer)
        {
            if (buffer.Count == 0)
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
            } //IPv6
            return _socket.SendTo(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None, RemoteAddress);
        }

        public int AsciiSend(string str)
        {
            using (var buf = str.ToAsciiBufferData())
            {
                return Send(buf);
            }
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
