﻿using System.Collections.Generic;
using System;
using System.IO;
using Bjd.Option;
using Bjd.Ctrl;
using Bjd;

namespace Bjd.SmtpServer
{
    class MlOption {
        public int MaxSummary { get; private set; }
        public int MaxGet { get; private set; }
        public bool AutoRegistration { get; private set; }
        public string ManageDir { get; private set; }
        public List<string> Docs { get; private set; }
        public int TitleKind { get; private set; }
        public Dat MemberList { get; private set; }

        public MlOption(Kernel kernel,OneOption op){
            var maxSummary = (int)op.GetValue("maxSummary");
            var maxGet = (int)op.GetValue("maxGet");
            var autoRegistration = (bool)op.GetValue("autoRegistration");//自動登録
            var titleKind = (int)op.GetValue("title");
            var docs = new List<string>();
            foreach (MlDocKind docKind in Enum.GetValues(typeof(MlDocKind))) {
                var buf = (string)op.GetValue(docKind.ToString().ToLower() + "Document");
                if (buf.Length < 2 || buf[buf.Length - 2] != '\r' || buf[buf.Length - 1] != '\n'){
                    buf = buf + "\r\n";
                }
                docs.Add(buf);
            }
            var manageDir = (string)op.GetValue("manageDir");
            //Ver6.0.1
            manageDir = kernel.ReplaceOptionEnv(manageDir);
            //Ver6.0.0で間違えたフォルダの修復
            //if (Directory.Exists("%ExecutablePath%") && Directory.Exists("%ExecutablePath%\\ml")) {
            if (Directory.Exists("%ExecutablePath%") && Directory.Exists($"%ExecutablePath%{Path.DirectorySeparatorChar}ml")) {
                try{
                    var path = Path.GetFullPath("%ExecutablePath%");
                    var dir = Path.GetDirectoryName(path);
                    if (dir != null){
                        Directory.Move(path + $"{Path.DirectorySeparatorChar}ml", dir + $"{Path.DirectorySeparatorChar}ml");
                        Directory.Delete(path);
                    }
                } catch (Exception){
                    ;

                }
            }


            var memberList = (Dat)op.GetValue("memberList");
            if (memberList == null){
                memberList = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.TextBox });
            }
            

            Init(maxSummary, maxGet, autoRegistration, titleKind, docs, manageDir, memberList);
        }

        //テスト用コンストラクタ
        public MlOption(int maxSummary, int maxGet, bool autoRegistration,int titleKind,List<string> docs,string manageDir,Dat memberList){
            Init(maxSummary, maxGet, autoRegistration, titleKind, docs, manageDir, memberList);
        }

        void Init(int maxSummary, int maxGet, bool autoRegistration, int titleKind, List<string> docs, string manageDir, Dat memberList){
            MaxSummary = maxSummary;
            MaxGet = maxGet;
            AutoRegistration = autoRegistration;//自動登録
            TitleKind = titleKind;
            Docs = docs;
            ManageDir = manageDir;
            MemberList = memberList;
        }

    }
}