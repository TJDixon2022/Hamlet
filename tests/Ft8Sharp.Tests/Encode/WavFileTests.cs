using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// The WAV reader, watched refusing everything it claims to refuse.
/// </summary>
/// <remarks>
/// <para>
/// <b>These run everywhere.</b> The bytes are built here rather than read from a file, so nothing
/// malformed has to be manufactured on disk and the reader's refusals are exercised on a machine
/// with no clone and no oracle — which is where a reader that had quietly stopped refusing would
/// otherwise go unnoticed until the one machine that runs the comparison ran it.
/// </para>
/// <para>
/// <b>Why a reader this narrow.</b> A reader that copes with a stereo file, or a longer fmt chunk,
/// or a truncated one, is a reader that will one day hand the comparison a shifted or interleaved
/// signal — and the comparison would faithfully report that as a difference between our synthesis
/// and upstream's, which is the one conclusion it must never reach by accident.
/// </para>
/// </remarks>
public class WavFileTests
{
    private readonly ITestOutputHelper _output;

    public WavFileTests(ITestOutputHelper output) => _output = output;

    /// <summary>A well-formed file of <paramref name="sampleCount"/> samples, as upstream writes it.</summary>
    private static byte[] Canonical(int sampleCount, int sampleRate = 12000, int channels = 1, int bits = 16)
    {
        var blockAlign = channels * bits / 8;
        var dataSize = sampleCount * blockAlign;
        var bytes = new byte[WavFile.CanonicalHeaderBytes + dataSize];
        var at = 0;

        void Tag(string t)
        {
            Encoding.ASCII.GetBytes(t).CopyTo(bytes, at);
            at += 4;
        }

        void U32(uint v)
        {
            BitConverter.GetBytes(v).CopyTo(bytes, at);
            at += 4;
        }

        void U16(ushort v)
        {
            BitConverter.GetBytes(v).CopyTo(bytes, at);
            at += 2;
        }

        Tag("RIFF");
        U32((uint)(4 + 8 + 16 + 8 + dataSize));
        Tag("WAVE");
        Tag("fmt ");
        U32(16);
        U16(1);
        U16((ushort)channels);
        U32((uint)sampleRate);
        U32((uint)(sampleRate * blockAlign));
        U16((ushort)blockAlign);
        U16((ushort)bits);
        Tag("data");
        U32((uint)dataSize);

        for (var i = 0; i < sampleCount; i++)
        {
            BitConverter.GetBytes((short)(i * 7)).CopyTo(bytes, WavFile.CanonicalHeaderBytes + (i * 2));
        }

        return bytes;
    }

    [Fact]
    public void AWellFormedFileIsReadAndItsSamplesComeBack()
    {
        var contents = WavFile.Parse(Canonical(100), "a made-up file");
        _output.WriteLine(
            $"{contents.SampleRate} Hz, {contents.BitsPerSample} bits, {contents.ChannelCount} channel, "
            + $"{contents.HeaderBytes}-byte header, {contents.Samples.Length} samples");

        Assert.Equal(12000, contents.SampleRate);
        Assert.Equal(16, contents.BitsPerSample);
        Assert.Equal(1, contents.ChannelCount);
        Assert.Equal(WavFile.CanonicalHeaderBytes, contents.HeaderBytes);
        Assert.Equal(100, contents.Samples.Length);
        Assert.Equal(0, contents.Samples[0]);
        Assert.Equal(7 * 99, contents.Samples[99]);

        // Negative samples survive the round trip, which is what says the reader is reading signed
        // sixteen-bit and not unsigned.
        var negative = Canonical(2);
        BitConverter.GetBytes((short)-1234).CopyTo(negative, WavFile.CanonicalHeaderBytes);
        Assert.Equal(-1234, WavFile.Parse(negative, "signed").Samples[0]);
    }

