﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Bjd
{
    public class Define
    {

        private Define() { }//�f�t�H���g�R���X�g���N�^�̉B��

        static string _executablePath;
        static string _executableDirectory;
        static string _productVersion;

        static Define()
        {
            //_executablePath = Application.ExecutablePath;
            var prc = System.Diagnostics.Process.GetCurrentProcess();
            //_executablePath = prc.StartInfo.FileName;
            var dir = System.IO.Directory.GetCurrentDirectory();
            _executableDirectory = dir;
            _executablePath = System.IO.Path.Combine(dir, "BJD.CoreCLR");

            //_productVersion = Application.ProductVersion;
            var asm = typeof(Define).GetTypeInfo().Assembly.GetName();
            _productVersion = asm.Version.ToString();


        }

        //Test�p
        public static void SetEnv(string path, string ver)
        {
            _executablePath = path;
            _productVersion = ver;
        }

        public static string Copyright()
        {
            return "Copyright(c) 1998/05.. by SIN/SapporoWorks";
        }
        public static string HostName()
        {
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
            return _localName;
        }
        public static string ApplicationName()
        {
            return "BlackJumboDog";
        }
        public static string ExecutablePath
        {
            get
            {
                return _executablePath;
            }
        }
        public static string ExecutableDirectory
        {
            get
            {
                return _executableDirectory;
            }
        }
        public static string ProductVersion
        {
            get
            {
                return _productVersion;
            }
        }

        public static string Date()
        {
            DateTime dt = DateTime.Now;
            //return dt.ToShortDateString() + " " + dt.ToLongTimeString();
            var formatDate = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            var formatTime = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            return dt.ToString(formatDate) + " " + dt.ToString(formatTime);
        }
        public static string ServerAddress()
        {
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
            if (_localAddress.Count > 0)
                return _localAddress[0];
            return "127.0.0.1";
        }
        public static List<string> ServerAddressList()
        {
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
            return _localAddress;
        }
        public static string WebHome()
        {
            return "http://www.sapporoworks.ne.jp/spw/";
        }
        public static string WebDocument()
        {
            return "http://www.sapporoworks.ne.jp/spw/?page_id=517";
        }
        public static string WebSupport()
        {
            return "http://www.sapporoworks.ne.jp/sbbs/sbbs.cgi?book=bjd";
        }


        static List<string> _localAddress;//�A�h���X
        static string _localName;//�z�X�g��
        static void InitLocalInformation()
        {
            if (_localAddress == null)
            {//�v���O�����N�����珉�߂ČĂяo���ꂽ�Ƃ��A�P�x�������s�����
                _localAddress = new List<string>();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nic in nics)
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                        continue;
                    IPInterfaceProperties props = nic.GetIPProperties();
                    foreach (UnicastIPAddressInformation info in props.UnicastAddresses)
                    {
                        if (info.Address.AddressFamily == AddressFamily.InterNetwork)
                            _localAddress.Add(info.Address.ToString());
                    }
                }

                _localName = Dns.GetHostName();
            }
        }
    }
}
