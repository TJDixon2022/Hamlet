using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.App.ViewModels;

/// <summary>One line of the Digital tab's decoded table.</summary>
/// <param name="Utc">When the slot opened, as `hhmmss`.</param>
/// <param name="Snr">
/// The signal-to-noise ratio, **or <see cref="NoMeasurement"/>, which is what it
/// is today**. See the remarks.
/// </param>
/// <param name="Dt">How far into the slot the transmission started, in seconds.</param>
/// <param name="Hz">The lowest of the eight tones, in the audio passband.</param>
/// <param name="Message">The text, exactly as it was sent.</param>
/// <remarks>
/// <para>**FORMATTED HERE RATHER THAN IN THE MARKUP**, so what a reader sees can
/// be asserted by a test that never opens a window.</para>
/// <para>**THE SNR COLUMN CARRIES A DASH AND NOT A NUMBER** (§0.0). The columns
/// were committed in work instruction 037 on the assumption that `snr` is what
/// the decoder produces; it is not. `Ft8Sharp` returns a Costas sync score — how
/// far the expected tone stood above the average of the eight — which is not
/// decibels, is not calibrated against anything, and would be read as a
/// measurement the moment it appeared under that heading. **A dash says nothing
/// was measured, which is true.** What to put there instead is Tim's under
/// §12.1 and is raised rather than decided.</para>
/// </remarks>
public sealed record DigitalDecodeRow(
    string Utc, string Snr, string Dt, string Hz, string Message)
{
    /// <summary>What the `snr` cell says while nothing measures one.</summary>
    public const string NoMeasurement = "—";

    /// <summary>The message split into its three fields, or null.</summary>
    /// <remarks>
    /// **NULL FOR ANYTHING THAT IS NOT PLAINLY THREE FIELDS.** Free text,
    /// telemetry and non-standard callsign forms are drawn as they arrived, with
    /// no colouring and no field tooltips, because labelling the wrong half of a
    /// message is worse than labelling none of it.
    /// </remarks>
    public Ft8MessageFields? Fields => Ft8Vocabulary.Split(Message);

    /// <summary>Who the message is addressed to, or "".</summary>
    /// <remarks>
    /// **`Addressee` AND `Sender` RATHER THAN `To` AND `From`.** The record
    /// already has a static `From(Ft8Decode)` factory, and a property of the
    /// same name does not compile. The pair is renamed together so they stay
    /// symmetrical.
    /// </remarks>
    public string Addressee => Fields?.To ?? "";

    /// <summary>Who sent it, or "".</summary>
    public string Sender => Fields?.From ?? "";

    /// <summary>The payload, or "".</summary>
    public string Payload => Fields?.Payload ?? "";

    /// <summary>The whole message, shown only where it has no three parts.</summary>
    /// <remarks>
    /// **THE TWO ARE EXCLUSIVE**, so a message never appears twice: either the
    /// three coloured fields are shown, or this is.
    /// </remarks>
    public string Unsplit => Fields is null ? Message : "";

    /// <summary>True where the message has three fields to colour.</summary>
    public bool HasFields => Fields is not null;

    /// <summary>Hover text naming the addressee field.</summary>
    /// <remarks>
    /// **STRUCTURE, NOT MEANING.** Saying which field is the addressee is a fact
    /// about the message format. It does not say who the station is, where they
    /// are, or why they are calling.
    /// </remarks>
    public string AddresseeHelp
        => string.Equals(Addressee, "CQ", StringComparison.OrdinalIgnoreCase)
            || Addressee.StartsWith("CQ ", StringComparison.OrdinalIgnoreCase)
            ? "Who this is addressed to. CQ means anyone."
            : "Who this is addressed to.";

    /// <summary>Hover text naming the sender field.</summary>
    public string SenderHelp => "Who sent it.";

    /// <summary>
    /// Hover text for the payload, from the closed table, or "" for silence.
    /// </summary>
    /// <remarks>
    /// **AN EMPTY STRING AND NOT A FALLBACK SENTENCE.** Avalonia shows no
    /// tooltip for an empty tip, which is exactly what Tim's ruling asks for:
    /// anything off the list gets nothing, not "unrecognised".
    /// </remarks>
    public string PayloadHelp => Ft8Vocabulary.Explain(Payload) ?? "";

    /// <summary>True where the payload is on the list and has hover text.</summary>
    public bool HasPayloadHelp => PayloadHelp.Length > 0;

    /// <summary>Puts a decode into the table's five columns.</summary>
    /// <param name="decode">What came out of the slot.</param>
    /// <returns>The row.</returns>
    /// <exception cref="ArgumentNullException">The decode is null.</exception>
    public static DigitalDecodeRow From(Ft8Decode decode)
    {
        ArgumentNullException.ThrowIfNull(decode);

        return new DigitalDecodeRow(
            decode.SlotStartUtc.ToString("HHmmss", CultureInfo.InvariantCulture),
            NoMeasurement,
            decode.OffsetSeconds.ToString("0.0", CultureInfo.InvariantCulture),
            decode.FrequencyHz.ToString("0", CultureInfo.InvariantCulture),
            decode.Message);
    }
}
