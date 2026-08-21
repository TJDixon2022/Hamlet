using System.Text.RegularExpressions;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Mode-follow works again, and the front end says which setting is which.
/// </summary>
/// <remarks>
/// <para>**THE CAUSE WAS ONE WORD OF EVIDENCE.** The guard added on 2026-08-18
/// says nothing takes the operator out of Morse while he is working Morse, and it
/// is right. What it read as "working Morse" was `IsDecoding`, which is true from
/// the moment the decoder starts listening until it stops — the whole session. So
/// **every target that was not CW was refused, permanently**, and the radio stayed
/// in CW at 14.243 MHz where the map says upper sideband.</para>
/// <para>**THE GUARD STAYS AND THE EVIDENCE CHANGES.** On 2026-08-18 mode-follow
/// wrote USB with the data variant on, over and over, while the operator sat on CW
/// main street with a signal decoding, and the send controls refused for
/// sixty-six seconds: he could not answer a station because the app had moved his
/// radio out from under him. What is asked now is whether the dial is inside a CW
/// segment, or whether a character has actually come through recently.</para>
/// <para>**EVERY TEST HERE DRIVES THE STATE DIRECTLY** (HM-DEC-093).</para>
/// </remarks>
public sealed class ModeFollowsTheMapAgainTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the decisions are printed.</param>
    public ModeFollowsTheMapAgainTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The SSB portion of 20 metres, where he was.</summary>
    private const long PhoneHz = 14_243_000;

    /// <summary>CW main street on 20 metres, where the 18th's incident was.</summary>
    private const long MorseHz = 14_050_000;

    private static readonly ModeTarget Phone
        = new(CivMode.Usb, false, "which is how voice is worked up here");

    private static readonly ModeTarget Morse
        = new(CivMode.Cw, false, "which is what this stretch is for");

    /// <remarks>
    /// <para>Proves the bug and the fix in one: **tuning into the phone portion
    /// with the decoder merely switched on now writes the mode**, where the old
    /// evidence refused it forever.</para>
    /// </remarks>
    [Fact]
    public void TuningIntoThePhonePortionWritesTheMode()
    {
        var decision = ModeFollowPlan.Decide(
            ModeFollowState.Armed(true),
            currentMode: CivMode.Cw,
            currentDataMode: false,
            target: Phone,
            frequencyHz: PhoneHz,
            workingCw: false);

        _output.WriteLine($"write {decision.Write}, {decision.Mode}, '{decision.Narration}'");

        Assert.True(decision.Write);
        Assert.Equal(CivMode.Usb, decision.Mode);
        Assert.False(decision.DataMode);

        // And it says so, because a radio that changes mode with no explanation
        // is its own confident wrong answer (HM-DEC-056).
        Assert.NotEqual("", decision.Narration);
    }

    /// <remarks>
    /// <para>Proves the 18th's protection is intact: **a dial inside a CW segment
    /// is the operator's own hand** and nothing moves him off it.</para>
    /// </remarks>
    [Fact]
    public void NothingTakesHimOutOfMorseWhileHeIsWorkingMorse()
    {
        var decision = ModeFollowPlan.Decide(
            ModeFollowState.Armed(true),
            currentMode: CivMode.Cw,
            currentDataMode: false,
            target: new ModeTarget(CivMode.Usb, true, "the digital block"),
            frequencyHz: MorseHz,
            workingCw: true);

        _output.WriteLine($"write {decision.Write}");

        Assert.False(decision.Write);
    }

    /// <remarks>
    /// <para>Proves the snap-back guard: **a second decision at the same
    /// frequency for the same mode writes nothing.** That guard is what stopped
    /// eighteen writes going out in one evening with the dial standing still, and
    /// this unit did not weaken it.</para>
    /// </remarks>
    [Fact]
    public void ASnapBackDoesNotWriteAgain()
    {
        var armed = ModeFollowState.Armed(true);

        var first = ModeFollowPlan.Decide(
            armed, CivMode.Cw, false, Phone, PhoneHz, workingCw: false);

        Assert.True(first.Write);

        // The radio confirmed it, so the state remembers where and what.
        var after = armed.Done(PhoneHz, first.Mode, first.DataMode);

        // The stale frequency arrives again and the mode has not caught up yet,
        // which is exactly the shape of the snap-back.
        var second = ModeFollowPlan.Decide(
            after, CivMode.Cw, false, Phone, PhoneHz, workingCw: false);

        _output.WriteLine($"first {first.Write}, second {second.Write}");

        Assert.False(second.Write);
    }

    /// <remarks>
    /// <para>Proves the operator's own hand still wins: **a mode he set is not
    /// overwritten**, until the next band change re-arms it.</para>
    /// </remarks>
    [Fact]
    public void AModeTheOperatorSetIsNotOverwritten()
    {
        var suspended = ModeFollowState.Armed(true).SuspendedByOperator();

        var decision = ModeFollowPlan.Decide(
            suspended, CivMode.Cw, false, Phone, PhoneHz, workingCw: false);

        Assert.False(decision.Write);

        // And a band change is a fresh start rather than a continuation.
        var rearmed = suspended.Rearmed();

        Assert.True(ModeFollowPlan
            .Decide(rearmed, CivMode.Cw, false, Phone, PhoneHz, workingCw: false)
            .Write);
    }

    /// <remarks>
    /// <para>Proves the ruling: **mode-follow writes the mode and nothing else.**
    /// A sweep of the follow path for any other write — frequency, filter, power,
    /// gain, preamp, attenuator — finds none.</para>
    /// </remarks>
    [Fact]
    public void NothingButTheModeIsEverWritten()
    {
        var source = SourceOf("MainWindowViewModel.cs");
        // **BOUNDED AT THE NEXT METHOD**, so this cannot pass by sweeping
        // the rest of the file and finding nothing there either.
        var follow = Between(
            source,
            "private async Task FollowTheMapAsync()",
            "private void OnAgeTick(");

        _output.WriteLine($"{follow.Split('\n').Length} lines of the follow path");

        Assert.True(
            follow.Length < 6_000,
            "the follow path sweep has lost its bound and is measuring the "
            + "wrong thing");

        Assert.Contains("SetModeAsync", follow, StringComparison.Ordinal);

        foreach (var forbidden in new[]
                 {
                     "SetFrequencyAsync", "SetFilter", "SetPower",
                     "SetGain", "SetPreamp", "SetAttenuator", "WriteAsync",
                 })
        {
            Assert.DoesNotContain(forbidden, follow, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// <para>Proves both tuning paths reach it. The dial arrives as a frequency
    /// the radio reported and the band-map click arrives as one Hamlet asked for,
    /// and both land in the same property; a band button takes its own route.
    /// </para>
    /// </remarks>
    [Fact]
    public void BothTuningPathsReachModeFollow()
    {
        var source = SourceOf("MainWindowViewModel.cs");

        var frequency = Between(
            source, "partial void OnFrequencyHzChanged(long value)", "\n    }\n");

        var band = Between(
            source,
            "partial void OnSelectedBandChanged(BandButtonViewModel value)",
            "\n    }\n");

        Assert.Contains("ScheduleModeFollow()", frequency, StringComparison.Ordinal);
        Assert.Contains("ScheduleModeFollow()", band, StringComparison.Ordinal);

        // And the frequency the radio reports goes through the same property, so
        // the dial is not a separate path that could be missed.
        Assert.Contains("ApplyRigFrequency", source, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves task 3's own defect: **every front-end reading carries its own
    /// name.** The chip read `off · off`, which says two things are off and
    /// nothing about which two, because the radio's own word for both is "off".
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(0, 0, "preamp off · att off")]
    [InlineData(1, 0, "preamp 1 · att off")]
    [InlineData(2, 20, "preamp 2 · att 20 dB")]
    [InlineData(null, null, "preamp unknown · att unknown")]
    public void EveryFrontEndReadingCarriesItsOwnName(
        int? preamp, int? attenuator, string expected)
    {
        var text = MainWindowViewModel.FrontEndTextFor(
            false,
            MainWindowViewModel.PreampLabel(preamp),
            MainWindowViewModel.AttenuatorLabel(attenuator));

        _output.WriteLine($"'{text}'");

        Assert.Equal(expected, text);

        // "on" would not do: preamp 1 and preamp 2 are different settings on this
        // radio and an operator judging his own front end needs to know which.
        Assert.DoesNotContain("· on", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves a setting never read says so rather than defaulting to off
    /// (HM-DEC-009).</para>
    /// </remarks>
    [Fact]
    public void ASettingNeverReadSaysUnknown()
    {
        Assert.Equal("preamp unknown", MainWindowViewModel.PreampLabel(null));
        Assert.Equal("att unknown", MainWindowViewModel.AttenuatorLabel(null));
    }

    /// <remarks>
    /// <para>Proves the front end is on the terminal panel and not only in the
    /// diagnostics dialog, which is what the previous unit was asked for and what
    /// the operator could not find.</para>
    /// </remarks>
    [Fact]
    public void TheFrontEndIsOnTheTerminalPanel()
    {
        var window = SourceOf("MainWindow.axaml", "Views");
        var terminal = Between(window, "x:Key=\"widget.terminal\"", "x:Key=\"widget.send\"");

        _output.WriteLine($"{terminal.Split('\n').Length} lines of the terminal panel");

        Assert.Contains("FrontEndText", terminal, StringComparison.Ordinal);
        Assert.Contains("OverflowAdvice", SourceOf("MainWindowViewModel.cs"),
            StringComparison.Ordinal);
    }

    private static string SourceOf(string name, string folder = "ViewModels")
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
               && !Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return File.ReadAllText(
            Path.Combine(directory!.FullName, "src", "Hamlet.App", folder, name));
    }

    private static string Between(string source, string from, string to)
    {
        var start = source.IndexOf(from, StringComparison.Ordinal);

        Assert.True(start >= 0, $"'{from}' is no longer in the source");

        var end = source.IndexOf(to, start, StringComparison.Ordinal);

        return end < 0 ? source[start..] : source[start..end];
    }
}
