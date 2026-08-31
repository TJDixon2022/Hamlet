using System.Text.RegularExpressions;

namespace Ft8Sharp.Tests.TableGen;

/// <summary>
/// Where this repository is, found from where the test is running rather than assumed.
/// </summary>
internal static class RepositoryTree
{
    /// <summary>The repository root — the folder holding <c>Hamlet.sln</c>.</summary>
    public static string Root { get; } = FindRoot();

    /// <summary>The checked-in generated tables file.</summary>
    public static string GeneratedTablesFile
        => Path.Combine(Root, Ft8TableConverter.GeneratedFileRelativePath);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Hamlet.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"No Hamlet.sln above {AppContext.BaseDirectory}, so the repository root could not be "
            + "found and the generated tables file cannot be located.");
    }
}

/// <summary>
/// Reads the byte arrays back out of the generated file, so a failed comparison can say which
/// table differs and at how many positions rather than only that two long strings are not equal.
/// </summary>
internal static class GeneratedTablesFile
{
    private static readonly Regex Property = new(
        @"public static ReadOnlySpan<byte> (?<name>[A-Za-z0-9_]+) => new byte\[\]\s*\{(?<body>[^}]*)\}",
        RegexOptions.Compiled);

    private static readonly Regex Element = new(@"0[xX](?<hex>[0-9A-Fa-f]{1,2})", RegexOptions.Compiled);

    /// <summary>Every emitted table in the file, by its C# property name.</summary>
    public static IReadOnlyDictionary<string, byte[]> ReadTables(string generatedSource)
    {
        var tables = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (Match property in Property.Matches(generatedSource))
        {
            var values = Element.Matches(property.Groups["body"].Value)
                .Select(m => System.Convert.ToByte(m.Groups["hex"].Value, 16))
                .ToArray();
            tables[property.Groups["name"].Value] = values;
        }

        return tables;
    }
}
