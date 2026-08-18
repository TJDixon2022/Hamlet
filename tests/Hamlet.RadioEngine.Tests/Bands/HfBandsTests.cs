using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Bands;

/// <summary>
/// The band plan, built from cited data rather than from memory (HM-DEC-110).
/// </summary>
/// <remarks>
/// <para>**THERE USED TO BE TWO BAND PLANS IN THIS TREE AND ONLY ONE OF THEM WAS
/// CITED**, which is the state §0 exists to prevent, and the uncited one had the
/// friendlier name. `BandPlan` carried seven bands of literals its own comment
/// marked as carried from general knowledge, and §0.2.1 forbids frequencies
/// asserted from a model's memory, so the scanner had to be built around it
/// rather than on it.</para>
/// <para>It is gone. Every number `HfBands` produces is derived from a citation,
/// and these are the tests that say so: if either data file moves, this fails on
/// the next run rather than on the evening somebody trusts the wrong one.</para>
/// </remarks>
public sealed class HfBandsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the derivation is printed.</param>
    public HfBandsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// <para>**THE WHOLE TABLE, PINNED.** Verified 2026-08-18 against the
    /// regulation itself rather than against the file that quotes it: the eCFR
    /// versioner API for title 47 as of 2026-08-01, §97.301(b) for the edges and
    /// §97.305(c) for the segments.</para>
    /// <para>**AND THE COLUMN MATTERS, WHICH IS WHY §4 INSISTS ON IT.** 97.301's
    /// tables carry ITU Regions 1, 2 and 3 side by side, and the United States is
    /// Region 2. Reading Region 1 by mistake would give 40 m as 7.000 to 7.200
    /// and 75 m as 3.600 to 3.800, both wrong here and both plausible enough to
    /// survive a review.</para>
    /// <para>Two rows in 97.305(c) say "Entire band" rather than a range, 80 m
    /// and 30 m, and the privileges file expands them from 97.301's own edges.
    /// That is correct: the regulation's 80 m is 3.5 to 3.6, with 3.6 to 4.0
    /// being a separate 75 m row.</para>
    /// </remarks>
    [Theory]
    [InlineData("80 m", 3_500_000, 4_000_000, 3_500_000, 3_600_000)]
    [InlineData("40 m", 7_000_000, 7_300_000, 7_000_000, 7_125_000)]
    [InlineData("30 m", 10_100_000, 10_150_000, 10_100_000, 10_150_000)]
    [InlineData("20 m", 14_000_000, 14_350_000, 14_000_000, 14_150_000)]
    [InlineData("17 m", 18_068_000, 18_168_000, 18_068_000, 18_110_000)]
    [InlineData("15 m", 21_000_000, 21_450_000, 21_000_000, 21_200_000)]
    [InlineData("10 m", 28_000_000, 29_700_000, 28_000_000, 28_300_000)]
    public void EveryEdgeAndSegmentComesFromTheRegulation(
        string name, long lowHz, long highHz, long cwLowHz, long cwHighHz)
    {
        var band = HfBands.Bands.Single(b => b.Name == name);

        _output.WriteLine($"{name}: band {band.LowHz / 1e6:0.000}-{band.HighHz / 1e6:0.000}, "
            + $"CW {band.CwLowHz / 1e6:0.000}-{band.CwHighHz / 1e6:0.000}");

        Assert.Equal(lowHz, band.LowHz);
        Assert.Equal(highHz, band.HighHz);
        Assert.Equal(cwLowHz, band.CwLowHz);
        Assert.Equal(cwHighHz, band.CwHighHz);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-110's own ruling: **a band button lands on the first
    /// "CW main street" block in the cited conventions.** Five of the seven
    /// already were that block, so the rule that moved the fewest dials is also
    /// the one that leaves nothing unsourced.</para>
    /// <para>**THE TWO THAT MOVED ARE NAMED HERE BECAUSE THEY MOVED UNDER
    /// SOMEBODY'S FINGERS.** 40 m goes from 7.030 to 7.028, and 30 m from 10.110,
    /// which matched no cited source at all, to 10.103.</para>
    /// </remarks>
    [Theory]
    [InlineData("80 m", 3_530_000)]
    [InlineData("40 m", 7_028_000)]
    [InlineData("30 m", 10_103_000)]
    [InlineData("20 m", 14_030_000)]
    [InlineData("17 m", 18_080_000)]
    [InlineData("15 m", 21_030_000)]
    [InlineData("10 m", 28_030_000)]
    public void EveryJumpSpotIsACitedMainStreetBlock(string name, long jumpHz)
    {
        var band = HfBands.Bands.Single(b => b.Name == name);

        _output.WriteLine($"{name} lands on {band.JumpHz / 1e6:0.000}");

        Assert.Equal(jumpHz, band.JumpHz);

        // AND IT IS ACTUALLY IN THE CONVENTIONS, rather than a number that
        // happens to match one. This is what would catch the jump spot drifting
        // away from the block it is supposed to name.
        var block = NeighborhoodData.Current.ForBand(name)
            .FirstOrDefault(h => h.Family == ModeFamily.Cw
                                 && h.Name == HfBands.LandingBlock);

        Assert.NotNull(block);
        Assert.Equal(block!.JumpHz, band.JumpHz);
    }

    /// <remarks>
    /// Proves §0.0 and §0.2.1: **every jump spot is somewhere the operator may
    /// actually transmit**, which is the check that would catch any of these
    /// numbers being simply wrong however well cited it looked.
    /// </remarks>
    [Fact]
    public void EveryJumpSpotIsInsideItsOwnCwSegmentAndAllowed()
    {
        var plan = new PrivilegePlan();

        foreach (var band in HfBands.Bands)
        {
            Assert.True(
                band.IsInCwSegment(band.JumpHz),
                $"{band.Name} lands outside its own CW segment");

            Assert.True(
                plan.MayTransmitAnyMode(LicenseClass.Extra, band.JumpHz),
                $"{band.Name} lands where the cited data allows nothing");
        }
    }

    /// <remarks>
    /// <para>**PROVES THE EDGES ARE THE REGULATION'S AND NOT NEARLY THE
    /// REGULATION'S.** A hertz outside each edge must be refused, or the edge is
    /// not where the CFR puts it, and this is the one place a confident error has
    /// legal consequences (HM-DEC-029).</para>
    /// </remarks>
    [Fact]
    public void AHertzOutsideEachEdgeIsOutsideTheBand()
    {
        var plan = new PrivilegePlan();

        foreach (var band in HfBands.Bands)
        {
            Assert.True(plan.MayTransmitAnyMode(LicenseClass.Extra, band.LowHz));
            Assert.True(plan.MayTransmitAnyMode(LicenseClass.Extra, band.HighHz));

            Assert.False(plan.MayTransmitAnyMode(LicenseClass.Extra, band.LowHz - 1));
            Assert.False(plan.MayTransmitAnyMode(LicenseClass.Extra, band.HighHz + 1));
        }
    }

    /// <remarks>
    /// <para>Proves the retirement itself: **the neighborhood file is not the
    /// source for a CW segment and cannot be made into one.** Its Morse rows fall
    /// short at the top of every band, by 10 kHz on 17 m up to 230 kHz on 10 m,
    /// and 40 m has a hole in the middle. That is not a defect in it: those rows
    /// are conventions somebody published, and the space between belongs to
    /// nobody (HM-DEC-054).</para>
    /// <para>It is worth a test rather than a comment because it is the mistake
    /// the next session would make, having seen the jump spots come from that
    /// file.</para>
    /// </remarks>
    [Fact]
    public void TheConventionsDoNotCoverTheSegmentsAndAreNotAskedTo()
    {
        var shortfalls = new List<string>();

        foreach (var band in HfBands.Bands)
        {
            var morse = NeighborhoodData.Current.ForBand(band.Name)
                .Where(h => h.Family == ModeFamily.Cw)
                .ToList();

            if (morse.Count == 0)
            {
                continue;
            }

            var top = morse.Max(h => h.HighHz);

            if (top < band.CwHighHz)
            {
                shortfalls.Add($"{band.Name} short by {(band.CwHighHz - top) / 1000} kHz");
            }
        }

        foreach (var line in shortfalls)
        {
            _output.WriteLine(line);
        }

        Assert.True(
            shortfalls.Count >= 5,
            "the conventions now reach the top of nearly every band, so the "
            + "reason the segments are taken from the regulation instead has "
            + "changed and this test's own argument needs re-reading");
    }

    /// <remarks>
    /// Proves §0: **the band plan holds no number that is not derived.** A
    /// literal creeping back in is the exact regression this file exists to
    /// catch, and grepping the source is the only way to catch it, because a
    /// hard-coded number that happens to be right passes every other test here.
    /// </remarks>
    [Fact]
    public void TheBandPlanSourceCarriesNoFrequencyLiteral()
    {
        var path = Path.Combine(
            Root(), "src", "Hamlet.RadioEngine", "Bands", "HfBands.cs");

        var code = File.ReadAllLines(path)
            .Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal))
            .Where(l => !l.TrimStart().StartsWith("///", StringComparison.Ordinal))
            .ToList();

        var offenders = code
            .Where(l => System.Text.RegularExpressions.Regex.IsMatch(
                l, @"\b\d[\d_]{5,}\b"))
            .ToList();

        foreach (var line in offenders)
        {
            _output.WriteLine(line.Trim());
        }

        Assert.True(
            offenders.Count == 0,
            $"{offenders.Count} frequency-shaped literals are back in the band "
            + "plan, which is the whole thing HM-DEC-110 removed");
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.RadioEngine")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
