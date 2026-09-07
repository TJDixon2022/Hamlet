using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 273, task 3: the tooltip says `He`, and the file no longer
/// states a rule its own code breaks.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-07** (HM-DEC-159). Unit 251 wrote a rule into
/// `Ft8Vocabulary`'s own remarks — *no pronoun chooses a gender* — and unit 271
/// followed it, wrote `they`, and raised the conflict as an ask rather than
/// choosing for him. He ruled `he`.</para>
/// <para>**THE SECOND TEST HERE IS THE ONE THAT MATTERS LONGEST.** The wording
/// could be put back by any session; a file that still stated the old rule would
/// invite exactly that, and the session after would find a comment and a
/// contradiction with no way to tell which was the decision. So the file is read
/// and asserted against, not just the sentence it produces.</para>
/// </remarks>
public sealed class TheTooltipSaysHeTests
{
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheTooltipSaysHeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every grid tooltip says `He` and none says `They`.</summary>
    [Theory]
    [InlineData("CQ IK4LZH JN54")]
    [InlineData("CQ ON4ABC JO20")]
    [InlineData("CQ VK9XYZ QG44")]
    [InlineData("KE9COB N5CH EM12")]
    public void TheTooltipSaysHe(string message)
    {
        foreach (var his in new[] { HisGrid, "" })
        {
            var help = Help(message, his);

            _output.WriteLine("[" + his + "] " + help);

            Assert.Contains("He is ", help, StringComparison.Ordinal);

            Assert.DoesNotContain("They are", help, StringComparison.Ordinal);
            Assert.DoesNotContain("they are", help, StringComparison.Ordinal);
        }
    }

    /// <summary>The country and the compass qualifier are untouched.</summary>
    /// <remarks>
    /// **NOTHING ELSE ABOUT THE WORDING CHANGES** (the instruction's own rule).
    /// The country still comes from the callsign and never from the grid, and
    /// where the DXCC table declines there is no country at all.
    /// </remarks>
    [Fact]
    public void NothingElseAboutTheWordingChanged()
    {
        Assert.Equal(
            "IK4LZH is calling anyone. He is in northern Italy, in grid JN54, "
            + "4,400 miles away on a bearing of 53 degrees.",
            Help("CQ IK4LZH JN54", HisGrid));

        Assert.Contains(
            "He is in Belgium,", Help("CQ ON4ABC JO20", HisGrid),
            StringComparison.Ordinal);

        // The table declines VK9, so no country at all - and the distance is
        // still given, because that is arithmetic and is not in doubt.
        var declined = Help("CQ VK9XYZ QG44", HisGrid);

        _output.WriteLine(declined);

        Assert.Contains("He is in grid QG44,", declined, StringComparison.Ordinal);
        Assert.DoesNotContain("Australia", declined, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// `Ft8Vocabulary` no longer states the rule its own code would break.
    /// </summary>
    /// <remarks>
    /// <para>**THE RULE CAME OUT RATHER THAN GAINING AN EXCEPTION** (Tim's
    /// ruling). A file that states a rule its code breaks is worse than either
    /// answer: the next session reads the rule, believes it, and reinstates it
    /// from habit.</para>
    /// <para>**IT READS THE SOURCE FILE**, which is unusual and is the point. What
    /// this guards is not a behaviour but a contradiction, and a contradiction
    /// between a comment and the code beneath it is invisible to every test that
    /// only calls the code.</para>
    /// </remarks>
    [Fact]
    public void TheVocabularyNoLongerStatesTheRule()
    {
        var path = Path.Combine(
            RepoRoot(), "src", "Hamlet.App", "ViewModels", "Ft8Vocabulary.cs");

        Assert.True(File.Exists(path), "no Ft8Vocabulary.cs at " + path);

        var source = File.ReadAllText(path);

        _output.WriteLine(
            "read " + source.Length + " characters of " + Path.GetFileName(path));

        Assert.DoesNotContain(
            "NO PRONOUN CHOOSES A GENDER", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "no pronoun chooses", source, StringComparison.OrdinalIgnoreCase);

        // And it says which ruling settled it, so the next reader finds the
        // record rather than the habit.
        Assert.Contains("HM-DEC-159", source, StringComparison.Ordinal);
    }

    /// <summary>The ruling is in the decision log, in the log's own format.</summary>
    /// <remarks>
    /// **A DECISION THAT IS NOT IN `DECISIONS.md` IS NOT MADE** (§9.5). This is
    /// the half of task 3 that stops the rule being reinstated by a session that
    /// never saw the instruction.
    /// </remarks>
    [Fact]
    public void TheRulingIsInTheDecisionLog()
    {
        var log = File.ReadAllText(Path.Combine(RepoRoot(), "DECISIONS.md"));

        Assert.Contains("id: HM-DEC-159", log, StringComparison.Ordinal);
        Assert.Contains("date: 2026-09-07", log, StringComparison.Ordinal);

        // The newest ruling is at the top, which is the log's own rule.
        var at = log.IndexOf("id: HM-DEC-159", StringComparison.Ordinal);
        var previous = log.IndexOf("id: HM-DEC-158", StringComparison.Ordinal);

        Assert.True(at >= 0 && previous > at, "HM-DEC-159 is not above HM-DEC-158");
    }

    private static string Help(string message, string hisGrid)
    {
        var settings = new AppSettings();

        settings.Operator.GridSquare = hisGrid;

        return new MainWindowViewModel(settings, null)
            .AddDecodeRowForTests("214135", "-11", "0.2", "1240", message)
            .PayloadHelp;
    }

    /// <summary>The repository root, walking up from the test binary.</summary>
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
