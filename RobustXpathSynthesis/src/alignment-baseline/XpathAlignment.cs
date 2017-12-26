using HtmlAgilityPack;
using RobustXpathSynthesis.src.ds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.alignment_baseline
{
    class XpathAlignment
    {

        public string xpath = "";
        public void LearnModel()
        {
            List<HtmlNode> tn = new List<HtmlNode>(DomPool.TargetNodes);
            List<string[]> tnXpaths = new List<string[]>();

            foreach(HtmlNode currNode in tn)
            {
                tnXpaths.Add(getXpathOfElement(currNode).Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (tnXpaths.Count() < 2)
            {
                string restemp = "/";
                for (int i = 0; i < tnXpaths.First().Length; i++)
                {
                    restemp = restemp + "/" + tnXpaths.First()[i];
                }
                xpath = restemp;
                return;
            }

            double[,] dist = new double[tn.Count(), tn.Count()];
            int minI = 0;
            int minJ = 0;
            double minVal = double.MaxValue;
            double[,] minD = null;
            for(int i=0;i< tn.Count(); i++)
            {
                dist[i, i] = 0;
                for(int j = 0; j < i; j++)
                {
                    double[,] d = alignment(tnXpaths.ElementAt(i), tnXpaths.ElementAt(j));
                    dist[i, j] = dist[j, i] = d[tnXpaths.ElementAt(i).Length, tnXpaths.ElementAt(j).Length];
                    if (dist[i, j] < minVal)
                    {
                        minVal = dist[i, j];
                        minI = i;
                        minJ = j;
                        minD = d;
                    }
                }
            }

            HashSet<int> doneWith = new HashSet<int>() { minI, minJ };
            string[] curr = MakeAlignment(tnXpaths.ElementAt(minI), tnXpaths.ElementAt(minJ),minD);

            while (doneWith.Count() < tnXpaths.Count())
            {
                double minDist = double.MaxValue;
                int minIndex = 0;
                for(int i = 0; i < tnXpaths.Count(); i++)
                {
                    if (doneWith.Contains(i))
                    {
                        continue;
                    }
                    double distAvg = 0;
                    foreach(int c in doneWith)
                    {
                        distAvg = distAvg + dist[i, c];
                    }
                    distAvg = distAvg / doneWith.Count();
                    if (distAvg < minDist)
                    {
                        minDist = distAvg;
                        minIndex = i;
                    }
                }
                double[,] dcurr = alignment(curr, tnXpaths.ElementAt(minIndex));
                curr = MakeAlignment(curr, tnXpaths.ElementAt(minIndex), dcurr);
                doneWith.Add(minIndex);
            }

            string res = "/";
            for(int i = 0; i < curr.Length; i++)
            {
                res = res + "/" + curr[i];
            }

            Regex reg = new Regex("[/]{3,100}");
            res = reg.Replace(res, "//");
            if (res.EndsWith("//"))
            {
                Regex regeol = new Regex("//$");
                res = regeol.Replace(res, "");

            }

            Regex indexReg = new Regex("\\[[0-9]+\\]");

            xpath = res+"| /*[not("+res+")]"+indexReg.Replace(res,"");

        }
        
        
        public HashSet<HtmlNode> RunOnTestSet()
        {
            return DomPool.TESTRunXpathQuery(xpath);
        }

        public HashSet<HtmlNode> RunOnTestSeenSet()
        {
            return DomPool.TESTSeenRunXpathQuery(xpath);
        }

        public static string getXpathOfElement(HtmlNode item)
        {
            string path = "";
            while(item != null && !item.Name.Contains("#"))
            {
                string itemselector = "/" + item.Name;
                int index = getIndex(item);
                if (index > 0) {
                    itemselector = itemselector+ "[" + index + "]";
                }

                path = itemselector + path;
                item = item.ParentNode;
            }
            return path;
        }

        public static int getIndex(HtmlNode item)
        {
            
            if (item.ParentNode != null)
            {
                int i = 1;
                HtmlNode parent = item.ParentNode;
                foreach(HtmlNode chld in parent.ChildNodes){
                    if (chld == item)
                    {
                        return i;
                    }
                    if (chld.Name.Equals(item.Name))
                    {
                        i++;
                    }
                }
                return i;
            }
            else
            {
                return -1;
            }
        }

        public static double[,] alignment(string[] left,string[] right)
        {
            double[,] d = new double[left.Length+1,right.Length+1];
            for(int i = 0; i <= left.Length; i++)
            {
                d[i, 0] = i*2;
            }

            for (int j = 0; j <= right.Length; j++)
            {
                d[0, j] = 0*2;
            }

            for(int i = 1; i <= left.Length; i++)
            {
                for(int j = 1; j <= right.Length; j++)
                {
                    double optionSkipLeft = 2+d[i - 1, j];
                    double optionSkipRight = 2 + d[i, j-1];
                    double optionAlign = d[i - 1, j - 1] + stepAlignCost(left[i - 1], right[j - 1]);
                    d[i, j] = Math.Min(optionAlign, Math.Min(optionSkipLeft, optionSkipRight));
                }
            }

            return d;

        }

        public static string[] MakeAlignment(string[] left,string[] right, double[,] d)
        {
            LinkedList<string> resList = new LinkedList<string>();
            int i = left.Length;
            int j = right.Length;
            while(j>0 || i > 0)
            {
                
                    if(i>0&&j>0&&(d[i,j] == (d[i - 1, j - 1] + stepAlignCost(left[i - 1], right[j - 1]))))
                    {
                        resList.AddFirst(stepAlign(left[i - 1], right[j - 1]));
                        i--; j--;
                    }
                    else
                    {
                        if(i>0&&(d[i,j] == (d[i - 1, j] + 2)))
                        {
                            resList.AddFirst("/");
                            i--;
                        }
                        else
                        {
                            resList.AddFirst("/");
                            j--;
                        }
                    }
                
            }
            return resList.ToArray();
        }

        public static double stepAlignCost(string step1, string step2)
        {
            string[] step1Split = step1.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);
            string[] step2Split = step2.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);
            double res = 0;
            if (!step1Split[0].Equals(step2Split[0]))
            {
                res = 1;
            }

            if (step1Split.Length < 2 && step2Split.Length < 2)
            {
                return res;
            }

            if (step2Split.Length != step1Split.Length || !step1Split[1].Equals(step2Split[1]))
            {
                res = res+1;
            }

            return res;
            
        }

        public static string stepAlign(string step1, string step2)
        {
            string[] step1Split = step1.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);
            string[] step2Split = step2.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);
            string res = "";
            if (step1Split[0].Equals(step2Split[0]))
            {
                res = step1Split[0];
            }
            else
            {
                res = "*";
            }

            if(step1Split.Length<2&& step2Split.Length<2)
            {
                return res;
            }

            if(step2Split.Length != step1Split.Length || !step1Split[1].Equals(step2Split[1]))
            {
                return res;
            }
            else
            {
                return res + "[" + step1Split[1];
            }
        }

    }
}
