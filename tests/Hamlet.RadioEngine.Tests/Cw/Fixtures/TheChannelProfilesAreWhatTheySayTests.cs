using System.Globalization;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Every CH-* profile <see cref="CwChannel"/> produces is what CW_SPEC.md 9.1 says
/// it is, measured on its own output (V-06, V-12; CW_SPEC.md 8.1 and 9.1; serves
/// HM-REQ-040, 041 and 042; work instruction 461 task 2).
/// </summary>
/// <remarks>
/// <para>**PROVED AGAINST THE PUBLISHED MODEL, NEVER BY THE DECODER** (CLAUDE.md
/// 12.5). Nothing here asks a decoder whether it can read the result. Every figure
/// is measured from what the layer produced - the audio, the pieces summed into it
/// and the path gains it applied - and never read back from the recipe.</para>
/// <para>**THE TOLERANCES, STATED BEFORE ANY RUN, EACH WITH ITS REASON:**</para>
/// <list type="bullet">
/// <item>Spread within 10 % of 9.1's figure. The estimate is the second moment of a
/// Welch spectrum over 2^18 tap samples, 127 half-overlapped segments at a
/// resolution of sigma / 32, which scatters about 2 %; 9.1 states its values to one
/// or two figures; the nearest wrong readings (the spread taken as sigma, or as the
/// width of the amplitude rather than the power spectrum) are 100 % and 41 % out.
/// The mean Doppler shift within 5 % of the spread: 9.1 has none.</item>
/// <item>Power within 0.3 dB: the two paths against each other, and their sum against
/// the unfaded power. A long-run mean of a Gaussian-spectrum fade over 2^18 taps at
/// 128 sigma scatters about 1.2 %, whatever the spread (0.05 dB); the wrong builds
/// are 3 dB (each path at full power) and infinitely (one path) out.</item>
/// <item>Delay within one sample, 0.125 ms, of 9.1's figure on every mark: the sample
/// is the resolution, and every 9.1 delay is a whole number of samples.</item>
/// <item>SNR within 0.5 dB, the instruction's figure: the tone's power over the band's
/// density beside it times 2500 Hz, the density from a Welch spectrum of the band
/// as added, averaged over 20 Hz either side of the tone.</item>
/// <item>V-06: no 10 ms block anywhere more than 30 dB below the band's RMS before the
/// first mark, and no sample clipped. A 10 ms block of this band holds about six
/// degrees of freedom, so the band alone falls 30 dB short with a chance near
/// 5e-9 a block; silence, a mute or a faded-away band falls without limit.</item>
/// </list>
/// </remarks>
public sealed class TheChannelProfilesAreWhatTheySayTests
{
    private const double Pitch = 600;

    private const double Wpm = 20;

    private const int Taps = 1 << 18;

    private const int Segment = 4096;

    private static readonly string LongMessage =
        string.Join(' ', Enumerable.Repeat(SyntheticCq.Text, 4));

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each measurement is printed.</param>
    public TheChannelProfilesAreWhatTheySayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The nine fading profiles 9.1 gives values for.</summary>
    public static TheoryData<string> Fading { get; } = Rows(p => p.Fades);

    /// <summary>All ten profiles 9.1 gives values for.</summary>
    public static TheoryData<string> Every { get; } = Rows(_ => true);

    /// <summary>CH-AWGN's SNR at the three levels the instruction names.</summary>
    public static TheoryData<double> Levels { get; } = new() { -7, -4, 15 };

    /// <summary>1. Each path's Doppler spectrum is Gaussian-wide as 9.1 says, measured off the tap process.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Fading))]
    public void EachPathSpreadsAsNamed(string id)
    {
        var profile = CwChannel.Profile(id);
        var rate = CwChannel.TapRate(profile.SpreadHz);

        foreach (var k in new[] { 1, 2 })
        {
            var (spread, shift) = Spread(CwChannel.Path(profile.SpreadHz, Seed(id), k, Taps), rate);

            Print($"spread | {id} | path {k} | measured two sigma {spread:0.0000} Hz against {profile.SpreadHz:0.###} Hz ({100 * ((spread / profile.SpreadHz) - 1):+0.0;-0.0;0.0} %) | shift {shift:+0.0000;-0.0000} Hz");

            Assert.InRange(spread, 0.9 * profile.SpreadHz, 1.1 * profile.SpreadHz);
            Assert.InRange(Math.Abs(shift), 0, 0.05 * profile.SpreadHz);
        }
    }

