using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class Form1: Form
    {
        // テンプレート文字列（必要に応じて変更してください）
        private const string FileTemplate = """
---
tags: 
completed: false
rate: 未
category: 
title: 
author: 
publisher: 
published_date: 
completion_date: 
link:
---

# {{title}}

## 基本情報
- **著者**: {{author}}
- **出版社**: {{publisher}}
- **出版日**: {{published_date}}
- **読了日**: {{completion_date}}
- **評価**: {{rate}}

{1}
""";

        public Form1()
        {
            InitializeComponent();

            ProcessMarkdownFiles(
                @"D:\notoa\Pictures\読書記録 aa3a810bc45345c1bc4400c49f984a0f\読書記録 0eb33686820847fa93ff04af3906271c",
                @"D:\notoa\Documents\Obsidians\読書記録");
        }

        private void ProcessMarkdownFiles(string folderA, string folderB)
        {
            try
            {
                // フォルダAからMarkdownファイルを取得
                string[] mdFilesInFolderA = Directory.GetFiles(folderA, "*.md", SearchOption.TopDirectoryOnly);

                // 処理結果の統計
                int createdCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;

                // 各Markdownファイルを処理
                foreach (string mdFile in mdFilesInFolderA)
                {
                    string fileName = Path.GetFileName(mdFile);
                    string[] names = fileName.Split(' ');
                    if(names.Length < 2)
                    {
                        Debug.Print("パス : " + fileName);
                        continue;
                    }
                    fileName = names[0];
                    string fileContent = File.ReadAllText(mdFile);

                    // フォルダB内の全てのサブフォルダを取得
                    string[] subFolders = Directory.GetDirectories(folderB, "*", SearchOption.AllDirectories);

                    // フォルダB自体も検索対象に含める
                    var allFolders = new List<string>(subFolders);
                    allFolders.Insert(0, folderB);

                    bool fileExistsInFolderB = false;

                    // 各サブフォルダ内で同名のファイルを検索
                    foreach (string subFolder in allFolders)
                    {
                        string targetFilePath = Path.Combine(subFolder, fileName + ".md");

                        if (File.Exists(targetFilePath))
                        {
                            fileExistsInFolderB = true;

                            // ファイルが存在する場合は更新
                            string formattedContent = string.Format(FileTemplate, fileName, fileContent);
                            File.WriteAllText(targetFilePath, formattedContent);
                            updatedCount++;

                            LogToConsole($"更新: {targetFilePath}");
                        }
                    }

                    // フォルダBのどこにも同名のファイルが存在しない場合は新規作成
                    if (!fileExistsInFolderB)
                    {
                        string targetFilePath = Path.Combine(folderB, fileName + ".md");
                        string formattedContent = string.Format(FileTemplate, fileName, fileContent);
                        File.WriteAllText(targetFilePath, formattedContent);
                        createdCount++;

                        LogToConsole($"作成: {targetFilePath}");
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                MessageBox.Show($"処理が完了しました。\n\n新規作成: {createdCount}件\n更新: {updatedCount}件\nスキップ: {skippedCount}件",
                    "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ログ出力用メソッド（必要に応じてTextBoxなどに出力）
        private void LogToConsole(string message)
        {
            Debug.Print(message);
            // UIにログを表示する場合は以下のようにします
            // textBoxLog.AppendText(message + Environment.NewLine);
        }
    }
}
