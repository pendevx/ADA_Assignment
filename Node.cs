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
        public HashSet<Edge> OutgoingEdges { get; private set; }

        public Node(string name)
        {
            Name = name;
            OutgoingEdges = new();
        }

        public void AddOutgoing(Edge e)
        {
            OutgoingEdges.Add(e);
        }

        public static bool operator ==(Node n1, Node n2)
        {
            return n1?.Name == n2?.Name;
        }

        public static bool operator !=(Node n1, Node n2)
        {
            return n1?.Name != n2?.Name;
        }
    }
}
