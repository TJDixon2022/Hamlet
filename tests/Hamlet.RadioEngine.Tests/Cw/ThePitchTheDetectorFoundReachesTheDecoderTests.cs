using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Keying always comes with a pitch, and that pitch is what the decoder mixes at (work instruction
/// 488, HM-DEC-193).
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): a station at 625 Hz and 23
/// words a minute - a 52 ms dit, the owner's rows of 2026-09-28 at 23:38 said 51 ms - keyed with
/// ordinary character and word gaps, about 22 dB over the noise in the passband, between seconds of
/// noise; the decoder started at 584 Hz, where the tracker sat on those rows.
/// </remarks>
public sealed class ThePitchTheDetectorFoundReachesTheDecoderTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const double StationHz = 625;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches and the text are printed.</param>
    public ThePitchTheDetectorFoundReachesTheDecoderTests(ITestOutputHelper output) => _output = output;

    internal static MonoAudio Station() => CwSignal.Generate(new CwSignalRequest(
        "CQ CQ DE N0CALL N0CALL K", WordsPerMinute: 23, ToneHz: StationHz, SampleRate: Rate, Amplitude: 0.5,
        NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 488));

    /// <summary>Every keying reading's pitch, counted by pitch; NaN counted under NaN.</summary>
    private static SortedDictionary<double, int> KeyingPitches(out int keying)
    {
        var audio = Station();
        var detector = new CwEnvelopeDetector(Rate);
        var pitches = new SortedDictionary<double, int>();

        keying = 0;

        for (var at = 0; at + Chunk <= audio.Samples.Length; at += Chunk)
        {
            detector.Process(audio.Samples.AsSpan(at, Chunk));

            var r = detector.Reading;

            if (r.Keying)
            {
                keying++;
                pitches[r.PitchHz] = pitches.GetValueOrDefault(r.PitchHz) + 1;
            }
        }

        return pitches;
    }

    private static string Line(SortedDictionary<double, int> pitches) =>
        string.Join(", ", pitches.Select(p => $"{(double.IsNaN(p.Key) ? "none" : $"{p.Key:0} Hz")} x{p.Value}"));

    /// <remarks>
    /// Proves change one: every reading that says keying carries a pitch, through the gaps between
    /// the marks as well as on them. Red before it, when the pitch was set only on the hop a mark was
    /// up - 1118 of 1408 keying readings had none.
    /// </remarks>
    [Fact]
    public void EveryReadingThatSaysKeyingCarriesAPitch()
    {
        var pitches = KeyingPitches(out var keying);

        _output.WriteLine($"readings saying keying {keying}: {Line(pitches)}");

        Assert.True(keying > 0, "the detector never said keying");
        Assert.DoesNotContain(double.NaN, pitches.Keys);
    }

    /// <remarks>
    /// **RED ON PURPOSE, AND NAMED** (work instruction 488 section 3's green, not met): the pitch a
    /// keying reading carries is the station's own, 625 Hz. It is not: two readings in three carry
    /// 575 or 675, the shoulders of the tone's lobe, because on those hops the 625 Hz bin itself
    /// calls no bars - its gaps measure about -20 dB where the shoulders' measure -42 - so the bin
    /// the bars were called in is a shoulder. The section forbids tuning; this states the defect.
    /// </remarks>
    [Fact]
    public void ThatPitchIsTheStationsOwn()
    {
        var pitches = KeyingPitches(out var keying);
        var elsewhere = pitches.Where(p => Math.Abs(p.Key - StationHz) > 0.5).Sum(p => p.Value);

        _output.WriteLine($"readings saying keying {keying}, of them at a pitch other than {StationHz:0} Hz {elsewhere}: {Line(pitches)}");

        Assert.Equal(0, elsewhere);
    }

    /// <remarks>
    /// Proves change two: fed the reading's pitch while it says keying, the decoder started at 584 Hz
    /// mixes at that number on every hop keying holds, gaps included, and never falls back to the
    /// tracker between characters. Red before change one: the gaps fed nothing and the tracker won.
    /// </remarks>
    [Fact]
    public void TheDecoderMixesAtThePitchTheReadingCarriesThroughTheGaps()
    {
        var audio = Station();
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 584)
        {
            DetectorPitch = () => detector.Reading is { Keying: true } r ? r.PitchHz : double.NaN,
        };
        int keyedHops = 0, notFed = 0;

        for (var at = 0; at + Chunk <= audio.Samples.Length; at += Chunk)
        {
            // The decoder reads the reading as it stands before this chunk; judge it against that.
            var reading = detector.Reading;

            decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, Chunk)));

            if (reading.Keying)
            {
                keyedHops++;

                if (double.IsNaN(reading.PitchHz) || Math.Abs(decoder.MixingHz - reading.PitchHz) > 0.5)
                {
                    notFed++;
                }
            }

            detector.Process(audio.Samples.AsSpan(at, Chunk));
        }

        _output.WriteLine($"chunks mixed while keying {keyedHops}, of them not at the pitch the reading carried {notFed}; tracker at {decoder.Report.ToneHz:0} Hz");

        Assert.True(keyedHops > 0, "the detector never said keying");
        Assert.Equal(0, notFed);
    }

    /// <remarks>
    /// Answers work instruction 488 section 5 and asserts nothing: the station wired as the tab wires
    /// it, the gate and the block rule on, and the rung fed three ways - nothing, as the owner's rows
    /// said it was; the reading's own pitch, as it is now; and the station's own 625 Hz while keying,
    /// to show what the shoulder costs. Nothing is tuned. Prints the settled text of each.
    /// </remarks>
    [Fact]
    public void WhatTheStationOfTwentyThreeThirtyEightReads()
    {
        string Read(Func<CwEnvelopeDetector, double> rung)
        {
            var audio = Station();
            var detector = new CwEnvelopeDetector(Rate);
            var decoder = new CwDecoder(Rate, 584)
            {
                KeyingGate = () => detector.Reading.Keying,
                DetectorPitch = () => rung(detector),
                DetectorBlocks = detector.BlocksBetween,
            };
            var settled = new List<CwCharacter>();

            decoder.CharacterSettled += settled.Add;

            for (var at = 0; at + Chunk <= audio.Samples.Length; at += Chunk)
            {
                decoder.Process(new AudioChunk(at, Rate, audio.Samples.AsSpan(at, Chunk)));
                detector.Process(audio.Samples.AsSpan(at, Chunk));
            }

            decoder.Flush();

            return string.Concat(settled.Select(c => c.Text));
        }

        var nothing = Read(_ => double.NaN);
        var carried = Read(d => d.Reading is { Keying: true } r ? r.PitchHz : double.NaN);
        var own = Read(d => d.Reading.Keying ? StationHz : double.NaN);

        _output.WriteLine("sent               `CQ CQ DE N0CALL N0CALL K`");
        _output.WriteLine($"rung fed nothing   `{nothing}` ({nothing.Count(c => c != ' ')} characters)");
        _output.WriteLine($"rung fed the pitch `{carried}` ({carried.Count(c => c != ' ')} characters)");
        _output.WriteLine($"rung fed 625 Hz    `{own}` ({own.Count(c => c != ' ')} characters)");
    }
}
