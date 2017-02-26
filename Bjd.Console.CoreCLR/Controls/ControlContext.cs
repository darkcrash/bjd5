using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Logs;

namespace Bjd.Console.Controls
{
    public class ControlContext
    {
        System.Threading.Tasks.Task KeyinputTask;
        System.Threading.Tasks.Task OutputTask;
        System.Threading.AutoResetEvent refresh = new System.Threading.AutoResetEvent(false);

        private Kernel _Kernel;
        private LogInteractiveConsoleService _LogService;
        private List<Control> ctrls = new List<Control>();
        public List<Control> Controls { get { return ctrls; } }

        public MenuControl Menu { get; }
        public ServerControl Server { get; }
        public LogsControl Logs { get; }
        public InfoControl Info { get; }
        public ServiceControl Service { get; }

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

            Info = new InfoControl(this);
            ctrls.Add(Info);

            Service = new ServiceControl(this);
            ctrls.Add(Service);

            Menu.Visible = true;
            Menu.Focused = true;
            Menu.Top = 0;

            Server.Visible = true;
            Server.Focused = true;
            Server.Top = 3;

            Logs.Visible = false;
            Logs.Focused = true;
            Logs.Top = 3;

            Info.Visible = false;
            Info.Focused = false;
            Info.Top = 3;

            Service.Visible = false;
            Service.Focused = true;
            Service.Top = 3;

            OutputTask = new System.Threading.Tasks.Task(() => Output(), token, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            OutputTask.Start();

            KeyinputTask = new System.Threading.Tasks.Task(() => KeyInputLoop(), token, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            KeyinputTask.Start();

        }

        protected void KeyInputLoop()
        {
            while (true)
            {
                var key = System.Console.ReadKey(true);
                var reqRefresh = false;
                foreach (var ctrl in ctrls)
                {
                    if (!ctrl.Visible) continue;
                    if (!ctrl.Focused) continue;
                    if (ctrl.Input(key))
                    {
                        reqRefresh = true;
                        ctrl.Redraw = true;
                    }
                }
                if (reqRefresh) refresh.Set();
            }
        }


        protected void Output()
        {
            var cc = new ConsoleContext();
            while (true)
            {
                var isWindowStateChanged = cc.WindowStateChanged();

                int height = 0;

                foreach (var ctrl in ctrls)
                {
                    if (height >= cc.MaxHeight) break;
                    if (!ctrl.Visible) continue;
                    cc.SetTop(ctrl);
                    if (!isWindowStateChanged && !ctrl.Redraw)
                    {
                        height += ctrl.Row;
                        if (height >= cc.MaxHeight) height = cc.MaxHeight;
                        continue;
                    }
                    for (var i = 0; i < ctrl.Row; i++)
                    {
                        if (height >= cc.MaxHeight) break;
                        System.Console.SetCursorPosition(0, height);
                        cc.WriteBlank();
                        System.Console.SetCursorPosition(0, height);
                        try
                        {
                            ctrl.Output(i, cc);
                        }
                        catch (Exception ex) { }
                        height++;
                    }
                    ctrl.Redraw = false;
                }

                // clear Blank
                System.Console.SetCursorPosition(0, height);
                cc.BlankToEnd();

                refresh.WaitOne();
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

        public void Refresh()
        {
            if (refresh != null)
                refresh.Set();
        }

    }



}
