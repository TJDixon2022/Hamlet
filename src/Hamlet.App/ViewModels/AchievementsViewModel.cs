using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>Where one mode's first contact stands.</summary>
/// <remarks>
/// **FOUR STATES AND NOT TWO, BECAUSE THE TREE HOLDS FOUR** (work instruction 287
/// task 1). The order names two reasons for an unearned row, *not yet done* and
/// *awaiting a send path*, and the reading found that CW has a send path and no way
/// into the log, and that WSPR is not a contact at all. Collapsing either onto one
/// of the other two would put a sentence on screen that is not true.
/// </remarks>
public enum ModeFirstState
{
    /// <summary>He has done it, and the row names the contact.</summary>
    Earned,

    /// <summary>Everything is in place. It is his to go and do.</summary>
    YoursToGo,

    /// <summary>Hamlet cannot do this yet, so it is not his to have missed.</summary>
    WaitingOnHamlet,

    /// <summary>Nobody works anybody, so there is no first to earn.</summary>
    NotAContact,
}

/// <summary>One row of the mode-firsts card.</summary>
/// <remarks>
/// <para>**EARNED AND UNEARNED DIFFER BY THREE THINGS BEFORE THEY DIFFER BY A
/// HUE** (§0.6). Every row carries a <see cref="Mark"/>, a <see cref="Standing"/>
/// in words, and, where it is earned, a line of evidence no unearned row has. Print
/// this card in grayscale and it reads exactly the same.</para>
/// <para>**AND THE TWO KINDS OF UNEARNED DO NOT LOOK ALIKE**, which is Tim's
/// ruling. *Not yet done* is an invitation and *waiting on Hamlet* is an admission,
/// and a thing he has not been given the means to do is not a thing he has failed
/// to do.</para>
/// <para>**FORMATTED HERE RATHER THAN IN THE MARKUP**, the same way
/// <see cref="ContactLogRow"/> is, so what a reader sees can be asserted by a test
/// that never opens a window.</para>
/// </remarks>
public sealed class ModeFirstRow
{
    /// <summary>Build a row from a mode and what the log holds for it.</summary>
    /// <param name="mode">The mode, as ADIF spells it.</param>
    /// <param name="state">Where it stands.</param>
    /// <param name="first">The earliest contact in it, or null.</param>
    /// <param name="why">What the hover says about this row.</param>
    internal ModeFirstRow(
        ContactMode mode, ModeFirstState state, AdifContact? first, string why)
    {
        Name = mode.Name;
        AdifSpelling = mode.AdifSpelling;
        State = state;
        Why = why;

        Earned = state == ModeFirstState.Earned;

        Evidence = first is null
            ? ""
            : Describe(first);

        Station = first is null || string.IsNullOrWhiteSpace(first.Call)
            ? ""
            : first.Call.Trim();
    }

    /// <summary>The mode, in Hamlet's own words.</summary>
    public string Name { get; }

    /// <summary>How a record spells it, e.g. `MODE=MFSK, SUBMODE=FT4`.</summary>
    public string AdifSpelling { get; }

    /// <summary>Where the row stands.</summary>
    public ModeFirstState State { get; }

    /// <summary>True where he has done it.</summary>
    public bool Earned { get; }

    /// <summary>The callsign, the date and the band, or "".</summary>
    public string Evidence { get; }

    /// <summary>True where there is evidence to draw.</summary>
    public bool HasEvidence => Evidence.Length > 0;

    /// <summary>The station worked, or "".</summary>
    /// <remarks>
    /// **NAMED SEPARATELY BECAUSE THE NOTICE SAYS IT IN A SENTENCE** and the card
    /// says it in a line, and splitting the line back apart to build the sentence
    /// is how the two come to disagree.
    /// </remarks>
    public string Station { get; }

    /// <summary>What the hover explains about this row.</summary>
    /// <remarks>
    /// **TEXT ONLY WHERE HE HOVERS** (Tim, 2026-09-08). Six rows each carrying a
    /// paragraph is a wall, and the state is already on the row in a word.
    /// </remarks>
    public string Why { get; }

    /// <summary>
    /// The mark that carries the state without a hue (§0.6).
    /// </summary>
    /// <remarks>
    /// **A FILLED RING IS DONE, A HOLLOW ONE IS OPEN TO HIM, A DASHED ONE IS ON
    /// HAMLET, AND A BAR IS SOMETHING THAT DOES NOT HAPPEN.** Four shapes for four
    /// states, so a reader who sees no color at all still reads the card.
    /// </remarks>
    public string Mark => State switch
    {
        ModeFirstState.Earned => "●",
        ModeFirstState.YoursToGo => "○",
        ModeFirstState.WaitingOnHamlet => "◌",
        _ => "—",
    };

