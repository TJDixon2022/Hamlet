using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Hamlet.RadioEngine.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The raw bytes a capture device hands over, turned into the mono floats
/// everything downstream starts from.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE ONE SEGMENT OF THE PATH NOBODY HAD EVER ASSERTED** (unit
/// 237). <see cref="RealOffAirAudioReachesTheTabTests"/> is the strongest evidence
/// this project has that Hamlet can hear a real band, and its own comment says
/// where it begins: ten-millisecond chunks of *mono floats, already made*. Every
/// test that has shown a decode reaching the tab entered the chain downstream of
/// the conversion. <c>WasapiAudioSource.Downmix</c> and
/// <c>WasapiAudioSource.ReadSample</c> — the code between the sound card's buffer
/// and those floats — were executed by no test in this repository at all.</para>
/// <para>**AND IT NEEDS NO SOUND CARD.** Every buffer here is built in this file
/// from sample values chosen in advance, so the verdict does not wait on hardware,
/// on a capture stream or on an owner ruling. What a real device declares is a
/// separate question and is corroboration, not the evidence.</para>
/// <para>**THE TWO CHANNELS ALWAYS CARRY DIFFERENT VALUES**, so a stride error —
/// reading the same channel twice, or reading a frame out of step — cannot hide
/// inside an average that happens to come out right.</para>
/// <para>**NOTHING HERE OPENS A DEVICE, STARTS A STREAM OR RECORDS AUDIO**
/// (`CLAUDE.md` §0.2 and §6), and nothing reaches a transmitter.</para>
/// </remarks>
public sealed class DeviceBytesBecomeTheFloatsTheTapSeesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the per-row error is printed.</param>
    public DeviceBytesBecomeTheFloatsTheTapSeesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The sample rate every format here is built at.</summary>
    /// <remarks>
    /// **48 000 BECAUSE THAT IS WHAT A USB CODEC DELIVERS.** The conversion does
    /// no arithmetic on the rate at all — it divides byte counts by the frame size
    /// — so the rate is a label here rather than a variable, and the two rates the
    /// path really has to survive are exercised where they matter, over the
    /// off-air recordings.
    /// </remarks>
    private const int Rate = 48000;

    /// <summary>
    /// The left channel's samples, and the right's, chosen so that most of them
    /// survive every depth exactly.
    /// </summary>
    /// <remarks>
    /// **ALL BUT THE LAST ARE WHOLE MULTIPLES OF 1/128**, which is the coarsest
    /// step any of these formats has, so an error at 8-bit is a real error and not
    /// the rounding this file chose for itself. The last pair is deliberately not:
    /// it is near full scale and lands between codes at every depth, which is what
    /// makes the per-row tolerance mean something.
    /// </remarks>
    private static readonly double[] Left =
        [0.0, 0.5, -0.5, 0.25, -0.75, 0.9990234375];

    /// <summary>The right channel, never equal to the left in the same frame.</summary>
    private static readonly double[] Right =
        [0.125, -0.125, 0.75, -0.25, 0.5, -0.9990234375];

    /// <summary>How a device lays a sample out in its buffer.</summary>
    /// <remarks>
    /// **STATED BY THE ROW, NEVER DERIVED FROM THE FORMAT.** Asking the code under
    /// test what shape the bytes are, and then building the bytes that shape, is a
    /// test that agrees with any answer. Each row below says what it wrote and
    /// separately says what it told Hamlet it wrote.
    /// </remarks>
    internal enum SampleLayout
    {
        /// <summary>Unsigned 8-bit, 128 for silence, as WAV has always had it.</summary>
        Pcm8,

        /// <summary>Signed 16-bit little-endian.</summary>
        Pcm16,

        /// <summary>Signed 24-bit, three bytes little-endian.</summary>
        Pcm24,

        /// <summary>Signed 32-bit little-endian.</summary>
        Pcm32,

        /// <summary>IEEE-754 single precision.</summary>
        Float32,
    }

    /// <summary>What one row of the table is.</summary>
    /// <param name="Layout">What the bytes actually are.</param>
    /// <param name="Format">What the device tells Hamlet they are.</param>
    /// <param name="Tolerance">
    /// The largest error this depth can produce honestly — one quantisation step.
    /// </param>
    internal sealed record DeviceRow(
        SampleLayout Layout, WaveFormat Format, double Tolerance);

    /// <summary>
    /// KSDATAFORMAT_SUBTYPE_IEEE_FLOAT, the subformat a shared-mode mix format
    /// carries when the device speaks float.
    /// </summary>
    private static readonly Guid SubtypeIeeeFloat =
        new("00000003-0000-0010-8000-00aa00389b71");

    /// <summary>KSDATAFORMAT_SUBTYPE_PCM.</summary>
    private static readonly Guid SubtypePcm =
        new("00000001-0000-0010-8000-00aa00389b71");

    /// <summary>Every format a capture device can present, by name.</summary>
    /// <param name="name">The row's name, as the theory data spells it.</param>
    /// <param name="channels">How many channels the device delivers.</param>
    /// <returns>The row.</returns>
    /// <remarks>
    /// **THE LAST TWO ROWS ARE THE POINT OF THE NIGHT.** Windows shared-mode
    /// capture commonly presents its mix format as WAVE_FORMAT_EXTENSIBLE with a
    /// subformat GUID, rather than with a top-level IEEE-float tag, and the word
    /// <c>Extensible</c> appears nowhere in this repository's own source.
    /// </remarks>
    internal static DeviceRow Row(string name, int channels) => name switch
    {
        "pcm8" => new DeviceRow(
            SampleLayout.Pcm8, new WaveFormat(Rate, 8, channels), 1.0 / 128),
        "pcm16" => new DeviceRow(
            SampleLayout.Pcm16, new WaveFormat(Rate, 16, channels), 1.0 / 32768),
        "pcm24" => new DeviceRow(
            SampleLayout.Pcm24, new WaveFormat(Rate, 24, channels), 1.0 / 8388608),
        "pcm32" => new DeviceRow(
            SampleLayout.Pcm32, new WaveFormat(Rate, 32, channels), 1e-6),
        "float32" => new DeviceRow(
            SampleLayout.Float32,
            WaveFormat.CreateIeeeFloatWaveFormat(Rate, channels),
            1e-7),

        // **THE ONE THE SILENT MORNING POINTS AT.** Same bytes as the row above
        // it, described the way Windows really describes them.
        "extensible-float32" => new DeviceRow(
            SampleLayout.Float32,
            MixFormat(channels, 32, SubtypeIeeeFloat),
            1e-7),
        "extensible-pcm16" => new DeviceRow(
            SampleLayout.Pcm16,
            MixFormat(channels, 16, SubtypePcm),
            1.0 / 32768),
        "extensible-pcm32" => new DeviceRow(
            SampleLayout.Pcm32,
            MixFormat(channels, 32, SubtypePcm),
            1e-6),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "no such row"),
    };

    /// <summary>
    /// Build a WAVEFORMATEXTENSIBLE the way Windows lays one out, and let NAudio
    /// read it back exactly as it reads the one WASAPI returns.
    /// </summary>
    /// <param name="channels">Channel count.</param>
    /// <param name="bits">Container bits per sample.</param>
    /// <param name="subFormat">The subformat GUID.</param>
    /// <returns>The format, as <c>AudioClient.MixFormat</c> would hand it over.</returns>
    /// <remarks>
    /// **THE BYTES ARE LAID OUT HERE, NOT ASKED FOR.** NAudio has a
    /// <c>WaveFormatExtensible</c> constructor, but what this test needs to know is
    /// what comes back through <see cref="WaveFormat.MarshalFromPtr"/> — the one
    /// call <c>AudioClient.MixFormat</c> makes on the pointer the operating system
    /// fills in. Building the structure by hand and marshalling it is the same
    /// path a real device takes, with the device replaced by 40 known bytes.
    /// </remarks>
    internal static WaveFormat MixFormat(int channels, int bits, Guid subFormat)
    {
        var blockAlign = channels * (bits / 8);
        var raw = new byte[40];

        // WAVEFORMATEX, then the extensible tail. WAVE_FORMAT_EXTENSIBLE is 0xFFFE.
        BitConverter.GetBytes((ushort)0xFFFE).CopyTo(raw, 0);
        BitConverter.GetBytes((ushort)channels).CopyTo(raw, 2);
        BitConverter.GetBytes(Rate).CopyTo(raw, 4);
        BitConverter.GetBytes(Rate * blockAlign).CopyTo(raw, 8);
        BitConverter.GetBytes((ushort)blockAlign).CopyTo(raw, 12);
        BitConverter.GetBytes((ushort)bits).CopyTo(raw, 14);
        BitConverter.GetBytes((ushort)22).CopyTo(raw, 16);
        BitConverter.GetBytes((ushort)bits).CopyTo(raw, 18);
        BitConverter.GetBytes(channels == 1 ? 0x4 : 0x3).CopyTo(raw, 20);
        subFormat.ToByteArray().CopyTo(raw, 24);

        var pointer = Marshal.AllocHGlobal(raw.Length);

        try
        {
            Marshal.Copy(raw, 0, pointer, raw.Length);
            return WaveFormat.MarshalFromPtr(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    /// <summary>
    /// **THE ASSERTION THE UNIT EXISTS FOR.** Bytes in a device's own layout become
    /// the samples that were put into them, whatever the device calls itself.
    /// </summary>
    /// <param name="name">Which row of the table.</param>
    /// <param name="channels">Mono or stereo.</param>
    [Theory]
    [InlineData("pcm8", 1)]
    [InlineData("pcm8", 2)]
    [InlineData("pcm16", 1)]
    [InlineData("pcm16", 2)]
    [InlineData("pcm24", 1)]
    [InlineData("pcm24", 2)]
    [InlineData("pcm32", 1)]
    [InlineData("pcm32", 2)]
    [InlineData("float32", 1)]
    [InlineData("float32", 2)]
    [InlineData("extensible-float32", 1)]
    [InlineData("extensible-float32", 2)]
    [InlineData("extensible-pcm16", 2)]
    [InlineData("extensible-pcm32", 2)]
    public void EveryFormatADeviceCanPresentBecomesTheSamplesThatWentIn(
        string name, int channels)
    {
        var row = Row(name, channels);
        var buffer = Encode(row.Layout, channels);
        var mono = Array.Empty<float>();

        var frames = WasapiAudioSource.Downmix(
            buffer, buffer.Length, row.Format, ref mono);

        Assert.Equal(Left.Length, frames);

        var worst = 0.0;
        var worstFrame = -1;

        _output.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "  {0} at {1} channel(s): declared {2} {3}-bit, written as {4}",
                name,
                channels,
                row.Format.Encoding,
                row.Format.BitsPerSample,
                row.Layout));
        _output.WriteLine("");

        for (var frame = 0; frame < frames; frame++)
        {
            var wanted = channels == 1
                ? Left[frame]
                : (Left[frame] + Right[frame]) / 2;

            var error = Math.Abs(mono[frame] - wanted);

            if (error > worst)
            {
                worst = error;
                worstFrame = frame;
            }

            _output.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "    frame {0}  wanted {1,20:0.000000000}  got {2,20:0.000000000}"
                    + "  error {3:0.000000000}",
                    frame,
                    wanted,
                    mono[frame],
                    error));
        }

        _output.WriteLine("");
        _output.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "  worst error {0:0.000000000} at frame {1}, tolerance {2:0.000000000}",
                worst,
                worstFrame,
                row.Tolerance));

        Assert.True(
            worst <= row.Tolerance,
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} at {1} channel(s), declared as {2} {3}-bit: the conversion is "
                + "out by {4:0.000000000} at frame {5} — it produced {6:0.000000000} "
                + "where {7:0.000000000} went in. A device speaking this format "
                + "delivers audio that is loud and is not the band.",
                name,
                channels,
                row.Format.Encoding,
                row.Format.BitsPerSample,
                worst,
                worstFrame,
                worstFrame < 0 ? 0 : mono[worstFrame],
                worstFrame < 0
                    ? 0
                    : channels == 1
                        ? Left[worstFrame]
                        : (Left[worstFrame] + Right[worstFrame]) / 2));
    }

    /// <summary>
    /// **WHAT NAUDIO ACTUALLY HANDS THE CONVERSION**, read out of the package
    /// rather than remembered.
    /// </summary>
    /// <remarks>
    /// <c>AudioClient.MixFormat</c> marshals the pointer the operating system fills
    /// in through <see cref="WaveFormat.MarshalFromPtr"/>, and that method is what
    /// decides which managed type a caller ends up holding. If a device declares
    /// WAVE_FORMAT_EXTENSIBLE, this is what <c>ReadSample</c> is given.
    /// </remarks>
    [Fact]
    public void AnExtensibleMixFormatArrivesWithItsTopLevelTagStillExtensible()
    {
        var format = MixFormat(2, 32, SubtypeIeeeFloat);

        _output.WriteLine(
            $"  NAudio 2.2.1 read the operating system's WAVEFORMATEXTENSIBLE as "
            + $"{format.GetType().Name}, Encoding {format.Encoding}, "
            + $"{format.BitsPerSample}-bit, {format.Channels} channels");

        var extensible = Assert.IsType<WaveFormatExtensible>(format);

        Assert.Equal(WaveFormatEncoding.Extensible, format.Encoding);
        Assert.Equal(32, format.BitsPerSample);
        Assert.Equal(SubtypeIeeeFloat, extensible.SubFormat);

        // **AND THE TOP-LEVEL TAG IS NOT IeeeFloat**, which is the whole of the
        // question: a discriminator that reads only the top-level tag cannot see
        // that these bytes are floats.
        Assert.NotEqual(WaveFormatEncoding.IeeeFloat, format.Encoding);
    }

    /// <summary>
    /// **AND THAT IS THE FORMAT THE CAPTURE TAKES**, read out of NAudio's own IL
    /// rather than assumed.
    /// </summary>
    /// <remarks>
    /// <para>The link this unit's reasoning depends on is that
    /// <c>WasapiCapture.WaveFormat</c> — the object <c>OnDataAvailable</c> hands
    /// <c>Downmix</c> — is the device's mix format and not something NAudio
    /// normalised on the way past. That cannot be observed without a sound card,
    /// so it is read from the constructor's instructions instead.</para>
    /// <para>**A FAILURE HERE IS A FINDING AND NOT A FAULT.** It would mean NAudio
    /// at this version obtains the format some other way, and the reasoning above
    /// this test would need redoing.</para>
    /// </remarks>
    [Fact]
    public void TheCaptureTakesItsFormatFromTheDevicesMixFormat()
    {
        var called = typeof(WasapiCapture)
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance)
            .SelectMany(Calls)
            .ToArray();

        var names = called
            .Select(m => $"{m.DeclaringType?.Name}.{m.Name}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        _output.WriteLine(
            "  what NAudio 2.2.1's WasapiCapture constructors call:");

        foreach (var name in names)
        {
            _output.WriteLine($"    {name}");
        }

        Assert.Contains("MMDevice.get_AudioClient", names, StringComparer.Ordinal);
        Assert.Contains("AudioClient.get_MixFormat", names, StringComparer.Ordinal);
    }

    /// <summary>
    /// **AN UNREADABLE FORMAT REFUSES** rather than answering with silence
    /// (HM-DEC-009).
    /// </summary>
    /// <remarks>
    /// A device speaking something this code cannot read is a device Hamlet cannot
    /// hear, and the honest report of that is nothing at all reaching the tap —
    /// which unit 236's level reading writes down as *no level at all*. Returning
    /// zero would put a quiet band on the sheet, and the operator would spend the
    /// morning turning a gain knob.
    /// </remarks>
    [Fact]
    public void AFormatNothingCanReadRefusesInsteadOfReturningQuietAudio()
    {
        // 64-bit IEEE float: a real WASAPI possibility and one no arm here reads.
        var format = WaveFormat.CreateCustomFormat(
            WaveFormatEncoding.IeeeFloat, Rate, 1, Rate * 8, 8, 64);

        var buffer = new byte[8 * Left.Length];
        var mono = Array.Empty<float>();

        var refusal = Assert.Throws<NotSupportedException>(
            () => WasapiAudioSource.Downmix(
                buffer, buffer.Length, format, ref mono));

        _output.WriteLine($"  the conversion refused with: {refusal.Message}");

        Assert.Contains("64", refusal.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Write the sample values into a device buffer in one layout.
    /// </summary>
    /// <param name="layout">What the bytes are to be.</param>
    /// <param name="channels">One or two.</param>
    /// <returns>The buffer, frame by frame, channels interleaved.</returns>
    /// <remarks>
    /// **WRITTEN HERE AND NOT BORROWED.** Encoding with anything that shares code
    /// with the conversion under test would produce a pair that agrees with itself
    /// and says nothing about either.
    /// </remarks>
    internal static byte[] Encode(SampleLayout layout, int channels)
    {
        var bytesPerSample = layout switch
        {
            SampleLayout.Pcm8 => 1,
            SampleLayout.Pcm16 => 2,
            SampleLayout.Pcm24 => 3,
            _ => 4,
        };

        var buffer = new byte[Left.Length * channels * bytesPerSample];

        for (var frame = 0; frame < Left.Length; frame++)
        {
            for (var channel = 0; channel < channels; channel++)
            {
                var value = channel == 0 ? Left[frame] : Right[frame];
                var at = ((frame * channels) + channel) * bytesPerSample;

                Write(buffer, at, value, layout);
            }
        }

        return buffer;
    }

    /// <summary>One sample, into the buffer, in one layout.</summary>
    /// <param name="buffer">Where it goes.</param>
    /// <param name="at">The byte offset.</param>
    /// <param name="value">The sample, as a fraction of full scale.</param>
    /// <param name="layout">What shape to write it in.</param>
    internal static void Write(byte[] buffer, int at, double value, SampleLayout layout)
    {
        switch (layout)
        {
            case SampleLayout.Pcm8:
                buffer[at] = (byte)Math.Clamp(
                    Math.Round(value * 128) + 128, 0, 255);
                break;

            case SampleLayout.Pcm16:
                BitConverter
                    .GetBytes((short)Math.Clamp(
                        Math.Round(value * 32768), short.MinValue, short.MaxValue))
                    .CopyTo(buffer, at);
                break;

            case SampleLayout.Pcm24:
                var packed = (int)Math.Clamp(
                    Math.Round(value * 8388608), -8388608, 8388607);
                buffer[at] = (byte)(packed & 0xFF);
                buffer[at + 1] = (byte)((packed >> 8) & 0xFF);
                buffer[at + 2] = (byte)((packed >> 16) & 0xFF);
                break;

            case SampleLayout.Pcm32:
                BitConverter
                    .GetBytes((int)Math.Clamp(
                        Math.Round(value * 2147483648.0),
                        int.MinValue,
                        int.MaxValue))
                    .CopyTo(buffer, at);
                break;

            default:
                BitConverter.GetBytes((float)value).CopyTo(buffer, at);
                break;
        }
    }

    /// <summary>Every method one method's instructions call.</summary>
    /// <param name="method">The method to read.</param>
    /// <returns>What it calls, where the token could be resolved.</returns>
    /// <remarks>
    /// **A REAL WALK AND NOT A SCAN FOR FOUR-BYTE PATTERNS.** The operand widths
    /// come from <see cref="OpCodes"/> itself, so every instruction's length is
    /// the framework's own answer rather than this file's guess, and a token read
    /// out of the middle of some other instruction's operand cannot be reported as
    /// a call that is not there.
    /// </remarks>
    private static IEnumerable<MethodBase> Calls(MethodBase method)
    {
        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();

        if (il is null)
        {
            yield break;
        }

        var module = method.Module;
        var at = 0;

        while (at < il.Length)
        {
            short value = il[at];
            at++;

            if (value == 0xFE && at < il.Length)
            {
                value = unchecked((short)(0xFE00 | il[at]));
                at++;
            }

            if (!Opcodes.TryGetValue(value, out var opcode))
            {
                yield break;
            }

            var operand = OperandLength(opcode, il, at);

            if (opcode == OpCodes.Call
                || opcode == OpCodes.Callvirt
                || opcode == OpCodes.Newobj)
            {
                var token = BitConverter.ToInt32(il, at);
                MethodBase? resolved;

                try
                {
                    resolved = module.ResolveMethod(token);
                }
                catch (ArgumentException)
                {
                    resolved = null;
                }

                if (resolved is not null)
                {
                    yield return resolved;
                }
            }

            at += operand;
        }
    }

    /// <summary>Every IL opcode the framework knows, by its value.</summary>
    private static readonly IReadOnlyDictionary<short, OpCode> Opcodes =
        typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode))
            .Select(f => (OpCode)f.GetValue(null)!)
            .ToDictionary(o => o.Value);

    /// <summary>How many bytes of operand one instruction carries.</summary>
    /// <param name="opcode">The instruction.</param>
    /// <param name="il">The whole body.</param>
    /// <param name="at">Where the operand starts.</param>
    /// <returns>The operand's length in bytes.</returns>
    private static int OperandLength(OpCode opcode, byte[] il, int at) =>
        opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget => 1,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 => 8,
            OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + (4 * BitConverter.ToInt32(il, at)),
            _ => 4,
        };
}
