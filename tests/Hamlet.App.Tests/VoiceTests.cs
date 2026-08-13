using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Hamlet.App.Tests;

/// <summary>
/// The voice standard, enforced rather than remembered (HM-DEC-040).
/// </summary>
/// <remarks>
/// <para>A rule that lives only in CLAUDE.md is a rule the next session
/// rediscovers by breaking it. This sweeps the source the way the
/// banned-phrase test sweeps the band character passages: over the files as
/// they sit on disk, so a new string written next month is covered without
/// anybody remembering to add it here.</para>
/// <para>It reads text rather than running the app, which is the trade that
/// makes it possible at all. The alternative is enumerating every string the
/// UI can produce, which is unbounded, and the interesting failures are the
/// ones somebody types into a literal.</para>
/// </remarks>
public sealed class VoiceTests
{
    private const char EmDash = '—';

    /// <summary>
    /// Source files whose user-facing strings the sweep covers.
    /// </summary>
    /// <remarks>
    /// Records, comments and code are outside the rule. CLAUDE.md and
    /// DECISIONS.md are deliberately full of dashes, because they are written
    /// for whoever maintains this rather than for the operator.
    /// </remarks>
    private static IEnumerable<string> CopyFiles()
    {
        var root = RepositoryRoot();

        foreach (var directory in new[] { "src" })
        {
            var path = Path.Combine(root, directory);
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                    || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                var extension = Path.GetExtension(file);
                if (extension is ".cs" or ".axaml")
                {
                    yield return file;
                }
            }
        }
    }

    /// <remarks>
    /// THE RULE (HM-DEC-040): at most one em dash in a passage, and usually
    /// none. A dash is a sentence that has not decided where it ends, and a
    /// second one in the same breath is a sentence that has given up.
    /// </remarks>
    [Fact]
    public void NoPassageOfCopyCarriesTwoEmDashes()
    {
        var offenders = new List<string>();

        foreach (var file in CopyFiles())
        {
            foreach (var passage in CopyIn(file))
            {
                var count = passage.Text.Count(c => c == EmDash);

                if (count > 1)
                {
                    offenders.Add(
                        $"{Path.GetFileName(file)}:{passage.Line} has {count}: {passage.Text}");
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <remarks>
    /// <para>Proves the sweep actually reads something. A file filter that
    /// silently matched nothing would pass every assertion above it forever,
    /// which is the failure mode of every test that walks a directory.</para>
    /// <para>The floor is deliberately well under the real count, so ordinary
    /// churn does not break it and deleting the Explorer would.</para>
    /// </remarks>
    [Fact]
    public void TheSweepIsActuallyReadingTheCopy()
    {
        var files = CopyFiles().ToList();
        var passages = files.SelectMany(CopyIn).ToList();

        Assert.True(files.Count > 20, $"only {files.Count} source files found");
        Assert.True(passages.Count > 200, $"only {passages.Count} passages found");

        // And it can actually see a dash: the band character text is the
        // longest prose in the app and is guaranteed to be in the set.
        Assert.Contains(passages, p => p.Text.Contains("night band", StringComparison.Ordinal));
    }

    /// <remarks>
    /// Proves the em dash count is being read from real strings rather than
    /// from comments. A sweep that skipped the literals and counted only the
    /// doc comments would report zero offenders forever.
    /// </remarks>
    [Fact]
    public void CommentsAreNotMistakenForCopy()
    {
        const string sample = """
            // A comment with — two — dashes in it.
            /// <summary>Another — with — two.</summary>
            var x = "real copy with — one dash";
            """;

        var passages = PassagesFromCSharp(sample).ToList();

        Assert.Single(passages);
        Assert.Contains("real copy", passages[0].Text, StringComparison.Ordinal);
    }

    /// <summary>One run of user-facing text, and where it came from.</summary>
    private readonly record struct Passage(int Line, string Text);

    /// <summary>
    /// The user-facing strings in a file, with adjacent concatenated literals
    /// joined into one passage.
    /// </summary>
    /// <remarks>
    /// Joining matters. Copy in this codebase is written as a run of
    /// concatenated literals across several lines, so counting per line would
    /// miss exactly the case the rule is about: one dash on line 40 and
    /// another on line 42, in the same sentence the operator reads.
    /// </remarks>
    private static IEnumerable<Passage> CopyIn(string file)
    {
        var text = File.ReadAllText(file);

        return Path.GetExtension(file) == ".axaml"
            ? PassagesFromXaml(text)
            : PassagesFromCSharp(text);
    }

    private static IEnumerable<Passage> PassagesFromCSharp(string source)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');

        var buffer = new List<string>();
        var start = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var literals = LiteralsIn(line).ToList();

            if (literals.Count == 0)
            {
                if (buffer.Count > 0)
                {
                    yield return new Passage(start + 1, string.Join(" ", buffer));
                    buffer.Clear();
                }

                continue;
            }

            if (buffer.Count == 0)
            {
                start = i;
            }

            buffer.AddRange(literals);

            // A line that does not continue a concatenation ends the passage.
            if (!line.TrimEnd().EndsWith("+", StringComparison.Ordinal)
                && !NextLineContinues(lines, i))
            {
                yield return new Passage(start + 1, string.Join(" ", buffer));
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            yield return new Passage(start + 1, string.Join(" ", buffer));
        }
    }

    private static bool NextLineContinues(string[] lines, int i)
        => i + 1 < lines.Length
           && lines[i + 1].TrimStart().StartsWith("+ \"", StringComparison.Ordinal);

    /// <summary>
    /// Double-quoted runs on a line, skipping the ones that are plainly not
    /// prose.
    /// </summary>
    /// <remarks>
    /// Identifiers, format strings, hex colors and resource names all live in
    /// double quotes and none of them is copy. Requiring a space and a letter
    /// keeps the sweep on sentences.
    /// </remarks>
    private static IEnumerable<string> LiteralsIn(string line)
    {
        foreach (Match match in Regex.Matches(line, "\"((?:[^\"\\\\]|\\\\.)*)\""))
        {
            var value = match.Groups[1].Value;

            if (value.Length < 12 || !value.Contains(' ') || !value.Any(char.IsLetter))
            {
                continue;
            }

            yield return value;
        }
    }

    private static IEnumerable<Passage> PassagesFromXaml(string source)
    {
        // Strip comments first: XAML comments carry plenty of dashes and none
        // of them reach the screen.
        var stripped = Regex.Replace(source, "<!--.*?-->", " ", RegexOptions.Singleline);
        var lines = stripped.Replace("\r\n", "\n").Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            foreach (Match match in Regex.Matches(
                lines[i], "(?:Text|Title|Content|Header|Watermark|ToolTip\\.Tip)=\"([^\"]*)\""))
            {
                var value = match.Groups[1].Value;

                if (value.Contains('{') || value.Trim().Length == 0)
                {
                    continue;
                }

                yield return new Passage(i + 1, value);
            }
        }
    }

    /// <summary>The repository root, found by walking up from the test binary.</summary>
    private static string RepositoryRoot()
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
