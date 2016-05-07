using System;
using System.IO;
using Bjd.util;

namespace BjdTest.test
{

    public class TmpOption : IDisposable
    {
        //private readonly Kernel _kernel= new Kernel();
        //private readonly string _testDataPath;

        /// <summary>
        /// 元ファイル
        /// </summary>
        private readonly string _originName;

        /// <summary>
        /// バックアップファイル
        /// </summary>
        private readonly string _backupName;
        
        /// <summary>
        /// テスト対象ファイル
        /// </summary>
        private readonly string _targetName;


        public TmpOption(string subDir, string fileName)
        {
            //_testDataPath = Util.CreateTempDirectory();

            // オリジナルファイル
            //var dir = TestUtil.ProhjectDirectory() + "\\BJD\\out";
            _originName = System.IO.Path.Combine(TestUtil.ProjectDirectory() , "Bjd.CoreCLR", "Option.ini");
           
            // BACKUPファイル
            //_backupName = string.Format("{0}\\Option.bak", _testDataPath);
            _backupName = System.IO.Path.Combine(TestUtil.ProjectDirectory(), "Bjd.CoreCLR", "Option.bak");

            // 上書きファイル
            //_targetName = string.Format("{0}\\{1}\\{2}", TestUtil.ProjectDirectory(), subDir, fileName);
            _targetName = System.IO.Path.Combine(TestUtil.ProjectDirectory(), subDir, fileName);

            // 対象ファイルがない場合はエラー
            if (!File.Exists(_targetName))
            {
                throw new Exception(string.Format("指定されたファイルが見つかりません。 {0}", _targetName));
            }

            // バックアップ作成
            if (File.Exists(_originName))
            {
                File.Copy(_originName, _backupName, true);
            }

            // 上書き
            File.Copy(_targetName, _originName, true);

        }

        public void Dispose()
        {
            // バックアップファイルがある場合はリストアする
            if (File.Exists(_backupName))
            {
                File.Copy(_backupName, _originName, true);
                File.Delete(_backupName);
            }
        }
    }
}
