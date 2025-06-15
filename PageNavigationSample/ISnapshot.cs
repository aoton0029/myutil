using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    /// <summary>
    /// スナップショットを作成・復元するためのインターフェース
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// 現在の状態のスナップショットを作成
        /// </summary>
        object CreateSnapshot();

        /// <summary>
        /// スナップショットから状態を復元
        /// </summary>
        void RestoreFromData(object data);
    }

    /// <summary>
    /// スナップショット管理クラス
    /// </summary>
    public class SnapshotManager
    {
        private Stack<object> snapshots = new Stack<object>();
        private readonly ISnapshot _target;

        public SnapshotManager(ISnapshot target)
        {
            this._target = target;
        }

        /// <summary>
        /// スナップショットを保存
        /// </summary>
        public void SaveSnapshot()
        {
            var snapshot = _target.CreateSnapshot();
            snapshots.Push(snapshot);
        }

        public object RestoreFromSnapshot()
        {
            if (snapshots.Count == 0)
            {
                throw new InvalidOperationException("No snapshots available to restore.");
            }
            var snapshot = snapshots.Pop();
            _target.RestoreFromData(snapshot);
            return snapshot;
        }

        public void ClearSnapshots()
        {
            snapshots.Clear();
        }
    }
}
