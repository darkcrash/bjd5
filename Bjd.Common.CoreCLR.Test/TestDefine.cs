using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Common.Test
{
    public class TestDefine
    {

        public static readonly TestDefine Instance = new TestDefine();

        public readonly string TestDirectory;
        public readonly string TestMailboxPath;

        private TestDefine()
        {
            TestDirectory = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
            TestDirectory = System.IO.Path.Combine(TestDirectory, "TestDirectory");
            try
            {
                System.IO.Directory.Delete(TestDirectory, true);
            }
            catch { }

            TestMailboxPath = System.IO.Path.Combine(TestDirectory, "mailbox");

            System.IO.Directory.CreateDirectory(TestDirectory);
            System.IO.Directory.CreateDirectory(TestMailboxPath);

        }


    }
}
