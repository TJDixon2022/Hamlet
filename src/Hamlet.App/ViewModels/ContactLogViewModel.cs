using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>One contact, as the log window draws it.</summary>
/// <remarks>
/// <para>**A FIELD THE RECORD DOES NOT CARRY IS SHOWN AS ABSENT, NEVER BLANK AND
/// NEVER GUESSED** (§0.0). A hole in a table is a hole a reader fills in from
/// habit, and the habit here would be to assume the band was the one he is on now.
/// Records written before unit 275 have no `BAND` and no `FREQ` at all, because
/// Hamlet did not observe where the contact happened, and the row says so in
/// words.</para>
/// <para>**FORMATTED HERE RATHER THAN IN THE MARKUP**, the same way
/// <see cref="DigitalDecodeRow"/> is, so what a reader sees can be asserted by a
/// test that never opens a window.</para>
/// </remarks>
public sealed class ContactLogRow
{
    /// <summary>What a cell says where the record carries no value.</summary>
    /// <remarks>
    /// **A WORD AND NOT A DASH.** A dash in a table of reports reads as a report,
    /// and this hobby writes plenty of them; the word cannot be mistaken for data.
    /// </remarks>
    public const string Absent = "not recorded";

    /// <summary>Build a row from one record of the file.</summary>
    /// <param name="record">The record, faults and all.</param>
    public ContactLogRow(AdifLogRecord record)
    {
        var c = record.Contact;

        Call = Or(c.Call);
        WhenUtc = c.StartedUtc is { } t
            ? t.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : Absent;
        Band = Or(c.Band);
        Submode = (c.Submode ?? "").Trim();
        Mode = WithSubmode(Or(c.Mode), Submode);
        ReportSent = Or(c.ReportSent);
        ReportReceived = Or(c.ReportReceived);
        TheirGridSquare = Or(c.GridSquare);
        MyGridSquare = Or(c.MyGridSquare);
        Comment = Or(c.Comment);

        StartedUtc = c.StartedUtc;
        Faults = record.Faults;
        IsSound = record.IsSound;
        Fault = Damage(record);
    }

    /// <summary>The station worked, or that it was not recorded.</summary>
    public string Call { get; }

    /// <summary>When it started, in UTC, or that it was not recorded.</summary>
    public string WhenUtc { get; }

    /// <summary>The band, or that it was not recorded.</summary>
    public string Band { get; }

    /// <summary>The mode, with the submode beside it where the record carries one.</summary>
    /// <remarks>
    /// <para>**AN FT4 CONTACT READ AS `MFSK` UNTIL UNIT 292**, which is the ADIF mode
    /// FT4, FT8, JS8 and half a dozen others all share. Unit 291 gave the record its
    /// `SUBMODE` and named this window as the surface that still dropped it; a log whose
    /// whole purpose is to say what he worked cannot show three different modes under
    /// one word.</para>
    /// <para>**ONE CELL AND NOT A SUBMODE COLUMN, AND THAT IS A CHOICE WITH A REASON.**
    /// Every other absent cell in this table means *Hamlet did not record this*, which
    /// is what <see cref="Absent"/> says. An FT8 contact's submode is not unrecorded -
    /// **it does not exist**, because FT8 is an ADIF mode in its own right and unit 291
    /// proved the record carries no `SUBMODE` tag at all. A column reading `not
    /// recorded` on every FT8 row would assert a hole where there is none, in a table
    /// whose other holes are real (§0.0).</para>
    /// <para>**FT8'S CELL IS BYTE-IDENTICAL TO WHAT IT WAS**: no submode, no separator,
    /// the mode alone. The composition only ever adds.</para>
    /// </remarks>
    public string Mode { get; }

    /// <summary>The submode as the record carries it, or "" where there is none.</summary>
    /// <remarks>
    /// **"" AND NOT <see cref="Absent"/>**, because absent here is a real answer rather
    /// than a gap: most modes have no submode. This is the raw field, kept beside the
    /// composed cell so a reader of this type can tell the two apart.
    /// </remarks>
    public string Submode { get; }

    /// <summary>The report he sent, or that it was not recorded.</summary>
    public string ReportSent { get; }

    /// <summary>The report he received, or that it was not recorded.</summary>
    public string ReportReceived { get; }

    /// <summary>The station's locator, or that it was not recorded.</summary>
    /// <remarks>
    /// <para>**IT WAS WRITTEN AND NOT SHOWN** (work instruction 282 task 3).
    /// `AdifLog` has written `GRIDSQUARE` and `MY_GRIDSQUARE` as separate fields
    /// since unit 274, and the window carried a column for the second and none at
    /// all for the first — so a log of contacts said where he was standing and never
    /// where the other station was, which is the half of a contact worth keeping.
    /// </para>
    /// <para>**AND THE TWO ARE LABELLED APART.** `my grid` and `their grid`, not one
    /// column called `grid`: a locator with nobody's name on it is a number a reader
    /// attaches to whichever station he was thinking of.</para>
    /// </remarks>
    public string TheirGridSquare { get; }

    /// <summary>His own locator at the time, or that it was not recorded.</summary>
    public string MyGridSquare { get; }

    /// <summary>What he typed, or that nothing was recorded.</summary>
    public string Comment { get; }

    /// <summary>The moment, for ordering. Null sorts oldest.</summary>
    public System.DateTime? StartedUtc { get; }

