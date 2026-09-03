using System.Text;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Reads the WAV upstream's generator writes, so that our samples can be held against its samples.
/// </summary>
/// <remarks>
/// <para>
/// <b>This lives in the test project and may never move into <c>src/Ft8Sharp/</c>.</b> The library
/// synthesizes a buffer and does nothing else with it — no file, no stream, no device. Reading
/// somebody else's file is the comparison's business, not the library's, and the boundary is
/// mechanical for a reason.
/// </para>
/// <para>
/// <b>Deliberately narrow.</b> It reads uncompressed PCM, mono, sixteen bits, with a canonical
/// 16-byte fmt chunk, and refuses everything else rather than coping. A reader that copes is a
/// reader that will one day quietly read a stereo file as twice as many mono samples and report the
/// resulting disagreement as a defect in the synthesizer. Every refusal below is watched by
/// <c>WavFileTests</c>; a guard that has never refused is not a guard.
/// </para>
/// <para>
/// <b>One thing it does cope with, added by unit 216 and only that one.</b> It walks the chunks
/// after <c>fmt</c> to find <c>data</c> rather than requiring it to be second. Upstream's generator
/// always writes it second; nine of the sixty off-air reference recordings in the pinned clone put
/// a chunk in between, and refusing those would have narrowed the reference-WAV criterion to
/// fifty-one files for a reason that has nothing to do with the audio. Nothing else was relaxed.
/// </para>
/// <para>
/// <b>Nothing it reads is ever committed.</b> The files live under <see cref="Path.GetTempPath"/>
/// and are deleted as each message is compared.
/// </para>
/// </remarks>
internal static class WavFile
{
    /// <summary>The canonical header: RIFF, WAVE, a 16-byte fmt chunk, and a data chunk.</summary>
    public const int CanonicalHeaderBytes = 44;

    /// <summary>What one file held.</summary>
    /// <param name="SampleRate">Samples per second, as the fmt chunk states it.</param>
    /// <param name="BitsPerSample">Bits per sample, as the fmt chunk states it.</param>
    /// <param name="ChannelCount">Channels, as the fmt chunk states it.</param>
    /// <param name="HeaderBytes">How many bytes precede the first sample.</param>
    /// <param name="Samples">The samples themselves, never committed and never printed.</param>
    internal sealed record Contents(
        int SampleRate,
        int BitsPerSample,
        int ChannelCount,
        int HeaderBytes,
        short[] Samples);

