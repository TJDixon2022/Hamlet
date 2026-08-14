using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Training;

/// <summary>What CW signal to synthesize, and what to do to it on the way.</summary>
/// <param name="Text">What is sent.</param>
/// <param name="WordsPerMinute">Sending speed, PARIS standard.</param>
/// <param name="ToneHz">The pitch a receiver would give this signal.</param>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="Amplitude">Peak amplitude of the keyed tone.</param>
/// <param name="NoiseAmplitude">
/// Standard deviation of the added noise, in the same units as
/// <paramref name="Amplitude"/>. Zero is a clean signal.
/// </param>
/// <param name="FadeSeconds">
/// How long one rise-and-fall of the signal takes. Zero holds it steady.
/// </param>
/// <param name="FadeDepth">
/// How far down the fade goes, 0 to 1. At 1 the signal disappears completely
/// at the bottom of each cycle.
/// </param>
/// <param name="InterferenceHz">Pitch of a second station, or zero for none.</param>
/// <param name="InterferenceAmplitude">Peak amplitude of that second station.</param>
/// <param name="InterferenceWpm">How fast the second station is sending.</param>
/// <param name="LeadInSeconds">Silence before the first element.</param>
/// <param name="TailSeconds">Silence after the last.</param>
/// <param name="Seed">Seeds the noise, so the same request gives the same file.</param>
public readonly record struct CwSignalRequest(
    string Text,
    int WordsPerMinute = 18,
    double ToneHz = CwSignal.DefaultToneHz,
    int SampleRate = CwSignal.DefaultSampleRate,
    double Amplitude = 0.5,
    double NoiseAmplitude = 0,
    double FadeSeconds = 0,
    double FadeDepth = 0,
    double InterferenceHz = 0,
    double InterferenceAmplitude = 0,
    int InterferenceWpm = 22,
    double LeadInSeconds = 0.4,
    double TailSeconds = 0.4,
    int Seed = 20260814);

/// <summary>
/// Synthesizes real Morse from real text, at an exact speed, with the
/// impairments a signal actually suffers on the way.
/// </summary>
/// <remarks>
/// <para>THE TEST RIG THE DECODER IS BUILT AGAINST (HM-DEC-007, HM-DEC-027).
/// A decoder that has only ever run on live audio cannot be regression-tested,
/// because the signal that broke it is gone. This produces known text at a
/// known speed with a known amount of noise on it, byte for byte the same
/// every run, which is what turns "the decoder got that wrong" into a fixture
/// somebody can fix against.</para>
/// <para>It is also the training radio's voice. Somebody who has never copied
/// Morse can put a signal on at twelve words a minute and read along, with no
/// antenna, no license anxiety and nobody on the other end waiting.</para>
/// <para>Synthesized rather than recorded, which settles a question a public
/// GPL-3.0 repository would otherwise have to answer: an off-air recording
/// carries somebody else's callsign and somebody else's transmission
/// (§2.1). Nothing here belongs to anyone.</para>
/// <para>No clock is read and nothing is random in the ordinary sense. The
/// noise comes from a seeded generator, so a fixture regenerated next year is
/// identical to the one committed today (§5).</para>
/// </remarks>
public static class CwSignal
{
    /// <summary>
    /// The pitch used when nothing else is asked for.
    /// </summary>
    /// <remarks>
    /// The IC-7300's CW pitch is adjustable from 300 to 900 Hz, and CI-V
    /// command <c>14 08</c> encodes exactly that range with 600 Hz at its
    /// midpoint (Full Manual, section 19, p. 19-3, and p. 4-14). So 600 is the
    /// middle of what this radio can do rather than a number carried in from
    /// general knowledge, which is the standard §4 sets.
    /// </remarks>
    public const double DefaultToneHz = 600;

    /// <summary>
    /// Sample rate for generated signals.
    /// </summary>
    /// <remarks>
    /// Eight kilohertz, not the forty-eight the radio's codec will deliver.
    /// A CW tone lives between 300 and 900 Hz and needs nothing above four
    /// kilohertz to be reconstructed exactly, so this is six times smaller with
    /// nothing lost, and fixture size is the difference between a folder that
    /// can live in the repository and one that cannot. The decoder derives
    /// everything from sample counts, so it neither knows nor cares which rate
    /// it was handed.
    /// </remarks>
    public const int DefaultSampleRate = 8_000;

    /// <summary>Rise and fall time of each keyed element, in seconds.</summary>
    /// <remarks>
    /// Five milliseconds, which is the usual shaping for a clean note. A hard
    /// edge would click across the band and would teach a decoder a transient
    /// no real transmitter produces.
    /// </remarks>
    private const double EdgeSeconds = 0.005;

    /// <summary>Generate the signal.</summary>
    /// <param name="request">What to send and what to do to it.</param>
    /// <returns>Mono audio at the requested rate.</returns>
    public static MonoAudio Generate(CwSignalRequest request)
    {
        var sampleRate = Math.Max(1_000, request.SampleRate);
        var ditSeconds = MorseCode.Dit(request.WordsPerMinute).TotalSeconds;
        var edges = EdgeTimes(request.Text, ditSeconds, request.LeadInSeconds);

        var totalSeconds = edges.Length > 0
            ? edges[^1] + request.TailSeconds
            : request.LeadInSeconds + request.TailSeconds;

        var count = Math.Max(1, (int)Math.Round(totalSeconds * sampleRate));
        var samples = new float[count];

        RenderKeyedTone(
            samples, sampleRate, edges, request.ToneHz, request.Amplitude,
            request.FadeSeconds, request.FadeDepth);

        if (request.InterferenceHz > 0 && request.InterferenceAmplitude > 0)
        {
            AddInterference(samples, sampleRate, request);
        }

        if (request.NoiseAmplitude > 0)
        {
            AddNoise(samples, request.NoiseAmplitude, request.Seed);
        }

        return new MonoAudio(sampleRate, samples);
    }

