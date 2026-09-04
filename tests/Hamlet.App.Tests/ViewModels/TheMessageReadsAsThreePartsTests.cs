using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 241, task 5: the message reads as three parts, and the
/// closed table explains the ones it knows.
/// </summary>
/// <remarks>
/// <para>**HOVER TEXT ON COMMON RESPONSES ONLY** (Tim's ruling, 2026-09-04).
/// Anything not on the list gets no tooltip at all - not a guess, not a partial
/// reading, not "unrecognised". Silence is the correct answer and not an error
/// state, the same way the `snr` dash is.</para>
/// <para>**THE HALF THAT IS EASY TO GET WRONG IS THE SILENCE.** A table that
/// explains everything it recognises is straightforward; one that says nothing
/// about a contest exchange, rather than guessing at it, is the part §0.0 is
/// actually asking for.</para>
/// </remarks>
public sealed class TheMessageReadsAsThreePartsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheMessageReadsAsThreePartsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every row of the ruled table produces its sentence.</summary>
    [Theory]
    [InlineData("CQ", "calling anyone")]
    [InlineData("CQ DX", "calling anyone, looking for distance")]
    [InlineData("RRR", "roger, everything received")]
    [InlineData("RR73", "roger, and best regards")]
    [InlineData("73", "best regards, contact finished")]
    [InlineData("EM74", "grid square: where he is")]
    [InlineData("FN42", "grid square: where he is")]
    [InlineData("-11", "signal report: hears you at -11 dB")]
    [InlineData("+03", "signal report: hears you at 3 dB")]
    [InlineData("R-05", "roger, and hears you at -5 dB")]
    [InlineData("R+12", "roger, and hears you at 12 dB")]
    public void EveryRowOfTheTableHasItsSentence(string payload, string expected)
    {
        var said = Ft8Vocabulary.Explain(payload);

        _output.WriteLine(payload.PadRight(6) + " -> " + (said ?? "<nothing>"));

        Assert.Equal(expected, said);
    }

    /// <summary>Payloads off the table produce nothing at all.</summary>
    /// <remarks>
    /// **NOT A FALLBACK STRING.** Each of these is a real thing that comes off
    /// the air, and Hamlet has nothing ruled to say about any of them.
    /// </remarks>
    [Theory]
    [InlineData("599")]
    [InlineData("QRZ")]
    [InlineData("TNX")]
    [InlineData("5NN")]
    [InlineData("KN4XYZ/P")]
    [InlineData("")]
    public void APayloadOffTheTableGetsNoTooltip(string payload)
    {
        var said = Ft8Vocabulary.Explain(payload);

        _output.WriteLine((payload.Length == 0 ? "<empty>" : payload).PadRight(10)
            + " -> " + (said ?? "<nothing>"));

        Assert.Null(said);
    }

    /// <summary>A grid square is never turned into a place.</summary>
    /// <remarks>
    /// **THE ONE INFERENCE THIS TABLE IS MOST TEMPTED INTO.** `EM66` is in
    /// Kentucky and Hamlet must not say so: naming a place from four characters
    /// is a fact asserted about a station from something that does not contain
    /// it (§0.0).
    /// </remarks>
    [Theory]
    [InlineData("EM66")]
    [InlineData("FN42")]
    [InlineData("JN11")]
    [InlineData("KM39")]
    [InlineData("CN89")]
    public void AGridNeverYieldsAPlaceName(string grid)
    {
        var said = Ft8Vocabulary.Explain(grid);

        _output.WriteLine(grid + " -> " + said);

        Assert.Equal("grid square: where he is", said);

        // The sentence names no country, state or town, and does not vary with
        // the grid: every square gets the same words.
        foreach (var place in new[]
        {
            "Kentucky", "Spain", "Turkey", "Canada", "England", "United States",
            "Europe", "Asia", "North", "South", "East", "West",
        })
        {
            Assert.DoesNotContain(place, said, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>A standard message splits into its three fields.</summary>
    [Theory]
    [InlineData("CQ TA3MPK KM39", "CQ", "TA3MPK", "KM39")]
    [InlineData("TA3MPK W4WTM EM74", "TA3MPK", "W4WTM", "EM74")]
    [InlineData("W4WTM TA3MPK R-05", "W4WTM", "TA3MPK", "R-05")]
    [InlineData("CQ DX EA3QQ JN11", "CQ DX", "EA3QQ", "JN11")]
    public void AStandardMessageSplitsIntoThree(
        string message, string to, string from, string payload)
    {
        var fields = Ft8Vocabulary.Split(message);

        _output.WriteLine(message + "  ->  [" + to + "] [" + from + "] [" + payload + "]");

        Assert.NotNull(fields);
        Assert.Equal(to, fields.To);
        Assert.Equal(from, fields.From);
        Assert.Equal(payload, fields.Payload);
    }

    /// <summary>
    /// A message that is not plainly three fields is left whole.
    /// </summary>
    /// <remarks>
    /// **LABELLING THE WRONG HALF IS WORSE THAN LABELLING NONE.** Free text and
    /// telemetry have no addressee and no sender, and splitting them at a space
    /// would put "who sent it" over something that is not a callsign.
    /// </remarks>
    [Theory]
    [InlineData("TNX FER QSO 73 GL")]
    [InlineData("K1ABC RR73")]
    [InlineData("HELLO WORLD")]
    [InlineData("0A1B2C3D4E5F6071")]
    [InlineData("")]
    public void AMessageWithoutThreeFieldsIsLeftWhole(string message)
    {
        var fields = Ft8Vocabulary.Split(message);

        _output.WriteLine((message.Length == 0 ? "<empty>" : message)
            + "  ->  " + (fields is null ? "<no fields>" : "split"));

        Assert.Null(fields);
    }

    /// <summary>The row shows one presentation or the other, never both.</summary>
    [Theory]
    [InlineData("CQ TA3MPK KM39", true)]
    [InlineData("TNX FER QSO 73 GL", false)]
    public void TheRowShowsTheFieldsOrTheWholeMessageButNotBoth(
        string message, bool expectFields)
    {
        var row = new DigitalDecodeRow(
            "214135", DigitalDecodeRow.NoMeasurement, "0.2", "1240", message);

        _output.WriteLine(message + "  hasFields " + row.HasFields
            + "  unsplit [" + row.Unsplit + "]");

        Assert.Equal(expectFields, row.HasFields);

        if (expectFields)
        {
            Assert.Equal("", row.Unsplit);
            Assert.NotEqual("", row.Addressee);
            Assert.NotEqual("", row.Sender);
        }
        else
        {
            Assert.Equal(message, row.Unsplit);
            Assert.Equal("", row.Addressee);
            Assert.Equal("", row.Sender);
        }
    }

    /// <summary>
    /// The row's payload tooltip is empty for a payload off the table, so
    /// nothing is shown.
    /// </summary>
    [Fact]
    public void ARowWithAnUnknownPayloadCarriesNoPayloadTooltip()
    {
        var known = new DigitalDecodeRow(
            "214135", DigitalDecodeRow.NoMeasurement, "0.2", "1240",
            "TA3MPK W4WTM RR73");

        var unknown = new DigitalDecodeRow(
            "214135", DigitalDecodeRow.NoMeasurement, "0.2", "1240",
            "TA3MPK W4WTM 599");

        _output.WriteLine("RR73 -> [" + known.PayloadHelp + "]");
        _output.WriteLine("599  -> [" + unknown.PayloadHelp + "]");

        Assert.True(known.HasPayloadHelp);
        Assert.Equal("roger, and best regards", known.PayloadHelp);

        Assert.False(unknown.HasPayloadHelp);
        Assert.Equal("", unknown.PayloadHelp);
    }

    /// <summary>Naming which field is which is structure and says nothing more.</summary>
    [Fact]
    public void TheCallsignTooltipsNameTheFieldAndNothingElse()
    {
        var calling = new DigitalDecodeRow(
            "214135", DigitalDecodeRow.NoMeasurement, "0.2", "1240",
            "CQ TA3MPK KM39");

        var answering = new DigitalDecodeRow(
            "214135", DigitalDecodeRow.NoMeasurement, "0.2", "1240",
            "TA3MPK W4WTM -11");

        _output.WriteLine("CQ row   to   [" + calling.AddresseeHelp + "]");
        _output.WriteLine("CQ row   from [" + calling.SenderHelp + "]");
        _output.WriteLine("reply to      [" + answering.AddresseeHelp + "]");

        Assert.Contains("addressed to", calling.AddresseeHelp, StringComparison.Ordinal);
        Assert.Contains("anyone", calling.AddresseeHelp, StringComparison.Ordinal);
        Assert.Equal("Who sent it.", calling.SenderHelp);

        // A directed reply is addressed to a station, and the tooltip does not
        // claim to know anything about that station.
        Assert.Equal("Who this is addressed to.", answering.AddresseeHelp);
    }
}
