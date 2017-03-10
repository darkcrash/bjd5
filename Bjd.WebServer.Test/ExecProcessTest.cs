﻿using System;
using System.IO;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.WebServer.IO;
using Bjd.WebServer.Outside;

namespace WebServerTest
{

    public class ExecProcessTest : IDisposable
    {
        private TestService _service;
        private string directory;

        public ExecProcessTest()
        {
            _service = TestService.CreateTestService();
            directory = _service.Kernel.Enviroment.ExecutableDirectory;
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Theory]
        [InlineData(1000000, 1)] //1で1Mbyte
        [InlineData(256, 1)] //1で1Mbyte
        //[TestCase(1000000, 2000)] //1で1Mbyte 自作cat.exeでは200MByteまでしか対応できない
        public void StartTest(int block, int count)
        {
            //var srcDir = string.Format("{0}\\WebServerTest", TestUtil.ProjectDirectory());
            //var srcDir = AppContext.BaseDirectory;

            //こちらの自作cat.exeでは、200Mbyteまでしか対応できていない
            //var execProcess = new ExecProcess(string.Format("{0}\\cat.exe", srcDir), "", srcDir, null);
            //var execProcess = new ExecProcess(Path.Combine(srcDir, "cat.exe"), "", srcDir, null);
            var execProcess = new ExecProcess("cat.exe", "", directory, null);

            var buf = new byte[block];
            for (var b = 0; b < block; b++)
            {
                buf[b] = (byte)b;
            }
            var inputStream = new WebStream(block * count);
            for (var i = 0; i < count; i++)
            {
                inputStream.Add(buf);
            }
            WebStream outputStream;
            execProcess.Start(inputStream, out outputStream);

            for (var i = 0; i < count; i++)
            {
                var len = outputStream.Read(buf, 0, buf.Length);
                Assert.Equal(len, block);
                if (i == 0)
                {
                    Assert.Equal(buf[0], 0);
                    Assert.Equal(buf[1], 1);
                    Assert.Equal(buf[2], 2);

                }
            }

            outputStream.Dispose();
            inputStream.Dispose();
        }

    }
}