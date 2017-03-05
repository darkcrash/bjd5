using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Configurations;
using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Console.Controls.Editors;

namespace Bjd.Console.Controls
{
    public class EditControl : Control
    {
        public string Title;

        private const int headerRow = 2;
        private OneVal currentVal;
        private Editors.IEditor editor;
        private Editors.IEditor activeEditor;

        public EditControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow;
        }

        public bool StartEdit(OneVal val)
        {
            currentVal = val;
            switch (val.CtrlType)
            {
                case CtrlType.Int:
                    var intEd = new IntEditor();
                    editor = intEd;
                    activeEditor = editor;
                    intEd.Value = (int)val.Value;
                    Row = headerRow + editor.Row;
                    return true;
            }

            return false;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Escape)
            {
                cContext.EndEdit();
                return true;
            }
            if (key.Key == ConsoleKey.Enter)
            {
                currentVal.SetValue(editor.EditValue);
                cContext.EndEdit();
                return true;
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
            if (activeEditor != null)
            {
                var result = activeEditor.Input(key);
                if (result) return true;
            }

            return false;
        }

        public override void Output(int r, ConsoleContext context)
        {
            switch (r)
            {
                case 0:
                    var lv = currentVal;
                    var lvValue = "";
                    try { lvValue = lv.Value.ToString(); } catch { }
                    context.Write($" {lv.Name}:{lv.ToCtrlString()}={currentVal.Value.ToString()}");
                    base.Output(r, context);
                    return;
                case 1:
                    context.Write($" return option [Escape]. save [Enter]{""} ");
                    base.Output(r, context);
                    return;
            }
            if (r == Row - 1)
            {
                Func<int, int, int> Div = (v1, v2) =>
                {
                    int result = 0;
                    while (v1 > v2) { result++; v1 -= v2; }
                    return result;
                };
                var offset = Div(context.Width - editor.Width - 1, 2);
                context.Write(new string(' ', offset));
                editor.Output(context);
                context.WriteBlank();
                return;
            }

            base.Output(r, context);

        }

    }



}
