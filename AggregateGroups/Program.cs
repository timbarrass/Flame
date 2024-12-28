using Mono.Options;
using SimpleRoseTree;

namespace AggregateGroups;

class Program
{
    static void Main(string[] args)
    {
        var show_help = false;
        var repeatThreshold = 100;
        var maxSequenceLength = 10;

        var file = string.Empty;
        var p = new OptionSet () {
            { "f|file=", "csv file, containing data in ID,PARENT,CONTENT,METRIC form.",
                v => file = v },
            { "t|repeatThreshold=", "threshold number of sequence repeats to trigger aggregation (default: 100).",
                v => repeatThreshold = int.Parse(v) },
            { "l|maxSequenceLength=", "max. length of repeated sequence to find (default: 10).",
                v => repeatThreshold = int.Parse(v) },
            { "h|help",  "show this message and exit",
                v => show_help = v != null },
        };

        List<string> additional;
        try {
            additional = p.Parse (args);
        }
        catch (OptionException e) {
            Console.WriteLine ("Try `flame --help' for more information.");
            return;
        }

        if (show_help) {
            ShowHelp (p);
            return;
        }

        var lines = string.IsNullOrEmpty(file)
            ? ReadStdIn().Skip(1)
            : File.ReadAllLines(file).Skip(1);

       var data = Builder.ParseLines(lines);

        var roots = Builder
            .BuildTree(data)
            .AggregateRepeatedConsecutiveGroups(maxSequenceLength, repeatThreshold);

        Console.WriteLine("Id,ParentId,Content,Metric");
        roots.Output();
    }

    private static IEnumerable<string> ReadStdIn()
    {
        string s;
        while ((s = Console.ReadLine()) != null)
        {
            yield return s;
        }
    }

    static void ShowHelp (OptionSet p)
    {
        Console.WriteLine ("Usage: aggregate [OPTIONS]+");
        Console.WriteLine ("Create a rose tree from an appropriately formatted file, then process");
        Console.WriteLine ("to identify repeated patterns and agregate them.");
        Console.WriteLine ("If no options are set, read from stdin.");
        Console.WriteLine ();
        Console.WriteLine ("Options:");
        p.WriteOptionDescriptions (Console.Out);
    }
}