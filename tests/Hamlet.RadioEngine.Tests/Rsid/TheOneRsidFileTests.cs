using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rsid;

/// <summary>
/// Work instruction 377 task 2: **the tree keeps ONE RSID data file, and the path that used to
/// hold the other one is retired rather than deleted** (criterion 2.2, section 6 ruling 1).
/// </summary>
/// <remarks>
/// <para>**WHY IT EXISTS.** Until this unit the tree held two RSID files that could disagree: the
/// one under `data/` that the engine embedded, which carried four of its eight codes' tone
/// sequences and neither of fldigi's tables, and `assets/data/rsid-codes.json`, which carried a
/// sequence for every code and both tables and which nothing read. **Two files that can disagree
/// about what is announced on the air is the fault**, not which of them was shorter.</para>
/// <para>**THE RETIRED PATH IS NAMED IN EXACTLY ONE PLACE AND IT IS HERE.** The needles below are
/// built from their segments so that this file's own text does not match its own scan, which means
/// the scan needs no exception for itself and cannot be weakened by one later.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004). No port
/// is opened, no device enumerated and nothing keyed.</para>
/// </remarks>
public sealed class TheOneRsidFileTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheOneRsidFileTests(ITestOutputHelper output) => _output = output;

    /// <summary>The retired path's folder, kept apart from its leaf so no literal of the path exists here.</summary>
    private const string RetiredFolder = "data";

    /// <summary>The retired path's leaf folder.</summary>
    private const string RetiredLeaf = "rsid";

    /// <summary>The retired path's file name, which is also the name the file that replaced it has.</summary>
    private const string RetiredFile = "rsid-codes.json";

    /// <summary>
    /// **The one RSID file is the one the engine embeds, it is the one in the tree, and it carries
    /// a tone sequence for every code it lists.**
    /// </summary>
    [Fact]
    public void TheOneRsidFileIsTheLongOneAndTheTreeCopyIsTheEmbeddedCopy()
    {
        var root = RepoRoot();
        var path = Path.Combine(root, "assets", "data", "rsid-codes.json");

        Assert.Equal("assets/data/rsid-codes.json", RsidCodes.FilePath);
        Assert.True(File.Exists(path), RsidCodes.FilePath + " is not in the tree");

        var tree = RsidCodes.Parse(File.ReadAllText(path));
        var embedded = OliviaData.Current.Rsid;

        Assert.Null(OliviaData.Current.Problem);
        Assert.NotNull(embedded);

        _output.WriteLine($"{RsidCodes.FilePath}: {tree.Codes.Count} codes, {tree.ToneSequences.Count} tone sequences");
        _output.WriteLine($"embedded as {OliviaData.RsidResourceName}: {embedded!.Codes.Count} codes, {embedded.ToneSequences.Count} tone sequences");

        // **THE TREE COPY AND THE EMBEDDED COPY ARE ONE FILE**, code for code and tone for tone.
        Assert.Equal(tree.Codes, embedded.Codes);
        Assert.Equal(tree.ToneSequences.Count, embedded.ToneSequences.Count);

        foreach (var (name, tones) in tree.ToneSequences)
        {
            Assert.True(embedded.ToneSequences.TryGetValue(name, out var mine), name + " is in the tree copy and not in the embedded copy");
            Assert.Equal(tones, mine);
        }

        // **EVERY CODE THE FILE LISTS HAS A SEQUENCE OF THE FILE'S OWN LENGTH**, which is the half
        // of criterion 2.2 that the short file could not meet.
        foreach (var (name, code) in embedded.Codes.OrderBy(c => c.Value))
        {
            var tones = RsidBurst.TonesFor(embedded, code);

            _output.WriteLine($"  {name,-16} {code,3}  {(tones is null ? "NO SEQUENCE" : tones.Count + " tones")}");

            Assert.NotNull(tones);
            Assert.Equal(embedded.Symbols, tones!.Count);
        }

        // **AND BOTH OF fldigi'S TABLES TRAVELED WITH IT**, read off the JSON because `RsidCodes`
        // does not parse them: they are what a later unit's decoder work would need and they were
        // the other half of what the short file was missing.
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        Assert.True(document.RootElement.TryGetProperty("squares", out var squares), "the file carries no squares table");
        Assert.True(document.RootElement.TryGetProperty("indices", out var indices), "the file carries no indices table");

        _output.WriteLine($"squares {squares.GetArrayLength()} entries, indices {indices.GetArrayLength()} entries");

        Assert.Equal(256, squares.GetArrayLength());
        Assert.Equal(12, indices.GetArrayLength());
    }

    /// <summary>
    /// **The retired path is still in the tree, says what replaced it, and nothing under `src` or
    /// `tests` reads it.**
    /// </summary>
    /// <remarks>
    /// **EMPTY IT, COMMENT IT, LIST IT** (`PHASE_PLAN.md` section 6). A deleted file is a fact
    /// somebody has to go to the history for; a file holding one key that names its replacement
    /// answers the question where it is asked.
    /// </remarks>
    [Fact]
    public void TheRetiredPathIsEmptiedNamesItsReplacementAndNothingReadsIt()
    {
        var root = RepoRoot();
        var folder = RetiredFolder;
        var leaf = RetiredLeaf;
        var file = RetiredFile;
        var retired = Path.Combine(root, folder, leaf, file);

        Assert.True(File.Exists(retired), "the retired file was deleted, and it must be emptied instead");

        using var document = JsonDocument.Parse(File.ReadAllText(retired));
        var keys = document.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        _output.WriteLine($"{folder}/{leaf}/{file} now holds: {string.Join(", ", keys)}");

        // **ONE KEY, AND IT NAMES THE FILE THAT REPLACED IT AND THE UNIT THAT DID IT.**
        var only = Assert.Single(keys);

        Assert.Equal("replaced_by", only);

        var said = document.RootElement.GetProperty("replaced_by").GetString()!;

        _output.WriteLine("replaced_by: " + said);

        Assert.Contains(RsidCodes.FilePath, said, StringComparison.Ordinal);
        Assert.Contains("377", said, StringComparison.Ordinal);

        // **AND IT CARRIES NONE OF WHAT IT USED TO**, so nothing can quietly start reading it again
        // and get a shorter answer than the one file gives.
        Assert.DoesNotContain("tone_sequences", said, StringComparison.Ordinal);
        Assert.False(document.RootElement.TryGetProperty("codes", out _));
        Assert.False(document.RootElement.TryGetProperty("tone_sequences", out _));

        // **NOW THE SCAN.** Every C# file and every project file under `src` and `tests`, in every
        // form the path is written in the tree: with forward slashes, with backslashes as the
        // csproj writes them, and as the segments a `Path.Combine` hands over.
        var needles = new[]
        {
            folder + "/" + leaf + "/" + file,
            folder + "\\" + leaf + "\\" + file,
            "\"" + folder + "\", \"" + leaf + "\", \"" + file + "\"",
        };

        var found = new List<string>();
        var scanned = 0;

        foreach (var where in new[] { "src", "tests" })
        {
            foreach (var each in Directory.EnumerateFiles(Path.Combine(root, where), "*.*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(each);

                if (!string.Equals(extension, ".cs", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // **NOTHING UNDER bin OR obj**, which is the build's copy of what is already scanned.
                if (each.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    || each.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    continue;
                }

                scanned++;

                var text = File.ReadAllText(each);

                foreach (var needle in needles.Where(n => text.Contains(n, StringComparison.Ordinal)))
                {
                    found.Add(Path.GetRelativePath(root, each) + " holds \"" + needle + "\"");
                }
            }
        }

        _output.WriteLine($"scanned {scanned} .cs and .csproj files under src and tests");
        _output.WriteLine(found.Count == 0 ? "no file names the retired path" : string.Join("\n", found));

        Assert.Empty(found);
    }

    private static string RepoRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
