using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Everything the decoder can honestly say about what it is hearing right now
/// (HM-DEC-088).
/// </summary>
/// <param name="Level">What is arriving at the sound card.</param>
/// <param name="ToneHz">The pitch being followed.</param>
/// <param name="SnrDb">
/// How far the tone stands above the band beside it, or NaN when that has not
/// been measured.
/// </param>
/// <param name="HasTone">Whether there is anything at that pitch worth calling a tone.</param>
/// <param name="ElementsSeen">How many marks and gaps have been measured.</param>
/// <param name="ElementsResolved">How many of those became part of a character.</param>
/// <param name="CharactersEmitted">How many characters reached the screen.</param>
/// <param name="CharactersUnsure">How many of those were marked or blocked.</param>
public readonly record struct CwDecodeReport(
    AudioLevel Level,
    double ToneHz,
    double SnrDb,
    bool HasTone,
    int ElementsSeen,
    int ElementsResolved,
    int CharactersEmitted,
    int CharactersUnsure)
{
    /// <summary>
    /// How far above the band a tone has to stand before it is worth mentioning.
    /// </summary>
    /// <remarks>
    /// Three decibels, which is twice the power of what is around it. Below that
    /// what is in the bin is as likely to be the noise as anything else, and
    /// naming it a tone would be exactly the guess §0.0 forbids.
    /// </remarks>
    public const double ToneThresholdDb = 3;

    /// <summary>Nothing measured.</summary>
    public static CwDecodeReport None { get; } = new(
        AudioLevel.None, 0, double.NaN, false, 0, 0, 0, 0);

    /// <summary>True when the audio path is handing over near-silence.</summary>
    public bool NearlySilent => Level.NearlySilent;

    /// <summary>True when samples are hitting the end of the scale.</summary>
    public bool Clipping => Level.Clipping;
}

/// <summary>
/// Says what the decoder can see, even when it is producing nothing
/// (HM-DEC-088).
/// </summary>
/// <remarks>
/// <para>**A STRONG SIGNAL THAT WILL NOT RESOLVE AND AN EMPTY BAND USED TO
/// PRODUCE THE SAME SCREEN**, and they are completely different problems. One is
/// a decoder that needs fixing, one is a band with nobody on it, and one is a
/// radio whose audio is not reaching the computer at all. Silence about all
/// three is §0.0.1 broken at the one place the application is supposed to be
/// strongest.</para>
/// <para>**EVERYTHING HERE IS A MEASUREMENT OF THE AUDIO AND NEVER AN INFERENCE
/// ABOUT A STATION.** No speed, no callsign, no confidence marks unless
/// characters are genuinely being decoded. "There is a tone at 620 hertz about
/// three decibels above the noise and the timing is not resolving into Morse" is
/// a diagnosis about this program. "Somebody is sending badly" would be a claim
/// about a person, from the same evidence, and it is not Hamlet's to make.</para>
/// <para>This is the same trap the speed estimate fell into once, where a number
/// derived from noise was displayed as a fact about an operator. The rule that
/// came out of it holds here in full: report what was measured, claim nothing
/// beyond it.</para>
/// </remarks>
public static class CwDecodeStory
{
    /// <summary>
    /// What to say about the decoder's own input, in the app's voice.
    /// </summary>
    /// <param name="report">What was measured.</param>
    /// <param name="listening">Whether the decoder is running at all.</param>
    /// <returns>One passage, or "" when characters are arriving and all is well.</returns>
    public static string Describe(CwDecodeReport report, bool listening)
    {
        if (!listening)
        {
            return "";
        }

        if (report.Clipping)
        {
            return "The audio coming into Hamlet is hitting the top of its range "
                + "and being flattened off, which turns a clean note into a rough "
                + "one and gives the decoder edges nobody sent. The level going "
                + "into the computer wants turning down.";
        }

        if (report.NearlySilent)
        {
            return "Hamlet is receiving almost no audio at all. What comes out of "
                + "the speaker and what goes down the USB cable are two separate "
                + "paths with two separate levels, so the radio can sound "
                + "perfectly good in your headphones while the computer is being "
                + "handed near-silence.";
        }

        if (report.CharactersEmitted > 0)
        {
            return "";
        }

        if (!report.HasTone)
        {
            return "Audio is arriving and there is nothing standing out of it at "
                + "any pitch Hamlet listens at, which is what an empty patch of "
                + "band sounds like to a decoder.";
        }

        var pitch = (int)Math.Round(report.ToneHz / 10) * 10;
        var over = report.SnrDb;

        var strength = over >= 12
            ? "well clear of"
            : over >= 6 ? "comfortably above" : "only just above";

        return $"There is a tone at about {pitch} hertz, {strength} the noise "
            + $"around it, and the timing is not resolving into Morse. That "
            + "means Hamlet can see something there and cannot make letters of "
            + "it yet.";
    }

    /// <summary>
    /// The one-line version, for a panel summary.
    /// </summary>
    /// <param name="report">What was measured.</param>
    /// <param name="listening">Whether the decoder is running.</param>
    /// <returns>A short line.</returns>
    public static string Summarize(CwDecodeReport report, bool listening)
    {
        if (!listening)
        {
            return "not listening";
        }

        if (report.Clipping)
        {
            return "input overloading";
        }

        if (report.NearlySilent)
        {
            return "almost no audio arriving";
        }

        if (report.CharactersEmitted > 0)
        {
            return "";
        }

        return report.HasTone
            ? $"a tone at {(int)Math.Round(report.ToneHz / 10) * 10} Hz, not resolving"
            : "nothing standing out of the noise";
    }
}
