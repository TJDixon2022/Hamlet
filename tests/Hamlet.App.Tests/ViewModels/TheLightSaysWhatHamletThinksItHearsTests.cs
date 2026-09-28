using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Tests.Views;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE LIGHT SAYS WHETHER HAMLET THINKS IT HEARS CW, IN WORDS** (work instruction 474
/// task 1, step 11 criterion 11.1, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**NO AUDIO.** The corpus is banned (R88), and the light shows the existing
/// detector's opinion rather than forming one, so the view model is driven with the
/// meter's verdict and the survey's admitted bins directly.</para>
/// <para>**THE WORDS CARRY IT AND THE COLOR ONLY AGREES** (§0.6), so every assertion is on
/// the words.</para>
/// </remarks>
public sealed class TheLightSaysWhatHamletThinksItHearsTests
{
    private static readonly KeyingCandidate Admitted = new(612.5, 60, 180, 3, 6, 18, 12, -40);

    /// <remarks>Proves a meter verdict of keying lights it and changes the words.</remarks>
    [Fact]
    public void AMeterVerdictOfKeyingChangesTheWords()
    {
        var light = new CwHearingViewModel();
        var before = light.LightWords;

        light.Observe(State(KeyingVerdict.Keying));

        Assert.Equal("I don't think I hear CW", before);
        Assert.True(light.IsLit, "the meter said keying and the light stayed dark");
        Assert.Equal("I think I hear CW", light.LightWords);
    }

    /// <remarks>Proves an admitted survey bin lights it with the meter still listening.</remarks>
    [Fact]
    public void AnAdmittedPitchLightsItWhileTheMeterIsStillListening()
    {
        var light = new CwHearingViewModel();

        light.Observe(State(KeyingVerdict.Listening, Admitted));

        Assert.True(light.IsLit, "the survey admitted a pitch and the light stayed dark");
        Assert.Equal("I think I hear CW", light.LightWords);
    }

    /// <remarks>Proves it goes dark again, in words, when neither says anything.</remarks>
    [Fact]
    public void ItGoesDarkInWordsWhenNeitherTheMeterNorTheSurveySaysAnything()
    {
        var light = new CwHearingViewModel();

        light.Observe(State(KeyingVerdict.Keying));
        light.Observe(State(KeyingVerdict.NoKeying));

        Assert.False(light.IsLit);
        Assert.Equal("I don't think I hear CW", light.LightWords);
    }

    /// <remarks>Proves the hover says what the light rests on and that it is a guess.</remarks>
    [Fact]
    public void TheHoverSaysWhatItRestsOnAndThatItIsAGuess()
    {
        Assert.Equal(
            "lit when the keying meter calls it keying or the survey admits a pitch; "
            + "this is Hamlet's guess, not a fact.",
            CwHearingViewModel.LightTip);
    }

    /// <remarks>
    /// Proves the light is on the CW tab, dark and in words, with its hover, in the built
    /// window. A binding that resolves and an element that is not there look the same to
    /// every other test (`TheCapturePressIsOnTheScreenTests`).
    /// </remarks>
    [AvaloniaFact]
    public void TheLightIsOnTheCwTabDarkAndInWords()
    {
        var (window, _) = TheControlsTimCanPress.Open();

        try
        {
            var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
            var dark = cw.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "CwHearingDark");
            var lit = cw.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "CwHearingLight");
            var words = dark.GetVisualDescendants().OfType<TextBlock>().Single();

            Assert.True(dark.IsEffectivelyVisible, "the dark light is not drawn on the CW tab");
            Assert.False(lit.IsVisible, "the light is lit with nothing listening");
            Assert.Contains(CwHearingViewModel.DarkWords, words.Inlines!.Text, StringComparison.Ordinal);
            Assert.Equal(CwHearingViewModel.LightTip, ToolTip.GetTip(dark));
        }
        finally
        {
            window.Close();
        }
    }

    private static CwHearingState State(KeyingVerdict verdict, params KeyingCandidate[] admitted)
        => new(
            new KeyingReading(verdict, verdict == KeyingVerdict.Keying ? 610 : 0, 70, 22, 30, 0.2, false),
            600,
            false,
            false,
            admitted);
}
