using HtmlAgilityPack;
using RobustXpathSynthesis.src.decisiontree;
using RobustXpathSynthesis.src.ds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.utilities
{
    class XpathTools
    {

        public static string FeatureSetToXpath(IEnumerable<Feature> featureSet) {

            /////Need to be rewritten to optimize code and performance
            Dictionary<string, Feature> featureMapping = new Dictionary<string, Feature>();
            HashSet<string> doneFeatures = new HashSet<string>();
            HashSet<string> conditionSet = new HashSet<string>();


            foreach(Feature currFeature in featureSet)
            {
                if (currFeature.type.Equals(Feature.FeatureType.Selector))
                {
                    if (!featureMapping.ContainsKey(currFeature.feature.First()))
                    {
                        featureMapping.Add(currFeature.feature.First(), currFeature);
                    }
                }
            }

            foreach (Feature currFeature in featureSet)
            {
                if (currFeature.type.Equals(Feature.FeatureType.Order))
                {
                    if (!currFeature.sameIndex)
                    {
                        string currSelector = "";
                        foreach (string curr in currFeature.feature)
                        {
                            Feature cf = new Feature();
                            featureMapping.TryGetValue(curr, out cf);
                            if (cf.nodeNameFeature)
                            {
                                currSelector = currSelector + ((currSelector.Equals(""))?"":"//") + cf.feature.First();
                            }
                            else
                            {
                                currSelector = currSelector +((currSelector.Equals("")) ? "" : "//") + "*[" + cf.feature.First() + "]";
                            }
                        }
                        conditionSet.Add(currSelector);
                    }
                    else {
                        string currSelector = "";
                        string nodeName = "*";
                        string nodeCondition = "";
                        foreach (string curr in currFeature.feature)
                        {
                            Feature cf = new Feature();
                            featureMapping.TryGetValue(curr, out cf);
                            if (cf.nodeNameFeature)
                            {
                                nodeName = cf.feature.First();
                            }
                            else
                            {
                                nodeCondition = nodeCondition + ((nodeCondition.Equals("")) ? "" : " and ") + cf.feature.First();
                            }
                        }

                        currSelector = "" + nodeName;
                        if (!nodeCondition.Equals(""))
                        {
                            currSelector = currSelector + "[" + nodeCondition + "]";
                        }
                        conditionSet.Add(currSelector);
                    }

                    foreach(string f in currFeature.feature)
                    {
                        doneFeatures.Add(f);
                    }
                }
            }

            foreach(string k in featureMapping.Keys)
            {
                if (doneFeatures.Contains(k)) { continue; }
                doneFeatures.Add(k);
                Feature curr = new Feature();
                featureMapping.TryGetValue(k, out curr);
                if (curr.nodeNameFeature) {
                    conditionSet.Add((curr.Axe.Equals(Feature.Axes.Self) ? "self::" : "ancestor-or-self::") + curr.feature.First());
                }
                else
                {
                    conditionSet.Add((curr.Axe.Equals(Feature.Axes.Self) ? "self::" : "ancestor-or-self::")+"*[" + curr.feature.First() + "]");
                }
            }


            string xpath = "";
            if (featureSet.Count() > 1)
            {
                xpath = "//*[";
                bool firstiteration = true;
                foreach (string cond in conditionSet)
                {
                    if (!firstiteration) { xpath = xpath + " ][ "; }
                    xpath = xpath + cond;
                    firstiteration = false;
                }


                xpath = xpath + "]";
            }
            else
            {
                string currCondition = conditionSet.First();
                if (currCondition.Contains("ancestor-or-self::"))
                {
                    currCondition = currCondition.Replace("ancestor-or-self::", "");
                    xpath = "//" + currCondition + " | //" + currCondition + "//*";
                }
                else
                {
                    currCondition = currCondition.Replace("self::", "");
                    xpath = "//" + currCondition;
                }
            }


            return xpath;
        }


        public static string DecisionTreeToXpath(DecisionNode dn, HashSet<Feature> prevFeatureSet, double precisionThreshold=1,Boolean first=true)
        {

            if ((dn.SelectedPositive.Count() == 0 || dn.precision >precisionThreshold)&&first){ return "";}

            string res = "";

            HashSet<Feature> currFeature = new HashSet<Feature>(dn.FeatureSet.Except(prevFeatureSet));
            if (currFeature.Count() == 0){
                if (dn.SetSelected != null)
                {
                    Console.Write("positive with no features");
                }
                return "";
            }

            Feature cf = currFeature.First();
            string currSelector="";

            if (cf.nodeNameFeature)
            {
                currSelector = (cf.Axe.Equals(Feature.Axes.Self)?"self::":"ancestor-or-self::") + cf.feature.First();
            }
            else
            {
                currSelector = (cf.Axe.Equals(Feature.Axes.Self) ? "self::" : "ancestor-or-self::") + " *[" + cf.feature.First() + "]";
            }
           
          

            string rightSelector = "";
            if (dn.SetSelected != null && dn.SetSelected.precision <= precisionThreshold) {
                rightSelector = DecisionTreeToXpath(dn.SetSelected, dn.FeatureSet);
            }

            string leftSelector = "";
            if (dn.SetNotSelected != null)
            {
                leftSelector = DecisionTreeToXpath(dn.SetNotSelected, dn.FeatureSet, precisionThreshold,false);
            }

            res = currSelector;

            if (!rightSelector.Equals(""))
            {
                res = res + " and " + rightSelector;
            }

            if (!leftSelector.Equals(""))
            {
                HashSet<HtmlNode> fToselectFrom = new HashSet<HtmlNode>(dn.SetSelected.SelectedNegative.Union(dn.SetSelected.SelectedPositive));
               HashSet<HtmlNode> restemp =  dn.SetNotSelected.selectTrue(fToselectFrom, prevFeatureSet,false, precisionThreshold);
               HashSet<HtmlNode> resRight = new HashSet<HtmlNode>(restemp.Intersect(DomPool.TargetNodesPrecision));
                double precisionF = ((double)resRight.Count() / restemp.Count());
                if (restemp.Count() == 0)
                {
                    precisionF = 1;
                }

               HashSet<HtmlNode> toSelectFrom =  new HashSet<HtmlNode>(dn.SetNotSelected.SelectedNegative.Union(dn.SetNotSelected.SelectedPositive));
                HashSet <HtmlNode> restemp2 = dn.SetNotSelected.selectTrue(toSelectFrom, prevFeatureSet, false, precisionThreshold);
                HashSet<HtmlNode> resRight2 = new HashSet<HtmlNode>(restemp2.Intersect(DomPool.TargetNodesPrecision));
                double precisionF2 = ((double)resRight2.Count() / restemp2.Count());
                if (restemp2.Count() == 0)
                {
                    precisionF2 = 1;
                }

                double diff = precisionF2 / precisionF;

               if (diff <= 1) { //Uncomment to use the MFX
                    res = "((" + res + ") or (" + leftSelector + "))";
                }
                else
                {
                    res = "((" + res + ") or (not(" + currSelector + ") and (" + leftSelector + ")))";
                }

                //  res = "((" + res + ") or (not("+currSelector+") and (" + leftSelector + ")))";// "(("+res + ") or (" + leftSelector+"))";
                //res = "((" + res + ") or (" + leftSelector + "))";// "(("+res + ") or (" + leftSelector+"))";
            }

            return res;
        }

        public static List<double> FindInterestingPrecisionLevels(DecisionNode dn, int roughlimit=7)
        {
            if (dn == null) { return new List<double>(); }
            List<double> left = FindInterestingPrecisionLevels(dn.SetNotSelected);
            List<double> right = FindInterestingPrecisionLevels(dn.SetSelected);
            HashSet<double> res = new HashSet<double>(left);
            res.UnionWith(right);
            res.Add(dn.precision);
            List<double> resSorted = new List<double>(res);
            //Descending 
            resSorted.Sort((a, b) => -1 * a.CompareTo(b));
            if (resSorted.Count() > roughlimit)
            {
                int skipLevel = resSorted.Count() / roughlimit;
                List<double> resFinal = new List<double>();
                for(int i = 0; i < resSorted.Count(); i++)
                {
                    if (i % skipLevel == 0)
                    {
                        if (resSorted.ElementAt(i) > 0)
                        {
                            resFinal.Add(resSorted.ElementAt(i));
                        }
                    }
                }
                return resFinal;
            }

            return resSorted;
        }

        public static HashSet<List<DecisionNode>> DecisionTreeToPathList(DecisionNode dn, double recallThreshold)
        {
            HashSet<List<DecisionNode>> res = new HashSet<List<DecisionNode>>();
            if (dn.SetSelected != null)
            {
                double covered = dn.SetSelected.InitialNodeSet.Intersect(DomPool.TargetNodes).Count();
                double all = DomPool.TargetNodes.Count();
                double threshold = 2 / ((double)DomPool.trainingDocsNames.Count());

                if (DomPool.trainingDocsNames.Count() == 1)
                {
                    threshold = 1;
                }

                if ((covered / all) < threshold)
                {
                    return new HashSet<List<DecisionNode>>() { new List<DecisionNode>() { dn} };
                }
            }

            if (dn.SetSelected != null)
            {
                HashSet<List<DecisionNode>> rightLists = DecisionTreeToPathList(dn.SetSelected, recallThreshold);
                foreach(List<DecisionNode> l in rightLists)
                {
                    List<DecisionNode> toadd = new List<DecisionNode>() { dn };
                    toadd.AddRange(l);
                    res.Add(toadd);
                }
            }

            if (dn.SetNotSelected != null)
            {
                HashSet<List<DecisionNode>> leftLists = DecisionTreeToPathList(dn.SetNotSelected, recallThreshold);
                foreach (List<DecisionNode> l in leftLists)
                {
                    res.Add(l);
                }
            }

            return res;
        }

        public static string EscapeString(string input)
        {
            return input.Replace("'", "");
        }


        public static string GenerateAForgivingXpath(DecisionNode dn, int phasesLimit = 1000)
        {
           // return "//*[" + DecisionTreeToXpath(dn, new HashSet<Feature>(), 1) + "]";
            List<double> precisionLevels = FindInterestingPrecisionLevels(dn);


            string fullCondition = "";
            string lastCondition = "";
            string conditionClosing = "";
            foreach (double pl in precisionLevels)
            {
                string currCondition = "";
                string beforeCondition = "";
                if (!lastCondition.Equals(""))
                {
                    beforeCondition = " | /*[not(." + lastCondition + ")]";
                }
                string condInside = DecisionTreeToXpath(dn, new HashSet<Feature>(), pl);
                currCondition = "//*" + (condInside.Equals("") ? "" : ("[" + condInside + "]"));
                fullCondition = fullCondition + beforeCondition + currCondition;
                conditionClosing = conditionClosing + "";
                //set lastCondition to curr for the next iteration
                lastCondition = currCondition;
            }

            return fullCondition;//+conditionClosing+"]";
        }

    }
}
