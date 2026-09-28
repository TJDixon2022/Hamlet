using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The detector holds a station through its gaps, and the decoder emits nothing the detector did
/// not hear (work instruction 485; R97, HM-DEC-190).
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). A keyed call at nine words a
/// minute, whose word gap and the dah after it outlast the detector's one-second pairing window,
/// between seconds of noise with no tone in it.
/// </remarks>
public sealed class NoDetectionNoLettersTests
{
    private const int Rate = 8000;
    private const double LeadSeconds = 4;
    private const double TailSeconds = 4;
    private const string Text = "CQ CQ DE N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the timings are printed.</param>
    public NoDetectionNoLettersTests(ITestOutputHelper output) => _output = output;

    private static MonoAudio Call() => CwSignal.Generate(new CwSignalRequest(
        Text, WordsPerMinute: 9, ToneHz: 600, SampleRate: Rate, Amplitude: 0.5,
        NoiseAmplitude: 0.05, LeadInSeconds: LeadSeconds, TailSeconds: TailSeconds, Seed: 485));

    private static double ToneEndSeconds(MonoAudio audio) => (audio.Samples.Length / (double)Rate) - TailSeconds;

    /// <remarks>
    /// Proves change two: once the detector has found the keyed call, it says keying at every hop
    /// until the last mark, through every character and word gap, and lets go within
    /// <see cref="CwEnvelopeDetector.HoldSeconds"/> of the tone stopping.
    /// </remarks>
    [Fact]
    public void TheDetectorHoldsTheStationThroughItsGaps()
    {
        var audio = Call();
        var detector = new CwEnvelopeDetector(Rate);
        var chunk = 80;
        var found = double.NaN;
        var dropped = new List<double>();
        var lastKeying = double.NaN;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            detector.Process(audio.Samples.AsSpan(at, chunk));

            var seconds = (at + chunk) / (double)Rate;
            var keying = detector.Reading.Keying;

            if (keying && double.IsNaN(found))
            {
                found = seconds;
            }

            if (keying)
            {
                lastKeying = seconds;
            }
            else if (!double.IsNaN(found) && seconds < ToneEndSeconds(audio))
            {
                dropped.Add(seconds);
            }
        }

        _output.WriteLine(
            $"found {found:0.000} s, tone ends {ToneEndSeconds(audio):0.000} s, last keying {lastKeying:0.000} s, "
            + $"hops dropped while the call was still sending {dropped.Count}"
            + (dropped.Count > 0 ? $", first at {dropped[0]:0.000} s" : ""));

        Assert.False(double.IsNaN(found), "the detector never found the keyed call");
        Assert.Empty(dropped);
        Assert.InRange(lastKeying, ToneEndSeconds(audio), ToneEndSeconds(audio) + CwEnvelopeDetector.HoldSeconds + 0.5);
    }

    /// <remarks>
    /// Proves change three: with the decoder gated by the detector, no character is emitted from
    /// the noise before the call or after it, on the settled pass or the leading edge.
    /// </remarks>
    [Fact]
    public void NothingIsEmittedFromTheSilences()
    {
        var audio = Call();
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600) { KeyingGate = () => detector.Reading.Keying };
        var settled = new List<CwCharacter>();
        var edged = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;
        decoder.CharacterDecoded += edged.Add;

        var chunk = 80;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, chunk)));
            detector.Process(audio.Samples.AsSpan(at, chunk));
        }

        decoder.Flush();

        var from = LeadSeconds - 0.25;
        var to = ToneEndSeconds(audio) + 0.25;

        bool InSilence(CwCharacter c) => !c.IsWordGap && (c.At.TotalSeconds < from || c.At.TotalSeconds > to);

        var outside = settled.Where(InSilence).Concat(edged.Where(InSilence)).ToList();

        _output.WriteLine(
            $"settled `{string.Concat(settled.Select(c => c.Text))}`; in the silences {outside.Count}: "
            + string.Join(" ", outside.Select(c => $"`{c.Text}` at {c.At.TotalSeconds:0.000} s")));

        Assert.Empty(outside);
    }
}
