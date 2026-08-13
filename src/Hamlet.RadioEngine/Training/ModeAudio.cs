namespace Hamlet.RadioEngine.Training;

/// <summary>Which sample the field guide is asking for.</summary>
/// <param name="Mode">Mode to demonstrate.</param>
/// <param name="WordsPerMinute">CW speed; ignored by other modes.</param>
/// <param name="Mistuned">
/// For SSB, render it off-frequency — the "ducks until you tune it right"
/// sound, which is the single most useful thing a newcomer can hear.
/// </param>
public readonly record struct AudioSampleRequest(
    TrainingMode Mode, int WordsPerMinute = 18, bool Mistuned = false);

/// <summary>
/// Generates what each mode sounds like, as audio, from the same model that
/// draws it on the waterfall.
/// </summary>
/// <remarks>
/// <para>Generated rather than recorded, on purpose (HM-DEC-027). Recorded
/// clips carry a licence and a provenance question into a GPL-3.0 repository,
/// cannot be parameterised, and cannot be asserted on. Generated audio is
/// licence-free, byte-for-byte deterministic, testable, and — the part that
/// matters for this operator — adjustable: CW at 12, 18 and 25 WPM is how
/// somebody finds their own copy speed, which is the groundwork FG-002
/// needs.</para>
/// <para>What is produced is the receiver's audio, not the radio-frequency
/// signal: the beat note a receiver gives you, in the few hundred hertz to
/// few kilohertz a speaker can reproduce. That is what the operator's ear
/// will actually meet on the air.</para>
/// <para>No clock is read and nothing is random. The same request always
/// produces the same samples (§5).</para>
/// </remarks>
public static class ModeAudio
{
    /// <summary>Sample rate of generated audio.</summary>
    public const int SampleRate = 48_000;

    /// <summary>The pitch a CW operator tunes for.</summary>
    public const double CwToneHz = 600;

    /// <summary>Peak amplitude, leaving headroom.</summary>
    private const double Peak = 0.32;

    /// <summary>How long a sample runs.</summary>
    /// <param name="mode">Mode being demonstrated.</param>
    /// <returns>Sample duration.</returns>
    /// <remarks>
    /// Long enough to hear the rhythm, short enough that nobody sits through
    /// it twice. FT8 gets a full fifteen-second cycle because its silence is
    /// half of what identifies it.
    /// </remarks>
    public static TimeSpan DurationFor(TrainingMode mode) => mode switch
    {
        TrainingMode.Ft8 => TimeSpan.FromSeconds(15),
        TrainingMode.Cw => TimeSpan.FromSeconds(8),
        _ => TimeSpan.FromSeconds(6),
    };

    /// <summary>
    /// Generate one sample as mono floats in [-1, 1].
    /// </summary>
    /// <param name="request">What to generate.</param>
    /// <returns>Audio samples at <see cref="SampleRate"/>.</returns>
    public static float[] Generate(AudioSampleRequest request)
    {
        var duration = DurationFor(request.Mode);
        var count = (int)(duration.TotalSeconds * SampleRate);
        var buffer = new float[count];

        switch (request.Mode)
        {
            case TrainingMode.Cw:
                RenderCw(buffer, request.WordsPerMinute);
                break;
            case TrainingMode.Ft8:
                RenderFt8(buffer);
                break;
            case TrainingMode.Rtty:
                RenderRtty(buffer);
                break;
            case TrainingMode.Psk31:
                RenderPsk31(buffer);
                break;
            case TrainingMode.Ssb:
                RenderSsb(buffer, request.Mistuned);
                break;
            default:
                break;
        }

        ApplyEdgeFade(buffer);
        return buffer;
    }

