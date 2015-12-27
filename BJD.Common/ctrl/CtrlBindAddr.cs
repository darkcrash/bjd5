using System;
using Bjd.net;
using Bjd.util;

namespace Bjd.ctrl{
    public class CtrlBindAddr : OneCtrl{

        private BindStyle _BindStyleSetting;

        private readonly Ip[] _listV4;
        private readonly Ip[] _listV6;

        public CtrlBindAddr(string help, Ip[] listV4, Ip[] listV6) : base(help){
            _listV4 = listV4;
            _listV6 = listV6;

        }

        public override CtrlType GetCtrlType(){
            return CtrlType.BindAddr;
        }

        protected override void AbstractCreate(object value, ref int tabIndex){

            //ComBox配置
            var labelStr = new[]{"IPv4", "IPv6"};

            //値の設定
            AbstractWrite(value);

        }



        protected override void AbstractDelete(){

        }


        //***********************************************************************
        // コントロールの値の読み書き
        //***********************************************************************

        protected override object AbstractRead(){
            var byndStyle = BindStyle.V46Dual;
            if (_radioButtonList[0].Checked){
                byndStyle = BindStyle.V4Only;
            }else if (_radioButtonList[1].Checked){
                byndStyle = BindStyle.V6Only;
            }
            var ipV4 = _listV4[_comboBoxList[0].SelectedIndex];
            var ipV6 = _listV6[_comboBoxList[1].SelectedIndex];

            return new BindAddr(_BindStyleSetting, ipV4, ipV6);
        }

        protected override void AbstractWrite(object value){
            if (value == null){
                return;
            }
            var bindAddr = (BindAddr) value;
            _BindStyleSetting = bindAddr.BindStyle;

            for (var i = 0; i < 2; i++){
                var list = (i == 0) ? _listV4 : _listV6;
                var ip = (i == 0) ? bindAddr.IpV4 : bindAddr.IpV6;
                var index = -1;
                for (var n = 0; n < list.Length; n++){
                    if (list[n] == ip){
                        index = n;
                        break;
                    }
                }
                if (index == -1){
                    index = 0;
                }
                _comboBoxList[i].SelectedIndex = index;
            }
            SetDisable(); //無効なオプションの表示
        }


        //***********************************************************************
        // コントロールへの有効・無効
        //***********************************************************************
        protected override void AbstractSetEnable(bool enabled){
            for (var i = 0; i < 3; i++){
                _radioButtonList[i].Enabled = enabled;
            }
            for (var i = 0; i < 2; i++){
                _comboBoxList[i].Enabled = enabled;
            }
            SetDisable(); //無効なオプションの表示
        }

        //***********************************************************************
        // OnChange関連
        //***********************************************************************
        //@Override
        //public void actionPerformed(ActionEvent e) {
        //    setOnChange();
        //    radioButtonCheckedChanged(); //ラジオボタンの変化によってコントロールの有効無効を設定する
        //}

        //***********************************************************************
        // CtrlDat関連
        //***********************************************************************
        protected override bool AbstractIsComplete(){
            //未設定状態は存在しない
            return true;
        }

        protected override string AbstractToText(){
            Util.RuntimeException("未実装");
            return "";
        }

        protected override void AbstractFromText(string s){
            Util.RuntimeException("未実装");
        }

        protected override void AbstractClear(){
            _radioButtonList[0].Checked = true;
            _comboBoxList[0].SelectedIndex = 0;
            _comboBoxList[1].SelectedIndex = 0;


        }
    }
}
