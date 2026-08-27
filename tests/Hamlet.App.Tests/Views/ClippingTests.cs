using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Nothing the operator reads may be cut off (HM-DEC-107 phase 4 of the UI
/// order).
/// </summary>
/// <remarks>
/// <para>**A CLIPPED SENTENCE IS §0.0 BROKEN BY THE LAYOUT.** The operator can
/// only half read it and, worse, cannot tell there was more. The line that got
/// cut was the one saying the decoder was hearing nothing, which is exactly the
/// line somebody stares at when the app looks broken.</para>
/// <para>**THE CLIPPING WAS WIDGET-LEVEL AND NOT THE CANVAS**, which the brief
/// asked to be established before anything was fixed. Each widget's body sits in
/// a scroll viewer that constrains its content to the widget's width, inside a
/// border with a corner radius that clips to it. Anything that cannot give way
/// at that width is cut at the widget's edge. The canvas itself scrolls both
/// ways and clips nothing.</para>
/// <para>So the rule is enforced rather than remembered, in the same shape
/// <c>VoiceTests</c> uses: prose either wraps or trims, and a sweep fails on
/// anything that does neither.</para>
/// </remarks>
public sealed class ClippingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the offenders are printed.</param>
    public ClippingTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// How long a literal string has to be before it counts as prose.
    /// </summary>
    /// <remarks>
    /// Twenty-four characters and a space in it. Shorter than that is a label
    /// like "input" or "Start from", which cannot be cut without the whole
    /// widget being narrower than one word, and wrapping those would only make
    /// the rule noisy enough to be switched off.
    /// </remarks>
    public const int ProseLength = 24;

    private static readonly Regex Block = new(
        @"<(?:Selectable)?TextBlock\b[\s\S]*?(?:/>|</(?:Selectable)?TextBlock>)",
        RegexOptions.Compiled);

    private static readonly Regex Literal = new(
        @"\bText\s*=\s*""([^""{][^""]*)""", RegexOptions.Compiled);

    /// <remarks>
    /// <para>Proves §0.0 and §0.5: **every sentence in the main window either
    /// wraps or is trimmed with an ellipsis, so none can be silently cut.** An
    /// ellipsis is an honest answer, because it says there is more; a sentence
    /// that simply stops at the widget's edge does not.</para>
    /// </remarks>
    [Fact]
    public void NoSentenceInTheMainWindowCanBeSilentlyCut()
    {
        var path = Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(path);
        var offenders = new List<string>();

        foreach (Match match in Block.Matches(xaml))
        {
            var text = match.Value;

            if (text.Contains("TextWrapping", StringComparison.Ordinal)
                || text.Contains("TextTrimming", StringComparison.Ordinal))
            {
                continue;
            }

            if (Literal.Match(text) is not { Success: true } literal)
            {
                // A binding. Whether it carries a word or a paragraph is
                // decided at run time, so it is judged by where it sits rather
                // than by what it says, and that is the sweep below.
                continue;
            }

            var words = literal.Groups[1].Value;

            if (words.Length < ProseLength || !words.Contains(' '))
            {
                continue;
            }

            var line = xaml.Take(match.Index).Count(c => c == '\n') + 1;

            offenders.Add($"line {line}: \"{words}\"");
        }

        foreach (var offender in offenders)
        {
            _output.WriteLine(offender);
        }

        Assert.True(
            offenders.Count == 0,
            $"{offenders.Count} sentences can be cut at a widget's edge with no "
            + $"sign to the operator that there was more:\n"
            + string.Join("\n", offenders));
    }

    /// <remarks>
    /// <para>Proves the ruling's own words: **a horizontal scrollbar was
    /// rejected**, because a summary scrolled off the edge fails §0.5's test in
    /// exactly the way a clipped one does. The widget body says so explicitly
    /// rather than leaving it to a framework default that a version bump could
    /// change under it.</para>

    /// <remarks>
    /// <para>Proves §0.5: **a widget refuses to shrink past a floor**, so reflow
    /// cannot collapse a level bar into two pixels. Below that a widget is not
    /// narrow, it is lost, and finding it again means hunting for a few pixels.
    /// The resize grip already refused to go under this number and the drawn
    /// frame did not, so the two are now the same number read from one
    /// place.</para>

    /// <summary>The repository root, found by walking up from the test binary.</summary>
    private static string Root()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
