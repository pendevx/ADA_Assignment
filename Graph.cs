using System;
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;

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
                if (weight == infinity) continue;
                if (Nodes[i] == n) continue;

                var e = new Edge(Nodes[i], n, weight, null);
                res.Add(e);
            }
            return res;
        }

        /// <summary>
        /// Perform Floyd-Warshall algorithm on the graph
        /// </summary>
        /// <returns>A function which takes in a source and a target string, and returns the best conversion rate from the source to the target</returns>
        public Func<string, string, (double, List<Node>?)> FindBestConversionRate() // Too generalized use case, needs to specialize for assignment
        {
            var shortestDistances = (decimal[][])Matrix.Clone(); // 2d decimal array of the shortest distances
            var predecessors = Nodes.Select(x => new Node[Nodes.Length].Select(y => x).ToArray()).ToArray(); // the previous nodes as a Node[][] 2d array

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
                            predecessors[i][j] = predecessors[k][j];
                        }
                    }
                }
            }

            PrintConversionRates(shortestDistances);

            return (source, target) =>
            {
                // get source and target node
                var src = GetNode(source);
                var tgt = GetNode(target);

                if (src == null)
                    throw new ArgumentNullException("source node was not found");
                if (target == null)
                    throw new ArgumentNullException("target node was not found");

                // the result (path)
                var res = new List<Node>();
                // the shortest distance from source to target
                var distResult = (double)shortestDistances[src.ID][tgt.ID];

                while (src != tgt)
                {
                    // if the result contains the target then we that means are trapped in a negative cycle on the path from src->target
                    // so we can break out of the loop and return null for the path to indicate that there is a negative cycle along the path from src->target
                    if (res.Contains(tgt))
                        return (distResult, null);

                    // add the target node to the result
                    // update the target to be its previous node as tracked in the predecessors 2d array above
                    res.Add(tgt);
                    tgt = predecessors[src.ID][tgt.ID];
                }

                // add the source node to complete the cycle
                // since we were tracking predecessors rather than children, the path was built in reverse, so we reverse it to correct the path
                res.Add(src);
                res.Reverse();

                var rate = Math.Pow(10, -distResult);
                return (rate, res);
            };
        }

        /// <summary>
        /// Prints the conversion rates
        /// </summary>
        /// <param name="weights">The conversion rates to be printed</param>
        void PrintConversionRates(decimal[][] weights)
        {
            Console.Write("".PadRight(5));
            foreach (var node in Nodes)
                Console.Write(node.Name.PadLeft(10));
            Console.WriteLine();

            for (int i = 0; i < weights.Length; i++)
            {
                Console.Write(Nodes[i].Name.PadRight(7));

                for (int j = 0; j < weights[i].Length; j++)
                {
                    var rate = Math.Pow(10, (double)-weights[i][j]);
                    Console.Write($"{rate,9:0.0000} ");
                }

                Console.WriteLine();
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Perform Bellman Ford algorithm on the graph
        /// </summary>
        /// <param name="src">The source node's name</param>
        /// <returns>A detected negative cycle, or null if none was found</returns>
        public IList<Node>? FindArbitrageOpportunities()
        {
            // get a random source node
            var source = Nodes[new Random().Next(Nodes.Length)];

            var n = Nodes.Length;
            var distances = new Dictionary<Node, decimal>(); // distances of each node
            var predecessors = new Dictionary<Node, Node>(); // predecessors of each node

            // initialize the distances to [0,inf,inf,inf,...]
            foreach (var node in Nodes) distances[node] = infinity;
            distances[source] = 0;

            void PerformCycle()
            {
                var newDistances = new Dictionary<Node, decimal>(distances); // updated distance from current cycle
                foreach (var node in Nodes)
                {
                    foreach (var edge in GetIncomingEdges(node))
                    {
                        var calculatedDistance = distances[edge.From] + edge.Weight;

                        if (calculatedDistance < distances[node])
                        {
                            // if calculatedDistance is lower value then update it in the distances and set the predecessor 
                            newDistances[node] = calculatedDistance;
                            predecessors[edge.To] = edge.From;
                        }
                    }
                }
                // update the distances to the new distances
                distances = newDistances;
            }

            // perform the cycle (relax edges) n-1 times
            for (int i = 0; i < n - 1; i++)
                PerformCycle();

            // new distances to check for change
            var newDistances = new Dictionary<Node, decimal>(distances);

            // run the cycle another n times to allow the negative cycle to spread across the entire graph
            // so it is detectable from any node
            for (int i = 0; i < n; i++)
                PerformCycle();

            // check for arbitrage opportunities
            // bellman ford can check for negative cycles by seeing if after at least one relaxation of edges, if there is a change in the distances
            var hasArbitrage = newDistances.Any(x => x.Value != distances[x.Key]);

            Console.WriteLine(hasArbitrage ? "Arbitrage found" : "No arbitrage found");
            if (!hasArbitrage) return null;

            var path = new List<Node>(); // the exact path of the negative cycle
            var curr = Nodes[0];
            
            while (true)
            {
                if (path.Contains(curr)) // exit if we have reached a node which we have already found (we have a negative cycle)
                    break;

                // add the current node to the path and backtrack current node to predecessor
                path.Add(curr);
                curr = predecessors[curr];
            }

            // add the first node to form full cycle
            // path was created in reverse (tracking predecessors rather than tracking children), so therefore reverse the path to correct the path
            path.Add(path[0]);
            path.Reverse();
            return path;
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

        /// <summary>
        /// Gets an edge
        /// </summary>
        /// <param name="from">The source node's name</param>
        /// <param name="to">The terminal node's name</param>
        /// <returns>The edge which goes from the source node <paramref name="from"/> to the terminal node <paramref name="to"/></returns>
        public Edge GetEdge(string from, string to)
        {
            var fromNode = GetNode(from);
            var toNode = GetNode(to);

            return GetEdge(fromNode, toNode);
        }

        /// <summary>
        /// Gets an edge
        /// </summary>
        /// <param name="from">The source node</param>
        /// <param name="to">The terminal node</param>
        /// <returns>The edge which goes from the source node <paramref name="from"/> to the terminal node <paramref name="to"/></returns>
        public Edge GetEdge(Node from, Node to)
        {
            if (from == null)
                throw new ArgumentNullException("value of 'from' doesn't exist");
            else if (to == null)
                throw new ArgumentNullException("value of 'to' doesn't exist");

            var edges = GetIncomingEdges(to);
            var edge = edges.Where(x => x.From == from).FirstOrDefault();

            if (edge == null)
                throw new NullReferenceException("no edge exist");

            return edge;
        }
    }
}
