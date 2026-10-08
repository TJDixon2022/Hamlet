using System.Globalization;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A PASSBAND THAT FLICKERS DOES NOT MAKE SENDERS** (work instruction 555, task 2, HM-DEC-259). The 550 Hz sender Hamlet held
/// live through every piece of W1AW's session of 2026-10-07 is not in the audio. One live-only cause the tree can act on was
/// left: the app sets the detector's passband from the rig state twenty times a second, and where the radio's CW pitch or
/// filter goes unread for a tick the detector summed the whole band and rebuilt its bins, then rebuilt them again when the
/// reading came back. Here a scripted rig state plays the radio: CW, its pitch and filter from the pieces' sheets, with the
/// pitch read dropped for one tick every twenty seconds and for three ticks every twenty seconds, ten seconds apart, while the
/// four pieces play through the app's chain. R88 is lifted for these four pieces.
/// </summary>
public sealed class AFlickeringPassbandTests(ITestOutputHelper output)
{
    private static readonly DateTime At = new(2026, 10, 7, 20, 0, 0, DateTimeKind.Utc);

    /// <summary>The radio as the sheets read it: CW, the pitch and filter of piece 1.</summary>
    private static RigState Known()
    {
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(TheW1awSessionReplayTests.Pieces[0]);

        return RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", At, "CI-V 04"),
            RigValue.Known(RigField.CwPitch, pitch, $"{pitch:0} Hz", At, "CI-V 14 09"),
            RigValue.Known(RigField.FilterBandwidth, width, $"{width:0} Hz", At, "CI-V 1A 03"),
        });
    }

    /// <summary>
    /// The scripted rig: the pitch read dropped for one 50 ms tick at 20, 40, 60 s and on, and for three ticks at 10, 30, 50 s
    /// and on; known otherwise.
    /// </summary>
    internal static Func<double, RigState> Flickering()
    {
        var known = Known();
        var dropped = known.With(new[] { RigValue.Unknown(RigField.CwPitch, "timed out") });

        return t =>
        {
            var into = t % 20;
            var tick = 0.05;

            return (into >= 0 && into < tick && t >= 20) || (into >= 10 && into < 10 + (3 * tick)) ? dropped : known;
        };
    }

    /// <remarks>
    /// Every sender held through the session, by pitch, with the passband steady and with it flickering; the text's places of
    /// difference between the two. Asserts that W1AW is the only sender printed, and that no sender beside it holds more marks
    /// than one flicker could make.
    /// </remarks>
    [Fact]
    public void W1awIsHeldAloneAcrossEveryDroppedRead()
    {
        var known = Known();
        CwEnvelopeDetector? steadyDetector = null;
        CwEnvelopeDetector? flickerDetector = null;
        var steady = TheW1awSessionReplayTests.Run(setUp: d => steadyDetector = d, rig: _ => known);
        var flicker = TheW1awSessionReplayTests.Run(setUp: d => flickerDetector = d, rig: Flickering());

        output.WriteLine($"passband rebuilds | steady {steadyDetector!.PassbandRebuilds} | flickering {flickerDetector!.PassbandRebuilds}");

        foreach (var (name, r) in new[] { ("steady", steady), ("flickering", flicker) })
        {
            foreach (var g in r.Senders.GroupBy(s => Math.Round(s.PitchHz / 25) * 25).OrderBy(g => g.Key))
            {
                output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                    $"sender {name} | {g.Key:0} Hz | seen {g.Min(s => s.Seconds):0} to {g.Max(s => s.Seconds):0} s, {g.Select(s => s.Seconds).Distinct().Count()} seconds held | marks up to {g.Max(s => s.Marks)} | shape up to {g.Max(s => s.Shape):0.00} | printed {g.Count(s => s.Printed)} s"));
            }

            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{name} | {r.Letters.Count} letters | {TheW1awSessionReplayTests.Text(r.Letters)[..Math.Min(200, TheW1awSessionReplayTests.Text(r.Letters).Length)]}"));
        }

        var diffs = TheW1awSessionReplayTests.Differences(
            TheW1awSessionReplayTests.Words(steady.Letters).Select(w => w.Word).ToList(),
            TheW1awSessionReplayTests.Words(flicker.Letters));

        foreach (var (s, f, at) in diffs)
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"flicker diff | {at:0.0} s | steady `{s}` | flickering `{f}`"));
        }

        output.WriteLine($"flicker diffs | {diffs.Count} places");

        Assert.All(flicker.Senders.Where(s => s.Printed), s => Assert.InRange(s.PitchHz, 585, 615));
        Assert.All(flicker.Senders.Where(s => Math.Abs(s.PitchHz - 600) > 15), s => Assert.True(s.Marks <= 10, $"a sender at {s.PitchHz:0} Hz held {s.Marks} marks"));
        Assert.Empty(diffs);
    }
}
