
using NUnit.Framework;

namespace tests;

[TestFixture]
public class Class1
{


    [Test]
    public void FindAllConsecutiveNonOverlappingRepeatedGroups()
    {
        // var lst = new List<string>() { "a", "b", "c", "a", "b", "c", "a", "b", "c" };
        var lst = new List<string>() { "a", "b", "c", "a", "b", "c", "a", "a", "b", "c", "a", "b", "c" };

        var consecutives = SimpleRoseTree.Tools.FindConsecutiveNonOverlappingRepeatingGroups(
            lst,
            s => s,
            10
        );

        Console.WriteLine("Consecutives:");

        foreach (var kvp in consecutives)
        {
            Console.WriteLine(
                  $"Key: {string.Join(",", kvp.Key)} "
                + $"Values: {string.Join(";", kvp.Value.Select(v => string.Join(",", v)))} "
                + $"Blocks: {kvp.Value.Count} "
                + $"TotalRepeats: {kvp.Value.Sum(v => v.Count - 1)}"
            );
        }

        // replace repeated groups with aggregate groups (in actual program would offer choice)
    }
}