using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The key-down run lengths in a recording, measured outside the decoder.
/// </summary>
/// <param name="RunsMs">Every key-down run, in milliseconds, in order.</param>
/// <param name="MedianMs">The middle one.</param>
/// <param name="SwingDb">
/// How far the envelope moves between its tenth and ninetieth percentile.
/// **Read it beside the element share and never alone**: where a pitch sits
/// outside the receiver's own filter there is almost nothing there at all, the
/// tenth percentile approaches zero, and the figure runs to eighty or ninety
/// decibels while measuring silence.
/// </param>
/// <param name="ElementShare">
/// What fraction of the stretch was spent keyed down for an element's length.
/// </param>
/// <param name="ElementPurity">
/// What fraction of the key-downs were element length rather than chatter.
/// </param>
/// <param name="Duty">
/// What fraction of the stretch was spent with the key down at all, between
/// nought and one.
/// </param>
/// <remarks>
/// <para>**THE DUTY PREDICTED EVERY OUTCOME OF THE SHACK EVENING OF 2026-08-25
/// AND NOTHING WAS RECORDING IT.** Thirteen captures, all on 40 m, all at the
/// same input level, tone locked within a few hertz on twelve of the thirteen.
/// Sorted by this one number the results sort themselves: ten captures between
/// 38 and 47 per cent read back with nought to eight characters unsure; one at
/// 24 per cent buried its real content in forty-eight characters of noise; one
/// at 18 per cent gave eight seconds of station and twenty-two of invented
/// text.</para>
/// <para>**IT IS THE SHARE OF ALL KEY-DOWN AND NOT OF ELEMENT-LENGTH KEY-DOWN**,
/// which is what <see cref="KeyingProfile.ElementShare"/> already measures and is
/// a different question. That one asks how much of the stretch looked like
/// Morse; this asks how much of it was loud, whatever shape it was. On a rag
/// chew the two are close; on a calling frequency full of chatter they are not,
/// and it is the second that says how much of the file is silence.</para>
/// <para>**IT SAYS NOTHING ABOUT WHETHER A FIST IS A FIST.** Measured on this
/// repository's own W1AW bulletin captures, the station's own bin runs 47 to 70
/// per cent, because a bulletin is continuous traffic and a call is not. Duty is
/// a fact about what somebody is sending, not about whether they are sending
/// well.</para>
/// </remarks>
public readonly record struct KeyingProfile(
    IReadOnlyList<double> RunsMs,
    double MedianMs,
    double SwingDb,
    double ElementShare,
    double ElementPurity,
    double Duty = 0)
{
    /// <summary>How much this pitch looks like somebody keying, from nought to one.</summary>
    /// <remarks>
    /// <para>**BOTH HALVES ARE NEEDED AND EACH ALONE PICKS THE WRONG PITCH.**
    /// Measured on this repository's one recording known to contain a readable
    /// station, keying at 500 Hz: the share alone chose 425 Hz, where the envelope
    /// crosses its threshold two hundred times in six seconds and enough of those
    /// crossings happen to last twenty milliseconds to beat the real pitch. The
    /// purity alone is won by any pitch with two runs in it.</para>
    /// <para>Together they say: most of this stretch was spent keyed down for an
    /// element's length, **and** most of the key-downs were elements rather than
    /// a gate chattering. At the true pitch that scored 0.29 and no other
    /// candidate reached 0.10.</para>
    /// </remarks>
    public double Score => ElementShare * ElementPurity;
}

/// <summary>What the best candidate pitch in a stretch of audio measured.</summary>
/// <param name="ToneHz">The pitch.</param>
/// <param name="Profile">What its envelope did.</param>
public readonly record struct KeyingSighting(double ToneHz, KeyingProfile Profile);

/// <summary>
/// A second opinion about what is in a recording, built from nothing the decoder
/// owns (HM-DEC-091, §12.5).
/// </summary>
/// <remarks>
/// <para>**THIS EXISTS BECAUSE A DIAGNOSIS WAS DRAWN FROM COUNTERS THAT WERE
/// COUNTING THE WRONG THING.** Two recordings read as nothing, a session
/// concluded the speed tracker could not lock on a sloppy fist, and the
/// measurement that settled it was taken outside this repository and could not be
/// reproduced inside it. An analysis nobody can re-run is an argument, which is
/// the same thing §0.0.1 says about a decode with no audio behind it.</para>
/// <para>**IT SHARES NO CODE WITH THE DECODER ON PURPOSE.** A measurement taken
/// with the instrument under test cannot referee it, and this project has twice
/// certified a fault with a fixture built from the same misunderstanding as the
/// code (§12.5). Quadrature mixdown, a one-pole smoother, a threshold from the
/// envelope's own percentiles: no Goertzel bank, no gate, no tracker.</para>
/// <para>Fifteen hundred key-downs at a six millisecond median is a threshold
/// being crossed by noise. Two hundred at forty-eight is somebody sending.</para>
/// <para>**IT LIVES IN THE ENGINE RATHER THAN THE TEST PROJECT** because the
/// meter that runs it beside a live decoder needs it (§0.1). Sharing no code with
/// the decoder is a property of what it computes and not of where it sits: there
/// is no Goertzel bank here, no gate, no tracker and no reference to any of
/// them.</para>
/// </remarks>
public static class KeyingEnvelope
{
    /// <summary>The lowest pitch a sweep looks at, in hertz.</summary>
    /// <remarks>
    /// Wider than the radio's own CW pitch range of 300 to 900 Hz at the top and
    /// narrower at the bottom (§4). **A sweep is not a claim about where the
    /// station is**, and a candidate below four hundred sits where this receiver's
    /// own low-frequency rumble lives.
    /// </remarks>
    public const double LowestToneHz = 400;

