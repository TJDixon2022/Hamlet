using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>One field of a log entry, as the dialog shows it.</summary>
/// <param name="Label">What the field is called on screen.</param>
/// <param name="Value">What Hamlet observed, or "".</param>
/// <param name="AdifField">The ADIF tag it will be written under.</param>
/// <param name="WhenAbsent">
/// What to say where nothing was observed, or "" for the general answer.
/// **Some fields are not things a station sends**: *Hamlet did not hear this* is
/// right about a report and wrong about a dial, so a field that needs its own
/// sentence carries one.
/// </param>
/// <remarks>
/// **THE ADIF NAME IS ON SCREEN BESIDE THE VALUE**, because this is the operator's
/// own record and a year from now *what did Hamlet call this* should not need a
/// source read. It is also how he checks the file against another logger.
/// </remarks>
public sealed record LogField(
    string Label, string Value, string AdifField, string WhenAbsent = "")
{
    /// <summary>The report he may correct, on the two RST rows; null on every other row.</summary>
    public LogReport? Report { get; init; }

    /// <summary>True on a row that is a text box rather than a reading.</summary>
    public bool IsReport => Report is not null;

    /// <summary>True on a row that is a reading and cannot be typed over.</summary>
    public bool IsReading => Report is null;

    /// <summary>True where Hamlet observed this and did not have to be told.</summary>
    /// <remarks>
    /// **OBSERVED IS A FACT ABOUT WHERE THE VALUE CAME FROM, NOT ABOUT WHETHER IT
    /// IS PRESENT.** Settings marks the grid `verified` for the same reason: a year
    /// from now he has to be able to tell what the radio heard from what he wrote.
    /// </remarks>
    public bool Observed => Value.Length > 0;

    /// <summary>What the dialog says where nothing was observed.</summary>
    /// <remarks>
    /// **IT SAYS WHY, ON ITS FACE** (the instruction's own rule). An empty box
    /// beside a label reads as a box he forgot to fill; this says Hamlet did not
    /// hear it, which is a different fact and the true one.
    /// </remarks>
    public string Shown
        => Value.Length > 0
            ? Value
            : WhenAbsent.Length > 0 ? WhenAbsent : "Hamlet did not hear this";
}

/// <summary>
/// One report on the Log dialog that the operator may correct - RST sent or RST received
/// (criterion 9.3, Tim's ruling A of 2026-09-22).
/// </summary>
/// <remarks>
/// <para>**HEARD AND YOURS ARE TOLD APART, AND A VALUE HAMLET DID NOT HEAR IS NEVER MARKED
/// HEARD** (§0.0). The box starts holding what the exchange carried, marked *heard by Hamlet*;
/// the moment what is in it differs from that, it is his and is marked *yours*. A contact with no
/// heard report starts blank, and anything typed into it is *yours*.</para>
/// <para>**`Source` IS A STABLE TOKEN FOR THE RECORD** (HM-DEC-077): `heard`, `yours`, or null
/// where the box is empty and the field is left out of the file.</para>
/// </remarks>
public sealed partial class LogReport : ObservableObject
{
    /// <summary>The mark on a value the exchange carried.</summary>
    public const string HeardMark = "heard by Hamlet";

    /// <summary>The mark on a value he typed.</summary>
    public const string YoursMark = "yours - typed by you";

    /// <summary>The mark on an empty box with nothing heard.</summary>
    public const string EmptyMark = "Hamlet did not hear this - type it if you have it";

    /// <summary>Creates one report box.</summary>
    /// <param name="heard">What the exchange carried, or "".</param>
    public LogReport(string heard)
    {
        Heard = heard.Trim();
        _text = Heard;
    }

    /// <summary>What the exchange carried, or "".</summary>
    public string Heard { get; }

    /// <summary>What is in the box.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mark), nameof(Source), nameof(IsYours))]
    private string _text;

    /// <summary>The value the entry will carry, or null where the box is empty.</summary>
    public string? Value => Text.Trim().Length > 0 ? Text.Trim() : null;

    /// <summary>`heard`, `yours`, or null where the box is empty.</summary>
    public string? Source => Value is null
        ? null
        : Heard.Length > 0 && string.Equals(Value, Heard, StringComparison.Ordinal) ? "heard" : "yours";

    /// <summary>True where what is in the box is his.</summary>
    public bool IsYours => Source == "yours";

    /// <summary>What the dialog says under the box.</summary>
    public string Mark => Source switch
    {
        "heard" => HeardMark,
        "yours" => YoursMark,
        _ => Heard.Length > 0 ? "empty - left out of the file" : EmptyMark,
    };
}

