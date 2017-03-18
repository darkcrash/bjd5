using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Servers;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Console.Controls
{
    public class ServerControl : Control
    {
        private int headerRow = 2;
        private int ActiveServerIndex = 0;
        private int ActiveServerIndexOffset = 0;

        private int RefreshInterval = 1000;
        private bool isRefreshInterval = false;
        private Task RefreshIntervalTask = Task.CompletedTask;
        private ManualResetEventSlim RefreshIntervalSignal = new ManualResetEventSlim(false);
        private CancellationTokenSource RefreshIntervalCancel;

        private List<OneServer> Servers;

        public ServerControl(ControlContext cc) : base(cc)
        {
            Reload();
            isRefreshInterval = true;

            RefreshIntervalCancel = new CancellationTokenSource();
            RefreshIntervalTask = new Task(() => RefreshIntervalLoop(), RefreshIntervalCancel.Token, TaskCreationOptions.LongRunning);
            RefreshIntervalTask.Start();

        }

        public override void Dispose()
        {
            RefreshIntervalCancel.Cancel(true);

            try { RefreshIntervalTask.Wait(); } catch { }

            RefreshIntervalCancel.Dispose();
            RefreshIntervalSignal.Dispose();

            RefreshIntervalCancel = null;
            RefreshIntervalSignal = null;

            base.Dispose();

        }

        public override bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.R)
            {
                Reload();
                return true;
            }
            if (key.Key == ConsoleKey.I)
            {
                isRefreshInterval = !isRefreshInterval;
                Interval();
                return true;
            }

            if (key.Key == ConsoleKey.OemPlus || key.Key == ConsoleKey.Add)
            {
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    var sv = Servers[ActiveServerIndex];
                    sv.MaxCount++;
                }
                else
                {
                    RefreshInterval += 100;
                }
                return true;
            }
            if (key.Key == ConsoleKey.OemMinus || key.Key == ConsoleKey.Subtract)
            {
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    var sv = Servers[ActiveServerIndex];
                    if (sv.MaxCount > 0) sv.MaxCount--;
                }
                else if (RefreshInterval > 100)
                {
                    RefreshInterval -= 100;
                }
                return true;
            }

            if (key.Key == ConsoleKey.UpArrow && ActiveServerIndex > 0)
            {
                ActiveServerIndex--;
                SetActiveServerViewIndex();
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow && ActiveServerIndex < Servers.Count - 1)
            {
                ActiveServerIndex++;
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
                    context.Write($"[R] Reload. [I] Refresh interval({isRefreshInterval} - {RefreshInterval}). [+][-] Up Down Interval.");
                    base.Output(row, context);
                    return;
                case 1:
                    context.Write($"[Shift]+[+][-] Up Down MaxThread.");
                    base.Output(row, context);
                    return;
            }
            var idx = row - headerRow + ActiveServerIndexOffset;
            if (Servers.Count <= idx) return;
            var sv = Servers[idx];
            var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
            var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
            context.Write($" {sv.ToConsoleString()}", frColor, bgColor);
            base.Output(row, context);

        }

        public override void KernelChanged()
        {
            Reload();
        }

        protected override void OnVisibleChanged()
        {
            base.OnVisibleChanged();
            Interval();
        }

        private void SetActiveServerViewIndex()
        {
            var vRows = VisibleRow - headerRow;
            var vServerMinIndex = ActiveServerIndexOffset;
            var vServerMaxIndex = vRows + ActiveServerIndexOffset - 1;
            if (vServerMinIndex > ActiveServerIndex)
            {
                ActiveServerIndexOffset--;
            }
            if (vServerMaxIndex <= ActiveServerIndex)
            {
                ActiveServerIndexOffset++;
            }

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
            Redraw = true;
        }

        private void Interval()
        {
            if (Visible && isRefreshInterval)
            {
                RefreshIntervalSignal.Set();
            }
            else
            {
                RefreshIntervalSignal.Reset();
            }


        }

        private void RefreshIntervalLoop()
        {
            try
            {
                while (true)
                {
                    RefreshIntervalSignal.Wait(RefreshIntervalCancel.Token);
                    if (RefreshIntervalCancel.IsCancellationRequested) return;
                    Task.Delay(RefreshInterval).Wait(RefreshIntervalCancel.Token);
                    if (RefreshIntervalCancel.IsCancellationRequested) return;
                    if (!Visible) continue;
                    Redraw = true;
                    if (cContext != null)
                        cContext.Refresh();
                }
            }
            catch (OperationCanceledException) { }
        }

    }



}
