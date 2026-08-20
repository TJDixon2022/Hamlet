using System.Globalization;
using System.Text;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One press: the operator heard a station here.</summary>
/// <param name="AtUtc">When he pressed.</param>
/// <param name="FrequencyHz">Where the radio was, from the same reading the
/// sidecar uses.</param>
/// <param name="Band">The band that reading falls in, or why it is unknown.</param>
/// <param name="Wav">The file written, or "" when none was.</param>
/// <param name="Refusal">Why no file was written, or "" when one was.</param>
/// <param name="ToneHz">The tone the decoder had, or null for none.</param>
/// <param name="SnrDb">How far it stood out, or null when unread.</param>
/// <param name="Wpm">The speed being tracked, or null when not tracking.</param>
/// <param name="Emitted">Characters the decoder emitted over <paramref name="Covers"/>.</param>
/// <param name="Unsure">How many of those it was unsure of.</param>
/// <param name="Covers">
/// What the two counts are counts of. **`Recording` is the only one of these
/// that answers the question the roster is scored on** (HM-DEC-091), and it is
/// not the default: a row built without saying so gets the weaker claim and says
/// on its face that the numbers are the evening's rather than this station's.
/// </param>
/// <param name="Text">
/// What the decoder had actually read at the moment of the press, or "" when it
/// had read nothing.
/// </param>
/// <param name="Meter">
/// What the keying meter said at the moment of the press, or "" when there was
/// nothing to say. **This is the column that makes an evening worth something
/// even when the fault does not fall out** (HM-DEC-091): a row where the operator
/// heard a station and the meter heard no keying says the signal was lost before
/// the decoder ever saw it, and no row has ever been able to say that.
/// </param>
public sealed record CwCase(
    DateTime AtUtc,
    long FrequencyHz,
    string Band,
    string Wav,
    string Refusal,
    double? ToneHz,
    double? SnrDb,
    int? Wpm,
    int Emitted,
    int Unsure,
    string Text = "",
    CwCountsCover Covers = CwCountsCover.Session,
    string Meter = "");

/// <summary>What a pair of counts on a roster row is counting.</summary>
public enum CwCountsCover
{
    /// <summary>
    /// Everything since the decoder started listening, which may be hours and
    /// several bands. Honest, and not an answer about this case.
    /// </summary>
    Session,

    /// <summary>The audio in the recording named on this row, and nothing else.</summary>
    Recording,

    /// <summary>There is no recording, because no file was written.</summary>
    NoRecording,
}

/// <summary>
/// The roster of cases: one row per press, appended as it happens.
/// </summary>
/// <remarks>
/// <para>**THERE IS NO NUMBER FOR THE THING THE OPERATOR IS TRYING TO IMPROVE.**
/// Every figure this project has produced counts characters against an answer key
/// on one capture. What he wants is cases: a station he hears on the air, which
/// succeeds if Hamlet produced text he could read. **Nothing in the application
/// counts that and nothing can derive it**, because a decoder that misses a
/// station also misses the case and would score itself a hundred per cent.</para>
/// <para>**SO THE DENOMINATOR IS HIS PRESS**, which sits outside the system being
/// measured. One press marks the case and keeps the audio, and the roster is the
/// list of cases with everything Hamlet knew at that moment beside each one.</para>
/// <para>**THE `read` COLUMN IS HIS AND NOTHING WRITES TO IT.** Not derived from
/// the character count, not defaulted, not pre-filled with a guess. A threshold
/// standing in for a judgement is the error class this project tabulated five
/// times in one week, and a column whose whole purpose is a human verdict is where
/// it would be easiest to make it a sixth.</para>
/// <para>Append-only, one file per evening, tab-separated so a person can read it
/// in a text editor and a scorer can split it on tabs.</para>
/// </remarks>
public static class CwCaseRoster
{
    /// <summary>The header every roster file opens with.</summary>
    public static string Header { get; } = string.Join(
        '\t',
        "time",
        "frequency",
        "band",
        "wav",
        "toneHz",
        "snrDb",
        "wpm",
        "chars",
        "meter",
        "text",
        "read");

