using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class GroupAttribute : ControlAttribute
    {
        public GroupAttribute() : base(Controls.CtrlType.Group) { }
        public GroupAttribute(Crlf crlfType) : base(Controls.CtrlType.Group, crlfType) { }
    }
}

