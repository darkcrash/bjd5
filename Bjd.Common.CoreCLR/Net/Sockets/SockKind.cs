using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bjd.Net.Sockets
{
    public enum SockKind
    {
        //bindされたサーバから生成されたソケット UDPの場合はクローンなのでclose()しない
        ACCEPT,
        //
        CLIENT
    }
}
