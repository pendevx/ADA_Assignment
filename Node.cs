namespace ADA_Assignment
{
    class Node
    {
        public string Name { get; }
        public int ID { get; set; }

        public Node(string name)
        {
            Name = name;
        }

        public static bool operator ==(Node n1, Node n2) => n1?.Name == n2?.Name;
        public static bool operator !=(Node n1, Node n2) => !(n1 == n2);
    }
}