    /// <summary>
    /// Morse at the requested speed, with a soft-edged envelope.
    /// </summary>
    /// <remarks>
    /// The rise and fall on each element are what stop a keyed tone sounding
    /// like a series of clicks — real transmitters shape their keying, and
    /// hard edges would teach a newcomer a sound no station makes.
    /// </remarks>
    private static void RenderCw(float[] buffer, int wordsPerMinute)
    {
        var text = MorseCode.CqCall("W1AW");
        var pattern = MorseCode.KeyPattern(text);
        var totalDits = MorseCode.LengthInDits(text);
        var ditSeconds = MorseCode.Dit(wordsPerMinute).TotalSeconds;

        // 5 ms rise/fall, the usual shaping for a clean note.
        var edge = 0.005;
        var phase = 0.0;
        var step = 2 * Math.PI * CwToneHz / SampleRate;

        for (var i = 0; i < buffer.Length; i++)
        {
            var seconds = (double)i / SampleRate;
            var dits = seconds / ditSeconds;
            var keyDown = MorseCode.IsKeyDown(pattern, totalDits, 14, dits);

            // Distance in seconds to the nearest keying transition, for the
            // envelope shaping.
            var gate = keyDown ? 1.0 : 0.0;
            if (keyDown)
            {
                var beforeDits = Math.Max(0, dits - (edge / ditSeconds));
                var afterDits = dits + (edge / ditSeconds);
                if (!MorseCode.IsKeyDown(pattern, totalDits, 14, beforeDits))
                {
                    gate = Fraction(dits, beforeDits, edge / ditSeconds);
                }
                else if (!MorseCode.IsKeyDown(pattern, totalDits, 14, afterDits))
                {
                    gate = 1.0 - Fraction(dits, dits, edge / ditSeconds);
                    gate = Math.Clamp(gate, 0.2, 1.0);
                }
            }

            phase += step;
            buffer[i] = (float)(Math.Sin(phase) * Peak * gate);
        }
    }

    /// <summary>
    /// FT8: a run of steady tones stepping between eight frequencies, then
    /// silence for the rest of the fifteen-second slot.
    /// </summary>
    /// <remarks>
    /// The tone spacing here is FT8's own — 6.25 Hz — which is why it warbles
    /// rather than beeping: the steps are too small to hear as separate
    /// notes.
    /// </remarks>
    private static void RenderFt8(float[] buffer)
    {
        const double baseHz = 1000;
        const double spacingHz = 6.25;
        const double symbolSeconds = 0.16;

        var phase = 0.0;
        var transmitSeconds = SignalSynthesizer.Ft8Transmission.TotalSeconds;

        for (var i = 0; i < buffer.Length; i++)
        {
            var seconds = (double)i / SampleRate;
            if (seconds >= transmitSeconds)
            {
                buffer[i] = 0;
                continue;
            }

            var symbol = (int)(seconds / symbolSeconds);
            var tone = Hash(symbol, 7) % 8;
            var hz = baseHz + (tone * spacingHz);

            phase += 2 * Math.PI * hz / SampleRate;
            buffer[i] = (float)(Math.Sin(phase) * Peak);
        }
    }

    /// <summary>RTTY: two tones 170 Hz apart, switching at 45.45 baud.</summary>
    private static void RenderRtty(float[] buffer)
    {
        const double markHz = 2125;
        var spaceHz = markHz - SignalSynthesizer.RttyShiftHz;
        var phase = 0.0;

        for (var i = 0; i < buffer.Length; i++)
        {
            var seconds = (double)i / SampleRate;
            var bit = (long)(seconds * SignalSynthesizer.RttyBaud);
            var mark = Hash((int)bit, 13) % 2 == 0;
            var hz = mark ? markHz : spaceHz;

            phase += 2 * Math.PI * hz / SampleRate;
            buffer[i] = (float)(Math.Sin(phase) * Peak);
        }
    }