    /// <summary>The state in words, beside the mark.</summary>
    public string Standing => State switch
    {
        ModeFirstState.Earned => "earned",
        ModeFirstState.YoursToGo => "not yet done",
        ModeFirstState.WaitingOnHamlet => "waiting on Hamlet",
        _ => "not a contact mode",
    };

    /// <summary>The ink for the mark, from the belt's own vocabulary.</summary>
    /// <remarks>
    /// **THE COLOR IS THE LAST CARRIER AND NEVER THE ONLY ONE** (§0.6). The mark
    /// and the word above it both say the state already.
    /// </remarks>
    public string Ink => State switch
    {
        ModeFirstState.Earned => "#2E7D4F",
        ModeFirstState.YoursToGo => "#D9A400",
        _ => "#7C7666",
    };

    /// <summary>The contact, said the way a person reads one.</summary>
    private static string Describe(AdifContact first)
    {
        var call = string.IsNullOrWhiteSpace(first.Call)
            ? ContactLogRow.Absent
            : first.Call.Trim();

        var when = first.StartedUtc is { } t
            ? t.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : ContactLogRow.Absent;

        var band = string.IsNullOrWhiteSpace(first.Band)
            ? ContactLogRow.Absent
            : first.Band.Trim();

        return call + " on " + when + ", " + band;
    }
}

/// <summary>
/// **What he has done, in his own record: the belt, and the first of each mode.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: an achievements screen beside the contact
/// log, carrying the belt and the first contact in each of CW, FT8, FT4, PSK31,
/// WSPR and Voice.</para>
/// <para>**IT READS AND IT NEVER WRITES.** There is no path from this class to
/// <see cref="Hamlet.App.Settings.ContactLogStore.Append"/>, to a file, or to a
/// send path. A log record is a statement the operator made and a screen about it
/// is not the place to revise one.</para>
/// <para>**EVERY FIGURE HERE IS DERIVED FROM THE RECORDS HANDED IN AND NOTHING IS
/// REMEMBERED** (unit 278's rule, and §0.0 behind it). A remembered first would go
/// on asserting a contact after the record behind it had gone, which is a claim
/// with no evidence under it. So a shrunken log shows fewer firsts, and that is the
/// honest picture rather than a fault.</para>
/// <para>**THE COUNT IS EVERY RECORD, WHICH IS THE SAME COUNT THE LOG WINDOW AND
/// THE STATUS BAR USE** (Tim: the count is every logged contact, not distinct
/// callsigns). One reading, one number, and the three surfaces cannot come to
/// disagree.</para>
/// </remarks>
public sealed class AchievementsViewModel
{
    /// <summary>Build the screen from the log.</summary>
    /// <param name="records">Every record in the file, faults and all.</param>
    /// <exception cref="ArgumentNullException">The records are null.</exception>
    public AchievementsViewModel(IReadOnlyList<AdifLogRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        Count = records.Count;
        Milestones = new ContactMilestones(Count);
        Rank = ContactBelt.For(Count);
        Progress = ContactBelt.Progress(Count);

        Firsts = ContactModes.Six.Select(m => Row(m, records)).ToList();
    }

    /// <summary>Build the screen from the log and the operator's own grid.</summary>
    /// <param name="records">Every record in the file, faults and all.</param>
    /// <param name="operatorGrid">His locator, for distances and for the grey line.</param>
    /// <exception cref="ArgumentNullException">The records are null.</exception>
    public AchievementsViewModel(
        IReadOnlyList<AdifLogRecord> records, string? operatorGrid)
        : this(records, operatorGrid, AchievementPoints.Absent(""))
    {
    }

    /// <summary>Build the screen from the log, the grid and the owner's points file.</summary>
    /// <param name="records">Every record in the file, faults and all.</param>
    /// <param name="operatorGrid">His locator, for distances and for the grey line.</param>
    /// <param name="points">The owner's points file, loaded or absent.</param>
    /// <exception cref="ArgumentNullException">The records or the points are null.</exception>
    /// <remarks>
    /// **THE POINTS ARE HANDED IN AND NOT READ HERE**, for `AchievementScreen`'s own
    /// reason: reading a file is an I/O act with a failure mode, and a view model that
    /// performs one cannot be constructed in a test without a folder. The caller reads the
    /// file; this composes the page from whatever came back, including *nothing came
    /// back*.
    /// </remarks>
    public AchievementsViewModel(
        IReadOnlyList<AdifLogRecord> records,
        string? operatorGrid,
        AchievementPoints points)
        : this(records)
    {
        ArgumentNullException.ThrowIfNull(points);

        var log = new AchievementLog(records, operatorGrid);

        Screen = new AchievementScreen(
            log, AchievementChallenges.For(log, operatorGrid));

        Page = new AchievementBadgePage(log, points);
    }

