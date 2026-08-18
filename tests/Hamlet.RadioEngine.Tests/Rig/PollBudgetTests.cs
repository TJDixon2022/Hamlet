using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// What the polling actually costs the bus (HM-DEC-109, phase 6 of the cleanup
/// order).
/// </summary>
/// <remarks>
/// <para>**"IT SHOULD BE INVISIBLE" IS NOT A MEASUREMENT**, which the last report
/// said about the frequency sweep and then did not measure. CI-V is a slow serial
/// bus shared with the transceive stream, and hammering it makes the radio
/// sluggish and the app unreliable, which is the hardest kind of defect to
/// attribute.</para>
/// <para>Arithmetic rather than a stopwatch, from the plan itself, so it stays
/// true when somebody adds a field and does not depend on how fast this machine
/// happens to be (§5).</para>
/// </remarks>
public sealed class PollBudgetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the budget is printed.</param>
    public PollBudgetTests(ITestOutputHelper output) => _output = output;

    /// <summary>How long a short read and its reply take on the wire.</summary>
    /// <remarks>
    /// Two milliseconds, which is the figure `RigPollPlan` already carries for
    /// 19200 baud. A frequency read is eleven bytes out and sixteen back, so it
    /// is at the larger end of "short" and the estimate is generous rather than
    /// flattering.
    /// </remarks>
    public const double WireMilliseconds = 2.0;

    /// <summary>An evening at the radio.</summary>
    public static readonly TimeSpan Evening = TimeSpan.FromHours(4);

    /// <remarks>
    /// <para>**THE NUMBER THE LAST REPORT OWED.** One field added to a sweep that
    /// runs twice a minute is 480 commands across an evening, against the tens of
    /// thousands the live fields already spend, and about a second of wire time
    /// in four hours.</para>
    /// </remarks>
    [Fact]
    public void TheFrequencySweepCostsAboutASecondOfWireTimeAnEvening()
    {
        var live = RigPollPlan.At(RigPollRate.Live).Count;
        var session = RigPollPlan.At(RigPollRate.Session).Count;

        var liveSweeps = Evening.TotalMilliseconds / RigPollPlan.LiveInterval.TotalMilliseconds;
        var sessionSweeps = Evening.TotalMilliseconds / RigPollPlan.SessionInterval.TotalMilliseconds;

        var liveReads = live * liveSweeps;
        var sessionReads = session * sessionSweeps;

        // The frequency is one field on the session sweep, so its own cost is one
        // read per sweep and nothing else.
        var frequencyReads = sessionSweeps;

        var totalMs = (liveReads + sessionReads) * WireMilliseconds;
        var frequencyMs = frequencyReads * WireMilliseconds;

        _output.WriteLine($"{live} live fields every {RigPollPlan.LiveInterval.TotalMilliseconds:0} ms");
        _output.WriteLine($"{session} session fields every {RigPollPlan.SessionInterval.TotalSeconds:0} s");
        _output.WriteLine("");
        _output.WriteLine($"over {Evening.TotalHours:0} hours:");
        _output.WriteLine($"  live      {liveReads,10:N0} reads");
        _output.WriteLine($"  session   {sessionReads,10:N0} reads");
        _output.WriteLine($"  frequency {frequencyReads,10:N0} reads  "
            + $"({frequencyReads / (liveReads + sessionReads):P2} of the traffic)");
        _output.WriteLine("");
        _output.WriteLine($"  wire time  {totalMs / 1000:N1} s of {Evening.TotalSeconds:N0} s "
            + $"= {totalMs / Evening.TotalMilliseconds:P2} of the bus");
        _output.WriteLine($"  frequency  {frequencyMs / 1000:N1} s "
            + $"= {frequencyMs / Evening.TotalMilliseconds:P3} of the bus");

        // **THE SWEEP IS THE CHEAP PART AND THE METER IS THE EXPENSIVE ONE.**
        // Anybody worrying about what this ruling cost is looking in the wrong
        // place by three orders of magnitude.
        Assert.True(
            frequencyMs < 2_000,
            $"the frequency sweep costs {frequencyMs / 1000:N1} seconds of wire "
            + "time an evening, which is no longer the rounding error it was");

        Assert.True(
            frequencyReads / (liveReads + sessionReads) < 0.01,
            "the frequency is now more than one percent of the traffic");
    }

    /// <remarks>
    /// Proves the ration itself still holds (HM-DEC-050): **the whole poll loop
    /// stays a small fraction of a bus somebody else is trying to use.** It is
    /// the claim `RigPollPlan` opens with, and nothing measured it either.
    /// </remarks>
    [Fact]
    public void TheWholePollLoopStaysWellUnderATenthOfTheBus()
    {
        var live = RigPollPlan.At(RigPollRate.Live).Count;
        var session = RigPollPlan.At(RigPollRate.Session).Count;

        var perSecond = (live / RigPollPlan.LiveInterval.TotalSeconds)
            + (session / RigPollPlan.SessionInterval.TotalSeconds);

        var busyFraction = perSecond * WireMilliseconds / 1000.0;

        _output.WriteLine($"{perSecond:0.0} reads a second, "
            + $"{busyFraction:P2} of the wire");

        Assert.True(
            busyFraction < 0.10,
            $"the poll loop is using {busyFraction:P1} of a bus the radio's own "
            + "transceive stream also has to get down");
    }
}
