using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE KEYING MAP** (work instruction 539, task 1, HM-DEC-243): every stretch of time and pitch in each of
/// the owner's twelve recordings where some station keys, read offline and non-causally, whether or not it has a reference.
/// </summary>
/// <remarks>
/// <para>**HOW IT IS READ.** At every pitch from 300 to 900 Hz in the detector's own 25 Hz steps the audio is mixed to
/// nought and passed four times, forward and back, through a 30 Hz one-pole low-pass, and its level taken each
/// millisecond. A mark is a stretch of 25 ms or more where a pitch stands 13 dB over the band at that instant - the median
/// level of every pitch read - stretches under 10 ms apart joined. The radio's AGC moves every pitch together, so it cancels;
/// noise is flat across pitch, and a keyed tone is not.</para>
/// <para>**A FLOOR OVER THE WHOLE RECORDING WAS TRIED FIRST AND FAILED**: a pitch 13 dB over its own 20th percentile keyed
/// three stations in thirty seconds of loud noise and one station from 300 to 900 Hz on every real recording, because the
/// narrow low-pass leaves noise correlated over tens of milliseconds and the AGC pumps the floor.</para>
/// <para>**A NEIGHBOUR'S LEAK IS NOT A STATION.** Through that low-pass a tone leaks 9 dB down one step away and 23 dB down
/// two steps away, so a loud station stands over the floor beside its own pitch too. A mark overlapped by a mark within
/// 100 Hz that is 6 dB louder is dropped as that one's leak.</para>
/// <para>**A STATION IS A RUN OF NEIGHBOURING PITCHES** holding five marks or more, its pitch their marks' mean; a mark
/// alone at a pitch nobody else keys at is dropped. Its keying is its marks, and its spans are its marks joined across
/// gaps under two seconds.</para>
/// <para>**THE MAP DOES NOT DEPEND ON THE REFERENCES.** A weak station with no reference is still keying, so a misread of
/// it is wrong, not invented. A station under 13 dB over its own floor is not seen, and anything read from it counts as
/// invented: that is this instrument's limit, stated rather than tuned.</para>
/// </remarks>
public sealed class TheKeyingMapTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the map is printed.</param>
    public TheKeyingMapTests(ITestOutputHelper output) => _output = output;

    /// <summary>The lowest pitch read.</summary>
    internal const double LowHz = 300;

    /// <summary>The highest pitch read.</summary>
    internal const double HighHz = 900;

    /// <summary>The step between pitches read: the detector's own bin.</summary>
    internal const double StepHz = CwEnvelopeDetector.BinSpacingHz;

    /// <summary>How far over its own floor a pitch must stand to be keyed.</summary>
    internal const double OverFloorDb = 13;

    /// <summary>The shortest mark.</summary>
    internal const double ShortestMarkSeconds = 0.025;

    /// <summary>The marks a station must hold.</summary>
    internal const int StationMarks = 5;

    /// <summary>How long a time with no keying must last to be a silence.</summary>
    internal const double SilenceSeconds = 2;

    /// <summary>
    /// How far a printed letter may sit from the map's keying in time: 10 ms. The live path's marks start within a few
    /// milliseconds of the map's on every recording, the 90th percentile under 7 ms on ten of the twelve
    /// (<c>TheLivePathAgainstTheMap</c>).
    /// </summary>
    internal const double Tolerance = 0.01;

    /// <summary>One keyed mark: its pitch on the grid, when, and its level over the band.</summary>
    internal readonly record struct Mark(double PitchHz, double From, double To, double OverDb);

    /// <summary>One station: its pitch, the grid pitches it keyed at, its marks and its spans.</summary>
    internal sealed record Station(double PitchHz, double LowHz, double HighHz, IReadOnlyList<Mark> Marks, IReadOnlyList<(double From, double To)> Spans);

    /// <summary>A recording's map: its length, its stations, every station mark, and its silences.</summary>
    internal sealed record Map(string Recording, double Seconds, IReadOnlyList<Station> Stations, IReadOnlyList<Mark> Marks, IReadOnlyList<(double From, double To)> Silences)
    {
        /// <summary>
        /// Whether any station keys within one bin either side of a pitch's nearest grid pitch, at any time overlapping a
        /// span widened by a tolerance.
        /// </summary>
        public bool Keyed(double pitchHz, double from, double to, double toleranceSeconds = Tolerance)
        {
            var grid = Math.Round(pitchHz / StepHz) * StepHz;

            return Marks.Any(m => Math.Abs(m.PitchHz - grid) <= StepHz + 0.5 && m.From <= to + toleranceSeconds && m.To >= from - toleranceSeconds);
        }

        /// <summary>The silence a span falls inside, its ends shrunk by a tolerance, or null.</summary>
        public (double From, double To)? SilenceAround(double from, double to, double toleranceSeconds = Tolerance)
            => Silences.Cast<(double From, double To)?>().FirstOrDefault(s => from < s!.Value.To - toleranceSeconds && to > s.Value.From + toleranceSeconds);
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Map> Maps = new(StringComparer.Ordinal);

    /// <summary>A recording's map, read once.</summary>
    internal static Map Of(string recording) => Maps.GetOrAdd(recording, r =>
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(r));

        return Read(r, audio.Samples, audio.SampleRate);
    });

    /// <summary>Reads audio's keying map.</summary>
    internal static Map Read(string name, float[] x, int rate)
    {
        var a = 1 - Math.Exp(-2 * Math.PI * 30 / rate);
        var step = rate / 1000;
        var count = x.Length / step;
        var pitches = new List<double>();

        for (var p = LowHz; p <= HighHz + 0.1; p += StepHz)
        {
            pitches.Add(p);
        }

        var levels = new double[pitches.Count][];

        Parallel.For(0, pitches.Count, k =>
        {
            var i0 = new double[x.Length];
            var q0 = new double[x.Length];

            for (var n = 0; n < x.Length; n++)
            {
                var phase = 2 * Math.PI * pitches[k] * n / rate;

                i0[n] = x[n] * Math.Cos(phase);
                q0[n] = x[n] * Math.Sin(phase);
            }

            foreach (var v in new[] { i0, q0 })
            {
                Pass(v, a, false);
                Pass(v, a, false);
                Pass(v, a, true);
                Pass(v, a, true);
            }

            var db = new double[count];

            for (var t = 0; t < count; t++)
            {
                var j = t * step;

                db[t] = 10 * Math.Log10((i0[j] * i0[j]) + (q0[j] * q0[j]) + 1e-20);
            }

            levels[k] = db;
        });

        // The band at each instant: the median level of the pitches read, which the radio's AGC moves with everything else.
        var band = new double[count];
        var column = new double[pitches.Count];

        for (var t = 0; t < count; t++)
        {
            for (var k = 0; k < pitches.Count; k++)
            {
                column[k] = levels[k][t];
            }

            Array.Sort(column);
            band[t] = column[column.Length / 2];
        }

        var marks = new List<Mark>();

        for (var k = 0; k < pitches.Count; k++)
        {
            var db = levels[k];
            var runs = new List<(int From, int To)>();
            var start = -1;

            for (var t = 0; t <= db.Length; t++)
            {
                var on = t < db.Length && db[t] - band[t] >= OverFloorDb;

                if (on && start < 0)
                {
                    start = t;
                }
                else if (!on && start >= 0)
                {
                    if (runs.Count > 0 && start - runs[^1].To < 10)
                    {
                        runs[^1] = (runs[^1].From, t);
                    }
                    else
                    {
                        runs.Add((start, t));
                    }

                    start = -1;
                }
            }

            foreach (var (from, to) in runs.Where(r => r.To - r.From >= ShortestMarkSeconds * 1000))
            {
                var over = 0.0;

                for (var t = from; t < to; t++)
                {
                    over += db[t] - band[t];
                }

                marks.Add(new Mark(pitches[k], from / 1000.0, to / 1000.0, over / (to - from)));
            }
        }

        // A neighbour's leak is not a station: a mark overlapped by a louder one within 100 Hz, by 6 dB, is dropped.
        var kept = marks
            .Where(m => !marks.Any(o => o.PitchHz != m.PitchHz && Math.Abs(o.PitchHz - m.PitchHz) <= 100 && o.From < m.To && o.To > m.From && o.OverDb >= m.OverDb + 6))
            .ToList();

        // Stations: runs of neighbouring grid pitches, each holding five marks or more together.
        var byPitch = kept.GroupBy(m => m.PitchHz).OrderBy(g => g.Key).ToList();
        var groups = new List<List<Mark>>();
        double? last = null;

        foreach (var g in byPitch)
        {
            if (last is not { } l || g.Key - l > StepHz + 0.5)
            {
                groups.Add(new List<Mark>());
            }

            groups[^1].AddRange(g);
            last = g.Key;
        }

        var stations = groups
            .Where(g => g.Count >= StationMarks)
            .Select(g =>
            {
                var ordered = g.OrderBy(m => m.From).ToList();
                var spans = new List<(double From, double To)>();

                foreach (var m in ordered)
                {
                    if (spans.Count > 0 && m.From - spans[^1].To < SilenceSeconds)
                    {
                        spans[^1] = (spans[^1].From, Math.Max(spans[^1].To, m.To));
                    }
                    else
                    {
                        spans.Add((m.From, m.To));
                    }
                }

                return new Station(g.Average(m => m.PitchHz), g.Min(m => m.PitchHz), g.Max(m => m.PitchHz), ordered, spans);
            })
            .ToList();

        var all = stations.SelectMany(s => s.Marks).OrderBy(m => m.From).ToList();
        var seconds = count / 1000.0;
        var silences = new List<(double From, double To)>();
        var quietFrom = 0.0;

        foreach (var m in all)
        {
            if (m.From - quietFrom >= SilenceSeconds)
            {
                silences.Add((quietFrom, m.From));
            }

            quietFrom = Math.Max(quietFrom, m.To);
        }

        if (seconds - quietFrom >= SilenceSeconds)
        {
            silences.Add((quietFrom, seconds));
        }

        return new Map(name, seconds, stations, all, silences);
    }

    private static void Pass(double[] v, double a, bool back)
    {
        var y = 0.0;

        for (var j = 0; j < v.Length; j++)
        {
            var k = back ? v.Length - 1 - j : j;

            y += a * (v[k] - y);
            v[k] = y;
        }
    }

    /// <summary>A map as the markdown of docs\cw-keying-map.md.</summary>
    internal static string Markdown(Map map)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CultureInfo.InvariantCulture, $"### `{map.Recording}`, {map.Seconds:0.0} s");
        sb.AppendLine();

        if (map.Stations.Count == 0)
        {
            sb.AppendLine("No station keys.");
        }
        else
        {
            sb.AppendLine("| station | grid pitches | marks | keys |");
            sb.AppendLine("|---|---|---|---|");

            foreach (var s in map.Stations.OrderBy(s => s.PitchHz))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {s.PitchHz:0} Hz | {s.LowHz:0}-{s.HighHz:0} | {s.Marks.Count} | {string.Join(", ", s.Spans.Select(p => FormattableString.Invariant($"{p.From:0.0}-{p.To:0.0} s")))} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine(map.Silences.Count == 0
            ? "Silences of two seconds or more: none."
            : "Silences of two seconds or more: " + string.Join(", ", map.Silences.Select(p => FormattableString.Invariant($"{p.From:0.0}-{p.To:0.0} s"))) + ".");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <remarks>
    /// Work instruction 539, task 1: the keying map of each of the owner's twelve recordings, printed as the markdown of
    /// <c>docs\cw-keying-map.md</c>. Asserts only that the first recording's station is found where its letters are.
    /// </remarks>
    [Fact]
    public void TheKeyingMapOfEachRecording()
    {
        foreach (var recording in TheRecordingsScoreboardTests.Stretches.Select(s => s.Recording).Distinct())
        {
            _output.WriteLine(Markdown(Of(recording)));
        }

        Assert.Contains(Of("cw-2026-10-02-200157").Stations, s => Math.Abs(s.PitchHz - 662.8) <= StepHz * 1.5);
    }

    /// <remarks>
    /// How far the live path's marks sit from the map's: for every letter printed, its first mark's start less the start of
    /// the nearest map mark within a bin and 300 ms. Asserts nothing; it sets the tolerance the score reads with.
    /// </remarks>
    [Fact]
    public void TheLivePathAgainstTheMap()
    {
        foreach (var recording in TheRecordingsScoreboardTests.Stretches.Select(s => s.Recording).Distinct())
        {
            var map = Of(recording);
            var (letters, _) = TheRecordingsScoreboardTests.ReadLive(recording);
            var offsets = letters
                .Select(l => map.Marks.Where(m => Math.Abs(m.PitchHz - (Math.Round(l.PitchHz / StepHz) * StepHz)) <= StepHz + 0.5 && Math.Abs(m.From - l.From) <= 0.3)
                    .Select(m => l.From - m.From).OrderBy(Math.Abs).Cast<double?>().FirstOrDefault())
                .ToList();
            var found = offsets.Where(o => o is not null).Select(o => o!.Value * 1000).Order().ToList();

            _output.WriteLine(found.Count == 0
                ? $"{recording}: {letters.Count} letters, none near a map mark"
                : FormattableString.Invariant($"{recording}: {letters.Count} letters, {found.Count} near a map mark; start offset median {found[found.Count / 2]:0} ms, 10th {found[found.Count / 10]:0}, 90th {found[found.Count * 9 / 10]:0}"));
        }
    }

    /// <remarks>
    /// The map's own noise check: thirty seconds of band noise at the scoreboard's loud level keys no station.
    /// </remarks>
    [Fact]
    public void LoudNoiseKeysNoStation()
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: 8000, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 5220)).Samples;
        var map = Read("noise", noise, 8000);

        _output.WriteLine(Markdown(map));

        Assert.Empty(map.Stations);
    }
}