    /// <summary>What a roster file is called for a given evening.</summary>
    /// <param name="atUtc">Any moment in it, UTC.</param>
    /// <param name="zone">The shack's own clock; the machine's when omitted.</param>
    /// <returns>The file name.</returns>
    /// <remarks>
    /// <para>**THE NAME TAKES THE LOCAL DATE, AND THAT IS THE WHOLE POINT.** An
    /// evening at the rig in Pennsylvania begins around eight and crosses midnight
    /// UTC four hours later, so a file named for the UTC date **splits one evening
    /// in two** and hides the second half in a file named for tomorrow. Scoring
    /// the first file and taking its count would report a percentage whose
    /// denominator had quietly lost part of itself, which is worse than no measure
    /// at all because it comes with confidence attached (§0.0).</para>
    /// <para>**THE ROWS INSIDE STAY UTC** and nothing here converts them. The two
    /// clocks are deliberate: the name answers "which evening was this", which is a
    /// question about a person, and the rows answer "when was this on the air",
    /// which is not. **The file says which is which on its own first line**
    /// (HM-DEC-091), because two clocks on one sheet is exactly the shape of fault
    /// that ruling exists for and a convention held only in somebody's memory is
    /// not a source.</para>
    /// </remarks>
    public static string FileName(DateTime atUtc, TimeZoneInfo? zone = null)
        => $"cases-{Local(atUtc, zone):yyyy-MM-dd}.txt";

    /// <summary>The line each roster opens with, above the column header.</summary>
    /// <param name="atUtc">Any moment in the evening, UTC.</param>
    /// <param name="zone">The shack's own clock; the machine's when omitted.</param>
    /// <returns>The line, without its terminator.</returns>
    /// <remarks>
    /// **FOR SOMEBODY READING THIS FILE COLD IN SIX MONTHS.** It names the evening
    /// in the clock the file is named for, says in the same breath that the times
    /// below are in the other one, and gives the offset between them **as it stood
    /// that night**, so a reader in a different part of the year is not left to
    /// work out whether daylight saving was in force. It opens with a `#` so a
    /// scorer can tell a note from a row without knowing what the note says.
    /// </remarks>
    public static string Evening(DateTime atUtc, TimeZoneInfo? zone = null)
    {
        var here = zone ?? TimeZoneInfo.Local;
        var utc = DateTime.SpecifyKind(atUtc, DateTimeKind.Utc);
        var offset = here.GetUtcOffset(utc);
        var sign = offset < TimeSpan.Zero ? "-" : "+";

        return string.Concat(
            "# Evening of ",
            Local(atUtc, here).ToString("dddd d MMMM yyyy", CultureInfo.InvariantCulture),
            " at the rig, local time UTC",
            sign,
            offset.Duration().ToString(@"hh\:mm", CultureInfo.InvariantCulture),
            ". Every time below is UTC.");
    }

