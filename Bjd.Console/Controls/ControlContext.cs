using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Logs;
using Bjd.Configurations;

namespace Bjd.Console.Controls
{
    public class ControlContext : IDisposable
    {
        System.Threading.Tasks.Task KeyinputTask;
        System.Threading.Tasks.Task OutputTask;
        System.Threading.ManualResetEventSlim refresh = new System.Threading.ManualResetEventSlim(false, 0);

        private Kernel _Kernel;
        private LogInteractiveConsoleService _LogService;
        private List<Control> ctrls = new List<Control>();
        private ConsoleContext consoleContext;
        private int beforeOptionRow;

        public List<Control> Controls { get { return ctrls; } }

        public MenuControl Menu { get; }
        public ServerControl Server { get; }
        public LogsControl Logs { get; }
        public OptionControl Option { get; }
        public InfoControl Info { get; }
        public ServiceControl Service { get; }
        public EditControl Edit { get; }

        public Kernel Kernel { get { return _Kernel; } set { KernelChanged(value); } }

        public LogInteractiveConsoleService LogService { get { return _LogService; } set { LogServiceChanged(value); } }

        public ControlContext(System.Threading.CancellationToken token)
        {

            Menu = new MenuControl(this);
            ctrls.Add(Menu);

            Server = new ServerControl(this);
            ctrls.Add(Server);

            Logs = new LogsControl(this);
            ctrls.Add(Logs);

            Option = new OptionControl(this);
            ctrls.Add(Option);

            Info = new InfoControl(this);
            ctrls.Add(Info);

            Service = new ServiceControl(this);
            ctrls.Add(Service);

            Edit = new EditControl(this);
            ctrls.Add(Edit);

            Menu.Visible = true;
            Menu.Focused = true;
            Menu.Top = 0;

            Server.Visible = true;
            Server.Focused = true;
            Server.Top = 3;

            Logs.Visible = false;
            Logs.Focused = true;
            Logs.Top = 3;

            Option.Visible = false;
            Option.Focused = true;
            Option.Top = 3;

            Info.Visible = false;
            Info.Focused = false;
            Info.Top = 3;

            Service.Visible = false;
            Service.Focused = true;
            Service.Top = 3;

            Edit.Visible = false;
            Edit.Focused = true;
            Edit.Top = 5;

            consoleContext = new ConsoleContext(token);

            OutputTask = new System.Threading.Tasks.Task(() => Output(token), token, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            OutputTask.Start();

            KeyinputTask = new System.Threading.Tasks.Task(() => KeyInputLoop(), token, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            KeyinputTask.Start();


        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            foreach(var ctrl in ctrls)
            {
                ctrl.Dispose();
            }

            OutputTask.Wait();

            refresh.Dispose();

            consoleContext.Dispose();
        }


        protected void KeyInputLoop()
        {
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (isDisposed) return;
                var reqRefresh = false;
                foreach (var ctrl in ctrls)
                {
                    if (!ctrl.Visible) continue;
                    if (!ctrl.Focused) continue;
                    if (ctrl.Input(key))
                    {
                        reqRefresh = true;
                        ctrl.Redraw = true;
                        break;
                    }
                }
                if (reqRefresh) Refresh();
            }
        }

        protected void Output(System.Threading.CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return;
                var isWindowStateChanged = consoleContext.WindowStateChanged();

                int height = consoleContext.InitialCursorTop;
                int maxHeightOffset = consoleContext.MaxHeight + consoleContext.InitialCursorTop;

                foreach (var ctrl in ctrls)
                {
                    if (height >= maxHeightOffset) break;
                    if (!ctrl.Visible) continue;
                    consoleContext.SetTop(ctrl);
                    if (!isWindowStateChanged && !ctrl.Redraw)
                    {
                        height += ctrl.Row;
                        if (height >= maxHeightOffset) height = maxHeightOffset;
                        continue;
                    }
                    for (var i = 0; i < ctrl.Row; i++)
                    {
                        if (height >= maxHeightOffset) break;
                        System.Console.SetCursorPosition(0, height);
                        try
                        {
                            ctrl.Output(i, consoleContext);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception) { }
                        height++;
                    }
                    ctrl.Redraw = false;
                }

                // clear Blank
                System.Console.SetCursorPosition(0, height);
                consoleContext.BlankToEnd();

                if (requestRefresh)
                {
                    requestRefresh = false;
                    continue;
                }
                refresh.Wait(token);
                refresh.Reset();
            }
        }


        private void KernelChanged(Kernel k)
        {
            _Kernel = k;
            foreach (var c in ctrls)
            {
                c.KernelChanged();
            }
        }

        private void LogServiceChanged(LogInteractiveConsoleService lics)
        {
            if (_LogService != null) _LogService.Appended -= _LogService_Appended;
            _LogService = lics;
            _LogService.Appended += _LogService_Appended;
        }

        private void _LogService_Appended(object sender, EventArgs e)
        {
            if (!Logs.Visible) return;
            Logs.Redraw = true;
            Refresh();

        }

        private bool requestRefresh = false;

        public void Refresh()
        {
            requestRefresh = true;
            if (refresh != null)
                refresh.Set();
        }


        public bool StartEdit(OneVal val)
        {
            var result = Edit.StartEdit(val);
            if (result)
            {
                Edit.Visible = true;
                Menu.Focused = false;
                Option.Focused = false;
                beforeOptionRow = Option.Row;
                Option.Row = 1;
                return true;
            }
            return false;
        }


        public void EndEdit()
        {
            Edit.Visible = false;
            Menu.Focused = true;
            Option.Focused = true;
            Option.Row = beforeOptionRow;
        }

    }



}
