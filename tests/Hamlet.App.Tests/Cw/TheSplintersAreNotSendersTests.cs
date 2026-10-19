using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// **SPLINTERS ARE NOT SENDERS** (work instruction 574, HM-DEC-278). On the owner's capture of 2026-10-19 the sheet's `senders`
/// line read `450 Hz shape 0.64, 29 marks; 450 Hz shape 0.19, 4 marks; 500 Hz shape 0.02, 445 marks; 500 Hz shape 0.00, 276
/// marks`, and the last two, KB2UZK's own pieces from the grid, made the four that tripped the four-senders capture trigger.
/// </summary>
public sealed class TheSplintersAreNotSendersTests
{
    private static CwShapeSideReading Held(params CwSenderStanding[] senders)
        => CwShapeSideReading.Nothing with { Senders = senders };

    /// <remarks>The capture's four: two senders held for the trigger and the sheet, two splinters listed after them.</remarks>
    [Fact]
    public void TheCapturesFourAreTwoSendersAndTwoSplinters()
    {
        var shape = Held(new(450, 0.64, 29, false), new(450, 0.19, 4, false), new(500, 0.02, 445, false), new(500, 0.00, 276, false));

        Assert.Equal(2, shape.HeldSenders.Count);
        Assert.Equal(
            "450 Hz shape 0.64, 29 marks; 450 Hz shape 0.19, 4 marks; splinters at 500 Hz, 445 marks; splinters at 500 Hz, 276 marks",
            MainWindowViewModel.SendersForTheRecord(shape));
    }

    /// <remarks>A printed sender is a sender whatever its shape, and a sheet of splinters alone says none held.</remarks>
    [Fact]
    public void APrintedSenderCountsAndSplintersAloneHoldNone()
    {
        Assert.Single(Held(new CwSenderStanding(600, 0.05, 40, true)).HeldSenders);
        Assert.Equal("none held; splinters at 500 Hz, 445 marks", MainWindowViewModel.SendersForTheRecord(Held(new CwSenderStanding(500, 0.02, 445, false))));
        Assert.Equal("none held", MainWindowViewModel.SendersForTheRecord(CwShapeSideReading.Nothing));
    }
}
