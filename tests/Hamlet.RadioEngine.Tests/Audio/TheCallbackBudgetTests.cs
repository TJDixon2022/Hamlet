using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 239, task 4: every callback that exceeds the buffer period
/// is counted, and the period it is counted against is the one in force.
/// </summary>
/// <remarks>
/// <para>**UNIT 238 ASSERTED AGAINST A BUDGET THE DEVICE NEVER HAD.** It used
/// 20,000 microseconds, which is 960 samples at 48 kHz - what
/// `BufferedAudioSource` hands the decoder, and a different quantity in a
/// different part of the pipeline. The shack machine's worst callback of 91,372
/// microseconds reads as four and a half times its budget against that figure
/// and as 91% of it against the real 100,000, and only one of those readings
/// could lead anybody anywhere.</para>
/// <para>**SO THE PERIOD IS NOW SET RATHER THAN INHERITED**, in
/// `WasapiAudioSource`'s constructor, and it travels with the counts to every
/// surface that reports them.</para>
/// </remarks>
public sealed class TheCallbackBudgetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheCallbackBudgetTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A callback that runs past its period is counted.</summary>
    /// <remarks>
    /// **THE WORK IS REALLY SLOW AND REALLY TIMED.** The budget is given a
    /// deliberately small period and handed work that spins past it, so what is
    /// asserted is the whole path - a stopwatch, a duration, a comparison and a
    /// count - rather than a number typed straight into `Record`.
    /// </remarks>
    [Fact]
    public void ALongRunningCallbackIsCounted()
    {
        // Five milliseconds, so the slow arm below is unambiguously past it on
        // any machine without the test taking a noticeable length of time.
        var budget = new CallbackBudget(5_000);

        budget.Time(() =>
        {
            var until = Stopwatch.GetTimestamp() + (Stopwatch.Frequency / 50);

            while (Stopwatch.GetTimestamp() < until)
            {
                Thread.SpinWait(100);
            }
        });

        _output.WriteLine("period          : 5000 us");
        _output.WriteLine("longest seen    : "
            + budget.LongestMicroseconds.ToString("0") + " us");
        _output.WriteLine("over period     : " + budget.OverPeriod);
        _output.WriteLine("over half       : " + budget.OverHalfPeriod);
        _output.WriteLine("timed           : " + budget.Measured);
        _output.WriteLine("describes as    : " + budget.Describe());

        Assert.Equal(1, budget.Measured);
        Assert.Equal(1, budget.OverPeriod);

        // **A FULL OVERRUN IS ALSO OVER HALF, AND BOTH COUNTS SAY SO.** They
        // overlap rather than partition, so a reader asking "how often was it
        // close" does not have to add two numbers together.
        Assert.Equal(1, budget.OverHalfPeriod);

        Assert.True(
            budget.LongestMicroseconds > 5_000,
            "the slow work did not actually run past the period; it took "
            + budget.LongestMicroseconds.ToString("0") + " us");
    }

    /// <summary>A callback well inside its period counts nothing.</summary>
    [Fact]
    public void AWellBehavedCallbackIsNotCounted()
    {
        // A whole second of budget against work that returns immediately: there
        // is no machine slow enough for this to be a flake.
        var budget = new CallbackBudget(1_000_000);

        for (var i = 0; i < 50; i++)
        {
            budget.Time(() => { });
        }

        _output.WriteLine("period       : 1000000 us");
        _output.WriteLine("longest seen : "
            + budget.LongestMicroseconds.ToString("0.###") + " us");
        _output.WriteLine("over period  : " + budget.OverPeriod);
        _output.WriteLine("over half    : " + budget.OverHalfPeriod);
        _output.WriteLine("timed        : " + budget.Measured);

        Assert.Equal(50, budget.Measured);
        Assert.Equal(0, budget.OverPeriod);
        Assert.Equal(0, budget.OverHalfPeriod);
    }

    /// <summary>Half the period is counted before the whole of it is.</summary>
    /// <remarks>
    /// **THIS IS THE COUNT THAT IS ACTUALLY USEFUL ON A WORKING MACHINE.** A
    /// callback consistently over half its budget has no headroom left for a
    /// collection or a scheduling hiccup, and it says so while everything is
    /// still working, where the full overrun count only moves once samples are
    /// already at risk.
    /// </remarks>
    [Fact]
    public void HalfThePeriodIsCountedBeforeTheWholeOfItIs()
    {
        var budget = new CallbackBudget(1_000);

        budget.Record(600);
        budget.Record(400);
        budget.Record(1_200);

        _output.WriteLine("600, 400 and 1200 us against a 1000 us period");
        _output.WriteLine("over period : " + budget.OverPeriod + " (the 1200)");
        _output.WriteLine("over half   : " + budget.OverHalfPeriod
            + " (the 600 and the 1200)");

        Assert.Equal(1, budget.OverPeriod);
        Assert.Equal(2, budget.OverHalfPeriod);
        Assert.Equal(3, budget.Measured);
    }

    /// <summary>An unknown period counts no overrun and says so.</summary>
    /// <remarks>
    /// **ZERO IS NOT KNOWN AND IS NOT A BUDGET OF NOTHING** (§0.0). The training
    /// radio and the WAV replay source are not WASAPI and have no device period.
    /// Reporting every callback as an overrun against a budget of zero would be
    /// a confident wrong answer about a source that is working perfectly.
    /// </remarks>
    [Fact]
    public void AnUnknownPeriodRefusesToCountOverruns()
    {
        var budget = new CallbackBudget(0);

        budget.Record(500_000);
        budget.Record(1);

        _output.WriteLine("describes as : " + budget.Describe());

        Assert.Equal(0, budget.OverPeriod);
        Assert.Equal(0, budget.OverHalfPeriod);
        Assert.Equal(2, budget.Measured);

        // The longest is still recorded, because that is a measurement and not
        // a comparison: it needs no budget to be true.
        Assert.Equal(500_000, budget.LongestMicroseconds);

        Assert.Contains("not read", budget.Describe(), StringComparison.Ordinal);
    }

    /// <summary>The counts reach the arrival record the surfaces read.</summary>
    /// <remarks>
    /// **THE COUNT IS WORTH NOTHING IF IT STOPS AT THE SOURCE.** Unit 238's own
    /// lesson: the callback duration was measured for weeks and no surface the
    /// operator reads carried it (HM-DEC-093).
    /// </remarks>
    [Fact]
    public void TheArrivalRecordCarriesThePeriodAndTheCounts()
    {
        var arrival = new AudioArrival(
            0.99, 0.99, 0, 0, 0, 0, 91_372, 480_000,
            BufferPeriodMicroseconds: 100_000,
            CallbacksOverPeriod: 3,
            CallbacksOverHalfPeriod: 41,
            CallbacksTimed: 1_200);

        _output.WriteLine(arrival.CallbackBudgetText);

        Assert.Contains("3 over the 100000 us buffer period",
            arrival.CallbackBudgetText, StringComparison.Ordinal);
        Assert.Contains("41 over half of it",
            arrival.CallbackBudgetText, StringComparison.Ordinal);
        Assert.Contains("1200 timed",
            arrival.CallbackBudgetText, StringComparison.Ordinal);

        // **AND WITHOUT A PERIOD IT SAYS SO RATHER THAN REPORTING ZERO.**
        var noPeriod = AudioArrival.None with { CallbacksTimed = 40 };

        _output.WriteLine(noPeriod.CallbackBudgetText);

        Assert.Contains("not read", noPeriod.CallbackBudgetText,
            StringComparison.Ordinal);
    }
}
