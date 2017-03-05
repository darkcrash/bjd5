using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class HiddenAttribute : ControlAttribute
    {
        public HiddenAttribute() : base(Controls.CtrlType.Hidden)
        {
            this.IsSecret = true;
        }
        public HiddenAttribute(Crlf crlfType) : base(Controls.CtrlType.Font, crlfType)
        {
            this.IsSecret = true;
        }

    }
}
