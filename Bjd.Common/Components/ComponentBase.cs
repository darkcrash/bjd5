using Bjd.Configurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Components
{

    //ComponentBase. component class Kernel include 
    public abstract class ComponentBase : IDisposable
    {
        public string NameTag { get; private set; }


        public ComponentBase(Kernel kernel, Conf conf)
        {
            this.NameTag = conf.NameTag;
        }

        public virtual void Dispose()
        {
        }
    }
}

