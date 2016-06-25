using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Bjd.Utils;

namespace Bjd.Plugins
{
    //プラグインフォルダ内のjarファイルを列挙するクラス
    public class ListPlugin : ListBase<IPlugin>
    {
        //dir 検索対象となるpluginsフォルダ
        public ListPlugin()
        {
            Trace.TraceInformation("ListPlugin..ctor Start");

            if (Define.Libraries != null)
            {
                //foreach (var lib in Define.Libraries)
                //{
                //    //if (!lib.Name.EndsWith("Server.CoreCLR")) continue;
                //    //Ar.Add(new OnePlugin(lib));
                //    //Trace.TraceInformation($"plugin {lib.Name}");
                //}

                foreach (var lib in Define.Libraries)
                {
                    var nm = new AssemblyName(lib.Name);
                    Assembly asm;
                    try { asm = Assembly.Load(nm); }
                    catch { continue; }
                    foreach (var t in asm.Modules.SelectMany(_ => _.GetTypes()))
                    {
                        var info = t.GetTypeInfo();
                        if (!info.IsClass) continue;
                        if (info.FullName == "Bjd.Plugins.OnePlugin") continue;
                        if (typeof(IPlugin).IsAssignableFrom(t))
                        {
                            var ctor = t.GetConstructor(Type.EmptyTypes);
                            if (ctor == null)
                            {
                                Trace.TraceError($"[IPlugin] require default conctructor {t.FullName}");
                                continue;
                            }
                            try
                            {
                                var instance = (IPlugin)ctor.Invoke(null);
                                Trace.TraceInformation($"[IPlugin] {instance.PluginName}");
                                Ar.Add(instance);
                            }
                            catch
                            {
                                Trace.TraceError($"[IPlugin] throw exception conctructor {t.FullName}");
                                continue;
                            }
                        }
                        var plugin = t as IPlugin;
                        if (plugin != null)
                        {
                            Trace.TraceInformation($"[IPlugin] {plugin.PluginName}");
                        }

                    }
                }

            }

            Trace.TraceInformation("ListPlugin..ctor End");
        }


        //名前によるプラグイン情報オブジェクト（OnePlugin）の検索
        //<font color=red>一覧に存在しない名前で検索を行った場合、設計上の問題として処理される</font>
        public IPlugin Get(String name)
        {
            int index = name.IndexOf("-");
            if (index != -1)
            {
                name = name.Substring(0, index);
            }

            foreach (IPlugin o in Ar)
            {
                if (o.Name == name)
                {
                    return o;
                }
            }
            Util.RuntimeException(string.Format("ListPlugin.get({0})==null", name));
            return null;
        }


    }
}