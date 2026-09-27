using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-120 through the live path: every character the operator sees
/// has been read by both decoders from the same samples at the same time (work
/// instruction 465, task 3; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**THE LIVE PATH, NOT THE HARNESS.** A synthetic keyed send with an exact
/// key is played through <see cref="CwDecoder.Process"/> in the 960-sample chunks
/// the application's buffered source delivers, with the second reader on, as
/// `MainWindowViewModel.StartDecoding` builds it.</para>
/// <para>**WATCHED FAILING FIRST** against the stub stage of the wiring, where
/// <see cref="CwDecoder"/> took the <c>secondReader</c> argument and routed
/// nothing: no character carried a record and the port read no samples
/// (`.run-unit/unit465-same-red.txt`).</para>
/// </remarks>
public sealed class BothDecodersReadTheSameSamplesTests
{
    private const int Chunk = 960;

    private static (float[] Samples, int Rate) Send(string name)
    {
        var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, name + ".wav"));

        return (audio.Samples, audio.SampleRate);
    }

    private static (List<CwCharacter> Settled, CwDecoder Decoder) Play(float[] samples, int rate, bool second)
    {
        var decoder = new CwDecoder(rate, SyntheticCq.StartingPitchHz, second);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        for (var at = 0; at < samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, rate, samples.AsSpan(at, Math.Min(Chunk, samples.Length - at))));
        }

        decoder.Flush();

        return (settled, decoder);
    }

    /// <remarks>
    /// HM-REQ-120: on `cq-18wpm-15db`, every settled character carries a record of
    /// the second decoder's reading on its span, or is marked one-sided; none is
    /// passed without one. The port was handed every sample ours was, and read the
    /// send: most characters are paired, each partner's span on the character's own.
    /// </remarks>
    [Fact]
    public void HmReq120_EveryCharacterWasReadByBothFromTheSameSamples()
    {
        var (samples, rate) = Send("cq-18wpm-15db");
        var (settled, decoder) = Play(samples, rate, second: true);
        var characters = settled.Where(c => !c.IsWordGap).ToList();

        Assert.NotEmpty(characters);
        Assert.NotNull(decoder.SecondReader);
        Assert.Null(decoder.SecondReader!.Unavailable);
        Assert.Equal(samples.Length, decoder.SecondReader.SamplesRead);
        Assert.All(characters, c => Assert.NotNull(c.Arbitration));

        var paired = characters.Where(c => c.Arbitration!.Case != CwArbitrationCase.OneSidedOurs).ToList();

        Assert.All(characters.Where(c => c.Arbitration!.Case == CwArbitrationCase.OneSidedOurs),
            c => Assert.Null(c.Arbitration!.SecondText));
        Assert.All(paired, c =>
        {
            Assert.NotNull(c.Arbitration!.SecondText);
            Assert.NotNull(CwArbiter.SameSpan(CwArbiter.SpanOf(c), (c.Arbitration.SecondStart, c.Arbitration.SecondEnd)));
        });
        Assert.True(paired.Count * 2 > characters.Count, $"{paired.Count} of {characters.Count} paired");
    }

    /// <remarks>
    /// HM-REQ-120, the same samples: the reader's streamed resampling hands the
    /// port what <see cref="FldigiRateAdapter.ToFldigiRate"/> hands it over the
    /// whole file, so at one pitch the streamed port prints exactly what the
    /// harness's port prints, and its readings are the harness's.
    /// </remarks>
    [Fact]
    public void HmReq120_TheStreamedPortReadsWhatTheWholeFilePortReads()
    {
        var (samples, rate) = Send("cq-18wpm-15db");
        var reader = new CwSecondReader(rate);
        var streamed = new List<CwSecondReading>();

        for (var at = 0; at < samples.Length; at += Chunk)
        {
            streamed.AddRange(reader.Read(samples.AsSpan(at, Math.Min(Chunk, samples.Length - at)), 615));
        }

        streamed.AddRange(reader.Flush());

        var whole = new FldigiCwDecoder(615) { TraceDecisions = true };

        whole.rx_process(FldigiRateAdapter.ToFldigiRate(samples, rate));

        Assert.Equal(1, reader.Constructions);
        Assert.Equal(whole.Text, reader.Port!.Text);
        Assert.Equal(CwSecondHarvester.Of(whole), streamed);
    }

    /// <remarks>
    /// HM-REQ-120 under today's vote table: the port is advisory live, so what
    /// reaches the transcript through both readers is what ours alone reads,
    /// character, class and p.
    /// </remarks>
    [Fact]
    public void HmReq120_UnderTodaysTableTheTranscriptIsOurs()
    {
        var (samples, rate) = Send("cq-18wpm-15db");
        var (both, _) = Play(samples, rate, second: true);
        var (alone, _) = Play(samples, rate, second: false);

        Assert.Equal(
            alone.Select(c => (c.Text, c.Confidence, c.Probability, c.At)),
            both.Select(c => (c.Text, c.Confidence, c.Probability, c.At)));
    }
}
