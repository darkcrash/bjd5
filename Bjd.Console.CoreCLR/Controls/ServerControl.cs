using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Servers;

namespace Bjd.Console.Controls
{
    public class ServerControl : Control
    {
        private int headerRow = 2;
        private int ActiveServerIndex = 0;
        private int ActiveServerIndexOffset = 0;
        private int RefreshInterval = 1000;

        private List<OneServer> Servers;

        public ServerControl(ControlContext cc) : base(cc)
        {
            Reload();
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.R)
            {
                Reload();
                return true;
            }

            if (key.Key == ConsoleKey.OemPlus || key.Key ==  ConsoleKey.Add)
            {
                var sv = Servers[ActiveServerIndex];
                sv.MaxCount++; 
                return true;
            }
            if (key.Key == ConsoleKey.OemMinus || key.Key == ConsoleKey.Subtract)
            {
                var sv = Servers[ActiveServerIndex];
                if (sv.MaxCount > 0) sv.MaxCount--;
                return true;
            }

            var vRows = VisibleRows - headerRow;
            var vServerMinIndex = ActiveServerIndexOffset;
            var vServerMaxIndex = vRows + ActiveServerIndexOffset - 1;
            if (key.Key == ConsoleKey.UpArrow && ActiveServerIndex > 0)
            {
                ActiveServerIndex--;
                if (vServerMinIndex > ActiveServerIndex)
                {
                    ActiveServerIndexOffset--;
                }
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow && ActiveServerIndex < Servers.Count - 1)
            {
                ActiveServerIndex++;
                if (vServerMaxIndex <= ActiveServerIndex)
                {
                    ActiveServerIndexOffset++;
                }
                return true;
            }

            return false;
        }

        public override void Output(int row, ConsoleContext context)
        {
            switch (row)
            {
                case 0:
                    context.Write("[R] Reload. [+][-] Up Down Max thread");
                    return;
                case 1:
                    context.Write("+ is Running. ");
                    return;
            }
            var idx = row - headerRow + ActiveServerIndexOffset;
            if (Servers.Count <= idx) return;
            var sv = Servers[idx];
            var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
            var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
            context.Write($" {sv.ToConsoleString()}", frColor, bgColor);

        }

        public override void KernelChanged()
        {
            Reload();
        }

        private void Reload()
        {
            if (cContext.Kernel != null)
            {
                Servers = new List<OneServer>(cContext.Kernel.ListServer);
            }
            else
            {
                Servers = new List<OneServer>();
            }
            Row = Servers.Count + headerRow;
        }

    }



}
