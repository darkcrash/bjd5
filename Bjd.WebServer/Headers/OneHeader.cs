using Bjd.Memory;
using System;
using System.Text;

namespace Bjd
{
    public class OneHeader : IHeader
    {
        public bool Enabled { get; set; } = true;
        protected string _Key;
        protected string _KeyUpper;
        //protected byte[] _Val;
        protected BufferData _Val;
        protected string _ValString;
        public string Key
        {
            get { return _Key; }
            set
            {
                _Key = value;
                _KeyUpper = _Key.ToUpper();
            }
        }

        public string KeyUpper
        {
            get { return _KeyUpper; }
        }

        public BufferData Val
        {
            get { return _Val; }
            set
            {
                _Val?.Dispose();
                _Val = value;
                //_ValString = Encoding.ASCII.GetString(_Val);
                _ValString = _Val.ToAsciiString();
            }
        }
        public string ValString
        {
            get { return _ValString; }
            set
            {
                _ValString = value;
                //_Val = Encoding.ASCII.GetBytes(_ValString);
                _Val?.Dispose();
                _Val = _ValString.ToAsciiBufferData();
            }
        }
        public OneHeader(string key, BufferData val)
        {
            Key = key;
            Val = val;
        }

        public OneHeader(string key, string keyUpper, BufferData val)
        {
            _Key = key;
            _KeyUpper = keyUpper;
            Val = val;
        }


        public OneHeader(string key, string val)
        {
            Key = key;
            ValString = val;
        }

        ~OneHeader()
        {
            _Val?.Dispose();
            _Val = null;
        }

        public void Dispose()
        {
            _Val?.Dispose();
            _Val = null;
        }

    }
}