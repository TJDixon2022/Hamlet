using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 291: the log can say FT4. Written at task 1 asserting that it
/// could not, and rewritten at task 2 asserting that it can.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS CATCHES.** An FT4 contact written as `MODE=FT4`. It
/// looks valid, it is not valid ADIF — the specification files FT4 as a **submode
/// of `MFSK`** — and another logger rejects the file without explanation.
/// `PHASE_PLAN.md` names this as the fault that outlives everything else in the
/// project: a record naming a mode the contact was not made in is wrong for as
/// long as the log exists, and there is nothing later that can tell it from a
/// true one.</para>
/// <para>**WHAT THE BEFORE PICTURE SAID, KEPT HERE BECAUSE IT IS THE MEASUREMENT.**
/// At task 1, against commit `f13b644`, all three of these passed asserting the
/// opposite: `AdifContact` had twelve public properties and no submode, a contact
/// driven through `Ft8ContactLogEntry.For` with `MFSK` conditions wrote
/// `&lt;MODE:4&gt;MFSK` with the string `SUBMODE` nowhere in it, and
/// `ContactModes.Named("FT4")!.Matches("MFSK", null)` was false. **The third of
/// those is still false and always will be** — a bare `MODE=MFSK` is not FT4 —
/// and what changed is that a record can now carry the other half.</para>
/// </remarks>
public sealed class TheLogCanSayFt4Tests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the records are printed.</param>
    public TheLogCanSayFt4Tests(ITestOutputHelper output)
        => _output = output;

    /// <summary>`AdifContact` carries a submode, and exactly one.</summary>
    /// <remarks>
    /// **BY REFLECTION RATHER THAN BY EYE**, because the count is the assertion.
    /// At task 1 this read twelve and named no submode; a second submode-shaped
    /// property appearing later would be two places for the same fact to be
    /// wrong, and this is where that shows up.
    /// </remarks>
    [Fact]
    public void AdifContactCarriesOneSubmodeProperty()
    {
        var names = typeof(AdifContact)
            .GetProperties()
            .Select(p => p.Name)
            .OrderBy(n => n, System.StringComparer.Ordinal)
            .ToList();

        _output.WriteLine(
            names.Count + " properties: " + string.Join(", ", names));

        Assert.Single(
            names,
            n => n.Contains("Submode", System.StringComparison.OrdinalIgnoreCase));

        // Thirteen init properties, counted rather than recalled — twelve at
        // task 1 and the submode makes thirteen. (`EqualityContract` is
        // protected and so is not in this list.)
        Assert.Equal(13, names.Count);
    }

    /// <summary>An FT4 contact comes out spelled the way ADIF spells it.</summary>
    /// <remarks>
    /// **DRIVEN THROUGH THE APPLICATION'S OWN PATH AND NOT A HAND-MADE
    /// CONTACT** (§12.5, and unit 274's `20 m`). The conditions record is what
    /// `MainWindowViewModel` hands in, so what this writes is what the file would
    /// hold. At task 1 the same call produced `&lt;MODE:4&gt;MFSK` and nothing
    /// else — the mode the operator worked was nowhere in the record.
    /// </remarks>
    [Fact]
    public void AnFt4ContactComesOutAsMfskWithTheSubmode()
    {
        var text = AdifLog.Record(Ft4Contact());

        _output.WriteLine(text);

        Assert.Contains("<MODE:4>MFSK", text, System.StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:3>FT4", text, System.StringComparison.Ordinal);

        // **AND NEVER THE THING THAT LOOKS RIGHT.** `MODE=FT4` is not valid ADIF
        // and is the fault that outlives everything else in this project.
        Assert.DoesNotContain("<MODE:3>FT4", text, System.StringComparison.Ordinal);

        var read = Assert.Single(AdifLog.Read(text));

        Assert.Equal("MFSK", read.Mode);
        Assert.Equal("FT4", read.Submode);
    }

    /// <summary>A record with no submode is absent rather than empty.</summary>
    /// <remarks>
    /// **STEP 3'S THIRD CRITERION, AND THE ONE A CARELESS IMPLEMENTATION
    /// BREAKS.** `&lt;SUBMODE:0&gt;` asserts that an empty submode was observed,
    /// which is a claim about the air rather than a gap in the record. The string
    /// must not appear at all, and the round trip must bring it back as null
    /// rather than as "".
    /// </remarks>
    [Fact]
    public void ARecordWithNoSubmodeSaysNothingAtAll()
    {
        foreach (var contact in new[]
                 {
                     Ft8Contact(),
                     Ft8Contact() with { Submode = "" },
                     Ft8Contact() with { Submode = null },
                 })
        {
            var text = AdifLog.Record(contact);

            _output.WriteLine(
                "[" + (contact.Submode ?? "(null)") + "] -> "
                + text.Replace("\n", " ").Trim());

            Assert.DoesNotContain("SUBMODE", text, System.StringComparison.Ordinal);

            var read = Assert.Single(AdifLog.Read(text));

            // **NULL AND NOT "".** An empty string read back would make the two
            // indistinguishable to `Matches`, and a row could then light off a
            // record that says nothing.
            Assert.Null(read.Submode);
        }
    }

    /// <summary>A bare `MODE=MFSK` is still not FT4, and never becomes it.</summary>
    /// <remarks>
    /// **THE REFUSAL IS DELIBERATE AND PERMANENT** (`ContactModes.cs:36-42`) and
    /// this unit does not soften it. Reading a bare `MODE=MFSK` as FT4 would show
    /// the operator a first he had not made, which is the §0.0 guess. What
    /// changed at task 2 is that a record can now carry the other half — not that
    /// a record missing it is read charitably.
    /// </remarks>
    [Fact]
    public void ABareMfskRecordIsStillNotFt4()
    {
        var ft4 = ContactModes.Named("FT4");

        Assert.NotNull(ft4);

        _output.WriteLine("FT4 spells as " + ft4.AdifSpelling);

        Assert.False(ft4.Matches("MFSK", null));
        Assert.False(ft4.Matches("MFSK", ""));
        Assert.True(ft4.Matches("MFSK", "FT4"));

        // Through the file rather than by hand: a record written with the mode
        // and no submode comes back matching nothing.
        var bare = Assert.Single(
            AdifLog.Read(AdifLog.Record(Ft4Contact() with { Submode = null })));

        Assert.Equal("MFSK", bare.Mode);
        Assert.False(ft4.Matches(bare.Mode, bare.Submode));

        // And the whole record does match.
        var whole = Assert.Single(AdifLog.Read(AdifLog.Record(Ft4Contact())));

        Assert.True(ft4.Matches(whole.Mode, whole.Submode));
    }

    /// <summary>An FT4 contact as the file would hold it.</summary>
    /// <remarks>
    /// **AT TASK 2 THE SUBMODE IS SET ON THE CONTACT AND NOT CARRIED IN THROUGH
    /// THE CONDITIONS**, because the conditions record still takes two independent
    /// strings and that is the shape task 3 replaces. This method is what changes
    /// there; what it asserts about the file does not.
    /// </remarks>
    private static AdifContact Ft4Contact()
    {
        var ft4 = ContactModes.Named("FT4")!;

        return Contact(ft4.AdifModes[0]) with { Submode = ft4.AdifSubmode };
    }

    /// <summary>The same, on FT8, which takes no submode.</summary>
    private static AdifContact Ft8Contact() => Contact("FT8");

    /// <summary>One contact off the ledger, in the mode handed in.</summary>
    private static AdifContact Contact(string mode)
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        var slot = new System.DateTime(2026, 9, 9, 14, 22, 30, System.DateTimeKind.Utc);

        ledger.RecordHeard("CQ IK4LZH JN54", slot);
        ledger.RecordSent("IK4LZH KC3QIS FN00", slot.AddSeconds(8));
        ledger.RecordHeard("KC3QIS IK4LZH -12", slot.AddSeconds(15));

        return Ft8ContactLogEntry.For(
            ledger.For("IK4LZH")!,
            "KC3QIS",
            new Ft8StationConditions(7_047_500, "40 m", mode, "FN00DJ"));
    }
}
