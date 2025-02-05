C#で検索を操作するクラスを抽象化し、検索条件も抽象化する方法を提案します。
この設計では、以下のような要素を含みます：

1. 検索条件の抽象化 (ISearchCriteria)

検索条件を共通のインターフェースとして定義し、異なる条件を扱えるようにする。



2. 検索処理の抽象化 (ISearchService)

検索の実装を統一し、データベースへの問い合わせを抽象化する。



3. SQLの実行 (SqlSearchService)

実際のデータベースとやり取りし、DataTable を取得する実装。





---

1. 検索条件のインターフェース

public interface ISearchCriteria
{
    string ToSqlWhereClause();
    Dictionary<string, object> GetParameters();
}

ToSqlWhereClause()：SQLのWHERE句を組み立てる

GetParameters()：SQLパラメータを辞書型で取得する


具体的な検索条件

public class NameSearchCriteria : ISearchCriteria
{
    public string Name { get; set; }

    public NameSearchCriteria(string name)
    {
        Name = name;
    }

    public string ToSqlWhereClause()
    {
        return "Name LIKE @Name";
    }

    public Dictionary<string, object> GetParameters()
    {
        return new Dictionary<string, object>
        {
            { "@Name", "%" + Name + "%" }
        };
    }
}


---

2. 検索サービスのインターフェース

using System.Data;

public interface ISearchService
{
    DataTable Search(ISearchCriteria criteria);
}


---

3. データベース検索の実装

using System;
using System.Data;
using System.Data.SqlClient;

public class SqlSearchService : ISearchService
{
    private readonly string _connectionString;

    public SqlSearchService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable Search(ISearchCriteria criteria)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM Users WHERE " + criteria.ToSqlWhereClause();

            foreach (var param in criteria.GetParameters())
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }

            var adapter = new SqlDataAdapter(command);
            var resultTable = new DataTable();
            adapter.Fill(resultTable);

            return resultTable;
        }
    }
}


---

4. 使用例

class Program
{
    static void Main()
    {
        string connectionString = "your_connection_string_here";
        ISearchService searchService = new SqlSearchService(connectionString);

        // 名前で検索
        ISearchCriteria criteria = new NameSearchCriteria("John");
        DataTable results = searchService.Search(criteria);

        // 結果の表示
        foreach (DataRow row in results.Rows)
        {
            Console.WriteLine($"ID: {row["ID"]}, Name: {row["Name"]}");
        }
    }
}


---

拡張性

検索条件を増やす

例えば、AgeSearchCriteria や EmailSearchCriteria を追加可能。


異なるデータソースへの対応

ISearchService を実装すれば、SQLiteやMongoDBでも適用可能。



この設計により、検索ロジックを柔軟に拡張できるようになります。

