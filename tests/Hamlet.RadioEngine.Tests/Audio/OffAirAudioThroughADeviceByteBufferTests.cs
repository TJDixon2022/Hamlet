using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.Tests.Shared;
using NAudio.Wave;
using Xunit;
using Xunit.Abstractions;

using static Hamlet.RadioEngine.Tests.Audio.DeviceBytesBecomeTheFloatsTheTapSeesTests;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Off-air audio delivered the way a sound card delivers it — as raw bytes in a
/// device's own format — all the way to the messages.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE SENTENCE THE UNIT WAS COMMISSIONED FOR** (unit 237, task
/// 4). <see cref="RealOffAirAudioReachesTheTabTests"/> proves Karlis Goba's
/// off-air recordings produce rows through a real <see cref="AudioTap"/> and a
/// real <see cref="Ft8SlotWatch"/> — starting from mono floats that were already
/// made. Here the same recordings are encoded into the byte buffers a capture
/// device hands over, pushed through the production conversion in
/// <c>WasapiAudioSource</c>, and then into the same helper by the same route. The
/// two runs differ in exactly one segment.</para>
/// <para>**IT REUSES THAT FILE'S OWN <c>Play</c> RATHER THAN COPYING IT.** A
/// second harness would put two differences between the two runs and only one of
/// them would be the conversion.</para>
/// <para>**BOTH CHANNELS CARRY THE SAME SAMPLE HERE**, which is what a downmix of
/// a monophonic receiver output is. The case where the two channels differ — the
/// one a stride error could hide in — is asserted from constructed bytes in
/// <see cref="DeviceBytesBecomeTheFloatsTheTapSeesTests"/>, where the expected
/// value is known exactly rather than being whatever the band was doing.</para>
/// <para>**WHAT IT MEASURED, ON THIS MACHINE, ON 2026-09-03.** Three busiest
/// recordings, at 48 000 and at 44 100, gave 47 rows through the float path. An
/// extensible-float device buffer gave **24 of those 47 before unit 237's fix and
/// all 47 after**, at both rates. Every other format gave 47 either way, except
/// eight-bit PCM at 46. The failure was never silence: half the band came through,
/// scrambled.</para>
/// <para>**NOTHING IS COPIED OUT OF THE CLONE AND ABSENCE IS A SKIP**, and
/// nothing here opens a device, starts a stream or reaches a transmitter.</para>
/// </remarks>
public sealed class OffAirAudioThroughADeviceByteBufferTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the per-format counts are printed.</param>
    public OffAirAudioThroughADeviceByteBufferTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// How many of the busiest recordings each format is played through.
    /// </summary>
    /// <remarks>
    /// **THREE, BECAUSE THE COST IS NINE RUNS PER RECORDING PER RATE.** Every
    /// format plays the whole recording through a ring buffer sample by sample and
    /// decodes each slot on the way past, and the float baseline plays again
    /// beside it. The question here is whether a format changes the answer, which
    /// three busy recordings answer as well as twelve;
    /// <see cref="RealOffAirAudioReachesTheTabTests"/> keeps the wider run.
    /// </remarks>
    private const int Played = 3;

    /// <summary>Every format a capture device can present, stereo.</summary>
    /// <remarks>
    /// **THE ORDER IS THE TABLE'S ORDER AND THE LAST ROWS ARE THE POINT.**
    /// <c>extensible-float32</c> is what Windows shared-mode capture actually
    /// presents, and before unit 237 it was read through the 32-bit integer arm.
    /// </remarks>
    private static readonly string[] Formats =
    [
        "pcm8",
        "pcm16",
        "pcm24",
        "pcm32",
        "float32",
        "extensible-float32",
        "extensible-pcm16",
        "extensible-pcm32",
    ];

    /// <summary>
    /// Formats that cannot be expected to give the float path's answer exactly.
    /// </summary>
    /// <remarks>
    /// **MEASURED, NOT ASSUMED, AND THE REPORT SAYS WHICH.** Eight bits is a step
    /// of 1/128 of full scale, which is above the noise floor of a quiet band, so
    /// a decode that depends on a weak signal can be lost in the quantisation. It
    /// is listed here because the run said so — 46 rows against the float path's
    /// 47, and none of the three recordings gave exactly the same set — and it is
    /// still played and still counted, because its number is a real fact about a
    /// device that speaks it.
    /// </remarks>
    private static readonly string[] Lossy = ["pcm8"];

    /// <summary>
    /// **THE UNIT'S NUMBER, AT 48 000** — the rate a USB codec delivers.
    /// </summary>
    [RequiresOffAirRecordingsFact]
    public void EveryDeviceFormatDecodesWhatTheFloatPathDecodesAt48000()
        => PlayThroughEveryFormat(48000);

    /// <summary>**AND AT 44 100**, where the resampler's phase is fractional.</summary>
    [RequiresOffAirRecordingsFact]
    public void EveryDeviceFormatDecodesWhatTheFloatPathDecodesAt44100()
        => PlayThroughEveryFormat(44100);

    /// <summary>Play the busiest recordings through every device format.</summary>
    /// <param name="deviceRate">What the sound card is pretending to be.</param>
    private void PlayThroughEveryFormat(int deviceRate)
    {
        var recordings = OffAirRecordings.Busiest(Played);

        Assert.NotEmpty(recordings);

        var chunk = RealOffAirAudioReachesTheTabTests.ChunkFor(deviceRate);

        _output.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "  device rate {0} Hz, delivered in {1}-sample chunks, stereo, "
                + "through WasapiAudioSource's own conversion and then a real "
                + "AudioTap and a real Ft8SlotWatch",
                deviceRate,
                chunk));
        _output.WriteLine("");

        // **THE FLOAT PATH FIRST, AND IT IS THE SAME HELPER.** This is the number
        // every previous unit has quoted: the recording, already converted.
        var baseline = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var floatTotal = 0;

        foreach (var recording in recordings)
        {
            var run = RealOffAirAudioReachesTheTabTests.Play(recording, deviceRate, chunk);

            baseline[recording.Name] = run.Decodes.Select(d => d.Message).ToArray();
            floatTotal += run.Decodes.Count;

            Assert.Equal(string.Empty, run.LastRefusal);
            Assert.Equal(0, run.Skipped);
        }

        _output.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "    {0,-22} {1,4} rows over {2} recordings   (mono floats, already made)",
                "float path",
                floatTotal,
                recordings.Count));

        var wrong = new List<string>();

        foreach (var name in Formats)
        {
            var row = Row(name, 2);
            var total = 0;
            var matched = 0;

            foreach (var recording in recordings)
            {
                var device = new DeviceBuffer(row.Layout, row.Format);

                var run = RealOffAirAudioReachesTheTabTests.Play(
                    recording, deviceRate, chunk, device.Arrive);

                var heard = run.Decodes.Select(d => d.Message).ToArray();

                total += heard.Length;

                if (heard.SequenceEqual(baseline[recording.Name], StringComparer.Ordinal))
                {
                    matched++;
                }

                Assert.Equal(string.Empty, run.LastRefusal);
                Assert.Equal(0, run.Skipped);
            }

            var exact = matched == recordings.Count;

            _output.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "    {0,-22} {1,4} rows over {2} recordings   "
                    + "{3} of {2} identical to the float path   ({4} {5}-bit)",
                    name,
                    total,
                    recordings.Count,
                    matched,
                    row.Format.Encoding,
                    row.Format.BitsPerSample));

            if (!exact && !Lossy.Contains(name, StringComparer.Ordinal))
            {
                wrong.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}: {1} rows against the float path's {2}, and {3} of {4} "
                        + "recordings gave a different set of messages",
                        name,
                        total,
                        floatTotal,
                        recordings.Count - matched,
                        recordings.Count));
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  This is not a radio. No device was opened, no stream was started and");
        _output.WriteLine(
            "  nothing was recorded. What is real here is the byte layout and the");
        _output.WriteLine("  conversion, and the band these recordings came off.");

        Assert.True(
            wrong.Count == 0,
            "a device byte format changed what the decoder heard: "
            + string.Join("; ", wrong));

        // **THE ONE THAT MATTERS.** Audio recorded off a real antenna produced
        // rows after arriving as the raw bytes a capture device delivers.
        Assert.True(
            floatTotal > 0,
            $"the float path itself produced nothing at {deviceRate} Hz");
    }

    /// <summary>
    /// One capture device's buffer, reused, with the production conversion behind
    /// it.
    /// </summary>
    /// <remarks>
    /// **THE ENCODER IS THIS FILE'S AND THE DECODER IS HAMLET'S.** Nothing about
    /// laying the bytes out is shared with the code being measured.
    /// </remarks>
    private sealed class DeviceBuffer
    {
        private readonly SampleLayout _layout;
        private readonly WaveFormat _format;

        private byte[] _bytes = [];
        private float[] _mono = [];

        /// <summary>Creates the buffer.</summary>
        /// <param name="layout">What the bytes are.</param>
        /// <param name="format">What the device says they are.</param>
        public DeviceBuffer(SampleLayout layout, WaveFormat format)
        {
            _layout = layout;
            _format = format;
        }

        /// <summary>Send a chunk out as device bytes and read it back.</summary>
        /// <param name="block">The samples on the wire.</param>
        /// <returns>What the conversion made of them.</returns>
        public ReadOnlySpan<float> Arrive(ReadOnlySpan<float> block)
        {
            var channels = _format.Channels;
            var bytesPerSample = _format.BitsPerSample / 8;
            var needed = block.Length * channels * bytesPerSample;

            if (_bytes.Length < needed)
            {
                _bytes = new byte[needed];
            }

            for (var frame = 0; frame < block.Length; frame++)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    Write(
                        _bytes,
                        ((frame * channels) + channel) * bytesPerSample,
                        block[frame],
                        _layout);
                }
            }

            var frames = WasapiAudioSource.Downmix(_bytes, needed, _format, ref _mono);

            return _mono.AsSpan(0, frames);
        }
    }
}
