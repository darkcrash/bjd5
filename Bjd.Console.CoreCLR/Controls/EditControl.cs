using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.Console.Controls
{
    public class EditControl : Control
    {
        public string Title;

        private const int headerRow = 2;
        private Editors.IEditor editor;
        private Editors.IEditor activeEditor;

        public EditControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (activeEditor != null)
            {
                var result = activeEditor.Input(key);
                if (result) return true;
            }
            if (key.Key == ConsoleKey.Tab)
            {
                if (key.Modifiers == ConsoleModifiers.Shift)
                {

                    return true;
                }
                else
                {

                    return true;
                }
            }

            return false;
        }

        public override void Output(int r, ConsoleContext context)
        {
            switch (r)
            {
                case 0:
                    context.Write($" {Title} ");
                    base.Output(r, context);
                    return;
                case 1:
                    context.Write($" return select option [BackSpace] or [Escape]{""} ");
                    base.Output(r, context);
                    return;
            }
            if (r == Row)
            {

                return;
            }

            base.Output(r, context);

        }

    }



}
