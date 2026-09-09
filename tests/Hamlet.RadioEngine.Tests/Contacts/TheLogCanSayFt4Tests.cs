using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 291, task 1: the starting position, recorded before the field
/// lands — an FT4 contact cannot be expressed at all.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** An FT4 contact written as
/// `MODE=FT4`. It looks valid, it is not valid ADIF — the specification files FT4
/// as a **submode of `MFSK`** — and another logger rejects the file without
/// explanation. `PHASE_PLAN.md` names this as the fault that outlives everything
/// else in the project: a record naming a mode the contact was not made in is
/// wrong for as long as the log exists, and there is nothing later that can tell
/// it from a true one.</para>
/// <para>**IT ASSERTS THE ABSENCE ON PURPOSE.** Each test below fails the moment
/// the submode lands, which is the point — this is the before picture, and tasks 2
/// and 4 rewrite it into the after one.</para>
/// </remarks>
public sealed class TheLogCanSayFt4Tests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the records are printed.</param>
    public TheLogCanSayFt4Tests(ITestOutputHelper output)
        => _output = output;

    /// <summary>`AdifContact` carries no submode of any kind.</summary>
    /// <remarks>
    /// **BY REFLECTION RATHER THAN BY EYE**, because what is being asserted is
    /// that no property exists — and a test that named one would not compile,
    /// so there is no way to write this against the type directly.
    /// </remarks>
    [Fact]
    public void TodayAdifContactHasNoSubmodeProperty()
    {
        var names = typeof(AdifContact)
            .GetProperties()
            .Select(p => p.Name)
            .OrderBy(n => n, System.StringComparer.Ordinal)
            .ToList();

        _output.WriteLine(
            names.Count + " properties: " + string.Join(", ", names));

        Assert.DoesNotContain(
            names,
            n => n.Contains("Submode", System.StringComparison.OrdinalIgnoreCase));

        // Twelve init properties, counted rather than recalled. Stated so a
        // property added without a thought about the log is visible here.
        // (`EqualityContract` is protected and so is not in this list.)
        Assert.Equal(12, names.Count);
    }

    /// <summary>An FT4 contact comes out of the writer with no `SUBMODE`.</summary>
    /// <remarks>
    /// **DRIVEN THROUGH THE APPLICATION'S OWN PATH AND NOT A HAND-MADE
    /// CONTACT** (§12.5, and unit 274's `20 m`). The conditions record is what
    /// `MainWindowViewModel` hands in, so what this writes is what the file would
    /// hold — including the fact that `MFSK` alone says nothing about FT4.
    /// </remarks>
    [Fact]
    public void TodayAnFt4ContactComesOutWithNoSubmodeTag()
    {
        var text = AdifLog.Record(Ft4Contact());

        _output.WriteLine(text);

        // `MODE=MFSK` on its own is *some kind of multi-frequency shift keying*.
        // The mode the operator actually worked is nowhere in the record.
        Assert.Contains("<MODE:4>MFSK", text, System.StringComparison.Ordinal);
        Assert.DoesNotContain("SUBMODE", text, System.StringComparison.Ordinal);
        Assert.DoesNotContain("FT4", text, System.StringComparison.Ordinal);
    }

    /// <summary>And so the FT4 row cannot recognize its own record.</summary>
    /// <remarks>
    /// **THE REFUSAL IS DELIBERATE AND PERMANENT** (`ContactModes.cs:36-42`).
    /// Reading a bare `MODE=MFSK` as FT4 would show the operator a first he had
    /// not made, which is the §0.0 guess. What is missing is the record's half of
    /// the pair, not the matcher's.
    /// </remarks>
    [Fact]
    public void TodayTheFt4ModeCannotMatchARecord()
    {
        var ft4 = ContactModes.Named("FT4");

        Assert.NotNull(ft4);

        _output.WriteLine("FT4 spells as " + ft4.AdifSpelling);

        Assert.False(ft4.Matches("MFSK", null));

        // And the round trip confirms `null` is all a reader can ever hand it:
        // the record went through the writer and came back with a mode and
        // nothing else to give.
        var read = Assert.Single(AdifLog.Read(AdifLog.Record(Ft4Contact())));

        Assert.Equal("MFSK", read.Mode);
        Assert.False(ft4.Matches(read.Mode, null));
    }

    /// <summary>An FT4 contact as the one write path would produce it.</summary>
    private static AdifContact Ft4Contact()
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        var slot = new System.DateTime(2026, 9, 9, 14, 22, 30, System.DateTimeKind.Utc);

        ledger.RecordHeard("CQ IK4LZH JN54", slot);
        ledger.RecordSent("IK4LZH KC3QIS FN00", slot.AddSeconds(8));
        ledger.RecordHeard("KC3QIS IK4LZH -12", slot.AddSeconds(15));

        return Ft8ContactLogEntry.For(
            ledger.For("IK4LZH")!,
            "KC3QIS",

            // **`MFSK` IS THE HONEST HALF AND IT IS NOT ENOUGH.** The literal
            // stands in for what `MainWindowViewModel.cs:10288` hands in for FT8;
            // step 4 is what makes that call site say this.
            new Ft8StationConditions(7_047_500, "40 m", "MFSK", "FN00DJ"));
    }
}
