﻿using Bjd.Memory;
using System;
using System.Text;

namespace Bjd
{
    public class KnowHeader : IHeader
    {
        public static readonly KnowHeader Empty = new KnowHeader("", "");

        public bool Enabled { get; set; } = false;

        private string _Key;
        public string Key => _Key;

        private string _KeyUpper;
        public string KeyUpper => _KeyUpper;

        protected BufferData _Val;
        protected string _ValString;

        public BufferData Val
        {
            get => _Val;
            set
            {
                if (value == null)
                {
                    _ValString = null;
                    _Val = null;
                    return;
                }
                _Val = value;
                //_ValString = Encoding.ASCII.GetString(_Val);
                _ValString = _Val.ToAsciiString();
            }
        }

        public string ValString
        {
            get => _ValString;
            set
            {
                if (value == null)
                {
                    _ValString = null;
                    _Val = null;
                    return;
                }
                _ValString = value;
                //_Val = Encoding.ASCII.GetBytes(_ValString);
                _Val?.Dispose();
                _Val = _ValString.ToAsciiBufferData();
            }
        }

        public KnowHeader(string key, string upperKey, BufferData val)
        {
            _Key = key;
            _KeyUpper = upperKey;
            Val = val;
        }

        public KnowHeader(string key, string upperKey, string val)
        {
            _Key = key;
            _KeyUpper = upperKey;
            ValString = val;
        }

        private KnowHeader(string key, string upperKey)
        {
            _Key = key;
            _KeyUpper = upperKey;
            _Val = BufferData.Empty;
            _ValString = null;
        }

        ~KnowHeader()
        {
            _Val?.Dispose();
            _Val = null;
        }

        public void Clear()
        {
            _ValString = null;
            _Val?.Dispose();
            _Val = null;
            Enabled = false;
        }

        public void Dispose()
        {
            _Val?.Dispose();
            _Val = null;
        }


    }
}