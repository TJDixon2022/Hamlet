using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-091: the tracked pitch is chosen by keying quality, never by level alone
/// or by the operator's configured pitch (work instruction 447, task 3;
/// PHASE_PLAN.md 4.3).
/// </summary>
/// <remarks>
/// <para>**TWO CASES THE CLAUSE NAMES, BOTH BUILT BY THE GENERATOR** over its shaped
/// noise band (V-06), and the decoder started at the operator's 600 Hz:</para>
/// <list type="bullet">
/// <item>**Level:** 625 Hz keyed at 15 dB under a steady 525 Hz carrier six
/// decibels louder, never keyed. The tracker must end on 625.</item>
/// <item>**The configured pitch:** 675 Hz keyed at 15 dB, 75 Hz from where the
/// decoder was told to look. The tracker must end on 675.</item>
/// </list>
/// <para>"Ends on" is the tracker's pitch over the last half of the message's
/// windows, as <see cref="CwPitchInstrument"/> finds them, within 10 Hz of the
/// truth: under half the tracker's own 25 Hz coarse spacing (author's,
/// overrulable).</para>
/// <para>**WHAT THIS DOES NOT PROVE** (CLAUDE.md 12.5): that the tracker chooses
/// by keying on the air. Task 2's move log shows it moving from cold to the
/// loudest bin before anything is confirmed, which is level alone; these cases
/// pass because the keyed station is confirmed before that matters.</para>
/// </remarks>
public sealed class TheTrackedPitchIsChosenByKeyingTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cases are printed.</param>
    public TheTrackedPitchIsChosenByKeyingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// HM-REQ-091: neither a louder unkeyed carrier nor the configured pitch holds
    /// the tracker off the keyed station.
    /// </summary>
    [Fact]
    public void NeitherALouderCarrierNorTheConfiguredPitchHoldsTheTracker()
    {
        var cases = ThePitchInstrumentIsProvedTests.Cases()
            .Where(c => c.Name is "625 Hz keyed, 525 Hz carrier +6 dB unkeyed, 20 WPM" or "675 Hz, 20 WPM, 15 dB")
            .ToList();

        Assert.Equal(2, cases.Count);

        foreach (var c in cases)
        {
            var (late, truth) = LateTrackerPitch(c);

            _output.WriteLine(string.Create(Invariant,
                $"HM-REQ-091 | {c.Name} | truth {truth:0.0} | tracker over the last half of the keyed windows {late:0.0} | error {late - truth:+0.0;-0.0;0.0}"));

            Assert.True(
                Math.Abs(late - truth) <= 10,
                string.Create(Invariant, $"{c.Name}: the tracker ended on {late:0.0} Hz, the keyed station is at {truth:0.0}"));
        }
    }

    /// <summary>
    /// Task 1's cases, rerun through the decoder: the tracker's pitch beside the
    /// truth and the instrument, every window. A printer; it asserts nothing.
    /// </summary>
    [Fact]
    public void TheInstrumentCasesThroughTheTracker()
    {
        _output.WriteLine("tracker case | case | truth Hz | tracker Hz (median over keyed windows, after the first measured hop) | MET-PITCH-ERR against the truth | worst window | windows more than 25 Hz off");

        foreach (var c in ThePitchInstrumentIsProvedTests.Cases())
        {
            var windows = CwPitchInstrument.Measure(c.Audio.Samples, c.Audio.SampleRate);
            var pitch = WhatPitchTheDecoderIsOnTests.Drive(c.Audio.Samples, c.Audio.SampleRate);
            var beside = WhatPitchTheDecoderIsOnTests.Compare(windows, pitch, c.Audio.SampleRate)
                .Where(b => b.MeasuredShare > 0)
                .ToList();
            var errors = beside.Select(b => b.TrackerHz - c.Truth(b.Window.EffectiveSeconds)).ToList();

            _output.WriteLine(string.Create(Invariant,
                $"tracker case | {c.Name} | {c.Described} | {(beside.Count == 0 ? double.NaN : Median(beside.Select(b => b.TrackerHz))):0.0} | {(errors.Count == 0 ? double.NaN : Median(errors)):+0.0;-0.0;0.0} | {(errors.Count == 0 ? double.NaN : errors.Max(Math.Abs)):0.0} | {errors.Count(e => Math.Abs(e) > 25)} of {errors.Count}"));
        }
    }

    private static (double Late, double Truth) LateTrackerPitch(ThePitchInstrumentIsProvedTests.PitchCase c)
    {
        var windows = CwPitchInstrument.Measure(c.Audio.Samples, c.Audio.SampleRate);
        var pitch = WhatPitchTheDecoderIsOnTests.Drive(c.Audio.Samples, c.Audio.SampleRate);
        var beside = WhatPitchTheDecoderIsOnTests.Compare(windows, pitch, c.Audio.SampleRate);
        var late = beside.Skip(beside.Count / 2).ToList();

        return (Median(late.Select(b => b.TrackerHz)), Median(late.Select(b => c.Truth(b.Window.EffectiveSeconds))));
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();

        return sorted.Count == 0
            ? double.NaN
            : sorted.Count % 2 == 1
                ? sorted[sorted.Count / 2]
                : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2.0;
    }
}
