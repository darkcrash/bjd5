using System;
using Bjd.plugin;
using Bjd.util;

namespace Bjd.option{
    //オプションのリストを表現するクラス
    //Kernelの中で使用される
    public class ListOption : ListBase<OneOption>{

        private readonly Kernel _kernel;

        public ListOption(Kernel kernel, ListPlugin listPlugin){
            _kernel = kernel;
            Initialize(listPlugin);
        }

        public OneOption Get(String nameTag){
            foreach (var o in Ar){
                if (o.NameTag == nameTag){
                    return o;
                }
            }
            return null;
        }

        //Ver6.0.0.
        public void Replice(OneOption oneOption){
            for (int i=0;i<Ar.Count;i++) {
                if (Ar[i].NameTag == oneOption.NameTag){
                    Ar[i] = oneOption;
                    break;
                }
            }
        }

        //null追加を回避するために、Ar.Add()は、このファンクションを使用する
        private bool Add(OneOption o){
            if (o == null){
                return false;
            }
            Ar.Add(o);
            return true;
        }

        //Kernel.Dispose()で、有効なオプションだけを出力するために使用する
        public void Save(IniDb iniDb){
            foreach (var o in Ar){
                o.Save(iniDb);
            }
        }


        //オプションリストの初期化
        private void Initialize(ListPlugin listPlugin){

            Ar.Clear();

            //固定的にBasicとLogを生成する
            const string executePath = ""; // Application.ExecutablePath
            Add(new OptionBasic(_kernel, executePath)); //「基本」オプション
            Add(new OptionLog(_kernel, executePath)); //「ログ」オプション

            foreach (var onePlugin in listPlugin){

                var oneOption = onePlugin.CreateOption(_kernel, "Option", onePlugin.Name);
                if (oneOption.NameTag == "Web"){
                    //WebServerの場合は、バーチャルホストごとに１つのオプションを初期化する
                    OneOption o = onePlugin.CreateOption(_kernel, "OptionVirtualHost", "VirtualHost");
                    if (Add(o)){
                        var dat = (Dat) o.GetValue("hostList");
                        if (dat != null){
                            foreach (var e in dat){
                                if (e.Enable){
                                    string name = string.Format("Web-{0}:{1}", e.StrList[1], e.StrList[2]);
                                    Add(onePlugin.CreateOption(_kernel, "Option", name));
                                }
                            }
                        }
                    }
                } else if (oneOption.NameTag == "Tunnel"){
                    //TunnelServerの場合は、１トンネルごとに１つのオプションを初期化する
                    OneOption o = onePlugin.CreateOption(_kernel, "OptionTunnel", "TunnelList");
                    if (Add(o)){
                        var dat = (Dat) o.GetValue("tunnelList");
                        if (dat != null){
                            foreach (var e in dat){
                                if (e.Enable){
                                    string name = string.Format("{0}:{1}:{2}:{3}", (e.StrList[0] == "0") ? "TCP" : "UDP", e.StrList[1], e.StrList[2], e.StrList[3]);
                                    Add(onePlugin.CreateOption(_kernel, "Option", String.Format("Tunnel-{0}", name)));
                                }
                            }
                        }
                    }
                } else{
                    Add(oneOption);

                    //DnsServerのプラグイン固有オプションの生成
                    if (oneOption.NameTag == "Dns"){
                        OneOption o = onePlugin.CreateOption(_kernel, "OptionDnsDomain", "DnsDomain");
                        if (Add(o)){
                            var dat = (Dat) o.GetValue("domainList");
                            if (dat != null){
                                foreach (var e in dat){
                                    if (e.Enable){
                                        Add(onePlugin.CreateOption(_kernel, "OptionDnsResource", String.Format("Resource-{0}", e.StrList[0])));
                                    }
                                }
                            }
                        }
                    } else if (oneOption.NameTag == "Smtp"){
                        //Ver6.0.0
                        OneOption o = onePlugin.CreateOption(_kernel, "OptionMl", "Ml");
                        //var o = (OneOption)Util.CreateInstance(kernel, path, "OptionMl", new object[] { kernel, path, "Ml" });
                        if (Add(o)){
                            var dat = (Dat)o.GetValue("mlList");
                            if (dat != null){
                                foreach (var e in dat){
                                    if (e.Enable){
                                        Add(onePlugin.CreateOption(_kernel, "OptionOneMl", String.Format("Ml-{0}", e.StrList[0])));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Get("Smtp") != null || Get("Pop") != null){
                Add(new OptionMailBox(_kernel, Define.ExecutablePath)); //メールボックス
            }
        }

    }
}

