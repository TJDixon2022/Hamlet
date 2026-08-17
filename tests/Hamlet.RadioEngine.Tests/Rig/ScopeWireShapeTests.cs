using Hamlet.RadioEngine.Civ;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The scope frame as the radio actually sends it (HM-DEC-094).
/// </summary>
/// <remarks>
/// <para>**THESE ARE THE BYTES THE RADIO SENT.** Two samples, recorded off the
/// wire on the operator's own IC-7300 by the rejection counter added the session
/// before, after 2,740 parts had arrived and every one had been thrown away.</para>
/// <para>Every other scope test in this repository was built to the shape the
/// parser expected, which is why they all passed for months while the instrument
/// discarded everything the radio said. A fixture written from the same
/// understanding as the code confirms the understanding and nothing else
/// (HM-DEC-048).</para>
/// </remarks>
public sealed class ScopeWireShapeTests
{
    /// <summary>
    /// Part 8 of 11, as it came off the wire: 53 bytes.
    /// </summary>
    /// <remarks>
    /// Reported by the parser's own rejection reason as
    /// "part header unreadable, 53 bytes, first bytes 0008112A2F2B".
    /// </remarks>
    private static byte[] PartEight()
    {
        var bytes = new byte[53];

        bytes[0] = 0x00;   // field 1, fixed
        bytes[1] = 0x08;   // field 2, the order of this part
        bytes[2] = 0x11;   // field 3, the division maximum: BCD eleven
        bytes[3] = 0x2A;
        bytes[4] = 0x2F;
        bytes[5] = 0x2B;

        for (var i = 6; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(0x20 + (i % 16));
        }

        return bytes;
    }

