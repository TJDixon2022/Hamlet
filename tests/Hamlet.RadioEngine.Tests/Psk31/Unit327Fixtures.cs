using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 327 task 1: **the eight-second idle fixture.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A TOOL, NOT AN ASSERTION**, in the shape unit 324 used for
/// <see cref="Unit324Fixtures"/>: a fact that does nothing at all unless an environment
/// variable names what to do, so it costs nothing in every run that is not making a
/// fixture. It writes the file and prints its SHA-256, which is then pinned in
/// `assets/fixtures/manifest-step2.json` and checked by the tests that read it.</para>
/// <para>**WHY UNIT 324'S IDLE FIXTURE IS NOT ENOUGH.** That one idles 188 bits - 6.016 s -
/// and its quiet tail is four seconds. Unit 327's retire rule keeps a carrier while its
/// demodulator still vouches for it, and that verdict is a magnitude-weighted mean that
/// takes a couple of seconds to let go of a strong carrier, so the tail has to be long
/// enough that the retirement lands **inside the file** and is `SignalGone` rather than
/// the audio simply running out. The work instruction also names the shape it wants:
/// **types for about ten seconds, idles eight, types again.**</para>
/// <para>**FACT-004, FACT-006: MADE ON A MACHINE WITH NO RADIO.** The file is synthetic.
/// Every number measured from it is an indication and never a finding, and what this phase
/// actually wants is two minutes of the operator's own 14.070.</para>
/// </remarks>
public sealed class Unit327Fixtures
{
    /// <summary>The environment variable that turns this tool on.</summary>
    public const string Switch = "HAMLET_MAKE_FIXTURES";

    /// <summary>What the fixture is called.</summary>
    public const string IdleEightName = "psk31-idle-8s-1000hz.wav";

    /// <summary>Where it puts its carrier, in hertz.</summary>
    /// <remarks>
    /// **1000, THE SAME PLACE EVERY SINGLE-SIGNAL FIXTURE PUTS ITS CARRIER**, and a whole
    /// number of carrier cycles per symbol at 8 kHz - 32 - which is what lets the two halves
    /// be laid end to end with the carrier phase running straight through the join.
    /// </remarks>
    public const double CarrierHz = 1000;

    /// <summary>How long the idle gap is, in bits.</summary>
    /// <remarks>
    /// **250 BITS IS 8.0 s AT 31.25 BAUD**, which is the eight seconds the work instruction
    /// asks for. Idle is a run of `0`s, which is a phase reversal every symbol, so the gap
    /// is eight seconds of a keyed carrier saying nothing - what a PSK31 operator's
    /// transmitter does while he reads what you sent him and decides what to say back.
    /// </remarks>
    public const int IdleBits = 250;

    /// <summary>The signal-to-noise it is made at, in 2500 Hz.</summary>
    public const double SnrDb = 10;

    /// <summary>The seed its noise is drawn with.</summary>
    public const int Seed = 327;

    /// <summary>How long it runs on after the station stops, in seconds.</summary>
    /// <remarks>
    /// **TEN, SO THE CARRIER IS RETIRED INSIDE THE FILE AND NOT BY THE FILE ENDING.**
    /// <see cref="Psk31Listener.RetiredWithinSeconds"/> is 6.0 under unit 327's rule, and a
    /// fixture whose tail is shorter than that can only ever produce `ListeningStopped` -
    /// which would leave the whole point of the file, that it retires **once**, at the end,
    /// and says why, untestable.
    /// </remarks>
    public const double QuietTailSeconds = 10;

    /// <summary>The line the station types, twice.</summary>
    /// <remarks>
    /// **THE REFERENCE CORPUS'S OWN CQ**, the same forty characters every other fixture
    /// sends, which is 10.1 s of varicode at 31.25 baud - the *types for ten seconds* the
    /// work instruction asks for, without a second text to keep in step with the others.
    /// </remarks>
    public const string Line = "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K\r\n";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tool.</summary>
    /// <param name="output">Where the hash is printed.</param>
    public Unit327Fixtures(ITestOutputHelper output) => _output = output;

