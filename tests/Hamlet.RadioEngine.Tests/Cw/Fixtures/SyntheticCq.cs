using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// CQ calls whose key is exact because the generator knows what it sent (work
/// instruction 414; PHASE_PLAN.md 1.1, 1.2).
/// </summary>
/// <remarks>
/// <para>**TEXTBOOK SPACING, ONE TO THREE TO ONE TO THREE TO SEVEN, SCALED FROM THE
/// DIT AT EVERY SPEED.** It is the one form a key can state without anybody
/// disputing it, and it is also the spacing the decoder falls back to at
/// `CwUnitEstimator.cs` 216, so these cases cannot see the fault step 3 is
/// attacking (`docs/phase-correctness/synthetic-cq.md`). The recipe's own default
/// spacing is 013347's fist and is not used here.</para>
/// <para>**`N0CALL`, ALREADY IN THE TREE'S FIXTURES**, carries a digit of five
/// elements and an `L` of four, and no new callsign enters the repository with
/// it (HM-OPEN-018).</para>
/// <para>**NO SYNTHETIC CASE IS EVER THE SOLE EVIDENCE FOR KEEPING A CHANGE**
/// (1.4, CLAUDE.md 12.5), and none of them is in any total 3.2 judges.</para>
/// </remarks>
public static class SyntheticCq
{
    /// <summary>What every case sends.</summary>
    public const string Text = "CQ CQ CQ DE N0CALL N0CALL K";

    /// <summary>The pitch the decoder is started at, the floors' and the anchors'.</summary>
    public const double StartingPitchHz = 600;

    /// <summary>The speeds of the grid, in words a minute.</summary>
    public static IReadOnlyList<double> Speeds { get; } = new[] { 12.0, 18.0, 25.0 };

    /// <summary>
    /// The in-passband signal-to-noise of the grid, in decibels: one that reads
    /// clean, one near where the decoder starts to lose characters, one below it.
    /// </summary>
    /// <remarks>
    /// The catalogue's own three tiers (<see cref="CwFixtureCatalogue.EasyDb"/>,
    /// <see cref="CwFixtureCatalogue.WorkingDb"/>, <see cref="CwFixtureCatalogue.EdgeDb"/>),
    /// the edge being HM-DEC-097's, and each confirmed as rendered by
    /// <see cref="WhatTheGeneratorMakesTests"/> before the set was built. None of
    /// them was chosen from a decode.
    /// </remarks>
    public static IReadOnlyList<double> Levels { get; } = new[]
    {
        CwFixtureCatalogue.EasyDb,
        CwFixtureCatalogue.WorkingDb,
        CwFixtureCatalogue.EdgeDb,
    };

    /// <summary>The speed of the five-unit row (task 4).</summary>
    public const double WideRowWpm = 18;

    /// <summary>Where the set lives.</summary>
    public static string Folder { get; } = Path.Combine(
        Path.GetDirectoryName(CwFixtureCatalogue.Folder)!, "synthetic-cq");

    /// <summary>Every case: the three by three grid, then the five-unit row.</summary>
    public static IReadOnlyList<CwFixtureRecipe> All { get; } = Build();

    /// <summary>A textbook recipe at one speed and one level.</summary>
    /// <param name="wpm">Words a minute; the dit is 1200 over it, in milliseconds.</param>
    /// <param name="snrDb">In-passband signal-to-noise.</param>
    /// <param name="seed">The noise seed.</param>
    /// <param name="characterGapUnits">Three for textbook, five for the wide row.</param>
    /// <returns>The recipe.</returns>
    public static CwFixtureRecipe Recipe(double wpm, double snrDb, int seed, double characterGapUnits = 3)
    {
        var dit = 1200.0 / wpm;
        var wide = characterGapUnits != 3;

        return new CwFixtureRecipe(
            Name: string.Create(CultureInfo.InvariantCulture,
                $"cq-{wpm:0}wpm-{Tier(snrDb)}{(wide ? $"-char{characterGapUnits:0}" : "")}"),
            Text: Text,
            DitMilliseconds: dit,
            DahMilliseconds: 3 * dit,
            ElementGapMilliseconds: dit,
            CharacterGapMilliseconds: characterGapUnits * dit,
            WordGapMilliseconds: 7 * dit,
            SignalToNoiseDb: snrDb,
            Seed: seed);
    }

