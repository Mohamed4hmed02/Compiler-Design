using Compiler_Design;

var nfa = new RegexToNfa();

var tbls = nfa.Transform("za|bb*|aa|n+");
foreach (var tbl in tbls)
{
    Console.WriteLine(tbl.Name);
    foreach (var item in tbl.Records)
    {
        Console.WriteLine($"|{item.FromState}|{item.ToState}|{item.Input}|");
    }
    Console.WriteLine();
}
