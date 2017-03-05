using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class LabelAttribute : ControlAttribute
    {
        public LabelAttribute() : base(Controls.CtrlType.Label) { }
        public LabelAttribute(Crlf crlfType) : base(Controls.CtrlType.Label, crlfType) { }

    }
}
