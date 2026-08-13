using System.Reflection;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The shell's half of HM-DEC-026 and HM-DEC-027: the simulated label is
/// derived and cannot be set, no setting can switch it off, and the field
/// guide offers the samples that teach.
/// </summary>
public sealed class TrainingRadioTests
{
    /// <remarks>
    /// Proves the label is read-only all the way up. The engine test shows
    /// the source cannot lie about being simulated; this shows the shell
    /// cannot override it — together they are the guarantee that there is no
    /// code path putting synthetic signals on screen unlabeled.
    /// </remarks>
    [Theory]
    [InlineData(nameof(MainWindowViewModel.SignalsAreSimulated))]
    [InlineData(nameof(MainWindowViewModel.SpectrumNotice))]
    [InlineData(nameof(MainWindowViewModel.WaterfallSummary))]
    public void DerivedSimulationState_CannotBeSet(string propertyName)
    {
        var property = typeof(MainWindowViewModel).GetProperty(
            propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.True(property is not null, $"{propertyName} should exist");
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite, $"{propertyName} must be derived, not assignable");
        Assert.Null(property.SetMethod);
    }

    /// <remarks>
    /// Proves there is no user setting that could put fake signals on screen
    /// unlabeled. HM-DEC-026 rules out a practice mode and a watermark
    /// toggle, and the way to keep that true a year from now is to fail a
    /// test the moment such a switch is added to the settings file.
    /// </remarks>
    [Fact]
    public void NoSetting_ControlsWhetherSignalsAreLabeled()
    {
        var suspicious = typeof(AppSettings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.Contains("Simulat", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Contains("Practice", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Contains("Watermark", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Contains("Training", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Name)
            .ToList();

        Assert.True(
            suspicious.Count == 0,
            "settings must not carry a switch over the simulated label; found: "
            + string.Join(", ", suspicious));
    }

    /// <remarks>
    /// Proves the training radio is named as a feature in the one string the
    /// operator actually reads — the port list.
    /// </remarks>
    [Fact]
    public void PortList_NamesTheTrainingRadio()
    {
        Assert.Equal("Training radio (no hardware)", MainWindowViewModel.TrainingRadio);
        Assert.DoesNotContain(
            "simulated", MainWindowViewModel.TrainingRadio, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "fake", MainWindowViewModel.TrainingRadio, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves CW offers a copy-speed ladder rather than one sample. Three
    /// speeds is how somebody finds the speed they can actually read, which
    /// is the groundwork FG-002 needs (HM-DEC-027).
    /// </remarks>
    [Fact]
    public void CwCard_OffersThreeSpeeds()
    {
        var cw = new ModeCardViewModel(ModeGuide.Modes.First(m => m.Name == "CW"));

        Assert.Equal(3, cw.Samples.Count);
        Assert.All(cw.Samples, s => Assert.Equal(TrainingMode.Cw, s.Request.Mode));
        Assert.Equal(
            new[] { 12, 18, 25 },
            cw.Samples.Select(s => s.Request.WordsPerMinute).ToArray());
    }

    /// <remarks>
    /// Proves SSB offers tuned and mistuned side by side. Hearing those two
    /// back to back is the fastest way to learn what the tuning knob does,
    /// and the field guide already promises "duck talk until tuned".
    /// </remarks>
    [Fact]
    public void SsbCard_OffersTunedAndMistuned()
    {
        var ssb = new ModeCardViewModel(ModeGuide.Modes.First(m => m.Name == "SSB"));

        Assert.Equal(2, ssb.Samples.Count);
        Assert.Contains(ssb.Samples, s => !s.Request.Mistuned);
        Assert.Contains(ssb.Samples, s => s.Request.Mistuned);
    }

    /// <remarks>
    /// Proves every field-guide entry can be heard and has a fingerprint to
    /// animate — including JS8, which shares FT8's machinery and therefore
    /// its picture and its sound.
    /// </remarks>
    [Fact]
    public void EveryModeCard_HasSomethingToHearAndToSee()
    {
        foreach (var mode in ModeGuide.Modes)
        {
            var card = new ModeCardViewModel(mode);

            Assert.True(card.HasSamples, $"{mode.Name} should offer a sample");
            Assert.All(card.Samples, s => Assert.False(string.IsNullOrWhiteSpace(s.Label)));

            // The fingerprint resolves to a synthesisable mode.
            var training = ModeFingerprintControl.ModeFor(mode.Signature);
            Assert.True(Enum.IsDefined(training));
        }

        Assert.Equal(
            TrainingMode.Ft8,
            ModeFingerprintControl.ModeFor(
                ModeGuide.Modes.First(m => m.Name == "JS8").Signature));
    }

    /// <remarks>
    /// Proves the waterfall's palette covers the whole amplitude range with
    /// opaque colors, and gets brighter with signal — a ramp that dimmed in
    /// the middle would make a strong signal look weak.
    /// </remarks>
    [Fact]
    public void Palette_IsOpaqueAndBrightensWithAmplitude()
    {
        var table = WaterfallPalette.Lookup();

        Assert.Equal(WaterfallPalette.Size, table.Length);

        static int Luma(int bgra)
        {
            var r = (bgra >> 16) & 0xFF;
            var g = (bgra >> 8) & 0xFF;
            var b = bgra & 0xFF;
            return (r * 2) + (g * 3) + b;
        }

        Assert.All(table, c => Assert.Equal(0xFF, (c >> 24) & 0xFF));
        Assert.True(Luma(table[255]) > Luma(table[128]));
        Assert.True(Luma(table[128]) > Luma(table[0]));
    }
}
