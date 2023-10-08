using System;
using System.Collections.Immutable;
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

            for (int i = 0; i < size; i++)
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
        List<Edge> GetIncomingEdges(Node n)
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
        /// Perform Floyd-Warshall algorithm on the graph
        /// </summary>
        /// <returns>A function which takes in a source and a target string, and returns the best conversion rate from the source to the target</returns>
        public Func<string, string, (decimal, List<Node>)> FindBestConversionRate() // Too generalized use case, needs to specialize for assignment
        {
            var shortestDistances = (decimal[][])Matrix.Clone();
            var prev = Nodes.Select(x => new Node[Nodes.Length].Select(y =>  x).ToArray()).ToArray();


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

                            // set the previous of [i][j] to be the previous of [k][j]
                            prev[i][j] = prev[k][j];
                        }
                    }
                }
            }

            PrintConversionRates(shortestDistances);

            return (source, target) =>
            {
                var src = GetNode(source);
                var tgt = GetNode(target);
                var res = new List<Node>();

                while (src != tgt)
                {
                    res.Add(tgt);
                    tgt = prev[src.ID][tgt.ID];
                }
                res.Add(src);
                res.Reverse();

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
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Perform Bellman Ford algorithm on the graph
        /// </summary>
        /// <param name="src">The source node's name</param>
        /// <returns>Distances from the source node to all other nodes</returns>
        /// <exception cref="ArgumentNullException">The source node is null</exception>
        public Dictionary<Node, decimal> FindArbitrageOpportunities(string src)
        {
            var source = GetNode(src);
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

                        if (edge.From == source)
                            newDistances[node] = Math.Min(newDistances[node], edge.Weight);
                        else
                            newDistances[node] = Math.Min(newDistances[node], distances[edge.From] + edge.Weight);
                    }
                }
                distances = newDistances;
            }

            for (int i = 0; i < n - 1; i++)
                PerformCycle();

            Console.WriteLine($"{(distances[source] < 0 ? "Arbitrage" : "No arbitrage")} in {src}");

            return distances;
        }

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
