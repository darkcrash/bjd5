using System;
using System.IO;
using Bjd.Utils;
using Bjd.Services;

namespace Bjd.Test
{

    public class TestOption : IDisposable
    {
        //private readonly Kernel _kernel= new Kernel();
        //private readonly string _testDataPath;

        /// <summary>
        /// 元ファイル
        /// </summary>
        private string _originName;

        /// <summary>
        /// バックアップファイル
        /// </summary>
        private string _backupName;

        /// <summary>
        /// テスト対象ファイル
        /// </summary>
        private string _targetName;

        /// <summary>
        /// テストディレクトリに配置されたテスト対象ファイル
        /// </summary>
        private string _TestTargetName;

        public TestOption(string fileName) : this("", fileName)
        {
        }

        public TestOption(string subDir, string fileName)
        {
            //_testDataPath = Util.CreateTempDirectory();

            // オリジナルファイル
            //var dir = TestUtil.ProhjectDirectory() + "\\BJD\\out";
            //_originName = System.IO.Path.Combine(TestUtil.ProjectDirectory(), "Option.ini");
            _originName = "Option.ini";

            // BACKUPファイル
            //_backupName = string.Format("{0}\\Option.bak", _testDataPath);
            //_backupName = System.IO.Path.Combine(TestUtil.ProjectDirectory(), "Option.bak");
            _backupName = "Option.bak";

            // 上書きファイル
            //_targetName = string.Format("{0}\\{1}\\{2}", TestUtil.ProjectDirectory(), subDir, fileName);
            //_targetName = System.IO.Path.Combine(TestUtil.ProjectDirectory(), subDir, fileName);
            _targetName = fileName;
            if (!string.IsNullOrEmpty(subDir))
                _targetName = System.IO.Path.Combine(subDir, fileName);

            var dir = TestService.ProjectDirectory;
            _originName = Path.Combine(dir, _originName);
            _backupName = Path.Combine(dir, _backupName);
            _targetName = Path.Combine(dir, _targetName);

        }

        internal void Copy(TestService service)
        {
            var tesDir = service.Kernel.Enviroment.ExecutableDirectory;
            _TestTargetName = Path.Combine(tesDir, "Option.ini");

            // 対象ファイルがない場合はエラー
            if (!File.Exists(_targetName))
            {
                throw new Exception(string.Format("指定されたファイルが見つかりません。 {0}", _targetName));
            }

            // テストディレクトリにコピー
            File.Copy(_targetName, _TestTargetName, true);

            //// バックアップ作成
            //if (File.Exists(_originName))
            //{
            //    File.Copy(_targetName, _backupName, true);
            //}

            //// 上書き
            //File.Copy(_targetName, _originName, true);

        }

        public void Dispose()
        {
            //// バックアップファイルがある場合はリストアする
            //if (File.Exists(_backupName))
            //{
            //    File.Copy(_backupName, _originName, true);
            //    File.Delete(_backupName);
            //}
            // テストディレクトリのファイルを削除
        }
    }
}
