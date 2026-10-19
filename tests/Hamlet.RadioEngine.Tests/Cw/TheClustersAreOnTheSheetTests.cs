using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE CLUSTERS ON THE SHEET** (work instruction 565, task 2, HM-DEC-269): every capture sheet carries the printed sender's
/// letter-gap cluster (centre and top), its word-gap cluster (centre and bottom) and its word line, in ms, so a capture on SLOW
/// can be read the next day without a replay.
/// </summary>
public sealed class TheClustersAreOnTheSheetTests
{
    /// <remarks>Both clusters, the letter cluster alone, and nobody printed.</remarks>
    [Fact]
    public void TheLineSaysEachCluster()
    {
        var both = new CwGapLines(0.06, 0.1, 0.254, [], [0.15, 0.18, 0.2, 0.22], [0.295, 0.4, 0.55]);
        var letter = both with { WordGaps = null, WordSeconds = 0.46 };

        Assert.Equal("letter centre 188, top 220 (4 gaps); word centre 415, bottom 295 (3 gaps); line 254 ms", CwSenderGate.ClustersOf(both));
        Assert.Equal("letter centre 188, top 220 (4 gaps); no word cluster; line 460 ms", CwSenderGate.ClustersOf(letter));
        Assert.Equal("nobody printed", CwSenderGate.ClustersOf(null));
        Assert.Equal("nobody printed", new CwSenderGate().ClustersLine);
    }
}
