private void c1FlexGrid1_OwnerDrawCell(object sender, C1.Win.C1FlexGrid.OwnerDrawCellEventArgs e)
{
    var grid = sender as C1.Win.C1FlexGrid.C1FlexGrid;

    // マージされたセルの範囲を取得
    var range = grid.GetMergedRange(e.Row, e.Col);

    if (range != null) // マージされている場合
    {
        // 範囲の最初の行を基準に背景色を設定
        int baseRow = range.TopRow;

        // 背景色を変更するロジック（例：行ごとの色分け）
        if (baseRow % 2 == 0)
        {
            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
        }
        else
        {
            e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);
        }

        // 通常のテキスト描画
        e.Graphics.DrawString(
            grid[e.Row, e.Col]?.ToString(),
            e.Style.Font,
            Brushes.Black,
            e.Bounds,
            StringFormat.GenericDefault
        );

        e.Handled = true; // 標準描画を無効化
    }
    else // マージされていない場合
    {
        // 通常の背景色変更処理
        if (e.Row % 2 == 0)
        {
            e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
        }
        else
        {
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
        }

        // 通常のテキスト描画
        e.Graphics.DrawString(
            grid[e.Row, e.Col]?.ToString(),
            e.Style.Font,
            Brushes.Black,
            e.Bounds,
            StringFormat.GenericDefault
        );

        e.Handled = true; // 標準描画を無効化
    }
}