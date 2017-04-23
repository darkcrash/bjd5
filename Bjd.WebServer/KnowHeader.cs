using System;
using System.Text;

namespace Bjd
{
    public class KnowHeader : OneHeader
    {
        public static readonly KnowHeader Empty = new KnowHeader("", "");

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

        private KnowHeader(string key, string upperKey)
             : base(key, "")
        {
            _Key = key;
            _KeyUpper = upperKey;
            _Val = new byte[0];
            _ValString = null;
        }

    }
}