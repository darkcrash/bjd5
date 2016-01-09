using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Bjd.Browse;
using Bjd.log;
using Bjd.mail;
using Bjd.net;
using Bjd.option;
using Bjd.plugin;
using Bjd.remote;
using Bjd.server;
using Bjd.sock;
using Bjd.trace;
using Bjd.util;

namespace Bjd
{
    public class Kernel : IDisposable
    {

        //プロセス起動時に初期化される変数 
        public RunMode RunMode { get; set; } //通常起動;
        public RemoteConnect RemoteConnect { get; set; } //�����[�g����Őڑ�����Ă��鎞���������������
        public DnsCache DnsCache { get; private set; }
        public Ver Ver { get; private set; }
        public MailBox MailBox { get; private set; }

        //�T�[�o�N�����ɍŏ����������ϐ�
        public ListOption ListOption { get; private set; }
        public ListServer ListServer { get; private set; }
        public LogFile LogFile { get; private set; }
        private bool _isJp = true;
        private Logger _logger;

        //Ver5.9.6
        public WebApi WebApi { get; private set; }

        //Ver5.8.6
        public IniDb IniDb { get; private set; }


        public bool IsJp()
        {
            return _isJp;
        }

        public string ServerName
        {
            get
            {
                var oneOption = ListOption.Get("Basic");
                if (oneOption != null)
                {
                    return (String)oneOption.GetValue("serverName");
                }
                return "";
            }
        }

        //�e�X�g�p�R���X�g���N�^
        public Kernel()
        {
            Trace.WriteLine("Kernel..ctor Start");
            DefaultInitialize();
            Trace.WriteLine("Kernel..ctor End");
        }

        //�e�X�g�p�R���X�g���N�^(MailBox�̂ݏ�����)
        public Kernel(String option)
        {
            Trace.WriteLine("Kernel..ctor Start");
            DefaultInitialize();

            if (option.IndexOf("MailBox") != -1)
            {
                var op = ListOption.Get("MailBox");
                var conf = new Conf(op);
                var dir = ReplaceOptionEnv((String)conf.Get("dir"));
                var datUser = (Dat)conf.Get("user");
                MailBox = new MailBox(null, datUser, dir);
            }
            Trace.WriteLine("Kernel..ctor End");
        }


        //�N�����ɁA�R���X�g���N�^����Ăяo����鏉����
        private void DefaultInitialize()
        {
            Trace.WriteLine("Kernel.DefaultInitialize Start");

            RunMode = RunMode.Normal;
            RemoteConnect = null;//�����[�g����Őڑ�����Ă��鎞���������������

            //�v���Z�X�N�����ɏ����������
            DnsCache = new DnsCache();

            Ver = new Ver(); //�o�[�W�����Ǘ�

            //RunMode
            RunMode = RunMode.Service;

            IniDb = new IniDb(Define.ExecutableDirectory, (RunMode == RunMode.Remote) ? "$remote" : "Option");

            MailBox = null;

            ListInitialize(); //�T�[�o�ċN���ŁA�ēx���s����鏉���� 

            //�E�C���h�T�C�Y�̕���
            var path = string.Format("{0}\\BJD.ini", Define.ExecutableDirectory);

            switch (RunMode)
            {
                case RunMode.Normal:
                case RunMode.Service:
                    break;
                default:
                    Util.RuntimeException("Kernel.defaultInitialize() not implement (RunMode)");
                    break;
            }

            Trace.WriteLine("Kernel.DefaultInitialize End");
        }

