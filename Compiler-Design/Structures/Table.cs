namespace Compiler_Design.Structures
{
    public class Table(string name)
    {
        public record Record(string FromState, string ToState, string Input);

        public IList<Record> Records { get; set; } = [];
        public string Name { get; } = name;
    }
}
