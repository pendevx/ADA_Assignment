using System;
using System.Text;

namespace ADA_Assignment
{
    class Graph
    {
        //public HashSet<Node> Nodes { get; private set; }
        //public HashSet<Edge> Edges { get; private set; }
        public decimal[][] Matrix { get; private set; }
        public Node[] Nodes { get; private set; }
        private int _i = 0; 
        const int infinity = 2147000000;

        public Graph(int size)
        {
            Matrix = new decimal[size][];
            Nodes = new Node[size];
            for (int i = 0; i < size; i++) Matrix[i] = new decimal[size];
        }

        public void AddNode(Node n)
        {
            Nodes[_i] = n;
            n.ID = _i++;
        }

        public void AddEdge(Edge e)
        {
            Matrix[e.From.ID][e.To.ID] = e.Weight;
        }

        private List<Edge> GetIncomingEdges(Node n)
        {
            var res = new List<Edge>();
            for (int i = 0; i < Nodes.Length; i++)
            {
                //res[i] = new Edge(Nodes[i], n, (double)Matrix[i][n.ID]);
                //var weight = (int)Matrix[i][n.ID];
                var weight = Matrix[i][n.ID];
                if (weight == 0) continue;

                //var e = new Edge(Nodes[i], n, weight, null);
                var e = new Edge(Nodes[i], n, weight, null);
                res.Add(e);
            }
            return res;
        }

        public Dictionary<Node, decimal> BellmanFord(Node source)
        {
            if (source == null) throw new ArgumentNullException("source cannot be null");

            var n = Nodes.Length;
            var distances = new Dictionary<Node, decimal>();

            foreach (var node in Nodes) distances[node] = infinity;
            distances[source] = 0;

            void PerformCycle()
            {
                var newDistances = new Dictionary<Node, decimal>(distances);
                foreach (var node in Nodes)
                {
                    foreach (var edge in GetIncomingEdges(node))
                    {
                        if (distances[edge.From] == infinity)
                            continue;
                        else if (edge.From == source)
                            newDistances[node] = Math.Min(newDistances[node], edge.Weight);
                        else
                            newDistances[node] = Math.Min(newDistances[node], distances[edge.From] + edge.Weight);
                    }
                }
                distances = newDistances;
            }

            for (int i = 0; i < n - 1; i++)
                PerformCycle();

            foreach (var d in distances)
                Console.WriteLine($"{(d.Value < 0 ? "No arbitrage" : "Arbitrage")} found in {d.Key.Name}");

            return distances;
        }

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();

        //    sb.AppendLine("FROM->TO    EDGE WEIGHT");

        //    foreach (var e in Edges)
        //    {
        //        double val = Math.Round((double)e.Weight, 4);
        //        if (val == -0) val = 0;
        //        sb.AppendLine($"{$"{e.From.Name}->{e.To.Name}".PadLeft(6)} {(val < 0 ? "" : " ")}      {val.ToString("0.0000")}");
        //    }

        //    return sb.ToString();
        //}

        public Node GetNode(string name)
        {
            var res = Nodes.Where(x => x.Name == name).FirstOrDefault();
            return res;
        }
    }
}
