using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bjd.option;
using Bjd.util;

namespace Bjd.ctrl{
    public class CtrlDat : OneCtrl{


        private Dat value;

        private readonly string[] _tagList = new[]{"Add", "Edit", "Del", "Import", "Export", "Clear"};

        private readonly ListVal _listVal;

        private Lang _lang;
        private const int Add = 0;
        private const int Edit = 1;
        private const int Del = 2;
        private const int Import = 3;
        public const int Export = 4;
        private const int CLEAR = 5;

        //public CtrlDat(string help, ListVal listVal, int height, bool isJp) : base(help){
        public CtrlDat(string help, ListVal listVal, LangKind langKind) : base(help){
            _listVal = listVal;
            _lang = new Lang(langKind,"CtrlDat");
        }

        public CtrlType[] CtrlTypeList{
            get{
                var ctrlTypeList = new CtrlType[_listVal.Count];
                int i = 0;
                foreach (var o in _listVal){
                    ctrlTypeList[i++] = o.OneCtrl.GetCtrlType();
                }
                return ctrlTypeList;
            }
        }

        //OnePage(CtrlTabPage.pageList) CtrlGroup CtrlDatにのみ存在する
        public ListVal ListVal{
            get { return _listVal; }
        }

        public override CtrlType GetCtrlType(){
            return CtrlType.Dat;
        }


        protected override void AbstractCreate(object value, ref int tabIndex){

            //値の設定
            AbstractWrite(value);

        }


        //コントロールの入力内容に変化があった場合
        public virtual void ListValOnChange(){


        }


        //インポート
        private void ImportDat(List<string> lines){
            foreach (var s in lines){
                var str = s;
                var isChecked = str[0] != '#';
                str = str.Substring(2);

                //カラム数の確認
                string[] tmp = str.Split('\t');
                if (_listVal.Count != tmp.Length){
                    Msg.Show(MsgKind.Error,
                             string.Format("{0} [ {1} ] ",
                                           //_isJp
                                           //    ? "カラム数が一致しません。この行はインポートできません。"
                                           //    : "The number of column does not agree and cannot import this line.", str));
                                           //      string.Format("{0} [ {1} ] ",
                                           _lang.Value("Message003"), str));

                    continue;
                }
                //Ver5.0.0-a9 パスワード等で暗号化されていない（平文の）場合は、ここで
                bool isChange = false;
                if (isChange){
                    var sb = new StringBuilder();
                    foreach (string l in tmp){
                        if (sb.Length != 0){
                            sb.Append('\t');
                        }
                        sb.Append(l);
                    }
                    str = sb.ToString();
                }
                //同一のデータがあるかどうかを確認する
                //if (_checkedListBox.Items.IndexOf(str) != -1){
                if (ListViewItemIndexOf(str) != -1) {
                    Msg.Show(MsgKind.Error,
                        string.Format("{0} [ {1} ] ",
                            //_isJp
                            //    ? "データ重複があります。この行はインポートできません。"
                            //    : "There is data repetition and cannot import this line.", str));
                            _lang.Value("Message005"), str));
                    continue;
                }

                //int index = _checkedListBox.Items.Add(str);
                int index = ListViewItemAdd(str);

                //最初にチェック（有効）状態にする
                _listView.Items[index].Checked = isChecked;
                _listView.Items[index].Selected = true;
            }
        }


        protected override void AbstractDelete(){
	        _listVal.DeleteCtrl(); //これが無いと、グループの中のコントロールが２回目以降表示されなくなる

	    }

	    //コントロールの入力が完了しているか
        protected new virtual bool IsComplete() {
	    	return _listVal.IsComplete();
    	}

        //***********************************************************************
	    // コントロールの値の読み書き
	    //***********************************************************************
        protected override object AbstractRead(){
            return this.value;
        }

        protected override void AbstractWrite(object value){
            if (value == null){
                return;
            }
            var dat = (Dat) value;
            this.value = dat;
        }

    	//***********************************************************************
    	// コントロールへの有効・無効
	    //***********************************************************************

        protected override void AbstractSetEnable(bool enabled){
        }
    
	    //***********************************************************************
	    // OnChange関連
	    //***********************************************************************
	    // 必要なし
	    //***********************************************************************
	    // CtrlDat関連
	    //***********************************************************************
        protected override bool AbstractIsComplete(){
    		Util.RuntimeException("使用禁止");
            return false;
        }

        protected override string AbstractToText(){
    		Util.RuntimeException("使用禁止");
            return null;
        }

        protected override void AbstractFromText(string s){
    		Util.RuntimeException("使用禁止");
        }

        protected override void AbstractClear(){
    		Util.RuntimeException("使用禁止");
        }

    }

}
