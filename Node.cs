using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADA_Assignment
{
    class Node
    {
        public string Name { get; }
        public HashSet<Edge> IncomingEdges { get; private set; }

        public Node(string name)
        {
            Name = name;
            IncomingEdges = new();
        }

        public void AddIncoming(Edge e)
        {
            IncomingEdges.Add(e);
        }

        public static bool operator ==(Node n1, Node n2) => n1?.Name == n2?.Name;
        public static bool operator !=(Node n1, Node n2) => !(n1 == n2);
    }
}
