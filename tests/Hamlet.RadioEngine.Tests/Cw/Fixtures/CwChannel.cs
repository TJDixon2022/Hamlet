using System.Globalization;
using System.Numerics;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>One CH-* channel profile as CW_SPEC.md 9.1 states it.</summary>
/// <param name="Id">The profile's name, <c>CH-LM</c> and so on.</param>
/// <param name="Condition">Latitude band and condition, in 9.1's words.</param>
/// <param name="DelayMs">Differential delay between the two paths; nought for CH-AWGN.</param>
/// <param name="SpreadHz">Frequency spread, two sigma of each path's Gaussian Doppler spectrum; nought for CH-AWGN.</param>
/// <param name="Status">9.1's status column, carried into every recipe: <c>confirmed</c>, <c>[verify]</c> or <c>project baseline</c>.</param>
public sealed record CwChannelProfile(
    string Id, string Condition, double DelayMs, double SpreadHz, string Status)
{
    /// <summary>Whether this profile fades: two paths, or CH-AWGN's one unfaded path.</summary>
    public bool Fades => SpreadHz > 0;
}

/// <summary>One generated channel case.</summary>
/// <param name="Audio">What the receiver hands over, 8000 samples a second.</param>
/// <param name="Key">The text keyed, exact by construction.</param>
/// <param name="Sidecar">The recipe that rebuilds it, and what it is.</param>
public sealed record CwChannelCase(MonoAudio Audio, string Key, string Sidecar);

/// <summary>
/// The CH-* channel profiles, generated over the project's shaped noise band
/// (work instruction 461; PHASE_PLAN.md 7.1; CW_SPEC.md 8.1 and 9.1; V-06).
/// </summary>
/// <remarks>
/// <para>**TWO INDEPENDENTLY FADING PATHS OF EQUAL POWER, A GAUSSIAN DOPPLER SPECTRUM,
/// A DIFFERENTIAL DELAY** - Watterson's model as ITU-R F.1487 tabulates it:
/// <c>Re{ A [h1(t) e(t) + h2(t) e(t - tau)] exp(j 2 pi f0 t) }</c>, with
/// <c>e</c> the keyed envelope exactly as <see cref="CwFixtureGenerator"/> shapes
/// it, and each <c>hk</c> a complex Gaussian process of half the unfaded power, so
/// the two paths are equal and their sum is the unfaded signal. CH-AWGN is one
/// path with <c>h1 = 1</c>.</para>
/// <para>**THE NOISE GOES IN AFTER THE FADING AND IS NEVER FADED.** It is the
/// receiver's band, <see cref="CwFixtureGenerator.ShapedNoise"/>, at half its
/// level so a faded peak cannot clip; there is no digital silence anywhere
/// (V-06).</para>
/// <para>**THE SNR IS IN THE 2500 Hz REFERENCE (8.1)**: the tone's unfaded mean
/// power over the band's density beside the tone times 2500 Hz. The density is
/// the three biquads' own response, which the unit's task 1 measured to agree
/// with the band's output within 0.05 dB.</para>
/// <para>**WHAT THESE FIXTURES DO NOT PROVE** (CLAUDE.md 12.5, V-04): they are the
/// published model, not the band; every value marked <c>[verify]</c> is
/// unvendored (CW_SPEC 12 item 2). The recipe is
/// <c>docs/phase-requirements/channels.md</c>.</para>
/// </remarks>
public static class CwChannel
{
    /// <summary>Samples a second of every case, the generator's.</summary>
    public const int SampleRate = CwFixtureGenerator.SampleRate;

    /// <summary>The reference bandwidth every SNR is stated in (CW_SPEC 8.1).</summary>
    public const double ReferenceHz = 2500;

    /// <summary>
    /// The factor on <see cref="CwFixtureGenerator.ShapedNoise"/>'s band: its RMS of
    /// 0.02 becomes 0.01 (-40 dBFS), so a faded peak at +15 dB reference stays
    /// under full scale.
    /// </summary>
    public const double NoiseScale = 0.5;

    /// <summary>The tap rate in multiples of the spread: 128 sigma.</summary>
    public const double TapRatePerSpreadHz = 64;

    /// <summary>The Doppler filter's half length in its own time constants.</summary>
    private const double FilterHalfWidthSigmas = 5;

    /// <summary>The tail and lead-in are the generator's: 1.0 s before, 1.5 s after.</summary>
    private const double TailSeconds = 1.5;

    private static readonly Lazy<(double Shaped, double Skirt)> Gains = new(BandGains);

