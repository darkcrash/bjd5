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
            Host = new KnowHeader("Host", "HOST", "");
            Connection = new KnowHeader("Connection", "CONNECTION", "");
            ContentLength = new KnowHeader("Content-Length", "CONTENT-LENGTH", "");
            Range = new KnowHeader("Range", "RANGE", "");
            Authorization = new KnowHeader("Authorization", "AUTHORIZATION", "");
            PathInfo = new KnowHeader("PathInfo", "PATHINFO", "");
            IfModifiedSince = new KnowHeader("If-Modified-Since", "IF-MODIFIED-SINCE", "");
            IfUnmodifiedSince = new KnowHeader("If-Unmodified-Since", "IF-UNMODIFIED-SINCE", "");
            Destination = new KnowHeader("Destination", "DESTINATION", "");
            IfMatch = new KnowHeader("If-Match", "IF-MATCH", "");
            IfNoneMatch = new KnowHeader("If-None-Match", "IF-NONE-MATCH", "");

        }

        public KnowHeader Host;
        public KnowHeader Connection;
        public KnowHeader ContentLength;
        public KnowHeader Range;
        public KnowHeader Authorization;
        public KnowHeader PathInfo;
        public KnowHeader IfModifiedSince;
        public KnowHeader IfUnmodifiedSince;
        public KnowHeader Destination;
        public KnowHeader IfMatch;
        public KnowHeader IfNoneMatch;

        public override void Clear()
        {
            base.Clear();

            _ar.Add(Host);
            _ar.Add(Connection);
            _ar.Add(ContentLength);
            _ar.Add(Range);
            _ar.Add(Authorization);
            _ar.Add(PathInfo);
            _ar.Add(IfModifiedSince);
            _ar.Add(IfUnmodifiedSince);
            _ar.Add(Destination);
            _ar.Add(IfMatch);
            _ar.Add(IfNoneMatch);

            Host.Clear();
            Connection.Clear();
            ContentLength.Clear();
            Range.Clear();
            Authorization.Clear();
            PathInfo.Clear();
            IfModifiedSince.Clear();
            IfUnmodifiedSince.Clear();
            Destination.Clear();
            IfMatch.Clear();
            IfNoneMatch.Clear();

        }



        protected override bool AppendHeader(string keyUpper, BufferData val)
        {
            IHeader header;
            switch (keyUpper)
            {
                case "HOST":
                    header = Host;
                    break;
                case "CONNECTION":
                    header = Connection;
                    break;
                case "CONTENT-LENGTH":
                    header = ContentLength;
                    break;
                case "RANGE":
                    header = Range;
                    break;
                case "AUTHORIZATION":
                    header = Authorization;
                    break;
                case "PATHINFO":
                    header = PathInfo;
                    break;
                case "IF-MODIFIED-SINCE":
                    header = IfModifiedSince;
                    break;
                case "IF-UNMODIFIED-SINCE":
                    header = IfUnmodifiedSince;
                    break;
                case "IF-MATCH":
                    header = IfMatch;
                    break;
                case "IF-NONE-MATCH":
                    header = IfNoneMatch;
                    break;
                case "DESTINATION":
                    header = Destination;
                    break;
                default:
                    return false;
            }
            header.Val = val;
            header.Enabled = true;
            return true;

        }

    }
}
