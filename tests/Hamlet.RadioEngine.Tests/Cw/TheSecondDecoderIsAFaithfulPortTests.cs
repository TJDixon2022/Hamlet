using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Cw.Second;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-122: the second decoder is a faithful port of fldigi's CW
/// receive path, with its license, authors and upstream commit in every
/// ported file, and it reads a send whose key is exact.
/// </summary>
/// <remarks>
/// <para>**THE KEY IS THE PATTERN M.1677-1 GIVES, WRITTEN HERE, AND NOT
/// FLDIGI'S TABLE OR <c>MorseAlphabet</c>'S** (CLAUDE.md 12.5, work instruction
/// 456). The keyer below reads only <see cref="Patterns"/>, and the rendered
/// marks are measured back into dots and dashes and checked against those
/// patterns before anything is decoded.</para>
/// <para>**THE RECIPE**, so another unit can rebuild it: `PARIS CQ` at 18 WPM
/// (fldigi's shipped CWspeed), one unit 1200/18 ms, marks 1 and 3 units, gaps
/// 1, 3 and 7 units; 600 Hz, peak 0.5, raised-cosine edges of 5 ms; 8000 Hz,
/// fldigi's own rate, so no resampling; 10.0 s of noise before the first mark
/// (five of fldigi's slowest time constants, decay weight 1000 at the 500 Hz
/// decision rate, so its AGC has settled from noise_floor 1.0; a first run at
/// 3.0 s read `NES CQ `) and 1.5 s after the last; the noise white Gaussian from xorshift32 and
/// Box-Muller, seed 20260926, shaped to a 300-2800 Hz band by a 257-tap
/// Blackman-windowed sinc band-pass and scaled so tone power over band-noise
/// power is +10 dB, which in a 2500 Hz band is the `CW_SPEC.md` 8.1 reference
/// SNR. Never digital silence (V-06).</para>
/// <para>The decoder is given the pitch the case was keyed at, 600 Hz, as
/// fldigi's waterfall cursor would give it, with the squelch off as fldigi's
/// own file benchmark runs it (benchmark.cxx:54).</para>
/// <para>**RED ON PURPOSE AT UNIT 456, AND NOT TUNED** (HM-REQ-122, 129). The
/// port reads `GARIS CQ `: with no squelch fldigi's thresholds sit inside the
/// noise and key on it; the send's first dot joins a noise key-down already
/// under way, a noise spike right after it sets the receiver idle (cw.cxx:818),
/// and the next key-down from idle clears what it held (cw.cxx:793), so
/// `.--.` arrives as `--.`. That is upstream's logic, ported as it is.</para>
/// </remarks>
public sealed class TheSecondDecoderIsAFaithfulPortTests
{
    private const string Sent = "PARIS CQ";
    private const int WordsPerMinute = 18;
    private const double PitchHz = 600;
    private const double Peak = 0.5;
    private const double EdgeSeconds = 0.005;
    private const double LeadInSeconds = 10.0;
    private const double TailSeconds = 1.5;
    private const double SnrDb = 10;
    private const int Seed = 20260926;
    private const int Rate = FldigiCwDecoder.CW_SAMPLERATE;

    /// <summary>
    /// The key as fldigi's rules print it: each letter at 2 to 4 dot lengths of
    /// silence, a space past 4 (cw.cxx:888-914), so the word gap gives one space
    /// and the silence after the last letter gives the trailing one; none before
    /// the first, because space_sent starts true (cw.cxx:773).
    /// </summary>
    private const string Expected = "PARIS CQ ";

    private const string Commit = "61b97f4133c488063f3de1795c894d22d5032e8a";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the case and the port's reading are printed.</param>
    public TheSecondDecoderIsAFaithfulPortTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The letters of the send with their ITU-R M.1677-1 patterns, typed here.</summary>
    public static IReadOnlyDictionary<char, string> Patterns { get; } = new Dictionary<char, string>
    {
        ['P'] = ".--.",
        ['A'] = ".-",
        ['R'] = ".-.",
        ['I'] = "..",
        ['S'] = "...",
        ['C'] = "-.-.",
        ['Q'] = "--.-",
    };

    /// <summary>Each ported file, its upstream path, and the authors its upstream header names.</summary>
    public static TheoryData<string, string, string[]> PortedFiles { get; } = new()
    {
        { "FldigiCwDecoder.cs", "src/cw_rtty/cw.cxx", new[] { "Dave Freese, W1HKJ", "Mauri Niininen, AG1LE", "Tomi Manninen (oh2bns@sral.fi)", "Lawrence Glaister (ve7it@shaw.ca)" } },
        { "FldigiFftFilter.cs", "src/filters/fftfilt.cxx", new[] { "Dave Freese, W1HKJ" } },
        { "FldigiFft.cs", "src/include/gfft.h", new[] { "Dave Freese, W1HKJ", "John Green" } },
        { "FldigiMovingAverage.cs", "src/filters/filters.cxx", new[] { "Dave Freese, W1HKJ" } },
        { "FldigiMorse.cs", "src/cw_rtty/morse.cxx", Array.Empty<string>() },
        { "FldigiMisc.cs", "src/include/misc.h", new[] { "Dave Freese, W1HKJ" } },
        { "FldigiProgdefaults.cs", "src/include/configuration.h", new[] { "Dave Freese, W1HKJ", "Stelios Bounanos, M0GLD" } },
    };

