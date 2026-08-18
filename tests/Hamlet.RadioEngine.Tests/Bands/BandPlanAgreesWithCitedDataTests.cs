using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Bands;

/// <summary>
/// The uncited band plan against the cited one (HM-OPEN-005, HM-DEC-107 phase 7
/// of the UI order).
/// </summary>
/// <remarks>
/// <para>**THERE ARE TWO BAND PLANS IN THIS TREE AND ONLY ONE OF THEM IS
/// CITED**, which is the state §0 exists to prevent, and the uncited one has the
/// friendlier name. `BandPlan.Bands` carries seven bands of literals its own
/// comment marks as carried from general knowledge.</para>
/// <para>It matters more than it used to. §0.2.1 forbids frequencies asserted
/// from a model's memory, so the scanner was built around `BandPlan` rather than
/// on it, and its segments come from the cited neighborhood data instead.</para>
/// <para>**NOTHING HAS BEEN MIGRATED AND THIS IS NOT THE MIGRATION.** It is the
/// measurement that says whether one is possible, kept as a test so the answer
/// cannot rot: if either file moves, this says so on the next run rather than on
/// the evening somebody trusts the wrong one.</para>
/// </remarks>
public sealed class BandPlanAgreesWithCitedDataTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is printed.</param>
    public BandPlanAgreesWithCitedDataTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// <para>**PROVES THE BAND EDGES ARE REDUNDANT.** Every `LowHz` and `HighHz`
    /// in `BandPlan` is exactly the Extra class's own range in
    /// `data/privileges/us-part97-privileges.json`, cited to 97.301(b), which by
    /// definition reaches every band edge. 80 m is the regulation's 80 m and
    /// 75 m rows joined, and that is the only join needed.</para>
    /// </remarks>
    [Fact]
    public void EveryBandEdgeIsAlreadyInTheCitedPrivilegeData()
    {
        var plan = new PrivilegePlan();
        var offenders = new List<string>();

        foreach (var band in BandPlan.Bands)
        {
            var lowInside = plan.MayTransmitAnyMode(LicenseClass.Extra, band.LowHz);
            var highInside = plan.MayTransmitAnyMode(LicenseClass.Extra, band.HighHz);

            // A hertz outside each edge must not be allowed, or the edge is not
            // where the regulation puts it.
            var belowOut = !plan.MayTransmitAnyMode(LicenseClass.Extra, band.LowHz - 1);
            var aboveOut = !plan.MayTransmitAnyMode(LicenseClass.Extra, band.HighHz + 1);

            _output.WriteLine($"{band.Name,-6} {band.LowHz / 1e6:0.000}-"
                + $"{band.HighHz / 1e6:0.000}  inside={lowInside}/{highInside}  "
                + $"outside={belowOut}/{aboveOut}");

            if (!lowInside || !highInside || !belowOut || !aboveOut)
            {
                offenders.Add(band.Name);
            }
        }

        Assert.True(
            offenders.Count == 0,
            "these bands' edges are not where the cited privilege data puts "
            + $"them: {string.Join(", ", offenders)}");
    }

    /// <remarks>
    /// <para>**AND THE CW SEGMENTS ARE REDUNDANT TOO, WHICH CORRECTS THE
    /// RECORD.** HM-OPEN-005 said they are convention rather than regulation and
    /// do not derive from the privilege boundaries. They do: every one is the
    /// union of the ranges carrying Data in the same cited file, to the hertz.
    /// 40 m needs two of them joined because the phone segment overlaps the
    /// first.</para>
    /// <para>What does **not** cover them is the neighborhood file. Its Morse
    /// rows fall short at the top of every band, by 10 kHz on 17 m and 230 kHz on
    /// 10 m, and 40 m has a hole in the middle. That is not a defect in it:
    /// those rows are places somebody published a convention for, and the space
    /// between belongs to nobody (HM-DEC-054). A CW segment is a regulatory
    /// boundary and the privileges file is its source.</para>
    /// </remarks>
    [Theory]
    [InlineData("80 m")]
    [InlineData("40 m")]
    [InlineData("30 m")]
    [InlineData("20 m")]
    [InlineData("17 m")]
    [InlineData("15 m")]
    [InlineData("10 m")]
    public void EveryCwSegmentIsAlreadyInTheCitedPrivilegeData(string name)
    {
        var band = BandPlan.Bands.Single(b => b.Name == name);
        var plan = new PrivilegePlan();

        // Data is permitted right across the CW segment and stops at its top.
        var lowOk = plan.MayTransmitAnyMode(LicenseClass.Extra, band.CwLowHz);
        var highOk = plan.MayTransmitAnyMode(LicenseClass.Extra, band.CwHighHz);

        _output.WriteLine($"{name}: CW segment {band.CwLowHz / 1e6:0.000}-"
            + $"{band.CwHighHz / 1e6:0.000}, allowed={lowOk}/{highOk}");

        Assert.True(lowOk && highOk,
            $"{name}'s CW segment reaches outside what the cited data allows");

        // AND IT LIES INSIDE THE BAND, which is the arithmetic that would catch
        // a transcription slip in either file.
        Assert.InRange(band.CwLowHz, band.LowHz, band.HighHz);
        Assert.InRange(band.CwHighHz, band.LowHz, band.HighHz);
    }

    /// <remarks>
    /// <para>**THE ONE THING THAT DOES NOT DERIVE, AND WHY THE MIGRATION
    /// STOPPED.** A jump spot is where a band button lands, and the cited data
    /// offers several candidates per band with no rule saying which. Five of the
    /// seven current spots are exactly a "CW main street" block; 40 m is the QRP
    /// watering hole's instead; and 30 m matches nothing cited at all, landing on
    /// 10.110 where the blocks are 10.103, 10.106 and 10.120.</para>
    /// <para>Choosing between those rules changes where a band button lands on
    /// between three and seven bands, which weighs cited data against the
    /// operator's muscle memory. That is a trade-off, so it is Tim's (§12.1),
    /// and a half-migrated band plan is worse than two whole ones.</para>
    /// <para>What this pins is only that a jump spot is somewhere sane: inside
    /// its own CW segment. It is the check that would have caught the fault if
    /// one of these numbers were simply wrong.</para>
    /// </remarks>
    [Fact]
    public void EveryJumpSpotAtLeastLandsInsideItsOwnCwSegment()
    {
        foreach (var band in BandPlan.Bands)
        {
            _output.WriteLine($"{band.Name,-6} jumps to {band.JumpHz / 1e6:0.000}, "
                + $"CW segment {band.CwLowHz / 1e6:0.000}-{band.CwHighHz / 1e6:0.000}");

            Assert.True(
                band.IsInCwSegment(band.JumpHz),
                $"{band.Name} lands on {band.JumpHz} Hz, which is outside its own "
                + "CW segment");
        }
    }
}
