using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class ServiceControl : Control
    {
        private const int headerRow = 1;
        private const int itemRow = 4;
        private int ActiveIndex = 0;
        private int ActiveIndexOffset = 0;
        public ServiceControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow + itemRow;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter )
            {
                switch (ActiveIndex)
                {
                    case 0:
                        Services.InteractiveConsoleService.instance.OnStop();
                        Services.InteractiveConsoleService.instance.OnStart();
                        break;
                    case 1:
                        Services.InteractiveConsoleService.instance.OnStart();
                        break;
                    case 2:
                        Services.InteractiveConsoleService.instance.OnStop();
                        break;
                    case 3:
                        Services.InteractiveConsoleService.Stop();
                        break;
                }
                return true;
            }

            if (key.Key == ConsoleKey.UpArrow && ActiveIndex > 0)
            {
                ActiveIndex--;
                SetActiveViewIndex();
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow && ActiveIndex < itemRow - 1)
            {
                ActiveIndex++;
                SetActiveViewIndex();
                return true;
            }

            return false;
        }

        public override void Output(int row, ConsoleContext context)
        {
            var idx = row - 1;
            var bgColor = (ActiveIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
            var frColor = (ActiveIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);

            switch (row)
            {
                case 0:
                    var msg = (cContext.Kernel == null ? "Not Runnning." : "Running.");
                    context.Write($" Status {msg}");
                    break;
                case 1:
                    context.Write($" Restart  ", frColor, bgColor);
                    break;
                case 2:
                    context.Write($" Start    ", frColor, bgColor);
                    break;
                case 3:
                    context.Write($" Stop     ", frColor, bgColor);
                    break;
                case 4:
                    context.Write($" Exit     ", frColor, bgColor);
                    break;
            }
        }

        private void SetActiveViewIndex()
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

        public override void KernelChanged()
        {
            base.KernelChanged();
            Redraw = true;
            cContext.Refresh();
        }

    }



}
