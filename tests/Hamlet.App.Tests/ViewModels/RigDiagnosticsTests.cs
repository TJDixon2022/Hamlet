using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The diagnostics screen: every field listed, provenance on each, and a paste
/// that carries no more than it should (HM-DEC-050, HM-DEC-019).
/// </summary>
public sealed class RigDiagnosticsTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static RigState Populated() => RigState.Empty
        .WithRigName("IC-7300")
        .With(new[]
        {
            RigValue.Known(RigField.Frequency, 7_030_000, "7.030000 MHz", Now, "transceive 00"),
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.FilterSelection, 1, "FIL1", Now, "CI-V 04"),
            RigValue.Known(RigField.FilterBandwidth, 3000, "3 kHz", Now, "CI-V 1A 03"),
            RigValue.Known(RigField.SMeter, 90, "S7", Now.AddMinutes(-5), "CI-V 15 02"),
            RigValue.Unsupported(RigField.Vfo, "no read documented"),
        });

    /// <remarks>
    /// Proves every field gets a row, including the ones nothing is known about.
    /// A screen that listed only what it had would leave somebody wondering
    /// whether a missing row meant "not read" or "not a thing", which is the
    /// question the screen exists to answer.
    /// </remarks>
    [Fact]
    public void EveryFieldGetsARowEvenWhenNothingIsKnown()
    {
        var vm = new RigDiagnosticsViewModel(null, RigState.Empty);

        Assert.Equal(Enum.GetValues<RigField>().Length, vm.Rows.Count);
        Assert.All(vm.Rows, r => Assert.False(string.IsNullOrWhiteSpace(r.Label)));
        Assert.All(vm.Rows, r => Assert.Equal(RigValueState.Unknown, r.State));
        Assert.Equal("no radio connected", vm.RigName);
    }

    /// <remarks>
    /// Proves the summary counts what was actually read rather than what was
    /// listed. "2 of 28" is the honest headline when a training radio is
    /// connected, and it is the number that says at a glance whether Hamlet can
    /// see anything at all.
    /// </remarks>
    [Fact]
    public void TheSummaryCountsOnlyWhatWasReallyRead()
    {
        var vm = new RigDiagnosticsViewModel(null, Populated());

        Assert.Equal("IC-7300", vm.RigName);
        Assert.Contains("5 of", vm.Summary, StringComparison.Ordinal);
        Assert.Contains(Enum.GetValues<RigField>().Length.ToString(), vm.Summary,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a stale row is marked and a fresh one is not. §0.0.1 wants a wrong
    /// value to arrive with its provenance, and "S7, five minutes ago" is a very
    /// different claim from "S7".
    /// </remarks>
    [Fact]
    public void AStaleRowIsMarkedAndCarriesItsAge()
    {
        var vm = new RigDiagnosticsViewModel(null, Populated());

        var meter = vm.Rows.Single(r => r.Label == "S-meter");
        var mode = vm.Rows.Single(r => r.Label == "Mode");

        Assert.True(meter.IsStale);
        Assert.False(meter.IsCurrent);
        Assert.Contains("minutes ago", meter.Age, StringComparison.Ordinal);

        Assert.False(mode.IsStale);
        Assert.True(mode.IsCurrent);
    }

    /// <remarks>
    /// Proves every row says where it came from, which is what makes the screen
    /// worth attaching to a bug report rather than only worth looking at.
    /// </remarks>
    [Fact]
    public void EveryRowSaysWhereItCameFrom()
    {
        var vm = new RigDiagnosticsViewModel(null, Populated());

        Assert.All(vm.Rows, r => Assert.False(string.IsNullOrWhiteSpace(r.Source)));
        Assert.Equal("CI-V 1A 03", vm.Rows.Single(r => r.Label == "Filter width").Source);
        Assert.Equal("transceive 00", vm.Rows.Single(r => r.Label == "Frequency").Source);
    }

    /// <remarks>
    /// Proves an observation Hamlet can justify reaches the screen. The filter
    /// wide open while the radio is in Morse is the exact case that cost an
    /// evening.
    /// </remarks>
    [Fact]
    public void AJustifiedObservationReachesTheScreen()
    {
        var vm = new RigDiagnosticsViewModel(null, Populated());

        Assert.True(vm.HasObservations);
        Assert.Contains(vm.Observations, o => o.Contains("3 kHz", StringComparison.Ordinal));
    }

    /// <remarks>
    /// Proves nothing is said when nothing has been read, so the panel is absent
    /// rather than empty.
    /// </remarks>
    [Fact]
    public void NothingIsObservedFromAnEmptyState()
    {
        var vm = new RigDiagnosticsViewModel(null, RigState.Empty);

        Assert.False(vm.HasObservations);
        Assert.Empty(vm.Observations);
    }

    /// <remarks>
    /// THE PASTE CARRIES NO MORE THAN IT SHOULD (HM-DEC-019). This text is
    /// written to end up in a public issue tracker, so it holds radio state and
    /// nothing about the operator: no callsign, no name, no location, no grid.
    /// The frequency is in it and belongs there, because what a radio is tuned
    /// to is the whole question a decode bug turns on.
    /// </remarks>
    [Fact]
    public void TheBugReportPasteCarriesRadioStateAndNothingAboutTheOperator()
    {
        var vm = new RigDiagnosticsViewModel(null, Populated());
        var text = vm.CopyText;

        Assert.Contains("IC-7300", text, StringComparison.Ordinal);
        Assert.Contains("Filter width", text, StringComparison.Ordinal);
        Assert.Contains("CI-V 1A 03", text, StringComparison.Ordinal);
        Assert.Contains("[stale]", text, StringComparison.Ordinal);

        foreach (var identifying in new[] { "KC3QIS", "Timothy", "Pittsburgh", "FN00" })
        {
            Assert.DoesNotContain(identifying, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// THE ROW THAT DENIED WHAT THE APP WAS HOLDING (HM-DEC-053). The Frequency
    /// row read "not on this radio" while the rig display beside it showed the
    /// live frequency, because the field is fed by the radio's broadcasts rather
    /// than by a poll and the sweep read that absence as an absent feature. A
    /// broadcast-fed field is a supported, populated field whose provenance is
    /// the broadcast, and it carries its age like every other row.
    /// </remarks>
    [Fact]
    public void ABroadcastFrequencyShowsItsValueItsProvenanceAndItsAge()
    {
        var state = RigState.Empty
            .WithRigName("IC-7300")
            .With(RigValue.Known(
                RigField.Frequency, 14_074_000, "14.074000 MHz",
                Now.AddSeconds(-20), "transceive 00"));

        var row = new RigDiagnosticsViewModel(null, state)
            .Rows.Single(r => r.Label == "Frequency");

        Assert.Equal(RigValueState.Known, row.State);
        Assert.Contains("14.074", row.Value, StringComparison.Ordinal);
        Assert.Equal("transceive 00", row.Source);
        Assert.Contains("seconds ago", row.Age, StringComparison.Ordinal);

        Assert.DoesNotContain("not on this radio", row.Value, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the pre-broadcast state reads unknown and not unsupported. Nothing
    /// is pushed until the operator moves the dial or the connect sweep asks, and
    /// "nobody has heard yet" is a different claim from "this radio has no
    /// frequency" (HM-DEC-030).
    /// </remarks>
    [Fact]
    public void BeforeAnythingArrivesTheFrequencyRowSaysUnknown()
    {
        var row = new RigDiagnosticsViewModel(null, RigState.Empty.WithRigName("IC-7300"))
            .Rows.Single(r => r.Label == "Frequency");

        Assert.Equal(RigValueState.Unknown, row.State);
        Assert.Equal("unknown", row.Value);
        Assert.Equal("", row.Age);
    }

    /// <remarks>
    /// Proves the screen can be built with no monitor behind it, which is what
    /// the designer and the tests do, and that asking it to refresh then does
    /// nothing rather than throwing.
    /// </remarks>
    [Fact]
    public async Task RefreshingWithoutARadioDoesNothingRatherThanFailing()
    {
        var vm = new RigDiagnosticsViewModel();

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.NotEmpty(vm.Rows);
    }
}
