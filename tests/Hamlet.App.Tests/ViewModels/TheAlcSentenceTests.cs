using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 356 task 2: **the sentence after a send says what it measured, and
/// never sends the operator to the radio.**
/// </summary>
/// <remarks>
/// <para>**THE SENTENCE THIS REPLACES, OFF TIM'S OWN SCREEN ON 2026-09-14**, after an FT8
/// send to HA1BF: *Your radio's own level control read 62 out of 120 while that went out.
/// Hamlet has not yet seen an FT8 or FT4 transmission on this radio to compare it with, so
/// it is not judging it for you: look at the ALC bar on the radio, and if it goes past the
/// marked zone, turn the transmit drive above down one step and send again.* His verdict:
/// *"this message is incorrect."*</para>
/// <para>**TWO FAULTS IN ONE SENTENCE.** It was an FT8 transmission, so under R15 the 62 it
/// had just read **is** the reference, and the sentence was composed before the send it
/// describes had been counted. And it told him to read a meter and turn a knob on the
/// radio, which R11 forbids outright: the operator sets nothing at the radio.</para>
/// <para>**FOUR FORMS, AND WHICH ONE APPEARS IS A MEASUREMENT** (§0.0). Set or raised the
/// reference; inside it; past it by the margin; and no reference at all, where R15 says
/// report and judge nothing.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio: this machine
/// has none and every reading is handed in (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class TheAlcSentenceTests
{
    /// <summary>Words that would send the operator to the radio, which none of these may carry.</summary>
    /// <remarks>
    /// **R11 IN A LIST.** *ALC bar* and *marked zone* name a meter on the front of the
    /// radio; *on the radio* and *at the radio* send him to it. A sentence that needs any
    /// of them has handed the judgement back to the person this path exists to spare.
    /// </remarks>
    private static readonly string[] SendsHimToTheRadio =
    {
        "ALC bar", "marked zone", "on the radio", "at the radio", "turn the knob",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every sentence is printed whole.</param>
    public TheAlcSentenceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The FT8 send that sets the reference names it, and says there is nothing to do.**</summary>
    /// <remarks>
    /// **THIS IS THE FORM THAT WAS IMPOSSIBLE BEFORE.** The sentence was composed before
    /// `LearnTheAlcFrom` ran, so on the send that set the reference it could only ever say
    /// that no reference existed. It is now composed with the flag that send returned.
    /// </remarks>
    [Fact]
    public void TheSendThatSetsTheReferenceNamesIt()
    {
        var model = Panel();

        var became = model.LearnTheAlcForTests(Reading(62), "FT8");

        model.Psk31AlcForTests = Reading(62);
        model.ReadTheAlcForTests(became);

        var said = model.Psk31AlcLine;

        _output.WriteLine("set     : " + said);

        Assert.True(became, "the 62 did not become the reference");
        Assert.Contains("62 of 120", said, StringComparison.Ordinal);
        Assert.Contains("reference from now on", said, StringComparison.Ordinal);
        Assert.Contains("Nothing for you to do.", said, StringComparison.Ordinal);

        // **AND IT NEVER SAYS IT HAS NOT SEEN ONE**, which is the sentence Tim read.
        Assert.DoesNotContain("has not yet seen", said, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("no reference", said, StringComparison.OrdinalIgnoreCase);

        NothingSendsHimToTheRadio(said);
    }

    /// <summary>**A later send inside the reference gets one line and nothing to do.**</summary>
    [Fact]
    public void ALaterSendInsideTheReferenceGetsOneLine()
    {
        var model = Panel();

        model.LearnTheAlcForTests(Reading(62), "FT8");

        // **A SECOND SEND THAT DOES NOT RAISE IT.** 58 is under 62, so nothing is learned
        // and the flag comes back false, which is the ordinary evening.
        var became = model.LearnTheAlcForTests(Reading(58), "FT8");

        model.Psk31AlcForTests = Reading(58);
        model.ReadTheAlcForTests(became);

        var said = model.Psk31AlcLine;

        _output.WriteLine("inside  : " + said);

        Assert.False(became, "58 raised a reference of 62");
        Assert.StartsWith("Level 58 of 120, inside the 62", said, StringComparison.Ordinal);
        Assert.Contains("Nothing for you to do.", said, StringComparison.Ordinal);

        NothingSendsHimToTheRadio(said);
    }

    /// <summary>**A PSK31 send above the reference by the margin points at the drive on this screen.**</summary>
    [Fact]
    public void APsk31SendAboveTheReferencePointsAtTheDriveOnThisScreen()
    {
        var model = Panel();

        model.LearnTheAlcForTests(Reading(62), "FT8");

        // **PAST BY MORE THAN THE MARGIN**, which is 15 (`Psk31AlcMargin`), so 90 is past
        // 62 plus 15 and 70 would not be.
        model.Psk31AlcForTests = Reading(90);
        model.ReadTheAlcForTests();

        var said = model.Psk31AlcLine;

        _output.WriteLine("past    : " + said);

        Assert.Contains("90 of 120", said, StringComparison.Ordinal);
        Assert.Contains("against the 62", said, StringComparison.Ordinal);

        // **THE ONE THING TO DO, AND IT IS ON THE SCREEN** (R11).
        Assert.Contains(
            "Turn the transmit drive on this screen down one step",
            said,
            StringComparison.Ordinal);

        NothingSendsHimToTheRadio(said);
    }

    /// <summary>**With no reference it reports the reading and judges nothing.**</summary>
    /// <remarks>
    /// **R15'S OWN WORDS**: with no reference, report and judge nothing. What it may not do
    /// is hand the judgement back to the operator and a meter, which is what it did.
    /// </remarks>
    [Fact]
    public void WithNoReferenceItReportsAndJudgesNothing()
    {
        var model = Panel();

        Assert.Null(model.Psk31AlcReference);

        model.Psk31AlcForTests = Reading(62);
        model.ReadTheAlcForTests();

        var said = model.Psk31AlcLine;

        _output.WriteLine("no ref  : " + said);

        Assert.Contains("62 of 120", said, StringComparison.Ordinal);
        Assert.Contains("is not judging it", said, StringComparison.Ordinal);
        Assert.Contains("Nothing for you to do.", said, StringComparison.Ordinal);

        NothingSendsHimToTheRadio(said);
    }

    /// <summary>**Not one of the four forms sends him to the radio.**</summary>
    /// <remarks>
    /// **THE SWEEP, RATHER THAN FOUR SEPARATE PROMISES.** Each test above checks its own
    /// sentence; this one builds all four in one model and checks them together, so a fifth
    /// form added later without a test of its own still meets R11 or this goes red.
    /// </remarks>
    [Fact]
    public void NotOneFormSendsHimToTheRadio()
    {
        var model = Panel();
        var forms = new List<string>();

        model.Psk31AlcForTests = Reading(62);
        model.ReadTheAlcForTests();
        forms.Add(model.Psk31AlcLine);

        forms.Add(Say(model, 62, "FT8"));
        forms.Add(Say(model, 58, "FT8"));

        model.Psk31AlcForTests = Reading(90);
        model.ReadTheAlcForTests();
        forms.Add(model.Psk31AlcLine);

        // **AND THE LINE THAT STANDS BESIDE THEM**, which is operator-facing too.
        forms.Add(model.Psk31AlcReferenceLine);

        foreach (var form in forms)
        {
            _output.WriteLine("[" + form + "]");
        }

        Assert.Equal(5, forms.Count);
        Assert.All(forms, NothingSendsHimToTheRadio);

        // **FOUR DIFFERENT SENTENCES**, so the sweep is over four forms and not one
        // repeated: a bug that collapsed them would pass the check above trivially.
        Assert.Equal(4, forms.Take(4).Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>Learn a reading, then say what that send read.</summary>
    private static string Say(MainWindowViewModel model, double level, string mode)
    {
        var became = model.LearnTheAlcForTests(Reading(level), mode);

        model.Psk31AlcForTests = Reading(level);
        model.ReadTheAlcForTests(became);

        return model.Psk31AlcLine;
    }

    private void NothingSendsHimToTheRadio(string said)
    {
        foreach (var phrase in SendsHimToTheRadio)
        {
            Assert.False(
                said.Contains(phrase, StringComparison.OrdinalIgnoreCase),
                "R11: this sentence says '" + phrase + "': " + said);
        }
    }

    private static RigValue Reading(double level, DateTime? atUtc = null)
        => RigValue.Known(
            RigField.Alc,
            level,
            CivAlc.Describe((int)level),
            atUtc ?? DateTime.UtcNow,
            "15 13");

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }
}
