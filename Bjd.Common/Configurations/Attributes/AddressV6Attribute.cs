using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class AddressV6Attribute : ControlAttribute
    {
        public AddressV6Attribute() : base(Controls.CtrlType.AddressV6) { }
        public AddressV6Attribute(Crlf crlfType) : base(Controls.CtrlType.AddressV6, crlfType) { }

    }
}
