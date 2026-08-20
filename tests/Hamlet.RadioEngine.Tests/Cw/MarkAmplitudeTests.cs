using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How loud each mark was, on recordings where it is known which marks are the
/// station's (HM-DEC-144).
/// </summary>
/// <remarks>
/// <para>**THE QUESTION THIS MEASURES.** A mark of about two dits is either a dah
/// from a fist that runs its elements together or two elements the gate joined,
/// and no measurement of its length will say which. The candidate under test is
/// that they differ physically even where they do not differ in length: a sliver
/// the gate chopped out of band noise is a threshold crossing and should sit near
/// the detection floor, while a real keyed mark should sit on the sender's own
/// plateau.</para>
/// <para>**IT REPORTS HEIGHTS AND DECIDES NOTHING.** No threshold is introduced
/// and no mark is classified. The tests assert only that the measurement runs and
/// stays inside sane bounds; what the numbers mean is in `OUTPUT.md` and is a
/// ruling rather than an assertion.</para>
/// <para>**THE ENVELOPE IS COMPUTED HERE AND SHARES NO CODE WITH THE GATE**
/// (§12.5). The gate's own threshold decision is what put these marks where they
/// are, so measuring their height with the gate's machinery would be asking the
/// instrument to grade itself. Quadrature mixdown, a 10 ms boxcar, sampled every
/// millisecond, which is the same shape as the keying meter's and independent of
/// the decoder.</para>
/// </remarks>
public sealed class MarkAmplitudeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public MarkAmplitudeTests(ITestOutputHelper output) => _output = output;

    /// <summary>One mark the gate produced, with how loud it was.</summary>
    private readonly record struct Mark(
        double FromSeconds,
        double ToSeconds,
        double Milliseconds,
        double MedianDb,
        double PeakDb);

    private static MonoAudio Audio(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static MonoAudio Fixture(string name) => WavAudio.Read(
        Path.Combine(
            CapturedSignalTests.Folder, "..", "receiver", name + ".wav"));

    /// <summary>
    /// The envelope at one pitch, in decibels, sampled every millisecond.
    /// </summary>
    /// <remarks>
    /// A hundred hertz of smoothing is a ten millisecond window, and a boxcar of
    /// that length over the quadrature arms is a Goertzel of that bandwidth. The
    /// arms are accumulated in a ring so nothing is allocated per sample.
    /// </remarks>
    private static double[] Envelope(MonoAudio audio, double toneHz)
    {
        var rate = audio.SampleRate;
        var window = Math.Max(1, rate / 100);
        var step = Math.Max(1, rate / 1000);
        var omega = 2 * Math.PI * toneHz / rate;

        var ringCos = new double[window];
        var ringSin = new double[window];
        double inPhase = 0, quadrature = 0;

        var envelope = new double[(audio.Samples.Length / step) + 1];
        var written = 0;

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            var sample = audio.Samples[i];
            var angle = omega * i;
            var slot = i % window;
            var c = sample * Math.Cos(angle);
            var s = sample * -Math.Sin(angle);

            inPhase += c - ringCos[slot];
            quadrature += s - ringSin[slot];
            ringCos[slot] = c;
            ringSin[slot] = s;

            if (i % step != 0)
            {
                continue;
            }

            var magnitude =
                Math.Sqrt((inPhase * inPhase) + (quadrature * quadrature)) / window;

            envelope[written++] = 20 * Math.Log10(Math.Max(magnitude, 1e-12));
        }

        Array.Resize(ref envelope, written);

        return envelope;
    }

    /// <summary>
    /// Every mark the gate produced, with the envelope's median and peak inside
    /// it.
    /// </summary>
    /// <remarks>
    /// **THE MEDIAN LEADS AND THE PEAK IS BESIDE IT.** A plateau is what a keyed
    /// mark has, and the median of the envelope inside a mark is a plateau's own
    /// height, defended against the rising and falling edges the smoother rounds
    /// off. The peak is reported too because a mark shorter than the smoother's
    /// own window never reaches its plateau at all, so a median alone would
    /// depress every short mark whatever it was, which is the confound this
    /// measurement has to be read against.
    /// </remarks>
    private static List<Mark> Marks(MonoAudio audio, double startHz, double toneHz)
    {
        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var hop = decoder.Tracker.HopSamples;
        var envelope = Envelope(audio, toneHz);
        var boundaries = new List<(double At, bool Mark)>();
        var seen = 0;
        var lastAt = 0.0;
        var marks = new List<Mark>();

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            var ends = at / (double)audio.SampleRate;
            var isMark = boundaries.Count % 2 == 0;

            boundaries.Add((ends, isMark));

            if (isMark)
            {
                var from = (int)Math.Round(lastAt * 1000);
                var to = Math.Min(envelope.Length - 1, (int)Math.Round(ends * 1000));

                if (to > from)
                {
                    var inside = envelope[from..(to + 1)];

                    marks.Add(new Mark(
                        lastAt,
                        ends,
                        (ends - lastAt) * 1000,
                        inside.OrderBy(v => v).ElementAt(inside.Length / 2),
                        inside.Max()));
                }
            }

            lastAt = ends;
            seen = decoder.Report.ElementsSeen;
        }

        return marks;
    }

    /// <summary>The tenth percentile of the envelope, which is what quiet is.</summary>
    private static double Floor(MonoAudio audio, double toneHz, double from, double to)
    {
        var envelope = Envelope(audio, toneHz);
        var slice = envelope[
            (int)Math.Round(from * 1000)..Math.Min(envelope.Length, (int)Math.Round(to * 1000))];

        return slice.OrderBy(v => v).ElementAt(slice.Length / 10);
    }

    /// <summary>The pitch the independent instrument chooses over a stretch.</summary>
    private static double Pitch(MonoAudio audio, double from, double to)
    {
        var start = (int)Math.Round(from * audio.SampleRate);
        var length = Math.Min(
            (int)Math.Round((to - from) * audio.SampleRate),
            audio.Samples.Length - start);
        var slice = new float[length];

        Array.Copy(audio.Samples, start, slice, 0, length);

        return KeyingEnvelope.Best(new MonoAudio(audio.SampleRate, slice))?.ToneHz
               ?? throw new InvalidOperationException("no pitch");
    }

    private void Table(
        string title,
        MonoAudio audio,
        double startHz,
        double from,
        double to,
        Func<Mark, bool>? known)
    {
        var pitch = Pitch(audio, from, to);
        var floor = Floor(audio, pitch, from, to);
        var all = Marks(audio, startHz, pitch);
        var window = all.Where(m => m.ToSeconds >= from && m.ToSeconds <= to).ToList();

        _output.WriteLine($"===== {title} =====");
        _output.WriteLine(
            $"pitch {pitch:0} Hz (swept), envelope floor {floor:0.0} dB, "
            + $"{window.Count} marks between {from:0.00} s and {to:0.00} s");
        _output.WriteLine(
            "  start      len   median   peak  above floor (med/peak)   station?");

        foreach (var mark in window)
        {
            var isKnown = known?.Invoke(mark);

            _output.WriteLine(
                $"  {mark.FromSeconds,7:0.00}s {mark.Milliseconds,5:0} ms "
                + $"{mark.MedianDb,7:0.0} {mark.PeakDb,7:0.0}   "
                + $"{mark.MedianDb - floor,6:0.0} / {mark.PeakDb - floor,6:0.0}      "
                + (isKnown is null ? "-" : isKnown.Value ? "YES" : "no"));
        }

        if (known is null)
        {
            Summarize(window, floor, "all marks");
            return;
        }

        Summarize(window.Where(m => known(m)).ToList(), floor, "station");
        Summarize(window.Where(m => !known(m)).ToList(), floor, "not station");

        var stationLow = window.Where(m => known(m)).Min(m => m.PeakDb);
        var otherHigh = window.Where(m => !known(m)).Max(m => m.PeakDb);

        _output.WriteLine(
            $"  SEPARATION on peak: quietest station mark {stationLow - floor:0.0} dB "
            + $"above floor, loudest other {otherHigh - floor:0.0} dB, "
            + $"gap {stationLow - otherHigh:0.0} dB");

        var stationLowMed = window.Where(m => known(m)).Min(m => m.MedianDb);
        var otherHighMed = window.Where(m => !known(m)).Max(m => m.MedianDb);

        _output.WriteLine(
            $"  SEPARATION on median: quietest station mark {stationLowMed - floor:0.0} dB "
            + $"above floor, loudest other {otherHighMed - floor:0.0} dB, "
            + $"gap {stationLowMed - otherHighMed:0.0} dB");
    }

    private void Summarize(IReadOnlyList<Mark> marks, double floor, string what)
    {
        if (marks.Count == 0)
        {
            _output.WriteLine($"  {what}: none");
            return;
        }

        _output.WriteLine(
            $"  {what}: {marks.Count} marks, lengths {marks.Min(m => m.Milliseconds):0}"
            + $" to {marks.Max(m => m.Milliseconds):0} ms, "
            + $"median height {marks.Average(m => m.MedianDb) - floor:0.0} dB, "
            + $"peak height {marks.Average(m => m.PeakDb) - floor:0.0} dB");
    }

    /// <remarks>
    /// <para>Task 1: the twenty most recent marks at the moment `N4L` is being
    /// sent. HM-DEC-144 settles which eleven of them are the station's, so the
    /// answer exists before the measurement.</para>
    /// </remarks>
    [Fact]
    public void TheCallsignWindowOnTheRecordingWhereTheAnswerIsKnown()
    {
        var audio = Audio("cw-2026-08-17-134712");

        // The twenty marks ending with the callsign's last element. Everything
        // between 21.45 s and 23.01 s is N4L (HM-DEC-144); everything before it
        // in the window is not.
        var all = Marks(audio, 500, Pitch(audio, 21.3, 23.1));
        var last = all.FindIndex(m => m.ToSeconds > 23.0);
        var window = all.Skip(Math.Max(0, last - 19)).Take(20).ToList();

        Table(
            "cw-2026-08-17-134712, the twenty marks ending with N4L",
            audio,
            500,
            window[0].FromSeconds - 0.01,
            window[^1].ToSeconds + 0.01,
            // **THE CALLSIGN'S OPENING DAH STARTS BEFORE THE STRETCH IT ENDS
            // IN.** HM-DEC-144 records the elements as running 21.45 s to
            // 23.01 s, and those are the moments each element *ended*: the first
            // dah is 225 ms long and begins at 21.23. Keyed on the start it is
            // labelled as not the station's, which is wrong and would have made
            // the separation below look like an overlap.
            m => m.ToSeconds >= 21.4 && m.ToSeconds <= 23.02);

        Assert.NotEmpty(window);
    }

    /// <remarks>
    /// <para>**WHETHER THE GAP SURVIVES A WEAKER STATION, MEASURED RATHER THAN
    /// REASONED.** The separation on `cw-2026-08-17-134712` is measured at one
    /// signal-to-noise ratio and one only, and a margin quoted from a single
    /// strong recording is the shape of claim this project has been caught by
    /// before. So the recording is buried in shaped noise, a few decibels at a
    /// time, and the same two groups are measured again at each step.</para>
    /// <para>The noise is band-shaped rather than flat for the reason
    /// `ACarrierNeverConvincesTheTrackerItIsAStation` records: a receiver hands
    /// the decoder what got through its own filter, and flat noise is a fact
    /// about the fixture rather than about this radio.</para>
    /// </remarks>
    /// <param name="addedDb">How much noise is mixed in, relative to the audio.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-20)]
    [InlineData(-14)]
    [InlineData(-10)]
    [InlineData(-6)]
    public void TheSeparationUnderNoise(int addedDb)
    {
        var clean = Audio("cw-2026-08-17-134712");
        var samples = (float[])clean.Samples.Clone();

        if (addedDb != 0)
        {
            var random = new Random(7300);
            var scale = Math.Pow(10, addedDb / 20.0);
            var band = 0.0;

            for (var i = 0; i < samples.Length; i++)
            {
                band = (0.965 * band) + (0.035 * ((random.NextDouble() - 0.5) * 2));
                samples[i] = (float)(samples[i] + (band * 3 * scale));
            }
        }

        var audio = new MonoAudio(clean.SampleRate, samples);

        Table(
            $"cw-2026-08-17-134712 with noise at {addedDb} dB",
            audio,
            500,

            // The same span task 1 uses, so the number of chatter marks is
            // comparable at every noise level. A window holding one of them
            // cannot say what the loudest one is.
            20.2,
            23.1,
            m => m.ToSeconds >= 21.4 && m.ToSeconds <= 23.02);

        Assert.True(samples.Length > 0);
    }

    /// <remarks>
    /// Task 2: the same measurement where the short marks are real keyed elements
    /// rather than chatter. If amplitude is a discriminator this must look
    /// different; if it looks the same, amplitude does not work.
    /// </remarks>
    [Fact]
    public void TheTightFistWhereTheShortMarksAreReal()
    {
        var audio = Fixture("tightfist-easy");

        Table(
            "tightfist-easy, every mark",
            audio,
            600,
            0,
            audio.Duration.TotalSeconds,
            null);

        Assert.True(audio.Samples.Length > 0);
    }

    /// <remarks>
    /// Task 2's control: a recording that decodes, so the table has something
    /// ordinary to be read against.
    /// </remarks>
    [Fact]
    public void TheControlThatDecodes()
    {
        var audio = Audio("cw-2026-08-18-004507");

        Table("cw-2026-08-18-004507, marks in the first ten seconds", audio, 600, 0, 10, null);

        Assert.True(audio.Samples.Length > 0);
    }
}
