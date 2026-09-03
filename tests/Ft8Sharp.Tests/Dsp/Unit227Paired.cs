using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 227's instrument: one slot on disk, read by both decoders.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why the file is not incidental.</b> Three units have now measured this receiver from the
/// inside and none of them moved criterion 2. The one reading nobody had was whether upstream's own
/// program does any better on the <em>identical</em> audio. Handing each decoder its own array would
/// leave two differences in the comparison — the algorithm and the quantisation — so every slot is
/// written once, as a 12 kHz sixteen-bit mono WAV, and both sides read that same file. Sixteen-bit
/// quantisation is then common to both, and <b>this port's own control number is re-taken through
/// the file</b> rather than carried over from the float array it came from.
/// </para>
/// <para>
/// <b>The slots are the recorded slots and not merely similar ones.</b> The rung construction, the
/// seeds, the population, the frequency and the offset are all
/// <see cref="Ft8Step6Ladder"/>'s — the same <c>seed + round(requested x 10)</c> draw rule the
/// recorded curve used, so a fresh process redraws the same noise.
/// </para>
/// <para>
/// <b>Nothing here adopts anything.</b> Under the phase plan's ruling that inheriting Goba's bugs is
/// accepted and step 6 is what would reveal an algorithmic weakness, a row where upstream decodes
/// better is <em>evidence</em> and never an adoption. No file under <c>src/</c> is touched by any
/// route through this class.
/// </para>
/// </remarks>
internal static class Unit227Paired
{
    /// <summary>The rate the whole ladder is measured at, and the rate upstream's demo defaults to.</summary>
    internal const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>The ladder's base frequency: exactly on a bin centre, and inside upstream's search.</summary>
    /// <remarks>
    /// <b>Measured against the pin on 2026-09-02, because a fixture outside upstream's search band
    /// would produce a zero that meant nothing.</b> <c>demo/decode_ft8.c</c> configures its monitor
    /// with <c>f_min = 200</c> and <c>f_max = 3000</c>. A base tone of 1000 Hz sits well inside that,
    /// with the eighth tone at 1043.75 Hz still far below the top.
    /// </remarks>
    internal const double OnGridHz = 1000.0;

    /// <summary>The ladder's sample offset: on the block grid, three symbols into the slot.</summary>
    internal static int AlignedOffset => Ft8Waveform.SamplesPerSymbol(Rate) * 3;

    /// <summary>What both decoders made of one slot.</summary>
    /// <param name="Label">Which message of the population it is.</param>
    /// <param name="Seed">The noise seed actually drawn, so a later session redraws this slot alone.</param>
    /// <param name="Delivered">The ratio actually put on the samples, in decibels.</param>
    /// <param name="OursReturned">Whether this port returned the transmitted text.</param>
    /// <param name="OursWrong">How many messages this port returned that were not it.</param>
    /// <param name="UpstreamReturned">Whether upstream returned the transmitted text.</param>
    /// <param name="UpstreamWrong">How many messages upstream returned that were not it.</param>
    /// <param name="UpstreamPrinted">
    /// The decode lines upstream printed, joined. Read at run time and never committed — a report
    /// quotes a count, and a shape where it must, and not upstream's output.
    /// </param>
    internal sealed record SlotOutcome(
        string Label,
        int Seed,
        double Delivered,
        bool OursReturned,
        int OursWrong,
        bool UpstreamReturned,
        int UpstreamWrong,
        string UpstreamPrinted);

    /// <summary>One side's count at one rung.</summary>
    internal sealed record Side(int Returned, int Wrong, int Trials)
    {
        internal double Rate => Trials == 0 ? 0.0 : 100.0 * Returned / Trials;

        internal (double Lower, double Upper) Interval => Ft8Step6Ladder.Wilson(Returned, Trials);

        internal string AsRow(string who) =>
            $"  {who,-38}: {Returned,3} of {Trials,3}, {Rate,5:F1} %, "
            + $"Wilson 95 {Interval.Lower,5:F1} to {Interval.Upper,5:F1}, WRONG {Wrong}";
    }

    /// <summary>The paired counts, which is the sharp instrument.</summary>
    /// <param name="Both">Slots both decoders returned.</param>
    /// <param name="OursOnly">Slots this port returned and upstream did not.</param>
    /// <param name="UpstreamOnly">Slots upstream returned and this port did not — <b>the address.</b></param>
    /// <param name="Neither">Slots neither returned.</param>
    internal sealed record Paired(int Both, int OursOnly, int UpstreamOnly, int Neither);

    /// <summary>Counts one side of a rung.</summary>
    internal static Side CountOurs(IReadOnlyList<SlotOutcome> slots) =>
        new(slots.Count(s => s.OursReturned), slots.Sum(s => s.OursWrong), slots.Count);

    /// <inheritdoc cref="CountOurs"/>
    internal static Side CountUpstream(IReadOnlyList<SlotOutcome> slots) =>
        new(slots.Count(s => s.UpstreamReturned), slots.Sum(s => s.UpstreamWrong), slots.Count);