    /// <summary>
    /// PSK31: a continuous tone whose phase reverses at 31.25 baud, with the
    /// amplitude dipping through each reversal.
    /// </summary>
    /// <remarks>
    /// That dip is the whole sound. A hard phase flip would click; the
    /// cosine-shaped transition PSK31 actually uses is what makes it the soft
    /// warble the field guide promises.
    /// </remarks>
    private static void RenderPsk31(float[] buffer)
    {
        const double toneHz = 1000;
        const double baud = 31.25;

        var phase = 0.0;
        var polarity = 1.0;
        var lastSymbol = -1;

        for (var i = 0; i < buffer.Length; i++)
        {
            var seconds = (double)i / SampleRate;
            var symbolPosition = seconds * baud;
            var symbol = (int)symbolPosition;

            if (symbol != lastSymbol)
            {
                lastSymbol = symbol;
                if (Hash(symbol, 17) % 2 == 0)
                {
                    polarity = -polarity;
                }
            }

            // Amplitude follows a raised cosine across each symbol, dipping
            // to nothing where a reversal can hide.
            var within = symbolPosition - symbol;
            var envelope = 0.5 * (1 - Math.Cos(2 * Math.PI * within));

            phase += 2 * Math.PI * toneHz / SampleRate;
            buffer[i] = (float)(Math.Sin(phase) * Peak * envelope * polarity);
        }
    }

    /// <summary>
    /// SSB: stacked harmonics with a moving formant and a syllable envelope —
    /// speech-shaped without being speech.
    /// </summary>
    /// <param name="buffer">Destination.</param>
    /// <param name="mistuned">
    /// When true, every component is shifted by a few hundred hertz. That
    /// shift is exactly what a mistuned sideband does, and it is why SSB
    /// sounds like ducks until the dial is right — hearing the two back to
    /// back is the fastest way to learn what tuning for is.
    /// </param>
    private static void RenderSsb(float[] buffer, bool mistuned)
    {
        var shift = mistuned ? 260.0 : 0.0;
        var pitchBase = 118.0;

        for (var i = 0; i < buffer.Length; i++)
        {
            var seconds = (double)i / SampleRate;

            // Syllables inside phrases, with gaps for breath.
            var syllable = Math.Max(0, Math.Sin(seconds * 2 * Math.PI * 3.4));
            var phrase = seconds % 6.0 < 4.4 ? 1.0 : 0.0;
            var envelope = syllable * phrase;

            if (envelope <= 0.001)
            {
                buffer[i] = 0;
                continue;
            }

            // Pitch drifts as a voice does.
            var pitch = pitchBase + (14 * Math.Sin(seconds * 2 * Math.PI * 0.7));

            // Two formants sweeping slowly: the vowel changing.
            var f1 = 520 + (180 * Math.Sin(seconds * 2 * Math.PI * 1.3));
            var f2 = 1420 + (380 * Math.Sin((seconds * 2 * Math.PI * 0.9) + 1.1));

            var sample = 0.0;
            for (var h = 1; h <= 22; h++)
            {
                var hz = (pitch * h) + shift;
                if (hz > 3000)
                {
                    break;
                }

                // Energy concentrated near the formants.
                var d1 = (hz - f1) / 260.0;
                var d2 = (hz - f2) / 420.0;
                var gain = Math.Exp(-d1 * d1) + (0.6 * Math.Exp(-d2 * d2)) + (0.08 / h);

                sample += Math.Sin(2 * Math.PI * hz * seconds) * gain;
            }

            buffer[i] = (float)(sample * Peak * envelope * 0.16);
        }
    }

    /// <summary>Fade the first and last few milliseconds, so playback does
    /// not start or end with a click.</summary>
    private static void ApplyEdgeFade(float[] buffer)
    {
        var fade = Math.Min(SampleRate / 100, buffer.Length / 2);
        for (var i = 0; i < fade; i++)
        {
            var g = (float)i / fade;
            buffer[i] *= g;
            buffer[buffer.Length - 1 - i] *= g;
        }
    }

    private static double Fraction(double value, double from, double width)
        => width <= 0 ? 1.0 : Math.Clamp((value - from) / width, 0, 1);

    private static uint Hash(int a, int b)
    {
        unchecked
        {
            var h = (uint)a * 2654435761u;
            h ^= h >> 15;
            h = (h ^ (uint)b) * 2246822519u;
            h ^= h >> 13;
            return h;
        }
    }
}
