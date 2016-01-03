using System;
using Bjd;
using Bjd.tool;

namespace BJD.SmtpServer
{
    public class Tool : OneTool {
        public Tool(Kernel kernel, string nameTag)
            : base(kernel, nameTag) {

        }
        public override string JpMenu { get { return "[SMTP] メールボックス（メールキュー）"; } }
        public override string EnMenu { get { return "[SMTP] MainBox(Queue)"; } }
        public override char Mnemonic { get { return 'B'; } }

    }
}


