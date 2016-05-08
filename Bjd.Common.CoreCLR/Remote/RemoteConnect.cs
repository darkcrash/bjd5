using System.Runtime.InteropServices;
using Bjd.Net;
using Bjd.Sockets;
using Bjd.Traces;

namespace Bjd.Remote
{
    //リモートサーバ側で動作しているときにクライアントへのアクセスするためのオブジェクト
    public class RemoteConnect
    {
        readonly SockTcp _sockTcp;
        public bool OpenTraceDlg { private get; set; }

        static int GetCurrentThreadId()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public RemoteConnect(SockTcp sockTcp)
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