    /// <summary>2. The two paths carry equal power, and together the unfaded power.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Fading))]
    public void ThePathsAreEqualAndSumToTheUnfadedPower(string id)
    {
        var profile = CwChannel.Profile(id);
        var p1 = MeanPower(CwChannel.Path(profile.SpreadHz, Seed(id), 1, Taps));
        var p2 = MeanPower(CwChannel.Path(profile.SpreadHz, Seed(id), 2, Taps));
        var ratio = 10 * Math.Log10(p1 / p2);
        var sum = 10 * Math.Log10(p1 + p2);

        // On the audio too, printed and not asserted: a minute of fading is too
        // short a mean for the slow profiles.
        var parts = CwChannel.Render(id, 15, LongMessage, Wpm, Pitch, Seed(id));
        var audio = AudioPowerAgainstUnfaded(parts);

        // **THE PATHS AS RENDERED CARRY THE PROCESSES MEASURED ABOVE.** Inside a
        // mark the envelope is exactly one, so each rendered path there is its
        // gain; set against the tap process over the same samples, linearly
        // interpolated as channels.md states, it must carry the same power. The
        // long-run figures above prove the process; this proves the render uses
        // it (a render that dropped a path passed without it - watched, round B).
        var rendered1 = RenderedAgainstTaps(parts, 1);
        var rendered2 = RenderedAgainstTaps(parts, 2);

        Print($"power | {id} | path 1 {10 * Math.Log10(p1):+0.00;-0.00;0.00} dB, path 2 {10 * Math.Log10(p2):+0.00;-0.00;0.00} dB | ratio {ratio:+0.00;-0.00;0.00} dB | sum {sum:+0.00;-0.00;0.00} dB against the unfaded | on the audio over {parts.Audio.Duration.TotalSeconds:0} s {audio:+0.00;-0.00;0.00} dB | rendered against their taps: path 1 {rendered1:+0.000;-0.000;0.000} dB, path 2 {rendered2:+0.000;-0.000;0.000} dB");

        Assert.InRange(ratio, -0.3, 0.3);
        Assert.InRange(sum, -0.3, 0.3);
        Assert.InRange(rendered1, -0.3, 0.3);
        Assert.InRange(rendered2, -0.3, 0.3);
    }

    /// <summary>3. Path two lags path one by 9.1's delay on every mark, read off the tap-applied signals.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Fading))]
    public void TheSecondPathLagsByTheDelay(string id)
    {
        var profile = CwChannel.Profile(id);
        var parts = CwChannel.Render(id, 15, SyntheticCq.Text, Wpm, Pitch, Seed(id));
        var one = Onsets(parts.Path1);
        var two = Onsets(parts.Path2);
        var want = profile.DelayMs * CwChannel.SampleRate / 1000;

        Assert.Equal(one.Count, two.Count);
        Assert.NotEmpty(one);

        var lags = one.Zip(two, (a, b) => b - a).ToList();
        var mean = lags.Average() * 1000 / CwChannel.SampleRate;

        Print($"delay | {id} | {lags.Count} marks | measured {mean:0.000} ms (lags {lags.Min()} to {lags.Max()} samples) against {profile.DelayMs:0.###} ms ({want:0} samples)");

        Assert.All(lags, lag => Assert.InRange(lag, want - 1, want + 1));
        Assert.InRange(mean, profile.DelayMs - 0.125, profile.DelayMs + 0.125);
    }

    /// <summary>4. CH-AWGN's SNR, measured and referred to 2500 Hz, is the one asked for.</summary>
    /// <param name="snrDb">The request, in the 2500 Hz reference.</param>
    [Theory]
    [MemberData(nameof(Levels))]
    public void TheSnrIsInTheReference(double snrDb)
    {
        var parts = CwChannel.Render("CH-AWGN", snrDb, LongMessage, Wpm, Pitch, 461200 + (int)snrDb);
        var measured = MeasuredSnr(parts, out var inPassband);

        Print($"snr | CH-AWGN | asked {snrDb:+0.0;-0.0} dB reference | measured {measured:+0.00;-0.00;0.00} dB reference ({measured - snrDb:+0.00;-0.00;0.00}) | {inPassband:+0.00;-0.00;0.00} dB in the band's own bandwidth");

        Assert.InRange(measured, snrDb - 0.5, snrDb + 0.5);
    }

