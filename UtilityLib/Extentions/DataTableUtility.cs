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

        /// <summary>
        /// DataTableにクエリを実行する汎用メソッド
        /// </summary>
        /// <param name="table">対象のDataTable</param>
        /// <param name="filterExpression">フィルタリング条件（例: "Age > 30 AND Name = 'Alice'"）</param>
        /// <param name="sortExpression">並び替え条件（例: "Age DESC, Name ASC"）</param>
        /// <returns>クエリ結果を含むDataTable</returns>
        public static DataTable ExecuteQuery(
            DataTable table,
            string filterExpression = null,
            string sortExpression = null)
        {
            // フィルタリングと並び替えを適用
            DataRow[] filteredRows = string.IsNullOrEmpty(filterExpression)
                ? table.Select()
                : table.Select(filterExpression);

            // フィルタ結果を元に新しいDataTableを作成
            DataTable result = table.Clone();
            foreach (var row in filteredRows)
            {
                result.ImportRow(row);
            }

            // 並び替えを適用
            if (!string.IsNullOrEmpty(sortExpression))
            {
                result = result.AsEnumerable()
                    .OrderBy(row => 0) // 初期化用ダミー
                    .OrderByDynamic(sortExpression)
                    .CopyToDataTable();
            }

            return result;
        }

        /// <summary>
        /// 動的なOrderByを実現する拡張メソッド
        /// </summary>
        public static IOrderedEnumerable<DataRow> OrderByDynamic(
            this IEnumerable<DataRow> source,
            string sortExpression)
        {
            var sortColumns = sortExpression.Split(',').Select(s => s.Trim()).ToArray();

            IOrderedEnumerable<DataRow> orderedRows = null;

            foreach (var sortColumn in sortColumns)
            {
                var parts = sortColumn.Split(' ');
                var columnName = parts[0];
                var direction = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : "ASC";

                orderedRows = orderedRows == null
                    ? (direction == "DESC"
                        ? source.OrderByDescending(row => row[columnName])
                        : source.OrderBy(row => row[columnName]))
                    : (direction == "DESC"
                        ? orderedRows.ThenByDescending(row => row[columnName])
                        : orderedRows.ThenBy(row => row[columnName]));
            }

            return orderedRows;
        }

    }
}
