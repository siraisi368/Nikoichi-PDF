using System.Collections.Generic;

namespace Nikoichi_PDF
{
    public class ImportPage
    {
        public int ImportPageNumber { get; set; }
        public int PushPage { get; set; }
    }

    public class RequiredFile
    {
        public string FilePath { get; set; }
        public List<ImportPage> ImportPages { get; set; }
        public bool fileExists { get; set; } = false;
    }

    public class nikoJsonWrap
    {
        public string ExportFileName { get; set; }
        public int TotalPages { get; set; }
        public List<RequiredFile> RequiredFiles { get; set; }
    }


}
