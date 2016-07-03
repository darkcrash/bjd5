using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class ComboBoxAttribute : ControlAttribute
    {
        public ComboBoxAttribute() : base(Controls.CtrlType.ComboBox) { }
        public ComboBoxAttribute(Crlf crlfType) : base(Controls.CtrlType.ComboBox, crlfType) { }

    }
}
