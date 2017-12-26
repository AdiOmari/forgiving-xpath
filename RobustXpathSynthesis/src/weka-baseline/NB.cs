using HtmlAgilityPack;
using java.io;
using RobustXpathSynthesis.src.ds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weka.classifiers.bayes;
using weka.classifiers.functions;
using weka.classifiers.trees;
using weka.core;
using weka.gui.treevisualizer;

namespace RobustXpathSynthesis.src.weka_baseline
{
    class NB
    {
        public  Dictionary<HtmlNode, HashSet<String>> nodeFeatures = new Dictionary<HtmlNode, HashSet<string>>();
        public  HashSet<HtmlNode> allNodes = null;
        //test Set
        public Dictionary<HtmlNode, HashSet<String>> testNodeFeatures = new Dictionary<HtmlNode, HashSet<string>>();
        public HashSet<HtmlNode> testAllNodes = null;
        //test Seen Set
        public Dictionary<HtmlNode, HashSet<String>> testSeenNodeFeatures = new Dictionary<HtmlNode, HashSet<string>>();
        public HashSet<HtmlNode> testSeenAllNodes = null;


        public NaiveBayes classifier = null;
        public HashSet<String> FeaturesUsed = null;

        //cache
        FastVector _fvWekaAttributes = null;


        private  void Init()
        {
          allNodes =  new HashSet<HtmlNode>((DomPool.TargetNodes).Union(DomPool.NonTargetNodes.Except(DomPool.TargetNodesPrecision)));
            foreach(HtmlNode currNode in allNodes)
            {
                nodeFeatures[currNode] = new HashSet<string>();
            }
        }

        private void InitTest()
        {
            testAllNodes = new HashSet<HtmlNode>((DomPool.TESTTargetNodes).Union(DomPool.TESTNonTargetNodes.Except(DomPool.TESTTargetNodesPrecision)));
            foreach (HtmlNode currNode in testAllNodes)
            {
                testNodeFeatures[currNode] = new HashSet<string>();
            }
        }

        private void InitTestSeen()
        {
            testSeenAllNodes = new HashSet<HtmlNode>((DomPool.TESTSeenTargetNodes).Union(DomPool.TESTSeenNonTargetNodes.Except(DomPool.TESTSeenTargetNodesPrecision)));
            foreach (HtmlNode currNode in testSeenAllNodes)
            {
                testSeenNodeFeatures[currNode] = new HashSet<string>();
            }
        }

        public void LearnModel()
        {
            Init();
            foreach(Feature currFeature in DomPool.SelectorFeatures)
            {
                String featureString = currFeature.ToString();
                HashSet<HtmlNode> resNodes = DomPool.RunXpathQuery(featureString);
                foreach(HtmlNode nd in resNodes)
                {
                    if (!allNodes.Contains(nd)) { continue; }
                    nodeFeatures[nd].Add(featureString);
                }
            }
            FastVector fvWekaAttributes = GetDataSetAtts();
            Instances trainingSet = new Instances("TS", fvWekaAttributes, 10);
            trainingSet.setClassIndex(fvWekaAttributes.size() - 1);

            foreach(HtmlNode currNode in allNodes)
            {
                Instance item = new SparseInstance(fvWekaAttributes.size());

                for(int i=0;i< fvWekaAttributes.size()-1;i++)
                {
                    weka.core.Attribute currFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(i);
                    if (nodeFeatures[currNode].Contains(currFeature.name()))
                    {
                        item.setValue(currFeature, 1);
                    }
                    else
                    {
                        item.setValue(currFeature, 0);
                    }
                }

                //set the class
                weka.core.Attribute classFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(fvWekaAttributes.size()-1);
                item.setValue(classFeature, (DomPool.TargetNodes.Contains(currNode)?"yes":"no"));
                item.setDataset(trainingSet);
                if (DomPool.TargetNodes.Contains(currNode))
                {
                    for (int t = 0; t < (DomPool.NonTargetNodes.Count() / DomPool.TargetNodes.Count()); t++)
                    {
                        trainingSet.add(new SparseInstance(item));
                    }
                }
                else {
                    trainingSet.add(item);
                }
            }

             //String[] options = new String[2];
            //options = new string[] { "-C", "0.05" };            // unpruned tree
            NaiveBayes cls = new NaiveBayes();         // new instance of tree
            //cls.setOptions(weka.core.Utils.splitOptions("-C 1.0 -L 0.0010 -P 1.0E-12 -N 0 -V -1 -W 1 -K \"weka.classifiers.functions.supportVector.PolyKernel -C 250007 -E 1.0\""));
            //cls.setOptions(options);     // set the options
           cls.buildClassifier(trainingSet);   // build classifier
            //save the resulting classifier
            classifier = cls;

            //  Reader treeDot = new StringReader(tree.graph());
            //  TreeBuild treeBuild = new TreeBuild();
            //  Node treeRoot = treeBuild.create(treeDot);
            FeaturesUsed = new HashSet<string>();

            foreach (Feature f in DomPool.SelectorFeatures)
            {
                FeaturesUsed.Add(f.ToString());
            }
        }


