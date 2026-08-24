using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Sweeps two senders in one passband across offset and level, and writes the
/// table to the repository root.
/// </summary>
/// <remarks>
/// <para>**THE UNIT'S BEFORE-NUMBER AND ITS AFTER-NUMBER COME FROM THIS SAME
/// CODE.** It is run once before the integrator is touched and once after, so
/// the only thing that differs between the two tables is the thing being
/// judged.</para>
/// <para>**IT ASSERTS NOTHING.** There is no prior figure to assert against and
/// setting one here would be a ratchet on a number nobody has ruled on. What it
/// produces is evidence.</para>
/// </remarks>
public sealed class TheTwoStationTable
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the analysis.</summary>
    /// <param name="output">Where the table is also printed.</param>
    public TheTwoStationTable(ITestOutputHelper output) => _output = output;

    /// <summary>The date this table was taken, which names the file.</summary>
    private const string TakenOn = "2026-08-23";

    /// <remarks>
    /// Writes the table. A measurement run rather than a test of behavior.
    /// </remarks>
    [Fact]
    public void Write()
    {
        var page = new StringBuilder();

        page.AppendLine($"# Two senders in one passband, {TakenOn}");
        page.AppendLine();
        page.AppendLine(
            "Nothing in this repository had measured what the decoder does with two");
        page.AppendLine(
            "stations in one passband. Every fixture held one sender and all nine");
        page.AppendLine(
            "captures were analysed as though one station were present.");
        page.AppendLine();
        page.AppendLine(
            $"The wanted station sends `{CwTwoInOnePassband.WantedText}` at 18 words a");
        page.AppendLine(
            $"minute, {CwTwoInOnePassband.WantedToneHz:0} Hz, 15 dB over a band of noise");
        page.AppendLine(
            "shaped to the receiver's own passband. The competing station sends");
        page.AppendLine(
            $"`{CwTwoInOnePassband.OtherText}` at 24 words a minute, starting a third of a");
        page.AppendLine(
            "second later so its marks land inside the wanted station's rather than");
        page.AppendLine("beside them. Both key throughout.");
        page.AppendLine();
        page.AppendLine(
            $"**Integrator: {CwIntegratorName()}.**");
        page.AppendLine();
        page.AppendLine("Regenerate with:");
        page.AppendLine();
        page.AppendLine("```");
        page.AppendLine(
            "dotnet test tests/Hamlet.RadioEngine.Tests "
            + "--filter FullyQualifiedName~TheTwoStationTable");
        page.AppendLine("```");
        page.AppendLine();

        Control(page);

        Section(
            page,
            "At a fixed pitch, with no tracker",
            "**This is the one a front-end change is judged on.** Nothing moves the "
            + "filter, so the only thing standing between the competing station and "
            + "the envelope is the integrator.",
            CwTwoInOnePassband.Fixed);

        Section(
            page,
            "Through the production path, tracker and all",
            "**This is what the operator would get.** The tracker can walk off to "
            + "the competing station, and where it does the text collapses for a "
            + "reason that has nothing to do with the filter.",
            CwTwoInOnePassband.Tracked);

        var path = Path.Combine(
            RepositoryRoot(), $"ANALYSIS-cw-two-stations-{TakenOn}.md");

        File.WriteAllText(path, page.ToString());

        _output.WriteLine(page.ToString());
        _output.WriteLine($"written to {path}");

        Assert.True(File.Exists(path));
    }

    /// <summary>
    /// The wanted station on its own, read four ways.
    /// </summary>
    /// <remarks>
    /// **WITHOUT THIS SECTION EVERY OTHER NUMBER IN THE FILE IS UNREADABLE.**
    /// Soup on a two-station recording reads as the second station's doing, and
    /// the whole question this unit asks is begged. The control says what the
    /// same decoder does with the same band and no competing station at all, and
    /// splits the production path into its stages so a difference can be
    /// attributed to one of them rather than to the path as a whole.
    /// </remarks>
    private static void Control(StringBuilder page)
    {
        page.AppendLine("## The control: one station, alone");
        page.AppendLine();
        page.AppendLine(
            "Same recipe, same seed, same band, with the competing station left");
        page.AppendLine(
            "out. Read four ways, so a difference can be attributed to a stage");
        page.AppendLine("rather than to the whole path.");
        page.AppendLine();

        var audio = CwTwoInOnePassband.Alone();

        var readings = new (string How, IReadOnlyList<CwCharacter> Characters)[]
        {
            ("whole file, fixed pitch",
                CwTwoInOnePassband.AsCharacters(
                    CwProbabilisticDecoder.Decode(
                        audio, CwTwoInOnePassband.WantedToneHz))),

            ("whole file, forced to 18 wpm",
                CwTwoInOnePassband.AsCharacters(
                    CwProbabilisticDecoder.Decode(
                        CwProbabilisticDecoder.Envelope(
                            audio.Samples,
                            audio.SampleRate,
                            CwTwoInOnePassband.WantedToneHz),
                        CwTwoInOnePassband.WantedToneHz,
                        18.0))),

            ("streaming window, pitch nailed to 600 Hz", Streamed(audio)),

            ("the production path, tracker and all",
                CwDecodeHarness.Decode(
                    audio, CwTwoInOnePassband.WantedToneHz).Characters),
        };

        page.AppendLine(
            "| read how | correct | wrong | invented | emitted | E-share | read |");
        page.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var (how, characters) in readings)
        {
            var reading = CwTwoInOnePassband.ScoreAgainstWanted(characters);

            page.AppendLine(
                $"| {how} | {reading.Correct} | {reading.Wrong} "
                + $"| {reading.Invented} | {reading.Emitted} "
                + $"| {Share(reading.EShare)} | `{Readable(reading.Text)}` |");
        }

        page.AppendLine();

        var (both, wantedAlone, otherAlone) = CwTwoInOnePassband.Overlap(40, 0);

        page.AppendLine(
            "**How hard the two-station fixture actually is**, at 40 Hz and equal");
        page.AppendLine(
            "level, measured through the decoder's own front end pointed at each");
        page.AppendLine(
            $"station in turn: **{Num(both, "0.00")} s with both keys down at once**, ");
        page.AppendLine(
            $"{Num(wantedAlone, "0.00")} s of the wanted station alone and "
            + $"{Num(otherAlone, "0.00")} s of the other alone. A fixture where the");
        page.AppendLine(
            "two never collide proves nothing about rejection and looks exactly");
        page.AppendLine("like one that does (§12.5).");
        page.AppendLine();
        page.AppendLine(
            "**`levelDb` is a ratio of keyed amplitudes, not of averages.** The two");
        page.AppendLine(
            "stations send different text at different speeds, so their key-down");
        page.AppendLine(
            "fractions differ and a whole-recording average of the competing");
        page.AppendLine(
            "station sits about six decibels below the wanted one at a stated level");
        page.AppendLine("of nought.");
        page.AppendLine();
    }

    private static IReadOnlyList<CwCharacter> Streamed(MonoAudio audio)
    {
        var stream = new CwProbabilisticStream(audio.SampleRate)
        {
            ToneHz = CwTwoInOnePassband.WantedToneHz,
        };

        var settled = new List<CwCharacter>();

        stream.CharacterSettled += settled.Add;
        stream.Process(audio.Samples);
        stream.Flush();

        return settled;
    }

    private static void Section(
        StringBuilder page,
        string heading,
        string blurb,
        Func<double, double, CwTwoStationReading> read)
    {
        page.AppendLine($"## {heading}");
        page.AppendLine();
        page.AppendLine(blurb);
        page.AppendLine();
        page.AppendLine(
            "`correct` counts characters read as sent, of nine. `invented` counts "
            + "characters read where nothing was sent at all, which is "
            + "`CwMatchKind.Invented` and not the `Wrong` the sensitivity sweep "
            + "prints under that name.");
        page.AppendLine();
        page.AppendLine(
            "| offset | level | correct | wrong | invented | emitted | E-share | read |");
        page.AppendLine("|---|---|---|---|---|---|---|---|");

        foreach (var offset in CwTwoInOnePassband.Offsets)
        {
            foreach (var level in CwTwoInOnePassband.Levels)
            {
                var reading = read(offset, level);

                page.AppendLine(
                    $"| {Num(offset, "0")} Hz | {Num(level, "+0;-0")} dB "
                    + $"| {reading.Correct} | {reading.Wrong} | {reading.Invented} "
                    + $"| {reading.Emitted} | {Share(reading.EShare)} "
                    + $"| `{Readable(reading.Text)}` |");
            }
        }

        page.AppendLine();
    }

    /// <summary>What shape the integrator is, for the table's own heading.</summary>
    /// <remarks>
    /// Read from the engine rather than typed, so a table cannot claim to have
    /// been taken through a filter it was not taken through.
    /// </remarks>
    private static string CwIntegratorName()
        => Hamlet.RadioEngine.Cw.CwProbabilisticDecoder.IntegratorName;

    private static string Num(double value, string format)
        => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Share(double value)
        => double.IsNaN(value)
            ? "no characters"
            : value.ToString("P0", CultureInfo.InvariantCulture);

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
