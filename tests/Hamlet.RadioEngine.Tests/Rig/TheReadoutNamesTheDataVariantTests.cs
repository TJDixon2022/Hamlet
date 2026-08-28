using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The readout distinguishes USB from USB-D, and says when it does not know.
/// </summary>
/// <remarks>
/// <para>**TASK 3 OF WORK INSTRUCTION 041.** The operator reported the rig
/// readout saying `USB` while the radio was in USB-D. Hamlet had read the flag
/// from `26 00` all along and showed it correctly in the "What the radio is
/// doing" window — **two surfaces disagreeing about one measured fact.**</para>
/// <para>**AND THE UNREAD CASE IS THE ONE THAT MATTERS MOST.** Printing the bare
/// mode when nobody has read the variant is the guess §0.0 forbids, and it is
/// what misled a reader for an hour on 2026-08-28.</para>
/// </remarks>
public sealed class TheReadoutNamesTheDataVariantTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the renderings are printed.</param>
    public TheReadoutNamesTheDataVariantTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Now =
        new(2026, 8, 28, 16, 41, 0, DateTimeKind.Utc);

    private static RigState With(params RigValue[] values)
        => RigState.Empty.With(values);

    private static RigValue Mode(CivMode mode, string text)
        => RigValue.Known(RigField.Mode, (int)mode, text, Now, "CI-V 04");

    private static RigValue Data(int on)
        => RigValue.Known(RigField.DataMode, on, on == 1 ? "on" : "off", Now, "CI-V 26 00");

    /// <remarks>
    /// Proves the three answers, and that the third is visibly not the first.
    /// </remarks>
    [Fact]
    public void TheVariantIsShownAndAnUnreadFlagSaysSo()
    {
        var dataOn = With(Mode(CivMode.Usb, "USB"), Data(1));
        var dataOff = With(Mode(CivMode.Usb, "USB"), Data(0));
        var unread = With(Mode(CivMode.Usb, "USB"));

        _output.WriteLine($"  flag on      -> {dataOn.ModeWithVariant}");
        _output.WriteLine($"  flag off     -> {dataOff.ModeWithVariant}");
        _output.WriteLine($"  flag unread  -> {unread.ModeWithVariant}");

        Assert.Equal("USB-D", dataOn.ModeWithVariant);
        Assert.Equal("USB", dataOff.ModeWithVariant);

        // **THE UNREAD CASE MUST NOT LOOK LIKE EITHER OF THE OTHER TWO**, which
        // is the whole point: it was showing as the bare mode and was believed.
        Assert.NotEqual("USB", unread.ModeWithVariant);
        Assert.NotEqual("USB-D", unread.ModeWithVariant);
        Assert.Equal("USB-?", unread.ModeWithVariant);
    }

    /// <remarks>
    /// Proves the rule is the same for every mode rather than special-cased for
    /// USB, and that an unread mode produces nothing rather than a placeholder.
    /// </remarks>
    [Fact]
    public void TheSameRuleAppliesToEveryModeAndAnUnreadModeSaysNothing()
    {
        Assert.Equal(
            "CW", With(Mode(CivMode.Cw, "CW"), Data(0)).ModeWithVariant);

        Assert.Equal(
            "RTTY-D", With(Mode(CivMode.Rtty, "RTTY"), Data(1)).ModeWithVariant);

        _output.WriteLine(
            "  CW with the flag off  -> "
            + With(Mode(CivMode.Cw, "CW"), Data(0)).ModeWithVariant);

        // Nothing read at all: the readout shows nothing rather than inventing a
        // mode to hang a variant on.
        // **NULL, NOT EMPTY.** An empty string reads as "nothing is set"; null
        // is the absence of a reading, and RigUnknownStateTests enforces that
        // distinction across the whole type.
        Assert.Null(RigState.Empty.ModeWithVariant);
        Assert.Null(With(Data(1)).ModeWithVariant);
    }
}
