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
/// <b>Deliberately narrow.</b> It reads exactly the one form upstream writes — canonical 44-byte
/// PCM, mono, sixteen bits — and refuses everything else rather than coping. A reader that copes is
/// a reader that will one day quietly read a stereo file as twice as many mono samples and report
/// the resulting disagreement as a defect in the synthesizer. Every refusal below is watched by
/// <c>WavFileTests</c>; a guard that has never refused is not a guard.
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

        RequireTag(bytes, 36, "data", what, "the second chunk is not data");

        var dataSize = ReadUInt32(bytes, 40);
        var available = bytes.Length - CanonicalHeaderBytes;
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
            samples[i] = (short)ReadUInt16(bytes, CanonicalHeaderBytes + (i * 2));
        }

        return new Contents(sampleRate, bits, channels, CanonicalHeaderBytes, samples);
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