/// <summary>
/// The Log dialog: what Hamlet observed, and a place for what the operator wants
/// to add.
/// </summary>
/// <remarks>
/// <para>**HAMLET POPULATES WHAT IT CAN AND THE OPERATOR ADDS NOTES** (Tim's
/// ruling, 2026-09-07). **Nothing he types overwrites anything the radio heard**:
/// the observed fields are not editable here at all, and his words go in
/// `COMMENT` and nowhere else. **The two reports are the one exception since work
/// instruction 390** (Tim's ruling A, 2026-09-22, criterion 9.3): each is a
/// <see cref="LogReport"/> box, and what he types there is marked *yours* and never
/// *heard*.</para>
/// <para>**A FIELD HAMLET COULD NOT OBSERVE IS EMPTY AND SAYS SO** rather than
/// carrying something plausible. A log outlives everything else in this project
/// and there is nothing later that could tell a guessed report from a heard one
/// (§0.0).</para>
/// <para>**LOGGING TRANSMITS NOTHING.** Nothing in this class reaches the send
/// path, and there is no route through it into one.</para>
/// </remarks>
public sealed partial class LogContactViewModel : ObservableObject
{
    private readonly AdifContact _observed;

    /// <summary>Creates the dialog's model.</summary>
    /// <param name="observed">What the ledger and the radio supplied.</param>
    /// <param name="frequencySaidPlainly">
    /// How the frequency is described on screen, or "" where none is known.
    /// </param>
    /// <exception cref="ArgumentNullException">The contact is null.</exception>
    public LogContactViewModel(AdifContact observed, string frequencySaidPlainly = "")
    {
        ArgumentNullException.ThrowIfNull(observed);

        _observed = observed;

        var fields = new List<LogField>
        {
            new("Station", observed.Call ?? "", "CALL"),
            new("Their grid", observed.GridSquare ?? "", "GRIDSQUARE"),
            // **THE REPORT THE MODE ACTUALLY EXCHANGED** (§3.2, work instruction 326
            // task 3). PSK31 exchanges an RST and FT8 a ratio in decibels, and the
            // two are kept apart in the record; exactly one is set on any one
            // contact. **Showing the wrong one would be worse than a blank box**:
            // `Summary` counts an empty field as something Hamlet did not hear, so a
            // report it read and is about to write would be reported as missed.
            //
            // **AND SINCE WORK INSTRUCTION 390 THE TWO REPORTS ARE HIS TO CORRECT** (Tim's
            // ruling A, 2026-09-22, criterion 9.3): each is a box holding what was exchanged,
            // marked *heard*, and anything he types in its place is marked *yours*. This
            // supersedes the 2026-09-07 read-only line for these two rows only.
            ReportRow("Report you sent", observed.RstSent ?? observed.ReportSent ?? "", "RST_SENT"),
            ReportRow("Report they sent", observed.RstReceived ?? observed.ReportReceived ?? "", "RST_RCVD"),
            new("Started", Moment(observed.StartedUtc), "QSO_DATE, TIME_ON"),
            new("Ended", Moment(observed.EndedUtc), "TIME_OFF"),
            new("Band", observed.Band ?? "", "BAND"),
            // **THE FREQUENCY SAYS WHAT IT IS RATHER THAN WHAT IT WAS HEARD
            // AS.** *Hamlet did not hear this* is right about a report and wrong
            // about a dial: a dial is not something a station sends. Rows decoded
            // before work instruction 275 carry none, and the record leaves both
            // `FREQ` and `BAND` out entirely rather than guessing.
            new("Frequency", frequencySaidPlainly, "FREQ",
                "Hamlet did not record the dial for this row, so the frequency "
                + "and the band are left out of the entry."),
            new("Mode", observed.Mode ?? "", "MODE"),
        };

        // **THE SUBMODE ROW IS THERE WHERE THERE IS A SUBMODE, AND NOT OTHERWISE**
        // (work instruction 292 task 6).
        //
        // **AN FT8 CONTACT'S SUBMODE IS NOT UNOBSERVED - IT DOES NOT EXIST.** FT8 is an
        // ADIF mode in its own right and unit 291 proved the record it writes carries
        // no `SUBMODE` tag at all. An always-present row would show `Hamlet did not hear
        // this` beside `SUBMODE`, which says Hamlet failed at something it did
        // perfectly, and `Summary` would count it: *Hamlet heard 11 of these 12*, about
        // a contact where it heard everything there was. That is a claim nobody
        // measured, in the dialog that decides what goes into the permanent record
        // (§0.0).
        //
        // **SO ABSENT IS ABSENT ON SCREEN EXACTLY AS IT IS IN THE FILE.** No row, no
        // empty box, no count. `TheLogDialogShowsTheSubmodeTests` asserts the FT8 case
        // and not only the FT4 one, because the FT8 case is the one that could go
        // quietly wrong.
        if (!string.IsNullOrWhiteSpace(observed.Submode))
        {
            fields.Add(new("Submode", observed.Submode!.Trim(), "SUBMODE"));
        }

        fields.Add(new("Your callsign", observed.StationCallsign ?? "", "STATION_CALLSIGN"));
        fields.Add(new("Your grid", observed.MyGridSquare ?? "", "MY_GRIDSQUARE"));

        Fields = new ObservableCollection<LogField>(fields);
    }

