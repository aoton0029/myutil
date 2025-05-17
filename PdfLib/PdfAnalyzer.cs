using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFLib
{
    /// <summary>
    /// PDFのメタデータおよび構造解析を行うクラス
    /// </summary>
    public class PdfAnalyzer : IDisposable
    {
        private PdfDocument _document;
        private string _filePath;
        private bool _isModified;
        private bool _disposed = false;

        /// <summary>
        /// PDFファイルを指定して初期化します
        /// </summary>
        /// <param name="filePath">PDFファイルのパス</param>
        public PdfAnalyzer(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDFファイルが見つかりません", filePath);

            _filePath = filePath;
            _document = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
            _isModified = false;
        }

        /// <summary>
        /// PDFのメタデータ情報を取得します
        /// </summary>
        /// <returns>メタデータ情報を含むオブジェクト</returns>
        public PdfMetadata GetMetadata()
        {
            var info = _document.Info;

            return new PdfMetadata
            {
                Title = info.Title,
                Author = info.Author,
                Subject = info.Subject,
                Keywords = info.Keywords,
                Creator = info.Creator,
                Producer = info.Producer,
                CreationDate = info.CreationDate,
                ModificationDate = info.ModificationDate
            };
        }

        /// <summary>
        /// PDFのメタデータを設定します
        /// </summary>
        /// <param name="metadata">設定するメタデータ</param>
        public void SetMetadata(PdfMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            var info = _document.Info;
            info.Title = metadata.Title;
            info.Author = metadata.Author;
            info.Subject = metadata.Subject;
            info.Keywords = metadata.Keywords;
            info.Creator = metadata.Creator;
            info.CreationDate = metadata.CreationDate.Value;
            info.ModificationDate = metadata.ModificationDate.Value;

            _isModified = true;
        }

        /// <summary>
        /// PDFの構造情報を解析して返します
        /// </summary>
        /// <returns>PDFの構造情報</returns>
        public PdfStructure AnalyzeStructure()
        {
            var structure = new PdfStructure
            {
                PageCount = _document.PageCount,
                Pages = new List<PageInfo>()
            };

            for (int i = 0; i < _document.PageCount; i++)
            {
                var page = _document.Pages[i];

                structure.Pages.Add(new PageInfo
                {
                    Index = i,
                    Width = page.Width.Point,
                    Height = page.Height.Point,
                    Orientation = page.Orientation.ToString(),
                    HasContent = page.Contents.Elements.Count > 0
                });
            }

            return structure;
        }

        /// <summary>
        /// 変更をPDFファイルに保存します
        /// </summary>
        public void Save()
        {
            if (_isModified)
            {
                _document.Save(_filePath);
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

            _document.Save(filePath);

            // 新しいファイルパスを現在のパスとして設定
            _filePath = filePath;
            _isModified = false;
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
                    if (_document != null)
                    {
                        if (_isModified)
                        {
                            try
                            {
                                _document.Save(_filePath);
                            }
                            catch
                            {
                                // 保存に失敗した場合は無視
                            }
                        }
                        _document.Close();
                        _document = null;
                    }
                }

                // アンマネージドリソースの解放

                _disposed = true;
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~PdfAnalyzer()
        {
            Dispose(false);
        }
    }

}
