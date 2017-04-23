using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Memory;

namespace Bjd
{
    public class HttpRequestHeaders : HttpHeaders
    {
        public HttpRequestHeaders()
        {
            Host = KnowHeader.Empty;
            Connection = KnowHeader.Empty;
            ContentLength = KnowHeader.Empty;
            Range = KnowHeader.Empty;

        }

        public OneHeader Host;
        public OneHeader Connection;
        public OneHeader ContentLength;
        public OneHeader Range;

        public override void Clear()
        {
            base.Clear();

            Host = KnowHeader.Empty;
            Connection = KnowHeader.Empty;
            ContentLength = KnowHeader.Empty;
            Range = KnowHeader.Empty;

        }


        protected override void AppendHeader(OneHeader header)
        {
            switch (header.KeyUpper)
            {
                case "HOST":
                    Host = header;
                    break;
                case "CONNECTION":
                    Connection = header;
                    break;
                case "CONTENT-LENGTH":
                    ContentLength = header;
                    break;
                case "RANGE":
                    Range = header;
                    break;
            }

        }


    }
}
