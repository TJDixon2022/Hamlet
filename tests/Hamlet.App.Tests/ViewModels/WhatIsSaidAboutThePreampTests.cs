using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 424 task 1, criterion 7.8: what the preamp is set to on each
/// band, and every sentence any component would say to the operator about it there.
/// </summary>
/// <remarks>
/// <para>**A FACT THAT ASSERTS NOTHING.** It drives the real
/// <see cref="ReceiverSetup"/> against <see cref="ScriptedRadio"/> exactly as
/// `EstablishReceiveConditionsAsync` does, with the block's own conditions, and prints
/// per frequency what the condition asks for, what was written, and verbatim what the
/// setup's own voice, the advice, the observations, the obstructions and the main
/// panel's front-end chip and overload sentence say about the preamp. Then the same with
/// the front end reading overloading. Printed before task 2 changes anything and again
/// at exit.</para>
/// <para>**ONE FREQUENCY ON EACH HF BAND AND ONE AT 50 MHZ**, which is 7.8's table:
/// the five the work instruction names (7.030, 14.050, 21.050, 28.050, 50.100) and one on
/// each of 160, 80, 30, 17 and 12 m. A frequency with no block on the map gets no
/// tune-in in the app; the CW row is then driven directly and the line says so.</para>
/// <para>**AND WHAT IS SAID ONCE HE HAS MOVED ON.** After the CW tune-in, the voices are
/// asked again as they would be in a block that states nothing (no field owned) and in
/// an FT8 block (FT8's fields owned), because a field Hamlet set does not stop being one
/// it set when the dial leaves the block.</para>
/// <para>**NO RADIO IS ON THIS MACHINE** (FACT-006): every line is an indication
/// against a scripted radio.</para>
/// </remarks>
public sealed class WhatIsSaidAboutThePreampTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where it is printed.</param>
    public WhatIsSaidAboutThePreampTests(ITestOutputHelper output) => _output = output;

    /// <summary>One frequency per HF band and one at 50 MHz, lowest first.</summary>
    internal static readonly (string Band, long Hz)[] Frequencies =
    {
        ("160 m", 1_810_000),
        ("80 m", 3_530_000),
        ("40 m", 7_030_000),
        ("30 m", 10_110_000),
        ("20 m", 14_050_000),
        ("17 m", 18_080_000),
        ("15 m", 21_050_000),
        ("12 m", 24_900_000),
        ("10 m", 28_050_000),
        ("6 m", 50_100_000),
    };

    /// <summary>**THE TABLE.** Quiet, then overloading, at every frequency.</summary>
    [Fact]
    public async Task PrintWhatIsSaid()
    {
        var row = ReceiverConditions.ForMode("CW").First(c => c.Field == RigField.Preamp);

        _output.WriteLine("THE CW PREAMP CONDITION");
        _output.WriteLine($"  wanted     {row.Wanted}");
        _output.WriteLine($"  wantedText {row.WantedText}");
        _output.WriteLine($"  confirmed  {row.Confirmed}");
        _output.WriteLine($"  condition  {row.Condition}");
        _output.WriteLine($"  says       {row.Says}");
        _output.WriteLine($"  because    {row.Because}");

        foreach (var overloading in new[] { false, true })
        {
            _output.WriteLine("");
            _output.WriteLine(overloading
                ? "######## THE FRONT END READING OVERLOADING AT THE TUNE-IN"
                : "######## A QUIET FRONT END");

            foreach (var (band, hz) in Frequencies)
            {
                await CaseAsync(band, hz, overloading);
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            "SENTENCES CONTRADICTING WHAT HAMLET SET, where the app tunes in (a block on the map), "
            + "quiet and overloading together:");
        _output.WriteLine($"  as the app asks right after the tune-in: {_inBlock}");
        _output.WriteLine($"  once he has moved into a block that does not state the preamp: {_movedOn}");
        _output.WriteLine($"  total: {_inBlock + _movedOn}");
        _output.WriteLine(
            "  (a sentence counts when the setup's own clause names a preamp value other than the one it "
            + "left, when the advice proposes a preamp other than the one Hamlet set, when an observation "
            + "objects to the preamp, or when the overload sentence names P.AMP/ATT for a preamp or "
            + "attenuator Hamlet set)");
    }

    private int _inBlock;
    private int _movedOn;

    private async Task CaseAsync(string band, long hz, bool overloading)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);
        var conditions = fromBlock.Count > 0 ? fromBlock : ReceiverConditions.ForMode("CW");

        _output.WriteLine("");
        _output.WriteLine($"=== {hz / 1e6:0.000} MHz, {band}{(overloading ? ", overloading" : "")}");
        _output.WriteLine(
            fromBlock.Count > 0
                ? $"  block: {block!.Name} ({block.ShortName}), states {fromBlock.Count} conditions"
                : $"  block: {(block is null ? "none on the map" : $"{block.Name} ({block.ShortName}), states nothing")}"
                  + " - THE APP WRITES NOTHING HERE; the CW row is driven directly below");

        // The radio as an operator might have left it: preamp off, which is what the
        // old rule wanted at 40 m and below and the manual does not.
        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Overloading = overloading;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var before = await ModeEntryBench.ReadAllAsync(rig);
        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, conditions, ReceiverSetupMemory.Empty);
        var writes = ModeEntryBench.Writes(radio)
            .Where(w => w.Field == RigField.Preamp)
            .Select(w => w.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        var after = await ModeEntryBench.ReadAllAsync(rig);

        var result = results.First(r => r.Condition.Field == RigField.Preamp);
        _output.WriteLine(
            $"  asks: {result.Condition.WantedText} [wanted {result.Condition.Wanted}, "
            + $"condition {result.Condition.Condition}]");
        _output.WriteLine(
            $"  radio said {before[RigField.Preamp].Text}; written {(writes.Count > 0 ? string.Join(",", writes) : "nothing")}; "
            + $"filed {result.Outcome}; now {after[RigField.Preamp].Text}; "
            + $"overflow {after[RigField.Overflow].Text}");

        var owned = ReceiverSetup.Owns(results);
        var counted = block is not null && fromBlock.Count > 0;

        _output.WriteLine("  WHAT IS SAID ABOUT THE PREAMP, AS THE APP ASKS IT AFTER THIS TUNE-IN");
        var here = Voices(after, owned, results);

        _output.WriteLine("  ...AND ONCE HE HAS MOVED INTO A BLOCK THAT STATES NOTHING (no field owned)");
        var nothing = Voices(after, new HashSet<RigField>(), null);

        var ft8 = ReceiverConditions.ForMode("FT8")
            .Select(c => c.Field).OfType<RigField>().ToHashSet();
        _output.WriteLine(
            $"  ...AND ONCE HE HAS MOVED INTO AN FT8 BLOCK (owned: {string.Join(", ", ft8.OrderBy(f => f.ToString()))})");
        var intoFt8 = Voices(after, ft8, null);

        if (counted)
        {
            _inBlock += here;
            _movedOn += nothing + intoFt8;
        }
    }

    // Prints every sentence and returns how many of them contradict what the setup left.
    private int Voices(
        RigState state, IReadOnlySet<RigField> owned, IReadOnlyList<ConditionResult>? results)
    {
        var said = new List<string>();
        var contradicting = 0;
        var now = state[RigField.Preamp].Text;

        if (results is not null)
        {
            var preamp = results.Where(r => r.Condition.Field == RigField.Preamp).ToList();
            var setup = ReceiverSetupVoice.Say(preamp);
            var other = OtherValueNamed(setup, now);
            contradicting += other ? 1 : 0;
            said.Add(
                $"setup (ReceiverSetupVoice.Say, the preamp's clause): {Quote(setup)}"
                + (other ? $" [NAMES A VALUE OTHER THAN THE {now} IT LEFT]" : ""));
            said.Add($"setup admissions (on the bar): {Quote(ReceiverSetupVoice.Admissions(preamp))}");
        }

        var advice = ReceiveAdvice.For(state, owned).First(a => a.Write.Field == RigField.Preamp);
        var proposes = advice.WouldChange && advice.Value != (int?)state[RigField.Preamp].Number;
        contradicting += proposes ? 1 : 0;
        said.Add(
            $"advice (ReceiveAdvice, Receive Help): {Quote(advice.Says)}"
            + (proposes ? $" [PROPOSES A WRITE: preamp {advice.Value}]" : ""));

        var observations = RigObservations.For(state, owned)
            .Where(o => o.Contains("preamp", StringComparison.OrdinalIgnoreCase))
            .ToList();
        contradicting += observations.Count;
        said.Add(
            "observations (RigObservations, diagnostics): "
            + (observations.Count > 0 ? string.Join(" | ", observations.Select(Quote)) + " [OBJECTS]" : "nothing about the preamp"));

        var inMorse = state.Mode is { } mode && CivValues.IsCw(mode);
        var obstructions = ReceiveObstructions.For(state, inMorse, competitorInside: true)
            .Where(o => o.Says.Contains("preamp", StringComparison.OrdinalIgnoreCase)
                        || o.Setting.Contains("preamp", StringComparison.OrdinalIgnoreCase))
            .ToList();
        said.Add(
            "obstructions (ReceiveObstructions, the advisory line): "
            + (obstructions.Count > 0 ? string.Join(" | ", obstructions.Select(o => Quote(o.Says))) : "nothing about the preamp"));

        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var preampOn = state[RigField.Preamp] is { IsKnown: true, Number: 1 or 2 };
        var preampLabel = MainWindowViewModel.PreampLabel(
            state[RigField.Preamp].IsKnown ? (int?)state[RigField.Preamp].Number : null);
        var attenuatorLabel = MainWindowViewModel.AttenuatorLabel(
            state[RigField.Attenuator].IsKnown ? (int?)state[RigField.Attenuator].Number : null);
        said.Add(
            "panel chip (FrontEndTextFor): "
            + Quote(MainWindowViewModel.FrontEndTextFor(overloading, preampLabel, attenuatorLabel)));
        var sentence = MainWindowViewModel.OverflowAdviceFor(
            overloading, preampOn,
            owned.Contains(RigField.Preamp) && owned.Contains(RigField.Attenuator));
        var knob = sentence.Contains("P.AMP/ATT", StringComparison.Ordinal);
        contradicting += knob ? 1 : 0;
        said.Add(
            "panel overload sentence (OverflowAdviceFor): " + Quote(sentence)
            + (knob ? " [ASKS HIM TO TURN P.AMP/ATT, WHICH HAMLET SET]" : ""));

        foreach (var line in said)
        {
            _output.WriteLine($"    {line}");
        }

        return contradicting;
    }

    // Whether a sentence names a preamp value other than the one the radio holds, in
    // the radio's own words: off, preamp 1, preamp 2.
    private static bool OtherValueNamed(string sentence, string now)
    {
        if (sentence.Length == 0)
        {
            return false;
        }

        var named = new[] { "off", "preamp 1", "preamp 2" }
            .Where(v => System.Text.RegularExpressions.Regex.IsMatch(
                sentence, $@"\b{v}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        return named.Any(v => !v.Equals(now, StringComparison.OrdinalIgnoreCase));
    }

    private static string Quote(string s) => s.Length == 0 ? "(says nothing)" : $"\"{s}\"";
}
