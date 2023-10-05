namespace ADA_Assignment
{
    class Edge
    {
        public Node To { get; set; }
        public Node From { get; set; }
        public decimal Weight { get; private set; }

        /// <summary>
        /// Use this constructor to pass in a conversion rate which needs to be converted into an edge weight
        /// </summary>
        /// <param name="from">The starting node</param>
        /// <param name="to">The finishing node</param>
        /// <param name="conversionRate">The rate of conversion</param>
        public Edge(Node from, Node to, double conversionRate)
        {
            From = from;
            To = to;
            Weight = RateToWeight(conversionRate);
        }

        /// <summary>
        /// Use this constructor to pass in an edge weight (do not convert)
        /// </summary>
        /// <param name="from">The starting node</param>
        /// <param name="to">The finishing node</param>
        /// <param name="weight">The edge weight</param>
        /// <param name="flag">Pass a random object as a flag to activate this constructor. It will not participate in the construction of the Edge object.</param>
        public Edge(Node from, Node to, decimal weight, object flag)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        private static decimal RateToWeight(double d) => (decimal)-Math.Log10(d);

        public static bool operator ==(Edge e1, Edge e2) => e1.To == e2.To && e1.From == e2.From;
        public static bool operator !=(Edge e1, Edge e2) => !(e1 == e2);
    }
}
