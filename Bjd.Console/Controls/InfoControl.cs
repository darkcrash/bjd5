using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class InfoControl : Control
    {
        private const int headerRow = 2;
        public InfoControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            return true;
        }

        public override void Output(int row, ConsoleContext context)
        {
            switch (row)
            {
                case 0:
                    context.Write($" {Define.ApplicationName} - {Define.ProductVersion}  {Define.Copyright}");
                    base.Output(row, context);
                    return;
                case 1:
                    context.Write($" {Define.HostName} {Define.OperatingSystem} ");
                    base.Output(row, context);
                    return;
            }
            var pgList = cContext.Kernel.ListPlugin;
            var idx = row - headerRow;
            var pg = pgList[idx];
            context.Write($"    {pg.Name} on {pg.PluginName}");
            base.Output(row, context);
        }

        public override void KernelChanged()
        {
            base.KernelChanged();
            if (cContext.Kernel != null)
            {
                var pg = cContext.Kernel.ListPlugin;
                Row = headerRow + pg.Count;
            }
            else
            {
                Row = headerRow;
            }
            Redraw = true;
        }

    }



}
