using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Decoding stands down while the radio is transmitting.
/// </summary>
/// <remarks>
/// <para>**THE PUREST FORM OF A CONFIDENT WRONG ANSWER** (HM-DEC-009). The
/// operator keyed the radio by hand and the terminal filled with fragments of his
/// own transmission, with nothing on screen to say they were his. On full
/// break-in the receiver opens between elements, so the sidetone arrives chopped
/// by transmit-receive switching and decodes as a page of isolated letters, which
/// looks exactly like a weak station being read. **The decoder was behaving
/// correctly on input it should never have been given.**</para>
/// <para>**AND THE SUBTLER HALF IS WORSE.** With CW transmit built, Hamlet would
/// decode its own sent text back and present it as received, and an operator
/// could read his own callsign returning and believe somebody answered.</para>
/// <para>**THE STATE COMES FROM THE RADIO** (HM-DEC-091). CI-V `1C 00`, which
/// Hamlet has read for months and nothing consumed. Never from the audio: not the
/// level, not the sidetone's pitch, not a change in the noise floor, because
/// every one of those is a guess about the transmitter made from the thing the
/// transmitter is drowning out.</para>
/// <para>**EVERY TEST HERE DRIVES THE STATE DIRECTLY** (HM-DEC-093). There is no
/// radio on the development machine and transmit cannot be exercised live.</para>
/// </remarks>
public sealed class HamletDoesNotDecodeYourOwnSendingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public HamletDoesNotDecodeYourOwnSendingTests(ITestOutputHelper output)
        => _output = output;

    private static MonoAudio Bulletin() => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-18-004507.wav"));

    /// <summary>One run over a recording, with transmit asserted for part of it.</summary>
    /// <param name="assertFrom">When the radio starts transmitting, in seconds.</param>
    /// <param name="assertTo">When it stops, in seconds.</param>
    /// <returns>What settled, and what the decoder's state was at the end.</returns>
    private (List<CwCharacter> Settled, CwDecoder Decoder) Run(
        double assertFrom, double assertTo)
    {
        var audio = Bulletin();
        var decoder = new CwDecoder(audio.SampleRate, 501);
        var hop = decoder.Tracker.HopSamples;
        var settled = new List<CwCharacter>();
        var clock = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            var seconds = at / (double)audio.SampleRate;

            // The radio is asked four times a second in the app; here it is told
            // every hop, which only makes the state fresher than it can be live.
            decoder.RadioIsTransmitting(
                seconds >= assertFrom && seconds < assertTo,
                clock + TimeSpan.FromSeconds(seconds));

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return (settled, decoder);
    }

    /// <remarks>
    /// <para>Proves the first of the four: **decoding is suspended while transmit
    /// is asserted.** Nothing at all reaches the transcript from a recording read
    /// with the transmitter keyed throughout.</para>
    /// </remarks>
    [Fact]
    public void NothingIsDecodedWhileTheRadioIsTransmitting()
    {
        var (settled, decoder) = Run(0, 999);

        _output.WriteLine(
            $"{settled.Count} characters, {decoder.SuspendedChunks} chunks dropped");

        Assert.Empty(settled);
        Assert.True(decoder.DecodingSuspended);
        Assert.True(decoder.SuspendedChunks > 0);
    }

    /// <remarks>
    /// <para>Proves the second: **decoding resumes when transmit drops**, and the
    /// same recording read with a transmission at the front comes back with the
    /// bulletin in it.</para>
    /// </remarks>
    [Fact]
    public void DecodingResumesWhenTheTransmitterDrops()
    {
        var (settled, decoder) = Run(0, 4);
        var text = string.Concat(settled.Select(c => c.Text));

        _output.WriteLine($"{settled.Count} characters after four seconds keyed");
        _output.WriteLine($"'{text}'");

        Assert.False(decoder.DecodingSuspended);
        Assert.NotEmpty(settled);

        // The bulletin's own words, out of the part that was not suspended.
        Assert.Contains(
            "STATIONHANDLING",
            text.Replace(" ", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the third: **full break-in cycling does not cost the decoder
    /// what it knows.** The transmitter is keyed and dropped repeatedly through
    /// the middle of the recording, as break-in does between elements, and what
    /// comes back afterwards is still the station.</para>
    /// <para>The new decoder holds no fitted speed and no tracked noise floor to
    /// lose: the speed is an outer hypothesis re-searched on every read, and the
    /// noise scale is taken from the window's own percentiles. **What it does
    /// hold is the window**, and suspension drops the audio rather than feeding
    /// the decoder silence, so the evidence either side of a transmission is
    /// still there when it ends.</para>
    /// </remarks>
    [Fact]
    public void BreakInCyclingDoesNotCostTheStation()
    {
        var audio = Bulletin();
        var decoder = new CwDecoder(audio.SampleRate, 501);
        var hop = decoder.Tracker.HopSamples;
        var settled = new List<CwCharacter>();
        var clock = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            var seconds = at / (double)audio.SampleRate;

            // Keyed for sixty milliseconds in every two hundred, from four
            // seconds to eight, which is the shape of a hand sending on full
            // break-in.
            var keying = seconds is >= 4 and < 8
                && seconds % 0.2 < 0.06;

            decoder.RadioIsTransmitting(
                keying, clock + TimeSpan.FromSeconds(seconds));

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var text = string.Concat(settled.Select(c => c.Text));

        _output.WriteLine($"{settled.Count} characters through the cycling");
        _output.WriteLine($"'{text}'");

        Assert.False(decoder.DecodingSuspended);

        // The tracker never moved off the station, and the decoder found the
        // sender's speed on its own afterwards exactly as it does without any of
        // this.
        Assert.InRange(decoder.Tracker.ToneHz, 480, 525);

        Assert.Contains(
            "STATIONHANDLING",
            text.Replace(" ", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the fourth: **no text decoded during transmit reaches
    /// anybody.** Not held and released afterwards, which would be the same
    /// misattribution with a delay: the audio is dropped before the decoder sees
    /// it, so there is nothing to release.</para>
    /// <para>The recording is read twice, once with the transmitter keyed over
    /// the whole of the first half and once not, and every character that comes
    /// back from the keyed run is from the second half.</para>
    /// </remarks>
    [Fact]
    public void NothingHeardWhileSendingIsReleasedAfterwards()
    {
        var keyedThroughout = Run(0, 15).Settled;
        var never = Run(999, 999).Settled;

        var during = keyedThroughout
            .Where(c => c.At < TimeSpan.FromSeconds(15))
            .ToList();

        _output.WriteLine(
            $"keyed for the first fifteen seconds: {keyedThroughout.Count} characters, "
            + $"{during.Count} of them stamped inside it");

        _output.WriteLine($"not keyed at all: {never.Count} characters");

        // **NOTHING FROM THE SUSPENDED STRETCH, AT ANY POINT.** The decoder's own
        // clock only advances on audio it was given, so a character stamped
        // inside the suspension could only have come from audio that was dropped.
        Assert.Empty(during);

        // And the run that was never suspended read more, which is what says the
        // first run actually lost the first half rather than the recording being
        // empty there.
        Assert.True(
            never.Count > keyedThroughout.Count,
            $"the unsuspended run read {never.Count} against {keyedThroughout.Count}");
    }

    /// <remarks>
    /// <para>Proves the hold-off is real and asymmetric: suspension is immediate
    /// and resumption waits <see cref="CwDecoder.ResumeAfter"/>, so a single late
    /// or dropped poll cannot resume decoding into the tail of a transmission.
    /// </para>
    /// </remarks>
    [Fact]
    public void SuspensionIsImmediateAndResumptionWaits()
    {
        var decoder = new CwDecoder(8_000, 600);
        var clock = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

        Assert.False(decoder.DecodingSuspended);

        decoder.RadioIsTransmitting(true, clock);
        Assert.True(decoder.DecodingSuspended);

        decoder.RadioIsTransmitting(false, clock + TimeSpan.FromMilliseconds(100));
        Assert.True(decoder.DecodingSuspended);

        decoder.RadioIsTransmitting(false, clock + TimeSpan.FromMilliseconds(500));
        Assert.True(decoder.DecodingSuspended);

        decoder.RadioIsTransmitting(false, clock + TimeSpan.FromMilliseconds(700));
        Assert.False(decoder.DecodingSuspended);
    }

    /// <remarks>
    /// <para>Proves §0.0: **a link that has gone quiet does not silence the
    /// band.** An unknown transmit state leaves decoding running, because a
    /// terminal that stops for a reason nobody can see reads as an empty band,
    /// and the cost of being wrong the other way is text the operator can
    /// recognise as his own.</para>
    /// </remarks>
    [Fact]
    public void NotKnowingIsNotTransmitting()
    {
        var decoder = new CwDecoder(8_000, 600);
        var clock = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

        decoder.RadioIsTransmitting(null, clock);

        Assert.False(decoder.DecodingSuspended);

        // And going from known-transmitting to unknown runs the hold-off from
        // that moment rather than resuming into the tail of a transmission.
        decoder.RadioIsTransmitting(true, clock);
        decoder.RadioIsTransmitting(null, clock + TimeSpan.FromSeconds(5));

        Assert.True(decoder.DecodingSuspended);

        decoder.RadioIsTransmitting(null, clock + TimeSpan.FromSeconds(6));

        Assert.False(decoder.DecodingSuspended);
    }
}
