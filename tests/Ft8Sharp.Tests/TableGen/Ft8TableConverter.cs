using System.Text;

namespace Ft8Sharp.Tests.TableGen;

/// <summary>One table this port carries across, and the shape it is expected to have.</summary>
/// <param name="CIdentifier">The identifier as upstream spells it.</param>
/// <param name="CSharpName">The property it becomes on <c>Ft8Sharp.Ft8Tables</c>.</param>
/// <param name="ExpectedElements">
/// What the element count must come to. This is a cross-check and never a source: the
/// converter counts what it parsed and then compares, so a file that has changed shape
/// fails by name instead of being quietly padded or truncated to fit.
/// </param>
/// <param name="Summary">What the table is for, in one sentence, for the generated file.</param>
public sealed record TableSpec(string CIdentifier, string CSharpName, int ExpectedElements, string Summary);

/// <summary>The result of one conversion: what was parsed, and the file it would be written as.</summary>
public sealed record ConversionResult(
    IReadOnlyList<ParsedTable> Tables,
    string GeneratedSource,
    IReadOnlyList<string> UnresolvedDimensions,
    int LdpcM,
    int LdpcN,
    int LdpcKBytes,
    int LdpcNmRowWidth,
    int LdpcMnRowWidth)
{
    public ParsedTable this[string identifier] => Tables.Single(t => t.Identifier == identifier);
}

/// <summary>
/// Converts the six FT8 protocol tables out of <c>ft8/constants.c</c> into the one
/// generated C# file this repository checks in.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the only legal route for a table value into this repository.</b> Not
/// transcription, not a paste from a terminal, not a value read out of a report — a tool
/// checked in beside the result, so that anybody can run it against the pinned commit and
/// diff what falls out.
/// </para>
/// <para>
/// <b>The three FT4-only tables are deliberately not converted.</b>
/// <c>kFT4_Costas_pattern</c>, <c>kFT4_Gray_map</c> and <c>kFT4_XOR_sequence</c> are in
/// the same file and are left in it: FT4 is parked, and a converter that emits everything
/// it finds would put an unused, unproven table into a published library.
/// </para>
/// </remarks>
public static class Ft8TableConverter
{
    /// <summary>The upstream commit everything here is converted from.</summary>
    public const string UpstreamCommit = "9fec6ca39886edbf96f4f5e71edc76da5074e871";

    /// <summary>The one file tables may come from.</summary>
    public const string SourceFile = "ft8/constants.c";

    /// <summary>The header the dimension macros are corroborated against.</summary>
    public const string HeaderFile = "ft8/constants.h";

    /// <summary>The generated file, relative to the repository root.</summary>
    public const string GeneratedFileRelativePath = @"src\Ft8Sharp\Tables\Ft8Tables.g.cs";

    /// <summary>How a person re-runs this converter, quoted into the generated header.</summary>
    public const string RegenerateCommand =
        "dotnet test tests/Ft8Sharp.Tests -e FT8_TABLEGEN_WRITE=1";

    /// <summary>The six tables, in the order they are emitted.</summary>
    public static readonly IReadOnlyList<TableSpec> Manifest = new[]
    {
        new TableSpec(
            "kFT8_Costas_pattern",
            "Ft8CostasPattern",
            7,
            "The seven-tone Costas array FT8 synchronises on, sent three times in every transmission."),
        new TableSpec(
            "kFT8_Gray_map",
            "Ft8GrayMap",
            8,
            "The Gray code that maps a three-bit symbol value onto one of the eight tones."),
        new TableSpec(
            "kFTX_LDPC_generator",
            "LdpcGenerator",
            996,
            "The LDPC(174,91) generator matrix, LdpcM rows of LdpcKBytes bytes, most significant bit first."),
        new TableSpec(
            "kFTX_LDPC_Nm",
            "LdpcNm",
            581,
            "For each of the LdpcM checks, the variable nodes it covers, padded with zero out to LdpcNmRowWidth."),
        new TableSpec(
            "kFTX_LDPC_Mn",
            "LdpcMn",
            522,
            "For each of the LdpcN variables, the LdpcMnRowWidth checks it takes part in."),
        new TableSpec(
            "kFTX_LDPC_Num_rows",
            "LdpcNumRows",
            83,
            "How many of each Nm row's entries are real rather than padding."),
    };