    /// <summary>
    /// **The opening page: eight badges with scores** (work instruction 331 task 6).
    /// </summary>
    /// <remarks>
    /// **TIM, 2026-09-12**: *"The opening dialog page should be a list of badges
    /// indicating type of achievements. Nice badges, real nice."* Null where the view model
    /// was built without a grid, which is the diagnostic constructor's state and not one
    /// the application reaches.
    /// </remarks>
    public AchievementBadgePage? Page { get; }

    /// <summary>True where the badge page has been built.</summary>
    public bool HasPage => Page is not null;

    /// <summary>
    /// **What he has opened, and the standing targets.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE SCREEN FROM WORK INSTRUCTION 298 ONWARD** and the six mode-first
    /// rows below it are what it replaces. `ACHIEVEMENTS_PHILOSOPHY.md` §3.1 forbids a
    /// card for something he has not opened, and unit 287's card drew all six modes
    /// always - four of which can never light on this application - under a heading
    /// reading `1 of 6`, which advertises five slots he cannot see.
    /// </remarks>
    public AchievementScreen? Screen { get; }

    /// <summary>True where the screen has been built.</summary>
    public bool HasScreen => Screen is not null;

    /// <summary>How many records the log holds.</summary>
    public int Count { get; }

    /// <summary>The belt he is on.</summary>
    public BeltRank Rank { get; }

    /// <summary>How far between this rank and the next, 0 to 1.</summary>
    public double Progress { get; }

    /// <summary>Where he stands against the badges.</summary>
    public ContactMilestones Milestones { get; }

    /// <summary>The six rows, in the order he listed them.</summary>
    public IReadOnlyList<ModeFirstRow> Firsts { get; }

    /// <summary>How many of the six he has actually earned.</summary>
    public int EarnedCount => Firsts.Count(f => f.Earned);

    /// <summary>The belt, in one line.</summary>
    public string BeltLine
        => Count == 1
            ? "1 contact logged, on the " + Rank.Name + " belt."
            : Count.ToString("N0", CultureInfo.InvariantCulture)
              + " contacts logged, on the " + Rank.Name + " belt.";

    /// <summary>The card's heading, counting what is earned.</summary>
    /// <remarks>
    /// **IT COUNTS AND IT DOES NOT JUDGE**, the same rule the log dialog's summary
    /// follows. Five of six unearned on a first evening is not a poor showing, and
    /// most of the five are not his to have done.
    /// </remarks>
    public string FirstsLine
        => "The first of each mode: " + EarnedCount + " of " + Firsts.Count + ".";

    /// <summary>How many of the six the operator could go and earn today.</summary>
    /// <remarks>
    /// **THE WORDING CHANGED WITH THE FACT** (work instruction 291 task 4). It
    /// said *how many of the six a record could carry at all*, which was the same
    /// count while the log could only hold FT8. A record can now carry FT4 and
    /// PSK31 too, and neither is earnable, so the two questions came apart and
    /// this one counts the rows he can act on.
    /// </remarks>
    public int EarnableCount
        => Firsts.Count(
            f => f.State is ModeFirstState.Earned or ModeFirstState.YoursToGo);

    /// <summary>What the card says about the rows that cannot light.</summary>
    /// <remarks>
    /// <para>**IT SPEAKS UNASKED, BECAUSE IT IS A FAULT AND NOT A TIP** (Tim's
    /// ruling: text only where he hovers, and a fault speaks unasked). A card with
    /// five rows he cannot fill in reads as five things he has not got round to,
    /// and that is not what it is.</para>
    /// <para>**THIS IS §0.0 ON A CARD.** The exposure of an achievements screen is
    /// an achievement claimed that was not earned, and its quieter twin is a row
    /// that looks earnable and never can be. Both are the application being more
    /// confident than its input allows.</para>
    /// </remarks>
    public string Honesty
    {
        get
        {
            var waiting = Firsts
                .Where(f => f.State is ModeFirstState.WaitingOnHamlet)
                .Select(f => f.Name)
                .ToList();

            if (waiting.Count == 0)
            {
                return "";
            }

            // **ONE LINE, AND THE SIX ROWS IT USED TO CAPTION ARE GONE** (work
            // instruction 298 task 2). A card of six with one filled in is the wall
            // §2 forbids; **the fact underneath it is not** - what Hamlet cannot yet
            // do is a thing the application owes him, and it is said in a sentence
            // rather than in four unearnable rows.
            var named = waiting.Count == 1
                ? waiting[0]
                : string.Join(", ", waiting.Take(waiting.Count - 1))
                  + " and " + waiting[^1];

            return "Hamlet cannot work " + named + " yet, so nothing here is "
                + "waiting on you for those.";
        }
    }

