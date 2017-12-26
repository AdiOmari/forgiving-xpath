using HtmlAgilityPack;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.decisiontree
{
    class DecisionTreeLearning
    {
        public static bool FixEnabledForNotBranch = true;

        public static void ImproveTree(DecisionNode dn,int level)
        {
            double maxScore = 0;
            Feature maxGainFeature = null; ;
            HashSet<HtmlNode> newFeatureSelected = null;
            Object lockObj = new object();
            double balanceFix = Math.Max(1,(Math.Pow(0.3, Math.Sqrt(level+1)))*(DomPool.NonTargetNodes.Count() / DomPool.TargetNodes.Count()));
            double dnEntropy = dn.CalculateEntropy(1, balanceFix);
               
                Parallel.ForEach(DomPool.SelectorFeatures, (currCandidate) =>
                {



                        HashSet<Feature> newSelectorSet = new HashSet<Feature>(dn.FeatureSet);
                        newSelectorSet.Add(currCandidate);
                        string currFeatureXpath = XpathTools.FeatureSetToXpath(new HashSet<Feature>() { currCandidate });
                        HashSet<HtmlNode> currFeatureXpathSelected = DomPool.RunXpathQuery(currFeatureXpath);
                        HashSet<HtmlNode> xpathSelected = new HashSet<HtmlNode>(currFeatureXpathSelected.Intersect(dn.InitialNodeSet));
                        HashSet<HtmlNode> xpathCurrSelected = new HashSet<HtmlNode>(dn.InitialNodeSet.Intersect(xpathSelected));
                        HashSet<HtmlNode> xpathCurrNotSelected = new HashSet<HtmlNode>(dn.InitialNodeSet.Except(xpathCurrSelected));
                       
                        //calculate information gain
                        HashSet<HtmlNode> currSelectedPositive = new HashSet<HtmlNode>(xpathCurrSelected.Intersect(dn.SelectedPositive));
                        HashSet<HtmlNode> currSelectedNegative = new HashSet<HtmlNode>(xpathCurrSelected.Intersect(dn.SelectedNegative));
                        HashSet<HtmlNode> currNotSelectedPositive = new HashSet<HtmlNode>(xpathCurrNotSelected.Intersect(dn.SelectedPositive));
                        HashSet<HtmlNode> currNotSelectedNegative = new HashSet<HtmlNode>(xpathCurrNotSelected.Intersect(dn.SelectedNegative));
                     
                        double sp = ((double)currSelectedPositive.Count()) / xpathCurrSelected.Count();
                        double sn = ((double)currSelectedNegative.Count()) / xpathCurrSelected.Count();

                        double relativeRecall = ((double)currSelectedPositive.Count()) / ((double)dn.SelectedPositive.Count());
                        //FIX:
                        sn = sn / (1 + Math.Pow(0, level + 1));
                        sn = sn / balanceFix;
                        double selectedProbability = ((double)xpathCurrSelected.Count()) / dn.InitialNodeSet.Count();
                  
                        double selectedEntropy = Statistics.CalculateEntropy(sp, sn);


                        double nsp = ((double)currNotSelectedPositive.Count()) / xpathCurrNotSelected.Count();
                        double nsn = 1 - nsp;
                        // Apply Fix
                        nsn = nsn / balanceFix;

                        double notselectedProbability = 1 - selectedProbability;
                        double notSelectedEntropy = Statistics.CalculateEntropy(nsp, nsn);

                    double balanceFixProb = balanceFix; 
                    double sumTemp = (selectedProbability * sn + selectedProbability * sp * balanceFixProb + notselectedProbability * nsn + notselectedProbability * nsp * balanceFixProb);
                    selectedProbability = (selectedProbability * sn + selectedProbability * sp * balanceFixProb) / sumTemp;
                    notselectedProbability = (notselectedProbability * nsn + notselectedProbability * nsp * balanceFixProb) / sumTemp;
                    double gain = dnEntropy - ((selectedProbability * selectedEntropy) + (notselectedProbability* notSelectedEntropy));


                    double phaseOfDecrease = 1;
                    if (DomPool.trainingDocsNames.Count() > 3)
                    {
                        phaseOfDecrease =  3/ DomPool.trainingDocsNames.Count();
                    }

                    //Choose the most cost effective feature 
                    gain = gain / (currCandidate.cost+(((1-relativeRecall)+(1-((double)DomPool.FeatureFrequencey[currCandidate.feature.First().ToLower()])/DomPool.trainingDocsNames.Count))) * Math.Pow(0.3,level));



                        lock (lockObj)
                        {
                            if (gain > maxScore && sp > nsp)
                            {
                                maxScore = gain;
                                maxGainFeature = currCandidate;
                                newFeatureSelected = xpathCurrSelected;
                            }
                        }
                       
            });

            if (maxGainFeature == null)
            {
                return;
            }

           
            dn.SetSelected = new DecisionNode();
            dn.SetSelected.InitialNodeSet = newFeatureSelected;
            dn.SetSelected.FeatureSet = new HashSet<Feature>(dn.FeatureSet);
            dn.SetSelected.FeatureSet.Add(maxGainFeature);
            dn.SetSelected.SelectedNegative = new HashSet<HtmlNode>(dn.SetSelected.InitialNodeSet.Intersect(dn.SelectedNegative));
            dn.SetSelected.SelectedPositive = new HashSet<HtmlNode>(dn.SetSelected.InitialNodeSet.Intersect(dn.SelectedPositive));
            dn.SetSelected.CalculateEntropy();

            dn.SetNotSelected = new DecisionNode();
            dn.SetNotSelected.InitialNodeSet = new HashSet<HtmlNode>(dn.InitialNodeSet.Except(newFeatureSelected));

            //FIX FOR NOT BRANCH, INSTEAD OF HAVING THE NOT.
            if (FixEnabledForNotBranch)
            {
                dn.SetNotSelected.InitialNodeSet.UnionWith(dn.SetSelected.SelectedNegative);
            }

            dn.SetNotSelected.FeatureSet = new HashSet<Feature>(dn.FeatureSet);
            dn.SetNotSelected.SelectedNegative = new HashSet<HtmlNode>(dn.SetNotSelected.InitialNodeSet.Intersect(dn.SelectedNegative));
            dn.SetNotSelected.SelectedPositive = new HashSet<HtmlNode>(dn.SetNotSelected.InitialNodeSet.Intersect(dn.SelectedPositive));
            dn.SetNotSelected.CalculateEntropy();
            dn.FeatureSet.Add(maxGainFeature);
        }

        public static void RecursiveTreeImprovement(DecisionNode treeRoot,int level=0)
        {
            if (treeRoot==null || treeRoot.entropy == 0) { return; }
            ImproveTree(treeRoot,level);



            RecursiveTreeImprovement(treeRoot.SetNotSelected, level);
            RecursiveTreeImprovement(treeRoot.SetSelected, ++level);
        }


    }
}
