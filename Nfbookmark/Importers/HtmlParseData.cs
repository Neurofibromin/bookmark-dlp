using System.Collections.Generic;

namespace Nfbookmark.Importers
{
    class HtmlParseData
    {
        public int Id { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string Name { get; set; }
        public int Depth { get; set; }
        public List<string> Urls { get; } = new List<string>();
    }
}