    /// <summary>Counts the four cells.</summary>
    internal static Paired Pair(IReadOnlyList<SlotOutcome> slots) =>
        new(
            slots.Count(s => s.OursReturned && s.UpstreamReturned),
            slots.Count(s => s.OursReturned && !s.UpstreamReturned),
            slots.Count(s => !s.OursReturned && s.UpstreamReturned),
            slots.Count(s => !s.OursReturned && !s.UpstreamReturned));

    /// <summary>
    /// <b>Walks one rung, writing every slot to disk and reading it back with both decoders.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nothing is told to either decode path.</b> The frequency and the offset go to the
    /// synthesizer; <see cref="Ft8SlotDecoder"/> is handed samples and upstream is handed a path.
    /// The transmitted text is used once, after both have answered, to compare.
    /// </para>
    /// <para>
    /// <b>The samples both sides read are the same numbers, not merely the same file.</b> The WAV is
    /// written by upstream's own <c>save_wav</c> rule and read back by upstream's own
    /// <c>load_wav</c> rule — <c>count / 32768.0f</c> — so this port sees exactly what upstream's
    /// <c>float signal[]</c> holds.
    /// </para>
    /// <para>
    /// <b>A slot that returns nothing is a measurement and not a failure.</b> Nothing here throws on
    /// a poor result, and nothing here is asserted.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<SlotOutcome> WalkRung(
        double requested,
        Action<string>? log = null)
    {
        var population = Ft8Step6Ladder.Population();
        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;
        var slots = new List<SlotOutcome>();

        foreach (var seedBase in Ft8Step6Ladder.Seeds)
        {
            // The draw rule of the recorded curve, transcribed rather than reinvented: the seed
            // depends only on the rung and the draw, so a fresh process draws the same noise.
            var seed = seedBase + (int)Math.Round(requested * 10.0);
            var noise = new GaussianNoise(seed);

            foreach (var entry in population)
            {
                var (clean, _) = SearchFixture.OneSignal(Rate, entry, OnGridHz, AlignedOffset);
                var signalPower = SearchFixture.TransmissionPower(Rate, entry, OnGridHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, Rate);
                var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);

                var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                slots.Add(ThroughTheFile(entry.Label, seed, delivered, mixed, expected, decoder, geometry));
            }

            log?.Invoke(
                $"    seed {seed}: {slots.Count} of {population.Count * Ft8Step6Ladder.Seeds.Length} "
                + $"slots done, ours {slots.Count(s => s.OursReturned)}, "
                + $"upstream {slots.Count(s => s.UpstreamReturned)}");
        }

        return slots;
    }

    /// <summary>
    /// Writes one slot, reads it back with both decoders, and deletes it.
    /// </summary>
    /// <remarks>
    /// <b>Deleted per slot rather than at the end.</b> A slot is 360 KB and the -21 dB rung alone is
    /// 306 of them; held to the end that is 110 MB of scratch audio. It is written under
    /// <see cref="Path.GetTempPath"/>, never under the tree, and never committed.
    /// </remarks>
    internal static SlotOutcome ThroughTheFile(
        string label,
        int seed,
        double delivered,
        float[] mixed,
        string expected,
        Ft8SlotDecoder decoder,
        Ft8WaterfallGeometry geometry)
    {
        var path = Path.Combine(Path.GetTempPath(), $"ft8-unit227-{Guid.NewGuid():N}.wav");
        try
        {
            WavFile.Write(path, mixed, Rate);

            var quantised = ReadBack(path);
            var ours = decoder.Decode(new Ft8Monitor(geometry).Analyse(quantised));
            var oursReturned = ours.Texts.Contains(expected, StringComparer.Ordinal);
            var oursWrong = ours.Texts.Count(t => !string.Equals(t, expected, StringComparison.Ordinal));

            var upstream = Ft8Decoder.Decode(path);
            var upstreamReturned = upstream.Lines
                .Any(l => string.Equals(l.Text, expected, StringComparison.Ordinal));
            var upstreamWrong = upstream.Lines
                .Count(l => !string.Equals(l.Text, expected, StringComparison.Ordinal));

            return new SlotOutcome(
                label,
                seed,
                delivered,
                oursReturned,
                oursWrong,
                upstreamReturned,
                upstreamWrong,
                string.Join(" | ", upstream.Lines.Select(l => l.Raw.Trim())));
        }
        finally
        {
            WavFile.DeleteQuietly(path);
        }
    }

    /// <summary>
    /// The samples as upstream's <c>load_wav</c> hands them to its own decoder: the sixteen-bit
    /// count divided by 32768.
    /// </summary>
    /// <remarks>
    /// <b>32768 and not 32767, and it is upstream's asymmetry rather than a slip.</b> The write side
    /// scales by 32767 and the read side divides by 32768, so a round trip is very slightly quiet.
    /// Both decoders see it, because both read the same file, which is the whole reason the file is
    /// in the middle.
    /// </remarks>
    internal static float[] ReadBack(string path)
    {
        var contents = WavFile.Read(path);
        var samples = new float[contents.Samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = contents.Samples[i] / 32768.0f;
        }

        return samples;
    }
}
