using System.Reflection;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The version, and the one place it comes from (HM-DEC-063).
/// </summary>
/// <remarks>
/// About reads the assembly at run time rather than carrying a string of its
/// own (HM-DEC-019), and the assembly gets its number from
/// <c>Directory.Build.props</c>. That chain is what keeps the About box, the
/// telemetry line and the binary from ever disagreeing, so it is the chain
/// these tests hold rather than any particular number.
/// </remarks>
public sealed class VersionTests
{
    /// <remarks>
    /// THE NUMBER IS READ, NEVER WRITTEN DOWN TWICE. A hard-coded string in the
    /// About box would be right on the day somebody typed it and wrong from the
    /// next release onward, and nothing would say so.
    /// </remarks>
    [Fact]
    public void TheAboutBoxReportsWhateverTheAssemblySays()
    {
        var assembly = typeof(AboutViewModel).Assembly.GetName().Version;

        Assert.NotNull(assembly);
        Assert.Equal(assembly!.ToString(3), AboutViewModel.AppVersion);

        // And it is a real number rather than the fallback, which would mean
        // the build stopped stamping one.
        Assert.NotEqual("unknown", AboutViewModel.AppVersion);
    }

    /// <remarks>
    /// Proves the shell and the engine ship as one thing. Two assemblies at
    /// different versions in one install is a bug report nobody can act on,
    /// because the number in the About box would not say which engine it was
    /// built against.
    /// </remarks>
    [Fact]
    public void TheShellAndTheEngineCarryTheSameVersion()
    {
        var shell = typeof(AboutViewModel).Assembly.GetName().Version;
        var engine = typeof(RadioEngine.Rig.IRig).Assembly.GetName().Version;

        Assert.Equal(shell, engine);
    }

    /// <remarks>
    /// Proves this build is at least the release this session set. A test that
    /// pinned the exact number would need editing on every release and would
    /// start failing for the wrong reason; a floor catches the real failure,
    /// which is somebody deleting the property and silently going back to
    /// 1.0.0.
    /// </remarks>
    [Fact]
    public void TheVersionIsAtLeastTheReleaseThatSetIt()
    {
        var version = typeof(AboutViewModel).Assembly.GetName().Version!;

        Assert.True(
            version >= new Version(1, 2, 0),
            $"the build reports {version.ToString(3)}, which is older than 1.2.0");
    }
}
