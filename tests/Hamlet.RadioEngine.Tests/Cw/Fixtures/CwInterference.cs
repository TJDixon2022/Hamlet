using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>One INT-* profile as CW_SPEC.md section 9.3 states it.</summary>
/// <param name="Id">The profile's name, <c>INT-ADJ</c> and so on.</param>
/// <param name="Term">Section 9.3's term column.</param>
/// <param name="Definition">Section 9.3's definition, quoted.</param>
public sealed record CwInterferenceProfile(string Id, string Term, string Definition);

/// <summary>One station or carrier laid over the wanted station.</summary>
/// <param name="Keyed">True for a TX-ITU station; false for a steady carrier.</param>
/// <param name="OffsetHz">Its pitch less the wanted station's.</param>
/// <param name="LevelDb">Its key-down power against the wanted station's key-down power.</param>
/// <param name="WordsPerMinute">A keyed station's speed, PARIS; NaN for a carrier.</param>
/// <param name="Text">A keyed station's text, repeated word by word to the wanted's end; empty for a carrier.</param>
public sealed record CwInterferer(bool Keyed, double OffsetHz, double LevelDb, double WordsPerMinute, string Text);

/// <summary>A profile with its parameters: what one case lays over the wanted station.</summary>
/// <param name="ProfileId">The INT-* profile.</param>
/// <param name="Label">The parameters as a requirement row writes them, <c>INT-ADJ(+100, 0, 25)</c>.</param>
/// <param name="Interferers">Every layer, in order.</param>
public sealed record CwInterferenceSpec(string ProfileId, string Label, IReadOnlyList<CwInterferer> Interferers);

/// <summary>
/// The INT-* interference profiles, mixed over the wanted station before CH-AWGN's band
/// (work instruction 469; PHASE_PLAN.md 7.3; CW_SPEC.md section 9.3; HM-REQ-060, 061,
/// 062 and 066).
/// </summary>
/// <remarks>
/// <para>**THE WANTED STATION IS <see cref="CwChannel"/>'S CASE, UNTOUCHED.** TX-ITU keying
/// <see cref="SyntheticCq.Text"/> at 600 Hz, through <see cref="CwChannel.RenderKeyed"/>,
/// so the band, the 2500 Hz reference and the SNR set on the wanted station alone are unit
/// 461's. Every interferer is summed between that tone and that band, at an amplitude set
/// from the wanted's own key-down amplitude, so a level is a ratio of key-down powers.</para>
/// <para>**THE WANTED KEY IS EXACT BY CONSTRUCTION** (R61, V-13): the text keyed and nothing
/// else. An interferer's text is in the sidecar and is never the key (R72).</para>
/// <para>**HEADROOM, NOT A CHANGE OF LEVEL.** Where the sum would pass 0.9 of full scale (a
/// carrier 20 dB over the wanted peaks near 2.2) the whole case - band, wanted and every
/// layer - is scaled by one factor, which leaves every SNR and every ratio as stated. The
/// factor is in the sidecar.</para>
/// <para>**AUDIO SYNTHESIS IN THE TEST FIXTURES, NOT A KEYER** (CLAUDE.md 0.2). The recipe
/// is <c>docs/phase-requirements/interference.md</c>.</para>
/// </remarks>
public static class CwInterference
{
    /// <summary>The wanted station's pitch: the channel and sender cases' and the decoder's start.</summary>
    public const double WantedHz = CwSender.PitchHz;

    /// <summary>The wanted station's SNR in the 2500 Hz reference (DECIDED (4)).</summary>
    public const double SnrDb = 15;

    /// <summary>The channel under every case.</summary>
    public const string Channel = "CH-AWGN";

    /// <summary>The adjacent and co-channel stations' text, the two-in-one passband's, kept when it came out (work instruction 545).</summary>
    public const string AdjacentText = "DE N0AAA UP";

    /// <summary>INT-COCHAN's offset: inside the 45 Hz detector bandwidth, ±22.5 Hz about the tone.</summary>
    public const double CoChannelOffsetHz = 10;

    /// <summary>Where a sum is scaled back to, of full scale.</summary>
    public const double HeadroomPeak = 0.9;