    /// <summary>
    /// Parses the six tables, corroborates their geometry, and returns the generated file's
    /// text without writing anything.
    /// </summary>
    /// <param name="constantsSource">The contents of <c>ft8/constants.c</c>.</param>
    /// <param name="headerSource">
    /// The contents of <c>ft8/constants.h</c>, or null. Macro dimensions are resolved from
    /// the initialiser's own structure either way; the header is corroboration, and a header
    /// that cannot be read is reported rather than failed.
    /// </param>
    public static ConversionResult Convert(string constantsSource, string? headerSource)
    {
        IReadOnlyDictionary<string, long> macros = headerSource is null
            ? new Dictionary<string, long>(StringComparer.Ordinal)
            : CSourceParser.ParseIntegerMacros(headerSource);

        var unresolved = new List<string>();
        var tables = new List<ParsedTable>();

        foreach (var spec in Manifest)
        {
            var table = CSourceParser.ParseArray(constantsSource, spec.CIdentifier);

            if (table.ElementCount != spec.ExpectedElements)
            {
                throw new TableConversionException(
                    spec.CIdentifier,
                    $"the initialiser parsed to {table.ElementCount} elements and this port expects "
                    + $"{spec.ExpectedElements}. The count comes from the parse and the expectation "
                    + "from the manifest, and they are meant to agree: either the source has moved "
                    + "off the pin or the parser has mis-read it.");
            }

            CSourceParser.CrossCheckDimensions(table, macros, unresolved);
            tables.Add(table);
        }

        var generator = tables.Single(t => t.Identifier == "kFTX_LDPC_generator");
        var nm = tables.Single(t => t.Identifier == "kFTX_LDPC_Nm");
        var mn = tables.Single(t => t.Identifier == "kFTX_LDPC_Mn");
        var numRows = tables.Single(t => t.Identifier == "kFTX_LDPC_Num_rows");

        RequireTwoDimensional(generator);
        RequireTwoDimensional(nm);
        RequireTwoDimensional(mn);

        var ldpcM = generator.Shape[0];
        var ldpcKBytes = generator.Shape[1];
        var ldpcN = mn.Shape[0];

        if (nm.Shape[0] != ldpcM)
        {
            throw new TableConversionException(
                nm.Identifier,
                $"it has {nm.Shape[0]} rows where the generator has {ldpcM}. Both are indexed by the "
                + "check number, so they cannot have different row counts.");
        }

        if (numRows.ElementCount != ldpcM)
        {
            throw new TableConversionException(
                numRows.Identifier,
                $"it has {numRows.ElementCount} entries where the generator has {ldpcM} rows. It "
                + "carries one length per check, so the two are the same number by construction.");
        }

        var result = new ConversionResult(
            tables,
            string.Empty,
            unresolved,
            ldpcM,
            ldpcN,
            ldpcKBytes,
            nm.Shape[1],
            mn.Shape[1]);

        return result with { GeneratedSource = Emit(result) };
    }

    /// <summary>Normalises line endings and nothing else, for comparing two versions of the file.</summary>
    /// <remarks>
    /// <c>.gitattributes</c> says nothing about <c>*.cs</c>, so whether the working copy holds
    /// LF or CRLF is a property of one machine's <c>core.autocrlf</c> rather than of the port.
    /// A regeneration test that went red over that would teach the next reader to distrust it,
    /// so endings are normalised — and nothing else is, because everything else is the proof.
    /// </remarks>
    public static string NormaliseLineEndings(string text)
        => text.Replace("\r\n", "\n").Replace("\r", "\n");

    private static void RequireTwoDimensional(ParsedTable table)
    {
        if (table.Shape.Count != 2)
        {
            throw new TableConversionException(
                table.Identifier,
                $"it parsed as {table.Shape.Count}-dimensional and the port needs it two-dimensional "
                + "to derive a stride from it.");
        }
    }

