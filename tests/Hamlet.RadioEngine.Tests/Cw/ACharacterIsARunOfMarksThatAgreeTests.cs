using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A character is a run of marks that agree on pitch and amplitude (work instruction 490, R103,
/// HM-DEC-195): the timing-only path beside the run path, on three synthetic signals.
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). Each case prints what each path
/// reads. The old path is the decoder as work instruction 489 left it, unbound; the new path is
/// the same decoder given the detector's marks.
/// </remarks>
public sealed class ACharacterIsARunOfMarksThatAgreeTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";
    private const string Answer = "TEST DE W1AW K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each path's text is printed.</param>
    public ACharacterIsARunOfMarksThatAgreeTests(ITestOutputHelper output) => _output = output;

    private static MonoAudio Station(string text, int wpm, double hz, double amplitude, double noise, double lead, int seed)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: hz, SampleRate: Rate, Amplitude: amplitude,
            NoiseAmplitude: noise, LeadInSeconds: lead, TailSeconds: 3, Seed: seed));

    /// <summary>The clean call: one pitch, one level, 23 words a minute, about 22 dB over the noise.</summary>
    private static float[] CleanCall() => Station(Call, 23, 625, 0.5, 0.04, 3, 490).Samples;

    /// <summary>
    /// The clean call with a short burst in the middle of every key-up stretch longer than a
    /// character gap: 40 ms each, eight decibels under the station, at pitches scattered from
    /// 550 to 675 Hz, inside the old path's filter.
    /// </summary>
    private static float[] CallWithBlips()
    {
        var samples = CleanCall();
        var keyed = Station(Call, 23, 625, 0.5, 0, 3, 490).Samples;
        var pitches = new[] { 575.0, 650, 600, 675, 550 };
        var burst = (int)(0.040 * Rate);
        var quietRun = 0;
        var placed = 0;

        for (var i = 0; i < keyed.Length; i++)
        {
            if (Math.Abs(keyed[i]) > 1e-4)
            {
                // A key-up stretch of 150 ms or more, before this mark and after the first mark, is a
                // gap between letters or words: a burst goes in its middle.
                if (quietRun >= (int)(0.150 * Rate) && i > (int)(3.2 * Rate))
                {
                    var middle = i - (quietRun / 2) - (burst / 2);
                    var hz = pitches[placed++ % pitches.Length];

                    for (var k = 0; k < burst; k++)
                    {
                        var shape = Math.Sin(Math.PI * k / burst);
                        samples[middle + k] += (float)(0.2 * shape * Math.Sin(2 * Math.PI * hz * k / Rate));
                    }
                }

                quietRun = 0;
            }
            else
            {
                quietRun++;
            }
        }

        return samples;
    }

    /// <summary>Two stations keying at once, 200 Hz apart: the call at 625 Hz and an answer at 825 Hz, quieter.</summary>
    private static float[] TwoStations()
    {
        var a = Station(Call, 23, 625, 0.5, 0.04, 3, 490).Samples;
        var b = Station(Answer, 18, 825, 0.3, 0, 3.4, 491).Samples;
        var mixed = new float[Math.Max(a.Length, b.Length)];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
        }

        return mixed;
    }


    /// <summary>What a path reads, character by character: the old path unbound, or the new path given the marks.</summary>
    internal static List<CwCharacter> ReadCharacters(float[] samples, bool runs)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600);
        var settled = new List<CwCharacter>();

        if (runs)
        {
            decoder.DetectorMarks = detector.MarksSince;
        }

        decoder.CharacterSettled += settled.Add;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, samples.AsSpan(at, Chunk)));
            detector.Process(samples.AsSpan(at, Chunk));
        }

        decoder.Flush();

        return settled;
    }

    /// <summary>What a path reads, as text with single spaces.</summary>
    internal static string Read(float[] samples, bool runs) => Text(ReadCharacters(samples, runs));

    private static string Text(IEnumerable<CwCharacter> characters)
        => string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Every mark the detector calls on the audio.</summary>
    private static IReadOnlyList<CwMark> Marks(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));
        }

        return detector.MarksSince(0).Marks;
    }

    /// <summary>
    /// The clean call's own marks: within a bin of 625 Hz and within 6 dB of the loudest there.
    /// Since work instruction 492 the detector hands out every bar, the noise's too, so the call
    /// is picked out of all the marks rather than being all of them.
    /// </summary>
    private static List<CwMark> CallsOwnMarks()
    {
        var near = Marks(CleanCall()).Where(m => Math.Abs(m.PitchHz - 625) <= CwRunReader.PitchToleranceHz).ToList();
        var loudest = near.Max(m => m.LevelDb);

        return near.Where(m => m.LevelDb >= loudest - 6).ToList();
    }

    private (string Old, string New) Both(string name, float[] samples, string sent)
    {
        var old = Read(samples, runs: false);
        var now = Read(samples, runs: true);

        _output.WriteLine($"{name}");
        _output.WriteLine($"  sent      `{sent}`");
        _output.WriteLine($"  old path  `{old}` ({old.Count(c => c != ' ')} characters)");
        _output.WriteLine($"  new path  `{now}` ({now.Count(c => c != ' ')} characters)");

        return (old, now);
    }

    /// <summary>
    /// The marks the run reader's printed letters were read from that are not the call's: at another pitch
    /// than 625 Hz by more than a bin, or at another level than the clean call's by more than the
    /// reader's own level tolerance.
    /// </summary>
    private int MarksNotTheCalls(float[] samples)
    {
        var clean = CallsOwnMarks();
        var callLevel = clean.Select(m => m.LevelDb).OrderBy(l => l).ElementAt(clean.Count / 2);
        var callContrast = clean.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).Average();
        var detector = new CwEnvelopeDetector(Rate);
        var reader = new CwRunReader();
        var sequence = 0L;
        var strangers = 0;
        var used = 0;

        reader.RunRead += (c, run) =>
        {
            foreach (var m in run)
            {
                used++;

                if (Math.Abs(m.PitchHz - 625) > CwRunReader.PitchToleranceHz
                    || Math.Abs(m.LevelDb - callLevel) > CwRunReader.LevelToleranceDb(callContrast))
                {
                    strangers++;
                    _output.WriteLine($"  not the call's: {m.FromSeconds:0.000} s, {m.PitchHz:0} Hz, {m.LevelDb:0.0} dB, in `{c.Text}`");
                }
            }
        };

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);
        }

        reader.Flush();
        _output.WriteLine($"  marks the printed letters were read from {used}, not the call's {strangers}");

        return strangers;
    }

    /// <remarks>Case 1: a clean call at one pitch and one level. The new path reads it as sent.</remarks>
    [Fact]
    public void ACleanCallReadsAsSent()
    {
        var (_, now) = Both("clean call", CleanCall(), Call);

        Assert.Equal(Call, now);
    }

    /// <remarks>
    /// Case 2: the same call with bursts between the letters, at another level and a scatter of
    /// pitches. They break the agreement, so no letter the new path prints stands on one. Red while
    /// it prints them - the first reader, which printed every run, printed four.
    /// </remarks>
    [Fact]
    public void BlipsBetweenTheLettersAreNotLetters()
    {
        Both("call with blips", CallWithBlips(), Call);

        Assert.Equal(0, MarksNotTheCalls(CallWithBlips()));
    }

    /// <remarks>
    /// **RED ON PURPOSE, AND NAMED** (work instructions 490 and 491): with the bursts in, the call
    /// reads as it reads without them. It does not, and not because a burst is read - none is. Work
    /// instruction 491 measured it: every one of the call's 65 marks is called, but some are called
    /// late, because the detector's check that paired bars clear their gaps' wander is taken over
    /// the last second and a burst in that second holds the pair back until it leaves; the run
    /// reader has ended the run by then and the letter splits - an L read as E and I, its third dit
    /// handed out 435 ms after it ended. The wander check itself refuses noise and stays.
    /// </remarks>
    [Fact]
    public void TheCallReadsWholeThroughTheBlips()
    {
        var clean = Read(CleanCall(), runs: true);

        Assert.Equal(clean, Read(CallWithBlips(), runs: true));
    }

    /// <remarks>
    /// Case 3: two stations at once, 200 Hz apart. The new path reads them as two senders and
    /// prints one of them, not a mixture: every mark under a printed letter is the call's.
    /// </remarks>
    [Fact]
    public void TwoStationsReadAsOneNotAMixture()
    {
        Both("two stations", TwoStations(), Call + "  |  " + Answer);

        Assert.Equal(0, MarksNotTheCalls(TwoStations()));
    }

    /// <remarks>
    /// **RED ON PURPOSE, AND NAMED** (work instructions 490 and 491): the one station printed reads
    /// whole. It does not: where the answer keys inside the call's lobe, the call's marks are
    /// called late or not at all (61 of 65 called), and the reader ends runs before late marks
    /// arrive, as in case 2.
    /// </remarks>
    [Fact]
    public void TheStationPrintedReadsWhole()
    {
        Assert.Equal(Call, Read(TwoStations(), runs: true));
    }

    /// <summary>Thirty seconds of loud noise over the whole band and no station.</summary>
    private static float[] NoiseAlone() => CwSignal.Generate(new CwSignalRequest(
        " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 491)).Samples;

    /// <remarks>
    /// Noise alone still makes nothing (work instruction 491): loud noise with no station, and the
    /// new path prints no character. Red if letting a bar pair across a bar at another level lets
    /// noise pair into runs.
    /// </remarks>
    [Fact]
    public void NoiseAloneReadsNothing()
    {
        var samples = NoiseAlone();
        var read = ReadCharacters(samples, runs: true).Where(c => !c.IsWordGap).ToList();

        _output.WriteLine($"noise alone: marks called {Marks(samples).Count}, characters printed {read.Count} `{Text(read)}`");

        Assert.Empty(read);
    }

    /// <remarks>
    /// Answers work instruction 491 section 6 and asserts nothing: how many of the clean call's marks the
    /// detector calls again - a mark within a bin of 625 Hz and the reader's level tolerance of the clean
    /// call's level, overlapping at least half of it - on each case, and how many others there are.
    /// </remarks>
    [Fact]
    public void HowManyOfTheCallsMarksAreCalled()
    {
        var clean = CallsOwnMarks();
        var callLevel = clean.Select(m => m.LevelDb).OrderBy(l => l).ElementAt(clean.Count / 2);
        var callContrast = clean.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).Average();

        foreach (var (name, samples) in new[] { ("clean call", CleanCall()), ("call with blips", CallWithBlips()), ("two stations", TwoStations()) })
        {
            var calls = Marks(samples).Where(m =>
                Math.Abs(m.PitchHz - 625) <= CwRunReader.PitchToleranceHz
                && Math.Abs(m.LevelDb - callLevel) <= CwRunReader.LevelToleranceDb(callContrast)).ToList();

            // A mark of the clean call is called when a mark of the call overlaps at least half of it.
            static double Overlap(CwMark x, CwMark y)
                => Math.Max(0, Math.Min(x.ToSeconds, y.ToSeconds) - Math.Max(x.FromSeconds, y.FromSeconds));

            var called = clean.Count(c => calls.Any(m => Overlap(c, m) >= (c.ToSeconds - c.FromSeconds) / 2));
            var extra = calls.Count(m => !clean.Any(c => Overlap(c, m) >= (c.ToSeconds - c.FromSeconds) / 2));

            _output.WriteLine($"{name}: of the clean call's {clean.Count} marks, called {called}; other marks at the call's pitch and level {extra}");
        }
    }

    /// <remarks>
    /// Case 4 of work instruction 492: a lone dit, or a lone dah, alone in silence, prints no letter.
    /// A lone bar, however clean, is not a character (R105).
    /// </remarks>
    /// <param name="text">E for the dit, T for the dah.</param>
    [Theory]
    [InlineData("E")]
    [InlineData("T")]
    public void ALoneDitOrDahPrintsNothing(string text)
    {
        var samples = Station(text, 23, 625, 0.5, 0.04, 3, 492).Samples;
        var read = ReadCharacters(samples, runs: true).Where(c => !c.IsWordGap).ToList();

        _output.WriteLine($"a lone `{text}`: marks called {Marks(samples).Count}, characters printed {read.Count} `{Text(read)}`");

        Assert.Empty(read);
    }

    /// <remarks>
    /// Answers work instruction 492 section 3 and asserts nothing: how long after a mark ends the
    /// detector hands it out, at worst, on each case - the mark's end against the audio the detector
    /// had heard when <see cref="CwEnvelopeDetector.MarksSince"/> first returned it.
    /// </remarks>
    [Fact]
    public void HowLateAMarkIsHandedOut()
    {
        foreach (var (name, samples) in new[] { ("clean call", CleanCall()), ("call with blips", CallWithBlips()), ("two stations", TwoStations()), ("noise alone", NoiseAlone()) })
        {
            var detector = new CwEnvelopeDetector(Rate);
            var sequence = 0L;
            var worst = 0.0;
            var count = 0;

            for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
            {
                detector.Process(samples.AsSpan(at, Chunk));

                var batch = detector.MarksSince(sequence);

                foreach (var m in batch.Marks)
                {
                    worst = Math.Max(worst, batch.HeardSeconds - m.ToSeconds);
                    count++;
                }

                sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            }

            _output.WriteLine($"{name}: marks {count}, worst delivery {worst * 1000:0} ms after the mark ended");
        }
    }
}