    /// <summary>5. V-06: no 10 ms block of any output falls below the band, anywhere, and nothing clips.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Every))]
    public void NoBlockFallsBelowTheBand(string id)
    {
        foreach (var snr in new[] { -7.0, 15.0 })
        {
            var parts = CwChannel.Render(id, snr, LongMessage, Wpm, Pitch, Seed(id) + (int)snr);
            var x = parts.Audio.Samples;
            var lead = (int)(0.9 * CwChannel.SampleRate);
            var floor = Math.Sqrt(x.Take(lead).Sum(v => (double)v * v) / lead);
            var block = CwChannel.SampleRate / 100;
            var lowest = double.MaxValue;
            var at = 0;

            for (var start = 0; start + block <= x.Length; start += block)
            {
                double sum = 0;

                for (var n = start; n < start + block; n++)
                {
                    sum += (double)x[n] * x[n];
                }

                var rms = Math.Sqrt(sum / block);

                if (rms < lowest)
                {
                    lowest = rms;
                    at = start;
                }
            }

            var below = 20 * Math.Log10(lowest / floor);

            Print($"v06 | {id} | {snr:+0;-0} dB reference | {x.Length / block} blocks | band rms {20 * Math.Log10(floor):0.00} dBFS | lowest block {below:+0.00;-0.00;0.00} dB at {(double)at / CwChannel.SampleRate:0.00} s | clipped {parts.Clipped} | peak {20 * Math.Log10(x.Max(v => Math.Abs(v))):0.0} dBFS");

            Assert.True(below > -30, $"{id} at {snr} dB: a 10 ms block at {(double)at / CwChannel.SampleRate:0.00} s sits {below:0.0} dB under the band.");
            Assert.Equal(0, parts.Clipped);
        }
    }

    /// <summary>6. The key is the message keyed, and a character the table cannot key is refused.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Every))]
    public void TheKeyIsTheMessage(string id)
    {
        var generated = CwChannel.Generate(id, -4, SyntheticCq.Text, Wpm, Pitch, Seed(id));

        Assert.Equal(SyntheticCq.Text, generated.Key);
        Assert.Throws<ArgumentException>(() => CwChannel.Generate(id, -4, "CQ ~", Wpm, Pitch, Seed(id)));
    }

    /// <summary>7. The same seed gives the same audio, byte for byte; another seed does not.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Every))]
    public void TheSameSeedGivesTheSameAudio(string id)
    {
        var a = CwChannel.Generate(id, -4, SyntheticCq.Text, Wpm, Pitch, Seed(id));
        var b = CwChannel.Generate(id, -4, SyntheticCq.Text, Wpm, Pitch, Seed(id));
        var c = CwChannel.Generate(id, -4, SyntheticCq.Text, Wpm, Pitch, Seed(id) + 1);

        Assert.Equal(Bytes(a.Audio.Samples), Bytes(b.Audio.Samples));
        Assert.Equal(a.Sidecar, b.Sidecar);
        Assert.NotEqual(Bytes(a.Audio.Samples), Bytes(c.Audio.Samples));
    }

    /// <summary>8. CH-MDV is refused by name, with 9.1's silence and 12's source as the reason.</summary>
    [Fact]
    public void ChMdvIsRefusedByName()
    {
        var refused = Assert.Throws<NotSupportedException>(
            () => CwChannel.Generate("CH-MDV", -4, SyntheticCq.Text, Wpm, Pitch, 461300));

        Print($"refusal | CH-MDV | {refused.Message}");

        Assert.Contains("CH-MDV", refused.Message, StringComparison.Ordinal);
        Assert.Contains("9.1 gives no delay or spread", refused.Message, StringComparison.Ordinal);
        Assert.Contains("12 item 2", refused.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => CwChannel.Profile("CH-XX"));
    }

    /// <summary>The output is the band plus the faded tone and nothing else, sample for sample.</summary>
    /// <param name="id">The profile.</param>
    [Theory]
    [MemberData(nameof(Every))]
    public void TheOutputIsThePiecesSummed(string id)
    {
        var parts = CwChannel.Render(id, -4, SyntheticCq.Text, Wpm, Pitch, Seed(id));

        for (var n = 0; n < parts.Audio.Samples.Length; n++)
        {
            var c = parts.Path1[n] + parts.Path2[n];
            var phase = 2 * Math.PI * Pitch * n / CwChannel.SampleRate;
            var tone = parts.Amplitude * ((c.Real * Math.Cos(phase)) - (c.Imaginary * Math.Sin(phase)));

            // The phase is summed in another order here, so the two agree to
            // rounding, not to the bit.
            Assert.Equal(tone, parts.Signal[n], 1e-9);
            Assert.Equal((float)Math.Clamp(parts.Noise[n] + parts.Signal[n], -1, 1), parts.Audio.Samples[n]);
        }

        Print($"sidecar | {id}\n{parts.Sidecar}");
    }

    private static TheoryData<string> Rows(Func<CwChannelProfile, bool> which)
    {
        var rows = new TheoryData<string>();

        foreach (var p in CwChannel.Profiles.Where(which))
        {
            rows.Add(p.Id);
        }

        return rows;
    }

    private static int Seed(string id)
        => 461100 + CwChannel.Profiles.ToList().FindIndex(p => p.Id == id);

