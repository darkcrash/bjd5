using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class InfoControl : Control
    {
        public InfoControl(ControlContext cContext) :base (cContext)
        {
            Row = 4;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            return true;
        }

        public override void Output(int row, ConsoleContext context)
        {
            switch(row)
            {
                case 0:
                    break;
                case 1:
                    context.Write($" {Define.ApplicationName} - {Define.ProductVersion}");
                    break;
                case 2:
                    context.Write($" {Define.Copyright}");
                    break;
                case 3:
                    break;
            }
        }

    }



}
