using System.Numerics;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One character the reference decoder produced.</summary>
/// <param name="Text">The character, or a placeholder.</param>
/// <param name="Pattern">The dots and dashes it was read from.</param>
/// <param name="StartSeconds">Where it begins on the audio clock.</param>
/// <param name="EndSeconds">Where it ends.</param>
/// <param name="Confidence">Nought to one, the worse of timing and contrast.</param>
public readonly record struct CwReferenceCharacter(
    string Text,
    string Pattern,
    double StartSeconds,
    double EndSeconds,
    double Confidence);

/// <summary>What the reference decoder made of a recording.</summary>
/// <param name="ToneHz">The pitch it read at, or NaN where it found none.</param>
/// <param name="DitMilliseconds">The fitted dit, or NaN where no clock fitted.</param>
/// <param name="DahMilliseconds">The fitted dah, or NaN.</param>
/// <param name="ContrastDb">The gate's median contrast.</param>
/// <param name="Characters">What it read, in order.</param>
/// <param name="Refusal">Why it emitted nothing, or null where it read something.</param>
public readonly record struct CwReferenceResult(
    double ToneHz,
    double DitMilliseconds,
    double DahMilliseconds,
    double ContrastDb,
    IReadOnlyList<CwReferenceCharacter> Characters,
    string? Refusal)
{
    /// <summary>The transcript as the operator would read it.</summary>
    public string Text
        => string.Concat(Characters.Select(c => c.Text));

    /// <summary>The speed the fitted clock implies, or NaN.</summary>
    public double WordsPerMinute
        => double.IsNaN(DitMilliseconds) ? double.NaN : 1200.0 / DitMilliseconds;

    /// <summary>Nothing was read.</summary>
    /// <param name="refusal">Why.</param>
    /// <param name="toneHz">
    /// The pitch it had got as far as, where it got one. **A refusal that names
    /// the pitch it refused at is worth more than one that does not**: the
    /// reference's own message says "tone at ~N Hz but element timings do not
    /// cluster as Morse", and dropping the number would make the two decoders
    /// disagree about what they know.
    /// </param>
    public static CwReferenceResult None(string refusal, double toneHz = double.NaN)
        => new(toneHz, double.NaN, double.NaN, 0, [], refusal);
}

/// <summary>
/// A port of `cwdecoder.py`, the reference decoder that reads the operator's own
/// captures.
/// </summary>
/// <remarks>
/// <para>**IT IS A PORT AND NOT AN ADAPTATION** (Tim's ruling of 2026-08-28).
/// Same algorithm, same constants, same order, function for function, so the two
/// can be read side by side and a disagreement is a porting bug rather than a
/// design difference. Six families of admission statistic were built and measured
/// dead across five units while this decoder sat in the repository root reading
/// the same audio. **None of those six appears in any published CW decoder,
/// because nobody needs them.**</para>
/// <para>**THE REFUSAL HERE IS STRUCTURAL RATHER THAN A THRESHOLD ANYBODY
/// CHOSE.** <see cref="FitClock"/> returns nothing when the marks do not form two
/// lengths, and nothing downstream runs without a clock. A bin of noise produces
/// no fitting clock, so there is no floor to sweep and no discriminator to pick
/// (HM-DEC-120).</para>
/// <para>**AND ACQUISITION DOES NOT ESTIMATE ITS NOISE FROM THE SIGNAL IT IS
/// SCORING**, which is the fault that made the quietest bin in the band win in
/// unit 1.12.6. <see cref="AcquireTone"/> scores each bin by how far its power
/// spreads, P90 minus P30 in decibels over active audio: a keyed tone spreads, a
/// steady carrier and an empty bin do not.</para>
/// <para>Nothing here keys a transmitter (§0.2).</para>
/// </remarks>
public static class CwReferenceDecoder
{
    /// <summary>How long each frame of the mute mask is, in milliseconds.</summary>
    public const int FrameMilliseconds = 10;

    /// <summary>Below this, in dBFS, the receiver is taken to be muted.</summary>
    public const double MuteThresholdDb = -60;

    /// <summary>How long a mute is held after the audio comes back, for AGC.</summary>
    public const int HoldoffMilliseconds = 150;

    /// <summary>The lowest pitch acquisition looks at.</summary>
    public const double LowestToneHz = 300;

    /// <summary>The highest pitch acquisition looks at.</summary>
    public const double HighestToneHz = 900;

    /// <summary>How far apart the acquisition candidates are.</summary>
    public const double ToneStepHz = 25;

    /// <summary>How much contrast a window needs before the gate will open in it.</summary>
    public const double MinimumContrastDb = 6.0;

