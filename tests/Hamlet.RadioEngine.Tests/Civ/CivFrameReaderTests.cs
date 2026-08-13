using Hamlet.RadioEngine.Civ;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Civ;

public sealed class CivFrameReaderTests
{
    private static readonly byte[] ReadFreqResponse =
    {
        0xFE, 0xFE, 0xE0, 0x94, 0x03, 0x00, 0x00, 0x03, 0x07, 0x00, 0xFD,
    };

    /// <remarks>Proves: a whole frame in one chunk parses to the right
    /// addresses, command and payload.</remarks>
    [Fact]
    public void WholeFrame_Parses()
    {
        var reader = new CivFrameReader();
        var frames = reader.Feed(ReadFreqResponse);

        var frame = Assert.Single(frames);
        Assert.Equal(0xE0, frame.To);
        Assert.Equal(0x94, frame.From);
        Assert.Equal(0x03, frame.Command);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x03, 0x07, 0x00 }, frame.Data);
    }

    /// <remarks>Proves: serial chunking is invisible — a frame split at every
    /// possible boundary still yields exactly one frame. This is the property
    /// that makes the read loop correct by construction.</remarks>
    [Fact]
    public void SplitAtEveryBoundary_StillOneFrame()
    {
        for (var split = 1; split < ReadFreqResponse.Length; split++)
        {
            var reader = new CivFrameReader();
            var first = reader.Feed(ReadFreqResponse.AsSpan(0, split));
            var second = reader.Feed(ReadFreqResponse.AsSpan(split));

            Assert.Equal(1, first.Count + second.Count);
        }
    }

    /// <remarks>Proves: noise before the preamble is discarded, counted, and
    /// the following frame still parses — diagnosable, not silent (§0.0.1).</remarks>
    [Fact]
    public void NoiseBeforeFrame_DiscardedAndCounted()
    {
        var reader = new CivFrameReader();
        var noisy = new byte[] { 0x42, 0x00, 0xFF }.Concat(ReadFreqResponse).ToArray();

        var frames = reader.Feed(noisy);

        Assert.Single(frames);
        Assert.Equal(3, reader.DiscardedByteCount);
    }

    /// <remarks>Proves: two frames in one chunk both come out, in order.</remarks>
    [Fact]
    public void TwoFramesOneChunk_BothParse()
    {
        var ok = new byte[] { 0xFE, 0xFE, 0xE0, 0x94, 0xFB, 0xFD };
        var reader = new CivFrameReader();

        var frames = reader.Feed(ReadFreqResponse.Concat(ok).ToArray());

        Assert.Equal(2, frames.Count);
        Assert.Equal(0x03, frames[0].Command);
        Assert.Equal(0xFB, frames[1].Command);
    }

    /// <remarks>Proves: a terminator arriving before a full header is treated
    /// as a malformed run and skipped; the next real frame still parses.</remarks>
    [Fact]
    public void MalformedShortFrame_SkippedLoudly()
    {
        var malformed = new byte[] { 0xFE, 0xFE, 0xE0, 0xFD };
        var reader = new CivFrameReader();

        var none = reader.Feed(malformed);
        var frames = reader.Feed(ReadFreqResponse);

        Assert.Empty(none);
        Assert.Single(frames);
        Assert.True(reader.DiscardedByteCount > 0);
    }
}
