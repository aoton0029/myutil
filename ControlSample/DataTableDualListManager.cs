using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControlSample
{
    public class DataTableDualList : INotifyPropertyChanged
    {
        private readonly string _idColumn;
        private readonly string _nameColumn;
        private readonly DataTable _table;
        private string _filterExpression = string.Empty;

        // プロパティ変更通知のためのイベント
        public event PropertyChangedEventHandler? PropertyChanged;

        // フィルター条件を保持するプロパティ
        public string FilterExpression
        {
            get => _filterExpression;
            set
            {
                if (_filterExpression != value)
                {
                    _filterExpression = value;
                    OnPropertyChanged(nameof(AvailableView));
                }
            }
        }

        // フィルター条件を組み合わせたRowFilterを作成
        private string GetAvailableRowFilter()
        {
            string baseFilter = "IsSelected = false";
            if (string.IsNullOrWhiteSpace(_filterExpression))
                return baseFilter;

            return $"{baseFilter} AND ({_filterExpression})";
        }

        // AvailableViewプロパティを修正してフィルターを適用
        public DataView AvailableView => new DataView(_table, GetAvailableRowFilter(), _nameColumn, DataViewRowState.CurrentRows);

        public DataView SelectedView => new DataView(_table, "IsSelected = true", "Order ASC", DataViewRowState.CurrentRows);

        public DataTableDualList(DataTable table, string idColumn, string nameColumn, IEnumerable<string>? preselectedIds = null)
        {
            _idColumn = idColumn;
            _nameColumn = nameColumn;
            _table = table;

            if (!_table.Columns.Contains("IsSelected"))
                _table.Columns.Add("IsSelected", typeof(bool));

            if (!_table.Columns.Contains("Order"))
                _table.Columns.Add("Order", typeof(int));

            // 選択状態の初期化
            preselectedIds ??= Enumerable.Empty<string>();
            int order = 0;

            foreach (DataRow row in _table.Rows)
            {
                var id = row[_idColumn]?.ToString();
                bool isSelected = preselectedIds.Contains(id);
                row["IsSelected"] = isSelected;
                row["Order"] = isSelected ? order++ : DBNull.Value;
            }
        }

        // プロパティ変更通知を発行するメソッド
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // フィルターを適用するメソッド
        public void ApplyFilter(string filterExpression)
        {
            FilterExpression = filterExpression;
        }

        // 特定の列に対するフィルターを適用するヘルパーメソッド
        public void ApplyColumnFilter(string columnName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilterExpression = string.Empty;
            }
            else
            {
                // DataViewのフィルター構文に従ってフィルタ式を作成
                FilterExpression = $"{columnName} LIKE '%{value.Replace("'", "''")}%'";
            }
        }

        public void Select(string id)
        {
            var row = FindRowById(id);
            if (row != null && !(bool)row["IsSelected"])
            {
                row["IsSelected"] = true;
                row["Order"] = SelectedView.Count;
                OnPropertyChanged(nameof(AvailableView));
                OnPropertyChanged(nameof(SelectedView));
            }
        }

        public void Deselect(string id)
        {
            var row = FindRowById(id);
            if (row != null && (bool)row["IsSelected"])
            {
                row["IsSelected"] = false;
                row["Order"] = DBNull.Value;
                Reorder();
                OnPropertyChanged(nameof(AvailableView));
                OnPropertyChanged(nameof(SelectedView));
            }
        }

        public void MoveUp(string id) => Move(id, -1);
        public void MoveDown(string id) => Move(id, 1);

        private void Move(string id, int delta)
        {
            var rows = SelectedView.Cast<DataRowView>().ToList();
            int index = rows.FindIndex(r => r[_idColumn]?.ToString() == id);
            int newIndex = index + delta;

            if (index < 0 || newIndex < 0 || newIndex >= rows.Count) return;

            var rowA = rows[index];
            var rowB = rows[newIndex];

            var tmp = rowA["Order"];
            rowA["Order"] = rowB["Order"];
            rowB["Order"] = tmp;

            SelectedView.Sort = "Order ASC";
            OnPropertyChanged(nameof(SelectedView));
        }

        public List<string> GetSelectedIdsInOrder()
        {
            return SelectedView.Cast<DataRowView>()
                .OrderBy(r => r["Order"])
                .Select(r => r[_idColumn]?.ToString() ?? "")
                .ToList();
        }

        private DataRow? FindRowById(string id)
        {
            return _table.AsEnumerable()
                .FirstOrDefault(r => r[_idColumn]?.ToString() == id);
        }

        private void Reorder()
        {
            var rows = _table.Select("IsSelected = true").OrderBy(r => r["Order"]).ToList();
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i]["Order"] = i;
            }
        }

        // オプション：保存・読み込み（選択IDのみ）
        public void SaveSelectedIds(string path)
        {
            var ids = GetSelectedIdsInOrder();
            File.WriteAllText(path, JsonSerializer.Serialize(ids));
        }

        public static List<string> LoadSelectedIds(string path)
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path)) ?? new()
                : new();
        }
    }
}
