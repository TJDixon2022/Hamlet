using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether a sender's gap lengths can be taken from the gaps rather than from
/// the unit.
/// </summary>
/// <remarks>
/// <para>**THE COUPLING IS REAL AND THE CURE NEEDS A TROUGH.** A boundary placed
/// at a multiple of the estimated unit ties the letter spacing to the speed
/// estimate, so one wrong number breaks both. Taking the boundary from the gaps
/// removes that, **but only where the gap distribution has an empty stretch to put
/// it in** — a boundary standing in the middle of a heap misclassifies whatever is
/// standing there, which is worse than the coupling.</para>
/// <para>**ONE CAPTURE IN NINE HAS IT, AND IT IS THE ONE THE COUPLING IS
/// BREAKING.** `cw-2026-08-18-004507` clusters into three heaps with troughs
/// between them; the other eight do not and fall back to one, three and seven
/// units. **Generated Morse does have the structure**, which is what says the
/// refusals are a fact about those recordings rather than about this code.</para>
/// <para>**IT IS MEASURED AND NOT WIRED IN.** Fed to the decoder it repairs
/// `004507`, bringing back `ACH STATION HANDLING` and `MESSAGE` whole, and it
/// costs `VA3VRR` and breaks `AA4MP/4 QNIK`, because a twelve second window can
/// show a trough the whole recording does not. The ledger is in the report for
/// 2026-08-22.</para>
/// </remarks>
public sealed class WhereTheGapsActuallySitTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the boundaries are printed.</param>
    public WhereTheGapsActuallySitTests(ITestOutputHelper output)
        => _output = output;

    private static IEnumerable<string> Captures()
    {
        var folder = CapturedSignalTests.Folder;

        return Directory.GetFiles(folder, "*.wav")
            .Concat(Directory.GetFiles(Path.Combine(folder, "unadjudicated"), "*.wav"))
            .OrderBy(p => p);
    }

    /// <remarks>
    /// <para>Records which captures have the structure and which do not, so a
    /// change in either direction is visible rather than silent.</para>
    /// </remarks>
    [Fact]
    public void OnlyTheOneTheCouplingBreaksHasTheTrough()
    {
        var withStructure = new List<string>();

        foreach (var path in Captures())
        {
            var audio = WavAudio.Read(path);
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            var envelope = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, decoder.Tracker.ToneHz);

            var unit = CwUnitEstimator.Measure(
                envelope, CwProbabilisticDecoder.HopMilliseconds);

            var gaps = CwUnitEstimator.MeasureGaps(
                envelope,
                CwProbabilisticDecoder.HopMilliseconds,
                unit.UnitMilliseconds);

            _output.WriteLine(
                $"{Path.GetFileNameWithoutExtension(path),-24} "
                + $"unit {unit.UnitMilliseconds,5:0.0} ms  "
                + $"boundaries {gaps.CharacterBoundaryMilliseconds,6:0.0} and "
                + $"{gaps.WordBoundaryMilliseconds,6:0.0} ms  "
                + $"({gaps.CharacterBoundaryMilliseconds / Math.Max(unit.UnitMilliseconds, 1e-9):0.00}u "
                + $"and {gaps.WordBoundaryMilliseconds / Math.Max(unit.UnitMilliseconds, 1e-9):0.00}u)  "
                + $"from the gaps themselves: {gaps.Separated}");

            if (gaps.Separated)
            {
                withStructure.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        Assert.Equal(new[] { "cw-2026-08-18-004507" }, withStructure);
    }

    /// <remarks>
    /// <para>Proves the mechanism on audio that does have the structure, so the
    /// refusal above is a fact about the captures rather than about the code.
    /// Generated Morse has textbook spacing, three clean heaps, and the clustering
    /// finds boundaries between them.</para>
    /// </remarks>
    [Fact]
    public void GeneratedMorseDoesHaveIt()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K TEST DE W1AW",
            WordsPerMinute: 18,
            ToneHz: 600,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            Seed: 3));

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, 600);

        var unit = CwUnitEstimator.Measure(
            envelope, CwProbabilisticDecoder.HopMilliseconds);

        var gaps = CwUnitEstimator.MeasureGaps(
            envelope, CwProbabilisticDecoder.HopMilliseconds, unit.UnitMilliseconds);

        _output.WriteLine(
            $"unit {unit.UnitMilliseconds:0.0} ms, gaps "
            + $"{gaps.ElementMilliseconds:0.0} / {gaps.CharacterMilliseconds:0.0} / "
            + $"{gaps.WordMilliseconds:0.0} ms, boundaries "
            + $"{gaps.CharacterBoundaryMilliseconds:0.0} and "
            + $"{gaps.WordBoundaryMilliseconds:0.0} ms, "
            + $"from the gaps themselves: {gaps.Separated}");

        Assert.True(
            gaps.Separated,
            "textbook spacing no longer clusters into three heaps with troughs, "
            + "so the clustering itself has stopped working");

        // And the boundary it found is between the two things it divides.
        Assert.InRange(
            gaps.CharacterBoundaryMilliseconds,
            gaps.ElementMilliseconds,
            gaps.CharacterMilliseconds);
    }
}
