
namespace ProxyTelnetServer {
    partial class Server {
        protected override void CheckLang()
        {
        }

        public override string GetMsg(int messageNo) {
            switch (messageNo) {
                case 1: return Kernel.IsJp()?"接続しました":"Connected";
                case 2: return Kernel.IsJp()?"接続に失敗しました(1)":"Failed in connection(1)";
                case 3: return Kernel.IsJp()?"接続に失敗しました(2)":"Failed in connection(1)";
            }
            return "unknown";
        }

    }
}