        public HashSet<HtmlNode> RunOnTestSet()
        {
            HashSet<HtmlNode> classifierSelectedNodes = new HashSet<HtmlNode>();
            InitTest();
            foreach (string featureString in FeaturesUsed)
            {
                HashSet<HtmlNode> resNodes = DomPool.TESTRunXpathQuery(useNormalPerformanceQUERY(featureString));
                foreach (HtmlNode nd in resNodes)
                {
                    if (!testAllNodes.Contains(nd)) { continue; }
                    testNodeFeatures[nd].Add(featureString);
                }
            }

            FastVector fvWekaAttributes = GetDataSetAtts();
            Instances testSet = new Instances("TestSet", fvWekaAttributes, 10);
            testSet.setClassIndex(fvWekaAttributes.size() - 1);

            foreach (HtmlNode currNode in testAllNodes)
            {
                Instance item = new SparseInstance(fvWekaAttributes.size());

                for (int i = 0; i < fvWekaAttributes.size() - 1; i++)
                {
                    weka.core.Attribute currFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(i);
                    if (testNodeFeatures[currNode].Contains(currFeature.name()))
                    {
                        item.setValue(currFeature, 1);
                    }
                    else
                    {
                        item.setValue(currFeature, 0);
                    }
                }

                //set the class
                weka.core.Attribute classFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(fvWekaAttributes.size() - 1);
                //string rightVal = DomPool.TargetNodes.Contains(currNode) ? "yes" : "no";
                item.setDataset(testSet);



                double classifierdv = classifier.classifyInstance(item);
                string classifierVal = classFeature.value((int)classifierdv);

                if (classifierVal.Equals("yes"))
                {
                    classifierSelectedNodes.Add(currNode);
                }

                testSet.add(item);
            }

            return classifierSelectedNodes;
        }

        public HashSet<HtmlNode> RunOnTestSeenSet()
        {
            HashSet<HtmlNode> classifierSelectedNodes = new HashSet<HtmlNode>();
            InitTestSeen();
            foreach (string featureString in FeaturesUsed)
            {
                HashSet<HtmlNode> resNodes = DomPool.TESTSeenRunXpathQuery(useNormalPerformanceQUERY(featureString));
                foreach (HtmlNode nd in resNodes)
                {
                    if (!testSeenAllNodes.Contains(nd)) { continue; }
                    testSeenNodeFeatures[nd].Add(featureString);
                }
            }

            FastVector fvWekaAttributes = GetDataSetAtts();
            Instances testSet = new Instances("TestSeenSet", fvWekaAttributes, 10);
            testSet.setClassIndex(fvWekaAttributes.size() - 1);

            foreach (HtmlNode currNode in testSeenAllNodes)
            {
                Instance item = new SparseInstance(fvWekaAttributes.size());

                for (int i = 0; i < fvWekaAttributes.size() - 1; i++)
                {
                    weka.core.Attribute currFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(i);
                    if (testSeenNodeFeatures[currNode].Contains(currFeature.name()))
                    {
                        item.setValue(currFeature, 1);
                    }
                    else
                    {
                        item.setValue(currFeature, 0);
                    }
                }

                //set the class
                weka.core.Attribute classFeature = (weka.core.Attribute)fvWekaAttributes.elementAt(fvWekaAttributes.size() - 1);
                //string rightVal = DomPool.TargetNodes.Contains(currNode) ? "yes" : "no";
                item.setDataset(testSet);



                double classifierdv = classifier.classifyInstance(item);
                string classifierVal = classFeature.value((int)classifierdv);

                if (classifierVal.Equals("yes"))
                {
                    classifierSelectedNodes.Add(currNode);
                }

                testSet.add(item);
            }

            return classifierSelectedNodes;
        }

        public static HashSet<String> getTreeFeatures(Node treeRoot)
        {
            if (treeRoot.getChild(0) == null)
            {
                return new HashSet<string>();
            }

            HashSet<string> res = new HashSet<string>();
            String currStr = treeRoot.getLabel();

            if(currStr.StartsWith("'")){
                currStr = currStr.Substring(1);
            }

            if (currStr.EndsWith("'"))
            {
                currStr = currStr.Substring(0, currStr.Length - 1);
            }

            res.Add(currStr);
            //we have only two values possible for a feature 1, and 0;
            for(int i = 0; i < 2; i++)
            {
                if (treeRoot.getChild(i) == null || treeRoot.getChild(i).getTarget() ==null) { break; }
                res.UnionWith(getTreeFeatures(treeRoot.getChild(i).getTarget()));
            }
            return res;

        }

        public FastVector GetDataSetAtts()
        {
            if (_fvWekaAttributes != null)
            {
                return _fvWekaAttributes;
            }

            // Declare features
            FastVector fvWekaAttributes = new FastVector(DomPool.SelectorFeatures.Count() + 1);

            foreach (Feature currFeature in DomPool.SelectorFeatures)
            {
                weka.core.Attribute feature = new weka.core.Attribute(currFeature.ToString());
                fvWekaAttributes.addElement(feature);
            }

            // Declare the class attribute along with its values
            FastVector fvClassVal = new FastVector(2);
            fvClassVal.addElement("yes");
            fvClassVal.addElement("no");
            weka.core.Attribute ClassAttribute = new weka.core.Attribute("theClass", fvClassVal);

            // Declare the feature vector
            fvWekaAttributes.addElement(ClassAttribute);
            _fvWekaAttributes = fvWekaAttributes;

            return _fvWekaAttributes;
        }

        public static string useNormalPerformanceQUERY(string q)
        {
            return q;/*
            if (q.Contains("|"))
            {
                string[] qSplit = q.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                q = qSplit[0];
            }
            else
            {
                return q;
            }

            string command = q.Replace("//", "");

            return "//*[ancestor-or-self::" + command + "]";
            */

        }

        

    }
}
