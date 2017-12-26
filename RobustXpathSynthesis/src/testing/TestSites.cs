using HtmlAgilityPack;
using RobustXpathSynthesis.src.alignment_baseline;
using RobustXpathSynthesis.src.decisiontree;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.utilities;
using RobustXpathSynthesis.src.weka_baseline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RobustXpathSynthesis.src.testing
{
    class TestSites
    {
        public static int results_count = 0;
        public static double ours_precision = 0;
        public static double ours_recall = 0;
        public static double alignment_precision = 0;
        public static double alignment_recall = 0;

        public static int pagesNum = 1005;
        public static Dictionary<String, Dictionary<String, Dictionary<String, HtmlNode>>> SiteDocuments = new Dictionary<string, Dictionary<String, Dictionary<String, HtmlNode>>>();
        public static Dictionary<String, Dictionary<String, HashSet<String>>> SiteLinks = new Dictionary<string, Dictionary<String, HashSet<String>>>();

        public static void TestAllSites()
        {
            List<SiteInfo> sitesToTest = new List<SiteInfo>();

            
              sitesToTest.Add(new SiteInfo()
                 {
                     SiteName = "currys.co.uk",
                     StartURLs = new List<String>() { "http://www.currys.co.uk/gbuk/tv-and-home-entertainment/televisions/televisions/301_3002_30002_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/household-appliances/small-kitchen-appliances/toasters/336_3157_30245_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/household-appliances/laundry/washing-machines/332_3119_30206_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/household-appliances/dishwashing/dishwashers/350_4035_31762_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/household-appliances/small-kitchen-appliances/coffee-machines-and-accessories/coffee-machines/336_3159_31562_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/household-appliances/small-kitchen-appliances/kettles/336_3156_30244_xx_xx/xx-criteria.html", "http://www.currys.co.uk/gbuk/computing/laptops/laptops/315_3226_30328_xx_xx/xx-criteria.html" },
                     NextPageXPath = "//*[@class[contains(.,\"pagination\")]]//li//a[@class[contains(.,\"next\")]]",
                     PageExtractionXpath = "//article//div[@class[contains(.,\"desc\")]]//a",
                     attributeExtraction = new Dictionary<string, string>() { { "ProductName", "//div[@class[contains(.,\"product-page\")]]//h1[@class[contains(.,\"page-title\")]]" }, { "productPrice", "//div[@id=\"product-actions-touch\"]//div[@class[contains(.,\"prd-amounts\")]]" } }
                 });
                 
            sitesToTest.Add(new SiteInfo()
                 {
                     SiteName = "PriceSpy",
                     StartURLs = new List<String>() { "http://pricespy.co.uk/category.php?k=1594" },
                     NextPageXPath = "//div[@class[contains(.,\"page\")]]//a[@title[contains(.,\"Next\")]]",//"//h4//span//a[@rel=\"next\"]",
                     PageExtractionXpath = "//td//h4//span//a[@href[contains(.,\"/product.php?p\") and not(contains(.,\"demo\"))]]",
                     attributeExtraction = new Dictionary<string, string>() { /*{ "ProductName", "//*[@class[contains(.,\"intro_body\")]]//h1" },*/{"productPrice","//a[@class[contains(.,\"price\")]]"} }
                 });
                
            sitesToTest.Add(new SiteInfo()
                 {
                     SiteName = "bestbuy",
                     StartURLs = new List<String>() { "http://www.bestbuy.com/site/tvs/all-flat-panel-tvs/abcat0101001.c?id=abcat0101001", "http://www.bestbuy.com/site/headphones/all-headphones/pcmcat144700050004.c?id=pcmcat144700050004","http://www.bestbuy.com/site/home-audio-systems/home-theater-systems/abcat0203000.c?id=abcat0203000", "http://www.bestbuy.com/site/refrigerators/all-refrigerators/pcmcat367400050001.c?id=pcmcat367400050001" },
                     NextPageXPath = "//li[@class[contains(.,\"pager-next\")]]//a[@*[contains(.,\"Next\")]]",
                     PageExtractionXpath = "//div[@class[contains(.,\"title\")]]//h4//a",
                     attributeExtraction = new Dictionary<string, string>() {/* { "ProductName", "//div[@id[contains(.,\"title\")]]//h1" },*/ { "productPrice", "//div[@class=\"item-price\"]" } }
                 });

            
            sitesToTest.Add(new SiteInfo()
              {
                  SiteName = "pricerunner",
                  StartURLs = new List<String>() { "http://www.pricerunner.co.uk/cl/34/Audio-Systems","http://www.pricerunner.co.uk/cl/14/Washing-Machines" },
                  NextPageXPath = "//div[@class[contains(.,\"paginator\")]]//a[@title[contains(.,\"Next\")]]",
                  PageExtractionXpath = "//div[@class=\"productname\"]//h3//a",
                  attributeExtraction = new Dictionary<string, string>() { /*{ "ProductName", "//h1[@class=\"pagetitle\"]" } ,*/ { "productPrice", "//div[@class[contains(.,\"productinfocontent\")]]//span[@itemprop=\"price\"]" } }
              }); 
              
            sitesToTest.Add(new SiteInfo()
            {
                SiteName = "ebuyer",
                StartURLs = new List<String>() { "http://www.ebuyer.com/store/Computer/cat/Laptops","http://www.ebuyer.com/store/Components/cat/Memory---PC","http://www.ebuyer.com/store/Storage/cat/Hard-Drive---Internal","http://www.ebuyer.com/store/Computer/cat/Monitors" },
                NextPageXPath = "//li[@class[contains(.,\"next\")]]//a[@title[contains(.,\"Next\")]]",
                PageExtractionXpath = "//div[@class=\"listing-info\"]//h3//a",
                attributeExtraction = new Dictionary<string, string>() { { "ProductName", "//h1[@itemprop=\"name\"]" }, { "productPrice", "//p[@class=\"price\"]//span[@itemprop=\"price\"]" } }
            });
              
            PorcessSites(sitesToTest);
        }


        public static void PorcessSites(List<SiteInfo> siteinfos)
        {
            foreach(SiteInfo si in siteinfos)
            {
                  try { 

                int leftPages = pagesNum * si.attributeExtraction.Keys.Count();
                    List<HtmlNode> sitePages = new List<HtmlNode>(0);
                    if (!SiteDocuments.ContainsKey(si.SiteName))
                    {
                        SiteDocuments[si.SiteName] = new Dictionary<string, Dictionary<String, HtmlNode>>();
                        SiteLinks[si.SiteName] = new Dictionary<string, HashSet<String>>();
                        foreach (String attrName in si.attributeExtraction.Keys)
                        {
                            SiteDocuments[si.SiteName][attrName] = new Dictionary<string, HtmlNode>();
                            SiteLinks[si.SiteName][attrName] = new HashSet<string>();
                        }
                    }

                    //Download all URLs
                    foreach (String startURL in si.StartURLs)
                    {
                        try
                        {
                            String currURL = startURL;
                            while (currURL != null)
                            {
                                HtmlDocument doc = new HtmlDocument();
                                var currHTML = URLDownloader.GetHtmlOfURL(currURL);
                                doc.LoadHtml(currHTML);
                                if (!String.IsNullOrEmpty(si.PageExtractionXpath))
                                {
                                    var links = doc.DocumentNode.SelectNodes(si.PageExtractionXpath);
                                    foreach (HtmlNode lnk in links)
                                    {
                                        try
                                        {
                                            String pageLink = lnk.Attributes["href"].Value;

                                   // var htmlstr = URLDownloader.GetHtmlOfURL(URLDownloader.UrlFixIfRelative(pageLink, currURL));
                                            foreach (string attr in si.attributeExtraction.Keys)
                                            {
                                                var newURL = URLDownloader.UrlFixIfRelative(pageLink, currURL);
                                                if (SiteLinks[si.SiteName][attr].Contains(newURL)) { continue; }
                                                //HtmlDocument innerDoc = new HtmlDocument();
                                                //innerDoc.LoadHtml(htmlstr);
                                                //SiteDocuments[si.SiteName][attr].Add(pageLink, innerDoc.DocumentNode);
                                                SiteLinks[si.SiteName][attr].Add(newURL);
                                                if (--leftPages <= 0) { break; }
                                            }
                                            if (leftPages <= 0) { break; }

                                        }
                                        catch(Exception e) {
                                                // Console.WriteLine(e.StackTrace);
                                            }
                                    }

                                }
                                else
                                {
                                    foreach (string attr in si.attributeExtraction.Keys)
                                    {
                                        try
                                        {
                                            if (SiteLinks[si.SiteName][attr].Contains(currURL)) { continue; }
                                            // HtmlDocument innerDoc = new HtmlDocument();
                                            // innerDoc.LoadHtml(currHTML);
                                            // SiteDocuments[si.SiteName][attr].Add(currURL, innerDoc.DocumentNode);
                                            SiteLinks[si.SiteName][attr].Add(currURL);
                                            if (--leftPages <= 0) { break; }
                                        }
                                        catch
                                        {
                                            break;
                                        }
                                    }
                                   
                                }
                                if (leftPages <= 0) { break; }
                                //get next page
                                String nextLink = null;
                                try
                                {
                                    
                                    nextLink = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(si.NextPageXPath).Attributes["href"].Value);
                                }
                                catch { nextLink = null; }

                                    if (nextLink != null)
                                    {
                                        nextLink=URLDownloader.UrlFixIfRelative(nextLink, currURL);
                                    }
                                if (!currURL.ToLower().Trim().Equals(nextLink.ToLower().Trim()))
                                {
                                    currURL = nextLink;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        catch { }
                    }

                    foreach (String attr in si.attributeExtraction.Keys)
                    {

                        var trainingkeys = new  HashSet<String>(SiteLinks[si.SiteName][attr].Take(5));//new HashSet<String>(SiteDocuments[si.SiteName][attr].Keys.Take(5));
                        var trainingDic = new Dictionary<String, HtmlNode>();//SiteDocuments[si.SiteName][attr].Where(x => trainingkeys.Contains(x.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                        foreach(String lnk in trainingkeys)
                        {
                            trainingDic.Add(lnk, GetHtmlNode(lnk));
                        }

                        var testDic = SiteDocuments[si.SiteName][attr].Where(x => !trainingkeys.Contains(x.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);



                        foreach (var lnk in trainingDic.Keys)
                        {

                            HtmlNode adoc = trainingDic[lnk];
                            try
                            {
                                var gt = adoc.SelectNodes(si.attributeExtraction[attr]);
                                if (gt != null)
                                {
                                    foreach (var targetNode in gt)
                                    {
                                        //Console.Write(":");
                                        if (targetNode.Attributes.Contains("userselected"))
                                        {
                                            targetNode.SetAttributeValue("userselected", "yes");
                                        }
                                        else
                                        {
                                            targetNode.Attributes.Add("userselected", "yes");
                                        }
                                    }
             
                                }

                            }
                            catch { }

                            MD5 md5 = MD5.Create();


                            if (!File.Exists("huge/" + si.SiteName + "/training/" + attr + "/" + getMD5(lnk) + ".html"))
                            {
                                Directory.CreateDirectory("huge/" + si.SiteName + "/training/" + attr);
                                File.WriteAllText("huge/" + si.SiteName + "/training/" + attr + "/" + getMD5(lnk) + ".html", adoc.InnerHtml);
                            }
                        }

                        DomPool.LoadDocuments(trainingDic);
                        //DomPool.LoadTestDocuments();
                        DomPool.Initiate(new HashSet<string>(trainingDic.Keys));
                        DomPool.ExtractAllFeatures();

                        // Run code
                        DecisionNode dn = new DecisionNode();
                        dn.InitialNodeSet = new HashSet<HtmlNode>(DomPool.TargetNodes.Union(DomPool.NonTargetNodes));
                        dn.SelectedNegative = new HashSet<HtmlNode>(DomPool.NonTargetNodes.Except(DomPool.TargetNodesPrecision));
                        dn.SelectedPositive = new HashSet<HtmlNode>(DomPool.TargetNodes);
                        dn.FeatureSet = new HashSet<Feature>();
                        dn.CalculateEntropy();

                        DecisionTreeLearning.RecursiveTreeImprovement(dn);
                        var xpath = XpathTools.GenerateAForgivingXpath(dn);
                        var xpathNonForgiving = XpathTools.DecisionTreeToXpath(dn, new HashSet<Feature>(), 1);
                        xpathNonForgiving = "//*" + (xpathNonForgiving.Equals("") ? "" : ("[" + xpathNonForgiving + "]"));

                        XpathAlignment model = new XpathAlignment();
                        model.LearnModel();
                        var alignmentXpath = model.xpath;


                        CheckOnTest(new HashSet<string>(SiteLinks[si.SiteName][attr].Except(trainingkeys)), xpath, si.attributeExtraction[attr], si.SiteName, attr, "ForgivingXP");
                        CheckOnTest(new HashSet<string>(SiteLinks[si.SiteName][attr].Except(trainingkeys)), alignmentXpath, si.attributeExtraction[attr], si.SiteName, attr, "Alignment");

                    }



            }finally{

            }

                SiteDocuments.Remove(si.SiteName);
        }
            Console.ReadLine();

        }




        public static string getMD5(string name)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(name);

            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

        }


        public static void CheckOnTest(HashSet<String> testLinks,String testedXpath,String groundtruthXpath,String siteName,String attr, String toolName)
        {
            var resultSet = new HashSet<HtmlNode>();
            var groundTruth = new HashSet<HtmlNode>();
            double precision = 0;
            double recall = 0;
            double devider = 0;
            foreach(var lnk in testLinks)
            {

                HtmlNode adoc = GetHtmlNode(lnk);
                try
                {

                   

                    var gt = adoc.SelectNodes(groundtruthXpath);
                    
                    if (gt != null) {
                        foreach (var targetNode in gt)
                        {
                            //Console.Write(":");
                            if (targetNode.Attributes.Contains("userselected"))
                            {
                                targetNode.SetAttributeValue("userselected", "yes");
                            }
                            else
                            {
                                targetNode.Attributes.Add("userselected", "yes");
                            }
                        }
                        // groundTruth.UnionWith(gt);
                        var currGT = new HashSet<HtmlNode>(gt);
                        var forPrecisionGT = GetForPrecision(currGT);
                        var oursSet = adoc.SelectNodes(testedXpath);
                        var ours = new HashSet<HtmlNode>();
                        if (!(oursSet==null)) { 
                            ours = new HashSet<HtmlNode>(oursSet);
                        }

                        if (ours.Count() > 0)
                        {
                            precision = precision + (double)(ours.Intersect(forPrecisionGT).Count()) / ours.Count();
                        }
                        recall = recall+(double)(ours.Intersect(currGT).Count()) / currGT.Count();
                        devider++;
                        //if (ours != null)
                        //{
                        //    resultSet.UnionWith(ours);
                        //}
                    }

                }
                finally { }

                MD5 md5 = MD5.Create();


                if (!File.Exists("huge/" + siteName + "/test/" + attr + "/" + getMD5(lnk) + ".html"))
                {
                    Directory.CreateDirectory("huge/" + siteName + "/test/" + attr);
                    File.WriteAllText("huge/" + siteName + "/test/" + attr + "/" + getMD5(lnk) + ".html", adoc.InnerHtml);
                }
            }

            precision = precision / devider;
            recall = recall / devider;

            //includeing the children of the selected node with the results should not hurt precision but should not contribute to recall


           

            Console.WriteLine("- "+toolName+"/" + siteName +":" + attr + " Recall:" + recall + " , Precision:" + precision);
            
        }


        public static HtmlNode GetHtmlNode(String url)
        {
            var htmlstr = URLDownloader.GetHtmlOfURL(url);
            HtmlDocument innerDoc = new HtmlDocument();
            innerDoc.LoadHtml(htmlstr);
            return innerDoc.DocumentNode;
        }


        public static HashSet<HtmlNode> GetForPrecision(HashSet<HtmlNode> groundTruth)
        {
            HashSet<HtmlNode> forPrecision = new HashSet<HtmlNode>(groundTruth);
            foreach (var currNd in groundTruth)
            {
                var children = currNd.SelectNodes(".//*");
                if (children == null || children.Count() == 0) { continue; }
                forPrecision.UnionWith(children);
            }
            return forPrecision;
        }



    }
}
