﻿using System;
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
    public class HttpResponseHeaders : HttpHeaders
    {
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

        }

        public OneHeader Server;
        public OneHeader Date;
        public OneHeader ContentLength;
        public OneHeader ContentType;
        public OneHeader MIMEVersion;
        public OneHeader Connection;

        public override void Clear()
        {
            base.Clear();
            _ar.Add(Server);
            _ar.Add(Date);
            _ar.Add(ContentLength);
            _ar.Add(ContentType);
            _ar.Add(Connection);
            _ar.Add(MIMEVersion);

            ContentLength.Enabled = false;
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

        public void SetContentType(string contentType)
        {
            ContentType.Enabled = true;
            ContentType.ValString = contentType;
        }


    }
}