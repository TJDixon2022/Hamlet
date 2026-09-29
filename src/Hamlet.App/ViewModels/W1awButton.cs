using System.Globalization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The one W1AW button on the CW tab, for the band the radio is on: it tunes to W1AW's Morse
/// frequency there, in CW (work instructions 494 and 495, HM-DEC-199).
/// </summary>
/// <param name="Label">"W1AW on 40 m", or what stands in its place where it cannot go.</param>
/// <param name="FrequencyHz">W1AW's Morse frequency on this band, or 0 where there is none to go to.</param>
/// <param name="Tip">What the button does, in the app's voice.</param>
/// <param name="Tone">Whether the operator's license covers sending Morse there.</param>
public sealed record W1awButton(string Label, long FrequencyHz, string Tip, PrivilegeTone Tone)
{
    /// <summary>Whether a press goes anywhere.</summary>
    public bool CanTune => FrequencyHz > 0;

    /// <summary>
    /// The button for where the dial is (work instruction 495). The owner: *"We only need one
    /// button - it turns to the current band."*
    /// </summary>
    /// <param name="table">W1AW's frequencies.</param>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="dialHz">Where the dial is.</param>
    /// <returns>The button.</returns>
    /// <remarks>
    /// <para>**NEVER A FALSE LABEL AND NEVER A PRESS THAT GOES NOWHERE** (§0.0). On a band W1AW
    /// sends Morse on, and that Hamlet knows as amateur spectrum, it names the band and a press
    /// goes there. On an amateur band W1AW does not send Morse on it says so and cannot be pressed.
    /// Off the spectrum Hamlet knows - 6 m among it, where W1AW does send but Hamlet would call the
    /// frequency "not an amateur band" (work instruction 494) - it says Hamlet does not know the
    /// band, which is true, rather than that W1AW is not there, which on 6 m is false.</para>
    /// <para>**THE LICENSE NEVER DISABLES IT.** Listening is never restricted (HM-DEC-029); the
    /// tip says when sending Morse there is not covered.</para>
    /// </remarks>
    public static W1awButton ForDial(
        W1awMorseFrequencies table, PrivilegePlan plan, LicenseClass licenseClass, long dialHz)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(plan);

        var band = W1awMorseFrequencies.AmateurBandFor(dialHz);

        if (band is null)
        {
            return new W1awButton(
                "W1AW: not a band Hamlet knows",
                0,
                "Hamlet does not know the amateur band this frequency is in, so it cannot say where W1AW "
                + "sends Morse from here. Tune to a band Hamlet knows and this button will offer W1AW there.",
                PrivilegeTone.Unknown);
        }

        if (table.RowForBandOf(dialHz) is not { } row || !AmateurSpectrum.IsAmateur(row.FrequencyHz))
        {
            return new W1awButton(
                "W1AW not on " + band,
                0,
                $"W1AW does not send Morse on {band}, so there is nowhere on this band for this button to "
                + "take you. On 40 m, 20 m, 80 m and the other bands it sends on, it tunes to W1AW in CW.",
                PrivilegeTone.Unknown);
        }

        var tone = PrivilegeStatusLine.Build(plan, licenseClass, row.FrequencyHz, TransmitMode.Cw).Tone;

        return new W1awButton("W1AW on " + band, row.FrequencyHz, TipFor(row.FrequencyHz, tone), tone);
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
