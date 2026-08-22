using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// When the decoder lets go of the audio it is holding, and what survives it.
/// </summary>
/// <remarks>
/// <para>**THE CORPUS CANNOT EXERCISE THIS AND THAT IS NOT A REASON TO SHIP IT
/// UNPROVED.** Four of the recordings hold a single sender, and the one fixture
/// built to hold two reaches the second through the tracker's acquiring branch,
/// so the trigger fires nowhere in this repository. The decision and the emptying
/// are therefore proved separately and directly: the line is a function of two
/// pitches and whether anybody was being read, and the emptying is a method on
/// the stream.</para>
/// <para>**WHY IT EXISTS.** The stream keeps twelve seconds of envelope and the
/// decoder fits one speed and one stream of characters across all of it. When the
/// tracker crosses to somebody else part-way through, the reading afterwards is
/// made over two people at once and comes out as clean-looking letters neither of
/// them sent, at the exact moment somebody answers a call (HM-DEC-009).</para>
/// </remarks>
public sealed class WhenTheWindowIsEmptiedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public WhenTheWindowIsEmptiedTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the ruled line: a move of at least the decoder's own filter width,
    /// made while somebody was being read.
    /// </remarks>
    [Fact]
    public void AMoveWiderThanTheFilterWhileReadingEmptiesIt()
    {
        var width = CwProbabilisticDecoder.BandwidthHz;

        _output.WriteLine($"the decoder listens through {width:0} Hz");

        Assert.True(CwDecoder.ShouldClearWindow(500, 500 + width, reading: true));
        Assert.True(CwDecoder.ShouldClearWindow(700, 700 - width, reading: true));
        Assert.True(CwDecoder.ShouldClearWindow(500, 900, reading: true));
    }

    /// <remarks>
    /// <para>Proves the half that keeps the window: a move inside the passband
    /// the held audio was already taken through cannot have put a different
    /// sender in it, and settling onto one station in two steps moves 40 to 50
    /// hertz.</para>
    /// </remarks>
    [Fact]
    public void ASmallerMoveDoesNot()
    {
        var width = CwProbabilisticDecoder.BandwidthHz;

        Assert.False(CwDecoder.ShouldClearWindow(600, 650, reading: true));
        Assert.False(CwDecoder.ShouldClearWindow(475, 525, reading: true));
        Assert.False(
            CwDecoder.ShouldClearWindow(500, 500 + width - 1, reading: true));
    }

    /// <remarks>
    /// <para>Proves the other half: Hamlet hunting for a station it has not found
    /// yet moves a long way and leaves nobody behind. On
    /// `cw-2026-08-18-004507` it goes 600 to 475 hertz in the first two seconds
    /// with nothing read, and emptying the window there throws away the opening
    /// of the message it is about to read.</para>
    /// </remarks>
    [Fact]
    public void ALongMoveWithNobodyBeingReadDoesNot()
    {
        Assert.False(CwDecoder.ShouldClearWindow(600, 475, reading: false));
        Assert.False(CwDecoder.ShouldClearWindow(600, 900, reading: false));
        Assert.False(CwDecoder.ShouldClearWindow(double.NaN, 900, reading: true));
    }

    /// <remarks>
    /// <para>Proves what emptying costs and what it must not cost: the held audio
    /// and the leading edge go, and **nothing already settled is retracted or
    /// said twice.** Characters read before the move are what the operator has
    /// already written down.</para>
    /// </remarks>
    [Fact]
    public void EmptyingDropsTheEdgeAndKeepsWhatWasSettled()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K",
            WordsPerMinute: 18,
            ToneHz: 600,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            Seed: 4));

        var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        var settled = new List<string>();
        var edges = new List<int>();

        stream.CharacterSettled += c => settled.Add(c.Text);
        stream.LeadingEdgeChanged += e => edges.Add(e.Count);

        stream.Process(audio.Samples);

        var before = string.Concat(settled);
        var settledBefore = stream.SettledCharacters;

        Assert.NotEqual(0, settledBefore);
        Assert.NotEqual(0, stream.EnvelopeHops);

        stream.Restart();

        _output.WriteLine($"settled before the move: '{before}'");
        _output.WriteLine($"envelope after: {stream.EnvelopeHops} hops");
        _output.WriteLine($"last reading after: '{stream.Last.Text}'");

        // The held audio and the tip both go.
        Assert.Equal(0, stream.EnvelopeHops);
        Assert.Equal("", stream.Last.Text);
        Assert.Equal(0, edges[^1]);

        // And nothing already said is taken back or said again.
        Assert.Equal(settledBefore, stream.SettledCharacters);
        Assert.Equal(before, string.Concat(settled));
    }
}
