using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>The harness scores a committed capture against a fixture, through the same
/// <see cref="Ft8LadderHarness.Available"/> seat the ladder walks.</b>
/// </summary>
/// <remarks>
/// <para>
/// <c>PHASE_PLAN.md</c> step 0's fourth exit says the harness scores <c>Ft8Sharp.Deep</c> against a
/// fixture, and <c>Ft8Sharp.Deep</c> does not exist until step 1. The arbiter's reading, recorded in
/// unit 244's outcome entry: <b>the exit is met by the scoring path working through
/// <c>Available()</c></b>, which today returns <c>Ft8Sharp</c> alone and which the sibling joins with
/// one entry. So the thing to demonstrate is not that a second decoder exists - it is that the path
/// counts every decoder it is handed and never indexes one.
/// </para>
/// <para>
/// <b>These tests score against the worked example, and therefore through
/// <see cref="Ft8LadderHarness.Compare"/> rather than <see cref="Ft8LadderHarness.ScoreFixture"/>.</b>
/// That is the point of the split: <c>Compare</c> does the arithmetic and states its provenance on
/// every printed line, while <c>ScoreFixture</c> is the call that makes a claim about WSJT-X and
/// refuses anything that is not one.
/// </para>
/// </remarks>
public class Ft8FixtureScoringTests(ITestOutputHelper output)
{
    private static Ft8CaptureFixture Example() =>
        Ft8CaptureFixture.Read(Ft8ExampleFixture.CommittedFixturePath);

    [Fact]
    public void TheHarnessScoresTheCommittedExampleThroughTheAvailableSeat()
    {
        var fixture = Example();
        var scores = Ft8LadderHarness.Compare(fixture);

        Assert.Equal(Ft8LadderHarness.Available().Count, scores.Count);
        Assert.Equal("Ft8Sharp", scores[0].Decoder);

        foreach (var line in Ft8LadderHarness.FixtureReport(scores))
        {
            output.WriteLine(line);
        }

        // Three counts, never two, and MATCHED + MISSED is the fixture's rows exactly.
        Assert.Equal(fixture.Rows.Count, scores[0].Rows);
        Assert.Equal(scores[0].Matched.Count + scores[0].Missed.Count, scores[0].Rows);

        // The example was synthesised at a comfortable ratio and every row is a message the ladder
        // actually put in, so the port is expected to find all three. If this ever goes red it is a
        // finding about the decoder, not about the fixture.
        Assert.Equal(3, scores[0].Matched.Count);
        Assert.Empty(scores[0].Missed);
    }

    /// <summary>
    /// <b>Two decoders in, two rows out, with no change anywhere in the harness.</b> This is what
    /// step 1 will do to <see cref="Ft8LadderHarness.Available"/>, done here with a second entry that
    /// is the same port under another name.
    /// </summary>
    /// <remarks>
    /// The second entry deliberately is <em>not</em> a different decoder - inventing one would be
    /// building a piece of <c>Ft8Sharp.Deep</c>, which this phase parks until step 1. What is being
    /// checked is the shape of the reporting path, and for that a second name is enough.
    /// </remarks>
    [Fact]
    public void WhenASecondDecoderJoinsTheSeatTheReportGrowsAColumnAndNothingElseChanges()
    {
        var port = new Ft8SlotDecoder();
        var two = new[]
        {
            new Ft8LadderHarness.Decoder("Ft8Sharp", samples => port.Decode(samples)),
            new Ft8LadderHarness.Decoder("second-seat", samples => port.Decode(samples)),
        };

        var scores = Ft8LadderHarness.Compare(Example(), two);

        Assert.Equal(2, scores.Count);
        Assert.Equal(["Ft8Sharp", "second-seat"], scores.Select(s => s.Decoder));

        // Same samples, same decoder behind both names, so the two rows agree exactly. That is the
        // paired comparison the ladder's remarks describe, and it is what makes a difference between
        // the columns attributable to the decoder rather than to the noise draw.
        Assert.Equal(scores[0].Matched, scores[1].Matched);
        Assert.Equal(scores[0].Missed, scores[1].Missed);
        Assert.Equal(scores[0].ReturnedWrong, scores[1].ReturnedWrong);

        var report = Ft8LadderHarness.FixtureReport(scores).ToArray();
        foreach (var line in report)
        {
            output.WriteLine(line);
        }

        Assert.Contains(report, l => l.StartsWith("Ft8Sharp", StringComparison.Ordinal));
        Assert.Contains(report, l => l.StartsWith("second-seat", StringComparison.Ordinal));
    }

    /// <summary>
    /// <b>A matched count never appears without its returned-wrong count.</b> The report is the only
    /// writer, precisely so that a caller cannot assemble its own lines and forget the third one.
    /// </summary>
    [Fact]
    public void TheReportAlwaysCarriesAllThreeCountsAndSaysWhatTheThirdMeansHere()
    {
        var report = Ft8LadderHarness.FixtureReport(Ft8LadderHarness.Compare(Example())).ToArray();

        Assert.Contains(report, l => l.Contains("MATCHED", StringComparison.Ordinal));
        Assert.Contains(report, l => l.Contains("MISSED", StringComparison.Ordinal));
        Assert.Contains(report, l => l.Contains("WRONG", StringComparison.Ordinal));
        Assert.Contains(report, l => l.Contains("WRONG HERE IS NOT THE LADDER'S WRONG", StringComparison.Ordinal));
    }

    /// <summary>
    /// <b>The report says on its face that this fixture is not WSJT-X's</b>, so counts cut out of it
    /// and pasted somewhere else still carry the qualification with them.
    /// </summary>
    [Fact]
    public void AnExampleFixturesReportSaysSoAtTheTop()
    {
        var report = Ft8LadderHarness.FixtureReport(Ft8LadderHarness.Compare(Example())).ToArray();

        Assert.Contains(report, l => l.Contains("NOT WSJT-X", StringComparison.Ordinal));
        Assert.Contains(
            report,
            l => l.Contains("no claim about WSJT-X may be made from these counts", StringComparison.Ordinal));
    }

    /// <summary><b>Scoring a claim against the example is refused, and the arithmetic path is not.</b></summary>
    [Fact]
    public void ScoreFixtureRefusesTheExampleWhileCompareDoesNot()
    {
        var fixture = Example();

        var thrown = Assert.Throws<Ft8FixtureException>(() => Ft8LadderHarness.ScoreFixture(fixture));
        Assert.Contains("Scoring Hamlet against this fixture", thrown.Message, StringComparison.Ordinal);
        output.WriteLine(thrown.Message);

        Assert.NotEmpty(Ft8LadderHarness.Compare(fixture));
    }

    /// <summary>
    /// <b>A capture at a rate this decode path was not built for is refused, not decoded anyway.</b>
    /// </summary>
    /// <remarks>
    /// Every committed capture in this repository today is CW's at 48 kHz and <c>WavFile</c> checks
    /// the rate against nothing, so this is a live way to be wrong rather than a theoretical one.
    /// </remarks>
    [Fact]
    public void ACaptureAtTheWrongSampleRateIsRefused()
    {
        var fixture = Example() with { SampleRate = 48_000 };

        var thrown = Assert.Throws<Ft8FixtureException>(() => Ft8LadderHarness.Compare(fixture));

        Assert.Contains("48000 samples per second", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("would read as a sensitivity result", thrown.Message, StringComparison.Ordinal);
        output.WriteLine(thrown.Message);
    }
}
