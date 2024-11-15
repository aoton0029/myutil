using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteLib
{
    public abstract class BaseDbContext : DbContext
    {
        private readonly string _databaseFileName;

        protected BaseDbContext(string databaseFileName)
        {
            if (string.IsNullOrWhiteSpace(databaseFileName))
                throw new ArgumentException("Database file name must not be empty.", nameof(databaseFileName));

            _databaseFileName = databaseFileName;
        }

        // データベース設定用のバーチャルメソッド
        protected virtual void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databaseFileName}");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            ConfigureDatabase(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            CustomizeModel(modelBuilder);
        }

        protected virtual void CustomizeModel(ModelBuilder modelBuilder)
        {
            // 継承先で必要に応じて実装
        }

        // SQLを直接実行するメソッド
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public int ExecuteSql(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlRaw(sql, parameters);
        }

        // SQLクエリでデータを取得するメソッド
        public async Task<List<T>> QuerySqlAsync<T>(string sql, params object[] parameters) where T : class
        {
            return await Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }

        public List<T> QuerySql<T>(string sql, params object[] parameters) where T : class
        {
            return Set<T>().FromSqlRaw(sql, parameters).ToList();
        }
    }
}