    private static string Tier(double snrDb)
        => snrDb == CwFixtureCatalogue.EasyDb ? "15db"
            : snrDb == CwFixtureCatalogue.WorkingDb ? "5db"
            : snrDb == CwFixtureCatalogue.EdgeDb ? "0db"
            : string.Create(CultureInfo.InvariantCulture, $"{snrDb:0}db");

    private static List<CwFixtureRecipe> Build()
    {
        var recipes = new List<CwFixtureRecipe>();
        var seed = 20260924;

        foreach (var wpm in Speeds)
        {
            foreach (var snr in Levels)
            {
                recipes.Add(Recipe(wpm, snr, seed++));
            }
        }

        // **THE FIVE-UNIT ROW (task 4): THE CHARACTER GAP UNIT 413 MEASURED ON THE
        // 7.052 MHz SENDER**, everything else textbook, so step 3 has an exact key
        // for its own fault. Still never sole evidence.
        foreach (var snr in Levels)
        {
            recipes.Add(Recipe(WideRowWpm, snr, seed++, characterGapUnits: 5));
        }

        return recipes;
    }

    /// <summary>
    /// What the decoder settles from the audio, fed hop by hop from a starting pitch
    /// of 600 Hz: the path the floors and the anchors use.
    /// </summary>
    /// <param name="audio">The audio.</param>
    /// <returns>The reading, unsure flags kept.</returns>
    public static CwReading Read(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, StartingPitchHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return CwReading.Of(settled);
    }

    /// <summary>The whole decode, its two ends' gaps trimmed.</summary>
    /// <param name="reading">Everything settled.</param>
    /// <returns>The reading from its first to its last non-space character.</returns>
    public static CwReading Trimmed(CwReading reading)
    {
        var from = 0;
        var to = reading.Text.Length;

        while (from < to && char.IsWhiteSpace(reading.Text[from]))
        {
            from++;
        }

        while (to > from && char.IsWhiteSpace(reading.Text[to - 1]))
        {
            to--;
        }

        return reading.Slice(from, to);
    }

    /// <summary>The whole decode against the whole key, which is exact.</summary>
    /// <param name="reading">Everything settled.</param>
    /// <returns>The score.</returns>
    public static CwScore Whole(CwReading reading)
        => CwScorer.Whole(Trimmed(reading), Text, CwKeyKind.Exact);

    /// <summary>The key against the stretch of the decode that fits it best, for comparison.</summary>
    /// <param name="reading">Everything settled.</param>
    /// <returns>The score.</returns>
    public static CwScore Within(CwReading reading)
        => CwScorer.Within(reading, Text, CwKeyKind.Exact);

