using System;
using System.Text;

namespace Bjd
{
    public class KnowHeader : OneHeader
    {

        public KnowHeader(string key, string upperKey, byte[] val)
            : base(key, val)
        {
            _Key = key;
            _KeyUpper = upperKey;
            Val = val;
        }

        public KnowHeader(string key, string upperKey, string val)
             : base(key, val)
        {
            _Key = key;
            _KeyUpper = upperKey;
            ValString = val;
        }

    }
}