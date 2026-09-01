using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The 15-bit field of the standard message that carries either a grid square, a signal report, or
/// one of four fixed tokens — with a one-bit <c>R</c> flag beside it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, functions <c>packgrid</c> and
/// <c>unpackgrid</c>. One boundary constant divides the field: below it the value is a grid, above
/// it the value less that boundary is a small code. The boundary is a named macro in the pin and
/// is asserted against it by machine.
/// </para>
/// <para>
/// <b>This field is small enough to finish rather than sample.</b> All 32 768 values are swept by
/// the tests, so there is no seed to state and no sampling argument to make.
/// </para>
/// <para>
/// <b>Two deliberate refusals where upstream returns text, and both are recorded in
/// <c>porting-notes.md</c>.</b>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>The boundary value itself is refused</b>, because both sub-ranges reach it: upstream's
///     unpacker takes it as the last grid square, while upstream's packer arrives at it from a
///     report of thirty-five below zero. The bits are genuinely ambiguous and upstream presents
///     one of the two readings as certain. HM-DEC-009 governs exactly that case, and nothing a
///     conforming transmitter sends lands here.
///   </description></item>
///   <item><description>
///     <b>A report code whose number will not fit two digits is refused as malformed</b>, because
///     upstream's fixed-width formatter emits a character that is not a digit for it — the text it
///     produces is not a report, it is not anything. Refusing is the same answer the caller would
///     want and it is reached deliberately rather than by printing punctuation into a callsign
///     line.
///   </description></item>
/// </list>
/// </remarks>
public static class Ft8GridField
{
    /// <summary>The width of the grid-and-report field.</summary>
    public const int Bits = 15;

    /// <summary>One past the largest value the field can hold: the size of the whole sweep.</summary>
    public const int Range = 1 << Bits;

    /// <summary>
    /// The boundary between the grid sub-range and the code sub-range: the number of distinct
    /// four-character grid squares the field admits.
    /// </summary>
    /// <remarks>Upstream's <c>MAXGRID4</c>. Asserted against the pin by machine.</remarks>
    public const int MaxGrid = 32400;

    /// <summary>The code, above <see cref="MaxGrid"/>, that means the message has no third field.</summary>
    public const int CodeNone = 1;

    /// <summary>The code for <c>RRR</c>.</summary>
    public const int CodeRrr = 2;

    /// <summary>The code for <c>RR73</c>.</summary>
    public const int CodeRr73 = 3;

    /// <summary>The code for <c>73</c>.</summary>
    public const int CodeSeventyThree = 4;

    /// <summary>The first code that carries a signal report rather than a fixed token.</summary>
    public const int FirstReportCode = 5;

    /// <summary>What is subtracted from a report code to get the report in decibels.</summary>
    public const int ReportOffset = 35;

    /// <summary>
    /// The largest report code whose number still fits the two digits upstream formats it into.
    /// </summary>
    /// <remarks>
    /// Derived, not chosen: upstream writes the report with a width of two, and a value of a
    /// hundred or more overruns that and produces a non-digit character. Everything above this is
    /// refused as malformed.
    /// </remarks>
    public const int LastReportCode = ReportOffset + 99;

    /// <summary>The bit that carries the <c>R</c> flag in the value <see cref="Pack"/> returns.</summary>
    /// <remarks>
    /// Upstream returns both in one sixteen-bit integer and the standard message's bit layout
    /// depends on that, shifting the flag into place along with the field. Kept as upstream has it
    /// rather than split into two returns, because splitting them is where the layout would drift.
    /// </remarks>
    public const int ReportFlag = 0x8000;

    /// <summary>
    /// Packs a grid square, a report or a token into the field, with the <c>R</c> flag in
    /// <see cref="ReportFlag"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Never throws</b>, for any string. Upstream's own fallthrough for text that is neither a
    /// grid nor a report is kept: it is read as a report, which yields the code for a report of
    /// zero when there are no digits in it. That is upstream's behaviour and a caller that hands
    /// this arbitrary text gets an arbitrary answer; the message layer above only ever hands it
    /// text that came out of <see cref="TryUnpack"/> or out of an operator's own field.
    /// </para>
    /// <para>
    /// <b>There is no route to an <c>R</c>-prefixed grid square.</b> Upstream's packer has none
    /// either, and says so in its own comment. The unpacker can produce one, so that asymmetry is
    /// upstream's and is reported rather than repaired — repairing it would be a change to the
    /// wire format, made at three in the morning, against a reference this port exists to match.
    /// </para>
    /// </remarks>
    public static int Pack(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return MaxGrid + CodeNone;
        }

