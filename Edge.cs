namespace ADA_Assignment
{
    class Edge
    {
        public Node To { get; set; }
        public Node From { get; set; }
        public decimal Weight { get; private set; }

        public Edge(Node from, Node to, double conversionRate)
        {
            From = from;
            To = to;
            Weight = RateToWeight(conversionRate);
        }

        public Edge(Node n1, Node n2, int weight, object flag)
        {
            From = n1;
            To = n2;
            Weight = weight;
        }

        private static decimal RateToWeight(double d) => (decimal)-Math.Log10(d);

        public static bool operator ==(Edge e1, Edge e2) => e1.To == e2.To && e1.From == e2.From;
        public static bool operator !=(Edge e1, Edge e2) => !(e1 == e2);
    }
}
