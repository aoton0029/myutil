using System.Data;

namespace ColorSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            comboBox1.ValueMember = "Value";
            comboBox1.DisplayMember = "Display";

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Display", typeof(string));
            dataTable.Columns.Add("Value", typeof(ThemeColors));
            dataTable.Rows.Add(nameof(ThemeColors.IvoryTheme), ThemeColors.IvoryTheme);
            dataTable.Rows.Add(nameof(ThemeColors.AzureTheme), ThemeColors.AzureTheme);
            dataTable.Rows.Add(nameof(ThemeColors.WarmBeigeTheme), ThemeColors.WarmBeigeTheme);
            dataTable.Rows.Add(nameof(ThemeColors.PaleBlueTheme), ThemeColors.PaleBlueTheme);
            dataTable.Rows.Add(nameof(ThemeColors.SoftIvoryTheme), ThemeColors.SoftIvoryTheme);
            dataTable.Rows.Add(nameof(ThemeColors.SoftBlueTheme), ThemeColors.SoftBlueTheme);
            dataTable.Rows.Add(nameof(ThemeColors.SoftPinkTheme), ThemeColors.SoftPinkTheme);
            dataTable.Rows.Add(nameof(ThemeColors.SoftGreenTheme), ThemeColors.SoftGreenTheme);

            comboBox1.DataSource = dataTable;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex > 0)
            {
                ThemeColors tc = (ThemeColors)comboBox1.SelectedValue;
                set(label1, "Primary", tc.Primary);
                set(label2, "PrimaryVariant", tc.PrimaryVariant);
                set(label3, "Secondary", tc.Secondary);
                set(label4, "SecondaryVariant", tc.SecondaryVariant);
                set(label5, "Tertiary", tc.Tertiary);
                set(label6, "TertiaryVariant", tc.TertiaryVariant);
                set(label7, "Background", tc.Background);
                set(label8, "Error", tc.Error);
                set(label9, "Accent", tc.Accent);
                set(label10, "Surface", tc.Surface);
                set(label11, "TextPrimary", tc.OnPrimary);
                set(label12, "TextSecondary", tc.OnSecondary);

            }
        }

        private void set(Label l, string s, Color c)
        {
            l.Text = s + "\r\n" + c.ToString();
            l.BackColor = c;
        }
    }
}
