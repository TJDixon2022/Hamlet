using System.Globalization;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// After the operator transmits to a station, the card waits on that station
/// (`PHASE_PLAN.md` §R18, work instruction 325 task 3).
/// </summary>
/// <remarks>
/// <para>**THE EVENING THIS COMES FROM.** Tim answered a station at 21:56:15 and
/// twenty-three seconds later the card read *Gone quiet* and dimmed. Nothing was
/// wrong with the station: the clock was counting from **his** last message,
/// which was already ninety seconds old when Tim called him, so the card was
/// reporting a silence that predated the question being asked.</para>
/// <para>**AND THE ROOT UNDERNEATH IT.** Unit 313 found the panel calling
/// `DigitalCards.Clear()` and rebuilding every card from scratch each slot, so
/// there was no per-card memory for *when did I last call him* to live in even
/// if the rule had been right. Cards are keyed by station now and updated in
/// place, which is asserted here as identity across three rebuilds.</para>
/// <para>**EVERY MOMENT HERE IS ON A SLOT BOUNDARY AND EVERY FIGURE IS
/// COMPUTED.** No window is built, nothing is keyed, and no clock is read: the
/// card's idea of *now* is handed in.</para>
/// </remarks>
public sealed class TheCardWaitsOnHimTests
{
    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he calls, standing in for the evening's D2IM.</summary>
    private const string His = "W1ABC";

    /// <summary>A slot boundary to hang the scene on.</summary>
    private static readonly DateTime Called =
        Moment("2026-09-11 21:56:15");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the state word and the dimming are printed.</param>
    public TheCardWaitsOnHimTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// He is called after ninety seconds of silence, and the card waits on him
    /// for the whole slot that follows.
    /// </summary>
    [Fact]
    public void TheCardWaitsForTheWholeSlotAfterTheSend()
    {
        var model = Scene();

        // Twenty-three seconds in - the exact moment his screen said Gone quiet.
        var card = At(model, Called.AddSeconds(23));

        _output.WriteLine("t+23s : " + card.StateWord + "  dim=" + card.IsDim);

        Assert.Equal("Waiting on him", card.StateWord);
        Assert.False(card.IsDim);

        // And still, at the last second of the slot he could answer in.
        card = At(model, Called.AddSeconds(29));

        _output.WriteLine("t+29s : " + card.StateWord + "  dim=" + card.IsDim);

        Assert.Equal("Waiting on him", card.StateWord);
        Assert.False(card.IsDim);
    }

    /// <summary>One full slot with nothing from him: Gone quiet, dimmed.</summary>
    [Fact]
    public void OneFullSlotWithNothingFromHimIsGoneQuiet()
    {
        var model = Scene();

        var card = At(model, Called.AddSeconds(30));

        _output.WriteLine("t+30s : " + card.StateWord + "  dim=" + card.IsDim);

        Assert.Equal("Gone quiet", card.StateWord);
        Assert.True(card.IsDim);
    }

    /// <summary>His reply inside that slot: the card advances and never dims.</summary>
    [Fact]
    public void HisReplyInsideTheSlotAdvancesTheCard()
    {
        var model = Scene();

        // He answers in his own slot, the one after the operator's.
        Heard(model, $"{Mine} {His} R-09", Called.AddSeconds(15));

        foreach (var seconds in new[] { 16, 30, 45 })
        {
            var card = At(model, Called.AddSeconds(seconds));

            _output.WriteLine(
                "t+" + seconds + "s : " + card.StateWord + "  dim=" + card.IsDim);

            Assert.NotEqual("Gone quiet", card.StateWord);
            Assert.False(card.IsDim);
        }
    }

    /// <summary>
    /// A card is the same object across three slot rebuilds, and what the
    /// operator did to it survives with it.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ROOT ASSERTION.** Identity is what unit 313 found missing,
    /// and `MapIsOpen` is the visible consequence: the enlarged map used to shut
    /// itself every slot because the card holding it had been thrown away.
    /// </remarks>
    [Fact]
    public void ACardSurvivesThreeSlotRebuildsUnchangedInIdentity()
    {
        var model = Scene();

        var first = At(model, Called.AddSeconds(5));

        first.MapIsOpen = true;

        for (var slot = 1; slot <= 3; slot++)
        {
            var again = At(model, Called.AddSeconds(5 + (15 * slot)));

            _output.WriteLine(
                "rebuild " + slot + " : same object=" + ReferenceEquals(first, again)
                + "  MapIsOpen=" + again.MapIsOpen);

            Assert.Same(first, again);
            Assert.True(again.MapIsOpen);
        }
    }

