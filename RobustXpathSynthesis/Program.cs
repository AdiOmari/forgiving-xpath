using HtmlAgilityPack;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.parse_results;
using RobustXpathSynthesis.src.phases;
using RobustXpathSynthesis.src.testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis
{
    class Program
    {

        const int READLINE_BUFFER_SIZE = 128384;
        private static string ReadLine()
        {
            Stream inputStream = Console.OpenStandardInput(READLINE_BUFFER_SIZE);
            byte[] bytes = new byte[READLINE_BUFFER_SIZE];
            int outputLength = inputStream.Read(bytes, 0, READLINE_BUFFER_SIZE);
            //Console.WriteLine(outputLength);
            char[] chars = Encoding.UTF7.GetChars(bytes, 0, outputLength);
            return new string(chars);
        }

        public static string FILES_LOCATION = "files";
        public static string ARCHIVE_FILES_LOCATION = "archive2";
        static void Main(string[] args)
        {
            Console.WriteLine("T for test, R for Run, S for seen overall testing and O for overall testing:");
            string res = ReadLine();

            if (res.ToLower().Trim().Equals("huge"))
            {

                TestSites.TestAllSites();
                return;
            }

            if (res.ToLower().Trim().Equals("t"))
            {
                DomPool.LoadDocuments(FILES_LOCATION);
                DomPool.Initiate();

                Console.WriteLine("insert query:");
                string q = ReadLine();
                while (!q.Equals("exit"))
                {
                    var runres = DomPool.RunXpathQuery(q);
                    if (runres != null)
                    {
                        Console.WriteLine("result size" + runres.Count());
                        HashSet<HtmlNode> spos = new HashSet<HtmlNode>(DomPool.TargetNodes.Intersect(runres));
                        HashSet<HtmlNode> sposprecision = new HashSet<HtmlNode>(DomPool.TargetNodesPrecision.Intersect(runres));
                        foreach(var entry in DomPool.docsAndNames)
                        {
                            HashSet<HtmlNode> docNodes = new HashSet<HtmlNode>(entry.Value.SelectNodes("//*"));
                            HashSet<HtmlNode> currspos = new HashSet<HtmlNode>(spos.Intersect(docNodes));
                            HashSet<HtmlNode> currrunres = new HashSet<HtmlNode>(runres.Intersect(docNodes));
                            HashSet<HtmlNode> currsposprecision = new HashSet<HtmlNode>(sposprecision.Intersect(docNodes));
                            HashSet<HtmlNode> currTargetNodes = new HashSet<HtmlNode>(DomPool.TargetNodes.Intersect(docNodes));
                            Console.WriteLine(entry.Key+"-Accuracy:" + (currsposprecision.Count() / ((double)currrunres.Count())) + ". Recall:" + (currspos.Count() / ((double)currTargetNodes.Count())) + "");
                        }

                        Console.WriteLine("Accuracy:"+(sposprecision.Count()/((double)runres.Count()))+". Recall:"+(spos.Count() / ((double)DomPool.TargetNodes.Count())) +"");
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                    Console.WriteLine("insert query:");
                    q = ReadLine();
                }
            }
            else {
                if (res.ToLower().Trim().Equals("r"))
                {
                    Console.WriteLine(LearnXpathWrapper.LearnXpathFromTrainingFiles(FILES_LOCATION));
                    Console.ReadLine();
                }
                else
                {
                    if (res.ToLower().Trim().Equals("s"))
                    {

                        Console.WriteLine("Output is redirected to resultsSeen.txt in the debug dir");
                        //write results to text file instead of windows
                        FileStream fs = new FileStream("resultsSeen.txt", FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs);
                        TextWriter tmp = Console.Out;
                        Console.SetOut(sw);

                        OverallSeenTesting.RunTest(FILES_LOCATION);
                        Console.SetOut(tmp);
                        sw.Flush();
                        sw.Close();

                    }
                    else {
                        if (res.ToLower().Trim().Equals("archive"))
                        {
                            Console.WriteLine("Output is redirected to results.txt in the debug dir");
                            //write results to text file instead of windows
                            FileStream fs = new FileStream("archive2-results.txt", FileMode.Create);
                            StreamWriter sw = new StreamWriter(fs);
                            TextWriter tmp = Console.Out;
                            Console.SetOut(sw);

                            OverallArchive2Testing.RunTest(ARCHIVE_FILES_LOCATION);
                            Console.SetOut(tmp);
                            sw.Flush();
                            sw.Close();
                        }
                        else { 
                                    if (res.ToLower().Trim().Equals("a"))
                                    {
                                        Console.WriteLine("Please enter file name to parse:");
                                        string fnp = ReadLine().Trim();
                                        parseres.learn(fnp);
                                        parseres.save("parsed"+ fnp);
                                    }
                                    else {
                                        Console.WriteLine("Output is redirected to results.txt in the debug dir");
                                        //write results to text file instead of windows
                                        FileStream fs = new FileStream("results.txt", FileMode.Create);
                                        StreamWriter sw = new StreamWriter(fs);
                                        TextWriter tmp = Console.Out;
                                        Console.SetOut(sw);

                                        OverallTesting.RunTest(FILES_LOCATION);
                                        Console.SetOut(tmp);
                                        sw.Flush();
                                        sw.Close();
                                    }
                        }
                    }

                }
            }
        }
    }
}
