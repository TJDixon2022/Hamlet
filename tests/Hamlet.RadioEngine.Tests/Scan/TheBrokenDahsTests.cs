using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **A DAH THAT BREAKS IN TWO** (work instruction 546, task 1): the nine dahs of the main scan catch that an offline read
/// hears whole and the chain printed as two dits, traced through the detector's stood marks, the gate's printed marks and the
/// sender's own window, so the break is found where it happens.
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the three scan catches in the tree; this reads the main one.</para>
/// <para>**THE PLAIN READ IS NOT GROUND TRUTH** (see <see cref="TheStrongStationsOverTests"/>). It says where a dah of about
/// 150 ms sits; the chain's marks around it are the measurement.</para>
/// </remarks>
public sealed class TheBrokenDahsTests
{
    /// <summary>Where the nine dahs start, in seconds, as the work instruction names them.</summary>
    internal static readonly double[] Times = [3.54, 31.62, 32.65, 40.69, 49.17, 52.14, 53.63, 69.54, 72.11];

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public TheBrokenDahsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Task 1: at each of the nine times, the plain read's marks, every mark the detector stood within 30 Hz of the station,
    /// the marks the gate printed with their kind, and the sender's own window over the stretch, its level every hop and the
    /// deepest dip inside the plain read's dah against the dah's own top. Asserts only that the station is heard.
    /// </remarks>
    [Fact]
    public void WhereTheNineDahsBreak()
    {
        var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(TheStrongStationsOverTests.Main));
        var chain = TheStrongStationsOverTests.ReadChain(audio.Samples, audio.SampleRate);
        var station = chain.PitchHz;
        var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, station);
        var lane = Lane(audio, station, plain.Dit);
        var broken = 0;
        var whole = 0;

        _output.WriteLine(string.Create(Invariant, $"station {station:0} Hz; plain dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms"));

        foreach (var t in Times)
        {
            var dah = plain.Marks.Where(m => m.Kind == '-' && m.From > t - 0.08 && m.From < t + 0.08).OrderBy(m => Math.Abs(m.From - t)).FirstOrDefault();
            var from = (dah?.From ?? t) - 0.06;
            var to = (dah?.To ?? t + 0.16) + 0.06;
            var sb = new StringBuilder();

            sb.Append(string.Create(Invariant, $"{t:0.00} s: plain "));
            sb.Append(dah is null ? "no dah" : string.Create(Invariant, $"{dah.From:0.000}-{dah.To:0.000} ({(dah.To - dah.From) * 1000:0} ms)"));

            var stood = chain.Marks.Where(m => Math.Abs(m.PitchHz - station) <= 30 && m.ToSeconds > from && m.FromSeconds < to).OrderBy(m => m.FromSeconds).ToList();

            sb.Append("; stood ");
            sb.Append(string.Join(", ", stood.Select(m => string.Create(Invariant, $"{m.FromSeconds:0.000}-{m.ToSeconds:0.000} ({m.LengthMs:0} ms, {m.LevelDb:0.0} dB)"))));

            for (var i = 1; i < stood.Count; i++)
            {
                sb.Append(string.Create(Invariant, $" gap {(stood[i].FromSeconds - stood[i - 1].ToSeconds) * 1000:0} ms"));
            }

            var printed = chain.Printed.Where(m => m.To > from && m.From < to).OrderBy(m => m.From).ToList();

            sb.Append("; printed ");
            sb.Append(string.Join(", ", printed.Select(m => string.Create(Invariant, $"{(m.To - m.From) * 1000:0} ms '{m.Kind}' letter {m.Letter}"))));

            if (printed.Count > 0)
            {
                var p = printed[0];

                sb.Append(string.Create(Invariant, $"; sender dit {p.Dit * 1000:0} ms, split {p.Split * 1000:0} ms, letter line {p.CharacterLine * 1000:0} ms, level {p.LevelRef:0.0} dB"));
            }

            if (dah is not null)
            {
                var inside = lane.Where(l => l.At >= dah.From + 0.02 && l.At <= dah.To - 0.02).ToList();

                if (inside.Count > 0)
                {
                    var top = inside.Select(l => l.Db).OrderByDescending(d => d).ElementAt(inside.Count / 4);

                    sb.Append(string.Create(Invariant, $"; own window: top {top:0.0} dB, deepest {inside.Min(l => l.Db):0.0} dB at {inside.MinBy(l => l.Db).At:0.000} s, a dip of {top - inside.Min(l => l.Db):0.0} dB"));
                }
            }

            if (stood.Count(m => dah is not null && m.ToSeconds > dah.From && m.FromSeconds < dah.To) >= 2)
            {
                broken++;
            }
            else
            {
                whole++;
            }

            _output.WriteLine(sb.ToString());
        }

        _output.WriteLine($"stood in two or more pieces: {broken} of {Times.Length}; whole or missing: {whole}");

        Assert.NotEmpty(chain.Text);
    }

    /// <summary>The sender's own window as the detector builds it, tuned to the station and its dit: its level every 5 ms.</summary>
    private static List<(double At, double Db)> Lane(MonoAudio audio, double pitchHz, double ditSeconds)
    {
        var lane = new CwSenderLane(audio.SampleRate);
        var hop = audio.SampleRate / 200;
        var levels = new List<(double, double)>();

        lane.Tune(pitchHz, ditSeconds);

        for (var n = 0; n < audio.Samples.Length; n++)
        {
            lane.Push(audio.Samples[n]);

            if ((n + 1) % hop == 0)
            {
                levels.Add((((n + 1) / (double)audio.SampleRate) - lane.DelaySeconds, lane.LevelDb));
            }
        }

        return levels;
    }
}
