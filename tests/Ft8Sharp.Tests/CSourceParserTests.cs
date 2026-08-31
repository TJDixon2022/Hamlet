using Ft8Sharp.Tests.TableGen;
using Xunit;

namespace Ft8Sharp.Tests;

/// <summary>
/// The converter's refusals, watched on synthetic C rather than asserted in a comment.
/// </summary>
/// <remarks>
/// <para>
/// Every fixture here is invented C written for this file. None of it is upstream data, and
/// none of these values came out of <c>ft8/constants.c</c> — the shapes are the smallest that
/// exercise the behaviour.
/// </para>
/// <para>
/// These run without the reference clone, which is the point: they say what the converter does
/// when the file it is handed is not the file it expects. <b>Silence on a missing table is the
/// failure mode that would poison every later step</b>, so each refusal is checked for naming
/// the identifier it refused over.
/// </para>
/// </remarks>
public class CSourceParserTests
{
    [Fact]
    public void ParsesNestedBracesCommentsHexAndTrailingCommas()
    {
        const string source = """
            /* a block comment { with braces } and a comma, in it */
            const uint8_t kTest_table[3][4] = {
                { 0x01, 2, 0X03, 4, },   // a line comment } with a brace
                { 5, 0x06, 7, 8 },
                /* another */ { 9, 10, 11u, 12L, },
            };
            """;

        var table = CSourceParser.ParseArray(source, "kTest_table");

        Assert.Equal(new[] { 3, 4 }, table.Shape);
        Assert.Equal(12, table.ElementCount);
        Assert.Equal("[3][4]", table.DeclaredDimensions);
        Assert.Equal(Enumerable.Range(1, 12).Select(i => (byte)i).ToArray(), table.Values);
    }

    [Fact]
    public void ParsesASingleDimensionTable()
    {
        const string source = "static const uint8_t kOne_line[4] = { 0, 1, 2, 3 };";

        var table = CSourceParser.ParseArray(source, "kOne_line");

        Assert.Equal(new[] { 4 }, table.Shape);
        Assert.Equal(4, table.ElementCount);
    }

    [Fact]
    public void RefusesAMissingIdentifierByName()
    {
        const string source = "const uint8_t kSomething_else[2] = { 1, 2 };";

        var refusal = Assert.Throws<TableConversionException>(
            () => CSourceParser.ParseArray(source, "kNot_here"));

        Assert.Equal("kNot_here", refusal.Identifier);
        Assert.Contains("kNot_here", refusal.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RefusesAValueThatWillNotFitAByte()
    {
        const string source = "const uint8_t kTooBig[3] = { 1, 256, 3 };";

        var refusal = Assert.Throws<TableConversionException>(
            () => CSourceParser.ParseArray(source, "kTooBig"));

        Assert.Equal("kTooBig", refusal.Identifier);
        Assert.Contains("uint8_t", refusal.Message, StringComparison.Ordinal);

        // The position is named and the value is not: a refusal message is not a second route
        // for licensed data any more than a console line is.
        Assert.Contains("flat position 1", refusal.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("256", refusal.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RefusesRaggedRows()
    {
        const string source = "const uint8_t kRagged[2][3] = { { 1, 2, 3 }, { 4, 5 } };";

        var refusal = Assert.Throws<TableConversionException>(
            () => CSourceParser.ParseArray(source, "kRagged"));

        Assert.Equal("kRagged", refusal.Identifier);
        Assert.Contains("ragged", refusal.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RefusesADeclaredDimensionTheInitialiserContradicts()
    {
        const string source = "const uint8_t kShort[5] = { 1, 2, 3 };";
        var table = CSourceParser.ParseArray(source, "kShort");

        var refusal = Assert.Throws<TableConversionException>(
            () => CSourceParser.CrossCheckDimensions(
                table,
                new Dictionary<string, long>(),
                new List<string>()));

        Assert.Equal("kShort", refusal.Identifier);
        Assert.Contains("contradiction", refusal.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RefusesAMacroDimensionTheHeaderContradicts()
    {
        const string source = "const uint8_t kMacroSized[SOME_M][2] = { { 1, 2 }, { 3, 4 } };";
        var table = CSourceParser.ParseArray(source, "kMacroSized");

        var refusal = Assert.Throws<TableConversionException>(
            () => CSourceParser.CrossCheckDimensions(
                table,
                new Dictionary<string, long> { ["SOME_M"] = 9 },
                new List<string>()));

        Assert.Equal("kMacroSized", refusal.Identifier);
        Assert.Contains("SOME_M", refusal.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsRatherThanFailsOnAMacroTheHeaderDoesNotResolve()
    {
        const string source = "const uint8_t kMacroSized[SOME_M][2] = { { 1, 2 }, { 3, 4 } };";
        var table = CSourceParser.ParseArray(source, "kMacroSized");
        var unresolved = new List<string>();

        CSourceParser.CrossCheckDimensions(table, new Dictionary<string, long>(), unresolved);

        // A header that will not parse is a gap in the corroboration; a header that parses and
        // disagrees is a contradiction. They are different findings and are treated differently.
        Assert.Equal(new[] { "kMacroSized[0] = SOME_M" }, unresolved);
    }

    [Fact]
    public void ResolvesMacroArithmeticFromAHeader()
    {
        const string header = """
            #ifndef _INCLUDE_TEST_H_
            #define _INCLUDE_TEST_H_
            #define TEST_K       (91)
            #define TEST_K_BYTES ((TEST_K + 7) / 8)   // rounded up
            #define TEST_HEX     0x2A
            #define TEST_FN(a)   ((a) + 1)
            #define TEST_OPAQUE  sizeof(int)
            #endif
            """;

        var macros = CSourceParser.ParseIntegerMacros(header);

        Assert.Equal(91, macros["TEST_K"]);
        Assert.Equal(12, macros["TEST_K_BYTES"]);
        Assert.Equal(42, macros["TEST_HEX"]);
        Assert.False(macros.ContainsKey("TEST_FN"));
        Assert.False(macros.ContainsKey("TEST_OPAQUE"));
    }
}
