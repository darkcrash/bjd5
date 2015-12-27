using System;
using System.Net.Sockets;
using System.Net;

namespace Bjd.Extensions
{
    static class SocketExtensions
    {

        public static int Send( Socket s, byte[] buffer, int offset, int length)
        {
            var e = new SocketAsyncEventArgs();
            e.SetBuffer(buffer, offset, length);
            var result = s.SendAsync(e);
            var waitT = System.Threading.Tasks.Task.Delay(0);
            e.Completed += (sender, arg) => waitT.Start();
            waitT.Wait();
            return e.BytesTransferred;
        }

        public static int Receive( Socket s, byte[] buffer, int offset, int length)
        {
            var e = new SocketAsyncEventArgs();
            e.SetBuffer(buffer, offset, length);
            var result = s.ReceiveAsync(e);
            var waitT = System.Threading.Tasks.Task.Delay(0);
            e.Completed += (sender, arg) => waitT.Start();
            waitT.Wait();
            return e.BytesTransferred;
        }

        public static int SendTo( Socket s, byte[] buffer, int length, IPEndPoint endPoint)
        {
            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = endPoint;
            e.SetBuffer(buffer, 0, length);
            s.SendToAsync(e);
            var waitT = System.Threading.Tasks.Task.Delay(0);
            e.Completed += (sender, arg) => waitT.Start();
            waitT.Wait();
            return e.BytesTransferred;
        }

        public static int ReceiveFrom( Socket s, byte[] buffer, ref EndPoint endPoint, int timeout)
        {
            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = endPoint;
            e.SetBuffer(buffer, 0, buffer.Length);
            s.ReceiveFromAsync(e);

            var waitT = System.Threading.Tasks.Task.Delay(0);
            e.Completed += (sender, arg) => waitT.Start();
            if (!waitT.Wait(timeout))
            {
                return 0;
            }

            endPoint = e.RemoteEndPoint;

            return e.BytesTransferred;
        }

    }
}
