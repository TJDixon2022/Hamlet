using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The <c>LICENSE</c> and the <c>NOTICE</c> exist beside the project file, and the NOTICE names
/// the published sources this library implements from.</b>
/// </summary>
/// <remarks>
/// <para>
/// <c>PHASE_PLAN.md</c> step 1's fourth must-pass exit is a NOTICE citing its sources <em>before a
/// line of them is implemented</em>. That is a claim about a file, and a claim about a file that
/// nothing reads is a claim that will rot the first time somebody tidies it - the citation would go
/// and the code that needs it would stay, and nobody would find out until a licence question was
/// being asked in earnest.
/// </para>
/// <para>
/// <b>These tests check that the words are there, and they cannot check that the words are true.</b>
/// Whether no WSJT-X source was read is a fact about how the code was written, not a property a test
/// can observe. What is mechanised here is the part that can be: the files exist, the licence is
/// named, and both papers are named by title.
/// </para>
/// </remarks>
public class Ft8DeepNoticeTests(ITestOutputHelper output)
{
    private static string Read(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.SiblingDirectory(), name));

    [Fact]
    public void LicenseAndNoticeSitBesideTheProjectFile()
    {
        var directory = RepositoryRoot.SiblingDirectory();

        foreach (var name in new[] { "Ft8Sharp.Deep.csproj", "LICENSE", "NOTICE" })
        {
            var path = Path.Combine(directory, name);
            Assert.True(
                File.Exists(path),
                $"{path} does not exist. A library published under its own licence carries that "
                    + "licence beside it, not a reference to one somewhere else in a repository it "
                    + "may not travel with.");
            output.WriteLine($"  {name,-24} {new FileInfo(path).Length,8} bytes");
        }
    }

    /// <summary>
    /// <b>GPL-3.0, verbatim.</b> The phase ruling of 2026-09-04 permits either the verbatim text or a
    /// file naming it by SPDX identifier and pointing at the root; unit 245 took the verbatim copy.
    /// </summary>
    [Fact]
    public void TheLicenseIsTheGnuGeneralPublicLicenseVersion3()
    {
        var licence = Read("LICENSE");

        Assert.Contains("GNU GENERAL PUBLIC LICENSE", licence, StringComparison.Ordinal);
        Assert.Contains("Version 3, 29 June 2007", licence, StringComparison.Ordinal);

        // The verbatim text, not a stub that names it. A three-line pointer would satisfy the two
        // assertions above and would not be what was committed.
        Assert.True(
            licence.Length > 30_000,
            $"LICENSE is {licence.Length} characters, which is too short to be the verbatim GPL-3.0 "
                + "text. If a later unit deliberately replaces it with an SPDX pointer, that is "
                + "permitted by the ruling and this assertion is what it must come and change.");
    }

    /// <summary>
    /// <b>Both published sources, by title.</b> These are the two the plan names: Fossorier and Lin
    /// for step 2's ordered statistics decoding, and the QEX paper for the protocol.
    /// </summary>
    [Fact]
    public void TheNoticeNamesBothPublishedSourcesByTitle()
    {
        var notice = Read("NOTICE");

        Assert.Contains(
            "Soft-Decision Decoding of Linear Block Codes Based on Ordered Statistics",
            notice,
            StringComparison.Ordinal);
        Assert.Contains("Fossorier", notice, StringComparison.Ordinal);
        Assert.Contains("1995", notice, StringComparison.Ordinal);

        Assert.Contains(
            "The FT4 and FT8 Communication Protocols",
            notice,
            StringComparison.Ordinal);
        Assert.Contains("QEX", notice, StringComparison.Ordinal);
        Assert.Contains("July/August 2020", notice, StringComparison.Ordinal);

        output.WriteLine(notice);
    }

    /// <summary>
    /// <b>The NOTICE states the four things the exit asks it to state.</b> That this library is
    /// GPL-3.0; that it depends on Ft8Sharp, which is MIT and stays MIT; that no WSJT-X source and no
    /// <c>ft4_ft8_public/</c> was read; and that what it implements comes from published description.
    /// </summary>
    [Fact]
    public void TheNoticeStatesTheLicenceTheDependencyAndWhatWasNotRead()
    {
        var notice = Read("NOTICE");

        Assert.Contains("GPL-3.0", notice, StringComparison.Ordinal);
        Assert.Contains("MIT", notice, StringComparison.Ordinal);
        Assert.Contains("WSJT-X", notice, StringComparison.Ordinal);
        Assert.Contains("ft4_ft8_public/", notice, StringComparison.Ordinal);
        Assert.Contains("published description", notice, StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>The port's own NOTICE still says MIT and still cites <c>ft8_lib</c>.</b> The seam is only
    /// split if both sides stay what they say they are, and the sibling arriving is exactly the
    /// moment somebody might tidy the two files into one.
    /// </summary>
    [Fact]
    public void ThePortsNoticeIsUnchangedInWhatItClaims()
    {
        var portDirectory = Path.Combine(RepositoryRoot.Locate(), "src", "Ft8Sharp");

        var licence = File.ReadAllText(Path.Combine(portDirectory, "LICENSE"));
        var notice = File.ReadAllText(Path.Combine(portDirectory, "NOTICE"));

        Assert.Contains("MIT License", licence, StringComparison.Ordinal);
        Assert.Contains("ft8_lib", notice, StringComparison.Ordinal);
        Assert.Contains("MIT License", notice, StringComparison.Ordinal);
        Assert.DoesNotContain("GPL", licence, StringComparison.Ordinal);
    }
}
