using System.Runtime.InteropServices;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Traces;

namespace Bjd.RemoteServer.Remote
{
    //リモートサーバ側で動作しているときにクライアントへのアクセスするためのオブジェクト
    public class RemoteConnect
    {
        readonly ISocket _sockTcp;
        public bool OpenTraceDlg { private get; set; }

        static int GetCurrentThreadId()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public RemoteConnect(ISocket sockTcp)
        {
            _sockTcp = sockTcp;
        }

        //クライアント側への送信
        public void AddTrace(TraceKind traceKind, string str, Ip ip)
        {
            if (!OpenTraceDlg)
                return;
            var threadId = GetCurrentThreadId();
            var buffer = string.Format("{0}\b{1}\b{2}\b{3}", traceKind.ToString(), threadId.ToString(), ip, str);
            //トレース(S->C)
            RemoteData.Send(_sockTcp, RemoteDataKind.DatTrace, buffer);
        }
    }
}