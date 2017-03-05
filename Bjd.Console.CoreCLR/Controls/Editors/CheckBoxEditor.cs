using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Configurations;
using System.Collections.Generic;

namespace Bjd.Console.Controls.Editors
{
    public class CheckBoxEditor : IEditor
    {
        public int Row
        {
            get
            {
                return 1;
            }
        }

        public int Width
        {
            get
            {
                return 21;
            }
        }

        public object EditValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (bool)value;
            }
        }

        public bool Value = false;


        public bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
            {
                try
                {
                    Value = !Value;
                }
                catch { Value = false; }
                return true;
            }

            return false;
        }

        public void Output(ConsoleContext context)
        {
            var text = "";
            if (Value)
            {
                text = $"  Checked - True  [*]";
            }
            else
            {
                text = $"Unchecked - False [ ]";

            }
            var bgColor = ConsoleColor.DarkYellow;
            var frColor = ConsoleColor.White;
            context.Write(text, frColor, bgColor);
        }
    }

}
