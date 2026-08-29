using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// A block states the receiver conditions the mode living in it requires.
/// </summary>
/// <remarks>
/// <para>**TASK 2 OF WORK INSTRUCTION 042.** The operator was told three times in
/// one afternoon to press buttons on the front of his radio. What a mode needs of
/// the receive side is a fact this project can hold, and holding it is the
/// difference between Hamlet setting the radio and Hamlet describing what is
/// wrong with it.</para>
/// <para>**A BLOCK THAT STATES NOTHING PRODUCES NO CLAIM AND NO WRITE**, and
/// that is asserted here as hard as the blocks that do state something. Filling
/// a gap with a plausible setting is the failure this file exists to prevent, and
/// a setting is worse than a sentence because it goes out as a byte.</para>
/// </remarks>
public sealed class TheBlockStatesWhatTheModeNeedsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the walk is printed.</param>
    public TheBlockStatesWhatTheModeNeedsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What every stated condition owes, whatever its mode.</summary>
    /// <param name="hood">The block, for the message.</param>
    /// <param name="conditions">Its conditions.</param>
    /// <remarks>
    /// A reason short enough to be missing is a reason that cannot be spoken, and
    /// task 4 speaks all of them.
    /// </remarks>
    private static void AssertEveryConditionCarriesItsReason(
        Neighborhood hood, IReadOnlyList<ReceiverCondition> conditions)
    {
        foreach (var condition in conditions)
        {
            Assert.NotEqual("", condition.Because);
            Assert.NotEqual("", condition.Control);
            Assert.True(
                condition.Because.Length > 60,
                $"{hood.Name} {condition.Control} gives a reason too short to say aloud");
        }
    }

    private static IEnumerable<(CwBand Band, Neighborhood Hood)> WholeMap()
        => HfBands.Bands.SelectMany(
            b => NeighborhoodPlan.ForBand(b).Select(n => (Band: b, Hood: n)));

    /// <remarks>
    /// <para>**EVERY DIGITAL BLOCK THE FILE SPEAKS FOR, ON EVERY BAND.** The one
    /// the operator lost an hour to is 20 m FT8, and a rule proved on one row is
    /// a rule with nowhere to be wrong.</para>
    /// <para>**AND THE SPAN IS THE BLOCK'S OWN WIDTH.** It is the same number the
    /// passband comes from, derived rather than written down, so an 80 m block
    /// two kilohertz wide says two and a 20 m block says three. A figure typed
    /// beside the row would have claimed three on both, which is the defect the
    /// passband walk caught last unit.</para>
    /// </remarks>
    [Fact]
    public void TheDigitalBlocksStateWhatTheirModeNeeds()
    {
        var spoke = 0;

        _output.WriteLine("  band  | block        | width | conditions");
        _output.WriteLine("  ------|--------------|-------|-----------");

        foreach (var (band, hood) in WholeMap())
        {
            var conditions = ReceiverConditions.ForBlock(hood);

            if (conditions.Count == 0)
            {
                continue;
            }

            spoke++;

            var wideHz = hood.HighHz - hood.LowHz;

            // **THIS TEST IS ABOUT THE DIGITAL BLOCKS AND ITS NAME SAYS SO.**
            // It walked every block that states anything, which was the same set
            // until 2026-08-29, when Tim's ruling had CW state what CW needs and
            // the two stopped being the same set. The four fields below are what
            // *this mode* needs; CW's four are different and are walked by
            // `TheMorseBlocksStateWhatMorseNeeds`. What every block owes,
            // whatever its mode, is asserted below for all of them.
            if (hood.Family is not ModeFamily.Digital)
            {
                AssertEveryConditionCarriesItsReason(hood, conditions);

                continue;
            }

            _output.WriteLine(
                $"  {band.Name,-5} | {hood.Name,-12} | {wideHz,5} | "
                + string.Join(", ", conditions.Select(c => $"{c.Control}={c.WantedText}")));

            // The four the order names, and the span.
            foreach (var field in new[]
            {
                RigField.NoiseBlanker, RigField.NoiseReduction,
                RigField.AutoNotch, RigField.Agc,
            })
            {
                Assert.Contains(conditions, c => c.Field == field);
            }

            var span = conditions.Single(c => c.Control == "scope span");

            // **DERIVED, NOT TYPED.** The width is quoted from the row itself.
            Assert.Contains(
                wideHz >= 1000 ? $"{wideHz / 1000.0:0.#} kHz" : $"{wideHz} Hz",
                span.WantedText,
                StringComparison.Ordinal);

            AssertEveryConditionCarriesItsReason(hood, conditions);
        }

        _output.WriteLine("");
        _output.WriteLine($"  {spoke} blocks state conditions");

        // FT8 and FT4 across the bands that carry them.
        Assert.True(spoke >= 8, $"only {spoke} blocks state anything");
    }

    /// <remarks>
    /// <para>**WHAT MORSE NEEDS IS NOT WHAT FT8 NEEDS, AND THE ROWS SAY SO**
    /// (Tim's ruling of 2026-08-29). The attenuator leads, because on the evening
    /// that produced this ruling it sat at 20 dB with the preamp off while the
    /// station faded S4 to S1 to nothing.</para>
    /// <para>**AND THE TWO NOBODY HAS MEASURED HERE PRODUCE NO WRITE.** The
    /// preamp and the AGC are stated so they can be spoken and marked unconfirmed
    /// so no byte goes out on them (§12.4). Which AGC setting reads better on a
    /// deep fade is a question about this receiver and this operator; whether the
    /// preamplifier helps depends on the antenna and the band noise here.</para>
    /// </remarks>
    [Fact]
    public void TheMorseBlocksStateWhatMorseNeeds()
    {
        var spoke = 0;

        foreach (var (band, hood) in WholeMap())
        {
            if (hood.Family is not ModeFamily.Cw)
            {
                continue;
            }

            var conditions = ReceiverConditions.ForBlock(hood);

            if (conditions.Count == 0)
            {
                // Not every Morse block is named `CW`; `CW DX` and `QRP` are
                // Morse and the lookup is by short name. That is unit 042's
                // shape and this unit did not change it (§12.6).
                continue;
            }

            spoke++;

            _output.WriteLine(
                $"  {band.Name,-5} | {hood.Name,-14} | "
                + string.Join(", ", conditions.Select(c => $"{c.Control}={c.WantedText}")));

            foreach (var field in new[]
            {
                RigField.Attenuator, RigField.Preamp,
                RigField.NoiseBlanker, RigField.Agc,
            })
            {
                Assert.Contains(conditions, c => c.Field == field);
            }

            // **THE ATTENUATOR COMES OFF.** It is the one the order names and the
            // one that cost the operator his evening.
            var attenuator = conditions.Single(c => c.Field == RigField.Attenuator);

            Assert.Equal(0, attenuator.Wanted);
            Assert.True(attenuator.CanBeWritten);

            // **AND THE TWO NOBODY MEASURED GO OUT AS WORDS AND NOT AS BYTES.**
            foreach (var field in new[] { RigField.Preamp, RigField.Agc })
            {
                var stated = conditions.Single(c => c.Field == field);

                Assert.False(
                    stated.CanBeWritten,
                    $"{stated.Control} is written to the radio on a value nobody "
                    + "has measured at this station");
                Assert.NotEqual("", stated.Confirm);
            }

            AssertEveryConditionCarriesItsReason(hood, conditions);
        }

        Assert.True(spoke > 0, "no Morse block states anything at all");
    }

    /// <remarks>
    /// <para>**THE BLOCKS THAT SAY NOTHING SAY NOTHING.** Phone rows and the open
    /// ground between the cited ones.</para>
    /// <para>**MORSE ROWS USED TO BE ON THAT LIST AND ARE NOT ANY MORE.** Unit
    /// 042's order was explicit that CW states what CW needs and no more, and
    /// that nobody had measured what CW needs of the noise controls, so its rows
    /// produced no claim and no write. **Tim's ruling of 2026-08-29 reverses
    /// that** and has CW state four conditions in the same shape. What survives
    /// of the old reasoning is where it was right: the two rows nobody has
    /// measured here, the preamp and the AGC, are stated unconfirmed and produce
    /// no write (§12.4).</para>
    /// </remarks>
    [Fact]
    public void ABlockWithNothingToSayProducesNoClaim()
    {
        var silent = 0;

        foreach (var (band, hood) in WholeMap())
        {
            if (ReceiverConditions.ForBlock(hood).Count != 0)
            {
                continue;
            }

            silent++;

            if (hood.Family is ModeFamily.Cw)
            {
                _output.WriteLine($"  {band.Name,-5} {hood.Name} states nothing");
            }
        }

        _output.WriteLine("");
        _output.WriteLine($"  {silent} blocks state nothing at all");

        Assert.True(silent > 50, $"only {silent} blocks are silent, which is too few");

        // And off the map entirely.
        Assert.Empty(ReceiverConditions.ForBlock(null));

        // The file says which modes it will not speak for, rather than leaving
        // the gap to be read as an oversight (§12.4).
        Assert.Contains(
            ReceiverConditions.Unknowns,
            u => u.Topic.Contains("PSK31", StringComparison.Ordinal));
        Assert.Contains(
            ReceiverConditions.Unknowns,
            u => u.Topic.Contains("CW", StringComparison.Ordinal));
    }

    /// <remarks>
    /// <para>**NO BYTE IS WRITTEN THAT IS NOT CITED** (HM-DEC-084). Anything this
    /// file marks as writable must have a real write in §4's table, with a page
    /// behind it, or it is a setting Hamlet would change on nothing.</para>
    /// <para>**AND THE TWO WAYS OF NOT BEING WRITABLE ARE DIFFERENT.** The scope
    /// span has no cited command at all, so it is spoken. The AGC has one and its
    /// value has not been established, so it is also spoken. Both are stated in
    /// the file and neither goes out on the wire.</para>
    /// </remarks>
    [Fact]
    public void EveryWritableConditionHasACitedCommandAndTheRestAreSpokenOnly()
    {
        var ft8 = WholeMap().First(x => x.Hood.ShortName == "FT8").Hood;
        var conditions = ReceiverConditions.ForBlock(ft8);

        foreach (var condition in conditions)
        {
            var write = CivWrites.All.FirstOrDefault(w => w.Field == condition.Field);

            _output.WriteLine(
                $"  {condition.Control,-15} wanted={condition.WantedText,-12} "
                + $"writable={condition.CanBeWritten,-5} "
                + $"cited={(write is null ? "no command" : "p. " + write.Page)}");

            if (!condition.CanBeWritten)
            {
                continue;
            }

            Assert.NotNull(write);
            Assert.Matches(@"^\d{1,2}-\d{1,2}$", write!.Page);

            // Nothing on the receive side may key a transmitter (§0.2).
            Assert.Equal(RigWriteTier.Receive, write.Tier);
        }

        // The span: stated, and not writable, because §4 carries no command.
        var span = conditions.Single(c => c.Control == "scope span");
        Assert.Null(span.Field);
        Assert.False(span.CanBeWritten);

        // The AGC: a real command, a value nobody has settled, and an owner.
        var agc = conditions.Single(c => c.Field == RigField.Agc);
        Assert.False(agc.Confirmed);
        Assert.False(agc.CanBeWritten);
        Assert.Equal("tim", agc.Confirm);

        // The three that are settled are settled.
        foreach (var field in new[]
        {
            RigField.NoiseBlanker, RigField.NoiseReduction, RigField.AutoNotch,
        })
        {
            var condition = conditions.Single(c => c.Field == field);
            Assert.True(condition.CanBeWritten, $"{condition.Control} is not writable");
            Assert.Equal(0, condition.Wanted);
        }
    }

    /// <remarks>
    /// **THE TWO FILES MAY NOT DRIFT APART.** The conditions are keyed by the
    /// same label the mode target is chosen by, so a mode stating conditions that
    /// no block is ever worked in would be a set of settings that can never fire,
    /// and nobody would notice.
    /// </remarks>
    [Fact]
    public void EveryModeStatingConditionsIsAModeSomeBlockIsWorkedIn()
    {
        var onTheMap = WholeMap()
            .Where(x => ModeFollowPlan.TargetFor(x.Hood) is not null)
            .Select(x => x.Hood.ShortName.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);

        foreach (var mode in ReceiverConditions.Modes)
        {
            _output.WriteLine($"  {mode} is worked somewhere: {onTheMap.Contains(mode)}");

            Assert.Contains(mode, onTheMap);
        }
    }
}
