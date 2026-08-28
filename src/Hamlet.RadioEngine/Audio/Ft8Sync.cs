namespace Hamlet.RadioEngine.Audio;

/// <summary>Somewhere a transmission may be, and how strongly it syncs.</summary>
/// <param name="FrequencyHz">The lowest of the eight tones.</param>
/// <param name="TimeOffsetSeconds">
/// How far into the slot the transmission starts.
/// </param>
/// <param name="Score">
/// How much stronger the expected tone was than the average of the eight, over
/// the twenty-one sync symbols. One means nothing there; eight is the most a
/// noiseless signal could give.
/// </param>
/// <remarks>
/// **A CANDIDATE IS NOT A MESSAGE AND MUST NEVER BE SHOWN AS ONE** (§0.0). This
/// says a signal with FT8's sync pattern appears to be here. It says nothing at
/// all about what was sent, and nothing goes on the decoded-text panel from it.
/// </remarks>
public sealed record SyncCandidate(
    double FrequencyHz, double TimeOffsetSeconds, double Score);

/// <summary>
/// Finds where in a slot an FT8 transmission starts, by its sync pattern.
/// </summary>
/// <remarks>
/// <para>**THE FIRST STAGE OF A DECODER, AND ONLY THE FIRST** (work instruction
/// 042, task 7). An FT8 transmission is seventy-nine symbols long and three of
/// its blocks are known in advance: the same seven-symbol Costas array sits at
/// the beginning, the middle and the end. Finding those three tells you where a
/// transmission is in time and frequency, which is what everything after this
/// needs and is not itself a decode.</para>
/// <para>**IT RUNS ON THE FRAMES AND NOT ON THE DRAWN BITMAP.** The waterfall is
/// a picture made for a person, with a floor tracker and a contrast range chosen
/// so the eye can read it; measuring off it would be measuring the drawing
/// rather than the signal.</para>
/// <para>**WHY THE COSTAS ARRAY WORKS.** Seven symbols, each a different one of
/// the seven tones it uses, arranged so that sliding it against itself in time
/// or frequency lines up at most one symbol. So a wrong guess about where a
/// transmission starts scores about as well as noise, and the right one stands
/// far above it. That is the property, and it is why the search can be a brute
/// sweep rather than anything clever.</para>
/// </remarks>
public static class Ft8Sync
{
    /// <summary>The seven-symbol sync array, as tone numbers.</summary>
    public static IReadOnlyList<int> Costas { get; } =
        new[] { 3, 1, 4, 0, 6, 5, 2 };

    /// <summary>Which symbol each Costas block starts at.</summary>
    public static IReadOnlyList<int> CostasAt { get; } = new[] { 0, 36, 72 };

    /// <summary>How far apart the eight tones are.</summary>
    public const double ToneSpacingHz = 6.25;

    /// <summary>How many tones the mode uses.</summary>
    public const int Tones = 8;

    /// <summary>How long one symbol lasts, which is one over the spacing.</summary>
    public const double SymbolSeconds = 1.0 / ToneSpacingHz;

    /// <summary>How many symbols a transmission runs for.</summary>
    public const int SymbolsPerTransmission = 79;

    /// <summary>How many steps of the search grid make one tone spacing.</summary>
    /// <remarks>
    /// Two, so the sweep steps by half a tone. A transmission does not oblige by
    /// landing on a grid, and a whole-spacing grid can sit a full half tone away
    /// from one, which costs most of the sync score.
    /// </remarks>
    public const int StepsPerTone = 2;

    /// <summary>How many search steps make one symbol in time.</summary>
    public const int StepsPerSymbol = 4;

    /// <summary>The lowest score worth reporting.</summary>
    /// <remarks>
    /// **MEASURED, NOT REASONED.** Pure noise gives about one on average and its
    /// best grid point across a whole slot reaches well above that, because the
    /// sweep tries tens of thousands of positions and takes the luckiest. So the
    /// floor is set from what an empty band actually reaches, with room above
    /// it, and `WhatAnEmptyBandScores` holds it there. A real transmission at
    /// three times the noise amplitude scores near eight, so the separation is
    /// not close.
    /// </remarks>
    public const double DefaultFloor = 4.0;

    /// <summary>The lowest audio frequency searched.</summary>
    public const double LowHz = 200;

    /// <summary>The highest.</summary>
    public const double HighHz = 3000;

