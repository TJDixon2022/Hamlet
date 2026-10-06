using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Everything the decoder can honestly say about what it is hearing right now (HM-DEC-088), **from the shape side**: the
/// detector's marks, the gate and the lookup table, the path that reaches the screen (work instruction 545, HM-DEC-249).
/// </summary>
/// <param name="Level">What is arriving at the sound card, from the audio tap: the input level, whatever is read.</param>
/// <param name="ToneHz">The printed sender's pitch, or NaN where nobody is printed.</param>
/// <param name="SendersHeld">How many senders the gate holds now, printed or not.</param>
/// <param name="ElementsResolved">How many dits and dahs became part of a character on the screen.</param>
/// <param name="CharactersEmitted">How many characters reached the screen.</param>
/// <param name="CharactersUnsure">How many of those were marked or blocked.</param>
/// <param name="Printing">Whether a sender is being printed.</param>
/// <param name="SpeedProof">
/// Whether <see cref="WordsPerMinute"/> is proved, from the printed sender's own dit; none where nobody is printed
/// (HM-REQ-034). **A speed the decoder has not earned is not a number.**
/// </param>
/// <param name="WordsPerMinute">The printed sender's speed from its dit, or null where it names none.</param>
/// <remarks>
/// <para>**THE OLD DECODER'S FIGURES CAME OUT WITH IT** (work instruction 545): the tracker's pitch, its held signal over
/// noise, its keying verdict, the interference and the competitor it found, the pitch proof and the unmeasured word
/// spacing all described the probabilistic lattice and the tone survey, which decided nothing that reached the screen.
/// What is said here is what the shape side holds.</para>
/// </remarks>
public readonly record struct CwDecodeReport(
    AudioLevel Level,
    double ToneHz,
    int SendersHeld,
    int ElementsResolved,
    int CharactersEmitted,
    int CharactersUnsure,
    bool Printing = false,
    CwSpeedProof SpeedProof = CwSpeedProof.None,
    int? WordsPerMinute = null)
{
    /// <summary>Whether the speed was proved.</summary>
    public bool SpeedWasProved => SpeedProof == CwSpeedProof.Proved;

    /// <summary>Whether the gate holds anybody at all: something keyed with the shape of Morse.</summary>
    public bool HearsSender => SendersHeld > 0;

    /// <summary>
    /// **THE "COMPETING" NOTE** (work instruction 545): another sender the gate holds beside the one being printed. Where the
    /// survey's competitor said somebody else was keying inside the passband, this says the gate holds a second sender.
    /// </summary>
    public bool Competing => Printing && SendersHeld > 1;

    /// <summary>Nothing heard yet.</summary>
    public static CwDecodeReport None { get; } = new(AudioLevel.None, double.NaN, 0, 0, 0, 0);

    /// <summary>True when almost no audio is arriving.</summary>
    public bool NearlySilent => Level.NearlySilent;

    /// <summary>True when the input is being clipped.</summary>
    public bool Clipping => Level.Clipping;
}

/// <summary>
/// The plain-language account of what the decoder is doing, in Hamlet's voice (§0.7), from the shape side (work
/// instruction 545): listening, a shape forming, or reading a sender at its speed.
/// </summary>
public static class CwDecodeStory
{
    /// <summary>What to tell the operator.</summary>
    /// <param name="report">The decoder's report.</param>
    /// <param name="listening">Whether Hamlet is listening at all.</param>
    /// <returns>A sentence or two, or empty where it is not listening.</returns>
    /// <remarks>
    /// **IT SAYS WHAT THE SHAPE SIDE IS DOING AND NOTHING MORE** (§0.0). The old decoder's story said a tone stood out of
    /// the noise at a pitch the tone survey chose; nothing on the screen came from that survey any more.
    /// </remarks>
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

        if (report.Printing)
        {
            var pitch = (int)Math.Round(report.ToneHz / 10) * 10;

            return report.WordsPerMinute is { } wpm
                ? $"Reading a sender at about {pitch} hertz, keying at about {wpm} words a minute."
                : $"Reading a sender at about {pitch} hertz.";
        }

        if (report.HearsSender)
        {
            return "A shape is forming. Something is keying with dits and dahs, and Hamlet waits until it has heard "
                + "enough of them to be sure it is somebody sending before it prints a letter.";
        }

        return "Listening. Nothing at any pitch has keyed with the shape of Morse yet, which is what an empty "
            + "patch of band sounds like to the decoder.";
    }
}
