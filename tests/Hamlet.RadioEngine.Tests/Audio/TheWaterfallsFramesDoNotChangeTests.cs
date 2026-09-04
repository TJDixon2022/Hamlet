using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 240, task 2: the picture does not change.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE TEST THE WHOLE TASK IS GOVERNED BY.** Task 2 replaces the
/// spectrum source's ring - a linear array shifted down one place per sample -
/// with a circular buffer and a write cursor. That is a change to how the
/// samples are stored and to nothing else, and the only way to say so credibly
/// is to fix the output first and compare against it afterwards.</para>
/// <para>**THE GOLDEN FILE WAS WRITTEN FROM THE OLD IMPLEMENTATION**, before a
/// line of it was touched, and is committed in that state. A test that captured
/// its own expectation after the change would pass whatever the change did,
/// which is the fixture fault §12.5 exists to name.</para>
/// <para>**IT PINS THE TIMESTAMP TOO, NOT ONLY THE BINS.** The class's own remark
/// says it is deterministic below the pump: a frame's time comes from how many
/// samples have been seen and never from a clock. A circular ring that got the
/// sample count right and the hop boundary wrong would draw the same picture at
/// the wrong moment, and on a mode whose whole geometry is fifteen-second slots
/// that is not a small error.</para>
/// </remarks>
public sealed class TheWaterfallsFramesDoNotChangeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the summary is printed.</param>
    public TheWaterfallsFramesDoNotChangeTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>The pinned frames, from the implementation before task 2.</summary>
    private static string GoldenPath => Path.Combine(
        Root(), "tests", "fixtures", "spectrum", "waterfall-frames.txt");

    /// <summary>
    /// The same audio in gives the same frames out, before and after the ring
    /// became circular.
    /// </summary>
    [Fact]
    public void TheSameAudioGivesTheSameFrames()
    {
        var produced = Render();

        Assert.True(
            File.Exists(GoldenPath),
            "the pinned frames are missing: " + GoldenPath
            + " - it is written from the implementation BEFORE task 2 and "
            + "committed, and regenerating it after a change proves nothing");

        var expected = File.ReadAllText(GoldenPath).Replace("\r\n", "\n");

        _output.WriteLine("frames rendered : "
            + (produced.Split('\n').Length - 1));
        _output.WriteLine("golden file     : " + GoldenPath);

        if (!string.Equals(expected, produced, StringComparison.Ordinal))
        {
            var a = expected.Split('\n');
            var b = produced.Split('\n');

            for (var i = 0; i < Math.Max(a.Length, b.Length); i++)
            {
                var was = i < a.Length ? a[i] : "<no line>";
                var now = i < b.Length ? b[i] : "<no line>";

                if (!string.Equals(was, now, StringComparison.Ordinal))
                {
                    _output.WriteLine("first difference at line " + (i + 1));
                    _output.WriteLine("  pinned  : " + Trim(was));
                    _output.WriteLine("  produced: " + Trim(now));

                    break;
                }
            }
        }

        Assert.Equal(expected, produced);
    }

    /// <summary>
    /// The frames run in order and at the hop the window declares.
    /// </summary>
    /// <remarks>
    /// **A SEPARATE ASSERTION FROM THE GOLDEN FILE, BECAUSE IT SURVIVES A
    /// DELIBERATE CHANGE TO THE PICTURE.** If the taper or the decibel floor is
    /// ever ruled to change, the golden file is regenerated and this one still
    /// holds: the cadence is a property of the pump, not of the picture.
    /// </remarks>
    [Fact]
    public void FramesArriveInOrderAtTheDeclaredHop()
    {
        var times = new List<DateTime>();
        var source = new AudioSpectrumSource(Rate);

        source.FrameReady += (in SpectrumFrame frame) => times.Add(frame.TimestampUtc);
        source.Start();

        source.Push(Audio(Rate * 3));

        Assert.NotEmpty(times);

        var hop = AudioSpectrumSource.WindowAt48K / AudioSpectrumSource.HopDivisor;
        var expected = TimeSpan.FromSeconds((double)hop / Rate);

        _output.WriteLine("frames  : " + times.Count);
        _output.WriteLine("hop     : " + hop + " samples = "
            + expected.TotalMilliseconds.ToString("0.###") + " ms");

        for (var i = 1; i < times.Count; i++)
        {
            Assert.True(times[i] > times[i - 1],
                "frame " + i + " is not after frame " + (i - 1));

            var gap = times[i] - times[i - 1];

            Assert.True(
                Math.Abs((gap - expected).TotalMilliseconds) < 0.001,
                "frame " + i + " came " + gap.TotalMilliseconds
                + " ms after the one before, not " + expected.TotalMilliseconds);
        }
    }

    /// <summary>Render the fixture's frames as text, for pinning.</summary>
    /// <remarks>
    /// **PUSHED IN UNEVEN BUFFERS ON PURPOSE.** A device does not hand over
    /// whole hops, and a ring that only works when the buffer divides the hop
    /// would pass a tidier fixture and fail on the radio. 4,800 is the real
    /// buffer at a 100 ms period and does not divide the 4,096-sample hop.
    /// </remarks>
    internal static string Render()
    {
        var text = new StringBuilder();
        var source = new AudioSpectrumSource(Rate);

        source.FrameReady += (in SpectrumFrame frame) =>
        {
            text.Append(frame.TimestampUtc.ToString(
                "HH:mm:ss.fffffff", CultureInfo.InvariantCulture));
            text.Append(' ');
            text.Append(frame.LowHz.ToString(CultureInfo.InvariantCulture));
            text.Append('-');
            text.Append(frame.HighHz.ToString(CultureInfo.InvariantCulture));
            text.Append(' ');

            foreach (var bin in frame.Bins)
            {
                text.Append(bin.ToString("X2", CultureInfo.InvariantCulture));
            }

            text.Append('\n');
        };

        source.Start();

        var audio = Audio(Rate * 4);

        for (var at = 0; at < audio.Length; at += 4_800)
        {
            source.Push(audio.AsSpan(at, Math.Min(4_800, audio.Length - at)));
        }

        return text.ToString();
    }

    /// <summary>
    /// Deterministic audio: three steady tones and a burst, so the picture has
    /// structure a wrong ring would visibly scramble.
    /// </summary>
    private static float[] Audio(int count)
    {
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var t = (double)i / Rate;

            var value =
                (0.30 * Math.Sin(2 * Math.PI * 700 * t))
                + (0.18 * Math.Sin(2 * Math.PI * 1_240 * t))
                + (0.09 * Math.Sin(2 * Math.PI * 2_010 * t));

            // A burst in the middle second, so a ring that loses ordering shows
            // it as a smear rather than as a shifted edge.
            if (i > count / 3 && i < count / 2)
            {
                value += 0.25 * Math.Sin(2 * Math.PI * 1_500 * t);
            }

            // A fixed pseudo-noise floor, deterministic and seedless.
            value += 0.0025 * Math.Sin(i * 12.9898);

            samples[i] = (float)value;
        }

        return samples;
    }

    private static string Trim(string line)
        => line.Length <= 96 ? line : line[..96] + "... (" + line.Length + " chars)";

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
