using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Servers;
using System.Linq;

namespace Bjd.Console.Controls
{
    public class LogsControl : Control
    {
        private string[] internalBuffer;
        public LogsControl(ControlContext cc) : base(cc)
        {
            Row = 50;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.I)
            {
                cContext.LogService.InformationEnabled = !cContext.LogService.InformationEnabled;
                return true;
            }
            if (key.Key == ConsoleKey.W)
            {
                cContext.LogService.WarningEnabled = !cContext.LogService.WarningEnabled;
                return true;
            }
            if (key.Key == ConsoleKey.D)
            {
                cContext.LogService.DetailEnabled = !cContext.LogService.DetailEnabled;
                return true;
            }
            return false;
        }
        public override void Output(int row, ConsoleContext context)
        {
            if (row == 0)
            {
                internalBuffer = cContext.LogService.GetBuffer(VisibleRow - 1);
                
                context.Write($"[I] Infomation:{cContext.LogService.InformationEnabled} [D] Detail:{cContext.LogService.DetailEnabled} [W] Warnning:{cContext.LogService.WarningEnabled}");
                return;
            }
            var idx = VisibleRow - 1 - row;
            if (internalBuffer.Length > idx)
            {
                context.Write(internalBuffer[idx]);
            }

        }

        protected override void OnVisibleRowChanged()
        {
            Row = VisibleRow;
            base.OnVisibleRowChanged();
        }

    }



}
