using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.CodeDom;
using System.Windows.Forms;

namespace Nikoichi_PDF
{
    class PDFGen
    {
        /// <summary>
        /// 指定フォルダ内のPDF生成用jsonを読み込みます。
        /// </summary>
        /// <param name="filepaths">jsonファイル</param>
        /// <returns>読み込んだPDFファイルの情報配列</returns>
        public List<nikoJsonWrap> loadfiles(string filepaths)
        {
            List<nikoJsonWrap> njw = new List<nikoJsonWrap>();
            string[] files = Directory.GetFiles(filepaths,"*.json");
            foreach (string filepath in files)
            {
                using (StreamReader sr = new StreamReader(filepath, Encoding.GetEncoding("utf-8")))
                {
                    try
                    {
                        var a = JsonConvert.DeserializeObject<nikoJsonWrap>(sr.ReadToEnd());
                        if(a.ExportFileName==null || a.TotalPages == 0)
                        {
                            continue;
                        }
                        else 
                        {
                            a.RequiredFiles = chk_files(a.RequiredFiles, filepaths);
                            njw.Add(a);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return njw;
        }

        /// <summary>
        /// ファイルの存在チェック&専用パス生成
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public List<RequiredFile> chk_files(List<RequiredFile> paths ,string folder_path)
        {
            List<RequiredFile> respData = new List<RequiredFile>();

            foreach(RequiredFile values in paths)
            {
                if (File.Exists(folder_path + @"\" + values.FilePath))
                {
                    respData.Add(new RequiredFile()
                    {
                        FilePath = folder_path + @"\" + values.FilePath,
                        ImportPages = values.ImportPages,
                        fileExists = true
                    });
                }
                else
                {
                    respData.Add(new RequiredFile()
                    {
                        FilePath = folder_path + @"\" + values.FilePath,
                        ImportPages = values.ImportPages,
                        fileExists = false
                    });
                }
            }
            return respData;
        }

        public static PdfImportedPage CreateCenteredTextPage(PdfCopy copy, string text, string fontPath= @"C:\Windows\Fonts\meiryo.ttc", float fontSize = 24f)
        {
            if (!File.Exists(fontPath))
                throw new FileNotFoundException("フォントファイルが見つかりません: " + fontPath);

            // PDFを書き出すためのMemoryStream
            byte[] pdfBytes;
            using (MemoryStream tempStream = new MemoryStream())
            {
                Document tempDoc = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(tempDoc, tempStream);
                tempDoc.Open();

                // 枠線描画（デバッグ用）
                Rectangle border = new Rectangle(0, 0, tempDoc.PageSize.Width, tempDoc.PageSize.Height);
                border.Border = Rectangle.BOX;
                border.BorderWidth = 1f;
                border.BorderColor = BaseColor.GRAY;
                tempDoc.Add(border);

                // フォント準備
                BaseFont baseFont = BaseFont.CreateFont(fontPath+",0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(baseFont, fontSize, Font.NORMAL, BaseColor.BLACK);

                float centerX = tempDoc.PageSize.Width / 2;
                float centerY = tempDoc.PageSize.Height / 2;

                ColumnText.ShowTextAligned(
                    writer.DirectContent,
                    Element.ALIGN_CENTER,
                    new Phrase(text, font),
                    centerX,
                    centerY,
                    0
                );

                tempDoc.Close(); // 明示的にクローズ
                pdfBytes = tempStream.ToArray(); // byte配列として保持
            }

            // PdfReader は PdfCopy に渡すまで生きている必要あり
            PdfReader reader = new PdfReader(pdfBytes);
            PdfImportedPage page = copy.GetImportedPage(reader, 1);

            // 注意: readerを呼び出し元で保持する必要がある場合もある
            return page;
        }

        /// <summary>
        /// PDFを生成します。
        /// </summary>
        /// <param name="makepdf">プリセット</param>
        /// <returns>生成されたPDFのByte配列</returns>
        /// <exception cref="Exception"></exception>
        public byte[] makePDF(nikoJsonWrap makepdf)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document();
                PdfCopy pdfCopy = new PdfCopy(document, memoryStream);
                document.Open();
                List<Dictionary<string,object>> bookmarks = new List<Dictionary<string, object>>();

                pdfCopy.AddPage(CreateCenteredTextPage(pdfCopy,Path.GetFileNameWithoutExtension(makepdf.ExportFileName)));

                foreach (var files in makepdf.RequiredFiles)
                {
                    if (!files.fileExists) throw new Exception("ファイルが存在しない、または存在しません。");

                    Dictionary<string, object> bookmark = new Dictionary<string, object>() {
                        {"Title",Path.GetFileNameWithoutExtension(files.FilePath)},
                        {"Action", "GoTo"},
                        {"Page", (files.ImportPages[0].PushPage+1).ToString()},
                    };
                    bookmarks.Add(bookmark);

                    using (PdfReader reader = new PdfReader(files.FilePath))
                    {
                        foreach (var page in files.ImportPages)
                        {
                            if(page.ImportPageNumber < 1 || page.ImportPageNumber > reader.NumberOfPages)
                            {
                                throw new Exception($"ページ数が不正です。{page.ImportPageNumber}は{files.FilePath}に存在しません。");
                            }
                            PdfImportedPage importedPage = pdfCopy.GetImportedPage(reader, page.ImportPageNumber);
                            pdfCopy.AddPage(importedPage);
                        }
                    }
                }
                pdfCopy.Outlines = bookmarks;
                document.Close();
                return memoryStream.ToArray();
            }

            
        }

        /// <summary>
        /// PDFの保存を行います。
        /// </summary>
        /// <param name="saveByte">保存したいByte配列</param>
        /// <param name="writepath">保存先</param>
        /// <param name="savedirectory">保存先のディレクトリ</param>
        /// <exception cref="Exception">なんか保存できないとエラーが出ます。たぶん。</exception>
        public void Save_PDF(byte[] saveByte,string writepath,string savedirectory)
        {
            if (saveByte != null)
            {
                if (!Directory.Exists(savedirectory))
                {
                    Directory.CreateDirectory(savedirectory);
                    File.WriteAllBytes(writepath, saveByte);
                }
                else
                {
                    File.WriteAllBytes(writepath, saveByte);
                }
            }
            else
            {
                throw new Exception("保存できないが");
            }
        }
    }
}
