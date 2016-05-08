﻿using System;
using System.Collections.Generic;
using Bjd;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Utils;

namespace Bjd.SmtpServer
{
    internal class MailSave
    {
        readonly MailBox _mailBox;
        private readonly LocalBox _localBox;
        readonly MailQueue _mailQueue;
        private Alias _alias;
        readonly Logger _logger;
        readonly List<string> _domainList;
        int _idCounter;//id作成のための順次番号生成カウンタ
        readonly ReceivedHeader _receivedHeader;//Receivedヘッダ文字列
        public MailSave(MailBox mailBox, Alias alias, MailQueue mailQueue, Logger logger, ReceivedHeader receivedHeader, List<string> domainList)
        {
            _mailBox = mailBox;
            _alias = alias;
            _mailQueue = mailQueue;
            _logger = logger;
            _receivedHeader = receivedHeader;
            _domainList = domainList;
            _localBox = new LocalBox(_logger);

        }

        public bool Save(MailAddress from, List<MailAddress> rcptList, Mail mail, string host, Ip addr)
        {

            List<MailAddress> toList = _alias.Reflection(rcptList, _logger);


            foreach (var to in toList)
            {
                if (!Save(from, to, mail, host, addr))
                {
                    //Ver5.9.8
                    //_logger.Set(LogKind.Error, null, 7, String.Format("From:{0} To:{1}", from, to));
                    _logger.Set(LogKind.Error, null, 55, String.Format("From:{0} To:{1}", from, to));
                    return false;
                }
            }
            return true;
        }

        //Server及びMlから使用される
        //メールの保存(宛先はML以外であることが確定してから使用する)
        //テスト用のモックオブジェクト(TsMailSaveでSave()をオーバーライドできるようにvirtualにする
        virtual public bool Save(MailAddress from, MailAddress to, Mail orgMail, string host, Ip addr)
        {

            //Mailのヘッダ内容等を変更するので、この関数内だけの変更にとどめるため、テンポラリを作成する
            var mail = new Mail(); //orgMail.CreateClone();
            mail.Init2(orgMail.GetBytes());

            //ユニークなID文字列の生成
            var uidStr = string.Format("bjd.{0:D20}.{1:D3}", DateTime.Now.Ticks, _idCounter++);
            //日付文字列の生成
            //var date = Util.LocalTime2Str(DateTime.Now);
            //Receivedヘッダの追加
            mail.AddHeader("Received", _receivedHeader.Get(to, host, addr));

            //            //Message-Idの追加
            //            if (null == mail.GetHeader("Message-ID"))
            //                mail.AddHeader("Message-ID", string.Format("<{0}@{1}>", uidStr, _domainList[0]));
            //            //Fromの追加
            //            if (null == mail.GetHeader("From"))
            //                mail.AddHeader("From", string.Format("<{0}>", @from));
            //            //Dateの追加
            //            if (null == mail.GetHeader("Date"))
            //                mail.AddHeader("Date", string.Format("{0}", date));

            //ローカル宛(若しくはローカルファイル)
            if (to.IsLocal(_domainList))
            {

                //ローカル保存の場合は、X-UIDLを追加する
                mail.AddHeader("X-UIDL", uidStr);

                //ヘッダを追加してサイズが変わるので、ここで初期化する
                var mailInfo = new MailInfo(uidStr, mail.Length, host, addr, from, to);

                if (to.IsFile())
                {  //ローカルファイルの場合(直接ファイルにAppendする)
                    if (!_localBox.Save(to, mail, mailInfo))
                    {
                        return false;
                    }
                }
                else
                { //ローカルユーザの場合（メールボックスへSaveする）
                    if (!_mailBox.Save(to.User, mail, mailInfo))
                    {
                        return false;
                    }
                }
                _logger.Set(LogKind.Normal, null, 8, mailInfo.ToString());
            }
            else
            {
                //Toの追加
                //                if (null == mail.GetHeader("To")) {
                //                    mail.AddHeader("To", string.Format("<{0}>", to));
                //                }

                //ヘッダを追加してサイズが変わるので、ここで初期化する
                var mailInfo = new MailInfo(uidStr, mail.Length, host, addr, from, to);
                if (!_mailQueue.Save(mail, mailInfo))
                {
                    _logger.Set(LogKind.Error, null, 9000059, mail.GetLastError());
                    return false;
                }
                _logger.Set(LogKind.Normal, null, 9, mailInfo.ToString());

            }
            return true;

        }
    }


}
