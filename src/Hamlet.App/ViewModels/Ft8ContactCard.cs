using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

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

    private readonly Ft8CardFacts _facts;
    private readonly string _place;
    private readonly string? _operatorGrid;
    private readonly Ft8CardTechnical? _technical;
    private readonly int? _floorDb;

    private DateTime? _nowUtc;

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
    public Ft8ContactCard(
        Ft8CardFacts facts,
        string? operatorGrid,
        Ft8CardActionKind action,
        string actionLabel,
        string actionMessage,
        DateTime? nowUtc,
        Ft8CardTechnical? technical = null,
        int? decodeFloorDb = null)
    {
        ArgumentNullException.ThrowIfNull(facts);

        _facts = facts;
        _nowUtc = nowUtc;
        _place = WhereHeIs(facts, operatorGrid);
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
    public string Place => _place;

    /// <summary>True where there is anything to say about where he is.</summary>
    public bool HasPlace => _place.Length > 0;

    /// <summary>The state, in one word a newcomer can read.</summary>
    /// <remarks>
    /// **A WORD AND NOT A SLOT COUNT** (Tim's ruling, 2026-09-08). *Gone quiet,
    /// 11 slots* is what the contact line says and it is a measurement; on a card the
    /// count is arithmetic the reader should not have to do, and the relative time
    /// beside it already says how stale this is. The count is on the hover.
    /// </remarks>
    public string StateWord => _facts.State switch
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
    public string Sentence => _facts.State switch
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
    public Ft8CardActionKind ActionKind { get; }

    /// <summary>What the button reads.</summary>
    public string ActionLabel { get; }

    /// <summary>The message a Send button would transmit, exactly as it goes out.</summary>
    public string ActionMessage { get; }

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
        Ft8CardActionKind.Send =>
            $"Sends “{ActionMessage}” to {Callsign}, once, in the next "
            + "slot. The ring beside this button is the seconds left to press it. "
            + "One press is one transmission and nothing here sends on a timer.",
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
    public bool ShowsRing => ActionKind == Ft8CardActionKind.Send;


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
    public bool ShowsLogLink => ActionKind != Ft8CardActionKind.Log;

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
        _operatorGrid, _facts.Grid, Callsign, _place);

    private Ft8GlobePlot? _globe;

    /// <summary>True where the globe has anything at all to draw.</summary>
    /// <remarks>
    /// **A STATION WITH NO GRID STILL GETS THE MARK**, because the answer *Hamlet
    /// does not know where he is* is worth a hover: it tells him the station never
    /// sent one, which is a fact about the contact rather than a gap in the screen.
    /// The mark goes only where neither end is known at all, which is a station with
    /// no grid on a machine with no grid in Settings, and there is nothing to say.
    /// </remarks>
    public bool ShowsGlobe => Globe.HasMap;

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
    /// **What the `i` mark holds: every technical detail the face gave up, with the
    /// context that makes a number mean something.**
    /// </summary>
    /// <remarks>
    /// <para>**NOTHING IS DELETED** (Tim's ruling, 2026-09-08). Every fact the old
    /// panel put on the screen is here, one deliberate hover away. The face carries
    /// what happened; this carries what it was made of.</para>
    /// <para>**WITH CONTEXT AND NOT BARE NUMBERS**, which is the stated purpose:
    /// *"a number alone teaches nobody."* `-4 dB` on its own is a reading somebody
    /// has to already understand; `-4 dB, and this decoder reads down to about -21,
    /// so you are a long way above the floor` is the same reading and an
    /// explanation of the scale it sits on.</para>
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
            var parts = new List<string>();

            if (Reports() is { Length: > 0 } reports)
            {
                parts.Add(reports);
            }

            if (WhereOnEarth() is { Length: > 0 } where)
            {
                parts.Add(where);
            }

            if (WhereOnTheBand() is { Length: > 0 } band)
            {
                parts.Add(band);
            }

            if (Slots() is { Length: > 0 } slots)
            {
                parts.Add(slots);
            }

            return string.Join(" ", parts);
        }
    }

    /// <summary>True where the `i` mark has anything to hold.</summary>
    public bool HasDetail => Detail.Length > 0;

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

        if (his is null && ours is null)
        {
            return "";
        }

        var swap = his is not null && ours is not null
            ? $"He hears you at {Signed(his.Value)} dB and you hear him at "
              + $"{Signed(ours.Value)} dB."
            : his is not null
                ? $"He hears you at {Signed(his.Value)} dB, and you have not told "
                  + "him how he is coming through yet."
                : $"You told him he is coming through at {Signed(ours!.Value)} dB, "
                  + "and he has not told you how you are doing yet.";

        var scale = _floorDb is { } floor
            ? "Those are decibels against the noise, so a minus number is the "
              + $"ordinary case here: this decoder reads down to about {floor}, and "
              + "anything well above that is a comfortable signal rather than a "
              + "marginal one."
            : "Those are decibels against the noise, and the decoder reads a long "
              + "way below zero, so a minus number is the ordinary case rather "
              + "than a problem.";

        return swap + " " + scale;
    }

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
            return "He has not put a grid square on the air, so Hamlet has no way "
                   + "to say how far away he is.";
        }

        var here = OperatorLocation.FromGrid(_operatorGrid);
        var there = OperatorLocation.FromGrid(grid);

        if (here is not { } from || there is not { } to)
        {
            return $"He is in grid {grid}, and Hamlet needs your own grid square "
                   + "in Settings before it can work out how far that is.";
        }

        var miles = GridPath.DescribeMiles(GridPath.MilesBetween(from, to));
        var bearing = GridPath.DescribeBearing(GridPath.BearingDegrees(from, to));

        return $"He is in grid {grid}, {miles} away, on an initial bearing of "
               + $"{bearing} from you. A four-character grid is a box about seventy "
               + "miles across, so the distance is good to about that and no better.";
    }

    /// <summary>Where in the passband and on the dial, and how the clocks agreed.</summary>
    private string WhereOnTheBand()
    {
        if (_technical is not { } tech)
        {
            return "";
        }

        var said = new List<string>();

        if (tech.AudioHz.Length > 0 && tech.DialHz > 0)
        {
            var mhz = (tech.DialHz / 1_000_000.0)
                .ToString("0.000000", CultureInfo.InvariantCulture);

            said.Add($"His tone sat {tech.AudioHz} Hz up inside the receiver's "
                     + $"passband while the dial was on {mhz} MHz. Everybody on the "
                     + "band shares one dial setting and takes a different slice of "
                     + "the audio, which is how dozens of stations fit where one "
                     + "voice would go.");
        }

        if (tech.Dt.Length > 0)
        {
            said.Add($"His transmission began {tech.Dt} seconds into the slot. Both "
                     + "clocks have to agree within about a second for this to "
                     + "decode at all, so a small number here is the two of you "
                     + "keeping the same time.");
        }

        return string.Join(" ", said);
    }

    /// <summary>The slot times, the slot count, and what closed the exchange.</summary>
    private string Slots()
    {
        var said = new List<string>();

        if (_facts.FirstAtUtc is { } first && _facts.LastAtUtc is { } last
            && first != last)
        {
            said.Add("This ran from "
                     + first.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                     + " to "
                     + last.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                     + " UTC.");
        }

        said.Add(_facts.Slots == 1
            ? "That is one slot ago."
            : $"That is {_facts.Slots.ToString(CultureInfo.InvariantCulture)} slots "
              + "ago, counted in the transmit-and-listen turns the band runs on "
              + "rather than in seconds.");

        if (_facts.HisLastPayload is { Length: > 0 } closing
            && Closing(closing) is { Length: > 0 } explained)
        {
            said.Add($"The last thing he sent you was {closing}, {explained}");
        }

        return string.Join(" ", said);
    }

    /// <summary>What a closing payload is, in a clause.</summary>
    /// <remarks>
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
    private static string WhereHeIs(Ft8CardFacts facts, string? operatorGrid)
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

        if (country.Length == 0)
        {
            return miles;
        }

        return miles.Length == 0 ? country : country + " · " + miles;
    }
}