    /// <summary>Where the file is, so he can go and look.</summary>
    public string LogPath { get; init; } = "";

    /// <summary>One row, from the mode and the whole log.</summary>
    /// <remarks>
    /// <para>**THE EARLIEST SOUND RECORD IN THE MODE WINS**, and a record with no
    /// end marker is not read. That is the source the *worked* mark has always
    /// used, and a first claimed off a half-written line is a claim this unit was
    /// not asked to make.</para>
    /// <para>**A RECORD WITH NO TIME STILL COUNTS AS A CONTACT**, and sorts last,
    /// because the mode is the achievement and the date is only how it is
    /// described. Every record Hamlet writes carries `QSO_DATE` and `TIME_ON`, so
    /// this is about somebody else's file imported into his.</para>
    /// </remarks>
    private static ModeFirstRow Row(
        ContactMode mode, IReadOnlyList<AdifLogRecord> records)
    {
        // **A BEACON MODE IS NEVER EARNED, INCLUDING FROM A RECORD THAT SAYS IT.**
        // Watched failing first: with the match run before this, a `MODE=WSPR`
        // record lit the row and the hover said *you worked this one*, naming a
        // station and a date. **That is the fault this screen exists to avoid** -
        // an achievement claimed that was not earned - and it arrives by the one
        // route nobody expects, which is a record that is perfectly legal ADIF.
        // Any logger would write such a line, and it still describes a beacon that
        // nobody answered.
        if (!mode.IsContactMode)
        {
            return new ModeFirstRow(
                mode, ModeFirstState.NotAContact, null, Why(mode));
        }

        var first = records
            .Where(r => r.Terminated)
            .Select(r => r.Contact)

            // **AND HERE IS WHERE IT WENT** (work instruction 291 task 4). Until
            // this unit `AdifContact` had no submode, so this line handed in a
            // literal null and FT4 and PSK31 matched nothing whatever the file
            // said. It now reads the record's own, which is what lets an
            // `MODE=MFSK, SUBMODE=FT4` record light the FT4 row.
            //
            // **A BARE `MODE=MFSK` STILL LIGHTS NOTHING**, and that is
            // deliberate: `Matches` refuses a mode whose submode the record does
            // not carry (`ContactModes.cs:36-42`), because *some kind of
            // multi-frequency shift keying* read as FT4 would show him a first he
            // had not made. Records written before this unit have no submode and
            // so are unaffected, which is the right answer rather than a
            // limitation.
            .Where(c => mode.Matches(c.Mode, c.Submode))
            .OrderBy(c => c.StartedUtc ?? DateTime.MaxValue)
            .FirstOrDefault();

        if (first is not null)
        {
            return new ModeFirstRow(
                mode, ModeFirstState.Earned, first,
                "You worked this one. The row shows the station, the date it "
                + "happened and the band it happened on, read from the record "
                + "itself. A record spells it " + mode.AdifSpelling + ".");
        }

        var state = StandingOf(mode.Name);

        return new ModeFirstRow(mode, state, null, Why(mode));
    }

