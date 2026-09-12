using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;

namespace Hamlet.App.ViewModels;

/// <summary>What the one button on a card does.</summary>
public enum Ft8CardActionKind
{
    /// <summary>Nothing is offered, because nothing obvious comes next.</summary>
    None,

    /// <summary>It transmits one message, once, when pressed.</summary>
    Send,

    /// <summary>It opens the Log dialog. It transmits nothing.</summary>
    Log,
}

/// <summary>
/// **The numbers a card keeps off its face, as the decode measured them.**
/// </summary>
/// <remarks>
/// **STRINGS BECAUSE THEY ARE ALREADY FORMATTED** where the decode row formatted
/// them, and reformatting them here would be a second rounding of the same
/// measurement (§0). An empty one is a real answer and its clause is simply absent.
/// </remarks>
/// <param name="AudioHz">Where his tone sat in the passband, in hertz.</param>
/// <param name="Dt">How far into the slot his transmission began, in seconds.</param>
/// <param name="DialHz">Where the radio was tuned when he was heard, or 0.</param>
public sealed record Ft8CardTechnical(string AudioHz, string Dt, long DialHz);

/// <summary>
/// **One station he is in contact with, said in words that need no radio
/// knowledge.**
/// </summary>
/// <remarks>
/// <para>**THE FACE NEEDS NO RADIO KNOWLEDGE** (Tim's ruling, 2026-09-08). His
/// reason is the design brief: *"the details are nerdy and geeky, and I want to hide
/// them behind an intentional decision to look at them."* So there is no `dB`, no
/// hertz, no time offset, no bearing and no `RR73` on any member this type puts on
/// the face. Every one of those is on the `i` hover instead, and none of them is
/// deleted.</para>
/// <para>**EVERY SENTENCE COMES FROM <see cref="Ft8CardFacts"/> AND FROM NOTHING
/// ELSE** (§0.0). This type composes words; it derives no facts, reads no ledger and
/// asks no question of its own. Where a fact is absent the clause is absent with it
/// rather than being filled with a plausible one - a card with no grid says nothing
/// about distance, and a card with no measured clock offset shows the slot's UTC and
/// no relative age.</para>
/// <para>**IT SAYS *HE SIGNED OFF* ONLY WHERE HE DID** (work instruction 297 task
/// 1). <see cref="Ft8CardFacts.HeSignedOff"/> is a test on the payload he actually
/// sent, never a reading of <see cref="Ft8ContactState.Complete"/> - which counts
/// `RRR` as an acknowledgement and would have this card announcing a farewell that
/// never happened.</para>
/// <para>**THE FOUR STATES SEPARATE WITHOUT COLOUR** (§0.6). Each carries its own
/// word - *Finished*, *Your turn*, *Waiting on him*, *Gone quiet* - and the dimming
/// of a quiet card is a second carrier beside the word rather than the only one.
/// Printed in grey the four are still four.</para>
/// <para>**IT TRANSMITS NOTHING.** <see cref="ActionMessage"/> is a string. The
/// button that carries it is bound to the shell's one send entry point, which arms
/// exactly one transmission for the next slot on the click and on nothing else
/// (§0.2).</para>
/// </remarks>
public sealed partial class Ft8ContactCard : ObservableObject
{
    /// <summary>Under this, the card says *just now* rather than counting.</summary>
    /// <remarks>
    /// **A SLOT IS SECONDS LONG AND THE COUNT ARRIVES MID-SLOT.** *3 seconds ago*
    /// about a message decoded from the slot that has only just closed reads as
    /// precision the boundary does not carry.
    /// </remarks>
    private const int JustNowSeconds = 5;

    // **THESE STOPPED BEING READONLY IN UNIT 325 AND THE REASON IS THE ROOT OF
    // THREE SEPARATE FAULTS** (§R18, work instruction 325 task 3). Unit 313 found
    // that the panel threw every card away and built new ones each slot, which is
    // why nothing a card knew about itself - the map being open, the warning
    // before clearing, and the *when did I last call him* the waiting state needs
    // - could survive a slot. A card is now one object per station for as long as
    // the station is on the panel, and the ledger is applied to it in place.
    private Ft8CardFacts _facts;
    private string _place;
    private string _country;
    private string? _operatorGrid;
    private Ft8CardTechnical? _technical;
    private int? _floorDb;
    private readonly Psk31TurnReading? _turn;

    private DateTime? _nowUtc;

    /// <summary>**Builds a PSK31 conversation card: one station certainly calling the operator, and whose turn it is.**</summary>
    /// <param name="callsign">The station, as the parse read it.</param>
    /// <param name="turn">Whose turn it is on his channel (`Psk31Turn`).</param>
    /// <param name="operatorGrid">The operator's own locator, or null.</param>
    /// <param name="offered">Which macro the card offers for one click (`Psk31Offer`).</param>
    /// <param name="grid">His grid, from a message he certainly sent, or null.</param>
    /// <param name="offeredText">
    /// That macro's text, exactly as it would go on the air, or "" where there is none. **The
    /// card is handed it and never composes it** (§0.1): `Psk31Macros` is the engine's and it
    /// is never told Settings exist.
    /// </param>
    /// <param name="complete">
    /// True where both sides have closed the exchange - Hamlet has confirmed and he has
    /// certainly said goodbye. It is what puts the Log on his card, and the Log lives only
    /// here (§R8).
    /// </param>
    /// <returns>The card.</returns>
    /// <remarks>
    /// <para>**THE SAME CARD TYPE, NOT A THIRD** (work instruction 319 task 3, §2). Where FT8's
    /// card shows the ledger's state word, this one shows whose turn it is.</para>
    /// <para>**WHERE PSK31 HAS NO FACT, THE CARD SHOWS NOTHING RATHER THAN A STAND-IN.** There is no
    /// FT8 ledger behind it, so there is no time line, no slot count, no report, no message count and
    /// no `i` detail. The facts record below carries the callsign and his grid; its other members are
    /// empty and nothing on a PSK31 card reads them.</para>
    /// <para>**HIS GRID IS HANDED ONLY FROM A CERTAIN MESSAGE** (work instruction 320, item 41). Without
    /// it the map row said he had not put a grid square on the air after his certain report carried
    /// one, which is a false sentence (HM-DEC-092). A grid read from a guess is not handed (§R1).</para>
    /// <para>**NOTHING ON IT TRANSMITS AND NOTHING ON IT LOGS** (§0.2, §6 of the instruction): no
    /// action, no Log link. The send door stays shut, and the Log is step 5's.</para>
    /// </remarks>
    public static Ft8ContactCard ForPsk31(
        string callsign, Psk31TurnReading turn, string? operatorGrid, Psk31Macro offered = Psk31Macro.None,
        string? grid = null, string offeredText = "", bool complete = false)
    {
        ArgumentNullException.ThrowIfNull(turn);

        var facts = new Ft8CardFacts(
            callsign, complete ? Ft8ContactState.Complete : Ft8ContactState.WaitingOnHim,
            0, null, null, false, false, 0, 0, 0,
            null, null, false, false, false, false, grid, null, null);

        return new Ft8ContactCard(facts, operatorGrid, turn, offered, offeredText ?? "", complete);
    }

    private Ft8ContactCard(
        Ft8CardFacts facts, string? operatorGrid, Psk31TurnReading turn, Psk31Macro offered,
        string offeredText, bool complete)
        : this(
            facts,
            operatorGrid,
            offered == Psk31Macro.None || offeredText.Length == 0
                ? Ft8CardActionKind.None
                : Ft8CardActionKind.Send,
            offeredText.Length == 0 ? "" : Psk31ActionLabel(offered),
            offeredText,
            null)
    {
        _turn = turn;
        _offered = offered;
        _psk31Complete = complete;
    }

    private readonly Psk31Macro _offered;

    /// <summary>True where a PSK31 exchange has been closed by both sides.</summary>
    private readonly bool _psk31Complete;

    /// <summary>What the one button on a PSK31 card reads.</summary>
    /// <param name="offered">Which macro the engine named.</param>
    /// <returns>Plain words, with no radio jargon in them.</returns>
    /// <remarks>
    /// **THE ACT, NOT THE FIELD SHAPE**, the same rule `MainWindowViewModel.PlainSendLabel`
    /// follows on the FT8 side: the message itself is on the hover, where a deliberate look
    /// finds it.
    /// **The words are a session's and not a ruling.**
    /// </remarks>
    private static string Psk31ActionLabel(Psk31Macro offered) => offered switch
    {
        Psk31Macro.Answer => "Answer him",
        Psk31Macro.Report => "Tell him how he is coming through",
        Psk31Macro.Confirm => "Confirm, and sign off",
        _ => "",
    };

