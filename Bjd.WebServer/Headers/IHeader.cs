using System;
using System.Text;

namespace Bjd
{
    public interface IHeader
    {
        bool Enabled { get; set; }
        string Key { get; }
        string KeyUpper { get; }

        byte[] Val { get; set; }
        string ValString { get; set; }

    }
}