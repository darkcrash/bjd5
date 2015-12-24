using System;
using System.Collections.Generic;

namespace Bjd
{
    public class Control : IDisposable
    {
        public MenuStrip MenuStrip { get; set; }
        public ContextMenuStrip ContextMenuStrip { get; set; }
        public object Tag { get; set; }
        public Size Size { get; set; }
        public int Maximum { get; set; }
        public int Index { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SelectedIndex { get; set; }
        public TreeNode SelectedNode { get; set; }
        public int TabIndex { get; set; }
        public bool TabStop { get; set; }
        public bool Multiline { get; set; }
        public bool AutoSize { get; set; }
        public bool Visible { get; set; }
        public bool ReadOnly { get; set; }
        public bool Enabled { get; set; }
        public bool Checked { get; set; }
        public bool ShowInTaskbar { get; set; }
        public bool FullRowSelect { get; set; }
        public bool Selected { get; set; }
        public bool InvokeRequired { get; } = false;
        public char PasswordChar { get; set; }
        public string Mask { get; set; }
        public virtual string Text { get; set; }
        public string Name { get; set; }
        public Control Parent { get; set; }
        public object Value { get; set; }
        public object View { get; set; }
        public Cursors Cursor { get; set; }
        public List<Control> Controls { get; } = new List<Control>();
        public List<Column> Columns { get; } = new List<Column>();
        public List<TreeNode> Nodes { get; } = new List<TreeNode>();
        public List<ListViewItem> Items { get; } = new List<ListViewItem>();
        public List<ListViewItem> SubItems { get; } = new List<ListViewItem>();
        public List<ListViewItem> SelectedItems { get; } = new List<ListViewItem>();
        public List<ListViewItem> HideSelection { get; } = new List<ListViewItem>();
        public ToolStripItemCollection DropDownItems { get; } = new ToolStripItemCollection();
        


        public object BeginInvoke(Delegate target) { return null; }
        public void SuspendLayout() { }
        public void ResumeLayout() { }
        public void ResumeLayout(bool p1) { }
        public object Invoke(Delegate target) { return null; }
        public void BeginUpdate() { }
        public void EndUpdate() { }
        public void Update() { }
        public void Show() { }
        public DialogResult ShowDialog() => DialogResult.OK;
        public void Focus() { }
        public void Close() { }
        public void Expand() { }

        public virtual void WndProc(ref object msg) { }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~Form() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

        public event EventHandler Click;
        public event EventHandler SelectedIndexChanged;
        public event EventHandler TextChanged;
        public event EventHandler CheckedChanged;

    }
}
