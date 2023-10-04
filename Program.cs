using System.Text;
using System.Text.Json;

namespace ADA_Assignment
{
    class Program
    {
        private static string _key = "7bab9b29ab1678992a96ca94";
        private static string[] currencies = { "AUD", "RUB", "NZD", "USD", "EUR", "HKD", "GBP" };

        static void Main(string[] args)
        {
            var client = new HttpClient();

            var tasks = new Task<string>[currencies.Length];

            for (int i = 0; i < currencies.Length; i++)
            {
                var res = FetchData(client, currencies[i]);
                tasks[i] = res;
            }
            Task.WaitAll(tasks);
            
            var collectedData = new Response[tasks.Length];
            
            for (int i = 0; i < tasks.Length; i++)
            {
                Response data = JsonSerializer.Deserialize<Response>(new MemoryStream(Encoding.UTF8.GetBytes(tasks[i].Result)));
                collectedData[i] = data;
            }

            var graph = BuildGraph(collectedData);
            var source = graph.GetNode("NZD");

            var x = graph.BellmanFord(source);

            foreach (var distance in x)
            {
                Console.WriteLine($"{distance.Key.Name} {distance.Value}");
            }
        }

        static async Task<string> FetchData(HttpClient client, string baseCurrency)
        {
            var response = await client.GetAsync($"https://v6.exchangerate-api.com/v6/{_key}/latest/{baseCurrency}");
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        static Graph BuildGraph(Response[] rates)
        {
            var ExtractData = (Response r) => r.conversion_rates.Where(x => currencies.Contains(x.Key));

            var conversionRates = new List<IEnumerable<KeyValuePair<string, double>>>(rates.Length);
            Array.ForEach(rates, x => conversionRates.Add(ExtractData(x)));

            var graph = new Graph();
            var nodes = new Node[currencies.Length];

            for (int i = 0; i < currencies.Length; i++)
            {
                var node = new Node(currencies[i]);
                graph.AddNode(node);
                nodes[i] = node;
            }

            for (int i = 0; i < conversionRates.Count; i++)
            {
                foreach (var rate in conversionRates[i])
                {
                    var to = graph.GetNode(rate.Key);
                    var edge = new Edge(nodes[i], to, rate.Value);
                    graph.AddEdge(edge);
                }
            }

            return graph;
        }


        // EXAMPLE GRAPHS FROM LECTURE SLIDES ---------------------------------------------------------------------------------------------------------------------------------------------------
        static Graph Graph1()
        {
            var graph = new Graph();

            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            var aa = new Edge(a, a, 1);
            var ab = new Edge(a, b, 0.651);
            var ac = new Edge(a, c, 0.581);

            var ba = new Edge(b, a, 1.531);
            var bb = new Edge(b, b, 1);
            var bc = new Edge(b, c, 0.952);

            var ca = new Edge(c, a, 1.711);
            var cb = new Edge(c, b, 1.049);
            var cc = new Edge(c, c, 1);

            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);
            graph.AddEdge(aa);
            graph.AddEdge(ab);
            graph.AddEdge(ac);
            graph.AddEdge(ba);
            graph.AddEdge(bb);
            graph.AddEdge(bc);
            graph.AddEdge(ca);
            graph.AddEdge(cb);
            graph.AddEdge(cc);

            return graph;
        }

        static Graph Graph2()
        {
            var graph = new Graph();

            var s = new Node("S");
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var d = new Node("D");
            var e = new Node("E");
            var f = new Node("F");
            var G = new Node("G");

            var sa = new Edge(s, a, 10, null);
            var sg = new Edge(s, G, 8, null);
            var ae = new Edge(a, e, 2, null);
            var ba = new Edge(b, a, 1, null);
            var bc = new Edge(b, c, 1, null);
            var cd = new Edge(c, d, 3, null);
            var de = new Edge(d, e, -1, null);
            var eb = new Edge(e, b, -2, null);
            var fa = new Edge(f, a, -4, null);
            var fe = new Edge(f, e, -1, null);
            var gf = new Edge(G, f, 1, null);

            graph.AddNode(s);
            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);
            graph.AddNode(d);
            graph.AddNode(e);
            graph.AddNode(f);
            graph.AddNode(G);
            graph.AddEdge(sa);
            graph.AddEdge(sg);
            graph.AddEdge(ae);
            graph.AddEdge(ba);
            graph.AddEdge(bc);
            graph.AddEdge(cd);
            graph.AddEdge(de);
            graph.AddEdge(eb);
            graph.AddEdge(fa);
            graph.AddEdge(fe);
            graph.AddEdge(gf);

            Console.WriteLine(graph.ToString());

            var res = graph.BellmanFord(s);

            foreach (var x in res) Console.WriteLine($"{x.Key.Name} {x.Value}");

            return graph;
        }
    }
}
