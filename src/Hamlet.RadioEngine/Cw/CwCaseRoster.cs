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
/// <param name="Emitted">Characters the decoder emitted this session.</param>
/// <param name="Unsure">How many of those it was unsure of.</param>
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
    int Unsure);

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
        "read");

    /// <summary>What a roster file is called for a given evening.</summary>
    /// <param name="atUtc">Any moment in it.</param>
    /// <returns>The file name.</returns>
    public static string FileName(DateTime atUtc)
        => $"cases-{atUtc:yyyy-MM-dd}.txt";

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
            $"{one.Emitted} emitted, {one.Unsure} unsure",

            // **LEFT EMPTY, AND THAT IS THE POINT.** Tim fills it in afterwards
            // from the roster and the audio. Nothing in this file may ever put a
            // value here.
            string.Empty);
    }

    /// <summary>Append one case to the evening's roster, creating it if new.</summary>
    /// <param name="folder">Where captures are kept.</param>
    /// <param name="one">The case.</param>
    /// <returns>The path written to.</returns>
    /// <remarks>
    /// Never throws: a roster that takes the application down with it is worse
    /// than one that misses a row (§8), and the press has already kept the audio
    /// by the time this runs.
    /// </remarks>
    public static string Append(string folder, CwCase one)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(one);

        var path = Path.Combine(folder, FileName(one.AtUtc));

        try
        {
            Directory.CreateDirectory(folder);

            var fresh = !File.Exists(path);
            var text = new StringBuilder();

            if (fresh)
            {
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
