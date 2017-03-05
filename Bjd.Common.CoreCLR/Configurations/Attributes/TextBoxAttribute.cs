using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class TextBoxAttribute : ControlAttribute
    {
        public TextBoxAttribute() : base(Controls.CtrlType.TextBox) { }
        public TextBoxAttribute(Crlf crlfType) : base(Controls.CtrlType.TextBox, crlfType) { }

    }
}