    /// <summary>True where this is a PSK31 conversation card.</summary>
    public bool IsPsk31 => _turn is not null;

    /// <summary>Which macro this card would offer for one click, or <see cref="Psk31Macro.None"/>.</summary>
    public Psk31Macro Offered => _offered;

    /// <summary>**The offered macro by name, or "".**</summary>
    /// <remarks>
    /// <para>**THE ENGINE'S ANSWER** (`Psk31Offer`, §R1's strict side): named only on a certain *your
    /// turn*.</para>
    /// <para>**NOT DRAWN WHILE THE DOOR IS SHUT** (work instruction 319, the arbiter's decision).
    /// Nothing in `MainWindow.axaml` binds it: a button that always refuses would assert a capability
    /// Hamlet does not have, and a picture binds as hard as a sentence (HM-DEC-092).</para>
    /// </remarks>
    public string OfferedMacro => _offered == Psk31Macro.None ? "" : _offered.ToString();

    /// <summary>Whose turn it is, on a PSK31 card, or null on an FT8 card.</summary>
    public Psk31TurnReading? Turn => _turn;

    /// <summary>**Whose turn it is, in words, where FT8's card has its state word.**</summary>
    /// <remarks>
    /// <para>**A GUESS SAYS SO IN WORDS** (§R1, §0.6): *a guess* is part of the word, so printed in
    /// grey it still reads as one. *Unknown* where Hamlet cannot tell.</para>
    /// <para>**THE WORDING IS A SESSION'S** (work instruction 319), not a ruling, beside unit 316's
    /// *guess* and *unknown* on the row.</para>
    /// </remarks>
    public string TurnWord => _turn is not { } turn
        ? ""
        : turn.State switch
        {
            Psk31TurnState.YourTurn => turn.IsCertain ? "Your turn" : "Your turn, a guess",
            Psk31TurnState.HisTurn => turn.IsCertain ? "His turn" : "His turn, a guess",
            Psk31TurnState.HeIsSending => "He is still sending",
            _ => "Unknown",
        };

    /// <summary>True where the turn word is a guess or an unknown.</summary>
    public bool TurnIsGuess => _turn is { IsCertain: false };

    /// <summary>What a PSK31 card says: whose turn, and why nothing is offered where nothing is.</summary>
    /// <remarks>
    /// **A BUTTON THAT IS NOT THERE HAS TO SAY WHY** (work instruction 323 task 3, §0.0).
    /// Hamlet offers a macro only where the parser is certain it is the operator's turn
    /// (§R1); on anything less the card would otherwise simply have no button, and *Hamlet
    /// is not sure* and *there is nothing to send* would look identical. **It is one added
    /// clause and not a second sentence**, so the card does not grow a paragraph.
    /// </remarks>
    private string TurnSentence()
        => _psk31Complete
            ? $"The exchange with {Callsign} is finished: you confirmed and he said goodbye."
            : TurnState() + (WaitingToBeSure ? " Waiting to be sure it is your turn before "
                + "offering anything to send." : "");

    /// <summary>True where nothing is offered because Hamlet is not certain enough to offer it.</summary>
    private bool WaitingToBeSure
        => _turn is not null && _offered == Psk31Macro.None
            && _turn.State != Psk31TurnState.HisTurn;

    /// <summary>What a PSK31 card says, one sentence per turn state.</summary>
    private string TurnState() => _turn!.State switch
    {
        Psk31TurnState.YourTurn when _turn.IsCertain => $"{Callsign} handed over to you, and it is your turn.",
        Psk31TurnState.YourTurn => $"{Callsign} seems to have handed over to you, but part of it did not read cleanly, so that is a guess.",
        Psk31TurnState.HisTurn when _turn.IsCertain => $"You handed over to {Callsign}, and it is his turn.",
        Psk31TurnState.HisTurn => $"It looks like his turn, but part of it did not read cleanly, so that is a guess.",
        Psk31TurnState.HeIsSending => $"Text is still arriving on the frequency {Callsign} called you on.",
        _ => $"Hamlet cannot tell whose turn it is with {Callsign}.",
    };

    /// <summary>Builds the card for one station.</summary>
    /// <param name="facts">What the ledger says about that station.</param>
    /// <param name="operatorGrid">The operator's own locator, or null.</param>
    /// <param name="action">Which of the three the button is.</param>
    /// <param name="actionLabel">What the button reads, in plain words.</param>
    /// <param name="actionMessage">
    /// For <see cref="Ft8CardActionKind.Send"/>, the message exactly as it would go
    /// on the air. Empty otherwise.
    /// </param>
    /// <param name="nowUtc">
    /// Corrected UTC, or null where no clock offset has been measured. **Null is a
    /// real answer** and produces a time line with no relative half (§0.0).
    /// </param>
    /// <param name="technical">
    /// The numbers the face keeps off itself, for the `i` hover, or null where the
    /// station has no decoded row to read them from.
    /// </param>
    /// <param name="decodeFloorDb">
    /// About how far below the noise this mode decodes, where the tree has a cited
    /// figure for it, and null otherwise. **Null is a real answer** (§12.4): the
    /// hover then teaches the sign and the direction and invents no number.
    /// </param>
    /// <exception cref="ArgumentNullException">There are no facts.</exception>
    /// <param name="who">Who the operator is and what town, where callook knows.</param>
    public Ft8ContactCard(
        Ft8CardFacts facts,
        string? operatorGrid,
        Ft8CardActionKind action,
        string actionLabel,
        string actionMessage,
        DateTime? nowUtc,
        Ft8CardTechnical? technical = null,
        int? decodeFloorDb = null,
        StationName? who = null)
    {
        ArgumentNullException.ThrowIfNull(facts);

        _facts = facts;
        _nowUtc = nowUtc;
        _place = WhereHeIs(facts, operatorGrid, who);
        _country = CountryOf(facts);
        _operatorGrid = operatorGrid;
        _technical = technical;
        _floorDb = decodeFloorDb;

        ActionKind = action;
        ActionLabel = actionLabel;
        ActionMessage = actionMessage;
    }

    /// <summary>The station. The one piece of jargon the operator already knows.</summary>
    public string Callsign => _facts.Callsign;

    /// <summary>Where in the world he is, and how far. Empty where unknown.</summary>
    /// <remarks>
    /// <para>**A COUNTRY IS THE FINEST PLACE HAMLET CAN NAME** (work instruction 297
    /// task 1). <see cref="DxccPrefixes.EntityOf"/> answers with a DXCC entity and
    /// <see cref="EntityQualifier"/> adds only *northern* or *southern* where the
    /// entity is tall enough for the word to mean something. **There is no state, no
    /// province and no town anywhere in this tree**, so a card cannot say *Arizona*
    /// and does not try.</para>
    /// <para>**THE COUNTRY COMES FROM THE CALLSIGN AND THE DISTANCE FROM THE GRID**,
    /// which is `Ft8Vocabulary`'s rule and is kept for its reason: a grid square is
    /// about 70 by 100 miles and straddles borders, so `W4/YV7AXM` is in the United
    /// States whatever his grid says. Where the prefix table declines, no country is
    /// named at all - certain, or silent.</para>
    /// <para>**NO BEARING** (Tim's ruling, 2026-09-08: *"what am I, some sort of
    /// submarine captain?"*). It is on the hover.</para>
    /// </remarks>
    /// <summary>Where the station is, in words, or "" on a receipt.</summary>
    /// <remarks>
    /// **A CALL TO ANYBODY HAS NO PLACE** (R1, R2, Tim 2026-09-11). This slot held
    /// `Portugal` on his screen, because `CQ` is a real Portuguese prefix and the
    /// card was the conversation frame with the other station set to the literal
    /// string. **The lookup refuses it now** and this refuses it again, because a
    /// receipt should not be asking the question at all.
    /// </remarks>
    public string Place => IsCallToAnyone ? "" : _place;

    /// <summary>True where there is anything to say about where he is.</summary>
    public bool HasPlace => _place.Length > 0;

