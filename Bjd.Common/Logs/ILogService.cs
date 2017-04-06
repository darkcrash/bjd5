﻿using Bjd.Memory;
using System;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public interface ILogService : IDisposable
    {
        void Append(LogMessage oneLog);

        void WriteLine(CharsData message);

        void TraceInformation(CharsData message);


        void TraceWarning(CharsData message);

        void TraceError(CharsData message);

    }
}
