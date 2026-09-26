using System.Diagnostics;
using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-123: before the second decoder votes, both decoders are scored
/// on every keyed recording and the synthetic set through the same scorer and
/// metrics, and the result is tabled per recording and per condition (work
/// instruction 458; PHASE_PLAN.md 9.2).
/// </summary>
/// <remarks>
/// <para>**THE SAME AUDIO, THE SAME CALLS.** Each decoder starts cold at sample 0
/// of the file, nothing added or removed. Our decoder is fed hop by hop from
/// 600 Hz exactly as <see cref="TheRequirementsAreMeasuredTests"/> feeds it. The
/// port is given the file through <see cref="FldigiRateAdapter"/> and the pitch
/// instrument's median over the file's keyed windows, never the construction
/// pitch and never our tracker's (458 DECIDED (3)). Both texts then go through
/// <see cref="TheRequirementsAreMeasuredTests.Measure"/> with the same scored
/// stretches, which the scorer locates in each decoder's own text by the same
/// rule, and through the same <see cref="CwMetrics"/> calls.</para>
/// <para>**THE PORT'S OUTPUT, MAPPED ONCE HERE AND NOWHERE ELSE** (458 DECIDED
/// (2)). fldigi carries no confidence. Every non-space string it prints is scored
/// as sure and as itself; its no-match output, `CW_noise` (`*` by default,
/// configuration.h:249-251), printed where <c>rx_lookup</c> finds nothing
/// (cw.cxx:893-898), is scored as a placeholder, as ours is; its spaces are word
/// boundaries as emitted. No port character is mapped to dim or to another
/// character.</para>
/// <para>**ITS ASSERTIONS ARE ABOUT THE HARNESS, NEVER ABOUT EITHER DECODER'S
/// SCORE.** Our row through here equals <see cref="TheRequirementsAreMeasuredTests"/>'
/// figures, and every recording has a row for both decoders or says why not.</para>
/// </remarks>
public sealed class BothDecodersAreScoredAlikeTests
{
    /// <summary>What the port prints where its table has no match, with the shipped defaults.</summary>
    internal const string PortNoMatch = "*";

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly Lazy<IReadOnlyList<Row>> AllRows = new(Build);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public BothDecodersAreScoredAlikeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One decoder's run over one file.</summary>
    /// <param name="Settled">What it put out, as characters the scorer takes; the port's mapped by <see cref="Mapped"/>.</param>
    /// <param name="EmittedSeconds">For each character, the input time at which it was put out, in seconds of the file.</param>
    /// <param name="Raw">The text as the decoder itself printed it.</param>
    /// <param name="DecodeTime">Wall time inside the decoder, resampling and the pitch instrument excluded.</param>
    /// <param name="Notes">For each character, the decoder's own evidence as it recorded it: our pattern, speed and confidence; the port's representation, receive speed and two_dots.</param>
    internal sealed record Decoded(
        IReadOnlyList<CwCharacter> Settled, IReadOnlyList<double> EmittedSeconds, string Raw, TimeSpan DecodeTime,
        IReadOnlyList<string> Notes);

    /// <summary>One recording, both decoders on it, both measured the same way.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Set">Its set as <see cref="TheRequirementsAreMeasuredTests"/> names it.</param>
    /// <param name="Real">Real, with an inferred key, or synthetic, with an exact one.</param>
    /// <param name="Condition">Its condition as <see cref="TheRequirementsAreMeasuredTests"/> states it.</param>
    /// <param name="Kind">The key's kind.</param>
    /// <param name="AudioFile">The file, from the repository root.</param>
    /// <param name="Seconds">Its length.</param>
    /// <param name="Ours">Our decoder's run.</param>
    /// <param name="OursMeasured">Our decoder measured.</param>
    /// <param name="OursPitchHz">The median of our tracker's measured pitch over the hops it had one, or NaN.</param>
    /// <param name="Port">The port's run, or null where it could not be run.</param>
    /// <param name="PortMeasured">The port measured, or null where it could not be run.</param>
    /// <param name="PortNotRun">Why the port could not be run, or null.</param>
    /// <param name="GivenPitchHz">The pitch the port was given, or NaN.</param>
    /// <param name="Windows">The pitch instrument's keyed windows over the file.</param>
    /// <param name="ResampleTime">Wall time in <see cref="FldigiRateAdapter"/>.</param>
    internal sealed record Row(
        string Name, string Set, bool Real, string Condition, CwKeyKind Kind, string AudioFile, double Seconds,
        Decoded Ours, TheRequirementsAreMeasuredTests.Measured OursMeasured, double OursPitchHz,
        Decoded? Port, TheRequirementsAreMeasuredTests.Measured? PortMeasured, string? PortNotRun,
        double GivenPitchHz, int Windows, TimeSpan ResampleTime);

    /// <summary>Every keyed recording and every synthetic case, both decoders on each, once per process.</summary>
    internal static IReadOnlyList<Row> Rows => AllRows.Value;