    /// <summary>The state, in one word a newcomer can read.</summary>
    /// <remarks>
    /// **A WORD AND NOT A SLOT COUNT** (Tim's ruling, 2026-09-08). *Gone quiet,
    /// 11 slots* is what the contact line says and it is a measurement; on a card the
    /// count is arithmetic the reader should not have to do, and the relative time
    /// beside it already says how stale this is. The count is on the hover.
    /// </remarks>
    public string StateWord => IsPsk31
        ? _psk31Complete ? "Finished" : TurnWord
        : IsCallToAnyone
        ? ReceiptWord
        : _facts.State switch
    {
        Ft8ContactState.Complete => "Finished",
        Ft8ContactState.YourMove => "Your turn",
        Ft8ContactState.WaitingOnHim => "Waiting on him",
        Ft8ContactState.GoneQuiet => "Gone quiet",
        _ => "",
    };

    /// <summary>What happened, in a sentence that needs no radio knowledge.</summary>
    /// <remarks>
    /// <para>**EVERY CLAUSE TRACES TO A FACT** and task 5 of work instruction 297
    /// quotes the trace. Nothing here is inferred from the state alone.</para>
    /// <para>**THE COMPLETE SENTENCE DOES NOT MENTION A FAREWELL UNLESS THERE WAS
    /// ONE.** *Finished* means both sides swapped what a contact needs, which an
    /// exchange ending `RRR` does; the goodbye is a separate clause on a separate
    /// fact.</para>
    /// <para>**AND *YOUR TURN* HAS TWO CAUSES.** He came back to the operator, or he
    /// called anyone and nobody has answered. `Ft8ContactStates.Read` puts both in
    /// the same state and says so in its own remarks, and they are different news:
    /// one is a conversation waiting on him, the other is an invitation nobody has
    /// taken.</para>
    /// </remarks>
    public string Sentence => IsPsk31
        ? TurnSentence()
        : IsCallToAnyone
        ? ReceiptSentence
        : _facts.State switch
    {
        Ft8ContactState.Complete => Finished(),
        Ft8ContactState.YourMove => _facts.HeCameBack
            ? $"{Callsign} came back to you, and it is your turn to answer him."
            : $"{Callsign} is calling and nobody has answered him yet.",
        Ft8ContactState.WaitingOnHim => _facts.HeCameBack
            ? $"You answered {Callsign} and he has not come back yet."
            : $"You called {Callsign} and he has not answered yet.",
        Ft8ContactState.GoneQuiet => Quiet(),
        _ => "",
    };

    /// <summary>The moment the time line is about, or null.</summary>
    public DateTime? LastAtUtc => _facts.LastAtUtc;

    /// <summary>
    /// **When it happened, both ways: the wall time and how long ago.**
    /// </summary>
    /// <remarks>
    /// <para>**BOTH, BY TIM'S RULING OF 2026-09-08.** The UTC ties the card to the
    /// band and to the log; the relative half says how stale the card is, which is
    /// the thing he cannot work out at a glance from a clock reading.</para>
    /// <para>**THE RELATIVE HALF NEEDS A MEASURED CLOCK AND SAYS NOTHING WITHOUT
    /// ONE** (§0.0). Without an offset there is no corrected now, so counting from
    /// the boundary would be counting against a reading nobody took. The UTC half
    /// still shows: it came off the decode and is a fact either way.</para>
    /// </remarks>
    public string TimeLine
    {
        get
        {
            if (_facts.LastAtUtc is not { } at)
            {
                return "";
            }

            var clock = at.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";

            return _nowUtc is { } now ? clock + " · " + Ago(at, now) : clock;
        }
    }

    /// <summary>Which of the three the button is.</summary>
    public Ft8CardActionKind ActionKind { get; private set; }

    /// <summary>What the button reads.</summary>
    public string ActionLabel { get; private set; }

    /// <summary>The message a Send button would transmit, exactly as it goes out.</summary>
    public string ActionMessage { get; private set; }

    /// <summary>True where there is a button at all.</summary>
    public bool HasAction => ActionKind != Ft8CardActionKind.None;

    /// <summary>What the button's hover says, including the message itself.</summary>
    /// <remarks>
    /// <para>**THE EXACT TEXT THAT WOULD GO ON THE AIR IS HERE AND NOT ON THE
    /// FACE.** The face carries the act in plain words; a reader who wants to know
    /// what Hamlet is about to transmit in his name can have it for one hover, which
    /// is the whole arrangement Tim asked for. **Nothing is hidden and nothing is
    /// deleted.**</para>
    /// <para>**IT SAYS WHEN IT WOULD GO, BECAUSE THAT IS THE PART THAT SURPRISES.**
    /// A press does not transmit now; it arms the next slot, and the ring beside the
    /// button is counting down to it.</para>
    /// </remarks>
    public string ActionTip => ActionKind switch
    {
        // **PSK31 HAS NO SLOT AND NO DEADLINE** (§R10). The FT8 wording below counts the
        // seconds to a boundary; here the press *is* the moment, so saying otherwise would
        // invent an urgency that does not exist.
        Ft8CardActionKind.Send when IsPsk31 =>
            $"Sends “{ActionMessage}” to {Callsign}, once, straight away. "
            + "One press is one transmission and nothing here sends on a timer.",
        Ft8CardActionKind.Send =>
            $"Sends “{ActionMessage}” to {Callsign}, once, in the next "
            + "slot. The ring beside this button is the seconds left to press it. "
            + "One press is one transmission and nothing here sends on a timer.",
        // **A RECEIPT IS NOT AN EXCHANGE WITH ANYBODY** (R1, R2). The wording below
        // names the other station, and on a call to anybody there is not one - it read
        // *what passed between you and CQ*, which is the same fault as the country
        // line, one hover further down.
        Ft8CardActionKind.Log when IsCallToAnyone =>
            "Opens the log window with your own call already filled in, so you can "
            + "look at it before anything is written down. It transmits nothing.",
        Ft8CardActionKind.Log =>
            $"Opens the log window with what passed between you and {Callsign} "
            + "already filled in, so you can look at it before anything is written "
            + "down. It transmits nothing.",
        _ => "",
    };

    /// <summary>True where the countdown ring belongs beside this card's button.</summary>
    /// <remarks>
    /// **THE RING SITS BESIDE THE BUTTON THAT NEEDS IT** (Tim's ruling, 2026-09-08),
    /// so the seconds read as *how long to press this* rather than as a clock the
    /// operator has to relate to something. **Only a Send button needs it**: logging
    /// has no slot to catch and no deadline, and a countdown beside it would invent
    /// an urgency that does not exist.
    /// **IT COUNTS DOWN AND DOES NOTHING ELSE** (§0.2). Nothing reads it, nothing
    /// arms on it, and its last second transmits nothing.
    /// </remarks>
    /// <remarks>**AND NEVER ON A PSK31 CARD** (§R10): there is no slot to catch.</remarks>
    public bool ShowsRing => !IsPsk31 && ActionKind == Ft8CardActionKind.Send;


    /// <summary>True where the card shows Log as a second control beside the action.</summary>
    /// <remarks>
    /// <para>**TIM RULED ON 2026-09-09 THAT THE LOG OPTION IS ALWAYS AVAILABLE**:
    /// *"the log option is always available... I just may be interested in everyone
    /// who responded to me, even if they do not respond back to me responding to
    /// them. It should be up to me what I want to log."* **Hamlet does not decide
    /// what counts as a contact.** A card that withheld it was the application
    /// asserting a standard he did not set.</para>
    /// <para>**IT IS FALSE ON A FINISHED CARD ONLY BECAUSE LOG IS ALREADY THE
    /// ACTION THERE**, so the option is present on every card in every state and is
    /// never drawn twice on one.</para>
    /// </remarks>
    /// <para>**AND IT IS FALSE ON A RECEIPT, WHICH IS A DIFFERENT REASON** (Tim,
    /// 2026-09-11: *"CQ should not have log option, that is self-gratification."*).
    /// The button came off the receipt in work instruction 314 and **this link
    /// appeared in its place**, because the rule above reads *Log is not already the
    /// action here* and that became true the moment the action became None. A CQ is
    /// not a contact by either route.</para>
    /// <para>**ON A PSK31 CARD IT APPEARS WHEN THE EXCHANGE IS FINISHED** (work instruction 323
    /// task 3): Hamlet has sent the confirmation and he has certainly said goodbye. **Before
    /// that it is not there**, because step 5 is what teaches the log what a PSK31 contact is,
    /// and the one place a PSK31 Log ever appears is his card - never the receipt (§R8).</para>
    public bool ShowsLogLink
        => IsPsk31
            ? _psk31Complete
            : !IsCallToAnyone && ActionKind != Ft8CardActionKind.Log;

    /// <summary>What the always-available Log control reads.</summary>
    public string LogLabel => "Log this contact";

