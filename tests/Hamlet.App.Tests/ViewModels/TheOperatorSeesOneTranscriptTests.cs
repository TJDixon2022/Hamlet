using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Proves HM-REQ-121: the operator sees one transcript, no decoder is named on
/// the CW tab, and the capture sheet records both readings per character (work
/// instruction 465, task 3; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**AN INJECTED DISAGREEMENT, BUILT BY HAND** (CLAUDE.md 12.5): ours reads R
/// at 0.70 and the port K at 0.90 on one span, both voting, through
/// <see cref="CwArbiter.Arbitrate"/>, then onto the tab's
/// <see cref="CwTranscript"/> as `StartDecoding` wires it, edge and settle.</para>
/// <para>**WATCHED FAILING FIRST** against a stub
/// <see cref="MainWindowViewModel.ArbitrationLine"/> that wrote nothing: both
/// sheet cases red (`.run-unit/unit465-one-red.txt`). The tab case was red there
/// only on this test's own first defect, reading the main window's comments,
/// which cite the project arbiter's rulings and are never drawn; so it was
/// watched red again against a stub <see cref="CwArbiter.Arbitrate"/> that put
/// both readings of the span on the transcript, and failed on one character per
/// span (`.run-unit/unit465-one-red-tab.txt`).</para>
/// </remarks>
public sealed class TheOperatorSeesOneTranscriptTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sheet line is printed.</param>
    public TheOperatorSeesOneTranscriptTests(ITestOutputHelper output)
        => _output = output;

    // Words that would name a decoder or say which one won.
    private static readonly string[] Names = { "fldigi", "port", "second", "ours", "arbiter", "hamlet's decoder", "decoder" };

    private static CwArbitrated Disagreement()
    {
        var ours = new CwCharacter("R", CwConfidence.High, 1, ".-.", double.NaN, 18, TimeSpan.FromSeconds(1.0))
        {
            SpanHops = 60,
            Probability = 0.70,
        };
        var port = new CwSecondReading("K", CwConfidence.High, "-.-", 0.90, 0.705, 0.995, 18);

        return CwArbiter.Arbitrate(new[] { ours }, new[] { port }, new CwVote(true, true));
    }

    /// <remarks>HM-REQ-121: one character on the span, and no decoder named anywhere the tab draws from - the transcript, its tip, the rendering of each character, or the main window's markup.</remarks>
    [Fact]
    public void HmReq121_TheTabShowsOneCharacterPerSpanAndNoDecoderName()
    {
        var arbitrated = Disagreement();
        var transcript = new CwTranscript();

        transcript.OfferEdge(arbitrated.Characters);
        transcript.Settle(arbitrated.Characters.Single());

        Assert.Equal("K", transcript.PlainText);
        Assert.Equal("", transcript.TipText);
        Assert.Equal("K", transcript.Render(transcript.Recent().Single()));

        // Comments are never drawn; they cite "the arbiter's ruling", the project's, not a decoder.
        var markup = System.Text.RegularExpressions.Regex.Replace(
            File.ReadAllText(Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml")),
            "<!--.*?-->", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        var tab = transcript.PlainText + transcript.TipText + string.Concat(transcript.Recent().Select(transcript.Render));

        foreach (var name in Names)
        {
            Assert.DoesNotContain(name, tab, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var name in new[] { "fldigi", "second decoder", "arbiter", "arbitrat", "CwReader" })
        {
            Assert.DoesNotContain(name, markup, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>HM-REQ-121 with 126: the sheet for the same read carries both characters, both p's, the case and which decoder was emitted.</remarks>
    [Fact]
    public void HmReq121_TheSheetForTheSameReadCarriesBoth()
    {
        var transcript = new CwTranscript();

        transcript.Settle(Disagreement().Characters.Single());

        var line = MainWindowViewModel.ArbitrationLine(transcript.Recent(), "since listening started");

        _output.WriteLine(line);

        Assert.Contains("K:port/disagree", line, StringComparison.Ordinal);
        Assert.Contains("ours R 0.700", line, StringComparison.Ordinal);
        Assert.Contains("port K 0.900", line, StringComparison.Ordinal);
    }

    /// <remarks>HM-REQ-127 on the sheet: a tie is marked; HM-REQ-121: a character that passed through no arbiter says so rather than printing nothing.</remarks>
    [Fact]
    public void HmReq121_TheSheetMarksATieAndSaysWhereNothingWasRecorded()
    {
        var ours = new CwCharacter("R", CwConfidence.High, 1, ".-.", double.NaN, 18, TimeSpan.FromSeconds(1.0)) { SpanHops = 60, Probability = 0.88 };
        var port = new CwSecondReading("K", CwConfidence.High, "-.-", 0.90, 0.705, 0.995, 18);
        var tie = CwArbiter.Arbitrate(new[] { ours }, new[] { port }, new CwVote(true, true)).Characters.Single();

        Assert.Contains("K:port/tie", MainWindowViewModel.ArbitrationLine(new[] { tie }, "x"), StringComparison.Ordinal);
        Assert.Contains("unrecorded", MainWindowViewModel.ArbitrationLine(new[] { ours }, "x"), StringComparison.Ordinal);
        Assert.Equal("nothing read yet", MainWindowViewModel.ArbitrationLine(Array.Empty<CwCharacter>(), "x"));
    }

    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Hamlet.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Hamlet.sln not found above the test binaries");
    }
}
