using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// What Hamlet can say about the radio's settings from values it actually read.
/// </summary>
/// <remarks>
/// <para>THE LINE, AND IT IS A NARROW ONE (HM-DEC-050). "Your filter is wide
/// open at 3 kHz, which is why several signals are landing in the decoder at
/// once" is a statement about a number Hamlet read and a mechanism it
/// understands, and it is exactly the sentence that would have ended a
/// half-hour diagnosis in a moment. "Narrow your filter" is not: that is
/// telling somebody what to change on their own radio, and this session does
/// not write to radios or tell people to.</para>
/// <para>SO EVERY OBSERVATION HERE IS A CONSEQUENCE, NOT AN INSTRUCTION. It
/// names a value that was read, states what follows from it, and stops. No
/// imperative, no "should", no "try", and above all no suggestion that anything
/// is broken: a wide filter is a perfectly good setting for listening around,
/// and the operator may have chosen it on purpose.</para>
/// <para>Nothing is said from a value that was not read. An observation resting
/// on an assumed setting would be the confident guess §0.0 forbids, wearing the
/// clothes of helpfulness.</para>
/// </remarks>
public static class RigObservations
{
    /// <summary>
    /// A passband this wide in a Morse mode admits more than one signal.
    /// </summary>
    /// <remarks>
    /// A Morse signal at ordinary speeds occupies something like a hundred
    /// hertz. A kilohertz of passband therefore has room for several of them at
    /// once, which is fine for finding somebody and unhelpful once a decoder is
    /// trying to read one.
    /// </remarks>
    public const int CrowdedCwPassbandHz = 1_000;

    /// <summary>Every observation the current state supports.</summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <returns>Observations, which may be empty.</returns>
    public static IReadOnlyList<string> For(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var said = new List<string>();

        WideFilterInCw(state, said);
        NoiseBlankerOnACrowdedBand(state, said);
        AttenuatorAndPreampTogether(state, said);
        SquelchHoldingTheAudioShut(state, said);
        AudioSentAsIf(state, said);

        return said;
    }

    /// <summary>
    /// The one that would have ended the half-hour: a wide passband while
    /// decoding Morse.
    /// </summary>
    private static void WideFilterInCw(RigState state, List<string> said)
    {
        if (state.Mode is not { } mode
            || !CivValues.IsCw(mode)
            || state.FilterBandwidthHz is not { } hertz
            || hertz < CrowdedCwPassbandHz)
        {
            return;
        }

        said.Add(
            $"The filter is open to {CivFilterWidth.Describe(hertz)} and the radio is "
            + "in Morse. A Morse signal is only about a hundred hertz wide, so "
            + "everything else inside that span is arriving at the decoder at the "
            + "same time as the one you are reading.");
    }

    /// <summary>
    /// The noise blanker is on, which is worth knowing when the band is busy.
    /// </summary>
    private static void NoiseBlankerOnACrowdedBand(RigState state, List<string> said)
    {
        if (state[RigField.NoiseBlanker] is not { IsKnown: true, Number: 1 })
        {
            return;
        }

        said.Add(
            "The noise blanker is on. It works by muting the instant a sharp tick "
            + "arrives, which is invisible on ignition noise and audible on a busy "
            + "band, where a strong nearby signal can look like a tick and get "
            + "chopped along with it.");
    }

    /// <summary>Both ends of the gain chain fighting each other.</summary>
    private static void AttenuatorAndPreampTogether(RigState state, List<string> said)
    {
        var attenuator = state[RigField.Attenuator];
        var preamp = state[RigField.Preamp];

        if (attenuator is not { IsKnown: true, Number: > 0 }
            || preamp is not { IsKnown: true, Number: > 0 })
        {
            return;
        }

        said.Add(
            $"The attenuator is at {attenuator.Text} and the preamp is on as well. "
            + "One is turning the front end down and the other is turning it back "
            + "up, so between them they are mostly cancelling out.");
    }

    /// <summary>The squelch is shut, which is why nothing is coming through.</summary>
    private static void SquelchHoldingTheAudioShut(RigState state, List<string> said)
    {
        if (state[RigField.SquelchStatus] is not { IsKnown: true, Number: 0 })
        {
            return;
        }

        var level = state[RigField.Squelch];
        var howHigh = level.IsKnown ? $", set to {level.Text}" : "";

        said.Add(
            $"The squelch is closed{howHigh}, so no audio is reaching the computer at "
            + "the moment. It opens again as soon as something arrives above the "
            + "level it is set to.");
    }

    /// <summary>
    /// The radio is sending the computer IF rather than audio, which no decoder
    /// here can read.
    /// </summary>
    private static void AudioSentAsIf(RigState state, List<string> said)
    {
        if (state[RigField.AccUsbOutputSelect] is not { IsKnown: true, Number: 1 })
        {
            return;
        }

        said.Add(
            "The radio is sending the computer its IF signal rather than audio. "
            + "Hamlet's decoder listens for a Morse note in the audio range, and an "
            + "IF feed carries something else entirely.");
    }
}
