using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    public class FolderAttribute : ControlAttribute
    {
        public FolderAttribute() : base(Controls.CtrlType.Folder) { }
        public FolderAttribute(Crlf crlfType) : base(Controls.CtrlType.Folder, crlfType) { }

    }
}
