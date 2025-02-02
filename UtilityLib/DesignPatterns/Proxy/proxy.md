## Proxyパターンとは？
**Proxyパターン**（プロキシパターン）は、**あるオブジェクトへのアクセスを制御するためのデザインパターン**です。実際のオブジェクト（**Real Subject**）の代わりに**Proxy（代理）**オブジェクトを提供し、**アクセス制御、キャッシュ、遅延初期化（Lazy Initialization）**などを行うことができます。

---

## Proxyパターンの種類
### 1. **Virtual Proxy（仮想プロキシ）**
   - リソースが重いオブジェクトの**遅延ロード（Lazy Loading）**を実現する。
   - 必要になるまで実際のオブジェクトを作成しない。

### 2. **Protection Proxy（保護プロキシ）**
   - **アクセス制御**を行い、特定のユーザーのみが対象オブジェクトにアクセスできるようにする。

### 3. **Remote Proxy（リモートプロキシ）**
   - ネットワーク越しにオブジェクトを操作する（例：Web API の呼び出し）。

### 4. **Cache Proxy（キャッシュプロキシ）**
   - 一度取得したデータを**キャッシュ**し、後のアクセスを高速化する。

---

## C#での実装例
### **1. Virtual Proxy（仮想プロキシ）の例**
画像のロード処理を遅延するプロキシを実装します。

```csharp
using System;

public interface IImage
{
    void Display();
}

// 実際の画像クラス（Real Subject）
public class RealImage : IImage
{
    private string _filename;

    public RealImage(string filename)
    {
        _filename = filename;
        LoadImageFromDisk(); // コンストラクタで画像をロード
    }

    private void LoadImageFromDisk()
    {
        Console.WriteLine($"Loading image: {_filename}");
    }

    public void Display()
    {
        Console.WriteLine($"Displaying image: {_filename}");
    }
}

// プロキシクラス
public class ProxyImage : IImage
{
    private RealImage _realImage;
    private string _filename;

    public ProxyImage(string filename)
    {
        _filename = filename;
    }

    public void Display()
    {
        // 初回アクセス時のみ実際の画像をロード
        if (_realImage == null)
        {
            _realImage = new RealImage(_filename);
        }
        _realImage.Display();
    }
}

// 使用例
class Program
{
    static void Main()
    {
        IImage image1 = new ProxyImage("photo1.jpg");
        IImage image2 = new ProxyImage("photo2.jpg");

        // 画像は初回アクセス時にロードされる
        image1.Display();
        Console.WriteLine("----------------");
        image1.Display(); // 2回目以降はロードしない
    }
}
```
#### **実行結果**
```
Loading image: photo1.jpg
Displaying image: photo1.jpg
----------------
Displaying image: photo1.jpg
```
**ポイント**
- `ProxyImage` は `RealImage` の代わりに動作し、**実際に必要になるまで `RealImage` を作成しない**。
- **メモリ節約**や**パフォーマンス向上**に役立つ。

---

### **2. Protection Proxy（保護プロキシ）の例**
管理者権限のあるユーザーのみがデータにアクセスできるようにする。

```csharp
using System;

// 共有インターフェース
public interface IDataAccess
{
    void AccessData();
}

// 実際のデータアクセスクラス
public class RealDataAccess : IDataAccess
{
    public void AccessData()
    {
        Console.WriteLine("Accessing sensitive data...");
    }
}

// プロキシクラス（アクセス制御）
public class ProtectionProxy : IDataAccess
{
    private RealDataAccess _realDataAccess;
    private string _userRole;

    public ProtectionProxy(string userRole)
    {
        _userRole = userRole;
    }

    public void AccessData()
    {
        if (_userRole == "Admin")
        {
            if (_realDataAccess == null)
                _realDataAccess = new RealDataAccess();
            
            _realDataAccess.AccessData();
        }
        else
        {
            Console.WriteLine("Access Denied: You do not have permission.");
        }
    }
}

// 使用例
class Program
{
    static void Main()
    {
        IDataAccess adminAccess = new ProtectionProxy("Admin");
        IDataAccess userAccess = new ProtectionProxy("User");

        adminAccess.AccessData(); // 許可される
        userAccess.AccessData();  // 拒否される
    }
}
```
#### **実行結果**
```
Accessing sensitive data...
Access Denied: You do not have permission.
```
**ポイント**
- `ProtectionProxy` が**アクセス制御**を行い、管理者 (`"Admin"`) のみ `RealDataAccess` にアクセスできる。

---

### **3. Cache Proxy（キャッシュプロキシ）の例**
データベースなどの**重い処理の結果をキャッシュ**する。

```csharp
using System;
using System.Collections.Generic;

// インターフェース
public interface IDataFetcher
{
    string GetData(int id);
}

// 実際のデータ取得クラス
public class RealDataFetcher : IDataFetcher
{
    public string GetData(int id)
    {
        Console.WriteLine($"Fetching data for ID: {id} from database...");
        return $"Data for {id}";
    }
}

// キャッシュプロキシ
public class CacheProxy : IDataFetcher
{
    private RealDataFetcher _realDataFetcher;
    private Dictionary<int, string> _cache = new Dictionary<int, string>();

    public CacheProxy()
    {
        _realDataFetcher = new RealDataFetcher();
    }

    public string GetData(int id)
    {
        if (_cache.ContainsKey(id))
        {
            Console.WriteLine($"Returning cached data for ID: {id}");
            return _cache[id];
        }

        string data = _realDataFetcher.GetData(id);
        _cache[id] = data;
        return data;
    }
}

// 使用例
class Program
{
    static void Main()
    {
        IDataFetcher dataFetcher = new CacheProxy();

        Console.WriteLine(dataFetcher.GetData(1)); // DBアクセス
        Console.WriteLine(dataFetcher.GetData(1)); // キャッシュから取得
        Console.WriteLine(dataFetcher.GetData(2)); // DBアクセス
    }
}
```
#### **実行結果**
```
Fetching data for ID: 1 from database...
Data for 1
Returning cached data for ID: 1
Data for 1
Fetching data for ID: 2 from database...
Data for 2
```
**ポイント**
- **データを一度取得したらキャッシュ**し、次回以降はデータベースに問い合わせない。
- **パフォーマンス向上**と**負荷軽減**を実現。

---

## まとめ
| 種類               | 目的                                     | 例 |
|-------------------|--------------------------------|---|
| Virtual Proxy    | 遅延初期化（Lazy Loading）     | 画像の遅延ロード |
| Protection Proxy | アクセス制御                   | ユーザー認証 |
| Remote Proxy     | リモートオブジェクトの代理     | API や RPC |
| Cache Proxy      | データのキャッシュ            | DB クエリキャッシュ |

Proxyパターンを使うことで、パフォーマンスの最適化やセキュリティ強化が可能になります。C#のアプリケーションで適切に活用すると、**柔軟で拡張性のある設計**が実現できます！ 🚀