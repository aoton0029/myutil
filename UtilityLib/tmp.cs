C#でstring配列を対応するenum型に変換する方法を説明します。特に、Flags付きのenumを扱う場合には、複数の文字列を組み合わせてフラグを生成できます。

以下に、string配列からenumに変換する汎用的な方法を実装した例を示します。


---

サンプルコード

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;

[Flags]
public enum CodeFlags
{
    [Display(Name = "Code 010")]
    Code010 = 1 << 0, // 1 (0001)

    [Display(Name = "Code 020")]
    Code020 = 1 << 1, // 2 (0010)

    [Display(Name = "Code 030")]
    Code030 = 1 << 2, // 4 (0100)

    [Display(Name = "Code 040")]
    Code040 = 1 << 3, // 8 (1000)

    None = 0
}

public static class EnumExtensions
{
    // Display属性から文字列を取得する拡張メソッド
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }

    // 文字列配列からフラグ型enumに変換するメソッド
    public static TEnum ConvertToEnum<TEnum>(this string[] stringValues) where TEnum : Enum
    {
        var type = typeof(TEnum);
        TEnum result = (TEnum)Enum.ToObject(type, 0); // 初期値はNoneまたは0

        foreach (var str in stringValues)
        {
            // 各enumのDisplay属性と一致する値を探す
            var flag = Enum.GetValues(type).Cast<TEnum>()
                .FirstOrDefault(e => e.GetDisplayName() == str);

            // 見つかった場合はフラグをORで結合
            result = (TEnum)Enum.ToObject(type, Convert.ToInt32(result) | Convert.ToInt32(flag));
        }

        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // 入力となる文字列配列
        string[] inputStrings = { "Code 010", "Code 030" };

        // 文字列配列からCodeFlagsに変換
        CodeFlags flags = inputStrings.ConvertToEnum<CodeFlags>();

        // 結果を表示
        Console.WriteLine($"Converted Flags: {flags}");

        // 個別のフラグを確認
        Console.WriteLine($"Contains Code010: {flags.HasFlag(CodeFlags.Code010)}");
        Console.WriteLine($"Contains Code020: {flags.HasFlag(CodeFlags.Code020)}");
    }
}


---

実行結果

Converted Flags: Code010, Code030
Contains Code010: True
Contains Code020: False


---

コードの解説

1. GetDisplayName拡張メソッド

各enum値のDisplayAttributeを取得し、そのNameプロパティを返します。

DisplayAttributeが設定されていない場合は、通常のenum名を返します。



2. ConvertToEnum拡張メソッド

入力された文字列配列の各文字列について、enumのDisplayAttributeと比較。

一致するenum値を見つけたら、OR演算で結果のフラグに追加します。



3. 使い方

例えば、{ "Code 010", "Code 030" }を渡すと、CodeFlags.Code010 | CodeFlags.Code030に変換されます。





---

この方法を使えば、DisplayAttributeで装飾されたenumを文字列から動的に生成することができます。また、汎用性が高く、他のenum型にも適用できます。

