using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE STATION THE OWNER HEARD AT 01:16:22 IS CALLED KEYING** (work instruction
/// 475, R89, step 11.4, HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>His verdict row of 2026-09-28 01:16:22 UTC, 7.020 to 7.054 MHz under AGC
/// FAST: he pressed "You're an idiot" with the light dark, the meter reading score
/// 0.27, median 48 ms and a swing of 18.6 dB. The swing was the one test it failed,
/// against a bar of 20.</para>
/// <para>**NO RECORDING** (R88). The window is built here: a 600 Hz tone keyed in
/// 48 ms dits at a duty of 0.27, dropping to 18.6 dB below itself between them
/// rather than to silence, so the meter measures the row's swing, element median
/// and score from audio whose answer is known by construction.</para>
/// </remarks>
public sealed class TheOwnersRefusedStationIsKeyingTests
{
    private const int Rate = 48_000;
    private const double ToneHz = 600;
    private const double DitMs = 48;
    private const double Duty = 0.27;
    private const double SwingDb = 18.6;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading is printed.</param>
    public TheOwnersRefusedStationIsKeyingTests(ITestOutputHelper output) => _output = output;

    private static MonoAudio RefusedStation()
    {
        var samples = new float[(int)(Rate * CwKeyingThresholds.Window.TotalSeconds)];
        var period = DitMs / Duty;
        var floor = Math.Pow(10, -SwingDb / 20);

        for (var i = 0; i < samples.Length; i++)
        {
            var ms = i * 1000.0 / Rate;
            var down = ms % period < DitMs;
            var level = down ? 0.5 : 0.5 * floor;

            samples[i] = (float)(level * Math.Sin(2 * Math.PI * ToneHz * i / Rate));
        }

        return new MonoAudio(Rate, samples);
    }

    /// <remarks>
    /// Proves R89 on step 11.4: a window measuring the refused row's figures -
    /// score over the tenth, element median inside 25 to 250 ms, swing between 17
    /// and 20 dB - is called keying from its first window, not held.
    /// </remarks>
    [Fact]
    public void TheRowRefusedForItsSwingIsCalledKeying()
    {
        var meter = new CwKeyingMeter();
        var reading = meter.Update(RefusedStation());

        _output.WriteLine(
            $"{reading.Verdict} at {reading.ToneHz:0} Hz, score {reading.Score:0.00}, "
            + $"element median {reading.ElementMedianMs:0} ms, plain median {reading.MedianMs:0} ms, "
            + $"swing {reading.SwingDb:0.0} dB against {CwKeyingThresholds.ConfidentSwingDb:0.0}, "
            + $"held {reading.Held}");

        // The window measures what the row measured, so the verdict below is about
        // the swing bar and nothing else.
        Assert.InRange(reading.Score, CwKeyingThresholds.KeyingScore, 1);
        Assert.InRange(
            reading.ElementMedianMs,
            CwKeyingThresholds.SlowestChatterMs,
            CwKeyingThresholds.LongestElementMs);
        Assert.InRange(reading.SwingDb, 17, 19.99);

        Assert.Equal(KeyingVerdict.Keying, reading.Verdict);
        Assert.False(reading.Held);
    }
}
