using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Configurations;
using System.Collections.Generic;

namespace Bjd.Console.Controls
{
    public class OptionControl : Control
    {
        private const int headerRow = 2;
        private ListOption options;
        private List<string> optionsList;
        private ConfigurationSmart currentOption;
        private Dat currentDat;
        private int ActiveIndex = 0;
        private int ActiveIndexOffset = 0;
        private int ActiveOptionIndex = 0;
        private int ActiveOptionIndexOffset = 0;
        private int ActiveListValIndex = 0;
        private int ActiveListValIndexOffset = 0;
        private string title = "";

        public OptionControl(ControlContext cContext) : base(cContext)
        {
            Row = headerRow;
        }

        public override bool Input(ConsoleKeyInfo key)
        {
            var count = 0;
            if (currentDat != null)
            {
                count = currentDat.GetList().Count;
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Escape)
                {
                    currentDat = null;
                    Row = currentOption.ListVal.Count + headerRow;
                    ActiveIndex = ActiveListValIndex;
                    ActiveIndexOffset = ActiveListValIndexOffset;
                    SetActiveServerViewIndex();
                    return true;
                }
            }
            else if (currentOption != null)
            {
                count = currentOption.ListVal.Count;
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Escape)
                {
                    currentOption = null;
                    Row = options.Count + headerRow;
                    ActiveIndex = ActiveOptionIndex;
                    ActiveIndexOffset = ActiveOptionIndexOffset;
                    SetActiveServerViewIndex();
                    title = "";
                    return true;
                }
                var item = currentOption.ListVal[ActiveIndex];
                if (key.Key == ConsoleKey.Enter)
                {
                    if (item.ValueType == typeof(Dat))
                    {
                        ActiveListValIndex = ActiveIndex;
                        ActiveListValIndexOffset = ActiveIndexOffset;
                        currentDat = item.Value as Dat;
                        Row = currentDat.GetList().Count + headerRow;
                        ActiveIndex = 0;
                        ActiveIndexOffset = 0;
                        return true;
                    }
                    if (cContext.StartEdit(item))
                    {
                        return true;
                    }
                }
            }
            else if (options != null)
            {
                count = options.Count;
                if (key.Key == ConsoleKey.Enter)
                {
                    ActiveOptionIndex = ActiveIndex;
                    ActiveOptionIndexOffset = ActiveIndexOffset;
                    currentOption = options[ActiveIndex];
                    Row = currentOption.ListVal.Count + headerRow;
                    ActiveIndex = 0;
                    ActiveIndexOffset = 0;

                    if (currentOption != null)
                    {
                        try { title = $"[{currentOption.MenuStr}]"; }
                        catch
                        {
                            try { title = $"[{currentOption.NameTag}]"; } catch { }
                        }
                    }
                    return true;
                }

                if (key.Key == ConsoleKey.S)
                {
                    var opt = options[ActiveIndex];
                    cContext.Kernel.Configuration.SaveJson(opt.NameTag, opt.ListVal);
                    return true;
                }

            }

            if (key.Key == ConsoleKey.UpArrow && ActiveIndex > 0)
            {
                ActiveIndex--;
                SetActiveServerViewIndex();
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow && ActiveIndex < count - 1)
            {
                ActiveIndex++;
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
                    context.Write($" ->{title}");
                    if (currentDat != null)
                    {
                        var title2 = $"[{currentOption.ListVal[ActiveListValIndex].Name}]";
                        context.Write($"->{title2}");
                    }
                    base.Output(row, context);
                    return;
                case 1:
                    if (currentOption == null)
                    {
                        var item = optionsList[ActiveIndex];
                        context.Write($"Save [S]  {item} ");
                    }
                    else
                    {
                        context.Write($" return [BackSpace] or [Escape].");
                    }
                    base.Output(row, context);
                    return;
            }
            var idx = row - headerRow + ActiveIndexOffset;
            var bgColor = (ActiveIndex == idx ? ConsoleColor.DarkBlue : ConsoleColor.Black);
            var frColor = (ActiveIndex == idx ? ConsoleColor.White : ConsoleColor.Gray);
            if (currentDat != null)
            {
                var lo = currentDat.GetList()[idx];
                var loValue = "";
                try { loValue = lo.Value.ToString(); } catch { }
                context.Write($"    {lo.Name}:{lo.ToCtrlString()}={loValue}", frColor, bgColor);
            }
            else if (currentOption != null)
            {
                var lv = currentOption.ListVal[idx];
                var lvValue = "";
                try { lvValue = lv.Value.ToString(); } catch { }
                context.Write($"    {lv.Name}:{lv.ToCtrlString()}={lvValue}", frColor, bgColor);
            }
            else
            {
                var item = optionsList[idx];
                context.Write(item, frColor, bgColor);
                //var lo = options[idx];
                //context.Write($"    {lo.NameTag} -> {lo.MenuStr}", frColor, bgColor);
            }
            base.Output(row, context);

        }


        public override void KernelChanged()
        {
            base.KernelChanged();
            if (cContext.Kernel != null)
            {
                options = cContext.Kernel.ListOption;
                Row = headerRow + options.Count;
                optionsList = new List<string>();
                foreach (var item in options)
                {
                    optionsList.Add($"    {item.NameTag} -> {item.MenuStr}");
                }
            }
            else
            {
                Row = headerRow;
            }
            Redraw = true;
        }

        private void SetActiveServerViewIndex()
        {
            var vRows = VisibleRow - headerRow;
            var vServerMinIndex = ActiveIndexOffset;
            var vServerMaxIndex = vRows + ActiveIndexOffset - 1;
            if (vServerMinIndex > ActiveIndex)
            {
                ActiveIndexOffset--;
            }
            if (vServerMaxIndex <= ActiveIndex)
            {
                ActiveIndexOffset++;
            }

        }
    }



}
