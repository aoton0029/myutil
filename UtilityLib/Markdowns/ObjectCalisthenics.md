https://goatreview.com/object-calisthenics-9-rules-clean-code/

「Object Calisthenics」（オブジェクト・カリステニクス）は、クリーンコードを実現するための実践的なルールを提供するものです。このアプローチは Jeff Bay が提唱したもので、ソフトウェアの設計とコード品質を向上させるための9つのルールを定義しています。以下にそれぞれのルールを解説します。

---

### 1. **1メソッドにつき1つの引数を使用する**
- **概要**:
  メソッドは可能な限り引数を少なくし、1つだけにするのが理想的です。
  
- **理由**:
  - 引数が多いと、メソッドの理解が難しくなる。
  - 単一責任の原則（SRP）に従いやすくなる。
  - 引数が多い場合、オブジェクトとしてまとめることで設計が改善される可能性が高い。

- **例**:
  ```csharp
  // Bad
  void UpdateUser(string name, int age, string address) { }

  // Good
  void UpdateUser(User user) { }
  ```

---

### 2. **エンティティの状態を持たないインスタンス変数を作らない**
- **概要**:
  クラス内のすべてのインスタンス変数は、クラスの状態を表現するために必要でなければなりません。

- **理由**:
  - 必要のない変数を排除することで、クラスが持つ責務が明確になる。
  - メモリやリソースを節約できる。

- **例**:
  ```csharp
  // Bad
  class User {
      private string name;
      private int age;
      private string unusedField; // 不要なフィールド
  }

  // Good
  class User {
      private string name;
      private int age;
  }
  ```

---

### 3. **プリミティブ型や文字列型をラップする**
- **概要**:
  単純なプリミティブ型や文字列型をそのまま使わず、適切にラップすることで、型の意味を明確化します。

- **理由**:
  - 型安全性が向上する。
  - ビジネスロジックをカプセル化できる。

- **例**:
  ```csharp
  // Bad
  void PrintSalary(int salary) { }

  // Good
  class Money {
      public int Amount { get; }
      public Money(int amount) { Amount = amount; }
  }
  void PrintSalary(Money salary) { }
  ```

---

### 4. **大きなクラスを作らない**
- **概要**:
  クラスが大きくなりすぎる場合、責務を分割する必要があります。

- **理由**:
  - 単一責任の原則（SRP）を遵守するため。
  - 大きなクラスは理解しにくく、テストも困難になる。

- **例**:
  ```csharp
  // Bad
  class UserService {
      public void RegisterUser() { }
      public void UpdateUser() { }
      public void DeleteUser() { }
      public void NotifyUser() { }
  }

  // Good
  class UserRegistrationService { }
  class UserNotificationService { }
  ```

---

### 5. **1クラスにつき1レベルの抽象化を使用する**
- **概要**:
  クラス内で異なるレベルの抽象化を混在させない。

- **理由**:
  - コードが分かりやすくなる。
  - メンテナンス性が向上する。

- **例**:
  ```csharp
  // Bad
  class User {
      public void SaveToDatabase() { }
      public void ValidateInput() { }
  }

  // Good
  class User { }
  class UserRepository {
      public void Save(User user) { }
  }
  ```

---

### 6. **一度に1つのドット（.）を使用する**
- **概要**:
  メソッドチェーンや複雑なドットの連鎖を避ける。

- **理由**:
  - カプセル化が損なわれる。
  - コードが読みやすくなる。

- **例**:
  ```csharp
  // Bad
  user.GetAddress().GetCity().ToUpper();

  // Good
  var address = user.GetAddress();
  var city = address.GetCity();
  var upperCaseCity = city.ToUpper();
  ```

---

### 7. **コレクションをラップする**
- **概要**:
  コレクション（リストや配列など）を直接扱わず、専用のクラスにラップする。

- **理由**:
  - コレクションの操作がより意図的に行える。
  - カプセル化と型安全性が向上する。

- **例**:
  ```csharp
  // Bad
  List<string> users = new List<string>();

  // Good
  class UserCollection {
      private List<string> users = new List<string>();
      public void AddUser(string user) { users.Add(user); }
  }
  ```

---

### 8. **スタンダードライブラリの使用を最小限にする**
- **概要**:
  標準ライブラリの直接使用を避け、ラップすることで柔軟性を持たせます。

- **理由**:
  - ライブラリの変更に柔軟に対応できる。
  - コードの意図がより明確になる。

- **例**:
  ```csharp
  // Bad
  DateTime now = DateTime.Now;

  // Good
  class Clock {
      public DateTime Now => DateTime.Now;
  }
  ```

---

### 9. **継承ではなく委譲を使用する**
- **概要**:
  継承を安易に使用せず、必要に応じて委譲を使う。

- **理由**:
  - 継承による設計の固定化を避けられる。
  - 柔軟な設計が可能になる。

- **例**:
  ```csharp
  // Bad
  class UserList : List<string> { }

  // Good
  class UserList {
      private List<string> users = new List<string>();
      public void Add(string user) { users.Add(user); }
  }
  ```

---

### まとめ
「Object Calisthenics」の9つのルールは、コードの読みやすさ、保守性、再利用性を高めるために設計されています。これらを実践することで、よりクリーンで拡張性の高いコードを実現できます。最初はルールをすべて守るのが難しいかもしれませんが、意識的に取り組むことで自然と良い設計が身につきます。