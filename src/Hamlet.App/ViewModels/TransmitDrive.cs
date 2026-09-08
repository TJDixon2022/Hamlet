using System;
using System.Globalization;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Transmit;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The one place a Transmit drive control turns a percentage on a screen into a
/// peak in the settings file, and the one place it says what that works out to.
/// </summary>
/// <remarks>
/// <para>**IT EXISTS BECAUSE THERE ARE NOW TWO CONTROLS OVER ONE SETTING** (work
/// instruction 269, task 2). The drive lived only in
/// <see cref="SettingsViewModel"/>, behind a modal dialog that covers the
/// waterfall, the decode table and the always-pressable Stop button; step D's
/// first exit criterion asks the operator to set it and read the level *under the
/// waterfall*, so a second control now sits there. **Two controls over one
/// setting is exactly the shape that drifts**, and it drifts silently: a control
/// that writes a percentage into <see cref="AppSettings.TransmitDrivePeak"/>,
/// which holds a peak, makes 25 mean 2500 % of full scale, and a control that
/// does not save loses the level the operator set against his radio's ALC the
/// next time Hamlet launches. Both are arithmetic, so both are here, once.</para>
/// <para>**THE VALIDATION IS ASKED OF <see cref="Ft8Composer.DriveIsUsable"/> AND
/// IS NOT ANSWERED AGAIN**, on either screen. A level a control accepted must
/// never be one the send path then refuses with a sentence at the moment the
/// operator presses send, and the only way to guarantee that is for both to be
/// the same question.</para>
/// <para>**THE SENTENCE IS THE ONE THE SETTINGS SCREEN ALREADY SHOWED**, moved
/// rather than rewritten: unit 265 wrote it, `SHACK_FACTS.md` FACT-004 is why it
/// is worded as it is, and the committed
/// <c>SettingsCarriesTheTransmitDriveTests.TheNoteSaysItIsAStartingPointToBeSetAgainstTheRadiosAlc</c>
/// asserts three of its phrases. It says on its face that the number is a
/// starting point set against the operator's own radio, because what the
/// IC-7300's USB modulation input expects is not in this repository and a default
/// must never read as a specification.</para>
/// </remarks>
internal static class TransmitDrive
{
    /// <summary>What a percentage on a screen is as a peak amplitude.</summary>
    /// <param name="percent">Percent of full scale.</param>
    /// <returns>The peak, in the units <see cref="AppSettings.TransmitDrivePeak"/> holds.</returns>
    internal static float PeakFor(double percent) => (float)(percent / 100.0);

    /// <summary>What a stored peak is as a percentage on a screen.</summary>
    /// <param name="peak">The peak amplitude in force.</param>
    /// <returns>Percent of full scale.</returns>
    internal static double PercentFor(float peak) => peak * 100.0;

    /// <summary>
    /// Writes a level the composer accepts, and writes nothing at all otherwise.
    /// </summary>
    /// <param name="settings">The one settings instance both screens share.</param>
    /// <param name="percent">What the operator set, as a percentage.</param>
    /// <returns>True where the level was usable and was saved.</returns>
    /// <remarks>
    /// **A VALUE THE COMPOSER WOULD REFUSE IS NOT WRITTEN TO THE FILE AT ALL.**
    /// The setting keeps the level that was last usable rather than storing one
    /// the send path would reject with a sentence at the moment the operator
    /// pressed send.
    /// </remarks>
    internal static bool Write(AppSettings settings, double percent)
    {
        var peak = PeakFor(percent);

        if (!Ft8Composer.DriveIsUsable(peak, out _))
        {
            return false;
        }

        settings.TransmitDrivePeak = peak;
        SettingsStore.Save(settings);

        return true;
    }

    /// <summary>What the drive works out to, and what it is for.</summary>
    /// <param name="percent">What the control is showing.</param>
    /// <param name="inForce">The peak actually in the settings, for the refusal.</param>
    /// <returns>One sentence for the operator to read.</returns>
    internal static string NoteFor(double percent, float inForce)
    {
        var peak = percent / 100.0;

        if (!Ft8Composer.DriveIsUsable((float)peak, out var why))
        {
            return "That is not a transmit level, so Hamlet is still using "
                + (inForce * 100.0).ToString("0.#", CultureInfo.InvariantCulture)
                + " %. " + char.ToUpperInvariant(why[0]) + why[1..];
        }

        // **THE NUMBER AND ITS UNITS, AND NOTHING ELSE** (Tim's ruling,
        // 2026-09-08: show, do not tell). This read three hundred and
        // twenty-five characters of ALC theory, permanently, under a control he
        // sets once. **The advice is not thrown away** - it is
        // <see cref="Tip"/>, a hover behind the Radio tips mark.
        return (20.0 * Math.Log10(peak)).ToString("0.0", CultureInfo.InvariantCulture)
            + " dBFS";
    }

    /// <summary>How to set the drive, for the Radio tips hover.</summary>
    /// <remarks>
    /// <para>**MOVED, NOT DELETED** (work instruction 280 task 8). Removing words
    /// must not remove facts, and this is genuinely useful the first time
    /// somebody sets it: it is advice, and advice does not occupy the screen
    /// while he operates.</para>
    /// <para>**IT IS ALSO IN `SHACK_FACTS.md` AS FACT-005**, where his own
    /// reading against his own ALC meter is recorded: 25 per cent, -12.04 dBFS,
    /// ALC -2.0 to -1.5. That is the measured version of this paragraph and it
    /// outranks it.</para>
    /// </remarks>
    internal static string Tip
        => "This is a starting point, not a specification. Set it against your "
            + "own radio's ALC meter: turn it up until the ALC just begins to "
            + "move and then back off. Full scale is a wide, distorted signal "
            + "over other people's band, which is why Hamlet starts at "
            + (Ft8Composer.DefaultDrivePeak * 100.0).ToString("0.#", CultureInfo.InvariantCulture)
            + " %.";
}
