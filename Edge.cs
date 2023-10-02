using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ADA_Assignment
{
    class Edge
    {
        public Node To { get; set; }
        public Node From { get; set; }
        public readonly double Weight;

        public Edge(Node n1, Node n2, double conversionRate)
        {
            From = n1;
            To = n2;
            Weight = RateToWeight(conversionRate);
        }

        public Edge(Node n1, Node n2, int weight, object flag)
        {
            From = n1;
            To = n2;
            Weight = weight;
        }

        private static double RateToWeight(double d)
        {
            return -Math.Log10(d);
        }

        public static bool operator ==(Edge e1, Edge e2) => e1.To == e2.To && e1.From == e2.From;
        public static bool operator !=(Edge e1, Edge e2) => !(e1 == e2);
    }
}
