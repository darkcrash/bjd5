using Bjd.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public class SockUtil
    {

        //指定したアドレス・ポートで待ち受けて、接続されたら、そのソケットを返す
        //失敗した時nullが返る
        //Ver5.9.2 Java fix
        //public static SockTcp CreateConnection(Kernel kernel,Ip ip, int port,ILife iLife){
        public static ISocket CreateConnection(Kernel kernel, Ip ip, int port, Ssl ssl, ILife iLife)
        {
            kernel.Logger.TraceInformation($"SockUtil.CreateConnection");
            const int listenMax = 1;
            //Ver5.9.2 Java fix
            //var sockServer = new SockServer(kernel,ProtocolKind.Tcp);
            var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, ssl);
            try
            {
                if (sockServer.SockState == SockState.Error) return null;
                if (!sockServer.Bind(ip, port, listenMax)) return null;

                while (iLife.IsLife())
                {
                    var child = sockServer.Select(iLife);
                    if (child == null) break;
                    // 受信開始
                    child.BeginAsync();
                    return child;
                }

                return null;
            }
            finally
            {
                //sockServer.Close(); //これ大丈夫？
                sockServer.Close();
                sockServer.Dispose();
            }
        }

        static object IsAvailableLock = new object();
        static object IsAvailableUdpLock = new object();

        //bindが可能かどうかの確認
        public static bool IsAvailable(Kernel kernel, Ip ip, int port)
        {
            kernel.Logger.TraceInformation($"SockUtil.IsAvailable");
            const int listenMax = 1;
            lock (IsAvailableLock)
            {
                var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, null);
                try
                {
                    if (sockServer.SockState == SockState.Error) return false;

                    if (sockServer.Bind(ip, port, listenMax)) return true;

                    return false;
                }
                finally
                {
                    sockServer.Close();
                    sockServer.Dispose();
                }
            }
        }
        public static bool IsAvailableUdp(Kernel kernel, Ip ip, int port)
        {
            kernel.Logger.TraceInformation($"SockUtil.IsAvailable");
            var sockServer = new SockServerUdp(kernel, ProtocolKind.Udp, null);
            lock (IsAvailableUdpLock)
            {
                try
                {
                    if (sockServer.SockState == SockState.Error) return false;

                    if (sockServer.Bind(ip, port)) return true;

                    return false;
                }
                finally
                {
                    sockServer.Close();
                    sockServer.Dispose();
                }
            }
        }

    }
}
