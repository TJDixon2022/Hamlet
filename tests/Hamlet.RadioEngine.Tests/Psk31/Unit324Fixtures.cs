using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 324 tasks 2 and 3: **make the two fixtures those tasks need.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A TOOL, NOT AN ASSERTION**, in the shape unit 322 used for
/// `Unit322Dump`: a fact that does nothing at all unless an environment variable names
/// what to do, so it costs nothing in every run that is not making a fixture. It writes
/// the files and prints their SHA-256, which is then pinned in
/// `assets/fixtures/manifest-step2.json` and checked by the tests that read them.</para>
/// <para>**FACT-004, FACT-006: MADE ON A MACHINE WITH NO RADIO.** Both files are
/// synthetic. Every number measured from them is an indication and never a finding, and
/// what this phase actually wants is two minutes of the operator's own 14.070.</para>
/// </remarks>
public sealed class Unit324Fixtures
{
    /// <summary>The environment variable that turns this tool on.</summary>
    public const string Switch = "HAMLET_MAKE_FIXTURES";

    /// <summary>Where the idle-gap fixture puts its carrier, in hertz.</summary>
    /// <remarks>
    /// **1000, THE SAME PLACE EVERY SINGLE-SIGNAL FIXTURE PUTS ITS CARRIER**, and a whole
    /// number of carrier cycles per symbol at 8 kHz - 32 - which is what lets the two
    /// halves be laid end to end with the carrier phase running straight through the join.
    /// </remarks>
    public const double IdleGapCarrierHz = 1000;

    /// <summary>How long the idle gap is, in bits.</summary>
    /// <remarks>
    /// **188 BITS IS 6.016 s AT 31.25 BAUD.** Idle is a run of `0`s, which is a phase
    /// reversal every symbol, so the gap is six seconds of a keyed carrier saying nothing -
    /// exactly what a PSK31 operator's transmitter does between words, and exactly the
    /// case that killed every carrier on the operator's own evening.
    /// </remarks>
    public const int IdleGapBits = 188;

    /// <summary>The signal-to-noise the idle-gap fixture is made at, in 2500 Hz.</summary>
    public const double IdleGapSnrDb = 10;

    /// <summary>The seed the idle-gap fixture's noise is drawn with.</summary>
    public const int Seed = 324;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tool.</summary>
    /// <param name="output">Where the hashes are printed.</param>
    public Unit324Fixtures(ITestOutputHelper output) => _output = output;

