using System.Text;

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

        public Dictionary<Node, decimal> BellmanFord(Node source)
        {
            if (source == null) throw new ArgumentNullException("source cannot be null");
            
            int infinity = 2147000000;
            var n = Nodes.Count;
            var distances = new Dictionary<Node, decimal>();

            foreach (var node in Nodes) distances[node] = infinity;
            distances[source] = 0;

            void PerformCycle()
            {
                var newDistances = new Dictionary<Node, decimal>(distances);
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
                PerformCycle();

            var oldDistances = new Dictionary<Node, decimal>(distances);

            foreach (var d in distances)
                Console.WriteLine($"{d.Key.Name} {d.Value}");

            PerformCycle();

            foreach (var d in distances)
                Console.WriteLine(d.Value == oldDistances[d.Key] ? "No arbitrage" : $"Arbitrage found in {d.Key.Name}");

            return distances;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("FROM->TO    EDGE WEIGHT");

            foreach (var e in Edges)
            {
                double val = Math.Round((double)e.Weight, 4);
                if (val == -0) val = 0;
                sb.AppendLine($"{$"{e.From.Name}->{e.To.Name}".PadLeft(6)} {(val < 0 ? "" : " ")}      {val.ToString("0.0000")}");
            }

            return sb.ToString();
        }

        public Node GetNode(string name)
        {
            var res = Nodes.Where(x => x.Name == name).FirstOrDefault();
            return res;
        }
    }
}
