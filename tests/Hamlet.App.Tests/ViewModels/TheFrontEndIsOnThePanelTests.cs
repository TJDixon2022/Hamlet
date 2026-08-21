using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Hamlet shows what the receiver's front end is doing, and says which button to
/// press when it is overloading.
/// </summary>
/// <remarks>
/// <para>**IT KNEW AND IT DID NOT SAY.** On 20 metres in daylight at S9 with
/// nothing readable, the radio was reporting `Overflow: overloading` with
/// preamp 1 on and RF gain at 100 per cent, Hamlet was reading all three, and the
/// operator found them in a text file the next day. Overload compresses the whole
/// passband together, so no tone stands above anything: measured on that
/// recording, 16 to 17 dB of envelope swing at **every** pitch from 450 to 700 Hz
/// where a real station gives 22 to 24 at one. The ear takes rhythm out of a
/// compressed mess and the decoder, which measures amplitude, cannot.</para>
/// <para>**READ ONLY.** Hamlet displays these and advises on them and does not
/// write them. Receive-path settings radiate nothing, so a write would be safe in
/// that narrow sense, but it is still the application changing his radio
/// underneath him, and mode-follow writing unprompted cost an evening and a
/// ruling.</para>
/// <para>**EVERY TEST HERE DRIVES RIG STATE DIRECTLY** (HM-DEC-093).</para>
/// </remarks>
public sealed class TheFrontEndIsOnThePanelTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public TheFrontEndIsOnThePanelTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime When
        = new(2026, 8, 21, 18, 38, 48, DateTimeKind.Utc);

    /// <summary>A rig state with the front end in a given condition.</summary>
    /// <param name="overflow">1 for overloading, 0 for clear, null for never read.</param>
    /// <param name="preamp">0, 1 or 2, or null for never read.</param>
    /// <param name="attenuator">0 or 20, or null for never read.</param>
    private static RigState FrontEnd(int? overflow, int? preamp, int? attenuator)
    {
        var values = new Dictionary<RigField, RigValue>();

        if (overflow is { } o)
        {
            values[RigField.Overflow] = RigValue.Known(
                RigField.Overflow, o, o == 1 ? "overloading" : "clear", When, "15 07");
        }

        if (preamp is { } p)
        {
            values[RigField.Preamp] = RigValue.Known(
                RigField.Preamp,
                p,
                p == 0 ? "preamp off" : $"preamp {p}",
                When,
                "16 02");
        }

        if (attenuator is { } a)
        {
            values[RigField.Attenuator] = RigValue.Known(
                RigField.Attenuator,
                a,
                a == 0 ? "attenuator off" : $"{a} dB attenuator",
                When,
                "11");
        }

        return new RigState(values, "IC-7300");
    }

    /// <remarks>
    /// <para>Proves the first: **overflow asserted puts the message on screen and
    /// overflow clearing takes it away.**</para>
    /// </remarks>
    [Fact]
    public void OverflowPutsTheMessageUpAndClearingTakesItDown()
    {
        var overloading = Advice(FrontEnd(overflow: 1, preamp: 1, attenuator: 0));
        var clear = Advice(FrontEnd(overflow: 0, preamp: 1, attenuator: 0));

        _output.WriteLine($"overloading: '{overloading}'");
        _output.WriteLine($"clear:       '{clear}'");

        Assert.NotEqual("", overloading);
        Assert.Equal("", clear);

        // And an operator who has never thought about front-end overload is told
        // which button, not which concept.
        Assert.Contains("P.AMP/ATT", overloading, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the second: **the preamp is named while it is on, and the
    /// attenuator only once it is off.** Advice about a knob already in the right
    /// position is noise.</para>
    /// </remarks>
    [Theory]
    [InlineData(1, "preamp", "attenuator")]
    [InlineData(2, "preamp", "attenuator")]
    [InlineData(0, "attenuator", null)]
    public void ThePreampComesFirstAndTheAttenuatorOnlyAfterIt(
        int preamp, string names, string? doesNotName)
    {
        var advice = Advice(FrontEnd(overflow: 1, preamp: preamp, attenuator: 0));

        _output.WriteLine($"preamp {preamp}: '{advice}'");

        Assert.Contains(names, advice, StringComparison.OrdinalIgnoreCase);

        if (doesNotName is not null)
        {
            Assert.DoesNotContain(doesNotName, advice, StringComparison.OrdinalIgnoreCase);
        }

        // Nothing advises on RF gain, whose read the operator has watched
        // contradict his own radio.
        Assert.DoesNotContain("RF gain", advice, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// <para>Proves the third: **a setting never read says so** (HM-DEC-009). Not
    /// a blank and not a default, because a panel asserting the preamp is off
    /// when the read failed is worse than one saying it does not know.</para>
    /// </remarks>
    [Fact]
    public void ASettingNeverReadSaysUnknown()
    {
        var chip = Chip(FrontEnd(overflow: null, preamp: null, attenuator: null));

        _output.WriteLine($"'{chip}'");

        Assert.Contains("unknown", chip, StringComparison.Ordinal);
        Assert.DoesNotContain("off", chip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the fourth, and it is the one Tim ruled: **nothing this unit
    /// added writes to the radio.** Not the preamp, not the attenuator, not RF
    /// gain, not as a fallback and not behind a flag. Receive-path settings
    /// radiate nothing, so a write would be safe in that narrow sense, and it
    /// would still be the application changing his radio underneath him.</para>
    /// <para>Checked by sweeping the application's own source for a write of any
    /// of the three fields, in the shape this project already uses to prove no
    /// telemetry payload can be handed a callsign.</para>
    /// </remarks>
    [Fact]
    public void NothingHereWritesToTheRadio()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
               && !Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        var writes = new Regex(
            @"(WriteAsync|CivWrites)\s*[.(].*(Preamp|Attenuator|RfGain)",
            RegexOptions.Compiled);

        var offenders = new List<string>();

        foreach (var file in Directory.GetFiles(
            Path.Combine(directory!.FullName, "src", "Hamlet.App"),
            "*.cs",
            SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
            {
                if (writes.IsMatch(lines[i]))
                {
                    offenders.Add($"{Path.GetFileName(file)}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        foreach (var offender in offenders)
        {
            _output.WriteLine($"OFFENDER {offender}");
        }

        Assert.Empty(offenders);
    }

    /// <summary>What the advice says for a given rig state.</summary>
    /// <remarks>
    /// The view model's own logic, driven through the two facts it reads, so this
    /// tests the rule rather than a copy of it.
    /// </remarks>
    private static string Advice(RigState state)
    {
        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var preamp = state[RigField.Preamp];
        var preampOn = preamp is { IsKnown: true } && preamp.Number is 1 or 2;

        return Hamlet.App.ViewModels.MainWindowViewModel.OverflowAdviceFor(
            overloading, preampOn);
    }

    /// <summary>What the chip beside the filter width reads.</summary>
    private static string Chip(RigState state)
    {
        var overloading = state[RigField.Overflow] is { IsKnown: true, Number: 1 };
        var preamp = state[RigField.Preamp];
        var attenuator = state[RigField.Attenuator];

        return Hamlet.App.ViewModels.MainWindowViewModel.FrontEndTextFor(
            overloading,
            preamp.IsKnown ? preamp.Text : "preamp unknown",
            attenuator.IsKnown ? attenuator.Text : "attenuator unknown");
    }
}
