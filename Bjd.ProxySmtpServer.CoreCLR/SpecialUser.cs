using System;
using System.Collections.Generic;
using System.Linq;
using Bjd.Options;

namespace Bjd.ProxySmtpServer
{
    class SpecialUser
    {
        readonly List<OneSpecialUser> _ar = new List<OneSpecialUser>();
        public SpecialUser(IEnumerable<DatRecord> dat)
        {
            foreach (var o in dat)
            {
                if (o.Enable)
                {
                    var before = o.ColumnValueList[0];
                    var server = o.ColumnValueList[1];
                    var port = Convert.ToInt32(o.ColumnValueList[2]);
                    var after = o.ColumnValueList[3];
                    _ar.Add(new OneSpecialUser(before, server, port, after));
                }
            }
        }

        public OneSpecialUser Search(string before)
        {
            return _ar.FirstOrDefault(oneSpecialUser => oneSpecialUser.Before == before);
        }
    }
}
