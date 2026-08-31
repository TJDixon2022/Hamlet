namespace Ft8Sharp.Tests.TableGen;

/// <summary>What comparing a conversion against a checked-in file came to.</summary>
/// <param name="Identical">
/// Whether the checked-in file is what the converter produces, line endings normalised and
/// nothing else.
/// </param>
/// <param name="Differences">
/// One line per table that differs, naming the C identifier and how many positions differ.
/// <b>Never a value.</b> Empty when the two are identical.
/// </param>
public sealed record ComparisonReport(bool Identical, IReadOnlyList<string> Differences);

/// <summary>
/// Compares what the converter produces against what is checked in, and says which table
/// differs when they disagree.
/// </summary>
/// <remarks>
/// Two strings not being equal is a true answer and a useless one. When the pin eventually
/// moves, what the next reader needs is the name of the table that changed and how much of it
/// — so the comparison reads the byte arrays back out of the checked-in file and diffs them
/// table by table, and separately reports a difference that is in the header, the geometry
/// constants or the layout rather than in the data.
/// </remarks>
public static class TableComparison
{
    public static ComparisonReport Compare(ConversionResult produced, string checkedInSource)
    {
        var expected = Ft8TableConverter.NormaliseLineEndings(produced.GeneratedSource);
        var actual = Ft8TableConverter.NormaliseLineEndings(checkedInSource);
        if (expected == actual)
        {
            return new ComparisonReport(true, Array.Empty<string>());
        }

        var onDisk = GeneratedTablesFile.ReadTables(actual);
        var differences = new List<string>();

        foreach (var spec in Ft8TableConverter.Manifest)
        {
            var converted = produced[spec.CIdentifier].Values;
            if (!onDisk.TryGetValue(spec.CSharpName, out var stored))
            {
                differences.Add($"{spec.CIdentifier}: absent from the checked-in file.");
                continue;
            }

            if (stored.Length != converted.Length)
            {
                differences.Add(
                    $"{spec.CIdentifier}: {stored.Length} elements checked in, {converted.Length} converted.");
                continue;
            }

            var differing = 0;
            for (var i = 0; i < converted.Length; i++)
            {
                if (stored[i] != converted[i])
                {
                    differing++;
                }
            }

            if (differing > 0)
            {
                differences.Add(
                    $"{spec.CIdentifier}: differs at {differing} of {converted.Length} positions.");
            }
        }

        if (differences.Count == 0)
        {
            differences.Add(
                "No table's values differ. The difference is in the file's header, its geometry "
                + "constants or its layout — the data is the same and the wrapper is not.");
        }

        return new ComparisonReport(false, differences);
    }
}
