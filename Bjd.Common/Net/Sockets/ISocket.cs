using Bjd.Logs;
using Bjd.Memory;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public interface ISocket : IDisposable, ISocketBase
    {
        IPEndPoint RemoteAddress { get; set; }
        IPEndPoint LocalAddress { get; set; }
        //string RemoteHostname { get; }
        event EventHandler SocketStateChanged;
        Ip LocalIp { get; }
        Ip RemoteIp { get; }
        Task CancelWaitAsync();
        string GetLastEror();
        SockState SockState { get; }
        void Close();

        byte[] RecvBuf { get; }
        int Length();
        byte[] Recv(int len, int sec, ILife iLife);
        ValueTask<BufferData> BufferRecvAsync(int len, int sec);
        byte[] LineRecv(int sec, ILife iLife);
        BufferData LineBufferRecv(int sec, ILife iLife);
        ValueTask<BufferData> LineBufferRecvAsync(int timeoutSec);

        //string StringRecv(Encoding enc, int sec, ILife iLife);
        //ValueTask<string> StringRecvAsync(Encoding enc, int sec, ILife iLife);
        //string StringRecv(int sec, ILife iLife);
        //string StringRecv(string charsetName, int sec, ILife iLife);
        //ValueTask<string> StringRecvAsync(string charsetName, int sec, ILife iLife);

        //string AsciiRecv(int timeout, ILife iLife);
        //ValueTask<string> AsciiRecvAsync(int timeout);

        //CharsData AsciiRecvChars(int timeout, ILife iLife);
        //ValueTask<CharsData> AsciiRecvCharsAsync(int timeoutSec);


        ValueTask<bool> SendAsync(BufferData buf);
        int Send(byte[] buf, int offset, int length);

        //int Send(byte[] buf, int length);
        //int Send(byte[] buf);

        int Send(IList<ArraySegment<byte>> buffers);
        int Send(BufferData buf);
        int SendNoTrace(byte[] buffer);
        int SendNoTrace(ArraySegment<byte> buffer);

        //int Send(byte[] buf);
        //int LineSend(byte[] buf);
        //bool StringSend(string str, Encoding enc);
        //bool StringSend(string str);
        //bool StringSend(string str, string charsetName);
        //int SendUseEncode(byte[] buf);

        [Obsolete()]
        string LastLineSend { get; }

        //ValueTask<bool> AsciiSendAsync(CharsData data);
        //ValueTask<bool> AsciiLineSendAsync(CharsData data);
        [Obsolete()]
        int AsciiSend(string str);

        //int SendNoEncode(byte[] buf);
    }
}
