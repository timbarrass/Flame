namespace SimpleRoseTree;

public static class Tools
{
    public static void DisplayTree(List<Node> tree)
    {
        foreach (var node in tree)
        {
            Console.WriteLine($"{node.Id} {node.Content} {node.Children.Count()}");
            DisplayTree(node.Children, 1, new [] { node.Id });
        }
    }

    public static void Output(this List<Node> tree)
    {
        foreach (var node in tree)
        {
            // child, parent, name, duration
            Console.WriteLine($"{string.Join(',', node.Id, node.ParentId, node.Content, node.Metric)}");
            node.Children.Output();
        }
    }

    public static List<Node> AggregateRepeatedConsecutiveGroups(
        this List<Node> tree,
        int maxSequenceLength,
        int repeatThreshold
    )
    {
        var revised = new List<Node>();

        foreach (var node in tree)
        {
            var blocks = FindConsecutiveNonOverlappingRepeatingGroups(
                    node.Children, sn => sn.Content,
                    maxSequenceLength
                )
                .Where(pair => pair.Value.Sum(v => v.Count()) > repeatThreshold)
                .OrderByDescending(pair => pair.Key.Count())
                .FirstOrDefault(); // groups with more than 100 repeats

            var aggregates = new Dictionary<int, (int, Node)>();
            // replace the nodes at indexes in blocks
            // with a single simplenode that summarises them
            if (blocks.Key != null)
            {
                // for each block, create an aggregate node
                foreach (var block in blocks.Value)
                {
                    // work out sum for each component
                    var breakdown = node.Children
                        .Skip(block.First())
                        .Take(block.Count() * blocks.Key.Count())
                        .GroupBy(c => c.Content)
                        .ToDictionary(g => g.Key, g => g.Sum(n => n.Metric));

                    var aggregate = new Node
                    {
                        Id = node.Children[block.First()].Id,
                        Content = $"({block.Count()}) " + string.Join("|", blocks.Key.Select(k => $"{k} ({breakdown[k]})")),
                        Children = new List<Node>(),
                        Metric = node.Children
                            .Skip(block.First())
                            .Take(block.Count() * blocks.Key.Count())
                            .Sum(c => c.Metric),
                        ParentId = node.Id
                    };

                    aggregates.Add(block.First(), (block.Last() + blocks.Key.Count(), aggregate));
                }
            }

            var updatedChildren = new List<Node>();
            int ignoreUntil = -1;
            for (var i = 0; i < node.Children.Count(); i++)
            {
                if (aggregates.ContainsKey(i))
                {
                    updatedChildren.Add(aggregates[i].Item2);
                    ignoreUntil = aggregates[i].Item1;
                }
                else if (i > ignoreUntil)
                {
                    updatedChildren.Add(node.Children[i]);
                }
            }

            // modify based on consecutives
            var replacement = new Node
            {
                Id = node.Id,
                Content = node.Content,
                Children = AggregateRepeatedConsecutiveGroups(updatedChildren, maxSequenceLength, repeatThreshold),
                Metric = node.Metric,
                ParentId = node.ParentId
            };

            revised.Add(replacement);
        }

        return revised;
    }

    private static void DisplayTree(List<Node> tree, int depth, IEnumerable<int> ancestors)
    {
        foreach (var node in tree)
        {
            Console.WriteLine($"{string.Join(',', ancestors)} {node.Id} {node.Content} {node.Metric} {node.Children.Count()}".PadLeft(depth));
            DisplayTree(node.Children, depth + 1, ancestors.Append(node.Id));
        }
    }

    public static Dictionary<IEnumerable<string>, List<HashSet<int>>> FindConsecutiveNonOverlappingRepeatingGroups<T>(
        List<T> lst, Func<T, string> extract,
        int maxSequenceLength
    )
    {
        // Find all consecutive, non-overlapping repeated groups
        // Each repeated group has list of starting indexes and a count of repeats
        // Potentially multiple runs of the same group, separated
        // We don't know anything about the groups in advance
        // a is repeated, but non-consecutive. Ditto a, b ...

        // find all possible unique groups, with index of occurrence?
        var groups = new Dictionary<IEnumerable<string>, List<int>>(new StringCollectionComparer());
        for (var i = 0; i < lst.Count; i++)
        {
            for (var l = 1; l <= Math.Min(lst.Count - i, maxSequenceLength); l++)
            {
                var slice = lst.Skip(i).Take(l).Select(extract).ToArray();
                if (!groups.ContainsKey(slice))
                {
                    groups.Add(slice, new List<int> { i });
                }
                else
                {
                    groups[slice].Add(i);
                }
            }
        }

        // for each subgroup, find consecutive occurrences?
        var consecutives = new Dictionary<IEnumerable<string>, List<HashSet<int>>>();
        foreach (var group in groups)
        {
            var len = group.Key.Count();

            var run = false;
            var consecutive = new HashSet<int>();
            for (var i = 0; i < group.Value.Count() - 1; i++)
            {

                if (group.Value[i + 1] == group.Value[i] + len)
                {
                    run = true;
                    consecutive.Add(group.Value[i]);
                    consecutive.Add(group.Value[i + 1]); // bit wasteful
                }
                else
                {
                    if (run)
                    {
                        if (!consecutives.ContainsKey(group.Key))
                        {
                            consecutives.Add(group.Key, new List<HashSet<int>>());
                        }

                        consecutives[group.Key].Add(new HashSet<int>(consecutive));
                        consecutive.Clear();
                    }

                    run = false;
                }

            }

            if (consecutive.Any())
            {
                if (!consecutives.ContainsKey(group.Key))
                {
                    consecutives.Add(group.Key, new List<HashSet<int>>());
                }

                consecutives[group.Key].Add(consecutive);
            }
        }

        return consecutives;
    }
}