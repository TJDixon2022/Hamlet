using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// What each integrator width buys and what it costs, measured rather than
/// argued.
/// </summary>
/// <remarks>
/// <para>**A NARROWER FILTER REJECTS MORE AND RESPONDS MORE SLOWLY, AND BOTH
/// HALVES HAVE TO BE ON ONE PAGE OR THE CHOICE IS BEING MADE ON ONE OF
/// THEM.** At thirty words a minute a dit is forty milliseconds and a twenty
/// hertz Hann spans seventy-five, so the filter is longer than the element it is
/// trying to resolve.</para>
/// <para>**EVERYTHING HERE IS READ AT A FIXED PITCH.** The tracker is out of the
/// path on purpose: it is measurably the largest source of error in this decoder
/// and it would swamp a filter's contribution entirely. What this table measures
/// is the filter.</para>
/// <para>**AND THE GATE'S OWN MARGIN IS A COST COLUMN**, because narrowing the
/// filter was measured to move it. A width that reads better and leaves the
/// empty band sitting on the gate has spent the one property that has never been
/// traded (HM-DEC-120).</para>
/// </remarks>
public sealed class TheIntegratorBandwidthTable
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the analysis.</summary>
    /// <param name="output">Where the table is also printed.</param>
    public TheIntegratorBandwidthTable(ITestOutputHelper output)
        => _output = output;

    /// <summary>The date this table was taken, which names the file.</summary>
    private const string TakenOn = "2026-08-23";

    /// <summary>The widths swept.</summary>
    private static readonly double[] Widths = { 60, 45, 30, 20 };

    /// <remarks>
    /// Writes the table. A measurement run rather than a test of behavior.
    /// </remarks>
    [Fact]
    public void Write()
    {
        var page = new StringBuilder();

        page.AppendLine($"# What each integrator width is worth, {TakenOn}");
        page.AppendLine();
        page.AppendLine(
            "Every figure is read at a fixed pitch, with the tone tracker out of");
        page.AppendLine(
            "the path. The tracker is measurably the largest source of error in");
        page.AppendLine(
            "this decoder and it would swamp a filter's contribution entirely; what");
        page.AppendLine("this table measures is the filter.");
        page.AppendLine();
        page.AppendLine(
            $"The production default is **{Num(CwProbabilisticDecoder.IntegratorBandwidthHz, "0")} Hz**");
        page.AppendLine(
            $"({CwProbabilisticDecoder.IntegratorName}). It is a constant and this");
        page.AppendLine(
            "sweep does not move it: a mutable static the whole suite shares is a");
        page.AppendLine(
            "way for one test to change another test's numbers without either");
        page.AppendLine("saying so.");
        page.AppendLine();
        page.AppendLine("Regenerate with:");
        page.AppendLine();
        page.AppendLine("```");
        page.AppendLine(
            "dotnet test tests/Hamlet.RadioEngine.Tests "
            + "--filter FullyQualifiedName~TheIntegratorBandwidthTable");
        page.AppendLine("```");
        page.AppendLine();

        Shape(page);
        Rejection(page);
        Sensitivity(page);
        FastFist(page);
        GateMargin(page);
        Corpus(page);
        Choice(page);

        var path = Path.Combine(
            RepositoryRoot(), $"ANALYSIS-cw-integrator-bandwidth-{TakenOn}.md");

        File.WriteAllText(path, page.ToString());

        _output.WriteLine(page.ToString());
        _output.WriteLine($"written to {path}");

        Assert.True(File.Exists(path));
    }

    private static void Shape(StringBuilder page)
    {
        page.AppendLine("## What each width is, in samples and in time");
        page.AppendLine();
        page.AppendLine(
            "At 48 kHz. The length is what costs: an integrator longer than a dit");
        page.AppendLine("rounds the top of every short mark.");
        page.AppendLine();
        page.AppendLine("| width | samples | spans | dit at 18 wpm | dit at 30 wpm |");
        page.AppendLine("|---|---|---|---|---|");

        foreach (var width in Widths)
        {
            var n = CwProbabilisticDecoder.IntegratorWindow(48_000, width);
            var ms = n * 1000.0 / 48_000;

            page.AppendLine(
                $"| {Num(width, "0")} Hz | {n} | {Num(ms, "0.0")} ms "
                + $"| {Num(ms / (1200.0 / 18) * 100, "0")} % of it "
                + $"| {Num(ms / (1200.0 / 30) * 100, "0")} % of it |");
        }

        page.AppendLine();
    }

    private static void Rejection(StringBuilder page)
    {
        page.AppendLine("## Rejection: two senders in one passband");
        page.AppendLine();
        page.AppendLine(
            "The wanted station's eleven characters, read against a competing");
        page.AppendLine(
            "station at each offset and level. **The ordered grid saturates**: every");
        page.AppendLine(
            "width reads the message whole at every offset from 40 Hz out and every");
        page.AppendLine(
            "level down from equal, so it discriminates nothing. Harder rows are");
        page.AppendLine(
            "added below it — closer, and louder than the wanted station — because a");
        page.AppendLine(
            "table where every cell is perfect measures nothing about the filter.");
        page.AppendLine();

        var grid = CwTwoInOnePassband.Offsets
            .SelectMany(o => CwTwoInOnePassband.Levels.Select(l => (Offset: o, Level: l)))
            .ToList();

        var harder = new (double Offset, double Level)[]
        {
            (30, 0), (30, 6), (20, 0), (20, 6), (15, 0), (10, 0),
        };

        page.AppendLine(
            "| offset | level | " + string.Join(" | ", Widths.Select(w => $"{w:0} Hz"))
            + " |");
        page.AppendLine("|---|---|" + string.Concat(Widths.Select(_ => "---|")));

        foreach (var (offset, level) in grid.Concat(harder))
        {
            var cells = Widths.Select(w =>
            {
                var r = CwTwoInOnePassband.Fixed(offset, level, w);

                return $"{r.Correct}/11, {r.Invented} made up";
            });

            page.AppendLine(
                $"| {Num(offset, "0")} Hz | {Num(level, "+0;-0")} dB | "
                + string.Join(" | ", cells) + " |");
        }

        page.AppendLine();
    }

    private static void Sensitivity(StringBuilder page)
    {
        page.AppendLine("## The cost in sensitivity");
        page.AppendLine();
        page.AppendLine(
            $"`{CwSensitivity.Message}` at 18 words a minute, one seed per level,");
        page.AppendLine(
            "read at a fixed pitch so the figures are about the filter. `invented`");
        page.AppendLine("is `CwMatchKind.Invented`.");
        page.AppendLine();
        page.AppendLine(
            "| generated | " + string.Join(" | ", Widths.Select(w => $"{w:0} Hz"))
            + " |");
        page.AppendLine("|---|" + string.Concat(Widths.Select(_ => "---|")));

        foreach (var db in new[] { 18.0, 11.0, 3.0, 0.0 })
        {
            var audio = CwSignal.Generate(new CwSignalRequest(
                CwSensitivity.Message,
                WordsPerMinute: CwSensitivity.WordsPerMinute,
                ToneHz: CwSensitivity.ToneHz,
                Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(db),
                Seed: 7919));

            page.AppendLine(
                $"| {Num(db, "0")} dB | "
                + string.Join(
                    " | ",
                    Widths.Select(w => Cell(audio, CwSensitivity.ToneHz, w,
                        CwSensitivity.Message)))
                + " |");
        }

        page.AppendLine();
    }

    private static void FastFist(StringBuilder page)
    {
        page.AppendLine("## The cost to a fast fist");
        page.AppendLine();
        page.AppendLine(
            "A narrower filter responds more slowly, and at 30 words a minute a dit");
        page.AppendLine(
            "is 40 ms. This is the column that decides the trade, because the");
        page.AppendLine("rejection column has nothing left to buy.");
        page.AppendLine();
        page.AppendLine(
            "| speed | " + string.Join(" | ", Widths.Select(w => $"{w:0} Hz")) + " |");
        page.AppendLine("|---|" + string.Concat(Widths.Select(_ => "---|")));

        foreach (var wpm in new[] { 18, 25, 30, 35 })
        {
            var audio = CwSignal.Generate(new CwSignalRequest(
                CwSensitivity.Message,
                WordsPerMinute: wpm,
                ToneHz: CwSensitivity.ToneHz,
                Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(18),
                Seed: 7919));

            page.AppendLine(
                $"| {wpm} wpm | "
                + string.Join(
                    " | ",
                    Widths.Select(w => Cell(audio, CwSensitivity.ToneHz, w,
                        CwSensitivity.Message)))
                + " |");
        }

        page.AppendLine();
    }

    private static void GateMargin(StringBuilder page)
    {
        page.AppendLine("## The cost to the gate's own margin");
        page.AppendLine();
        page.AppendLine(
            "`Gate = 15` sits in the space between what an empty band scores and");
        page.AppendLine(
            "what a station scores. **Narrowing the filter was measured to move");
        page.AppendLine(
            "it**, so the margin is a cost column: a width that reads better and");
        page.AppendLine(
            "leaves the empty band sitting on the gate has spent the one property");
        page.AppendLine("that has never been traded (HM-DEC-120).");
        page.AppendLine();
        page.AppendLine(
            "| recording | holds | " + string.Join(" | ", Widths.Select(w => $"{w:0} Hz"))
            + " |");
        page.AppendLine("|---|---|" + string.Concat(Widths.Select(_ => "---|")));

        var folder = CapturedSignalTests.Folder;

        var rows = new (string Name, string Holds, double Tone)[]
        {
            ("unadjudicated/cw-2026-08-20-014854", "nothing", 600),
            ("unadjudicated/cw-2026-08-20-014935", "nothing", 825),
            ("cw-2026-08-18-004507", "a station", 501),
            ("cw-2026-08-17-013347", "a station", 600),
        };

        foreach (var (name, holds, tone) in rows)
        {
            var audio = WavAudio.Read(Path.Combine(folder, name + ".wav"));

            var cells = Widths.Select(w =>
            {
                var r = CwProbabilisticDecoder.Decode(
                    CwProbabilisticDecoder.Envelope(
                        audio.Samples, audio.SampleRate, tone, w),
                    tone);

                return Num(r.LikelihoodRatio, "0.0");
            });

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` | {holds} | "
                + string.Join(" | ", cells) + " |");
        }

        page.AppendLine();
        page.AppendLine(
            $"The gate is **{Num(CwProbabilisticDecoder.Gate, "0")}**. A row holding");
        page.AppendLine(
            "nothing must stay well under it and a row holding a station well over.");
        page.AppendLine();
    }

    private static void Corpus(StringBuilder page)
    {
        page.AppendLine("## The corpus");
        page.AppendLine();
        page.AppendLine(
            "Characters emitted and E-share on the real captures, read at a fixed");
        page.AppendLine(
            "pitch. No answer key exists for most of these, so what is shown is how");
        page.AppendLine("much comes out and how much of it is the letter `E`.");
        page.AppendLine();
        page.AppendLine(
            "| recording | " + string.Join(" | ", Widths.Select(w => $"{w:0} Hz")) + " |");
        page.AppendLine("|---|" + string.Concat(Widths.Select(_ => "---|")));

        var folder = CapturedSignalTests.Folder;

        var rows = new (string Name, double Tone)[]
        {
            ("cw-2026-08-17-013347", 600),
            ("cw-2026-08-17-134712", 600),
            ("cw-2026-08-18-004507", 501),
            ("unadjudicated/cw-2026-08-18-003758", 501),
        };

        foreach (var (name, tone) in rows)
        {
            var audio = WavAudio.Read(Path.Combine(folder, name + ".wav"));

            var cells = Widths.Select(w =>
            {
                var r = CwProbabilisticDecoder.Decode(
                    CwProbabilisticDecoder.Envelope(
                        audio.Samples, audio.SampleRate, tone, w),
                    tone);

                var letters = r.Characters
                    .Where(c => !string.Equals(c.Text, " ", StringComparison.Ordinal))
                    .ToList();

                var eShare = letters.Count == 0
                    ? 0
                    : (double)letters.Count(
                        c => string.Equals(c.Text, "E", StringComparison.Ordinal))
                      / letters.Count;

                return $"{letters.Count} chars, E {Num(eShare * 100, "0")} %";
            });

            page.AppendLine(
                $"| `{Path.GetFileName(name)}` | " + string.Join(" | ", cells) + " |");
        }

        page.AppendLine();
    }

    private static void Choice(StringBuilder page)
    {
        page.AppendLine("## What was chosen, and why");
        page.AppendLine();
        page.AppendLine(
            "**Forty-five hertz, which is where matching the boxcar's own main lobe");
        page.AppendLine("lands.**");
        page.AppendLine();
        page.AppendLine(
            "**On the grid this unit was asked to sweep, nothing discriminates.**");
        page.AppendLine(
            "Every width reads the wanted station whole at every offset from 40 Hz");
        page.AppendLine(
            "out and every level down from equal. The ordered measurement returns a");
        page.AppendLine("tie, and a tie is an answer.");
        page.AppendLine();
        page.AppendLine(
            "**The rows that do discriminate were added here, and that is exactly");
        page.AppendLine(
            "why they did not decide it.** Below about 30 Hz of separation the");
        page.AppendLine(
            "narrower filters win outright, and 30 Hz would buy the 30-and-20-hertz");
        page.AppendLine(
            "cases at no measured cost to a fast fist at all. But those rows are");
        page.AppendLine(
            "this session's invention, no ruling sanctions them, and fitting a");
        page.AppendLine(
            "production constant to a fixture the same session wrote is the shape of");
        page.AppendLine("the failure §12.5 exists to stop.");
        page.AppendLine();
        page.AppendLine("**What narrowing costs, measured:**");
        page.AppendLine();
        page.AppendLine(
            "- **the gate's margin, which is the binding one.** The empty band on");
        page.AppendLine(
            "  `cw-2026-08-20-014854` climbs 6.6, 8.0, 9.3, 10.0 against a gate of");
        page.AppendLine(
            "  15. Silence holds at every width, so HM-DEC-120's property is not");
        page.AppendLine(
            "  traded, but the room under the gate goes from 8.4 to 5.0.");
        page.AppendLine(
            "- **the corpus.** `cw-2026-08-17-013347` reads 82, 83, 79 and 49");
        page.AppendLine(
            "  characters as the filter narrows, and its E-share rises from 45 % to");
        page.AppendLine("  53 %. Twenty hertz is plainly worse there.");
        page.AppendLine(
            "- **sensitivity: nothing measured**, down to 0 dB at every width.");
        page.AppendLine(
            "- **a fast fist: nothing measured**, up to 35 words a minute at every");
        page.AppendLine(
            "  width, including a 75 ms integrator on a 34 ms dit. The segmental");
        page.AppendLine(
            "  decoder scores a span rather than thresholding a level, so a smeared");
        page.AppendLine(
            "  envelope loses contrast and keeps its timing. That is a real property");
        page.AppendLine("  of this architecture and it was not assumed.");
        page.AppendLine();
        page.AppendLine(
            "**Thirty hertz is the live alternative and the choice between them is a");
        page.AppendLine(
            "trade rather than a deduction.** It buys close-in rejection that 45 Hz");
        page.AppendLine(
            "does not have, for 1.3 dB of gate margin and four characters on one");
        page.AppendLine(
            "capture. A trade is not a session's to make (§12.1), so it is named");
        page.AppendLine("here and handed back.");
        page.AppendLine();
    }

    private static string Cell(
        MonoAudio audio, double toneHz, double width, string sent)
    {
        var result = CwProbabilisticDecoder.Decode(
            CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, toneHz, width),
            toneHz);

        var characters = CwTwoInOnePassband.AsCharacters(result);
        var matches = CwAlignment.Align(characters, sent);
        var expected = CwAlignment.SymbolCount(sent);

        var correct = matches.Count(
            m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap);

        var invented = matches.Count(
            m => m.Kind == CwMatchKind.Invented && !m.Decoded.IsWordGap);

        return $"{correct}/{expected}, {invented} made up";
    }

    private static string Num(double value, string format)
        => value.ToString(format, CultureInfo.InvariantCulture);

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
