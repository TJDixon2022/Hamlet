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
/// The Log dialog: what Hamlet observed, and a place for what the operator wants
/// to add.
/// </summary>
/// <remarks>
/// <para>**HAMLET POPULATES WHAT IT CAN AND THE OPERATOR ADDS NOTES** (Tim's
/// ruling, 2026-09-07). **Nothing he types overwrites anything the radio heard**:
/// the observed fields are not editable here at all, and his words go in
/// `COMMENT` and nowhere else.</para>
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

        Fields = new ObservableCollection<LogField>(
        [
            new("Station", observed.Call ?? "", "CALL"),
            new("Their grid", observed.GridSquare ?? "", "GRIDSQUARE"),
            new("Report you sent", observed.ReportSent ?? "", "RST_SENT"),
            new("Report they sent", observed.ReportReceived ?? "", "RST_RCVD"),
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
            new("Your callsign", observed.StationCallsign ?? "", "STATION_CALLSIGN"),
            new("Your grid", observed.MyGridSquare ?? "", "MY_GRIDSQUARE"),
        ]);
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
    public AdifContact Entry
        => _observed with
        {
            Comment = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
        };

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