    /// <summary>One string the port printed, as the scorer takes it (458 DECIDED (2)).</summary>
    /// <param name="e">What fldigi printed.</param>
    /// <returns>A word boundary for a space, a placeholder for the no-match output, and otherwise the string itself at high confidence.</returns>
    internal static CwCharacter Mapped(FldigiCwEmission e)
    {
        var at = TimeSpan.FromSeconds(e.InputSample / (double)FldigiCwDecoder.CW_SAMPLERATE);

        return e.Text == MorseAlphabet.WordGap
            ? new CwCharacter(MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, e.ReceiveSpeed, at)
            : e.Text == PortNoMatch
                ? new CwCharacter(MorseAlphabet.Unreadable, CwConfidence.Unreadable, 0, e.Representation, double.NaN, e.ReceiveSpeed, at)
                : new CwCharacter(e.Text, CwConfidence.High, 1, e.Representation, double.NaN, e.ReceiveSpeed, at);
    }

    private static IReadOnlyList<Row> Build()
    {
        var rows = new List<Row>();

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var file = Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav");
            var condition = TheRequirementsAreMeasuredTests.RealCondition(k.Name);

            rows.Add(Both(k.Name, k.Set, true, condition, CwKeyKind.Inferred, file, 600,
                settled => k.Score(CwReading.Of(settled))));
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var file = Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav");

            rows.Add(Both(recipe.Name, "synthetic", false, TheRequirementsAreMeasuredTests.SyntheticCondition(recipe),
                CwKeyKind.Exact, file, SyntheticCq.StartingPitchHz, SyntheticScores));
        }

