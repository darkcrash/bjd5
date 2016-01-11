using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bjd.acl;
using Bjd.ctrl;
using Bjd.log;
using Bjd.net;
using Bjd.option;
using Bjd.sock;
using Bjd.util;

namespace Bjd.server
{

    //OneServer �P�̃o�C���h�A�h���X�F�|�[�g���ƂɃT�[�o��\������N���X<br>
    //�e�T�[�o�I�u�W�F�N�g�̊��N���X<br>
    public abstract class OneServer : ThreadBase
    {

        protected Conf Conf;
        public Logger Logger;
        protected bool IsJp;
        protected int Timeout;//sec
        SockServerTcp _sockServerTcp;
        SockServerUdp _sockServerUdp;
        readonly OneBind _oneBind;
        //Ver5.9.2 Java fix
        protected Ssl ssl = null;

        public String NameTag { get; private set; }
        protected Kernel Kernel; //SockObj��Trace�̂���
        protected AclList AclList = null;

        //�q�X���b�h�Ǘ�
        readonly object SyncObj = new object(); //�r������I�u�W�F�N�g
        readonly List<Task> _childThreads = new List<Task>();
        readonly int _multiple; //�����ڑ���

        //�X�e�[�^�X�\���p
        public override String ToString()
        {
            var stat = IsJp ? "+ �T�[�r�X�� " : "+ In execution ";
            if (ThreadBaseKind != ThreadBaseKind.Running)
            {
                stat = IsJp ? "- ��~ " : "- Initialization failure ";
            }
            return string.Format("{0}\t{1,20}\t[{2}\t:{3} {4}]\tThread {5}/{6}", stat, NameTag, _oneBind.Addr, _oneBind.Protocol.ToString().ToUpper(), (int)Conf.Get("port"), Count(), _multiple);
        }



        public int Count()
        {
            return _childThreads.Count;
        }

        //�����[�g����(�f�[�^�̎擾)
        public String cmd(String cmdStr)
        {
            return "";
        }

        public SockState SockState
        {
            get
            {
                if (_sockServerTcp != null)
                    return _sockServerTcp.SockState;
                if (_sockServerUdp != null)
                    return _sockServerUdp.SockState;
                return sock.SockState.Error;
            }
        }

        //Ver6.1.6
        protected readonly Lang Lang;

        //�R���X�g���N�^
        protected OneServer(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel.CreateLogger(conf.NameTag, true, null))
        {
            Kernel = kernel;
            NameTag = conf.NameTag;
            Conf = conf;
            _oneBind = oneBind;
            IsJp = kernel.IsJp();
            kernel.CancelToken.Register(() => this.StopLife());

            //Ver6.1.6
            Lang = new Lang(IsJp ? LangKind.Jp : LangKind.En, "Server" + conf.NameTag);
            CheckLang();//��`�̃e�X�g

            //�e�X�g�p
            if (Conf == null)
            {
                var optionSample = new OptionSample(kernel, "");
                Conf = new Conf(optionSample);
                Conf.Set("port", 9990);
                Conf.Set("multiple", 10);
                Conf.Set("acl", new Dat(new CtrlType[0]));
                Conf.Set("enableAcl", 1);
                Conf.Set("timeOut", 3);
            }
            //�e�X�g�p
            if (_oneBind == null)
            {
                var ip = new Ip(IpKind.V4Localhost);
                _oneBind = new OneBind(ip, ProtocolKind.Tcp);
            }

            Logger = kernel.CreateLogger(conf.NameTag, (bool)Conf.Get("useDetailsLog"), this);
            _multiple = (int)Conf.Get("multiple");

            //DHCP�ɂ�ACL�����݂��Ȃ�
            if (NameTag != "Dhcp")
            {
                //ACL���X�g ��`�������ꍇ�́AaclList�𐶐����Ȃ�
                var acl = (Dat)Conf.Get("acl");
                AclList = new AclList(acl, (int)Conf.Get("enableAcl"), Logger);
            }
            Timeout = (int)Conf.Get("timeOut");
        }



        public new void Start()
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.Start ");

            base.Start();
            //Ver5.9.8
            if (!IsLife())
            {
                return;
            }

