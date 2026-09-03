namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// How loud the audio one FT8 slot was cut from actually was (unit 236).
/// </summary>
/// <param name="PeakDbFullScale">
/// The loudest sample in the slot, in decibels relative to full scale, or
/// **null where there was no level to take** — see the remarks. Zero is the
/// loudest the path can carry.
/// </param>
/// <param name="RmsDbFullScale">
/// The root mean square of the whole slot, in decibels relative to full scale,
/// or null on the same terms as <paramref name="PeakDbFullScale"/>.
/// </param>
/// <param name="SampleCount">How many samples the slot held.</param>
/// <param name="ZeroSampleCount">
/// How many of them were exactly zero. **This is the number that separates a
/// dead input from a quiet one.** A muted device, an unplugged codec and a
/// sound card handing over digital silence deliver samples that are literally
/// nought; a closed band delivers a receiver's own noise, which is quiet and is
/// not zero. Nothing else in the census can tell those two apart.
/// </param>
/// <remarks>
/// <para>**IT IS A LEVEL AND IT IS NOT A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md`
/// §0.0). It says how loud the audio in the slot was. It says nothing whatever
/// about how strong any signal in that audio was, it is not comparable with the
/// published sensitivity figure for this mode, and it must never appear under an
/// `snr` heading. The same prohibition already governs
/// <see cref="Ft8SlotCensus.TopSyncScores"/> for the same reason.</para>
/// <para>**AN ALL-ZERO SLOT HAS NO LEVEL AND SAYS SO** (`CLAUDE.md` §0.0,
/// HM-DEC-009). The logarithm of nought is not a number, and every other place
/// in this tree that meets it substitutes
/// <see cref="AudioLevel.SilenceDb"/> — minus ninety — which is a floor for a
/// moving bar and is the right answer there. It is the wrong answer here. A
/// census line is read months later by somebody averaging a morning, and minus
/// ninety in that column is a measurement that will be averaged with real ones.
/// So both levels are null, and <see cref="ZeroSampleCount"/> standing at the
/// whole slot is what says why.</para>
/// <para>**AND NOTHING BELOW THAT IS FLOORED.** A slot holding a single least
/// significant bit of sixteen-bit audio measures about minus ninety decibels for
/// real, and clamping would make it indistinguishable from the refusal above.
/// The number is reported as the arithmetic produces it.</para>
/// <para>**NOTHING HERE IS INTERPRETED** (`CLAUDE.md` §12.1). There is no
/// verdict, no threshold and no adjective. Two levels and two counts, and the
/// reader decides what they mean.</para>
/// </remarks>
public sealed record Ft8SlotLevel(
    double? PeakDbFullScale,
    double? RmsDbFullScale,
    int SampleCount,
    int ZeroSampleCount)
{
    /// <summary>Nothing was measured.</summary>
    /// <remarks>
    /// **THE STATE OF A CENSUS LINE NOBODY GAVE AUDIO TO**, which is not the
    /// same as a slot measured and found silent: that one has a sample count.
    /// </remarks>
    public static Ft8SlotLevel None { get; } = new(null, null, 0, 0);

    /// <summary>
    /// What fraction of the slot was exactly zero, or null where there were no
    /// samples to take a fraction of.
    /// </summary>
    public double? ZeroSampleFraction
        => SampleCount <= 0 ? null : (double)ZeroSampleCount / SampleCount;

    /// <summary>
    /// Measure a slot's audio.
    /// </summary>
    /// <param name="audio">The slot's own samples, at the rate they arrived.</param>
    /// <returns>The level, or <see cref="None"/> where there was no audio.</returns>
    /// <exception cref="ArgumentNullException">The audio is null.</exception>
    /// <remarks>
    /// **TAKEN AT THE RATE THE SAMPLES ARRIVED, BEFORE THE RESAMPLER.** A
    /// resampler is one of the things a slot that found nothing could be, and a
    /// measurement taken downstream of a suspect cannot clear it.
    /// </remarks>
    public static Ft8SlotLevel Of(MonoAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var samples = audio.Samples;

        if (samples.Length == 0)
        {
            return None;
        }

        var peak = 0.0;
        var sumOfSquares = 0.0;
        var zeros = 0;

        foreach (var sample in samples)
        {
            var magnitude = Math.Abs((double)sample);

            if (magnitude > peak)
            {
                peak = magnitude;
            }

            if (sample == 0f)
            {
                zeros++;
            }

            sumOfSquares += magnitude * magnitude;
        }

        var rms = Math.Sqrt(sumOfSquares / samples.Length);

        return new Ft8SlotLevel(ToDb(peak), ToDb(rms), samples.Length, zeros);
    }

    /// <summary>
    /// A magnitude in decibels relative to full scale, or null where there is no
    /// logarithm to take.
    /// </summary>
    /// <remarks>
    /// **THE NULL IS THE POINT** and is the whole difference between this and
    /// the tap's own conversion, which floors at minus ninety so a level bar has
    /// somewhere to start.
    /// </remarks>
    private static double? ToDb(double magnitude)
        => magnitude <= 0 ? null : 20 * Math.Log10(magnitude);
}
