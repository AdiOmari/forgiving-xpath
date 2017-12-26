using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobustXpathSynthesis.src.ds;
using HtmlAgilityPack;
using RobustXpathSynthesis.src.config;

namespace RobustXpathSynthesis.src.phases
{

    
    class FeatureExtraction
    {

        public static HashSet<String> AttributesToIgnore = new HashSet<string>();//new String[] { /*"onclick", "style",*/"id" });

        public static int LEVEL_FEATURE_LIMIT = 50;

        public static HashSet<Feature> Extract(HtmlNode item) {
            HashSet<Feature> selectorFeatures = new HashSet<Feature>();
            HashSet<Feature> orderFeatures = new HashSet<Feature>();

            HtmlNode currLevel = item;
            while (currLevel != null) {
                if (currLevel.Name.Contains("#")) {break;}

                HashSet<Feature> currLevelFeatures = new HashSet<Feature>();
                //Here I should filter the features and keep the K (LEVEL_FEATURE_LIMIT) significant ones only.
                currLevelFeatures.UnionWith(FeatureFiltering.KeepTopK(ExtractNodeFeatures(currLevel), LEVEL_FEATURE_LIMIT));

                foreach(Feature f_curr in currLevelFeatures) {
                    foreach (Feature f_before in selectorFeatures) {
                        Feature orderFeature = new Feature();
                        orderFeature.type = Feature.FeatureType.Order;
                        orderFeature.feature = new List<string>(f_curr.feature);
                        orderFeature.feature.Add(f_before.feature.First());
                        orderFeature.cost = FeatureCosts.ORDER_COST;
                        orderFeatures.Add(orderFeature);
                    }
                }
                

                if (currLevel.Equals(item))
                {
                    HashSet<Feature> nodeSelfFeatures = new HashSet<Feature>();
                    foreach(Feature fcurr in currLevelFeatures)
                    {
                        Feature selfFeature = new Feature(fcurr);
                        selfFeature.Axe = Feature.Axes.Self;
                        selfFeature.cost = selfFeature.cost+0.2 ;//self is more specific than ancestor-or-self, therefore "costs" more.
                        nodeSelfFeatures.Add(selfFeature);
                    }

                    currLevelFeatures.UnionWith(nodeSelfFeatures);
                }

                selectorFeatures.UnionWith(currLevelFeatures);

                currLevel = currLevel.ParentNode;
            }

            HashSet<Feature> allFeatures = new HashSet<Feature>();
            allFeatures.UnionWith(selectorFeatures);
            allFeatures.UnionWith(orderFeatures);

            return allFeatures;
        }

        public static HashSet<Feature> ExtractNodeFeatures(HtmlNode curr) {

            HashSet<Feature> allFeatures = new HashSet<Feature>();

            HashSet<Feature> attributeFeatures = new HashSet<Feature>();
            //attribute features
            foreach (HtmlAttribute att in curr.Attributes) {
                if (AttributesToIgnore.Contains(att.Name.ToLower().Trim())|| att.Name.ToLower().Contains(":")) { continue; }
                attributeFeatures.UnionWith(FeatureGeneralization.GeneralizeAttributeFeatures(att));
            }

            //node name 
            Feature nn = new Feature();
            nn.cost = FeatureCosts.NODE_NAME;
            nn.feature = new List<string>(new string[] { curr.Name });
            nn.nodeNameFeature = true;
            nn.type = Feature.FeatureType.Selector;
            allFeatures.Add(nn);

            //text features
            string text = curr.InnerText.Trim();
            if (text.Length > 3 && text.Length < 10){
                attributeFeatures.UnionWith(FeatureGeneralization.GeneralizeTextFeatures(text));
            }

            HashSet<Feature> childFeatures = new HashSet<Feature>();
            //children features
            foreach (HtmlNode chld in curr.ChildNodes)
            {
                if (chld.InnerText.Length >= 10 || chld.InnerText.Length <= 3) { continue; }

                Feature f = new Feature();
                if (!chld.Name.Contains("#")) { 
                    f.feature = new List<string> { "./" + chld.Name + "" };
                    f.cost = FeatureCosts.NODE_NAME + FeatureCosts.CHILD_COST;
                    f.type = Feature.FeatureType.Selector;
                    childFeatures.Add(f);
                }
                HashSet<Feature> chldFeatures =  FeatureGeneralization.GeneralizeTextFeatures(chld.InnerText);
                foreach (Feature cf in chldFeatures) {
                    cf.feature = new List<string> { "./*[" + cf.feature.First() + "]" };
                    cf.cost = cf.cost + FeatureCosts.CHILD_COST;
                    childFeatures.Add(cf);
                }
            }

            allFeatures.UnionWith(attributeFeatures);
            allFeatures.UnionWith(childFeatures);
            //
            return allFeatures;
        }
    }
}
