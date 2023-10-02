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
            e.From.AddOutgoing(e);
        }

        public Dictionary<Node, double> BellmanFord(Node source)
        {
            int infinity = 2147400000;
            var n = Nodes.Count;
            var distances = new Dictionary<Node, double>();
            var visited = new HashSet<Node>();

            foreach (var node in Nodes) distances[node] = infinity;
            distances[source] = 0;

            void PerformCycle(int i)
            {
                foreach (var node in Nodes)
                {
                    if (distances[node] == infinity) continue;

                    foreach (var e in node.OutgoingEdges)
                    {
                        if (distances[e.To] == infinity)
                        {

                        }
                        distances[e.To] = Math.Min(distances[e.To] == infinity ? e.Weight : distances[e.To], distances[e.To] + e.Weight);
                    }
                }

                foreach (var node in distances)
                {
                    Console.WriteLine($"{node.Key.Name} {node.Value}");
                }
                Console.WriteLine();
            }

            for (int i = 0; i < n - 1; i++)
            {
                foreach (var node in distances)
                {
                    Console.WriteLine($"{node.Key.Name} {node.Value}");
                }
                Console.WriteLine();
                
                PerformCycle(i);
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
