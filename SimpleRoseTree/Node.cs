namespace SimpleRoseTree;

public class Node
{
    public string Content { get; set; }

    public double Metric { get; set; }

    public int Id { get; set; }

    public int ParentId { get; set; }

    public List<Node> Children { get; set; } = new List<Node>();

    public override string ToString() => string.Join(',', Id, ParentId, Content, Metric);
}