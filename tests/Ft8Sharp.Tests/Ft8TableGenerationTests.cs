using Ft8Sharp.Tests.TableGen;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests;

/// <summary>
/// Asks the question criterion 4 turns on: are the tables checked into this repository the
/// tables the converter produces from the pinned clone today?
/// </summary>
/// <remarks>
/// <para>
/// A generated file sitting in <c>src/Ft8Sharp/Tables/</c> proves nothing on its own. Anybody
/// could have hand-edited a byte in it, and nobody would ever find out. What makes the
/// conversion mean something is this test: parse the clone again, emit again into memory, and
/// compare. When the pin eventually moves, this goes red and names the table that changed,
/// instead of the port quietly acquiring a second provenance.
/// </para>
/// <para>
/// <b>Shapes and match-or-not only.</b> Counts, dimensions, identifiers and how many positions
/// differ are metadata and are free. No message here prints a table value, not one element and
/// not a checksum of a row — the bytes go to disk by machine and are read by nobody.
/// </para>
/// </remarks>
public class Ft8TableGenerationTests
{
    private readonly ITestOutputHelper _output;

    public Ft8TableGenerationTests(ITestOutputHelper output) => _output = output;

    [RequiresReferenceCloneFact]
    public void CheckedInTablesAreWhatTheConverterProduces()
    {
        var result = ConvertFromTheClone();

        _output.WriteLine($"{"C identifier",-24} {"dimensions",-14} elements");
        foreach (var spec in Ft8TableConverter.Manifest)
        {
            var table = result[spec.CIdentifier];
            _output.WriteLine(
                $"{table.Identifier,-24} {"[" + string.Join("][", table.Shape) + "]",-14} {table.ElementCount}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"derived geometry        : LdpcM={result.LdpcM} LdpcN={result.LdpcN} "
            + $"LdpcKBytes={result.LdpcKBytes} NmRowWidth={result.LdpcNmRowWidth} "
            + $"MnRowWidth={result.LdpcMnRowWidth}");
        _output.WriteLine(result.UnresolvedDimensions.Count == 0
            ? $"header cross-check      : every declared dimension resolved against {Ft8TableConverter.HeaderFile}"
            : "header cross-check      : unresolved — " + string.Join(", ", result.UnresolvedDimensions));

        var path = RepositoryTree.GeneratedTablesFile;
        Assert.True(
            File.Exists(path),
            $"{path} is not there. The converter runs and the tables parse, but nothing is checked "
            + $"in for it to be compared against. Run: {Ft8TableConverter.RegenerateCommand}");

        var checkedIn = File.ReadAllText(path);
        var comparison = TableComparison.Compare(result, checkedIn);

        _output.WriteLine(string.Empty);
        _output.WriteLine($"checked-in file         : {path}");
        _output.WriteLine($"characters produced     : {result.GeneratedSource.Length}");
        _output.WriteLine($"characters checked in   : {checkedIn.Length}");
        _output.WriteLine($"byte-identical          : {comparison.Identical}");

        if (comparison.Identical)
        {
            return;
        }

        Assert.Fail(
            "The tables checked in are not the tables the converter produces from "
            + $"{Ft8TableConverter.SourceFile} at commit {Ft8TableConverter.UpstreamCommit}.\n  "
            + string.Join("\n  ", comparison.Differences)
            + $"\nIf upstream has moved deliberately, re-run: {Ft8TableConverter.RegenerateCommand}");
    }

    /// <summary>
    /// Watches the comparison refuse. A guard nobody has seen refuse is an assumption.
    /// </summary>
    /// <remarks>
    /// One element of one table is changed <b>in memory, by machine</b> — the checked-in file is
    /// never touched, nothing is hand-edited, and the altered value is neither chosen nor printed.
    /// What is asserted is that the refusal names the table that changed, counts the positions,
    /// stays quiet about the other five tables, and carries no value in its message.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheComparisonRefusesAFileWithOneAlteredElement()
    {
        var result = ConvertFromTheClone();
        var altered = AlterOneElementOf("LdpcNm", result.GeneratedSource);
        Assert.NotEqual(result.GeneratedSource, altered);

        var comparison = TableComparison.Compare(result, altered);

        _output.WriteLine($"identical               : {comparison.Identical}");
        foreach (var difference in comparison.Differences)
        {
            _output.WriteLine($"reported                : {difference}");
        }

        Assert.False(comparison.Identical);
        var reported = Assert.Single(comparison.Differences);
        Assert.Equal("kFTX_LDPC_Nm: differs at 1 of 581 positions.", reported);
        Assert.DoesNotContain("0x", reported, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Changes the first element of one emitted table to a different byte, in a copy of the text.
    /// The replacement is arithmetic on what was there, so no value is chosen by anybody.
    /// </summary>
    private static string AlterOneElementOf(string propertyName, string generatedSource)
    {
        var marker = $"public static ReadOnlySpan<byte> {propertyName} => new byte[]";
        var start = generatedSource.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"{propertyName} is not in the generated source.");

        var element = generatedSource.IndexOf("0x", start, StringComparison.Ordinal);
        Assert.True(element >= 0, $"{propertyName} has no elements in the generated source.");

        var was = System.Convert.ToByte(generatedSource.Substring(element + 2, 2), 16);
        var now = (byte)((was + 1) % 256);
        return string.Concat(
            generatedSource.AsSpan(0, element + 2),
            now.ToString("X2"),
            generatedSource.AsSpan(element + 4));
    }

    /// <summary>
    /// Rewrites the checked-in generated file. Skipped unless asked for by environment variable,
    /// because a test that edits the source tree every time it runs is a trap.
    /// </summary>
    [TableGenWriteFact]
    public void RewriteTheCheckedInTablesFile()
    {
        var result = ConvertFromTheClone();
        var path = RepositoryTree.GeneratedTablesFile;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, result.GeneratedSource);

        _output.WriteLine($"wrote                   : {path}");
        _output.WriteLine($"characters              : {result.GeneratedSource.Length}");
        _output.WriteLine($"tables                  : {result.Tables.Count}");
        _output.WriteLine($"elements                : {result.Tables.Sum(t => t.ElementCount)}");
    }

    private static ConversionResult ConvertFromTheClone()
    {
        var constantsPath = Path.Combine(ReferenceClone.Location, "ft8", "constants.c");
        var headerPath = Path.Combine(ReferenceClone.Location, "ft8", "constants.h");

        Assert.True(
            File.Exists(constantsPath),
            $"{constantsPath} is not there. Tables come from that file and from no other.");

        var header = File.Exists(headerPath) ? File.ReadAllText(headerPath) : null;
        return Ft8TableConverter.Convert(File.ReadAllText(constantsPath), header);
    }
}

/// <summary>
/// A fact that rewrites checked-in source, and so runs only when it is explicitly asked for.
/// </summary>
/// <remarks>
/// <c>dotnet run</c> is not available in this loop, so a test is the only executable surface a
/// converter can have. That makes the generator a test — but a generator that fires on every
/// <c>dotnet test</c> would rewrite the tree under anybody who ran the suite, and the
/// comparison it is meant to be checked by could never fail. So it is gated:
/// <c>dotnet test tests/Ft8Sharp.Tests -e FT8_TABLEGEN_WRITE=1</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TableGenWriteFactAttribute : FactAttribute
{
    public TableGenWriteFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FT8_TABLEGEN_WRITE") is not { Length: > 0 })
        {
            Skip = "This rewrites checked-in source and only runs when asked to. Run: "
                + Ft8TableConverter.RegenerateCommand;
        }
        else if (ReferenceClone.Probe(out var detail) != ReferenceClone.Reach.Reachable)
        {
            Skip = $"The pinned ft8_lib clone is not reachable at {ReferenceClone.Location}, so "
                + $"there is nothing to convert from. {detail}";
        }
    }
}