    /// <summary>Read a recording the way `cwdecoder.py`'s `run` does.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>What it read, or a refusal saying why it read nothing.</returns>
    public static CwReferenceResult Run(IReadOnlyList<float> samples, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate <= 0 || samples.Count < sampleRate / 10)
        {
            return CwReferenceResult.None("too little audio to read");
        }

        var x = new double[samples.Count];

        for (var i = 0; i < x.Length; i++)
        {
            x[i] = samples[i];
        }

        var mask = MuteMask(x, sampleRate);
        var acquired = AcquireTone(x, sampleRate, mask);

        if (acquired is not { } f0)
        {
            return CwReferenceResult.None(
                "no keyed tone found in 300-900 Hz -> emit nothing");
        }

        var fine = FineEnvelope(x, sampleRate, f0, mask, 0.025);
        var used = UsedTone(fine);
        var (key, contrast) = Gate(fine.T, fine.Edb, fine.Active);
        var dt = fine.T[1] - fine.T[0];

        key = Deglitch(key, dt, 20);

        var r = Runs(key, fine.T, fine.Active);
        var clock = FitClock(Untruncated(r.Marks, r.Trunc));

        if (clock is not { } fitted)
        {
            return CwReferenceResult.None(
                $"tone at ~{used:0} Hz but element timings do not cluster as Morse "
                + "-> emit nothing (this is the honest output)",
                used);
        }

        var (dit, dah) = fitted;

        // Bandwidth follows the decoded speed, as specified. Acquisition ran at
        // ~40 Hz; a slow fist is re-read at ~20 Hz, which is worth about 3 dB of
        // sensitivity on a weak signal and is where the real 013347 capture lives.
        // A fast fist keeps the wider bandwidth, because at 25 WPM a 50 ms window
        // is longer than the dit it is trying to measure.
        if (1200 / dit <= 18)
        {
            fine = FineEnvelope(x, sampleRate, f0, mask, 0.050);
            used = UsedTone(fine);
            (key, contrast) = Gate(fine.T, fine.Edb, fine.Active);
            dt = fine.T[1] - fine.T[0];
            key = Deglitch(key, dt, 20);
            r = Runs(key, fine.T, fine.Active);

            // **THE REFIT IS A REFINEMENT AND A REFINEMENT THAT CHANGES THE
            // ANSWER IS NOT ONE.** The widened acceptance below exists so a heavy
            // fist can be acquired at all; letting it also apply here took
            // farnsworth-light from 100% to 73%, because the narrower re-read
            // produced a clock the old band would have rejected and the original
            // was better.
            var refit = FitClock(Untruncated(r.Marks, r.Trunc), acquiring: false);

            if (refit is { } better)
            {
                (dit, dah) = better;
                fitted = better;
            }
        }

        key = Deglitch(key, dt, 0.4 * dit);
        r = Runs(key, fine.T, fine.Active);

        var positive = contrast.Where(c => c > 0).ToArray();
        var cbar = positive.Length > 0 ? Median(positive) : 0;

        var chars = Decode(
            r.Marks, r.Spaces, r.Starts, fitted, cbar, r.Trunc);

