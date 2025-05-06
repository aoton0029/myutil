using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public class DataExporter
    {
        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, string delimiter = ",")
        {
            // データが空の場合
            if (data == null || !data.Any())
            {
                await File.WriteAllTextAsync(filePath, string.Empty);
                return;
            }

            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && (
                    p.PropertyType.IsPrimitive ||
                    p.PropertyType == typeof(string) ||
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(decimal)))
                .ToArray();

            var csv = new StringBuilder();

            // ヘッダー行
            csv.AppendLine(string.Join(delimiter, properties.Select(p => $"\"{p.Name}\"")));

            // データ行
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return value == null ? "\"\"" : $"\"{value}\"";
                });

                csv.AppendLine(string.Join(delimiter, values));
            }

            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        }

        public async Task ExportToJsonAsync<T>(IEnumerable<T> data, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        public async Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath)
        {
            // DataTableに変換
            var dt = new DataTable();
            var properties = typeof(T).GetProperties();

            // 列を追加
            foreach (var prop in properties)
            {
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // 行を追加
            foreach (var item in data)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }

            // Excel形式でCSVに出力（Excel形式のエクスポート代替）
            var csv = new StringBuilder();

            // ヘッダー行
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                csv.Append($"\"{dt.Columns[i].ColumnName}\"");
                if (i < dt.Columns.Count - 1)
                    csv.Append(",");
            }
            csv.AppendLine();

            // データ行
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var value = row[i];
                    if (value != DBNull.Value)
                    {
                        // 日付型の場合は日本の日付形式に変換
                        if (value is DateTime dateValue)
                        {
                            csv.Append($"\"{dateValue:yyyy/MM/dd HH:mm:ss}\"");
                        }
                        else
                        {
                            csv.Append($"\"{value}\"");
                        }
                    }

                    if (i < dt.Columns.Count - 1)
                        csv.Append(",");
                }
                csv.AppendLine();
            }

            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        }

        public string GetSaveFilePath(string defaultFileName, string filter)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = filter,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }
    }
}
