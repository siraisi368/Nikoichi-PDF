using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                        njw.Add(JsonConvert.DeserializeObject<nikoJsonWrap>(sr.ReadToEnd()));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return njw;
        }
    }
}
