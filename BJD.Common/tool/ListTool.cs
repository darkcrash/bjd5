using System.Linq;
using System.IO;
using Bjd.util;

namespace Bjd.tool {

    //****************************************************************
    // ツール管理クラス(Managerの中でのみ使用される)
    //****************************************************************
    
    public class ListTool : ListBase<OneTool> {
        public OneTool Get(string nameTag){
            return Ar.FirstOrDefault(o => o.NameTag == nameTag);
        }

        //null追加を回避するために、ar.Add()は、このファンクションを使用する
        bool Add(OneTool o) {
            if (o == null)
                return false;
            Ar.Add(o);
            return true;
        }

        //ツールリストの初期化
        public void Initialize(Kernel kernel) {
            Ar.Clear();

            //「ステータス表示」の追加
            var nameTag = Path.GetFileNameWithoutExtension(Define.ExecutablePath);
            //Add((OneTool)Util.CreateInstance(kernel,Application.ExecutablePath, "Tool", new object[] { kernel, nameTag }));
            Add(new Tool(kernel,nameTag));


            //OptionListを検索して初期化する
            foreach (var o in kernel.ListOption) {
                if (o.UseServer) {
                    var oneTool = (OneTool)Util.CreateInstance(kernel, o.Path, "Tool", new object[] { kernel, o.NameTag });
                    if (oneTool != null) {
                        Ar.Add(oneTool);
                    }
                }
            }
        }

    }
}
