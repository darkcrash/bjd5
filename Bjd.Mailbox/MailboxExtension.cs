using Bjd.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bjd.Mailbox
{
    public static class MailboxExtension
    {
        public static MailBox GetMailBox(this Kernel k)
        {
            return k.ListComponent.Get<MailBox>();
        }
    }
}
