using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Rig;

/// <summary>What Hamlet would change, and whether it needs to (HM-DEC-084).</summary>
/// <param name="Write">The setting, with its citation.</param>
/// <param name="Value">What it would be set to.</param>
/// <param name="Says">One line: what would change and why.</param>
/// <param name="AlreadyRight">True when there is nothing to do.</param>
/// <param name="Unreadable">
/// True when Hamlet could not read the setting, so it neither acts nor pretends.
/// </param>
public sealed record ReceiveSuggestion(
    CivWrite Write, int Value, string Says, bool AlreadyRight, bool Unreadable)
{
    /// <summary>True when pressing the button would actually do this one.</summary>
    public bool WouldChange => !AlreadyRight && !Unreadable;
}

/// <summary>
/// "I can hear it and Hamlet can't" (HM-DEC-084).
/// </summary>
/// <remarks>
/// <para>**SETTINGS ARE CONSEQUENCES OF INTENT, NEVER THINGS THE OPERATOR
/// OPERATES.** A rig control app gives somebody a Noise Blanker button and
/// expects them to know when to press it. Hamlet gives them one button that says
/// "I can hear it and you can't", does the handful of things that usually cause
/// that, says what it changed in plain words, and offers to put it back. Nobody
/// ever learns what auto notch is. They learn that pressing that button usually
/// helps.</para>
/// <para>**THE LIST IS COMPUTED FROM LIVE RIG STATE AND IS NEVER HARDCODED.**
/// What is already right says so and stays visible, because hiding it is tidier
/// and teaches nothing while showing it is the app proving what it checked.
/// **What could not be read says that**, rather than being guessed at or
/// silently dropped, which would leave somebody believing Hamlet had looked at
/// something it never saw (§0.0).</para>
/// <para>Nothing here can put a signal on the air. Every write it proposes is
/// tier one (<see cref="RigWriteTier.Receive"/>), which is what makes "do all
/// four" one press rather than four confirmations.</para>
/// <para>Pure: state in, suggestions out (§5).</para>
/// </remarks>
public static class ReceiveAdvice
{
    /// <summary>Below this the receive gain is holding signals down.</summary>
    /// <remarks>
    /// The evening this was written it sat at 42 percent and the receiver was
    /// deaf for two hours. Anything under most of the way open is worth saying,
    /// because a gain control is the one thing on the front panel that silently
    /// undoes everything else.
    /// </remarks>
    public const int OpenGainAbove = 240;

    /// <summary>
    /// Everything Hamlet would change to hear a faint signal better.
    /// </summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <returns>The list, in the order it reads, never null.</returns>
    public static IReadOnlyList<ReceiveSuggestion> For(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var inMorse = state.Mode is { } mode && CivValues.IsCw(mode);

        return new[]
        {
            AutoNotch(state, inMorse),
            NoiseReduction(state),
            NoiseBlanker(state),
            Agc(state, inMorse),
            Filter(state, inMorse),
            Preamp(state),
            Gain(state),
            UsbLevel(state),
        };
    }

