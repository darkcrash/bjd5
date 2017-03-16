using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Plugins;
using Bjd.Services;
using Xunit;
using Xunit.Abstractions;

namespace Bjd.Common.Test.plugin
{

    public class ListPluginTest
    {

        private ITestOutputHelper output;

        public ListPluginTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Pluginsフォルダの中のdllファイルを列挙()
        {
            using (TestService service = TestService.CreateTestService())
            {
                service.SetOption("Option.ini");
                //service.ContentDirectory("mailbox");
                service.AddOutput(output);

                //const string currentDir = @"C:\tmp2\bjd5\BJD\out";

                //var sut = new ListPlugin(currentDir);
                var sut = new ListPlugin(service.Kernel);
                const int expected = 16;
                //exercise
                var actual = sut.Count;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Option及びServerインスタンスの生成()
        {
            //setUp
            using (TestService service = TestService.CreateTestService())
            {
                service.SetOption("Option.ini");
                //service.ContentDirectory("mailbox");
                service.AddOutput(output);

                //var kernel = new Kernel();
                var kernel = service.Kernel;
                kernel.ListInitialize();
                //const string currentDir = @"C:\tmp2\bjd5\BJD\out";

                //var sut = new ListPlugin(string.Format("{0}\\bin\\plugins", currentDir));
                var sut = new ListPlugin(kernel);
                foreach (var onePlugin in sut)
                {
                    //Optionインスタンス生成
                    var oneOption = onePlugin.CreateOption(kernel, "Option", onePlugin.Name);
                    Assert.NotNull(oneOption);

                    //Serverインスタンス生成
                    var conf = new Conf(oneOption);
                    var oneBind = new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp);
                    var oneServer = onePlugin.CreateServer(kernel, conf, oneBind);
                    Assert.NotNull(oneServer);
                }
            }

        }

    }

}
