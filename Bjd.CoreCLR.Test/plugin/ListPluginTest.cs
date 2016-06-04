using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Plugins;
using Bjd.Services;
using Xunit;

namespace Bjd.Common.Test.plugin
{

    public class ListPluginTest
    {

        [Fact]
        public void Pluginsフォルダの中のdllファイルを列挙()
        {
            //setUp
            TestService service = TestService.CreateTestService();

            //const string currentDir = @"C:\tmp2\bjd5\BJD\out";

            //var sut = new ListPlugin(currentDir);
            var sut = new ListPlugin();
            const int expected = 16;
            //exercise
            var actual = sut.Count;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Option及びServerインスタンスの生成()
        {
            //setUp
            TestService service = TestService.CreateTestService();

            //var kernel = new Kernel();
            var kernel = service.Kernel;
            //const string currentDir = @"C:\tmp2\bjd5\BJD\out";

            //var sut = new ListPlugin(string.Format("{0}\\bin\\plugins", currentDir));
            var sut = new ListPlugin();
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
