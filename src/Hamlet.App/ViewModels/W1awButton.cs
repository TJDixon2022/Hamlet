using System.Globalization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.ViewModels;

/// <summary>
/// One W1AW button on the CW tab: tunes to W1AW's Morse frequency on one band, in CW (work
/// instruction 494, HM-DEC-199).
/// </summary>
/// <param name="Label">"W1AW 40 m".</param>
/// <param name="FrequencyHz">W1AW's Morse frequency on that band.</param>
/// <param name="Tip">What the button does, in the app's voice.</param>
/// <param name="Tone">Whether the operator's license covers sending Morse there.</param>
public sealed record W1awButton(string Label, long FrequencyHz, string Tip, PrivilegeTone Tone)
{
    /// <summary>
    /// One button for each of W1AW's frequencies Hamlet can honestly take the dial to, lowest first.
    /// </summary>
    /// <param name="table">W1AW's frequencies.</param>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <returns>The buttons.</returns>
    /// <remarks>
    /// <para>**A FREQUENCY HAMLET DOES NOT KNOW AS AMATEUR SPECTRUM IS NOT OFFERED.** 2 m is past
    /// what the IC-7300 tunes, which is HF and 50 MHz; and 6 m, which the radio does tune, is
    /// absent from the spectrum Hamlet knows, so the card would say "not an amateur band" there,
    /// which is false (§0.0). Both are left out rather than shown as a button that cannot work
    /// or that makes the screen say something untrue.</para>
    /// <para>**EVERY BUTTON OFFERED CAN BE PRESSED.** Listening is never restricted (HM-DEC-029)
    /// and grey is kept for what cannot be used (HM-DEC-087); what a license does not cover is
    /// sending, and the tip says so where it applies.</para>
    /// </remarks>
    public static IReadOnlyList<W1awButton> For(
        W1awMorseFrequencies table, PrivilegePlan plan, LicenseClass licenseClass)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(plan);

        return table.Rows
            .Where(row => AmateurSpectrum.Describe(row.FrequencyHz).IsAmateur)
            .Select(row =>
            {
                var tone = PrivilegeStatusLine.Build(plan, licenseClass, row.FrequencyHz, TransmitMode.Cw).Tone;

                return new W1awButton("W1AW " + row.Band, row.FrequencyHz, TipFor(row.FrequencyHz, tone), tone);
            })
            .ToList();
    }

    private static string TipFor(long hz, PrivilegeTone tone)
    {
        var megahertz = (hz / 1_000_000.0).ToString("0.0000", CultureInfo.InvariantCulture);

        var license = tone switch
        {
            PrivilegeTone.Yours => "Your license covers sending Morse here too.",
            PrivilegeTone.ListenOnly => "Your license does not cover sending Morse here, but listening is never restricted.",
            _ => "Set your license class in Settings to see whether you may send here. Listening is never restricted.",
        };

        return $"Tunes to {megahertz} MHz and sets the radio to CW. W1AW is the ARRL's headquarters station, "
            + "and it sends code practice, slow at 5 to 15 words a minute and fast at 10 to 35, and bulletins "
            + "at 18, on the ARRL's published schedule. The times are the ARRL's and are not shown here, so this "
            + $"says where W1AW sends and not that it is sending now. {license}";
    }
}
