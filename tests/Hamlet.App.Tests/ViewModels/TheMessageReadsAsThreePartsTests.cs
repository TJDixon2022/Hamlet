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
    /// <remarks>
    /// **THE WORDING CHANGED IN UNIT 251 AND THE TABLE DID NOT.** Every row here
    /// is the same closed vocabulary; what moved is that each sentence is built
    /// from the message's own two callsigns instead of addressing the reader.
    /// </remarks>
    [Theory]
    [InlineData(
        "W4WTM TA3MPK RRR",
        "TA3MPK is telling W4WTM that everything came through.")]
    [InlineData(
        "W4WTM TA3MPK RR73",
        "TA3MPK is telling W4WTM that everything came through, and is signing off with best regards.")]
    [InlineData(
        "W4WTM TA3MPK 73",
        "TA3MPK is signing off to W4WTM with best regards.")]
    [InlineData(
        "W4WTM TA3MPK EM74",
        "TA3MPK is telling W4WTM which grid square they are transmitting from.")]
    [InlineData(
        "CQ TA3MPK KM39",
        "TA3MPK is calling anyone, and saying which grid square they are transmitting from.")]
    [InlineData(
        "CQ DX EA3QQ JN11",
        "EA3QQ is calling anyone, and saying which grid square they are transmitting from.")]
    [InlineData(
        "W4WTM TA3MPK -11",
        "TA3MPK hears W4WTM at -11 dB, and is sending that back as the signal report.")]
    [InlineData(
        "W4WTM TA3MPK +03",
        "TA3MPK hears W4WTM at +3 dB, and is sending that back as the signal report.")]
    [InlineData(
        "W4WTM TA3MPK R-05",
        "TA3MPK has W4WTM's message, and is answering with a report of its own: TA3MPK hears W4WTM at -5 dB.")]
    [InlineData(
        "W4WTM TA3MPK R+12",
        "TA3MPK has W4WTM's message, and is answering with a report of its own: TA3MPK hears W4WTM at +12 dB.")]
    public void EveryRowOfTheTableHasItsSentence(string message, string expected)
    {
        var said = Ft8Vocabulary.Explain(Ft8Vocabulary.Split(message));

        _output.WriteLine(message.PadRight(20) + " -> " + (said ?? "<nothing>"));

        Assert.Equal(expected, said);
    }

    /// <summary>Payloads off the table produce nothing at all.</summary>
    /// <remarks>
    /// **NOT A FALLBACK STRING.** Each of these is a real thing that comes off
    /// the air, and Hamlet has nothing ruled to say about any of them.
    /// </remarks>
    [Theory]
    [InlineData("W4WTM TA3MPK 599")]
    [InlineData("W4WTM TA3MPK QRZ")]
    [InlineData("W4WTM TA3MPK TNX")]
    [InlineData("W4WTM TA3MPK 5NN")]
    [InlineData("W4WTM TA3MPK KN4XYZ/P")]

    // **A COURTESY OR A REPORT WITH NO ADDRESSEE.** `CQ` is a call to anyone
    // rather than a station, so there is nobody for the roger to have reached or
    // for the report to be about. Neither shape is standard FT8, and both get the
    // same silence everything else off the list gets.
    [InlineData("CQ TA3MPK RR73")]
    [InlineData("CQ TA3MPK -11")]
    [InlineData("CQ DX TA3MPK R+07")]
    public void APayloadOffTheTableGetsNoTooltip(string message)
    {
        var said = Ft8Vocabulary.Explain(Ft8Vocabulary.Split(message));

        _output.WriteLine(message.PadRight(22) + " -> " + (said ?? "<nothing>"));

        Assert.Null(said);
    }

    /// <summary>A message with no three fields explains nothing.</summary>
    [Fact]
    public void AMessageWithNoThreeFieldsExplainsNothing()
    {
        Assert.Null(Ft8Vocabulary.Explain(null));
        Assert.Null(Ft8Vocabulary.Explain(Ft8Vocabulary.Split("TNX FER QSO OM")));
        Assert.Null(Ft8Vocabulary.Explain(Ft8Vocabulary.Split("")));
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
        var said = Ft8Vocabulary.Explain(
            Ft8Vocabulary.Split("W4WTM TA3MPK " + grid));

        _output.WriteLine(grid + " -> " + said);

        Assert.Equal(
            "TA3MPK is telling W4WTM which grid square they are transmitting from.",
            said);

        // The sentence names no country, state or town, does not vary with the
        // grid, and does not even repeat the four characters - so there is
        // nothing in it a place name could be grown on the end of.
        Assert.DoesNotContain(grid, said, StringComparison.OrdinalIgnoreCase);

        foreach (var place in new[]
        {
            "Kentucky", "Spain", "Turkey", "Canada", "England", "United States",
            "Europe", "Asia", "North", "South", "East", "West",
        })
        {
            Assert.DoesNotContain(place, said, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// A report between two other stations says so, and does not put the reader
    /// in the middle of it.
    /// </summary>
    /// <remarks>
    /// <para>**THE FAULT, FROM THE OPERATOR'S OWN SCREEN, 2026-09-05.**
    /// `KE9COB N5CH R+14` is N5CH answering KE9COB. The operator is not in that
    /// message anywhere, and the tooltip read *roger, and hears you at 14 dB* -
    /// a sentence about a contact he was not part of, worded as though it were
    /// his own signal being reported.</para>
    /// <para>**IT ASSERTS THE ABSENCE AND NOT ONLY THE PRESENCE.** Checking that
    /// both callsigns appear would pass on a sentence that named them and then
    /// went on to say *you*; what makes this the right test is that the word is
    /// not there at all, and neither is the operator's own callsign.</para>
    /// </remarks>
    [Theory]
    [InlineData("KE9COB N5CH R+14")]
    [InlineData("KE9COB N5CH -09")]
    [InlineData("KE9COB N5CH RR73")]
    [InlineData("KE9COB N5CH EM66")]
    public void AReportBetweenTwoOtherStationsDoesNotMentionTheOperator(string message)
    {
        var said = Ft8Vocabulary.Explain(Ft8Vocabulary.Split(message));

        _output.WriteLine(message + " -> " + said);

        Assert.NotNull(said);

        Assert.Contains("KE9COB", said, StringComparison.Ordinal);
        Assert.Contains("N5CH", said, StringComparison.Ordinal);

        // **NO CALLSIGN BUT THE MESSAGE'S OWN TWO, WHICH IS STRONGER THAN
        // NAMING HIS AND CHECKING FOR IT.** This class does not know the
        // operator's callsign and must not need to: `Explain` is given three
        // fields and nothing else, so the only calls that can appear are the two
        // that came off the air. Anything else in the sentence shaped like a
        // callsign came from somewhere this function has no business reading.
        var calls = System.Text.RegularExpressions.Regex
            .Matches(said!, @"\b[A-Z0-9]*[0-9][A-Z]+\b")
            .Select(m => m.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        _output.WriteLine("  callsigns in the sentence: "
            + string.Join(", ", calls));

        Assert.All(
            calls,
            c => Assert.True(
                c is "KE9COB" or "N5CH",
                "the sentence names " + c + ", which is in neither field of "
                + message));

        // **THE WORD, NOT THE SUBSTRING.** `your` and `you` would both be
        // wrong here, and so would `You` at the start of a sentence; splitting
        // on non-letters catches every form of it without also catching a
        // callsign that happens to contain the letters.
        var words = said!
            .Split(
                new[] { ' ', ',', '.', ':', ';', '\'', '-' },
                StringSplitOptions.RemoveEmptyEntries);

        Assert.DoesNotContain(
            words,
            w => w.Equals("you", StringComparison.OrdinalIgnoreCase)
                 || w.Equals("your", StringComparison.OrdinalIgnoreCase)
                 || w.Equals("yours", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The roger and the report are both said, because `R+14` carries both.
    /// </summary>
    [Fact]
    public void ARogeredReportSaysTheRogerAsWellAsTheNumber()
    {
        var said = Ft8Vocabulary.Explain(Ft8Vocabulary.Split("KE9COB N5CH R+14"));

        _output.WriteLine(said);

        Assert.NotNull(said);

        // The roger half: N5CH has what KE9COB sent.
        Assert.Contains("N5CH has KE9COB's message", said, StringComparison.Ordinal);

        // And the report half, with its number and its sign.
        Assert.Contains("+14 dB", said, StringComparison.Ordinal);
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

        // Worded from the row's own two callsigns since unit 251, and the reader
        // is not one of them.
        Assert.Equal(
            "W4WTM is telling TA3MPK that everything came through, and is "
            + "signing off with best regards.",
            known.PayloadHelp);

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
