using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class Control
    {
        protected ControlContext cContext;
        private bool _Visible = false;
        public int VisibleRows { get; set; } = 0;

        public int Top { get; set; } = 1;

        public bool Focused { get; set; }
        public bool Visible { get { return _Visible; } set { _Visible = value; OnVisibleChanged(); } }
        public int Row { get; set; }
        public int Column { get; set; }


        public Control(ControlContext cc)
        {
            this.cContext = cc;
            Column = System.Console.WindowWidth;
        }

        public virtual void Output(int row, ConsoleContext context)
        {
        }

        public virtual bool Input(ConsoleKeyInfo key)
        {
            return true;
        }

        public virtual void KernelChanged() { }

        protected virtual void OnVisibleChanged() { }

    }



}
