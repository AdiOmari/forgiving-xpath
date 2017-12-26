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
    class DecisionNode
    {
        public HashSet<HtmlNode> InitialNodeSet;
        public HashSet<HtmlNode> SelectedNegative;
        public HashSet<HtmlNode> SelectedPositive;

        public HashSet<Feature> FeatureSet=null;
        public DecisionNode SetSelected=null;
        public DecisionNode SetNotSelected=null;

        private double? _precision = null;
        private double? _recall = null;

        public HashSet<HtmlNode> selectTrue(HashSet<HtmlNode> nodes, HashSet<Feature> prevFeatures, Boolean right, double threshold=1)
        {
            if (this.precision >= threshold)
            {
                return nodes;
            }

            HashSet<Feature> currFeature = new HashSet<Feature>(this.FeatureSet.Except(prevFeatures));
            if (currFeature.Count() == 0)
            {
                if (right) { return nodes; }
                else
                {
                    return new HashSet<HtmlNode>();
                }
            }

            Feature cf = currFeature.First();
            HashSet<HtmlNode> featureRes= DomPool.RunXpathQuery(XpathTools.FeatureSetToXpath(new HashSet<Feature>() { cf }));
            featureRes.IntersectWith(nodes);
            HashSet<HtmlNode> rightRes = this.SetSelected.selectTrue(featureRes,this.FeatureSet,true, threshold);
            HashSet<HtmlNode> leftRes = this.SetNotSelected.selectTrue(nodes, prevFeatures, false, threshold);

            return new HashSet<HtmlNode>(rightRes.Union(leftRes));
        }


        public double precision
        {
            get {
                if (SetSelected == null){ return 0; }
                if (_precision == null)
                {
                    _precision = ((double)SetSelected.InitialNodeSet.Intersect(DomPool.TargetNodesPrecision).Count()) / SetSelected.InitialNodeSet.Count();
                }
                return _precision.Value;
            }
        }

        public double recall
        {
            get
            {
                if (SetSelected == null) { return 0; }
                if (_recall == null)
                {
                    _recall= ((double)SetSelected.InitialNodeSet.Intersect(DomPool.TargetNodes).Count()) / DomPool.TargetNodes.Count();
                }
                return _recall.Value;
            }
        }

        public double entropy = 0;

        public void CalculateEntropy() {
            double nPortion = ((double)SelectedNegative.Count()) / InitialNodeSet.Count();
            double pPortion = ((double)SelectedPositive.Count()) / InitialNodeSet.Count();
            entropy = Statistics.CalculateEntropy(pPortion, nPortion);
        }

        public double CalculateEntropy(double NegativeWeight,double balanceFix)
        {
            double nPortion = ((double)SelectedNegative.Count()) / InitialNodeSet.Count() / NegativeWeight;
            nPortion = nPortion / balanceFix;
            double pPortion = (((double)SelectedPositive.Count()) / InitialNodeSet.Count());
            return Statistics.CalculateEntropy(pPortion, nPortion);
        }

    }
}
