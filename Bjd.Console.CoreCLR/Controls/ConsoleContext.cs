using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Console.Controls
{
    public class ConsoleContext : IDisposable
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

        public int InitialCursorTop { get { return _initialCursorTop; } }
        private int _initialCursorTop = 0;
        public int VisibleRows = 0;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int MaxHeight { get; private set; }
        public string Blank { get; private set; }
        public string BlankLeft { get; private set; }

        public ConsoleContext(System.Threading.CancellationToken token)
        {
            _initialCursorTop = System.Console.CursorTop;
            System.Console.WindowTop = _initialCursorTop;

            System.Console.CursorVisible = false;
            WindowStateChanged();

            token.Register(CancelAction);

        }

        public void Dispose()
        {
            System.Console.CursorTop = _initialCursorTop + System.Console.WindowHeight;
            System.Console.WindowTop = _initialCursorTop + System.Console.WindowHeight;
        }

        private void CancelAction()
        {
            System.Console.CursorVisible = true;
        }

        public bool WindowStateChanged()
        {
            if (Width == System.Console.WindowWidth && Height == System.Console.WindowHeight) return false;
            Width = System.Console.WindowWidth;
            Height = System.Console.WindowHeight;
            MaxHeight = Height - 1;
            Blank = new string(' ', Width);
            BlankLeft = new string(' ', Width - 1);
            System.Console.CursorVisible = false;
            System.Console.WindowTop = _initialCursorTop;
            return true;
        }

        public void SetTop(Control ctrl)
        {
            System.Console.SetCursorPosition(0, ctrl.Top + _initialCursorTop);
            ctrl.VisibleRow = Height - ctrl.Top;
            ctrl.Column = Width;
        }

        public void Write(string message)
        {
            if (System.Console.WindowWidth < message.Length) message = message.Remove(System.Console.WindowWidth);
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
            var stPos = System.Console.CursorTop + _initialCursorTop;
            var edPos = System.Console.WindowHeight + _initialCursorTop - 1;
            if (stPos == edPos)
            {
                System.Console.Write(BlankLeft);
                return;
            }
            var left = System.Console.CursorLeft;
            if (left > 0)
            {
                var width = System.Console.WindowWidth;
                var rightSize = width - left;
                var bl = new string(' ', rightSize);
                System.Console.Write(bl);
                return;
            }
            System.Console.Write(Blank);
        }

        public void BlankToEnd()
        {
            var stPos = System.Console.CursorTop;
            var edPos = System.Console.WindowHeight + _initialCursorTop;
            for (var i = stPos; i < edPos; i++)
            {
                System.Console.SetCursorPosition(0, i);
                if (i == edPos - 1)
                {
                    System.Console.Write(BlankLeft);
                }
                else
                {
                    System.Console.Write(Blank);
                }
            }
        }

    }



}