    /// <summary>What its hover says.</summary>
    /// <remarks>
    /// **IT SAYS THE RECORD WILL BE HONEST**, because the thing he is being offered
    /// on an unfinished contact is a partial record, and a partial record that
    /// looked complete would be worse than none (§0.0). The dialog shows him every
    /// field before anything is written.
    /// </remarks>
    public string LogTip
        => $"Opens the log window with whatever passed between you and {Callsign}, "
           + "however far it got. Anything Hamlet did not observe is shown as not "
           + "recorded and is left out of the record entirely, so a half exchange "
           + "never looks like a whole one. It transmits nothing, and nothing is "
           + "written until you press Save.";


    /// <summary>**Where the two stations are, for the globe beside the `i`.**</summary>
    /// <remarks>
    /// <para>**TIM ASKED FOR IT ON 2026-09-08, A DESIGN WAS APPROVED, AND THE AUTHOR
    /// QUEUED IT INSTEAD OF WRITING IT** (work instruction 299). It is here now.
    /// </para>
    /// <para>**THE PLACE IS THE FACE'S OWN**, so the caption under the map and the
    /// line on the card cannot come to disagree about where he is (§0).</para>
    /// <para>**NOTHING IS LOOKED UP AND NOTHING IS FETCHED.** Both positions are the
    /// two grid squares put through the coastline's own projection, which is
    /// arithmetic on values the messages themselves carried.</para>
    /// </remarks>
    public Ft8GlobePlot Globe => _globe ??= new Ft8GlobePlot(
        _operatorGrid, _facts.Grid, Callsign, _country);

    private Ft8GlobePlot? _globe;

    /// <summary>True where the card has a map worth drawing.</summary>
    /// <remarks>
    /// **A RECEIPT HAS NO MAP** (R2). There is no other station, so there is nothing
    /// to place and nothing for a caption to be about - and the caption it did carry
    /// explained the whereabouts of something that does not exist.
    /// </remarks>
    public bool ShowsGlobe => !IsCallToAnyone && Globe.HasMap;

    /// <summary>True where there is a map to press and something to zoom to.</summary>
    /// <remarks>
    /// <para>**R8** (Tim, 2026-09-11): *"When I click on a map it gets enlarged."*
    /// **Conversation cards only.** A receipt has no other station, so it has no map;
    /// and a card whose station the picture cannot place has a map with his own marker
    /// on it and no path, so there is nothing to zoom to.</para>
    /// <para>**IT GOVERNS THE BUTTON AND THE COMMAND BOTH** (0.5.1, HM-DEC-087). A
    /// control that is not drawn and a command that would have worked if it were are
    /// different things, and only the second survives somebody reaching the card by
    /// keyboard.</para>
    /// </remarks>
    public bool MapOpens => ShowsGlobe && Globe.Opens;

    /// <summary>True while the enlarged map is up.</summary>
    /// <remarks>
    /// **IT LIVES ON THE CARD AND NOT ON THE WINDOW** (R6). The panel holds as many
    /// conversations as he likes, each with its own map, so one shared *the map is
    /// open* would open the wrong one the moment there are two.
    /// </remarks>
    [ObservableProperty]
    private bool _mapIsOpen;

    /// <summary>Open the enlarged map.</summary>
    /// <remarks>
    /// **IT TRANSMITS NOTHING** (0.2). It is a picture the operator is looking at more
    /// closely, and nothing on this path touches the radio.
    /// </remarks>
    [RelayCommand]
    private void OpenTheMap()
    {
        if (!MapOpens)
        {
            return;
        }

        MapIsOpen = true;
    }

    /// <summary>Put the enlarged map away.</summary>
    /// <remarks>
    /// **THE DISMISS X** (R8). Nothing is lost by closing it: the small map is still
    /// on the card and the caption under it still says where he is.
    /// </remarks>
    [RelayCommand]
    private void CloseTheMap() => MapIsOpen = false;

    /// <summary>True where the card is drawn back, so the live ones lead.</summary>
    /// <remarks>
    /// **DIMMED AND NEVER REMOVED** (Tim's ruling, 2026-09-08). A card never
    /// disappears on its own; three cards competing equally is the fault this
    /// answers, and a station that has stopped transmitting is the one of the three
    /// least likely to be worth a transmission. **It is a second carrier and not the
    /// only one** (§0.6): the word *Gone quiet* says the same thing.
    /// </remarks>
    public bool IsDim => _facts.State == Ft8ContactState.GoneQuiet;

    /// <summary>How faded a dimmed card is drawn.</summary>
    public double CardOpacity => IsDim ? 0.62 : 1.0;

    /// <summary>The way down to the raw messages.</summary>
    /// <remarks>
    /// **NOTHING IS HIDDEN** (Tim's ruling, 2026-09-08). The messages are one click
    /// down and never gone, and the count is on the button so he knows what he is
    /// opening. **It counts this conversation and not everything heard from him**:
    /// a station working three others has transmissions in the ledger that are not
    /// part of this exchange, and offering to show six when four are somebody else's
    /// is a count the panel cannot honour.
    /// </remarks>
    /// <remarks>
    /// **NOTHING BINDS THIS SINCE WORK INSTRUCTION 330 TASK 2.** The card's top row carried
    /// it as a button while the right column carried the count and its own link, so one
    /// card had two ways down to one conversation and the second of them was the line Tim
    /// photographed cut off at the card's edge. The top row's button is gone; the table's
    /// `Messages` row says `2 · show them`, which is <see cref="MessagesValue"/> and
    /// <see cref="MessagesLinkLabel"/>. **This is kept and not deleted** because it is the
    /// wording of the count and `Unit297CardSentenceTests` holds it against §R19 and the
    /// card's voice; a second surface that wants the whole sentence should read it here
    /// rather than compose it again.
    /// </remarks>
    public string MessagesLabel
        => _facts.Messages == 1
            ? "show the 1 message"
            : "show the " + _facts.Messages.ToString(CultureInfo.InvariantCulture)
              + " messages";

    /// <summary>True where there is anything to show.</summary>
    public bool HasMessages => _facts.Messages > 0;


