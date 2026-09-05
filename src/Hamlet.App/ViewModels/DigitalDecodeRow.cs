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
/// <para>**THE SNR COLUMN CARRIED A DASH FROM WORK INSTRUCTION 037 UNTIL UNIT
/// 251** (§0.0). The columns were committed on the assumption that `snr` is what
/// the decoder produces; it is not. `Ft8Sharp` returns a Costas sync score — how
/// far the expected tone stood above the average of the eight — which is not
/// decibels, is not calibrated against anything, and would have been read as a
/// measurement the moment it appeared under that heading. A dash said nothing
/// was measured, which was true.</para>
/// <para>**SINCE UNIT 251 SOMETHING MEASURES ONE.**
/// `Ft8Sharp.Deep.Ft8DeepSignalToNoise` reads the power in the tone that was
/// transmitted against the seven tones that were not, at the same instant, and
/// carries the per-bin ratio to the 2500 Hz reference bandwidth by a derived
/// 26.0206 dB. Over 510 synthesized messages, five rungs and two placements, it
/// agreed with the ratio actually delivered to a **mean absolute error of
/// 0.26 dB and a 95th percentile of 0.62 dB** — `docs/unit251-snr-trace.md` §6.
/// `PHASE_PLAN.md` step 2 says the column keeps its dash if agreement is worse
/// than 2 dB. **It is not, so the column shows a number.**</para>
/// <para>**AND THE DASH DID NOT GO AWAY** — it moved. A message whose ratio
/// could not be measured still shows <see cref="NoMeasurement"/>, because a
/// floored or guessed decibel figure is indistinguishable on the screen from a
/// measured weak one, which is the fault §0.0 exists for.</para>
/// <para>**WHOLE DECIBELS, AND THE REASON IS THE 95th AND NOT THE MEAN.** A
/// tenth of a decibel in this column would say the difference between a station
/// at -13.2 and one at -13.4 means something. It does not: one message in twenty
/// is 0.62 dB or further out, so the first decimal is noise being drawn as
/// signal. Whole decibels is the coarsest unit that still separates the stations
/// an operator has to choose between, it is what this mode is quoted in
/// everywhere else, and it fits the 48-pixel monospace column with room for the
/// sign.</para>
/// </remarks>
public sealed record DigitalDecodeRow(
    string Utc, string Snr, string Dt, string Hz, string Message)
{
    /// <summary>What the `snr` cell says for a message whose ratio was not measured.</summary>
    /// <remarks>
    /// **NOT "while nothing measures one" ANY MORE.** Something does, since unit
    /// 251. This is what a message whose symbol sequence could not be recovered,
    /// or whose frame ran off the end of the slot, shows instead of a number.
    /// </remarks>
    public const string NoMeasurement = "—";

    /// <summary>
    /// Puts a measured ratio into the `snr` cell, or <see cref="NoMeasurement"/>.
    /// </summary>
    /// <param name="decibels">The ratio, or null where none was measured.</param>
    /// <returns>Whole decibels with an explicit sign, or the dash.</returns>
    /// <remarks>
    /// <para>**WHOLE DECIBELS, WITH THE SIGN ALWAYS SHOWN.** The remarks on this
    /// record say why the precision stops there. The sign is always drawn
    /// because most FT8 reports are negative and a bare `3` in a column of
    /// `-13`s reads as a missing minus rather than as a strong station.</para>
    /// <para>**AWAY FROM ZERO AT THE HALF**, which is what
    /// <see cref="MidpointRounding.AwayFromZero"/> gives and what
    /// <see cref="Math.Round(double)"/> does not: banker's rounding would send
    /// -13.5 and -14.5 to the same cell and nothing on the screen would say so.</para>
    /// </remarks>
    public static string FormatSnr(double? decibels)
    {
        if (decibels is not { } measured || double.IsNaN(measured) || double.IsInfinity(measured))
        {
            return NoMeasurement;
        }

        var whole = (int)Math.Round(measured, MidpointRounding.AwayFromZero);
        return whole.ToString("+0;-0;+0", CultureInfo.InvariantCulture);
    }

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
