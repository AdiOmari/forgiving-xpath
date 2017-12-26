using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobustXpathSynthesis.src.utilities
{
    class Statistics
    {
        public static double CalculateEntropy(double a, double b)
        {
            if(a==0 || b == 0){return 0;}

            return (-1) * ((a / (a + b)) * Math.Log(a / (a + b)) + (b / (a + b)) * Math.Log(b / (a + b)));
        }
    }
}