    /// <summary>Section 9.3's four INT-* rows, in its order.</summary>
    public static IReadOnlyList<CwInterferenceProfile> Profiles { get; } = new[]
    {
        new CwInterferenceProfile("INT-ADJ", "Adjacent station",
            "A second CW signal Δf hertz away, ΔdB relative to the wanted one, at its own speed."),
        new CwInterferenceProfile("INT-COCHAN", "Co-channel station",
            "A second signal within the detector bandwidth of the wanted one. The veto case."),
        new CwInterferenceProfile("INT-CARRIER", "Unkeyed carrier",
            "A steady tone. The \"loudest is not keyed\" case."),
        new CwInterferenceProfile("INT-PILEUP", "Pileup",
            "Three or more stations inside the passband. The \"emit nothing\" case."),
    };

    /// <summary>INT-ADJ(Δf, ΔdB, WPM): one TX-ITU station.</summary>
    public static CwInterferenceSpec Adjacent(double offsetHz, double levelDb, double wordsPerMinute)
        => new("INT-ADJ", string.Create(CultureInfo.InvariantCulture, $"INT-ADJ({offsetHz:+0;-0}, {levelDb:0.#}, {wordsPerMinute:0})"),
            new[] { new CwInterferer(true, offsetHz, levelDb, wordsPerMinute, AdjacentText) });

    /// <summary>INT-COCHAN: one TX-ITU station at +10 Hz, 0 dB, 25 WPM.</summary>
    public static CwInterferenceSpec CoChannel()
        => new("INT-COCHAN", string.Create(CultureInfo.InvariantCulture, $"INT-COCHAN(+{CoChannelOffsetHz:0}, 0, 25)"),
            new[] { new CwInterferer(true, CoChannelOffsetHz, 0, 25, AdjacentText) });

    /// <summary>INT-CARRIER(Δf, ΔdB): a steady tone from the first sample to the last.</summary>
    public static CwInterferenceSpec Carrier(double offsetHz, double levelDb)
        => new("INT-CARRIER", string.Create(CultureInfo.InvariantCulture, $"INT-CARRIER({offsetHz:+0;-0}, {levelDb:+0;-0;0})"),
            new[] { new CwInterferer(false, offsetHz, levelDb, double.NaN, "") });

    /// <summary>INT-PILEUP: three TX-ITU stations at 0 dB, at 525, 675 and 750 Hz, 12, 25 and 18 WPM.</summary>
    public static CwInterferenceSpec Pileup()
        => new("INT-PILEUP", "INT-PILEUP(3 at 0 dB: -75/12, +75/25, +150/18)", new[]
        {
            new CwInterferer(true, -75, 0, 12, AdjacentText),
            new CwInterferer(true, 75, 0, 25, "QRZ DE N0AAA"),
            new CwInterferer(true, 150, 0, 18, "5NN TU"),
        });

    /// <summary>
    /// The fixed seed of a case, set before any decode: 469000 plus the wanted speed's
    /// index into <see cref="SyntheticCq.Speeds"/>. It seeds the band, so every case at a
    /// speed and its control share one band.
    /// </summary>
    public static int Seed(double wantedWpm)
    {
        var index = SyntheticCq.Speeds.ToList().IndexOf(wantedWpm);

        return index < 0
            ? throw new ArgumentException($"{wantedWpm} is not one of SyntheticCq.Speeds.", nameof(wantedWpm))
            : 469000 + index;
    }

    /// <summary>Generate one case.</summary>
    /// <param name="spec">The profile and its parameters; null for the control, the wanted alone.</param>
    /// <param name="wantedWpm">The wanted station's speed.</param>
    /// <returns>The audio, the wanted key and the sidecar.</returns>
    public static CwChannelCase Generate(CwInterferenceSpec? spec, double wantedWpm)
    {
        var parts = Render(spec, wantedWpm, Seed(wantedWpm));

        return new CwChannelCase(parts.Audio, parts.Key, parts.Sidecar);
    }