    /// <summary>
    /// **True where the X has been pressed on a finished contact that is not in
    /// the log, and the card is saying so before it goes.**
    /// </summary>
    /// <remarks>
    /// <para>**A FINISHED CARD THAT HAS NOT BEEN LOGGED IS THE HAZARD** (work
    /// instruction 297 task 4). Clearing it drops a contact he could still have
    /// written down, and the whole point of the state word *Finished* is that this
    /// is the card he is most likely to tidy away. **It is not silently discarded**:
    /// the first press says what is about to be lost, the Log button is still on the
    /// card, and a second press clears it.</para>
    /// <para>**IT IS A WARNING AND NEVER A REFUSAL** (§0.5.1). Nothing here can stop
    /// him clearing anything; the second press always works, and the X is never
    /// greyed. He is being told, not asked for permission.</para>
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClearWarning))]
    private bool _warnsBeforeClearing;

    /// <summary>What the card says before it lets a finished contact go.</summary>
    /// <remarks>
    /// **IT SAYS WHAT IS BEING DROPPED AND NOT THAT SOMETHING WENT WRONG.** Nothing
    /// has gone wrong; he pressed a control and it is telling him the consequence
    /// before it happens, which is what the instruction asks for in place of a
    /// silent discard.
    /// </remarks>
    public string ClearWarning
        => WarnsBeforeClearing
            ? $"You finished this contact with {Callsign} and it is not in your "
              + "log. Clearing the card lets it go, and Hamlet keeps no record of "
              + "it anywhere else. Log it first if you want it, or press the X "
              + "again to clear it anyway."
            : "";

    /// <summary>The facts behind the card, for the hover and for tests.</summary>
    public Ft8CardFacts Facts => _facts;


    /// <summary>
    /// **What the `i` mark holds: every technical detail the face gave up, as
    /// facts.**
    /// </summary>
    /// <remarks>
    /// <para>**NOTHING IS DELETED** (Tim's ruling, 2026-09-08). Every fact the old
    /// panel put on the screen is here, one deliberate hover away. The face carries
    /// what happened; this carries what it was made of.</para>
    /// <para>**AND THE EXPLANATIONS ARE WITHDRAWN** (work instruction 305 task 4).
    /// This used to carry the context that makes a number mean something - unit 299
    /// was told to and did exactly that - and it reached **1015 characters over
    /// thirteen sentences**, which the operator hovered and could not read. **Show,
    /// do not tell**: a fact he hovers for is a fact, not a lesson, and the cut form
    /// measures 230 characters carrying the same twelve figures. The long wording is
    /// in git history and is not reproduced anywhere.</para>
    /// <para>**EVERY CLAUSE IS ABSENT WHERE ITS FACT IS** (§0.0). No grid, no
    /// distance and no bearing; no measured ratio, no report sentence; no dial, no
    /// frequency sentence. A hover that filled a gap with a plausible figure would
    /// be worse than one that came up short, because a deliberate look is exactly
    /// when the operator is most inclined to believe what he finds.</para>
    /// <para>**AND IT NEVER DIAGNOSES.** It says where the numbers sit on their
    /// scales and stops. Nothing here is about propagation, his equipment or his
    /// attention.</para>
    /// </remarks>
    public string Detail
    {
        get
        {
            return string.Join(Between, DetailRows);
        }
    }

    /// <summary>**The same facts, one group to a row, for the view to stack.**</summary>
    /// <remarks>
    /// <para>**TWELVE BULLETS IN ONE FLAT STACK IS THE SAME WALL, SHORTER** (Tim,
    /// 2026-09-10: *"I like that you shortened it, but make those bullet points and
    /// easy to see"*). So the rows are groups a reader takes in at a glance - what
    /// the signals were, where he is, where on the band, when - **and the last thing
    /// he sent on its own at the bottom**, because that is the one that says whether
    /// the contact is finished.</para>
    /// <para>**A ROW IS ABSENT WHERE ITS FACTS ARE** (§0.0). No reports, no signal
    /// row; no grid, no place row. A hover that filled a gap with a plausible figure
    /// would be worse than one that came up short, because a deliberate look is
    /// exactly when the operator is most inclined to believe what he finds.</para>
    /// </remarks>
    public IReadOnlyList<string> DetailRows
    {
        get
        {
            if (IsCallToAnyone)
            {
                return CallRows();
            }

            // **A PSK31 CARD HAS NO LEDGER BEHIND IT**, so no report, place, band or slot row is a
            // fact on it, and `Slots()` would print *0 slots ago* as one.
            if (IsPsk31)
            {
                return Array.Empty<string>();
            }

            var rows = new List<string>();

            foreach (var group in new[]
            {
                Reports(), WhereOnEarth(), WhereOnTheBand(), Slots(),
            })
            {
                if (group.Length > 0)
                {
                    rows.Add(group);
                }
            }

            if (_facts.HisLastPayload is { Length: > 0 } closing)
            {
                rows.Add("He last sent " + closing);
            }

            return rows;
        }
    }

    /// <summary>**What the `i` shows: one bullet to a row.**</summary>
    /// <remarks>
    /// <para>**BULLETS, BECAUSE HE ASKED FOR BULLETS** (Tim, 2026-09-10: *"I like
    /// that you shortened it, but make those bullet points and easy to see"*). A
    /// hover is read at a glance or it is not read, and a row of facts separated by
    /// middle dots is still one line to scan.</para>
    /// <para>**THE ROWS ARE <see cref="DetailRows"/> AND THE MARKER IS ADDED HERE**,
    /// so nothing that reads the facts has to strip decoration off them.</para>
    /// </remarks>
    public string DetailHover
        => string.Join(
            "\n", DetailRows.Select(row => "\u2022  " + row));

    /// <summary>True where the `i` mark has anything to hold.</summary>
    public bool HasDetail => Detail.Length > 0;

    /// <summary>What separates one fact from the next in the hover.</summary>
    /// <remarks>
    /// **A MIDDLE DOT AND NOT A FULL STOP** (work instruction 305 task 4). Facts in a
    /// row are a list rather than prose, and a full stop between them invites the
    /// sentence that used to follow it. It is the same separator the card face and
    /// the panel summaries already use, so it is not a new thing to learn.
    /// </remarks>
    private const string Between = " · ";

    /// <summary>What a receipt says where a conversation says its state.</summary>
    /// <remarks>
    /// **IT SAYS WHAT HAPPENED, NOT WHAT HAS NOT** (R4, Tim 2026-09-11: *"just the
    /// last I don't want reminders of failures"*). *Waiting on him* and *he has not
    /// answered yet* both frame ordinary silence twenty seconds into a slot as a
    /// station failing to come back, and there is no him to fail. **He called. That
    /// is the fact, and the time beside it says it is live.**
    /// </remarks>
    public const string ReceiptWord = "Calling";

    /// <summary>The one line a receipt carries.</summary>
    /// <remarks>
    /// **A PLACEHOLDER SHOULD READ AS ONE** (R2). It is quieter than a conversation
    /// card rather than the same frame with empty slots in it - **empty slots are
    /// what produced Portugal.**
    /// </remarks>
    public const string ReceiptSentence = "Your call went out to anyone listening.";

    /// <summary>Whether this card is the operator's own call to anybody.</summary>
    public bool IsCallToAnyone
        => string.Equals(
            Callsign, Ft8ContactLedger.CallToAnyone, StringComparison.Ordinal);

    /// <summary>What was called, when, and how many times.</summary>
    /// <remarks>
    /// <para>**THREE FACTS AND NOT ONE WORD ABOUT WHETHER ANYBODY WILL ANSWER**
    /// (§0.0). The elapsed time and the count are measurements; *waiting for a
    /// reply* is a prediction, and a card that made one would be the application
    /// telling the operator something it does not know.</para>
    /// <para>**THE TEXT IS THE ONE THAT WENT OUT**, taken from the ledger's own
    /// record of what was sent rather than recomposed here.</para>
    /// </remarks>
    private string CallFacts() => string.Join(Between, CallRows());

    /// <summary>What a receipt says: the call, and when it went out.</summary>
    /// <remarks>
    /// <para>**NO COUNT** (R4, Tim 2026-09-11: *"just the last I don't want reminders
    /// of failures"*). Unit 305 built a *sent n times* tally here; it comes off. **A
    /// rising number on an unanswered call is a score of the silence**, and
    /// `ACHIEVEMENTS_PHILOSOPHY.md` §2 is the same instinct one screen over - a wall
    /// of blanks reeks of failure.</para>
    /// <para>**THE LAST CALL ONLY** (R3). Pressing again refreshes this to the new
    /// slot rather than adding to it, so what the row says is always the live one.
    /// </para>
    /// </remarks>
    private List<string> CallRows()
    {
        var said = new List<string>();

        if (_facts.YourLastMessage is { Length: > 0 } sent)
        {
            said.Add("Sent " + sent);
        }

        if (_facts.LastAtUtc is { } last)
        {
            said.Add("At " + last.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                     + " UTC");
        }

        return said;
    }

    /// <summary>The reports each way, and what the scale means.</summary>
    /// <remarks>
    /// **THE FLOOR IS NAMED ONLY WHERE THE TREE HAS A FIGURE FOR IT** (§12.4). The
    /// sensitivity phase measured this decoder against a published -21 dB for FT8
    /// and no equivalent figure for FT4 exists anywhere in this repository, so on
    /// FT4 the sentence teaches the sign and the direction and does not invent a
    /// number to teach the distance.
    /// </remarks>
    private string Reports()
    {
        var his = _facts.ReportFromHim;
        var ours = _facts.ReportToHim;

        var said = new List<string>();

        if (his is { } h)
        {
            said.Add("He hears you " + Signed(h) + " dB");
        }

        if (ours is { } o)
        {
            said.Add("You hear him " + Signed(o) + " dB");
        }

        if (said.Count > 0 && _floorDb is { } floor)
        {
            said.Add("Decoder floor " + floor + " dB");
        }

        return string.Join(Between, said);
    }

    /// <summary>
    /// **The card's right column: what Hamlet knows about him, beside the map.**
    /// </summary>
    /// <remarks>
    /// <para>**TIM, 2026-09-12**: *"the real estate to the right of the map is valuable"* -
    /// and it was empty - and *"remove the info from below the map, I really never noticed
    /// it."* So the caption moved up into the space beside the picture and grew the rest of
    /// what the card already knew and was not saying.</para>
    /// <para>**ONLY WHAT HAMLET KNOWS FOR CERTAIN, AND ABSENT IS NOT DASHED** (§0.0,
    /// §0.5.1's cousin). Every line here has its own `Has` and is simply not drawn where
    /// the fact is missing. **A dash would be a measurement that was attempted and failed**;
    /// a station who has never sent a grid is not a station whose grid failed to read.</para>
    /// <para>**AND EVERY ONE OF THEM IS ALREADY A FACT ON THIS CARD.** Nothing here goes
    /// looking anywhere new: the distance and the bearing are the caption's own, the grid is
    /// `Ft8CardFacts.Grid`, the last message is `HisLastPayload` with `LastAtUtc` beside it,
    /// the count is `Messages`, and the reason is the mark's. **Nothing is invented for the
    /// column** (§0.0).</para>
    /// </remarks>
    public bool HasRightColumn
        => HasDistanceLine || HasGridLine || HasSolarTimeLine || HasLastHeardLine
            || HasMessages || HasNudgeLine;

    /// <summary>**How far away he is and which way, in one line.**</summary>
    /// <remarks>
    /// **MOVED UP FROM THE CAPTION, NOT COMPUTED AGAIN** (work instruction 327 task 4). It
    /// is `GridPath.DescribeMiles` over `GridPath.MilesBetween` and
    /// <see cref="OperatorLocation.DescribeCompass"/>, the same pair
    /// <see cref="WhereOnEarth"/> has used since unit 306, so the column and the hover
    /// cannot come to disagree about one station. **The degrees stay gone** (HM-DEC-038): a
    /// bearing is one of sixteen points and never a number.
    /// </remarks>
    public string DistanceLine
    {
        get
        {
            var here = OperatorLocation.FromGrid(_operatorGrid);
            var there = OperatorLocation.FromGrid(_facts.Grid);

            if (here is not { } from || there is not { } to)
            {
                return "";
            }

            return GridPath.DescribeMiles(GridPath.MilesBetween(from, to))
                + " · "
                + OperatorLocation.DescribeCompass(GridPath.BearingDegrees(from, to));
        }
    }

    /// <summary>True where both grids are known and a distance could be measured.</summary>
    public bool HasDistanceLine => DistanceLine.Length > 0;

    /// <summary>**The distance and the compass word, for the table's `Distance` row.**</summary>
    /// <remarks>
    /// **THE SAME STRING** - this line never carried a label of its own, so the table's
    /// value and the sentence are one property under two names, and the second name is
    /// here so the markup reads like the other five rows rather than making one row's
    /// binding look different from its neighbors.
    /// </remarks>
    public string DistanceValue => DistanceLine;

    /// <summary>**His grid.**</summary>
    /// <remarks>
    /// **THE COUNTRY IS NOT REPEATED HERE BECAUSE THE HEADER CARRIES IT.** The work
    /// instruction asks for the country *if the header does not carry it*; `Place` is bound
    /// on the card's head and holds it, so putting it here as well would be the same fact
    /// twice on one card.
    /// </remarks>
    public string GridLine
        => GridValue.Length > 0 ? "Grid " + GridValue : "";

    /// <summary>True where he has sent a grid.</summary>
    public bool HasGridLine => GridLine.Length > 0;

    /// <summary>**His grid, with no label on it - the table carries the label.**</summary>
    /// <remarks>
    /// **THE COLUMN IS A TABLE SINCE WORK INSTRUCTION 330 TASK 2** (Tim, 2026-09-12: *"the
    /// right of globe text is not aligned or even, make it nice. It's a jumbled mess."*).
    /// A table has its labels in a column of their own, so the value properties carry no
    /// label of their own and the `*Line` properties above are the same fact written as a
    /// sentence. **The two are one source and neither recomputes anything**, so the table
    /// and anything still reading a line cannot come to disagree about one station.
    /// </remarks>
    public string GridValue => _facts.Grid is { Length: > 0 } grid ? grid : "";

    /// <summary>**What time it is where he is, by the sun.**</summary>
    /// <remarks>
    /// <para>**LOCAL SOLAR TIME FROM HIS LONGITUDE, AND IT SAYS SO** (work instruction 327
    /// task 4). Fifteen degrees of longitude is an hour, so a grid gives a time directly and
    /// with no lookup at all: *his time, by the sun*.</para>
    /// <para>**IT IS NOT A TIME ZONE AND THE LABEL IS THE WHOLE OF §0.0 HERE.** Hamlet does
    /// not know his zone, his country's summer time or where his borders run, and a clock
    /// reading presented as *his local time* would be exactly the confident answer the prime
    /// directive forbids. **Solar time is a fact about the sun and his longitude** and it is
    /// stated as one. A grid is a square, so it is good to within a few minutes and the
    /// reading is given in whole minutes rather than seconds.</para>
    /// <para>**AND IT NEEDS THE CORRECTED CLOCK** (§0.0). Without a measured now there is no
    /// UTC to offset, so there is no line - not a dash and not the machine's own clock.</para>
    /// </remarks>
    public string SolarTimeLine
        => SolarTimeValue.Length > 0 ? "His time: " + SolarTimeValue : "";

    /// <summary>True where his grid and a corrected clock are both known.</summary>
    public bool HasSolarTimeLine => SolarTimeLine.Length > 0;

    /// <summary>**The clock reading and what it is, with no label - `09:21 by the sun`.**</summary>
    /// <remarks>
    /// **`by the sun` IS NOT THE LABEL AND STAYS IN THE VALUE** (§0.0). The label says
    /// *His time*; without the four words after the clock the row would claim a time zone
    /// Hamlet has never read, which is the whole reason this line is worded the way it is.
    /// </remarks>
    public string SolarTimeValue
    {
        get
        {
            if (_nowUtc is not { } now
                || OperatorLocation.FromGrid(_facts.Grid) is not { } there)
            {
                return "";
            }

            return now.AddHours(there.Longitude / 15.0)
                    .ToString("HH:mm", CultureInfo.InvariantCulture)
                + " by the sun";
        }
    }

    /// <summary>**What he last sent, and how long ago.**</summary>
    /// <remarks>
    /// **A READING CARRIES ITS AGE** (HM-DEC-111). *73* on its own is a message that may
    /// have arrived ten seconds or forty minutes ago, and on a card that stays up while a
    /// band closes those are different facts. The age half is absent without a corrected
    /// clock, for the reason <see cref="TimeLine"/>'s is.
    /// </remarks>
    public string LastHeardLine
        => LastHeardValue.Length > 0 ? "Last heard: " + LastHeardValue : "";

    /// <summary>True where he has said something this card can quote.</summary>
    public bool HasLastHeardLine => LastHeardLine.Length > 0;

    /// <summary>**What he last sent and how long ago, with no label on it.**</summary>
    public string LastHeardValue
    {
        get
        {
            if (_facts.HisLastPayload is not { Length: > 0 } said)
            {
                return "";
            }

            var value = said.Trim();

            return _facts.LastAtUtc is { } at && _nowUtc is { } now
                ? value + " · " + Ago(at, now)
                : value;
        }
    }

    /// <summary>**How many messages this conversation holds.**</summary>
    /// <remarks>
    /// **THE COUNT AND THE WAY DOWN TO THEM ARE NOW TOGETHER** (work instruction 327 task
    /// 4). The count is this line and *show the N messages* is beside it, where the card's
    /// action row used to carry the whole thing on its own. It counts **this conversation**
    /// and never everything heard from him, which is <see cref="MessagesLabel"/>'s rule and
    /// is unchanged.
    /// </remarks>
    public string MessageCountLine
        => _facts.Messages == 1
            ? "1 message"
            : _facts.Messages.ToString(CultureInfo.InvariantCulture) + " messages";

    /// <summary>**The count on its own, for the table's `Messages` row.**</summary>
    /// <remarks>
    /// **THE WORD *messages* IS THE ROW'S LABEL AND IS NOT SAID TWICE** (work instruction
    /// 330 task 2). Tim's screenshot read `2 messages  show the messag` in a column
    /// labelled by nothing at all; the row now reads `Messages   2 · show them`, which is
    /// the same two facts and one instance of the word.
    /// </remarks>
    public string MessagesValue
        => _facts.Messages.ToString(CultureInfo.InvariantCulture);

    /// <summary>**The one way down to the conversation, on the whole card.**</summary>
    /// <remarks>
    /// <para>**IT WAS ON THE CARD TWICE** (work instruction 330 task 2). Unit 327 put the
    /// count and its link in the right column and left <see cref="MessagesLabel"/>'s button
    /// on the card's top row, so *show the 2 messages* and *show the messages* were both on
    /// one card, doing one thing, and the second of them was the line Tim's screenshot
    /// showed cut off at the card's edge.</para>
    /// <para>**THE SHORT WORDING IS BECAUSE THE ROW ALREADY SAYS WHAT AND HOW MANY.** The
    /// label is *Messages* and the value is the count, so the link only has to say what
    /// pressing it does.</para>
    /// </remarks>
    public string MessagesLinkLabel => "show them";

    /// <summary>**Why this station is worth working, in the mark's own words.**</summary>
    /// <remarks>
    /// <para>**THE SAME ANSWER THE ROW'S MARK GIVES** (work instruction 327 task 4). It is
    /// handed in by <see cref="UseNudge"/> from the one `NudgeSet` the panel builds, so the
    /// disc on the list and the line on the card cannot say different things about one
    /// station.</para>
    /// <para>**AND A DOOR NAMES NOTHING HERE EITHER** (§3.1). *Would open a new area* is
    /// the category; the area is not in this object.</para>
    /// </remarks>
    public string NudgeLine => _nudge.Kind switch
    {
        NudgeKind.Visible => "New country",
        NudgeKind.Door => "Would open a new area",
        _ => "",
    };

    /// <summary>True where working him would open something.</summary>
    public bool HasNudgeLine => NudgeLine.Length > 0;

    /// <summary>Why, in full, for the popup the line opens.</summary>
    public string NudgeReasonLine => NudgeWords.Reason(_nudge);

    /// <summary>What working him earns, for the same popup.</summary>
    public string NudgeEarnsLine => NudgeWords.Earns;

    /// <summary>True while the reason is on screen.</summary>
    [ObservableProperty]
    private bool _nudgeIsOpen;

    private NudgeReason _nudge;

    /// <summary>Tell the card what the mark says about this station.</summary>
    /// <param name="reason">What the panel's one nudge set knows.</param>
    /// <remarks>
    /// **HANDED IN RATHER THAN LOOKED UP** (§0.1, and unit 278's read-once shape). The card
    /// holds no log, builds no nudge set and reads no file; the panel that already has one
    /// tells it the answer.
    /// </remarks>
    public void UseNudge(NudgeReason reason)
    {
        _nudge = reason;

        OnPropertyChanged(nameof(NudgeLine));
        OnPropertyChanged(nameof(HasNudgeLine));
        OnPropertyChanged(nameof(NudgeReasonLine));
        OnPropertyChanged(nameof(HasRightColumn));
    }

    /// <summary>Open the reason for this card's mark.</summary>
    /// <remarks>**IT TRANSMITS NOTHING** (§0.2), the same as the row's.</remarks>
    [RelayCommand]
    private void OpenTheNudge()
    {
        if (!HasNudgeLine)
        {
            return;
        }

        NudgeIsOpen = true;

        NudgeOpened?.Invoke(_nudge.Kind);
    }

    /// <summary>Put the reason away.</summary>
    [RelayCommand]
    private void CloseTheNudge() => NudgeIsOpen = false;

    /// <summary>Called when the card's mark is pressed, so the shell can write it down.</summary>
    /// <remarks>**THE KIND AND NOTHING ELSE** (HM-DEC-018, §2.1).</remarks>
    public Action<NudgeKind>? NudgeOpened { get; set; }

    /// <summary>His grid, the distance and the bearing.</summary>
    /// <remarks>
    /// **THE BEARING LIVES HERE** (Tim's ruling, 2026-09-08). It came off the face
    /// because he is not pointing a beam by hand at an FT8 station, and it is kept
    /// because somebody who later puts up a directional antenna will want it.
    /// </remarks>
    private string WhereOnEarth()
    {
        if (_facts.Grid is not { Length: > 0 } grid)
        {
            return "";
        }

        var said = new List<string> { "Grid " + grid };

        var here = OperatorLocation.FromGrid(_operatorGrid);
        var there = OperatorLocation.FromGrid(grid);

        if (here is { } from && there is { } to)
        {
            // **THE DISTANCE AND THE DIRECTION ARE ONE FACT AND READ AS ONE ROW**
            // (work instruction 306 task 4). **The degrees are gone and the compass
            // word stays**: HM-DEC-038 has said for months that a bearing is one of
            // sixteen points and never a number, and this hover was the one place in
            // the tree still printing the reading off an instrument. *480 miles
            // northeast* is a direction a person can picture.
            said.Add(
                GridPath.DescribeMiles(GridPath.MilesBetween(from, to))
                + " "
                + OperatorLocation.DescribeCompass(
                    GridPath.BearingDegrees(from, to)));
        }

        return string.Join(Between, said);
    }

    /// <summary>Where in the passband and on the dial, and how the clocks agreed.</summary>
    private string WhereOnTheBand()
    {
        if (_technical is not { } tech)
        {
            return "";
        }

        var said = new List<string>();

        if (tech.AudioHz.Length > 0)
        {
            said.Add(tech.AudioHz + " Hz in the passband");
        }

        if (tech.DialHz > 0)
        {
            said.Add("Dial "
                     + (tech.DialHz / 1_000_000.0)
                         .ToString("0.000000", CultureInfo.InvariantCulture)
                     + " MHz");
        }

        if (tech.Dt.Length > 0)
        {
            said.Add(tech.Dt + " s into the slot");
        }

        return string.Join(Between, said);
    }

    /// <summary>The slot times, the slot count, and what closed the exchange.</summary>
    private string Slots()
    {
        var said = new List<string>();

        if (_facts.FirstAtUtc is { } first && _facts.LastAtUtc is { } last
            && first != last)
        {
            said.Add(first.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                     + " to "
                     + last.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                     + " UTC");
        }

        said.Add(_facts.Slots == 1
            ? "1 slot ago"
            : _facts.Slots.ToString(CultureInfo.InvariantCulture) + " slots ago");

        // **WHAT HE LAST SENT IS NOT A TIMING FACT AND HAS ITS OWN ROW**
        // (work instruction 306 task 4). <see cref="DetailRows"/> adds it at the
        // bottom, because it is the one that says whether the contact is finished.
        return string.Join(Between, said);
    }

    /// <summary>What a closing payload is, in a clause.</summary>
    /// <remarks>
    /// <para>**NOTHING CALLS THIS SINCE UNIT 305 TASK 4**, which took the
    /// explanatory clause off the hover: the fact that survives is *He last sent
    /// RR73*, and what `RR73` means is a lesson rather than a fact. **It is left
    /// standing on purpose** - that instruction says the explanatory text is not to
    /// be deleted, moved or commented out while whether it lives anywhere else is
    /// Tim's question, asked separately.</para>
    /// **NOT A SECOND COPY OF `Ft8Vocabulary.Explain`**, which answers a different
    /// question at a different length: it says who a station is and what he is
    /// asking for, in whole sentences, on a message row. This is a clause inside a
    /// longer paragraph naming what one field shape is, and the shapes it names are
    /// the ones `Ft8MessageSplit` already tests for.
    /// </remarks>
    private static string Closing(string payload)
    {
        if (payload is "RRR")
        {
            return "which is a roger: he received you and did not say goodbye, so "
                   + "the contact is finished either way.";
        }

        if (payload is "RR73")
        {
            return "which is a roger and a goodbye in one.";
        }

        if (payload is "R73" or "73")
        {
            return "which is the goodbye people say at the end of a contact.";
        }

        if (Ft8MessageSplit.IsReport(payload, out var rogered, out var decibels))
        {
            return rogered
                ? $"which says he got your report and puts you at {Signed(decibels)} dB."
                : $"which is how well he is hearing you, in decibels against the noise.";
        }

        return Ft8MessageSplit.IsGrid(payload)
            ? "which is his grid square, the box on the map he is transmitting from."
            : "";
    }

    /// <summary>A report with its sign always shown, the way the air carries it.</summary>
    private static string Signed(int decibels)
        => decibels.ToString("+0;-0;0", CultureInfo.InvariantCulture);

    /// <summary>
    /// **Bring this card level with the ledger without replacing it.**
    /// </summary>
    /// <param name="facts">What the ledger now says about the station.</param>
    /// <param name="operatorGrid">The operator's own locator, or null.</param>
    /// <param name="action">Which of the three the button is now.</param>
    /// <param name="actionLabel">What the button now reads.</param>
    /// <param name="actionMessage">What a Send button would now transmit.</param>
    /// <param name="nowUtc">Corrected UTC, or null where no offset is measured.</param>
    /// <param name="technical">The numbers behind the `i` hover, or null.</param>
    /// <param name="decodeFloorDb">The mode's cited decode floor, or null.</param>
    /// <param name="who">Who he is and what town, where callook knows.</param>
    /// <exception cref="ArgumentNullException">There are no facts.</exception>
    /// <exception cref="ArgumentException">The facts are about another station.</exception>
    /// <remarks>
    /// <para>**THIS IS UNIT 313'S ROOT, FIXED** (§R18, work instruction 325 task
    /// 3). The panel used to call `DigitalCards.Clear()` and rebuild every card
    /// from scratch every slot. Three separate symptoms came out of that one
    /// line: the enlarged map closed itself every fifteen seconds, the card
    /// jumped out from under the pointer as the collection was replaced, and
    /// nothing a card learned about itself could outlive a slot. Unit 314 fixed
    /// the same shape for the PSK31 row by replacing in place; this is the same
    /// move for the card.</para>
    /// <para>**WHAT IS DELIBERATELY NOT TOUCHED.** <see cref="MapIsOpen"/> and
    /// <see cref="WarnsBeforeClearing"/> are the operator's state and not the
    /// ledger's, so they survive: he opened that map and he is the one who closes
    /// it. The PSK31 fields are not touched either - a PSK31 card is built by
    /// <see cref="ForPsk31"/> and updated by the PSK31 path.</para>
    /// <para>**IT REFUSES ANOTHER STATION'S FACTS.** Writing W1ABC's ledger onto
    /// D2IM's card would leave the panel showing one station's callsign over
    /// another's exchange, which is §0.0's fault in the most direct form
    /// available. Identity is the whole point of keying by station, so it is
    /// checked rather than trusted.</para>
    /// <para>**ONE EMPTY-NAME NOTIFICATION, NOT FORTY.** Almost everything on the
    /// face is derived from the facts record, so naming each property would be a
    /// list that silently goes stale the next time one is added. An empty name
    /// tells the binding layer every property may have changed, which is exactly
    /// what has happened.</para>
    /// </remarks>
    public void Refresh(
        Ft8CardFacts facts,
        string? operatorGrid,
        Ft8CardActionKind action,
        string actionLabel,
        string actionMessage,
        DateTime? nowUtc,
        Ft8CardTechnical? technical = null,
        int? decodeFloorDb = null,
        StationName? who = null)
    {
        ArgumentNullException.ThrowIfNull(facts);

        if (!string.Equals(facts.Callsign, _facts.Callsign, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"this card is {_facts.Callsign} and the facts handed to it are "
                + $"{facts.Callsign}. A card is keyed by station and never re-pointed.",
                nameof(facts));
        }

        _facts = facts;
        _nowUtc = nowUtc;
        _place = WhereHeIs(facts, operatorGrid, who);
        _country = CountryOf(facts);
        _operatorGrid = operatorGrid;
        _technical = technical;
        _floorDb = decodeFloorDb;

        ActionKind = action;
        ActionLabel = actionLabel;
        ActionMessage = actionMessage;

        OnPropertyChanged(string.Empty);
    }

    /// <summary>Move the relative time on, without rebuilding the card.</summary>
    /// <param name="nowUtc">Corrected UTC, or null where no offset is measured.</param>
    /// <remarks>
    /// **ONCE A SECOND, ON THE TICK THAT WAS ALREADY THERE.** The relative half is
    /// in seconds under a minute, so a slower cadence would show a stale count on
    /// the exact card he is deciding about; a faster one would redraw text that has
    /// not changed. `MainWindowViewModel`'s age timer already runs at one second for
    /// the spot ages and this rides it rather than adding a timer.
    /// **IT RAISES ONE PROPERTY.** The rest of the card is a function of the ledger
    /// and changes when the ledger does, not when the clock does.
    /// </remarks>
    public void Retime(DateTime? nowUtc)
    {
        if (_nowUtc == nowUtc)
        {
            return;
        }

        _nowUtc = nowUtc;
        OnPropertyChanged(nameof(TimeLine));
    }

    /// <summary>The Finished sentence, with its goodbye clause only where earned.</summary>
    private string Finished()
    {
        var both = _facts.ReportsBothWays
            ? $"You and {Callsign} each told the other how you were coming through, "
              + "and you both confirmed it."
            : $"You and {Callsign} got through to each other and you both "
              + "confirmed it.";

        return _facts.HeSignedOff ? both + " He said goodbye." : both;
    }

    /// <summary>The Gone quiet sentence.</summary>
    /// <remarks>
    /// **IT SAYS HE HAS STOPPED TRANSMITTING AND NEVER WHY** (§0.0). Nothing about
    /// propagation, his equipment or his attention is in the ledger.
    /// **AND IT IS MEASURED FROM EVERYTHING HE SENT, NOT FROM WHAT HE SENT US**,
    /// which is `Ft8ContactState`'s own rule: a station working three others at once
    /// is transmitting constantly while answering us rarely, and *gone quiet* means
    /// he has stopped altogether.
    /// </remarks>
    private string Quiet()
        => _facts.HeCameBack
            ? $"Nothing more has been heard from {Callsign}. He may have moved on."
            : $"Nothing more has been heard from {Callsign}.";

    /// <summary>How long ago, in words.</summary>
    private static string Ago(DateTime atUtc, DateTime nowUtc)
    {
        var seconds = (int)Math.Round((nowUtc - atUtc).TotalSeconds);

        if (seconds < JustNowSeconds)
        {
            return "just now";
        }

        if (seconds < 60)
        {
            return seconds.ToString(CultureInfo.InvariantCulture) + " seconds ago";
        }

        var minutes = seconds / 60;

        if (minutes == 1)
        {
            return "a minute ago";
        }

        if (minutes < 60)
        {
            return minutes.ToString(CultureInfo.InvariantCulture) + " minutes ago";
        }

        var hours = minutes / 60;

        return hours == 1 ? "an hour ago"
            : hours.ToString(CultureInfo.InvariantCulture) + " hours ago";
    }

    /// <summary>The country and the distance, or "" where neither is known.</summary>
    /// <summary>The country alone, in the short form, or "".</summary>
    /// <remarks>
    /// **THE GLOBE CAPTION WANTS A PLACE AND NOT THE WHOLE HEADER LINE.** Handing it
    /// <see cref="Place"/> put the distance in twice and leaked the header's own
    /// separator into a sentence: *K9XP is in United States · 500 miles, grid EN52.
    /// That is 500 miles from you.* The caption composes its own distance, so it
    /// needs the country and nothing else.
    /// </remarks>
    private static string CountryOf(Ft8CardFacts facts)
    {
        var entity = DxccPrefixes.EntityOf(facts.Callsign);

        return entity is null
            ? ""
            : EntitySpoken.Short(EntityQualifier.DescribeFromGrid(entity, facts.Grid));
    }

    private static string WhereHeIs(
        Ft8CardFacts facts, string? operatorGrid, StationName? who)
    {
        var entity = DxccPrefixes.EntityOf(facts.Callsign);

        // **THE FACE TAKES THE SHORT FORM AND THE HOVER KEEPS THE SPOKEN ONE**
        // (Tim, 2026-09-09: `the United States` where the card wants a place). It
        // shortens and never narrows: nothing here names a place below the country,
        // because the callsign gives none and a grid square straddles state lines.
        var country = entity is null
            ? ""
            : EntitySpoken.Short(EntityQualifier.DescribeFromGrid(entity, facts.Grid));

        var here = OperatorLocation.FromGrid(operatorGrid);
        var there = OperatorLocation.FromGrid(facts.Grid);

        var miles = here is { } from && there is { } to
            ? GridPath.DescribeMiles(GridPath.MilesBetween(from, to))
            : "";

        // **WHO HE IS AND WHAT TOWN HE IS IN, WHERE CALLOOK KNOWS** (work
        // instruction 302 tasks 4 and 5, Tim's ruling of 2026-09-09). It replaces
        // the country rather than joining it: `Richard, Sun City AZ` already says
        // the United States to anybody reading it, and a card that said both would
        // be saying one thing twice.
        //
        // **AND IT IS SILENT EVERYWHERE ELSE.** `VP2MAA` answers `INVALID` and gets
        // `Montserrat` exactly as it always did - **silence is the correct answer
        // and not a gap** (Tim, 2026-09-08). Most of what makes FT8 interesting is
        // outside the United States and none of those cards change.
        //
        // **NOTHING BELOW THE COUNTRY IS EVER INFERRED.** Not from the grid, which
        // is a box that straddles state lines, and not from the call area, which is
        // where somebody was licensed rather than where they live. This is the only
        // source in the application that can honestly say Arizona.
        var him = Named(who);

        var place = him.Length > 0 ? him : country;

        if (place.Length == 0)
        {
            return miles;
        }

        return miles.Length == 0 ? place : place + " · " + miles;
    }

    /// <summary>`Richard, Sun City AZ`, or as much of it as is known.</summary>
    private static string Named(StationName? who)
    {
        if (who is null || !who.HasAnything)
        {
            return "";
        }

        if (who.Name.Length == 0)
        {
            return who.Town;
        }

        return who.Town.Length == 0 ? who.Name : who.Name + ", " + who.Town;
    }
}