    /// <summary>
    /// How long a message takes to send, silence at either end included.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The duration.</returns>
    public static TimeSpan DurationOf(CwSignalRequest request)
    {
        var ditSeconds = MorseCode.Dit(request.WordsPerMinute).TotalSeconds;
        var dits = MorseCode.LengthInDits(request.Text);
        return TimeSpan.FromSeconds(
            request.LeadInSeconds + (dits * ditSeconds) + request.TailSeconds);
    }

    /// <summary>
    /// The instants at which the key changes state, starting with the first
    /// key-down and ending with the last key-up.
    /// </summary>
    private static double[] EdgeTimes(string text, double ditSeconds, double leadInSeconds)
    {
        var pattern = MorseCode.KeyPattern(text);
        var edges = new double[pattern.Count + 1];
        edges[0] = leadInSeconds;

        for (var i = 0; i < pattern.Count; i++)
        {
            edges[i + 1] = edges[i] + (pattern[i] * ditSeconds);
        }

        return edges;
    }

    /// <summary>
    /// Write the keyed tone, shaping each element's edges and applying the
    /// slow fade if one was asked for.
    /// </summary>
    private static void RenderKeyedTone(
        float[] samples,
        int sampleRate,
        double[] edges,
        double toneHz,
        double amplitude,
        double fadeSeconds,
        double fadeDepth)
    {
        if (edges.Length < 2)
        {
            return;
        }

        var step = 2 * Math.PI * toneHz / sampleRate;
        var phase = 0.0;
        var segment = 0;
        var depth = Math.Clamp(fadeDepth, 0, 1);

        for (var i = 0; i < samples.Length; i++)
        {
            var t = (double)i / sampleRate;
            phase += step;

            // Advance to the segment holding this instant. The cursor only
            // moves forward, so the whole render stays linear in the sample
            // count rather than searching per sample.
            while (segment < edges.Length - 1 && t >= edges[segment + 1])
            {
                segment++;
            }

            if (segment >= edges.Length - 1)
            {
                continue;
            }

            // Segments alternate, and the pattern starts with the key down.
            if (segment % 2 != 0)
            {
                continue;
            }

            var gate = Gate(t, edges[segment], edges[segment + 1]);
            if (gate <= 0)
            {
                continue;
            }

            var fade = fadeSeconds > 0
                ? 1 - (depth * 0.5 * (1 - Math.Cos(2 * Math.PI * t / fadeSeconds)))
                : 1.0;

            samples[i] += (float)(Math.Sin(phase) * amplitude * gate * fade);
        }
    }

    /// <summary>
    /// A raised-cosine envelope over one keyed element.
    /// </summary>
    /// <remarks>
    /// Taking the smaller of the rise and the fall means an element shorter
    /// than two edge times never reaches full amplitude, which is what a real
    /// transmitter does at high speed and is worth reproducing: it is one of
    /// the reasons fast CW is harder to copy.
    /// </remarks>
    private static double Gate(double t, double start, double end)
    {
        var rise = Math.Clamp((t - start) / EdgeSeconds, 0, 1);
        var fall = Math.Clamp((end - t) / EdgeSeconds, 0, 1);
        var shape = Math.Min(rise, fall);
        return 0.5 * (1 - Math.Cos(Math.PI * shape));
    }

    /// <summary>A second station a few hundred hertz away, sending its own text.</summary>
    private static void AddInterference(
        float[] samples, int sampleRate, CwSignalRequest request)
    {
        var ditSeconds = MorseCode.Dit(request.InterferenceWpm).TotalSeconds;

        // Different text and a different start offset, so the two stations are
        // not accidentally keying in step with one another.
        var edges = EdgeTimes("DE DL1ABC UP", ditSeconds, 0.05);

        RenderKeyedTone(
            samples, sampleRate, edges, request.InterferenceHz,
            request.InterferenceAmplitude, 0, 0);
    }

    /// <summary>
    /// Add white noise from a seeded generator.
    /// </summary>
    /// <remarks>
    /// Box-Muller over a small xorshift, rather than <c>Random</c>. The point
    /// is not statistical purity, it is that a fixture regenerated on another
    /// machine in another year is the same file: <c>Random</c>'s unseeded
    /// behavior and its seeded sequence have both changed between .NET
    /// versions, and a fixture that quietly changed would take its assertions
    /// with it (§5).
    /// </remarks>
    private static void AddNoise(float[] samples, double amplitude, int seed)
    {
        var state = (uint)(seed == 0 ? 1 : seed);

        double NextUniform()
        {
            // xorshift32, and never zero, which Box-Muller's logarithm needs.
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return ((state & 0xFFFFFF) + 1) / 16777217.0;
        }

        for (var i = 0; i < samples.Length; i += 2)
        {
            var u1 = NextUniform();
            var u2 = NextUniform();
            var magnitude = amplitude * Math.Sqrt(-2 * Math.Log(u1));

            samples[i] += (float)(magnitude * Math.Cos(2 * Math.PI * u2));

            if (i + 1 < samples.Length)
            {
                samples[i + 1] += (float)(magnitude * Math.Sin(2 * Math.PI * u2));
            }
        }
    }
}
