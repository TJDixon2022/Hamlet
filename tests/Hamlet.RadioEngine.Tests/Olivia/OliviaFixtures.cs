using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>One Olivia fixture as the manifest describes it, after its hash was checked.</summary>
/// <param name="Path">Where the file is.</param>
/// <param name="Variant">The manifest's variant, used only to check the detection.</param>
/// <param name="CenterHz">The manifest's center, used only to check the detection, or NaN.</param>
/// <param name="Text">The manifest's text.</param>
public sealed record OliviaFixture(string Path, string? Variant, double CenterHz, string Text);

/// <summary>
/// **The Olivia fixtures, hashed before use, and the one character error rate every fixture test
/// uses** (work instruction 361 decision J and criterion 2.5).
/// </summary>
public static class OliviaFixtures
{
    /// <summary>The manifest's entry for a file, after its hash has been checked against it.</summary>
    /// <param name="file">The fixture's file name.</param>
    /// <returns>The entry.</returns>
    public static OliviaFixture Load(string file)
    {
        var folder = System.IO.Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(System.IO.Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = System.IO.Path.Combine(folder, file);
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

        // **A FIXTURE THAT CHANGED UNDERNEATH THE TEST IS A DIFFERENT TEST** (2.5).
        Assert.Equal(entry.GetProperty("sha256").GetString(), hash);

        var variant = entry.GetProperty("variant");
        var center = entry.GetProperty("center_hz");

        return new OliviaFixture(
            path,
            variant.ValueKind == JsonValueKind.String ? variant.GetString() : null,
            center.ValueKind == JsonValueKind.Number ? center.GetDouble() : double.NaN,
            entry.GetProperty("text").GetString() ?? "");
    }

    /// <summary>
    /// **The character error rate, stated once** (decision J): the Levenshtein distance between
    /// what was decoded and the manifest's text, over the manifest text's length.
    /// </summary>
    /// <param name="decoded">What the demodulator gave.</param>
    /// <param name="expected">The manifest's text.</param>
    /// <returns>The rate; for an empty expected text, the number of characters decoded.</returns>
    /// <remarks>
    /// CR LF and a lone CR become LF in both; case is compared exactly; nothing else is
    /// normalized. Characters decoded before sync are insertions like any other, not trimmed.
    /// </remarks>
    public static double CharacterErrorRate(string decoded, string expected)
    {
        var a = Unify(decoded);
        var b = Unify(expected);

        if (b.Length == 0)
        {
            return a.Length;
        }

        return (double)Distance(a, b) / b.Length;
    }

    /// <summary>The Levenshtein edit distance.</summary>
    /// <param name="a">One text.</param>
    /// <param name="b">The other.</param>
    /// <returns>Insertions, deletions and substitutions, the fewest that turn one into the other.</returns>
    public static int Distance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    /// <summary>The repository's root.</summary>
    /// <returns>The folder holding Hamlet.sln.</returns>
    public static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(System.IO.Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static string Unify(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
