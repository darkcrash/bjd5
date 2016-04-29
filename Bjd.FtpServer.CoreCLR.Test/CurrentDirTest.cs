using System.Collections.Generic;
using System.IO;
using Bjd.FtpServer;
using Bjd;
using Xunit;

namespace FtpServerTest
{


    public class CurrentDirTest
    {


        public CurrentDirTest()
        {
            Define.Initialize(null);
        }


        [Fact]
        public void ContentTypeTest()
        {
            //....\\FtpServerTest\\TestDir
            var startup = Directory.GetCurrentDirectory();
            var path1 = Path.GetDirectoryName(startup);
            var path2 = Path.GetDirectoryName(path1);
            var dir = Path.GetDirectoryName(path2);
            //dir = c:\tmp2\bjd5
            //var dir = new File(".").getAbsoluteFile().getParentFile().getParent(); //c:\dev\bjd6


            //var workDir = dir + "\\work";
            //var rootDirectory = workDir + "\\FtpTestDir";
            var workDir = System.IO.Path.Combine(startup, "work");
            //var rootDirectory = Path.Combine(workDir, "Bjd.FtpServer.CoreCLR.Test");
            var rootDirectory = Path.Combine(startup, "TestDir");


            var listMount = new ListMount(null);

            //var op  = new Option(kernel, "", "Ftp");
            //var homeDir = string.Format("{0}\\home0", rootDirectory);
            var homeDir = Path.Combine(rootDirectory, "home0");

            //ディレクトリ変更 と表示テキスト
            //var currentDir = new CurrentDir(homeDir, listMount); //初期化
            var currentDir = new CurrentDir(homeDir, listMount); //初期化

            Assert.Equal(currentDir.Cwd("home0-sub0"), true);
            Assert.Equal(currentDir.GetPwd(), "/home0-sub0");

            currentDir = new CurrentDir(homeDir, listMount); //初期化
            Assert.Equal(currentDir.Cwd("home0-sub0/sub0-sub0"), true);
            Assert.Equal(currentDir.GetPwd(), "/home0-sub0/sub0-sub0");

            currentDir = new CurrentDir(homeDir, listMount); //初期化
            Assert.Equal(currentDir.Cwd("home0-sub0/sub0-sub0"), true);
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/home0-sub0");

            //ホームディレクトリより階層上へは移動できない
            currentDir = new CurrentDir(homeDir, listMount); //初期化
            Assert.Equal(currentDir.Cwd("home0-sub0/sub0-sub0"), true);
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/home0-sub0");
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/");
            Assert.Equal(currentDir.Cwd(".."), false);
            Assert.Equal(currentDir.GetPwd(), "/");

            //存在しないディレクトリへの変更
            currentDir = new CurrentDir(homeDir, listMount);
            Assert.Equal(currentDir.Cwd("home0-sub0/sub0"), false);
            Assert.Equal(currentDir.GetPwd(), "/");

            //初期化文字列の対応
            currentDir = new CurrentDir(homeDir + "\\", listMount);
            Assert.Equal(currentDir.Cwd("home0-sub0"), true);
            Assert.Equal(currentDir.GetPwd(), "/home0-sub0");

            //ファイル一覧の取得
            currentDir = new CurrentDir(homeDir, listMount);
            var ar = new List<string>{
                "d home0-sub0",
                "d home0-sub1",
                "d home0-sub2",
                "- home0-1.txt",
                "- home0-2.txt",
                "- home0-3.txt"
            };
            Assert.Equal(confirm(currentDir, "*.*", ar), true);

            //ファイル一覧の取得
            currentDir = new CurrentDir(homeDir, listMount);
            Assert.Equal(currentDir.Cwd("home0-sub0"), true);
            ar.Clear();
            ar.Add("- home0-sub0-1.txt");
            ar.Add("- home0-sub0-2.txt");
            ar.Add("- home0-sub0-3.txt");
            Assert.Equal(confirm(currentDir, "*.txt", ar), true);

            //**************************************************
            //仮想フォルダを追加して試験する
            //**************************************************
            var fromFolder = string.Format("{0}\\home2", rootDirectory);
            var toFolder = string.Format("{0}\\home0", rootDirectory);
            listMount.Add(fromFolder, toFolder);

            //ファイル一覧の取得
            currentDir = new CurrentDir(homeDir, listMount);

            ar.Clear();
            ar.Add("d home0-sub0");
            ar.Add("d home0-sub1");
            ar.Add("d home0-sub2");
            ar.Add("d home2");
            ar.Add("- home0-1.txt");
            ar.Add("- home0-2.txt");
            ar.Add("- home0-3.txt");
            Assert.Equal(confirm(currentDir, "*.*", ar), true);
            Assert.Equal(currentDir.Cwd("home2"), true);
            Assert.Equal(currentDir.GetPwd(), "/home2");
            Assert.Equal(currentDir.Cwd("home2-sub0"), true);
            Assert.Equal(currentDir.GetPwd(), "/home2/home2-sub0");
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/home2");
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/");

            Assert.Equal(currentDir.Cwd("home2/home2-sub0"), true);
            Assert.Equal(currentDir.GetPwd(), "/home2/home2-sub0");
            Assert.Equal(currentDir.Cwd("../../.."), false);
            Assert.Equal(currentDir.GetPwd(), "/home2/home2-sub0");
            Assert.Equal(currentDir.Cwd("../.."), true);
            Assert.Equal(currentDir.GetPwd(), "/");

            //**************************************************
            ////仮想フォルダを追加して試験する
            //**************************************************
            fromFolder = workDir + "\\FtpTestDir2\\tmp";
            toFolder = string.Format("{0}\\home0", rootDirectory);
            listMount.Add(fromFolder, toFolder);

            //ファイル一覧の取得
            currentDir = new CurrentDir(homeDir, listMount);
            ar.Clear();
            ar.Add("d home0-sub0");
            ar.Add("d home0-sub1");
            ar.Add("d home0-sub2");
            ar.Add("d home2");
            ar.Add("d tmp");
            ar.Add("- home0-1.txt");
            ar.Add("- home0-2.txt");
            ar.Add("- home0-3.txt");
            Assert.Equal(confirm(currentDir, "*.*", ar), true);
            Assert.Equal(currentDir.Cwd("tmp"), true);
            Assert.Equal(currentDir.GetPwd(), "/tmp");
            Assert.Equal(currentDir.Cwd("sub"), true);
            Assert.Equal(currentDir.GetPwd(), "/tmp/sub");
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/tmp");
            Assert.Equal(currentDir.Cwd(".."), true);
            Assert.Equal(currentDir.GetPwd(), "/");

            Assert.Equal(currentDir.Cwd("tmp/sub"), true);
            Assert.Equal(currentDir.GetPwd(), "/tmp/sub");
            Assert.Equal(currentDir.Cwd("../../.."), false);
            Assert.Equal(currentDir.GetPwd(), "/tmp/sub");
            Assert.Equal(currentDir.Cwd("../.."), true);
            Assert.Equal(currentDir.GetPwd(), "/");

        }

