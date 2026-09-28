using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 424 task 2, criterion 7.8, as R98 left it (work instruction 486): the
/// CW preamp is what the owner has ruled, off on every band, with the manual's page kept
/// in the row's text as history.
/// </summary>
/// <remarks>
/// <para>**R98 (HM-DEC-191, 2026-09-28) REPLACED THE MANUAL'S RULE FOR MORSE.** Until then
/// the row carried `IC-7300_ENG_FM_12b` page 4-3 (HM-DEC-177, R74): preamp 1 from 1.8 to
/// 29.999 MHz, preamp 2 at 50 MHz, off while the front end reads overloading. The owner
/// ruled it off for Morse (*"I still hate the preamp crap."*), so the row is now a plain
/// constant, off, and these facts pin that instead. The class keeps its name because the
/// file does; what it holds the setup to is the row's own value, which is now the ruling's.</para>
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
    /// At 7.030, in the QRP block, a radio at preamp 1 is written off (R98, work
    /// instruction 486; under HM-DEC-177 this fact wrote a radio at off to preamp 1).
    /// </summary>
    [Fact]
    public async Task At7030ARadioAtPreamp1IsWrittenOff()
    {
        var (written, now) = await TuneInAsync(7_030_000, start: 1, overloading: false);

        Assert.Equal(new[] { 0 }, written);
        Assert.Equal(0, now);
    }

    /// <summary>
    /// One frequency on every HF band, each through the block the app finds there, and
    /// 1.810 through the CW row: off, from preamp 1 and from preamp 2 (R98, work
    /// instruction 486; this was preamp 1 under HM-DEC-177).
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <param name="start">Where the preamp was.</param>
    [Theory]
    [InlineData(1_810_000, 1)]
    [InlineData(3_530_000, 1)]
    [InlineData(7_030_000, 2)]
    [InlineData(10_110_000, 1)]
    [InlineData(14_050_000, 1)]
    [InlineData(14_050_000, 2)]
    [InlineData(18_080_000, 1)]
    [InlineData(21_050_000, 1)]
    [InlineData(28_050_000, 1)]
    public async Task OnEveryHfBandItIsOff(long hz, byte start)
    {
        var (written, now) = await TuneInAsync(hz, start, overloading: false);

        Assert.Equal(new[] { 0 }, written);
        Assert.Equal(0, now);
    }

    /// <summary>
    /// At 50.100, through the CW row, off as well (R98, work instruction 486; this was
    /// preamp 2 under HM-DEC-177).
    /// </summary>
    [Fact]
    public async Task At50MhzItIsOffToo()
    {
        var (written, now) = await TuneInAsync(50_100_000, start: 2, overloading: false);

        Assert.Equal(new[] { 0 }, written);
        Assert.Equal(0, now);
    }

    /// <summary>
    /// A radio already off is left alone, on HF and at 50 MHz: nothing is written
    /// (HM-DEC-174, R98, work instruction 486).
    /// </summary>
    /// <param name="hz">The dial.</param>
    [Theory]
    [InlineData(7_030_000)]
    [InlineData(14_050_000)]
    [InlineData(50_100_000)]
    public async Task ARadioAlreadyOffIsLeftAlone(long hz)
    {
        var (written, now) = await TuneInAsync(hz, start: 0, overloading: false);

        Assert.Empty(written);
        Assert.Equal(0, now);
    }

    /// <summary>
    /// With the front end reading overloading the preamp is off, on 40 m and on 20 m,
    /// whatever it was. Under R98 it is off whether or not the front end overloads.
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
    /// Where the radio will not say whether it is overloading, the preamp is still written
    /// off (R98, work instruction 486). Under HM-DEC-177 the overflow flag was the rule's
    /// input and an unread flag wrote nothing; the row is now a constant and reads no flag,
    /// so an unread one withholds nothing.
    /// </summary>
    [Fact]
    public async Task WithTheOverloadUnreadItIsStillWrittenOff()
    {
        var (written, now) = await TuneInAsync(14_050_000, start: 1, overloading: null);

        Assert.Equal(new[] { 0 }, written);
        Assert.Equal(0, now);
    }

    /// <summary>
    /// The condition is the owner's ruling: off, confirmed, a constant, and its text cites
    /// R98 and HM-DEC-191 while keeping the manual page as history (work instruction 486;
    /// until then this fact asserted the manual page was the source of the value).
    /// </summary>
    [Fact]
    public void TheConditionIsTheOwnersRulingAndKeepsTheManualPage()
    {
        var row = ReceiverConditions.ForMode("CW").Single(c => c.Field == RigField.Preamp);

        _output.WriteLine(row.WantedText);
        _output.WriteLine(row.Because);

        Assert.Equal(0, row.Wanted);
        Assert.Equal("off", row.WantedText);
        Assert.True(row.Confirmed);
        Assert.False(row.IsConditional);
        Assert.Contains("R98", row.Because, StringComparison.Ordinal);
        Assert.Contains("HM-DEC-191", row.Because, StringComparison.Ordinal);
        Assert.Contains("IC-7300", row.Because, StringComparison.Ordinal);
        Assert.Contains("page 4-3", row.Because, StringComparison.Ordinal);
    }

    /// <summary>
    /// No Morse row carries an overload rule for the preamp, so nothing follows the
    /// overload in CW, CW DX or QRP (R98, work instruction 486).
    /// </summary>
    /// <param name="mode">The mode.</param>
    [Theory]
    [InlineData("CW")]
    [InlineData("CW DX")]
    [InlineData("QRP")]
    public void NoMorseRowCarriesAnOverloadRule(string mode)
    {
        var row = ReceiverConditions.ForMode(mode).Single(c => c.Field == RigField.Preamp);

        Assert.Equal(0, row.Wanted);
        Assert.Null(row.WhenOverloading);
        Assert.Empty(row.Bands);
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
