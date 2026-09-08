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
        Mode = Or(c.Mode);
        ReportSent = Or(c.ReportSent);
        ReportReceived = Or(c.ReportReceived);
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

    /// <summary>The mode, or that it was not recorded.</summary>
    public string Mode { get; }

    /// <summary>The report he sent, or that it was not recorded.</summary>
    public string ReportSent { get; }

    /// <summary>The report he received, or that it was not recorded.</summary>
    public string ReportReceived { get; }

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
            : Count.ToString(CultureInfo.InvariantCulture) + " contacts logged.";

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
                : Damaged.ToString(CultureInfo.InvariantCulture)
                    + " records could not be read whole. They are still listed, "
                    + "marked, rather than left out.";

    /// <summary>True where there is damage to mention.</summary>
    public bool HasDamage => Damaged > 0;

}
