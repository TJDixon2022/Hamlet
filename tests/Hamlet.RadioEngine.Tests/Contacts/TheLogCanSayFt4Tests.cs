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

    /// <summary>
    /// The two tags come from one source and cannot be made to disagree.
    /// </summary>
    /// <remarks>
    /// <para>**THE CHEAP AND WRONG SHAPE IS TWO INDEPENDENT STRINGS**, which is
    /// exactly how a record comes to read `MODE=FT8, SUBMODE=FT4`. What stops it
    /// here is that `Ft8StationConditions` carries one <see cref="ContactMode"/>
    /// and `Ft8ContactLogEntry.For` reads both tags off it, so there is no
    /// assignment anywhere that could set one without the other. **The proof is
    /// this assertion and not the paragraph**: every one of the six goes through
    /// the write path, and each comes out spelled the way `ContactModes` spells
    /// it or not at all.</para>
    /// <para>**AND VOICE COMES OUT WITH NO MODE, WHICH IS THE RIGHT ANSWER.**
    /// *Voice* is Hamlet's own word for `SSB`, `AM` and `FM`; writing the first of
    /// the three would name a mode the operator may not have worked, and there is
    /// nothing later that could tell such a record from a true one (§0.0).</para>
    /// </remarks>
    [Fact]
    public void EverySpellingComesFromTheOneTableOrNotAtAll()
    {
        foreach (var mode in ContactModes.Six)
        {
            var contact = Contact(mode);

            _output.WriteLine(
                mode.Name.PadRight(6) + " spells " + mode.AdifSpelling.PadRight(28)
                + " -> MODE=" + (contact.Mode ?? "(absent)")
                + ", SUBMODE=" + (contact.Submode ?? "(absent)"));

            if (mode.AdifModes.Count > 1)
            {
                // A family, and a record cannot say which. Both halves out.
                Assert.Null(contact.Mode);
                Assert.Null(contact.Submode);

                continue;
            }

            Assert.Equal(mode.AdifModes[0], contact.Mode);
            Assert.Equal(mode.AdifSubmode, contact.Submode);

            // **NEITHER HALF EVER STANDS ALONE.** A submode with no mode beside
            // it names nothing at all, and a mode that needs one and lacks it is
            // a record no row may light from.
            Assert.False(contact.Submode is not null && contact.Mode is null);
        }

        // And with no conditions at all, neither tag is invented.
        var bare = Contact(null);

        Assert.Null(bare.Mode);
        Assert.Null(bare.Submode);
    }

    /// <summary>An FT8 contact still logs exactly as it does today.</summary>
    /// <remarks>
    /// **THE FAILURE MODE OF A SHARED SOURCE IS THAT FT8 QUIETLY GROWS A
    /// SUBMODE.** `FT8` is a Mode and takes no submode (`AdifLog.cs:104`), so the
    /// record must read `MODE=FT8` with the string `SUBMODE` nowhere in it, exactly
    /// as it did before this unit.
    /// </remarks>
    [Fact]
    public void AnFt8ContactStillLogsAsItAlwaysDid()
    {
        var contact = Ft8Contact();

        Assert.Equal("FT8", contact.Mode);
        Assert.Null(contact.Submode);

        var text = AdifLog.Record(contact);

        _output.WriteLine(text);

        Assert.Contains("<MODE:3>FT8", text, System.StringComparison.Ordinal);
        Assert.DoesNotContain("SUBMODE", text, System.StringComparison.Ordinal);
        Assert.DoesNotContain("MFSK", text, System.StringComparison.Ordinal);
    }

    /// <summary>An FT4 contact as the one write path would produce it.</summary>
    /// <remarks>
    /// **NOTHING IS SET ON THE CONTACT AFTERWARDS** (work instruction 291 task 3).
    /// The mode goes in through the conditions as one object and both tags come
    /// out of it. At task 2 this method had to reach in and set `Submode` by hand,
    /// because the conditions still took a bare string; that it no longer does is
    /// the assertion.
    /// </remarks>
    private static AdifContact Ft4Contact() => Contact(ContactModes.Named("FT4"));

    /// <summary>The same, on FT8, which takes no submode.</summary>
    private static AdifContact Ft8Contact() => Contact(ContactModes.Named("FT8"));

    /// <summary>One contact off the ledger, in the mode handed in.</summary>
    private static AdifContact Contact(ContactMode? mode)
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
