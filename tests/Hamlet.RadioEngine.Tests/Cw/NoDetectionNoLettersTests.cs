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
    /// Proves work instruction 486's change one: nothing reaches a surface - the settled
    /// transcript or the leading edge - at any moment the detector says no keying, the tail of
    /// the over included, except what the leading edge already showed while keying, which stays on the screen (work instruction 487, R100). Each character is judged by the screen at the moment it arrives.
    /// </remarks>
    [Fact]
    public void NothingReachesTheScreenWhileTheDetectorSaysNoKeying()
    {
        var audio = Call();
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600) { KeyingGate = () => detector.Reading.Keying, DetectorGatesKeying = true };
        var arrived = new List<(CwCharacter Character, bool Keying)>();

        // What the leading edge showed while keying: kept on the screen when keying drops (R100).
        var shown = new HashSet<(string, TimeSpan)>();

        decoder.CharacterSettled += c => arrived.Add((c, detector.Reading.Keying || shown.Contains((c.Text, c.At))));
        decoder.CharacterDecoded += c =>
        {
            if (detector.Reading.Keying)
            {
                shown.Add((c.Text, c.At));
            }

            arrived.Add((c, detector.Reading.Keying));
        };

        var chunk = 80;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, chunk)));
            detector.Process(audio.Samples.AsSpan(at, chunk));
        }

        decoder.Flush();

        var late = arrived.Where(a => !a.Character.IsWordGap && !a.Keying).ToList();

        _output.WriteLine(
            $"arrived {arrived.Count(a => !a.Character.IsWordGap)}, while the detector said no keying {late.Count}: "
            + string.Join(" ", late.Select(a => $"`{a.Character.Text}` heard at {a.Character.At.TotalSeconds:0.000} s")));

        Assert.Contains(arrived, a => a.Keying && !a.Character.IsWordGap);
        Assert.Empty(late);
    }

    /// <summary>
    /// A keyed call with a burst of loud noise in its middle, inside the detector's open window:
    /// the call's first words, a burst of noise with no tone, then the rest. Returns the audio and
    /// where the burst lies, in seconds.
    /// </summary>
    internal static (MonoAudio Audio, double BurstFrom, double BurstTo) CallWithABurst(double burstAmplitude)
    {
        var first = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ", WordsPerMinute: 9, ToneHz: 600, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.05, LeadInSeconds: LeadSeconds, TailSeconds: 0.15, Seed: 487));
        var second = CwSignal.Generate(new CwSignalRequest(
            "DE N0CALL K", WordsPerMinute: 9, ToneHz: 600, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.05, LeadInSeconds: 0.15, TailSeconds: TailSeconds, Seed: 488));
        // Blips at the station's pitch, of uneven length and level, over the same noise: what a
        // decoder spells letters out of and what the detector will not call bars, because no two
        // neighbours stand within its flat tolerance of each other, nor of the station: their
        // levels alternate 4 and 8 dB below it. A blip at the station's own level right after its
        // last mark is a bar the detector calls, and then the rule lets it through by design.
        var random = new Random(489);
        var blips = new List<float>();

        for (var n = 0; n < 12; n++)
        {
            var on = 20 + random.Next(120);
            var off = 15 + random.Next(60);
            var level = burstAmplitude * Math.Pow(10, -(4 + ((n % 2) * 4)) / 20.0);

            for (var i = 0; i < (on + off) * Rate / 1000; i++)
            {
                var tone = i < on * Rate / 1000 ? level * Math.Sin(2 * Math.PI * 600 * blips.Count / Rate) : 0;
                blips.Add((float)(tone + (0.05 * ((random.NextDouble() * 2) - 1))));
            }
        }

        var burst = new MonoAudio(Rate, blips.ToArray());

        var samples = first.Samples.Concat(burst.Samples).Concat(second.Samples).ToArray();
        var from = first.Samples.Length / (double)Rate;

        return (new MonoAudio(Rate, samples), from, from + (burst.Samples.Length / (double)Rate));
    }

    private static (List<CwCharacter> Reached, List<CwCharacter> Settled) Decode(MonoAudio audio, bool blocks)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600)
        {
            KeyingGate = () => detector.Reading.Keying,
            DetectorPitch = () => detector.Reading.Keying ? detector.Reading.PitchHz : double.NaN,
            DetectorBlocks = blocks ? detector.BlocksBetween : null,
            DetectorGatesKeying = true,
            DetectorSteersPitch = true,
            DetectorGatesBlocks = true,
        };
        var reached = new List<CwCharacter>();
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += reached.Add;
        decoder.CharacterSettled += settled.Add;
        decoder.CharacterDecoded += reached.Add;

        for (var at = 0; at + 80 <= audio.Samples.Length; at += 80)
        {
            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, 80)));
            detector.Process(audio.Samples.AsSpan(at, 80));
        }

        decoder.Flush();

        return (reached, settled);
    }

    /// <remarks>
    /// Proves work instruction 487's change one (R99): with a burst of loud noise inside the open
    /// window, no character read from the burst reaches a surface once a letter needs blocks.
    /// Watched failing first with the blocks unasked, which is work instruction 486's gate.
    /// </remarks>
    /// <param name="blocks">Whether the decoder asks the detector for blocks.</param>
    [Theory]
    [InlineData(true)]
    public void ALetterReadFromNoiseDoesNotReachTheScreen(bool blocks)
    {
        var (audio, burstFrom, burstTo) = CallWithABurst(0.5);
        var (reached, settled) = Decode(audio, blocks);
        var (_, alone) = Decode(audio, blocks: false);

        bool InBurst(CwCharacter c) => !c.IsWordGap && c.At.TotalSeconds > burstFrom && c.At.TotalSeconds < burstTo;

        // What the rule costs on the clean call, with no burst in it.
        var (_, cleanAlone) = Decode(Call(), blocks: false);
        var (_, cleanBlocks) = Decode(Call(), blocks: true);

        _output.WriteLine(
            $"clean call: settled with the gate alone {cleanAlone.Count(c => !c.IsWordGap)} `{string.Concat(cleanAlone.Select(c => c.Text))}`; "
            + $"with blocks asked {cleanBlocks.Count(c => !c.IsWordGap)} `{string.Concat(cleanBlocks.Select(c => c.Text))}`");
        _output.WriteLine(
            $"burst {burstFrom:0.000} to {burstTo:0.000} s; settled with the gate alone {alone.Count(c => !c.IsWordGap)} `{string.Concat(alone.Select(c => c.Text))}`; "
            + $"settled {(blocks ? "with blocks asked" : "again with the gate alone")} {settled.Count(c => !c.IsWordGap)} `{string.Concat(settled.Select(c => c.Text))}`; in the burst: "
            + string.Join(" ", reached.Where(InBurst).Select(c => $"`{c.Text}` {c.Pattern} span {c.SpanHops} hops at {c.At.TotalSeconds:0.000} s")));

        Assert.DoesNotContain(reached, InBurst);
    }

    /// <remarks>
    /// Proves work instruction 486's change two: with the detector reporting keying at 675 Hz and
    /// the decoder started at 536 Hz, the decoder mixes at 675 Hz after its first hop.
    /// </remarks>
    [Fact]
    public void TheDecoderMixesWhereTheDetectorHears()
    {
        var decoder = new CwDecoder(Rate, 536) { DetectorPitch = () => 675, DetectorSteersPitch = true };
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.05, LeadInSeconds: 0.1, TailSeconds: 0.1, Seed: 486));

        decoder.Process(new AudioChunk(0, Rate, noise.Samples.AsSpan(0, decoder.Tracker.HopSamples)));

        _output.WriteLine($"mixing {decoder.Stream.ToneHz:0.0} Hz after one hop");

        Assert.Equal(675, decoder.Stream.ToneHz, 3);
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
        var decoder = new CwDecoder(Rate, 600) { KeyingGate = () => detector.Reading.Keying, DetectorGatesKeying = true };
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