    /// <summary>
    /// Every profile CW_SPEC.md 9.1 gives values for, at those values, with its
    /// status. CH-MDV is not here; <see cref="Profile"/> refuses it.
    /// </summary>
    public static IReadOnlyList<CwChannelProfile> Profiles { get; } = new[]
    {
        new CwChannelProfile("CH-LQ", "Low, quiet", 0.5, 0.5, "[verify]"),
        new CwChannelProfile("CH-LM", "Low, moderate", 2, 1.5, "confirmed"),
        new CwChannelProfile("CH-LD", "Low, disturbed", 6, 10, "[verify]"),
        new CwChannelProfile("CH-MQ", "Mid, quiet", 0.5, 0.1, "[verify]"),
        new CwChannelProfile("CH-MM", "Mid, moderate", 1, 0.5, "[verify]"),
        new CwChannelProfile("CH-MD", "Mid, disturbed", 2, 1, "[verify]"),
        new CwChannelProfile("CH-HQ", "High, quiet", 1, 0.5, "[verify]"),
        new CwChannelProfile("CH-HM", "High, moderate", 3, 10, "[verify]"),
        new CwChannelProfile("CH-HD", "High, disturbed", 7, 30, "[verify]"),
        new CwChannelProfile("CH-AWGN", "No fading, white noise", 0, 0, "project baseline"),
    };

    /// <summary>A profile by name.</summary>
    /// <param name="id">The name.</param>
    /// <returns>The profile.</returns>
    /// <exception cref="NotSupportedException">
    /// CH-MDV: 9.1 gives no delay or spread for it and none is invented
    /// (work instruction 461 DECIDED (3)).
    /// </exception>
    /// <exception cref="ArgumentException">A name 9.1 does not have.</exception>
    public static CwChannelProfile Profile(string id)
    {
        if (string.Equals(id, "CH-MDV", StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                "CH-MDV is refused: CW_SPEC.md 9.1 gives no delay or spread for it "
                + "(\"[read from Annex 3]\"), and no value is invented. The source to vendor is "
                + "CW_SPEC.md 12 item 2, ITU-R F.1487 Annex 3.");
        }

        return Profiles.SingleOrDefault(p => p.Id == id)
            ?? throw new ArgumentException($"{id} is not a CW_SPEC.md 9.1 profile.", nameof(id));
    }

    /// <summary>Generate one case.</summary>
    /// <param name="profileId">The CH-* profile.</param>
    /// <param name="snrDb">Signal-to-noise in the 2500 Hz reference (8.1).</param>
    /// <param name="message">What is keyed: letters, figures and prosign carets the table spells, words split by spaces.</param>
    /// <param name="wordsPerMinute">The speed, PARIS; the keying is TX-ITU, 1:3:1:3:7.</param>
    /// <param name="pitchHz">The tone.</param>
    /// <param name="seed">Seeds the band and both paths.</param>
    /// <returns>The audio, the key and the sidecar.</returns>
    public static CwChannelCase Generate(
        string profileId, double snrDb, string message, double wordsPerMinute, double pitchHz, int seed)
    {
        var parts = Render(profileId, snrDb, message, wordsPerMinute, pitchHz, seed);

        return new CwChannelCase(parts.Audio, parts.Key, parts.Sidecar);
    }

