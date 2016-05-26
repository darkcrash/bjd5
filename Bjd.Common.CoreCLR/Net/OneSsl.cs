using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Bjd.Net
{
    public class OneSsl
    {
        readonly SslStream _stream;

        //クライアント接続
        public OneSsl(Socket socket, string targetServer)
        {
            _stream = new SslStream(new NetworkStream(socket));
            var t = _stream.AuthenticateAsClientAsync(targetServer);
            t.Wait();
        }

        //サーバ接続
        public OneSsl(Socket socket, X509Certificate2 x509Certificate2)
        {
            _stream = new SslStream(new NetworkStream(socket));
            try
            {
                var t = _stream.AuthenticateAsServerAsync(x509Certificate2);
                t.Wait();
            }
            catch (Exception)
            {

            }
            _stream.ReadTimeout = 5000;
            _stream.WriteTimeout = 5000;
        }

        ~OneSsl()
        {
            try
            {
                //_stream.Close();
                _stream.Dispose();
            }
            catch
            {
            }
        }

        public int Write(byte[] buf, int len)
        {
            _stream.Write(buf, 0, len);
            return buf.Length;
        }

        public void BeginRead(byte[] buf, int offset, int count, AsyncCallback ac, object o, CancellationToken token)
        {
            //_stream.BeginRead(buf, offset, count, ac, o);
            BeginRead(_stream, buf, offset, count, ac, o, token);
        }

        public int EndRead(IAsyncResult ar)
        {
            //return _stream.EndRead(ar);
            return EndRead(_stream, ar);
        }

        public void Close()
        {
            //_stream.Close();
            _stream.Dispose();
        }


        private static void BeginRead(SslStream t, byte[] buf, int offset, int count, AsyncCallback ac, object o, CancellationToken token)
        {
            var result = t.ReadAsync(buf, offset, count);
            Result r = new Result(ac, o);
            result.ContinueWith(_ => r.Complete(_), token);
        }

        private static int EndRead(SslStream t, IAsyncResult ar)
        {
            var result = (Result)ar;
            return result.ReadCount;
        }


        private class Result : IAsyncResult
        {
            private System.Threading.ManualResetEvent _wait;
            private bool _Completed = false;
            private AsyncCallback callback;
            public int ReadCount = -1;
            public Result(AsyncCallback ac, object state)
            {
                this._wait = new ManualResetEvent(false);
                this.AsyncState = state;
                this.AsyncWaitHandle = this._wait;
                this.callback = ac;
            }

            public void Complete(Task<int> result)
            {
                this.ReadCount = result.Result;
                this._Completed = true;
                this._wait.Set();
                this.callback(this);
            }

            public object AsyncState { get; }

            public WaitHandle AsyncWaitHandle { get; }

            public bool CompletedSynchronously { get; } = false;

            public bool IsCompleted { get { return _Completed; } }

        }

    }
}