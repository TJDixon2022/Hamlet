using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// A WAV on disk goes through the same reader the tab uses, and comes back with a
/// census that names how far every candidate got — with no radio in the room.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE INSTRUMENT THE NIGHT OF 2026-09-03 DID NOT HAVE.** The owner
/// pressed the capture button at 14.074, got an empty table, and the only artefacts
/// were a screenshot and a spectrogram analysis. From here, any WAV can be handed
/// to the same path and will say which stage refused.</para>
/// <para>**NO RECORDING OF ANY KIND IS COMMITTED.** The audio is built by
/// <see cref="Ft8Waveform"/>, written to a temporary file and deleted. The
/// operator's own captures are off-air recordings of real stations and never enter
/// a repository headed for publication — the ruling that keeps `ft8_lib`'s WAVs out
/// of this tree, extended to his by unit 233.</para>
/// <para>**AND IT IS DRIVEN AT A DEVICE RATE RATHER THAN 12 kHz**, so the resampler
/// is inside the path being asserted. A sound card delivers 44 100 or 48 000, and a
/// census taken on a rate no sound card produces would say nothing about the
/// morning it is meant to explain.</para>
/// <para>**THERE IS NO FIXTURE HERE EXPECTING ZERO DECODES.** A test whose expected
/// outcome is *nothing decoded* ratchets a fault into green and fails every future
/// repair. What is asserted is that the census is *reported*, populated and
/// consistent — not what any particular recording contains.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2). Audio into an
/// array, records out.</para>
/// </remarks>
public sealed class ACapturedFileDiagnosesItselfTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit233", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the census is printed.</param>
    public ACapturedFileDiagnosesItselfTests(ITestOutputHelper output)
        => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    private const int RecordingSeconds = 30;

    private const double PlacedAtHz = 1240;

    /// <summary>
    /// Thirty seconds back from here is 14:22:17, so exactly one whole slot fits:
    /// the one opening at 14:22:30.
    /// </summary>
    private static readonly DateTime EndedAt =
        new(2026, 9, 3, 14, 22, 47, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 3, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** A file is read off disk, run through the
    /// reader at the rate it declares, and every field of the census comes back
    /// populated.
    /// </summary>
    /// <param name="rate">
    /// What a sound card delivers. 44 100 is deliberately not a whole ratio of
    /// 12 000, so the resampler is doing real work in both cases.
    /// </param>
    [Theory]
    [InlineData(44100)]
    [InlineData(48000)]
    public void AFileOnDiskComesBackWithACensusThatNamesEveryStage(int rate)
    {
        var path = WriteRecording(rate, withTransmission: true);

        var audio = WavAudio.Read(path);

        Assert.Equal(rate, audio.SampleRate);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        Report(heard);

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Equal(1, heard.SlotsDecoded);

        // One census entry per slot that was cut and run — not one per message,
        // and not one only when something decoded.
        var slot = Assert.Single(heard.Slots);

        Assert.Equal(
            new DateTime(2026, 9, 3, 14, 22, 30, DateTimeKind.Utc), slot.SlotStartUtc);

        // **THE RATE THE FILE DECLARED, NOT THE RATE THE LIBRARY WORKS AT.** A
        // census that always said 12 000 would describe the resampler rather than
        // the sound card, and the sound card is what a silent morning is about.
        Assert.Equal(rate, slot.SampleRate);

        // The stages narrow, in order, and none of them may exceed the one before.
        Assert.True(slot.CandidateCount > 0);
        Assert.InRange(slot.ParitySatisfiedCount, 1, slot.CandidateCount);
        Assert.InRange(slot.ChecksumPassedCount, 1, slot.ParitySatisfiedCount);
        Assert.InRange(slot.BecameTextCount, 1, slot.ChecksumPassedCount);
        Assert.InRange(slot.DuplicateCount, 0, slot.BecameTextCount);

        // At most three, strongest first, and every one at or above the search's
        // own minimum. They are Costas match counts and not decibels of anything.
        Assert.NotEmpty(slot.TopSyncScores);
        Assert.InRange(slot.TopSyncScores.Count, 1, 3);
        Assert.Equal(
            slot.TopSyncScores.OrderByDescending(s => s).ToArray(),
            slot.TopSyncScores.ToArray());

        // The reading says which clock it believed, so a wrong clock and a deaf
        // receiver can be told apart afterwards.
        Assert.Equal(Measured, heard.Offset);
    }

    /// <summary>
    /// **A SLOT THAT FOUND NOTHING STILL GETS A CENSUS ENTRY**, all zeroes, because
    /// a band with nobody on it and a decoder that was never run are different
    /// facts and only the second is a defect.
    /// </summary>
    [Fact]
    public void ASlotWithNothingInItIsCountedRatherThanOmitted()
    {
        var path = WriteRecording(48000, withTransmission: false);

        var heard = Ft8Reader.Read(WavAudio.Read(path), EndedAt, Measured);

        Report(heard);

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Equal(1, heard.SlotsDecoded);

        var slot = Assert.Single(heard.Slots);

        Assert.Equal(0, slot.CandidateCount);
        Assert.Equal(0, slot.ParitySatisfiedCount);
        Assert.Equal(0, slot.ChecksumPassedCount);
        Assert.Equal(0, slot.BecameTextCount);
        Assert.Empty(slot.TopSyncScores);
        Assert.Equal(48000, slot.SampleRate);
    }

    /// <summary>
    /// **THE REFUSAL SENTENCE IS PRESENT EXACTLY WHEN NO WHOLE SLOT WAS CUT**, and
    /// there is no census at all in that case — a slot that never ran must not
    /// appear as a slot that ran and found nothing.
    /// </summary>
    [Fact]
    public void AFileHoldingNoWholeSlotRefusesInWordsAndReportsNoCensus()
    {
        var path = Path.Combine(_folder, "too-short.wav");

        WavAudio.Write(path, new MonoAudio(48000, new float[48000 * 4]));

        var heard = Ft8Reader.Read(WavAudio.Read(path), EndedAt, Measured);

        Report(heard);

        Assert.NotEqual(string.Empty, heard.Refusal);
        Assert.Equal(Ft8SlotCutter.TooShort, heard.Refusal);
        Assert.Equal(0, heard.SlotsDecoded);
        Assert.Empty(heard.Slots);
    }

    /// <summary>
    /// A clock nobody has measured refuses in the cutter's own words and reports no
    /// census, which is the commonest newcomer failure in this mode.
    /// </summary>
    [Fact]
    public void AnUnmeasuredClockRefusesAndReportsNoCensus()
    {
        var path = WriteRecording(48000, withTransmission: true);

        var heard = Ft8Reader.Read(
            WavAudio.Read(path), EndedAt, ClockOffset.Unknown);

        Report(heard);

        Assert.Equal(Ft8SlotCutter.NoOffset, heard.Refusal);
        Assert.Empty(heard.Slots);
        Assert.Equal(ClockOffset.Unknown, heard.Offset);
    }

    /// <summary>
    /// The count the join has always published still means what it meant: the sum
    /// of the census's candidates.
    /// </summary>
    [Fact]
    public void TheOldCandidateCountIsStillTheSumOfTheNewOne()
    {
        var path = WriteRecording(48000, withTransmission: true);

        var heard = Ft8Reader.Read(WavAudio.Read(path), EndedAt, Measured);

        Report(heard);

        Assert.Equal(
            heard.Slots.Sum(s => s.CandidateCount), heard.CandidatesFound);
    }

    private void Report(Ft8Reception heard)
    {
        _output.WriteLine(
            $"  slots {heard.SlotsDecoded}  candidates {heard.CandidatesFound}  "
            + $"refusal [{heard.Refusal}]");

        foreach (var slot in heard.Slots)
        {
            _output.WriteLine(
                $"    {slot.SlotStartUtc:HH:mm:ss}  candidates {slot.CandidateCount}"
                + $"  parity {slot.ParitySatisfiedCount}"
                + $"  checksum {slot.ChecksumPassedCount}"
                + $"  text {slot.BecameTextCount}"
                + $"  duplicate {slot.DuplicateCount}"
                + $"  at {slot.SampleRate} Hz"
                + $"  top Costas match counts [{string.Join(", ", slot.TopSyncScores)}]");
        }
    }

    /// <summary>Thirty seconds of audio, written to a WAV and read back off disk.</summary>
    private string WriteRecording(int rate, bool withTransmission)
    {
        var samples = new float[rate * RecordingSeconds];

        if (withTransmission)
        {
            var message = new byte[Ft8StandardMessage.MessageBytes];

            Assert.Equal(
                Ft8PackResult.Ok,
                Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

            Ft8Waveform
                .SynthesizeSlot(
                    Ft8SymbolEncoder.Encode(message), rate, (float)PlacedAtHz)
                .CopyTo(samples.AsSpan(13 * rate));
        }

        var path = Path.Combine(_folder, $"ft8-synthetic-{rate}.wav");

        WavAudio.Write(path, new MonoAudio(rate, samples));

        return path;
    }
}
