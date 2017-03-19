using System;
using System.Text;

namespace Bjd
{
    public class OneHeader
    {
        public string Key { get; set; }
        public string KeyUpper { get; set; }
        public byte[] Val { get; set; }
        public string ValString { get; set; }
        public OneHeader(string key, byte[] val)
        {
            Key = key;
            KeyUpper = key.ToUpper();
            Val = val;
            ValString = Encoding.ASCII.GetString(val);
        }

    }
}