    /// <summary>What Hamlet observed, in the order a person reads it.</summary>
    public ObservableCollection<LogField> Fields { get; }

    /// <summary>How many of the fields Hamlet actually heard.</summary>
    public int ObservedCount => Fields.Count(f => f.Observed);

    /// <summary>What the dialog says about how complete the entry is.</summary>
    /// <remarks>
    /// **IT COUNTS AND IT DOES NOT JUDGE.** An incomplete entry is still a contact
    /// worth logging; this says what is there so he can decide, and never that the
    /// contact was not good enough.
    /// </remarks>
    public string Summary
        => ObservedCount == Fields.Count
            ? $"Hamlet heard all {Fields.Count} of these."
            : $"Hamlet heard {ObservedCount} of these {Fields.Count}. The rest are "
              + "empty because it did not hear them, and they will be left out of "
              + "the file rather than written blank.";

    /// <summary>Where the RST sent in the entry came from: `heard`, `yours`, or null.</summary>
    public string? SentSource => ReportFor("RST_SENT")?.Source;

    /// <summary>Where the RST received in the entry came from: `heard`, `yours`, or null.</summary>
    public string? ReceivedSource => ReportFor("RST_RCVD")?.Source;

    private LogReport? ReportFor(string adif)
        => Fields.FirstOrDefault(f => f.AdifField == adif)?.Report;

    /// <summary>What the operator typed. His, and nothing else touches it.</summary>
    [ObservableProperty]
    private string _notes = "";

    /// <summary>True once Save has been pressed.</summary>
    /// <remarks>
    /// **CANCEL WRITES NOTHING**, and the way that is guaranteed is that the
    /// caller writes only when this is true. The dialog itself never touches a
    /// file.
    /// </remarks>
    public bool Saved { get; private set; }

    /// <summary>The entry as it would be written, with his notes on it.</summary>
    /// <remarks>
    /// **THE NOTES ARE ADDED AND NOTHING OBSERVED IS REPLACED.** This is a `with`
    /// over the record the ledger produced, touching one field.
    /// </remarks>
    /// <remarks>
    /// **AND THE TWO REPORTS CARRY THE VALUE IN THE BOX** (criterion 9.3). Where the box still
    /// holds what was heard the field is left exactly as the ledger made it; where he changed it,
    /// his value goes in the field the report was already in - the RST for a keyboard mode, the
    /// decibels for FT8 and FT4 - and an emptied box leaves the field out of the file.
    /// </remarks>
    public AdifContact Entry
        => WithReports(_observed with
        {
            Comment = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
        });

    private AdifContact WithReports(AdifContact entry)
    {
        var sent = ReportFor("RST_SENT");
        var received = ReportFor("RST_RCVD");
        var rst = UsesRst(_observed);

        if (sent is not null && sent.Value != NullIfEmpty(sent.Heard))
        {
            entry = rst
                ? entry with { RstSent = sent.Value, ReportSent = null }
                : entry with { ReportSent = sent.Value, RstSent = null };
        }

        if (received is not null && received.Value != NullIfEmpty(received.Heard))
        {
            entry = rst
                ? entry with { RstReceived = received.Value, ReportReceived = null }
                : entry with { ReportReceived = received.Value, RstReceived = null };
        }

        return entry;
    }

    /// <summary>Whether this contact's reports are RSTs rather than decibels.</summary>
    /// <remarks>
    /// The field a report is already in decides it; with neither heard, the mode does - PSK31 and
    /// Olivia exchange an RST, through `ContactModes`, the table that owns the pair.
    /// </remarks>
    private static bool UsesRst(AdifContact contact)
    {
        if (contact.RstSent is not null || contact.RstReceived is not null)
        {
            return true;
        }

        if (contact.ReportSent is not null || contact.ReportReceived is not null)
        {
            return false;
        }

        return (ContactModes.Named("PSK31") is { } psk31 && psk31.Matches(contact.Mode, contact.Submode))
               || ContactModes.Olivia(null).Matches(contact.Mode, contact.Submode);
    }

    private static string? NullIfEmpty(string value) => value.Length > 0 ? value : null;

    private static LogField ReportRow(string label, string heard, string adif)
        => new(label, heard, adif) { Report = new LogReport(heard) };

    /// <summary>Accept the entry.</summary>
    [RelayCommand]
    private void Save() => Saved = true;

    /// <summary>A moment as the dialog shows it, or "".</summary>
    private static string Moment(DateTime? utc)
        => utc is { } at
            ? at.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
              + " UTC"
            : "";
}
