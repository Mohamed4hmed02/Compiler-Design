using System.Text;
using Compiler_Design.Structures;

namespace Compiler_Design
{
    public class RegexToNfa
    {
        private class Body
        {
            public string Expression { get; set; } = string.Empty;
            public char Operator { get; set; }
            public Tree<Body>.Node? RightOperatorNode { get; set; }
        }

        private readonly string _special = "()+*|";
        private readonly char _noneOperator = 'N';
        private readonly IList<Table> _tables = [];

        public IEnumerable<Table> Transform(string expression)
        {
            var strBuilder = new StringBuilder();

            for (int i = 0; i < expression.Length - 1; i++)
            {
                strBuilder.Append(expression[i]);
                //za|bb*|aa|n+b
                if ((!_special.Contains(expression[i]) && !_special.Contains(expression[i + 1])) ||
                    ((expression[i] == '*' || expression[i] == '+') && !_special.Contains(expression[i + 1])))
                    strBuilder.Append('.');
            }
            strBuilder.Append(expression.Last());

            var tree = ConstructTree(strBuilder.ToString());
            ConstructNfa(tree.Root);

            //foreach (var item in tree.GetNodes())
            //{
            //    Console.WriteLine($"{item?.Body?.Expression} {item?.Body?.Operator}");
            //}

            return _tables;
        }

        private void ConstructNfa(Tree<Body>.Node? node)
        {
            while (node is not null)
            {
                if (node.Left is not null)
                {
                    var tbl = Determine(node.Left);
                    if (tbl.Records.Any())
                        _tables.Add(tbl);
                }

                if (node.Right is not null)
                {
                    var tbl = Determine(node.Right);
                    if (tbl.Records.Any())
                        _tables.Add(tbl);
                }

                node = node.Right;
            }
        }

        private Tree<Body> ConstructTree(string expression)
        {
            var tree = new Tree<Body>();

            int lastPoint = 0;
            int lastPointForOr = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                //0123456789
                //z.a|b.b*|a.a|n*.b
                if (expression[i] == '|')
                {
                    var rightNode = new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = expression[(i + 1)..],
                            Operator = _noneOperator
                        },
                    };

                    tree.InsertFromLeft(new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = expression.Substring(lastPointForOr, i - lastPointForOr),
                            Operator = '|',
                            RightOperatorNode = rightNode
                        },
                    });

                    tree.InsertFromLeft(rightNode);

                    lastPoint = lastPointForOr = i + 1;
                }
                else if (expression[i] == '.')
                {
                    string rightExpression = expression[i + 1].ToString();
                    if ((i + 2) < expression.Length && (expression[i + 2] == '*' || expression[i + 2] == '+'))
                        rightExpression += expression[i + 2];

                    var rightNode = new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = rightExpression,
                            Operator = _noneOperator
                        },
                    };

                    int lp = lastPoint;
                    if (i - lp==0)
                    {
                        int j = lp;
                        while (j-- > 0 && expression[j] != '|') ;
                        lp = ++j;
                    }

                    tree.InsertFromLeft(new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = expression.Substring(lp, i - lp),
                            Operator = '.',
                            RightOperatorNode = rightNode
                        }
                    });

                    tree.InsertFromLeft(rightNode);

                    lastPoint = i + 1;
                }
                else if (expression[i] == '+')
                {
                    tree.InsertFromLeft(new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = expression.Substring(lastPoint, i - lastPoint),
                            Operator = '+'
                        }
                    });

                    lastPoint = i + 1;
                }
                else if (expression[i] == '*')
                {
                    tree.InsertFromLeft(new Tree<Body>.Node
                    {
                        Body = new Body
                        {
                            Expression = expression.Substring(lastPoint, i - lastPoint),
                            Operator = '*'
                        }
                    });

                    lastPoint = i + 1;
                }
                else if (expression[i] == '(')
                {
                    // implementation
                }
            }

            return tree;
        }

        private Table Determine(Tree<Body>.Node node)
        {
            var leftBody = node.Body!;

            if (leftBody.Operator == '|')
            {
                var rightBody = node.Body!.RightOperatorNode!.Body;

                return OrCase(leftBody.Expression, rightBody!.Expression, $"{leftBody.Expression} union {rightBody.Expression}");
            }
            else if (leftBody.Operator == '+')
                return PlusCase(leftBody.Expression, $"{leftBody.Expression}+");
            else if (leftBody.Operator == '*')
                return ClosureCase(leftBody.Expression, $"{leftBody.Expression}*");
            else if (leftBody.Operator == '.')
            {
                var rightBody = node.Body!.RightOperatorNode!.Body;

                return ConcatCase(leftBody.Expression, rightBody!.Expression, $"{leftBody.Expression} concat {rightBody.Expression}");
            }
            else if (leftBody.Operator == _noneOperator)
                return new Table("");
            return BaseCase(leftBody.Expression, leftBody.Expression);
        }

        private static Table OrCase(string left, string right, string name)
        {
            var tbl = new Table(name);

            tbl.Records.Add(new Table.Record($"q0", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q0", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q1", $"q3", left));
            tbl.Records.Add(new Table.Record($"q2", $"q4", right));
            tbl.Records.Add(new Table.Record($"q3", $"q5", "@"));
            tbl.Records.Add(new Table.Record($"q4", $"q5", "@"));

            return tbl;
        }

        private static Table ConcatCase(string left, string right, string name)
        {
            var tbl = new Table(name);

            tbl.Records.Add(new Table.Record($"q0", $"q1", left));
            tbl.Records.Add(new Table.Record($"q1", $"q2", right));

            return tbl;
        }

        private static Table BaseCase(string input, string name)
        {
            var tbl = new Table(name);

            tbl.Records.Add(new Table.Record($"q0", $"q1", input));

            return tbl;
        }

        private static Table ClosureCase(string input, string name)
        {
            var tbl = new Table(name);

            tbl.Records.Add(new Table.Record($"q0", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q0", $"q3", "@"));
            tbl.Records.Add(new Table.Record($"q1", $"q2", input));
            tbl.Records.Add(new Table.Record($"q2", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q2", $"q3", "@"));

            return tbl;
        }

        private static Table PlusCase(string input, string name)
        {
            var tbl = new Table(name);

            tbl.Records.Add(new Table.Record($"q0", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q1", $"q2", input));
            tbl.Records.Add(new Table.Record($"q2", $"q1", "@"));
            tbl.Records.Add(new Table.Record($"q2", $"q3", "@"));

            return tbl;
        }
    }
}
