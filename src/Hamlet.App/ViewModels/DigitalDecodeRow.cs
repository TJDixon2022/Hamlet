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
