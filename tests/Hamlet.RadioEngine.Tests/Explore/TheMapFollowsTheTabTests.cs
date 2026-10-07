using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// **THE MAP FOLLOWS THE TAB** (work instruction 551, HM-DEC-255). The owner, 2026-10-07, on the CW tab at 7.0475 MHz: *"Why
/// is this region purple? It is CW, right?"* and *"When we are on the CW tab it should reflect CW, and on the Data tab it
/// should reflect data."*
/// </summary>
public sealed class TheMapFollowsTheTabTests(ITestOutputHelper output)
{
    private static readonly PrivilegePlan Plan = new();

    private static IReadOnlyList<Neighborhood> FortyMetres() => NeighborhoodPlan.WithEdges(HfBands.BandFor(7_047_500)!);

    private static Neighborhood At(IReadOnlyList<Neighborhood> map, long hz) => map.First(n => n.Contains(hz));

    /// <remarks>Prints every block of 40 m, whether it is shared for a General and for an Extra, and what each tab makes it.</remarks>
    [Theory]
    [InlineData(LicenseClass.General)]
    [InlineData(LicenseClass.Extra)]
    public void FortyMetresOnEachTab(LicenseClass licenseClass)
    {
        var raw = FortyMetres();
        var cw = ModeLens.On(raw, "CW", Plan, licenseClass);
        var digital = ModeLens.On(raw, "Digital", Plan, licenseClass);
        var voice = ModeLens.On(raw, "Voice", Plan, licenseClass);

        output.WriteLine($"40 m for a {licenseClass}: block | kHz | data says | shared | CW tab | Digital tab | Voice tab");

        for (var i = 0; i < raw.Count; i++)
        {
            output.WriteLine(
                $"{raw[i].Name} ({raw[i].ShortName}) | {raw[i].LowHz / 1000.0:0.00}-{raw[i].HighHz / 1000.0:0.00} | {raw[i].Family} | "
                + $"{(ModeLens.IsShared(raw[i], Plan, licenseClass) ? "shared" : "only one")} | "
                + $"{cw[i].Family} {cw[i].ShortName} | {digital[i].Family} {digital[i].ShortName} | {voice[i].Family} {voice[i].ShortName}");
        }

        // The stretch around W1AW's 7.0475: RTTY row, the W1AW block and FT4 sprint.
        foreach (var hz in new long[] { 7_045_000, 7_047_500, 7_049_000 })
        {
            Assert.Equal(ModeFamily.Cw, At(cw, hz).Family);
            Assert.Equal(ModeLens.CwName, At(cw, hz).ShortName);
            Assert.Equal(ModeFamily.Digital, At(digital, hz).Family);
        }

        // On the Digital tab the W1AW block is named Data; RTTY row and FT4 sprint keep their own names, being data already.
        Assert.Equal(ModeLens.DataName, At(digital, 7_047_500).ShortName);
        Assert.Equal("RTTY", At(digital, 7_045_000).ShortName);

        // The SSB end and past the band's edges are one thing, the same on all three tabs; the Voice tab is the data's own.
        foreach (var hz in new long[] { 7_250_000, 6_990_000, 7_310_000 })
        {
            Assert.Equal(At(raw, hz), At(cw, hz));
            Assert.Equal(At(raw, hz), At(digital, hz));
        }

        Assert.Equal(raw, voice);
        Assert.Equal(ModeFamily.Phone, At(cw, 7_250_000).Family);
        Assert.Equal(ModeFamily.OutsideTheBand, At(digital, 6_990_000).Family);
    }

    /// <remarks>A DX window a General may not transmit in is listen-only for him and is not shared: it keeps its colour.</remarks>
    [Fact]
    public void AListenOnlyDxWindowDoesNotChange()
    {
        var raw = FortyMetres();
        var window = At(raw, 7_010_000);

        Assert.False(ModeLens.IsShared(window, Plan, LicenseClass.General));
        Assert.Equal(window, At(ModeLens.On(raw, "Digital", Plan, LicenseClass.General), 7_010_000));
        Assert.True(ModeLens.IsShared(window, Plan, LicenseClass.Extra));
    }
}
