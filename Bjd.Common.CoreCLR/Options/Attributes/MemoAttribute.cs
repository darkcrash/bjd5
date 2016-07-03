using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class MemoAttribute : ControlAttribute
    {
        public MemoAttribute() : base(Controls.CtrlType.Memo) { }
        public MemoAttribute(Crlf crlfType) : base(Controls.CtrlType.Memo, crlfType) { }

    }
}
