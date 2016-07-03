using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class TabPageAttribute : ControlAttribute
    {
        public TabPageAttribute() : base(Controls.CtrlType.TabPage) { }
        public TabPageAttribute(Crlf crlfType) : base(Controls.CtrlType.TabPage, crlfType) { }
    }
}

