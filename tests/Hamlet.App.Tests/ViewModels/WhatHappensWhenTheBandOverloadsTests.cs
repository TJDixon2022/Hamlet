using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Rig;
using Xunit;
using Xunit.Abstractions;
using Run = Hamlet.RadioEngine.Tests.Rig.LivePollBench.Run;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 426 task 1, criterion 7.8: what happens when the band starts
/// overloading after he has tuned in, and every sentence the app would say about each of
/// the nine fields the CW row states.
/// </summary>
/// <remarks>
/// <para>**A FACT THAT ASSERTS NOTHING.** It drives the real <see cref="ReceiverSetup"/>
/// against <see cref="ScriptedRadio"/> as `EstablishReceiveConditionsAsync` does, then
/// polls the radio the way the live poll does and hands each reading to whatever the app
/// does with a poll (`LivePollBench.OnPollAsync`). Four points per frequency: a quiet
/// tune-in, `Overflow` rising for several polls, `Overflow` clearing, and his hand setting
/// the preamp himself followed by the overload coming back.</para>
/// <para>**AT EACH POINT** it prints what the preamp is asked for, every write that went
/// to the radio, the preamp the radio reads back, and every sentence the setup's voice,
/// Receive Help (`ReceiveAdvice`), `RigObservations`, `ReceiveObstructions`, the front-end
/// chip and the overload sentence say about the nine fields: in the block, once he has
/// moved into a block that states nothing, and once he has moved into FT8's block.</para>
/// <para>**WHAT IS COUNTED** is a sentence asking him to change a field Hamlet set and
/// still owns: the field's value is the one Hamlet last left (his hand has not moved it),
/// and the tune-in behind the view states it. A sentence asking him to change a field
/// Hamlet left but no longer owns, once he has moved on, is counted apart and marked.</para>
/// <para>**NO RADIO IS ON THIS MACHINE** (FACT-006): every line is an indication against
/// a scripted radio.</para>
/// </remarks>
public sealed class WhatHappensWhenTheBandOverloadsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where it is printed.</param>
    public WhatHappensWhenTheBandOverloadsTests(ITestOutputHelper output) => _output = output;

    /// <summary>The six frequencies the work instruction names.</summary>
    internal static readonly (string Band, long Hz)[] Frequencies =
    {
        ("80 m", 3_530_000),
        ("40 m", 7_030_000),
        ("20 m", 14_050_000),
        ("15 m", 21_050_000),
        ("10 m", 28_050_000),
        ("6 m", 50_100_000),
    };

    /// <summary>The nine fields the CW row states, in the row's order.</summary>
    internal static readonly RigField[] Nine = ModeEntryBench.Fields;

    /// <summary>The four points of the script.</summary>
    internal static readonly string[] Points =
    {
        "1 quiet tune-in",
        "2 overflow rising",
        "3 overflow clearing",
        "4 his hand, then overload again",
    };

    private readonly Dictionary<(string Point, RigField Field), int> _owned = new();
    private readonly Dictionary<(string Point, RigField Field), int> _notOwned = new();

    /// <summary>**THE TRACE.**</summary>
    [Fact]
    public async Task PrintWhatHappens()
    {
        var row = ReceiverConditions.ForMode("CW").First(c => c.Field == RigField.Preamp);
        _output.WriteLine("THE CW PREAMP CONDITION");
        _output.WriteLine($"  wantedText {row.WantedText}");
        _output.WriteLine($"  condition  {row.Condition}, whenOverloading {row.WhenOverloading}");
        _output.WriteLine("  one poll is one live pass, a fresh Overflow reading every 252.9 ms median (MeasureTheLivePollCadence)");

        foreach (var (band, hz) in Frequencies)
        {
            await CaseAsync(band, hz);
        }

        _output.WriteLine("");
        _output.WriteLine("SENTENCES ASKING HIM TO CHANGE A FIELD HAMLET SET AND STILL OWNS, per field and point "
                          + "(in the block, after moving on and in FT8, all six frequencies):");
        PrintTable(_owned);
        _output.WriteLine("");
        _output.WriteLine("SENTENCES ASKING HIM TO CHANGE A FIELD HAMLET LEFT AND NO LONGER OWNS (moved on), "
                          + "counted apart:");
        PrintTable(_notOwned);
    }

    /// <summary>
    /// **THE LIVE POLL'S CADENCE, MEASURED** rather than read off the plan: the live pass
    /// as `RigStateMonitor.RunAsync` runs it - the plan's live fields read in order through
    /// Hamlet's own rig, the ALC left out as it is off transmit, then the plan's interval -
    /// timed over twenty passes against the scripted radio.
    /// </summary>
    /// <remarks>
    /// <para>The monitor itself is not timed, because its connect-time sweep reads every
    /// field and waits out a 750 ms timeout on each one the scripted radio does not speak,
    /// which measures the script's silence and not the poll.</para>
    /// <para>The scripted radio answers at once, so this is the loop and not the CI-V wire;
    /// `PollBudgetTests` carries 2 ms a read at 19200 baud for the wire, printed beside it.
    /// No IC-7300 is on this machine (FACT-006).</para>
    /// </remarks>
    [Fact]
    public async Task MeasureTheLivePollCadence()
    {
        var radio = ModeEntryBench.AsLeft(7_030_000, data: false);
        radio.Transmitting = false;
        radio.AnswersMeters = true;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var live = RigPollPlan.At(RigPollRate.Live).Where(f => f != RigField.Alc).ToList();
        var state = RigState.Empty;
        var stamps = new List<DateTime>();
        var unanswered = new HashSet<RigField>();

        for (var pass = 0; pass < 20; pass++)
        {
            foreach (var field in live)
            {
                var read = await rig.ReadAsync(field, state);
                state = state.With(read);
                if (!state[field].IsKnown)
                {
                    unanswered.Add(field);
                }
            }

            stamps.Add(state[RigField.Overflow].AtUtc ?? DateTime.MinValue);
            await Task.Delay(RigPollPlan.LiveInterval);
        }

        var gaps = stamps.Zip(stamps.Skip(1), (a, b) => (b - a).TotalMilliseconds).OrderBy(g => g).ToList();

        _output.WriteLine($"live fields per pass: {live.Count} ({string.Join(", ", live)}); plan interval {RigPollPlan.LiveInterval.TotalMilliseconds} ms");
        _output.WriteLine($"unanswered by the scripted radio: {(unanswered.Count == 0 ? "none" : string.Join(", ", unanswered))}");
        _output.WriteLine($"gap between fresh Overflow readings over {gaps.Count} passes, ms: min {gaps[0]:0.0}, median {gaps[gaps.Count / 2]:0.0}, max {gaps[^1]:0.0}");
        _output.WriteLine($"wire estimate on the IC-7300 at 19200 baud: {live.Count} reads x {PollBudgetWireMs} ms = {live.Count * PollBudgetWireMs} ms a pass on top of that");
    }

    // PollBudgetTests.WireMilliseconds, restated because that type is in the engine's tests.
    private const double PollBudgetWireMs = 2.0;

    /// <summary>
    /// **THE 7.8 TABLE** (work instruction 426 task 4): one frequency on each HF band from
    /// 160 m to 10 m and 50.100 MHz, quiet at the tune-in and then overloading after it -
    /// what is asked for, what is written, what is read back, and every sentence the app
    /// would say about the preamp there, in the block.
    /// </summary>
    [Fact]
    public async Task PrintTheTable()
    {
        foreach (var (band, hz) in WhatIsSaidAboutThePreampTests.Frequencies)
        {
            var block = ModeEntryBench.BlockAt(hz);
            var fromBlock = ReceiverConditions.ForBlock(block);

            _output.WriteLine("");
            _output.WriteLine($"=== {hz / 1e6:0.000} MHz, {band}: "
                              + (fromBlock.Count > 0
                                  ? $"{block!.Name} ({block.ShortName})"
                                  : (block is null ? "NO BLOCK ON THE MAP (P23)" : $"{block.Name} states nothing")
                                    + " - the app tunes in nothing here and writes nothing; the CW row is driven directly"));

            var run = await LivePollBench.TuneInQuietAsync(hz);
            using var rig = run.Rig;
            var tuneInWrites = LivePollBench.PreampWrites(run.Radio);
            var state = await LivePollBench.PollAsync(run);
            Row(run, "quiet at the tune-in", state, tuneInWrites);

            ModeEntryBench.ClearWrites(run.Radio);
            run.Radio.Overloading = true;
            state = await LivePollBench.PollAsync(run, 12);
            Row(run, "overloading after the tune-in, 12 polls", state, LivePollBench.PreampWrites(run.Radio));
        }
    }

    private void Row(Run run, string when, RigState state, IReadOnlyList<int> writes)
    {
        var row = run.Conditions.First(c => c.Field == RigField.Preamp);
        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var asked = overloading && row.WhenOverloading is { } off
            ? off
            : row.Bands.FirstOrDefault(b => b.Contains(run.Radio.FrequencyHz))?.Wanted;
        var owned = ReceiverSetup.Owns(run.Results);
        var preampOn = state[RigField.Preamp] is { IsKnown: true, Number: 1 or 2 };

        _output.WriteLine($"  {when}: asked preamp {asked?.ToString(CultureInfo.InvariantCulture) ?? "nothing"}; "
                          + $"written {(writes.Count == 0 ? "nothing" : string.Join(",", writes))}; "
                          + $"read back {state[RigField.Preamp].Text}");
        var said = new List<string>
        {
            "setup: " + Quote(ReceiverSetupVoice.Say(run.Results.Where(r => r.Condition.Field == RigField.Preamp).ToList())),
            "narrated after the tune-in: " + (run.Narrated.Count == 0 ? "(nothing)" : string.Join(" | ", run.Narrated.Select(Quote))),
            "Receive Help: " + Quote(ReceiveAdvice.For(state, owned).First(a => a.Write.Field == RigField.Preamp).Says),
            "observations: " + string.Join(" | ", RigObservations.For(state, owned).Where(o => o.Contains("preamp", StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty("(nothing about the preamp)")),
            "chip: " + Quote(MainWindowViewModel.FrontEndTextFor(
                overloading,
                MainWindowViewModel.PreampLabel(state[RigField.Preamp].IsKnown ? (int?)state[RigField.Preamp].Number : null),
                MainWindowViewModel.AttenuatorLabel(state[RigField.Attenuator].IsKnown ? (int?)state[RigField.Attenuator].Number : null))),
            "overload sentence: " + Quote(MainWindowViewModel.OverflowAdviceFor(
                overloading, preampOn,
                owned.Contains(RigField.Preamp) && owned.Contains(RigField.Attenuator),
                state[RigField.Attenuator] is { IsKnown: true, Number: > 0 })),
        };

        foreach (var line in said)
        {
            _output.WriteLine($"    {line}");
        }
    }

    private void PrintTable(Dictionary<(string Point, RigField Field), int> counts)
    {
        _output.WriteLine("  " + "field".PadRight(16) + string.Join("", Points.Select(p => p[..1].PadLeft(4))) + "   total");
        var total = 0;
        foreach (var field in Nine)
        {
            var line = Points.Select(p => counts.GetValueOrDefault((p, field))).ToList();
            total += line.Sum();
            _output.WriteLine("  " + field.ToString().PadRight(16) + string.Join("", line.Select(n => n.ToString(CultureInfo.InvariantCulture).PadLeft(4))) + $"   {line.Sum()}");
        }

        _output.WriteLine($"  all nine fields: {total}");
    }

    private static Task<RigState> PollAsync(Run run) => LivePollBench.PollAsync(run);

    private async Task CaseAsync(string band, long hz)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);

        _output.WriteLine("");
        _output.WriteLine($"=================== {hz / 1e6:0.000} MHz, {band}");
        _output.WriteLine(
            fromBlock.Count > 0
                ? $"  block: {block!.Name} ({block.ShortName}), states {fromBlock.Count} conditions"
                : $"  block: {(block is null ? "none on the map (P23)" : $"{block.Name}, states nothing")}"
                  + " - THE APP WRITES NOTHING HERE; the CW row is driven directly below");

        // 1. A quiet tune-in.
        var run = await LivePollBench.TuneInQuietAsync(hz);
        var radio = run.Radio;
        using var rig = run.Rig;
        var state = await PollAsync(run);
        Point(run, Points[0], state, "tune-in with the front end quiet, then one poll");

        // 2. Overflow rising and holding for several polls.
        ModeEntryBench.ClearWrites(radio);
        radio.Overloading = true;
        for (var i = 0; i < 12; i++)
        {
            state = await PollAsync(run);
        }

        Point(run, Points[1], state, "Overflow reads overloading for 12 polls (about 3 s of live poll)");

        // 3. Overflow clearing and staying clear.
        ModeEntryBench.ClearWrites(radio);
        radio.Overloading = false;
        for (var i = 0; i < 40; i++)
        {
            state = await PollAsync(run);
        }

        Point(run, Points[2], state, "Overflow reads not overloading for 40 polls (about 10 s of live poll)");

        // 4. His hand sets the preamp himself, then the overload comes back and goes.
        ModeEntryBench.ClearWrites(radio);
        var his = (byte)(radio.Switches[ModeEntryBench.Preamp] == 2 ? 1 : 2);
        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, his);
        state = await PollAsync(run);
        radio.Overloading = true;
        for (var i = 0; i < 12; i++)
        {
            state = await PollAsync(run);
        }

        var overloadedAfterHand = state;
        radio.Overloading = false;
        for (var i = 0; i < 40; i++)
        {
            state = await PollAsync(run);
        }

        Point(run, Points[3], overloadedAfterHand,
            $"he sets the preamp to {his} himself, then Overflow overloading 12 polls; sentences as it overloads");
        _output.WriteLine($"    ...then clear 40 polls: preamp reads {state[RigField.Preamp].Text}, writes in all of point 4: {WritesText(radio)}");
    }

    private void Point(Run run, string point, RigState state, string what)
    {
        var preampRow = run.Conditions.First(c => c.Field == RigField.Preamp);
        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: > 0 };
        var hz = run.Radio.FrequencyHz;
        var asked = overloading && preampRow.WhenOverloading is { } off
            ? off
            : preampRow.Bands.FirstOrDefault(b => b.Contains(hz))?.Wanted;

        _output.WriteLine("");
        _output.WriteLine($"  --- point {point}: {what}");
        _output.WriteLine($"    asked for: preamp {asked?.ToString(CultureInfo.InvariantCulture) ?? "nothing (no stretch)"} "
                          + $"(overflow {state[RigField.Overflow].Text}, transmit {state[RigField.TransmitStatus].Text})");
        _output.WriteLine($"    writes to the radio in this point: {WritesText(run.Radio)}");
        _output.WriteLine($"    preamp read back: {state[RigField.Preamp].Text}; attenuator {state[RigField.Attenuator].Text}");
        if (run.Narrated.Count > 0)
        {
            _output.WriteLine($"    narrated since the tune-in: {string.Join(" | ", run.Narrated.Select(Quote))}");
        }

        var owned = ReceiverSetup.Owns(run.Results);
        var ft8 = ReceiverConditions.ForMode("FT8").Select(c => c.Field).OfType<RigField>().ToHashSet();

        _output.WriteLine("    IN THE BLOCK (owned: the nine)");
        Voices(run, point, state, owned, run.Results, movedOn: false);
        _output.WriteLine("    MOVED INTO A BLOCK THAT STATES NOTHING (owned: none)");
        Voices(run, point, state, new HashSet<RigField>(), null, movedOn: true);
        _output.WriteLine($"    MOVED INTO FT8's BLOCK (owned: {string.Join(", ", ft8.OrderBy(f => f.ToString()))})");
        Voices(run, point, state, ft8, null, movedOn: true);
    }

    private static string WritesText(ScriptedRadio radio)
    {
        var writes = ModeEntryBench.Writes(radio);
        return writes.Count == 0
            ? "none"
            : string.Join(", ", writes.Select(w => $"{w.Field} {w.Value}"));
    }

    // Whether Hamlet set this field and it still holds what Hamlet left.
    private static bool HamletsValue(Run run, RigField field, RigState state)
        => run.Memory.LastSet.TryGetValue(field, out var left)
           && state[field] is { IsKnown: true, Number: { } n }
           && (int)n == left;

    private void Voices(
        Run run, string point, RigState state, IReadOnlySet<RigField> owned,
        IReadOnlyList<ConditionResult>? results, bool movedOn)
    {
        var lines = new List<string>();

        void Ask(RigField field, string voice, string sentence)
        {
            var hamlets = HamletsValue(run, field, state);
            var stillOwned = hamlets && owned.Contains(field);
            var mark = stillOwned
                ? " [ASKS HIM TO CHANGE A FIELD HAMLET SET AND OWNS]"
                : hamlets && movedOn ? " [asks him to change a field Hamlet left, not owned here]" : "";
            if (stillOwned)
            {
                _owned[(point, field)] = _owned.GetValueOrDefault((point, field)) + 1;
            }
            else if (hamlets && movedOn)
            {
                _notOwned[(point, field)] = _notOwned.GetValueOrDefault((point, field)) + 1;
            }

            lines.Add($"{field}: {voice}: {Quote(sentence)}{mark}");
        }

        void Say(RigField field, string voice, string sentence)
            => lines.Add($"{field}: {voice}: {Quote(sentence)}");

        if (results is not null)
        {
            foreach (var field in Nine)
            {
                var mine = results.Where(r => r.Condition.Field == field).ToList();
                var clause = ReceiverSetupVoice.Say(mine);
                var stale = mine.FirstOrDefault() is { Outcome: ConditionOutcome.Changed or ConditionOutcome.AlreadyRight, NowText: { } now }
                            && state[field].IsKnown
                            && !string.Equals(now, state[field].Text, StringComparison.OrdinalIgnoreCase)
                            && HamletsValue(run, field, state);
                if (stale)
                {
                    Ask(field, "setup (ReceiverSetupVoice.Say), names a value the radio no longer holds", clause);
                }
                else if (clause.Length > 0)
                {
                    Say(field, "setup (ReceiverSetupVoice.Say)", clause);
                }
            }
        }

        foreach (var advice in ReceiveAdvice.For(state, owned))
        {
            var field = advice.Write.Field;
            if (!Nine.Contains(field))
            {
                continue;
            }

            if (advice.WouldChange && advice.Value != (int?)state[field].Number)
            {
                Ask(field, $"Receive Help (ReceiveAdvice), proposes {field} {advice.Value}", advice.Says);
            }
            else
            {
                Say(field, "Receive Help (ReceiveAdvice)", advice.Says);
            }
        }

        foreach (var observation in RigObservations.For(state, owned))
        {
            foreach (var field in Named(observation))
            {
                Ask(field, "observation (RigObservations)", observation);
            }
        }

        var inMorse = state.Mode is { } mode && CivValues.IsCw(mode);
        foreach (var obstruction in ReceiveObstructions.For(state, inMorse, competitorInside: true))
        {
            foreach (var field in Named(obstruction.Setting + " " + obstruction.Says))
            {
                Ask(field, "obstruction (ReceiveObstructions, advisory line)", obstruction.Says);
            }
        }

        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var preampOn = state[RigField.Preamp] is { IsKnown: true, Number: 1 or 2 };
        var attenuatorIn = state[RigField.Attenuator] is { IsKnown: true, Number: > 0 };
        var preampLabel = MainWindowViewModel.PreampLabel(state[RigField.Preamp].IsKnown ? (int?)state[RigField.Preamp].Number : null);
        var attenuatorLabel = MainWindowViewModel.AttenuatorLabel(state[RigField.Attenuator].IsKnown ? (int?)state[RigField.Attenuator].Number : null);
        Say(RigField.Preamp, "panel chip (FrontEndTextFor)", MainWindowViewModel.FrontEndTextFor(overloading, preampLabel, attenuatorLabel));

        var sentence = MainWindowViewModel.OverflowAdviceFor(
            overloading, preampOn,
            owned.Contains(RigField.Preamp) && owned.Contains(RigField.Attenuator),
            attenuatorIn);
        if (sentence.Contains("until the preamp reads off", StringComparison.Ordinal))
        {
            Ask(RigField.Preamp, "panel overload sentence (OverflowAdviceFor)", sentence);
        }
        else if (sentence.Contains("to bring it in", StringComparison.Ordinal))
        {
            Ask(RigField.Attenuator, "panel overload sentence (OverflowAdviceFor)", sentence);
        }
        else if (sentence.Length > 0)
        {
            Say(RigField.Preamp, "panel overload sentence (OverflowAdviceFor)", sentence);
        }

        foreach (var line in lines)
        {
            _output.WriteLine($"      {line}");
        }
    }

    // The fields a sentence names, in the words the voices use for them.
    internal static IEnumerable<RigField> Named(string sentence)
    {
        var words = new (string Word, RigField Field)[]
        {
            ("automatic notch", RigField.AutoNotch),
            ("manual notch", RigField.ManualNotch),
            ("noise blanker", RigField.NoiseBlanker),
            ("noise reduction", RigField.NoiseReduction),
            ("gain control", RigField.Agc),
            ("receive gain", RigField.RfGain),
            ("squelch", RigField.Squelch),
            ("attenuator", RigField.Attenuator),
            ("preamp", RigField.Preamp),
        };

        return words
            .Where(w => sentence.Contains(w.Word, StringComparison.OrdinalIgnoreCase))
            .Select(w => w.Field)
            .Distinct();
    }

    private static string Quote(string s) => s.Length == 0 ? "(says nothing)" : $"\"{s}\"";
}