        if (text == "RRR")
        {
            return MaxGrid + CodeRrr;
        }

        if (text == "RR73")
        {
            return MaxGrid + CodeRr73;
        }

        if (text == "73")
        {
            return MaxGrid + CodeSeventyThree;
        }

        if (text.Length >= 4
            && Ft8Text.InRange(text[0], 'A', 'R')
            && Ft8Text.InRange(text[1], 'A', 'R')
            && Ft8Text.IsDigit(text[2])
            && Ft8Text.IsDigit(text[3]))
        {
            var grid = text[0] - 'A';
            grid = (grid * 18) + (text[1] - 'A');
            grid = (grid * 10) + (text[2] - '0');
            grid = (grid * 10) + (text[3] - '0');
            return grid;
        }

        if (text[0] == 'R')
        {
            var db = Ft8Text.DdToInt(text[1..], 3);
            return ((MaxGrid + ReportOffset + db) & 0xFFFF) | ReportFlag;
        }

        var plain = Ft8Text.DdToInt(text, 3);
        return (MaxGrid + ReportOffset + plain) & 0xFFFF;
    }

    /// <summary>
    /// Unpacks the field back to a grid square, a report, a token, or nothing at all — or says why
    /// it cannot.
    /// </summary>
    /// <param name="value">The 15-bit field. Anything at or above <see cref="Range"/> is refused.</param>
    /// <param name="reportFlag">The <c>R</c> flag that sat beside it.</param>
    /// <param name="text">
    /// The decoded text, written only on <see cref="Ft8FieldResult.Ok"/>. It is the empty string
    /// where the field says the message has no third part, which is a success and not a refusal.
    /// </param>
    /// <param name="fieldType">What kind of thing was decoded, written only on success.</param>
    /// <remarks><b>Never throws, for any input.</b></remarks>
    public static Ft8FieldResult TryUnpack(
        int value,
        bool reportFlag,
        out string text,
        out Ft8FieldType fieldType)
    {
        text = string.Empty;
        fieldType = Ft8FieldType.Unknown;

        if (value < 0 || value >= Range)
        {
            return Ft8FieldResult.Malformed;
        }

        if (value < MaxGrid)
        {
            var n = value;
            Span<char> grid = stackalloc char[4];
            grid[3] = (char)('0' + (n % 10));
            n /= 10;
            grid[2] = (char)('0' + (n % 10));
            n /= 10;
            grid[1] = (char)('A' + (n % 18));
            n /= 18;
            grid[0] = (char)('A' + (n % 18));

            text = reportFlag ? "R " + grid.ToString() : grid.ToString();
            fieldType = Ft8FieldType.Grid;
            return Ft8FieldResult.Ok;
        }

        var code = value - MaxGrid;

        switch (code)
        {
            case 0:
                // The one value both sub-ranges claim. Refused rather than read as a grid square
                // that the transmitter may not have sent.
                return Ft8FieldResult.Malformed;

            case CodeNone:
                text = string.Empty;
                fieldType = Ft8FieldType.None;
                return Ft8FieldResult.Ok;

            case CodeRrr:
                text = "RRR";
                fieldType = Ft8FieldType.Token;
                return Ft8FieldResult.Ok;

            case CodeRr73:
                text = "RR73";
                fieldType = Ft8FieldType.Token;
                return Ft8FieldResult.Ok;

            case CodeSeventyThree:
                text = "73";
                fieldType = Ft8FieldType.Token;
                return Ft8FieldResult.Ok;

            default:
                if (code > LastReportCode)
                {
                    // Upstream's two-digit formatter cannot write this number and emits something
                    // that is not a digit. Refused as malformed.
                    return Ft8FieldResult.Malformed;
                }

                var report = Ft8Text.IntToDd(code - ReportOffset, 2, true);
                text = reportFlag ? "R" + report : report;
                fieldType = Ft8FieldType.Report;
                return Ft8FieldResult.Ok;
        }
    }
}
