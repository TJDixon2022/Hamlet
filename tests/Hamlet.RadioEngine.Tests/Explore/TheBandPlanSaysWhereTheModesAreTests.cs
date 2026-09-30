using System;
using System.Linq;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// **The band plan is checked against the app's own frequencies** (work instruction 499,
/// HM-DEC-203). The map called W1AW's own Morse frequency on 40 m a data block, mode-follow put the
/// radio in USB-D at 3 kHz, and the CW terminal filled with junk, while
/// `data/bands/w1aw-morse.json` held the right answer and nothing held the two files to each other.
/// </summary>
public sealed class TheBandPlanSaysWhereTheModesAreTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each row prints what the map says at it.</param>
    public TheBandPlanSaysWhereTheModesAreTests(ITestOutputHelper output) => _output = output;

    /// <summary>What the map draws at a frequency, or null on a band Hamlet does not draw.</summary>
    private static Neighborhood? MapAt(long hz)
        => HfBands.BandFor(hz) is { } band
            ? NeighborhoodPlan.ForBand(band).FirstOrDefault(n => n.Contains(hz))
            : null;

    /// <remarks>
    /// Case 1: every one of W1AW's nine Morse frequencies is Morse on the map. Where Hamlet draws the
    /// band, the block there is a CW block; where it does not - 160 m, 6 m and 2 m - the map draws
    /// nothing there to disagree with, and the row says so.
    /// </remarks>
    [Fact]
    public void EveryW1awMorseFrequencyIsInACwBlockOfTheMap()
    {
        var rows = W1awMorseFrequencies.Default.Rows;
        var wrong = 0;

        Assert.Equal(9, rows.Count);

        foreach (var row in rows)
        {
            var here = MapAt(row.FrequencyHz);
            var says = HfBands.BandFor(row.FrequencyHz) is null
                ? "a band the map does not draw"
                : here is null ? "no block" : $"{here.ShortName} ({here.Family}), cite {here.Cite}";
            var right = HfBands.BandFor(row.FrequencyHz) is null || here?.Family == ModeFamily.Cw;

            _output.WriteLine($"{row.Band,-6} {row.FrequencyHz,10}  {says}  {(right ? "ok" : "WRONG")}");

            if (!right)
            {
                wrong++;
            }
        }

        Assert.Equal(0, wrong);
    }

    /// <summary>
    /// The ARRL band plan's ranges for the bands Hamlet draws, quoted from https://www.arrl.org/band-plan
    /// on 2026-09-29, and the family the map must draw inside each.
    /// </summary>
    /// <remarks>
    /// A single frequency in the plan - `7.040 RTTY/Data DX`, `14.230 SSTV`, an AM calling frequency -
    /// is a convention and is not a range, so it is not in this table. The last column names the
    /// cited rows the plan does not mention and that the map keeps inside a range: W1AW's own Morse
    /// frequency, which the ARRL itself sends Morse on inside its data range, and FT4 on 10 m, where
    /// WSJT-X's dial frequency sits inside the plan's CW range.
    /// </remarks>
    private static readonly (string Band, long LowHz, long HighHz, string Plan, ModeFamily Family, string[] Except)[] Plan =
    {
        ("80 m", 3_570_000, 3_600_000, "3.570-3.600 RTTY/Data", ModeFamily.Digital, new[] { "W1AW" }),
        ("40 m", 7_080_000, 7_125_000, "7.080-7.125 RTTY/Data", ModeFamily.Digital, Array.Empty<string>()),
        ("30 m", 10_130_000, 10_140_000, "10.130-10.140 RTTY", ModeFamily.Digital, Array.Empty<string>()),
        ("30 m", 10_140_000, 10_150_000, "10.140-10.150 Packet", ModeFamily.Digital, Array.Empty<string>()),
        ("20 m", 14_070_000, 14_095_000, "14.070-14.095 RTTY", ModeFamily.Digital, Array.Empty<string>()),
        ("20 m", 14_095_000, 14_099_500, "14.095-14.0995 Packet", ModeFamily.Digital, Array.Empty<string>()),
        ("20 m", 14_100_500, 14_112_000, "14.1005-14.112 Packet", ModeFamily.Digital, Array.Empty<string>()),
        ("17 m", 18_100_000, 18_105_000, "18.100-18.105 RTTY", ModeFamily.Digital, Array.Empty<string>()),
        ("17 m", 18_105_000, 18_110_000, "18.105-18.110 Packet", ModeFamily.Digital, Array.Empty<string>()),
        ("15 m", 21_070_000, 21_110_000, "21.070-21.110 RTTY/Data", ModeFamily.Digital, Array.Empty<string>()),
        ("10 m", 28_000_000, 28_070_000, "28.000-28.070 CW", ModeFamily.Cw, Array.Empty<string>()),
        ("10 m", 28_070_000, 28_150_000, "28.070-28.150 RTTY", ModeFamily.Digital, Array.Empty<string>()),
        ("10 m", 28_150_000, 28_190_000, "28.150-28.190 CW", ModeFamily.Cw, new[] { "FT4" }),
    };

    /// <remarks>
    /// Case 4: every band's blocks match the plan. Inside each of the ARRL's ranges the map draws the
    /// range's family at every 500 Hz, apart from the named rows; nothing is open ground, and nothing
    /// of another family.
    /// </remarks>
    [Fact]
    public void EveryBandsBlocksMatchTheArrlBandPlan()
    {
        var wrong = 0;

        foreach (var (band, low, high, plan, family, except) in Plan)
        {
            var off = 0;
            var names = new System.Collections.Generic.SortedSet<string>(StringComparer.Ordinal);

            for (var hz = low; hz < high; hz += 500)
            {
                var here = MapAt(hz);

                names.Add(here?.ShortName is { Length: > 0 } s ? s : here?.Name ?? "nothing");

                if (here is null || (here.Family != family && !except.Contains(here.ShortName) && !except.Contains(here.Name)))
                {
                    off++;
                }
            }

            _output.WriteLine($"{band,-5} {plan,-24} {family,-8} blocks: {string.Join(", ", names)}{(off > 0 ? $"  {off} points WRONG" : "")}");
            wrong += off;
        }

        Assert.Equal(0, wrong);
    }

    /// <remarks>
    /// And the source is named where the next person will look: every band Hamlet draws carries the
    /// ARRL band plan among its file's sources.
    /// </remarks>
    [Fact]
    public void TheArrlBandPlanIsCited()
    {
        var source = NeighborhoodData.Current.Sources.FirstOrDefault(s => s.Id == "arrl-bandplan");

        Assert.NotNull(source);
        Assert.Equal("https://www.arrl.org/band-plan", source!.Url);
    }
}
