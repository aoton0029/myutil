https://goatreview.com/object-calisthenics-9-rules-clean-code/

�uObject Calisthenics�v�i�I�u�W�F�N�g�E�J���X�e�j�N�X�j�́A�N���[���R�[�h���������邽�߂̎��H�I�ȃ��[����񋟂�����̂ł��B���̃A�v���[�`�� Jeff Bay ���񏥂������̂ŁA�\�t�g�E�F�A�̐݌v�ƃR�[�h�i�������コ���邽�߂�9�̃��[�����`���Ă��܂��B�ȉ��ɂ��ꂼ��̃��[����������܂��B

---

### 1. **1���\�b�h�ɂ�1�̈������g�p����**
- **�T�v**:
  ���\�b�h�͉\�Ȍ�����������Ȃ����A1�����ɂ���̂����z�I�ł��B
  
- **���R**:
  - �����������ƁA���\�b�h�̗���������Ȃ�B
  - �P��ӔC�̌����iSRP�j�ɏ]���₷���Ȃ�B
  - �����������ꍇ�A�I�u�W�F�N�g�Ƃ��Ă܂Ƃ߂邱�ƂŐ݌v�����P�����\���������B

- **��**:
  ```csharp
  // Bad
  void UpdateUser(string name, int age, string address) { }

  // Good
  void UpdateUser(User user) { }
  ```

---

### 2. **�G���e�B�e�B�̏�Ԃ������Ȃ��C���X�^���X�ϐ������Ȃ�**
- **�T�v**:
  �N���X���̂��ׂẴC���X�^���X�ϐ��́A�N���X�̏�Ԃ�\�����邽�߂ɕK�v�łȂ���΂Ȃ�܂���B

- **���R**:
  - �K�v�̂Ȃ��ϐ���r�����邱�ƂŁA�N���X�����Ӗ������m�ɂȂ�B
  - �������⃊�\�[�X��ߖ�ł���B

- **��**:
  ```csharp
  // Bad
  class User {
      private string name;
      private int age;
      private string unusedField; // �s�v�ȃt�B�[���h
  }

  // Good
  class User {
      private string name;
      private int age;
  }
  ```

---

### 3. **�v���~�e�B�u�^�╶����^�����b�v����**
- **�T�v**:
  �P���ȃv���~�e�B�u�^�╶����^�����̂܂܎g�킸�A�K�؂Ƀ��b�v���邱�ƂŁA�^�̈Ӗ��𖾊m�����܂��B

- **���R**:
  - �^���S�������シ��B
  - �r�W�l�X���W�b�N���J�v�Z�����ł���B

- **��**:
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

### 4. **�傫�ȃN���X�����Ȃ�**
- **�T�v**:
  �N���X���傫���Ȃ肷����ꍇ�A�Ӗ��𕪊�����K�v������܂��B

- **���R**:
  - �P��ӔC�̌����iSRP�j�����炷�邽�߁B
  - �傫�ȃN���X�͗������ɂ����A�e�X�g������ɂȂ�B

- **��**:
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

### 5. **1�N���X�ɂ�1���x���̒��ۉ����g�p����**
- **�T�v**:
  �N���X���ňقȂ郌�x���̒��ۉ������݂����Ȃ��B

- **���R**:
  - �R�[�h��������₷���Ȃ�B
  - �����e�i���X�������シ��B

- **��**:
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

### 6. **��x��1�̃h�b�g�i.�j���g�p����**
- **�T�v**:
  ���\�b�h�`�F�[���╡�G�ȃh�b�g�̘A���������B

- **���R**:
  - �J�v�Z���������Ȃ���B
  - �R�[�h���ǂ݂₷���Ȃ�B

- **��**:
  ```csharp
  // Bad
  user.GetAddress().GetCity().ToUpper();

  // Good
  var address = user.GetAddress();
  var city = address.GetCity();
  var upperCaseCity = city.ToUpper();
  ```

---

### 7. **�R���N�V���������b�v����**
- **�T�v**:
  �R���N�V�����i���X�g��z��Ȃǁj�𒼐ڈ��킸�A��p�̃N���X�Ƀ��b�v����B

- **���R**:
  - �R���N�V�����̑��삪���Ӑ}�I�ɍs����B
  - �J�v�Z�����ƌ^���S�������シ��B

- **��**:
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

### 8. **�X�^���_�[�h���C�u�����̎g�p���ŏ����ɂ���**
- **�T�v**:
  �W�����C�u�����̒��ڎg�p������A���b�v���邱�Ƃŏ_����������܂��B

- **���R**:
  - ���C�u�����̕ύX�ɏ_��ɑΉ��ł���B
  - �R�[�h�̈Ӑ}����薾�m�ɂȂ�B

- **��**:
  ```csharp
  // Bad
  DateTime now = DateTime.Now;

  // Good
  class Clock {
      public DateTime Now => DateTime.Now;
  }
  ```

---

### 9. **�p���ł͂Ȃ��Ϗ����g�p����**
- **�T�v**:
  �p�������ՂɎg�p�����A�K�v�ɉ����ĈϏ����g���B

- **���R**:
  - �p���ɂ��݌v�̌Œ艻���������B
  - �_��Ȑ݌v���\�ɂȂ�B

- **��**:
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

### �܂Ƃ�
�uObject Calisthenics�v��9�̃��[���́A�R�[�h�̓ǂ݂₷���A�ێ琫�A�ė��p�������߂邽�߂ɐ݌v����Ă��܂��B���������H���邱�ƂŁA���N���[���Ŋg�����̍����R�[�h�������ł��܂��B�ŏ��̓��[�������ׂĎ��̂������������܂��񂪁A�ӎ��I�Ɏ��g�ނ��ƂŎ��R�Ɨǂ��݌v���g�ɂ��܂��B