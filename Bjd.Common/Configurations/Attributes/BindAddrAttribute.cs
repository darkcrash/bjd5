using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class BindAddrAttribute : ControlAttribute
    {
        public BindAddrAttribute() : base(Controls.CtrlType.BindAddr) { }
        public BindAddrAttribute(Crlf crlfType) : base(Controls.CtrlType.BindAddr, crlfType) { }
    }
}
