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
        private int _Row = 0;
        private int _Column = 0;
        private int _VisibleRow = 0;

        public int Top { get; set; } = 1;

        public bool Focused { get; set; }
        public bool Redraw { get; set; }
        public bool Visible
        {
            get { return _Visible; }
            set
            {
                if (_Visible == value) return;
                _Visible = value;
                OnVisibleChanged();
            }
        }
        public int Row
        {
            get { return _Row; }
            set
            {
                if (_Row == value) return;
                _Row = value;
                OnRowChanged();
            }
        }
        public int Column
        {
            get { return _Column; }
            set
            {
                if (_Column == value) return;
                _Column = value;
                OnColumnChanged();
            }
        }
        public int VisibleRow
        {
            get { return _VisibleRow; }
            set
            {
                if (_VisibleRow == value) return;
                _VisibleRow = value;
                OnVisibleRowChanged();
            }
        }


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

        protected virtual void OnVisibleChanged()
        {
            Redraw = Visible;
        }

        protected virtual void OnColumnChanged()
        {
            Redraw = Visible;
        }

        protected virtual void OnRowChanged()
        {
            Redraw = Visible;
        }

        protected virtual void OnVisibleRowChanged()
        {
            Redraw = Visible;
        }

    }



}
