using System.Collections.Generic;
using System.Linq;
using Bjd.Options;
using Bjd.Utils;

namespace Bjd.FtpServer
{


    public class ListMount : ListBase<OneMount>{

        public ListMount(IEnumerable<DatRecord> dat){
            if (dat != null){
                //有効なデータだけを対象にする
                foreach (var o in dat.Where(o => o.Enable)){
                    Add(o.ColumnValueList[0], o.ColumnValueList[1]);
                }
            }
        }

        public void Add(string fromFolder, string toFolder){
            Ar.Add(new OneMount(fromFolder, toFolder));
        }
    }
}