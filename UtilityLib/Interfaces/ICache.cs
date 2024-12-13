using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Interfaces
{
    public interface ICache<TKey, TValue>
    {
        // データをキャッシュに追加または更新する
        void Set(TKey key, TValue value);

        // キャッシュからデータを取得する
        TValue Get(TKey key);

        // 指定したキーがキャッシュに存在するかを確認する
        bool ContainsKey(TKey key);

        // 指定したキーのデータをキャッシュから削除する
        void Remove(TKey key);

        // キャッシュをクリアする
        void Clear();
    }
}
