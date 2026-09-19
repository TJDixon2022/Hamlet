using System;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 365 task 4: **the send cap and the turn indicator's patience are characters of
/// the variant, not figures in seconds** (step 4 criterion 4.4, `PHASE_PLAN.md` §3.2, decision AV).
/// </summary>
/// <remarks>
/// <para>**THE THREE COUNTS ARE THE FILE'S.** They are read through <see cref="OliviaTiming"/> from
/// `data/olivia/timing.json`, the same way the retire factor is, and this test reads the file off
/// the disk itself to say so.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaTimingTests
{
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts and the seconds are printed.</param>
    public TheOliviaTimingTests(ITestOutputHelper output) => _output = output;

    private static OliviaTiming Timing
        => OliviaData.Current.Timing ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    /// <summary>**4.4 at the engine: the cap and the patience are the timing table's characters.**</summary>
    [Fact]
    public void TheCapAndThePatienceAreCharactersOfTheVariant()
    {
        var spc = Unit365Trace.Psk31SecondsPerCharacter();
        var patienceSeconds = Psk31Macros.AnswerSeconds(Unit365Trace.His, Unit365Trace.Mine);

        _output.WriteLine(
            $"PSK31: {spc:0.000000} s a character on the four framed macros, idle left out; "
            + $"patience {patienceSeconds:0.0000} s (Psk31Macros.AnswerSeconds, R18's stated slot)");
        _output.WriteLine(
            $"counts from timing.json: macro {Timing.CapMacroCharacters}, typed {Timing.CapTypedCharacters}, "
            + $"patience {Timing.PatienceCharacters}");

        // THE COUNTS ARE PSK31'S OWN RULES COUNTED IN PSK31 CHARACTERS (decision AV).
        Assert.Equal((int)Math.Floor(OperatorSend.LongestUnslottedSeconds / spc), Timing.CapMacroCharacters);
        Assert.Equal((int)Math.Floor(60 / spc), Timing.CapTypedCharacters);
        Assert.Equal((int)Math.Ceiling(patienceSeconds / spc), Timing.PatienceCharacters);

        // AND THEY ARE READ FROM THE FILE, NOT WRITTEN IN CODE.
        var onDisk = OliviaTiming.Parse(File.ReadAllText(Path.Combine(OliviaFixtures.Root(), OliviaTiming.FilePath)));

        Assert.Equal(onDisk.CapMacroCharacters, Timing.CapMacroCharacters);
        Assert.Equal(onDisk.CapTypedCharacters, Timing.CapTypedCharacters);
        Assert.Equal(onDisk.PatienceCharacters, Timing.PatienceCharacters);
        Assert.NotEqual(0, onDisk.CapMacroCharacters);

        // **AND NOTHING IN THE CODE DECIDES THEM**: the same type, handed a file carrying other
        // counts, gives those. A count written into the code could not follow the file this way,
        // and a scan for the numbers themselves would only find `pj_mfsk.h:121` and `TryGetInt32`.
        var altered = OliviaTiming.Parse(File.ReadAllText(Path.Combine(OliviaFixtures.Root(), OliviaTiming.FilePath))
            .Replace("\"cap_macro_characters\": " + onDisk.CapMacroCharacters, "\"cap_macro_characters\": 7", StringComparison.Ordinal)
            .Replace("\"cap_typed_characters\": " + onDisk.CapTypedCharacters, "\"cap_typed_characters\": 11", StringComparison.Ordinal)
            .Replace("\"patience_characters\": " + onDisk.PatienceCharacters, "\"patience_characters\": 13", StringComparison.Ordinal));

        Assert.Equal(7, altered.CapMacroCharacters);
        Assert.Equal(11, altered.CapTypedCharacters);
        Assert.Equal(13, altered.PatienceCharacters);
        Assert.Equal(altered.SecondsPerCharacter["8/250"] * 7, altered.CapSeconds("8/250", OliviaSendKind.Macro), 9);

        // EACH VARIANT'S SECONDS ARE ITS COUNT TIMES ITS SECONDS PER CHARACTER.
        foreach (var (variant, seconds) in Timing.SecondsPerCharacter.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            _output.WriteLine(
                $"{variant}: {seconds:0.00000} s a character; macro cap {Timing.CapSeconds(variant, OliviaSendKind.Macro):0.000} s, "
                + $"typed cap {Timing.CapSeconds(variant, OliviaSendKind.TypedLine):0.000} s, "
                + $"patience {Timing.PatienceSeconds(variant):0.000} s");

            Assert.Equal(seconds * Timing.CapMacroCharacters, Timing.CapSeconds(variant, OliviaSendKind.Macro), 9);
            Assert.Equal(seconds * Timing.CapTypedCharacters, Timing.CapSeconds(variant, OliviaSendKind.TypedLine), 9);
            Assert.Equal(seconds * Timing.PatienceCharacters, Timing.PatienceSeconds(variant), 9);
        }

        // THE FRAMED REPORT FITS AT 8/250, WHICH IS WHAT THE CAP IN CHARACTERS IS FOR.
        var report = Psk31Macros.Report(Unit365Trace.His, Unit365Trace.Mine, Unit365Trace.Name, Unit365Trace.Qth, Unit365Trace.Grid);
        var composed = OliviaModulator.Compose(report, "8/250", 1000, Rate, Ft8Composer.DefaultDrivePeak);

        _output.WriteLine(
            $"the Report at 8/250: {report.Length} characters, {composed.TextSeconds:0.000} s of text after a "
            + $"{composed.AnnouncementSeconds:0.000} s announcement, cap {composed.Cap:0.000} s, fit {composed.Fit}");

        Assert.Equal(UnslottedFit.Fits, composed.Fit);

        // AND A TEXT ONE CHARACTER OVER THE MACRO COUNT DOES NOT.
        var over = new string('e', Timing.CapMacroCharacters + 1);
        var overComposed = OliviaModulator.Compose(over, "8/250", 1000, Rate, Ft8Composer.DefaultDrivePeak);

        _output.WriteLine(
            $"one character over at 8/250: {over.Length} characters, {overComposed.TextSeconds:0.000} s of text, "
            + $"cap {overComposed.Cap:0.000} s, fit {overComposed.Fit}");

        Assert.Equal(UnslottedFit.LongerThanTheCap, overComposed.Fit);
    }
}
