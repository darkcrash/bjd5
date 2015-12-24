using System;

namespace Bjd
{
    public class DrawListViewSubItemEventArgs : EventArgs
    {
        public int ItemIndex;
        public ListViewItem Item;
        public int ColumnIndex;
        public void DrawBackground() { }
    }
}
