using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The digital capture sheet identifies the radio, including what it could not
/// read.
/// </summary>
/// <remarks>
/// <para>**TASK 1'S ACCEPTANCE** (work instruction 041): the sheet alone must
/// identify the mode, the data flag and the passband width — the three fields
/// whose absence cost two hours on 2026-08-28.</para>
/// <para>**THE FAILURE STATE IS THE TEST.** The operator was in CW at 500 Hz
/// under a three-kilohertz block and no file Hamlet wrote said so. A sheet that
/// cannot describe that state has not fixed anything.</para>
/// </remarks>
public sealed class TheDigitalSheetIdentifiesTheRadioTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sheet is printed.</param>
    public TheDigitalSheetIdentifiesTheRadioTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Now =
        new(2026, 8, 28, 16, 41, 0, DateTimeKind.Utc);

    private static RigState State(params RigValue[] values)
        => RigState.Empty.With(values);

    /// <remarks>
    /// <para>**TODAY'S FAILURE STATE, WRITTEN DOWN.** CW, data off, FIL2 at
    /// 500 Hz, on a block that needs three kilohertz.</para>
    /// <para>The sheet has to name all of it, and has to say plainly that the
    /// window is too narrow — not leave a reader to compare two numbers.</para>
    /// </remarks>
    [Fact]
    public void TheSheetNamesTodaysFailureState()
    {
        var sheet = DigitalCaptureSheet.Compose(
            Now, 30.0, 48000,
            State(
                RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
                RigValue.Known(RigField.DataMode, 0, "off", Now, "CI-V 26 00"),
                RigValue.Known(RigField.FilterSelection, 2, "FIL2", Now, "CI-V 04"),
                RigValue.Known(RigField.FilterBandwidth, 500, "500 Hz", Now, "CI-V 1A 03"),
                RigValue.Known(RigField.Frequency, 14_074_000, "14.074000 MHz", Now, "CI-V 03")),
            new ClockOffset(0.12, Now.AddSeconds(-30)),
            Now,
            "FT8 city",
            3000);

        _output.WriteLine(sheet);

        Assert.Contains("mode       CW", sheet, StringComparison.Ordinal);
        Assert.Contains("dataMode   off", sheet, StringComparison.Ordinal);
        Assert.Contains("FIL2", sheet, StringComparison.Ordinal);

        // The sheet says it is too narrow rather than leaving two numbers to be
        // compared by whoever reads it.
        Assert.Contains("TOO NARROW", sheet, StringComparison.Ordinal);
        Assert.Contains("3000 Hz", sheet, StringComparison.Ordinal);

        Assert.Contains("trimmed    no", sheet, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**TASK 5 OF WORK INSTRUCTION 042: BOTH ENDS, LABELLED.** The sheet
    /// used to carry one line reading `captured 20:47:20` and nothing saying
    /// whether that was the start of the thirty seconds or the moment of the
    /// button.</para>
    /// <para>**THAT AMBIGUITY IS NOT COSMETIC.** Analysis of the operator's own
    /// file found FT8 keying on a clean fifteen-second cycle sitting 2.4 seconds
    /// off where a slot boundary should fall, and it could not be resolved,
    /// because a window read from the wrong end is out by its own length. Thirty
    /// seconds is two whole slots, so the error is invisible in the cycle and
    /// fatal to the alignment.</para>
    /// </remarks>
    [Fact]
    public void TheSheetNamesBothEndsAndWhichOneThePressIs()
    {
        var sheet = DigitalCaptureSheet.Compose(
            Now, 30.0, 48000, State(), ClockOffset.Unknown, Now, "FT8 city", 3000);

        _output.WriteLine(sheet);

        // The press is 16:41:00, so the window opened thirty seconds earlier.
        Assert.Contains(
            "windowFrom 2026-08-28 16:40:30 UTC", sheet, StringComparison.Ordinal);
        Assert.Contains(
            "windowTo   2026-08-28 16:41:00 UTC", sheet, StringComparison.Ordinal);

        // And which end the button was.
        Assert.Contains("press      2026-08-28 16:41:00 UTC", sheet, StringComparison.Ordinal);
        Assert.Contains("the END of the window", sheet, StringComparison.Ordinal);

        // The old ambiguous line is gone rather than kept beside the new ones,
        // because two lines for one instant is a second thing to drift (§0).
        Assert.DoesNotContain("captured   ", sheet, StringComparison.Ordinal);

        // A short window says so in both ends rather than only in the seconds.
        var brief = DigitalCaptureSheet.Compose(
            Now, 4.5, 48000, State(), ClockOffset.Unknown, Now, "FT8 city", 3000);

        Assert.Contains(
            "windowFrom 2026-08-28 16:40:55 UTC", brief, StringComparison.Ordinal);
        Assert.Contains("seconds    4.5", brief, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**AN UNREAD DATA FLAG SAYS SO AND DOES NOT SHOW A BARE MODE.**
    /// Showing `USB` when the flag was never read is the guess §0.0 forbids, and
    /// it is precisely the ambiguity that misled a reader today.</para>
    /// </remarks>
    [Fact]
    public void AnUnreadValueSaysSoRatherThanShowingAPlausibleOne()
    {
        var sheet = DigitalCaptureSheet.Compose(
            Now, 30.0, 48000,
            State(
                RigValue.Known(RigField.Mode, (int)CivMode.Usb, "USB", Now, "CI-V 04")),
            ClockOffset.Unknown,
            Now,
            "FT8 city",
            3000);

        _output.WriteLine(sheet);

        // The mode is read; the variant is not, and the sheet refuses to let the
        // first stand in for the second.
        Assert.Contains("mode       USB", sheet, StringComparison.Ordinal);
        Assert.Contains(
            "whether this is USB or USB-D is NOT established",
            sheet, StringComparison.Ordinal);

        // The passband is unread, so no claim is made about it either way.
        Assert.Contains(
            "the passband is not established", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("TOO NARROW", sheet, StringComparison.Ordinal);

        // And the clock, which was never queried.
        Assert.Contains("not checked", sheet, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the good state reads as good: USB-D, wide enough, and the sheet
    /// says the width covers the block rather than merely printing it.
    /// </remarks>
    [Fact]
    public void TheWorkingStateReadsAsWorking()
    {
        var sheet = DigitalCaptureSheet.Compose(
            Now, 30.0, 48000,
            State(
                RigValue.Known(RigField.Mode, (int)CivMode.Usb, "USB", Now, "CI-V 04"),
                RigValue.Known(RigField.DataMode, 1, "on", Now, "CI-V 26 00"),
                RigValue.Known(RigField.FilterSelection, 1, "FIL1", Now, "CI-V 04"),
                RigValue.Known(RigField.FilterBandwidth, 3000, "3.0 kHz", Now, "CI-V 1A 03")),
            new ClockOffset(-0.04, Now),
            Now,
            "FT8 city",
            3000);

        _output.WriteLine(sheet);

        Assert.Contains("dataMode   on", sheet, StringComparison.Ordinal);
        Assert.Contains("wide enough for the 3000 Hz", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("TOO NARROW", sheet, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **A BLOCK WITH NO STATED REQUIREMENT PRODUCES NO CLAIM EITHER WAY.**
    /// Ninety-three of the map's blocks state none, and inventing one for them
    /// would be a number this file does not carry (§12.4).
    /// </remarks>
    [Fact]
    public void ABlockWithNoRequirementIsNotJudged()
    {
        var sheet = DigitalCaptureSheet.Compose(
            Now, 30.0, 48000,
            State(
                RigValue.Known(RigField.FilterBandwidth, 500, "500 Hz", Now, "CI-V 1A 03")),
            ClockOffset.Unknown,
            Now,
            "CW fast lane",
            null);

        _output.WriteLine(sheet);

        Assert.Contains("500 Hz", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("TOO NARROW", sheet, StringComparison.Ordinal);
        Assert.Contains(
            "no passband requirement stated", sheet, StringComparison.Ordinal);
    }
}
