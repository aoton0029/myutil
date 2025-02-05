using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Query.Searchs
{
    public class DatabaseSearchService : ISearchService
    {
        private readonly string _connectionString;

        public DatabaseSearchService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable Search(SearchCriteria criteria)
        {
            DataTable resultTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    // SQLクエリの基本形（必要に応じて変更）
                    string sql = "SELECT * FROM Articles WHERE 1=1";

                    if (!string.IsNullOrEmpty(criteria.Keyword))
                    {
                        sql += " AND (Title LIKE @Keyword OR Content LIKE @Keyword)";
                        cmd.Parameters.AddWithValue("@Keyword", $"%{criteria.Keyword}%");
                    }

                    if (criteria.StartDate.HasValue)
                    {
                        sql += " AND PublishedDate >= @StartDate";
                        cmd.Parameters.AddWithValue("@StartDate", criteria.StartDate.Value);
                    }

                    if (criteria.EndDate.HasValue)
                    {
                        sql += " AND PublishedDate <= @EndDate";
                        cmd.Parameters.AddWithValue("@EndDate", criteria.EndDate.Value);
                    }

                    cmd.CommandText = sql;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultTable);
                    }
                }
            }

            return resultTable;
        }
    }
}