    private static DateTime Local(DateTime atUtc, TimeZoneInfo? zone)
        => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(atUtc, DateTimeKind.Utc),
            zone ?? TimeZoneInfo.Local);

    /// <summary>One row, as it is written.</summary>
    /// <param name="one">The case.</param>
    /// <returns>The line, without its terminator.</returns>
    /// <remarks>
    /// **EVERY COLUMN COMES FROM SOMETHING ALREADY MEASURED, OR SAYS IT DOES NOT**
    /// (HM-DEC-091). `none`, `unread` and `not tracking` are real answers and a
    /// plausible number is not, so nothing here substitutes a default for a
    /// reading the decoder did not have.
    /// </remarks>
    public static string Row(CwCase one)
    {
        ArgumentNullException.ThrowIfNull(one);

        // **A REFUSED WRITE IS STILL A CASE** (HM-DEC-090). The guard that
        // declines to write a duplicate recording is right and is not weakened
        // here; what changes is that its refusal becomes a row with a reason
        // rather than a status line nobody keeps. A case with no evidence is part
        // of the denominator, and omitting it would flatter the score.
        var wav = one.Wav.Length > 0
            ? one.Wav
            : $"none ({(one.Refusal.Length > 0 ? one.Refusal : "no reason given")})";

        return string.Join(
            '\t',
            one.AtUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            (one.FrequencyHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture),
            one.Band,
            wav,
            one.ToneHz is { } tone
                ? tone.ToString("0", CultureInfo.InvariantCulture)
                : "none",
            one.SnrDb is { } snr && !double.IsNaN(snr)
                ? snr.ToString("0.0", CultureInfo.InvariantCulture)
                : "unread",
            one.Wpm is { } wpm && wpm > 0
                ? wpm.ToString(CultureInfo.InvariantCulture)
                : "not tracking",
            // **A COUNT SAYS WHAT IT IS A COUNT OF** (HM-DEC-091). This column
            // held the decoder's running totals, which start when listening
            // starts and stop when it stops: a press seven hours into an evening
            // put a character count earned hours earlier on another band into a
            // row about a station heard just now. The number was never wrong; the
            // column was, because a figure beside a recording is read as being
            // about the recording, and the percentage gets computed from it
            // either way.
            one.Covers switch
            {
                CwCountsCover.Recording
                    => $"{one.Emitted} emitted, {one.Unsure} unsure",
                CwCountsCover.NoRecording
                    => $"{one.Emitted} emitted, {one.Unsure} unsure "
                       + "(the whole session; no recording was kept)",
                _ => $"{one.Emitted} emitted, {one.Unsure} unsure "
                     + "(the whole session, not this case)",
            },

            // **A COUNT IS A POINTER TO EVIDENCE; THE TEXT IS EVIDENCE.** With
            // `19 emitted, 6 unsure` and nothing else, scoring a case means
            // opening the recording and listening to it, and thirty cases is an
            // evening that does not get spent. With what the decoder read sitting
            // in the row, most cases are decided by reading the file and the audio
            // is only needed for the ambiguous ones.
            //
            // **WHETHER HAMLET COULD HEAR KEYING AT ALL**, which is a different
            // question from whether it read anything and has never been on a row.
            // The pair that matters is a case the operator marked and a meter that
            // found nothing being keyed: that is the signal going missing before
            // the decoder, and it is what an evening of these rows is for.
            Readable(one.Meter, "not measured"),

            // **AND HAVING READ NOTHING IS THE MOST IMPORTANT ROW ON THE SHEET**
            // (HM-DEC-091), so it says so in words rather than leaving a cell that
            // looks like a column somebody forgot to fill in.
            Readable(one.Text),

            // **LEFT EMPTY, AND THAT IS THE POINT.** Tim fills it in afterwards
            // from the roster and the audio. Nothing in this file may ever put a
            // value here — and a column of decoded text sitting beside it makes
            // deriving a verdict newly tempting, which is exactly why it does not
            // happen.
            string.Empty);
    }

    /// <summary>What Hamlet had read, as one line of a tab-separated file.</summary>
    /// <param name="text">The transcript's tail, or "".</param>
    /// <returns>The cell.</returns>
    /// <remarks>
    /// A tab or a newline in this cell would split the row and put the operator's
    /// own `read` column under a different heading, so both become a single space.
    /// The decoder can emit neither, but the transcript carries word gaps as
    /// spaces and a future change to either is one line away from breaking the
    /// file everything tomorrow is scored from.
    /// </remarks>
    public static string Readable(string? text) => Readable(text, "nothing read");

    /// <summary>One cell of a tab-separated file, with its own empty answer.</summary>
    /// <param name="text">The value, or "".</param>
    /// <param name="whenEmpty">
    /// What an empty one says. **An empty cell is not a real answer** and reads as
    /// a column somebody forgot to fill in, so every column says its own version
    /// in words: a transcript says nothing was read and a meter says it was not
    /// measured, which are different facts (§0.0).
    /// </param>
    /// <returns>The cell.</returns>
    public static string Readable(string? text, string whenEmpty)
    {
        var clean = (text ?? string.Empty)
            .Replace('\t', ' ')
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        return clean.Length == 0 ? whenEmpty : clean;
    }

    /// <summary>Append one case to the evening's roster, creating it if new.</summary>
    /// <param name="folder">Where captures are kept.</param>
    /// <param name="one">The case.</param>
    /// <param name="zone">The shack's own clock; the machine's when omitted.</param>
    /// <returns>The path written to.</returns>
    /// <remarks>
    /// Never throws: a roster that takes the application down with it is worse
    /// than one that misses a row (§8), and the press has already kept the audio
    /// by the time this runs.
    /// </remarks>
    public static string Append(string folder, CwCase one, TimeZoneInfo? zone = null)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(one);

        var path = Path.Combine(folder, FileName(one.AtUtc, zone));

        try
        {
            Directory.CreateDirectory(folder);

            var fresh = !File.Exists(path);
            var text = new StringBuilder();

            if (fresh)
            {
                text.AppendLine(Evening(one.AtUtc, zone));
                text.AppendLine(Header);
            }

            text.AppendLine(Row(one));

            File.AppendAllText(path, text.ToString());
        }
        catch (Exception)
        {
            // A row that cannot be written loses one case and nothing else
            // (§8: never-throw discipline).
        }

        return path;
    }
}
