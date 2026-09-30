using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The scope's hover on a block says its shape score** (work instruction 502, R110, HM-DEC-206):
/// how far inside the shape of a keyed tone the mark under it scored, or that none was handed out.
/// </summary>
public sealed class TheBlockSaysItsShapeTests
{
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    /// <remarks>A block with a mark under it says its score; one with none says so.</remarks>
    [Fact]
    public void TheHoverSaysTheScoreOrThatNoMarkWasHandedOut()
    {
        var marked = new CwGraphBar(Now, Now.AddMilliseconds(150), 150, true, 0.873);
        var unmarked = new CwGraphBar(Now, Now.AddMilliseconds(20), 20, false);

        Assert.Equal("dah, 150 ms, shape 0.87 of 1", CwScopeControl.BarTip(marked));
        Assert.Equal("dit, 20 ms, not handed out as a mark", CwScopeControl.BarTip(unmarked));
    }
}
