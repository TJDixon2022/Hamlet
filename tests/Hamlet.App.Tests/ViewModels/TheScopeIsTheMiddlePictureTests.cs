using Avalonia;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE SCOPE IS THE MIDDLE PICTURE: A TRACE, BARS ALONG THE BOTTOM, TWO LINES OF WORDS**
/// (work instruction 478 task 1, step 12 criterion 12.2 rewritten, R92).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"We should have replaced the temp controls with something that
/// looks like #2"* - a keyed station's trace with flat tops and flat bottoms, and the dits and
/// dahs marked underneath. No floor, no threshold, no margin, no passband.</para>
/// <para>**A DRIVEN DETECTOR, NO RECORDING** (R88): C and Q at twenty words a minute over
/// seeded noise, made here.</para>
/// </remarks>
public sealed class TheScopeIsTheMiddlePictureTests
{
    private const int Rate = 48_000;

    /// <remarks>
    /// Proves the scope draws the bars and the two lines of words, and no floor, threshold,
    /// margin or passband, and no level trace since R97 (work instruction 485); the first
    /// instruction numbered 480 took the trace off, the second put it back, and 485 took it off
    /// for good. TheScopeDrawsLiveTests asserts the canvas on the live path.
    /// </remarks>
    [Fact]
    public void TheBarsAndTwoLinesOfWordsAreDrawn()
    {
        var detector = Keyed(742, 500, 1.73);
        var frame = CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading, 742, null);

        var lines = CwScopeControl.Lines(frame);

        Assert.Equal(
            new[] { "Bars", "Mixing", "Tone" },
            lines.Select(l => l.Kind.ToString()).OrderBy(k => k, StringComparer.Ordinal).ToArray());

        foreach (var line in lines)
        {
            Assert.False(string.IsNullOrWhiteSpace(line.Label), $"{line.Kind} has no words");

            foreach (var gone in new[] { "gap level", "midway", "floor", "threshold", "margin", "filter", "whole band" })
            {
                Assert.DoesNotContain(gone, line.Label, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <remarks>
    /// Proves the pitch reads "tone 742 Hz heard" while keying - in a mark and in the gap after it -
    /// and "no keying" before the keying and after it has stopped.
    /// </remarks>
    [Fact]
    public void TheToneLineSaysThePitchWhileKeyingAndNoKeyingOtherwise()
    {
        var quiet = Keyed(742, 500, 0.7);
        var before = CwScopeFrame.From(quiet.History(), quiet.HopMs, quiet.Reading, double.NaN, null);

        Assert.Equal("no keying", before.ToneLine);

        // 90 ms into Q's first dah, where a paired bar is up.
        var mark = Keyed(742, 500, 1.73);
        var inMark = CwScopeFrame.From(mark.History(), mark.HopMs, mark.Reading, 742, before);

        Assert.Matches(@"^tone 7[3-5]\d Hz heard$", inMark.ToneLine);

        // 40 ms into the gap after it: still keying, so the pitch holds.
        var gap = Keyed(742, 500, 1.86);

        Assert.True(gap.Reading.Keying && !gap.Reading.Mark, "the driven detector is not in a keyed gap at 1.86 s");

        var inGap = CwScopeFrame.From(gap.History(), gap.HopMs, gap.Reading, 742, inMark);

        Assert.Equal(inMark.ToneLine, inGap.ToneLine);

        // Two seconds after the last element: nobody is keying.
        var after = Keyed(742, 500, 4.6);

        Assert.False(after.Reading.Keying);
        Assert.Equal("no keying", CwScopeFrame.From(after.History(), after.HopMs, after.Reading, 742, inGap).ToneLine);
    }

    /// <remarks>Proves where the decoder mixes is shown beside the detector's pitch, as "decoding at" (work instruction 489).</remarks>
    [Fact]
    public void TheMixingLineSaysWhereTheDecoderIs()
    {
        var detector = Keyed(742, 500, 1.73);

        Assert.Equal(
            "decoding at 750 Hz",
            CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading, 750, null).MixingLine);
        Assert.Equal(
            "not mixing",
            CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading, double.NaN, null).MixingLine);
    }

    /// <remarks>
    /// Proves the hover over a bar says its length and whether it read as a dit or a dah, and
    /// the hover off every bar and letter says what the graph is (§0.6). Until work instruction
    /// 480 the bar's hover was the words "a mark" and the trace had its own.
    /// </remarks>
    [Fact]
    public void TheHoverSaysWhatABarIs()
    {
        var detector = Keyed(742, 500, CwKeyedSeconds);
        var now = DateTime.UtcNow;
        var graph = new CwTrainingGraph();
        graph.Update(detector.History(), detector.HopMs, now);

        var frame = CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading, 742, null) with
        {
            Training = graph.Frame(now),
        };

        const double width = 800;
        var bar = CwScopeControl.Items(frame, width).First(i => i.Kind == CwScopeItemKind.Bar);

        Assert.Matches(
            @"^(dit|dah), \d+ ms$",
            CwScopeControl.TipAt(frame, width, new Point((bar.X + bar.X2) / 2, CwScopeControl.BarTop + 2)));
        Assert.Equal(
            CwHearingViewModel.ScopeTip,
            CwScopeControl.TipAt(frame, width, new Point((bar.X + bar.X2) / 2, CwScopeControl.BarTop + CwScopeControl.TrainingBarHeight + 6)));
    }

    /// <summary>How long the keyed pattern below runs, in seconds.</summary>
    private const double CwKeyedSeconds = 2.58;

    /// <summary>C, Q at 20 words a minute after 0.8 s of noise, then quiet.</summary>
    private static readonly (bool On, double Ms)[] Keying =
    {
        (false, 800),
        (true, 180), (false, 60), (true, 60), (false, 60), (true, 180), (false, 60), (true, 60),
        (false, 180),
        (true, 180), (false, 60), (true, 180), (false, 60), (true, 60), (false, 60), (true, 180),
        (false, 3000),
    };

    /// <summary>A detector fed the keyed pattern for the first <paramref name="seconds"/>.</summary>
    private static CwEnvelopeDetector Keyed(double? pitchHz, double? widthHz, double seconds)
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(pitchHz, widthHz);

        var random = new Random(476);
        var total = (int)(seconds * Rate);
        var audio = new float[total];
        var n = 0;

        foreach (var (on, ms) in Keying)
        {
            for (var i = 0; i < (int)Math.Round(ms * Rate / 1000) && n < total; i++, n++)
            {
                var u1 = 1.0 - random.NextDouble();
                var u2 = random.NextDouble();
                var noise = 0.06 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                var tone = on ? 0.3 * Math.Sin(2 * Math.PI * 742 * n / Rate) : 0;

                audio[n] = (float)(tone + noise);
            }
        }

        for (var offset = 0; offset < total; offset += 960)
        {
            detector.Process(audio.AsSpan(offset, Math.Min(960, total - offset)));
        }

        return detector;
    }
}
