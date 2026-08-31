using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Work instruction 056, task 2: every element the decoder resolves carries the
/// frequency measured over that element's own samples.
/// </summary>
/// <remarks>
/// <para>**THE ACCEPTANCE IS THAT NOTHING MOVES.** This adds a measurement and
/// not a decision, so the corpus score has to be identical to the digit — which
/// is checked outside this file, against the corpus itself. What is checked here
/// is that the measurement is a measurement: that it recovers a frequency nobody
/// told it, at about the resolution the element's own length allows, and that it
/// refuses rather than guessing where it cannot.</para>
/// <para>**THE TONES ARE SYNTHESIZED AND THAT IS THE POINT.** A real capture
/// cannot say what its own elements were sent at, so it cannot test an
/// instrument's accuracy — only its self-consistency. These fixtures know the
/// answer to the hertz. HM-DEC-091 is not weakened: nothing here claims anything
/// about the corpus or about a station, and every claim about real audio in this
/// unit is measured on real audio elsewhere.</para>
/// </remarks>
public sealed class EveryElementCarriesItsOwnPitchTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public EveryElementCarriesItsOwnPitchTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>A steady tone of a stated length.</summary>
    private static float[] Tone(double hz, double milliseconds, double amplitude = 0.5)
    {
        var count = (int)(Rate * milliseconds / 1000.0);
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = (float)(amplitude * Math.Sin(2 * Math.PI * hz * i / Rate));
        }

        return samples;
    }

    /// <summary>
    /// A dah is long enough to resolve a few hertz, and the measurement gets
    /// there.
    /// </summary>
    /// <remarks>
    /// 190 ms resolves to about 5 Hz (<see cref="CwElementPitch.ResolutionHz"/>),
    /// and the tolerance asserted is that figure rather than something tighter:
    /// a test that demanded better than the audio can carry would be asserting
    /// the interpolator's luck.
    /// </remarks>
    [Theory]
    [InlineData(583.5)]
    [InlineData(600.0)]
    [InlineData(615.0)]
    [InlineData(628.25)]
    public void ADahResolvesToAboutFiveHertz(double hz)
    {
        var samples = Tone(hz, 190);
        var measured = CwElementPitch.Measure(samples, Rate, 600, 0, samples.Length);
        var allowed = CwElementPitch.ResolutionHz(190);

        _output.WriteLine(
            $"sent {hz:0.00} Hz  read {measured:0.00} Hz  "
            + $"error {Math.Abs(measured - hz):0.00}  allowed {allowed:0.00}");

        Assert.False(double.IsNaN(measured), "a 190 ms dah must be measurable");
        Assert.True(
            Math.Abs(measured - hz) <= allowed,
            $"read {measured:0.00} for a tone at {hz:0.00}, "
            + $"which is further than {allowed:0.00} Hz");
    }

    /// <summary>A dit is short and the answer is correspondingly coarse.</summary>
    /// <remarks>
    /// **THE POINT OF THE TEST IS THE WIDTH OF THE TOLERANCE, NOT THE PASS.** 55
    /// ms resolves to about 18 Hz, so a dit cannot settle a 13 Hz separation on
    /// its own and task 3's split must not rest on one.
    /// </remarks>
    [Theory]
    [InlineData(590.0)]
    [InlineData(602.0)]
    [InlineData(615.0)]
    public void ADitResolvesToAboutEighteenHertz(double hz)
    {
        var samples = Tone(hz, 55);
        var measured = CwElementPitch.Measure(samples, Rate, 600, 0, samples.Length);
        var allowed = CwElementPitch.ResolutionHz(55);

        _output.WriteLine(
            $"sent {hz:0.00} Hz  read {measured:0.00} Hz  "
            + $"error {Math.Abs(measured - hz):0.00}  allowed {allowed:0.00}");

        Assert.False(double.IsNaN(measured), "a 55 ms dit must be measurable");
        Assert.True(
            Math.Abs(measured - hz) <= allowed,
            $"read {measured:0.00} for a tone at {hz:0.00}, "
            + $"which is further than {allowed:0.00} Hz");
    }

    /// <summary>
    /// An element too short to say anything says nothing, rather than saying the
    /// mixdown pitch.
    /// </summary>
    /// <remarks>
    /// **§0.0.** Returning the pitch it was pointed at would be a measurement
    /// that always agrees with the decoder and therefore never disagrees with it,
    /// which is worse than no measurement because it looks like corroboration.
    /// </remarks>
    [Fact]
    public void TooShortToMeasureSaysSoRatherThanGuessing()
    {
        var samples = Tone(615, 10);
        var measured = CwElementPitch.Measure(samples, Rate, 600, 0, samples.Length);

        _output.WriteLine($"10 ms element reads {measured}");

        Assert.True(double.IsNaN(measured), $"expected NaN, got {measured}");
    }

    /// <summary>A gap keeps NaN and is never given a frequency.</summary>
    [Fact]
    public void AGapIsNotMeasured()
    {
        var elements = new[]
        {
            new CwElement(true, 0, 10),
            new CwElement(false, 10, 20),
            new CwElement(true, 20, 30),
        };

        var samples = Tone(600, 400);

        var measured = CwElementPitch.MeasureAll(
            elements, samples, Rate, 600, CwProbabilisticDecoder.HopMilliseconds);

        Assert.True(double.IsNaN(measured[1].PitchHz), "a gap has no pitch");
        Assert.False(double.IsNaN(measured[0].PitchHz), "a mark does");
        Assert.False(double.IsNaN(measured[2].PitchHz), "a mark does");
    }

    /// <summary>
    /// Two senders thirteen hertz apart are separable on their dahs, and this is
    /// the measurement task 3 rests on.
    /// </summary>
    /// <remarks>
    /// **THE MARGIN IS STATED AND IT IS NOT COMFORTABLE.** Thirteen hertz against
    /// a dah's own 5 Hz resolution is about two and a half resolutions, which is
    /// enough; against a dit's 18 Hz it is less than one, which is not. That
    /// asymmetry is the whole reason a split has to be conservative.
    /// </remarks>
    [Fact]
    public void TwoSendersThirteenHertzApartAreSeparableOnTheirDahs()
    {
        var lower = CwElementPitch.Measure(Tone(602, 190), Rate, 600, 0, (int)(Rate * 0.190));
        var upper = CwElementPitch.Measure(Tone(615, 190), Rate, 600, 0, (int)(Rate * 0.190));

        var apart = Math.Abs(upper - lower);
        var resolution = CwElementPitch.ResolutionHz(190);

        _output.WriteLine(
            $"602 reads {lower:0.00}, 615 reads {upper:0.00}, "
            + $"apart {apart:0.00} against a resolution of {resolution:0.00}");

        Assert.True(
            apart > resolution,
            $"{apart:0.00} Hz apart does not clear {resolution:0.00} Hz");
    }

    /// <summary>
    /// The decoder's own element stream comes out with the text, and the marks in
    /// it match the letters read.
    /// </summary>
    /// <remarks>
    /// **THE STREAM IS PRODUCED BY THE WALK THAT SPELLS THE TEXT**, so a mark
    /// count that disagreed with the pattern would mean the two had come apart.
    /// Checked on generated Morse, where the pattern is known.
    /// </remarks>
    [Fact]
    public void TheElementStreamComesOutWithTheText()
    {
        var audio = WavAudio.Read(Path.Combine(
            CwFixtures.Folder, "clean-18wpm.wav"));

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, 600);

        var read = CwProbabilisticDecoder.Decode(envelope, 600, null, null, false);

        var marks = read.Elements.Count(e => e.IsMark);
        var expected = read.Characters.Sum(c => c.Pattern.Length);

        _output.WriteLine(
            $"read \"{read.Text}\"  {read.Elements.Count} elements, "
            + $"{marks} marks against {expected} from the patterns");

        Assert.NotEmpty(read.Elements);
        Assert.Equal(expected, marks);
    }
}
