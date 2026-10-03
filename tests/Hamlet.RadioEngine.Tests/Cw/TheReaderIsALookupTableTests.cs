using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;
using Symbol = Hamlet.RadioEngine.Cw.CwSymbol;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE READER IS A LOOKUP TABLE** (work instruction 532, HM-DEC-236): the reader takes the gate's stream of dots, dashes,
/// letter ends and word ends and looks each letter up; it decides nothing about the marks.
/// </summary>
public sealed partial class TheReaderIsALookupTableTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where what was found is printed.</param>
    public TheReaderIsALookupTableTests(ITestOutputHelper output) => _output = output;

    private static string ReaderSource([System.Runtime.CompilerServices.CallerFilePath] string here = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "..", "src", "Hamlet.RadioEngine", "Cw", "CwRunReader.cs"));

    [GeneratedRegex(@"(?<![A-Za-z_])\d+(\.\d+)?")]
    private static partial Regex Number();

    [GeneratedRegex(@"time|second|length|level|pitch|score|hertz|hz|db|millisecond|duration", RegexOptions.IgnoreCase)]
    private static partial Regex Figure();

    /// <remarks>
    /// Reads <c>src\Hamlet.RadioEngine\Cw\CwRunReader.cs</c> with its comments taken out, and fails on a numeric literal
    /// other than 0 or 1, or on any word for a time, a length, a level, a pitch or a score.
    /// </remarks>
    [Fact]
    public void TheReaderIsALookupTable()
    {
        var path = ReaderSource();
        var code = File.ReadAllLines(path)
            .Select(l => l.Contains("//", StringComparison.Ordinal) ? l[..l.IndexOf("//", StringComparison.Ordinal)] : l)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();
        var numbers = code.SelectMany(l => Number().Matches(l).Select(m => m.Value)).Where(n => n != "0" && n != "1").ToList();
        var figures = code.SelectMany(l => Figure().Matches(l).Select(m => m.Value)).ToList();

        _output.WriteLine($"{path}: {File.ReadAllLines(path).Length} lines, {code.Count} of code; numbers other than 0 and 1: {numbers.Count}; times, lengths, levels, pitches or scores: {figures.Count}");

        Assert.Empty(numbers);
        Assert.Empty(figures);
    }

    private static CwCharacter Reading() => new(MorseAlphabet.Unreadable, CwConfidence.High, 1, string.Empty, double.NaN, 20, TimeSpan.Zero);

    private static IEnumerable<Symbol> Letter(string pattern)
        => pattern.Select(c => new Symbol(c == '-' ? CwSymbolKind.Dash : CwSymbolKind.Dot))
            .Append(new Symbol(CwSymbolKind.LetterEnd) { Reading = Reading(), Marks = Array.Empty<CwMark>() });

    private static string Read(params string[] letters)
    {
        var reader = new CwRunReader();
        var text = new List<string>();

        reader.CharacterRead += c => text.Add(c.Text);
        reader.Take(letters.SelectMany(Letter));

        return string.Concat(text);
    }

    /// <remarks>`--..  -..  ...-` in, `ZDV` out; `.-.-.` is `&lt;AR&gt;`; a pattern the table does not hold is the placeholder.</remarks>
    [Fact]
    public void TheReaderReadsSymbols()
    {
        Assert.Equal("ZDV", Read("--..", "-..", "...-"));
        Assert.Equal("<AR>", Read(".-.-."));
        Assert.Equal(MorseAlphabet.Unreadable, Read("........."));

        var reader = new CwRunReader();
        var text = new List<string>();

        reader.CharacterRead += c => text.Add(c.Text);
        reader.Take(Letter("-").Append(new Symbol(CwSymbolKind.WordEnd) { Reading = Reading() with { Text = MorseAlphabet.WordGap } }).Concat(Letter(".")));

        Assert.Equal("T E", string.Concat(text));
    }
}
