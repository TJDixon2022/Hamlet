using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Hamlet does not turn the radio's spectrum output on, which HM-DEC-062 ruled
/// and 8c2abf3 broke.
/// </summary>
/// <remarks>
/// <para>**IT SHIPPED TWICE WITH NOTHING NOTICING.** That ruling says in terms
/// that nothing here turns the scope on, that it is a write, and that the path is
/// reads only. Version 1.8.0 put `_ = AskForTheSpectrumAsync(radio)` in the
/// connect path, and it wrote `27 11` at every connect from then on.</para>
/// <para>**AND THE COST WAS NOT HYPOTHETICAL.** A waveform sweep is 475 points in
/// eleven parts, on the order of six hundred bytes, on a cable carrying about
/// eleven and a half thousand bytes a second. HM-OPEN-042 then found the readback
/// could not confirm the write, so Hamlet reported it refused without knowing
/// whether it had succeeded.</para>
/// <para>This test reads the source, because the fault was a line in a
/// composition path rather than anything a type could forbid. Grep is a poor
/// instrument and it is the right one here: what has to be prevented is somebody
/// writing that call again.</para>
/// </remarks>
public sealed class ScopeIsNeverTurnedOnTests
{
    private static string AppSource()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !Directory.Exists(Path.Combine(here.FullName, "src")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        var app = Path.Combine(here!.FullName, "src", "Hamlet.App");
        Assert.True(Directory.Exists(app), "the app project has moved");

        return string.Join(
            "\n",
            Directory.GetFiles(app, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
    }

    /// <remarks>
    /// Proves HM-DEC-062: nothing in the app asks the radio to send its spectrum.
    /// Reading `27 10` and `27 11` to say what is on is what that ruling allows
    /// and is untouched; writing is what it forbids.
    /// </remarks>
    [Fact]
    public void NothingInTheAppWritesTheScopeOutput()
    {
        var source = AppSource();

        // The comments explaining why the write is gone name it, so this looks
        // for the call rather than the word.
        var writes = Regex.Matches(
            source,
            @"SetSettingAsync\s*\(\s*CivWrites\.ScopeOutput",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        Assert.True(
            writes.Count == 0,
            "HM-DEC-062 says nothing here turns the scope on, and something does: "
            + "that ruling was broken by 8c2abf3 in 1.8.0 and shipped twice");
    }

    /// <remarks>
    /// Proves the read stays. The panel has to be able to say what is on, and
    /// that is the half HM-DEC-062 permits.
    /// </remarks>
    [Fact]
    public void TheScopeSettingsAreStillRead()
    {
        Assert.Contains(CivReads.All, r => r.Field == RigField.ScopeOutput);
        Assert.Contains(CivReads.All, r => r.Field == RigField.ScopeOn);

        Assert.Equal(0x27, CivReads.ScopeOutput.Command);
        Assert.Equal(new byte[] { 0x11 }, CivReads.ScopeOutput.SubCommand);
    }

    /// <remarks>
    /// Proves the write itself is still a cited, documented command rather than
    /// being deleted. It may be ruled back in, and a table entry is not the same
    /// thing as a call site: §4 is the citation and HM-DEC-062 is the policy.
    /// </remarks>
    [Fact]
    public void TheCommandStaysInTheCitedTable()
    {
        Assert.Contains(CivWrites.All, w => w.Field == RigField.ScopeOutput);
    }

    /// <remarks>
    /// Proves the setting that decides whether the radio announces its own
    /// changes is read and never written, for the same reason (HM-OPEN-043).
    /// </remarks>
    [Fact]
    public void TransceiveIsReadAndNeverWritten()
    {
        Assert.Contains(CivReads.All, r => r.Field == RigField.CivTransceive);
        Assert.DoesNotContain(CivWrites.All, w => w.Field == RigField.CivTransceive);
    }
}
