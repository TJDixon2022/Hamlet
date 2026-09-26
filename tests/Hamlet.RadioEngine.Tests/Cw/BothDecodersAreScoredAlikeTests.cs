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
    internal sealed record Decoded(
        IReadOnlyList<CwCharacter> Settled, IReadOnlyList<double> EmittedSeconds, string Raw, TimeSpan DecodeTime);

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

        return (new Decoded(settled, emitted, CwReading.Of(settled).Text, watch.Elapsed), pitch);
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
            watch.Elapsed);
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
