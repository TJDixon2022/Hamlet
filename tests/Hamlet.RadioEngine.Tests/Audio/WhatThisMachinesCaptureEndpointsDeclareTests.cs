using Hamlet.RadioEngine.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// What the capture devices on the machine running this actually declare, and
/// whether the conversion can read it.
/// </summary>
/// <remarks>
/// <para>**CORROBORATION, NOT THE EVIDENCE** (unit 237, task 5). The verdict on
/// the conversion is settled from constructed bytes in
/// <see cref="DeviceBytesBecomeTheFloatsTheTapSeesTests"/> and needs no hardware
/// at all. This asks a different and smaller question: does a real endpoint on
/// this machine present the format that was wrong.</para>
/// <para>**IT READS AND STARTS NOTHING** (`CLAUDE.md` §0.2, §6). It enumerates
/// active capture endpoints and reads the format each one declares. No stream is
/// initialised, no recording begins, no audio is captured, and nothing is
/// written anywhere. This is the same enumeration
/// <see cref="WasapiAudioDevices"/> already performs every time the app opens the
/// settings page.</para>
/// <para>**AND IT NAMES NO DEVICE** (HM-DEC-018). Endpoints are numbered in the
/// order the enumerator gives them. A format is not a name; a friendly name is
/// exactly the kind of string that could carry a callsign, and nothing here
/// reads one.</para>
/// <para>**IT ASSERTS A CONTRACT AND NOT A MACHINE.** A test that asserted *this
/// machine has a float device* would be red on every other machine and would say
/// nothing about the code. What is asserted is the thing HM-DEC-009 cares about:
/// no format any endpoint here declares can make the conversion answer with
/// invented silence. Either it converts, or it refuses out loud.</para>
/// </remarks>
public sealed class WhatThisMachinesCaptureEndpointsDeclareTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the formats are printed.</param>
    public WhatThisMachinesCaptureEndpointsDeclareTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every active capture endpoint's declared format, and what the conversion
    /// does with it.
    /// </summary>
    [Fact]
    public void NoFormatAnEndpointDeclaresCanProduceInventedSilence()
    {
        var formats = Declared();

        _output.WriteLine($"  {formats.Count} active capture endpoint(s) on this machine");
        _output.WriteLine("");

        if (formats.Count == 0)
        {
            // **A MACHINE WITH NO SOUND CARD IS A NORMAL MACHINE** (§8), and a
            // suite that went red on one would be a suite nobody could run.
            _output.WriteLine("  nothing to read - no assertion is available here");
            return;
        }

        for (var at = 0; at < formats.Count; at++)
        {
            var format = formats[at];

            if (format is null)
            {
                _output.WriteLine(
                    $"    endpoint {at + 1}  not determined - the endpoint would "
                    + "not report its format");
                continue;
            }

            var subFormat = format is WaveFormatExtensible extensible
                ? extensible.SubFormat.ToString()
                : "none - the tag is the whole answer";

            _output.WriteLine(
                $"    endpoint {at + 1}  {format.Encoding} {format.BitsPerSample}-bit, "
                + $"{format.Channels} channel(s), {format.SampleRate} Hz");
            _output.WriteLine($"                subformat {subFormat}");
            _output.WriteLine($"                {WhatBecomesOfIt(format)}");
        }
    }

    /// <summary>
    /// Push bytes that are certainly not silence through the production
    /// conversion, and say what came back.
    /// </summary>
    /// <param name="format">What the endpoint declared.</param>
    /// <returns>A line for the record.</returns>
    /// <remarks>
    /// **THE BYTES ARE 0x7F AND NOTHING ELSE IS ASSUMED.** What those bytes mean
    /// depends on the layout, which is the thing under test, so nothing here
    /// claims to know what value should come out. What it does claim is that a
    /// buffer with no zero byte in it must not become a stream of zeros, because
    /// that is the one answer nobody downstream can tell from a dead band.
    /// </remarks>
    private static string WhatBecomesOfIt(WaveFormat format)
    {
        var frames = 16;
        var buffer = new byte[frames * Math.Max(1, format.Channels)
            * Math.Max(1, format.BitsPerSample / 8)];

        Array.Fill(buffer, (byte)0x7F);

        var mono = Array.Empty<float>();

        try
        {
            var made = WasapiAudioSource.Downmix(
                buffer, buffer.Length, format, ref mono);

            Assert.True(
                made > 0,
                $"a {format.Encoding} {format.BitsPerSample}-bit buffer produced no "
                + "frames at all, which is neither a conversion nor a refusal");

            var quiet = true;

            for (var at = 0; at < made; at++)
            {
                if (mono[at] != 0f)
                {
                    quiet = false;
                    break;
                }
            }

            Assert.False(
                quiet,
                $"a {format.Encoding} {format.BitsPerSample}-bit buffer with no zero "
                + "byte in it converted to complete silence. That is invented "
                + "silence and HM-DEC-009 forbids it: a refusal is honest and a "
                + "plausible value is not.");

            return $"reads it - {made} frames, first sample {mono[0]}";
        }
        catch (NotSupportedException refusal)
        {
            // **A REFUSAL IS THE OTHER CORRECT ANSWER**, and it is the one the
            // operator can act on, because nothing reaches the tap and the slot
            // level reads as no level at all rather than as a quiet band.
            return $"refuses it - {refusal.Message}";
        }
    }

    /// <summary>The format each active capture endpoint declares.</summary>
    /// <returns>One entry per endpoint, null where it would not say.</returns>
    /// <remarks>
    /// **NEVER THROWS** (§8). A machine with no WASAPI, a driver mid-install or an
    /// endpoint that has gone away all produce a shorter list rather than a red.
    /// </remarks>
    private static IReadOnlyList<WaveFormat?> Declared()
    {
        var formats = new List<WaveFormat?>();

        try
        {
            using var enumerator = new MMDeviceEnumerator();

            foreach (var device in enumerator.EnumerateAudioEndPoints(
                DataFlow.Capture, DeviceState.Active))
            {
                using (device)
                {
                    try
                    {
                        formats.Add(device.AudioClient.MixFormat);
                    }
                    catch (Exception)
                    {
                        formats.Add(null);
                    }
                }
            }
        }
        catch (Exception)
        {
            return formats;
        }

        return formats;
    }
}