    /// <summary>
    /// One case taken apart: the output and the pieces summed into it, for the
    /// tests that measure it.
    /// </summary>
    internal static CwChannelParts Render(
        string profileId, double snrDb, string message, double wordsPerMinute, double pitchHz, int seed)
    {
        var profile = Profile(profileId);
        var key = Key(message);

        // TX-ITU: SyntheticCq's textbook recipe, 1:3:1:3:7 from the dit, the pitch
        // held (no drift), keyed through the generator's own edges and gate.
        var recipe = SyntheticCq.Recipe(wordsPerMinute, 0, seed) with
        {
            Text = key,
            ToneHz = pitchHz,
            DriftHz = 0,
        };

        var edges = CwFixtureGenerator.KeyEdges(recipe, out var messageStart);
        var seconds = (edges.Length > 0 ? edges[^1] : messageStart) + TailSeconds;
        var count = Math.Max(1, (int)Math.Round(seconds * SampleRate));

        // The band, first and whole, at half the generator's level.
        var noise = new float[count];
        CwFixtureGenerator.ShapedNoise(noise, seed);

        for (var i = 0; i < count; i++)
        {
            noise[i] = (float)(noise[i] * NoiseScale);
        }

        var n0 = BandDensity(pitchHz) * NoiseScale * NoiseScale;
        var power = Math.Pow(10, snrDb / 10) * n0 * ReferenceHz;
        var amplitude = Math.Sqrt(2 * power);

        var envelope = Envelope(edges, count);
        var delay = (int)Math.Round(profile.DelayMs * SampleRate / 1000);

        var path1 = new Complex[count];
        var path2 = new Complex[count];

        if (profile.Fades)
        {
            var h1 = Interpolated(Path(profile.SpreadHz, seed, 1, TapsFor(profile.SpreadHz, count)), profile.SpreadHz, count);
            var h2 = Interpolated(Path(profile.SpreadHz, seed, 2, TapsFor(profile.SpreadHz, count)), profile.SpreadHz, count);

            for (var i = 0; i < count; i++)
            {
                path1[i] = h1[i] * envelope[i];
                path2[i] = i >= delay ? h2[i] * envelope[i - delay] : Complex.Zero;
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                path1[i] = envelope[i];
            }
        }

        var signal = new double[count];
        var audio = new float[count];
        var clipped = 0;
        var w = 2 * Math.PI * pitchHz / SampleRate;

        for (var i = 0; i < count; i++)
        {
            var c = path1[i] + path2[i];
            var phase = w * i;

            signal[i] = amplitude * ((c.Real * Math.Cos(phase)) - (c.Imaginary * Math.Sin(phase)));

            var sample = noise[i] + signal[i];

            if (Math.Abs(sample) > 1)
            {
                clipped++;
            }

            audio[i] = (float)Math.Clamp(sample, -1, 1);
        }

        var output = new MonoAudio(SampleRate, audio);
        var sidecar = Sidecar(profile, snrDb, key, recipe, pitchHz, seed, output, n0, power, delay, messageStart, clipped);

        return new CwChannelParts(output, key, sidecar, profile, edges, delay, amplitude, path1, path2, signal, noise, clipped);
    }

    /// <summary>
    /// Path <paramref name="k"/>'s gain at the tap rate, as the case applies it:
    /// unit-power complex Gaussian noise through the Doppler filter, over root two.
    /// </summary>
    /// <param name="spreadHz">The profile's spread.</param>
    /// <param name="seed">The case's seed.</param>
    /// <param name="k">1 or 2.</param>
    /// <param name="count">How many tap samples.</param>
    /// <returns>The gains, at <see cref="TapRate"/>.</returns>
    internal static Complex[] Path(double spreadHz, int seed, int k, int count)
    {
        var filter = DopplerFilter();
        var white = ComplexGaussian(PathSeed(seed, k), count + filter.Length - 1);
        var gain = new Complex[count];
        var half = 1 / Math.Sqrt(2);

        for (var n = 0; n < count; n++)
        {
            var sum = Complex.Zero;

            for (var m = 0; m < filter.Length; m++)
            {
                sum += white[n + m] * filter[m];
            }

            gain[n] = sum * half;
        }

        return gain;
    }

    /// <summary>The tap rate for a spread: 64 times it, which is 128 sigma.</summary>
    /// <param name="spreadHz">The spread.</param>
    /// <returns>Tap samples a second.</returns>
    internal static double TapRate(double spreadHz) => TapRatePerSpreadHz * spreadHz;

    /// <summary>The seed of path k: the case's seed plus 7919 k, nought read as one.</summary>
    internal static uint PathSeed(int seed, int k)
    {
        var s = unchecked((uint)(seed + (7919 * k)));
        return s == 0 ? 1 : s;
    }

    /// <summary>
    /// The band's one-sided density at a frequency, before <see cref="NoiseScale"/>,
    /// from the generator's three biquads and its skirt.
    /// </summary>
    /// <param name="hz">Where.</param>
    /// <returns>Power per hertz.</returns>
    internal static double BandDensity(double hz)
    {
        var (g, s) = Gains.Value;
        return Math.Pow(((g * Biquads(hz)) + s).Magnitude, 2) / (SampleRate / 2.0);
    }

    /// <summary>The band's equivalent noise bandwidth referred to the density at a frequency, before scaling.</summary>
    /// <param name="hz">Where.</param>
    /// <returns>Hertz.</returns>
    internal static double EquivalentBandwidth(double hz)
    {
        const double Step = 0.05;
        double total = 0;

        for (var f = Step / 2; f < SampleRate / 2.0; f += Step)
        {
            total += BandDensity(f) * Step;
        }

        return total / BandDensity(hz);
    }

    private static int TapsFor(double spreadHz, int count)
        => (int)Math.Floor((count - 1) * TapRate(spreadHz) / SampleRate) + 2;