    /// <summary>The highest pitch a sweep looks at, in hertz.</summary>
    public const double HighestToneHz = 1200;

    /// <summary>How far apart the candidates are, in hertz.</summary>
    public const double ToneStepHz = 25;

    /// <summary>
    /// The pitch in a stretch of audio with the most keying contrast, and what it
    /// measured.
    /// </summary>
    /// <param name="audio">The stretch.</param>
    /// <returns>The tone and its profile, or null when there is no audio.</returns>
    /// <remarks>
    /// <para>**THE CANDIDATE THAT SPENT THE MOST OF THE STRETCH KEYED DOWN FOR AN
    /// ELEMENT'S LENGTH WINS.** Two other rankings were tried and measured
    /// first, and both are wrong in ways worth writing down. Ranking by the
    /// longest runs hands the sweep to a carrier, which produces one enormous
    /// key-down and no keying at all. **Ranking by the widest envelope swing was
    /// tried and measured and it is worse**: on the one recording in this
    /// repository known to contain a readable station, keying at 500 Hz with a
    /// 57 ms dit, it chose 700 and 800 Hz in every window and reported a four
    /// millisecond median, because a pitch outside the receiver's own filter has
    /// almost nothing in it, so its quiet tenth approaches zero and the decibel
    /// difference runs to ninety while measuring silence.</para>
    /// <para>**RANKING BY THE ELEMENT SHARE ALONE WAS TRIED AND MEASURED TOO,
    /// AND IT ALSO PICKED THE WRONG PITCH**, choosing 425 Hz on that recording in
    /// every window and reporting a seven millisecond median. Off the station's
    /// own pitch the envelope crosses its threshold two hundred times in six
    /// seconds, and enough of those crossings last twenty milliseconds to carry
    /// the share past the real answer.</para>
    /// <para>What separates them is <see cref="KeyingProfile.Score"/>, which asks
    /// the second question as well: were most of the key-downs elements, or was
    /// most of it chatter with some elements in it. At 500 Hz that scored 0.29 and
    /// nothing else in eight hundred hertz of candidates reached 0.10.</para>
    /// <para>**AND IT SWEEPS RATHER THAN ASKING THE DECODER.** The decoder chose
    /// 800 Hz on a recording whose narrow content sat at 608; a meter that
    /// inherited that choice could only ever agree with it, and disagreeing with
    /// it is the entire job.</para>
    /// </remarks>
    public static KeyingSighting? Best(MonoAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);

        if (audio.Samples.Length == 0)
        {
            return null;
        }

        KeyingSighting? best = null;

        for (var tone = LowestToneHz; tone <= HighestToneHz; tone += ToneStepHz)
        {
            var profile = Measure(audio, tone);

            if (best is null || profile.Score > best.Value.Profile.Score)
            {
                best = new KeyingSighting(tone, profile);
            }
        }

