using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.ds
{
    class FeatureSet
    {
        public HashSet<Feature> SelectorFeatures;
        public Dictionary<string, string> OrderBeforeFeatures;
        public Dictionary<string, string> OrderSameIndexFeatures;
    }
}