        return rows;
    }

    /// <summary>The synthetic set's one stretch, exactly as <see cref="TheRequirementsAreMeasuredTests"/> takes it.</summary>
    private static IReadOnlyList<CwScore> SyntheticScores(IReadOnlyList<CwCharacter> settled)
    {
        var reading = CwReading.Of(settled);
        var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

        return new[] { SyntheticCq.Whole(reading) with { Start = from } };
    }

    private static Row Both(
        string name, string set, bool real, string condition, CwKeyKind kind, string file, double startingHz,
        Func<IReadOnlyList<CwCharacter>, IReadOnlyList<CwScore>> scores)
    {
        var audio = WavAudio.Read(file);
        var (ours, oursPitch) = DriveOurs(audio.Samples, audio.SampleRate, startingHz);
        var oursMeasured = TheRequirementsAreMeasuredTests.Measure(name, set, condition, kind, ours.Settled, scores(ours.Settled));
        var relative = Path.GetRelativePath(CwToneSurveyTests.RepositoryRoot(), file).Replace('\\', '/');
        var seconds = audio.Samples.Length / (double)audio.SampleRate;

        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);

        if (windows.Count == 0)
        {
            return new Row(name, set, real, condition, kind, relative, seconds, ours, oursMeasured, oursPitch,
                null, null, "the pitch instrument found no keyed window, so there is no pitch to give it (458 DECIDED (3))",
                double.NaN, 0, TimeSpan.Zero);
        }

        var given = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
        var resample = Stopwatch.StartNew();
        double[] input;

        try
        {
            input = FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate);
        }
        catch (ArgumentException e)
        {
            return new Row(name, set, real, condition, kind, relative, seconds, ours, oursMeasured, oursPitch,
                null, null, "the rate adapter refused the file: " + e.Message, given, windows.Count, TimeSpan.Zero);
        }

        resample.Stop();

        var port = DrivePort(input, given);
        var portMeasured = TheRequirementsAreMeasuredTests.Measure(name, set, condition, kind, port.Settled, scores(port.Settled));

        return new Row(name, set, real, condition, kind, relative, seconds, ours, oursMeasured, oursPitch,
            port, portMeasured, null, given, windows.Count, resample.Elapsed);
    }

    /// <summary>Our decoder, fed hop by hop as the metrics feed it, with each character's emission time and the tracker's pitch read beside it.</summary>
    private static (Decoded Decoded, double PitchHz) DriveOurs(float[] samples, int sampleRate, double startingHz)
    {
        var decoder = new CwDecoder(sampleRate, startingHz);
        var settled = new List<CwCharacter>();
        var emitted = new List<double>();
        var pitches = new List<double>();
        var hop = decoder.Tracker.HopSamples;
        var now = 0L;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);
            emitted.Add(now / (double)sampleRate);
        };

        var watch = Stopwatch.StartNew();

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            now = at + hop;
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));

            if (decoder.Tracker.HasMeasuredPitch)
            {
                pitches.Add(decoder.Tracker.ToneHz);
            }
        }

        now = samples.Length;
        decoder.Flush();
        watch.Stop();

        var pitch = pitches.Count == 0 ? double.NaN : pitches.OrderBy(p => p).ElementAt(pitches.Count / 2);

        var notes = settled.Select(c => $"pattern {c.Pattern} at {c.WordsPerMinute} WPM, {c.Confidence}").ToList();

        return (new Decoded(settled, emitted, CwReading.Of(settled).Text, watch.Elapsed, notes), pitch);
    }

    /// <summary>The port over the whole file at 8000 Hz, given a pitch, as fldigi's file benchmark runs it with the squelch off.</summary>
    private static Decoded DrivePort(double[] input, double pitchHz)
    {
        var decoder = new FldigiCwDecoder(pitchHz);
        var watch = Stopwatch.StartNew();

        decoder.rx_process(input);
        watch.Stop();

        return new Decoded(
            decoder.Emissions.Select(Mapped).ToList(),
            decoder.Emissions.Select(e => e.InputSample / (double)FldigiCwDecoder.CW_SAMPLERATE).ToList(),
            decoder.Text,
            watch.Elapsed,
            decoder.Emissions.Select(e => $"rep {e.Representation} at {e.ReceiveSpeed} WPM, two_dots {e.TwoDots}").ToList());
    }

    /// <summary>The characters of a decoder's run a stretch covers, with their emission times.</summary>
    private static IEnumerable<(CwCharacter Character, double Emitted)> CoveredWithTimes(Decoded d, CwScore score)
    {
        var covered = TheRequirementsAreMeasuredTests.Covered(d.Settled, score);

        if (covered.Count == 0)
        {
            yield break;
        }

        var first = IndexOf(d.Settled, covered[0]);

        for (var i = 0; i < covered.Count; i++)
        {
            yield return (covered[i], d.EmittedSeconds[first + i]);
        }
    }

    private static int IndexOf(IReadOnlyList<CwCharacter> list, CwCharacter c)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], c))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// When each key character's last mark ends, from the recipe the generator keys
    /// from, with <see cref="MorseCode.Spell"/>, the table it spells with.
    /// </summary>
    private static IReadOnlyList<double> KeyCharacterEnds(CwFixtureRecipe recipe)
    {
        // The generator's LeadInSeconds; no synthetic case carries a preamble.
        var at = 1.0 + recipe.PreambleSeconds;
        var ends = new List<double>();
        var firstWord = true;

        foreach (var word in recipe.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!firstWord)
            {
                at += recipe.WordGapMilliseconds / 1000.0;
            }

            firstWord = false;

            for (var c = 0; c < word.Length; c++)
            {
                if (c > 0)
                {
                    at += recipe.CharacterGapMilliseconds / 1000.0;
                }

                var pattern = MorseCode.Spell(word[c])!;

                for (var e = 0; e < pattern.Length; e++)
                {
                    if (e > 0)
                    {
                        at += recipe.ElementGapMilliseconds / 1000.0;
                    }

                    at += (pattern[e] == '.' ? recipe.DitMilliseconds : recipe.DahMilliseconds) / 1000.0;
                }

                ends.Add(at);
            }
        }

        return ends;
    }

    /// <summary>The four metrics in their parts, summed over whatever was measured.</summary>
    /// <param name="SureWrong">Sure characters where the key has a different one.</param>
    /// <param name="SureAdded">Sure characters where the key has none.</param>
    /// <param name="SureEmitted">Sure characters emitted inside the stretches.</param>
    /// <param name="Invented">MET-INVENTED's count, counted by <see cref="CwMetrics.Invented"/>.</param>
    /// <param name="Sent">Characters sent.</param>
    /// <param name="SureRight">Sure characters the key has in place.</param>
    /// <param name="Inserted">Word boundaries inserted.</param>
    /// <param name="Deleted">Word boundaries deleted.</param>
    /// <param name="Words">Words sent.</param>
    /// <param name="Measured">Recordings measured.</param>
    /// <param name="Recordings">Recordings in all.</param>
    internal sealed record Totals(
        int SureWrong, int SureAdded, int SureEmitted, int Invented, int Sent, int SureRight,
        int Inserted, int Deleted, int Words, int Measured, int Recordings)
    {
        /// <summary>MET-CER-SURE's numerator.</summary>
        public int SureErrors => SureWrong + SureAdded;

        /// <summary>The four metrics summed over recordings measured, each counted by its own <see cref="CwMetrics"/> call.</summary>
        /// <param name="measured">One entry per recording; null where a decoder could not be run.</param>
        /// <returns>The sums.</returns>
        public static Totals Of(IReadOnlyList<TheRequirementsAreMeasuredTests.Measured?> measured)
        {
            var stretches = measured.Where(m => m is { NotComputable: null }).SelectMany(m => m!.Stretches).ToList();
            var e = stretches.Select(CwMetrics.SureErrors).ToList();
            var n = stretches.Select(CwMetrics.Invented).ToList();
            var c = stretches.Select(CwMetrics.Coverage).ToList();
            var w = stretches.Select(CwMetrics.WordBoundaries).ToList();

            return new Totals(
                e.Sum(x => x.SureWrong), e.Sum(x => x.SureAdded), e.Sum(x => x.SureEmitted),
                n.Sum(x => x.Count), n.Sum(x => x.Sent), c.Sum(x => x.SureRight),
                w.Sum(x => x.Inserted), w.Sum(x => x.Deleted), w.Sum(x => x.WordsSent),
                measured.Count(m => m is { NotComputable: null }), measured.Count);
        }
    }

    private static string Share(int count, int of) => of == 0
        ? "no number"
        : (count / (double)of).ToString("0.000", Invariant);

    private static string CerSure(Totals t) => $"{t.SureErrors} of {t.SureEmitted} ({Share(t.SureErrors, t.SureEmitted)})";

    private static string InventedText(Totals t) => $"{t.Invented} / {t.Sent} ({Share(t.Invented, t.Sent)})";

    private static string CoverageText(Totals t) => $"{t.SureRight} / {t.Sent} ({Share(t.SureRight, t.Sent)})";

    private static string Wbe(Totals t) => $"{t.Inserted + t.Deleted} ({t.Inserted} ins, {t.Deleted} del) / {t.Words} ({Share(t.Inserted + t.Deleted, t.Words)})";

    private static string Hz(double hz) => double.IsNaN(hz) ? "none" : hz.ToString("0.0", Invariant);

    private static string S(double seconds) => seconds.ToString("0.000", Invariant);

    /// <remarks>
    /// Work instruction 458 task 1, a printer: for every recording both decoders
    /// read, the audio file, the key's kind, the condition, each scored stretch on
    /// the recording's clock for each decoder, and the pitch each was on; then,
    /// on one synthetic case, when each decoder puts a right character out
    /// against when its last mark ended. Asserts only that every recording was
    /// read by our decoder, as the metrics read it.
    /// </remarks>
    [Fact]
    public void TheScoringPathThePitchAndTheLatency()
    {
        _output.WriteLine("corpus | recording | set | key | audio | seconds | condition");

        foreach (var r in Rows)
        {
            _output.WriteLine(string.Create(Invariant,
                $"corpus | {r.Name} | {r.Set} | {CwMetrics.KindWord(r.Kind)} | {r.AudioFile} | {r.Seconds:0.00} | {r.Condition}"));
        }

        _output.WriteLine("span | recording | decoder | stretch | key | scored text | first put out s | last put out s | first At s | last At s");

        foreach (var r in Rows)
        {
            SpanLines(r.Name, "ours", r.Ours, r.OursMeasured);

            if (r.Port is { } port && r.PortMeasured is { } measured)
            {
                SpanLines(r.Name, "port", port, measured);
            }
            else
            {
                _output.WriteLine($"span | {r.Name} | port | not run: {r.PortNotRun}");
            }
        }

        _output.WriteLine("pitch | recording | set | ours tracked (median of measured hops) Hz | port given (instrument median) Hz | instrument windows | constructed Hz");

        foreach (var r in Rows)
        {
            var constructed = r.Real ? "none: a real recording" : "615 with 3 Hz of drift either side (CwFixtureRecipe)";

            _output.WriteLine(
                $"pitch | {r.Name} | {r.Set} | {Hz(r.OursPitchHz)} | {Hz(r.GivenPitchHz)} | {r.Windows} | {constructed}");
        }

        Latency("cq-18wpm-15db");

        Assert.Equal(WhatTheStrayLettersRestOnTests.KeyedRecordings.Count + SyntheticCq.All.Count, Rows.Count);
    }

    /// <remarks>
    /// HM-REQ-123, sameness: our decoder's row through this harness equals
    /// <see cref="TheRequirementsAreMeasuredTests"/>' own, recording by recording
    /// and in total, on all four metrics. Watched failing first with a wrong
    /// expected figure (work instruction 458, task 2).
    /// </remarks>
    [Fact]
    public void OurRowIsTheMetricsOwnRow()
    {
        var theirs = TheRequirementsAreMeasuredTests.Real.Concat(TheRequirementsAreMeasuredTests.Synthetic)
            .ToDictionary(m => m.Name, StringComparer.Ordinal);
        var failures = new List<string>();

        _output.WriteLine("sameness | recording | here: CER-SURE, INVENTED, coverage, WBE | TheRequirementsAreMeasuredTests");

        foreach (var r in Rows)
        {
            var here = Totals.Of(new TheRequirementsAreMeasuredTests.Measured?[] { r.OursMeasured });
            var there = Totals.Of(new TheRequirementsAreMeasuredTests.Measured?[] { theirs[r.Name] });

            _output.WriteLine($"sameness | {r.Name} | {CerSure(here)}, {InventedText(here)}, {CoverageText(here)}, {Wbe(here)} | "
                + $"{CerSure(there)}, {InventedText(there)}, {CoverageText(there)}, {Wbe(there)}");

            if (here != there)
            {
                failures.Add($"{r.Name}: here {here}, there {there}");
            }
        }

        foreach (var real in new[] { true, false })
        {
            var here = Totals.Of(Rows.Where(r => r.Real == real).Select(r => (TheRequirementsAreMeasuredTests.Measured?)r.OursMeasured).ToList());
            var there = Totals.Of((real ? TheRequirementsAreMeasuredTests.Real : TheRequirementsAreMeasuredTests.Synthetic)
                .Select(m => (TheRequirementsAreMeasuredTests.Measured?)m).ToList());

            // Watched failing first at 01b6a651, with SureWrong one too many here.
            var expected = there;
            var set = real ? "real, inferred" : "synthetic, exact";

            _output.WriteLine($"sameness | total {set} | here {CerSure(here)}, {InventedText(here)}, {CoverageText(here)}, {Wbe(here)} | "
                + $"expected {CerSure(expected)}, {InventedText(expected)}, {CoverageText(expected)}, {Wbe(expected)}");

            if (here != expected)
            {
                failures.Add($"total {set}: here {here}, expected {expected}");
            }
        }

        _output.WriteLine("sameness | entry, 457's exit | real 33 of 436, 33 / 473, 403 / 473, 37 (29 ins, 8 del) / 113 | synthetic 14 of 173, 14 / 252, 159 / 252, 44 (13 ins, 31 del) / 84");

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    /// <remarks>
    /// HM-REQ-123, coverage: every keyed recording and every synthetic case has a
    /// row for both decoders, or a row saying why a decoder could not be run on
    /// it, with the error.
    /// </remarks>
    [Fact]
    public void EveryRecordingHasARowForBothDecoders()
    {
        var wanted = WhatTheStrayLettersRestOnTests.KeyedRecordings.Select(k => k.Name)
            .Concat(SyntheticCq.All.Select(c => c.Name))
            .ToList();

        foreach (var r in Rows)
        {
            _output.WriteLine(r.PortNotRun is { } why
                ? $"row | {r.Name} | ours {(r.OursMeasured.NotComputable ?? "measured")} | port not run: {why}"
                : $"row | {r.Name} | ours {(r.OursMeasured.NotComputable ?? "measured")} | port {(r.PortMeasured!.NotComputable ?? "measured")}");
        }

        Assert.Equal(wanted, Rows.Select(r => r.Name).ToList());
        Assert.All(Rows, r => Assert.True(r.PortMeasured is not null || r.PortNotRun is not null, r.Name));
        Assert.All(Rows, r => Assert.True(r.OursMeasured.NotComputable is not null || r.OursMeasured.Stretches.Count > 0, r.Name));
        Assert.All(Rows.Where(r => r.PortMeasured is not null),
            r => Assert.True(r.PortMeasured!.NotComputable is not null || r.PortMeasured.Stretches.Count > 0, r.Name));
    }

    /// <remarks>
    /// HM-REQ-123, the table: writes `docs/phase-requirements/parity.md` - the
    /// mapping, both decoders per recording and per condition on the four
    /// metrics, each decoder's decode time, and what the table does not prove -
    /// and prints each decoder's scored text beside the key. Asserts only that
    /// the file was written with its five parts.
    /// </remarks>
    [Fact]
    public void TheParityTableIsWritten()
    {
        var path = Path.Combine(CwToneSurveyTests.RepositoryRoot(), "docs", "phase-requirements", "parity.md");
        var text = ParityMarkdown();

        File.WriteAllText(path, text);

        foreach (var r in Rows)
        {
            for (var i = 0; i < r.OursMeasured.Scores.Count; i++)
            {
                _output.WriteLine($"text | {r.Name} | {i + 1} | key `{r.OursMeasured.Scores[i].Key}`");
                _output.WriteLine($"text | {r.Name} | {i + 1} | ours `{r.OursMeasured.Scores[i].Region}`");

                if (r.PortMeasured is { NotComputable: null } p)
                {
                    _output.WriteLine($"text | {r.Name} | {i + 1} | port `{p.Scores[i].Region}`");
                }
            }

            _output.WriteLine($"text | {r.Name} | all | port printed `{r.Port?.Raw}`");
        }

        _output.WriteLine(text);

        foreach (var part in new[] { "## 1. ", "## 2. ", "## 3. ", "## 4. ", "## 5. " })
        {
            Assert.Contains(part, text, StringComparison.Ordinal);
        }
    }

    private static string ParityMarkdown()
    {
        var md = new System.Text.StringBuilder();
        var portRuns = Rows.Where(r => r.Port is not null).Select(r => r.Port!).ToList();
        var portAll = portRuns.SelectMany(p => p.Settled).Select(c => CwSymbol.Of(c).Class).ToList();
        var portScored = Rows.Where(r => r.PortMeasured is { NotComputable: null })
            .SelectMany(r => r.PortMeasured!.Stretches).SelectMany(a => a.Steps)
            .Where(s => s.Decoded is not null).Select(s => s.Decoded!.Value.Class).ToList();
        var portGaps = Rows.Where(r => r.PortMeasured is { NotComputable: null })
            .SelectMany(r => r.PortMeasured!.Stretches).SelectMany(a => a.Steps).Count(s => s.Decoded is not null && s.GapBefore);

        md.Append("# Parity: Hamlet's decoder and the fldigi port on the same audio (HM-REQ-123)\n\n");
        md.Append("Written by `BothDecodersAreScoredAlikeTests.TheParityTableIsWritten` (work instruction 458, PHASE_PLAN.md 9.2). ");
        md.Append("Both decoders start cold at sample 0 of each file with nothing added or removed. Ours is fed hop by hop from 600 Hz, as ");
        md.Append("`TheRequirementsAreMeasuredTests` feeds it; the port is given the file at 8000 Hz through `FldigiRateAdapter` and the pitch ");
        md.Append("instrument's median over the file's keyed windows, with fldigi's shipped defaults at `61b97f41` and the squelch off. ");
        md.Append("Both texts go through `TheRequirementsAreMeasuredTests.Measure` and the same `CwMetrics` calls, each stretch located in ");
        md.Append("each decoder's own text by the same rule. Neither decoder was changed. Every number carries its key's kind (V-13).\n\n");

        md.Append("## 1. The mapping of the port's unclassed output\n\n");
        md.Append("458 DECIDED (2), the author's, overrulable: every non-space character the port prints is scored as sure, because fldigi ");
        md.Append("shows every character alike and an operator reads it as asserted (CLAUDE.md 0.0), and mapping it to dim would hide its ");
        md.Append("errors from MET-CER-SURE; its no-match output at `rx_lookup`'s caller is scored as a placeholder, as ours is; its spaces ");
        md.Append("are word boundaries as emitted, 456's 5-unit finding included.\n\n");
        md.Append("- **No-match output:** `rx_lookup` returns `\"\"` when the table has no entry (`src/cw_rtty/morse.cxx:254`); its caller then ");
        md.Append("prints `CW_noise` (`src/cw_rtty/cw.cxx:892-898`), `*` by default (`src/include/configuration.h:249-251`). `*` is not ");
        md.Append("in fldigi's table, so it is only ever this. Scored as a placeholder: never wrong, never coverage.\n");
        md.Append("- **Characters:** the table's printed form, `<BT>` for a prosign with `CW_prosign_display` off, accented letters and `_` ");
        md.Append("included, each scored as itself at sure. None is mapped to another character or to dim.\n");
        md.Append("- **Spaces:** printed once after more than 4 dot lengths of silence (`cw.cxx:909-914`), each a word boundary.\n\n");
        md.Append($"Counts over everything the port printed on the {portRuns.Count} files: {portAll.Count(c => c == CwSymbolClass.Sure)} sure, ");
        md.Append($"{portAll.Count(c => c == CwSymbolClass.Placeholder)} placeholder (`*`), {portAll.Count(c => c == CwSymbolClass.WordGap)} word boundaries, ");
        md.Append($"{portAll.Count(c => c == CwSymbolClass.NotSure)} not sure. Inside the scored stretches: {portScored.Count(c => c == CwSymbolClass.Sure)} sure, ");
        md.Append($"{portScored.Count(c => c == CwSymbolClass.Placeholder)} placeholder, {portGaps} word boundaries between scored characters.\n\n");

        md.Append("## 2. Per recording\n\n");
        md.Append("MET-CER-SURE is sure characters wrong or added of sure emitted; MET-INVENTED is sure added plus sure wrong over characters sent; ");
        md.Append("coverage is sure and right over characters sent (R82); MET-WBE is word boundaries inserted plus deleted over words sent. ");
        md.Append("Summed over a recording's stretches.\n\n");
        md.Append("| recording | key | CER-SURE ours | CER-SURE port | INVENTED ours | INVENTED port | coverage ours | coverage port | WBE ours | WBE port |\n");
        md.Append("|---|---|---|---|---|---|---|---|---|---|\n");

        foreach (var r in Rows)
        {
            var o = Totals.Of(new TheRequirementsAreMeasuredTests.Measured?[] { r.OursMeasured });
            var ours = r.OursMeasured.NotComputable;
            var port = r.PortNotRun ?? r.PortMeasured?.NotComputable;
            var p = Totals.Of(new[] { r.PortMeasured });

            md.Append($"| {r.Name} | {CwMetrics.KindWord(r.Kind)} | ");
            md.Append(ours is null ? CerSure(o) : "no number: " + ours).Append(" | ");
            md.Append(port is null ? CerSure(p) : "no number: " + port).Append(" | ");
            md.Append(ours is null ? InventedText(o) : "-").Append(" | ");
            md.Append(port is null ? InventedText(p) : "-").Append(" | ");
            md.Append(ours is null ? CoverageText(o) : "-").Append(" | ");
            md.Append(port is null ? CoverageText(p) : "-").Append(" | ");
            md.Append(ours is null ? Wbe(o) : "-").Append(" | ");
            md.Append(port is null ? Wbe(p) : "-").Append(" |\n");
        }

        md.Append("\n## 3. Per condition\n\n");
        md.Append("The two conditions the tree has: real recordings with inferred keys, and the synthetic set with exact keys (458 DECIDED (4)). ");
        md.Append("**Neither is a `CH-*` condition, and no real capture is counted toward one (PHASE_PLAN.md 7.4).** Under each, the finer rows ");
        md.Append("`TheRequirementsAreMeasuredTests` states: for a real recording the sender `CW_SPEC.md` section 10 names, for a synthetic case ");
        md.Append("its character gap and in-passband level.\n\n");
        md.Append("| condition | key | recordings | CER-SURE ours | CER-SURE port | INVENTED ours | INVENTED port | coverage ours | coverage port | WBE ours | WBE port |\n");
        md.Append("|---|---|---|---|---|---|---|---|---|---|---|\n");

        foreach (var real in new[] { true, false })
        {
            var set = Rows.Where(r => r.Real == real).ToList();

            ConditionRow(md, real ? "**real HF, all**" : "**synthetic, all**", set);

            foreach (var g in set.GroupBy(r => r.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                ConditionRow(md, g.Key, g.ToList());
            }
        }

        md.Append("\n## 4. Decode time\n\n");
        md.Append("Wall time inside each decoder on this machine, summed per condition: ours from the first hop to `Flush`; the port inside ");
        md.Append("`rx_process`, with `FldigiRateAdapter`'s resampling to 8000 Hz given apart. The pitch instrument is in neither. ");
        md.Append("These figures move from run to run; nothing asserts them.\n\n");
        md.Append("| condition | recordings | audio s | ours s | port s | resampling s |\n|---|---|---|---|---|---|\n");

        foreach (var real in new[] { true, false })
        {
            var set = Rows.Where(r => r.Real == real).ToList();

            md.Append(string.Create(Invariant,
                $"| {(real ? "real HF, inferred keys" : "synthetic, exact keys")} | {set.Count} | {set.Sum(r => r.Seconds):0.0} | "
                + $"{set.Sum(r => r.Ours.DecodeTime.TotalSeconds):0.00} | {set.Sum(r => r.Port?.DecodeTime.TotalSeconds ?? 0):0.00} | "
                + $"{set.Sum(r => r.ResampleTime.TotalSeconds):0.00} |\n"));
        }

        md.Append("\n## 5. What the table does not prove\n\n");
        md.Append("- **The keys on the real rows are inferred** (V-13): reasoned from the form of a call, adjudicated, or differenced from ");
        md.Append("consecutive transcripts, never transcribed. A disagreement with one is not by itself proof either decoder is wrong.\n");
        md.Append("- **The synthetic set is never sole evidence** (CLAUDE.md 12.5): one generator, one text, shaped band noise not shown to be ");
        md.Append("`CH-AWGN`, and a 1.0 s lead-in of noise before the first mark, where the port loses its first element (457's verdict (b), ");
        md.Append("fldigi's own); that loss is scored here, not excused.\n");
        md.Append("- **Neither decoder has a calibrated confidence yet** (HM-REQ-124, 9.5). The port has none at all and every character it ");
        md.Append("prints is counted sure by the mapping above, so its MET-CER-SURE is its whole character error; ours marks some characters as ");
        md.Append("placeholders, which are never wrong. The two MET-CER-SURE columns therefore do not measure the same kind of restraint.\n");
        md.Append("- **The port runs on fldigi's shipped defaults at 18 WPM with tracking on**, given the instrument's pitch; fldigi at the ");
        md.Append("radio would take its pitch from the operator's cursor. Nothing here says how fldigi performs with other settings.\n");
        md.Append("- **The stretches are located in each decoder's own text by the same rule.** `CwScorer.Within` fits each key to the text ");
        md.Append("with free ends, independently, so on a text far from its keys two stretches can fall on overlapping characters, and a ");
        md.Append("text that reads little can be fitted where it happens to resemble the key. It is the rule ours is scored by, unchanged.\n");

        return md.ToString();
    }

    /// <summary>The captures in the tree our decoder reads as nothing, none of them keyed (work instruction 458, task 3).</summary>
    private static readonly string[] NothingRead =
    {
        "unadjudicated/cw-2026-08-20-014854",
        "unadjudicated/cw-2026-08-20-014935",
        "unadjudicated/cw-2026-08-22-014113",
        "unadjudicated/cw-2026-08-22-014308",
        "unadjudicated/cw-2026-08-26-125941",
    };

    /// <remarks>
    /// Work instruction 458 task 3 (PHASE_PLAN.md 9.3), a printer: every recording
    /// where the port scores better than ours on any of the four metrics, each
    /// stretch aligned to its key for both decoders with each character's class,
    /// and beside each departure from the key the decoder's own evidence; then
    /// what the port reads on each capture ours reads as nothing, printed and not
    /// scored, since none has a key. Asserts nothing about either score.
    /// </remarks>
    [Fact]
    public void WhereThePortScoresBetter()
    {
        _output.WriteLine("class codes: a letter alone is sure and right; [K>D] sure and wrong; [+D] sure and added; [-K] missing; ■ a placeholder; a leading space a word boundary");

        var wins = 0;

        foreach (var r in Rows)
        {
            if (r.PortMeasured is not { NotComputable: null } pm || r.Port is null || r.OursMeasured.NotComputable is not null)
            {
                continue;
            }

            var o = Totals.Of(new TheRequirementsAreMeasuredTests.Measured?[] { r.OursMeasured });
            var p = Totals.Of(new TheRequirementsAreMeasuredTests.Measured?[] { pm });
            var better = new List<string>();

            var cerBetter = o.SureEmitted > 0 && p.SureEmitted > 0 && p.SureErrors * o.SureEmitted < o.SureErrors * p.SureEmitted;

            foreach (var (metric, isBetter) in new[]
                     {
                         ("MET-CER-SURE", cerBetter),
                         ("MET-INVENTED", p.Invented < o.Invented),
                         ("coverage", p.SureRight > o.SureRight),
                         ("MET-WBE", p.Inserted + p.Deleted < o.Inserted + o.Deleted),
                     })
            {
                if (isBetter)
                {
                    better.Add(metric);
                }
            }

            if (better.Count == 0)
            {
                continue;
            }

            wins++;
            _output.WriteLine($"win | {r.Name} | {CwMetrics.KindWord(r.Kind)} | port better on {string.Join(", ", better)} | "
                + $"ours {CerSure(o)}, {InventedText(o)}, {CoverageText(o)}, {Wbe(o)} | port {CerSure(p)}, {InventedText(p)}, {CoverageText(p)}, {Wbe(p)}");

            for (var i = 0; i < pm.Scores.Count; i++)
            {
                _output.WriteLine($"win | {r.Name} | {i + 1} | key  | {pm.Scores[i].Key}");
                AlignedLines(r.Name, i, "ours", r.Ours, r.OursMeasured);
                AlignedLines(r.Name, i, "port", r.Port, pm);
            }
        }

        _output.WriteLine($"wins | {wins} of {Rows.Count} recordings where the port scores better on at least one of the four metrics");

        foreach (var name in NothingRead)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);

            if (windows.Count == 0)
            {
                _output.WriteLine($"nothing | {name} | ours: nothing read | port not run: the pitch instrument found no keyed window");
                continue;
            }

            var given = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
            var port = DrivePort(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate), given);

            _output.WriteLine(string.Create(Invariant,
                $"nothing | {name} | ours: nothing read | port given {given:0.0} Hz over {windows.Count} windows | port `{port.Raw}` | no key, not scored"));
        }

        Assert.Equal(WhatTheStrayLettersRestOnTests.KeyedRecordings.Count + SyntheticCq.All.Count, Rows.Count);
    }

    private void AlignedLines(string name, int stretch, string decoder, Decoded run, TheRequirementsAreMeasuredTests.Measured m)
    {
        var covered = TheRequirementsAreMeasuredTests.Covered(run.Settled, m.Scores[stretch]).Where(c => !c.IsWordGap).ToList();
        var line = new System.Text.StringBuilder();
        var departures = new List<string>();
        var d = 0;

        foreach (var step in m.Stretches[stretch].Steps)
        {
            var gap = step.Decoded is not null && step.GapBefore ? " " : string.Empty;
            string token;

            if (step.Decoded is not { } symbol)
            {
                token = $"[-{step.Key}]";
                line.Append(token);
                continue;
            }

            var character = covered[d++];
            var note = run.Notes[IndexOf(run.Settled, character)];

            token = symbol.Class == CwSymbolClass.Placeholder ? (step.Key is null ? "[+■]" : $"[{step.Key}>■]")
                : step.Key is null ? $"[+{symbol.Text}]"
                : step.Key == symbol.Text ? symbol.Text
                : $"[{step.Key}>{symbol.Text}]";

            line.Append(gap).Append(token);

            if (token != symbol.Text)
            {
                departures.Add($"{token} {note}");
            }
        }

        _output.WriteLine($"win | {name} | {stretch + 1} | {decoder} | {line}");

        foreach (var departure in departures)
        {
            _output.WriteLine($"win | {name} | {stretch + 1} | {decoder} departs | {departure}");
        }
    }

    private static void ConditionRow(System.Text.StringBuilder md, string label, IReadOnlyList<Row> rows)
    {
        var o = Totals.Of(rows.Select(r => (TheRequirementsAreMeasuredTests.Measured?)r.OursMeasured).ToList());
        var p = Totals.Of(rows.Select(r => r.PortMeasured).ToList());
        var kinds = string.Join(" and ", rows.Select(r => CwMetrics.KindWord(r.Kind)).Distinct());

        md.Append($"| {label} | {kinds} | ours {o.Measured} of {o.Recordings}, port {p.Measured} of {p.Recordings} | {CerSure(o)} | {CerSure(p)} | ");
        md.Append($"{InventedText(o)} | {InventedText(p)} | {CoverageText(o)} | {CoverageText(p)} | {Wbe(o)} | {Wbe(p)} |\n");
    }

    private void SpanLines(string name, string decoder, Decoded run, TheRequirementsAreMeasuredTests.Measured m)
    {
        if (m.NotComputable is { } why)
        {
            _output.WriteLine($"span | {name} | {decoder} | none | no number: {why}");
            return;
        }

        for (var i = 0; i < m.Scores.Count; i++)
        {
            var covered = CoveredWithTimes(run, m.Scores[i]).ToList();
            var key = m.Scores[i].Key;
            var text = m.Scores[i].Region;

            _output.WriteLine(covered.Count == 0
                ? $"span | {name} | {decoder} | {i + 1} | `{key}` | `{text}` | nothing covered"
                : $"span | {name} | {decoder} | {i + 1} | `{key}` | `{text}` | {S(covered[0].Emitted)} | {S(covered[^1].Emitted)} | "
                  + $"{S(covered[0].Character.At.TotalSeconds)} | {S(covered[^1].Character.At.TotalSeconds)}");
        }
    }

    private void Latency(string caseName)
    {
        var recipe = SyntheticCq.All.Single(c => c.Name == caseName);
        var row = Rows.Single(r => r.Name == caseName);
        var ends = KeyCharacterEnds(recipe);

        _output.WriteLine($"latency | {caseName} | put out minus the end of the character's last mark, for each character read sure and right");
        _output.WriteLine("latency | decoder | key index | character | mark ends s | put out s | latency s");

        var medians = new List<string>();

        var both = new (string Decoder, Decoded? Run, TheRequirementsAreMeasuredTests.Measured? Measured)[]
        {
            ("ours", row.Ours, row.OursMeasured),
            ("port", row.Port, row.PortMeasured),
        };

        foreach (var (decoder, run, m) in both)
        {
            if (run is null || m is null || m.NotComputable is not null)
            {
                _output.WriteLine($"latency | {decoder} | not measured");
                continue;
            }

            var covered = CoveredWithTimes(run, m.Scores[0]).Where(c => !c.Character.IsWordGap).ToList();
            var latencies = new List<double>();
            var d = 0;

            foreach (var step in m.Stretches[0].Steps)
            {
                if (step.Decoded is not { } symbol)
                {
                    continue;
                }

                var (character, emitted) = covered[d++];

                if (symbol.Class == CwSymbolClass.Sure && step.Key is { } key && key == symbol.Text)
                {
                    var end = ends[step.KeyBefore];
                    latencies.Add(emitted - end);
                    _output.WriteLine($"latency | {decoder} | {step.KeyBefore} | {character.Text} | {S(end)} | {S(emitted)} | {S(emitted - end)}");
                }
            }

            var median = latencies.Count == 0 ? double.NaN : latencies.OrderBy(l => l).ElementAt(latencies.Count / 2);
            medians.Add(string.Create(Invariant, $"{decoder} median {median:0.000} s over {latencies.Count}"));
        }

        _output.WriteLine($"latency | {caseName} | {string.Join(" | ", medians)}");
    }
}
