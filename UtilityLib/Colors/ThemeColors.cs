using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Colors
{
    public class ThemeColors
    {
        // プライマリーカラー (Primary)
        public Color Primary { get; set; }
        public Color PrimaryVariant { get; set; }

        // セカンダリーカラー (Secondary)
        public Color Secondary { get; set; }
        public Color SecondaryVariant { get; set; }

        // 第三のカラー (Tertiary)
        public Color Tertiary { get; set; }
        public Color TertiaryVariant { get; set; }

        // アクセントカラー (Accent)
        public Color Accent { get; set; }

        // 背景 (Background) とテキスト (Text)
        public Color Background { get; set; }
        public Color Surface { get; set; }
        public Color TextPrimary { get; set; }
        public Color TextSecondary { get; set; }

        // エラーカラー (Error)
        public Color Error { get; set; }

        // **Ivory テーマ**
        public static ThemeColors IvoryTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"), // オレンジ系
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = Color.Ivory,
            Surface = ColorTranslator.FromHtml("#F5F5F5"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020") // マテリアルデザインのエラーカラー
        };

        // **Azure テーマ**
        public static ThemeColors AzureTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#007FFF"),
            PrimaryVariant = ColorTranslator.FromHtml("#005FCC"),
            Secondary = ColorTranslator.FromHtml("#0096FF"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"), // オレンジ系
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = Color.Azure,
            Surface = ColorTranslator.FromHtml("#E0F7FA"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020") // マテリアルデザインのエラーカラー
        };

    }
}
