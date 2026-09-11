using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 309 task 4: **pressing CQ makes a card, and the guard that makes
/// this regression impossible rather than merely fixed.**
/// </summary>
/// <remarks>
/// <para>**FIVE GREEN TESTS COEXISTED WITH NO CARD ON THE SCREEN, AND THIS IS WHY.**
/// `ThePressingOfCqTests` drives `RecordSentForTests` - a seam that books straight
/// into the ledger - and then calls `RebuildCardsForTests()` by hand. **It never
/// presses the button.** So it asserts that the ledger books a CQ and that a card is
/// built from a booked CQ, and says nothing at all about whether pressing CQ books
/// anything.</para>
/// <para>**AND IN THE APPLICATION IT DID NOT.** The one production call to
/// `RecordSent` is inside the slot-boundary path, after a transmission has gone out,
/// so on any evening where the send does not complete there is no booking and no
/// card. A *starter* card is the one that appears when he presses, which is the whole
/// of what he asked for.</para>
/// <para>**THE GUARD IS THE POINT** - *booked implies present*. If the ledger holds a
/// transmission, a card for it is on the panel. A test of the press alone would go
/// green again the next time something else stops a card being built.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2). There is no radio and no sound card; the
/// press refuses at the send path exactly as it does on a bench, and the card is the
/// thing being asserted.</para>
/// </remarks>
public sealed class TheCqPressMakesACardTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public TheCqPressMakesACardTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Pressing the button puts a card on the panel.**</summary>
    /// <remarks>
    /// **THROUGH THE COMMAND, NOT THROUGH A TEST SEAM.** This is the assertion whose
    /// absence let the regression reach him.
    /// </remarks>
    [Fact]
    public void PressingTheButtonPutsACardOnThePanel()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        Print(model);

        var card = Assert.Single(model.DigitalCards);

        Assert.Equal(Ft8ContactLedger.CallToAnyone, card.Callsign);

        // **AND IT SAYS WHAT WENT OUT**, which is what a starter card is for.
        Assert.Contains("CQ", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>**Pressing twice counts up in the one card.**</summary>
    [Fact]
    public void PressingTwiceCountsUpInTheOneCard()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);
        model.SendCallToAnyoneCommand.Execute(null);
        model.SendCallToAnyoneCommand.Execute(null);

        Print(model);

        var card = Assert.Single(model.DigitalCards);

        Assert.Contains("3", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>**Booked implies present: the invariant, over every booking.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE GUARD UNIT 307 WAS TO ADD AND NEVER DID.** It does not
    /// test the press; it tests that **the panel and the ledger cannot disagree**.
    /// Whatever books a transmission - the press, an answer, a boundary - the card
    /// for it exists.</para>
    /// <para>**WATCHED FAILING AGAINST THE TREE AS IT STOOD.** Before this unit the
    /// press booked nothing, so the first case below left the ledger empty and the
    /// panel empty and passed vacuously; what failed is the press case above. With
    /// the press booking, this asserts the pair stay level.</para>
    /// </remarks>
    [Fact]
    public void EveryBookedTransmissionHasACardOnThePanel()
    {
        foreach (var (what, drive) in new (string, Action<MainWindowViewModel>)[]
        {
            ("a CQ press", m => m.SendCallToAnyoneCommand.Execute(null)),
            ("three CQ presses", m =>
            {
                m.SendCallToAnyoneCommand.Execute(null);
                m.SendCallToAnyoneCommand.Execute(null);
                m.SendCallToAnyoneCommand.Execute(null);
            }),
            ("a CQ press then an answer", m =>
            {
                m.SendCallToAnyoneCommand.Execute(null);
                Heard(m, "02:11:15", HisCall + " K9XP EN52");
            }),
        })
        {
            var model = Panel();

            drive(model);

            var booked = model.LedgerForTests?.Stations ?? Array.Empty<string>();
            var onScreen = model.DigitalCards.Select(c => c.Callsign).ToList();

            _output.WriteLine(
                what.PadRight(26)
                + "booked [" + string.Join(", ", booked) + "]"
                + "  cards [" + string.Join(", ", onScreen) + "]");

            // **EVERY BOOKED STATION HAS A CARD.** The invariant, whichever way the
            // booking happened.
            foreach (var station in booked)
            {
                Assert.Contains(station, onScreen, StringComparer.OrdinalIgnoreCase);
            }

            // **AND SOMETHING WAS BOOKED AT ALL**, so this cannot pass by being
            // empty at both ends - which is exactly how it would have passed before
            // this unit.
            Assert.NotEmpty(booked);
        }
    }

    /// <summary>**A press that goes nowhere still leaves the card.**</summary>
    /// <remarks>
    /// **THE CARD IS A RECORD OF WHAT HE ASKED FOR, NOT OF WHAT THE RADIO DID.** On
    /// a machine with no transmit device the send path refuses - and the starter card
    /// is still the honest thing to show, because he did press CQ. What the card says
    /// is what was composed and when; it makes no claim that anything went on the
    /// air.
    /// </remarks>
    [Fact]
    public void APressThatGoesNowhereStillLeavesTheCard()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        _output.WriteLine("send line : " + model.DigitalSendLine);
        _output.WriteLine("cards     : " + model.DigitalCards.Count);

        // **NOTHING WENT OUT** - there is no radio and no sound card here.
        Assert.False(model.HasSomethingToStop);

        // **AND THE CARD IS THERE ANYWAY.**
        Assert.Single(model.DigitalCards);
    }

    private void Print(MainWindowViewModel model)
    {
        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine("CARD  " + card.Callsign);
            _output.WriteLine("  face   : " + card.TimeLine);
            _output.WriteLine("  detail : " + card.Detail);
        }

        _output.WriteLine("");
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        // **A MEASURED CLOCK, WHICH IS THE STATE HIS MACHINE WAS IN** - his own
        // telemetry reads `measured`, offset 0.033 s. **Cards are counted in slots
        // between two moments**, so without an offset `CardsNow` is null and
        // `RebuildCards` builds nothing at all. That is a second, separate way to
        // get no starter card and it is reported rather than changed here.
        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
