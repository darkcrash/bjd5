using Bjd.Memory;
using System;
using System.Text;

namespace Bjd
{
    public interface IHeader : IDisposable
    {
        bool Enabled { get; set; }
        string Key { get; }
        string KeyUpper { get; }

        BufferData Val { get; set; }
        string ValString { get; set; }

    }
}