        return best;
    }

    /// <summary>How often the envelope is read, in milliseconds.</summary>
    public const double StepMs = 1;

    /// <summary>Where the smoother rolls off, in hertz.</summary>
    public const double SmoothingHz = 100;

    /// <summary>The shortest key-down that could be an element, in milliseconds.</summary>
    /// <remarks>
    /// Twenty milliseconds is a dit at sixty words a minute, which is faster than
    /// anybody sends by hand. Below it is the gate chattering rather than anybody
    /// keying.
    /// </remarks>
    public const double ShortestElementMs = 20;

    /// <summary>The longest key-down that could be an element, in milliseconds.</summary>
    /// <remarks>
    /// Half a second is a dah at about three and a half words a minute. Beyond it
    /// the thing being measured is a carrier, a fade or a tuning note, none of
    /// which is somebody sending.
    /// </remarks>
    public const double LongestElementMs = 500;

    /// <summary>Measure the keying in a recording at one pitch.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">The pitch to listen at.</param>
    /// <returns>The profile.</returns>
    /// <exception cref="ArgumentNullException">No audio.</exception>
    public static KeyingProfile Measure(MonoAudio audio, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var envelope = Envelope(audio, toneHz);

        if (envelope.Count == 0)
        {
            return new KeyingProfile(Array.Empty<double>(), 0, 0, 0, 0, 0);
        }

        var sorted = envelope.OrderBy(v => v).ToArray();
        var low = sorted[(int)(sorted.Length * 0.10)];
        var high = sorted[(int)(sorted.Length * 0.90)];

        // **MIDWAY IN AMPLITUDE, WHICH IS NOT MIDWAY IN DECIBELS.** Halfway
        // between a quiet tenth and a loud tenth on a linear scale sits about six
        // decibels under the loud one; halfway on a logarithmic scale sits at
        // their geometric mean, which on this recording lands close enough to the
        // noise to be crossed seventeen hundred times by nothing at all. The
        // first is also where this project already takes an element's edge
        // (HM-DEC-105, HM-DEC-119), for the same reason.
        var threshold = (low + high) / 2;

        var runs = new List<double>();
        var run = 0;

        foreach (var value in envelope)
        {
            if (value >= threshold)
            {
                run++;
                continue;
            }

            if (run > 0)
            {
                runs.Add(run * StepMs);
            }

            run = 0;
        }

        if (run > 0)
        {
            runs.Add(run * StepMs);
        }

        var median = runs.Count == 0
            ? 0
            : runs.OrderBy(v => v).ElementAt(runs.Count / 2);

        // **HOW MUCH OF THIS STRETCH LOOKS LIKE MORSE**, which is the one figure
        // here that survives being asked of the wrong pitch. Anything under
        // twenty milliseconds is shorter than an element at any speed anybody
        // sends and is chatter; anything over half a second is longer than a dah
        // at any speed and is a carrier or a fade. What is left is elements.
        var elements = runs
            .Where(r => r is >= ShortestElementMs and <= LongestElementMs)
            .ToList();

        var span = envelope.Count * StepMs;

        return new KeyingProfile(
            runs,
            median,
            Decibels(high) - Decibels(low),
            elements.Sum() / span,
            runs.Count == 0 ? 0 : (double)elements.Count / runs.Count,
            runs.Sum() / span);
    }

    private static double Decibels(double magnitude)
        => 20 * Math.Log10(Math.Max(magnitude, 1e-12));

    private static List<double> Envelope(MonoAudio audio, double toneHz)
    {
        var rate = audio.SampleRate;
        var step = Math.Max(1, (int)Math.Round(rate * StepMs / 1000.0));

        // **A HUNDRED HERTZ OF SMOOTHING IS A TEN MILLISECOND WINDOW**, and a
        // boxcar of that length over the quadrature arms is exactly a Goertzel of
        // that bandwidth. A one-pole of the same nominal corner was tried first
        // and left nine hundred crossings under ten milliseconds on this
        // recording, which is the smoother's own ripple counted as keying: the
        // shape of a filter is part of what it measures (HM-DEC-119).
        var window = Math.Max(1, (int)Math.Round(rate / SmoothingHz));
        var omega = 2 * Math.PI * toneHz / rate;

        // **A ROTATING PHASOR RATHER THAN A COSINE PER SAMPLE, AND A RING RATHER
        // THAN TWO FULL-LENGTH ARRAYS.** This runs thirty-three times a second
        // beside a live decoder (§8's never-throw discipline binds here for the
        // same reason it binds on the decoder's own record): the trigonometry was
        // most of the cost and the arrays were four megabytes a candidate. The
        // phasor is renormalized every window, because rounding walks its length
        // away from one over a few hundred thousand rotations.
        var stepCos = Math.Cos(omega);
        var stepSin = Math.Sin(omega);

        double phaseCos = 1;
        double phaseSin = 0;

        var ringCos = new double[window];
        var ringSin = new double[window];

        double inPhase = 0;
        double quadrature = 0;

        var envelope = new List<double>((audio.Samples.Length / step) + 1);

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            var sample = audio.Samples[i];
            var slot = i % window;

            inPhase += (sample * phaseCos) - ringCos[slot];
            quadrature += (sample * -phaseSin) - ringSin[slot];

            ringCos[slot] = sample * phaseCos;
            ringSin[slot] = sample * -phaseSin;

            var nextCos = (phaseCos * stepCos) - (phaseSin * stepSin);

            phaseSin = (phaseSin * stepCos) + (phaseCos * stepSin);
            phaseCos = nextCos;

            if (slot == window - 1)
            {
                var length = Math.Sqrt((phaseCos * phaseCos) + (phaseSin * phaseSin));

                phaseCos /= length;
                phaseSin /= length;
            }

            if (i % step != 0)
            {
                continue;
            }

            envelope.Add(
                Math.Sqrt((inPhase * inPhase) + (quadrature * quadrature)) / window);
        }

        return envelope;
    }
}