    /// <summary>The key file written beside a case.</summary>
    /// <param name="recipe">The case.</param>
    /// <returns>The markdown.</returns>
    public static string KeyFile(CwFixtureRecipe recipe)
    {
        var i = CultureInfo.InvariantCulture;
        var dit = recipe.DitMilliseconds;
        var text = new StringBuilder();

        text.Append("# Answer key - ").Append(recipe.Name).Append('\n');
        text.Append('\n');
        text.Append("**Exact by construction.** This recording was generated, not received. The generator\n");
        text.Append("keyed exactly the text below and nothing else, so the key is known for certain and\n");
        text.Append("nobody had to read Morse to write it (PHASE_PLAN.md R61). It is labeled exact wherever\n");
        text.Append("a number is reported against it.\n");
        text.Append('\n');
        text.Append("**The key:**\n");
        text.Append('\n');
        text.Append("    ").Append(recipe.Text).Append('\n');
        text.Append('\n');
        text.Append("**Scored region.** The whole decode against the whole key (`CwScorer.Whole`), the gaps\n");
        text.Append("at the decode's two ends trimmed. Anything the decoder prints from the noise before the\n");
        text.Append("first `C` or after the last `K` is its own error, because the generator sent nothing\n");
        text.Append("there. `CwScorer.Within` is printed beside it for comparison and is not the number.\n");
        text.Append('\n');
        text.Append("**The recipe**, enough to rebuild the file byte for byte (1.2). `CwFixtureRecipe` fed to\n");
        text.Append("`CwFixtureGenerator.Generate`, written by `WavAudio.Write`; recipe fields not listed take\n");
        text.Append("their defaults.\n");
        text.Append('\n');
        text.Append("| field | value |\n");
        text.Append("|---|---|\n");
        text.Append(string.Create(i, $"| Name | `{recipe.Name}` |\n"));
        text.Append(string.Create(i, $"| Text | `{recipe.Text}` |\n"));
        text.Append(string.Create(i, $"| speed | {recipe.WordsPerMinute:0} wpm, the dit 1200 / {recipe.WordsPerMinute:0} = {dit:0.###} ms |\n"));
        string Units(double ms, string what)
        {
            var units = ms / dit;

            return string.Create(i,
                $"| {what} | {(units == 1 ? $"1200.0 / {recipe.WordsPerMinute:0}" : $"{units:0} x (1200.0 / {recipe.WordsPerMinute:0})")}, {ms:0.###} to three places, {units:0} unit{(units == 1 ? "" : "s")} |\n");
        }

        text.Append(Units(dit, "DitMilliseconds"));
        text.Append(Units(recipe.DahMilliseconds, "DahMilliseconds"));
        text.Append(Units(recipe.ElementGapMilliseconds, "ElementGapMilliseconds"));
        text.Append(Units(recipe.CharacterGapMilliseconds, "CharacterGapMilliseconds"));
        text.Append(Units(recipe.WordGapMilliseconds, "WordGapMilliseconds"));
        text.Append(string.Create(i, $"| SignalToNoiseDb | {recipe.SignalToNoiseDb:0.0}, tone RMS over noise RMS inside the 350-870 Hz passband |\n"));
        text.Append(string.Create(i, $"| ToneHz | {recipe.ToneHz:0} |\n"));
        text.Append(string.Create(i, $"| DriftHz | {recipe.DriftHz:0}, either side, over 10 s |\n"));
        text.Append(string.Create(i, $"| QsbHz, QsbDepthDb | {recipe.QsbHz:0}, {recipe.QsbDepthDb:0} - steady |\n"));
        text.Append(string.Create(i, $"| PreambleSeconds | {recipe.PreambleSeconds:0} - none |\n"));
        text.Append(string.Create(i, $"| Seed | {recipe.Seed} |\n"));
        text.Append('\n');
        text.Append("The decode is `CwDecoder` at 8000 samples a second, fed hop by hop from a starting pitch\n");
        text.Append("of 600 Hz, the path the floors use (`SyntheticCq.Read`).\n");
        text.Append('\n');
        text.Append("**What this case does not prove** (1.4), in full in\n");
        text.Append("`docs/phase-correctness/synthetic-cq.md`: the spacing is ");
        text.Append(recipe.CharacterGapMilliseconds / dit == 3
            ? "textbook, the same spacing the decoder\nfalls back to at `CwUnitEstimator.cs` 216, so a clean read here says nothing about a sender\nwho spaces letters five dits apart;"
            : "textbook but for a character gap of\nfive units, one sender's measured spacing and nobody else's, and the decoder's own fallback\nat `CwUnitEstimator.cs` 216 is still textbook;");
        text.Append(" the noise is generated, not the band's; one tone,\n");
        text.Append("no second station; the keying is machine-perfect. **No synthetic case is ever the sole\n");
        text.Append("evidence for keeping a change**, and this one is in no total that 3.2 judges.\n");

        return text.ToString();
    }

