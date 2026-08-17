using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The scope counter row, and when it is allowed to be silent (HM-DEC-093).
/// </summary>
/// <remarks>
/// <para>**THE ROW EXISTS BECAUSE AN EMPTY WATERFALL IS A CLAIM.** "Receiving
/// frames and the band is quiet" and "no frame has ever arrived" paint exactly
/// the same picture, and this feature was reported working three times while the
/// second was true.</para>
/// <para>**AND IT HIDES WHILE EVERY STAGE WORKS**, because a line that never
/// changes is a line people stop reading, and that is the line that has to be
/// read on the evening a stage goes to zero.</para>
/// </remarks>
public sealed class ScopeFlowTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the verdicts are printed.</param>
    public ScopeFlowTests(ITestOutputHelper output) => _output = output;

    private static readonly DateTime Now = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ScopeStage Check(
        long received, long parsed, long sweeps, double secondsAgo = 0,
        bool attached = true)
        => ScopeFlow.Check(
            attached, received, parsed, sweeps,
            received == 0 ? null : Now.AddSeconds(-secondsAgo), Now);

    /// <remarks>
    /// Proves HM-DEC-093 as ruled: **a healthy path says nothing.** The row read
    /// the same three numbers permanently once the waterfall started working,
    /// which is furniture.
    /// </remarks>
    [Fact]
    public void AWorkingScopePathSaysNothingAtAll()
    {
        var stage = Check(received: 4147, parsed: 4147, sweeps: 376, secondsAgo: 0.2);

        Assert.Equal(ScopeStage.Flowing, stage);
        Assert.Equal("", ScopeFlow.Say(stage, 4147, 4147, 0));
    }

    /// <remarks>
    /// <para>**THE PROPERTY HM-DEC-093 EXISTS TO PROTECT, AND IT SURVIVES THE
    /// ROW BEING HIDDEN.** A quiet band and a path that has never received a
    /// byte draw the same black rectangle, so the second one has to say so in
    /// words. Hiding the row while healthy is only safe because this case is not
    /// healthy.</para>
    /// </remarks>
    [Fact]
    public void AQuietBandAndACableThatNeverSpokeAreDifferentPictures()
    {
        var quietBand = Check(received: 4147, parsed: 4147, sweeps: 376, secondsAgo: 0.2);
        var neverSpoke = Check(received: 0, parsed: 0, sweeps: 0);

        var quietSays = ScopeFlow.Say(quietBand, 4147, 4147, 0);
        var neverSays = ScopeFlow.Say(neverSpoke, 0, 0, 0);

        _output.WriteLine($"quiet band : '{quietSays}'");
        _output.WriteLine($"never spoke: '{neverSays}'");

        Assert.NotEqual(quietSays, neverSays);
        Assert.Equal("", quietSays);
        Assert.Contains("not a quiet band", neverSays, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-093: **the first zero is the address of the fault**,
    /// so the stages are tested in the order the data travels. Each of these was
    /// a real state of this repository at some point, and the parse one is the
    /// fault that discarded 2,740 parts while the suite stayed green.</para>
    /// </remarks>
    [Theory]
    [InlineData(0, 0, 0, ScopeStage.NothingEverArrived)]
    [InlineData(2740, 0, 0, ScopeStage.NothingRead)]
    [InlineData(2740, 2740, 0, ScopeStage.NoSweepCompletes)]
    public void EachBrokenStageIsNamedAsItself(
        long received, long parsed, long sweeps, ScopeStage expected)
    {
        var stage = Check(received, parsed, sweeps);

        _output.WriteLine($"{received}/{parsed}/{sweeps} -> {stage}: "
            + ScopeFlow.Say(stage, received, parsed, 0));

        Assert.Equal(expected, stage);
        Assert.NotEqual("", ScopeFlow.Say(stage, received, parsed, 0));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-093 and §0.0: **a path that worked and stopped is not
    /// a path that is working.** This is what the word "receiving" used to get
    /// wrong: it was driven by the cumulative sweep count, so the first sweep of
    /// an evening bought the word for the rest of it and the cable could come
    /// out without the summary noticing.</para>
    /// </remarks>
    [Theory]
    [InlineData(0.5, ScopeStage.Flowing)]
    [InlineData(2.9, ScopeStage.Flowing)]
    [InlineData(3.1, ScopeStage.Stopped)]
    [InlineData(600, ScopeStage.Stopped)]
    public void SweepsThatArrivedOnceDoNotCountAsArrivingNow(
        double secondsAgo, ScopeStage expected)
    {
        var stage = Check(received: 4147, parsed: 4147, sweeps: 376, secondsAgo: secondsAgo);

        _output.WriteLine($"{secondsAgo:0.0} s ago -> {stage}");

        Assert.Equal(expected, stage);
    }

    /// <remarks>
    /// Proves HM-DEC-093: a stopped path says how long it has been stopped, and
    /// says the picture on screen is old rather than letting it be read as now.
    /// </remarks>
    [Fact]
    public void AStoppedPathSaysThePictureIsOld()
    {
        var says = ScopeFlow.Say(ScopeStage.Stopped, 4147, 4147, quietSeconds: 12);

        _output.WriteLine(says);

        Assert.Contains("12 seconds", says, StringComparison.Ordinal);
        Assert.Contains("last thing the radio sent", says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves §0.0: with no stream attached there is nothing to report and
    /// nothing is claimed, which is not the same as a stream reporting zero.
    /// </remarks>
    [Fact]
    public void NoStreamIsNotAStreamReportingNothing()
    {
        var nothing = Check(0, 0, 0, attached: false);
        var silent = Check(0, 0, 0);

        Assert.Equal(ScopeStage.NotAttached, nothing);
        Assert.NotEqual(nothing, silent);
        Assert.Equal("", ScopeFlow.Say(nothing, 0, 0, 0));
    }
}
