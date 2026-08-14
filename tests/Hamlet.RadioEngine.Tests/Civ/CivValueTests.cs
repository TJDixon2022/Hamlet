using Hamlet.RadioEngine.Civ;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Civ;

/// <summary>
/// Turning CI-V bytes into the values they stand for, against the Full Manual's
/// own tables (HM-DEC-049, HM-DEC-050).
/// </summary>
public sealed class CivValueTests
{
    /// <remarks>
    /// Proves the two-byte level scale is read as decimal digits rather than as
    /// a plain number. The IC-7300 writes 128 as 01 28, so reading the low byte
    /// as 0x28 would put the CW pitch at 500 Hz when the radio says 600. The
    /// three anchors are the manual's own (p. 19-3).
    /// </remarks>
    [Theory]
    [InlineData(0x00, 0x00, 0)]
    [InlineData(0x01, 0x28, 128)]
    [InlineData(0x02, 0x55, 255)]
    [InlineData(0x00, 0x99, 99)]
    [InlineData(0x01, 0x00, 100)]
    public void ATwoByteLevelIsReadAsDecimalDigits(byte high, byte low, int expected)
        => Assert.Equal(expected, CivValues.Level(high, low));

    /// <remarks>
    /// Proves a byte that is not valid BCD is refused rather than rounded into
    /// something plausible. A garbled frame must not become a confident number.
    /// </remarks>
    [Theory]
    [InlineData(0x0A, 0x00)]
    [InlineData(0x00, 0xFF)]
    public void ALevelThatIsNotValidBcdIsRefused(byte high, byte low)
        => Assert.Null(CivValues.Level(high, low));

    /// <remarks>
    /// Proves every documented mode byte maps to the name the radio's own
    /// display shows (p. 19-9). The mode badge is the app's most-read surface
    /// and it used to be hardcoded to "CW".
    /// </remarks>
    [Theory]
    [InlineData(0x00, "LSB")]
    [InlineData(0x01, "USB")]
    [InlineData(0x02, "AM")]
    [InlineData(0x03, "CW")]
    [InlineData(0x04, "RTTY")]
    [InlineData(0x05, "FM")]
    [InlineData(0x07, "CW-R")]
    [InlineData(0x08, "RTTY-R")]
    public void EveryDocumentedModeByteHasItsName(byte value, string expected)
    {
        var mode = CivValues.Mode(value);

        Assert.NotNull(mode);
        Assert.Equal(expected, CivValues.Name(mode!.Value));
    }

    /// <remarks>
    /// Proves an undocumented byte produces nothing rather than a nearest match.
    /// 06 is absent from the manual's table, and a badge showing the wrong mode
    /// is the prime directive broken where it is read most (§0.0).
    /// </remarks>
    [Theory]
    [InlineData(0x06)]
    [InlineData(0x09)]
    [InlineData(0xFF)]
    public void AnUndocumentedModeByteIsRefused(byte value)
        => Assert.Null(CivValues.Mode(value));

    /// <remarks>
    /// Proves the filter designator is read rather than assumed. It used to read
    /// FIL2 whatever the radio was doing.
    /// </remarks>
    [Theory]
    [InlineData(0x01, "FIL1")]
    [InlineData(0x02, "FIL2")]
    [InlineData(0x03, "FIL3")]
    public void EveryFilterByteHasItsName(byte value, string expected)
        => Assert.Equal(expected, CivValues.FilterName(value));

    /// <remarks>
    /// Proves an unknown filter byte is refused.
    /// </remarks>
    [Fact]
    public void AnUnknownFilterByteIsRefused()
        => Assert.Null(CivValues.FilterName(0x04));