        //�T�[�o�ċN���ŁA�ēx���s����鏉����
        public void ListInitialize()
        {
            Trace.WriteLine("Kernel.ListInitialize Start");
            //Logger���g�p�ł��Ȃ��Ԃ̃��O�́A������ɕۑ����āA���Logger�ɑ���
            var tmpLogger = new TmpLogger();

            //************************************************************
            // �j��
            //************************************************************
            if (ListOption != null)
            {
                ListOption.Dispose();
                ListOption = null;
            }
            if (ListServer != null)
            {
                ListServer.Dispose();
                ListServer = null;
            }
            if (MailBox != null)
            {
                MailBox = null;
            }
            if (LogFile != null)
            {
                LogFile.Dispose();
                LogFile = null;
            }

            //var listPlugin = new ListPlugin(Define.ExecutableDirectory);
            var listPlugin = new ListPlugin();
            foreach (var o in listPlugin)
            {
                //�����[�g�N���C�A���g�̏ꍇ�A���̃��O�́A��₱�����̂ŕ\�����Ȃ�
                if (RunMode == RunMode.Normal)
                {
                    tmpLogger.Set(LogKind.Detail, null, 9000008, string.Format("{0}Server", o.Name));
                }
            }

            _isJp = IniDb.IsJp();

            ListOption = new ListOption(this, listPlugin);

            //OptionBasic
            var confBasic = new Conf(ListOption.Get("Basic"));

            //OptionLog
            var confOption = new Conf(ListOption.Get("Log"));

            if (RunMode == RunMode.Normal || RunMode == RunMode.Service)
            {
                //LogFile�̏�����
                var saveDirectory = (String)confOption.Get("saveDirectory");
                saveDirectory = ReplaceOptionEnv(saveDirectory);
                var normalLogKind = (int)confOption.Get("normalLogKind");
                var secureLogKind = (int)confOption.Get("secureLogKind");
                var saveDays = (int)confOption.Get("saveDays");
                //Ver6.0.7
                var useLogFile = (bool)confOption.Get("useLogFile");
                var useLogClear = (bool)confOption.Get("useLogClear");
                if (!useLogClear)
                {
                    saveDays = 0; //���O�̎����폜�������ȏꍇ�AsaveDays��0��Z�b�g����
                }
                if (saveDirectory == "")
                {
                    tmpLogger.Set(LogKind.Error, null, 9000045, "It is not appointed");
                }
                else {
                    tmpLogger.Set(LogKind.Detail, null, 9000032, saveDirectory);
                    try
                    {
                        LogFile = new LogFile(saveDirectory, normalLogKind, secureLogKind, saveDays, useLogFile);
                    }
                    catch (IOException e)
                    {
                        LogFile = null;
                        tmpLogger.Set(LogKind.Error, null, 9000031, e.Message);
                    }
                }

                //Ver5.8.7 Java fix
                //mailBox������
                foreach (var o in ListOption)
                {
                    //SmtpServer�Ⴕ���́APop3Server���g�p�����ꍇ�̂݃��[���{�b�N�X�����������                
                    if (o.NameTag == "Smtp" || o.NameTag == "Pop3")
                    {
                        if (o.UseServer)
                        {
                            var conf = new Conf(ListOption.Get("MailBox"));
                            var dir = ReplaceOptionEnv((String)conf.Get("dir"));
                            var datUser = (Dat)conf.Get("user");
                            var logger = CreateLogger("MailBox", (bool)conf.Get("useDetailsLog"), null);
                            MailBox = new MailBox(logger, datUser, dir);
                            break;
                        }
                    }
                }

            }
            _logger = CreateLogger("kernel", true, null);
            tmpLogger.Release(_logger);

            ListServer = new ListServer(this, listPlugin);

            //ListTool = new ListTool();
            //ListTool.Initialize(this);

            WebApi = new WebApi();

            Trace.WriteLine("Kernel.ListInitialize End");
        }

        //Conf�̐���
        //���O��ListOption������������Ă���K�v������
        public Conf CreateConf(String nameTag)
        {
            Trace.WriteLine("Kernel.CreateConf Start");
            try
            {
                if (ListOption == null)
                {
                    Util.RuntimeException("createConf() ListOption==null");
                    return null;
                }
                var oneOption = ListOption.Get(nameTag);
                if (oneOption != null)
                {
                    return new Conf(oneOption);
                }
                return null;
            }
            finally
            {
                Trace.WriteLine("Kernel.CreateConf End");
            }
        }

        //Logger�̐���
        //���O��ListOption������������Ă���K�v������
        public Logger CreateLogger(String nameTag, bool useDetailsLog, ILogger logger)
        {
            Trace.WriteLine("Kernel.CreateLogger Start");
            try
            {
                if (ListOption == null)
                {
                    Util.RuntimeException("CreateLogger() ListOption==null || LogFile==null");
                }
                var conf = CreateConf("Log");
                if (conf == null)
                {
                    //CreateLogger��g�p����ۂɁAOptionLog�������ł��Ȃ��̂́A�݌v��̖�肪����
                    Util.RuntimeException("CreateLogger() conf==null");
                    return null;
                }
                var dat = (Dat)conf.Get("limitString");
                var isDisplay = ((int)conf.Get("isDisplay")) == 0;
                var logLimit = new LogLimit(dat, isDisplay);

                var useLimitString = (bool)conf.Get("useLimitString");
                return new Logger(this, logLimit, LogFile, _isJp, nameTag, useDetailsLog, useLimitString, logger);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Trace.WriteLine("Kernel.CreateLogger End");
            }
        }

