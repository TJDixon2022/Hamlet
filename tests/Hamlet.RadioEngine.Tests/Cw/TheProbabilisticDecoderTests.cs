using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The decoder that never thresholds: what it reads, what it refuses, and what it
/// costs.
/// </summary>
/// <remarks>
/// <para>**PORTED FROM `tools/reference-decoder/reference_decoder.py`**, which is
/// in this repository so the port had an implementation to be checked against
/// rather than a description in a work order. The strings below were produced by
/// the Python first and are reproduced by this port; where they differ it is
/// because Hamlet's alphabet renders `-...-` as the prosign it is rather than as
/// `=`.</para>
/// <para>**NOTHING HERE IS AN ANSWER KEY** (§12.5). No adjudicated truth exists
/// for any of these recordings and a session may not write one. What is asserted
/// is agreement between two independent implementations, and the one property
/// that must hold whatever the text says.</para>
/// </remarks>
public sealed class TheProbabilisticDecoderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheProbabilisticDecoderTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The pitches the reference reports, so this compares decoders.</summary>
    /// <remarks>
    /// Passed in rather than searched for, because finding a station is the tone
    /// tracker's job and this is a test of the decoder (HM-DEC-091).
    /// </remarks>
    private static double Tone(string name) => name switch
    {
        var n when n.Contains("004507", StringComparison.Ordinal) => 501,
        var n when n.Contains("003016", StringComparison.Ordinal) => 669,
        var n when n.Contains("003126", StringComparison.Ordinal) => 675,
        var n when n.Contains("003758", StringComparison.Ordinal) => 501,
        _ => 600,
    };

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name));

    /// <remarks>
    /// <para>Proves the port: **the same string the Python produces**, on the one
    /// recording whose content is an ARRL bulletin and therefore recognisable
    /// without anybody adjudicating it. The Python reads
    /// `E JJ AT ARRL DOT NET = EACH STATION HANDLING THIS MESSAG E PE`, and the
    /// only difference below is how a prosign is written: the reference prints
    /// `=` where Hamlet prints `<BT>`.</para>
    /// <para>**RE-RECORDED 2026-08-22 WHEN THE LENGTH PENALTY BECAME A RATIO.**
    /// The expectation is what the reference says, so it moves when the reference
    /// moves and never when only Hamlet does. Both were re-run against this
    /// recording and both now read `NET` and `EACH` where they read `NE T` and
    /// `E ACH`, which is the promotion of element gaps into character gaps going
    /// away.</para>
    /// </remarks>
    [Fact]
    public void ItReadsWhatTheReferenceReads()
    {
        var result = CwProbabilisticDecoder.Decode(
            Read("cw-2026-08-18-004507.wav"), Tone("004507"));

        _output.WriteLine(
            $"{result.WordsPerMinute:0} WPM, ratio {result.LikelihoodRatio:0.0}");

        _output.WriteLine($"'{result.Text}'");

        Assert.Equal(
            "E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE",
            result.Text);

        // And it found the speed on its own, with no seed and nothing measured
        // from run lengths.
        Assert.Equal(18, result.WordsPerMinute);
    }

    /// <remarks>
    /// <para>Proves the speeds are found rather than told: four recordings, four
    /// different fists, no seed anywhere.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="wpm">The speed the reference settled on.</param>
    [Theory]
    [InlineData("cw-2026-08-18-004507.wav", 18)]
    [InlineData("unadjudicated/cw-2026-08-18-003016.wav", 22)]
    [InlineData("unadjudicated/cw-2026-08-18-003126.wav", 28)]
    [InlineData("unadjudicated/cw-2026-08-18-003758.wav", 16)]
    public void TheSpeedIsFoundAndNotTold(string name, int wpm)
    {
        var result = CwProbabilisticDecoder.Decode(Read(name), Tone(name));

        _output.WriteLine(
            $"{name}: {result.WordsPerMinute:0} WPM, "
            + $"ratio {result.LikelihoodRatio:0.0}");

        _output.WriteLine($"    '{result.Text}'");

        Assert.Equal(wpm, result.WordsPerMinute);
        Assert.NotEqual("", result.Text);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-120 **by construction rather than by a guard**: "the
    /// whole stretch is noise" is a competing hypothesis with a score, and on a
    /// recording holding no keying at any pitch it wins. The separation is not
    /// marginal — 3 to 7 with nothing there against 24 to 39 with a station — so
    /// the gate is not sitting on a cliff.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854.wav")]
    [InlineData("unadjudicated/cw-2026-08-20-014935.wav")]
    public void ARecordingWithNoStationInItSaysNothing(string name)
    {
        var result = CwProbabilisticDecoder.Decode(Read(name), 600);

        _output.WriteLine(
            $"{name}: ratio {result.LikelihoodRatio:0.0} "
            + $"against a gate of {CwProbabilisticDecoder.Gate:0.0}");

        Assert.Equal("", result.Text);
        Assert.Empty(result.Characters);

        Assert.True(
            result.LikelihoodRatio < CwProbabilisticDecoder.Gate / 2,
            $"the empty band scored {result.LikelihoodRatio:0.0}, which is close "
            + "enough to the gate that the separation is no longer wide");
    }

    /// <remarks>
    /// <para>Proves the gap is wide on both sides, which is what makes the gate a
    /// number somebody can change rather than one balanced on an edge.</para>
    /// </remarks>
    [Fact]
    public void TheGateSitsInAWideGap()
    {
        var withStation = new[]
        {
            "cw-2026-08-18-004507.wav",
            "unadjudicated/cw-2026-08-18-003016.wav",
            "unadjudicated/cw-2026-08-18-003126.wav",
            "unadjudicated/cw-2026-08-18-003758.wav",
        }.Select(n => CwProbabilisticDecoder.Decode(Read(n), Tone(n)).LikelihoodRatio)
         .ToList();

        var without = new[]
        {
            "unadjudicated/cw-2026-08-20-014854.wav",
            "unadjudicated/cw-2026-08-20-014935.wav",
        }.Select(n => CwProbabilisticDecoder.Decode(Read(n), 600).LikelihoodRatio)
         .ToList();

        _output.WriteLine(
            $"with a station {withStation.Min():0.0} to {withStation.Max():0.0}, "
            + $"without {without.Min():0.0} to {without.Max():0.0}, "
            + $"gate {CwProbabilisticDecoder.Gate:0.0}");

        Assert.True(withStation.Min() > CwProbabilisticDecoder.Gate * 1.5);
        Assert.True(without.Max() < CwProbabilisticDecoder.Gate / 2);
    }

    /// <remarks>
    /// <para>Proves the streaming path reads what the offline one reads, and
    /// **that it keeps up**. The reference said the cost per second of live audio
    /// was the one piece nobody had measured; it is under a tenth of real time on
    /// this machine.</para>
    /// </remarks>
    [Fact]
    public void ItKeepsUpWithLiveAudio()
    {
        var name = "cw-2026-08-18-004507.wav";
        var audio = Read(name);
        var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = Tone(name) };
        var settled = new List<string>();

        stream.CharacterSettled += c => settled.Add(c.Text);

        var watch = Stopwatch.StartNew();
        var block = audio.SampleRate / 10;

        for (var at = 0; at + block <= audio.Samples.Length; at += block)
        {
            stream.Process(audio.Samples.AsSpan(at, block));
        }

        stream.Flush();
        watch.Stop();

        var text = string.Concat(settled);
        var share = watch.Elapsed.TotalSeconds / audio.Duration.TotalSeconds;

        _output.WriteLine($"{share * 100:0.0}% of real time, {settled.Count} characters");
        _output.WriteLine($"'{text}'");

        // **THE BULLETIN'S OWN WORDS**, which is the same audio the offline pass
        // reads and the same reading. Word spacing differs between the two,
        // because a twelve second window fits its own word-gap hypothesis, and
        // every decoder in the field runs words together.
        Assert.Contains("STATION HANDLING", text, StringComparison.Ordinal);
        Assert.Contains(
            "ARRLDOTNET",
            text.Replace(" ", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);

        Assert.True(share < 0.5, $"it took {share * 100:0} per cent of real time");
    }

    /// <remarks>
    /// <para>Proves the silence holds on the streaming path too, which is the one
    /// the operator actually watches.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854.wav")]
    [InlineData("unadjudicated/cw-2026-08-20-014935.wav")]
    public void NothingIsSettledFromAnEmptyBandLive(string name)
    {
        var audio = Read(name);
        var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        var settled = new List<string>();
        var edge = new List<CwCharacter>();

        stream.CharacterSettled += c => settled.Add(c.Text);
        stream.LeadingEdgeChanged += e => edge = e.ToList();

        var block = audio.SampleRate / 10;

        for (var at = 0; at + block <= audio.Samples.Length; at += block)
        {
            stream.Process(audio.Samples.AsSpan(at, block));
        }

        stream.Flush();

        _output.WriteLine(
            $"{name}: {settled.Count} settled, {edge.Count} on the edge, "
            + $"ratio {stream.Last.LikelihoodRatio:0.0}");

        Assert.Empty(settled);
        Assert.Empty(edge);
    }
}
