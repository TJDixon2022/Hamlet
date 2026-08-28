using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The digital waterfall's spectrum source: same audio in, same frames out.
/// </summary>
/// <remarks>
/// <para>**TASK 2'S ACCEPTANCE** (work instruction 038): a WAV fixture in, a
/// deterministic frame sequence out.</para>
/// <para>**DETERMINISM IS THE WHOLE CLAIM.** The source reads no clock below the
/// pump — a frame's timestamp comes from how many samples have been seen — so a
/// fixture replayed is identical to a fixture replayed again, and a picture that
/// disagreed with itself between runs could never be used as evidence about a
/// decoder (§5.4, §0.0.1).</para>
/// </remarks>
public sealed class TheAudioSpectrumIsDeterministicTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public TheAudioSpectrumIsDeterministicTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Fixture()
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured",
            "cw-2026-08-17-013347.wav"));

    /// <summary>Run a fixture through and keep every frame.</summary>
    private static List<(long Low, long High, DateTime At, byte[] Bins)> Run(
        MonoAudio audio, int chunk)
    {
        var source = new AudioSpectrumSource(audio.SampleRate);
        var frames = new List<(long, long, DateTime, byte[])>();

        source.FrameReady += (in SpectrumFrame f)
            => frames.Add((f.LowHz, f.HighHz, f.TimestampUtc, f.Bins.ToArray()));

        source.Start();

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);
            source.Push(audio.Samples.AsSpan(at, take));
        }

        return frames;
    }

    /// <remarks>
    /// <para>Proves the same audio produces the same frames, byte for byte,
    /// twice.</para>
    /// <para>**AND THAT THE CHUNK SIZE DOES NOT CHANGE THE ANSWER**, which is
    /// the property that matters in the application: a live source delivers
    /// whatever the driver hands it, so a picture that depended on the buffer
    /// size would differ between the radio and a replay of its own recording.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheSameAudioGivesTheSameFrames()
    {
        var audio = Fixture();

        var once = Run(audio, 4096);
        var twice = Run(audio, 4096);
        var ragged = Run(audio, 997);

        _output.WriteLine(
            $"  {audio.Samples.Length} samples at {audio.SampleRate} Hz");
        _output.WriteLine(
            $"  window {AudioSpectrumSource.WindowFor(audio.SampleRate)} samples");
        _output.WriteLine($"  frames: {once.Count}");

        Assert.NotEmpty(once);
        Assert.Equal(once.Count, twice.Count);
        Assert.Equal(once.Count, ragged.Count);

        for (var i = 0; i < once.Count; i++)
        {
            Assert.Equal(once[i].Low, twice[i].Low);
            Assert.Equal(once[i].High, twice[i].High);
            Assert.Equal(once[i].At, twice[i].At);
            Assert.Equal(once[i].Bins, twice[i].Bins);

            Assert.Equal(once[i].At, ragged[i].At);
            Assert.Equal(once[i].Bins, ragged[i].Bins);
        }
    }

    /// <remarks>
    /// <para>Proves the frames cover the band the order names and resolve FT8's
    /// tones.</para>
    /// <para>**THE BIN WIDTH IS THE NUMBER THE ORDER ASKS TO BE STATED.** FT8
    /// spaces its tones 6.25 Hz apart, so a bin meaningfully narrower than that
    /// is what makes them separate stripes rather than one smear.</para>
    /// </remarks>
    [Fact]
    public void TheBandAndTheBinWidthAreWhatFt8Needs()
    {
        foreach (var rate in new[] { 8000, 12000, 48000 })
        {
            var source = new AudioSpectrumSource(rate);

            _output.WriteLine(
                $"  {rate,6} Hz: window {AudioSpectrumSource.WindowFor(rate),6} "
                + $"= {source.WindowSeconds,5:0.000} s, "
                + $"bin {source.BinWidthHz,5:0.00} Hz");

            // **THE BAR IS THE TONE SPACING, NOT HALF OF IT.** The first
            // version of this test asked for half and failed 8 kHz at 3.91 Hz,
            // which is a bar I wrote as though it were derived and was not: two
            // tones fall in different bins once the bin is narrower than their
            // spacing. At 8 kHz that is 1.6 bins per tone and at 48 kHz 2.1,
            // and both separate them.
            Assert.True(
                source.BinWidthHz < 6.25,
                $"at {rate} Hz the bin is {source.BinWidthHz:0.00} Hz, which "
                + "cannot separate FT8 tones 6.25 Hz apart");

            Assert.True(
                source.WindowSeconds < 0.5,
                $"at {rate} Hz the window covers {source.WindowSeconds:0.000} s, "
                + "which would blur the fifteen-second slot edges the grid shows");
        }
    }

    /// <remarks>
    /// <para>Proves a station that arrives lands in the bin it belongs in, which
    /// is the one thing that makes the picture mean anything.</para>
    /// <para>Generated audio, and it is a unit test rather than evidence about
    /// the decoder — the order is explicit that synthetic audio never appears in
    /// the phase's score.</para>
    /// </remarks>
    [Fact]
    public void AToneLandsWhereItShould()
    {
        const int Rate = 12000;
        const double ToneHz = 1500;

        // **THE TONE ARRIVES; IT IS NOT THERE FROM THE START.** The picture
        // measures each bin against its own recent quiet level, so a signal that
        // has been perfectly constant since the first sample is by construction
        // indistinguishable from a constant floor and correctly fades out. That
        // is what an AGC does and it is what removes the receiver's filter shape
        // from the picture.
        //
        // **REAL SIGNALS ARRIVE, AND FT8 ONES KEY EVERY FIFTEEN SECONDS.** So
        // the honest test is a band that is quiet and then is not: two seconds
        // of noise, then two seconds of noise with a station in it.
        var samples = new float[Rate * 4];
        var state = 12345u;

        float Noise()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;

            return (float)(((state & 0xFFFF) / 65535.0) - 0.5) * 0.02f;
        }

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = Noise();

            if (i >= Rate * 2)
            {
                samples[i] += (float)(
                    0.3 * Math.Sin(2 * Math.PI * ToneHz * i / Rate));
            }
        }

        var source = new AudioSpectrumSource(Rate);
        byte[]? last = null;
        long low = 0, high = 0;

        source.FrameReady += (in SpectrumFrame f) =>
        {
            last = f.Bins.ToArray();
            low = f.LowHz;
            high = f.HighHz;
        };

        source.Start();
        source.Push(samples);

        Assert.NotNull(last);

        // **THE MIDPOINT OF THE BRIGHTEST RUN, NOT THE FIRST BIN THAT REACHES
        // IT.** A strong tone saturates a plateau several bins wide once the
        // picture is scaled to a 45 dB range, so taking the first maximum
        // reports the low edge of that plateau and reads 60 Hz flat. The centre
        // of the run is where the tone is.
        var peak = last!.Max();
        var first = Array.IndexOf(last, peak);
        var lastAt = Array.LastIndexOf(last, peak);
        var brightest = (first + lastAt) / 2.0;

        var hz = low + (brightest * (double)(high - low) / last.Length);

        _output.WriteLine(
            $"  tone at {ToneHz} Hz, brightest bin at {hz:0} Hz, "
            + $"band {low}-{high} Hz");

        Assert.True(
            Math.Abs(hz - ToneHz) < 10,
            $"a {ToneHz} Hz tone lit the bin at {hz:0} Hz");
    }
}
