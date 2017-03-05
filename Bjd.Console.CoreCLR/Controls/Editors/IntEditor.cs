using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Configurations;
using System.Collections.Generic;

namespace Bjd.Console.Controls.Editors
{
    public class IntEditor : IEditor
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
                return 10;
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
                Value = (int)value;
            }
        }

        public int Value = 0;

        private int digit = 1;

        public bool Input(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow)
            {
                try
                {
                    Value += Convert.ToInt32(Math.Pow(10, digit) / 10);
                }
                catch { Value = int.MaxValue; }
                return true;
            }
            if (key.Key == ConsoleKey.DownArrow)
            {
                try
                {
                    Value -= Convert.ToInt32(Math.Pow(10, digit) / 10);
                    if (Value < 0) Value = 0;
                }
                catch { Value = 0; }
                return true;
            }
            if (key.Key == ConsoleKey.LeftArrow)
            {
                try
                {
                    digit += 1;
                    if (digit > 10) digit = 10;
                }
                catch { digit = 10; }
                return true;
            }
            if (key.Key == ConsoleKey.RightArrow)
            {
                try
                {
                    digit -= 1;
                    if (digit < 1) digit = 1;
                }
                catch { digit = 1; }
                return true;
            }

            return false;
        }

        public void Output(ConsoleContext context)
        {
            var text = Value.ToString().PadLeft(10);
            var targetDigitIndex = 10 - digit;
            for (var i = 0; i < 10; i++)
            {
                var bgColor = (targetDigitIndex == i ? ConsoleColor.DarkYellow : ConsoleColor.DarkBlue);
                var frColor = (targetDigitIndex == i ? ConsoleColor.White : ConsoleColor.Gray);
                context.Write(text[i].ToString(), frColor, bgColor);
            }
        }
    }

}
