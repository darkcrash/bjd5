using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd
{
    public class KernelEvents
    {
        public event EventHandler Cancel;

        internal void OnCancel(object sender)
        {
            if (this.Cancel == null) return;
            try { Cancel(sender, EventArgs.Empty); }
            catch { }
        }

    }
}
