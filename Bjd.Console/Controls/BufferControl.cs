using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Servers;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Memory;
using Bjd.Threading;
using Bjd.Net.Sockets;

namespace Bjd.Console.Controls
{
    public class BufferControl : Control
    {
        private int headerRow = 2;
        private int ActiveServerIndex = 0;
        private int ActiveServerIndexOffset = 0;

        private int RefreshInterval = 1000;
        private bool isRefreshInterval = false;
        private Task RefreshIntervalTask = Task.CompletedTask;
        private ManualResetEventSlim RefreshIntervalSignal = new ManualResetEventSlim(false, 0);
        private CancellationTokenSource RefreshIntervalCancel;

        private List<BufferPool> BufferPoolList;
        private List<CharsPool> CharsPoolList;
        private List<SimpleResetPool> SimpleResetPoolList;
        private List<SimpleAsyncAwaiterPool> SimpleAsyncAwaitPoolList;
        private List<SockQueuePool> SockQueuePoolList;

        public BufferControl(ControlContext cc) : base(cc)
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
                    //var sv = Servers[ActiveServerIndex];
                    //sv.MaxCount++;
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
                    //var sv = Servers[ActiveServerIndex];
                    //if (sv.MaxCount > 0) sv.MaxCount--;
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
            if (key.Key == ConsoleKey.DownArrow && ActiveServerIndex < (BufferPoolList.Count + CharsPoolList.Count + SimpleResetPoolList.Count + SockQueuePoolList.Count) - 1)
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
                    context.Write($"");
                    base.Output(row, context);
                    return;
            }
            var idx = row - headerRow + ActiveServerIndexOffset;
            if (BufferPoolList.Count > idx)
            {
                var sv = BufferPoolList[idx];
                var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write($"Buffer    :{sv.ToConsoleString()}", frColor, bgColor);
                base.Output(row, context);
                return;
            }
            var cnt = BufferPoolList.Count + CharsPoolList.Count;
            if (cnt > idx)
            {
                var sv = CharsPoolList[idx - BufferPoolList.Count];
                var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write($"Chars     :{sv.ToConsoleString()}", frColor, bgColor);
                base.Output(row, context);
                return;
            }
            cnt = cnt + SimpleResetPoolList.Count;
            if (cnt > idx)
            {
                var sv = SimpleResetPoolList[idx - BufferPoolList.Count - CharsPoolList.Count];
                var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write($"ResetEvent:{sv.ToConsoleString()}", frColor, bgColor);
                base.Output(row, context);
            }
            cnt = cnt + SimpleAsyncAwaitPoolList.Count;
            if (cnt > idx)
            {
                var sv = SimpleAsyncAwaitPoolList[idx - BufferPoolList.Count - CharsPoolList.Count - SimpleResetPoolList.Count];
                var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write($"Awaiter   :{sv.ToConsoleString()}", frColor, bgColor);
                base.Output(row, context);
            }
            cnt = cnt + SockQueuePoolList.Count;
            if (cnt > idx)
            {
                var sv = SockQueuePoolList[idx - BufferPoolList.Count - CharsPoolList.Count - SimpleResetPoolList.Count - SimpleAsyncAwaitPoolList.Count];
                var bgColor = (ActiveServerIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                var frColor = (ActiveServerIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write($"SockQueue :{sv.ToConsoleString()}", frColor, bgColor);
                base.Output(row, context);
            }

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
                BufferPoolList = new List<BufferPool>();
                foreach (var p in BufferPool.PoolList)
                {
                    BufferPoolList.Add((BufferPool)p);
                }
                CharsPoolList = new List<CharsPool>();
                foreach (var p in CharsPool.PoolList)
                {
                    CharsPoolList.Add((CharsPool)p);
                }
                SimpleResetPoolList = new List<SimpleResetPool>();
                foreach (var p in SimpleResetPool.PoolList)
                {
                    SimpleResetPoolList.Add((SimpleResetPool)p);
                }
                SimpleAsyncAwaitPoolList = new List<SimpleAsyncAwaiterPool>();
                foreach (var p in SimpleAsyncAwaiterPool.PoolList)
                {
                    SimpleAsyncAwaitPoolList.Add((SimpleAsyncAwaiterPool)p);
                }
                SockQueuePoolList = new List<SockQueuePool>();
                foreach (var p in SockQueuePool.PoolList)
                {
                    SockQueuePoolList.Add((SockQueuePool)p);
                }
            }
            else
            {
                BufferPoolList = new List<BufferPool>();
                CharsPoolList = new List<CharsPool>();
                SimpleResetPoolList = new List<SimpleResetPool>();
                SimpleAsyncAwaitPoolList = new List<SimpleAsyncAwaiterPool>();
                SockQueuePoolList = new List<SockQueuePool>();
            }
            Row = BufferPoolList.Count + CharsPoolList.Count + SimpleResetPoolList.Count + SimpleAsyncAwaitPoolList.Count + SockQueuePoolList.Count + headerRow;
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
