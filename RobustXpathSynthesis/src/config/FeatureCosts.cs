using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.config
{
    class FeatureCosts
    {
        //ATT
        public static double ATT_BASE = 1.1;
        public static double ATT_EQUAL = ATT_BASE + 1;
        public static double ATT_ANY_EQUAL = ATT_BASE + 0.9;
        public static double ATT_CONTAINS = ATT_BASE + 0.4;
        public static double ATT_ANY_CONTAINS = ATT_BASE + 0.2;

        //NODE
        public static double NODE_NAME = 1;

        //TEXT
        public static double TEXT_EQUAL = 3.5;
        public static double TEXT_CONTAINS = 2.5;

        //AXIS
        public static double CHILD_COST = 1;

        //ORDER
        public static double ORDER_COST = 0.1;


    }
}
