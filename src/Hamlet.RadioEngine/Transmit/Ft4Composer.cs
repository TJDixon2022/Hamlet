using Ft8Sharp.Encode;

namespace Hamlet.RadioEngine.Transmit;

/// <summary>
/// The one seam between what the operator wants to say and the audio of one FT4
/// slot.
/// </summary>
/// <remarks>
/// <para>**IT IS <see cref="Ft8Composer"/>'S SIBLING AND NOT ITS BRANCH** (work
/// instruction 293 task 2). Both are faces onto <see cref="DigitalComposer"/>,
/// which holds the packing, the round trip, the five refusals and the drive; what
/// each face supplies is a <see cref="ComposeGeometry"/>, and that is four
/// constants and three port calls. **Neither type is inside the other and neither
/// can change what the other composes.**</para>
/// <para>**THE MESSAGE LAYER IS SHARED AND IS NOT COPIED.** FT4 carries FT8's
/// 77-bit payload - <c>PHASE_PLAN.md</c> says so and unit 288 measured it - so the
/// packing, the three-pass hash ordering and the round trip through
/// <c>Ft8MessageDecoder</c> are the same code that FT8 runs, not a copy of it. That
/// is why <see cref="Ft8Transmission.Type"/> on an FT4 transmission is an
/// <c>Ft8MessageType</c>: it is the same message in the same bits, going out under
/// a different modulation.</para>
/// <para>**THE MODULATION IS UNIT 289'S AND NOTHING HERE CHANGES A LINE OF IT.**
/// <c>Ft4SymbolEncoder.Encode</c> and <c>Ft4Waveform.Synthesize</c> were nailed to
/// <c>gen_ft8 -ft4</c> symbol for symbol and sample for sample in the port; this
/// calls them. **There is no Costas array here, no tone table, no phase
/// accumulator and no symbol assembly of any kind**, and
/// <c>TheFt4ComposerIsThePortsOwnAudio</c> asserts the audio a message composes to
/// against what <c>Ft4Waveform</c> produces for that message's symbols, sample for
/// sample, so this seam cannot drift from the thing unit 289 nailed to
/// upstream.</para>
/// <para>**FT4'S NYQUIST ARITHMETIC IS NOT FT8'S AND IS NOT WRITTEN DOWN HERE.**
/// FT4 has four tones at 20.8333 Hz where FT8 has eight at 6.25, so the top tone
/// sits 62.5 Hz above tone 0 rather than 43.75 Hz. Both figures come out of
/// <c>Ft4Waveform</c>'s own constants through <see cref="ComposeGeometry.Ft4"/>.
/// </para>
/// <para>**IT KEYS NOTHING AND OPENS NOTHING**, exactly as its sibling does. It
/// takes words and returns an array of floats. Where that array sits in time - a
/// 7.5-second grid and the transmission inside it - is <c>SlotGrid</c>'s question
/// and is asked by <c>Ft8TransmitSequence</c>. **Nothing here carries a
/// transmission length**, which is what keeps the open 4.48-against-5.04 question
/// out of this file entirely.</para>
/// </remarks>
public static class Ft4Composer
{
    /// <summary>The rate the port's FT4 decoder reads at, and this seam's default.</summary>
    /// <remarks>
    /// <c>Ft4Waveform.DefaultSampleRate</c>, taken from the port's own synthesiser
    /// rather than written again here. **It is the same 12 000 Hz FT8 uses**, and
    /// it is read from FT4's own constant rather than shared with FT8's, so a
    /// change upstream to either one cannot silently move the other.
    /// </remarks>
    public const int DefaultSampleRate = Ft4Waveform.DefaultSampleRate;

    /// <summary>Where tone 0 sits in the audio passband unless another is asked for.</summary>
    public const float DefaultBaseFrequencyHz = Ft4Waveform.DefaultBaseFrequency;

