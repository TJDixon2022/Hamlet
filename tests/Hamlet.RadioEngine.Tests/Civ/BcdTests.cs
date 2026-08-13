using Hamlet.RadioEngine.Civ;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Civ;

public sealed class BcdTests
{
    /// <remarks>Proves: the little-endian byte-pair encoding matches the
    /// CI-V convention on worked examples spanning both phase 1 bands.</remarks>
    [Theory]
    [InlineData(7_030_000, new byte[] { 0x00, 0x00, 0x03, 0x07, 0x00 })]
    [InlineData(14_074_000, new byte[] { 0x00, 0x40, 0x07, 0x14, 0x00 })]
    [InlineData(0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(9_999_999_999, new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99 })]
    public void Encode_MatchesWorkedExamples(long hz, byte[] expected)
    {
        Assert.Equal(expected, Bcd.EncodeFrequencyHz(hz));
    }

    /// <remarks>Proves: decode inverts encode across representative values —
    /// determinism per §5.</remarks>
    [Theory]
    [InlineData(7_030_000)]
    [InlineData(14_074_000)]
    [InlineData(146_520_000)]
    [InlineData(1)]
    public void Decode_InvertsEncode(long hz)
    {
        Assert.Equal(hz, Bcd.DecodeFrequencyHz(Bcd.EncodeFrequencyHz(hz)));
    }

    /// <remarks>Proves: a non-BCD nibble fails loud instead of decoding to a
    /// plausible wrong frequency — prime directive at the byte level.</remarks>
    [Fact]
    public void Decode_RejectsNonBcdNibble()
    {
        var bad = new byte[] { 0x00, 0x0A, 0x00, 0x00, 0x00 };
        Assert.Throws<ArgumentException>(() => Bcd.DecodeFrequencyHz(bad));
    }

    /// <remarks>Proves: wrong-length fields fail loud.</remarks>
    [Fact]
    public void Decode_RejectsWrongLength()
    {
        Assert.Throws<ArgumentException>(() => Bcd.DecodeFrequencyHz(new byte[4]));
    }

    /// <remarks>Proves: out-of-range frequencies are rejected at encode.</remarks>
    [Fact]
    public void Encode_RejectsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Bcd.EncodeFrequencyHz(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bcd.EncodeFrequencyHz(10_000_000_000));
    }
}
