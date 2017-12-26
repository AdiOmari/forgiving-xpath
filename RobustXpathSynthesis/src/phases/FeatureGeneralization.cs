using HtmlAgilityPack;
using RobustXpathSynthesis.src.ds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RobustXpathSynthesis.src.config;
using RobustXpathSynthesis.src.utilities;

namespace RobustXpathSynthesis.src.phases
{
    class FeatureGeneralization
    {

        public static HashSet<Feature> GeneralizeAttributeFeatures(HtmlAttribute att) {
            if (att.Name.Equals(DomPool.selectionAttribute)|| att.Name.Equals(DomPool.optionalSelectionAttribute)) { return new HashSet<Feature>(); }
            HashSet<Feature> res = new HashSet<Feature>();
            //BASE att name existance condition
            Feature f = new Feature();
            f.type = Feature.FeatureType.Selector;
            f.feature = new List<string>() { "@" + att.Name };
            f.cost = FeatureCosts.ATT_BASE;
            res.Add(f);

            if (att.Value.Trim().Length >= 1)
            {

                f = new Feature();
                f.type = Feature.FeatureType.Selector;
                f.feature = new List<string>() { "@" + att.Name + "='" + XpathTools.EscapeString(att.Value.Trim()) + "'" };
                f.cost = FeatureCosts.ATT_EQUAL;
                res.Add(f);

                f = new Feature();
                f.type = Feature.FeatureType.Selector;
                f.feature = new List<string>() { "@*='" + XpathTools.EscapeString(att.Value.Trim()) + "'" };
                f.cost = FeatureCosts.ATT_ANY_EQUAL;
                res.Add(f);
            }

            Regex reg = new Regex("(?=([A-Z\\-\\s/\\?\\=_]))");
            string[] splitted = reg.Split(XpathTools.EscapeString(att.Value));
            //fix, it must keep the letters and remove the symboles.
            foreach(var split in splitted) {
                string curr = split.Replace("-", "").Trim();
                if (curr.Length < 2) { continue; }
                Feature f_contains = new Feature();
                f_contains.type = Feature.FeatureType.Selector;
                f_contains.feature = new List<string>() { "@" + att.Name + "[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'" + curr + "')]" };
                f_contains.cost = FeatureCosts.ATT_CONTAINS;
                res.Add(f_contains);

                Feature f_any_contains = new Feature();
                f_any_contains.type = Feature.FeatureType.Selector;
                f_any_contains.feature = new List<string>() { "@*[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'" + curr + "')]" };
                f_any_contains.cost = FeatureCosts.ATT_ANY_CONTAINS;
                res.Add(f_any_contains);
            }

            for (int i=0;i<splitted.Length-1;i++)
            {
                var split = splitted[i] + splitted[i + 1];
                string curr = split.Replace("-", "").Trim();
                if (curr.Length < 2) { continue; }
                Feature f_contains = new Feature();
                f_contains.type = Feature.FeatureType.Selector;
                f_contains.feature = new List<string>() { "@" + att.Name + "[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'" + curr + "')]" };
                f_contains.cost = FeatureCosts.ATT_CONTAINS+0.05;
                res.Add(f_contains);

                Feature f_any_contains = new Feature();
                f_any_contains.type = Feature.FeatureType.Selector;
                f_any_contains.feature = new List<string>() { "@*[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'" + curr + "')]" };
                f_any_contains.cost = FeatureCosts.ATT_ANY_CONTAINS + 0.05;
                res.Add(f_any_contains);
            }

            return res;
        }

        public static HashSet<Feature> GeneralizeTextFeatures(String text) {
            HashSet<Feature> res = new HashSet<Feature>();
            Feature f = new Feature();
            f.type = Feature.FeatureType.Selector;
            f.feature = new List<string>() { "text()='" + XpathTools.EscapeString(text.Trim()) + "'" };
            f.cost = FeatureCosts.TEXT_EQUAL;
            res.Add(f);
            Regex reg = new Regex("(?=([A-Z\\-\\s]))");
            string[] splitted = reg.Split(XpathTools.EscapeString(text));
            foreach (var split in splitted) {
                var curr = split.Replace("-", "").Trim();
                if (curr.Length < 2) { continue; }
                Feature f_contains = new Feature();
                f_contains.type = Feature.FeatureType.Selector;
                f_contains.feature = new List<string>() { "contains(text(),'" + curr + "')" };
                f_contains.cost = FeatureCosts.TEXT_CONTAINS;
                res.Add(f_contains);
            }
            return res;
        }

    }
}
