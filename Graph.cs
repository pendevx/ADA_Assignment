using System;
using System.Text;

namespace ADA_Assignment
{
    class Graph
    {
        public decimal[][] Matrix { get; }
        public Node[] Nodes { get; }
        int _i = 0;
        public const int infinity = 2147000000;

        public Graph(int size)
        {
            Matrix = new decimal[size][];
            Nodes = new Node[size];
            Matrix = Matrix.Select(x => new decimal[size]).ToArray();

            for (int i =0;i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Matrix[i][j] = i == j ? 0 : infinity;
                }
            }
        }

        /// <summary>
        /// Add a node to the graph
        /// </summary>
        /// <param name="n">The node to add</param>
        public void AddNode(Node n)
        {
            Nodes[_i] = n;
            n.ID = _i++;
        }

        /// <summary>
        /// Add an edge to the graph
        /// </summary>
        /// <param name="e">The edge to add</param>
        public void AddEdge(Edge e)
        {
            Matrix[e.From.ID][e.To.ID] = e.Weight;
        }

        /// <summary>
        /// Get all the incoming edges into a node
        /// Incoming edges are those which have a FROM as a non-null Node and have a TO matching the parameter <paramref name="n"/>
        /// </summary>
        /// <param name="n">The node the get incoming edges for</param>
        /// <returns>The edges which enter the node</returns>
        private List<Edge> GetIncomingEdges(Node n)
        {
            var res = new List<Edge>();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var weight = Matrix[i][n.ID];
                if (weight == 0) continue;

                var e = new Edge(Nodes[i], n, weight, null);
                res.Add(e);
            }
            return res;
        }

        /// <summary>
        /// Finds the best conversion rate between any two pairs of nodes
        /// </summary>
        /// <returns>The best conversion rate from any node to the rest</returns>
        public decimal[][] FindBestConversionRate() // Too generalized use case, needs to specialize for assignment
        {
            var shortestDistances = Matrix.Clone() as decimal[][];

            for (int k = 0; k < Nodes.Length; k++)
            {
                for (int j = 0; j < Nodes.Length; j++)
                {
                    for (int i = 0; i < Nodes.Length; i++)
                    {
                        shortestDistances[i][j] = Math.Min(Matrix[i][j], Matrix[i][k] + Matrix[k][j]);
                    }
                }
            }

            return shortestDistances;
        }

        /// <summary>
        /// Perform Bellman Ford algorithm on the graph
        /// </summary>
        /// <param name="source">The source node</param>
        /// <returns>Distances from the source node to all other nodes</returns>
        /// <exception cref="ArgumentNullException">The source node is null</exception>
        public Dictionary<Node, decimal> FindArbitrageOpportunities(Node source)
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

            //foreach (var d in distances)
            //    Console.WriteLine($"{(d.Value < 0 ? "No arbitrage" : "Arbitrage")} found in {d.Key.Name}");

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

        /// <summary>
        /// Gets a node
        /// </summary>
        /// <param name="name">The name of the node to retrieve</param>
        /// <returns>The node with a name matching <paramref name="name"/></returns>
        public Node GetNode(string name)
        {
            var res = Nodes.Where(x => x.Name == name).FirstOrDefault();
            return res;
        }
    }
}
