using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// One owned-settings list, and what every mode's row says about each entry.
/// </summary>
/// <remarks>
/// <para>**TWO CONVERSATIONS ARE BUILDING AGAINST ONE RADIO** (Tim's ruling of
/// 2026-08-29). This one works CW at night and another works FT8 in daylight, and
/// each was writing its own set of deltas: whichever ran last won on whatever it
/// happened to touch, and nothing stated what became of a setting a mode never
/// mentioned.</para>
/// <para>**COVERAGE IS REPORTED AND NOT FAILED.** A row that says nothing about a
/// setting leaves it alone, which is the honest behaviour while another
/// conversation owns that row. Failing on it would be this unit writing values
/// into rows it does not own (§12.4).</para>
/// </remarks>
public sealed class EveryModeAnswersForEverySettingTests
{
    private readonly ITestOutputHelper _output;

    public EveryModeAnswersForEverySettingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The coverage table the other conversation needs.</summary>
    /// <remarks>
    /// **THIS IS THE UNIT'S HANDOVER.** It prints, for every block that states
    /// anything, which owned settings it states, which it defers to the operator,
    /// and which are absent — so the conversation that owns FT8 can fill its side
    /// in without collision.
    /// </remarks>
    [Fact]
    public void TheCoverageTableIsReported()
    {
        _output.WriteLine("  block            | stated | deferred | absent");
        _output.WriteLine("  -----------------|--------|----------|-------");

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, hood) in WholeMap())
        {
            if (!seen.Add(hood.ShortName))
            {
                continue;
            }

            var coverage = OwnedSettings.Coverage(hood);
            var stated = coverage.Count(c => c.Answer == OwnedAnswer.Stated);
            var deferred = coverage.Count(c => c.Answer == OwnedAnswer.OperatorsChoice);
            var absent = coverage.Count(c => c.Answer == OwnedAnswer.Absent);

            if (stated + deferred == 0)
            {
                continue;
            }

            _output.WriteLine(
                $"  {hood.ShortName,-16} | {stated,6} | {deferred,8} | {absent,6}");

            foreach (var (setting, answer) in coverage)
            {
                if (answer == OwnedAnswer.Absent)
                {
                    _output.WriteLine($"      absent: {setting.Control}");
                }
            }
        }

        Assert.NotEmpty(OwnedSettings.All);
    }

    /// <summary>Every owned setting names a field and a citation.</summary>
    /// <remarks>
    /// **NO BYTE IS WRITTEN THAT IS NOT CITED** (HM-DEC-084). The citation
    /// travels with the setting, so a row that states a value has a page behind
    /// it rather than a comment somewhere else.
    /// </remarks>
    [Fact]
    public void EveryOwnedSettingCarriesItsCitation()
    {
        Assert.Equal(12, OwnedSettings.All.Count);

        foreach (var owned in OwnedSettings.All)
        {
            Assert.NotEqual("", owned.Control);
            Assert.NotEqual("", owned.Citation);
        }
    }

    /// <summary>
    /// The settings that are deliberately not owned stay off the list.
    /// </summary>
    /// <remarks>
    /// **THE CW PITCH AND THE AF LEVEL CHANGE WHAT THE OPERATOR HEARS**, which is
    /// his ear rather than a receive condition, and **break-in is a transmit
    /// setting** — the manual's footnote 2 on p. 19-7 makes PC text become
    /// transmitted CW while it is on, so §0.2 keeps every automatic write away
    /// from it. This goes red if any of them is ever added.
    /// </remarks>
    [Fact]
    public void WhatIsNotOwnedStaysOffTheList()
    {
        foreach (var field in new[]
        {
            RigField.CwPitch, RigField.AfLevel, RigField.BreakIn,
            RigField.BreakInDelay, RigField.NoiseBlankerLevel,
            RigField.NoiseReductionLevel, RigField.NotchPosition,
        })
        {
            Assert.DoesNotContain(OwnedSettings.All, o => o.Field == field);
        }
    }

    /// <summary>CW answers for the settings Tim tabled.</summary>
    /// <remarks>
    /// Auto notch off is the one that matters most: it hunts steady carriers and
    /// a keyed Morse signal is a steady carrier, so it eats the thing being read.
    /// </remarks>
    [Fact]
    public void MorseStatesWhatMorseNeeds()
    {
        var cw = NeighborhoodPlan.ForBand(HfBands.Bands.First(b => b.Name == "40 m"))
            .First(n => n.ShortName == "CW");

        var stated = ReceiverConditions.ForBlock(cw);

        _output.WriteLine(string.Join(
            ", ", stated.Select(c => $"{c.Control}={c.WantedText}")));

        foreach (var (field, wanted) in new[]
        {
            (RigField.AutoNotch, 0),
            (RigField.ManualNotch, 0),
            (RigField.NoiseBlanker, 0),
            (RigField.NoiseReduction, 0),
            (RigField.Agc, 1),
            (RigField.RfGain, 255),
            (RigField.Squelch, 0),
        })
        {
            var row = stated.SingleOrDefault(c => c.Field == field);

            Assert.NotNull(row);
            Assert.Equal(wanted, row!.Wanted);
            Assert.False(row.IsConditional);
        }
    }

    /// <summary>The two rules are rules and not constants.</summary>
    /// <remarks>
    /// **BOTH WERE WRONG IN OPPOSITE DIRECTIONS ON ONE EVENING** while Hamlet
    /// held the reading that decides them: 20 dB of attenuator while a station
    /// faded S4 to S1 to nothing, and no attenuator while the front end read
    /// overloading at S9 plus 10. A constant is wrong half the time by
    /// construction.
    /// </remarks>
    [Fact]
    public void TheAttenuatorAndThePreampAreRulesNotConstants()
    {
        var cw = NeighborhoodPlan.ForBand(HfBands.Bands.First(b => b.Name == "40 m"))
            .First(n => n.ShortName == "CW");

        var stated = ReceiverConditions.ForBlock(cw);

        var attenuator = stated.Single(c => c.Field == RigField.Attenuator);
        var preamp = stated.Single(c => c.Field == RigField.Preamp);

        Assert.True(attenuator.IsConditional);
        Assert.Equal("overflow", attenuator.Condition);

        Assert.True(preamp.IsConditional);
        Assert.Equal("band", preamp.Condition);
    }

    /// <summary>Every stated row carries a reason long enough to say aloud.</summary>
    [Fact]
    public void EveryStatedRowCarriesItsReason()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, hood) in WholeMap())
        {
            if (!seen.Add(hood.ShortName))
            {
                continue;
            }

            foreach (var row in ReceiverConditions.ForBlock(hood))
            {
                Assert.NotEqual("", row.Control);
                Assert.True(
                    row.Because.Length > 60,
                    $"{hood.ShortName} {row.Control} gives a reason too short to "
                    + "say aloud");
            }
        }
    }

    private static IEnumerable<(CwBand Band, Neighborhood Hood)> WholeMap()
        => HfBands.Bands.SelectMany(
            b => NeighborhoodPlan.ForBand(b).Select(n => (Band: b, Hood: n)));
}
