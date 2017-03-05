using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Configurations;
using System.Collections.Generic;

namespace Bjd.Console.Controls.Editors
{
    public interface IEditor 
    {
        int Width { get; }
        int Row { get; }

        object EditValue { get; set; }

        bool Input(ConsoleKeyInfo key);

        void Output(ConsoleContext context);

    }

}