    /// <summary>Write a case: the WAV, its sidecar with fixed newlines, and its key file.</summary>
    /// <param name="recipe">The case.</param>
    public static void Write(CwFixtureRecipe recipe)
    {
        Directory.CreateDirectory(Folder);

        var (audio, sidecar) = CwFixtureGenerator.Generate(recipe);

        WavAudio.Write(Path.Combine(Folder, recipe.Name + ".wav"), audio);
        File.WriteAllText(
            Path.Combine(Folder, recipe.Name + ".txt"),
            sidecar.Replace("\r\n", "\n", StringComparison.Ordinal),
            new UTF8Encoding(false));
        File.WriteAllText(
            Path.Combine(Folder, recipe.Name + ".key.md"),
            KeyFile(recipe),
            new UTF8Encoding(false));
    }

    /// <summary>The bytes the recipe builds, as <see cref="WavAudio.Write(Stream, MonoAudio)"/> encodes them.</summary>
    /// <param name="recipe">The case.</param>
    /// <returns>The WAV file's bytes.</returns>
    public static byte[] Bytes(CwFixtureRecipe recipe)
    {
        var (audio, _) = CwFixtureGenerator.Generate(recipe);

        using var encoded = new MemoryStream();
        WavAudio.Write(encoded, audio);

        return encoded.ToArray();
    }

    /// <summary>How many of each gap the text keys, from the Morse table.</summary>
    /// <param name="text">The text.</param>
    /// <returns>Marks, element gaps, character gaps and word gaps.</returns>
    public static (int Marks, int Element, int Character, int Word) Counts(string text)
    {
        int marks = 0, element = 0, character = 0, word = 0;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        word = words.Length - 1;

        foreach (var w in words)
        {
            character += w.Length - 1;

            foreach (var ch in w)
            {
                var pattern = MorseCode.Spell(ch) ?? "";

                marks += pattern.Length;
                element += Math.Max(0, pattern.Length - 1);
            }
        }

        return (marks, element, character, word);
    }
}

