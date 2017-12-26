using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace RobustXpathSynthesis.src.parse_results
{
    class parseres
    {
        public static Dictionary<string, string> resAnalyzed = new Dictionary<string, string>();
        public static Dictionary<string, string[]> catAtts = new Dictionary<string, string[]>();
        public static void learn(string path)
        {
            string fileString = File.ReadAllText(path);
            Regex reg = new Regex("Time:[\\d]*");
            string[] splits = reg.Split(fileString);
            foreach (string split in splits) {
                string i = Regex.Match(split, @"\+\+\+\+\+\+\+\+\+ Detailed Results for i=\d").Value.Replace("+++++++++ Detailed Results for i=", "");
                string tool = Regex.Match(split, @"\[\-\] tool:[\S]*").Value.Replace("[-] tool:", "");
                string recall = Regex.Match(split, @"^Recall:[0-9\.]*", RegexOptions.Multiline).Value.Replace("Recall:", "");
                string precision = Regex.Match(split, @"^Precision:[0-9\.]*", RegexOptions.Multiline).Value.Replace("Precision:", "");
                string fscore = Regex.Match(split, @"^F\-score:[0-9\.]*", RegexOptions.Multiline).Value.Replace("F-score:", "");
                string cat = Regex.Match(split, @">[\\a-z]+").Value.Replace(">", "");
                string attr = Regex.Match(split, @">[\\a-z]+").Value.Replace(".html", "");
                if (resAnalyzed.ContainsKey(tool + ":" + cat + ":" + attr + ":" + i))
                {
                    resAnalyzed[tool + ":" + cat + ":" + attr + ":" + i] = resAnalyzed[tool + ":" + cat + ":" + attr + ":" + i] + "," + /*precision + "," +*/ recall /*+ "," + fscore*/;
                }
                else {
                    resAnalyzed.Add(tool + ":" + cat + ":" + attr + ":" + i, /*precision +""+ "," +*/ recall+"" /*+ "," + ""+fscore*/);
                }
            }
        }

        public static void save(string path)
        {
            string res = "";
            string[] tools = new string[] { "our", "our-not-forgiving", "j48", "nb","svm", "xpath-align"};
            catAtts = new Dictionary<string, string[]>() { /*{ "book", new string[] { "author", "price", "title" } }, { "shoppings", new string[] { "price", "title" } }, { "hotel", new string[] { "address", "price", "title" } }, { "movie", new string[] { "actor", "genre", "title" } },*/ { "", new string[] { ""/*@"archive\barnesandnoble\author", @"archive\barnesandnoble\price", @"archive\barnesandnoble\title", @"archive\infibeam\price", @"archive\infibeam\title", @"archive\powells\author", @"archive\powells\price", @"archive\powells\title", @"archive\thriftbooks\author", @"archive\thriftbooks\price", @"archive\thriftbooks\title" */} } };
            foreach (string tool in tools)
            {
                res = res + "\n\n" + tool + ":";
                foreach (var cat in catAtts)
                {
                  //  res = res + "\n" + cat.Key;
                    foreach (string attr in cat.Value)
                    {
                        res = res + "\n"+attr + " ";
                        for (int i = 1; i <= 4; i++)
                        {
                            res = res + resAnalyzed[tool + ":" + cat.Key + ":" + attr + ":" + i]+",";
                            res = res + "\n";
                        }
                    }
                }

            }
            File.WriteAllText(path, res);
        }

    }
}
