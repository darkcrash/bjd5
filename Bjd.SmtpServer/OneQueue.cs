using Bjd;
using Bjd.Mailbox;

namespace Bjd.SmtpServer
{
    class OneQueue {
        readonly Kernel _kernel;
        readonly string _fname;
        public OneQueue(Kernel kernel, string fname, MailInfo mailInfo) {
            _kernel = kernel;
            _fname = fname;
            MailInfo = mailInfo;
        }

        public MailInfo MailInfo { get; private set; }
        public Mail Mail(MailQueue mailQueue) {
            var mail = new Mail(_kernel);
            return mailQueue.Read(_fname, ref mail) ? mail : null;
        }

        public void Delete(MailQueue mailQueue) {
            mailQueue.Delete(_fname);
        }
    }

}
