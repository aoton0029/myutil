using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class DataTableUtility
    {
        public static DataTable LeftJoin(DataTable dt1, DataTable dt2, string key1, string key2)
        {
            DataTable dt = new DataTable();
            dt = dt1.Clone();
            foreach (DataColumn dc in dt2.Columns)
            {
                if (dc.ColumnName != key2)
                {
                    dt.Columns.Add(dc.ColumnName);
                }
            }

            var query = from t1 in dt1.AsEnumerable()
                        join t2 in dt2.AsEnumerable()
                        on t1[key1] equals t2[key2] into temp
                        from t in temp.DefaultIfEmpty()
                        select t1.ItemArray.Concat((t == null) ? dt2.NewRow().ItemArray : t.ItemArray).ToArray();

            foreach (object[] values in query)
            {
                dt.Rows.Add(values);
            }

            return dt;
        }

        public static DataTable InnerJoin(DataTable dt1, DataTable dt2, string key1, string key2)
        {
            DataTable dt = new DataTable();
            dt = dt1.Clone();
            foreach (DataColumn dc in dt2.Columns)
            {
                if (dc.ColumnName != key2)
                {
                    dt.Columns.Add(dc.ColumnName);
                }
            }

            var query = from t1 in dt1.AsEnumerable()
                        join t2 in dt2.AsEnumerable()
                        on t1[key1] equals t2[key2]
                        select t1.ItemArray.Concat(t2.ItemArray).ToArray();

            foreach (object[] values in query)
            {
                dt.Rows.Add(values);
            }

            return dt;
        }


        public static DataTable AddPathColumn(DataTable table, string idColumnName, string parentIDColumnName, string pathColumnName)
        {
            table.Columns.Add(pathColumnName, typeof(string));

            foreach (DataRow row in table.Rows)
            {
                string path = GetPath(table, row, idColumnName, parentIDColumnName);
                row[pathColumnName] = path;
            }

            return table;

            string GetPath(DataTable table, DataRow row, string aIdCol, string aParentIdCol)
            {
                if (row[aParentIdCol] == DBNull.Value)
                {
                    return row[aIdCol].ToString();
                }
                else
                {
                    DataRow parentRow = table.Select($"{aIdCol} = {row[aParentIdCol]}").FirstOrDefault();
                    if (parentRow != null)
                    {
                        string parentPath = GetPath(table, parentRow, aIdCol, aParentIdCol);
                        return $"{parentPath}.{row[aIdCol]}";
                    }
                    else
                    {
                        return row[aIdCol].ToString();
                    }
                }
            }
        }


        // 階層番号列を追加する汎用メソッド
        public static DataTable AddHierarchyColumn(DataTable table, string idColumnName, string parentIDColumnName, string hierarchyColumnName)
        {
            table.Columns.Add(hierarchyColumnName, typeof(string));

            foreach (DataRow row in table.Rows)
            {
                string hierarchyNumber = GetHierarchyNumber(row, table, idColumnName, parentIDColumnName);
                row[hierarchyColumnName] = hierarchyNumber;
            }

            return table;

            string GetHierarchyNumber(DataRow row, DataTable table, string idColumnName, string parentIDColumnName)
            {
                if (row[parentIDColumnName] == DBNull.Value || Convert.ToInt32(row[parentIDColumnName]) == 0)
                {
                    return "1"; // 最上位階層は1とする
                }
                else
                {
                    DataRow parentRow = table.Select($"{idColumnName} = {row[parentIDColumnName]}").FirstOrDefault();
                    if (parentRow != null)
                    {
                        string parentHierarchy = GetHierarchyNumber(parentRow, table, idColumnName, parentIDColumnName);
                        int parentHierarchyInt = Convert.ToInt32(parentHierarchy);
                        return $"{parentHierarchyInt + 1}";
                    }
                    else
                    {
                        return "1"; // 親が見つからない場合も最上位階層とみなす
                    }
                }
            }
        }

        /// <summary>
        /// 列名を指定して2つのDataTableの差分行を取得する
        /// </summary>
        /// <param name="table1">比較元のDataTable</param>
        /// <param name="table2">比較先のDataTable</param>
        /// <param name="columnsToCompare1">table1の比較対象列名リスト</param>
        /// <param name="columnsToCompare2">table2の比較対象列名リスト</param>
        /// <returns>差分行を含むDataTable</returns>
        public static DataTable GetDifferenceRows(
            DataTable table1,
            DataTable table2,
            List<string> columnsToCompare1,
            List<string> columnsToCompare2)
        {
            if (columnsToCompare1.Count != columnsToCompare2.Count)
            {
                throw new ArgumentException("比較対象列名リストの数が一致していません。");
            }

            // 差分を格納する結果用DataTable
            DataTable result = table1.Clone();

            // table2の比較列データをハッシュセットに変換
            var table2KeySet = new HashSet<string>(
                table2.AsEnumerable().Select(row => string.Join(
                    "|",
                    columnsToCompare2.Select(col => row[col]?.ToString() ?? string.Empty)
                ))
            );

            // table1の行を比較して差分を取得
            foreach (var row in table1.AsEnumerable())
            {
                var key = string.Join(
                    "|",
                    columnsToCompare1.Select(col => row[col]?.ToString() ?? string.Empty)
                );

                if (!table2KeySet.Contains(key))
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }

        public static DataRow GetRowWithMaxValue(DataTable dataTable, string columnName)
        {
            DataRow maxRow = null;
            double maxValue = double.MinValue;

            foreach (DataRow row in dataTable.Rows)
            {
                double currentValue = Convert.ToDouble(row[columnName]);
                if (currentValue > maxValue)
                {
                    maxValue = currentValue;
                    maxRow = row;
                }
            }

            return maxRow;
        }

        public static List<T> GetUniqueValues<T>(DataTable dataTable, string columnName)
        {
            var uniqueValues = new HashSet<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                uniqueValues.Add((T)Convert.ChangeType(row[columnName], typeof(T)));
            }

            return uniqueValues.ToList();
        }

        public static DataTable MergeTables(DataTable table1, DataTable table2, string[] keyColumns)
        {
            DataTable resultTable = table1.Clone();

            foreach (DataRow row in table1.Rows)
            {
                resultTable.ImportRow(row);
            }

            foreach (DataRow row in table2.Rows)
            {
                bool match = resultTable.AsEnumerable()
                    .Any(existingRow => keyColumns.All(key => existingRow[key].Equals(row[key])));

                if (!match)
                {
                    resultTable.ImportRow(row);
                }
            }

            return resultTable;
        }

        public static DataTable GroupBy(DataTable dataTable, string groupColumn, string aggregateColumn, Func<IEnumerable<object>, object> aggregateFunction)
        {
            var groupedTable = new DataTable();
            groupedTable.Columns.Add(groupColumn, dataTable.Columns[groupColumn].DataType);
            groupedTable.Columns.Add("Aggregate", typeof(object));

            var groups = dataTable.AsEnumerable()
                .GroupBy(row => row[groupColumn]);

            foreach (var group in groups)
            {
                var newRow = groupedTable.NewRow();
                newRow[groupColumn] = group.Key;
                newRow["Aggregate"] = aggregateFunction(group.Select(row => row[aggregateColumn]));
                groupedTable.Rows.Add(newRow);
            }

            return groupedTable;
        }

        public static void AddConditionFlagColumn(DataTable dataTable, string flagColumn, string condition)
        {
            if (!dataTable.Columns.Contains(flagColumn))
            {
                dataTable.Columns.Add(flagColumn, typeof(bool));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                row[flagColumn] = dataTable.Select(condition).Contains(row);
            }
        }

        public static void ChangeColumnType(DataTable dataTable, string columnName, Type newType)
        {
            if (!dataTable.Columns.Contains(columnName))
                throw new ArgumentException($"Column '{columnName}' does not exist.");

            // 新しい列を作成
            string tempColumnName = columnName + "_temp";
            dataTable.Columns.Add(tempColumnName, newType);

            foreach (DataRow row in dataTable.Rows)
            {
                if (!row.IsNull(columnName))
                {
                    row[tempColumnName] = Convert.ChangeType(row[columnName], newType);
                }
            }

            // 古い列を削除して新しい列に置き換え
            dataTable.Columns.Remove(columnName);
            dataTable.Columns[tempColumnName].ColumnName = columnName;
        }

        public static void UpdateRowsByCondition(DataTable dataTable, string condition, string columnName, object newValue)
        {
            var rows = dataTable.Select(condition);
            foreach (var row in rows)
            {
                row[columnName] = newValue;
            }
        }

        /// <summary>
        /// データテーブル間で行を移動する汎用メソッド
        /// </summary>
        /// <param name="sourceTable">元のデータテーブル</param>
        /// <param name="destinationTable">移動先のデータテーブル</param>
        /// <param name="filterExpression">行を移動させる条件を表すフィルタ式</param>
        public static void MoveRowsFlexible(DataTable sourceTable, DataTable destinationTable, string filterExpression)
        {
            // フィルタ条件に一致する行を取得
            DataRow[] rowsToMove = sourceTable.Select(filterExpression);

            foreach (DataRow sourceRow in rowsToMove)
            {
                // 移動先の新しい行を作成
                DataRow newRow = destinationTable.NewRow();

                foreach (DataColumn destinationColumn in destinationTable.Columns)
                {
                    // 移動先の列名が元のテーブルに存在する場合のみ値をコピー
                    if (sourceTable.Columns.Contains(destinationColumn.ColumnName))
                    {
                        newRow[destinationColumn.ColumnName] = sourceRow[destinationColumn.ColumnName];
                    }
                }

                // 移動先テーブルに行を追加
                destinationTable.Rows.Add(newRow);

                // 元のテーブルから行を削除
                sourceTable.Rows.Remove(sourceRow);
            }
        }

        /// <summary>
        /// 指定された列から最大値を取得する汎用メソッド
        /// </summary>
        /// <param name="table">データテーブル</param>
        /// <param name="columnName">最大値を取得する列の名前</param>
        /// <typeparam name="T">列のデータ型</typeparam>
        /// <returns>最大値。データが存在しない場合はデフォルト値を返す。</returns>
        public static T GetMaxValue<T>(DataTable table, string columnName)
        {
            if (!table.Columns.Contains(columnName))
                throw new ArgumentException($"列 '{columnName}' が存在しません。");

            // 列データをフィルタして最大値を取得
            var values = table.AsEnumerable()
                              .Where(row => !row.IsNull(columnName)) // DBNullを無視
                              .Select(row => row.Field<T>(columnName));

            return values.Any() ? values.Max() : default(T); // 値がなければデフォルト値を返す
        }

        /// <summary>
        /// 指定した列から値を検索して結果を返す汎用メソッド。
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="dataTable">検索対象のデータテーブル</param>
        /// <param name="columnName">検索対象の列名</param>
        /// <param name="searchValue">検索する値</param>
        /// <returns>検索結果の値を返す。見つからない場合はデフォルト値を返す。</returns>
        public static T FindValue<T>(DataTable dataTable, string columnName, object searchValue)
        {
            if (!dataTable.Columns.Contains(columnName))
            {
                throw new ArgumentException($"列 '{columnName}' は存在しません。");
            }

            // 列の型を確認
            var columnType = dataTable.Columns[columnName].DataType;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[columnName].Equals(searchValue))
                {
                    // 見つかった行の値を指定した型に変換して返す
                    return (T)Convert.ChangeType(row[columnName], typeof(T));
                }
            }

            // 見つからない場合はデフォルト値を返す
            return default;
        }

        /// <summary>
        /// データテーブルから別のデータテーブルへ行を移動させるメソッド
        /// </summary>
        /// <param name="sourceTable">元のデータテーブル</param>
        /// <param name="destinationTable">移動先のデータテーブル</param>
        /// <param name="filterExpression">行を移動させる条件を表すフィルタ式</param>
        public static void MoveRows(DataTable sourceTable, DataTable destinationTable, string filterExpression)
        {
            // フィルタ条件に一致する行を取得
            DataRow[] rowsToMove = sourceTable.Select(filterExpression);

            foreach (DataRow row in rowsToMove)
            {
                // 移動先テーブルに新しい行を作成
                DataRow newRow = destinationTable.NewRow();

                // 元の行の値をコピー
                newRow.ItemArray = row.ItemArray;

                // 移動先テーブルに追加
                destinationTable.Rows.Add(newRow);

                // 元のテーブルから削除
                sourceTable.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 指定された条件に基づいて、片方のDataTableからもう片方にデータを移動します。
        /// </summary>
        /// <param name="sourceTable">移動元のDataTable</param>
        /// <param name="targetTable">移動先のDataTable</param>
        /// <param name="filterExpression">移動するデータを指定するフィルタ条件</param>
        /// <param name="keyColumnName">一意のキー列名</param>
        public void MoveRows(DataTable sourceTable, DataTable targetTable, string filterExpression, string keyColumnName)
        {
            // フィルタ条件に一致する行を取得
            DataRow[] rowsToMove = sourceTable.Select(filterExpression);

            foreach (DataRow row in rowsToMove)
            {
                // 移動先に同じキーの行が存在しない場合に移動
                if (!RowExistsInTable(targetTable, keyColumnName, row[keyColumnName]))
                {
                    DataRow newRow = targetTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    targetTable.Rows.Add(newRow);
                }

                // 元のDataTableから行を削除
                sourceTable.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 指定されたキー値がDataTableに存在するかを確認します。
        /// </summary>
        /// <param name="table">チェック対象のDataTable</param>
        /// <param name="keyColumnName">キー列名</param>
        /// <param name="keyValue">チェックするキー値</param>
        /// <returns>キー値が存在する場合はtrue、存在しない場合はfalse</returns>
        private bool RowExistsInTable(DataTable table, string keyColumnName, object keyValue)
        {
            return table.Select($"{keyColumnName} = '{keyValue}'").Length > 0;
        }

        /// <summary>
        /// 指定した行インデックスを移動元DataTableから移動先DataTableに移動します。
        /// </summary>
        /// <param name="sourceTable">移動元のDataTable</param>
        /// <param name="targetTable">移動先のDataTable</param>
        /// <param name="rowIndex">移動する行のインデックス</param>
        public void MoveRowByIndex(DataTable sourceTable, DataTable targetTable, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= sourceTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "指定された行インデックスが無効です。");
            }

            // 移動元から行を取得
            DataRow rowToMove = sourceTable.Rows[rowIndex];

            // 新しい行を移動先に追加
            DataRow newRow = targetTable.NewRow();
            newRow.ItemArray = rowToMove.ItemArray;
            targetTable.Rows.Add(newRow);

            // 移動元の行を削除
            sourceTable.Rows.RemoveAt(rowIndex);
        }

        /// <summary>
        /// 指定したキー値を持つ行を移動元DataTableから移動先DataTableに移動します。
        /// </summary>
        /// <param name="sourceTable">移動元のDataTable</param>
        /// <param name="targetTable">移動先のDataTable</param>
        /// <param name="keyColumnName">キー列名</param>
        /// <param name="keyValue">移動する行のキー値</param>
        public void MoveRowByKey(DataTable sourceTable, DataTable targetTable, string keyColumnName, object keyValue)
        {
            DataRow[] rowsToMove = sourceTable.Select($"{keyColumnName} = '{keyValue}'");
            if (rowsToMove.Length == 0)
            {
                throw new ArgumentException("指定されたキー値に一致する行が見つかりません。", nameof(keyValue));
            }

            DataRow rowToMove = rowsToMove[0];

            // 新しい行を移動先に追加
            DataRow newRow = targetTable.NewRow();
            newRow.ItemArray = rowToMove.ItemArray;
            targetTable.Rows.Add(newRow);

            // 元のテーブルから行を削除
            sourceTable.Rows.Remove(rowToMove);
        }
    }
}
