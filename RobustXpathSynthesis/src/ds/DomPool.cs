using HtmlAgilityPack;
using RobustXpathSynthesis.src.phases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.ds
{
    class DomPool
    {
        //CACHES FOR K-Fold Testing
        public static Dictionary<String, HtmlNode> docsAndNames = new Dictionary<string, HtmlNode>();
        public static Dictionary<String, Dictionary<String, HashSet<HtmlNode>>> docsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();
        public static Dictionary<String, HashSet<Feature>> docsFeatures = new Dictionary<string, HashSet<Feature>>();

        //CACHES FOR TEST SET
        public static Dictionary<String, HtmlNode> testDocsAndNames = new Dictionary<string, HtmlNode>();
        public static Dictionary<String, Dictionary<String, HashSet<HtmlNode>>> testDocsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();

        //Input for K-Fold Iteration
        public static HashSet<String> trainingDocsNames = new HashSet<String>();

        public static string selectionAttribute = "userselected";
        public static string optionalSelectionAttribute = "optionaluserselected";
        //public static List<HtmlNode> docs = new List<HtmlNode>();
        public static List<String> allDocsNames = new List<string>();
        public static void LoadDocuments(string dirPath) {
            docsAndNames = new Dictionary<string, HtmlNode>(); 
            allDocsNames = new List<string>();
            docsFeatures = new Dictionary<string, HashSet<Feature>>();
            docsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();
           
            string[] files = Directory.GetFiles(dirPath, "*.html");
            foreach(string file in files){
                HtmlDocument doc = new HtmlDocument();
                doc.Load(file);
                string[] splitFiles = file.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                string nameFile = splitFiles[splitFiles.Length - 1];
                docsAndNames.Add(nameFile, doc.DocumentNode);
                docsQueryCache.Add(nameFile, new Dictionary<string, HashSet<HtmlNode>>());
                allDocsNames.Add(nameFile);
            }
            allDocsNames = allDocsNames.OrderBy(x => x).ToList();
        }

        public static void LoadDocuments(Dictionary<String, HtmlNode> allDocs)
        {
            docsAndNames = new Dictionary<string, HtmlNode>();
            allDocsNames = new List<string>();
            docsFeatures = new Dictionary<string, HashSet<Feature>>();
            docsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();
            foreach (string fileName in allDocs.Keys)
            {
                docsAndNames.Add(fileName, allDocs[fileName]);
                docsQueryCache.Add(fileName, new Dictionary<string, HashSet<HtmlNode>>());
                allDocsNames.Add(fileName);
            }
            allDocsNames = allDocsNames.OrderBy(x => x).ToList();
        }


        public static void LoadTestDocuments(string dirPath)
        {
            testDocsAndNames = new Dictionary<string, HtmlNode>();
            testDocsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();

            string[] files = Directory.GetFiles(dirPath, "*.html");
            foreach (string file in files)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(file);
                string[] splitFiles = file.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                string nameFile = splitFiles[splitFiles.Length - 1];
                testDocsAndNames.Add(nameFile, doc.DocumentNode);
                testDocsQueryCache.Add(nameFile, new Dictionary<string, HashSet<HtmlNode>>());
            }
        }


        public static void LoadTestDocuments(Dictionary<String, HtmlNode> testDocs)
        {
            testDocsAndNames = new Dictionary<string, HtmlNode>();
            testDocsQueryCache = new Dictionary<string, Dictionary<string, HashSet<HtmlNode>>>();

            foreach (string fileName in testDocs.Keys)
            {
                testDocsAndNames.Add(fileName, testDocs[fileName]);
                testDocsQueryCache.Add(fileName, new Dictionary<string, HashSet<HtmlNode>>());
            }
        }


        public static HashSet<HtmlNode> TargetNodes = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> TargetNodesPrecision = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> NonTargetNodes = new HashSet<HtmlNode>();

        public static HashSet<HtmlNode> TESTTargetNodes = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> TESTTargetNodesPrecision = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> TESTNonTargetNodes = new HashSet<HtmlNode>();

        public static HashSet<HtmlNode> TESTSeenTargetNodes = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> TESTSeenTargetNodesPrecision = new HashSet<HtmlNode>();
        public static HashSet<HtmlNode> TESTSeenNonTargetNodes = new HashSet<HtmlNode>();

        public static Dictionary<string, int> FeatureFrequencey = new Dictionary<string, int>();
        //public static Dictionary<string, int> FeatureDocumentFrequencey = new Dictionary<string, int>();
        public static HashSet<Feature> SelectorFeatures = new HashSet<Feature>();
        public static HashSet<Feature> OrderFeatures = new HashSet<Feature>();

        //Query result cache
        public static Dictionary<string, HashSet<HtmlNode>> queryResultCache = new Dictionary<string, HashSet<HtmlNode>>();

        public static void Initiate(HashSet<String> trainingDocs=null) {
            if (trainingDocs == null)
            {
                trainingDocsNames = new HashSet<String>(allDocsNames);
            }
            else {
                //Set the set of training documents names
                trainingDocsNames = trainingDocs;
            }
            //Reset the Dom Pool Vars
            TargetNodes = new HashSet<HtmlNode>();
            TargetNodesPrecision = new HashSet<HtmlNode>();
            NonTargetNodes = new HashSet<HtmlNode>();

            TESTTargetNodes = new HashSet<HtmlNode>();
            TESTTargetNodesPrecision = new HashSet<HtmlNode>();
            TESTNonTargetNodes = new HashSet<HtmlNode>();

            TESTSeenTargetNodes = new HashSet<HtmlNode>();
            TESTSeenTargetNodesPrecision = new HashSet<HtmlNode>();
            TESTSeenNonTargetNodes = new HashSet<HtmlNode>();
            //Reset the query result cache
            queryResultCache = new Dictionary<string, HashSet<HtmlNode>>();

            int minSelected = 100;

            foreach (String srcDomName in trainingDocsNames)
            {
                HtmlNode srcDom = null;
                docsAndNames.TryGetValue(srcDomName, out srcDom);
                HtmlNodeCollection selected = srcDom.SelectNodes("//*[@" + selectionAttribute + "]");
                if (selected==null||selected.Count <= 0) { continue; }
                if (selected.Count() < minSelected)
                {
                    minSelected = selected.Count();
                }
            }

                foreach (String srcDomName in trainingDocsNames) {
                HtmlNode srcDom = null;
                docsAndNames.TryGetValue(srcDomName, out srcDom);
                HtmlNodeCollection selected = srcDom.SelectNodes("//*[@" + selectionAttribute + "]");
                if (selected == null) { continue; }
                TargetNodes.UnionWith(selected.Take(minSelected));
                TargetNodesPrecision.UnionWith(selected);
                HtmlNodeCollection selectedChildren = srcDom.SelectNodes("//*[@" + selectionAttribute + "]//* | //*[@" + optionalSelectionAttribute + "] | //*[@" + optionalSelectionAttribute + "]//*");
                if (selectedChildren != null)
                {
                    TargetNodesPrecision.UnionWith(selectedChildren);
                }
                //select the rest and add them to 
                HtmlNodeCollection all = srcDom.SelectNodes("//*");
                HashSet<HtmlNode> nonTarget = new HashSet<HtmlNode>(all);
                nonTarget.ExceptWith(selected);
                if (selectedChildren != null)
                {
                    nonTarget.ExceptWith(selectedChildren);
                }
                NonTargetNodes.UnionWith(nonTarget);
            }

            foreach (String srcDomName in allDocsNames.Except(trainingDocsNames))
            {
                HtmlNode srcDom = null;
                docsAndNames.TryGetValue(srcDomName, out srcDom);
                HtmlNodeCollection selected = srcDom.SelectNodes("//*[@" + selectionAttribute + "]");
                TESTTargetNodes.UnionWith(selected);
                TESTTargetNodesPrecision.UnionWith(selected);
                HtmlNodeCollection selectedChildren = srcDom.SelectNodes("//*[@" + selectionAttribute + "]//* | //*[@" + optionalSelectionAttribute + "] | //*[@" + optionalSelectionAttribute + "]//*");
                if (selectedChildren != null)
                {
                    TESTTargetNodesPrecision.UnionWith(selectedChildren);
                }
                //select the rest and add them to 
                HtmlNodeCollection all = srcDom.SelectNodes("//*");
                HashSet<HtmlNode> nonTarget = new HashSet<HtmlNode>(all);
                nonTarget.ExceptWith(selected);
                if (selectedChildren != null)
                {
                    nonTarget.ExceptWith(selectedChildren);
                }
                TESTNonTargetNodes.UnionWith(nonTarget);
            }

            if (testDocsAndNames.Count() > 0)
            {
                foreach (String srcDomName in testDocsAndNames.Keys.Intersect(trainingDocsNames))
                {
                    HtmlNode srcDom = null;
                    testDocsAndNames.TryGetValue(srcDomName, out srcDom);
                    HtmlNodeCollection selected = srcDom.SelectNodes("//*[@" + selectionAttribute + "]");
                    TESTSeenTargetNodes.UnionWith(selected);
                    TESTSeenTargetNodesPrecision.UnionWith(selected);
                    HtmlNodeCollection selectedChildren = srcDom.SelectNodes("//*[@" + selectionAttribute + "]//* | //*[@" + optionalSelectionAttribute + "] | //*[@" + optionalSelectionAttribute + "]//*");
                    if (selectedChildren != null)
                    {
                        TESTSeenTargetNodesPrecision.UnionWith(selectedChildren);
                    }
                    //select the rest and add them to 
                    HtmlNodeCollection all = srcDom.SelectNodes("//*");
                    HashSet<HtmlNode> nonTarget = new HashSet<HtmlNode>(all);
                    nonTarget.ExceptWith(selected);
                    if (selectedChildren != null)
                    {
                        nonTarget.ExceptWith(selectedChildren);
                    }
                    TESTSeenNonTargetNodes.UnionWith(nonTarget);
                }
            }


        }

        public static void ExtractAllFeatures() {
            OrderFeatures = new HashSet<Feature>();
            SelectorFeatures = new HashSet<Feature>();
            FeatureFrequencey = new Dictionary<string, int>();

            foreach (String docName in trainingDocsNames)
            {
                
                HtmlNode src = null;
                docsAndNames.TryGetValue(docName, out src);
                HashSet<HtmlNode> docNodes = new HashSet<HtmlNode>(src.SelectNodes("//*"));

                //extractFeatures
                HashSet<Feature> extacted = new HashSet<Feature>();
                if (docsFeatures.ContainsKey(docName))
                {
                    docsFeatures.TryGetValue(docName, out extacted);
                }
                else {
                    HashSet<HtmlNode> currNodes = new HashSet<HtmlNode>(docNodes.Intersect(TargetNodes));


                    foreach (HtmlNode curr in currNodes)
                    {
                        extacted.UnionWith(FeatureExtraction.Extract(curr));
                    }
                    docsFeatures.Add(docName, extacted);
                }

                foreach (Feature currFeature in extacted)
                {
                    if (currFeature.type.Equals(Feature.FeatureType.Order))
                    {
                        OrderFeatures.Add(currFeature);
                    }
                    else
                    {
                        SelectorFeatures.Add(currFeature);
                        int val = 0;
                        if (FeatureFrequencey.TryGetValue(currFeature.feature.First().ToLower(), out val))
                        {
                            FeatureFrequencey.Remove(currFeature.feature.First().ToLower());
                            FeatureFrequencey.Add(currFeature.feature.First().ToLower(), val + 1);
                        }
                        else
                        {
                            FeatureFrequencey.Add(currFeature.feature.First().ToLower(), 1);
                        }
                    }
                }
                
            }
            int thresholdNumber = 2;
            if (trainingDocsNames.Count() < 2)
            {
                thresholdNumber = 1;
            }

            var sharedFeatures = new HashSet<String>(FeatureFrequencey.Where(x => (x.Value >=  thresholdNumber/*(docs.Count())*/)).Select(x => x.Key));
            HashSet<Feature> selectorFeaturesFiltered = new HashSet<Feature>();
            foreach (Feature f in SelectorFeatures)
            {
                if (sharedFeatures.Contains(f.feature.First().ToLower()))
                {
                    selectorFeaturesFiltered.Add(f);
                }
            }

            SelectorFeatures = selectorFeaturesFiltered;
        }


        public static HashSet<HtmlNode> RunXpathQuery(string xpath) {

            HashSet<HtmlNode> res = new HashSet<HtmlNode>();
            lock (queryResultCache)
            {
                if (queryResultCache.ContainsKey(xpath))
                {
                    queryResultCache.TryGetValue(xpath, out res);
                    return res;
                }
            }


            //foreach (HtmlNode srcDom in docs)
            //{
            Parallel.ForEach(trainingDocsNames, (docName) => {

                HtmlNode srcDom = docsAndNames[docName];

                HashSet<HtmlNode> currDocRes = null;

                if (docsQueryCache[docName].ContainsKey(xpath)) {
                    lock (docsQueryCache[docName])
                    {
                        currDocRes = docsQueryCache[docName][xpath];
                    }
                }else
                {
                    HtmlNodeCollection selected = srcDom.SelectNodes(xpath);
                    currDocRes = new HashSet<HtmlNode>();
                    if (selected != null)
                    {
                        currDocRes.UnionWith(selected);
                    }

                    lock (docsQueryCache[docName])
                    {
                        docsQueryCache[docName].Add(xpath, currDocRes);
                    }
                }

                if (currDocRes.Count()>0)
                {
                    lock (res)
                    {
                        res.UnionWith(currDocRes);
                    }
                }
            });
            //}

            if (!(xpath.Contains(" and ")|| xpath.Contains(" or ")))
            {
                lock (queryResultCache)
                {
                    queryResultCache.Add(xpath, res);
                }
            }

            return res;
        }

        public static HashSet<HtmlNode> TESTRunXpathQuery(string xpath)
        {
            HashSet<HtmlNode> res = new HashSet<HtmlNode>();

            Parallel.ForEach(allDocsNames.Except(trainingDocsNames), (docName) => {

                HtmlNode srcDom = null;
               
                if (!docsAndNames.ContainsKey(docName))
                {
                    srcDom = testDocsAndNames[docName];
                }else
                {
                    srcDom = docsAndNames[docName];
                }

                HashSet<HtmlNode> currDocRes = null;

                if (docsQueryCache[docName].ContainsKey(xpath))
                {
                    lock (docsQueryCache[docName])
                    {
                        currDocRes = docsQueryCache[docName][xpath];
                    }
                }
                else
                {
                    HtmlNodeCollection selected = srcDom.SelectNodes(xpath);
                    currDocRes = new HashSet<HtmlNode>();
                    if (selected != null)
                    {
                        currDocRes.UnionWith(selected);
                    }

                    lock (docsQueryCache[docName])
                    {
                        docsQueryCache[docName].Add(xpath, currDocRes);
                    }
                }

                if (currDocRes.Count() > 0)
                {
                    lock (res)
                    {
                        res.UnionWith(currDocRes);
                    }
                }
            });

            return res;
        }


        public static HashSet<HtmlNode> TESTSeenRunXpathQuery(string xpath) {
            HashSet<HtmlNode> res = new HashSet<HtmlNode>();

            Parallel.ForEach(trainingDocsNames, (docName) => {

                HtmlNode srcDom = testDocsAndNames[docName];

                HashSet<HtmlNode> currDocRes = null;

                if (testDocsQueryCache[docName].ContainsKey(xpath))
                {
                    lock (testDocsQueryCache[docName])
                    {
                        currDocRes = testDocsQueryCache[docName][xpath];
                    }
                }
                else
                {
                    HtmlNodeCollection selected = srcDom.SelectNodes(xpath);
                    currDocRes = new HashSet<HtmlNode>();
                    if (selected != null)
                    {
                        currDocRes.UnionWith(selected);
                    }

                    lock (testDocsQueryCache[docName])
                    {
                        testDocsQueryCache[docName].Add(xpath, currDocRes);
                    }
                }

                if (currDocRes.Count() > 0)
                {
                    lock (res)
                    {
                        res.UnionWith(currDocRes);
                    }
                }
            });

            return res;
        }





    }
}
