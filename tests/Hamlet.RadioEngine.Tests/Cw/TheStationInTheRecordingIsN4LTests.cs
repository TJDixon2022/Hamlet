using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// `cw-2026-08-17-134712` holds a station and its callsign is N4L.
/// </summary>
/// <remarks>
/// <para>**THIS RECORDING WAS RULED A CARRIER AND IT IS NOT ONE.** HM-DEC-095
/// characterized its strong signal as unkeyed on the 17th, and three sessions
/// then chased a real defect while a ruling in the tree said they were chasing a
/// ghost. The gate's own elements across a second and a half of it spell a
/// United States amateur callsign sent by hand at about twenty-two words a
/// minute, and a carrier cannot produce that.</para>
/// <para>**IT IS THE FIRST ADJUDICATED GROUND TRUTH THIS RECORDING HAS EVER
/// HAD.** Everything else about it is measurement: the keying meter's 0.37 at
/// 500 Hz with a 54 ms element, the gate's 55 ms dits and 235 ms dahs, the two
/// agreeing to within a millisecond. None of that says what was sent. This
/// does.</para>
/// <para>**THE CUTS ARE FITTED FROM THIS STRETCH'S OWN ELEMENTS** (§12.5,
/// HM-DEC-115, HM-DEC-119). Nothing here asks the decoder what a dit is, what a
/// dah is or where a character ends, because the decoder's answer to all three is
/// what is under investigation. The marks are split at the midpoint of their own
/// two means and the gaps likewise, which is the same shape of fit the settled
/// pass uses and shares no code with it.</para>
/// <para>**IT ASSERTS THE ELEMENTS AND THE LETTERS, NOT THAT THE DECODER EMITS
/// THEM.** It does not, and that is the point:
/// `ARecordingWithKeyingInItIsReadTests` holds that failure separately.</para>
/// </remarks>
public sealed class TheStationInTheRecordingIsN4LTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the element sequence is printed.</param>
    public TheStationInTheRecordingIsN4LTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Where the callsign sits, in seconds.</summary>
    /// <remarks>
    /// **THE CLEANEST SECOND AND A HALF OF THE RECORDING**, and the boundaries
    /// are generous either side so that a small change in the gate moves what is
    /// inside them rather than clipping the ends off. The callsign is found by
    /// reading the elements between them, not by trusting them.
    /// </remarks>
    private const double FromSeconds = 21.3;

    /// <summary>Where the clean stretch ends, in seconds.</summary>
    private const double ToSeconds = 23.1;

    /// <summary>One element the gate produced.</summary>
    /// <param name="AtSeconds">When it ended.</param>
    /// <param name="Milliseconds">How long it ran.</param>
    /// <param name="IsMark">Whether the key was down.</param>
    private readonly record struct Element(
        double AtSeconds, double Milliseconds, bool IsMark);

    /// <summary>
    /// Every element the gate produced, taken from the decoder's own running
    /// count rather than from any classifier.
    /// </summary>
    /// <remarks>
    /// The decoder is fed one hop at a time and `ElementsSeen` is watched. It
    /// counts a mark and a gap alternately from the first mark onward, so the
    /// parity of the count is what says which this was. Nothing is asked of the
    /// speed estimator.
    /// </remarks>
    private static List<Element> Elements()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-17-134712.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 500);
        var hop = decoder.Tracker.HopSamples;
        var elements = new List<Element>();
        var seen = 0;
        var lastAt = 0L;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            elements.Add(new Element(
                at / (double)audio.SampleRate,
                (at - lastAt) * 1000.0 / audio.SampleRate,
                elements.Count % 2 == 0));

            lastAt = at;
            seen = decoder.Report.ElementsSeen;
        }

        return elements;
    }

    /// <summary>The midpoint of two means, fitted from the values themselves.</summary>
    /// <param name="values">The lengths.</param>
    /// <returns>Where the short ones stop and the long ones begin.</returns>
    private static double Cut(IReadOnlyList<double> values)
    {
        var split = values.Average();

        for (var pass = 0; pass < 12; pass++)
        {
            var low = values.Where(v => v < split).DefaultIfEmpty(split).Average();
            var high = values.Where(v => v >= split).DefaultIfEmpty(split).Average();
            var next = (low + high) / 2;

            if (Math.Abs(next - split) < 1e-9)
            {
                break;
            }

            split = next;
        }

        return split;
    }

    /// <remarks>
    /// <para>Proves the overturn of HM-DEC-095: the gate's elements across the
    /// clean stretch spell **N4L**, a United States amateur callsign, and a
    /// carrier cannot produce a dah, a dit, four dits and a dah, a dit, a dah and
    /// two dits with character gaps in the right places.</para>
    /// <para>The letters are read through <see cref="MorseAlphabet"/>, which is
    /// the same table the decoder uses, because the question here is what the
    /// pattern spells and not how the pattern was arrived at.</para>
    /// </remarks>
    [Fact]
    public void TheElementsAcrossTheCleanStretchSpellTheCallsign()
    {
        var stretch = Elements()
            .Where(e => e.AtSeconds >= FromSeconds && e.AtSeconds <= ToSeconds)
            .ToList();

        Assert.NotEmpty(stretch);

        _output.WriteLine(
            $"{stretch.Count} elements between {FromSeconds:0.00} s and {ToSeconds:0.00} s");

        foreach (var element in stretch)
        {
            _output.WriteLine(
                $"  {element.AtSeconds,6:0.00}s  "
                + $"{(element.IsMark ? "mark" : "gap ")}  {element.Milliseconds,5:0} ms");
        }

        var marks = stretch.Where(e => e.IsMark).Select(e => e.Milliseconds).ToList();
        var gaps = stretch.Where(e => !e.IsMark).Select(e => e.Milliseconds).ToList();

        var markCut = Cut(marks);
        var gapCut = Cut(gaps);

        _output.WriteLine(
            $"fitted from this stretch: a mark is a dah past {markCut:0} ms, "
            + $"a gap ends a character past {gapCut:0} ms");

        // The stretch has to open on a mark for the letters to divide correctly,
        // and it does: a leading gap would be the tail of whatever came before.
        var letters = new List<string>();
        var pattern = new System.Text.StringBuilder();

        foreach (var element in stretch)
        {
            if (element.IsMark)
            {
                pattern.Append(element.Milliseconds >= markCut ? '-' : '.');
                continue;
            }

            if (element.Milliseconds < gapCut || pattern.Length == 0)
            {
                continue;
            }

            letters.Add(MorseAlphabet.Lookup(pattern.ToString()) ?? "?");
            pattern.Clear();
        }

        if (pattern.Length > 0)
        {
            letters.Add(MorseAlphabet.Lookup(pattern.ToString()) ?? "?");
        }

        var read = string.Concat(letters);

        _output.WriteLine($"the elements spell {read}");

        Assert.Equal("N4L", read);
    }

    /// <remarks>
    /// <para>Proves the same thing one level lower, so a change to
    /// <see cref="MorseAlphabet"/> or to the fitting above cannot make the
    /// callsign appear or disappear on its own: **the marks are two clusters
    /// about four dits apart and the gaps are two clusters about four apart**,
    /// which is what somebody keying produces and what a carrier and a band of
    /// noise both fail to produce.</para>
    /// </remarks>
    [Fact]
    public void TheStretchHasTwoMarkLengthsAndTwoGapLengths()
    {
        var stretch = Elements()
            .Where(e => e.AtSeconds >= FromSeconds && e.AtSeconds <= ToSeconds)
            .ToList();

        var marks = stretch.Where(e => e.IsMark).Select(e => e.Milliseconds).ToList();
        var gaps = stretch.Where(e => !e.IsMark).Select(e => e.Milliseconds).ToList();

        var dit = marks.Where(m => m < Cut(marks)).Average();
        var dah = marks.Where(m => m >= Cut(marks)).Average();
        var inside = gaps.Where(g => g < Cut(gaps)).Average();
        var between = gaps.Where(g => g >= Cut(gaps)).Average();

        _output.WriteLine(
            $"dit {dit:0.0} ms, dah {dah:0.0} ms, ratio {dah / dit:0.00}");
        _output.WriteLine(
            $"element gap {inside:0.0} ms, character gap {between:0.0} ms");

        // **THE NUMBERS THIS WHOLE INVESTIGATION TURNS ON.** A 55 ms dit is about
        // twenty-two words a minute, which is an ordinary hand speed, and a dah
        // of four and a bit dits is a heavy fist rather than an impossible one.
        Assert.InRange(dit, 45, 70);
        Assert.InRange(dah, 200, 280);
        Assert.InRange(dah / dit, 3.5, 5.0);

        // And the gaps are Farnsworth in the manner HM-DEC-115 measured: the gap
        // inside a character is shorter than the dit, not equal to it.
        Assert.True(inside < dit, $"element gap {inside:0} ms was not under the dit");
        Assert.InRange(between, 100, 250);
    }
}