    /// <summary>
    /// Search a slot for transmissions.
    /// </summary>
    /// <param name="slot">One slot of audio.</param>
    /// <param name="most">How many candidates to report at most.</param>
    /// <param name="floor">
    /// The lowest score worth reporting. One is what pure noise gives.
    /// </param>
    /// <returns>Candidates, strongest first.</returns>
    /// <remarks>
    /// <para>**THE SCORE IS A RATIO AND NOT A DECIBEL FIGURE**, deliberately. It
    /// is how much stronger the tone the Costas array calls for was than the
    /// average across all eight, so it needs no reference level and no noise
    /// measurement, and it cannot be made to look good by a loud band.</para>
    /// <para>**NEARBY CANDIDATES ARE SUPPRESSED**, because a strong signal scores
    /// well at every grid point around itself and reporting all of them would
    /// turn one station into a dozen. The strongest wins its neighbourhood.</para>
    /// </remarks>
    public static IReadOnlyList<SyncCandidate> Search(
        MonoAudio slot, int most = 20, double floor = DefaultFloor)
    {
        ArgumentNullException.ThrowIfNull(slot);

        var rate = slot.SampleRate;

        if (rate <= 0 || slot.Samples.Length == 0)
        {
            return Array.Empty<SyncCandidate>();
        }

        var perSymbol = (int)Math.Round(SymbolSeconds * rate);
        var hop = perSymbol / StepsPerSymbol;

        if (hop <= 0 || slot.Samples.Length < perSymbol)
        {
            return Array.Empty<SyncCandidate>();
        }

        var step = ToneSpacingHz / StepsPerTone;
        var bins = (int)Math.Floor((HighHz - LowHz) / step) + 1;
        var frames = ((slot.Samples.Length - perSymbol) / hop) + 1;

        if (bins < (Tones - 1) * StepsPerTone + 1 || frames < 1)
        {
            return Array.Empty<SyncCandidate>();
        }

        var power = Spectrogram(slot.Samples, rate, perSymbol, hop, frames, bins, step);

        // The last Costas block starts 72 symbols in and runs seven, so a
        // transmission needs 79 symbols of frames behind its start.
        var lastStart = frames - (SymbolsPerTransmission * StepsPerSymbol);
        var topBin = bins - ((Tones - 1) * StepsPerTone) - 1;

        var found = new List<SyncCandidate>();

        for (var start = 0; start <= lastStart; start++)
        {
            for (var bin = 0; bin <= topBin; bin++)
            {
                var score = Score(power, bins, start, bin);

                if (score >= floor)
                {
                    found.Add(new SyncCandidate(
                        LowHz + (bin * step),
                        start * hop / (double)rate,
                        score));
                }
            }
        }

        return Strongest(found, most, step);
    }

    /// <summary>How well a transmission starting here matches the sync array.</summary>
    private static double Score(float[] power, int bins, int start, int bin)
    {
        var total = 0.0;

        foreach (var at in CostasAt)
        {
            for (var symbol = 0; symbol < Costas.Count; symbol++)
            {
                var frame = start + ((at + symbol) * StepsPerSymbol);
                var row = frame * bins;

                var across = 0.0;

                for (var tone = 0; tone < Tones; tone++)
                {
                    across += power[row + bin + (tone * StepsPerTone)];
                }

                if (across <= 0)
                {
                    continue;
                }

                var wanted = power[row + bin + (Costas[symbol] * StepsPerTone)];

                total += wanted * Tones / across;
            }
        }

        return total / (CostasAt.Count * Costas.Count);
    }

    /// <summary>
    /// The strongest candidates, with their own neighbours suppressed.
    /// </summary>
    private static IReadOnlyList<SyncCandidate> Strongest(
        List<SyncCandidate> found, int most, double step)
    {
        var kept = new List<SyncCandidate>();

        // One tone spacing apart in frequency and half a second in time. Closer
        // than that and it is the same signal seen twice, which would turn one
        // station into a dozen on the operator's screen.
        var apartHz = ToneSpacingHz;
        var apartSeconds = 0.5;

        foreach (var candidate in found.OrderByDescending(c => c.Score))
        {
            if (kept.Count >= most)
            {
                break;
            }

            var crowded = kept.Any(
                k => Math.Abs(k.FrequencyHz - candidate.FrequencyHz) < apartHz
                     && Math.Abs(k.TimeOffsetSeconds - candidate.TimeOffsetSeconds)
                        < apartSeconds);

            if (!crowded)
            {
                kept.Add(candidate);
            }
        }

        _ = step;
        return kept;
    }

    /// <summary>
    /// Power at every grid frequency in every frame, by Goertzel.
    /// </summary>
    /// <remarks>
    /// <para>**A GOERTZEL BANK RATHER THAN A TRANSFORM** (§6). The grid is a few
    /// hundred known frequencies spaced to suit the mode, and a transform would
    /// have to be padded to a length whose bins happened to land on them. This
    /// evaluates exactly the frequencies wanted, at exactly the spacing the mode
    /// uses, with nothing to interpolate afterwards.</para>
    /// <para>The window is one symbol long, which is what makes a symbol
    /// separable from its neighbours at all: one over the symbol length is the
    /// tone spacing, so the mode is built to be read this way.</para>
    /// </remarks>
    private static float[] Spectrogram(
        float[] samples, int rate, int perSymbol, int hop,
        int frames, int bins, double step)
    {
        var power = new float[frames * bins];

        // Goertzel coefficients, once, because they depend only on the
        // frequency and the window.
        var coefficient = new double[bins];

        for (var bin = 0; bin < bins; bin++)
        {
            var hertz = LowHz + (bin * step);
            coefficient[bin] = 2.0 * Math.Cos(2.0 * Math.PI * hertz / rate);
        }

        for (var frame = 0; frame < frames; frame++)
        {
            var from = frame * hop;
            var row = frame * bins;

            for (var bin = 0; bin < bins; bin++)
            {
                var c = coefficient[bin];
                var s1 = 0.0;
                var s2 = 0.0;

                for (var i = 0; i < perSymbol; i++)
                {
                    var s0 = samples[from + i] + (c * s1) - s2;
                    s2 = s1;
                    s1 = s0;
                }

                power[row + bin] = (float)((s1 * s1) + (s2 * s2) - (c * s1 * s2));
            }
        }

        return power;
    }
}
