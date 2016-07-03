using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class RadioAttribute : ControlAttribute
    {
        public RadioAttribute() : base(Controls.CtrlType.Radio) { }
        public RadioAttribute(Crlf crlfType) : base(Controls.CtrlType.Radio, crlfType) { }

    }
}