    // Linear interpolation from the tap rate to the audio rate; at 128 sigma the
    // images sit about 42 dB down.
    private static Complex[] Interpolated(Complex[] taps, double spreadHz, int count)
    {
        var ratio = TapRate(spreadHz) / SampleRate;
        var h = new Complex[count];

        for (var i = 0; i < count; i++)
        {
            var u = i * ratio;
            var n = (int)Math.Floor(u);
            var f = u - n;

            h[i] = (taps[n] * (1 - f)) + (taps[n + 1] * f);
        }

        return h;
    }

    // A sampled Gaussian whose power response is exp(-f^2 / (2 sigma^2)): the
    // amplitude response has deviation root-two sigma, so the impulse response
    // has 1 / (2 pi root-two sigma) seconds, which at 128 sigma is 14.4 taps
    // whatever the spread. Normalised to unit power.
    private static double[] DopplerFilter()
    {
        var sigmaTaps = 2 * TapRatePerSpreadHz / (2 * Math.PI * Math.Sqrt(2));
        var half = (int)Math.Ceiling(FilterHalfWidthSigmas * sigmaTaps);
        var h = new double[(2 * half) + 1];

        for (var m = -half; m <= half; m++)
        {
            h[m + half] = Math.Exp(-(m * m) / (2 * sigmaTaps * sigmaTaps));
        }

        var norm = Math.Sqrt(h.Sum(v => v * v));

        for (var m = 0; m < h.Length; m++)
        {
            h[m] /= norm;
        }

        return h;
    }

    // Unit-power complex Gaussian: xorshift32 and Box-Muller, the generator's own
    // pair, each part at half the power.
    private static Complex[] ComplexGaussian(uint seed, int count)
    {
        var state = seed;

        double NextUniform()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return ((state & 0xFFFFFF) + 1) / 16777217.0;
        }

        var x = new Complex[count];
        var scale = 1 / Math.Sqrt(2);

        for (var n = 0; n < count; n++)
        {
            var magnitude = Math.Sqrt(-2 * Math.Log(NextUniform()));
            var angle = 2 * Math.PI * NextUniform();

            x[n] = new Complex(magnitude * Math.Cos(angle) * scale, magnitude * Math.Sin(angle) * scale);
        }

