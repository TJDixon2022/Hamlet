using Avalonia.Headless.XUnit;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **TUNED OUTSIDE HIS PRIVILEGES, THE MAP, THE NEIGHBORHOOD PANEL AND THE RADIO PANEL STAY IN THEIR
/// COLUMNS; ONLY THE PANEL'S WORDS AND COLOR CHANGE** (work instruction 423, step 6 criterion 6.3).
/// </summary>
/// <remarks>
/// <para>Tim reported it at the radio (R62): tuned to a frequency his license does not cover, the
/// window rearranged itself. A General operator on the CW tab on 20 m is tuned by the rig face's own
/// write to 14.050 MHz, which General's row 14025000 to 14150000 covers (97.301(d)), and then to
/// 14.010 MHz, which on 20 m only Extra's row covers (97.301(b)).</para>
/// <para>**THE SIX FIGURES ARE READ OFF THE LAID-OUT WINDOW**: the left edge and the width of the
/// card, the sun map and the rig face, at both frequencies. More than one pixel between them fails
/// and names the panel and both figures. **And a panel frozen still fails**: the tone, the green
/// block's background and its words must each differ between the two frequencies, because 6.3 says
/// they still change.</para>
/// <para>**THE SIZES** are the one the window opens at, the smallest task 1 saw the panels move at,
/// the 1400 anchor where the map left the band's left edge, and 1920, where the layout never
/// settled.</para>
/// </remarks>
public sealed class ThePanelsHoldTheirColumnsOutsideHisPrivilegesTests
{
    /// <summary>The sizes measured.</summary>
    public static readonly (double Width, double Height)[] Sizes =
    {
        TheTopRowTuned.DefaultSize, (1280, 720), (1400, 1040), (1920, 1040),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where each size's figures are printed.</param>
    public ThePanelsHoldTheirColumnsOutsideHisPrivilegesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The columns hold and the words and color change, at every size.</summary>
    [AvaloniaFact]
    public void TheColumnsHoldAndOnlyThePanelsWordsAndColorChange()
    {
        var misses = new List<string>();

        foreach (var (width, height) in Sizes)
        {
            var at = $"{width} x {height}";
            var (window, panel) = TheTopRowTuned.Open(width, height, LicenseClass.General);

            try
            {
                var said = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
                var covered = TheTopRowTuned.Read(window, panel);

                if (said is not null)
                {
                    misses.Add($"{at}: tuned to 14.050 MHz the layout did not settle - {said}");
                }

                said = TheTopRowTuned.TryTune(window, TheTopRowTuned.NotCovered);
                var outside = TheTopRowTuned.Read(window, panel);

                if (said is not null)
                {
                    misses.Add($"{at}: tuned to 14.010 MHz the layout did not settle - {said}");
                }

                _output.WriteLine($"{at}: tone {covered.Tone} -> {outside.Tone}");
                Column(misses, at, "the neighborhood panel", covered.Card, outside.Card);
                Column(misses, at, "the sun map", covered.Map, outside.Map);
                Column(misses, at, "the radio panel", covered.Rig, outside.Rig);

                // **AND BACK AGAIN, TO WHERE THEY STARTED** (added at unit 423 task 3): task 1's trace
                // after the first change had the map come back from 14.010 to 327 x 178 beside the
                // card at 1400 x 1040, where it had stood at 393 x 214 at the band's left edge.
                said = TheTopRowTuned.TryTune(window, TheTopRowTuned.Covered);
                var back = TheTopRowTuned.Read(window, panel);

                if (said is not null)
                {
                    misses.Add($"{at}: tuned back to 14.050 MHz the layout did not settle - {said}");
                }

                _output.WriteLine($"{at}: and back to 14.050 MHz");
                Column(misses, at, "the neighborhood panel", covered.Card, back.Card, "back at 14.050 MHz from 14.010");
                Column(misses, at, "the sun map", covered.Map, back.Map, "back at 14.050 MHz from 14.010");
                Column(misses, at, "the radio panel", covered.Rig, back.Rig, "back at 14.050 MHz from 14.010");

                if (covered.Tone == outside.Tone)
                {
                    misses.Add($"{at}: the privilege panel's tone is {covered.Tone} at both frequencies");
                }

                if (covered.Background == outside.Background)
                {
                    misses.Add($"{at}: the privilege panel's color is {covered.Background} at both frequencies");
                }

                if (Words(covered).SequenceEqual(Words(outside)))
                {
                    misses.Add($"{at}: the privilege panel says the same words at both frequencies");
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    private void Column(List<string> misses, string at, string name, PanelBounds covered, PanelBounds outside)
        => Column(misses, at, name, covered, outside, "at 14.010 MHz");

    private void Column(List<string> misses, string at, string name, PanelBounds covered, PanelBounds then, string thenAt)
    {
        _output.WriteLine(
            $"  {name}: left {TheTopRowTuned.Px(covered.Left)} -> {TheTopRowTuned.Px(then.Left)}, "
            + $"width {TheTopRowTuned.Px(covered.Width)} -> {TheTopRowTuned.Px(then.Width)}");

        if (Math.Abs(then.Left - covered.Left) > 1)
        {
            misses.Add(
                $"{at}: {name}'s left edge is {TheTopRowTuned.Px(covered.Left)} at 14.050 MHz and "
                + $"{TheTopRowTuned.Px(then.Left)} {thenAt}");
        }

        if (Math.Abs(then.Width - covered.Width) > 1)
        {
            misses.Add(
                $"{at}: {name}'s width is {TheTopRowTuned.Px(covered.Width)} at 14.050 MHz and "
                + $"{TheTopRowTuned.Px(then.Width)} {thenAt}");
        }
    }

    /// <summary>The words alone, without the widths they were drawn at.</summary>
    private static IEnumerable<string> Words(TopRowReading reading)
        => reading.Sentences.Select(s => s[..s.IndexOf(" drawn ", StringComparison.Ordinal)]);
}
