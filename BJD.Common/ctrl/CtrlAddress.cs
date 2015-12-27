using System;
using Bjd.net;
using Bjd.util;

namespace Bjd.ctrl{
    public class CtrlAddress : OneCtrl{

        private Ip value;


        public CtrlAddress(string help) : base(help){
            
        }

        public override CtrlType GetCtrlType(){
            return CtrlType.AddressV4;
        }

        protected override void AbstractCreate(object value, ref int tabIndex){
            var left = Margin;
            var top = Margin;


            //値の設定
            AbstractWrite(value);

        }


        protected override void AbstractDelete(){
        }

        //***********************************************************************
        // コントロールの値の読み書き
        //***********************************************************************
        protected override object AbstractRead(){
            try{
                return value;
            }
            catch (Exception e){
                //ここでの例外は、設計の問題
                Util.RuntimeException(e.Message);
                return null;
            }
        }

        protected override void AbstractWrite(object value){
            this.value = (Ip)value;
        }

        //***********************************************************************
        // コントロールへの有効・無効
        //***********************************************************************
        protected override void AbstractSetEnable(bool enabled){
        }

        //***********************************************************************
        // OnChange関連
        //***********************************************************************
        //@Override
        //public void changedUpdate(DocumentEvent e) {
        //}
        //@Override
        //public void insertUpdate(DocumentEvent e) {
        //    setOnChange();
        //}
        //@Override
        //public void removeUpdate(DocumentEvent e) {
        //    setOnChange();
        //}

        //***********************************************************************
        // CtrlDat関連
        //***********************************************************************

        protected override bool AbstractIsComplete(){
            return (this.value != null);
        }

        protected override string AbstractToText(){
            var ip = (Ip) AbstractRead();
            return ip.ToString();
        }

        protected override void AbstractFromText(string s){
            Ip ip;
            try{
                ip = new Ip(s);
            }catch (ValidObjException){
                ip = new Ip(IpKind.V4_0);
            }
            AbstractWrite(ip);
        }

        protected override void AbstractClear(){
            this.value = null;
        }
    }
}
