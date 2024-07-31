using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.TreeNodes
{
    internal class Treenode_3
    {
        /// <summary>要素タイプ列挙</summary>
        public enum NODE_TYPE
        {
            /// <summary>格納値を取得・設定できない</summary>
            NO_HAVE_DATA,

            /// <summary>格納値が一つのデータ</summary>
            HAVE_DATA,
        };

        /// <summary>木構造のデータ構造を提供</summary>
        public class TreeData : ICloneable
        {
            /// <summary>子要素リスト</summary>
            private List<TreeData> m_childeList;

            /// <summary>この要素の親要素</summary>
            private TreeData m_parentNode;

            /// <summary>この要素の格納値</summary>
            private Object m_value;

            /// <summary>要素パスの区切り文字</summary>
            static char s_pathSeparator = '/';

            /// <summary>要素名の大文字・小文字の区別</summary>
            static bool s_ignoreCase = true;

            /// <summary>要素名を指定して，インスタンスを生成</summary>
            /// 
            /// <param name="name">
            /// 要素名</param>
            /// 
            /// <exception cref="System.ArgumentNullException">
            /// name が null</exception>
            /// <exception cref="System.ArgumentException">
            /// name が空文字</exception>
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名に区切り文字を使用しないこと</exception>
            public TreeData(String name)
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name が null です．");
                }
                //if (name.IsEmptyOrSpace())
                //{
                //    throw new ArgumentException("name が空文字です．");
                //}
                //if (name.Contains(TreeData.PathSeparator.ToString()))
                //{
                //    throw new TreeDataException("要素名に区切り文字を使用しないこと");
                //}

                this.Name = name;
                this.m_parentNode = null;
                this.m_childeList = new List<TreeData>();
                this.m_value = null;
                this.NodeType = NODE_TYPE.NO_HAVE_DATA;
            }

            /// <summary>要素名・要素タイプを指定して，インスタンスを生成</summary>
            /// 
            /// <param name="name">
            /// 要素名</param>
            /// <param name="type">
            /// 要素タイプ</param>
            /// 
            /// <exception cref="System.ArgumentNullException">
            /// name が null</exception>
            /// <exception cref="System.ArgumentException">
            /// name が空文字</exception>
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名に区切り文字を使用しないこと</exception>
            public TreeData(String name, NODE_TYPE type)
                : this(name)
            {
                this.NodeType = type;
            }

            /// <summary>要素名・要素タイプ・格納データを指定して，インスタンスを生成</summary>
            /// 
            /// <param name="name">
            /// 要素名</param>
            /// <param name="type">
            /// 要素タイプ</param>
            /// <param name="data">
            /// 格納データ</param>
            /// 
            /// <exception cref="System.ArgumentNullException">
            /// name が null</exception>
            /// <exception cref="System.ArgumentException">
            /// name が空文字</exception>
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名に区切り文字を使用しないこと</exception>
            public TreeData(String name, NODE_TYPE type, Object data)
                : this(name, type)
            {
                this.m_value = data;
            }

            /// <summary>コピーとなるインスタンスを生成</summary>
            /// 
            /// <param name="name">
            /// 要素データ</param>
            protected TreeData(TreeData other)
            {
                this.Name = other.Name;
                this.m_parentNode = null;
                this.m_childeList = new List<TreeData>();
                this.m_value = other.m_value;
                this.NodeType = other.NodeType;
            }

            /// <summary>親要素を取得する</summary>
            public TreeData Parent
            {
                get { return this.m_parentNode; }
            }

            /// <summary>この要素が Root かどうかを表す</summary>
            public bool IsRoot
            {
                get { return (this.Parent == null); }
            }

            /// <summary>子要素を所有しているかを表す</summary>
            public bool HasChild
            {
                get { return (this.CountMyChild != 0); }
            }

            /// <summary>要素名を取得・設定する</summary>
            public String Name { get; set; }

            /// <summary>格納オブジェクトを取得・設定する</summary>
            /// 
            /// <exception cref="TreeNode.TreeDataException">
            /// この要素は格納値を持たないタイプ</exception>
            public Object Value
            {
                get
                {
                    if (this.NodeType == NODE_TYPE.NO_HAVE_DATA)
                    {
                        //throw new TreeDataException("この要素は格納値を持たないタイプ");
                    }
                    return this.m_value;
                }
                set
                {
                    if (this.NodeType == NODE_TYPE.NO_HAVE_DATA)
                    {
                        //throw new TreeDataException("この要素は格納値を持たないタイプ");
                    }
                    this.m_value = value;
                }
            }

            /// <summary>要素パスの区切り文字を取得・設定する</summary>
            public static char PathSeparator
            {
                get { return s_pathSeparator; }
                set { s_pathSeparator = value; }
            }

            /// <summary>要素名の大文字・小文字を区別するかどうかを表す</summary>
            /// 
            /// <remarks>
            /// この設定は要素名の検索時にのみ使用。false 時は区別をしない
            /// デフォルト値は false</remarks>
            public static bool IgnoreNameCase
            {
                get { return s_ignoreCase; }
                set { s_ignoreCase = value; }
            }

            /// <summary>子要素の List を取得する</summary>
            public List<TreeData> GetChildList
            {
                get { return this.m_childeList; }
            }

            /// <summary>Root からの深度を取得する</summary>
            public int GetDepth
            {
                get { return __GetDepth(this); }
            }

            /// <summary>この要素までのパスを取得する</summary>
            public String GetPath
            {
                get
                {
                    String path = "";
                    __GetPath(this, ref path);
                    return path;
                }
            }

            /// <summary>子要素の数を取得する</summary>
            public int CountMyChild
            {
                get { return this.m_childeList.Count; }
            }

            /// <summary>この要素より下の階層いる子要素の数を取得する</summary>
            public int CountAllChild
            {
                get
                {
                    int counter = -1;        // 自分自身を数に入れないため -1
                    __GetChildNodeCount(this, ref counter);
                    return counter;
                }
            }

            /// <summary>指定した要素名を持つ要素を取得する</summary>
            /// 
            /// <param name="name">
            /// 要素名</param>
            /// 
            /// <returns>指定した要素名を持つ要素</returns>
            public TreeData this[String name]
            {
                get { return __SearchChildByName(this, name); }
            }

            /// <summary>要素タイプを指定する</summary>
            /// 
            /// <seealso cref="TreeNode.NODE_TYPE"/>
            public NODE_TYPE NodeType { get; set; }

            /// <summary>指定する子要素を追加する</summary>
            /// 
            /// <param name="data">
            /// この要素直下に追加するオブジェクト</param>
            /// 
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名が重複</exception>
            public void AddChild(TreeData data)
            {
                __AddChild(this, data);
            }

            /// <summary>指定する子要素を追加する</summary>
            /// 
            /// <param name="name">
            /// この要素直下に追加する要素名</param>
            /// <param name="value">
            /// 格納オブジェクト（デフォルト null）</param>
            /// 
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名が重複</exception>
            public void AddChild(String name, NODE_TYPE type, Object value = null)
            {
                __AddChild(this, name, type, value);
            }

            /// <summary>要素パスを指定し，子要素を追加する</summary>
            /// 
            /// <param name="path">
            /// この要素を起点とする要素パス</param>
            /// <param name="type">
            /// 子要素の要素タイプ</param>
            /// <param name="value">
            /// 格納オブジェクト（デフォルト null）</param>
            /// 
            /// <exception cref="TreeNode.TreeDataException">
            /// 要素名が重複</exception>
            public void AddChildByPath(String path, NODE_TYPE type, Object value = null)
            {
                __AddChildByPath(this, path, type, value);
            }

            /// <summary>すべての子要素を削除する</summary>
            public void Clear()
            {
                __Clear(this);
            }

            /// <summary>指定するパスに該当する要素を削除する</summary>
            /// 
            /// <param name="path">
            /// この要素を起点とする要素パス</param>
            public void RemoveChild(String path)
            {
                __RemoveChild(this, path);
            }

            /// <summary>指定したパスの格納オブジェクトを取得する</summary>
            /// 
            /// <param name="path">
            /// この要素からの要素パス</param>
            /// 
            /// <returns>格納オブジェクト</returns>
            /// 
            /// <exception cref="TreeNode.TreeDataException">
            /// この要素は格納値を持たないタイプ</exception>
            public Object GetValue(String path)
            {
                return __GetValue(this, path);
            }

            /// <summary>指定したパスの格納オブジェクトを設定する</summary>
            /// 
            /// <param name="path">
            /// この要素からの要素パス</param>
            /// <param name="value">
            /// 格納オブジェクト</param>
            ///
            /// <exception cref="TreeNode.TreeDataException">
            /// この要素は格納値を持たないタイプ</exception>
            public void SetValue(String path, Object value)
            {
                __SetValue(this, path, value);
            }

            /// <summary>指定した要素パスの要素を取得する</summary>
            /// 
            /// <param name="path">
            /// 要素パス</param>
            /// 
            /// <returns>要素データ</returns>
            public TreeData GetChild(String path)
            {
                return __GetChild(this, path);
            }

            /// <summary>オブジェクトをコピーする</summary>
            /// 
            /// <returns>オブジェクトのコピー</returns>
            public Object Clone()
            {
                return __Clone(this);
            }

            /// <summary>指定したパスに要素があるか検索する</summary>
            /// 
            /// <param name="path">
            /// 要素パス</param>
            /// 
            /// <returns>検索要素があった時 true</returns>
            public bool FindChild(String path)
            {
                return __FindChild(this, path);
            }

            /// <summary>指定した要素より下位の階層にあるすべての子要素の数を取得する</summary>
            /// 
            /// <param name="node">
            /// カウントを開始する要素</param>
            /// <param name="count">
            /// カウント数が格納される．カウント数に指定した要素も含まれる</param>
            private static void __GetChildNodeCount(TreeData node, ref int count)
            {
                count++;

                if (node.HasChild)
                {
                    foreach (TreeData child in node.GetChildList)
                    {
                        __GetChildNodeCount(child, ref count);
                    }
                }
            }

            /// <summary>指定した要素パスに要素があるか検索し，その要素を返す</summary>
            /// 
            /// <param name="parent">
            /// 検索する親要素</param>
            /// <param name="path">
            /// 検索する要素パス</param>
            /// 
            /// <returns>パスに要素がある場合は参照値，ない場合は null を返す</returns>
            /// 
            /// <exception cref="System.ArgumentNullException">
            /// path が null</exception>
            /// <exception cref="System.ArgumentException">
            /// path が空文字</exception>
            private static TreeData __SearchChildByPath(TreeData parent, String path)
            {
                String buff;
                return __SearchChildByPath(parent, path, out buff);
            }

            /// <summary>指定した要素パスに要素があるか検索し，その要素を返す</summary>
            /// 
            /// <remarks>
            /// 検索に引っかからなかった場合，戻り値に null，
            /// notChildName に該当しなかった要素名を設定する</remarks>
            /// 
            /// <param name="parent">
            /// 検索する親要素</param>
            /// <param name="path">
            /// 検索する要素パス</param>
            /// <param name="notChildName">
            /// 検索に該当しない要素名</param>
            /// 
            /// <returns>パスに要素がある場合は参照値，ない場合は null を返す</returns>
            /// 
            /// <exception cref="System.ArgumentNullException">
            /// path が null</exception>
            /// <exception cref="System.ArgumentException">
            /// path が空文字</exception>
            private static TreeData __SearchChildByPath(TreeData parent, String path, out String notChildName)
            {
                if (path == null)
                {
                    throw new ArgumentNullException("path が null");
                }
                //if (path.IsEmptyOrSpace())
                //{
                //    throw new ArgumentException("path が空文字");
                //}

                String[] pathParam = path.Split(TreeData.PathSeparator);
                notChildName = String.Empty;

                TreeData node = parent;
                for (int index = 0; index < pathParam.Length; index++)
                {
                    String search = pathParam[index];
                    //if (search.IsEmptyOrSpace()) { continue; }

                    node = node[search];
                    if (node == null)
                    {
                        notChildName = search;
                        break;
                    }
                }
                return node;

            }

            /// <summary>指定した名前の要素があるか検索し，その要素を返す</summary>
            /// 
            /// <param name="parent">
            /// 検索する親要素</param>
            /// <param name="name">
            /// 検索する要素名</param>
            /// 
            /// <returns>該当する要素がある場合は参照値，ない場合は null を返す</returns>
            private static TreeData __SearchChildByName(TreeData parent, String name)
            {
                foreach (TreeData child in parent.GetChildList)
                {
                    if (String.Compare(child.Name, name, IgnoreNameCase) == 0)
                    {
                        return child;
                    }
                }
                return null;
            }

            /// <summary>指定した要素に関連するすべての要素を表示する</summary>
            /// 
            /// <param name="node">
            /// 表示する要素</param>
            public static void PrintAllNode(TreeData node)
            {
                String msg;
                if (node.NodeType == NODE_TYPE.NO_HAVE_DATA)
                {
                    msg = String.Format("Name:{0},", node.Name);
                }
                else
                {
                    String valueStr = (node.Value == null) ? "null" : node.Value.ToString();

                    msg = String.Format("Name:{0}, Value:{1}", node.Name, valueStr);
                }

                int strWidth = 4 * __GetDepth(node) + msg.Length;
                Debug.WriteLine(msg.PadLeft(strWidth));

                if (node.HasChild)
                {
                    foreach (TreeData child in node.GetChildList)
                    {
                        TreeData.PrintAllNode(child);
                    }
                }
            }

            /// <summary>親要素に指定する子要素を追加する</summary>
            private static void __AddChild(TreeData parent, TreeData child)
            {
                if (parent[child.Name] != null)
                {
                    //throw new TreeDataException("要素名が重複");
                }

                child.m_parentNode = parent;
                parent.m_childeList.Add(child);
            }

            /// <summary>親要素に指定する子要素を追加する</summary>
            private static void __AddChild(TreeData parent, String name, NODE_TYPE childNodeType, Object value = null)
            {
                TreeData newNode = new TreeData(name, childNodeType, value);
                __AddChild(parent, newNode);
            }

            /// <summary>親要素からの要素パスを指定し，子要素を追加する</summary>
            private static void __AddChildByPath(TreeData parent, String path, NODE_TYPE type, Object value)
            {
                String[] childParam = path.Split(TreeData.PathSeparator);
                int last = childParam.Length - 1;

                TreeData node = parent;

                for (uint index = 0; index < last; index++)
                {
                    String child = childParam[index];
                    //if (child.IsEmptyOrSpace()) { continue; }

                    if (node[child] == null)
                    {
                        __AddChild(node, child, NODE_TYPE.NO_HAVE_DATA);
                    }
                    node = node[child];
                }
                TreeData newNode = new TreeData(childParam[last], type, value);
                __AddChild(node, newNode);
            }

            /// <summary>すべての子要素を削除する</summary>
            private static void __Clear(TreeData node)
            {
                if (node.HasChild == false) { return; }

                foreach (TreeData child in node.GetChildList)
                {
                    TreeData.__Clear(child);
                    child.m_parentNode = null;
                    child.GetChildList.Clear();
                }
            }

            /// <summary>開始要素から指定するパスに該当する要素を削除する</summary>
            private static void __RemoveChild(TreeData element, String path)
            {
                TreeData removeNode = __SearchChildByPath(element, path);
                if (removeNode == null) { return; }

                __Clear(removeNode);
                if (removeNode.IsRoot == false)
                {
                    TreeData nodeParent = removeNode.Parent;
                    nodeParent.GetChildList.Remove(removeNode);
                }
            }

            /// <summary>検索要素から指定したパスの格納オブジェクトを取得する</summary>
            private static Object __GetValue(TreeData element, String path)
            {
                TreeData node = __SearchChildByPath(element, path);
                return node.Value;
            }

            /// <summary>検索要素から指定したパスの格納オブジェクトを設定する</summary>
            private static void __SetValue(TreeData element, String path, Object value)
            {
                TreeData node = __SearchChildByPath(element, path);
                node.Value = value;
            }

            /// <summary>検索要素から指定したパスの要素を取得する</summary>
            private static TreeData __GetChild(TreeData element, String path)
            {
                TreeData node = __SearchChildByPath(element, path);
                return node;
            }

            /// <summary>オブジェクトをコピーする</summary>
            private static TreeData __Clone(TreeData other)
            {
                TreeData newNode = new TreeData(other);

                if (other.HasChild)
                {
                    foreach (TreeData child in other.GetChildList)
                    {
                        TreeData childClone = __Clone(child);
                        __AddChild(newNode, childClone);
                    }
                }

                return newNode;
            }

            /// <summary>検索要素から指定したパスに要素があるか検索する</summary>
            private static bool __FindChild(TreeData element, String path)
            {
                TreeData node = __SearchChildByPath(element, path);
                return (node != null);
            }

            /// <summary>指定要素までのパスを取得する</summary>
            private static void __GetPath(TreeData element, ref String path)
            {
                if (element.IsRoot) { return; }

                path = TreeData.PathSeparator.ToString() + element.Name + path;

                __GetPath(element.Parent, ref path);
            }

            /// <summary>指定要素からの深度を取得する</summary>
            private static int __GetDepth(TreeData element)
            {
                int depth = 0;
                TreeData node = element;
                while (node.IsRoot == false)
                {
                    node = node.Parent;
                    depth++;
                }
                return depth;
            }
        }
    }
}