    /// <summary>One case taken apart, for the proof and the fact.</summary>
    /// <param name="spec">The profile; null for the wanted alone.</param>
    /// <param name="wantedWpm">The wanted station's speed.</param>
    /// <param name="seed">Seeds the band.</param>
    /// <param name="nominal">
    /// The proof's watched-red mixer: every layer ignores its profile and is the wanted
    /// station again, at its pitch and level and keyed like it.
    /// </param>
    /// <returns>The case and its layers.</returns>
    internal static CwInterferenceParts Render(CwInterferenceSpec? spec, double wantedWpm, int seed, bool nominal = false)
    {
        var recipe = SyntheticCq.Recipe(wantedWpm, 0, seed) with { ToneHz = WantedHz, DriftHz = 0 };
        var sender = string.Create(CultureInfo.InvariantCulture,
            $"TX-ITU, 1:3:1:3:7, {recipe.WordsPerMinute:0.#} wpm, dit {recipe.DitMilliseconds:0.###} ms, 5 ms raised-cosine edges");
        var wanted = CwChannel.RenderKeyed(Channel, SnrDb, recipe, WantedHz, seed, sender);
        var count = wanted.Audio.Samples.Length;
        var rate = CwChannel.SampleRate;

        var (starts, ends) = Words(wanted.Edges);
        var start = starts.Count > 1 ? starts[1] : starts[0];
        var end = wanted.Edges[^1];

        var layers = new List<CwInterferenceLayer>();

        foreach (var x in spec?.Interferers ?? Array.Empty<CwInterferer>())
        {
            if (nominal)
            {
                layers.Add(new CwInterferenceLayer(x, x.Text, wanted.Edges, (double[])wanted.Signal.Clone(), WantedHz, 0, true));
                continue;
            }

            var hz = WantedHz + x.OffsetHz;
            var amplitude = wanted.Amplitude * Math.Pow(10, x.LevelDb / 20);
            var w = 2 * Math.PI * hz / rate;
            var signal = new double[count];

            if (!x.Keyed)
            {
                for (var i = 0; i < count; i++)
                {
                    signal[i] = amplitude * Math.Cos(w * i);
                }

                layers.Add(new CwInterferenceLayer(x, "", Array.Empty<double>(), signal, hz, 0, false));
                continue;
            }

            var (text, edges) = KeyedTo(x, start, end);
            var envelope = CwChannel.Envelope(edges, count);

            for (var i = 0; i < count; i++)
            {
                signal[i] = amplitude * envelope[i] * Math.Cos(w * i);
            }

            layers.Add(new CwInterferenceLayer(x, text, edges, signal, hz, start, false));
        }

        // Band, wanted and every layer summed; one factor for all where the sum would clip.
        var sum = new double[count];
        var peak = 0.0;

        for (var i = 0; i < count; i++)
        {
            var s = wanted.Noise[i] + wanted.Signal[i];

            foreach (var l in layers)
            {
                s += l.Signal[i];
            }

            sum[i] = s;
            peak = Math.Max(peak, Math.Abs(s));
        }

        var scale = peak > HeadroomPeak ? HeadroomPeak / peak : 1.0;
        var audio = new float[count];

        for (var i = 0; i < count; i++)
        {
            audio[i] = (float)(sum[i] * scale);
        }

        var output = new MonoAudio(rate, audio);
        var sidecar = Sidecar(spec, wanted, layers, scale, seed, nominal, output);

        return new CwInterferenceParts(output, wanted.Key, sidecar, spec, wanted, layers, scale, starts, ends);
    }

    /// <summary>
    /// The wanted station's word starts and word ends, in seconds, from its own key edges:
    /// a word ends at a key-up followed by a gap longer than five dits, or at the last key-up.
    /// </summary>
    internal static (IReadOnlyList<double> Starts, IReadOnlyList<double> Ends) Words(double[] edges)
    {
        var starts = new List<double>();
        var ends = new List<double>();

        if (edges.Length < 2)
        {
            return (starts, ends);
        }

        var shortest = double.MaxValue;

        for (var k = 0; k + 1 < edges.Length; k += 2)
        {
            shortest = Math.Min(shortest, edges[k + 1] - edges[k]);
        }

        starts.Add(edges[0]);

        for (var k = 1; k + 1 < edges.Length; k += 2)
        {
            if (edges[k + 1] - edges[k] > 5 * shortest)
            {
                ends.Add(edges[k]);
                starts.Add(edges[k + 1]);
            }
        }

        ends.Add(edges[^1]);

        return (starts, ends);
    }

    // A keyed interferer's text and edges: its words in turn, cycled, each kept only if
    // its last key-up lands by the wanted's last, the whole shifted to start at `start`.
    private static (string Text, double[] Edges) KeyedTo(CwInterferer x, double start, double end)
    {
        var words = x.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var kept = new List<string>();
        double[] edges = Array.Empty<double>();

        for (var n = 0; n < 1000; n++)
        {
            var trial = kept.Append(words[n % words.Length]).ToList();
            var recipe = SyntheticCq.Recipe(x.WordsPerMinute, 0, 0) with { Text = string.Join(' ', trial), ToneHz = WantedHz + x.OffsetHz, DriftHz = 0 };
            var e = CwFixtureGenerator.KeyEdges(recipe, out var messageStart);
            var shifted = e.Select(t => t - messageStart + start).ToArray();

            if (shifted[^1] > end)
            {
                break;
            }

            kept = trial;
            edges = shifted;
        }

        return (string.Join(' ', kept), edges);
    }