        //�I������
        public void Dispose()
        {
            Trace.WriteLine("Kernel.Dispose Start");
            try
            {

                //Ver5.8.6 Java fix 
                if (RunMode == RunMode.Normal)
                {
                    var iniTmp = new IniDb(Define.ExecutableDirectory, "$tmp");//�o�b�N�A�b�v��쐬����ini�t�@�C����폜����
                                                                               //��U�A�ʃt�@�C���Ɍ��ݗL���Ȃ�̂���������߂�
                    ListOption.Save(iniTmp);
                    //�㏑������
                    File.Copy(iniTmp.Path, IniDb.Path, true);
                    iniTmp.Delete();
                }
                else if (RunMode == RunMode.Remote)
                {
                    IniDb.Delete(); //$Remote.ini�̍폜
                }

                //**********************************************
                // �j��
                //**********************************************
                ListServer.Dispose(); //�e�T�[�o�͒�~�����
                ListOption.Dispose();
                MailBox = null;

            }
            finally
            {
                Trace.WriteLine("Kernel.Dispose End");
            }
        }

        //�I�v�V�����Ŏw�肳���ϐ���u���ς���

        public String ReplaceOptionEnv(String str)
        {
            var executablePath = Define.ExecutableDirectory;
            executablePath = executablePath.Replace("\\\\", "\\\\\\\\");
            str = str.Replace("%ExecutablePath%", executablePath);
            return str;
        }

        internal void Start()
        {
            if (ListServer.Count == 0)
            {
                _logger.Set(LogKind.Error, null, 9000030, "");
            }
            else {
                ListServer.Start();
            }
        }

        internal void Stop()
        {
            ListServer.Stop();
        }

        //�����[�g����(�f�[�^�̎擾)
        public string Cmd(string cmdStr)
        {
            var sb = new StringBuilder();


            sb.Append(IsJp() ? "(1) �T�[�r�X���" : "(1) Service Status");
            sb.Append("\b");

            foreach (var sv in ListServer)
            {
                sb.Append("  " + sv);
                sb.Append("\b");
            }
            sb.Append(" \b");

            sb.Append(IsJp() ? "(2) ���[�J���A�h���X" : "(2) Local address");
            sb.Append("\b");
            foreach (string addr in Define.ServerAddressList())
            {
                sb.Append(string.Format("  {0}", addr));
                sb.Append("\b");
            }

            return sb.ToString();
        }

        public String ChangeTag(String src)
        {
            var tagList = new[] { "$h", "$v", "$p", "$d", "$a", "$s" };

            foreach (var tag in tagList)
            {
                while (true)
                {
                    var index = src.IndexOf(tag);
                    if (index == -1)
                    {
                        break;
                    }
                    var tmp1 = src.Substring(0, index);
                    var tmp2 = "";
                    switch (tag)
                    {
                        case "$h":
                            var serverName = ServerName;
                            tmp2 = serverName == "" ? "localhost" : serverName;
                            break;
                        case "$v":
                            tmp2 = Ver.Version();
                            break;
                        case "$p":
                            tmp2 = Define.ApplicationName();
                            break;
                        case "$d":
                            tmp2 = Define.Date();
                            break;
                        case "$a":
                            var localAddress = LocalAddress.GetInstance();
                            tmp2 = localAddress.RemoteStr();
                            //tmp2 = Define.ServerAddress();
                            break;
                        case "$s":
                            tmp2 = ServerName;
                            break;
                        default:
                            Util.RuntimeException(string.Format("undefind tag = {0}", tag));
                            break;
                    }
                    var tmp3 = src.Substring(index + 2);
                    src = tmp1 + tmp2 + tmp3;
                }
            }
            return src;
        }

        //IP�A�h���X�̈ꗗ�擾
        public List<Ip> GetIpList(String hostName)
        {
            var ar = new List<Ip>();
            try
            {
                var ip = new Ip(hostName);
                ar.Add(ip);
            }
            catch (ValidObjException)
            {
                ar = DnsCache.GetAddress(hostName).ToList();
            }
            return ar;
        }

        public string GetBrowseInfo(string path)
        {
            var sb = new StringBuilder();
            try
            {
                if (path == "")
                {
                    path = "\\";
                }

                {
                    string[] dirs = Directory.GetDirectories(path);
                    Array.Sort(dirs);
                    foreach (string s in dirs)
                    {
                        var name = s.Substring(path.Length);
                        var info = new DirectoryInfo(s);
                        const long size = 0;
                        var dt = info.LastWriteTime;
                        var p = new OneBrowse(BrowseKind.Dir, name, size, dt); //�P�f�[�^����
                        sb.Append(p + "\t"); //���M�����񐶐�
                    }
                    var files = Directory.GetFiles(path);
                    Array.Sort(files);
                    foreach (var s in files)
                    {
                        var name = s.Substring(path.Length);
                        var info = new FileInfo(s);
                        var size = info.Length;
                        var dt = info.LastWriteTime;
                        var p = new OneBrowse(BrowseKind.File, name, size, dt); //�P�f�[�^����
                        sb.Append(p + "\t"); //���M�����񐶐�
                    }
                }
            }
            catch
            {
                sb.Length = 0;
            }
            return sb.ToString();
        }

    }
}


