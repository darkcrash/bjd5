﻿using System;
using Bjd;
using Bjd.net;
using Bjd.option;
using Bjd.Common.Test;
using Xunit;
using Bjd.SipServer;

namespace SipServerTest
{
    public class ServerTest: IDisposable, IClassFixture<ServerTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            private  TmpOption _op; //設定ファイルの上書きと退避
            internal Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ
                                         //private SockTcp _v6Cl; //クライアント
                                         //private SockTcp _v4Cl; //クライアント

            public ServerFixture()
            {
                TestUtil.CopyLangTxt();//BJD.Lang.txt

                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.SipServer.CoreCLR.Test", "SipServerTest.ini");
                var kernel = new Kernel();
                var option = kernel.ListOption.Get("Sip");
                var conf = new Conf(option);

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();

            }
            public void Dispose()
            {
                //サーバ停止
                _v4Sv.Stop();
                _v6Sv.Stop();

                _v4Sv.Dispose();
                _v6Sv.Dispose();

                //設定ファイルのリストア
                _op.Dispose();

            }
        }

        private ServerFixture _fixture;

        public ServerTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public  void Dispose()
        {


        }

        //        [SetUp]
        //        public void SetUp() {
        //            //クライアント起動
        //            _v4Cl = Inet.Connect(new Ip(IpKind.V4Localhost), 21, 10, null);
        //            _v6Cl = Inet.Connect(new Ip(IpKind.V6Localhost), 21, 10, null);
        //            //クライアントの接続が完了するまで、少し時間がかかる
        //            //Thread.Sleep(10);
        //
        //        }
        //
        //        [TearDown]
        //        public void TearDown() {
        //            //クライアント停止
        //            _v4Cl.Close();
        //            _v6Cl.Close();
        //        }


        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = _fixture._v4Sv;
            var expected = "+ サービス中 \t                 Sip\t[127.0.0.1\t:TCP 5060]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(actual, expected);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = _fixture._v6Sv;
            var expected = "+ サービス中 \t                 Sip\t[::1\t:TCP 5060]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(actual, expected);

        }


        //        [Fact]
        //        public void ConnectTest() {
        //            var cl = _tsServer.UdpClient();
        //            //Assert.AreEqual(cl.Connected, true);
        //            cl.Close();
        //        }

    }
}