        //CurrentDir.List()の確認用メソッド
        private bool confirm(CurrentDir currentDir, string mask, List<string> list)
        {

            //widwMode=trueで試験を実施する
            var wideMode = true;
            //一覧取得
            var ar = currentDir.List(mask, wideMode);
            //件数確認
            if (ar.Count != list.Count)
            {
                return false;
            }
            //確認用テンポラリ文字列を生成する
            //var tmp = ar.Select(l => l.Split(' ')).Select(a => a[0][0] + " " + a[8]).ToList();
            var tmp = new List<string>();
            foreach (var s in ar)
            {
                var t = s.Split(' ');
                var l = string.Format("{0} {1}", t[0][0], t[8]);
                tmp.Add(l);

            }
            //指定リストに該当したテンポラリ行を削除していく
            foreach (var l in list)
            {
                var index = tmp.IndexOf(l);
                if (index < 0)
                {
                    return false;
                }
                tmp.RemoveAt(index);
            }
            //テンポラリが０行になったら成功
            if (tmp.Count != 0)
            {
                return false;
            }

            //widwMode=falseでもう一度同じ要領で試験を実施する
            wideMode = false;
            //一覧取得
            tmp = currentDir.List(mask, wideMode);
            //件数確認
            if (tmp.Count != list.Count)
            {
                return false;
            }

            //指定レストに該当したテンポラリ行を削除していく
            foreach (var l in list)
            {
                var index = tmp.IndexOf(l.Substring(2));
                if (index < 0)
                {
                    return false;
                }
                tmp.RemoveAt(index);
            }
            //テンポラリが０行になったら成功
            if (tmp.Count != 0)
            {
                return false;
            }
            return true;

        }
    }
}
