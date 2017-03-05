using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Configurations;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{

    class GroupList : IEnumerable
    {

        readonly List<OneGroup> _ar = new List<OneGroup>();

        public GroupList(Dat groupList)
        {
            foreach (var o in groupList)
            {
                if (!o.Enable)
                    continue;
                var group = o.ColumnValueList[0];
                var users = o.ColumnValueList[1];
                _ar.Add(new OneGroup(group, users));
            }
        }
        //イテレータ
        public IEnumerator GetEnumerator()
        {
            return _ar.GetEnumerator();
        }
    }
}