    /// <summary>A station that leaves the panel takes its card with it.</summary>
    /// <remarks>
    /// **KEYING BY STATION IS NOT THE SAME AS NEVER REMOVING ANYTHING.** A card
    /// is created when its station first appears and removed when it is no longer
    /// wanted; the reconcile that keeps identity has to be able to do both or the
    /// panel only ever grows.
    /// </remarks>
    [Fact]
    public void ASecondStationGetsItsOwnCardAndKeepsIt()
    {
        var model = Scene();

        Heard(model, $"{Mine} K9RST EN61", Called.AddSeconds(15));

        At(model, Called.AddSeconds(20));

        var mineCard = CardFor(model, His);
        var otherCard = CardFor(model, "K9RST");

        Assert.NotNull(otherCard);
        Assert.NotSame(mineCard, otherCard);

        At(model, Called.AddSeconds(35));

        _output.WriteLine(
            "cards : " + string.Join(", ", model.DigitalCards.Select(c => c.Callsign)));

        Assert.Same(mineCard, CardFor(model, His));
        Assert.Same(otherCard, CardFor(model, "K9RST"));
    }

    /// <summary>
    /// PSK31's stated equivalent of one slot is the time the answer takes on the
    /// air, and it is measured rather than chosen.
    /// </summary>
    /// <remarks>
    /// **§R18 SAYS THE UNIT STATES IT.** PSK31 has no slots, so the length of the
    /// macro Hamlet would send back is the honest stand-in. It is counted off the
    /// varicode bits that would actually be keyed, so it cannot drift from what
    /// the modulator produces.
    /// </remarks>
    [Fact]
    public void Psk31StatesItsOwnEquivalentOfOneSlot()
    {
        var seconds = Hamlet.RadioEngine.Psk31.Psk31Macros.AnswerSeconds(His, Mine);

        var text = Hamlet.RadioEngine.Psk31.Psk31Macros.Answer(His, Mine);

        _output.WriteLine(
            "answer  : \"" + text + "\" (" + text.Length + " characters)");
        _output.WriteLine(
            "on air  : " + seconds.ToString("0.00", CultureInfo.InvariantCulture) + " s");

        // It is the modulator's own arithmetic and not a second copy of it.
        Assert.Equal(
            (Hamlet.RadioEngine.Psk31.Psk31Modulator.BitsFor(text).Length + 1) / 31.25,
            seconds,
            6);

        // And it is a real turnaround rather than an instant one: longer than an
        // FT4 slot and, for these two calls, in the same register as an FT8 one.
        Assert.True(seconds > 7.5, "the stated PSK31 turnaround is " + seconds + " s");
    }

    /// <summary>
    /// A station the operator never called reads exactly as it always did.
    /// </summary>
    /// <remarks>
    /// **THE NEW RULE IS A FLOOR AND NOT A REWRITE.** Gone quiet still means gone
    /// quiet for a station nobody has spoken to.
    /// </remarks>
    [Fact]
    public void AStationNobodyCalledStillGoesQuiet()
    {
        var model = Panel();

        Heard(model, $"{Mine} K9RST EN61", Called);

        model.CardsNowForTests = Called.AddSeconds(90);
        model.RebuildCardsForTests();

        var card = CardFor(model, "K9RST");

        Assert.True(
            card is not null,
            "no card for K9RST. Cards: ["
            + string.Join(", ", model.DigitalCards.Select(c => c.Callsign)) + "]");

        _output.WriteLine("uncalled, 90 s on : " + card!.StateWord);

        Assert.Equal("Gone quiet", card.StateWord);
        Assert.True(card.IsDim);
    }

    /// <summary>
    /// The scene: he was heard ninety seconds ago and the operator has just
    /// called him.
    /// </summary>
    private static MainWindowViewModel Scene()
    {
        var model = Panel();

        // **HE CALLED THE OPERATOR, WHICH IS WHAT MAKES A CARD AT ALL.** A CQ is
        // on the left-hand list and gets no card until it is answered.
        Heard(model, $"{Mine} {His} -12", Called.AddSeconds(-90));

        // **THROUGH THE SAME DOOR THE SEND PATH USES.** `RecordSentForTests` is
        // the hook the transmit tests already book with, so this scene cannot
        // book a send in a shape the application never produces.
        model.RecordSentForTests($"{His} {Mine} -09", Called);

        return model;
    }

    /// <summary>A message from the air, through the real decode path.</summary>
    private static void Heard(MainWindowViewModel model, string message, DateTime slotUtc)
        => model.AddDecodeRowForTests(
            slotUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            "-09", "0.2", "1240", message, slotUtc, heardOnHz: 14_074_000);

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        // Cards are counted in slots between two moments, so without a measured
        // offset `CardsNow` is null and nothing is built at all.
        model.ClockOffset = new ClockOffset(0.033, DateTime.UtcNow);

        return model;
    }

    /// <summary>Rebuild at a moment and hand back the station's card.</summary>
    private static Ft8ContactCard At(MainWindowViewModel model, DateTime nowUtc)
    {
        model.CardsNowForTests = nowUtc;
        model.RebuildCardsForTests();

        var card = CardFor(model, His);

        Assert.True(
            card is not null,
            "no card for " + His + ". Cards: ["
            + string.Join(", ", model.DigitalCards.Select(c => c.Callsign)) + "]");

        return card!;
    }

    private static Ft8ContactCard? CardFor(MainWindowViewModel model, string who)
        => model.DigitalCards.FirstOrDefault(
            c => string.Equals(c.Callsign, who, StringComparison.OrdinalIgnoreCase));

    private static DateTime Moment(string at)
        => DateTime.ParseExact(
            at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
