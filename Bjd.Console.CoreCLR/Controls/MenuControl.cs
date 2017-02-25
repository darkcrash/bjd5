using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class MenuControl : Control
    {
        private int maxMenu = 4;
        public MenuControl(ControlContext cc) : base(cc)
        {
            Row = 3;
        }

        public int ActiveMenu { get; set; } = 1;

        public override bool Input(ConsoleKeyInfo key)
        {
            var result = false;
            if (key.Key == ConsoleKey.LeftArrow && ActiveMenu > 1)
            {
                ActiveMenu -= 1;
                result = true;
            }
            if (key.Key == ConsoleKey.RightArrow && ActiveMenu < maxMenu)
            {
                ActiveMenu += 1;
                result = true;
            }
            cContext.Server.Visible = (ActiveMenu == 1);
            cContext.Logs.Visible = (ActiveMenu == 2);
            cContext.Info.Visible = (ActiveMenu == 3);

            if (key.Key == ConsoleKey.Enter && ActiveMenu == 4)
            {
                Services.InteractiveConsoleService.Stop();
                result = true;
            }

            return result;

        }

        public override void Output(int row, ConsoleContext context)
        {
            switch(row)
            {
                case 0:
                    context.Write(new string('-', Column));
                    break;
                case 1:
                    context.Write("|");
                    context.Write(" Servers ", (ActiveMenu == 1 ? ConsoleColor.White : ConsoleColor.Gray), (ActiveMenu == 1 ? ConsoleColor.DarkBlue : ConsoleColor.Black));
                    context.Write("|");
                    context.Write(" Logs    ", (ActiveMenu == 2 ? ConsoleColor.White : ConsoleColor.Gray), (ActiveMenu == 2 ? ConsoleColor.DarkBlue : ConsoleColor.Black));
                    context.Write("|");
                    context.Write(" Info    ", (ActiveMenu == 3 ? ConsoleColor.White : ConsoleColor.Gray), (ActiveMenu == 3 ? ConsoleColor.DarkBlue : ConsoleColor.Black));
                    context.Write("|");
                    context.Write(" Exit    ", (ActiveMenu == 4 ? ConsoleColor.White : ConsoleColor.Gray), (ActiveMenu == 4 ? ConsoleColor.DarkBlue : ConsoleColor.Black));
                    context.Write("|");
                    break;
                case 2:
                    context.Write(new string('-', Column));
                    break;
            }
        }

    }



}
