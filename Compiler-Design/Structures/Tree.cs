namespace Compiler_Design.Structures
{
    public class Tree<TType> where TType : notnull
    {
        public class Node
        {
            public TType? Body { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public bool IsLeaf => Left is null && Right is null;
        }

        private Node _current;
        public Node Root { get; }

        public Tree()
        {
            Root = new();
            _current = Root;
        }

        public void InsertFromLeft(Node newNode)
        {
            if (_current.Left is null)
                _current.Left = newNode;
            else if (_current.Right is null)
            {
                _current.Right = newNode;
                _current = _current.Right;
            }
        }

        public IEnumerable<Node> GetNodes()
        {
            var lst = new List<Node>();
            var next = Root;
            while (next is not null)
            {
                if (next.Left is not null)
                    lst.Add(next.Left);
                if (next.Right is not null)
                    lst.Add(next.Right);
                next = next.Right;
            }
            return lst;
        }
    }
}
