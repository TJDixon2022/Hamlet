using System;
using System.Globalization;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 331 task 5: **the slot that was yours says so.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The message we put up about slots after a transmit is
/// wrong. We transmitted that slot so nothing could be heard."* The readiness line said
/// *one slot decoded, and nothing on the band looked like FT8 at all* about the slot Hamlet
/// was transmitting in.</para>
/// <para>**§0.0, NOT WORDING.** Hamlet suspends decoding while the radio keys
/// (HM-DEC-147), so the slot produced nothing by design; a line that reads that silence as
/// a measurement of the band is a claim nobody took a measurement for, and the operator
/// acting on it goes looking for a fault in his antenna.</para>
/// <para>**THE SENTENCE ALREADY EXISTED ON THE CENSUS LINE UNDER THE TABLE** - *21:56:15
/// UTC was yours - Hamlet was transmitting and did not listen*, seen on 2026-09-11 - and
/// the readiness line above the panels did not know about it. One sentence, one place, two
/// forms.</para>
/// </remarks>
public sealed class TheSlotThatWasYoursTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheSlotThatWasYoursTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: after a send, the line names the slot as the operator's and does not
    /// claim the band was empty.**
    /// </summary>
    [Fact]
    public void AfterASendTheLineNamesTheSlotAsHisAndClaimsNothingAboutTheBand()
    {
        var model = Model();

        var slot = Slot("14:37:30");

        model.RememberTransmittedSlotForTests(slot);

        var line = model.DecodeNoteForTests(
            slotsDecoded: 1, candidatesFound: 0, decodes: 0, slot);

        _output.WriteLine("after a send : " + line);

        Assert.Equal(SlotWasYours.Slot(slot), line);
        Assert.Contains("14:37:30", line, StringComparison.Ordinal);
        Assert.Contains("was yours", line, StringComparison.Ordinal);
        Assert.Contains(SlotWasYours.DidNotListen, line, StringComparison.Ordinal);

        // **AND IT CLAIMS NOTHING ABOUT THE BAND.**
        Assert.DoesNotContain("nothing on the band", line, StringComparison.Ordinal);
        Assert.DoesNotContain("looked like FT8", line, StringComparison.Ordinal);
        Assert.DoesNotContain("looked like a signal", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 2: the decoded-slot count excludes a slot that was not listened to.**
    /// </summary>
    [Fact]
    public void TheDecodedSlotCountExcludesTheSlotThatWasHis()
    {
        var model = Model();

        var his = Slot("14:37:30");
        var theirs = Slot("14:37:45");

        model.RememberTransmittedSlotForTests(his);

        // **TWO SLOTS IN THE READING AND ONE OF THEM WAS HIS**, so the line is about one.
        var line = model.DecodeNoteForTests(
            slotsDecoded: 2, candidatesFound: 0, decodes: 0, his, theirs);

        _output.WriteLine("two slots, one of them his : " + line);

        Assert.Contains("one slot", line, StringComparison.Ordinal);
        Assert.DoesNotContain("2 slots", line, StringComparison.Ordinal);

        // **AND WITH A MESSAGE OUT OF THE ONE HE LISTENED TO, THE COUNT IS STILL ONE.**
        var withText = model.DecodeNoteForTests(
            slotsDecoded: 2, candidatesFound: 3, decodes: 1, his, theirs);

        _output.WriteLine("the same reading with a decode : " + withText);

        Assert.Equal("one message out of one slot", withText);
    }

    /// <summary>
    /// **Assertion 3: a reading with no send of his is untouched.**
    /// </summary>
    /// <remarks>
    /// **THE REGRESSION THIS GUARDS.** The fix is about one case and the ordinary case is
    /// the one the operator sees all evening; a count that started excluding slots nobody
    /// transmitted in would be a worse fault than the one being fixed.
    /// </remarks>
    [Fact]
    public void AReadingWithNoSendOfHisIsUntouched()
    {
        var model = Model();

        var line = model.DecodeNoteForTests(
            slotsDecoded: 1, candidatesFound: 0, decodes: 0, Slot("14:37:30"));

        _output.WriteLine("no send of his : " + line);

        Assert.Equal(
            "one slot decoded, and nothing on the band looked like FT8 at all", line);

        var found = model.DecodeNoteForTests(
            slotsDecoded: 1, candidatesFound: 7, decodes: 0, Slot("14:37:30"));

        _output.WriteLine("signals found, none read : " + found);

        Assert.Contains("7 places", found, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 4: FT4's boundary keeps its tenth, so two slots cannot print one time.**
    /// </summary>
    /// <remarks>
    /// **FT4's SLOTS ARE 7.5 SECONDS APART**, so half of them land on a half second. A
    /// whole-second stamp would name the wrong one and neither could be matched against a
    /// capture - which is the same rule the census line has used since unit 280 and is now
    /// in one place for both.
    /// </remarks>
    [Fact]
    public void TheFt4FormKeepsTheHalfSecondBoundary()
    {
        var model = Model();

        var half = Slot("14:37:37").AddMilliseconds(500);

        model.RememberTransmittedSlotForTests(half);

        var line = model.DecodeNoteForTests(
            slotsDecoded: 1, candidatesFound: 0, decodes: 0, half);

        _output.WriteLine("FT4, on a half-second boundary : " + line);

        Assert.Contains("14:37:37.5", line, StringComparison.Ordinal);
        Assert.Contains(SlotWasYours.DidNotListen, line, StringComparison.Ordinal);

        // **AND THE WHOLE-SECOND FT8 CASE IS UNCHANGED**, with no trailing tenth.
        Assert.Equal(
            "14:37:30 UTC was yours - " + SlotWasYours.DidNotListen,
            SlotWasYours.Slot(Slot("14:37:30")));
    }

    /// <summary>
    /// **Assertion 5: PSK31's unslotted send says how long, because it has no slot.**
    /// </summary>
    [Fact]
    public void ThePsk31FormNamesTheStretchAndNotASlot()
    {
        var said = SlotWasYours.Unslotted(Slot("14:37:30"), 11.0);

        _output.WriteLine("PSK31 : " + said);

        Assert.Equal(
            "Hamlet was transmitting from 14:37:30 for 11 s and did not listen", said);

        // **NO SLOT IS NAMED**, because PSK31 keeps no slot clock and inventing a boundary
        // for it would be a claim about a clock this mode does not have (§R1).
        Assert.DoesNotContain("slot", said, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("was yours", said, StringComparison.Ordinal);

        // **THE SECONDS ARE WHOLE**, because a fractional tail claims a precision the
        // sound card does not deliver.
        Assert.Equal(
            "Hamlet was transmitting from 14:37:30 for 11 s and did not listen",
            SlotWasYours.Unslotted(Slot("14:37:30"), 10.6));
    }

    /// <summary>
    /// **Assertion 6: the PSK31 sentence reaches the panel's own census line.**
    /// </summary>
    [Fact]
    public void ThePsk31SentenceReachesThePanel()
    {
        var model = Model();

        Assert.False(model.HasDigitalCensus);

        model.SayTheSendWasOurs(Slot("14:37:30"), 11.0);

        _output.WriteLine("census line : " + model.DigitalCensusLine);

        Assert.True(model.HasDigitalCensus);
        Assert.Equal(
            SlotWasYours.Unslotted(Slot("14:37:30"), 11.0), model.DigitalCensusLine);
    }

    private static MainWindowViewModel Model()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";

        return new MainWindowViewModel(settings, null);
    }

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-12 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