/// <summary>
/// What the rendered audio holds, measured from the audio and not read back from
/// the recipe (work instruction 414, task 1).
/// </summary>
/// <param name="PitchHz">Where the tone stands, by a one-hertz scan of the passband.</param>
/// <param name="Marks">Every key-down, in milliseconds, at half the tone's amplitude.</param>
/// <param name="Spaces">Every key-up between the first mark and the last.</param>
/// <param name="ToneRms">The tone's RMS, from the envelope inside marks less the envelope of the noise.</param>
/// <param name="NoiseRms">The noise's RMS over the stretch before the first mark.</param>
public sealed record RenderedKeying(
    double PitchHz,
    IReadOnlyList<double> Marks,
    IReadOnlyList<double> Spaces,
    double ToneRms,
    double NoiseRms)
{
    /// <summary>Tone over noise, in decibels.</summary>
    public double SnrDb => 20 * Math.Log10(ToneRms / NoiseRms);

    /// <summary>
    /// Measure a rendering.
    /// </summary>
    /// <param name="audio">The audio.</param>
    /// <returns>The measurement.</returns>
    /// <remarks>
    /// <para>**THE ENVELOPE IS THE TONE'S OWN**: the audio mixed down at the pitch
    /// the scan found and averaged over five milliseconds, so its height inside a
    /// mark is the tone's peak amplitude with the noise's added in power.</para>
    /// <para>**EDGES ARE WHERE IT CROSSES HALF THE TONE'S AMPLITUDE**, with a tenth
    /// either side as hysteresis and runs under six milliseconds merged into their
    /// neighbors. The generator's five-millisecond raised-cosine edges put that
    /// crossing two and a half milliseconds inside each end of a keyed element, so
    /// a rendering true to its recipe measures each mark five milliseconds short
    /// and each gap five long.</para>
    /// <para>**THE NOISE IS MEASURED BEFORE THE FIRST MARK**, from the raw samples,
    /// so it includes the out-of-band skirt, thirty decibels down.</para>
    /// </remarks>
    public static RenderedKeying Measure(MonoAudio audio)
    {
        var x = audio.Samples;
        var rate = audio.SampleRate;
        var pitch = Pitch(x, rate);
        var window = (int)Math.Round(0.005 * rate);

        var envelope = Envelope(x, rate, pitch, window);

        // First pass from a guess at the tone's height, then again from the
        // height the first pass measured.
        var sorted = envelope.OrderBy(v => v).ToArray();
        var height = sorted[(int)(sorted.Length * 0.9)];
        var runs = Runs(envelope, height, rate);
        var noise = NoiseEnvelopePower(envelope, runs);

        for (var pass = 0; pass < 2; pass++)
        {
            height = Math.Sqrt(Math.Max(1e-12, MarkEnvelopePower(envelope, runs, rate) - noise));
            runs = Runs(envelope, height, rate);
            noise = NoiseEnvelopePower(envelope, runs);
        }

        var marks = runs.Select(r => 1000.0 * (r.End - r.Start) / rate).ToList();
        var spaces = new List<double>();

        for (var k = 1; k < runs.Count; k++)
        {
            spaces.Add(1000.0 * (runs[k].Start - runs[k - 1].End) / rate);
        }

        var before = runs.Count > 0 ? Math.Max(1, runs[0].Start - window * 4) : x.Length;
        var noiseRms = Math.Sqrt(x.Take(before).Sum(v => (double)v * v) / before);

        // The envelope inside a mark is the tone's peak with the noise's power on
        // top of it; take the noise's out and the peak over root two is the RMS.
        var toneRms = height / Math.Sqrt(2);

        return new RenderedKeying(pitch, marks, spaces, toneRms, noiseRms);
    }

    /// <summary>
    /// Measure a rendering taken apart into its tone and its noise.
    /// </summary>
    /// <param name="audio">The case as rendered.</param>
    /// <param name="noise">
    /// The same recipe rendered with the tone four hundred decibels down: the same
    /// seed and the same length, so the same noise sample for sample, and a tone
    /// too small to move a single float.
    /// </param>
    /// <returns>The measurement, edges and tone from the difference, noise from the noise.</returns>
    /// <remarks>
    /// **AT THE WEAK END THE ENVELOPE CANNOT FIND THE EDGES, AND THAT IS THE NOISE
    /// DOING ITS JOB, NOT THE GENERATOR FAILING.** Taking the noise out leaves the
    /// keying exactly as the case rendered it, so its gaps and its level can be
    /// measured at any signal-to-noise. The tone's RMS is taken over the interiors
    /// of its marks and the noise's over the whole file.
    /// </remarks>
    public static RenderedKeying MeasureApart(MonoAudio audio, MonoAudio noise)
    {
        if (noise.Samples.Length != audio.Samples.Length)
        {
            throw new ArgumentException("The noise rendering is not the case's length.", nameof(noise));
        }

        var tone = new float[audio.Samples.Length];

        for (var n = 0; n < tone.Length; n++)
        {
            tone[n] = audio.Samples[n] - noise.Samples[n];
        }

        var keying = Measure(new MonoAudio(audio.SampleRate, tone));

        var noiseRms = Math.Sqrt(noise.Samples.Sum(v => (double)v * v) / noise.Samples.Length);

        // The tone's own RMS is the envelope's height inside its marks over root
        // two, as Measure found it with no noise under it.
        return keying with { NoiseRms = noiseRms };
    }

    private static double Pitch(float[] x, int rate)
    {
        var best = 0.0;
        var bestHz = 0.0;

        for (var hz = (int)CwFixtureGenerator.PassbandLowHz; hz <= (int)CwFixtureGenerator.PassbandHighHz; hz++)
        {
            var w = 2 * Math.Cos(2 * Math.PI * hz / rate);
            double s1 = 0, s2 = 0;

            for (var i = 0; i < x.Length; i++)
            {
                var s0 = x[i] + (w * s1) - s2;
                s2 = s1;
                s1 = s0;
            }

            var power = (s1 * s1) + (s2 * s2) - (w * s1 * s2);

            if (power > best)
            {
                best = power;
                bestHz = hz;
            }
        }

        return bestHz;
    }

    private static double[] Envelope(float[] x, int rate, double hz, int window)
    {
        var i = new double[x.Length];
        var q = new double[x.Length];

        for (var n = 0; n < x.Length; n++)
        {
            var phase = 2 * Math.PI * hz * n / rate;
            i[n] = x[n] * Math.Cos(phase);
            q[n] = x[n] * Math.Sin(phase);
        }

        var envelope = new double[x.Length];
        double si = 0, sq = 0;

        // A centered boxcar, so both edges of a mark are delayed alike and its
        // length is not.
        var half = window / 2;

        for (var n = 0; n < x.Length + half; n++)
        {
            if (n < x.Length)
            {
                si += i[n];
                sq += q[n];
            }

            if (n - window >= 0 && n - window < x.Length)
            {
                si -= i[n - window];
                sq -= q[n - window];
            }

            var at = n - half;

            if (at >= 0 && at < x.Length)
            {
                envelope[at] = 2 * Math.Sqrt((si * si) + (sq * sq)) / window;
            }
        }

        return envelope;
    }

    private static List<(int Start, int End)> Runs(double[] envelope, double height, int rate)
    {
        var up = 0.6 * height;
        var down = 0.4 * height;
        var shortest = (int)Math.Round(0.006 * rate);

        var runs = new List<(int Start, int End)>();
        var on = false;
        var start = 0;

        for (var n = 0; n < envelope.Length; n++)
        {
            if (!on && envelope[n] >= up)
            {
                on = true;
                start = Cross(envelope, n, 0.5 * height);
            }
            else if (on && envelope[n] < down)
            {
                on = false;
                runs.Add((start, Cross(envelope, n, 0.5 * height)));
            }
        }

        // Merge a gap too short to be keyed, then drop a mark too short to be.
        var merged = new List<(int Start, int End)>();

        foreach (var run in runs)
        {
            if (merged.Count > 0 && run.Start - merged[^1].End < shortest)
            {
                merged[^1] = (merged[^1].Start, run.End);
            }
            else
            {
                merged.Add(run);
            }
        }

        return merged.Where(r => r.End - r.Start >= shortest).ToList();
    }

    private static int Cross(double[] envelope, int n, double level)
    {
        // Walk back to where the envelope passed half height, so hysteresis moves
        // no edge.
        var above = envelope[n] >= level;
        var k = n;

        while (k > 0 && (envelope[k - 1] >= level) == above)
        {
            k--;
        }

        return k;
    }

    private static double MarkEnvelopePower(double[] envelope, List<(int Start, int End)> runs, int rate)
    {
        var margin = (int)Math.Round(0.005 * rate);
        double sum = 0;
        var count = 0;

        foreach (var (start, end) in runs)
        {
            if (end - start < 4 * margin)
            {
                continue;
            }

            for (var n = start + margin; n < end - margin; n++)
            {
                sum += envelope[n] * envelope[n];
                count++;
            }
        }

        return count == 0 ? 0 : sum / count;
    }

    private static double NoiseEnvelopePower(double[] envelope, List<(int Start, int End)> runs)
    {
        var before = runs.Count > 0 ? Math.Max(1, runs[0].Start - 200) : envelope.Length;

        return envelope.Take(before).Sum(v => v * v) / before;
    }
}
