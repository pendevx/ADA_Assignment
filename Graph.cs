using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ADA_Assignment
{
    class Graph
    {
        public HashSet<Node> Nodes { get; private set; }
        public HashSet<Edge> Edges { get; private set; }

        public Graph()
        {
            Nodes = new();
            Edges = new();
        }

        public void AddNode(Node n)
        {
            Nodes.Add(n);
        }

        public void AddEdge(Edge e)
        {
            Edges.Add(e);
            e.To.AddIncoming(e);
        }

        public Dictionary<Node, double> BellmanFord(Node source)
        {
            int infinity = 2147400000;
            var n = Nodes.Count;
            var distances = new Dictionary<Node, double>();

            foreach (var node in Nodes) distances[node] = infinity;
            distances[source] = 0;

            void PerformCycle()
            {
                var newDistances = new Dictionary<Node, double>(distances);
                foreach (var node in Nodes)
                {
                    foreach (var edge in node.IncomingEdges)
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
            {
                PerformCycle();
            }

            var oldDistances = new Dictionary<Node, double>(distances).ToArray();
            PerformCycle();
            var distancesArr = distances.ToArray();
            for (int i = 0; i < oldDistances.Length; i++)
            {
                Console.WriteLine(oldDistances[i].Value == distancesArr[i].Value ? "No arbitrage" : "Arbitrage found");
            }

            return distances;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("FROM->TO    EDGE WEIGHT");

            foreach (var e in Edges)
            {
                double val = Math.Round(e.Weight, 4);
                if (val == -0) val = 0;
                sb.AppendLine($"{$"{e.From.Name}->{e.To.Name}".PadLeft(6)} {(val < 0 ? "" : " ")}      {val.ToString("0.0000")}");
            }

            return sb.ToString();
        }
    }
}
