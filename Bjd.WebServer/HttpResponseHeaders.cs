﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Memory;

namespace Bjd
{
    public class HttpResponseHeaders : HttpHeaders
    {
        static System.Collections.Concurrent.ConcurrentDictionary<string, bool> contentTypeIsIncludeText = new ConcurrentDictionary<string, bool>();
        static Func<string, bool> AddIsIncludeTextFunc = _ => _.ToLower().IndexOf("text") != -1;

        static bool IsIncludeText(string contentType)
        {
            return contentTypeIsIncludeText.GetOrAdd(contentType, AddIsIncludeTextFunc);
        }


        public HttpResponseHeaders()
        {
            Server = new KnowHeader("Server", "SERVER", "");
            _ar.Add(Server);

            Connection = new KnowHeader("Connection", "CONNECTION", "");
            _ar.Add(Connection);

            MIMEVersion = new KnowHeader("MIME-Version", "MIME-VERSION", "");
            _ar.Add(MIMEVersion);

            Date = new KnowHeader("Date", "DATE", "");
            _ar.Add(Date);

            ContentLength = new KnowHeader("Content-Length", "CONTENT-LENGTH", "");
            _ar.Add(ContentLength);
            ContentLength.Enabled = false;

            ContentType = new KnowHeader("Content-Type", "CONTENT-TYPE", "");
            _ar.Add(ContentType);
            ContentType.Enabled = false;

            LastModified = new KnowHeader("Last-Modified", "LAST-MODIFIED", "");
            _ar.Add(LastModified);
            LastModified.Enabled = false;

            ETag = new KnowHeader("ETag", "ETAG", "");
            _ar.Add(ETag);
            ETag.Enabled = false;

            ContentRange = new KnowHeader("Content-Range", "CONTENT-RANGE", "");
            _ar.Add(ContentRange);
            ContentRange.Enabled = false;

            AcceptRange = new KnowHeader("Accept-Range", "ACCEPT-RANGE", "");
            _ar.Add(AcceptRange);
            AcceptRange.Enabled = false;

            Location = new KnowHeader("Location", "LOCATION", "");
            _ar.Add(Location);
            Location.Enabled = false;
        }

        public IHeader Server;
        public IHeader Date;
        public IHeader ContentLength;
        public IHeader ContentType;
        public IHeader MIMEVersion;
        public IHeader Connection;
        public IHeader LastModified;
        public IHeader ETag;
        public IHeader ContentRange;
        public IHeader AcceptRange;
        public IHeader Location;
        public bool IsText = false;

        public override void Clear()
        {
            base.Clear();
            _ar.Add(Server);
            _ar.Add(Date);
            _ar.Add(ContentLength);
            _ar.Add(ContentType);
            _ar.Add(Connection);
            _ar.Add(LastModified);
            _ar.Add(ETag);
            _ar.Add(MIMEVersion);
            _ar.Add(AcceptRange);
            _ar.Add(ContentRange);
            _ar.Add(Location);

            Server.Enabled = true;
            Date.Enabled = true;
            ContentLength.Enabled = false;
            ContentType.Enabled = false;
            Connection.Enabled = true;
            LastModified.Enabled = false;
            ETag.Enabled = false;
            MIMEVersion.Enabled = true;
            AcceptRange.Enabled = false;
            ContentRange.Enabled = false;
            Location.Enabled = false;
            IsText = false;
        }

        public void SetContentLength(int length)
        {
            ContentLength.Enabled = true;
            ContentLength.ValString = length.ToString();
        }
        public void SetContentLength(long length)
        {
            ContentLength.Enabled = true;
            ContentLength.ValString = length.ToString();
        }

        public void SetAcceptRange(string range)
        {
            AcceptRange.Enabled = true;
            AcceptRange.ValString = range;
        }

        public void SetContentRange(string range)
        {
            ContentRange.Enabled = true;
            ContentRange.ValString = range;
        }

        public void SetContentType(string contentType)
        {
            ContentType.Enabled = true;
            ContentType.ValString = contentType;
            IsText = IsIncludeText(contentType);
        }

        public void SetLastModified(string lastModified)
        {
            LastModified.Enabled = true;
            LastModified.ValString = lastModified;
        }

        public void SetETag(string etag)
        {
            ETag.Enabled = true;
            ETag.ValString = etag;
        }

        public void SetLocation(BufferData location)
        {
            Location.Enabled = true;
            Location.Val = location;
        }
        public void SetLocation(string location)
        {
            Location.Enabled = true;
            Location.ValString = location;
        }

    }
}
