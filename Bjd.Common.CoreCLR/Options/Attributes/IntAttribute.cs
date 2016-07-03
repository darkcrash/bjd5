using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class IntAttribute : ControlAttribute
    {
        public IntAttribute() : base(Controls.CtrlType.Int) { }
        public IntAttribute(Crlf crlfType) : base(Controls.CtrlType.Int, crlfType) { }

    }
}
