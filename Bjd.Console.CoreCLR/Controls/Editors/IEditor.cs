using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.Console.Controls.Editors
{
    public interface IEditor 
    {
        int Width { get; }
        int Row { get; }


        bool Input(ConsoleKeyInfo key);

        void Output(ConsoleContext context);

    }

}
