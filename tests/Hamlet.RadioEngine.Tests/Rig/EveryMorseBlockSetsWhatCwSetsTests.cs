using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 420 task 1, criterion 7.7: entering Morse in a `CW DX` or `QRP`
/// block sets the receiver exactly as entering Morse in a `CW` block does (R70,
/// HM-DEC-175).
/// </summary>
/// <remarks>
/// <para>**WHAT WAS WRONG.** The conditions file is keyed by the block's short name
/// and the Morse family has three of them. Only `CW` stated anything, so tuning into
/// the bottom of 40 m, 20 m, 15 m and 80 m, or into any QRP watering hole, wrote
/// nothing. 7.030 is the first hertz of the 40 m QRP block, and there a radio left at
/// preamp 1 stayed at preamp 1 against the CW row's own text.</para>
/// <para>**EACH CASE DOES WHAT THE APP DOES**: the block's own conditions handed to
/// the real <see cref="ReceiverSetup"/>, beside the `CW` block of the same band
/// driven at the same dial, both from a radio at preamp 1, AGC mid, the noise blanker
/// on and RF gain at full. Every result is an indication against
/// <see cref="ScriptedRadio"/> (FACT-006).</para>
/// </remarks>
public sealed class EveryMorseBlockSetsWhatCwSetsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each field's two outcomes are printed.</param>
    public EveryMorseBlockSetsWhatCwSetsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Every confirmed CW condition ends where the `CW` block puts it, filed the same
    /// way and written the same way.
    /// </summary>
    /// <param name="hz">The dial: 7.030 `QRP`, 14.010 `CW DX`, 14.050 `CW`.</param>
    [Theory]
    [InlineData(7_030_000L)]
    [InlineData(14_010_000L)]
    [InlineData(14_050_000L)]
    public async Task EnteringMorseHereSetsWhatTheCwBlockSets(long hz)
    {
        var block = ModeEntryBench.BlockAt(hz)!;
        var cwBlock = NeighborhoodPlan
            .ForBand(HfBands.BandFor(hz)!)
            .First(n => n.ShortName == "CW");

        var (here, hereRadio) = await TuneInAsync(hz, block);
        var (cw, cwRadio) = await TuneInAsync(hz, cwBlock);

        _output.WriteLine(
            $"{hz / 1e6:0.000} MHz is {block.Name} ({block.ShortName}); "
            + $"beside {cwBlock.Name} (CW) at the same dial");

        foreach (var condition in ReceiverConditions.ForMode("CW").Where(c => c.CanBeWritten))
        {
            var field = condition.Field!.Value;
            var mine = here.FirstOrDefault(r => r.Condition.Field == field);
            var theirs = cw.Single(r => r.Condition.Field == field);

            _output.WriteLine(
                $"  {condition.Control,-16} here {mine?.Outcome.ToString() ?? "no tune-in",-18} "
                + $"now {mine?.NowText ?? "-",-10} | CW block {theirs.Outcome,-18} now {theirs.NowText}");

            Assert.True(
                mine is not null,
                $"{block.ShortName} at {hz / 1e6:0.000} MHz states no {condition.Control}, "
                + "so entering Morse there leaves it wherever it was");
            Assert.Equal(theirs.Outcome, mine!.Outcome);
            Assert.Equal(theirs.NowText, mine.NowText);
        }

        Assert.Equal(ModeEntryBench.Writes(cwRadio), ModeEntryBench.Writes(hereRadio));
    }

    /// <summary>
    /// **AT 7.030 THE PREAMP ENDS AT OFF**, which is what the CW row's own text says
    /// at 40 m and below.
    /// </summary>
    [Fact]
    public async Task At7030ThePreampEndsOff()
    {
        var block = ModeEntryBench.BlockAt(7_030_000)!;
        var (results, radio) = await TuneInAsync(7_030_000, block);

        _output.WriteLine(
            $"{block.Name} ({block.ShortName}) states {results.Count} conditions; "
            + $"preamp now {radio.Switches[ModeEntryBench.Preamp]}");

        Assert.Equal(0, radio.Switches[ModeEntryBench.Preamp]);
    }

    /// <summary>
    /// **EVERY MORSE BLOCK ON THE MAP STATES THE CW ROW**, row for row, so no band's
    /// bottom edge or watering hole is left out.
    /// </summary>
    [Fact]
    public void EveryMorseBlockStatesTheCwRow()
    {
        var cwRow = ReceiverConditions.ForMode("CW");
        var family = HfBands.Bands
            .SelectMany(b => NeighborhoodPlan.ForBand(b).Select(n => (Band: b, Hood: n)))
            .Where(x => x.Hood.Family == ModeFamily.Cw)
            .ToList();

        var silent = family
            .Where(x => !cwRow.All(c => ReceiverConditions.ForBlock(x.Hood).Contains(c)))
            .ToList();

        foreach (var (band, hood) in silent)
        {
            _output.WriteLine($"  {band.Name,-5} {hood.Name} ({hood.ShortName}) does not state the CW row");
        }

        _output.WriteLine(
            $"CW-family blocks stating the CW row: {family.Count - silent.Count} of {family.Count}");

        Assert.Empty(silent);
    }

    /// <summary>
    /// **A CONDITION MARKED UNCONFIRMED IS STILL NOT WRITTEN** (the conditions file's
    /// own rule, CLAUDE.md 12.4), in all three blocks and in the FT8 block that proves
    /// the rule.
    /// </summary>
    /// <remarks>
    /// The CW row marks none of its nine unconfirmed, so in the three Morse blocks the
    /// rule has nothing to act on and the count printed is zero. FT8's AGC is the one
    /// the file marks `confirmed: false`, so the FT8 block at 14.074 is driven beside
    /// them: its AGC is filed spoken-only and no AGC byte goes out.
    /// </remarks>
    [Fact]
    public async Task AnUnconfirmedConditionIsStillNotWritten()
    {
        foreach (var hz in new[] { 7_030_000L, 14_010_000L, 14_050_000L, 14_074_000L })
        {
            var block = ModeEntryBench.BlockAt(hz)!;
            var radio = ModeEntryBench.AsLeftWithThePreampOn(hz);
            radio.Switches[ModeEntryBench.Agc] = 2;
            using var rig = await ModeEntryBench.ConnectAsync(radio);

            var (results, _) = await ReceiverSetup.ApplyAsync(
                rig, ReceiverConditions.ForBlock(block), ReceiverSetupMemory.Empty);

            var unconfirmed = results.Where(r => !r.Condition.Confirmed).ToList();
            var writes = ModeEntryBench.Writes(radio);

            _output.WriteLine(
                $"{hz / 1e6:0.000} {block.ShortName,-6} states {results.Count}, unconfirmed "
                + $"{unconfirmed.Count}: "
                + string.Join(", ", unconfirmed.Select(r => $"{r.Condition.Control} {r.Outcome}"))
                + $"; fields written {string.Join(",", writes.Select(w => w.Field).Distinct())}");

            foreach (var r in unconfirmed)
            {
                Assert.Equal(ConditionOutcome.SpokenOnly, r.Outcome);
                Assert.DoesNotContain(writes, w => w.Field == r.Condition.Field);
            }

            if (block.ShortName == "FT8")
            {
                Assert.Contains(unconfirmed, r => r.Condition.Field == RigField.Agc);
                Assert.Equal(2, radio.Switches[ModeEntryBench.Agc]);
            }
            else
            {
                Assert.Empty(unconfirmed);
            }
        }
    }

    private static async Task<(IReadOnlyList<ConditionResult> Results, ScriptedRadio Radio)>
        TuneInAsync(long hz, Neighborhood block)
    {
        var radio = ModeEntryBench.AsLeftWithThePreampOn(hz);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(block), ReceiverSetupMemory.Empty);

        return (results, radio);
    }
}
