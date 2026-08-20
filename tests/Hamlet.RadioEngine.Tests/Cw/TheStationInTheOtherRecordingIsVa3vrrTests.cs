using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// `cw-2026-08-17-013347` holds a station and its callsign is `VA3VRR`.
/// </summary>
/// <remarks>
/// <para>**THE SECOND ADJUDICATED GROUND TRUTH THIS PROJECT HAS.** `N4L` in
/// `cw-2026-08-17-134712` has been the only one for five sessions and every
/// argument in that time has rested on it. This one is a different fist at a
/// different speed on a different band, which is what makes it worth having:
/// `N4L` sends a dah of 4.24 dits and this station sends 2.73, so a rule fitted
/// to one of them has somewhere to be wrong.</para>
/// <para>**IT IS NOT TAKEN FROM THE DECODER'S OWN READING.** The decoder emits
/// `VA3VRR` here, and one of those characters comes out at low confidence, which
/// is exactly why a callsign asserted from an unchecked decode is worth nothing.
/// What is asserted below is read from the gate's elements with the cuts fitted
/// from that stretch and from nothing else (§12.5): the marks are split at the
/// midpoint of their own two means and the gaps at theirs. Nothing asks the
/// decoder what a dit is, what a dah is, or where a character ends.</para>
/// <para>**THE LEADING SILENCE IS DROPPED AND THAT MATTERS.** The stretch is
/// entered on a gap of 325 ms, the quiet before the station starts, and left in
/// the fit it drags the long-gap centre so far that no character divides at all.
/// A gap before the first mark is not one of this sender's, and the sequence
/// starts where the key first goes down.</para>
/// </remarks>
public sealed class TheStationInTheOtherRecordingIsVa3vrrTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the element sequence is printed.</param>
    public TheStationInTheOtherRecordingIsVa3vrrTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Where the callsign sits, in seconds.</summary>
    /// <remarks>
    /// Generous either side of where the decoder reads it, so a small change in
    /// the gate moves what falls inside rather than clipping an end off. The
    /// callsign is found by reading the elements between them, not by trusting
    /// them.
    /// </remarks>
    private const double FromSeconds = 22.4;

    /// <summary>Where the callsign stretch ends, in seconds.</summary>
    private const double ToSeconds = 28.6;

    /// <summary>One element the gate produced.</summary>
    private readonly record struct Element(
        double AtSeconds, double Milliseconds, bool IsMark);

    private static List<Element> Elements()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-17-013347.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;
        var elements = new List<Element>();
        var seen = 0;
        var lastAt = 0.0;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            if (decoder.Report.ElementsSeen == seen)
            {
                continue;
            }

            var ends = at / (double)audio.SampleRate;

            elements.Add(new Element(
                ends, (ends - lastAt) * 1000, elements.Count % 2 == 0));

            lastAt = ends;
            seen = decoder.Report.ElementsSeen;
        }

        return elements;
    }

    /// <summary>The midpoint of two means, fitted from the values themselves.</summary>
    private static double Cut(IReadOnlyList<double> values)
    {
        var split = values.Average();

        for (var pass = 0; pass < 24; pass++)
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

    private static List<Element> Stretch()
        => Elements()
            .Where(e => e.AtSeconds >= FromSeconds && e.AtSeconds <= ToSeconds)
            .SkipWhile(e => !e.IsMark)
            .ToList();

    /// <remarks>
    /// <para>Proves a second station: the gate's elements across this stretch
    /// spell **VA3VRR**, a Canadian amateur callsign, read with cuts fitted from
    /// the stretch's own marks and gaps.</para>
    /// </remarks>
    [Fact]
    public void TheElementsAcrossTheStretchSpellTheCallsign()
    {
        var stretch = Stretch();

        Assert.NotEmpty(stretch);

        _output.WriteLine(
            $"{stretch.Count} elements from {stretch[0].AtSeconds - (stretch[0].Milliseconds / 1000):0.00} s");

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

        Assert.Equal("VA3VRR", read);
    }

    /// <remarks>
    /// <para>Proves the same one level lower, so a change to the fitting above
    /// cannot make the callsign appear or disappear on its own: **the marks are
    /// two clusters and the gaps are two clusters**, which is what somebody keying
    /// produces.</para>
    /// <para>**AND THIS FIST IS NOT `N4L`'S**, which is why it is worth having. It
    /// sends a dah of about two and three quarter dits where `N4L` sends 4.24
    /// (HM-DEC-144), and its element gap is about three quarters of a dit where
    /// `N4L`'s is about two thirds.</para>
    /// </remarks>
    [Fact]
    public void TheStretchIsADifferentFistFromN4L()
    {
        var stretch = Stretch();
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

        Assert.InRange(dit, 90, 115);
        Assert.InRange(dah, 250, 300);
        Assert.InRange(dah / dit, 2.4, 3.1);

        // Farnsworth again, in the manner HM-DEC-115 measured: the gap inside a
        // character is shorter than the dit rather than equal to it.
        Assert.True(inside < dit, $"element gap {inside:0} ms was not under the dit");
        Assert.InRange(between, 110, 200);

        // And it is a different fist from the only other one this project has
        // proved, which is the whole reason for keeping it.
        Assert.True(
            dah / dit < 3.5,
            "this fist's ratio is as heavy as N4L's, so it adds nothing new");
    }
}