    /// <summary>
    /// HM-REQ-122: given the pitch, the port prints the key's text, spaced by
    /// fldigi's own rules.
    /// </summary>
    [Fact]
    public void ItReadsASendWhoseKeyIsExact()
    {
        var (audio, marks, noiseRms) = Render();

        Assert.Equal(Sent, Measured(marks));

        var decoder = new FldigiCwDecoder(PitchHz);
        decoder.rx_process(audio);

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"HM-REQ-122 | recipe | `{Sent}` at {WordsPerMinute} WPM, {PitchHz:0} Hz, peak {Peak}, {Rate} Hz, "
            + $"lead-in {LeadInSeconds:0.0} s, tail {TailSeconds:0.0} s, band 300-2800 Hz noise RMS {noiseRms:0.00000}, "
            + $"SNR {SnrDb:0.0} dB in 2500 Hz, seed {Seed}, {audio.Length} samples"));
        _output.WriteLine($"HM-REQ-122 | key `{Sent}` | emitted `{decoder.Text}` | expected `{Expected}`");
        _output.WriteLine("emission | text | at sample | at s | rep | cw_receive_speed | two_dots | CWupper | CWlower");

        foreach (var e in decoder.Emissions)
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"emission | `{e.Text}` | {e.InputSample} | {e.InputSample / (double)Rate:0.000} | {e.Representation} | {e.ReceiveSpeed} | {e.TwoDots} | {e.Upper:0.0000} | {e.Lower:0.0000}"));
        }

        _output.WriteLine("key | kind | at s | element | value | CWupper | CWlower | agc_peak | noise_floor | sig_avg | two_dots | held");

        foreach (var k in decoder.KeyEvents.Where(k => k.InputSample < (LeadInSeconds + 1.2) * Rate))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"key | {k.Kind} | {k.InputSample / (double)Rate:0.000} | {k.Element} | {k.Value:0.0000} | {k.Upper:0.0000} | {k.Lower:0.0000} | {k.AgcPeak:0.00000} | {k.NoiseFloor:0.00000} | {k.SigAvg:0.00000} | {k.TwoDots} | {k.Representation}"));
        }

        _output.WriteLine($"key events | {decoder.KeyEvents.Count} in all, {decoder.KeyEvents.Count(k => k.InputSample < LeadInSeconds * Rate)} before the first mark");

        Assert.Equal(Expected, decoder.Text);
    }

    /// <summary>
    /// HM-REQ-122: every ported file carries fldigi's GPL-3 notice, the authors
    /// its upstream header names, the upstream path and the commit.
    /// </summary>
    /// <param name="file">The ported file.</param>
    /// <param name="upstream">Its upstream path.</param>
    /// <param name="authors">The authors the upstream header names.</param>
    [Theory]
    [MemberData(nameof(PortedFiles))]
    public void EachPortedFileCarriesItsLicenseAuthorsAndCommit(string file, string upstream, string[] authors)
    {
        var path = Path.Combine(CwToneSurveyTests.RepositoryRoot(), "src", "Hamlet.RadioEngine", "Cw", "Second", file);
        var header = string.Join('\n', File.ReadLines(path).TakeWhile(l => !l.StartsWith("using ", StringComparison.Ordinal) && !l.StartsWith("namespace ", StringComparison.Ordinal)));

        var missing = new List<string>();

        foreach (var needle in new[]
                 {
                     "https://github.com/w1hkj/fldigi",
                     Commit,
                     upstream,
                     "General Public License as published by",
                     "either version 3 of the License, or",
                     "WITHOUT ANY WARRANTY",
                     "<http://www.gnu.org/licenses/>",
                 }.Concat(authors))
        {
            if (!header.Contains(needle, StringComparison.Ordinal)) missing.Add(needle);
        }

        _output.WriteLine($"HM-REQ-122 | {file} | {upstream} | authors {authors.Length} | missing {(missing.Count == 0 ? "none" : string.Join("; ", missing))}");

        Assert.Empty(missing);
    }

    /// <summary>Keys the send, measures its marks, and adds the noise band.</summary>
    private static (double[] Audio, List<(bool On, int Start, int End)> Runs, double NoiseRms) Render()
    {
        var unit = 1.2 / WordsPerMinute;
        var segments = new List<(bool On, double Units)>();

        foreach (var word in Sent.Split(' '))
        {
            if (segments.Count > 0) segments.Add((false, 7));

            for (var c = 0; c < word.Length; c++)
            {
                if (c > 0) segments.Add((false, 3));

                var pattern = Patterns[word[c]];

                for (var e = 0; e < pattern.Length; e++)
                {
                    if (e > 0) segments.Add((false, 1));
                    segments.Add((true, pattern[e] == '.' ? 1 : 3));
                }
            }
        }

        var edges = new List<(double Start, double End)>();
        var t = LeadInSeconds;

        foreach (var (on, units) in segments)
        {
            if (on) edges.Add((t, t + (units * unit)));
            t += units * unit;
        }

        var count = (int)Math.Round((t + TailSeconds) * Rate);
        var tone = new double[count];
        var gate = new double[count];
        var step = 2 * Math.PI * PitchHz / Rate;
        var k = 0;

        for (var n = 0; n < count; n++)
        {
            var at = (double)n / Rate;
            while (k < edges.Count - 1 && at >= edges[k].End) k++;

            var (start, end) = edges[k];
            var rise = Math.Clamp((at - start) / EdgeSeconds, 0, 1);
            var fall = Math.Clamp((end - at) / EdgeSeconds, 0, 1);
            gate[n] = 0.5 * (1 - Math.Cos(Math.PI * Math.Min(rise, fall)));
            tone[n] = Peak * gate[n] * Math.Sin(step * n);
        }

        var runs = new List<(bool On, int Start, int End)>();
        var from = 0;

        for (var n = 1; n <= count; n++)
        {
            if (n == count || (gate[n] > 0.5) != (gate[from] > 0.5))
            {
                runs.Add((gate[from] > 0.5, from, n));
                from = n;
            }
        }

        var noise = BandNoise(count);
        var toneRms = Peak / Math.Sqrt(2);
        var noiseRms = toneRms / Math.Pow(10, SnrDb / 20);
        var raw = Math.Sqrt(noise.Sum(v => v * v) / count);

        for (var n = 0; n < count; n++) tone[n] += noise[n] * noiseRms / raw;

        return (tone, runs, noiseRms);
    }

    /// <summary>The rendered marks read back into letters by their lengths in units.</summary>
    private static string Measured(List<(bool On, int Start, int End)> runs)
    {
        var unitSamples = 1.2 / WordsPerMinute * Rate;
        var reverse = Patterns.ToDictionary(p => p.Value, p => p.Key);
        var text = new StringBuilder();
        var letter = new StringBuilder();

        foreach (var (on, start, end) in runs.SkipWhile(r => !r.On))
        {
            var units = (int)Math.Round((end - start) / unitSamples);

            if (on)
            {
                letter.Append(units == 1 ? '.' : '-');
                continue;
            }

            if (units >= 3 || end == runs[^1].End)
            {
                text.Append(reverse.TryGetValue(letter.ToString(), out var ch) ? ch : '?');
                letter.Clear();
            }

            if (units >= 7 && end != runs[^1].End) text.Append(' ');
        }

        return text.ToString();
    }

    /// <summary>Seeded white Gaussian noise shaped to 300-2800 Hz.</summary>
    private static double[] BandNoise(int count)
    {
        var state = (uint)Seed;

        double NextUniform()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return ((state & 0xFFFFFF) + 1) / 16777217.0;
        }

        var white = new double[count];

        for (var i = 0; i < count; i += 2)
        {
            var magnitude = Math.Sqrt(-2 * Math.Log(NextUniform()));
            var angle = 2 * Math.PI * NextUniform();
            white[i] = magnitude * Math.Cos(angle);
            if (i + 1 < count) white[i + 1] = magnitude * Math.Sin(angle);
        }

        const int Taps = 257;
        var taps = new double[Taps];
        var low = 300.0 / Rate;
        var high = 2800.0 / Rate;

        for (var i = 0; i < Taps; i++)
        {
            var x = i - (Taps / 2);
            var band = x == 0
                ? 2 * (high - low)
                : (Math.Sin(2 * Math.PI * high * x) - Math.Sin(2 * Math.PI * low * x)) / (Math.PI * x);
            var window = 0.42 - (0.5 * Math.Cos(2 * Math.PI * i / (Taps - 1))) + (0.08 * Math.Cos(4 * Math.PI * i / (Taps - 1)));
            taps[i] = band * window;
        }

        var shaped = new double[count];

        for (var n = 0; n < count; n++)
        {
            var sum = 0.0;

            for (var i = 0; i < Taps; i++)
            {
                var at = n + i - (Taps / 2);
                if (at >= 0 && at < count) sum += taps[i] * white[at];
            }

            shaped[n] = sum;
        }

        return shaped;
    }
}