    /// <summary>
    /// The auto notch, which is the one that prompted the whole ruling.
    /// </summary>
    /// <remarks>
    /// A Morse note is a steady tone switching on and off, and the auto notch
    /// hunts steady tones and takes them out. In CW it removes the thing being
    /// decoded. Hamlet has printed that on its diagnostics screen for weeks and
    /// could not act on it, and that gap is the feature.
    /// </remarks>
    private static ReceiveSuggestion AutoNotch(RigState state, bool inMorse)
    {
        var value = state[RigField.AutoNotch];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.AutoNotch, 0,
                "Hamlet could not read the automatic notch, so it is leaving it "
                + "alone.");
        }

        if (value.Number is 0)
        {
            return Fine(
                CivWrites.AutoNotch,
                "The automatic notch is already off, which is what Morse wants.");
        }

        return new ReceiveSuggestion(
            CivWrites.AutoNotch, 0,
            inMorse
                ? "Turn off the automatic notch. It removes steady tones, and a "
                  + "Morse note is a steady tone switching on and off, so it is "
                  + "hunting the thing you are trying to read."
                : "Turn off the automatic notch. It removes steady tones, which "
                  + "is what a Morse signal is.",
            AlreadyRight: false, Unreadable: false);
    }

    /// <summary>
    /// Noise reduction, which rounds off the edges Morse is made of.
    /// </summary>
    /// <remarks>
    /// It works by deciding what in the audio looks like signal and rebuilding
    /// that, and what it rebuilds is smooth. Morse is not smooth: it is a tone
    /// starting and stopping, and where each element starts and stops is the
    /// entire content. Softening those edges makes the band more comfortable to
    /// sit in and makes a dit and a dah harder to tell apart, which is a good
    /// trade for a person and a bad one for a decoder.
    /// </remarks>
    private static ReceiveSuggestion NoiseReduction(RigState state)
    {
        var value = state[RigField.NoiseReduction];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.NoiseReduction, 0,
                "Hamlet could not read the noise reduction, so it is leaving it "
                + "alone.");
        }

        return value.Number is 0
            ? Fine(CivWrites.NoiseReduction, "The noise reduction is already off.")
            : new ReceiveSuggestion(
                CivWrites.NoiseReduction, 0,
                "Turn off the noise reduction. It smooths the audio, and the sharp "
                + "starts and stops it smooths away are what tells a dit from a "
                + "dah.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>
    /// The automatic gain control, which wants to be quick for Morse.
    /// </summary>
    /// <remarks>
    /// <para>Slow AGC holds the receiver's gain down for a moment after anything
    /// loud, which is right for a voice and wrong for keying: one strong element
    /// pulls the gain down and the quiet elements behind it arrive smaller than
    /// they were. Fast lets it recover between elements.</para>
    /// <para>**Off is left alone rather than corrected.** Some operators run
    /// without it deliberately on a crowded band, it is a documented setting
    /// (`16 12`, `00` is off, p. 19-3), and changing something somebody chose
    /// because it is unusual is the protectiveness HM-DEC-084 exists to remove.
    /// </para>
    /// </remarks>
    private static ReceiveSuggestion Agc(RigState state, bool inMorse)
    {
        var value = state[RigField.Agc];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.Agc, AgcFast,
                "Hamlet could not read the gain control, so it is leaving it "
                + "alone.");
        }

        if (value.Number is 0)
        {
            return Fine(
                CivWrites.Agc,
                "The automatic gain control is switched off, which is a choice "
                + "some operators make on a crowded band, so Hamlet is leaving it "
                + "as it is.");
        }

        return value.Number == AgcFast
            ? Fine(CivWrites.Agc, "The gain control is already on its fast setting.")
            : new ReceiveSuggestion(
                CivWrites.Agc, AgcFast,
                inMorse
                    ? "Set the gain control to fast. On a slower setting one loud "
                      + "element holds the receiver down and the quiet ones behind "
                      + "it arrive smaller than they really were."
                    : "Set the gain control to fast, which suits keying better "
                      + "than a slower setting does.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>
    /// The receive filter, which wants to be narrow and not too narrow.
    /// </summary>
    /// <remarks>
    /// <para>**BOTH DIRECTIONS ARE FAULTS AND ONLY ONE OF THEM IS OBVIOUS.** Wide
    /// open, the filter lets in every station either side and the decoder is
    /// listening to a crowd. Too narrow, and a station tuned a little off the
    /// pitch you are listening at falls outside the filter and disappears
    /// entirely, which looks exactly like nobody being there.</para>
    /// <para>Five hundred hertz, which on this radio's scale is index ten
    /// (50 Hz to 500 Hz in 50 Hz steps, Full Manual p. 4-6). Wide enough that
    /// being a couple of hundred hertz off does not lose the signal, narrow
    /// enough to keep the neighbors out. It is the setting the decoder was
    /// designed around, since it hunts a note anywhere from 300 to 900 Hz.</para>
    /// </remarks>
    private static ReceiveSuggestion Filter(RigState state, bool inMorse)
    {
        var value = state[RigField.FilterBandwidth];

        // AN UNREAD MODE IS UNREAD, NOT "NOTHING TO SUGGEST". Saying Hamlet has
        // no advice here would imply it looked at the mode and found something
        // other than Morse, and where the mode has not been read it did not
        // (§0.0).
        if (state.Mode is null)
        {
            return Unread(
                CivWrites.FilterWidth, CwFilterIndex,
                "Hamlet could not read the mode, so it has nothing to say about "
                + "the filter width.");
        }

        if (!inMorse)
        {
            return Fine(
                CivWrites.FilterWidth,
                "This is not Morse, so Hamlet is leaving the filter alone.");
        }

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.FilterWidth, CwFilterIndex,
                "Hamlet could not read the filter width, so it is leaving it "
                + "alone.");
        }

        if (value.Number is not { } index)
        {
            return Unread(
                CivWrites.FilterWidth, CwFilterIndex,
                "Hamlet could not read the filter width, so it is leaving it "
                + "alone.");
        }

        if (index >= NarrowestUsefulIndex && index <= WidestUsefulIndex)
        {
            return Fine(
                CivWrites.FilterWidth,
                "The filter is already somewhere sensible for Morse.");
        }

        return new ReceiveSuggestion(
            CivWrites.FilterWidth, CwFilterIndex,
            index > WidestUsefulIndex
                ? "Narrow the filter to about five hundred hertz. Wide open it is "
                  + "letting in every station either side of this one, and the "
                  + "decoder is listening to all of them at once."
                : "Open the filter out to about five hundred hertz. This narrow, a "
                  + "station tuned even slightly off falls outside it altogether "
                  + "and vanishes, which looks exactly like nobody being there.",
            AlreadyRight: false, Unreadable: false);
    }

    /// <summary>
    /// The gain control's fast setting (`16 12`, Full Manual p. 19-3).
    /// </summary>
    /// <remarks>
    /// The row reads `00=OFF, 01=FAST, 02=MID, 03=SLOW`, and that off is a real
    /// value rather than a fourth speed is one of the four corrections
    /// HM-DEC-084 made when the write table was read column-aware.
    /// </remarks>
    public const int AgcFast = 1;

    /// <summary>Five hundred hertz on this radio's filter scale (p. 4-6).</summary>
    /// <remarks>
    /// The scale runs 50 Hz to 500 Hz in 50 Hz steps, so index nine is 500 Hz.
    /// </remarks>
    public const int CwFilterIndex = 9;

    /// <summary>Below this the filter can lose a station that is tuned slightly off.</summary>
    /// <remarks>Index three, which is two hundred hertz.</remarks>
    public const int NarrowestUsefulIndex = 3;

    /// <summary>Above this the filter is letting the neighbors in.</summary>
    /// <remarks>Index nineteen, which is a thousand hertz.</remarks>
    public const int WidestUsefulIndex = 19;

    /// <summary>The noise blanker, which mistakes strong signals for clicks.</summary>
    private static ReceiveSuggestion NoiseBlanker(RigState state)
    {
        var value = state[RigField.NoiseBlanker];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.NoiseBlanker, 0,
                "Hamlet could not read the noise blanker, so it is leaving it "
                + "alone.");
        }

        return value.Number is 0
            ? Fine(CivWrites.NoiseBlanker, "The noise blanker is already off.")
            : new ReceiveSuggestion(
                CivWrites.NoiseBlanker, 0,
                "Turn off the noise blanker. It mutes the instant a sharp tick "
                + "arrives, and on a busy band a strong nearby signal looks like "
                + "a tick and gets chopped along with the noise.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>The preamp, which is what a faint signal wants.</summary>
    private static ReceiveSuggestion Preamp(RigState state)
    {
        var value = state[RigField.Preamp];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.Preamp, 1,
                "Hamlet could not read the preamp, so it is leaving it alone.");
        }

        return value.Number is > 0
            ? Fine(CivWrites.Preamp, "The preamp is already on.")
            : new ReceiveSuggestion(
                CivWrites.Preamp, 1,
                "Switch the preamp on. It is more gain at the front end, which is "
                + "what a faint signal needs.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>
    /// The level the radio sends down the USB cable, which nothing on the front
    /// panel changes (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE ONE THE AF KNOB DOES NOT TOUCH**, and it may be the
    /// whole answer to signals the operator can hear and the decoder cannot. The
    /// speaker and the computer are two separate outputs with two separate
    /// levels: turning the volume up to hear a faint signal better does nothing
    /// at all for the decoder, and there is no indication anywhere on the radio
    /// that the other one exists.</para>
    /// <para>`1A 05 0060`, p. 19-4. Half is what the radio ships at and is a
    /// sensible place to be; well below that and the decoder is being handed a
    /// fraction of what is in the headphones.</para>
    /// </remarks>
    private static ReceiveSuggestion UsbLevel(RigState state)
    {
        var value = state[RigField.AccUsbAfLevel];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.AccUsbAfLevel, HealthyUsbLevel,
                "Hamlet could not read the level the radio sends down the USB "
                + "cable, so it is leaving it alone.");
        }

        if (value.Number is not { } level)
        {
            return Unread(
                CivWrites.AccUsbAfLevel, HealthyUsbLevel,
                "Hamlet could not read the level the radio sends down the USB "
                + "cable, so it is leaving it alone.");
        }

        return level >= LowUsbLevel
            ? Fine(
                CivWrites.AccUsbAfLevel,
                "The level the radio sends down the USB cable is up where it "
                + "should be.")
            : new ReceiveSuggestion(
                CivWrites.AccUsbAfLevel, HealthyUsbLevel,
                "Turn up the level the radio sends down the USB cable. It is a "
                + "separate output from the speaker, so the volume in your "
                + "headphones says nothing about it, and low is why a signal you "
                + "can hear perfectly well can arrive here as almost nothing.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>Where the USB output level is set to when it is too low.</summary>
    /// <remarks>
    /// A hundred and twenty-eight of two hundred and fifty-five, which is the
    /// middle and is where the radio ships. Not the top: an output run flat out
    /// clips, and clipping is the opposite failure and just as fatal to a decode.
    /// </remarks>
    public const int HealthyUsbLevel = 128;

    /// <summary>Below this the computer is being handed a fraction of the signal.</summary>
    public const int LowUsbLevel = 77;

    /// <summary>The receive gain, which cost two hours.</summary>
    private static ReceiveSuggestion Gain(RigState state)
    {
        var value = state[RigField.RfGain];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.RfGain, 255,
                "Hamlet could not read the receive gain, so it is leaving it "
                + "alone.");
        }

        if (value.Number is { } level && level >= OpenGainAbove)
        {
            return Fine(
                CivWrites.RfGain,
                "The receive gain is already open all the way, so there is "
                + "nothing to do there.");
        }

        var percent = value.Number is { } n
            ? (int)Math.Round(n / 255.0 * 100)
            : 0;

        return new ReceiveSuggestion(
            CivWrites.RfGain, 255,
            $"Open the receive gain all the way. It is at about {percent} percent, "
            + "and a gain control turned down is the one thing that quietly "
            + "undoes everything else.",
            AlreadyRight: false, Unreadable: false);
    }

    private static ReceiveSuggestion Fine(CivWrite write, string says)
        => new(write, 0, says, AlreadyRight: true, Unreadable: false);

    private static ReceiveSuggestion Unread(CivWrite write, int value, string says)
        => new(write, value, says, AlreadyRight: false, Unreadable: true);
}
