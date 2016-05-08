using System;
using System.IO;
using System.Linq;
using Bjd.Utils;
using System.Diagnostics;

namespace Bjd.Plugins
{
    //プラグインフォルダ内のjarファイルを列挙するクラス
    public class ListPlugin : ListBase<OnePlugin>
    {
        //dir 検索対象となるpluginsフォルダ
        public ListPlugin()
        {
            Trace.TraceInformation("ListPlugin..ctor Start");

            ////フォルダが存在しない場合、初期化終了
            //if (!Directory.Exists(dir))
            //{
            //    return;
            //}

            //DLLを検索し、各オプションを生成する
            //Ver5.2.4 関係ない*Server.dll以外は、対象外とする
            //var list = Directory.GetFiles(dir, "*Server.dll").ToList();
            //var list = Directory.GetFiles(dir, "*Server.CoreCLR.dll", SearchOption.AllDirectories).ToList();
            //list.Sort();
            //foreach (var path in list)
            //{
            //    Ar.Add(new OnePlugin(path));
            //    Trace.TraceInformation($"ListPlugin..ctor {path}");
            //}

            Trace.Indent();
            if (Define.Libraries != null)
            {
                foreach (var lib in Define.Libraries)
                {
                    if (!lib.Name.EndsWith("Server.CoreCLR"))
                        continue;
                    Ar.Add(new OnePlugin(lib));
                    Trace.TraceInformation($"plugin {lib.Name}");
                }
            }
            Trace.Unindent();


            Trace.TraceInformation("ListPlugin..ctor End");
        }

        //名前によるプラグイン情報オブジェクト（OnePlugin）の検索
        //<font color=red>一覧に存在しない名前で検索を行った場合、設計上の問題として処理される</font>
        public OnePlugin Get(String name)
        {
            int index = name.IndexOf("-");
            if (index != -1)
            {
                name = name.Substring(0, index);
            }

            foreach (OnePlugin o in Ar)
            {
                if (o.Name == name)
                {
                    return o;
                }
            }
            Util.RuntimeException(string.Format("ListPlugin.get({0})==null", name));
            return null;
        }

        //jarファイルに梱包されているクラスの列挙
        //file 対象jarファイル
        //クラス名配列
        //        private String[] GetClassNameList(File file){
        //
        //            //パッケージ名の生成
        //            //sample.jar
        //            String packageName = file.getName();
        //            //sample
        //            packageName = packageName.substring(0, packageName.length() - 4);
        //            //bjd.plubgins.sample
        //            packageName = string.Format("bjd.plugins.%s", packageName);
        //
        //            List<String> ar = new List<string>();
        //            try{
        //                //jarファイル内のファイルを列挙
        //                JarInputStream jarIn = new JarInputStream(new FileInputStream(file));
        //                JarEntry entry;
        //                while ((entry = jarIn.getNextJarEntry()) != null){
        //                    if (!entry.isDirectory()){
        //                        //ディレクトリは対象外
        //                        //　Server.class
        //                        String className = entry.getName();
        //                        if (className.lastIndexOf("Server.class") == -1 && className.lastIndexOf("Option.class") == -1){
        //                            //対象外
        //                            continue;
        //                        }
        //
        //                        //　Server　　.classを外す
        //                        int index = className.indexOf(".class");
        //                        if (index != -1){
        //                            className = className.substring(0, index);
        //                        }
        //                        className = className.replace("/", ".");
        //                        ar.add(className);
        //                    }
        //                }
        //                jarIn.close();
        //                return (String[]) ar.toArray(new String[0]);
        //            }catch (Exception ex){
        //                ex.printStackTrace();
        //            }
        //            return new String[0];
        //        }
    }
}