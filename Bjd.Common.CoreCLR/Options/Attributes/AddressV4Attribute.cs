using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options.Attributes
{
    public class AddressV4Attribute : ControlAttribute
    {
        public AddressV4Attribute() : base(Controls.CtrlType.AddressV4) { }
        public AddressV4Attribute(Crlf crlfType) : base(Controls.CtrlType.AddressV4, crlfType) { }
    }
}
