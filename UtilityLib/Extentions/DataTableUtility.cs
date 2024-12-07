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

        //public static DataTable LeftJoin(DataTable leftTable, DataTable rightTable, string leftJoinColumn, string rightJoinColumn)
        //{
        //    var resultTable = new DataTable();

        //    // 左テーブルのカラムをコピー
        //    foreach (DataColumn column in leftTable.Columns)
        //    {
        //        resultTable.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
        //    }

        //    // 右テーブルのカラムをコピー
        //    foreach (DataColumn column in rightTable.Columns)
        //    {
        //        // 重複するカラムがある場合はリネームする（オプション）
        //        if (resultTable.Columns.Contains(column.ColumnName))
        //        {
        //            resultTable.Columns.Add(new DataColumn(column.ColumnName + "_Right", column.DataType));
        //        }
        //        else
        //        {
        //            resultTable.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
        //        }
        //    }

        //    // Left Join を実行
        //    var query = from leftRow in leftTable.AsEnumerable()
        //                join rightRow in rightTable.AsEnumerable()
        //                on leftRow[leftJoinColumn] equals rightRow[rightJoinColumn] into tempJoin
        //                from tempRow in tempJoin.DefaultIfEmpty()
        //                select new
        //                {
        //                    LeftData = leftRow.ItemArray,
        //                    RightData = tempRow != null ? tempRow.ItemArray : rightTable.NewRow().ItemArray
        //                };

        //    // 結果を新しいテーブルにコピー
        //    foreach (var item in query)
        //    {
        //        var combinedRow = resultTable.NewRow();
        //        combinedRow.ItemArray = item.LeftData.Concat(item.RightData).ToArray();
        //        resultTable.Rows.Add(combinedRow);
        //    }

        //    return resultTable;
        //}

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

       

    }
}
