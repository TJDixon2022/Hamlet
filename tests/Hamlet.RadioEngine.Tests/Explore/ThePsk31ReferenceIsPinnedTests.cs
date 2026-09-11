using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 312 task 3: **the reference is pinned, and nothing is read from
/// it yet.**
/// </summary>
/// <remarks>
/// <para>**`PHASE_PLAN.md` §R5.** `fldigi` is the reference implementation, GPL-3 like
/// Hamlet, cloned **outside the tree** and pinned to one commit. The same rule the FT8
/// phase set for `ft8_lib`: **read, never ported wholesale.**</para>
/// <para>**WHY A TEST AND NOT JUST A NOTE.** A reference nobody pinned is a reference
/// that has moved by the time anybody checks a claim against it, and a path into
/// somebody's own machine compiled into `src/` is a build that only works here. Both
/// are the kind of thing that is obvious the day it is written and invisible three
/// units later.</para>
/// <para>**IT ASSERTS THE RECORD, NOT THE CLONE.** Whether `C:\Source\fldigi` exists
/// on this machine is not something a test can require of a contributor, and nothing
/// in this phase's step 0 reads a line of it. What must be true is that the note says
/// which commit was read and under what licence.</para>
/// </remarks>
public sealed class ThePsk31ReferenceIsPinnedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the note is printed.</param>
    public ThePsk31ReferenceIsPinnedTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The note exists and names a commit and a licence.**</summary>
    [Fact]
    public void TheNoteNamesACommitAndALicence()
    {
        var path = Path.Combine(Root(), "docs", "psk31-reference.md");

        Assert.True(File.Exists(path), "no docs/psk31-reference.md");

        var note = File.ReadAllText(path);

        _output.WriteLine(note);

        // **A COMMIT, AND A REAL ONE.** Forty hex characters, which is what a git
        // object id is; a note saying *the latest* pins nothing.
        var commit = Regex.Match(note, @"\b[0-9a-f]{40}\b");

        Assert.True(commit.Success, "the note names no full commit hash");

        _output.WriteLine("commit : " + commit.Value);

        // **THE LICENCE, READ FROM THE CLONE'S OWN COPYING FILE** rather than
        // recalled. Hamlet is GPL-3.0 and the reference has to be compatible with
        // it, and that is a fact about a file rather than a thing anybody remembers.
        Assert.Contains("GPL", note, StringComparison.OrdinalIgnoreCase);

        // **AND THE RULE THAT GOVERNS IT.**
        Assert.Contains("never ported wholesale", note, StringComparison.OrdinalIgnoreCase);

        // **AND THE VARICODE TABLE'S OWN PUBLIC SOURCE**, so step 1 cites it rather
        // than transcribing it from memory.
        Assert.Contains("varicode", note, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**No file under `src/` holds a path into the clone.**</summary>
    /// <remarks>
    /// <para>**A BUILD THAT ONLY WORKS ON ONE MACHINE IS NOT A BUILD** (§0). The clone
    /// lives outside the tree deliberately, and a source file that names its location
    /// makes the separation decorative.</para>
    /// <para>**NARROWED 2026-09-11, AND THE FIRST VERSION WAS WRONG RATHER THAN
    /// INCONVENIENT.** Unit 313 wrote this as a search for the word `fldigi` anywhere
    /// under `src/`, which is broader than what its own summary says and broader than
    /// what §R5 asks for: the rule is *read, never ported wholesale*, and **a citation
    /// naming where a published table came from is exactly what that rule requires**.
    /// Work instruction 314 task 2 carried the varicode into the tree with its
    /// citation, as instructed, and this test failed it. What must not appear is a
    /// **path into the clone**, and that is what it looks for now.</para>
    /// </remarks>
    [Fact]
    public void NoSourceFileHoldsAPathIntoTheClone()
    {
        var src = Path.Combine(Root(), "src");

        var offenders = Directory
            .EnumerateFiles(src, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj"
                + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin"
                + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(f => NamesTheClone(File.ReadAllText(f)))
            .ToList();

        foreach (var f in offenders)
        {
            _output.WriteLine("names the reference: " + f);
        }

        Assert.Empty(offenders);
    }

    /// <summary>Whether some source text points at the clone on disk.</summary>
    /// <param name="text">The file's content.</param>
    /// <returns>True where it holds a path into the reference clone.</returns>
    /// <remarks>
    /// **BOTH SLASHES, BECAUSE A C# STRING AND A COMMENT SPELL A PATH DIFFERENTLY**,
    /// and either one compiled into `src/` would be a build that works on one machine.
    /// </remarks>
    private static bool NamesTheClone(string text)
        => text.Contains(@"Source\fldigi", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Source/fldigi", StringComparison.OrdinalIgnoreCase);

    private static string Root()
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
