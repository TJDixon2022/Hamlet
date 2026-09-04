using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 241, task 1.3: how fast the decoded table fills, and what
/// cap that argues for.
/// </summary>
/// <remarks>
/// <para>**THE RATE IS ARITHMETIC ON ONE MEASURED DATUM AND THE SLOT GEOMETRY,
/// AND THE DATUM WAS NOT TAKEN HERE.** Hamlet's first FT8 decode off the air
/// happened on the shack machine on 2026-09-04 at 21:41 UTC: fourteen messages
/// out of one slot, sixty-three shown. This is the development machine and has
/// no radio (`SHACK_FACTS.md` FACT-004), so nothing here re-measures that -
/// there is no telemetry for 2026-09-04 in this tree at all. What this pins is
/// the arithmetic that follows from it, so the cap in task 6 is chosen against a
/// number somebody can check rather than against a feeling.</para>
/// <para>**FOURTEEN A SLOT IS ONE BAND ON ONE EVENING AND NOT A CONSTANT.** It
/// is the only reading this project has, and a quiet band gives fewer while a
/// contest weekend on twenty metres gives more. The cap is therefore chosen to
/// hold a stated *duration* rather than a stated number of rows, and the test
/// says what duration.</para>
/// </remarks>
public sealed class HowFastTheDecodedTableGrowsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the arithmetic is printed.</param>
    public HowFastTheDecodedTableGrowsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Messages in the 2026-09-04 21:41 UTC slot, on the shack machine.</summary>
    private const int RowsPerSlot = 14;

    /// <summary>What the panel had accumulated when the operator looked.</summary>
    private const int ShownWhenReported = 63;

    /// <summary>What the rate comes to, and what it means for the cap.</summary>
    [Fact]
    public void TheRatePerHourAndWhatItCosts()
    {
        var slotsPerMinute = 60.0 / Ft8Slots.SlotSeconds;
        var slotsPerHour = slotsPerMinute * 60;

        var perHour = RowsPerSlot * slotsPerHour;
        var perEvening = perHour * 5;

        var capHoldsSlots = MainWindowViewModel.MaxDigitalDecodes / (double)RowsPerSlot;
        var capHoldsMinutes = capHoldsSlots / slotsPerMinute;

        _output.WriteLine("measured on the shack machine, 2026-09-04 21:41 UTC:");
        _output.WriteLine("  " + RowsPerSlot + " messages in one slot, "
            + ShownWhenReported + " shown when the operator looked");
        _output.WriteLine("  (" + (ShownWhenReported / (double)RowsPerSlot).ToString("0.0")
            + " slots' worth, so about "
            + (ShownWhenReported / (double)RowsPerSlot / slotsPerMinute).ToString("0.0")
            + " minutes of listening)");
        _output.WriteLine("");
        _output.WriteLine("FT8 slot geometry: " + Ft8Slots.SlotSeconds + " s, so "
            + slotsPerMinute + " slots a minute and " + slotsPerHour + " an hour");
        _output.WriteLine("");
        _output.WriteLine("  rows an hour          : " + perHour.ToString("0"));
        _output.WriteLine("  rows a five-hour night: " + perEvening.ToString("0"));
        _output.WriteLine("");
        _output.WriteLine("  the cap in force      : "
            + MainWindowViewModel.MaxDigitalDecodes);
        _output.WriteLine("  which holds           : "
            + capHoldsSlots.ToString("0") + " slots, about "
            + capHoldsMinutes.ToString("0") + " minutes of a band this busy");

        // **THE FINDING, AND IT IS WHY TASK 6 IS ABOUT THE SUMMARY RATHER THAN
        // ABOUT THE NUMBER.** A cap already existed before this unit and it is
        // not obviously wrong - five hundred rows is nine minutes of the busiest
        // band anybody has recorded here, and the panel does not virtualise, so
        // raising it costs layout on every row whether or not it is on screen.
        // What was missing is that the operator is never told it is happening.
        // A list that silently discards rows he believes are still there is
        // §0.0's fault whatever the number is.
        Assert.Equal(240, slotsPerHour);
        Assert.Equal(3_360, perHour);
        Assert.Equal(16_800, perEvening);

        Assert.True(
            capHoldsMinutes >= 5,
            "the cap holds only " + capHoldsMinutes.ToString("0.0")
            + " minutes of a band as busy as the one measured, which is less "
            + "than one over-and-back exchange");
    }

    /// <summary>
    /// A five-hour evening overruns the cap many times over, so the trim is not
    /// a corner case.
    /// </summary>
    /// <remarks>
    /// **IT IS THE ORDINARY CASE, WHICH IS THE WHOLE POINT.** At this rate the
    /// cap is reached in nine minutes and then every subsequent row silently
    /// displaces one the operator may still be reading. That is not an edge to
    /// be guarded; it is what happens every evening after the first ten minutes.
    /// </remarks>
    [Fact]
    public void TheCapIsReachedEveryEveningAndNotOccasionally()
    {
        var perEvening = RowsPerSlot * (60.0 / Ft8Slots.SlotSeconds) * 60 * 5;
        var timesOver = perEvening / MainWindowViewModel.MaxDigitalDecodes;

        _output.WriteLine("a five-hour evening produces " + perEvening.ToString("0")
            + " rows against a cap of " + MainWindowViewModel.MaxDigitalDecodes);
        _output.WriteLine("that is " + timesOver.ToString("0.0")
            + " times the cap, so the trim runs all evening");

        Assert.True(
            timesOver > 10,
            "the cap is only exceeded " + timesOver.ToString("0.0")
            + " times over an evening, which would make the trim a corner case "
            + "rather than the ordinary state");
    }
}
