using System;

namespace Bjd.tool {
    public abstract class OneTool:IDisposable {

        protected Kernel Kernel;

        public string NameTag { get; private set; }
        abstract public string JpMenu { get; }
        abstract public string EnMenu { get; }
        abstract public char Mnemonic { get; }


        protected OneTool(Kernel kernel, string nameTag) {
            Kernel = kernel;
            NameTag = nameTag;
        }
        public void Dispose() {
            
        }
    }
}
