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
/// <param name="HasKeying">
/// Whether anything on the band is actually being keyed, as against merely
/// present (HM-DEC-095).
/// </param>
/// <param name="Interference">
/// The strongest thing in the passband that is not somebody sending, if there is
/// one (HM-DEC-095).
/// </param>
/// <param name="OwnTransmitSeconds">
/// How much of what was heard was the operator's own transmitter (HM-DEC-095).
/// </param>
/// <param name="WordSpacingUnmeasured">
/// True when this sender left no word gaps long enough to measure, so the
/// transcript comes out unspaced (HM-DEC-142). Distinct from an empty
/// transcript, which is the decoder producing nothing.
/// </param>
/// <param name="PitchWasAsserted">
/// True where the operator said he could hear a station and Hamlet took the
/// loudest bin in the band, rather than finding one itself.
/// **`PitchWasMeasured` stays false throughout**, because nothing was measured;
/// this says who chose the number instead (Tim's ruling of 2026-08-26).
/// </param>
/// <param name="PitchWasMeasured">
/// True when <see cref="ToneHz"/> came from keying the survey admitted, false
/// when it is the middle of whatever bank the tracker is pointed at. **The two
/// are different facts and a sheet that prints one number for both is asserting
/// a measurement nobody took** (§0.0, HM-DEC-009).
/// </param>
/// <param name="Competitor">
/// Somebody else keying inside the same passband, where the survey found one.
/// **Null says the survey did not find one and never that the frequency is
/// clear** (HM-DEC-009).
/// </param>
public readonly record struct CwDecodeReport(
    AudioLevel Level,
    double ToneHz,
    double SnrDb,
    bool HasTone,
    int ElementsSeen,
    int ElementsResolved,
    int CharactersEmitted,
    int CharactersUnsure,
    bool HasKeying = false,
    ToneInterference? Interference = null,
    double OwnTransmitSeconds = 0,
    bool WordSpacingUnmeasured = false,
    CwCompetitor? Competitor = null,
    bool PitchWasMeasured = false,
    bool PitchWasAsserted = false)
{
    /// <summary>
    /// How far above the band a tone has to stand before it is worth mentioning.
    /// </summary>
    /// <remarks>
    /// <para>**TWELVE, AND THE NUMBER IS CALIBRATED RATHER THAN CHOSEN**
    /// (HM-DEC-090). It used to be three, which was right for the figure it was
    /// reading when it was written and wrong for the one it reads now: the SNR is
    /// a held peak measured while the tone is keyed, not an average across the
    /// silence, and the two live on completely different scales.</para>
    /// <para>Measured against this repository's own signals: half a minute of
    /// band noise with nothing on it reports about seven decibels, because a held
    /// peak eventually catches the loudest moment noise has. The decoder's own
    /// working limit, the weakest signal from which it still reads most of a
    /// message, reports about sixteen. Twelve sits between them with five
    /// decibels of margin on each side.</para>
    /// <para>**BOTH MARGINS MATTER AND FOR THE SAME REASON.** Too low and noise
    /// is announced as a station; too high and a real one is called silence.
    /// Phantom output and deafness are both §0.0 failures, and the second is
    /// only quieter (§0.0).</para>
    /// </remarks>
    public const double ToneThresholdDb = 12;

    /// <summary>
    /// How far the tone has to fall before it stops counting as one.
    /// </summary>
    /// <remarks>
    /// <para>**HYSTERESIS, FOR THE SAME REASON THE KEYING GATE HAS IT.** It takes
    /// more to decide a signal is there than to decide it is still there, because
    /// a marginal one dips below any single line in the quiet stretches of its own
    /// message. Without this the gate shut mid-message on weak signals and cost a
    /// decibel of the decoder's reach, measured (HM-DEC-090).</para>
    /// <para>Eight is still a decibel above what half a minute of empty band
    /// reports, and nothing can fall to it without first having climbed past
    /// twelve, so noise cannot open the gate and then hold it open.</para>
    /// </remarks>
    public const double ToneReleaseDb = 8;

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

        // **THE LOAD-BEARING HALF OF HM-DEC-142, AND IT GOES WHERE HE IS
        // LOOKING.** The transcript is coming out with no spaces in it, because
        // this sender left no gap long enough to call a word break. The letters
        // are measured and the word boundaries are not, and saying so is the
        // difference between an odd-looking transcript and a stated condition
        // (§0.0). Ahead of the "all is well" return, because characters are
        // arriving and something still needs saying.
        if (report.WordSpacingUnmeasured)
        {
            return "The letters below are what Hamlet heard, run together. "
                + "Whoever is sending has not left a gap long enough to call a "
                + "word break, which is ordinary in a callsign or an exchange, so "
                + "the letters are measured and the spaces between words are not "
                + "shown at all rather than being put where they might have gone.";
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

        // ON THE HELD-PEAK SCALE (HM-DEC-090), where the decoder's own working
        // limit reads about sixteen and an empty band about seven. The old
        // numbers were right for an average that no longer exists.
        var strength = over >= 28
            ? "well clear of"
            : over >= 18 ? "comfortably above" : "only just above";

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
