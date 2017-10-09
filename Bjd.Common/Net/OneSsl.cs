using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Generic;

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

        public int Write(byte[] buf, int offset, int len)
        {
            _stream.Write(buf, offset, len);
            return buf.Length;
        }
        public int Write(byte[] buf, int len)
        {
            _stream.Write(buf, 0, len);
            return buf.Length;
        }

        public Task WriteAsync(byte[] buf, int len, CancellationToken token)
        {
            return _stream.WriteAsync(buf, 0, len, token);
        }

        public int Write(ArraySegment<byte> buf)
        {
            _stream.Write(buf.Array, buf.Offset, buf.Count);
            return buf.Count;
        }

        public int Write(IList<ArraySegment<byte>> buf)
        {
            var s = 0;
            foreach(var b in buf)
            {
                _stream.Write(b.Array, b.Offset, b.Count);
                s += b.Count;
            }
            return s;
        }


        public Task<int> ReadAsync(ArraySegment<byte> buf, CancellationToken token)
        {
            return _stream.ReadAsync(buf.Array, buf.Offset, buf.Count, token);
        }

        public void Close()
        {
            //_stream.Close();
            _stream.Dispose();
        }

    }
}