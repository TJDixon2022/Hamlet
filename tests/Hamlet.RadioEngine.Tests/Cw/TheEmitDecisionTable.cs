using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What the corpus reads once the emit decision belongs to the character, with
/// the tracker steering and with the pitch held.
/// </summary>
/// <remarks>
/// <para>**THE TWO COLUMNS ARE THE UNIT.** One is what the operator gets today.
/// The other is what he gets with the tracker taken out of the path, which is
/// the thing the lock exists to let him do, and the gap between them is the
/// tracker's cost measured on real audio rather than on a generated
/// fixture.</para>
/// <para>**IT ASSERTS ONLY THE SILENCE PROPERTY.** Everything else is a
/// measurement, because there is no prior figure to assert against and setting
/// one here would be a ratchet on a number nobody has ruled (HM-DEC-120 is the
/// exception and it is checked).</para>
/// </remarks>
public sealed class TheEmitDecisionTable
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the analysis.</summary>
    /// <param name="output">Where the table is also printed.</param>
    public TheEmitDecisionTable(ITestOutputHelper output) => _output = output;

    /// <summary>The date this table was taken, which names the file.</summary>
    private const string TakenOn = "2026-08-24";

    /// <summary>Every capture, with where the station sits and what it holds.</summary>
    private static readonly (string Name, double Tone, string Holds)[] Corpus =
    {
        ("cw-2026-08-17-013347", 600, "VA3VRR (HM-DEC-145)"),
        ("cw-2026-08-17-013622", 600, "unadjudicated"),
        ("cw-2026-08-17-134712", 600, "N4L (HM-DEC-144)"),
        ("cw-2026-08-18-004507", 501, "an ARRL bulletin"),
        ("unadjudicated/cw-2026-08-18-003016", 669, "unadjudicated"),
        ("unadjudicated/cw-2026-08-18-003126", 675, "unadjudicated"),
        ("unadjudicated/cw-2026-08-18-003758", 501, "AA4MP/4 QNIK (HM-DEC-126)"),
        ("unadjudicated/cw-2026-08-20-014854", 600, "nothing"),
        ("unadjudicated/cw-2026-08-20-014935", 825, "nothing"),
    };

    /// <remarks>
    /// Writes the table and checks the one property that is not negotiable.
    /// </remarks>
    [Fact]
    public void Write()
    {
        var page = new StringBuilder();

        page.AppendLine($"# What the corpus reads, {TakenOn}");
        page.AppendLine();
        page.AppendLine(
            "The emit decision now belongs to the character rather than to the");
        page.AppendLine(
            "window. The window ratio survives as an outer silence guard at its");
        page.AppendLine(
            $"existing value of {Num(CwProbabilisticDecoder.Gate, "0")}, and each");
        page.AppendLine(
            "character must additionally carry more evidence than the key never");
        page.AppendLine(
            "having gone down across its own span. That margin is");
        page.AppendLine(
            $"**{Num(CwProbabilisticDecoder.CharacterMargin, "0")}** — the point");
        page.AppendLine(
            "where the two explanations are equally good, rather than a place on");
        page.AppendLine("the scale that had to be chosen.");
        page.AppendLine();
        page.AppendLine(
            "`■` counts characters that were heard and could not be resolved. They");
        page.AppendLine(
            "are marked rather than removed, so the count of characters does not");
        page.AppendLine("change when the judgement does.");
        page.AppendLine();
        page.AppendLine("Regenerate with:");
        page.AppendLine();
        page.AppendLine("```");
        page.AppendLine(
            "dotnet test tests/Hamlet.RadioEngine.Tests "
            + "--filter FullyQualifiedName~TheEmitDecisionTable");
        page.AppendLine("```");
        page.AppendLine();

        page.AppendLine("## Through the production path, tracker steering");
        page.AppendLine();
        page.AppendLine(
            "| capture | holds | window | emitted | ■ | read |");
        page.AppendLine("|---|---|---|---|---|---|");

        var silent = new List<string>();

        foreach (var (name, tone, holds) in Corpus)
        {
            var row = Tracked(name, tone);

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` | {holds} "
                + $"| {Num(row.Window, "0.0")} | {row.Emitted} | {row.Blocks} "
                + $"| `{Readable(row.Text)}` |");

            if (holds == "nothing" && row.Emitted > 0)
            {
                silent.Add(name);
            }
        }

        page.AppendLine();
        page.AppendLine("## With the pitch held at the measured peak");
        page.AppendLine();
        page.AppendLine(
            "The lock engaged after eight seconds, at whatever the interpolated");
        page.AppendLine(
            "peak said then, and the tracker stopped steering from that moment.");
        page.AppendLine();
        page.AppendLine(
            "| capture | locked to | emitted | ■ | read |");
        page.AppendLine("|---|---|---|---|---|");

        foreach (var (name, tone, _) in Corpus)
        {
            var row = Locked(name, tone);

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` "
                + $"| {(double.IsNaN(row.LockedAt) ? "refused" : Num(row.LockedAt, "0.0") + " Hz")} "
                + $"| {row.Emitted} | {row.Blocks} | `{Readable(row.Text)}` |");

            if (Corpus.First(c => c.Name == name).Holds == "nothing"
                && row.Emitted > 0)
            {
                silent.Add(name + " (locked)");
            }
        }

        page.AppendLine();

        var path = Path.Combine(
            RepositoryRoot(), $"ANALYSIS-cw-emit-decision-{TakenOn}.md");

        File.WriteAllText(path, page.ToString());

        _output.WriteLine(page.ToString());

        // **THE ONE PROPERTY THAT IS NOT A MEASUREMENT** (HM-DEC-120). Both
        // captures holding no station emit nothing, on both paths.
        Assert.True(
            silent.Count == 0,
            "audio holding no station emitted characters: "
            + string.Join(", ", silent));
    }

    private static (double Window, int Emitted, int Blocks, string Text) Tracked(
        string name, double tone)
    {
        var audio = Read(name);
        var decoder = new CwDecoder(audio.SampleRate, tone);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        Pump(decoder, audio, lockAfterSeconds: double.NaN);

        return Score(decoder, settled);
    }

    private static (double LockedAt, int Emitted, int Blocks, string Text) Locked(
        string name, double tone)
    {
        var audio = Read(name);
        var decoder = new CwDecoder(audio.SampleRate, tone);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        Pump(decoder, audio, lockAfterSeconds: 8);

        var scored = Score(decoder, settled);

        return (decoder.LockedToneHz, scored.Emitted, scored.Blocks, scored.Text);
    }

    private static void Pump(CwDecoder decoder, MonoAudio audio, double lockAfterSeconds)
    {
        var hop = decoder.Tracker.HopSamples;
        var lockAt = double.IsNaN(lockAfterSeconds)
            ? long.MaxValue
            : (long)(lockAfterSeconds * audio.SampleRate);

        var locked = false;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (!locked && at >= lockAt)
            {
                decoder.Lock();
                locked = true;
            }
        }

        decoder.Flush();
    }

    private static (double Window, int Emitted, int Blocks, string Text) Score(
        CwDecoder decoder, IReadOnlyList<CwCharacter> settled)
    {
        var letters = settled.Where(c => !c.IsWordGap).ToList();

        return (
            decoder.Stream.Last.LikelihoodRatio,
            letters.Count,
            letters.Count(c => c.IsUnreadable),
            string.Concat(settled.Select(c => c.Text)));
    }

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