    private static string Sidecar(
        CwInterferenceSpec? spec, CwChannelParts wanted, IReadOnlyList<CwInterferenceLayer> layers,
        double scale, int seed, bool nominal, MonoAudio audio)
    {
        var i = CultureInfo.InvariantCulture;
        var text = new StringBuilder(wanted.Sidecar);

        void Line(FormattableString s) => text.AppendLine(s.ToString(i));

        text.AppendLine();

        if (spec is null)
        {
            Line($"intProfile    none - the control: the wanted station alone, the same seed and band");
        }
        else
        {
            var profile = Profiles.Single(p => p.Id == spec.ProfileId);
            Line($"intProfile    {spec.Label} - {profile.Term} (CW_SPEC.md 9.3)");
            Line($"intDefinition {profile.Definition}");
        }

        Line($"intMixer      {(nominal ? "NOMINAL - the watched-red mixer: every layer is the wanted station again, its pitch, level and keying" : "each layer summed over the wanted tone, before the band; level against the wanted's key-down amplitude")}");
        Line($"wantedKey     {wanted.Key} - exact by construction (R61, V-13); the only key scored");
        Line($"wantedPitch   {WantedHz:0.#} Hz, TX-ITU, amplitude {wanted.Amplitude:0.######} (key-down peak)");
        Line($"seed          {seed} (469000 + the wanted speed's index into SyntheticCq.Speeds; the band's)");

        foreach (var (l, n) in layers.Select((l, n) => (l, n + 1)))
        {
            var x = l.Interferer;
            var how = x.Keyed
                ? string.Create(i, $", {x.WordsPerMinute:0} wpm, start {l.StartSeconds:0.000} s (the wanted's second word), text `{l.Text}` - never scored")
                : ", from the first sample to the last, phase 0";

            Line($"layer {n}       {(x.Keyed ? "keyed TX-ITU" : "steady carrier")}, {l.PitchHz:0.#} Hz ({x.OffsetHz:+0.#;-0.#} Hz), {x.LevelDb:+0.0;-0.0;0.0} dB against the wanted's key-down power{how}");
        }

        Line($"headroom      x {scale:0.######}{(scale < 1 ? " - the whole case, band, wanted and every layer, scaled so the peak is 0.9 FS; every SNR and ratio unchanged" : " (none needed)")}");
        Line($"peak          {AudioTap.PeakOf(audio):0.0} dBFS after headroom, no sample clipped");

        return text.ToString();
    }
}

/// <summary>One layer as mixed.</summary>
/// <param name="Interferer">What was asked for.</param>
/// <param name="Text">The text actually keyed, the words that fit before the wanted's end.</param>
/// <param name="Edges">Its key-change instants, in seconds; empty for a carrier.</param>
/// <param name="Signal">The layer alone, before the band and before headroom.</param>
/// <param name="PitchHz">Its tone.</param>
/// <param name="StartSeconds">Its first mark; nought for a carrier.</param>
/// <param name="Nominal">True where the watched-red mixer made it.</param>
internal sealed record CwInterferenceLayer(
    CwInterferer Interferer, string Text, double[] Edges, double[] Signal, double PitchHz, double StartSeconds, bool Nominal);

/// <summary>A case and the pieces summed into it.</summary>
/// <param name="Audio">What the receiver hands over.</param>
/// <param name="Key">The wanted station's key.</param>
/// <param name="Sidecar">The recipe.</param>
/// <param name="Spec">The profile, or null for the control.</param>
/// <param name="Wanted">The wanted station on CH-AWGN, band and tone apart.</param>
/// <param name="Layers">Every interferer.</param>
/// <param name="Scale">The headroom factor on the whole case.</param>
/// <param name="WordStarts">The wanted's word starts, seconds.</param>
/// <param name="WordEnds">The wanted's word ends, seconds.</param>
internal sealed record CwInterferenceParts(
    MonoAudio Audio,
    string Key,
    string Sidecar,
    CwInterferenceSpec? Spec,
    CwChannelParts Wanted,
    IReadOnlyList<CwInterferenceLayer> Layers,
    double Scale,
    IReadOnlyList<double> WordStarts,
    IReadOnlyList<double> WordEnds);
