using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Produces the corpus baseline table this phase is measured against, and writes
/// it to the repository root.
/// </summary>
/// <remarks>
/// <para>**THE CORPUS HAS NEVER BEEN MEASURED WITH A PER-CHARACTER INSTRUMENT
/// ATTACHED.** The figures the phase was scoped from were counted over
/// transcript text after the fact, and the sidecar's own margin figures were
/// shown to be fiction, so nothing written per character could say whether that
/// character was read from a signal or minted from noise. Every number this
/// writes is measured by code committed in the same session, and none is copied
/// forward from a review or a brief.</para>
/// <para>**IT IS A MEASUREMENT AND NOT A RATCHET.** Nothing here asserts a
/// figure, because there is no prior figure to assert against: that is the
/// point. Later units are judged by how this table moves, and a table that
/// failed the build on movement would be a ratchet on a number nobody has ruled
/// on.</para>
/// <para>**THE SPLIT IS BY AN INDEPENDENT WITNESS.** The keying meter sweeps 400
/// to 1200 Hz over its own six-second window and shares nothing with the
/// decoder, so whether it said somebody was keying at a character's moment is
/// evidence about that character that the decoder did not supply
/// (HM-DEC-091).</para>
/// </remarks>
public sealed class TheCwBaselineTable
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the analysis.</summary>
    /// <param name="output">Where the table is also printed.</param>
    public TheCwBaselineTable(ITestOutputHelper output) => _output = output;

    /// <summary>The date this table was taken, which names the file.</summary>
    /// <remarks>
    /// Fixed rather than read from a clock, so re-running the analysis rewrites
    /// the same file instead of leaving a trail of near-identical ones, and so
    /// the content is the same on any machine on any day (§5).
    /// </remarks>
    private const string TakenOn = "2026-08-23";

    /// <summary>How often the witness is asked, in seconds.</summary>
    private const double WitnessStepSeconds = 0.5;

    /// <summary>What the tone tracker reports for each recording.</summary>
    /// <remarks>
    /// Passed in rather than searched for. Finding a station is the survey's job
    /// and this is a measurement of the decoder, so a survey that chose the wrong
    /// bin would show up here as a decoder fault (HM-DEC-091).
    /// </remarks>
    private static double Tone(string name) => name switch
    {
        var n when n.Contains("004507", StringComparison.Ordinal) => 501,
        var n when n.Contains("003016", StringComparison.Ordinal) => 669,
        var n when n.Contains("003126", StringComparison.Ordinal) => 675,
        var n when n.Contains("003758", StringComparison.Ordinal) => 501,
        _ => 600,
    };

    /// <summary>
    /// The readings a person adjudicated, quoted so the table carries them.
    /// </summary>
    /// <remarks>
    /// **THESE ARE NOT TAKEN FROM ANY DECODER'S OUTPUT.** Each was cut by hand
    /// from the gate's own elements and ruled on; a decode that agrees with one
    /// is evidence and a decode that disagrees is a defect, and neither is
    /// established by re-running the decoder (§12.5).
    /// </remarks>
    private static string? Adjudicated(string name) => name switch
    {
        var n when n.Contains("134712", StringComparison.Ordinal)
            => "N4L (HM-DEC-144)",
        var n when n.Contains("013347", StringComparison.Ordinal)
            => "VA3VRR (HM-DEC-145)",
        _ => null,
    };

    /// <summary>The two recordings an independent sweep says hold no keying.</summary>
    private static bool HoldsNoStation(string name)
        => name.Contains("014854", StringComparison.Ordinal)
           || name.Contains("014935", StringComparison.Ordinal);

    /// <remarks>
    /// Writes the table. Reads nothing but the corpus and produces nothing but
    /// the file, so it is a measurement run rather than a test of behavior.
    /// </remarks>
    [Fact]
    public void Write()
    {
        var page = new StringBuilder();

        page.AppendLine(
            $"# The CW decoder's baseline, {TakenOn}");
        page.AppendLine();
        page.AppendLine(
            "Every figure below was measured by code committed in the same session");
        page.AppendLine(
            "that wrote this file. Nothing is copied forward from a review or a");
        page.AppendLine(
            "brief, and nothing here is a target: it is what the shipped decoder");
        page.AppendLine("does today, so later work can be judged by how it moves.");
        page.AppendLine();
        page.AppendLine("Regenerate with:");
        page.AppendLine();
        page.AppendLine("```");
        page.AppendLine(
            "dotnet test tests/Hamlet.RadioEngine.Tests "
            + "--filter FullyQualifiedName~TheCwBaselineTable");
        page.AppendLine("```");
        page.AppendLine();

        WriteCaptures(page);
        WriteSweep(page);
        WriteStreamingGate(page);

        var path = Path.Combine(
            RepositoryRoot(), $"ANALYSIS-cw-baseline-{TakenOn}.md");

        File.WriteAllText(path, page.ToString());

        _output.WriteLine(page.ToString());
        _output.WriteLine($"written to {path}");

        Assert.True(File.Exists(path));
    }

    private static void WriteCaptures(StringBuilder page)
    {
        page.AppendLine("## The corpus, capture by capture");
        page.AppendLine();
        page.AppendLine(
            "`shipped` is the production path: the streaming windower, with the");
        page.AppendLine(
            "sender's unit measured from the window and handed to the decoder as");
        page.AppendLine(
            "its only speed hypothesis. `grid` is the offline whole-file decode");
        page.AppendLine(
            "with `atWordsPerMinute` null, so the speed grid searches. **The two");
        page.AppendLine(
            "differ in more than the speed** because one reads a rolling window and");
        page.AppendLine(
            "the other reads the whole file at once, so the gap between them is an");
        page.AppendLine(
            "upper bound on what the forced speed is worth rather than a");
        page.AppendLine(
            "measurement of it. The production default is untouched either way.");
        page.AppendLine();
        page.AppendLine(
            "`witness` is `KeyingEnvelope`'s verdict at that character's own");
        page.AppendLine(
            "moment, swept 400 to 1200 Hz over six seconds and sharing nothing with");
        page.AppendLine(
            "the decoder. `E-share` is the share of emitted letters that are `E`,");
        page.AppendLine(
            "and `single-character words` is the share of whitespace-delimited");
        page.AppendLine("words that are one character long.");
        page.AppendLine();
        page.AppendLine(
            "**The span LLR is comparable within a recording and not across");
        page.AppendLine(
            "them.** It is a sum of per-hop log-likelihoods, and the per-hop");
        page.AppendLine(
            "difference works out at roughly the squared ratio of the signal");
        page.AppendLine(
            "amplitude to the noise scale, both of which are estimated from the");
        page.AppendLine(
            "recording's own envelope. A quiet recording therefore produces");
        page.AppendLine(
            "enormous numbers rather than confident ones, and the estimate setting");
        page.AppendLine(
            "that scale is `Percentile(sorted, 25) * 0.6`, which is the very thing");
        page.AppendLine("the next unit is scoped to look at.");
        page.AppendLine();

        foreach (var path in Captures())
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var audio = WavAudio.Read(path);
            var tone = Tone(name);

            page.AppendLine($"### `{name}`");
            page.AppendLine();
            page.AppendLine(
                $"{Num(audio.Duration.TotalSeconds, "0.0")} s at {audio.SampleRate} Hz, "
                + $"read at {Num(tone, "0")} Hz.");
            page.AppendLine();

            if (Adjudicated(name) is { } ruled)
            {
                page.AppendLine(
                        $"**Adjudicated reading: `{ruled}`.** Quoted from the ruling "
                    + "rather than from any decoder.");
                page.AppendLine();
            }

            if (HoldsNoStation(name))
            {
                page.AppendLine(
                    "**An independent sweep says this holds no keying at all.** The "
                    + "right emission is none.");
                page.AppendLine();
            }

            var shipped = CwDecodeHarness.Decode(audio, tone);
            var grid = CwProbabilisticDecoder.Decode(audio, tone);
            var witness = Witness(audio);

            page.AppendLine("| | shipped | grid |");
            page.AppendLine("|---|---|---|");
            page.AppendLine(
                $"| characters emitted | {shipped.Letters.Count} "
                + $"| {GridLetters(grid).Count} |");
            page.AppendLine(
                $"| E-share | {Share(EShare(shipped.Letters))} "
                + $"| {Share(EShare(GridLetters(grid)))} |");
            page.AppendLine(
                $"| single-character words "
                + $"| {Share(SingleCharacterWords(shipped.Text))} "
                + $"| {Share(SingleCharacterWords(GridText(grid)))} |");
            page.AppendLine(
                $"| words per minute read | {shipped.WordsPerMinute} "
                + $"| {Num(grid.WordsPerMinute, "0.0")} |");
            page.AppendLine();

            page.AppendLine("The witness split, over the shipped decode:");
            page.AppendLine();
            page.AppendLine(
                "**Three rows and not two.** `listening` is the meter before it has");
            page.AppendLine(
                "formed a verdict at all, which is its first six seconds and any");
            page.AppendLine(
                "stretch where it has not yet seen enough, and folding that into");
            page.AppendLine(
                "`no keying` would report an absence of evidence as evidence of");
            page.AppendLine("absence (§0.0).");
            page.AppendLine();
            page.AppendLine(
                "| witness | characters | E-share | single-char words "
                + "| span LLR P10 / median / P90 |");
            page.AppendLine("|---|---|---|---|---|");

            foreach (var verdict in new[]
            {
                KeyingVerdict.Keying,
                KeyingVerdict.NoKeying,
                KeyingVerdict.Listening,
            })
            {
                // Word gaps travel with the characters here, because the
                // single-character-word share is a fact about where the spaces
                // fell and dropping them would make every group score nought.
                var group = shipped.Characters
                    .Where(c => witness(c.At) == verdict).ToList();

                var letters = group.Where(c => !c.IsWordGap).ToList();

                page.AppendLine(
                    $"| {Name(verdict)} | {letters.Count} "
                    + $"| {Share(EShare(letters))} "
                    + $"| {Share(SingleCharacterWords(Join(group)))} "
                    + $"| {Spread(letters)} |");
            }

            page.AppendLine();

            page.AppendLine("What each read:");
            page.AppendLine();
            page.AppendLine("```");
            page.AppendLine("shipped: " + Readable(shipped.Text));
            page.AppendLine("grid:    " + Readable(GridText(grid)));
            page.AppendLine("```");
            page.AppendLine();
        }
    }

    private static void WriteSweep(StringBuilder page)
    {
        page.AppendLine("## The sensitivity sweep");
        page.AppendLine();
        page.AppendLine(
            $"`{CwSensitivity.Message}` at {CwSensitivity.WordsPerMinute} words a "
            + $"minute, {Num(CwSensitivity.ToneHz, "0")} Hz, averaged over "
            + $"{CwSensitivity.Seeds} noise draws at each level.");
        page.AppendLine();
        page.AppendLine(
            "**`invented` here counts characters aligned against nothing sent**,");
        page.AppendLine(
            "which is `CwMatchKind.Invented`. It is not the column the existing");
        page.AppendLine(
            "sweep prints under that name: `CwRefusalFloorTableTests` counts");
        page.AppendLine(
            "`CwMatchKind.Wrong`, a substitution at a position where something was");
        page.AppendLine(
            "sent, so a transcript full of characters that were never on the air");
        page.AppendLine(
            "scores nought there. Both are printed below so the difference can be");
        page.AppendLine("seen rather than argued about.");
        page.AppendLine();

        page.AppendLine(
            "Counts rather than shares, averaged over the seeds. A share needs a");
        page.AppendLine(
            "denominator, and correct and invented do not have the same one: a");
        page.AppendLine(
            "character that was sent and missed is not the same event as a");
        page.AppendLine(
            "character that was emitted and never sent, so putting both over the");
        page.AppendLine(
            "message length produces a table whose rows add to more than");
        page.AppendLine(
            $"everything. The message holds "
            + $"{CwAlignment.SymbolCount(CwSensitivity.Message)} characters.");
        page.AppendLine();
        page.AppendLine(
            "| generated | correct | wrong | invented | emitted | invented share "
            + "of what was read | read |");
        page.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var db in new[] { 18.0, 11.0, 3.0 })
        {
            var correct = 0;
            var wrong = 0;
            var invented = 0;
            var emitted = 0;
            var text = string.Empty;

            for (var seed = 1; seed <= CwSensitivity.Seeds; seed++)
            {
                var result = CwDecodeHarness.Decode(
                    new CwSignalRequest(
                        CwSensitivity.Message,
                        WordsPerMinute: CwSensitivity.WordsPerMinute,
                        ToneHz: CwSensitivity.ToneHz,
                        Amplitude: 0.5,
                        NoiseAmplitude: CwSensitivity.NoiseFor(db),
                        Seed: seed * 7919),
                    CwSensitivity.ToneHz);

                var matches = CwAlignment.Align(
                    result.Characters, CwSensitivity.Message);

                correct += matches.Count(
                    m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap);

                wrong += matches.Count(
                    m => m.Kind == CwMatchKind.Wrong && !m.Decoded.IsWordGap);

                invented += matches.Count(
                    m => m.Kind == CwMatchKind.Invented && !m.Decoded.IsWordGap);

                emitted += result.Letters.Count;

                if (seed == 1)
                {
                    text = result.Text;
                }
            }

            page.AppendLine(
                $"| {Num(db, "0")} dB "
                + $"| {Num((double)correct / CwSensitivity.Seeds, "0.0")} "
                + $"| {Num((double)wrong / CwSensitivity.Seeds, "0.0")} "
                + $"| {Num((double)invented / CwSensitivity.Seeds, "0.0")} "
                + $"| {Num((double)emitted / CwSensitivity.Seeds, "0.0")} "
                + $"| {Share(emitted == 0 ? double.NaN : (double)invented / emitted)} "
                + $"| `{Readable(text)}` |");
        }

        page.AppendLine();
    }

    private static void WriteStreamingGate(StringBuilder page)
    {
        page.AppendLine("## The streaming gate, read by read");
        page.AppendLine();
        page.AppendLine(
            "`Gate = 15` was set from a 3-to-6 against 24-to-39 separation the");
        page.AppendLine(
            "offline reference measured on whole files. **The instrument that");
        page.AppendLine(
            "actually gates is the streaming windower, and it has never been");
        page.AppendLine(
            "measured.** These are its own per-read likelihood ratios, taken from");
        page.AppendLine(
            "`CwProbabilisticStream.Last` after every read, split by whether");
        page.AppendLine(
            "somebody was keying at that read's own moment.");
        page.AppendLine();
        page.AppendLine(
            "**The split is by the same independent witness the corpus table");
        page.AppendLine(
            "uses**, asked at the moment of each read rather than once per file,");
        page.AppendLine(
            "because the question the gate has to answer is whether *this window*");
        page.AppendLine(
            "holds keying. A whole-file split would compare recordings, and the");
        page.AppendLine(
            "gate never gets to see a whole file.");
        page.AppendLine();
        page.AppendLine(
            "| recording | witness | reads | ratio P10 / median / P90 |");
        page.AppendLine("|---|---|---|---|");

        foreach (var path in Captures())
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var audio = WavAudio.Read(path);

            var stream = new CwProbabilisticStream(audio.SampleRate)
            {
                ToneHz = Tone(name),
            };

            var witness = Witness(audio);
            var byVerdict = new Dictionary<KeyingVerdict, List<double>>
            {
                [KeyingVerdict.Keying] = new(),
                [KeyingVerdict.NoKeying] = new(),
                [KeyingVerdict.Listening] = new(),
            };

            var last = double.NaN;

            var hop = Math.Max(
                1,
                (int)(audio.SampleRate
                    * CwProbabilisticDecoder.HopMilliseconds / 1000.0));

            for (var at = 0; at + hop <= audio.Samples.Length; at += hop)
            {
                stream.Process(audio.Samples.AsSpan(at, hop));

                var ratio = stream.Last.LikelihoodRatio;

                // One entry per read rather than one per hop: the reading only
                // changes when a read happens, and counting hops would report the
                // same number twelve times and call it twelve measurements.
                if (ratio.Equals(last))
                {
                    continue;
                }

                last = ratio;

                byVerdict[witness(
                    TimeSpan.FromSeconds((double)(at + hop) / audio.SampleRate))]
                    .Add(ratio);
            }

            var station = HoldsNoStation(name)
                ? " (an independent sweep says this holds no keying at all)"
                : string.Empty;

            page.AppendLine($"| `{name}`{station} | | | |");

            foreach (var verdict in new[]
            {
                KeyingVerdict.Keying,
                KeyingVerdict.NoKeying,
                KeyingVerdict.Listening,
            })
            {
                var ratios = byVerdict[verdict];

                page.AppendLine(
                    $"| | {Name(verdict)} | {ratios.Count} | {SpreadOf(ratios)} |");
            }
        }

        page.AppendLine();
        page.AppendLine(
            "**A read repeats most of its window twice a second**, so these are not");
        page.AppendLine(
            "independent samples and a median describes the recording rather than a");
        page.AppendLine(
            "decision. What the next unit needs from them is whether the two groups");
        page.AppendLine("separate at all on the instrument that actually gates.");
        page.AppendLine();
        page.AppendLine(
            "**And the ratio's scale is the same one the span LLR's is**: it rests");
        page.AppendLine(
            "on the window's own noise estimate, so a window holding nothing can");
        page.AppendLine(
            "score higher than a window holding a station, because the estimate");
        page.AppendLine(
            "collapses when there is nothing to estimate from. A gate derived from");
        page.AppendLine(
            "these numbers without that being fixed first would be a gate on how");
        page.AppendLine("quiet the band was.");
        page.AppendLine();
    }

    private static IReadOnlyList<string> Captures()
    {
        var folder = CapturedSignalTests.Folder;

        return Directory.GetFiles(folder, "*.wav")
            .Concat(Directory.GetFiles(
                Path.Combine(folder, "unadjudicated"), "*.wav"))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<CwProbabilisticCharacter> GridLetters(
        CwProbabilisticResult result)
        => result.Characters
            .Where(c => !string.Equals(c.Text, " ", StringComparison.Ordinal))
            .ToList();

    private static string GridText(CwProbabilisticResult result)
        => string.Concat(result.Characters.Select(c => c.Text));

    private static string Join(IReadOnlyList<CwCharacter> characters)
        => string.Concat(characters.Select(c => c.Text));

    /// <summary>What the witness said, in the words the app uses.</summary>
    /// <param name="verdict">The verdict.</param>
    /// <returns>The word.</returns>
    private static string Name(KeyingVerdict verdict) => verdict switch
    {
        KeyingVerdict.Keying => "said keying",
        KeyingVerdict.NoKeying => "said no keying",
        _ => "had not decided",
    };

    private static double EShare(IReadOnlyList<CwCharacter> letters)
        => letters.Count == 0
            ? double.NaN
            : (double)letters.Count(
                c => string.Equals(c.Text, "E", StringComparison.Ordinal))
              / letters.Count;

    private static double EShare(IReadOnlyList<CwProbabilisticCharacter> letters)
        => letters.Count == 0
            ? double.NaN
            : (double)letters.Count(
                c => string.Equals(c.Text, "E", StringComparison.Ordinal))
              / letters.Count;

    private static double SingleCharacterWords(string text)
    {
        var words = text.Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        return words.Length == 0
            ? double.NaN
            : (double)words.Count(w => w.Length == 1) / words.Length;
    }

    /// <summary>One number, formatted the same on every machine (§5).</summary>
    /// <param name="value">The number.</param>
    /// <param name="format">How to render it.</param>
    /// <returns>The rendering.</returns>
    private static string Num(double value, string format)
        => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Share(double value)
        => double.IsNaN(value)
            ? "no characters"
            : value.ToString("P0", CultureInfo.InvariantCulture);

    private static string Spread(IReadOnlyList<CwCharacter> letters)
        => SpreadOf(letters
            .Select(c => c.SpanLogLikelihoodRatio)
            .Where(r => !double.IsNaN(r))
            .ToList());

    private static string SpreadOf(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return "nothing measured";
        }

        var sorted = values.OrderBy(v => v).ToArray();

        return string.Format(
            "{0:0.0} / {1:0.0} / {2:0.0}",
            At(sorted, 0.10),
            At(sorted, 0.50),
            At(sorted, 0.90));
    }

    private static double At(double[] sorted, double share)
        => sorted[Math.Clamp(
            (int)Math.Round(share * (sorted.Length - 1)), 0, sorted.Length - 1)];

    private static string Readable(string text)
        => text.Length == 0
            ? "(nothing)"
            : text.Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);

    /// <summary>
    /// The keying verdict at any moment of a recording, from the meter itself.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>A lookup from a moment to what the witness said then.</returns>
    /// <remarks>
    /// The meter is fed the same six-second window the app feeds it, stepped half
    /// a second at a time, so its verdict has the cadence and the hysteresis it
    /// has on the air. A character's moment takes the newest verdict formed at or
    /// before it, which is what the operator would have been looking at.
    /// </remarks>
    private static Func<TimeSpan, KeyingVerdict> Witness(MonoAudio audio)
    {
        var meter = new CwKeyingMeter();
        var window = (int)(CwKeyingThresholds.Window.TotalSeconds * audio.SampleRate);
        var step = Math.Max(1, (int)(WitnessStepSeconds * audio.SampleRate));
        var stamps = new List<(double At, KeyingVerdict Verdict)>();

        for (var end = window; end <= audio.Samples.Length; end += step)
        {
            var slice = new MonoAudio(
                audio.SampleRate, audio.Samples[(end - window)..end]);

            stamps.Add((
                (double)end / audio.SampleRate, meter.Update(slice).Verdict));
        }

        return at =>
        {
            var seconds = at.TotalSeconds;
            var verdict = KeyingVerdict.Listening;

            foreach (var stamp in stamps)
            {
                if (stamp.At > seconds)
                {
                    break;
                }

                verdict = stamp.Verdict;
            }

            return verdict;
        };
    }

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
