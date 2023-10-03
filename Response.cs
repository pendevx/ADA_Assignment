using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ADA_Assignment
{
    class Rate
    {
        public string currency { get; set; }
        public string rate { get; set; }
    }
    class Response
    {
        //public List<Rate> rates { get; set; }
        public Dictionary<string, double> rates { get; set; }
        public bool success { get; set; }
    }
}
