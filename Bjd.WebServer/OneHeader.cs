﻿using System;
using System.Text;

namespace Bjd
{
    public class OneHeader
    {
        public bool Enabled = true;
        protected string _Key;
        protected string _KeyUpper;
        protected byte[] _Val;
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

        public byte[] Val
        {
            get { return _Val; }
            set
            {
                _Val = value;
                ValString = Encoding.ASCII.GetString(_Val);
            }
        }
        public string ValString
        {
            get { return _ValString; }
            set
            {
                _ValString = value;
                _Val = Encoding.ASCII.GetBytes(_ValString);
            }
        }
        public OneHeader(string key, byte[] val)
        {
            Key = key;
            Val = val;
        }

        public OneHeader(string key, string val)
        {
            Key = key;
            ValString = val;
        }

    }
}