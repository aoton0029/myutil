using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControlSample
{
    public class DataTableDualListManager
    {
        private DataTable _table;
        private DataView _availableView;
        private DataView _selectedView;
        private DataGridView _availableGrid;
        private DataGridView _selectedGrid;

        public DataTableDualListManager(DataGridView availableGrid, DataGridView selectedGrid)
        {
            _availableGrid = availableGrid;
            _selectedGrid = selectedGrid;
        }

        public void Load(DataTable sourceTable, string idColumn, string nameColumn, List<string> preselectedIds = null)
        {
            _table = sourceTable.Copy();
            preselectedIds ??= new List<string>();

            // 列を追加（なければ）
            if (!_table.Columns.Contains("IsSelected"))
                _table.Columns.Add("IsSelected", typeof(bool));
            if (!_table.Columns.Contains("Order"))
                _table.Columns.Add("Order", typeof(int));

            foreach (DataRow row in _table.Rows)
            {
                var id = row[idColumn]?.ToString();
                row["IsSelected"] = preselectedIds.Contains(id);
            }

            // 選択済みに順序をつける
            int order = 0;
            foreach (DataRow row in _table.Select("IsSelected = true"))
            {
                row["Order"] = order++;
            }

            // DataView 作成
            _availableView = new DataView(_table, "IsSelected = false", nameColumn, DataViewRowState.CurrentRows);
            _selectedView = new DataView(_table, "IsSelected = true", "Order ASC", DataViewRowState.CurrentRows);

            // DataGridView にバインド
            _availableGrid.DataSource = _availableView;
            _selectedGrid.DataSource = _selectedView;
        }

        public void SelectCurrent()
        {
            if (_availableGrid.CurrentRow?.DataBoundItem is DataRowView row)
            {
                row["IsSelected"] = true;
                row["Order"] = _selectedView.Count;
            }
        }

        public void DeselectCurrent()
        {
            if (_selectedGrid.CurrentRow?.DataBoundItem is DataRowView row)
            {
                row["IsSelected"] = false;
                row["Order"] = DBNull.Value;
                ReorderSelected();
            }
        }

        public void MoveUp()
        {
            Move(-1);
        }

        public void MoveDown()
        {
            Move(1);
        }

        private void Move(int direction)
        {
            if (_selectedGrid.CurrentRow == null) return;

            int index = _selectedGrid.CurrentRow.Index;
            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= _selectedView.Count) return;

            var rowA = _selectedView[index];
            var rowB = _selectedView[targetIndex];

            var temp = rowA["Order"];
            rowA["Order"] = rowB["Order"];
            rowB["Order"] = temp;

            _selectedView.Sort = "Order ASC";
        }

        private void ReorderSelected()
        {
            for (int i = 0; i < _selectedView.Count; i++)
            {
                _selectedView[i]["Order"] = i;
            }
        }

        public List<string> GetSelectedIds(string idColumn)
        {
            return _table.AsEnumerable()
                .Where(r => r.Field<bool>("IsSelected"))
                .OrderBy(r => r.Field<int>("Order"))
                .Select(r => r.Field<string>(idColumn))
                .ToList();
        }
    }

    public class DataTableDualList
    {
        private readonly string _idColumn;
        private readonly string _nameColumn;
        private readonly DataTable _table;

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

        public DataView AvailableView => new DataView(_table, "IsSelected = false", _nameColumn, DataViewRowState.CurrentRows);
        public DataView SelectedView => new DataView(_table, "IsSelected = true", "Order ASC", DataViewRowState.CurrentRows);

        public void Select(string id)
        {
            var row = FindRowById(id);
            if (row != null && !(bool)row["IsSelected"])
            {
                row["IsSelected"] = true;
                row["Order"] = SelectedView.Count;
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
