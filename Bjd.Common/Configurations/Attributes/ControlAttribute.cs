using Bjd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Configurations.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class ControlAttribute : Attribute
    {
        public CtrlType ControlType { get; protected set; }

        public Crlf crlfType { get; protected set; }

        public bool IsSecret { get; protected set; }

        public ControlAttribute(CtrlType t) :base ()
        {
            this.ControlType = t;
            this.crlfType = Crlf.Nextline;
            this.IsSecret = false;
        }
        public ControlAttribute(CtrlType t, Crlf crlfType) : base()
        {
            this.ControlType = t;
            this.crlfType = crlfType;
            this.IsSecret = false;
            
        }

        public OneVal Create(Kernel kernel, string name, object value)
        {
            var val = new OneVal(kernel, ControlType, name, value, this.crlfType, this.IsSecret);
            return val;
        }

    }
}
