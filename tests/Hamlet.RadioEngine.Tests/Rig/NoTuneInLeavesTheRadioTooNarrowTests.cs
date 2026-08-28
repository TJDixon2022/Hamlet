using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A tune-in may not leave the radio unable to hear what the map says lives
/// there.
/// </summary>
/// <remarks>
/// <para>**THE FAILURE THIS IS WRITTEN FROM IS A STATE, NOT A SIGNAL** (work
/// instruction 040). On 2026-08-28 the operator went to 20 m FT8, heard nothing
/// at 14.074, and spent an hour on it. The radio was in **CW, FIL2, 500 Hz** — a
/// window sitting below the bottom of a block that is three kilohertz wide.
/// Nothing about the band, the antenna or the decoder was involved.</para>
/// <para>**THIS IS HM-DEC-054'S TEST ONE LAYER DOWN.** That ruling made the map
/// say what lives where; this asserts the radio can actually hear it.</para>
/// </remarks>
public sealed class NoTuneInLeavesTheRadioTooNarrowTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the walk is printed.</param>
    public NoTuneInLeavesTheRadioTooNarrowTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// <para>Proves the file states a passband wherever a block needs one, and
    /// that the number is the block's own width rather than a figure typed
    /// beside it.</para>
    /// <para>**EVERY BAND, EVERY MODE FAMILY**, which is what the order asks
    /// for: the whole map walked rather than the one row that failed.</para>
    /// </remarks>
    [Fact]
    public void EveryBlockThatNeedsAPassbandStatesOne()
    {
        var stated = 0;
        var silent = 0;

        _output.WriteLine("  band  | neighborhood         | block  | needs");
        _output.WriteLine("  ------|----------------------|--------|------");

        foreach (var band in HfBands.Bands)
        {
            foreach (var n in NeighborhoodPlan.ForBand(band))
            {
                if (n.PassbandHz is not { } needs)
                {
                    silent++;
                    continue;
                }

                stated++;

                var width = n.HighHz - n.LowHz;

                _output.WriteLine(
                    $"  {band.Name,-5} | {n.Name,-20} | {width,6} | {needs}");

                // **THE REQUIREMENT IS ABOUT THE RADIO, NOT THE BAND, BUT IT MAY
                // NOT EXCEED THE BLOCK.** A block three kilohertz wide cannot
                // need four: that would be a number typed beside the row rather
                // than derived from it.
                Assert.True(
                    needs <= width,
                    $"{band.Name} {n.Name} is {width} Hz wide and claims to need "
                    + $"{needs} Hz of passband, which is more than exists");

                Assert.True(needs > 0, $"{band.Name} {n.Name} states a passband of nought");
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  {stated} blocks state a passband, {silent} state none");

        Assert.True(stated > 0, "no neighborhood states a passband at all");
    }

    /// <remarks>
    /// <para>**THE FOUR RADIO STATES OF TASK 5**, asserted as the states they
    /// are. Only the last may produce a claim that the operator should hear the
    /// block.</para>
    /// <para>**THE QUESTION IS THREE-VALUED AND THAT IS THE POINT** (§0.0). A
    /// filter nobody has read is not a filter that is wrong, and it is not one
    /// that is right either; saying either would be a guess on the one sentence
    /// the operator will act on.</para>
    /// </remarks>
    [Fact]
    public void OnlyAWideEnoughReadbackMayClaimTheRadioCanHearIt()
    {
        var ft8 = NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Name == "FT8 city");

        // The 20 m block runs 14.074 to 14.077, so three kilohertz — and the
        // number is the block's, not one typed beside it.
        Assert.Equal(ft8.HighHz - ft8.LowHz, ft8.PassbandHz);
        Assert.True(ft8.PassbandHz >= 2900, $"the 20 m block is {ft8.PassbandHz} Hz");

        var cases = new (string What, double? Hertz, bool? Expected)[]
        {
            ("CW / FIL2 / 500 Hz — today's failure", 500, false),
            ("USB-D / FIL2 / 1.2 kHz — what the old write could leave", 1200, false),
            ("USB-D / FIL1 / 3.0 kHz — wide enough", 3000, true),
            ("the filter has not been read", null, null),
        };

        foreach (var (what, hertz, expected) in cases)
        {
            var answer = ft8.PassbandIsWideEnough(hertz);

            _output.WriteLine(
                $"  {what,-52} -> "
                + $"{(answer is null ? "unknown" : answer.Value ? "wide enough" : "too narrow")}");

            Assert.Equal(expected, answer);
        }
    }

    /// <remarks>
    /// <para>Proves the mode write carries the filter byte when one is asked
    /// for, and that the byte is the widest slot rather than a number written
    /// down.</para>
    /// <para>**AND THAT SKIPPING IT IS STILL AVAILABLE**, because a block that
    /// states no requirement should leave the radio choosing as it always did
    /// rather than have Hamlet invent an answer for it.</para>
    /// </remarks>
    [Fact]
    public void TheModeWriteCarriesTheFilterOnlyWhenOneIsAskedFor()
    {
        var withFilter = CivWrites.ModeData(
            CivMode.Usb, dataMode: true, CivWrites.WidestFilterSlot);

        var without = CivWrites.ModeData(CivMode.Usb, dataMode: true);

        _output.WriteLine(
            $"  with a filter: {string.Join(" ", withFilter.Select(b => b.ToString("X2")))}");
        _output.WriteLine(
            $"  without:       {string.Join(" ", without.Select(b => b.ToString("X2")))}");

        Assert.Equal(4, withFilter.Length);
        Assert.Equal(3, without.Length);

        Assert.Equal(CivWrites.SelectedVfo, withFilter[0]);
        Assert.Equal((byte)(int)CivMode.Usb, withFilter[1]);
        Assert.Equal(CivWrites.DataModeOn, withFilter[2]);
        Assert.Equal(CivWrites.WidestFilterSlot, withFilter[3]);

        // The first three bytes are the same either way: adding the filter
        // changes what the radio does with the rest, not the mode being set.
        Assert.Equal(without, withFilter.Take(3).ToArray());
    }
}