    /// <summary>What could not be read out of this record, in his words.</summary>
    public IReadOnlyList<string> Faults { get; }

    /// <summary>True where the record is whole and every field was readable.</summary>
    public bool IsSound { get; }

    /// <summary>One line about the damage, or "".</summary>
    public string Fault { get; }

    /// <summary>True where there is damage to draw.</summary>
    public bool HasFault => Fault.Length > 0;

    private static string Or(string? value)
        => string.IsNullOrWhiteSpace(value) ? Absent : value.Trim();

    /// <summary>The mode cell, with the submode after it where there is one.</summary>
    /// <param name="mode">The mode, already resolved through <see cref="Or"/>.</param>
    /// <param name="submode">The submode, or "" where the record carries none.</param>
    /// <returns>The cell.</returns>
    /// <remarks>
    /// **A MIDDLE DOT AND NOT A SLASH.** `MFSK/FT4` reads as one token and is what a
    /// reader would take for a mode name; the dot is the separator this application
    /// already uses between two facts on one line, as the waterfall summary does.
    /// </remarks>
    private static string WithSubmode(string mode, string submode)
        => submode.Length == 0 ? mode : mode + " · " + submode;

    /// <summary>What the row says about a record that could not be read whole.</summary>
    /// <remarks>
    /// **IT IS SHOWN, NOT SKIPPED.** A log that quietly drops a record is worse
    /// than one that shows a bad one: the operator can see a damaged line and go
    /// and look, and he cannot see a line that is not there.
    /// </remarks>
    private static string Damage(AdifLogRecord record)
        => record.Faults.Count == 0 ? "" : string.Join(" ", record.Faults);
}

/// <summary>**His own contacts, newest first, read and never written.**</summary>
/// <remarks>
/// <para>**IT READS. IT NEVER WRITES** (work instruction 278). Nothing here edits,
/// deletes, reorders or repairs a record. **A log record is a statement the
/// operator made**, and a view is not the place to revise one.</para>
/// <para>**THE COUNT IS THE NUMBER OF RECORDS AND NOTHING ELSE** (Tim's ruling,
/// 2026-09-08). Not distinct callsigns, not confirmed contacts, not an estimate.
/// Work the same station on three bands and that is three, because that is what he
/// ruled and because a number a person repeats to other people has to be the true
/// one.</para>
/// </remarks>
public sealed class ContactLogViewModel
{
    /// <summary>Build the view over a file's records.</summary>
    /// <param name="records">Every record the reader found, faults and all.</param>
    /// <param name="path">Where the file is, for the operator to go and look.</param>
    public ContactLogViewModel(IReadOnlyList<AdifLogRecord> records, string path)
    {
        LogPath = path;

        // **NEWEST FIRST, AND A RECORD WITH NO TIME SORTS OLDEST.** A record whose
        // date could not be read has no place in a timeline, and putting it at the
        // top would give it a prominence the file does not support.
        foreach (var record in records
            .Select(r => new ContactLogRow(r))
            .OrderByDescending(r => r.StartedUtc ?? System.DateTime.MinValue))
        {
            Contacts.Add(record);
        }

        Count = records.Count;
        Damaged = records.Count(r => !r.IsSound);
    }

    /// <summary>The rows, newest first.</summary>
    public ObservableCollection<ContactLogRow> Contacts { get; } = new();

    /// <summary>How many records are in the file.</summary>
    public int Count { get; }

    /// <summary>How many of them could not be read whole.</summary>
    public int Damaged { get; }

    /// <summary>Where the file is.</summary>
    public string LogPath { get; }

    /// <summary>True where there is anything to show.</summary>
    public bool HasContacts => Contacts.Count > 0;

    /// <summary>What the window says when the log is empty.</summary>
    /// <remarks>
    /// **IT SAYS SO IN HIS WORDS** (§0.7), the way the For you panel does. An empty
    /// table with headings over it looks like something that failed to load, and
    /// the true answer is friendlier than that and teaches what would change it.
    /// </remarks>
    public string Idle
        => "Nothing here yet. Hamlet writes a line into this log every time you "
            + "fill in the contact dialog after working somebody, and this window "
            + "is where those lines come back. The file lives at " + LogPath + ".";

    /// <summary>The count, as a sentence.</summary>
    public string CountLine
        => Count == 1
            ? "1 contact logged."
            : Count.ToString("N0", CultureInfo.InvariantCulture) + " contacts logged.";

    /// <summary>What the window says about damage, or "".</summary>
    /// <remarks>
    /// **NAMED AT THE TOP AS WELL AS ON THE ROW.** A damaged record five hundred
    /// rows down is one he will never scroll to, and the count of them is the thing
    /// that would send him looking.
    /// </remarks>
    public string DamageLine
        => Damaged == 0
            ? ""
            : Damaged == 1
                ? "One record could not be read whole. It is still listed, marked, "
                    + "rather than left out."
                : Damaged.ToString("N0", CultureInfo.InvariantCulture)
                    + " records could not be read whole. They are still listed, "
                    + "marked, rather than left out.";

    /// <summary>Where he stands against the badges.</summary>
    /// <remarks>
    /// **DERIVED FROM THE COUNT EVERY TIME.** A badge says the log holds a certain
    /// number of contacts, so it cannot be remembered past the records behind it.
    /// </remarks>
    public ContactMilestones Milestones => new(Count);

    /// <summary>True where there is damage to mention.</summary>
    public bool HasDamage => Damaged > 0;

}
