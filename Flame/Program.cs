using FlameGraphNet.Core;
using Mono.Options;
using SimpleRoseTree;

namespace Flame;

public class Program
{
    static void Main(string[] args)
    {
        var show_help = false;

        var file = string.Empty;
        var p = new OptionSet () {
            { "f|file=", "csv file, containing data in ID,PARENT,CONTENT,METRIC form.",
                v => file = v },
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

        if (!data.Any())
        {
            Console.WriteLine ("No data");
            return;
        }

        FlameGraph newGraph = new FlameGraph(new FlameGraphOptions()
        {
            Title = file,
            Width = 1200,
            Height = 600,
        });

        var roots = Builder.BuildTree(data);

        var wrappedRoot = new FlameGraphNode<Node>(
            roots.First(),
            node => node.Content,
            node => node.Metric,
            node => node.Children
        );

        string outFile = $"flame.svg";
        DeleteFileWhenExists(outFile);
        newGraph.BuildTo(wrappedRoot, outFile);
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
        Console.WriteLine ("Usage: flame [OPTIONS]+");
        Console.WriteLine ("Create a flame graph from an appropriately formatted file.");
        Console.WriteLine ("If no options are set, read from stdin.");
        Console.WriteLine ();
        Console.WriteLine ("Options:");
        p.WriteOptionDescriptions (Console.Out);
    }

    private static void DeleteFileWhenExists(string resultFilePath)
    {
        if (File.Exists(resultFilePath))
        {
            File.Delete(resultFilePath);
        }
    }
}