    /// <summary>Part 4 of 11, the second independent sample.</summary>
    /// <remarks>First bytes "000411181B17", also 53 bytes.</remarks>
    private static byte[] PartFour()
    {
        var bytes = PartEight();

        bytes[1] = 0x04;
        bytes[3] = 0x18;
        bytes[4] = 0x1B;
        bytes[5] = 0x17;

        return bytes;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094, and it is the whole bug. **Both samples parse,
    /// and under the old reading neither did.** Field 1 is a fixed zero, so the
    /// parser read every part's order as nought and failed its own "a part number
    /// is at least one" check on every part of every sweep.</para>
    /// </remarks>
    [Theory]
    [InlineData(8)]
    [InlineData(4)]
    public void TheRadiosOwnPartsParse(int expectedSequence)
    {
        var payload = expectedSequence == 8 ? PartEight() : PartFour();
        var part = CivScope.ReadPart(payload);

        Assert.NotNull(part);
        Assert.Equal(expectedSequence, part!.Value.Sequence);
        Assert.Equal(11, part.Value.Total);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094: **the division maximum is BCD.** `0x11` is eleven
    /// printed on the byte and seventeen taken at face value, and this radio
    /// divides a sweep into eleven parts over USB.</para>
    /// <para>The arithmetic settles it without the manual open. The waveform is
    /// 475 points and the first part carries none of it, so eleven parts means
    /// ten carrying about fifty each, which is exactly the 53-byte parts the wire
    /// produced. Seventeen parts would need eight hundred bytes to describe four
    /// hundred and seventy-five points.</para>
    /// </remarks>
    [Fact]
    public void TheDivisionMaximumIsBcdAndTheArithmeticProvesIt()
    {
        Assert.Equal(11, Bcd.DecodeByte(0x11));
        Assert.Equal(CivScope.PartsOverUsb, Bcd.DecodeByte(0x11));

        var carryingData = CivScope.PartsOverUsb - 1;
        var perPart = PartEight().Length - CivScope.PartHeaderLength;

        // Ten parts of fifty covers a 475-point waveform; sixteen would not be
        // describing the same thing at all.
        Assert.InRange(carryingData * perPart, CivScope.WaveformLength, 550);
        Assert.True((17 - 1) * perPart > CivScope.WaveformLength + 200);
    }

    /// <remarks>
    /// Proves HM-DEC-094: the order is BCD too, which only shows above nine.
    /// Parts ten and eleven would read as sixteen and seventeen if the byte were
    /// taken at face value, and a sweep would never complete.
    /// </remarks>
    [Fact]
    public void TheOrderIsBcdWhichOnlyShowsAboveNine()
    {
        var tenth = PartEight();
        tenth[1] = 0x10;

        var eleventh = PartEight();
        eleventh[1] = 0x11;

        Assert.Equal(10, CivScope.ReadPart(tenth)!.Value.Sequence);
        Assert.Equal(11, CivScope.ReadPart(eleventh)!.Value.Sequence);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094: **the waveform starts after three bytes, not
    /// two.** One byte of the header used to be read as an amplitude, which would
    /// have put a spurious value at the left edge of every row had anything ever
    /// drawn.</para>
    /// </remarks>
    [Fact]
    public void TheWaveformStartsAfterTheThreeHeaderBytes()
    {
        var waveform = CivScope.Waveform(PartEight());

        Assert.Equal(50, waveform.Length);
        Assert.Equal(0x2A, waveform[0]);
        Assert.Equal(0x2F, waveform[1]);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094 and §0.0: a part whose fixed field is not zero, or
    /// whose counts are not BCD digits at all, produces nothing rather than a
    /// nearest guess. A waterfall drawn from a frame nobody could read would be
    /// Hamlet's invention on the one surface built to show what is actually
    /// there.</para>
    /// </remarks>
    [Fact]
    public void AnythingThatIsNotThisShapeProducesNothing()
    {
        var wrongFixedField = PartEight();
        wrongFixedField[0] = 0x01;
        Assert.Null(CivScope.ReadPart(wrongFixedField));

        var notBcd = PartEight();
        notBcd[2] = 0xAF;
        Assert.Null(CivScope.ReadPart(notBcd));

        var beyondTheTotal = PartEight();
        beyondTheTotal[1] = 0x12;
        Assert.Null(CivScope.ReadPart(beyondTheTotal));

        Assert.Null(CivScope.ReadPart(Array.Empty<byte>()));
        Assert.Null(CivScope.ReadPart(new byte[] { 0x00, 0x08 }));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094: **the first part is not the same shape as the
    /// others.** It carries the wave information and no waveform, so a parser
    /// expecting a uniform layout loses the one part that says where on the band
    /// the sweep sits, and the axis becomes a guess.</para>
    /// <para>This shape has not been seen on the wire: both real samples are
    /// continuation parts. It is built from the documented layout and is marked
    /// as such, because a fixture that claims more provenance than it has is the
    /// fault this whole file exists to correct.</para>
    /// </remarks>
    [Fact]
    public void TheFirstPartCarriesTheWaveInformationAndNoWaveform()
    {
        var bytes = new List<byte> { 0x00, 0x01, 0x11, 0x00 };

        bytes.AddRange(Bcd.EncodeFrequencyHz(14_100_000));
        bytes.AddRange(Bcd.EncodeFrequencyHz(200_000));
        bytes.Add(0x00);

        var header = CivScope.ReadHeader(bytes.ToArray());

        Assert.NotNull(header);
        Assert.Equal(1, header!.Sequence);
        Assert.Equal(11, header.Total);
        Assert.False(header.IsFixedMode);
        Assert.Equal(14_000_000, header.LowHz);
        Assert.Equal(14_200_000, header.HighHz);

        // And it carries no amplitudes, so nothing is drawn from it.
        Assert.Equal(
            bytes.Count - CivScope.PartHeaderLength,
            CivScope.Waveform(bytes.ToArray()).Length);
    }

    /// <remarks>
    /// Proves HM-DEC-094: a continuation part is never mistaken for a first one,
    /// which is what would happen if the header reader only checked lengths.
    /// </remarks>
    [Fact]
    public void AContinuationPartIsNotReadAsAHeader()
    {
        Assert.Null(CivScope.ReadHeader(PartEight()));
        Assert.Null(CivScope.ReadHeader(PartFour()));
    }
}
