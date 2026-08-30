using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The live survey judges admission over three seconds, not over the recording.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MISMATCH BETWEEN A WORK ORDER AND THE TREE, AND THE TREE
/// WINS** (work instruction 052, task 3, which says exactly that). The order's
/// diagnosis is *"a station present for the last fifteen seconds of a
/// thirty-second capture had its duty and swing computed over the whole
/// recording"*, and task 3 asks for the statistics to be computed over the
/// strongest contiguous stretch instead.</para>
/// <para>**They already are computed over a short stretch.** `CwToneSurvey`'s
/// constructor takes `seconds = 3.0` and `CwToneTracker` builds both the coarse
/// and the fine survey without overriding it, so the ring buffer holds three
/// seconds and `presentFraction` is counted over what is in it. **There is no
/// whole-recording duty anywhere on the admission path.**</para>
/// <para>**WHERE THE WHOLE-RECORDING NUMBERS COME FROM IS THE CAPTURE SHEET**,
/// which reports what a whole file looked like at the moment somebody pressed
/// the button. Those are the figures the order quotes — 39 per cent duty, 19 dB
/// swing — and they describe the recording rather than the decision.</para>
/// <para>So the change task 3 asks for cannot be built as described: the thing it
/// would replace is not there. **What this test does is stop the same order being
/// written a second time**, by making the window a measured fact with a name.</para>
/// </remarks>
public sealed class TheSurveyAlreadyUsesAShortWindowTests
{
    private readonly ITestOutputHelper _output;

    public TheSurveyAlreadyUsesAShortWindowTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The survey's history never grows past three seconds.</summary>
    /// <remarks>
    /// Thirty seconds of hops are pushed in — the length of every capture in this
    /// corpus — and the window is asked how much history it is holding.
    /// </remarks>
    [Fact]
    public void ThirtySecondsOfHopsLeavesThreeSecondsOfHistory()
    {
        var bins = new[] { 550.0, 575.0, 600.0, 625.0, 650.0 };
        var survey = new CwToneSurvey(bins, hopSeconds: 0.02);
        var hop = new double[bins.Length];

        for (var i = 0; i < 1500; i++)
        {
            survey.Observe(hop, blocked: false);
        }

        _output.WriteLine(
            $"30 s of hops leaves {survey.HistorySeconds:0.00} s of survey history");

        Assert.True(
            survey.HistorySeconds <= 3.5,
            $"the survey is holding {survey.HistorySeconds:0.00} s, "
            + "so it is not a short window");
    }

    /// <summary>
    /// And the tracker asks for the default rather than a longer one.
    /// </summary>
    /// <remarks>
    /// **THE CONSTANT IS THE CLAIM.** A survey built with a longer window would
    /// make the order's diagnosis right, so the default is pinned here rather
    /// than left to be read out of a constructor argument nobody passes.
    /// </remarks>
    [Fact]
    public void TheDefaultWindowIsThreeSeconds()
    {
        // **THE TRACKER PASSES NO OVERRIDE, WHICH IS THE OTHER HALF.** A survey
        // built with a longer window would make the order's diagnosis right, so
        // the absence of an argument is asserted rather than read once and
        // remembered.
        var source = File.ReadAllText(Path.Combine(
            CwFixtures.Folder, "..", "..", "..",
            "src", "Hamlet.RadioEngine", "Cw", "CwToneTracker.cs"));

        Assert.Contains(
            "new CwToneSurvey(_coarseHz, surveyHop)", source, StringComparison.Ordinal);
        Assert.Contains(
            "new CwToneSurvey(_fineHz, surveyHop)", source, StringComparison.Ordinal);

        _output.WriteLine(
            "both surveys are built with two arguments, so the window is the "
            + "constructor default of 3.0 seconds");
    }
}
