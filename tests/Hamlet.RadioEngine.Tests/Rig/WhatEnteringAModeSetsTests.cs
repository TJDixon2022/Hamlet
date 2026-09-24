using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 1, criterion 7.5's last clause: the table of every
/// field a tune-in touches, for CW at 14.050 and 7.030 and for each data mode the
/// conditions file speaks for.
/// </summary>
/// <remarks>
/// <para>**THE TABLE, AND IT ASSERTS NOTHING.** It drives the real
/// <see cref="ReceiverSetup"/> against <see cref="ScriptedRadio"/> and prints, per
/// field, what the condition asks for, what the radio answered, whether a write
/// was sent, what the result was filed as, which components mention the field,
/// and whether any of them would ask the operator to change it after the setup
/// has set it. Printed before task 2 changes anything and again after.</para>
/// <para>**14.050 IS ABOVE 40 M AND 7.030 IS AT IT**, so both halves of the
/// preamp's band rule appear.</para>
/// <para>**NO RADIO IS ON THIS MACHINE** (FACT-006): every line is an indication
/// against a scripted radio.</para>
/// </remarks>
public sealed class WhatEnteringAModeSetsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the table.</summary>
    /// <param name="output">Where it is printed.</param>
    public WhatEnteringAModeSetsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Who mentions each field, read from the code: the setup through the mode's
    /// conditions, and the four voices by what they look at.
    /// </summary>
    /// <remarks>
    /// ReceiveAdvice.For reads AutoNotch, NoiseReduction, NoiseBlanker, Agc,
    /// FilterBandwidth, Preamp, RfGain and AccUsbAfLevel; RigObservations.For
    /// reads the filter, NoiseBlanker, NoiseReduction, AutoNotch, Attenuator with
    /// Preamp, SquelchStatus with Squelch, and the USB output; ReceiveObstructions
    /// reads NoiseBlanker, NoiseReduction and the filter; the main panel's front-end
    /// chip and OverflowAdviceFor read Overflow, Preamp and Attenuator.
    /// </remarks>
    private static readonly Dictionary<RigField, string[]> Voices = new()
    {
        [RigField.AutoNotch] = new[] { "advice", "observations" },
        [RigField.ManualNotch] = Array.Empty<string>(),
        [RigField.NoiseBlanker] = new[] { "advice", "observations", "obstructions" },
        [RigField.NoiseReduction] = new[] { "advice", "observations", "obstructions" },
        [RigField.Agc] = new[] { "advice" },
        [RigField.RfGain] = new[] { "advice" },
        [RigField.Squelch] = new[] { "observations" },
        [RigField.Attenuator] = new[] { "observations", "panel" },
        [RigField.Preamp] = new[] { "advice", "observations", "panel" },
    };

    /// <summary>
    /// **THE TABLE.** CW at 14.050 and 7.030, FT8 and FT4, each from the radio as
    /// an operator left it; then the already-right count; then the operator's hand.
    /// </summary>
    [Fact]
    public async Task PrintTheTable()
    {
        _output.WriteLine(
            "modes the conditions file speaks for: "
            + string.Join(", ", ReceiverConditions.Modes.OrderBy(m => m)));

        foreach (var mode in ReceiverConditions.Modes.OrderBy(m => m))
        {
            foreach (var c in ReceiverConditions.ForMode(mode).Where(c => !c.Confirmed))
            {
                _output.WriteLine(
                    $"  {mode} {c.Control}: confirmed false, stated and not written "
                    + $"(asks {c.WantedText}, confirm {c.Confirm})");
            }
        }

        await CaseAsync("CW at 14.050 MHz", 14_050_000, data: false);
        await CaseAsync("CW at 7.030 MHz", 7_030_000, data: false);
        await CaseAsync(
            "CW at 14.050 MHz, the front end reading overloading", 14_050_000,
            data: false, overloading: true);
        await CaseAsync("FT8 at 14.074 MHz", 14_074_000, data: true);
        await CaseAsync("FT4 at 14.080 MHz", 14_080_000, data: true);

        await AlreadyRightAsync(14_050_000);
        await AlreadyRightAsync(7_030_000);

        await TheHandAsync(14_050_000, preampAlreadyOn: false);
        await TheHandAsync(14_050_000, preampAlreadyOn: true);
    }

    // What the app would hand the setup is the block's conditions; where the
    // block states none, the CW row is driven directly as well, so the table
    // shows what entering CW there would write and says that the app does not.
    private async Task CaseAsync(string title, long hz, bool data, bool overloading = false)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);

        _output.WriteLine("");
        _output.WriteLine($"=== {title}");
        _output.WriteLine(
            $"the app's block here: {block?.Name ?? "none"} ({block?.ShortName ?? "-"}, "
            + $"{block?.LowHz}-{block?.HighHz}), which states {fromBlock.Count} conditions");

        var conditions = fromBlock;

        if (fromBlock.Count == 0)
        {
            var mode = data ? "FT8" : "CW";
            conditions = ReceiverConditions.ForMode(mode);

            _output.WriteLine(
                $"  THE APP WRITES NOTHING HERE. Below, the {mode} row driven directly, "
                + "which is what entering the mode would write if the block stated it.");
        }

        var radio = ModeEntryBench.AsLeft(hz, data);
        radio.Overloading = overloading;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var before = await ModeEntryBench.ReadAllAsync(rig);
        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, conditions, ReceiverSetupMemory.Empty);
        var writes = ModeEntryBench.Writes(radio);
        var after = await ModeEntryBench.ReadAllAsync(rig);

        Print(results, before, writes, after);
    }

    private void Print(
        IReadOnlyList<ConditionResult> results,
        RigState before,
        IReadOnlyList<(RigField Field, int Value)> writes,
        RigState after)
    {
        var inMorse = after.Mode is { } m && CivValues.IsCw(m);
        var advice = ReceiveAdvice.For(after);
        var observations = RigObservations.For(after);
        var obstructions = ReceiveObstructions.For(after, inMorse, competitorInside: false);

        _output.WriteLine(
            $"  {"control",-16}{"asks",-44}{"radio said",-12}{"write sent",-12}"
            + $"{"filed as",-19}{"now",-10}{"mentioned by",-40}asks him to change it after");

        foreach (var r in results)
        {
            var c = r.Condition;
            var field = c.Field;
            var sent = field is { } f
                ? string.Join(",", writes.Where(w => w.Field == f).Select(w => w.Value.ToString()))
                : "";

            var mentioned = new List<string> { "setup" };
            if (field is { } ff && Voices.TryGetValue(ff, out var v))
            {
                mentioned.AddRange(v);
            }

            var asks = new List<string>();
            if (field is { } g)
            {
                asks.AddRange(advice
                    .Where(a => a.Write.Field == g && a.WouldChange)
                    .Select(a => "advice: " + Clip(a.Says)));

                var word = Word(g);
                if (word.Length > 0)
                {
                    asks.AddRange(observations
                        .Where(o => o.Contains(word, StringComparison.OrdinalIgnoreCase))
                        .Select(o => "observations: " + Clip(o)));

                    asks.AddRange(obstructions
                        .Where(o => o.Setting.Contains(word, StringComparison.OrdinalIgnoreCase))
                        .Select(o => "obstructions: " + Clip(o.Says)));
                }
            }

            _output.WriteLine(
                $"  {c.Control,-16}"
                + $"{Clip($"{c.WantedText} [{c.Wanted?.ToString() ?? "-"}{(c.IsConditional ? ", " + c.Condition : "")}{(c.Confirmed ? "" : ", unconfirmed")}]", 42),-44}"
                + $"{(field is { } h ? before[h].Text : "-"),-12}"
                + $"{(sent.Length > 0 ? sent : "none"),-12}"
                + $"{r.Outcome,-19}"
                + $"{(field is { } i ? after[i].Text : "-"),-10}"
                + $"{string.Join("+", mentioned),-40}"
                + (asks.Count > 0 ? string.Join(" | ", asks) : "no"));
        }

        var overloading = after[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var preampOn = after[RigField.Preamp] is { IsKnown: true, Number: 1 or 2 };

        _output.WriteLine(
            "  panel (MainWindowViewModel.OverflowAdviceFor): speaks on preamp and attenuator "
            + $"only while the overflow reads overloading; overflow now {after[RigField.Overflow].Text}, "
            + (!overloading
                ? "so it says nothing"
                : preampOn
                    ? "preamp on, so it asks him to press P.AMP/ATT until the preamp reads off"
                    : "preamp off, so it asks him to hold P.AMP/ATT for the attenuator"));
    }

    // The already-right count: a radio at every value the CW row asks for, one
    // tune-in, and how many fields were written anyway. This is the unit's number.
    private async Task AlreadyRightAsync(long hz)
    {
        var radio = ModeEntryBench.AlreadyRightForCw(hz);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForMode("CW"), ReceiverSetupMemory.Empty);
        var writes = ModeEntryBench.Writes(radio);

        _output.WriteLine("");
        _output.WriteLine(
            $"=== already right, CW row at {hz / 1e6:0.000} MHz: fields written when the "
            + $"radio was already right: {writes.Select(w => w.Field).Distinct().Count()}");

        foreach (var (field, value) in writes)
        {
            var r = results.First(x => x.Condition.Field == field);
            _output.WriteLine(
                $"  wrote {field} = {value}; the radio read {r.WasText}; filed {r.Outcome}");
        }
    }

    // The operator's hand: a tune-in, then he sets the preamp off himself, then
    // a second tune-in of the same mode on the same band.
    private async Task TheHandAsync(long hz, bool preampAlreadyOn)
    {
        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Switches[ModeEntryBench.Preamp] = (byte)(preampAlreadyOn ? 1 : 0);
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var cw = ReceiverConditions.ForMode("CW");
        var (first, memory) = await ReceiverSetup.ApplyAsync(rig, cw, ReceiverSetupMemory.Empty);

        radio.OperatorTurnsASwitch(ModeEntryBench.Preamp, 0);
        ModeEntryBench.ClearWrites(radio);

        var (second, _) = await ReceiverSetup.ApplyAsync(rig, cw, memory);
        var wrote = ModeEntryBench.Writes(radio).Where(w => w.Field == RigField.Preamp).ToList();

        _output.WriteLine("");
        _output.WriteLine(
            $"=== the hand at {hz / 1e6:0.000} MHz, preamp {(preampAlreadyOn ? "already 1" : "off")} "
            + "before the first tune-in");
        _output.WriteLine(
            $"  first tune-in filed the preamp {first.First(r => r.Condition.Field == RigField.Preamp).Outcome}; "
            + $"memory holds preamp: {(memory.LastSet.TryGetValue(RigField.Preamp, out var p) ? p.ToString() : "nothing")}");
        _output.WriteLine(
            $"  he sets it off by hand; second tune-in filed {second.First(r => r.Condition.Field == RigField.Preamp).Outcome}, "
            + $"wrote preamp {(wrote.Count > 0 ? string.Join(",", wrote.Select(w => w.Value)) : "nothing")}, "
            + $"radio now preamp {radio.Switches[ModeEntryBench.Preamp]}");
    }

    private static string Word(RigField field) => field switch
    {
        RigField.Preamp => "preamp",
        RigField.Attenuator => "attenuator",
        RigField.NoiseBlanker => "noise blanker",
        RigField.NoiseReduction => "noise reduction",
        RigField.AutoNotch => "automatic notch",
        RigField.Squelch => "squelch",
        _ => "",
    };

    private static string Clip(string s, int n = 60)
        => s.Length <= n ? s : s[..(n - 3)] + "...";
}
