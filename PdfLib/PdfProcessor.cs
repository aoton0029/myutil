using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFLib
{
    /// <summary>
    /// iTextSharpを使用してPDFの操作、テキストや画像、表の抽出や編集を行うクラス
    /// </summary>
    public class PdfProcessor : IDisposable
    {
        private PdfReader _reader;
        private PdfStamper _stamper;
        private string _filePath;
        private MemoryStream _outputStream;
        private bool _isModified;
        private bool _disposed = false;

        /// <summary>
        /// PDFファイルを指定して初期化します
        /// </summary>
        /// <param name="filePath">PDFファイルのパス</param>
        public PdfProcessor(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDFファイルが見つかりません", filePath);

            _filePath = filePath;
            _reader = new PdfReader(filePath);
            _outputStream = new MemoryStream();
            _stamper = new PdfStamper(_reader, _outputStream);
            _isModified = false;
        }

        /// <summary>
        /// PDFからすべてのテキストを抽出します
        /// </summary>
        /// <returns>抽出されたテキスト</returns>
        public string ExtractAllText()
        {
            StringBuilder text = new StringBuilder();

            for (int i = 1; i <= _reader.NumberOfPages; i++)
            {
                text.Append(ExtractTextFromPage(i));
                text.Append(Environment.NewLine);
            }

            return text.ToString();
        }

        /// <summary>
        /// 指定したページからテキストを抽出します
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <returns>抽出されたテキスト</returns>
        public string ExtractTextFromPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            return PdfTextExtractor.GetTextFromPage(_reader, pageNumber, strategy);
        }

        /// <summary>
        /// 指定した位置にテキストを追加します
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <param name="text">追加するテキスト</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="fontSize">フォントサイズ</param>
        public void AddText(int pageNumber, string text, float x, float y, float fontSize = 12)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            PdfContentByte canvas = _stamper.GetOverContent(pageNumber);
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            canvas.BeginText();
            canvas.SetFontAndSize(baseFont, fontSize);
            canvas.SetTextMatrix(x, y);
            canvas.ShowText(text);
            canvas.EndText();

            _isModified = true;
        }

        /// <summary>
        /// 画像を抽出します
        /// </summary>
        /// <param name="outputFolder">画像を保存するフォルダパス</param>
        /// <returns>抽出された画像のパスリスト</returns>
        public List<string> ExtractImages(string outputFolder)
        {
            if (string.IsNullOrEmpty(outputFolder))
                throw new ArgumentNullException(nameof(outputFolder));

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            List<string> imagePaths = new List<string>();

            for (int i = 1; i <= _reader.NumberOfPages; i++)
            {
                PdfDictionary page = _reader.GetPageN(i);
                PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);

                if (resources != null)
                {
                    PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
                    if (xobjects != null)
                    {
                        foreach (PdfName name in xobjects.Keys)
                        {
                            PdfObject obj = xobjects.Get(name);
                            if (obj.IsIndirect())
                            {
                                PdfDictionary xobject = (PdfDictionary)PdfReader.GetPdfObject(obj);
                                if (xobject.Get(PdfName.SUBTYPE).Equals(PdfName.IMAGE))
                                {
                                    try
                                    {
                                        PdfImageObject image = new PdfImageObject((PRStream)xobject);
                                        System.Drawing.Image drawingImage = image.GetDrawingImage();
                                        string imagePath = System.IO.Path.Combine(outputFolder, $"image_p{i}_{name.ToString()}.png");
                                        drawingImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                                        imagePaths.Add(imagePath);
                                    }
                                    catch (Exception)
                                    {
                                        // 画像の抽出に失敗した場合は無視
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return imagePaths;
        }

        /// <summary>
        /// 画像を追加します
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <param name="imagePath">画像ファイルのパス</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void AddImage(int pageNumber, string imagePath, float x, float y, float width, float height)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                throw new FileNotFoundException("画像ファイルが見つかりません", imagePath);

            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imagePath);
            image.SetAbsolutePosition(x, y);

            if (width > 0 && height > 0)
                image.ScaleAbsolute(width, height);

            PdfContentByte canvas = _stamper.GetOverContent(pageNumber);
            canvas.AddImage(image);

            _isModified = true;
        }

        /// <summary>
        /// 表を追加します
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <param name="data">表のデータ（行×列）</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="cellWidth">セル幅</param>
        /// <param name="cellHeight">セル高さ</param>
        public void AddTable(int pageNumber, string[][] data, float x, float y, float cellWidth, float cellHeight)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            PdfContentByte canvas = _stamper.GetOverContent(pageNumber);
            PdfPTable table = new PdfPTable(data[0].Length);
            table.TotalWidth = cellWidth * data[0].Length;
            table.LockedWidth = true;

            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 12);

            foreach (string[] row in data)
            {
                foreach (string cellText in row)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(cellText, font));
                    cell.FixedHeight = cellHeight;
                    table.AddCell(cell);
                }
            }

            table.WriteSelectedRows(0, -1, x, y, canvas);

            _isModified = true;
        }

        /// <summary>
        /// 表形式データを抽出します（簡易的な実装）
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <returns>抽出された表データ（行ごとのテキスト）</returns>
        public List<string> ExtractTableData(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            // 注: 完全な表抽出は複雑なため、ここでは簡易的に行ごとのテキストを返す
            string pageText = ExtractTextFromPage(pageNumber);
            List<string> rows = new List<string>(pageText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

            // 簡易的なフィルタリング - タブや複数スペースを含む行を表データとみなす
            return rows.FindAll(row => row.Contains("\t") || row.Contains("  "));
        }

        /// <summary>
        /// 変更をPDFファイルに保存します
        /// </summary>
        public void Save()
        {
            if (_isModified)
            {
                CloseStamperIfOpen();

                using (FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    _outputStream.Position = 0;
                    _outputStream.CopyTo(fs);
                }

                ReopenStamper();
                _isModified = false;
            }
        }

        /// <summary>
        /// 変更を新しいPDFファイルに保存します
        /// </summary>
        /// <param name="filePath">保存先のファイルパス</param>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            CloseStamperIfOpen();

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                _outputStream.Position = 0;
                _outputStream.CopyTo(fs);
            }

            // 新しいファイルをオープン
            _filePath = filePath;
            _reader = new PdfReader(_filePath);
            _outputStream = new MemoryStream();
            _stamper = new PdfStamper(_reader, _outputStream);
            _isModified = false;
        }

        /// <summary>
        /// PDFの構造情報を解析して返します
        /// </summary>
        /// <returns>PDFの構造情報</returns>
        public PdfStructure AnalyzeStructure()
        {
            var structure = new PdfStructure
            {
                PageCount = _reader.NumberOfPages,
                Pages = new List<PageInfo>()
            };

            for (int i = 1; i <= _reader.NumberOfPages; i++)
            {
                Rectangle pageSize = _reader.GetPageSize(i);
                structure.Pages.Add(new PageInfo
                {
                    Index = i - 1, // 0ベースのインデックス
                    Width = pageSize.Width,
                    Height = pageSize.Height,
                    Orientation = pageSize.Width > pageSize.Height ? "Landscape" : "Portrait",
                    HasContent = true // iTextSharpでは簡単にはコンテンツの有無を判定できないため、常にtrueとする
                });
            }

            return structure;
        }

        private void CloseStamperIfOpen()
        {
            if (_stamper != null)
            {
                _stamper.Close();
                _stamper = null;
            }
        }

        private void ReopenStamper()
        {
            _outputStream = new MemoryStream();
            _reader = new PdfReader(_filePath);
            _stamper = new PdfStamper(_reader, _outputStream);
        }

        /// <summary>
        /// 指定した座標範囲内のテキストを抽出します
        /// </summary>
        /// <param name="pageNumber">ページ番号（1から開始）</param>
        /// <param name="x1">左上X座標</param>
        /// <param name="y1">左上Y座標</param>
        /// <param name="x2">右下X座標</param>
        /// <param name="y2">右下Y座標</param>
        /// <returns>抽出されたテキスト</returns>
        public string ExtractTextFromRegion(int pageNumber, float x1, float y1, float x2, float y2)
        {
            if (pageNumber < 1 || pageNumber > _reader.NumberOfPages)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            if (x1 > x2 || y1 < y2) // PDFの座標系ではY軸が下から上に増加するため
                throw new ArgumentException("座標範囲が無効です");

            // 座標で絞り込むカスタム抽出ストラテジー
            var regionFilter = new RegionTextExtractionStrategy(x1, y1, x2, y2);
            return PdfTextExtractor.GetTextFromPage(_reader, pageNumber, regionFilter);
        }

        /// <summary>
        /// 指定した座標範囲内のテキストを抽出するストラテジークラス
        /// </summary>
        private class RegionTextExtractionStrategy : LocationTextExtractionStrategy
        {
            private readonly float _minX;
            private readonly float _minY;
            private readonly float _maxX;
            private readonly float _maxY;

            public RegionTextExtractionStrategy(float x1, float y1, float x2, float y2)
            {
                _minX = Math.Min(x1, x2);
                _maxX = Math.Max(x1, x2);
                _minY = Math.Min(y1, y2);
                _maxY = Math.Max(y1, y2);
            }

            public override void RenderText(TextRenderInfo renderInfo)
            {
                // テキストの位置情報を取得
                var baseline = renderInfo.GetBaseline();
                var rectangle = renderInfo.GetAscentLine();

                // テキストの最小・最大座標を計算
                float textMinX = Math.Min(baseline.GetStartPoint()[Vector.I1], baseline.GetEndPoint()[Vector.I1]);
                float textMaxX = Math.Max(baseline.GetStartPoint()[Vector.I1], baseline.GetEndPoint()[Vector.I1]);
                float textMinY = Math.Min(baseline.GetStartPoint()[Vector.I2], baseline.GetEndPoint()[Vector.I2]);
                float textMaxY = Math.Max(rectangle.GetStartPoint()[Vector.I2], rectangle.GetEndPoint()[Vector.I2]);

                // テキストが指定した領域内にあるかチェック
                bool isInRegion = !(textMinX > _maxX || textMaxX < _minX || textMinY > _maxY || textMaxY < _minY);

                if (isInRegion)
                {
                    // 領域内のテキストのみ処理
                    base.RenderText(renderInfo);
                }
            }
        }

        /// <summary>
        /// PDFのメタデータを取得します
        /// </summary>
        /// <returns>PDFのメタデータ</returns>
        public PdfMetadata GetMetadata()
        {
            var metadata = new PdfMetadata();

            // 基本的なドキュメント情報
            metadata.Title = _reader.Info.ContainsKey("Title") ? _reader.Info["Title"] : null;
            metadata.Author = _reader.Info.ContainsKey("Author") ? _reader.Info["Author"] : null;
            metadata.Subject = _reader.Info.ContainsKey("Subject") ? _reader.Info["Subject"] : null;
            metadata.Keywords = _reader.Info.ContainsKey("Keywords") ? _reader.Info["Keywords"] : null;
            metadata.Creator = _reader.Info.ContainsKey("Creator") ? _reader.Info["Creator"] : null;
            metadata.Producer = _reader.Info.ContainsKey("Producer") ? _reader.Info["Producer"] : null;

            // 日付情報の取得と変換
            if (_reader.Info.ContainsKey("CreationDate"))
            {
                try
                {
                    // PDFの日付形式（例: D:20210615123456+09'00'）を解析
                    string creationDateStr = _reader.Info["CreationDate"];
                    metadata.CreationDate = ParsePdfDate(creationDateStr);
                }
                catch
                {
                    metadata.CreationDate = null;
                }
            }

            if (_reader.Info.ContainsKey("ModDate"))
            {
                try
                {
                    string modDateStr = _reader.Info["ModDate"];
                    metadata.ModificationDate = ParsePdfDate(modDateStr);
                }
                catch
                {
                    metadata.ModificationDate = null;
                }
            }

            return metadata;
        }

        /// <summary>
        /// PDFのメタデータを設定します
        /// </summary>
        /// <param name="metadata">設定するメタデータ</param>
        public void SetMetadata(PdfMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            // 基本的なドキュメント情報を設定
            if (!string.IsNullOrEmpty(metadata.Title))
                _stamper.Writer.Info.Put(new PdfName("Title"), new PdfString(metadata.Title, PdfObject.TEXT_UNICODE));

            if (!string.IsNullOrEmpty(metadata.Author))
                _stamper.Writer.Info.Put(new PdfName("Author"), new PdfString(metadata.Author, PdfObject.TEXT_UNICODE));

            if (!string.IsNullOrEmpty(metadata.Subject))
                _stamper.Writer.Info.Put(new PdfName("Subject"), new PdfString(metadata.Subject, PdfObject.TEXT_UNICODE));

            if (!string.IsNullOrEmpty(metadata.Keywords))
                _stamper.Writer.Info.Put(new PdfName("Keywords"), new PdfString(metadata.Keywords, PdfObject.TEXT_UNICODE));

            if (!string.IsNullOrEmpty(metadata.Creator))
                _stamper.Writer.Info.Put(new PdfName("Creator"), new PdfString(metadata.Creator, PdfObject.TEXT_UNICODE));

            if (!string.IsNullOrEmpty(metadata.Producer))
                _stamper.Writer.Info.Put(new PdfName("Producer"), new PdfString(metadata.Producer, PdfObject.TEXT_UNICODE));

            // 日付情報を設定
            if (metadata.CreationDate.HasValue)
            {
                string pdfDate = FormatPdfDate(metadata.CreationDate.Value);
                _stamper.Writer.Info.Put(new PdfName("CreationDate"), new PdfString(pdfDate));
            }

            if (metadata.ModificationDate.HasValue)
            {
                string pdfDate = FormatPdfDate(metadata.ModificationDate.Value);
                _stamper.Writer.Info.Put(new PdfName("ModDate"), new PdfString(pdfDate));
            }

            _isModified = true;
        }

        /// <summary>
        /// PDFの日付形式の文字列をDateTimeに変換します
        /// </summary>
        /// <param name="pdfDate">PDF日付形式の文字列</param>
        /// <returns>変換されたDateTime</returns>
        private DateTime? ParsePdfDate(string pdfDate)
        {
            if (string.IsNullOrEmpty(pdfDate))
                return null;

            // PDF日付形式: D:YYYYMMDDHHmmSSOHH'mm'
            // D: は省略可能, O はタイムゾーン識別子（+,-,Z）

            // "D:" プレフィックスを削除
            if (pdfDate.StartsWith("D:"))
                pdfDate = pdfDate.Substring(2);

            try
            {
                int year = int.Parse(pdfDate.Substring(0, 4));
                int month = int.Parse(pdfDate.Substring(4, 2));
                int day = int.Parse(pdfDate.Substring(6, 2));

                int hour = 0, minute = 0, second = 0;

                if (pdfDate.Length >= 10)
                    hour = int.Parse(pdfDate.Substring(8, 2));

                if (pdfDate.Length >= 12)
                    minute = int.Parse(pdfDate.Substring(10, 2));

                if (pdfDate.Length >= 14)
                    second = int.Parse(pdfDate.Substring(12, 2));

                DateTime dateTime = new DateTime(year, month, day, hour, minute, second);

                // タイムゾーン情報がある場合
                if (pdfDate.Length > 14)
                {
                    char tzChar = pdfDate[14];
                    if (tzChar == '+' || tzChar == '-' && pdfDate.Length >= 17)
                    {
                        int tzHour = int.Parse(pdfDate.Substring(15, 2));
                        int tzMinute = 0;

                        if (pdfDate.Length >= 20)
                            tzMinute = int.Parse(pdfDate.Substring(18, 2));

                        TimeSpan offset = new TimeSpan(tzHour, tzMinute, 0);
                        if (tzChar == '-')
                            offset = -offset;

                        // UTCに調整
                        dateTime = dateTime.Add(-offset);

                        // ローカルタイムに変換
                        dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
                    }
                    else if (tzChar == 'Z')
                    {
                        // UTC時間をローカルに変換
                        dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
                    }
                }

                return dateTime;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// DateTimeをPDFの日付形式の文字列に変換します
        /// </summary>
        /// <param name="dateTime">変換する日時</param>
        /// <returns>PDF日付形式の文字列</returns>
        private string FormatPdfDate(DateTime dateTime)
        {
            // PDFの日付形式: D:YYYYMMDDHHmmSSOHH'mm'

            // UTCに変換
            DateTime utcTime = dateTime.ToUniversalTime();

            // ローカルとUTCの時差
            TimeSpan offset = dateTime - utcTime;

            string offsetStr;
            if (offset == TimeSpan.Zero)
            {
                offsetStr = "Z";
            }
            else
            {
                char sign = offset.TotalMinutes >= 0 ? '+' : '-';
                TimeSpan absOffset = offset.TotalMinutes >= 0 ? offset : -offset;
                offsetStr = $"{sign}{absOffset.Hours:D2}'{absOffset.Minutes:D2}'";
            }

            return $"D:{dateTime:yyyyMMddHHmmss}{offsetStr}";
        }


        /// <summary>
        /// リソースを解放します
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドリソースも解放する場合はtrue</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // マネージドリソースの解放
                    if (_isModified)
                    {
                        try
                        {
                            Save();
                        }
                        catch
                        {
                            // 保存に失敗した場合は無視
                        }
                    }

                    if (_stamper != null)
                    {
                        _stamper.Close();
                        _stamper = null;
                    }

                    if (_reader != null)
                    {
                        _reader.Close();
                        _reader = null;
                    }

                    if (_outputStream != null)
                    {
                        _outputStream.Dispose();
                        _outputStream = null;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~PdfProcessor()
        {
            Dispose(false);
        }
    }
}