    [Fact]
    public void ATruncatedHeaderIsRefused()
    {
        foreach (var length in new[] { 0, 12, 43 })
        {
            var thrown = Assert.Throws<InvalidDataException>(
                () => WavFile.Parse(Canonical(10).AsSpan(0, length), "a truncated file"));
            _output.WriteLine($"{length,3} bytes: {thrown.Message}");
            Assert.Contains(length.ToString(), thrown.Message);
        }
    }

    [Fact]
    public void AWrongFormatTagIsRefused()
    {
        foreach (var (at, tag, name) in new[]
                 {
                     (0, "RIFX", "the RIFF tag"),
                     (8, "AIFF", "the WAVE form"),
                     (12, "LIST", "the fmt chunk"),
                     (36, "fact", "the data chunk"),
                 })
        {
            var bytes = Canonical(10);
            Encoding.ASCII.GetBytes(tag).CopyTo(bytes, at);
            var thrown = Assert.Throws<InvalidDataException>(() => WavFile.Parse(bytes, "a wrong tag"));
            _output.WriteLine($"{name,-16}: {thrown.Message}");
            Assert.Contains(tag, thrown.Message);
        }

        // A compressed format is refused too, even with every tag in place.
        var compressed = Canonical(10);
        BitConverter.GetBytes((ushort)3).CopyTo(compressed, 20);
        var format = Assert.Throws<InvalidDataException>(
            () => WavFile.Parse(compressed, "a compressed file"));
        _output.WriteLine($"audio format 3  : {format.Message}");
        Assert.Contains("PCM", format.Message);

        // And a longer fmt chunk, which would move every sample index after it.
        var longFmt = Canonical(10);
        BitConverter.GetBytes(18u).CopyTo(longFmt, 16);
        var fmt = Assert.Throws<InvalidDataException>(() => WavFile.Parse(longFmt, "a long fmt chunk"));
        _output.WriteLine($"an 18-byte fmt  : {fmt.Message}");
        Assert.Contains("18", fmt.Message);
    }

    [Fact]
    public void AWrongChannelCountIsRefused()
    {
        foreach (var channels in new[] { 0, 2, 6 })
        {
            var bytes = Canonical(10);
            BitConverter.GetBytes((ushort)channels).CopyTo(bytes, 22);
            var thrown = Assert.Throws<InvalidDataException>(() => WavFile.Parse(bytes, "not mono"));
            _output.WriteLine($"{channels} channels: {thrown.Message}");
            Assert.Contains(channels.ToString(), thrown.Message);
        }

        // And a bit depth that has no sixteen-bit counts to compare.
        foreach (var bits in new[] { 8, 24, 32 })
        {
            var bytes = Canonical(10);
            BitConverter.GetBytes((ushort)bits).CopyTo(bytes, 34);
            var thrown = Assert.Throws<InvalidDataException>(() => WavFile.Parse(bytes, "not 16-bit"));
            _output.WriteLine($"{bits,2} bits   : {thrown.Message}");
            Assert.Contains(bits.ToString(), thrown.Message);
        }
    }

    [Fact]
    public void AFileShorterThanItsHeaderClaimsIsRefused()
    {
        var bytes = Canonical(100);

        // The header still says a hundred samples; the file carries fifty.
        var truncated = bytes.AsSpan(0, WavFile.CanonicalHeaderBytes + 100).ToArray();
        var thrown = Assert.Throws<InvalidDataException>(
            () => WavFile.Parse(truncated, "a short file"));
        _output.WriteLine(thrown.Message);
        Assert.Contains("200", thrown.Message);
        Assert.Contains("100", thrown.Message);

        // A file that carries MORE than its header claims is read to the claim, not past it — that
        // is not a malformed file, it is a file with something after the data chunk.
        var padded = new byte[bytes.Length + 64];
        bytes.CopyTo(padded, 0);
        Assert.Equal(100, WavFile.Parse(padded, "a padded file").Samples.Length);
    }
}