        return new CwReferenceResult(used, dit, dah, cbar, chars, null);
    }

    /// <summary>
    /// Power at one frequency over sliding windows, hopped.
    /// </summary>
    /// <param name="x">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="toneHz">The frequency.</param>
    /// <param name="n">How long a window is, in samples.</param>
    /// <param name="hop">How far the window moves each step.</param>
    /// <returns>One magnitude per window.</returns>
    /// <remarks>
    /// `goertzel_power` (`cwdecoder.py:34`). The reference's own note: a
    /// vectorized reference for what a per-sample streaming Goertzel produces.
    /// </remarks>
    public static double[] GoertzelPower(
        double[] x, int sampleRate, double toneHz, int n, int hop)
    {
        var count = Math.Max(0, (int)Math.Ceiling((x.Length - n) / (double)hop));
        var outp = new double[count];
        var coeff = new Complex[n];

        for (var i = 0; i < n; i++)
        {
            var angle = -2 * Math.PI * toneHz / sampleRate * i;

            coeff[i] = new Complex(Math.Cos(angle), Math.Sin(angle));
        }

        for (var i = 0; i < count; i++)
        {
            var s = i * hop;
            double re = 0;
            double im = 0;

            for (var k = 0; k < n; k++)
            {
                var v = x[s + k];

                re += v * coeff[k].Real;
                im += v * coeff[k].Imaginary;
            }

            outp[i] = Math.Sqrt((re * re) + (im * im)) / n;
        }

        return outp;
    }

    /// <summary>Which frames the receiver was muted or the operator sending in.</summary>
    /// <param name="x">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>One flag per ten-millisecond frame.</returns>
    /// <remarks>
    /// `mute_mask` (`cwdecoder.py:46`). Each mute is extended by the holdoff so
    /// the receiver's own recovery is not read as a signal.
    /// </remarks>
    public static bool[] MuteMask(double[] x, int sampleRate)
    {
        var fl = sampleRate * FrameMilliseconds / 1000;
        var frames = fl > 0 ? x.Length / fl : 0;
        var muted = new bool[frames];

        for (var f = 0; f < frames; f++)
        {
            double sum = 0;

            for (var i = f * fl; i < (f + 1) * fl; i++)
            {
                sum += x[i] * x[i];
            }

            var rms = 20 * Math.Log10(Math.Sqrt(sum / fl) + 1e-9);

            muted[f] = rms < MuteThresholdDb;
        }

        var hold = (int)Math.Ceiling(HoldoffMilliseconds / (double)FrameMilliseconds);
        var outm = (bool[])muted.Clone();

        for (var i = 0; i < frames; i++)
        {
            if (!muted[i])
            {
                continue;
            }

            for (var j = i; j <= Math.Min(i + hold, frames - 1); j++)
            {
                outm[j] = true;
            }
        }

        return outm;
    }

    /// <summary>How much of the recording is not muted.</summary>
    /// <param name="mask">The mute mask.</param>
    /// <returns>Nought to one.</returns>
    public static double FractionActive(bool[] mask)
        => mask.Length == 0 ? 1 : 1.0 - ((double)mask.Count(m => m) / mask.Length);

    /// <summary>Which bin in the band holds a keyed tone.</summary>
    /// <param name="x">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="mask">The mute mask.</param>
    /// <returns>The pitch, on the 25 Hz grid, or null where nothing scored.</returns>
    /// <remarks>
    /// <para>`acquire_tone` (`cwdecoder.py:62`). **Four lines, and it does not
    /// estimate its noise scale from the signal it is scoring.** A keyed tone's
    /// power spreads between key-up and key-down; a steady carrier and an empty
    /// bin do not, whatever their level. So the score is P90 minus P30 of the
    /// bin's power in decibels, over active audio only.</para>
    /// <para>**IT RETURNS A BIN CENTRE AND NOTHING ELSE.** The grid is 300 to 900
    /// by 25, so the answer is always a multiple of 25; the finer number a sheet
    /// reports comes from <see cref="FineEnvelope"/>'s tracking afterwards.</para>
    /// </remarks>
    public static double? AcquireTone(double[] x, int sampleRate, bool[] mask)
    {
        var n = (int)(sampleRate * 0.025);
        var hop = (int)(sampleRate * 0.010);

        if (n <= 0 || hop <= 0)
        {
            return null;
        }

        double? bestF = null;
        var bestScore = 0.0;

        for (var f0 = LowestToneHz; f0 <= HighestToneHz + 1e-9; f0 += ToneStepHz)
        {
            var p = GoertzelPower(x, sampleRate, f0, n, hop);
            var kept = new List<double>(p.Length);

            for (var i = 0; i < p.Length; i++)
            {
                var frame = (int)(i * hop / (double)sampleRate * 1000 / FrameMilliseconds);

                frame = Math.Clamp(frame, 0, Math.Max(0, mask.Length - 1));

                if (mask.Length > 0 && !mask[frame])
                {
                    kept.Add(p[i]);
                }
            }

            if (kept.Count < 50)
            {
                continue;
            }

            var pdb = kept.Select(v => 20 * Math.Log10(v + 1e-9)).ToArray();

            Array.Sort(pdb);

            var score = Percentile(pdb, 90) - Percentile(pdb, 30);

            if (score > bestScore)
            {
                bestScore = score;
                bestF = f0;
            }
        }

        return bestF;
    }

    /// <summary>The detection envelope at the acquired pitch.</summary>
    /// <param name="T">Where each hop sits on the audio clock, in seconds.</param>
    /// <param name="Edb">The envelope, in decibels.</param>
    /// <param name="FInst">Which of the seven offsets won at each hop.</param>
    /// <param name="Active">Whether the audio was live at each hop.</param>
    public readonly record struct Envelope(
        double[] T, double[] Edb, double[] FInst, bool[] Active);

    /// <summary>Track the tone and take its envelope.</summary>
    /// <param name="x">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="centerHz">The acquired pitch.</param>
    /// <param name="mask">The mute mask.</param>
    /// <param name="windowSeconds">How long the analysis window is.</param>
    /// <returns>The envelope and what it was taken at.</returns>
    /// <remarks>
    /// <para>`fine_envelope` (`cwdecoder.py:77`), with the reference's own note
    /// carried over because it records a measurement rather than an
    /// opinion:</para>
    /// <para>HM-DEC-103: this took a fixed 50 ms window, which is ~20 Hz ENBW, in
    /// flat contradiction of this decoder's own specification — "bandwidth
    /// follows the decoded speed: ~40 Hz ENBW before clock lock or above ~18 WPM,
    /// ~20 Hz once locked at slow speeds". The rule was written and never
    /// implemented. It matters because a window that long smears each keyed edge,
    /// so the gate opens early and shuts late and every mark measures about 26 ms
    /// long. Adding a constant to a dit and a dah compresses their ratio, and
    /// this decoder refuses any clock outside 2.5-3.8. At 25 WPM a dit is 48 ms,
    /// so a textbook 1:3 fist measures 74/172 = 2.33 and is refused: **no fixture
    /// above about 15 WPM could pass, whatever it contained.**</para>
    /// </remarks>
    public static Envelope FineEnvelope(
        double[] x,
        int sampleRate,
        double centerHz,
        bool[] mask,
        double windowSeconds)
    {
        var n = (int)(sampleRate * windowSeconds);
        var hop = (int)(sampleRate * 0.010);
        var offsets = new double[] { -15, -10, -5, 0, 5, 10, 15 };
        var powers = new double[offsets.Length][];

        for (var o = 0; o < offsets.Length; o++)
        {
            powers[o] = GoertzelPower(x, sampleRate, centerHz + offsets[o], n, hop);
        }

        var count = powers[0].Length;
        var edb = new double[count];
        var finst = new double[count];
        var t = new double[count];
        var active = new bool[count];

        for (var i = 0; i < count; i++)
        {
            var k = 0;

            for (var o = 1; o < offsets.Length; o++)
            {
                if (powers[o][i] > powers[k][i])
                {
                    k = o;
                }
            }

            edb[i] = 20 * Math.Log10(powers[k][i] + 1e-9);
            finst[i] = centerHz + offsets[k];
            t[i] = i * hop / (double)sampleRate;

            var frame = Math.Clamp(
                (int)(t[i] * 1000 / FrameMilliseconds),
                0,
                Math.Max(0, mask.Length - 1));

            active[i] = mask.Length == 0 || !mask[frame];
        }

        return new Envelope(t, edb, finst, active);
    }

    /// <summary>Two centres fitted to a set of values.</summary>
    /// <param name="v">The values.</param>
    /// <param name="iterations">How many rounds to run.</param>
    /// <returns>The lower and upper centre.</returns>
    /// <remarks>
    /// `two_means` (`cwdecoder.py:105`). Seeded on the fifteenth and
    /// eighty-fifth percentiles, which is what keeps it away from a single
    /// cluster's own tails.
    /// </remarks>
    public static (double Low, double High) TwoMeans(double[] v, int iterations = 15)
    {
        if (v.Length == 0)
        {
            return (0, 0);
        }

        var sorted = (double[])v.Clone();

        Array.Sort(sorted);

        var c1 = Percentile(sorted, 15);
        var c2 = Percentile(sorted, 85);

        for (var it = 0; it < iterations; it++)
        {
            double sa = 0, sb = 0;
            int na = 0, nb = 0;

            foreach (var value in v)
            {
                if (Math.Abs(value - c1) <= Math.Abs(value - c2))
                {
                    sa += value;
                    na++;
                }
                else
                {
                    sb += value;
                    nb++;
                }
            }

            if (na > 0)
            {
                c1 = sa / na;
            }

            if (nb > 0)
            {
                c2 = sb / nb;
            }
        }

        return (c1, c2);
    }

    /// <summary>Decide, hop by hop, whether the key is down.</summary>
    /// <param name="t">The audio clock.</param>
    /// <param name="edb">The envelope in decibels.</param>
    /// <param name="active">Whether the audio was live.</param>
    /// <param name="windowSeconds">How long a threshold window is.</param>
    /// <param name="minimumContrastDb">How much contrast a window needs.</param>
    /// <param name="hysteresisDb">How far apart the two thresholds sit.</param>
    /// <returns>The key state and the contrast behind it.</returns>
    /// <remarks>
    /// `gate` (`cwdecoder.py:113`). **A window whose two centres are less than
    /// six decibels apart gets no threshold at all**, so the key stays up
    /// through it — that is the structural half of the refusal, before any clock
    /// is fitted.
    /// </remarks>
    public static (bool[] Key, double[] Contrast) Gate(
        double[] t,
        double[] edb,
        bool[] active,
        double windowSeconds = 3.0,
        double minimumContrastDb = MinimumContrastDb,
        double hysteresisDb = 6.0)
    {
        var count = edb.Length;
        var key = new bool[count];
        var contrast = new double[count];

        if (count < 2)
        {
            return (key, contrast);
        }

        var dt = t[1] - t[0];
        var w = (int)(windowSeconds / dt);

        if (w < 2)
        {
            return (key, contrast);
        }

        var thMid = new double[count];

        Array.Fill(thMid, double.NaN);

        for (var a = 0; a < count; a += w / 2)
        {
            var b = Math.Min(a + w, count);
            var window = new List<double>(b - a);

            for (var i = a; i < b; i++)
            {
                if (active[i])
                {
                    window.Add(edb[i]);
                }
            }

            if (window.Count < 20)
            {
                continue;
            }

            var (c1, c2) = TwoMeans(window.ToArray());

            if (c2 - c1 >= minimumContrastDb)
            {
                for (var i = a; i < b; i++)
                {
                    thMid[i] = (c1 + c2) / 2;
                    contrast[i] = Math.Max(contrast[i], c2 - c1);
                }
            }
        }

        var on = false;

        for (var i = 0; i < count; i++)
        {
            if (!active[i] || double.IsNaN(thMid[i]))
            {
                on = false;
                key[i] = false;

                continue;
            }

            var hi = thMid[i] + (hysteresisDb / 2);
            var lo = thMid[i] - (hysteresisDb / 2);

            if (!on && edb[i] > hi)
            {
                on = true;
            }
            else if (on && edb[i] < lo)
            {
                on = false;
            }

            key[i] = on;
        }

        return (key, contrast);
    }

    /// <summary>Merge runs too short to be an element.</summary>
    /// <param name="key">The key state.</param>
    /// <param name="dt">How long a hop is, in seconds.</param>
    /// <param name="minimumMilliseconds">The shortest run to keep.</param>
    /// <returns>The key state with the specks merged away.</returns>
    /// <remarks>`deglitch` (`cwdecoder.py:135`).</remarks>
    public static bool[] Deglitch(bool[] key, double dt, double minimumMilliseconds)
    {
        var mn = Math.Max(1, (int)(minimumMilliseconds / 1000 / dt));
        var k = (bool[])key.Clone();

        foreach (var (target, val) in new[] { (false, true), (true, false) })
        {
            var bounds = new List<int> { 0 };

            for (var i = 1; i < k.Length; i++)
            {
                if (k[i] != k[i - 1])
                {
                    bounds.Add(i);
                }
            }

            bounds.Add(k.Length);

            for (var b = 0; b < bounds.Count - 1; b++)
            {
                var a = bounds[b];
                var e = bounds[b + 1];

                if (k[a] == target && e - a < mn)
                {
                    for (var i = a; i < e; i++)
                    {
                        k[i] = val;
                    }
                }
            }
        }

        return k;
    }

    /// <summary>The marks and the spaces between them.</summary>
    /// <param name="Marks">How long each key-down was, in milliseconds.</param>
    /// <param name="Spaces">How long each gap was, in milliseconds.</param>
    /// <param name="Starts">Where each mark began, in seconds.</param>
    /// <param name="Trunc">Whether a mute boundary cut the evidence short.</param>
    public readonly record struct RunSet(
        double[] Marks, double[] Spaces, double[] Starts, bool[] Trunc);

    /// <summary>Cut the key state into marks and spaces.</summary>
    /// <param name="key">The key state.</param>
    /// <param name="t">The audio clock.</param>
    /// <param name="active">Whether the audio was live.</param>
    /// <param name="borderMilliseconds">How close to a mute counts as truncated.</param>
    /// <returns>The runs.</returns>
    /// <remarks>
    /// `runs` (`cwdecoder.py:145`). A mark that begins or ends within the border
    /// of a mute is marked truncated, because its length is a fact about the mute
    /// rather than about the sender.
    /// </remarks>
    public static RunSet Runs(
        bool[] key, double[] t, bool[]? active = null, double borderMilliseconds = 60)
    {
        var starts = new List<int>();
        var ends = new List<int>();

        for (var i = 1; i < key.Length; i++)
        {
            if (key[i] && !key[i - 1])
            {
                starts.Add(i);
            }
            else if (!key[i] && key[i - 1])
            {
                ends.Add(i);
            }
        }

        if (ends.Count > 0 && starts.Count > 0 && ends[0] < starts[0])
        {
            ends.RemoveAt(0);
        }

        var n = Math.Min(starts.Count, ends.Count);

        if (n == 0 || t.Length < 2)
        {
            return new RunSet([], [], [], []);
        }

        var dt = t[1] - t[0];
        var marks = new double[n];
        var startAt = new double[n];
        var trunc = new bool[n];

        for (var i = 0; i < n; i++)
        {
            marks[i] = (ends[i] - starts[i]) * dt * 1000;
            startAt[i] = t[starts[i]];
        }

        var spaces = new double[Math.Max(0, n - 1)];

        for (var i = 0; i < n - 1; i++)
        {
            spaces[i] = (starts[i + 1] - ends[i]) * dt * 1000;
        }

        if (active is not null)
        {
            var w = Math.Max(1, (int)(borderMilliseconds / 1000 / dt));

            for (var i = 0; i < n; i++)
            {
                var a0 = Math.Max(0, starts[i] - w);
                var b0 = Math.Min(active.Length, ends[i] + w);

                for (var j = a0; j < starts[i] && !trunc[i]; j++)
                {
                    if (!active[j])
                    {
                        trunc[i] = true;
                    }
                }

                for (var j = ends[i]; j < b0 && !trunc[i]; j++)
                {
                    if (!active[j])
                    {
                        trunc[i] = true;
                    }
                }
            }
        }

        return new RunSet(marks, spaces, startAt, trunc);
    }

    /// <summary>Fit a dit and a dah, refusing a smear rather than a heavy fist.</summary>
    /// <param name="marks">The mark lengths, in milliseconds.</param>
    /// <param name="acquiring">
    /// False on the slow-fist re-read, where this is a refinement rather than a
    /// first answer and must not replace a working clock.
    /// </param>
    /// <returns>The dit and dah, or null where no clock fits.</returns>
    /// <remarks>
    /// <para>`fit_clock` (`cwdecoder.py:163`), with the reference's own comment
    /// carried over verbatim because it is the record of a mistake this project
    /// has already made:</para>
    /// <para>**THE RATIO BAND WAS 2.5 TO 3.8 AND IT REFUSED A REAL STATION.**
    /// Hamlet's own recording cw-2026-08-17-134712 holds a fist sending a dah of
    /// 4.24 dits, read out of the gate's elements and adjudicated as HM-DEC-144,
    /// and this decoder scored the fixture cut from it at 0% saying the timings
    /// do not cluster as Morse. A judge that cannot read a fist the radio has
    /// heard is not independent, it is wrong.</para>
    /// <para>**WHAT REPLACES IT IS FITTED FROM THE MARKS RATHER THAN ASSUMED
    /// ABOUT THEM.** The question a clock fit has to answer is whether these are
    /// two lengths or one smear, and the statistic for that is how far the two
    /// centres sit apart counted in their own scatter (HM-DEC-095). A fist sends
    /// its dits within a few per cent of each other whatever ratio it chooses; a
    /// gate chattering on noise produces a continuum that two-means cuts in half
    /// wherever it likes.</para>
    /// <para>The dit sanity range stays, because 4 to 40 words a minute is a fact
    /// about people rather than an assumption about their spacing.</para>
    /// <para>**AND THIS IS WHERE THE REFUSAL LIVES.** Returning null here stops
    /// everything downstream, so a band of noise produces no letters without
    /// anybody choosing a threshold (HM-DEC-120).</para>
    /// </remarks>
    public static (double Dit, double Dah)? FitClock(
        double[] marks, bool acquiring = true)
    {
        if (marks.Length < 8)
        {
            return null;
        }

        var (c1, c2) = TwoMeans(marks);

        if (c1 < 30 || c1 > 350)
        {
            // 4-40 WPM sanity
            return null;
        }

        var ratio = c2 / Math.Max(c1, 1e-9);

        if (ratio is >= 2.5 and <= 3.8)
        {
            return (c1, c2);
        }

        if (!acquiring)
        {
            return null;
        }

        return WellSeparated(marks, c1, c2) ? (c1, c2) : null;
    }

    /// <summary>Whether these are two lengths rather than one spread cut in half.</summary>
    /// <param name="marks">The mark lengths.</param>
    /// <param name="c1">The lower centre.</param>
    /// <param name="c2">The upper centre.</param>
    /// <returns>True where the two centres are genuinely apart.</returns>
    /// <remarks>
    /// <para>`well_separated` (`cwdecoder.py:193`), comment carried over
    /// verbatim:</para>
    /// <para>**THE ONLY WAY INTO THE CLOCK FOR A FIST OUTSIDE THE TEXTBOOK
    /// BAND**, and it is a widening that can only accept: anything the ratio band
    /// already took is taken before this is asked, so no reading that worked
    /// before can change.</para>
    /// <para>Tried first as a replacement for the band rather than an addition to
    /// it, and measured: at five decibels the marks scatter enough that
    /// fast-working fell from 58% to nothing at all. The scatter test is right
    /// about shape and wrong about noise, and the band is the other way round, so
    /// the reference keeps both.</para>
    /// </remarks>
    public static bool WellSeparated(double[] marks, double c1, double c2)
    {
        if (c2 <= c1)
        {
            return false;
        }

        var lo = new List<double>();
        var hi = new List<double>();

        foreach (var m in marks)
        {
            if (Math.Abs(m - c1) <= Math.Abs(m - c2))
            {
                lo.Add(m);
            }
            else
            {
                hi.Add(m);
            }
        }

        if (lo.Count < 3 || hi.Count < 3)
        {
            return false;
        }

        var spread = lo.Average(m => Math.Abs(m - c1))
            + hi.Average(m => Math.Abs(m - c2));

        return spread <= 1e-9 || (c2 - c1) / spread >= 4.0;
    }

    /// <summary>Cluster gaps into intra-character, character and word.</summary>
    /// <param name="spaces">The gap lengths, in milliseconds.</param>
    /// <param name="dit">The fitted dit.</param>
    /// <returns>The two boundaries.</returns>
    /// <remarks>
    /// <para>`classify_gaps` (`cwdecoder.py:212`). Falls back to dit multiples
    /// where there are too few gaps to cluster.</para>
    /// <para>**A BOUNDARY SEPARATES TWO POPULATIONS; A JUMP AT THE EDGE OF THE
    /// DATA TRIMS AN OUTLIER** (HM-DEC-107 phase 3). Taking the largest ratios
    /// anywhere in the sorted gaps let a single stray short gap decide a class
    /// boundary. At 25 WPM the gaps are 40, 130 and 330 ms, and the two largest
    /// ratios were the real character-to-word step at 150->270 and a lone 20->30
    /// at the very bottom, which put the element cut at 24 ms. Every 40 ms
    /// element gap then exceeded it and **every element became its own
    /// character**: `TETETTET TETETTET TEEE` out of `CQ CQ DE N0CALL...`.</para>
    /// <para>Requiring a few gaps on each side is what makes a cut a boundary.
    /// Three is enough to reject a lone outlier and small enough to keep the word
    /// gaps, which are genuinely rare — a message has many element gaps, some
    /// character gaps and two or three word gaps, and a floor set by fraction
    /// rather than count would throw the last of those away.</para>
    /// </remarks>
    public static (double IntraHi, double CharHi) ClassifyGaps(
        double[] spaces, double dit)
    {
        var g = spaces.Where(s => s < 12 * dit).OrderBy(s => s).ToArray();
        var intraHi = 0.85 * dit;
        var charHi = 3.0 * dit;

        if (g.Length < 10)
        {
            return (intraHi, charHi);
        }

        var r = new double[g.Length - 1];

        for (var i = 0; i < r.Length; i++)
        {
            r[i] = g[i + 1] / Math.Max(g[i], 1e-9);
        }

        const int Support = 3;

        // `np.argsort(r)[::-1]` — ascending and stable, then reversed.
        var order = Enumerable.Range(0, r.Length)
            .OrderBy(i => r[i])
            .ThenBy(i => i)
            .Reverse()
            .ToArray();

        var j = order
            .Where(i => i + 1 >= Support && g.Length - (i + 1) >= Support)
            .Take(2)
            .ToArray();

        var cuts = j
            .Where(i => r[i] > 1.25)
            .Select(i => Math.Sqrt(g[i] * g[i + 1]))
            .OrderBy(c => c)
            .ToArray();

        if (cuts.Length == 2)
        {
            return (cuts[0], cuts[1]);
        }

        if (cuts.Length == 1)
        {
            return (cuts[0], charHi);
        }

        return (intraHi, charHi);
    }

    /// <summary>Turn elements into characters.</summary>
    /// <param name="marks">The mark lengths.</param>
    /// <param name="spaces">The gap lengths.</param>
    /// <param name="starts">Where each mark began.</param>
    /// <param name="clock">The fitted dit and dah.</param>
    /// <param name="contrastDb">The gate's median contrast.</param>
    /// <param name="trunc">Which marks a mute boundary cut short.</param>
    /// <returns>The characters, in order.</returns>
    /// <remarks>
    /// `decode` (`cwdecoder.py:245`). The table is allowed to say no, an unbroken
    /// long tone is reported as a carrier and never as letters, and confidence is
    /// the worse of the timing margin and the contrast margin (HM-DEC-048).
    /// </remarks>
    public static IReadOnlyList<CwReferenceCharacter> Decode(
        double[] marks,
        double[] spaces,
        double[] starts,
        (double Dit, double Dah) clock,
        double contrastDb,
        bool[]? trunc = null)
    {
        trunc ??= new bool[marks.Length];

        var (dit, dah) = clock;
        var mid = Math.Sqrt(dit * dah);
        var (intraHi, charHi) = ClassifyGaps(spaces, dit);
        var outp = new List<CwReferenceCharacter>();
        var sym = string.Empty;
        var symT0 = 0.0;
        var margins = new List<double>();
        var tainted = false;

        void Flush(double endT)
        {
            if (sym.Length == 0)
            {
                return;
            }

            if (tainted)
            {
                outp.Add(new CwReferenceCharacter(
                    MorseAlphabet.Unreadable, sym, symT0, endT, 0.0));

                return;
            }

            var tm = margins.Count > 0 ? margins.Min() : 0.0;
            var ch = MorseAlphabet.Lookup(sym);
            var snrM = Math.Min(1.0, Math.Max(0.0, (contrastDb - 6.0) / 14.0));
            var conf = Math.Min(tm, snrM);

            outp.Add(ch is null
                ? new CwReferenceCharacter(
                    MorseAlphabet.Unreadable, sym, symT0, endT, 0.0)
                : new CwReferenceCharacter(ch, sym, symT0, endT, conf));
        }

        for (var i = 0; i < marks.Length; i++)
        {
            var m = marks[i];

            if (sym.Length == 0)
            {
                symT0 = starts[i];
                tainted = false;
            }

            if (trunc[i])
            {
                tainted = true;
            }

            if (m >= 8 * dah)
            {
                // unbroken long tone
                Flush(starts[i]);
                sym = string.Empty;
                margins.Clear();

                outp.Add(new CwReferenceCharacter(
                    "<carrier>", string.Empty, starts[i], starts[i] + (m / 1000), 1.0));

                continue;
            }

            sym += m < mid ? "." : "-";

            margins.Add(Math.Min(
                1.0,
                Math.Abs(Math.Log(m / mid)) / Math.Log(Math.Sqrt(dah / dit))));

            var endT = starts[i] + (m / 1000);

            if (i < spaces.Length)
            {
                var sp = spaces[i];

                if (sp > charHi)
                {
                    Flush(endT);
                    sym = string.Empty;
                    margins.Clear();

                    outp.Add(new CwReferenceCharacter(
                        MorseAlphabet.WordGap, string.Empty, endT, endT, 1.0));
                }
                else if (sp > intraHi)
                {
                    Flush(endT);
                    sym = string.Empty;
                    margins.Clear();
                }
            }
        }

        Flush(marks.Length > 0
            ? starts[^1] + (marks[^1] / 1000)
            : 0);

        return outp;
    }

    /// <summary>The pitch the tracking actually settled on.</summary>
    private static double UsedTone(Envelope fine)
    {
        var live = new List<double>();

        for (var i = 0; i < fine.Edb.Length; i++)
        {
            if (fine.Active[i])
            {
                live.Add(fine.Edb[i]);
            }
        }

        if (live.Count == 0)
        {
            return double.NaN;
        }

        var sorted = live.ToArray();

        Array.Sort(sorted);

        var cut = Percentile(sorted, 85);
        var loud = new List<double>();

        for (var i = 0; i < fine.Edb.Length; i++)
        {
            if (fine.Edb[i] > cut)
            {
                loud.Add(fine.FInst[i]);
            }
        }

        return loud.Count == 0 ? double.NaN : Median(loud.ToArray());
    }

    /// <summary>The marks a mute boundary did not cut short.</summary>
    private static double[] Untruncated(double[] marks, bool[] trunc)
    {
        var kept = new List<double>(marks.Length);

        for (var i = 0; i < marks.Length; i++)
        {
            if (!trunc[i])
            {
                kept.Add(marks[i]);
            }
        }

        return kept.ToArray();
    }

    /// <summary>The middle value, numpy's way.</summary>
    private static double Median(double[] values)
    {
        var sorted = (double[])values.Clone();

        Array.Sort(sorted);

        return Percentile(sorted, 50);
    }

    /// <summary>
    /// One value out of a sorted set, interpolating between neighbours as
    /// `numpy.percentile` does.
    /// </summary>
    private static double Percentile(double[] sorted, double percent)
    {
        if (sorted.Length == 0)
        {
            return 0;
        }

        var at = percent / 100.0 * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }
}
