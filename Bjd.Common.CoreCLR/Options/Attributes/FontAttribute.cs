using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class FontAttribute : ControlAttribute
    {
        public FontAttribute() : base(Controls.CtrlType.Font) { }
        public FontAttribute(Crlf crlfType) : base(Controls.CtrlType.Font, crlfType) { }

    }
}
