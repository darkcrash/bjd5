using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Options;

namespace Bjd.Console.Controls
{
    public class OptionControl : Control
    {
        private const int headerRow = 2;
        private ListOption options;
        private OneOption currentOption;
        private int ActiveOptionIndex = 0;
        private int ActiveOptionIndexOffset = 0;
        private int ActiveIndex = 0;
        private int ActiveIndexOffset = 0;

        public OptionControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            var count = 0;
            if (currentOption != null)
            {
                count = currentOption.ListVal.Count;
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Escape)
                {
                    currentOption = null;
                    ActiveIndex = ActiveOptionIndex;
                    ActiveIndexOffset = ActiveOptionIndexOffset;
                    SetActiveServerViewIndex();
                    return true;
                }
            }
            else
            {
                count = options.Count;
                if (key.Key == ConsoleKey.Enter)
                {
                    ActiveOptionIndex = ActiveIndex;
                    ActiveOptionIndexOffset = ActiveIndexOffset;
                    currentOption = options[ActiveIndex];
                    ActiveIndex = 0;
                    SetActiveServerViewIndex();
                    return true;
                }
            }
            if (key.Key == ConsoleKey.UpArrow && ActiveIndex > 0)
            {
                ActiveIndex--;
                SetActiveServerViewIndex();
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow && ActiveIndex < count - 1)
            {
                ActiveIndex++;
                SetActiveServerViewIndex();
                return true;
            }

            return false;
        }

        public override void Output(int row, ConsoleContext context)
        {
            switch (row)
            {
                case 0:
                    var title = (currentOption != null ? currentOption.MenuStr : "");
                    context.Write($" ->[{title}]");
                    return;
                case 1:
                    context.Write($" return select option [BackSpace] or [Escape]{""} ");
                    return;
            }
            var idx = row - headerRow + ActiveIndexOffset;
            if (currentOption == null)
            {
                var lo = options[idx];

                var bgColor = (ActiveIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);

                context.Write($"    {lo.NameTag} on {lo.MenuStr}", frColor, bgColor);
            }
            else
            {
                var lv = currentOption.ListVal[idx];

                var bgColor = (ActiveIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);

                context.Write($"    {lv.Name}:{lv.Value}", frColor, bgColor);

            }
        }

        public override void KernelChanged()
        {
            base.KernelChanged();
            if (cContext.Kernel != null)
            {
                options = cContext.Kernel.ListOption;
                Row = headerRow + options.Count;
            }
            else
            {
                Row = headerRow;
            }
            Redraw = true;
        }

        private void SetActiveServerViewIndex()
        {
            var vRows = VisibleRow - headerRow;
            var vServerMinIndex = ActiveIndexOffset;
            var vServerMaxIndex = vRows + ActiveIndexOffset - 1;
            if (vServerMinIndex > ActiveIndex)
            {
                ActiveIndexOffset--;
            }
            if (vServerMaxIndex <= ActiveIndex)
            {
                ActiveIndexOffset++;
            }

        }
    }



}
