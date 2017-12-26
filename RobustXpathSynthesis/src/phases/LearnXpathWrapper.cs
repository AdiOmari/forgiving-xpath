using HtmlAgilityPack;
using RobustXpathSynthesis.src.decisiontree;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.phases
{
    class LearnXpathWrapper
    {
        public static string LearnXpathFromTrainingFiles(string filesLocation)
        {
            DomPool.LoadDocuments(filesLocation);
            DomPool.Initiate();
            DomPool.ExtractAllFeatures();

            DecisionNode dn = new DecisionNode();
            dn.InitialNodeSet = new HashSet<HtmlNode>(DomPool.TargetNodes.Union(DomPool.NonTargetNodes));
            dn.SelectedNegative = new HashSet<HtmlNode>(DomPool.NonTargetNodes.Except(DomPool.TargetNodesPrecision));
            dn.SelectedPositive = new HashSet<HtmlNode>(DomPool.TargetNodes);
            dn.FeatureSet = new HashSet<Feature>();
            dn.CalculateEntropy();

            DecisionTreeLearning.RecursiveTreeImprovement(dn);

            return XpathTools.GenerateAForgivingXpath(dn);
                //"//*["+XpathTools.DecisionTreeToXpath(dn,new HashSet<Feature>())+"]";
        }
    }
}
