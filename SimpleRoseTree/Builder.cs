namespace SimpleRoseTree;

public class Builder
{
    public static List<Node> BuildTree(List<(int, int, string, double)> data)
    {
        var nodes = new Dictionary<int, Node>();

        foreach ((int childId, int parentId, string content, double metric) in data)
        {
            // Create nodes if they don't exist
            if (!nodes.ContainsKey(parentId))
                nodes[parentId] = new Node
                {
                    Id = parentId,
                    Metric = metric,
                    Content = content,
                    ParentId = -1
                };
            if (!nodes.ContainsKey(childId))
            {
                nodes[childId] = new Node
                {
                    Id = childId,
                    Metric = metric,
                    Content = content,
                    ParentId = parentId
                };

                // Add child to parent
                nodes[parentId].Children.Add(nodes[childId]);
            }
        }

        // Find root nodes (those with no parent)
        return nodes.Values.Where(node => node.Id == -1).ToList();
    }

    public static List<(int, int, string, double)> ParseLines(IEnumerable<string> lines)
    {
        var data = new List<(int, int, string, double)>();
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            int childId = int.Parse(parts[0]);
            int parentId = int.Parse(parts[1]);
            string name = parts[2].Trim();
            double duration = double.Parse(parts[3]);

            data.Add((childId, parentId, name, duration));
        }

        return data;
    }
}