    /// <summary>Reads one file, refusing anything that is not the form upstream writes.</summary>
    public static Contents Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return Parse(bytes, path);
    }

    /// <summary>
    /// The parse, split out so the refusals can be watched against bytes built in a test rather
    /// than against files that would have to be manufactured on disk to be malformed.
    /// </summary>
    public static Contents Parse(ReadOnlySpan<byte> bytes, string what)
    {
        if (bytes.Length < CanonicalHeaderBytes)
        {
            throw new InvalidDataException(
                $"{what} is {bytes.Length} bytes, which is shorter than the {CanonicalHeaderBytes}-byte "
                + "header a WAV file must at least carry, so there is no header to read.");
        }

        RequireTag(bytes, 0, "RIFF", what, "the file is not RIFF");
        RequireTag(bytes, 8, "WAVE", what, "the RIFF form is not WAVE");
        RequireTag(bytes, 12, "fmt ", what, "the first chunk is not fmt");

        var fmtSize = ReadUInt32(bytes, 16);
        if (fmtSize != 16)
        {
            throw new InvalidDataException(
                $"{what} declares a {fmtSize}-byte fmt chunk. Upstream writes the canonical 16-byte "
                + "PCM one and this reader deliberately reads no other, because a longer fmt chunk "
                + "moves the data chunk and every sample index after it.");
        }

        var format = ReadUInt16(bytes, 20);
        if (format != 1)
        {
            throw new InvalidDataException(
                $"{what} declares audio format {format}, and only 1 — uncompressed PCM — carries "
                + "samples this comparison can read. Anything else would be decoded rather than read.");
        }

        var channels = ReadUInt16(bytes, 22);
        if (channels != 1)
        {
            throw new InvalidDataException(
                $"{what} declares {channels} channels and this reader takes only mono. A stereo file "
                + "read as mono yields twice as many samples, interleaved, and the comparison would "
                + "report the interleaving as a defect in the synthesizer.");
        }

        var sampleRate = (int)ReadUInt32(bytes, 24);
        var bits = ReadUInt16(bytes, 34);
        if (bits != 16)
        {
            throw new InvalidDataException(
                $"{what} declares {bits} bits per sample and this reader takes only 16. The "
                + "comparison is measured in sixteen-bit counts and a different width has no counts "
                + "to compare.");
        }

        // WALK TO THE DATA CHUNK RATHER THAN ASSUMING IT IS SECOND.
        //
        // Upstream's own generator writes it second, at byte 36, and for four units that was every
        // file this reader had to read. Unit 216 handed it upstream's off-air REFERENCE RECORDINGS,
        // and nine of the sixty carry a chunk between fmt and data — a 158-byte one, which is why
        // those files are 360202 bytes where the rest are 360044. Refusing them would have narrowed
        // criterion 3 to fifty-one files for a reason that has nothing to do with the audio.
        //
        // So the walk is added and NOTHING ELSE IS RELAXED: the RIFF and WAVE tags, the fmt chunk's
        // position and length, the format, the channel count, the bit width and the truncation check
        // are all exactly as they were. A file with no data chunk anywhere is still refused, and the
        // refusal names every chunk it did find.
        var dataAt = -1;
        var dataSize = 0u;
        var tagsSeen = new List<string>();
        var cursor = 20 + (int)fmtSize;
        while (cursor + 8 <= bytes.Length)
        {
            var tag = Encoding.ASCII.GetString(bytes.Slice(cursor, 4));
            var size = ReadUInt32(bytes, cursor + 4);
            tagsSeen.Add(Printable(tag));

            if (string.Equals(tag, "data", StringComparison.Ordinal))
            {
                dataAt = cursor + 8;
                dataSize = size;
                break;
            }

            // Chunks are padded to an even length, and a zero-length chunk would not advance.
            var advance = (long)size + (size % 2);
            cursor += 8 + (int)Math.Min(advance, bytes.Length);
        }

        if (dataAt < 0)
        {
            throw new InvalidDataException(
                $"{what} carries no data chunk. The chunks after fmt were: "
                + $"{(tagsSeen.Count == 0 ? "none" : string.Join(", ", tagsSeen))}. "
                + "A WAV with no data chunk has no samples to compare.");
        }

        var available = bytes.Length - dataAt;
        if (dataSize > (uint)available)
        {
            throw new InvalidDataException(
                $"{what} declares a {dataSize}-byte data chunk and carries {available} bytes after its "
                + "header. A file shorter than its own header claims would be read as a short signal "
                + "and compared against a full one.");
        }

        var samples = new short[dataSize / 2];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (short)ReadUInt16(bytes, dataAt + (i * 2));
        }

        return new Contents(sampleRate, bits, channels, dataAt, samples);
    }

    /// <summary>
    /// <b>Writes a slot the way upstream's own <c>save_wav</c> writes one</b>, so that a file this
    /// harness makes is a file both decoders would accept and neither is being handed something
    /// only the other has seen before.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Added by unit 227, and the reason it exists is the whole point of that unit's method.</b>
    /// The paired comparison writes each slot to disk once and has <em>both</em> decoders read that
    /// same file. Sixteen-bit quantisation is then common to both, and this port's own control
    /// number is taken through the file rather than from the float array it came from.
    /// </para>
    /// <para>
    /// <b>The conversion is upstream's, transcribed rather than improved.</b> Read from
    /// <c>common/wave.c</c> in the pin on 2026-09-02: clamp to -1..+1, then
    /// <c>(int)(0.5 + (x * 32767.0))</c> — the arithmetic in <em>double</em>, and the cast
    /// truncating toward zero, which is not <see cref="Math.Round(double)"/> and is worth one count
    /// on about half the samples if got wrong. Unit 212 measured that same half-before-truncate on
    /// the generator side and this is the same rule.
    /// </para>
    /// <para>
    /// <b>The header is the canonical 44 bytes and nothing else.</b> Upstream's <c>load_wav</c> does
    /// not walk chunks — it reads <c>fmt</c> then <c>data</c> at fixed offsets and refuses a
    /// <c>fmt</c> chunk that is not 16 bytes. A file with a chunk in between would be read as
    /// nonsense rather than refused, so this writes only the form upstream writes.
    /// </para>
    /// <para>
    /// <b>Nothing written here is ever committed.</b> The caller puts it under
    /// <see cref="Path.GetTempPath"/> and deletes it.
    /// </para>
    /// </remarks>
    public static void Write(string path, ReadOnlySpan<float> samples, int sampleRate)
    {
        const int bitsPerSample = 16;
        const int channels = 1;
        const int blockAlign = channels * bitsPerSample / 8;

        var dataSize = samples.Length * blockAlign;
        var bytes = new byte[CanonicalHeaderBytes + dataSize];

        WriteTag(bytes, 0, "RIFF");
        WriteUInt32(bytes, 4, (uint)(4 + 8 + 16 + 8 + dataSize));
        WriteTag(bytes, 8, "WAVE");

        WriteTag(bytes, 12, "fmt ");
        WriteUInt32(bytes, 16, 16);
        WriteUInt16(bytes, 20, 1);
        WriteUInt16(bytes, 22, channels);
        WriteUInt32(bytes, 24, (uint)sampleRate);
        WriteUInt32(bytes, 28, (uint)(sampleRate * blockAlign));
        WriteUInt16(bytes, 32, blockAlign);
        WriteUInt16(bytes, 34, bitsPerSample);

        WriteTag(bytes, 36, "data");
        WriteUInt32(bytes, 40, (uint)dataSize);

        for (var i = 0; i < samples.Length; i++)
        {
            WriteUInt16(bytes, CanonicalHeaderBytes + (i * 2), (ushort)Quantise(samples[i]));
        }

        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// One float becoming one sixteen-bit count, by upstream's rule and not by the framework's.
    /// </summary>
    /// <remarks>
    /// Exposed so <c>WavFileTests</c> can watch the clamp and the half-before-truncate on named
    /// values rather than inferring them from a file.
    /// </remarks>
    public static short Quantise(float sample)
    {
        var x = (double)sample;
        if (x > 1.0)
        {
            x = 1.0;
        }
        else if (x < -1.0)
        {
            x = -1.0;
        }

        return (short)(int)(0.5 + (x * 32767.0));
    }

    private static void WriteTag(byte[] bytes, int at, string tag)
    {
        for (var i = 0; i < 4; i++)
        {
            bytes[at + i] = (byte)tag[i];
        }
    }

    private static void WriteUInt32(byte[] bytes, int at, uint value)
    {
        bytes[at] = (byte)value;
        bytes[at + 1] = (byte)(value >> 8);
        bytes[at + 2] = (byte)(value >> 16);
        bytes[at + 3] = (byte)(value >> 24);
    }

    private static void WriteUInt16(byte[] bytes, int at, ushort value)
    {
        bytes[at] = (byte)value;
        bytes[at + 1] = (byte)(value >> 8);
    }

    /// <summary>Removes a temporary file, and never minds if it is already gone.</summary>
    public static void DeleteQuietly(string? path)
    {
        if (path is null)
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // A file left in the system temp folder is untidy, never wrong, and never in the tree.
        }
    }

    private static void RequireTag(ReadOnlySpan<byte> bytes, int at, string tag, string what, string why)
    {
        var found = Encoding.ASCII.GetString(bytes.Slice(at, 4));
        if (!string.Equals(found, tag, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"{what}: {why} — expected '{tag}' at byte {at} and found '{Printable(found)}'.");
        }
    }

    private static string Printable(string raw) =>
        new(raw.Select(c => char.IsAsciiLetterOrDigit(c) || c == ' ' ? c : '?').ToArray());

    private static uint ReadUInt32(ReadOnlySpan<byte> bytes, int at) =>
        (uint)(bytes[at] | (bytes[at + 1] << 8) | (bytes[at + 2] << 16) | (bytes[at + 3] << 24));

    private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, int at) =>
        (ushort)(bytes[at] | (bytes[at + 1] << 8));
}