        return x;
    }

    private static float[] Envelope(double[] edges, int count)
    {
        var e = new float[count];
        var segment = 0;

        if (edges.Length < 2)
        {
            return e;
        }

        for (var i = 0; i < count; i++)
        {
            var t = (double)i / SampleRate;

            while (segment < edges.Length - 1 && t >= edges[segment + 1])
            {
                segment++;
            }

            if (segment >= edges.Length - 1 || segment % 2 != 0)
            {
                continue;
            }

            e[i] = (float)CwFixtureGenerator.Gate(t, edges[segment], edges[segment + 1]);
        }

        return e;
    }

    private static Complex Biquads(double hz)
    {
        var centre = Math.Sqrt(CwFixtureGenerator.PassbandLowHz * CwFixtureGenerator.PassbandHighHz);
        var q = centre / (CwFixtureGenerator.PassbandHighHz - CwFixtureGenerator.PassbandLowHz);
        var w0 = 2 * Math.PI * centre / SampleRate;
        var alpha = Math.Sin(w0) / (2 * q);
        var z1 = Complex.FromPolarCoordinates(1, -2 * Math.PI * hz / SampleRate);
        var one = (alpha - (alpha * z1 * z1)) / ((1 + alpha) - (2 * Math.Cos(w0) * z1) + ((1 - alpha) * z1 * z1));

        return one * one * one;
    }

    // The shaped part's gain that makes its expected RMS 0.02, and the skirt's.
    private static (double Shaped, double Skirt) BandGains()
    {
        const double Step = 0.05;
        var nyquist = SampleRate / 2.0;
        double shaped = 0;

        for (var f = Step / 2; f < nyquist; f += Step)
        {
            shaped += Math.Pow(Biquads(f).Magnitude, 2) * Step / nyquist;
        }

        const double Target = 0.02;
        return (Target / Math.Sqrt(shaped), Target * Math.Pow(10, -CwFixtureGenerator.OutOfBandDropDb / 20));
    }

    // The key is the message as keyed: words split by spaces, every character one
    // the table spells or a prosign caret. Anything else is refused rather than
    // skipped, because a skipped character would be in the key and not the audio.
    private static string Key(string message)
    {
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var c in words.SelectMany(w => w))
        {
            if (c != '^' && MorseCode.Spell(c) is null)
            {
                throw new ArgumentException($"'{c}' is not in the Morse table, so it would be in the key and not in the audio.", nameof(message));
            }
        }

        return string.Join(' ', words);
    }

    private static string Sidecar(
        CwChannelProfile profile, double snrDb, string key, CwFixtureRecipe recipe, double pitchHz, int seed,
        MonoAudio audio, double n0, double power, int delay, double messageStart, int clipped)
    {
        var i = CultureInfo.InvariantCulture;
        var text = new StringBuilder();
        var enbw = EquivalentBandwidth(pitchHz);

        void Line(FormattableString s) => text.AppendLine(s.ToString(i));

        Line($"profile       {profile.Id} - {profile.Condition} - status {profile.Status} (CW_SPEC.md 9.1)");
        Line($"paths         {(profile.Fades ? "2, independent, equal power, each half the unfaded power" : "1, unfaded (h1 = 1)")}");
        Line($"delay         {profile.DelayMs:0.###} ms, {delay} samples");
        Line($"spread        {profile.SpreadHz:0.###} Hz, two sigma of a Gaussian Doppler spectrum, sigma {profile.SpreadHz / 2:0.###} Hz, no shift");
        Line($"doppler       {(profile.Fades ? $"complex white Gaussian at {TapRate(profile.SpreadHz):0.###} taps/s through a sampled Gaussian FIR, unit power, linear to {SampleRate} Hz" : "none")}");
        Line($"generated     tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwChannel.cs");
        Line($"seed          {seed} (band); path 1 {PathSeed(seed, 1)}, path 2 {PathSeed(seed, 2)} (seed + 7919 k)");
        Line($"sampleRate    {SampleRate}");
        Line($"seconds       {audio.Duration.TotalSeconds:0.00}");
        text.AppendLine();
        Line($"key           {key}");
        Line($"sender        TX-ITU, 1:3:1:3:7, {recipe.WordsPerMinute:0.#} wpm, dit {recipe.DitMilliseconds:0.###} ms, 5 ms raised-cosine edges");
        Line($"messageStart  {messageStart:0.00} s");
        Line($"toneHz        {pitchHz:0.#} Hz, no drift");
        text.AppendLine();
        Line($"snr           {snrDb:0.0} dB in the 2500 Hz reference (CW_SPEC.md 8.1)");
        Line($"band          CwFixtureGenerator.ShapedNoise, 350-870 Hz, three biquads, skirt 30 dB down in power, x {NoiseScale} (rms 0.01, -40 dBFS), added after the fading");
        Line($"enbw          {enbw:0.0} Hz referred to the density at {pitchHz:0.#} Hz; offset to 2500 Hz {-10 * Math.Log10(ReferenceHz / enbw):+0.00;-0.00} dB");
        Line($"inPassband    {snrDb + (10 * Math.Log10(ReferenceHz / enbw)):0.00} dB (tone power over the band's power)");
        Line($"n0            {10 * Math.Log10(n0):0.00} dB/Hz at the tone; unfaded tone power {10 * Math.Log10(power):0.00} dBFS");
        text.AppendLine();
        Line($"peak          {AudioTap.PeakOf(audio):0.0} dBFS, {clipped} samples clipped");
        Line($"notProved     the published model, not the band; [verify] values unvendored (CW_SPEC.md 12 item 2)");

        return text.ToString();
    }
}

/// <summary>A case and the pieces summed into it.</summary>
/// <param name="Audio">The output.</param>
/// <param name="Key">The key.</param>
/// <param name="Sidecar">The recipe.</param>
/// <param name="Profile">The profile.</param>
/// <param name="Edges">The key-change instants, in seconds.</param>
/// <param name="DelaySamples">Path two's delay, in samples.</param>
/// <param name="Amplitude">The unfaded tone's peak amplitude.</param>
/// <param name="Path1">Path one's complex baseband, gain times envelope.</param>
/// <param name="Path2">Path two's, gain times the delayed envelope; all nought for CH-AWGN.</param>
/// <param name="Signal">The faded tone, before the band.</param>
/// <param name="Noise">The band, as added.</param>
/// <param name="Clipped">Samples that reached full scale.</param>
internal sealed record CwChannelParts(
    MonoAudio Audio,
    string Key,
    string Sidecar,
    CwChannelProfile Profile,
    double[] Edges,
    int DelaySamples,
    double Amplitude,
    Complex[] Path1,
    Complex[] Path2,
    double[] Signal,
    float[] Noise,
    int Clipped);
