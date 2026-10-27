using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// **THE SENDERS LINE NAMES THE LANE** (the owner's ruling on HM-DEC-295, 2026-10-27): with the sequence reader the path to the
/// printer, the capture sheet's `senders` line says the station it prints, its lane's pitch and width, its dit and separation,
/// and says so where it prints none.
/// </summary>
public sealed class TheSendersLineNamesTheLaneTests
{
    [Fact]
    public void ThePrintedStationIsNamedWithItsLane()
        => Assert.Equal(
            "675 Hz, a 15 Hz lane, dit 80 ms, separation 2.16, printed  (the sequence reader)",
            MainWindowViewModel.SequenceSendersForTheRecord(new CwSequenceReader.Station(675, 15, 80, 2.16)));

    [Fact]
    public void NoStationPrintedSaysSo()
        => Assert.Equal("none printed  (the sequence reader prints no station now)", MainWindowViewModel.SequenceSendersForTheRecord(null));
}
