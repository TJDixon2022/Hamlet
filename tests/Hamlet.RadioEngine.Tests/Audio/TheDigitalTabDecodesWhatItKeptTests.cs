using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Audio kept by the capture press is cut on the quarter minute, put on the FT8
/// grid, and read.
/// </summary>
/// <remarks>
/// <para>**UNIT 224, AND THE FIRST TEST IN THIS PROJECT THAT TAKES TEXT OUT OF
/// AUDIO.** Everything up to here was built inside `Ft8Sharp` and measured there;
/// this asserts the join — that what Hamlet's own tap holds, at the rate Hamlet's
/// own sound card delivers it, comes back as the message that went in.</para>
/// <para>**THE RATE IS THE POINT.** `WasapiAudioSource` passes the device's rate
/// straight through and the training radio runs at 8000 Hz, while every
/// measurement this phase has taken was at 12 000. The theory below runs the same
/// message through four rates including one that is not a whole ratio of the
/// target, because a resampler that only works on powers of two is a resampler
/// that fails on somebody's sound card and not in this test.</para>
/// <para>**NO CLOCK IS READ.** The moment the recording ended is handed in, so
/// the same buffer decodes to the same text at any hour of any day (§5).</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (§0.2). The synthesizer is a test
/// oracle and its samples go into an array.</para>
/// </remarks>
public sealed class TheDigitalTabDecodesWhatItKeptTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the decodes are printed.</param>
    public TheDigitalTabDecodesWhatItKeptTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What the tap keeps, which is <see cref="AudioTap.SecondsKept"/>.</summary>
    private const int RecordingSeconds = 30;

    /// <summary>Where the transmission is put, in the passband.</summary>
    private const double PlacedAtHz = 1240;

    /// <summary>
    /// The moment the recording ended, by a clock that has been measured at no
    /// drift. Thirty seconds back from here is 14:22:17, so exactly one whole
    /// slot fits: the one opening at 14:22:30.
    /// </summary>
    private static readonly DateTime EndedAt =
        new(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** A message goes into a stretch of captured
    /// audio at a sound card's own rate, and comes back out as itself.
    /// </summary>
    /// <param name="rate">
    /// What the device delivers. 8000 is the training radio, 12000 is what the
    /// library was measured at, 44100 and 48000 are what sound cards do — and
    /// 44100 is deliberately not a whole ratio of 12000.
    /// </param>
    [Theory]
    [InlineData(8000)]
    [InlineData(12000)]
    [InlineData(44100)]
    [InlineData(48000)]
    public void AMessageInTheKeptAudioComesBackAsItself(int rate)
    {
        var audio = Recording(rate, out var placedAtSample);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        _output.WriteLine(
            $"  {rate} Hz, {audio.Samples.Length} samples, "
            + $"transmission written at sample {placedAtSample}");
        _output.WriteLine(
            $"  slots {heard.SlotsDecoded}, candidates {heard.CandidatesFound}, "
            + $"refusal [{heard.Refusal}]");

        foreach (var decode in heard.Decodes)
        {
            _output.WriteLine(
                $"    {decode.SlotStartUtc:HHmmss}  dt {decode.OffsetSeconds:0.00}  "
                + $"{decode.FrequencyHz:0} Hz  sync {decode.SyncScore}  {decode.Message}");
        }

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Equal(1, heard.SlotsDecoded);

        var found = Assert.Single(heard.Decodes);

        Assert.Equal("CQ K1ABC FN42", found.Message);

        // The slot it belongs to, not the moment the button was pressed.
        Assert.Equal(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc), found.SlotStartUtc);

        // Placed at 1240 Hz; the search reports in bins a fraction of a tone wide.
        Assert.InRange(found.FrequencyHz, PlacedAtHz - 4, PlacedAtHz + 4);

        // `SynthesizeSlot` pads the front by an eighth of the slot, near enough.
        Assert.InRange(found.OffsetSeconds, 0.9, 1.5);
    }

    /// <summary>
    /// **A CLOCK NOBODY HAS CHECKED DECODES NOTHING AND SAYS SO**, which is the
    /// commonest newcomer failure in this mode and the one that looks exactly like
    /// a dead band.
    /// </summary>
    [Fact]
    public void AnUnmeasuredClockRefusesInWordsRatherThanShowingAnEmptyTable()
    {
        var audio = Recording(48000, out _);

        var heard = Ft8Reader.Read(audio, EndedAt, ClockOffset.Unknown);

        _output.WriteLine($"  refusal: {heard.Refusal}");

        Assert.Empty(heard.Decodes);
        Assert.Equal(0, heard.SlotsDecoded);
        Assert.Equal(Ft8SlotCutter.NoOffset, heard.Refusal);
    }

    /// <summary>
    /// **AUDIO WITH NOBODY ON IT IS A DIFFERENT ANSWER FROM AUDIO NOBODY COULD
    /// CUT**, and the reader distinguishes them.
    /// </summary>
    [Fact]
    public void SilenceDecodesNothingAndDoesNotRefuse()
    {
        var audio = new MonoAudio(48000, new float[48000 * RecordingSeconds]);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        _output.WriteLine(
            $"  slots {heard.SlotsDecoded}, candidates {heard.CandidatesFound}");

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Equal(1, heard.SlotsDecoded);
        Assert.Empty(heard.Decodes);
    }

    /// <summary>
    /// **THE RESAMPLER KEEPS THE SIGNAL AND LOSES THE FOLD.** A tone above the
    /// target's Nyquist has to disappear rather than reappear inside the passband,
    /// which is the failure that would make the decoder deaf in hiss.
    /// </summary>
    [Fact]
    public void AToneAboveTheNewNyquistIsRemovedRatherThanFolded()
    {
        const int From = 48000;
        const int Seconds = 2;

        var wanted = Tone(From, Seconds, 1500);
        var folding = Tone(From, Seconds, 10500);

        var keptRms = Rms(Ft8Resample.Resample(wanted, From, 12000));
        var foldedRms = Rms(Ft8Resample.Resample(folding, From, 12000));

        _output.WriteLine($"  1500 Hz in, {Rms(wanted):0.0000} -> {keptRms:0.0000}");
        _output.WriteLine($"  10500 Hz in, {Rms(folding):0.0000} -> {foldedRms:0.0000}");

        // 10500 Hz would alias to 1500 Hz under naive decimation. It must not.
        Assert.InRange(keptRms, 0.68, 0.72);
        Assert.True(
            foldedRms < keptRms / 100,
            $"a 10500 Hz tone came through at {foldedRms:0.000000} against the "
                + $"wanted tone's {keptRms:0.000000}, so it folded into the passband");
    }

    /// <summary>Thirty seconds of audio with one transmission in the whole slot.</summary>
    /// <param name="rate">The rate to build it at.</param>
    /// <param name="placedAtSample">Where the slot's first sample was written.</param>
    private static MonoAudio Recording(int rate, out int placedAtSample)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), rate, (float)PlacedAtHz);

        var samples = new float[rate * RecordingSeconds];

        // 14:22:30 is thirteen seconds after the recording began.
        placedAtSample = 13 * rate;

        slot.CopyTo(samples.AsSpan(placedAtSample));

        return new MonoAudio(rate, samples);
    }

    private static float[] Tone(int rate, int seconds, double hz)
    {
        var samples = new float[rate * seconds];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * hz * i / rate);
        }

        return samples;
    }

    private static double Rms(ReadOnlySpan<float> samples)
    {
        double sum = 0;

        foreach (var sample in samples)
        {
            sum += sample * (double)sample;
        }

        return samples.Length == 0 ? 0 : Math.Sqrt(sum / samples.Length);
    }
}
