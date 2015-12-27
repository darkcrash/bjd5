using System;
using System.Collections.Generic;
using Bjd.ctrl;
using Bjd.util;

namespace Bjd.option {
    //OneValのリストを表現するクラス<br>
    //OneValと共に再帰処理が可能になっている<br>
    public class ListVal : ListBase<OneVal>{

        public void Add(OneVal oneVal){

            // 追加オブジェクトの一覧
            var list = oneVal.GetList(null);

            foreach (var o in list){
                if (null != Search(o.Name)){
                    //Msg.Show(MsgKind.Error, string.Format("ListVal.add({0}) 名前が重複しているため追加できませんでした", o.Name));
                    Console.WriteLine(string.Format("ListVal.add({0}) 名前が重複しているため追加できませんでした", o.Name));
                }
            }
            // 重複が無いので追加する
            Ar.Add(oneVal);
        }

        //階層下のOneValを一覧する(全部の値を列挙する)
        public List<OneVal> GetList(List<OneVal> list){
            if (list == null){
                list = new List<OneVal>();
            }
            foreach (var o in Ar){
                list = o.GetList(list);
            }
            return list;
        }

        //階層下のOneValを一覧する(DATの下は検索しない)
        public List<OneVal> GetSaveList(List<OneVal> list) {
            if (list == null) {
                list = new List<OneVal>();
            }
            foreach (var o in Ar) {
                list = o.GetSaveList(list);
            }
            return list;
        }

        // 階層下のOneValを検索する
        // 見つからないときnullが返る
        // この処理は多用されるため、スピードアップのため、例外を外してnullを返すようにした
        public OneVal Search(String name){
            foreach (var o in GetList(null)){
                if (o.Name == name){
                    return o;
                }
            }
            //例外では、処理が重いので、nullを返す
            return null;
        }

    }
}
