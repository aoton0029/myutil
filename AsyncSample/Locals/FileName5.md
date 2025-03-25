`TaskFactory.StartNew`を使用した場合のスレッド挙動は、`Task.Run`と似ていますが、特定のカスタマイズを行いたい場合に使用されます。その挙動を時系列で示し、`await`や`Wait`との違いを解説します。

---

### **1. `TaskFactory.StartNew`の特徴**

- **バックグラウンドスレッド**でタスクを実行します。
- カスタマイズ可能なオプション（例: スケジューリング、親子タスクの関係）を指定できます。
- **非推奨の場合が多い**:
  - 通常の非同期処理には`Task.Run`が推奨されます。
  - 高度な制御が必要な場合のみ`TaskFactory.StartNew`を利用します。

---

### **2. スレッドの挙動**

#### **時系列図**

```plaintext
UIスレッド              |        バックグラウンドスレッド
-----------------------|---------------------------------
処理開始               | 
   ↓                   | 
   ↓TaskFactory.StartNew |
UIスレッド解放          | ----> タスク開始
                       |       （例: 計算処理やI/O操作）
                       | ----> タスク実行中
                       | ----> タスク完了
処理再開（戻らない場合）| 
```

#### **`await`を使った場合**
- `TaskFactory.StartNew`のタスクが完了後、`await`でスレッド切り替えが発生します。
- デフォルトでは呼び出し元スレッド（UIスレッド）に戻ります。

---

### **3. `TaskFactory.StartNew`のコード例と動作**

#### **(1) 基本的な使用例**

```csharp
private async void btnStartNew_Click(object sender, EventArgs e)
{
    Log("btnStartNew_Click (Start)");

    // バックグラウンドでタスクを開始
    await Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000); // 1秒間待機
    });

    Log("btnStartNew_Click (End)");
}

private void Log(string message)
{
    string log = $"{DateTime.Now:HH:mm:ss.fff} [Thread: {Thread.CurrentThread.ManagedThreadId}] {message}";
    Console.WriteLine(log);
    listBox1.Items.Add(log);
}
```

**実行結果（UIスレッドに戻る場合）**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNew_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 1] btnStartNew_Click (End)
```

---

#### **(2) `ConfigureAwait(false)`を使用**

```csharp
private async void btnStartNewWithConfigureAwait_Click(object sender, EventArgs e)
{
    Log("btnStartNewWithConfigureAwait_Click (Start)");

    await Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000);
    }).ConfigureAwait(false);

    Log("btnStartNewWithConfigureAwait_Click (End - Not UI Thread)");
}
```

**実行結果（UIスレッドに戻らない場合）**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNewWithConfigureAwait_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 4] btnStartNewWithConfigureAwait_Click (End - Not UI Thread)
```

---

#### **(3) `Wait`を使用**

```csharp
private void btnStartNewWait_Click(object sender, EventArgs e)
{
    Log("btnStartNewWait_Click (Start)");

    Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000);
    }).Wait(); // 同期的に待機

    Log("btnStartNewWait_Click (End)");
}
```

**実行結果（UIスレッドがブロックされる）**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNewWait_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 1] btnStartNewWait_Click (End)
```

---

### **4. `TaskFactory.StartNew`の違いと注意点**

#### **(1) `Task.Run`との違い**
- `Task.Run`は`TaskFactory.StartNew`の簡略化された方法で、推奨される標準的な非同期処理の方法です。
- `TaskFactory.StartNew`は、スケジューリングオプションや親子タスクを明示的に設定したい場合に使用します。

#### **(2) UIスレッドに戻る場合の影響**
- デフォルトでは、`await`を使用するとUIスレッドに戻ります。
- UIスレッドに戻ることで、UI要素を安全に操作できます。

#### **(3) UIスレッドに戻らない場合の利点**
- `ConfigureAwait(false)`を使用するとスレッド切り替えが回避され、パフォーマンスが向上します。
- ただし、UI操作が必要な場合は例外が発生します。

#### **(4) 同期的に待機する場合のリスク**
- `Wait`を使用すると、UIスレッドがブロックされ、アプリケーションの応答性が低下します。
- 特にWindows FormsやWPFでは避けるべきです。

---

### **5. スレッド挙動の比較表**

| 機能                          | `Task.Run`                     | `TaskFactory.StartNew`         | `TaskFactory.StartNew` + `Wait` |
|-------------------------------|--------------------------------|--------------------------------|---------------------------------|
| **バックグラウンドスレッド**   | 使用                          | 使用                          | 使用                           |
| **呼び出し元スレッドに戻る**   | `await`で戻る                | `await`で戻る                | 同期的に処理                   |
| **カスタマイズ可能性**         | 低い                          | 高い（スケジューリングなど）  | 高い                           |
| **UIスレッドの解放**           | 非同期処理中解放              | 非同期処理中解放              | ブロックされる                 |

---

### **6. まとめ**

- **`Task.Run`を優先**:
  - 通常の非同期処理には`Task.Run`を使用。
  - シンプルで推奨される方法。

- **`TaskFactory.StartNew`の用途**:
  - 高度なスケジューリングやタスク階層構造を必要とする場合に限定して使用。

- **UIスレッドの安全性**:
  - `await`を使用し、UIスレッドでの処理を安全に行う。
  - UI操作が不要な場合は`ConfigureAwait(false)`を活用して効率化。

- **同期的な待機は避ける**:
  - `Wait`は応答性を低下させるため、特にUIアプリケーションでは使用を避ける。