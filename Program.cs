using System.Text;
using System.Text.Json;

namespace ADA_Assignment
{
    class Program
    {
        static string _key;
        static readonly string[] allowedCurrencies = { "AUD", "RUB", "NZD", "USD", "EUR", "HKD", "GBP", "CNY", "CAD", "CHF", "KRW", "SEK", "INR", "BRL", "CZK", "TRY" };
        static readonly HttpClient _httpClient = new HttpClient();

        static void Main(string[] args)
        {
            try
            {
                _key = File.ReadAllText("./key.txt").Trim();
            } 
            catch (Exception)
            {
                Console.WriteLine("The key.txt file does not exist!");
                return;
            }
            
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

            Console.ReadLine();
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
        /// <param name="baseCurrency">The base currency of conversion rates</param>
        /// <returns>The API response as a string</returns>
        static async Task<string> FetchData(string baseCurrency) =>
            await _httpClient.GetStringAsync($"https://v6.exchangerate-api.com/v6/{_key}/latest/{baseCurrency}");

        /// <summary>
        /// Builds the graph from an array of responses
        /// </summary>
        /// <param name="rates">The rates to convert into the graph</param>
        /// <param name="currencies">The currencies which should be extracted</param>
        /// <returns>The graph built from the array of responses</returns>
        static Graph BuildGraph(Response[] rates, string[] currencies)
        {
            // Local function to extract the data
            var ExtractData = (Response r) => r.conversion_rates.Where(x => currencies.Contains(x.Key));

            // The list of conversion rates 
            // List<...>: a collection of base currencies
            // IEnumerable<...>: a collection of conversion rates
            // KeyValuePair<...>: the conversion rates from the base currency to the key of the KVP
            var conversionRates = new List<IEnumerable<KeyValuePair<string, double>>>(rates.Length);
            Array.ForEach(rates, x => conversionRates.Add(ExtractData(x)));

            // Create the graph 
            var graph = new Graph(currencies.Length);
            // Create nodes to keep track of, required later
            var nodes = new Node[currencies.Length];

            // For each currency, create a new node and add it to the graph
            for (int i = 0; i < currencies.Length; i++)
            {
                var node = new Node(currencies[i]);
                graph.AddNode(node);
                nodes[i] = node;
            }

            // For each conversion rate, add it as an edge to the graph
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
    }
}
