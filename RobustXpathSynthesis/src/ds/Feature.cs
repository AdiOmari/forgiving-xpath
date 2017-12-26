using RobustXpathSynthesis.src.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.ds
{
    class Feature
    {
        public enum FeatureType { Order, Selector };
        public enum Axes { Self, SelfOrPred };
        public FeatureType type;
        public List<string> feature;
        public bool sameIndex; // relevant only to order type
        public bool nodeNameFeature = false;
        public double cost;
        public Axes Axe= Axes.SelfOrPred;
        public int? _hashCode = null;
        public string _stringRepresentation = null;

        public Feature() { }
        public Feature(Feature inFeature)
        {
            type = inFeature.type;
            feature = new List<string>(inFeature.feature);
            sameIndex = inFeature.sameIndex;
            nodeNameFeature = inFeature.nodeNameFeature;
            cost = inFeature.cost;
            Axe = inFeature.Axe;
        }

        public override string ToString()
        {
            if (_stringRepresentation == null)
            {
                if (feature.Count() == 1)
                {
                    _stringRepresentation = XpathTools.FeatureSetToXpath((new List<Feature>() { this }));
                }
                else
                {
                    _stringRepresentation = String.Join(">", feature);
                }
            }
            return _stringRepresentation;
        }

        public override bool Equals(System.Object obj)
        {
            if (!type.Equals(((Feature)obj).type) || !Axe.Equals(((Feature)obj).Axe))
            {
                return false;
            }
            return this.ToString().Equals(((Feature)obj).ToString());
        }

        public override int GetHashCode()
        {
            if (_hashCode == null) {
                _hashCode = (this.ToString()).GetHashCode();
            }
            return _hashCode.Value;
        }
 
    }
}