    /// <summary>Write both fixtures, where the environment asks for them.</summary>
    [Fact]
    public void MakeThem()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Switch)))
        {
            return;
        }

        Write("psk31-four-signals-48k.wav", FourSignalsAtFortyEight());
        Write("psk31-idle-gap-1000hz.wav", IdleGap());
    }

    /// <summary>
    /// **The four-signal fixture, raised to the rate the operator's sound card runs at.**
    /// </summary>
    /// <remarks>
    /// <para>**IT IS THE SAME AUDIO, NOT A SECOND RECORDING.** Every carrier, every word
    /// and the noise are the ones `psk31-four-signals.wav` already carries and the
    /// manifest already pins; only the grid under them changes. So a difference between
    /// what Hamlet reads from the two files is a difference the plumbing made.</para>
    /// <para>**WHAT THIS FIXTURE CANNOT CATCH, SAID OUT LOUD** (§0.0). It is raised with
    /// a band-limited filter, so it holds nothing at all above 4 kHz - which means a path
    /// that came down to 8 kHz by keeping every sixth sample and filtering nothing would
    /// read it perfectly. The anti-alias filter is therefore proved separately and
    /// directly, by feeding <see cref="Psk31Resampler"/> a tone above the new Nyquist and
    /// measuring what comes out where it would fold to.</para>
    /// </remarks>
    private static MonoAudio FourSignalsAtFortyEight()
    {
        var eight = WavAudio.Read(Fixture("psk31-four-signals.wav"));

        return new MonoAudio(
            48_000, Ft8Resample.Resample(eight.Samples, eight.SampleRate, 48_000));
    }

    /// <summary>
    /// **One station that says something, idles for six seconds, and says it again.**
    /// </summary>
    /// <remarks>
    /// <para>**THE CONVENTION IS THE REFERENCE ONE**, taken from
    /// <see cref="Psk31Modulator"/> rather than written again here: BPSK at 31.25 baud,
    /// a `0` bit a phase reversal, each character its varicode followed by `00`, and idle
    /// a run of `0`s. The gap is <see cref="IdleGapBits"/> idle bits, which is keying -
    /// so a search that retires on *signal gone* must hold the carrier across it, and one
    /// that retires on *no characters* cannot.</para>
    /// <para>**THE JOIN IS IN THE MIDDLE OF THE GAP AND IT IS DELIBERATE.** The modulator
    /// starts and ends every run on silence, so laying two runs end to end leaves one
    /// symbol where the envelope dips to zero and back - 32 ms. Putting it in the middle
    /// of six seconds of idle means it falls where no character is being carried, and the
    /// idle before it is made an even or odd number of bits so that the first symbol of
    /// the second half continues the reversals rather than breaking them.</para>
    /// </remarks>
    private static MonoAudio IdleGap()
    {
        const string Text = "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K\r\n";
        const int Rate = 8_000;

        var half = IdleGapBits / 2;

        // **THE JOIN CONTINUES THE REVERSALS.** The second run starts on +1, so the first
        // run has to end on -1 for the symbol across the join to be a reversal like every
        // other idle symbol. One more idle bit, 32 ms, buys that where the parity is wrong.
        var before = new string('0', 40) + Varicode.Encode(Text) + new string('0', half);

        if (!EndsNegative(before))
        {
            half++;
        }

        var first = Psk31Modulator.Modulate(
            Text, Rate, IdleGapCarrierHz, 0.5f, idleBefore: 40, idleAfter: half);

        var second = Psk31Modulator.Modulate(
            Text, Rate, IdleGapCarrierHz, 0.5f, idleBefore: IdleGapBits - half, idleAfter: 40);

        var joined = new float[first.Length + second.Length];

        first.CopyTo(joined, 0);
        second.CopyTo(joined, first.Length);

        return new MonoAudio(Rate, Noisy(joined, IdleGapSnrDb, Rate));
    }

    /// <summary>Whether a run of bits leaves the differential symbol at -1.</summary>
    private static bool EndsNegative(string bits)
    {
        var sign = 1;

        foreach (var bit in bits)
        {
            if (bit == '0')
            {
                sign = -sign;
            }
        }

        return sign < 0;
    }

    /// <summary>
    /// Add white noise at a stated signal-to-noise in 2500 Hz.
    /// </summary>
    /// <remarks>
    /// **THE SAME REFERENCE BANDWIDTH THE OTHER FIXTURES USE**, the way an FT8 report
    /// quotes it, so +10 dB here means the same thing it means in `manifest.json`. The
    /// draw is Box-Muller off a seeded generator, so the file is made again byte for byte.
    /// </remarks>
    private static float[] Noisy(float[] signal, double snrDb, int rate)
    {
        double power = 0;

        foreach (var sample in signal)
        {
            power += (double)sample * sample;
        }

        power /= signal.Length;

        var inReference = power / Math.Pow(10, snrDb / 10);
        var sigma = Math.Sqrt(inReference * (rate / 2.0) / Psk31CarrierSearch.ReferenceBandwidthHz);

        var random = new Random(Seed);
        var made = new float[signal.Length];

        for (var i = 0; i < made.Length; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            var normal = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

            made[i] = (float)Math.Clamp(signal[i] + (sigma * normal), -1, 1);
        }

        return made;
    }

    private void Write(string name, MonoAudio audio)
    {
        var path = Fixture(name);

        WavAudio.Write(path, audio);

        using var stream = File.OpenRead(path);

        var digest = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

        _output.WriteLine(
            name + "  " + digest + "  "
            + audio.SampleRate.ToString(CultureInfo.InvariantCulture) + " Hz  "
            + (audio.Samples.Length / (double)audio.SampleRate)
                .ToString("0.00", CultureInfo.InvariantCulture) + " s");
    }

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

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
