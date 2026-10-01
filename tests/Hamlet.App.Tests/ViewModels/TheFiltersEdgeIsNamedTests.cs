using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The app says when a station is at the filter's edge** (work instruction 514, task 3, HM-DEC-218).
/// </summary>
/// <remarks>
/// A frame as the scope tick builds it: the detector's passband from the rig state, 500 Hz wide on a
/// 600 Hz pitch, and the pitch the reader prints. Nothing is written to the radio.
/// </remarks>
public sealed class TheFiltersEdgeIsNamedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheFiltersEdgeIsNamedTests(ITestOutputHelper output) => _output = output;

    private static CwScopeFrame Printing(double hz, bool fromRig = true)
        => CwScopeFrame.From(
            Array.Empty<CwScopeHop>(),
            5,
            CwEnvelopeReading.None with { PassbandLowHz = 350, PassbandHighHz = 850, PassbandFromRig = fromRig },
            mixingHz: hz);

    /// <remarks>Task 3: a printed sender at 380 Hz in a 500 Hz filter on 600 shows the sentence and a hover; one at 600 shows nothing.</remarks>
    [Fact]
    public void AStationAtTheEdgeIsNamedAndOneInsideIsNot()
    {
        var edge = Printing(380);
        var inside = Printing(600);

        _output.WriteLine($"380 Hz: `{edge.EdgeLine}` / hover `{edge.EdgeTip}`");
        _output.WriteLine($"600 Hz: `{inside.EdgeLine}`");

        Assert.Equal(CwScopeFrame.EdgeWords, edge.EdgeLine);
        Assert.Contains("500 Hz", edge.EdgeTip);
        Assert.Contains("600 Hz", edge.EdgeTip);
        Assert.Equal(string.Empty, inside.EdgeLine);
    }

    /// <remarks>Task 3: with no passband from the rig, nothing is claimed about a filter; and printing nobody, nothing is shown.</remarks>
    [Fact]
    public void WithoutTheRigsFilterOrAStationNothingIsSaid()
    {
        Assert.Equal(string.Empty, Printing(380, fromRig: false).EdgeLine);
        Assert.Equal(string.Empty, Printing(double.NaN).EdgeLine);
    }
}
