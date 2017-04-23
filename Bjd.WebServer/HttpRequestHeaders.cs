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
        }

        public OneHeader Host;
        public OneHeader Connection;
        public OneHeader ContentLength;
        public OneHeader Range;
        public OneHeader Authorization;
        public OneHeader PathInfo;
        public OneHeader IfModifiedSince;
        public OneHeader IfUnmodifiedSince;
        public OneHeader Destination;
        public OneHeader IfMatch;
        public OneHeader IfNoneMatch;

        public override void Clear()
        {
            base.Clear();

            Host = KnowHeader.Empty;
            Connection = KnowHeader.Empty;
            ContentLength = KnowHeader.Empty;
            Range = KnowHeader.Empty;
            Authorization = KnowHeader.Empty;
            PathInfo = KnowHeader.Empty;
            IfModifiedSince = KnowHeader.Empty;
            IfUnmodifiedSince = KnowHeader.Empty;
            Destination = KnowHeader.Empty;
            IfMatch = KnowHeader.Empty;
            IfNoneMatch = KnowHeader.Empty;

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
                case "AUTHORIZATION":
                    Authorization = header;
                    break;
                case "PATHINFO":
                    PathInfo = header;
                    break;
                case "IF_MODIFIED_SINCE":
                    IfModifiedSince = header;
                    break;
                case "IF_UNMODIFIED_SINCE":
                    IfUnmodifiedSince = header;
                    break;
                case "IF-MATCH":
                    IfMatch = header;
                    break;
                case "IF-NONE-MATCH":
                    IfNoneMatch = header;
                    break;
                case "DESTINATION":
                    Destination = header;
                    break;
            }
            

        }


    }
}