            //bind����������܂őҋ@����
            while ((_sockServerTcp == null && _sockServerUdp == null) || this.SockState == sock.SockState.Idle)
            {
                Thread.Sleep(100);
            }
        }


        public new void Stop()
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.Stop ");
            if (_sockServerTcp == null)
            {
                return; //���łɏI���������I����Ă���
            }
            base.Stop(); //life=false �ł��ׂẴ��[�v��������
            _sockServerTcp.Close();

            // �S���̎q�X���b�h���I������̂�҂�
            while (Count() > 0)
            {
                Thread.Sleep(500);
            }
            _sockServerTcp = null;

        }

        public new void Dispose()
        {
            // super.dispose()�́AThreadBase��stop()���Ă΂�邾���Ȃ̂ŕK�v�Ȃ�
            Stop();
        }

        //�X���b�h��~����
        protected abstract void OnStopServer(); //�X���b�h��~����

        protected override void OnStopThread()
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.OnStopThread {this.GetType().FullName} ");
            OnStopServer(); //�q�N���X�̃X���b�h��~����
            if (ssl != null)
            {
                ssl.Dispose();
            }
        }

        //�X���b�h�J�n����
        //�T�[�o������ɋN���ł���ꍇ(isInitSuccess==true)�̂݃X���b�h�J�n�ł���
        protected abstract bool OnStartServer(); //�X���b�h�J�n����

        protected override bool OnStartThread()
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.OnStartThread {this.GetType().FullName}");
            return OnStartServer(); //�q�N���X�̃X���b�h�J�n����
        }

        protected override void OnRunThread()
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.OnRunThread {this.GetType().FullName}");

            var port = (int)Conf.Get("port");
            var bindStr = string.Format("{0}:{1} {2}", _oneBind.Addr, port, _oneBind.Protocol);

            Logger.Set(LogKind.Normal, null, 9000000, bindStr);

            //DOS��󂯂��ꍇ�Amultiple���܂ŘA���A�N�Z�X�܂ł͋L�����Ă��܂�
            //DOS���I��������A���̕��������A�Ɏ��Ԃ�v����

            //Ver5.9,2 Java fix
            //_sockServer = new SockServer(this.Kernel,_oneBind.Protocol);
            switch (_oneBind.Protocol)
            {
                case ProtocolKind.Tcp:
                    _sockServerTcp = new SockServerTcp(Kernel, _oneBind.Protocol, ssl);
                    if (ssl != null && !ssl.Status)
                    {
                        Logger.Set(LogKind.Error, null, 9000024, bindStr);
                        //[C#]
                        ThreadBaseKind = ThreadBaseKind.Running;
                    }
                    else if (this.SockState != sock.SockState.Error)
                    {
                        RunTcpServer(port);
                    }
                    _sockServerTcp.Close();
                    break;
                case ProtocolKind.Udp:
                    _sockServerUdp = new SockServerUdp(Kernel, _oneBind.Protocol, ssl);
                    if (this.SockState != sock.SockState.Error)
                    {
                        RunUdpServer(port);
                    }
                    _sockServerUdp.Close();
                    break;
            }

            //Java fix
            Logger.Set(LogKind.Normal, null, 9000001, bindStr);

        }

        private void RunTcpServer(int port)
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.RunTcpServer {this.GetType().FullName}");

            //[C#]
            ThreadBaseKind = ThreadBaseKind.Running;
            const int listenMax = 10;

            if (!_sockServerTcp.Bind(_oneBind.Addr, port, listenMax))
            {
                Logger.Set(LogKind.Error, _sockServerTcp, 9000006, _sockServerTcp.GetLastEror());
                return;
            }

            while (IsLife())
            {
                var child = _sockServerTcp.Select(this);
                if (child == null)
                {
                    break;
                }
                if (Count() >= _multiple)
                {
                    Logger.Set(LogKind.Secure, _sockServerTcp, 9000004, string.Format("count:{0}/multiple:{1}", Count(), _multiple));
                    //�����ڑ����𒴂����̂Ń��N�G�X�g��L�����Z�����܂�
                    child.Close();
                    continue;
                }

                // ACL�����̃`�F�b�N
                if (AclCheck(child) == AclKind.Deny)
                {
                    child.Close();
                    child.Dispose();
                    continue;
                }
                var t = new Task(() => this.SubThread(child));
                t.ContinueWith(this.RemoveTask);
                this.AddTask(t);
                t.Start();
            }

        }

        private void RunUdpServer(int port)
        {
            System.Diagnostics.Trace.TraceInformation($"OneServer.RunUdpServer {this.GetType().FullName}");

            //[C#]
            ThreadBaseKind = ThreadBaseKind.Running;

            if (!_sockServerUdp.Bind(_oneBind.Addr, port))
            {
                Logger.Set(LogKind.Error, _sockServerUdp, 9000006, _sockServerUdp.GetLastEror());
                //println(string.Format("bind()=false %s", sockServer.getLastEror()));
                return;
            }

            while (IsLife())
            {
                var child = _sockServerUdp.Select(this);
                if (child == null)
                {
                    //Select�ŗ�O�����������ꍇ�́A���̃R�l�N�V������̂ĂāA���̑҂��󂯂ɓ���
                    continue;
                }
                if (Count() >= _multiple)
                {
                    Logger.Set(LogKind.Secure, _sockServerUdp, 9000004, string.Format("count:{0}/multiple:{1}", Count(), _multiple));
                    //�����ڑ����𒴂����̂Ń��N�G�X�g��L�����Z�����܂�
                    child.Close();
                    continue;
                }

                // ACL�����̃`�F�b�N
                if (AclCheck(child) == AclKind.Deny)
                {
                    child.Close();
                    continue;
                }
                var t = new Task(() => this.SubThread(child));
                t.ContinueWith(this.RemoveTask);
                this.AddTask(t);
                t.Start();
            }

        }
        private void RemoveTask(Task t)
        {
            lock (SyncObj)
            {
                _childThreads.Remove(t);
            }
        }
        private void AddTask(Task t)
        {
            lock (SyncObj)
            {
                _childThreads.Add(t);
            }
        }

        //ACL�����̃`�F�b�N
        //sockObj �����Ώۂ̃\�P�b�g
        private AclKind AclCheck(SockObj sockObj)
        {
            var aclKind = AclKind.Allow;
            if (AclList != null)
            {
                var ip = new Ip(sockObj.RemoteAddress.Address.ToString());
                aclKind = AclList.Check(ip);
            }

            if (aclKind == AclKind.Deny)
            {
                _denyAddress = sockObj.RemoteAddress.ToString();
            }
            return aclKind;
        }

        protected abstract void OnSubThread(SockObj sockObj);

        private String _denyAddress = ""; //Ver5.3.5 DoS�Ώ�

        //�P���N�G�X�g�ɑ΂���q�X���b�h�Ƃ��ċN�������
        public void SubThread(SockObj o)
        {
            var sockObj = (SockObj)o;

            //�N���C�A���g�̃z�X�g����t��������
            sockObj.Resolve((bool)Conf.Get("useResolve"), Logger);

            //_subThread�̒���SockObj�͔j������i������UDP�̏ꍇ�́A�N���[���Ȃ̂�Close()���Ă�socket�͔j������Ȃ��j
            Logger.Set(LogKind.Detail, sockObj, 9000002, string.Format("count={0} Local={1} Remote={2}", Count(), sockObj.LocalAddress, sockObj.RemoteAddress));

            //Ver5.8.9 Java fix �ڑ��P�ʂ̂��ׂĂ̗�O��L���b�`���ăv���O�����̒�~������
            //OnSubThread(sockObj); //�ڑ��P�ʂ̏���
            try
            {
                OnSubThread(sockObj); //�ڑ��P�ʂ̏���
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Fail(ex.Message);
                System.Diagnostics.Trace.Fail(ex.StackTrace);
                if (Logger != null)
                {
                    Logger.Set(LogKind.Error, null, 9000061, ex.Message);
                    Logger.Exception(ex, null, 2);
                }
            }
            finally
            {
                sockObj.Close();
                Logger.Set(LogKind.Detail, sockObj, 9000003, string.Format("count={0} Local={1} Remote={2}", Count(), sockObj.LocalAddress, sockObj.RemoteAddress));
                sockObj.Dispose();
            }

        }

        //Java Fix
        //RemoteServer�ł̂ݎg�p�����
        public abstract void Append(OneLog oneLog);

        //1�s�Ǎ��ҋ@
        public Cmd WaitLine(SockTcp sockTcp)
        {
            var tout = new util.Timeout(Timeout);

            while (IsLife())
            {
                Cmd cmd = recvCmd(sockTcp);
                if (cmd == null)
                {
                    return null;
                }
                if (cmd.CmdStr != "")
                {
                    return cmd;
                }
                if (tout.IsFinish())
                {
                    return null;
                }
                Thread.Sleep(100);
            }
            return null;
        }

        //TODO RecvCmd�̃p�����[�^�`����ύX���邪�A����́A��قǁAWeb,Ftp,Smtp��Server�Ŏg�p����Ă��邽�߉e�����ł�\��
        //�R�}���h�擾
        //�R�l�N�V�����ؒf�ȂǃG���[��������������null���Ԃ����
        protected Cmd recvCmd(SockTcp sockTcp)
        {
            if (sockTcp.SockState != sock.SockState.Connect)
            {
                //�ؒf����Ă���
                return null;
            }
            var recvbuf = sockTcp.LineRecv(Timeout, this);
            //�ؒf���ꂽ�ꍇ
            if (recvbuf == null)
            {
                return null;
            }

            //��M�ҋ@���̏ꍇ
            if (recvbuf.Length == 0)
            {

                //Ver5.8.5 Java fix
                //return new Cmd("", "", "");
                return new Cmd("waiting", "", ""); //�ҋ@���̏ꍇ�A���̂��Ƃ�������悤��"waiting"��Ԃ�
            }

            //CRLF�̔r��
            recvbuf = Inet.TrimCrlf(recvbuf);

            //String str = new String(recvbuf, Charset.forName("Shift-JIS"));
            //var str = Encoding.GetEncoding("Shift-JIS").GetString(recvbuf);
            var str = Encoding.GetEncoding("utf-8").GetString(recvbuf);
            if (str == "")
            {
                return new Cmd("", "", "");
            }
            //��M�s��R�}���h�ƃp�����[�^�ɕ������i�R�}���h�ƃp�����[�^�͂P�ȏ�̃X�y�[�X�ŋ�؂��Ă���j
            String cmdStr = null;
            String paramStr = null;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                {
                    if (cmdStr == null)
                    {
                        cmdStr = str.Substring(0, i);
                    }
                }
                if (cmdStr == null || str[i] == ' ')
                {
                    continue;
                }
                paramStr = str.Substring(i);
                break;
            }
            if (cmdStr == null)
            {
                //�p�����[�^��؂肪������Ȃ������ꍇ
                cmdStr = str; //�S���R�}���h
            }
            return new Cmd(str, cmdStr, paramStr);
        }

        //������
        //        public void Append(OneLog oneLog){
        //            Util.RuntimeException("OneServer.Append(OneLog) ������");
        //        }

        //�����[�g����(�f�[�^�̎擾)
        public virtual String Cmd(String cmdStr)
        {
            return "";
        }

        /********************************************************/
        //�ڐA�̂��߂̎b�菈�u(POP3�ł̂ݎg�p����Ă���)
        /********************************************************/
        protected bool RecvCmd(SockTcp sockTcp, ref string str, ref string cmdStr, ref string paramStr)
        {

            var cmd = recvCmd(sockTcp);
            if (cmd == null)
            {
                return false;
            }
            cmdStr = cmd.CmdStr;
            paramStr = cmd.ParamStr;
            str = cmd.Str;
            return true;
        }

        public bool WaitLine(SockTcp sockTcp, ref string cmdStr, ref string paramStr)
        {
            var cmd = WaitLine(sockTcp);
            if (cmd == null)
            {
                return false;
            }
            cmdStr = cmd.CmdStr;
            paramStr = cmd.ParamStr;
            return true;
        }

        //Ver6.1.6
        // string GetMsg(int messageNo)�̊e���b�Z�[�W��BJD.Lang.txt�ɒ�`����Ă��邩�ǂ����̊m�F
        protected abstract void CheckLang();
    }
}

