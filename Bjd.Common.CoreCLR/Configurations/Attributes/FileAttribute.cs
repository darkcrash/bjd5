using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class FileAttribute : ControlAttribute
    {
        public FileAttribute() : base(Controls.CtrlType.File) { }
        public FileAttribute(Crlf crlfType) : base(Controls.CtrlType.File, crlfType) { }

    }
}
