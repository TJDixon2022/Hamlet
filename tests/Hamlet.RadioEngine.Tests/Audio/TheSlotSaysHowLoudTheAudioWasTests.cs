using System.Globalization;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Hamlet.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Every slot the reader runs now says how loud the audio in it was (unit 236).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE FORK EVERY DIAGNOSIS OF 2026-09-03 DIED ON.** The owner sat
/// at 14.074, pressed what twenty-seven units had built, and saw an empty table.
/// Four units went after the silence and not one of them could answer the first
/// question anybody asks about a receiver that hears nothing: **was there any audio
/// in it.** <see cref="Ft8SlotCensus"/> carried five decode counts, three Costas
/// match counts and a sample rate, so a muted sound card, a USB cable half out, a
/// laptop microphone in a quiet room and a twenty metre band with no decodable FT8
/// on it produced byte-identical evidence.</para>
/// <para>**IT IS A LEVEL AND IT IS NOT A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md`
/// §0.0). Nothing here is compared against this mode's published sensitivity
/// figure, because how loud the audio was and how strong a signal in it was are
/// different quantities.</para>
/// <para>**AND AN ALL-ZERO SLOT REFUSES RATHER THAN READING MINUS NINETY**
/// (HM-DEC-009). That is the first test below and it is the one the design turns
/// on: the tap's own conversion floors a zero at
/// <see cref="AudioLevel.SilenceDb"/>, which is correct for a moving bar and is a
/// plausible number in a column somebody will average.</para>
/// <para>**MEASURED BEFORE ANY BOUND IS ASSERTED** (the habit since unit 212).
/// Every value is printed first, so a tolerance chosen after seeing the number
/// cannot launder a failure.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2), and no capture
/// device is opened.</para>
/// </remarks>
public sealed class TheSlotSaysHowLoudTheAudioWasTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the levels are printed.</param>
    public TheSlotSaysHowLoudTheAudioWasTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What a sound card delivers.</summary>
    private const int DeviceRate = 48_000;

    /// <summary>What the tap keeps.</summary>
    private const int RecordingSeconds = 30;

    /// <summary>Where the synthesized transmission is put, in the passband.</summary>
    private const double PlacedAtHz = 1240;

    /// <summary>
    /// The moment the recording ended, by a clock measured at no drift. Thirty
    /// seconds back is 14:22:17, so exactly one whole slot fits: 14:22:30.
    /// </summary>
    private static readonly DateTime EndedAt =
        new(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// The moment an off-air recording is treated as having ended.
    /// </summary>
    /// <remarks>
    /// **ON A BOUNDARY, BECAUSE UPSTREAM'S RECORDINGS ARE SLOT CAPTURES.** They are
    /// exactly fifteen seconds long and they begin where a slot begins, so the only
    /// end moment that leaves one whole slot in them is a quarter minute itself.
    /// Ending anywhere else cuts across two of upstream's slots and the reader
    /// correctly finds none.
    /// </remarks>
    private static readonly DateTime OffAirEndedAt =
        new(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc);

    /// <summary>
    /// **DIGITAL SILENCE REFUSES, AND THE ZERO COUNT IS WHAT SAYS WHY.**
    /// </summary>
    /// <remarks>
    /// A slot of nought has no logarithm, so both levels are absent rather than
    /// floored. What separates it from a genuinely quiet band is the count of
    /// samples that were exactly zero standing at the whole slot: a receiver
    /// listening to a dead band still delivers its own noise, and noise is not
    /// nought.
    /// </remarks>
    [Fact]
    public void ASlotOfDigitalSilenceReportsNoLevelAndEveryLastSampleZero()
    {
        var audio = new MonoAudio(DeviceRate, new float[DeviceRate * RecordingSeconds]);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        var slot = Assert.Single(heard.Slots);

        Print("digital silence", slot);

        Assert.Null(slot.Level.PeakDbFullScale);
        Assert.Null(slot.Level.RmsDbFullScale);
        Assert.Equal(slot.Level.SampleCount, slot.Level.ZeroSampleCount);
        Assert.Equal(1.0, slot.Level.ZeroSampleFraction);
        Assert.Equal(DeviceRate * 15, slot.Level.SampleCount);
    }

    /// <summary>
    /// **A KNOWN AMPLITUDE READS WHAT THE ARITHMETIC PREDICTS.**
    /// </summary>
    /// <remarks>
    /// A sine at half full scale peaks at 0.5, which is twenty times the base-ten
    /// logarithm of a half, or -6.0206 decibels relative to full scale. Its root
    /// mean square over whole cycles is a half over the square root of two, which
    /// is -9.0309. The slot is exactly fifteen thousand whole cycles long, so
    /// neither figure has a partial cycle in it. **The bound is a twentieth of a
    /// decibel and it is asserted against arithmetic done here rather than against
    /// a number this run produced.**
    /// </remarks>
    [Fact]
    public void AToneAtHalfFullScaleReadsMinusSixPeakAndMinusNineRms()
    {
        var samples = new float[DeviceRate * RecordingSeconds];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)(0.5 * Math.Sin(2 * Math.PI * 1000 * i / DeviceRate));
        }

        var heard = Ft8Reader.Read(
            new MonoAudio(DeviceRate, samples), EndedAt, Measured);

        var slot = Assert.Single(heard.Slots);

        Print("a 1000 Hz sine at half full scale", slot);

        var predictedPeak = 20 * Math.Log10(0.5);
        var predictedRms = 20 * Math.Log10(0.5 / Math.Sqrt(2));

        _output.WriteLine(Line(
            "  predicted peak ", predictedPeak, " dBFS, predicted rms ",
            predictedRms, " dBFS"));

        Assert.NotNull(slot.Level.PeakDbFullScale);
        Assert.NotNull(slot.Level.RmsDbFullScale);
        Assert.Equal(predictedPeak, slot.Level.PeakDbFullScale!.Value, 0.05);
        Assert.Equal(predictedRms, slot.Level.RmsDbFullScale!.Value, 0.05);

        // A sine crosses zero, but a float sine lands exactly on nought only where
        // the phase is exactly nought, so the count is a handful rather than a
        // fifteenth of the slot. What matters is that it is nothing like the whole.
        Assert.NotNull(slot.Level.ZeroSampleFraction);
        Assert.True(
            slot.Level.ZeroSampleFraction!.Value < 0.01,
            Line(
                "  a half-scale sine reported ", slot.Level.ZeroSampleFraction.Value,
                " of its samples exactly zero, which is not a tone"));
    }

    /// <summary>
    /// **A SLOT THAT DECODED AND A SLOT THAT DID NOT, AND BOTH CARRY A LEVEL.**
    /// </summary>
    /// <remarks>
    /// The census entry exists for every slot that ran, which is
    /// <see cref="Ft8Reception"/>'s own rule. The point of this one is that the
    /// level is not a by-product of decoding: the quiet slot found no message and
    /// still says how loud it was, which is exactly the case the record was blind
    /// to on 2026-09-03.
    /// </remarks>
    [Fact]
    public void TwoSlotsFromOneRecordingBothCarryALevelWhicheverDecoded()
    {
        var heard = Ft8Reader.Read(TwoSlots(), EndedAt, Measured);

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Equal(2, heard.Slots.Count);

        foreach (var slot in heard.Slots)
        {
            Print(
                slot.SlotStartUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                slot);
        }

        var quiet = heard.Slots[0];
        var loud = heard.Slots[1];

        Assert.Equal(0, quiet.BecameTextCount);
        Assert.True(
            loud.BecameTextCount > 0,
            $"the slot holding a transmission became {loud.BecameTextCount} messages");

        // Both measured, neither refused: noise is quiet and it is not nought.
        Assert.NotNull(quiet.Level.PeakDbFullScale);
        Assert.NotNull(loud.Level.PeakDbFullScale);
        Assert.NotNull(quiet.Level.RmsDbFullScale);
        Assert.NotNull(loud.Level.RmsDbFullScale);

        Assert.True(
            quiet.Level.RmsDbFullScale!.Value < loud.Level.RmsDbFullScale!.Value,
            Line(
                "  the noise slot read ", quiet.Level.RmsDbFullScale.Value,
                " dBFS rms against the transmission's ",
                loud.Level.RmsDbFullScale.Value, " dBFS"));
    }

    /// <summary>
    /// **REAL AUDIO OFF A REAL ANTENNA, AND IT LOOKS LIKE NEITHER OF THE OTHERS.**
    /// </summary>
    /// <remarks>
    /// <para>**NO MAGIC NUMBER IS ASSERTED AGAINST SOMEBODY'S RECORDING.** What is
    /// asserted is that it is distinguishable from the two cases above: it is not
    /// digital silence, so it has a level at all and is not all zeros; and it is
    /// not a synthesized sine, whose peak stands exactly 3.01 decibels above its
    /// own root mean square. Real audio off a band has a crest factor well above
    /// that, and four decibels is the loose side of loose.</para>
    /// <para>**SKIPPED AND NOT FAILED WITHOUT THE CLONE.** The recordings are never
    /// committed, so a fresh clone has none of them.</para>
    /// </remarks>
    [RequiresOffAirRecordingsFact]
    public void RealOffAirAudioReadsALevelUnlikeSilenceAndUnlikeATone()
    {
        var recording = OffAirRecordings.Busiest(1).SingleOrDefault();

        Assert.NotNull(recording);

        var audio = recording!.Read();

        _output.WriteLine(
            $"  {recording.Name}: {audio.Samples.Length} samples at "
            + $"{audio.SampleRate} Hz");

        var heard = Ft8Reader.Read(audio, OffAirEndedAt, Measured);

        Assert.NotEmpty(heard.Slots);

        foreach (var slot in heard.Slots)
        {
            Print("off air", slot);

            Assert.NotNull(slot.Level.PeakDbFullScale);
            Assert.NotNull(slot.Level.RmsDbFullScale);

            // Not digital silence.
            Assert.NotNull(slot.Level.ZeroSampleFraction);
            Assert.True(
                slot.Level.ZeroSampleFraction!.Value < 1.0,
                "a real recording reported every sample exactly zero");

            // Not a sine, whose crest is 3.01 dB and nothing else.
            var crest =
                slot.Level.PeakDbFullScale!.Value - slot.Level.RmsDbFullScale!.Value;

            _output.WriteLine(Line("    peak stands ", crest, " dB above the rms"));

            Assert.True(
                crest > 4.0,
                Line(
                    "  a real recording's peak stood only ", crest,
                    " dB above its rms, which is a synthesized tone's shape"));
        }
    }

    /// <summary>
    /// Forty-five seconds holding two whole slots: quiet noise, then a message.
    /// </summary>
    /// <remarks>
    /// **THE NOISE IS DETERMINISTIC.** A fixed seed, because a level that changes
    /// run to run is a test that fails on some evenings.
    /// </remarks>
    private static MonoAudio TwoSlots()
    {
        const int Seconds = 45;

        var samples = new float[DeviceRate * Seconds];
        var noise = new Random(236);

        // The recording begins at 14:22:02, so 14:22:15 is thirteen seconds in and
        // 14:22:30 is twenty-eight. The first slot gets noise and nothing else.
        for (var i = 13 * DeviceRate; i < 28 * DeviceRate; i++)
        {
            samples[i] = (float)((noise.NextDouble() - 0.5) * 0.002);
        }

        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), DeviceRate, (float)PlacedAtHz);

        slot.CopyTo(samples.AsSpan(28 * DeviceRate));

        return new MonoAudio(DeviceRate, samples);
    }

    /// <summary>Print one slot's level before anything is asserted about it.</summary>
    /// <param name="what">What the slot is.</param>
    /// <param name="slot">The census line.</param>
    private void Print(string what, Ft8SlotCensus slot)
    {
        var level = slot.Level;

        _output.WriteLine(
            $"  {what}: {level.SampleCount} samples at {slot.SampleRate} Hz, "
            + $"candidates {slot.CandidateCount}, became text {slot.BecameTextCount}");

        _output.WriteLine(
            "    peak "
            + (level.PeakDbFullScale is { } peak
                ? Line(string.Empty, peak, " dBFS")
                : "none - the slot was digital silence")
            + ", rms "
            + (level.RmsDbFullScale is { } rms
                ? Line(string.Empty, rms, " dBFS")
                : "none - the slot was digital silence"));

        _output.WriteLine(
            $"    zero samples {level.ZeroSampleCount} of {level.SampleCount}"
            + (level.ZeroSampleFraction is { } fraction
                ? Line(", which is ", fraction, " of the slot")
                : ", so there is no fraction to take"));
    }

    /// <summary>
    /// A line with its numbers formatted the same way whatever the machine's
    /// locale, so a printed measurement never reads <c>0,5</c> on one machine and
    /// <c>0.5</c> on another.
    /// </summary>
    /// <param name="parts">The pieces, in order.</param>
    /// <returns>Them, joined.</returns>
    private static string Line(params object[] parts)
        => string.Concat(parts.Select(p => p is double d
            ? d.ToString("0.0000", CultureInfo.InvariantCulture)
            : p.ToString()));
}
