using Hamlet.RadioEngine.Contacts;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The splitter came down into the engine in unit 258 and divides messages
/// exactly as it did in the app.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** A move that quietly changed
/// what splits. The eight rows below are the survey's own table
/// (`docs/unit257-contact-state-survey.md` section 2), which was read off the
/// app's implementation before anything moved; if the engine's copy answers
/// differently on any of them, the ledger and the tooltip are now reading the
/// same message two ways and the move was not a move.</para>
/// <para>**AND THE SECOND COPY.** `Ft8Vocabulary.Split`, `Ft8Vocabulary.IsGrid`,
/// `Ft8Vocabulary.IsReport` and `DecodedFilter.IsCallToAnyone` are all one-line
/// forwards to this file now. Nothing asserts that from here - the engine may not
/// reach the app (0.1) - so this class asserts the engine half and the app's own
/// `TheMessageReadsAsThreePartsTests` continues to assert the app half against
/// the same rules through the forward.</para>
/// </remarks>
public sealed class TheSplitterMovedAndSplitsTheSameTests
{
    /// <summary>The shapes it takes and the shapes it refuses, unchanged.</summary>
    /// <param name="message">The message.</param>
    /// <param name="to">The addressee it must produce, or null for a refusal.</param>
    /// <param name="from">The sender it must produce.</param>
    /// <param name="payload">The payload it must produce.</param>
    [Theory]
    // Four words with a leading CQ: the call and its direction are ONE addressee.
    [InlineData("CQ DX W1ABC FN42", "CQ DX", "W1ABC", "FN42")]
    [InlineData("CQ POTA W5LST EM33", "CQ POTA", "W5LST", "EM33")]
    // Plainly three fields.
    [InlineData("K1ABC W9XYZ RR73", "K1ABC", "W9XYZ", "RR73")]
    [InlineData("CQ W1ABC FN42", "CQ", "W1ABC", "FN42")]
    // Wrongly-shaped but still three fields, which the format says is a split.
    [InlineData("HW CPY OM", "HW", "CPY", "OM")]
    // Refused: four words without a leading CQ, two fields, one field, nothing.
    [InlineData("TNX BOB 73 GL", null, null, null)]
    [InlineData("K1ABC W9XYZ", null, null, null)]
    [InlineData("ABCDEFGHIJKLM", null, null, null)]
    [InlineData("   ", null, null, null)]
    public void TheEngineSplitsWhatTheAppSplitAndRefusesWhatItRefused(
        string message, string? to, string? from, string? payload)
    {
        var fields = Ft8MessageSplit.Split(message);

        if (to is null)
        {
            Assert.Null(fields);
            return;
        }

        Assert.NotNull(fields);
        Assert.Equal(to, fields!.To);
        Assert.Equal(from, fields.From);
        Assert.Equal(payload, fields.Payload);
    }

    /// <summary>`CQ` is an addressee and never a station.</summary>
    /// <param name="to">The addressee field.</param>
    /// <param name="anyone">Whether it is a call to anyone.</param>
    [Theory]
    [InlineData("CQ", true)]
    [InlineData("CQ DX", true)]
    [InlineData("CQ POTA", true)]
    [InlineData("cq dx", true)]
    [InlineData("W1ABC", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CqInAnyOfItsFormsIsACallToAnyone(string? to, bool anyone)
        => Assert.Equal(anyone, Ft8MessageSplit.IsCallToAnyone(to));

    /// <summary>A grid is letters then digits, and `RR73` is not one.</summary>
    /// <param name="text">The payload.</param>
    /// <param name="grid">Whether it has a grid's shape.</param>
    [Theory]
    [InlineData("FN42", true)]
    [InlineData("IO91", true)]
    [InlineData("RR73", true)]
    [InlineData("-13", false)]
    [InlineData("RRR", false)]
    [InlineData("73", false)]
    public void AGridIsTwoLettersAndTwoDigits(string text, bool grid)
        => Assert.Equal(grid, Ft8MessageSplit.IsGrid(text));

    /// <summary>A report carries its sign, with or without a roger.</summary>
    /// <param name="text">The payload.</param>
    /// <param name="report">Whether it is a report.</param>
    /// <param name="rogered">Whether it carries a leading R.</param>
    /// <param name="decibels">The level it reports.</param>
    [Theory]
    [InlineData("-13", true, false, -13)]
    [InlineData("+05", true, false, 5)]
    [InlineData("R-09", true, true, -9)]
    [InlineData("R+14", true, true, 14)]
    [InlineData("RRR", false, false, 0)]
    [InlineData("RR73", false, false, 0)]
    [InlineData("73", false, false, 0)]
    [InlineData("FN42", false, false, 0)]
    public void AReportCarriesItsSign(
        string text, bool report, bool rogered, int decibels)
    {
        Assert.Equal(report, Ft8MessageSplit.IsReport(text, out var r, out var db));

        if (!report)
        {
            return;
        }

        Assert.Equal(rogered, r);
        Assert.Equal(decibels, db);
    }
}
