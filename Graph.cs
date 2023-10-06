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
            if (_i == Nodes.Length) throw new ApplicationException("Cannot add more nodes than the originally specified matrix size.");
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

        ///// <summary>
        ///// Perform Floyd-Warshall algorithm on the graph
        ///// </summary>
        ///// <returns>The best conversion rate from any node to the rest</returns>
        //public decimal[][] FindBestConversionRate() // Too generalized use case, needs to specialize for assignment
        //{
        //    var shortestDistances = Matrix.Clone() as decimal[][];

        //    for (int k = 0; k < Nodes.Length; k++)
        //    {
        //        for (int j = 0; j < Nodes.Length; j++)
        //        {
        //            for (int i = 0; i < Nodes.Length; i++)
        //            {
        //                shortestDistances[i][j] = Math.Min(Matrix[i][j], Matrix[i][k] + Matrix[k][j]);
        //            }
        //        }
        //    }

        //    return shortestDistances;
        //}

        /// <summary>
        /// Perform Floyd-Warshall algorithm on the graph
        /// </summary>
        /// <returns>A function which takes in a source and a target string, and returns the best conversion rate from the source to the target</returns>
        public Func<string, string, (decimal, List<Node>)> FindBestConversionRate() // Too generalized use case, needs to specialize for assignment
        {
            var shortestDistances = (decimal[][])Matrix.Clone();
            var prev = Nodes.ToDictionary(x => x);

            for (int k = 0; k < Nodes.Length; k++)
            {
                for (int j = 0; j < Nodes.Length; j++) // i and j is swapped as the matrix is rotated by 90 degrees
                {
                    for (int i = 0; i < Nodes.Length; i++)
                    {
                        var org = Matrix[i][j];
                        var newval = Matrix[i][k] + Matrix[k][j];

                        if (newval < org)
                        {
                            shortestDistances[i][j] = newval;
                            // j gets k as a previous
                            prev[Nodes[j]] = Nodes[k];
                        }

                        //shortestDistances[i][j] = Math.Min(Matrix[i][j], Matrix[i][k] + Matrix[k][j]);
                    }
                }
            }

            PrintConversionRates(shortestDistances);

            return (source, target) =>
            {
                var src = GetNode(source);
                var tgt = GetNode(target);

                var res = new List<Node>();

                var curr = tgt;
                while (curr != src)
                {

                    var previous = prev[tgt];
                    
                }

                return (shortestDistances[src.ID][tgt.ID], res);
            };
        }

        void PrintConversionRates(decimal[][] conversionRates)
        {
            Console.Write("".PadRight(5));
            foreach (var node in Nodes)
                Console.Write(node.Name.PadLeft(10));
            Console.WriteLine();

            for (int i = 0; i < conversionRates.Length; i++)
            {
                Console.Write(Nodes[i].Name.PadRight(7));

                for (int j = 0; j < conversionRates[i].Length; j++)
                    Console.Write($"{conversionRates[i][j],9:0.0000} ");

                Console.WriteLine();
            }
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

            foreach (var d in distances)
                Console.WriteLine($"{(d.Value >= 0 ? "No arbitrage" : "Arbitrage")} found in {d.Key.Name}");

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
