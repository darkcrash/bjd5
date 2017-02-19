using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd
{
    public class KernelEvents
    {
        public event EventHandler Cancel;
        public event EventHandler ListInitialized;

        internal void OnCancel(object sender)
        {
            if (this.Cancel == null) return;
            try { Cancel(sender, EventArgs.Empty); }
            catch { }
        }

        internal void OnListInitialized(object sender)
        {
            if (this.ListInitialized == null) return;
            try { ListInitialized(sender, EventArgs.Empty); }
            catch { }
        }
    }
}
