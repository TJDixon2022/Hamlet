using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The detector stops steering and gating the decoder until its pitch is fit (work instruction
/// 489, R102, HM-DEC-194).
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): unit 488's clean call at
/// 625 Hz and 23 words a minute, which the detector calls 50 Hz to one side two hops in three.
/// </remarks>
public sealed class TheDecoderGetsItsEarsBackTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two readings are printed.</param>
    public TheDecoderGetsItsEarsBackTests(ITestOutputHelper output) => _output = output;

    /// <summary>The settled text of the call, with the detector wired as the tab wires it or not at all.</summary>
    internal static string Read(bool wired)
    {
        var audio = ThePitchTheDetectorFoundReachesTheDecoderTests.Station();
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 584);
        var settled = new List<CwCharacter>();

        if (wired)
        {
            decoder.KeyingGate = () => detector.Reading.Keying;
            decoder.DetectorPitch = () => detector.Reading is { Keying: true } r ? r.PitchHz : double.NaN;
            decoder.DetectorBlocks = detector.BlocksBetween;
        }

        decoder.CharacterSettled += settled.Add;

        for (var at = 0; at + Chunk <= audio.Samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, Chunk)));
            detector.Process(audio.Samples.AsSpan(at, Chunk));
        }

        decoder.Flush();

        return string.Concat(settled.Select(c => c.Text));
    }

    /// <remarks>
    /// Proves section 3: with the detector wired to the decoder exactly as the tab wires it, the call
    /// reads what the same decoder reads unbound - what it read before unit 486. Red while the
    /// detector's pitch, keying and blocks decide what the decoder hears and lets out.
    /// </remarks>
    [Fact]
    public void AWiredDecoderReadsWhatAnUnboundOneReads()
    {
        var unbound = Read(wired: false);
        var wired = Read(wired: true);

        _output.WriteLine("sent     `CQ CQ DE N0CALL N0CALL K`");
        _output.WriteLine($"unbound  `{unbound}` ({unbound.Count(c => c != ' ')} characters)");
        _output.WriteLine($"wired    `{wired}` ({wired.Count(c => c != ' ')} characters)");

        Assert.Equal(unbound, wired);
    }
}