    /// <summary>
    /// **Where a mode stands with Hamlet, which is a fact about the application.**
    /// </summary>
    /// <param name="name">Hamlet's name for the mode.</param>
    /// <returns>The state an unearned row is in.</returns>
    /// <remarks>
    /// <para>**READ FROM THE TREE ON 2026-09-08, NOT ASSUMED** (work instruction
    /// 287 task 1, and re-read on 2026-09-09 for 291 task 4). There is one path
    /// into the contact log and it is `LogContactAsync`, which takes a decoded FT8
    /// row; the mode it writes is unconditionally FT8 — the literal `"FT8"` until
    /// work instruction 291 and `ContactModes.Named("FT8")` since, which is a type
    /// change and not a condition — and the log dialog does not let the operator
    /// edit it. **So FT8 is still the only mode Hamlet writes a record in**, and
    /// that is what these states are about.</para>
    /// <para>**WHAT THE LOG CAN HOLD AND WHAT HAMLET CAN WRITE ARE NOW DIFFERENT
    /// QUESTIONS** (work instruction 291). A record can carry `MODE=MFSK,
    /// SUBMODE=FT4`, so an FT4 contact imported from another logger lights the row
    /// off the file; what Hamlet cannot do is make one. That is why FT4 stays
    /// `WaitingOnHamlet` when unearned rather than becoming `YoursToGo`, and the
    /// hover says which of the two gaps closed.</para>
    /// <para>**CW IS THE ONE THE WORK INSTRUCTION HAD WRONG.** It said CW has no
    /// send path; HM-DEC-059 built one and `MainWindowViewModel` attaches a
    /// `CwTransmitter` over a `KeyerCwSender`. He can work a station on the key
    /// tonight, with Hamlet keying the radio, and **there is nowhere for that
    /// contact to be written down**. So it is waiting on Hamlet, and its sentence
    /// says the true thing rather than borrowing another row's.</para>
    /// <para>**THIS WILL GO STALE THE DAY A SEND PATH LANDS**, which is why it is
    /// one method with one list rather than a flag on six rows.</para>
    /// </remarks>
    private static ModeFirstState StandingOf(string name) => name switch
    {
        "FT8" => ModeFirstState.YoursToGo,
        "WSPR" => ModeFirstState.NotAContact,
        _ => ModeFirstState.WaitingOnHamlet,
    };

    /// <summary>What the hover says about an unearned row.</summary>
    private static string Why(ContactMode mode) => mode.Name switch
    {
        "FT8" => "Nothing stands in the way of this one. Work a station on FT8, "
            + "right-click the line it came in on and log it, and this fills "
            + "itself in from the record. A record spells it MODE=FT8.",

        "CW" => "Hamlet can key the radio in Morse, and it has nowhere to write "
            + "the contact down. Logging happens from a decoded FT8 line and "
            + "from nothing else at the moment, so a contact you make on the "
            + "key does not reach the file. That is the application's gap "
            + "rather than yours. A record would spell it MODE=CW.",

        // **ONE THING STANDS IN THE WAY WHERE TWO DID** (work instruction 291
        // task 4). This sentence used to say the record had no room for the mode
        // either, and that stopped being true the moment `AdifContact` gained
        // `SUBMODE`. A screen telling the operator Hamlet cannot do something it
        // can now do is the §0.0 fault this card was built to avoid, so the
        // sentence names only the gap that remains.
        "FT4" => "One thing stands in the way, and it is not yours. Hamlet can "
            + "tune you to the FT4 frequencies and cannot work them yet. The "
            + "log itself is ready: ADIF files FT4 as a submode of MFSK, and "
            + "Hamlet now writes both halves, so an FT4 contact would be "
            + "recorded correctly the day you can make one. A record spells it "
            + "MODE=MFSK with SUBMODE=FT4.",

        // **IT USED TO OPEN WITH \"THE SAME TWO THINGS AS FT4\" AND WENT STALE BY
        // REFERENCE.** PSK31's record room now exists as a side effect of the
        // submode landing; its send path does not, and nothing in this project
        // has started one. It says its own reasons rather than borrowing FT4's.
        "PSK31" => "One thing stands in the way, and it is not yours. Hamlet can "
            + "tune you to the PSK31 watering holes and cannot work them. The "
            + "log has room for it: ADIF files PSK31 as a submode of PSK, and "
            + "Hamlet learned to write a submode when it learned to write FT4, "
            + "so this one came along for the ride. A record spells it "
            + "MODE=PSK with SUBMODE=PSK31.",

        "WSPR" => "WSPR is a beacon rather than a conversation. You send a "
            + "two-minute signal carrying your callsign, your grid and your "
            + "power, receivers around the world decode it and report where "
            + "they heard you, and nobody works anybody. There is no contact "
            + "to log, so this is not a first that anyone could earn. What it "
            + "would take instead is your own callsign turning up in the spot "
            + "database, which is who heard me rather than who did I work, and "
            + "that is a different achievement waiting on a ruling.",

        _ => "Hamlet has no voice path of any kind, so this is not something you "
            + "have missed. Voice is Hamlet's own word for a family rather than "
            + "an ADIF value: a record would spell it MODE=SSB, MODE=AM or "
            + "MODE=FM, depending on how you worked the station.",
    };
}
