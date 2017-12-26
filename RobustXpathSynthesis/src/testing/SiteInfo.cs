using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.testing
{
    class SiteInfo
    {
        public String SiteName;
        public static int PagesLimit =10;
        public List<String> StartURLs = new List<String>();
        public String NextPageXPath = "";
        public String PageExtractionXpath = "";
        public Dictionary<String, String> attributeExtraction = new Dictionary<string, string>();
    }
}
