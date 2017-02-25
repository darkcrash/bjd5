using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Servers;

namespace Bjd.Console.Controls
{
    public class LogsControl : Control
    {
        private List<OneServer> Servers;
        private string[] internalBuffer;
        public LogsControl(ControlContext cc) : base(cc)
        {
            Row = 50;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            return true;
        }

        public override void Output(int row, ConsoleContext context)
        {
            if (row == 0)
            {
                internalBuffer = cContext.LogService.GetBuffer();
            }

            context.Write(internalBuffer[internalBuffer.Length - row - 1]);

        }


    }



}
