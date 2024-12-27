using System.Collections;

namespace SimpleRoseTree;

public sealed class StringCollectionComparer : EqualityComparer<IEnumerable<string>>
{
    // Hilariously, this doesn't work for List<string>
    public override bool Equals(IEnumerable<string> x, IEnumerable<string> y)
        => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

    public override int GetHashCode(IEnumerable<string> x)
        => StructuralComparisons.StructuralEqualityComparer.GetHashCode(x);
}