    /// <summary>
    /// Turns what the operator wants to say into one padded FT4 slot of audio.
    /// </summary>
    /// <param name="text">The message, in the operator's own words.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <param name="drivePeak">
    /// The peak amplitude to build at. Defaults to
    /// <see cref="Ft8Composer.DefaultDrivePeak"/> - **the same level, from the same
    /// place, for the same reason**; how loud to drive a radio's modulation input
    /// is not a property of how many tones a mode has, and a second default here
    /// would be a second thing for the operator's setting to disagree with.
    /// </param>
    /// <returns>A slot of audio, or a refusal naming what would not pack.</returns>
    /// <remarks>
    /// **THIS IS THE ROUTE THAT GOES INTO A DECODER**, as
    /// <see cref="Ft8Composer.Compose"/> is for FT8.
    /// </remarks>
    public static Ft8ComposeResult Compose(
        string? text,
        int sampleRate = DefaultSampleRate,
        float baseFrequencyHz = DefaultBaseFrequencyHz,
        float drivePeak = Ft8Composer.DefaultDrivePeak)
        => DigitalComposer.Build(
            ComposeGeometry.Ft4, text, sampleRate, baseFrequencyHz, drivePeak, wholeSlot: true);

    /// <summary>
    /// Turns what the operator wants to say into the signal alone - the tones,
    /// with no silence at either end.
    /// </summary>
    /// <param name="text">The message, in the operator's own words.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <param name="drivePeak">The peak amplitude to build at.</param>
    /// <returns>The signal, or a refusal naming what would not pack.</returns>
    /// <remarks>
    /// **THIS IS THE ROUTE THAT GOES ON THE AIR.** The two differ in one call -
    /// <c>Ft4Waveform.Synthesize</c> here, <c>Ft4Waveform.SynthesizeSlot</c> there -
    /// and in nothing else, which is the same difference
    /// <see cref="Ft8Composer"/>'s own two routes have. **The padded slot is the
    /// wrong thing to play** for the reason FT8's is: on the air the silence at
    /// either end is time and not samples, and where the transmission starts is
    /// stated to <c>Ft8TransmitSequence</c> as a figure rather than built into an
    /// array.
    /// </remarks>
    public static Ft8ComposeResult ComposeSignal(
        string? text,
        int sampleRate = DefaultSampleRate,
        float baseFrequencyHz = DefaultBaseFrequencyHz,
        float drivePeak = Ft8Composer.DefaultDrivePeak)
        => DigitalComposer.Build(
            ComposeGeometry.Ft4, text, sampleRate, baseFrequencyHz, drivePeak, wholeSlot: false);

    /// <summary>
    /// Whether the port will synthesise FT4 at this rate, asked of the port rather
    /// than decided here.
    /// </summary>
    /// <remarks>
    /// **IT IS FT4'S OWN TWO LENGTH FUNCTIONS AND NOT FT8'S.** A rate at which an
    /// FT8 symbol is a whole number of samples and an FT4 symbol is not would pass
    /// <see cref="Ft8Composer.RateIsUsable"/> and put every FT4 sample after the
    /// first symbol at the wrong offset.
    /// </remarks>
    public static bool RateIsUsable(int sampleRate, out string explanation)
        => DigitalComposer.RateIsUsable(ComposeGeometry.Ft4, sampleRate, out explanation);

    /// <summary>Whether every FT4 tone would fit in the channel at this rate.</summary>
    public static bool BaseFrequencyIsUsable(
        float baseFrequencyHz, int sampleRate, out string explanation)
        => DigitalComposer.BaseFrequencyIsUsable(
            ComposeGeometry.Ft4, baseFrequencyHz, sampleRate, out explanation);

    /// <summary>
    /// Whether a transmit drive level is a peak amplitude audio can be built at.
    /// </summary>
    /// <remarks>
    /// **THE SAME METHOD FT8 ASKS**, because the answer does not depend on the
    /// modulation. It is named here so that a caller holding an FT4 composer has
    /// every one of the five refusals in front of it.
    /// </remarks>
    public static bool DriveIsUsable(float drivePeak, out string explanation)
        => DigitalComposer.DriveIsUsable(drivePeak, out explanation);
}
