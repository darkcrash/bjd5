using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class CheckBoxAttribute : ControlAttribute
    {
        public CheckBoxAttribute() : base(Controls.CtrlType.CheckBox) { }
        public CheckBoxAttribute(Crlf crlfType) : base(Controls.CtrlType.CheckBox, crlfType) { }

    }
}
