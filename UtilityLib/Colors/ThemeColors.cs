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
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
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

        // **Soft Ivory Theme (#FCF9EF)**
        public static ThemeColors SoftIvoryTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"), // オレンジ系
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#FCF9EF"),  // Soft Ivory
            Surface = ColorTranslator.FromHtml("#F5F5F5"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };

        // **Warm Beige Theme (#F4EEE9)**
        public static ThemeColors WarmBeigeTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"),
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#F4EEE9"),  // Warm Beige
            Surface = ColorTranslator.FromHtml("#EDE7E3"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };

        // **Soft Pink Theme (#F8F2F7)**
        public static ThemeColors SoftPinkTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"),
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#F8F2F7"),  // Soft Pink
            Surface = ColorTranslator.FromHtml("#F0E4EC"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };

        // **Soft Blue Theme (#EAF1F2)**
        public static ThemeColors SoftBlueTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"), // オレンジ系
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#EAF1F2"),  // ソフトブルー
            Surface = ColorTranslator.FromHtml("#DCE8EA"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };

        // **Soft Green Theme (#E5EDE9)**
        public static ThemeColors SoftGreenTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"),
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#E5EDE9"),  // ソフトグリーン
            Surface = ColorTranslator.FromHtml("#D9E2DC"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };

        // **Pale Blue Theme (#E6EAF2)**
        public static ThemeColors PaleBlueTheme => new ThemeColors
        {
            Primary = ColorTranslator.FromHtml("#6200EE"),
            PrimaryVariant = ColorTranslator.FromHtml("#3700B3"),
            Secondary = ColorTranslator.FromHtml("#03DAC6"),
            SecondaryVariant = ColorTranslator.FromHtml("#018786"),
            Tertiary = ColorTranslator.FromHtml("#FF6F00"),
            TertiaryVariant = ColorTranslator.FromHtml("#E65100"),
            Accent = ColorTranslator.FromHtml("#FF0266"),
            Background = ColorTranslator.FromHtml("#E6EAF2"),  // ペールブルー
            Surface = ColorTranslator.FromHtml("#D8DFEB"),
            TextPrimary = ColorTranslator.FromHtml("#000000"),
            TextSecondary = ColorTranslator.FromHtml("#757575"),
            Error = ColorTranslator.FromHtml("#B00020")
        };
    }
}
