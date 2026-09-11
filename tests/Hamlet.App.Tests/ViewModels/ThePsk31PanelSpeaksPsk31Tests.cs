using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 322 task 4: **the panel stops speaking FT8 under PSK31.**
/// </summary>
/// <remarks>
/// <para>**THREE LEFTOVERS FROM THE OPERATOR'S OWN SCREENSHOT** of 2026-09-11, 17:22
/// UTC. The waterfall header read *200-3000 Hz · 15 s slots*; the empty decoded panel
/// read *every message that comes out of a slot lands here*; and the neighbourhood map's
/// PSK31 outline was not lit with the tab selected.</para>
/// <para>**PSK31 HAS NO SLOTS** (`PHASE_PLAN.md` §3). It is a continuous carrier carrying
/// free text, and a caption saying otherwise is §0.0 broken by a sentence under a picture
/// - it tells the operator to wait for a boundary that never comes.</para>
/// <para>**COMPUTED, NOT SEEN.** These read view-model strings and a rule the render
/// asks. Nothing here looks at a pixel.</para>
/// </remarks>
public sealed class ThePsk31PanelSpeaksPsk31Tests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the strings are printed.</param>
    public ThePsk31PanelSpeaksPsk31Tests(ITestOutputHelper output) => _output = output;

    /// <summary>**Nothing bound on the PSK31 panel says slot.**</summary>
    [Fact]
    public void NothingBoundOnThePsk31PanelSaysSlot()
    {
        var model = Panel("PSK31");

        var bound = new (string Name, string Text)[]
        {
            ("waterfall header", model.DigitalWaterfallSummary),
            ("decoded idle", model.DigitalDecodedIdle),
            ("mode strip", model.DigitalModeStripLine),
            ("decoded summary", model.DigitalDecodedSummary),
        };

        foreach (var (name, text) in bound)
        {
            _output.WriteLine(name.PadRight(18) + ": " + text);
        }

        foreach (var (name, text) in bound)
        {
            Assert.DoesNotContain("slot", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**And FT8 still says it, because FT8 has them.**</summary>
    /// <remarks>
    /// **THE HALF A CHANGE LIKE THIS MOST EASILY BREAKS.** Taking the word out
    /// everywhere would leave the mode that really does run in fifteen-second slots
    /// unable to say so.
    /// </remarks>
    [Fact]
    public void AndFt8StillSaysItBecauseFt8HasThem()
    {
        var model = Panel("FT8");

        _output.WriteLine("waterfall header  : " + model.DigitalWaterfallSummary);
        _output.WriteLine("decoded idle      : " + model.DigitalDecodedIdle);

        Assert.Contains("slot", model.DigitalWaterfallSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("slot", model.DigitalDecodedIdle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**The header says the passband and the mode.**</summary>
    [Fact]
    public void TheHeaderSaysThePassbandAndTheMode()
    {
        var model = Panel("PSK31");

        var header = model.DigitalWaterfallSummary;

        _output.WriteLine("header: " + header);

        Assert.Contains("PSK31", header, StringComparison.Ordinal);
        Assert.Contains("Hz", header, StringComparison.Ordinal);
    }

    /// <summary>**The empty panel says what a line here will be.**</summary>
    [Fact]
    public void TheEmptyPanelSaysWhatALineHereWillBe()
    {
        var model = Panel("PSK31");

        var idle = model.DigitalDecodedIdle;

        _output.WriteLine("idle: " + idle);

        // **A STATION, NOT A MESSAGE** (`PHASE_PLAN.md` §3.4). Text arrives a character
        // at a time and a line belongs to a signal, not to a slot.
        Assert.Contains("station", idle, StringComparison.OrdinalIgnoreCase);

        // **AND IT SAYS THE SQUELCH IS THE REASON A LINE IS NOT THERE**, which is the
        // question the operator asked about his own empty list.
        Assert.Contains("sure", idle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**No FT8 decoder runs and no slot grid is cut under PSK31.**</summary>
    /// <remarks>
    /// <para>**REHOMED FROM `ThePsk31TabIsInertTests` BY WORK INSTRUCTION 323 TASK 1**, which
    /// retires that class. Its premise - *the PSK31 tab does nothing* - ends the moment the
    /// send door opens, and a class named for a premise that has expired teaches the next
    /// reader something untrue. **These two assertions did not expire**: PSK31 is not FT8, so
    /// FT8's decoder must not run on it and FT8's fifteen-second grid must not be cut for it,
    /// whether or not Hamlet can now answer in it.</para>
    /// <para>**NOT HIDDEN - NOT RUNNING.** The slot watch is what cuts audio into FT8 slots
    /// and hands them to the decoder; under PSK31 it must not be asked for a slot at all.</para>
    /// </remarks>
    [Fact]
    public void NoDecoderRunsAndNoSlotGridIsCutUnderPsk31()
    {
        var model = Panel("PSK31", Heard());

        Assert.Equal("PSK31", model.ChosenDigitalMode);

        // **THIRTY LOOKS, WHICH IS SEVEN AND A HALF SECONDS OF TICKS.**
        for (var i = 0; i < 30; i++)
        {
            model.LookForASlotForTests();
        }

        _output.WriteLine("rows on the list : " + model.DigitalDecodes.Count);
        _output.WriteLine("slot looks       : " + model.SlotLooksForTests);
        _output.WriteLine("slots read       : " + model.SlotsReadForTests);

        Assert.Empty(model.DigitalDecodes);
        Assert.Empty(model.DigitalVisibleDecodes);

        // **THE WATCH WAS NEVER ASKED**, which is *not running* rather than running and
        // finding nothing.
        Assert.Equal(0, model.SlotLooksForTests);
        Assert.Equal(0, model.SlotsReadForTests);
    }

    /// <summary>**And FT8 and FT4 still reach the watch on the same audio.**</summary>
    /// <remarks>
    /// **THE HALF A CHANGE LIKE THIS MOST EASILY BREAKS.** Silencing a mode by silencing
    /// the tick would silence all four, and the two that work would go with it.
    /// </remarks>
    [Fact]
    public void Ft8AndFt4AreUntouched()
    {
        foreach (var mode in new[] { "FT8", "FT4" })
        {
            var model = Panel(mode, Heard());

            for (var i = 0; i < 30; i++)
            {
                model.LookForASlotForTests();
            }

            _output.WriteLine(mode + ": slot looks " + model.SlotLooksForTests
                + ", slots read " + model.SlotsReadForTests);

            // **THE WATCH IS ASKED ON EVERY TICK.** Thirty ticks inside a millisecond
            // close no slot on any mode - the boundary is fifteen seconds away - so what
            // is asserted is that the tick reaches the watch at all, which is the thing
            // PSK31 must not do and these two must.
            Assert.Equal(30, model.SlotLooksForTests);
        }
    }

    /// <summary>A tap with real audio in it, because the slot look needs one.</summary>
    private static AudioTap Heard()
    {
        var tap = new AudioTap();

        tap.Take(new float[12_000], 12_000);

        return tap;
    }

    /// <summary>**The outline for the chosen mode is the one lit.**</summary>
    [Fact]
    public void TheOutlineForTheChosenModeIsTheOneLit()
    {
        var model = Panel("PSK31");

        var twenty = model.Bands.FirstOrDefault(b => b.Band.Name == "20 m");

        Assert.NotNull(twenty);

        model.SelectedBand = twenty!;
        model.ChooseDigitalModeCommand.Execute("PSK31");

        var lit = model.Neighborhoods
            .Where(h => NeighborhoodMapControl.IsChosen(h, model.ChosenDigitalMode))
            .ToList();

        foreach (var hood in model.Neighborhoods)
        {
            _output.WriteLine(
                (NeighborhoodMapControl.IsChosen(hood, model.ChosenDigitalMode) ? "* " : "  ")
                + hood.ShortName.PadRight(8) + hood.LowHz + " to " + hood.HighHz);
        }

        var one = Assert.Single(lit);

        Assert.Equal("PSK31", one.ShortName);
        Assert.Equal(14_070_000, one.JumpHz);
    }

    private MainWindowViewModel Panel(string mode, AudioTap? tap = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = tap ?? new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ChooseDigitalModeCommand.Execute(mode);

        // **A SPECTRUM, SO THE HEADER HAS SOMETHING TO DESCRIBE.** Without one it reads
        // *not listening yet*, which says nothing about slots either way and would let
        // this pass for the wrong reason.
        model.DigitalSpectrum = new AudioSpectrumSource(8000);

        return model;
    }
}
