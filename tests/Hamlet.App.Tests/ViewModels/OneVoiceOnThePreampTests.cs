using System.Text.RegularExpressions;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 424 task 3, criterion 7.8's second half: no sentence about the
/// preamp contradicts what Hamlet set (R74, HM-DEC-177).
/// </summary>
/// <remarks>
/// <para>**WATCHED FAILING AT 14.050.** After task 2 the setup wrote preamp 1 there and
/// then said *I set the preamp to preamp 1 from 1.8 to 29.999 MHz, preamp 2 at 50 MHz,
/// and off while the front end reads overloading*: the row's whole rule, read out as if
/// it were the value, so the one sentence about what Hamlet did names two values it did
/// not set. Before task 2 the same clause said *off at 40 m and below because below
/// 40 m the noise arrives with the signal*, which is Tim's complaint.</para>
/// <para>**AND THE TWO VOICES THAT WERE WRONG WHILE OVERLOADING.** With the front end
/// reading overloading the setup turns the preamp off and brings the attenuator in, and
/// once he has moved into a block that does not state them the advice said *Switch the
/// preamp on* and the panel said *Hold P.AMP/ATT for a moment to bring it in*, of an
/// attenuator already in. The manual turns the preamp off with strong signals (page
/// 4-3), so the advice was wrong, and the panel was asking for a change already made.
/// Neither is silenced: each now says what is true.</para>
/// <para>Every result is an indication against <see cref="ScriptedRadio"/> (FACT-006).</para>
/// </remarks>
public sealed class OneVoiceOnThePreampTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public OneVoiceOnThePreampTests(ITestOutputHelper output) => _output = output;

    /// <summary>The frequencies the app tunes in at, one per band on the map.</summary>
    public static TheoryData<long, bool> OnTheMap()
    {
        var data = new TheoryData<long, bool>();

        foreach (var hz in new long[]
                 {
                     3_530_000, 7_030_000, 10_110_000, 14_050_000, 18_080_000,
                     21_050_000, 28_050_000,
                 })
        {
            data.Add(hz, false);
            data.Add(hz, true);
        }

        return data;
    }

    /// <summary>
    /// **The red one.** At 14.050 the setup's own sentence about the preamp names the
    /// value it left and no other.
    /// </summary>
    [Fact]
    public async Task At14050TheSetupSaysTheValueItSetAndNoOther()
    {
        var (results, state) = await TuneInAsync(14_050_000, overloading: false);
        var said = ReceiverSetupVoice.Say(
            results.Where(r => r.Condition.Field == RigField.Preamp).ToList());

        _output.WriteLine(said);

        Assert.Equal("preamp 1", state[RigField.Preamp].Text);
        Assert.Contains("preamp 1", said, StringComparison.Ordinal);
        Assert.Empty(OtherValuesNamed(said, "preamp 1"));
    }

    /// <summary>
    /// At every frequency the app tunes in at, quiet and overloading, in the block and
    /// once he has moved on, no sentence contradicts what the setup left.
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <param name="overloading">Whether the front end reads overloading at the tune-in.</param>
    [Theory]
    [MemberData(nameof(OnTheMap))]
    public async Task NoSentenceContradictsWhatHamletSet(long hz, bool overloading)
    {
        var (results, state) = await TuneInAsync(hz, overloading);
        var now = state[RigField.Preamp].Text;
        var faults = new List<string>();

        var setup = ReceiverSetupVoice.Say(
            results.Where(r => r.Condition.Field == RigField.Preamp).ToList());
        _output.WriteLine($"{hz / 1e6:0.000} MHz{(overloading ? ", overloading" : "")}: preamp {now}, "
                          + $"attenuator {state[RigField.Attenuator].Text}");
        _output.WriteLine($"  setup: {setup}");

        if (OtherValuesNamed(setup, now).Any())
        {
            faults.Add($"the setup names {string.Join(", ", OtherValuesNamed(setup, now))} after leaving {now}");
        }

        var ft8 = ReceiverConditions.ForMode("FT8").Select(c => c.Field).OfType<RigField>().ToHashSet();
        var contexts = new (string Name, IReadOnlySet<RigField> Owned)[]
        {
            ("in the block", ReceiverSetup.Owns(results)),
            ("moved into a block that states nothing", new HashSet<RigField>()),
            ("moved into an FT8 block", ft8),
        };

        foreach (var (name, owned) in contexts)
        {
            var advice = ReceiveAdvice.For(state, owned).First(a => a.Write.Field == RigField.Preamp);
            var panel = MainWindowViewModel.OverflowAdviceFor(
                overloading,
                state[RigField.Preamp] is { IsKnown: true, Number: 1 or 2 },
                owned.Contains(RigField.Preamp) && owned.Contains(RigField.Attenuator),
                state[RigField.Attenuator] is { IsKnown: true, Number: > 0 });
            var objections = RigObservations.For(state, owned)
                .Where(o => o.Contains("preamp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _output.WriteLine($"  {name}: advice \"{advice.Says}\"; panel \"{panel}\"");

            if (advice.WouldChange && advice.Value != (int?)state[RigField.Preamp].Number)
            {
                faults.Add($"{name}: the advice proposes preamp {advice.Value}: {advice.Says}");
            }

            // A sentence asking for the attenuator the row wanted and the radio refused
            // (the 0x14 write unit 419 parked) asks for the condition's own value, so
            // only a request to move P.AMP/ATT away from where the setup left it counts.
            if (WhatIsSaidAboutThePreampTests.UndoesTheSetup(panel, state))
            {
                faults.Add($"{name}: the panel asks him to undo P.AMP/ATT: {panel}");
            }

            faults.AddRange(objections.Select(o => $"{name}: an observation objects: {o}"));
        }

        Assert.Empty(faults);
    }

    /// <summary>
    /// With the preamp off and the attenuator already in, the overload sentence states
    /// the overload and does not ask for the attenuator again; with it out, it still
    /// names the button (HM-DEC-148 stands).
    /// </summary>
    [Fact]
    public void TheOverloadSentenceDoesNotAskForAnAttenuatorAlreadyIn()
    {
        var alreadyIn = MainWindowViewModel.OverflowAdviceFor(
            true, preampIsOn: false, frontEndOwned: false, attenuatorIsIn: true);
        var stillOut = MainWindowViewModel.OverflowAdviceFor(
            true, preampIsOn: false, frontEndOwned: false, attenuatorIsIn: false);

        _output.WriteLine(alreadyIn);

        Assert.Contains("front end is overloading", alreadyIn, StringComparison.Ordinal);
        Assert.DoesNotContain("P.AMP/ATT", alreadyIn, StringComparison.Ordinal);
        Assert.Contains("P.AMP/ATT", stillOut, StringComparison.Ordinal);
    }

    private static async Task<(IReadOnlyList<ConditionResult> Results, RigState State)> TuneInAsync(
        long hz, bool overloading)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var conditions = ReceiverConditions.ForBlock(block);
        Assert.NotEmpty(conditions);

        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Overloading = overloading;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(rig, conditions, ReceiverSetupMemory.Empty);

        return (results, await ModeEntryBench.ReadAllAsync(rig));
    }

    // The preamp values a sentence names other than the one the radio holds, in the
    // radio's own words.
    private static IEnumerable<string> OtherValuesNamed(string sentence, string now)
        => new[] { "off", "preamp 1", "preamp 2" }
            .Where(v => !v.Equals(now, StringComparison.OrdinalIgnoreCase))
            .Where(v => Regex.IsMatch(sentence, $@"\b{v}\b", RegexOptions.IgnoreCase));
}