    /// <remarks>
    /// THE NUMBER THAT COST AN EVENING. The manual's endpoints for the
    /// non-AM scale are 00=50 Hz and 31/40=2700 Hz/3600 Hz (p. 19-3), and the
    /// step sizes that join them are on p. 4-6: 50 Hz apart up to 500 Hz, then
    /// 100 Hz apart. Every anchor the manual states is checked here, because
    /// this arithmetic is what turns an index into the figure an operator can
    /// act on.
    /// </remarks>
    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 100)]
    [InlineData(9, 500)]
    [InlineData(10, 600)]
    [InlineData(31, 2700)]
    [InlineData(40, 3600)]
    public void TheFilterWidthScaleMatchesTheManualsAnchors(int index, int expected)
    {
        Assert.Equal(expected, CivFilterWidth.Hertz(index, CivMode.Cw));
        Assert.Equal(expected, CivFilterWidth.Hertz(index, CivMode.Usb));
    }

    /// <remarks>
    /// Proves AM has its own scale: 200 Hz to 10 kHz in 200 Hz steps (p. 4-6).
    /// Reading an AM index on the SSB scale would report 2.4 kHz as 600 Hz.
    /// </remarks>
    [Theory]
    [InlineData(0, 200)]
    [InlineData(1, 400)]
    [InlineData(49, 10_000)]
    public void AmHasItsOwnFilterWidthScale(int index, int expected)
        => Assert.Equal(expected, CivFilterWidth.Hertz(index, CivMode.Am));

    /// <remarks>
    /// Proves FM's width does not come from this command at all. Its passband is
    /// not adjustable, so the index says nothing about it and the width comes
    /// from which filter is selected (p. 4-6).
    /// </remarks>
    [Fact]
    public void FmTakesItsWidthFromTheFilterSlotRatherThanTheIndex()
    {
        Assert.Null(CivFilterWidth.Hertz(10, CivMode.Fm));

        Assert.Equal(15_000, CivFilterWidth.FixedFmHertz("FIL1"));
        Assert.Equal(10_000, CivFilterWidth.FixedFmHertz("FIL2"));
        Assert.Equal(7_000, CivFilterWidth.FixedFmHertz("FIL3"));
        Assert.Null(CivFilterWidth.FixedFmHertz("FIL4"));
    }

    /// <remarks>
    /// Proves an index outside the documented range is refused rather than
    /// extrapolated.
    /// </remarks>
    [Theory]
    [InlineData(-1)]
    [InlineData(50)]
    public void AFilterIndexOutsideTheDocumentedRangeIsRefused(int index)
        => Assert.Null(CivFilterWidth.Hertz(index, CivMode.Cw));

    /// <remarks>
    /// Proves a bandwidth is spoken the way an operator says it rather than as a
    /// raw count of hertz.
    /// </remarks>
    [Theory]
    [InlineData(500, "500 Hz")]
    [InlineData(2400, "2.4 kHz")]
    [InlineData(3600, "3.6 kHz")]
    [InlineData(10_000, "10 kHz")]
    public void ABandwidthIsSpokenTheWayAnOperatorSaysIt(int hertz, string expected)
        => Assert.Equal(expected, CivFilterWidth.Describe(hertz));

    /// <remarks>
    /// Proves the S-meter reads against the three points the manual documents
    /// and nothing more (p. 19-3). Between them the scale is a straight line,
    /// which is what the radio's own display draws.
    /// </remarks>
    [Theory]
    [InlineData(0, "S0")]
    [InlineData(120, "S9")]
    [InlineData(241, "S9+60")]
    public void TheSMeterMatchesTheManualsAnchors(int reading, string expected)
        => Assert.Equal(expected, CivSMeter.Describe(reading));

    /// <remarks>
    /// Proves the meter reads sensibly between its anchors and never claims
    /// precision it does not have. An S-meter is a relative indication whose
    /// absolute accuracy the manual never claims, which is why nothing converts
    /// it to microvolts (§0.0).
    /// </remarks>
    [Fact]
    public void TheSMeterStaysInsideItsOwnScale()
    {
        for (var reading = 0; reading <= 255; reading++)
        {
            var text = CivSMeter.Describe(reading);
            var fraction = CivSMeter.Fraction(reading);

            Assert.StartsWith("S", text, StringComparison.Ordinal);
            Assert.InRange(fraction, 0, 1);
            Assert.DoesNotContain("dBm", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("uV", text, StringComparison.OrdinalIgnoreCase);
        }

        // Rising readings never fall on the meter.
        var previous = -1.0;
        for (var reading = 0; reading <= 241; reading++)
        {
            var fraction = CivSMeter.Fraction(reading);
            Assert.True(fraction >= previous, $"the meter went backwards at {reading}");
            previous = fraction;
        }
    }
}
