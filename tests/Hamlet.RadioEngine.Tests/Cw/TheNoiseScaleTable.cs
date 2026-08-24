using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the corrected noise scale does, and whether the outer guard can go.
/// </summary>
/// <remarks>
/// <para>**THE GUARD IS EXPRESSED IN UNITS THIS UNIT CHANGED.** Correcting the
/// Rayleigh scale deflates every window ratio by roughly the factor the old
/// scale inflated it, so a bar of fifteen means something entirely different
/// after than before. The unit is forbidden to tune it or remove it, so what it
/// can do is measure what a margin would have to be and hand the decision
/// back.</para>
/// <para>**THE UNGATED READS ARE THE ONLY WAY TO SEE THE EMPTY BANDS AT ALL.**
/// The guard refuses every window either empty capture produces, so the corpus
/// has never yielded a single noise-minted character to compare a margin
/// against. Bypassing it is the only way to ask what those characters would
/// score.</para>
/// </remarks>
public sealed class TheNoiseScaleTable
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the analysis.</summary>
    /// <param name="output">Where the table is also printed.</param>
    public TheNoiseScaleTable(ITestOutputHelper output) => _output = output;

    /// <summary>The date this table was taken, which names the file.</summary>
    private const string TakenOn = "2026-08-24";

    /// <summary>Every capture, where the station sits, and what it holds.</summary>
    private static readonly (string Name, double Tone, string Call)[] Corpus =
    {
        ("cw-2026-08-17-013347", 600, "VA3VRR"),
        ("cw-2026-08-17-013622", 600, ""),
        ("cw-2026-08-17-134712", 600, "N4L"),
        ("cw-2026-08-18-004507", 501, ""),
        ("unadjudicated/cw-2026-08-18-003016", 669, ""),
        ("unadjudicated/cw-2026-08-18-003126", 675, ""),
        ("unadjudicated/cw-2026-08-18-003758", 501, "AA4MP/4QNIK"),
        ("unadjudicated/cw-2026-08-24-012403", 439.81, "KD0UNKD0UNK"),
        ("unadjudicated/cw-2026-08-22-031905", 499.9, ""),
        ("unadjudicated/cw-2026-08-23-001520", 600, ""),
        ("unadjudicated/cw-2026-08-20-014854", 600, "EMPTY"),
        ("unadjudicated/cw-2026-08-20-014935", 825, "EMPTY"),
    };

    /// <remarks>
    /// Writes the table. The silence property is the one thing asserted.
    /// </remarks>
    [Fact]
    public void Write()
    {
        var page = new StringBuilder();

        page.AppendLine($"# The corrected noise scale, {TakenOn}");
        page.AppendLine();
        page.AppendLine(
            "The Rayleigh scale is now taken from the quarter point by identity,");
        page.AppendLine(
            $"`P25 / {Num(CwProbabilisticDecoder.RayleighQuarterPoint, "0.000")}`,");
        page.AppendLine(
            "rather than by a factor of six tenths that made it 0.455 sigma. Key-up");
        page.AppendLine(
            "is a proper Rayleigh density, so the noise hypothesis stays");
        page.AppendLine(
            "competitive in the upper tail where noise actually lives. Both are");
        page.AppendLine(
            $"estimated over a rolling {Num(CwProbabilisticDecoder.NoiseSpanSeconds, "0.0")} s");
        page.AppendLine("span on both paths rather than once per recording.");
        page.AppendLine();
        page.AppendLine("Regenerate with:");
        page.AppendLine();
        page.AppendLine("```");
        page.AppendLine(
            "dotnet test tests/Hamlet.RadioEngine.Tests "
            + "--filter FullyQualifiedName~TheNoiseScaleTable");
        page.AppendLine("```");
        page.AppendLine();

        var problems = Ungated(page);

        Spans(page);

        var path = Path.Combine(
            RepositoryRoot(), $"ANALYSIS-cw-noise-scale-{TakenOn}.md");

        File.WriteAllText(path, page.ToString());

        _output.WriteLine(page.ToString());

        // **THE ONE PROPERTY THAT IS NOT A MEASUREMENT** (HM-DEC-120): both
        // captures holding no station emit nothing through the production path.
        // Everything else in this file is evidence, including the losses.
        foreach (var (name, tone, call) in Corpus.Where(c => c.Call == "EMPTY"))
        {
            var audio = Read(name);
            var r = CwProbabilisticDecoder.Decode(audio, tone);

            Assert.True(
                r.Characters.Count == 0,
                $"{name} emitted {r.Characters.Count} characters.");
        }

        _output.WriteLine(
            problems.Count == 0
                ? "no callsign was lost"
                : "LOST: " + string.Join("; ", problems));
    }

    /// <summary>
    /// Every capture read with the outer guard bypassed, and where the margins
    /// land.
    /// </summary>
    private static List<string> Ungated(StringBuilder page)
    {
        page.AppendLine("## With the outer guard bypassed");
        page.AppendLine();
        page.AppendLine(
            "Whole-file reads, so the empty captures produce characters that can");
        page.AppendLine(
            "be measured at all. `window` is what the guard would have seen.");
        page.AppendLine();
        page.AppendLine(
            "| capture | window | chars | margins: min / median / max | read |");
        page.AppendLine("|---|---|---|---|---|");

        var problems = new List<string>();

        double emptyBest = double.NegativeInfinity;
        var callWorst = new List<(string Call, double Margin)>();

        foreach (var (name, tone, call) in Corpus)
        {
            var audio = Read(name);

            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            var r = CwProbabilisticDecoder.DecodeForMeasurement(
                env, tone, ungated: true, CwProbabilisticDecoder.NoiseSpanSeconds);

            var letters = r.Characters.Where(c => c.Pattern.Length > 0).ToList();

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` | {Num(r.LikelihoodRatio, "0.00")} "
                + $"| {letters.Count} | {Spread(letters)} | `{Readable(Text(r))}` |");

            if (call == "EMPTY")
            {
                foreach (var c in letters)
                {
                    emptyBest = Math.Max(emptyBest, c.SpanMargin);
                }
            }
            else if (call.Length > 0)
            {
                var text = string.Concat(letters.Select(c => c.Text));
                var at = text.IndexOf(call, StringComparison.Ordinal);

                callWorst.Add((
                    call,
                    at < 0
                        ? double.NaN
                        : letters.Skip(at).Take(call.Length).Min(c => c.SpanMargin)));
            }
        }

        page.AppendLine();
        page.AppendLine("## Can the guard go?");
        page.AppendLine();
        page.AppendLine(
            "The question is whether a character margin exists that silences both");
        page.AppendLine(
            "empty captures and keeps all three adjudicated callsigns, with the");
        page.AppendLine("outer guard gone.");
        page.AppendLine();
        page.AppendLine("| | margin |");
        page.AppendLine("|---|---|");
        page.AppendLine(
            $"| the best character either empty capture produces | "
            + $"**{Num(emptyBest, "0.00")}** |");

        foreach (var (call, margin) in callWorst)
        {
            page.AppendLine(
                $"| the weakest character of `{call}` | "
                + (double.IsNaN(margin)
                    ? "**not read at all**"
                    : Num(margin, "0.00"))
                + " |");
        }

        page.AppendLine();

        var missing = callWorst.Where(c => double.IsNaN(c.Margin)).ToList();
        var found = callWorst.Where(c => !double.IsNaN(c.Margin)).ToList();
        var callFloor = found.Count == 0
            ? double.NegativeInfinity
            : found.Min(c => c.Margin);

        if (found.Count > 0 && callFloor > emptyBest)
        {
            page.AppendLine(
                $"**For the callsigns that are read at all, yes.** The gap is "
                + $"{Num(emptyBest, "0.00")} to {Num(callFloor, "0.00")}, and any");
            page.AppendLine(
                "margin inside it silences both empty captures while keeping every");
            page.AppendLine("one of them.");
        }
        else
        {
            page.AppendLine(
                $"**No.** An empty capture produces a character scoring "
                + $"{Num(emptyBest, "0.00")}, at or above the weakest character of");
            page.AppendLine(
                string.Join(
                    " and ",
                    found.Where(c => c.Margin <= emptyBest).Select(c => $"`{c.Call}`"))
                + ", so a margin that silences the noise cuts the callsign.");
        }

        if (missing.Count > 0)
        {
            page.AppendLine();
            page.AppendLine(
                "**But the question is smaller than it looks, and that is the more");
            page.AppendLine(
                "important half.** "
                + string.Join(" and ", missing.Select(c => $"`{c.Call}`"))
                + " are not read at all on this path, so no margin can keep them:");
            page.AppendLine(
                "they are already gone before any character is judged. A margin");
            page.AppendLine(
                "chosen from the callsigns that survive would be chosen from a");
            page.AppendLine("corpus that has quietly shrunk.");
        }

        page.AppendLine();

        return problems;
    }

    /// <summary>How much every figure moves when the estimation span moves.</summary>
    private static void Spans(StringBuilder page)
    {
        page.AppendLine("## How much the span matters");
        page.AppendLine();
        page.AppendLine(
            "The same reads at one and a half, two and a half and four seconds, so");
        page.AppendLine(
            "the provisional span arrives with its own sensitivity measured.");
        page.AppendLine();
        page.AppendLine("| capture | 1.5 s | 2.5 s | 4.0 s |");
        page.AppendLine("|---|---|---|---|");

        foreach (var (name, tone, call) in Corpus)
        {
            var audio = Read(name);

            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            var cells = new[] { 1.5, 2.5, 4.0 }.Select(span =>
            {
                var r = CwProbabilisticDecoder.DecodeForMeasurement(
                    env, tone, ungated: true, span);

                var letters = r.Characters.Where(c => c.Pattern.Length > 0).ToList();

                var kept = call.Length > 0 && call != "EMPTY"
                    && string.Concat(letters.Select(c => c.Text))
                        .Contains(call, StringComparison.Ordinal)
                    ? $", {call} kept"
                    : call.Length > 0 && call != "EMPTY" ? $", {call} LOST" : "";

                return $"{Num(r.LikelihoodRatio, "0.0")}, {letters.Count} chars{kept}";
            });

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` | " + string.Join(" | ", cells) + " |");
        }

        page.AppendLine();
    }

    private static string Spread(IReadOnlyList<CwProbabilisticCharacter> letters)
    {
        if (letters.Count == 0)
        {
            return "none";
        }

        var m = letters.Select(c => c.SpanMargin).OrderBy(x => x).ToArray();

        return $"{Num(m[0], "0.00")} / {Num(m[m.Length / 2], "0.00")} / "
            + Num(m[^1], "0.00");
    }

    private static string Text(CwProbabilisticResult r)
        => string.Concat(r.Characters.Select(c => c.Text));

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static string Num(double value, string format)
        => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Readable(string text)
        => text.Length == 0
            ? "(nothing)"
            : text.Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal)
                .Replace("|", "/", StringComparison.Ordinal);

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(
                Path.Combine(directory.FullName, "src", "Hamlet.RadioEngine")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
