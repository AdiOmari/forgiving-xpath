using HtmlAgilityPack;
using RobustXpathSynthesis.src.ds;
using RobustXpathSynthesis.src.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.phases
{
    class FeatureFiltering
    {

        public static HashSet<Feature> KeepTopK(HashSet<Feature> featureSet, int k) {
            //this line is here to disable the filtering, it costs in performance much more than it saves
            if (featureSet.Count() <= k*200) { return featureSet; }
            HashSet<Feature> finalRes = null;
            LinkedList<object[]> toSort = new LinkedList<object[]>();
            
            foreach(Feature f in featureSet)
            {
                HashSet<HtmlNode> res = DomPool.RunXpathQuery(XpathTools.FeatureSetToXpath(new HashSet<Feature>(new Feature[] { f })));
                HashSet<HtmlNode> selectedPos = new HashSet<HtmlNode>(res.Intersect(DomPool.TargetNodes));
                double entropy = Statistics.CalculateEntropy(((double)selectedPos.Count() / res.Count()), 1 - ((double)selectedPos.Count() / res.Count()));
                object[] toSortObj = new object[2];
                toSortObj[0] = f;
                toSortObj[1] = entropy;
                toSort.AddFirst(toSortObj);
            }

            var resTopK = toSort.OrderBy(x => ((double)(x[1]))).Select(x=>(Feature)(x[0])).Take(k);
            finalRes = new HashSet<Feature>(resTopK.ToList());
            return finalRes;
        }

    }
}
