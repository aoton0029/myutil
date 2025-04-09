using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample
{
    public static class SafeFileWriter
    {
        public static void WriteAtomic(string targetPath, string content)
        {
            var tempPath = targetPath + ".tmp";
            var backupPath = targetPath + ".bak";

            // 一時ファイルに書き込み
            File.WriteAllText(tempPath, content, Encoding.UTF8);

            // バックアップ保存（任意）
            if (File.Exists(targetPath))
            {
                File.Copy(targetPath, backupPath, overwrite: true);
            }

            // 上書きではなく、完全な置き換えで元ファイルと入れ替える
            File.Replace(tempPath, targetPath, backupPath, ignoreMetadataErrors: true);
        }
    }

}
