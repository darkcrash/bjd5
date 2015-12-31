using System;
using Bjd.util;

namespace Bjd.ctrl
{
    public abstract class OneCtrl
    {
        //OneValのコンストラクタでnameの初期化に使用される
        //OneValのコンストラクタ内以外で利用してはならない
        public string Name { get; set; }

        public string Help { get; private set; }

        //コンストラクタ
        public OneCtrl(string help)
        {
            if (help == null)
            {
                Help = "";
            }
            else {
                Help = help;
            }
        }

        //[C#] コントロールの変化
        protected void Change(object sender, EventArgs e)
        {

        }


        //コントロールの種類の取得（継承クラスで実装）
        public abstract CtrlType GetCtrlType();

        //コントロールの生成（継承クラスで実装）
        protected abstract void AbstractCreate(Object value, ref int tabIndex);

        //コントロールの生成
        public void Create(Object value, ref int tabIndex)
        {

            // 全部の子コントロールをベースとなるpanelのサイズは、abstractCreate()で変更される
            AbstractCreate(value, ref tabIndex); // panelの上に独自コントロールを配置する
        }

        //コントロールの破棄（継承クラスで実装）
        protected abstract void AbstractDelete();

        //コントロールの破棄
        public void Delete()
        {
            AbstractDelete();
        }



        // ***********************************************************************
        // コントロールの値の読み書き
        // データが無効なときnullが返る
        // ***********************************************************************
        //コントロールの値の取得(継承クラスで実装)<br>
        //TODO abstractRead() nullを返す際に、コントロールを赤色表示にする
        protected abstract Object AbstractRead();

        //コントロールの値の取得
        public Object Read()
        {
            return AbstractRead();
        }

        //コントロールの値の設定(継承クラスで実装)
        protected abstract void AbstractWrite(Object value);

        //コントロールの値の設定
        public void Write(Object value)
        {
            AbstractWrite(value);
        }

        // ***********************************************************************
        // コントロールへの有効・無効
        // ***********************************************************************
        //有効・無効の設定(継承クラスで実装)
        protected abstract void AbstractSetEnable(bool enabled);

        //有効・無効の設定
        public void SetEnable(bool enabled)
        {
            AbstractSetEnable(enabled);
        }

        // ***********************************************************************
        // CtrlDat関連　(Add/Del/Edit)の状態の変更、チェックリストボックスとのテキストの読み書き
        // ***********************************************************************
        protected abstract bool AbstractIsComplete();
        //CtrlDatで入力が入っているかどうかでボタン
        public bool IsComplete()
        {
            return AbstractIsComplete();
        }


        protected abstract String AbstractToText();

        //CtrlDatでリストボックスに追加するため使用される
        public string ToText()
        {
            return AbstractToText();
        }
        protected abstract void AbstractFromText(String s);

        //CtrlDatでリストボックスから値を戻す時、使用される
        public void FromText(String s)
        {
            AbstractFromText(s);
        }

        protected abstract void AbstractClear();
        //CtrlDatでDelDelボタンを押したときに使用される
        public void Clear()
        {
            AbstractClear();
        }
    }

}