    private static (double Spread, double Shift) Spread(Complex[] tap, double rate)
    {
        var (hz, density) = CwSpectrum.Welch(tap, rate, Segment);
        double m0 = 0, m1 = 0, m2 = 0;

        for (var k = 0; k < hz.Length; k++)
        {
            m0 += density[k];
            m1 += density[k] * hz[k];
            m2 += density[k] * hz[k] * hz[k];
        }

        var mean = m1 / m0;
        var sigma = Math.Sqrt((m2 / m0) - (mean * mean));

        return (2 * sigma, mean);
    }

    private static double MeanPower(Complex[] x)
        => x.Sum(v => (v.Real * v.Real) + (v.Imaginary * v.Imaginary)) / x.Length;

    // The faded tone's mean power where both envelopes are fully on, against the
    // unfaded tone's A^2 / 2, in decibels.
    private static double AudioPowerAgainstUnfaded(CwChannelParts parts)
    {
        double sum = 0;
        var count = 0;

        for (var n = 0; n < parts.Signal.Length; n++)
        {
            if (!FullyOn(parts, n))
            {
                continue;
            }

            sum += parts.Signal[n] * parts.Signal[n];
            count++;
        }

        return 10 * Math.Log10(sum / count / (parts.Amplitude * parts.Amplitude / 2));
    }

    // Path k's power as rendered, over the samples where both envelopes are fully
    // on, against its tap process over the same samples, in decibels.
    private static double RenderedAgainstTaps(CwChannelParts parts, int k)
    {
        var spread = parts.Profile.SpreadHz;
        var count = parts.Signal.Length;
        var ratio = CwChannel.TapRate(spread) / CwChannel.SampleRate;
        var taps = CwChannel.Path(spread, Seed(parts.Profile.Id), k, (int)Math.Floor((count - 1) * ratio) + 2);
        var path = k == 1 ? parts.Path1 : parts.Path2;
        double rendered = 0, process = 0;

        for (var n = 0; n < count; n++)
        {
            if (!FullyOn(parts, n))
            {
                continue;
            }

            var u = n * ratio;
            var i = (int)Math.Floor(u);
            var h = (taps[i] * (1 - (u - i))) + (taps[i + 1] * (u - i));

            rendered += path[n].Magnitude * path[n].Magnitude;
            process += h.Magnitude * h.Magnitude;
        }

        return 10 * Math.Log10(rendered / process);
    }

    // Both paths' envelopes at full height: the samples well inside a mark on both.
    private static bool FullyOn(CwChannelParts parts, int n)
    {
        var t = (double)n / CwChannel.SampleRate;
        var lag = (double)parts.DelaySamples / CwChannel.SampleRate;
        const double Edge = 0.006;

        for (var s = 0; s + 1 < parts.Edges.Length; s += 2)
        {
            if (t >= parts.Edges[s] + lag + Edge && t <= parts.Edges[s + 1] - Edge)
            {
                return true;
            }
        }

        return false;
    }

    private static double MeasuredSnr(CwChannelParts parts, out double inPassband)
    {
        // The tone's power inside marks, from the signal summed into the output.
        double sum = 0;
        var count = 0;

        for (var n = 0; n < parts.Signal.Length; n++)
        {
            if (FullyOn(parts, n))
            {
                sum += parts.Signal[n] * parts.Signal[n];
                count++;
            }
        }

        var tone = sum / count;

        // The band as it stands in the output: the output less the tone.
        var band = new float[parts.Audio.Samples.Length];

        for (var n = 0; n < band.Length; n++)
        {
            band[n] = (float)(parts.Audio.Samples[n] - parts.Signal[n]);
        }

        var (hz, density) = CwSpectrum.WelchReal(band, CwChannel.SampleRate, Segment);
        var n0 = CwSpectrum.DensityNear(hz, density, Pitch, 20);
        var bandPower = band.Sum(v => (double)v * v) / band.Length;

        inPassband = 10 * Math.Log10(tone / bandPower);

        return 10 * Math.Log10(tone / (n0 * CwChannel.ReferenceHz));
    }

    // The first sample of every mark: where a path's baseband leaves exact nought.
    private static List<int> Onsets(Complex[] path)
    {
        var onsets = new List<int>();
        var on = false;

        for (var n = 0; n < path.Length; n++)
        {
            var now = path[n] != Complex.Zero;

            if (now && !on)
            {
                onsets.Add(n);
            }

            on = now;
        }

        return onsets;
    }

    private static byte[] Bytes(float[] samples)
    {
        var bytes = new byte[samples.Length * sizeof(float)];
        Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private void Print(FormattableString line)
        => _output.WriteLine(line.ToString(CultureInfo.InvariantCulture));
}
