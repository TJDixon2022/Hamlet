using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The survey and the gate no longer share an analysis window, and what every
/// width of the gate's own is worth.
/// </summary>
/// <remarks>
/// <para>**THEY ASKED ONE QUESTION THROUGH ONE FILTER AND THEY ARE NOT ONE
/// QUESTION.** The survey searches frequency and the gate measures time, and the
/// shared window was chosen from the fitted speed — so the search was being
/// narrowed by an estimate the search itself produced, and three previous
/// attempts at narrowing detection turned every station-finding test red because
/// they narrowed acquisition with it.</para>
/// <para>**THE SEPARATION IS A NO-OP UNTIL SOMEBODY SETS A WIDTH**, which is what
/// makes it safe to land on its own. With <see cref="CwToneTracker.GateWindowHops"/>
/// unset both passes are the same arithmetic over the same buffer and the suite
/// is unchanged.</para>
/// <para>**AND NO SINGLE FIXED WIDTH IS RIGHT**, which is why none is set.
/// Counting characters across the six real recordings with content in them:
/// following the fitted speed reads 120, and thirty-five milliseconds reads 143.
/// Thirty-five also invents nothing at any level on the sensitivity sweep, so on
/// those two measurements alone it wins outright. **Fixed at thirty-five it also
/// turns the displacement suite red and takes `exchange-easy` with it**, which
/// HM-DEC-114 makes pass or fail. The right width is evidently not a constant,
/// and choosing between real captures and synthesized fixtures is not a
/// session's.</para>
/// </remarks>
public sealed class TheGateHasItsOwnWindowNowTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sweeps are printed.</param>
    public TheGateHasItsOwnWindowNowTests(ITestOutputHelper output)
        => _output = output;

    private const double Centre = 600;

    private static IReadOnlyList<string> RealRecordings()
    {
        var folder = CapturedSignalTests.Folder;

        return Directory.GetFiles(folder, "*.wav")
            .Concat(Directory.GetFiles(Path.Combine(folder, "unadjudicated"), "*.wav"))
            .OrderBy(p => p)
            .ToList();
    }

    private static (int Characters, string Text) Read(string path, int? gateHops)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, Centre);
        var hop = decoder.Tracker.HopSamples;
        var text = new List<string>();

        decoder.Tracker.GateWindowHops = gateHops;
        decoder.CharacterDecoded += c => text.Add(c.Text);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return (decoder.Report.CharactersEmitted, string.Concat(text));
    }

    /// <remarks>
    /// <para>Proves the separation costs nothing: unset, the gate takes the
    /// survey's window and reads exactly what it read before.</para>
    /// </remarks>
    [Fact]
    public void UnsetTheGateStillTakesTheSurveysWindow()
    {
        var tracker = new CwToneTracker(48_000, Centre);

        Assert.Null(tracker.GateWindowHops);
        Assert.Equal(tracker.WindowSamples, tracker.GateWindowSamples);

        tracker.FollowSpeed(30);
        Assert.Equal(tracker.WindowSamples, tracker.GateWindowSamples);

        tracker.FollowSpeed(12);
        Assert.Equal(tracker.WindowSamples, tracker.GateWindowSamples);

        // And a width the ring cannot hold is clamped rather than honoured: a
        // gate asking for more audio than was kept would be reading whatever was
        // in the buffer before it (§0.0).
        tracker.GateWindowHops = 1000;

        Assert.Equal(tracker.MaximumWindowSamples, tracker.GateWindowSamples);
    }

    /// <remarks>
    /// <para>Proves the survey keeps its own window whatever the gate does, which
    /// is the entire point: **station-finding cannot be affected by a choice made
    /// for the gate**, and three earlier attempts failed precisely because it
    /// could.</para>
    /// </remarks>
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    public void TheSurveyKeepsItsWindowWhateverTheGateTakes(int gateHops)
    {
        var tracker = new CwToneTracker(48_000, Centre);

        tracker.FollowSpeed(12);

        var surveyWindow = tracker.WindowSamples;

        tracker.GateWindowHops = gateHops;

        Assert.Equal(surveyWindow, tracker.WindowSamples);
        Assert.Equal(tracker.HopSamples * gateHops, tracker.GateWindowSamples);
    }

    /// <remarks>
    /// <para>The sweep, recorded rather than argued. Nothing is asserted about
    /// which width is best; what is asserted is the one property that
    /// disqualifies a width outright.</para>
    /// </remarks>
    [Fact]
    public void EveryWidthLeavesTheEmptyRecordingsSilent()
    {
        var widths = new int?[] { null, 4, 5, 6, 7, 8, 9, 10 };

        _output.WriteLine(
            "recording                     "
            + string.Join("  ", widths.Select(
                w => (w is null ? "follow" : $"{w * 5}ms").PadLeft(6))));

        foreach (var path in RealRecordings())
        {
            var cells = widths.Select(
                w => Read(path, w).Characters.ToString().PadLeft(6));

            _output.WriteLine(
                $"{Path.GetFileNameWithoutExtension(path),-28}  {string.Join("  ", cells)}");
        }

        // **THE PROPERTY THAT DECIDES WHETHER ANY OF THIS IS WORTH HAVING**
        // (HM-DEC-090, HM-DEC-120). While the two filters shared a window, a
        // narrower one made these speak; separated, no width tried does.
        foreach (var name in new[]
                 {
                     "unadjudicated/cw-2026-08-20-014854.wav",
                     "unadjudicated/cw-2026-08-20-014935.wav",
                 })
        {
            foreach (var width in widths)
            {
                var read = Read(
                    Path.Combine(CapturedSignalTests.Folder, name), width);

                Assert.True(
                    read.Characters == 0,
                    $"{name} emitted {read.Characters} at "
                    + $"{(width is null ? "the survey's window" : width * 5 + " ms")}: "
                    + $"'{read.Text}'");
            }
        }
    }

    /// <remarks>
    /// <para>Proves the cliff, which is what disqualifies the widths that read
    /// most: **a gate wider than thirty-five milliseconds invents characters
    /// below the refusal floor** on the synthesized sensitivity sweep, where
    /// HM-DEC-120's property is that nothing is invented at any level.</para>
    /// <para>Two widths rather than the whole sweep, because each one is thirty
    /// levels times the seed count and this runs in the ordinary suite.
    /// Thirty-five is the best of the widths that are clean and fifty is past the
    /// cliff.</para>
    /// </remarks>
    [Fact]
    public void AWiderGateInventsBelowTheRefusalFloor()
    {
        var clean = CwSensitivity.Sweep(fromDb: 4, toDb: -8, gateWindowHops: 7);
        var wide = CwSensitivity.Sweep(fromDb: 4, toDb: -8, gateWindowHops: 10);

        var cleanWorst = clean.OrderByDescending(p => p.Wrong).First();
        var wideWorst = wide.OrderByDescending(p => p.Wrong).First();

        _output.WriteLine(
            $"35 ms gate: worst invented {cleanWorst.Wrong:0.000} at {cleanWorst.SnrDb:0.0} dB");
        _output.WriteLine(
            $"50 ms gate: worst invented {wideWorst.Wrong:0.000} at {wideWorst.SnrDb:0.0} dB");

        Assert.Equal(0.0, cleanWorst.Wrong);

        Assert.True(
            wideWorst.Wrong > 0,
            "the wider gate has stopped inventing, which would mean the "
            + "disqualification no longer applies and the width should be "
            + "re-swept");
    }
}
