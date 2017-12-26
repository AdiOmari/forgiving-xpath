using HtmlAgilityPack;
using RobustXpathSynthesis.src.alignment_baseline;
using RobustXpathSynthesis.src.decisiontree;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.utilities;
using RobustXpathSynthesis.src.weka_baseline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.phases
{
    class OverallArchive2Testing
    {

        public static void RunTest(string filesLocation)
        {

            Dictionary<string, double> overalliRes = new Dictionary<string, double>();
            string[] folders = Directory.GetDirectories(filesLocation);
            foreach (string fldr in folders)
            {
                Console.WriteLine("Running for site:" + fldr);
                string[] innerfolders = Directory.GetDirectories(fldr);
                foreach (string innerdir in innerfolders)
                {
                    Console.Out.Flush();
                    Console.WriteLine("Running for att:" + innerdir);


                    DomPool.LoadDocuments(innerdir);
                    //for(int i= (DomPool.allDocsNames.Count() - 1); i <= (DomPool.allDocsNames.Count()-1)/*DomPool.allDocsNames.Count()*/; i++)
                    for (int i = 1; i <= 1/*(DomPool.allDocsNames.Count() - 1)*/; i++)
                    {
                        string[] tools = new string[] { "our", "our-not-forgiving", "j48", "nb", "xpath-align", "svm"};
                        int toolStart = 0;
                        Dictionary<string, string> xpathNonForgiving = new Dictionary<string, string>();
                        for (int tool = toolStart; tool < 1; tool++)
                        {
                            Console.WriteLine("[-] running for training set size=" + i);
                            IEnumerable<IEnumerable<int>> subsetsIndexes = SequenceUpToAndOthers(DomPool.allDocsNames.Count(), i);/*Subsets(DomPool.allDocsNames.Count(), i);*/
                            //Reduce size ...for testing only
                            //subsetsIndexes = subsetsIndexes.Take(30);
                            double totalAccuracy = 0;
                            double totalRecall = 0;
                            long totalTime = 0;
                            Console.WriteLine("[-] tool:" + tools[tool]);
                            Console.WriteLine("+ will run " + subsetsIndexes.Count() + " different iterations for the current set size");
                            int s = 0;
                            Dictionary<String, double> SiteTotalRecall = new Dictionary<string, double>();
                            Dictionary<String, double> SiteTotalPrecision = new Dictionary<string, double>();
                            Dictionary<String, double> SiteTotalTests = new Dictionary<string, double>();
                            foreach (string site in DomPool.allDocsNames)
                            {
                                SiteTotalPrecision[site] = 0;
                                SiteTotalRecall[site] = 0;
                                SiteTotalTests[site] = 0;
                            }


                            foreach (IEnumerable<int> currSubsetIndexes in subsetsIndexes)
                            {
                                List<int> listRep = new List<int>(currSubsetIndexes);
                                string stringRep = listRep.Aggregate("", (b, x) => b + "," + x);
                                s++;
                                if (s % 10 == 0)
                                {
                                    //Console.Write("(" + s + "/" + subsetsIndexes.Count() + ") ");

                                    Console.Write(".");
                                }
                                //if (tool == toolStart)
                                //{
                                HashSet<String> currSubset = GetSubSet(DomPool.allDocsNames, currSubsetIndexes);
                                DomPool.Initiate(currSubset);
                                DomPool.ExtractAllFeatures();
                                //}
                                var runres = new HashSet<HtmlNode>();
                                //our method
                                if (tool < 2)
                                {
                                    string xpath = "";
                                    if (tool == 0) {
                                        DecisionNode dn = new DecisionNode();
                                        dn.InitialNodeSet = new HashSet<HtmlNode>(DomPool.TargetNodes.Union(DomPool.NonTargetNodes));
                                        dn.SelectedNegative = new HashSet<HtmlNode>(DomPool.NonTargetNodes.Except(DomPool.TargetNodesPrecision));
                                        dn.SelectedPositive = new HashSet<HtmlNode>(DomPool.TargetNodes);
                                        dn.FeatureSet = new HashSet<Feature>();
                                        dn.CalculateEntropy();

                                        DecisionTreeLearning.RecursiveTreeImprovement(dn);


                                        xpath = XpathTools.GenerateAForgivingXpath(dn);

                                        xpathNonForgiving[stringRep] = XpathTools.DecisionTreeToXpath(dn, new HashSet<Feature>(), 1);
                                        xpathNonForgiving[stringRep] = "//*" + (xpathNonForgiving[stringRep].Equals("") ? "" : ("[" + xpathNonForgiving[stringRep] + "]"));
                                    }

                                    if (tool == 1)
                                    {
                                        xpath = xpathNonForgiving[stringRep];
                                    }

                                    Console.WriteLine("Query:" + xpath);

                                    var watch = Stopwatch.StartNew();
                                    runres = DomPool.TESTRunXpathQuery(xpath);
                                    watch.Stop();
                                    var elapsedMs = watch.ElapsedMilliseconds;
                                    totalTime = totalTime + elapsedMs;
                                }
                                else
                                {
                                    if (tool == 2)
                                    {
                                        ModelLearner model = new ModelLearner();
                                        model.LearnModel();
                                        var watch = Stopwatch.StartNew();
                                        runres = model.RunOnTestSet();

                                        watch.Stop();
                                        var elapsedMs = watch.ElapsedMilliseconds;
                                        totalTime = totalTime + elapsedMs;
                                    }
                                    else
                                    {
                                        if (tool == 3)
                                        {
                                            NB model = new NB();
                                            model.LearnModel();
                                            var watch = Stopwatch.StartNew();
                                            runres = model.RunOnTestSet();
                                            watch.Stop();
                                            var elapsedMs = watch.ElapsedMilliseconds;
                                            totalTime = totalTime + elapsedMs;
                                        }
                                        else
                                        {
                                            if (tool == 4)
                                            {
                                                XpathAlignment model = new XpathAlignment();
                                                model.LearnModel();
                                                var watch = Stopwatch.StartNew();
                                                runres = model.RunOnTestSet();
                                                watch.Stop();
                                                var elapsedMs = watch.ElapsedMilliseconds;
                                                totalTime = totalTime + elapsedMs;
                                            }
                                            else
                                            {
                                                SVM model = new SVM();
                                                model.LearnModel();
                                                var watch = Stopwatch.StartNew();
                                                runres = model.RunOnTestSet();
                                                watch.Stop();
                                                var elapsedMs = watch.ElapsedMilliseconds;
                                                totalTime = totalTime + elapsedMs;
                                            }


                                        }
                                    }
                                }


                                HashSet<HtmlNode> spos = new HashSet<HtmlNode>(DomPool.TESTTargetNodes.Intersect(runres));
                                HashSet<HtmlNode> sposprecision = new HashSet<HtmlNode>(DomPool.TESTTargetNodesPrecision.Intersect(runres));

                                foreach (var entry in DomPool.docsAndNames)
                                {
                                    if (DomPool.trainingDocsNames.Contains(entry.Key))
                                    {
                                        continue;
                                    }

                                    HashSet<HtmlNode> docNodes = new HashSet<HtmlNode>(entry.Value.SelectNodes("//*"));
                                    HashSet<HtmlNode> currspos = new HashSet<HtmlNode>(spos.Intersect(docNodes));
                                    HashSet<HtmlNode> currrunres = new HashSet<HtmlNode>(runres.Intersect(docNodes));
                                    HashSet<HtmlNode> currsposprecision = new HashSet<HtmlNode>(sposprecision.Intersect(docNodes));
                                    HashSet<HtmlNode> currTargetNodes = new HashSet<HtmlNode>(DomPool.TESTTargetNodes.Intersect(docNodes));
                                    double currSiteAccuracy = (currsposprecision.Count() / ((double)currrunres.Count()));
                                    double currSiteRecall = (currspos.Count() / ((double)currTargetNodes.Count()));
                                    if (((double)currrunres.Count()) > 0)
                                    {
                                        SiteTotalPrecision[entry.Key] = SiteTotalPrecision[entry.Key] + currSiteAccuracy;
                                        SiteTotalRecall[entry.Key] = SiteTotalRecall[entry.Key] + currSiteRecall;
                                    }

                                    SiteTotalTests[entry.Key] = SiteTotalTests[entry.Key] + 1;
                                }

                                double currAccuracy = (sposprecision.Count() / ((double)runres.Count()));
                                double currRecall = (spos.Count() / ((double)DomPool.TESTTargetNodes.Count()));
                                if (runres.Count() > 0)
                                {
                                    totalAccuracy = totalAccuracy + currAccuracy;
                                    totalRecall = totalRecall + currRecall;
                                }
                            }

                            totalAccuracy = totalAccuracy / subsetsIndexes.Count();
                            totalRecall = totalRecall / subsetsIndexes.Count();
                            Console.WriteLine("########## Results " + tools[tool] + " for i=" + i + "##########");

                            Console.WriteLine("+++++++++ Detailed Results for i=" + i + "++++++++++#");
                            double count = 0;
                            double totalSumPrecision = 0;
                            double totalSumRecall = 0;
                            double avgRecall = 0;
                            double avgPrecision = 0;
                            double avgFscore = 0;
                            double numPrecision = 0;

                            foreach (string site in DomPool.allDocsNames)
                            {


                                if (SiteTotalTests[site] < 1)
                                { continue; } // SiteTotalTests[site]++; }
                                else
                                {
                                    numPrecision++;
                                }

                                double sitePrecision = SiteTotalPrecision[site] / SiteTotalTests[site];
                                double siteRecall = SiteTotalRecall[site] / SiteTotalTests[site];
                                double siteFscore = 2 * (sitePrecision * siteRecall) / (sitePrecision + siteRecall);
                                if (siteRecall == 0 && sitePrecision == 0)
                                {
                                    siteFscore = 0;
                                }

                                count++;
                                avgRecall = avgRecall + siteRecall;
                                avgPrecision = avgPrecision + sitePrecision;
                                avgFscore = avgFscore + siteFscore;

                                // Console.WriteLine(">" + site + ": Precision:" + sitePrecision + " , Recall:" + siteRecall+", F-score:"+siteFscore);
                            }
                            Console.WriteLine("++++++++++++++++Total+++++++++++++++++");
                            avgRecall = avgRecall / count;
                            avgPrecision = avgPrecision / numPrecision;
                            avgFscore = avgFscore / count;

                            Console.WriteLine("Recall:" + avgRecall);
                            Console.WriteLine("Precision:" + avgPrecision);
                            Console.WriteLine("F-score:" + avgFscore);
                            Console.WriteLine("Time:" + totalTime);

                            if (overalliRes.ContainsKey(i + ":" + tool + ":recall"))
                            {
                                overalliRes[i + ":" + tool + ":recall"] = overalliRes[i + ":" + tool + ":recall"] + avgRecall;
                                overalliRes[i + ":" + tool + ":precision"] = overalliRes[i + ":" + tool + ":precision"] + avgPrecision;
                                overalliRes[i + ":" + tool + ":fscore"] = overalliRes[i + ":" + tool + ":fscore"] + avgFscore;
                            }
                            else
                            {
                                overalliRes[i + ":" + tool + ":recall"] =  avgRecall;
                                overalliRes[i + ":" + tool + ":precision"] = avgPrecision;
                                overalliRes[i + ":" + tool + ":fscore"] = avgFscore;
                            }
                        }
                    }

                    



                }
            }

            Console.WriteLine("############### OVER ALL #################");
            foreach(var kv in overalliRes)
            {
                Console.WriteLine(kv.Key + " = " + (kv.Value / 11));
            }

            Console.ReadLine();


        }

        private static HashSet<String> GetSubSet(List<String> files, IEnumerable<int> subset)
        {
            HashSet<String> res = new HashSet<string>();
            foreach (int index in subset)
            {
                res.Add(files.ElementAt(index - 1));
            }
            return res;

        }

        //gets all subsets of size "subsetSize" of numbers 1....n
        private static IEnumerable<IEnumerable<int>> Subsets(int n, int subsetSize)
        {
            IEnumerable<int> sequence = Enumerable.Range(1, n);

            // generate list of sequences containing only 1 element e.g. {1}, {2}, ...
            var oneElemSequences = sequence.Select(x => new[] { x }).ToList();

            // generate List of int sequences
            var result = new List<List<int>>();
            // add initial empty set
            result.Add(new List<int>());

            // generate powerset, but skip sequences that are too long
            foreach (var oneElemSequence in oneElemSequences)
            {
                int length = result.Count;

                for (int i = 0; i < length; i++)
                {
                    if (result[i].Count >= subsetSize)
                        continue;

                    result.Add(result[i].Concat(oneElemSequence).ToList());
                }
            }

            return result.Where(x => x.Count == subsetSize);
        }


        //gets all subsets of size "subsetSize" of numbers 1....n
        private static IEnumerable<IEnumerable<int>> SequenceUpToAndOthers(int n, int i)
        {
            int[] others = new int[] { 6, 7, 8, 9, 10, 11 };
           // IEnumerable<int> sequence = Enumerable.Range(1, 5);
            var oneElemSequences = Subsets(5, 2);//sequence.Select(x => new[] { x }).ToList();
            var oneElemSequencesAndOthers = oneElemSequences.Select(x => x.Concat(others)).ToList();
            return oneElemSequencesAndOthers;
        }
    
    }
}
