using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class DatAttribute : ControlAttribute
    {
        public DatAttribute() : base(Controls.CtrlType.Dat) { }
        public DatAttribute(Crlf crlfType) : base(Controls.CtrlType.Dat, crlfType) { }

    }
}
