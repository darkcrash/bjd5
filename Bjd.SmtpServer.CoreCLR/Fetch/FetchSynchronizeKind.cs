using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Options
{
    public enum FetchSynchronizeKind
    {
        KeepEmailOnServer,
        SynchronizeWithMailbox,
        DeleteEmailFromServer
    }
}
