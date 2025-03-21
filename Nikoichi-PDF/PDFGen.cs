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

        public byte[] makePDF(nikoJsonWrap makepdf)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document();
                PdfCopy pdfCopy = new PdfCopy(document, memoryStream);
                document.Open();

                foreach(var files in makepdf.RequiredFiles)
                {
                    if (!files.fileExists) throw new Exception("ファイルが存在しない、または存在しません。");

                    using (PdfReader reader = new PdfReader(files.FilePath))
                    {
                        foreach(var page in files.ImportPages)
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

                document.Close();
                return memoryStream.ToArray();
            }

            
        }

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
