using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace ADA_Assignment
{
    class Program
    {
        static readonly string _key = "592ba3456dc1f48f236c1d34";
        static readonly string[] allowedCurrencies = { "AUD", "RUB", "NZD", "USD", "EUR", "HKD", "GBP", "CNY", "CAD", "CHF", "KRW", "SEK", "INR", "BRL", "CZK", "TRY" };
        static readonly HttpClient _httpClient = new HttpClient();

        static void Main(string[] args)
        {
            // print available currencies
            Console.WriteLine("Here is a list of the allowed currencies: ");
            foreach (var currency in allowedCurrencies)
                Console.Write($"{currency} ");

            // prompt for currencies
            Console.Write("\n\nPlease choose at least four currencies, typing them in separated by a space: ");

            List<string> currencies = null;

            // get input + input validation
            while (true)
            {
                var input = Console.ReadLine().ToUpper().Trim().Split(" ").GroupBy(x => x).Select(x => x.FirstOrDefault());

                if (input.Count() >= 4 && input.All(x => allowedCurrencies.Contains(x)))
                {
                    currencies = input.ToList();
                    break;
                }

                Console.WriteLine("Please follow the input criteria and re-input your options.");
            }

            // fetch data
            var tasks = currencies.Select(x => FetchData(x)).ToArray();
            Task.WaitAll(tasks);

            // deserialize JSON response
            var collectedData = tasks.Select(x => DeserializeJson<Response>(x.Result)).ToArray();

            // build graph
            var graph = BuildGraph(collectedData, currencies.ToArray());

            Console.WriteLine("\nWe will first check if there is any arbitrary opportunities: ");

            // find arbitrage opportunities and get the path if there is one
            var bfPath = graph.FindArbitrageOpportunities();
            if (bfPath != null)
            {
                foreach (var x in bfPath)
                    Console.Write(x.Name + " ");
            }

            Console.WriteLine();

            // get the best conversion rate between all of the nodes (floyd warshall)
            var res = graph.FindBestConversionRate();

            // prompt for input of 2 currencies
            Console.WriteLine("Enter the source currency and terminal currency, and we will find the best change exchange path for you");
            Console.WriteLine("Please enter the currencies split up using a space (e.g. 'NZD USD'):");

            // get the conversion rate and path for best conversion rate
            var inp = Console.ReadLine().ToUpper().Trim().Split(" ");
            var (rate, path) = res(inp[0], inp[1]);
            Console.WriteLine(rate);

            // if null, dont print path (found negative cycle)
            // otherwise, print the path
            if (path == null)
                Console.WriteLine("There is an arbitrage cycle along this path! You may keep swapping money between the currencies to become rich");
            else
                foreach (var node in path) Console.Write($"{node.Name} ");
        }

        /// <summary>
        /// Deserialize a JSON string into an object of type T
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized into</typeparam>
        /// <param name="str">The JSON string to deserialize</param>
        /// <returns>The object deserialized from its JSON format</returns>
        static T DeserializeJson<T>(string str) => JsonSerializer.Deserialize<T>(new MemoryStream(Encoding.UTF8.GetBytes(str)));

        /// <summary>
        /// Fetches data from the api
        /// </summary>
        /// <param name="client">The HttpClient to make the request</param>
        /// <param name="baseCurrency">The base currency</param>
        /// <returns>The API response as a string</returns>
        static async Task<string> FetchData(string baseCurrency) =>
            await _httpClient.GetStringAsync($"https://v6.exchangerate-api.com/v6/{_key}/latest/{baseCurrency}");

        /// <summary>
        /// Builds the graph from an array of responses
        /// </summary>
        /// <param name="rates">The rates to convert into the graph</param>
        /// <returns>The graph built from the array of responses</returns>
        static Graph BuildGraph(Response[] rates, string[] currencies)
        {
            var ExtractData = (Response r) => r.conversion_rates.Where(x => currencies.Contains(x.Key));

            var conversionRates = new List<IEnumerable<KeyValuePair<string, double>>>(rates.Length);
            Array.ForEach(rates, x => conversionRates.Add(ExtractData(x)));

            var graph = new Graph(currencies.Length);
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


        //// EXAMPLE GRAPHS FROM LECTURE SLIDES ---------------------------------------------------------------------------------------------------------------------------------------------------
        static Graph Graph1()
        {
            var graph = new Graph(3);

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
            var graph = new Graph(8);

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

            return graph;
        }

        static Graph Graph3()
        {
            var graph = new Graph(5);

            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var d = new Node("D");
            var e = new Node("E");

            var ab = new Edge(a, b, 3, null);
            var ac = new Edge(a, c, 8, null);
            var ae = new Edge(a, e, -4, null);
            var bd = new Edge(b, d, 1, null);
            var be = new Edge(b, e, 7, null);
            var cb = new Edge(c, b, 4, null);
            var da = new Edge(d, a, 2, null);
            var dc = new Edge(d, c, -5, null);
            var ed = new Edge(e, d, 6, null);

            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);
            graph.AddNode(d);
            graph.AddNode(e);

            graph.AddEdge(ab);
            graph.AddEdge(ac);
            graph.AddEdge(ae);
            graph.AddEdge(bd);
            graph.AddEdge(be);
            graph.AddEdge(cb);
            graph.AddEdge(da);
            graph.AddEdge(dc);
            graph.AddEdge(ed);

            return graph;
        }

        // no negative cycle
        static Graph Graph4()
        {
            var graph = new Graph(6);

            var s = new Node("S");
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var d = new Node("D");
            var e = new Node("E");

            var sa = new Edge(s, a, 10, null);
            var sc = new Edge(s, c, 3, null);
            var ab = new Edge(a, b, 2, null);
            var ad = new Edge(a, d, -3, null);
            var ba = new Edge(b, a, -1, null);
            var cd = new Edge(c, d, 3, null);
            var db = new Edge(d, b, 4, null);
            var de = new Edge(d, e, 1, null);
            var eb = new Edge(e, b, 4, null);

            graph.AddNode(s);
            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);
            graph.AddNode(d);
            graph.AddNode(e);

            graph.AddEdge(sa);
            graph.AddEdge(sc);
            graph.AddEdge(ab);
            graph.AddEdge(ad);
            graph.AddEdge(ba);
            graph.AddEdge(cd);
            graph.AddEdge(db);
            graph.AddEdge(de);
            graph.AddEdge(eb);

            return graph;
        }

        // negative cycle A-B-D-C
        static Graph Graph5()
        {
            var graph = new Graph(6);

            var s = new Node("S");
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var d = new Node("D");
            var e = new Node("E");

            var sa = new Edge(s, a, 5, null);
            var ab = new Edge(a, b, 1, null);
            var bc = new Edge(b, c, 2, null);
            var bd = new Edge(b, d, -4, null);
            var be = new Edge(b, e, 5, null);
            var cs = new Edge(c, s, -2, null);
            var ca = new Edge(c, a, -2, null);
            var dc = new Edge(d, c, 4, null);
            var ed = new Edge(e, d, 5, null);

            graph.AddNode(s);
            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);
            graph.AddNode(d);
            graph.AddNode(e);

            graph.AddEdge(sa);
            graph.AddEdge(ab);
            graph.AddEdge(bc);
            graph.AddEdge(bd);
            graph.AddEdge(be);
            graph.AddEdge(cs);
            graph.AddEdge(ca);
            graph.AddEdge(dc);
            graph.AddEdge(ed);

            return graph;
        }

        // negative cycle A-B-C
        static Graph Graph6()
        {
            var graph = new Graph(3);

            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            var ab = new Edge(a, b, 1, null);
            var bc = new Edge(b, c, 2, null);
            var ca = new Edge(c, a, -4, null);

            graph.AddNode(a);
            graph.AddNode(b);
            graph.AddNode(c);

            graph.AddEdge(ab);
            graph.AddEdge(bc);
            graph.AddEdge(ca);

            return graph;
        }
    }
}