    private static string Emit(ConversionResult result)
    {
        var text = new StringBuilder();
        void Line(string s = "") => text.Append(s).Append('\n');

        Line("// <auto-generated>");
        Line("//");
        Line("//     DO NOT EDIT BY HAND.");
        Line("//");
        Line("//     Every byte in this file was read out of the upstream C source named below by");
        Line("//     the tool named below. Nothing here was transcribed, pasted or corrected by a");
        Line("//     person, and a hand edit here would be undetectable afterwards -- which is why");
        Line("//     the regeneration check exists rather than a code review of the values.");
        Line("//");
        Line($"//     upstream    ft8_lib, commit {UpstreamCommit}");
        Line($"//     source      {SourceFile}");
        Line("//     tool        Ft8Sharp.Tests.TableGen.Ft8TableConverter");
        Line($"//     regenerate  {RegenerateCommand}");
        Line("//     proven by   Ft8TableGenerationTests.CheckedInTablesAreWhatTheConverterProduces");
        Line("//");
        Line("//     There is deliberately no generation time and no machine name in this header.");
        Line("//     What this file exists to prove is that converting the pinned clone again");
        Line("//     produces the same bytes, and a clock would make it differ from itself on every");
        Line("//     run, which would destroy exactly that proof.");
        Line("//");
        Line("//     The three FT4-only tables in the same source -- kFT4_Costas_pattern,");
        Line("//     kFT4_Gray_map and kFT4_XOR_sequence -- are deliberately not converted. FT4 is");
        Line("//     parked, and an unused table in a published library is a liability.");
        Line("//");
        Line("// </auto-generated>");
        Line();
        Line("using System;");
        Line();
        Line("namespace Ft8Sharp;");
        Line();
        Line("/// <summary>");
        Line("/// The FT8 protocol tables, machine-converted from the pinned ft8_lib clone.");
        Line("/// </summary>");
        Line("/// <remarks>");
        Line("/// Values are flattened row-major behind the stride constants below, so a row is");
        Line("/// addressable without a magic number: row <c>m</c> of the generator is");
        Line("/// <c>LdpcGenerator.Slice(m * LdpcKBytes, LdpcKBytes)</c>. Index bases are upstream's");
        Line("/// and are not renumbered -- see porting-notes.md.");
        Line("/// </remarks>");
        Line("public static class Ft8Tables");
        Line("{");
        Line("    /// <summary>The upstream commit every table here was converted from.</summary>");
        Line($"    public const string UpstreamCommit = \"{UpstreamCommit}\";");
        Line();
        Line("    /// <summary>The number of LDPC check nodes: rows of the generator, of Nm and of Num_rows.</summary>");
        Line($"    public const int LdpcM = {result.LdpcM};");
        Line();
        Line("    /// <summary>The number of LDPC variable nodes: the codeword length in bits, and the rows of Mn.</summary>");
        Line($"    public const int LdpcN = {result.LdpcN};");
        Line();
        Line("    /// <summary>The bytes per generator row, one row per check node.</summary>");
        Line($"    public const int LdpcKBytes = {result.LdpcKBytes};");
        Line();
        Line("    /// <summary>The width of one Nm row, padding included.</summary>");
        Line($"    public const int LdpcNmRowWidth = {result.LdpcNmRowWidth};");
        Line();
        Line("    /// <summary>The width of one Mn row.</summary>");
        Line($"    public const int LdpcMnRowWidth = {result.LdpcMnRowWidth};");

        foreach (var spec in Manifest)
        {
            var table = result[spec.CIdentifier];
            Line();
            Line($"    /// <summary>{spec.Summary}</summary>");
            Line($"    /// <remarks>{spec.CIdentifier}{DescribeShape(table)}, {table.ElementCount} elements.</remarks>");
            Line($"    public static ReadOnlySpan<byte> {spec.CSharpName} => new byte[]");
            Line("    {");
            foreach (var line in Rows(table))
            {
                Line($"        {line}");
            }

            Line("    };");
        }

        Line("}");
        return text.ToString();
    }

    private static string DescribeShape(ParsedTable table)
        => " [" + string.Join("][", table.Shape) + "]";

    /// <summary>
    /// One source line per table row where the table has rows, sixteen values to the line where
    /// it does not. Layout only — the values are whatever was parsed, in the order it parsed them.
    /// </summary>
    private static IEnumerable<string> Rows(ParsedTable table)
    {
        var perLine = table.Shape.Count == 2 ? table.Shape[1] : 16;
        for (var offset = 0; offset < table.Values.Length; offset += perLine)
        {
            var take = Math.Min(perLine, table.Values.Length - offset);
            var line = new StringBuilder();
            for (var i = 0; i < take; i++)
            {
                line.Append("0x").Append(table.Values[offset + i].ToString("X2")).Append(',');
                if (i + 1 < take)
                {
                    line.Append(' ');
                }
            }

            yield return line.ToString();
        }
    }
}