    /// <summary>Write the fixture, where the environment asks for it.</summary>
    [Fact]
    public void MakeThem()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Switch)))
        {
            return;
        }

        Write(IdleEightName, IdleEight());
    }

    /// <summary>
    /// **One station that types for ten seconds, idles for eight, and types again.**
    /// </summary>
    /// <remarks>
    /// <para>**THE CONVENTION IS THE REFERENCE ONE**, taken from
    /// <see cref="Psk31Modulator"/> rather than written again here, which is what
    /// `assets/reference-modem.py` documents: BPSK at 31.25 baud, a `0` bit a phase
    /// reversal, each character its varicode followed by `00`, and idle a run of `0`s.</para>
    /// <para>**THE JOIN IS IN THE MIDDLE OF THE GAP AND IT IS DELIBERATE**, for unit 324's
    /// reason: the modulator starts and ends every run on silence, so laying two runs end to
    /// end leaves one symbol where the envelope dips to zero and back - 32 ms. Putting it in
    /// the middle of eight seconds of idle means it falls where no character is being
    /// carried, and the idle before it is made an odd or even number of bits so that the
    /// symbol across the join is a reversal like every other idle symbol.</para>
    /// </remarks>
    public static MonoAudio IdleEight()
    {
        const int Rate = 8_000;

        var half = IdleBits / 2;

        // **THE JOIN CONTINUES THE REVERSALS.** The second run starts on +1, so the first
        // run has to end on -1 for the symbol across the join to be a reversal like every
        // other idle symbol. One more idle bit, 32 ms, buys that where the parity is wrong.
        var before = new string('0', 40) + Varicode.Encode(Line) + new string('0', half);

        if (!EndsNegative(before))
        {
            half++;
        }

        var first = Psk31Modulator.Modulate(
            Line, Rate, CarrierHz, 0.5f, idleBefore: 40, idleAfter: half);

        var second = Psk31Modulator.Modulate(
            Line, Rate, CarrierHz, 0.5f, idleBefore: IdleBits - half, idleAfter: 40);

        var tail = (int)(QuietTailSeconds * Rate);
        var joined = new float[first.Length + second.Length + tail];

        first.CopyTo(joined, 0);
        second.CopyTo(joined, first.Length);

        // **THE NOISE IS DRAWN OVER THE TAIL TOO**, so the file ends in band noise rather
        // than in digital silence, which is what a receiver hands over when nobody is
        // transmitting and is what the search has to decide about.
        return new MonoAudio(Rate, Noisy(joined, SnrDb, Rate, first.Length + second.Length));
    }

    /// <summary>Where the station stops keying, in seconds from the start of the file.</summary>
    /// <remarks>
    /// **READ OFF THE SAME MODULATOR THE FIXTURE IS MADE WITH**, so a test asking how long
    /// after the carrier stopped a row survived cannot be reading a number typed twice.
    /// </remarks>
    public static double StopsAtSeconds()
    {
        var audio = IdleEight();

        return (audio.Samples.Length - (QuietTailSeconds * 8_000)) / 8_000.0;
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

    /// <summary>Add white noise at a stated signal-to-noise in 2500 Hz.</summary>
    /// <remarks>
    /// **THE SAME REFERENCE BANDWIDTH THE OTHER FIXTURES USE**, the way an FT8 report quotes
    /// it, so +10 dB here means the same thing it means in `manifest.json`. The draw is
    /// Box-Muller off a seeded generator, so the file is made again byte for byte.
    /// </remarks>
    private static float[] Noisy(float[] signal, double snrDb, int rate, int signalLength)
    {
        double power = 0;

        for (var i = 0; i < signalLength; i++)
        {
            power += (double)signal[i] * signal[i];
        }

        power /= signalLength;

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
                .ToString("0.00", CultureInfo.InvariantCulture) + " s  stops at "
            + StopsAtSeconds().ToString("0.00", CultureInfo.InvariantCulture) + " s");
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
