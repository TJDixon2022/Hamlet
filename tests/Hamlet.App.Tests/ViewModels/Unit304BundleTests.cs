using System;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 304 task 4: **the bundle is the whole picture, in one paste.**
/// </summary>
/// <remarks>
/// <para>**WHAT COPY DIAGNOSTICS CARRIED BEFORE THIS** was enough to say which build
/// he was on and **nothing whatever about what the machine was doing** - and every one
/// of the three failures of 2026-09-10 was a state nobody could see.</para>
/// <para>**IT IS ONE PASTE.** Not a folder, not a zip, not a path to go and find.
/// That is the instruction's rule and it is the whole difference between a bundle and
/// the telemetry folder, which already existed and which nobody opens.</para>
/// <para>**AND THE BUTTON'S PROMISE HAS TO SURVIVE THE GROWTH.** The About window
/// says the diagnostics carry *no callsign, no name and no location*; that was easy
/// to keep when the text was eight lines and is the thing to check now it is
/// hundreds.</para>
/// </remarks>
public sealed class Unit304BundleTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the bundle is quoted.</param>
    public Unit304BundleTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The bundle carries the state, not only the versions.**</summary>
    [Fact]
    public void TheBundleCarriesTheStateNotOnlyTheVersions()
    {
        var text = new AboutViewModel(Settings(), null).DiagnosticsText;

        _output.WriteLine(text);

        // The versions it always had.
        Assert.Contains("Hamlet ", text, StringComparison.Ordinal);
        Assert.Contains("OS: ", text, StringComparison.Ordinal);

        // **AND THE STATE, WHICH IS THE POINT.**
        Assert.Contains("--- state now ---", text, StringComparison.Ordinal);

        foreach (var fact in new[]
        {
            "audioOutputs", "transmitDeviceSelected", "transmitDevicePresent",
            "settingsNamedButAbsent", "telemetryCategoriesOff",
        })
        {
            Assert.Contains(fact, text, StringComparison.Ordinal);
        }
    }

    /// <summary>**It says what it does not know, in the snapshot's own words.**</summary>
    /// <remarks>
    /// **THE SAME WORD IN BOTH PLACES** (the instruction). A reader comparing the
    /// bundle against the startup snapshot is comparing like with like, and `unknown`
    /// means the same thing in each.
    /// </remarks>
    [Fact]
    public void ItSaysWhatItDoesNotKnowInTheSameWords()
    {
        var text = new AboutViewModel(Settings(), null).DiagnosticsText;

        var unknowns = text.Split('\n')
            .Where(l => l.Contains(": unknown", StringComparison.Ordinal))
            .ToList();

        foreach (var line in unknowns)
        {
            _output.WriteLine(line.Trim());
        }

        _output.WriteLine("");
        _output.WriteLine(unknowns.Count + " facts say unknown");

        // **AND EVERY ONE OF THEM IS FOLLOWED BY A REASON**, which is the rule the
        // whole unit turns on: not absent, not a plausible default.
        foreach (var line in unknowns)
        {
            var key = line.Split(':')[0].Trim();

            Assert.Contains(
                key + "Why", text, StringComparison.Ordinal);
        }
    }

    /// <summary>**It carries the run-up to the fault, and says how much.**</summary>
    [Fact]
    public void ItCarriesTheRunUpToTheFault()
    {
        var text = new AboutViewModel(Settings(), null).DiagnosticsText;

        _output.WriteLine(
            "the bundle asks for the last " + AboutViewModel.RecentLines + " lines");

        Assert.Contains(
            "--- last " + AboutViewModel.RecentLines + " telemetry lines ---",
            text,
            StringComparison.Ordinal);
    }

    /// <summary>**It is one paste, and this is how big.**</summary>
    /// <remarks>
    /// **TASK 6 ASKS WHAT IT COSTS AND THIS IS THE MEASUREMENT**, printed rather than
    /// asserted at a threshold, because a figure that fails a test on a machine with
    /// more sound cards would be a false red.
    /// </remarks>
    [Fact]
    public void ItIsOnePasteAndThisIsHowBig()
    {
        var text = new AboutViewModel(Settings(), null).DiagnosticsText;

        var lines = text.Split('\n').Length;

        _output.WriteLine("characters : " + text.Length);
        _output.WriteLine("lines      : " + lines);
        _output.WriteLine(
            "kilobytes  : " + (text.Length / 1024.0).ToString("0.0"));

        Assert.True(text.Length > 0);
    }

    /// <summary>**No callsign, no name, no location - the button's own promise.**</summary>
    /// <remarks>
    /// **THE ABOUT WINDOW SAYS IT IN SO MANY WORDS** and the bundle just grew from
    /// eight lines to hundreds. §2.1 does not bend because the text got longer.
    /// </remarks>
    [Fact]
    public void TheButtonsPromiseSurvivesTheGrowth()
    {
        var settings = Settings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.OperatorName = "Timothy Dixon";
        settings.Operator.Location = "Trafford, PA";
        settings.Operator.GridSquare = "FN00DJ";

        var text = new AboutViewModel(settings, null).DiagnosticsText;

        _output.WriteLine("bundle is " + text.Length + " characters");

        foreach (var personal in new[]
        {
            "KC3QIS", "Timothy", "Dixon", "Trafford", "FN00DJ",
        })
        {
            Assert.DoesNotContain(
                personal, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**A machine that cannot describe itself still gets a bundle.**</summary>
    /// <remarks>
    /// **THE BUNDLE IS FOR A BROKEN MACHINE**, so it has to survive one. Nothing here
    /// may throw out of the button.
    /// </remarks>
    [Fact]
    public void AMachineThatCannotDescribeItselfStillGetsABundle()
    {
        var text = new AboutViewModel(Settings(), null).DiagnosticsText;

        _output.WriteLine("it built, " + text.Length + " characters");

        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    private static AppSettings Settings()
        => new() { ReconnectOnStartup = false };
}
