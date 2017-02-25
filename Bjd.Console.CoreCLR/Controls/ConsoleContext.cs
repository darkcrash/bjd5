using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class ConsoleContext
    {
        public ConsoleColor BackColor
        {
            get { return System.Console.BackgroundColor; }
            set { System.Console.BackgroundColor = value; }
        }

        public ConsoleColor ForeColor
        {
            get { return System.Console.ForegroundColor; }
            set { System.Console.ForegroundColor = value; }
        }

        public int VisibleRows = 0;

        public string Blank { get; }
        public string BlankLeft { get; }

        public ConsoleContext()
        {
            System.Console.CursorVisible = false;
            Blank = new string(' ', System.Console.WindowWidth);
            BlankLeft = new string(' ', System.Console.WindowWidth - 1);

        }

        public void SetTop(Control ctrl)
        {
            System.Console.SetCursorPosition(0, ctrl.Top);
            ctrl.VisibleRows = System.Console.WindowHeight - ctrl.Top;
        }

        public void Write(string message)
        {
            if (System.Console.BufferWidth < message.Length) message = message.Remove(System.Console.BufferWidth);
            System.Console.Write(message);
        }

        public void Write(string message, ConsoleColor foreColor)
        {
            var beforeForeColor = this.ForeColor;
            this.ForeColor = foreColor;
            Write(message);
            this.ForeColor = beforeForeColor;
        }

        public void Write(string message, ConsoleColor foreColor, ConsoleColor backColor)
        {
            var beforeBackColor = this.BackColor;
            this.BackColor = backColor;
            Write(message, foreColor);
            this.BackColor = beforeBackColor;
        }

        public void WriteBlank()
        {
            if (System.Console.CursorTop == System.Console.WindowHeight - 1)
            {
                System.Console.Write(BlankLeft);
                return;
            }
            System.Console.Write(Blank);
        }

        public void BlankToEnd()
        {
            for (var i = System.Console.CursorTop + 1; i < System.Console.WindowHeight; i++)
            {
                System.Console.SetCursorPosition(0, i);
                WriteBlank();
            }
        }

    }



}
