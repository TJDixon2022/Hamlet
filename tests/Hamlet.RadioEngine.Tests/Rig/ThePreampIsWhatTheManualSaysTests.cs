using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 424 task 2, criterion 7.8: the CW preamp is what the radio's manual
/// states, with the page cited, and off when the front end reads overloading.
/// </summary>
/// <remarks>
/// <para>**THE MANUAL, `IC-7300_ENG_FM_12b`, PAGE 4-3** (HM-DEC-177, R74). P.AMP1 is
/// the wide dynamic range preamp, most effective for the HF low bands; P.AMP2 the
/// high-gain one, most effective for the 50 MHz bands; with strong signals the preamp
/// is turned off. Icom's sensitivity figures are quoted with preamp 1 on from 1.8 to
/// 29.999 MHz and preamp 2 on at 50 MHz.</para>
/// <para>**WATCHED FAILING AT 7.030**, where the old rule, *preamp 1 above 40 m, off
/// at 40 m and below*, left a radio at off and the manual says preamp 1.</para>
/// <para>**EVERY FREQUENCY IS DRIVEN THROUGH ITS OWN BLOCK WHERE THE MAP HAS ONE**,
/// exactly as the app hands the setup the block's conditions. 1.810 and 50.100 have
/// no block on the map, so the CW row is driven directly: that is what entering CW
/// there would write, and the app writes nothing there today.</para>
/// <para>**NO RADIO IS ON THIS MACHINE** (FACT-006): every write is against
/// <see cref="ScriptedRadio"/>.</para>
/// </remarks>
public sealed class ThePreampIsWhatTheManualSaysTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the writes are printed.</param>
    public ThePreampIsWhatTheManualSaysTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The red one.** At 7.030, in the QRP block, a radio at off is written to preamp 1.
    /// </summary>
    [Fact]
    public async Task At7030ARadioAtOffIsWrittenToPreamp1()
    {
        var (written, now) = await TuneInAsync(7_030_000, start: 0, overloading: false);

        Assert.Equal(new[] { 1 }, written);
        Assert.Equal(1, now);
    }

    /// <summary>
    /// One frequency on every HF band, each through the block the app finds there, and
    /// 1.810 through the CW row: preamp 1 from off and from preamp 2.
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <param name="start">Where the preamp was.</param>
    [Theory]
    [InlineData(1_810_000, 0)]
    [InlineData(3_530_000, 0)]
    [InlineData(7_030_000, 2)]
    [InlineData(10_110_000, 0)]
    [InlineData(14_050_000, 0)]
    [InlineData(14_050_000, 2)]
    [InlineData(18_080_000, 0)]
    [InlineData(21_050_000, 0)]
    [InlineData(28_050_000, 0)]
    public async Task OnEveryHfBandItIsPreamp1(long hz, byte start)
    {
        var (_, now) = await TuneInAsync(hz, start, overloading: false);

        Assert.Equal(1, now);
    }

    /// <summary>At 50.100, through the CW row, preamp 2.</summary>
    [Fact]
    public async Task At50MhzItIsPreamp2()
    {
        var (written, now) = await TuneInAsync(50_100_000, start: 0, overloading: false);

        Assert.Equal(new[] { 2 }, written);
        Assert.Equal(2, now);
    }

    /// <summary>
    /// With the front end reading overloading the preamp is off, on 40 m and on 20 m,
    /// whatever it was.
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <param name="start">Where the preamp was.</param>
    [Theory]
    [InlineData(7_030_000, 1)]
    [InlineData(14_050_000, 1)]
    [InlineData(14_050_000, 2)]
    [InlineData(28_050_000, 1)]
    public async Task OverloadingItIsOff(long hz, byte start)
    {
        var (_, now) = await TuneInAsync(hz, start, overloading: true);

        Assert.Equal(0, now);
    }

    /// <summary>
    /// Where the radio will not say whether it is overloading, the rule has no input and
    /// nothing is written (§0.0, §12.4).
    /// </summary>
    [Fact]
    public async Task WithTheOverloadUnreadNothingIsWritten()
    {
        var (written, now) = await TuneInAsync(14_050_000, start: 0, overloading: null);

        Assert.Empty(written);
        Assert.Equal(0, now);
    }

    /// <summary>The condition's own text cites the manual page it comes from.</summary>
    [Fact]
    public void TheConditionCitesTheManualPage()
    {
        var row = ReceiverConditions.ForMode("CW").Single(c => c.Field == RigField.Preamp);

        _output.WriteLine(row.WantedText);
        _output.WriteLine(row.Because);

        Assert.Contains("IC-7300", row.WantedText, StringComparison.Ordinal);
        Assert.Contains("page 4-3", row.WantedText, StringComparison.Ordinal);
        Assert.Contains("page 4-3", row.Because, StringComparison.Ordinal);
    }

    private async Task<(int[] Written, int Now)> TuneInAsync(long hz, byte start, bool? overloading)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);
        var conditions = (fromBlock.Count > 0 ? fromBlock : ReceiverConditions.ForMode("CW"))
            .Where(c => c.Field == RigField.Preamp)
            .ToList();

        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Switches[ModeEntryBench.Preamp] = start;
        radio.Overloading = overloading;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(rig, conditions, ReceiverSetupMemory.Empty);

        var written = ModeEntryBench.Writes(radio)
            .Where(w => w.Field == RigField.Preamp)
            .Select(w => w.Value)
            .ToArray();
        var now = radio.Switches[ModeEntryBench.Preamp];

        _output.WriteLine(
            $"{hz / 1e6:0.000} MHz ({(fromBlock.Count > 0 ? block!.ShortName : "no block, CW row")}), "
            + $"preamp {start} before, overflow {(overloading is { } o ? (o ? "overloading" : "not overloading") : "unread")}: "
            + $"wrote [{string.Join(",", written)}], filed {results.Single().Outcome}, radio now preamp {now}");

        return (written, now);
    }
}
