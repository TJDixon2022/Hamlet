using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Headless.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 283, task 4: the same shape, looked for once more.
/// </summary>
/// <remarks>
/// <para>**RUNTIME-COMPOSED TEXT IS INVISIBLE TO A SOURCE SEARCH**, and that is why
/// the last four units each fixed one instance and each missed the next. So this
/// looks for the shape rather than the string: **every sentence-length thing any
/// permanently-visible surface actually draws**, listed with the surface it is on,
/// so a person can check each one against the source and see which ones a grep
/// cannot find.</para>
/// <para>**IT REPORTS WHAT IT LOOKED AT, NOT ONLY WHAT IT FOUND** — unit 276's
/// context-menu sweep and unit 279's finder sweep set that pattern. Every capped
/// surface is walked, including the Digital tab in its working state, and the count
/// of blocks examined is printed whether or not any of them is a paragraph.</para>
/// <para>**IT FIXES NOTHING.** The order says so in as many words, and taking on a
/// second instance is how a unit stops finishing.</para>
/// </remarks>
public sealed class WhatElseIsComposedAtRuntimeTests
{
    /// <summary>Anything at least this long is worth a person's eye.</summary>
    /// <remarks>
    /// A hundred characters. The four paragraphs this phase has found ran 304, 448,
    /// 473 and 884, and the shortest thing anybody has called a paragraph is the
    /// 124-character empty state unit 280 deliberately kept. Below a hundred a
    /// string is a label or a reading.
    /// </remarks>
    private const int WorthALook = 100;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the inventory is printed.</param>
    public WhatElseIsComposedAtRuntimeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Every sentence-length string any capped surface draws.**</summary>
    [AvaloniaFact]
    public void EverySentenceLengthStringOnEveryCappedSurface()
    {
        var examined = 0;
        var found = new List<(string Surface, string Text)>();

        foreach (var (surface, _, _) in HowMuchTheApplicationSaysTests.Ceilings)
        {
            var shown = HowMuchTheApplicationSaysTests
                .Shown(HowMuchTheApplicationSaysTests.SurfaceForSweep(surface))
                .ToList();

            examined += shown.Count;

            foreach (var text in shown.Where(t => t.Length >= WorthALook))
            {
                found.Add((surface, text));
            }
        }

        _output.WriteLine(
            examined + " strings examined across "
            + HowMuchTheApplicationSaysTests.Ceilings.Count + " surfaces");
        _output.WriteLine(
            found.Count + " of them are " + WorthALook + " characters or more");
        _output.WriteLine("");

        foreach (var (surface, text) in found
            .OrderByDescending(f => f.Text.Length))
        {
            _output.WriteLine(
                text.Length.ToString().PadLeft(5) + "  " + surface);
            _output.WriteLine("       " + text);
            _output.WriteLine("");
        }

        // **ONE THING THE SWEEP NOTICED, PRINTED AND NOT FIXED.** The working
        // Digital tab draws *nothing on this frequency yet* while two rows are on
        // the lists. It is **not** the mine list's empty state - that is correctly
        // hidden, and the counts below say so - it is `DigitalModeStripStatus`, a
        // third element carrying the same sentence.
        //
        // **WHETHER THAT IS A DEFECT IS NOT SETTLED HERE.** The rows go in through
        // a test seam, which may not feed whatever the mode strip reads, so this
        // prints the evidence rather than making a claim. Task 4 reports; it does
        // not fix.
        var panel = new Hamlet.App.ViewModels.MainWindowViewModel(
            HowMuchTheApplicationSaysTests.Settled(), null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);
        panel.AddSentRowForTests("W3YNI KC3QIS R-09", slot.AddSeconds(15));

        _output.WriteLine("");
        _output.WriteLine(
            "mine rows " + panel.DigitalMineDecodes.Count
            + ", DigitalMineCount " + panel.DigitalMineCount
            + ", HasDigitalMineDecodes " + panel.HasDigitalMineDecodes);

        var working = HowMuchTheApplicationSaysTests
            .SurfaceForSweep("MainWindow — Digital tab, working");

        foreach (var v in working.GetVisualDescendants())
        {
            var text = v switch
            {
                Avalonia.Controls.TextBlock b when b.IsEffectivelyVisible => b.Text,
                Hamlet.App.Controls.GlossaryTextControl g when g.IsEffectivelyVisible => g.Text,
                _ => null,
            };

            if (text is not null && text.StartsWith("nothing on this frequency", StringComparison.Ordinal))
            {
                _output.WriteLine(
                    "drawn by: " + (v as Avalonia.Controls.Control)?.Name
                    + "  (" + v.GetType().Name + ")");
            }
        }

        // **A SWEEP THAT SWEPT NOTHING HAS NOT PASSED** (unit 279's rule). This
        // asserts nothing about the count it found, because what is a paragraph and
        // what is a necessary explanation is a judgement and not a session's.
        Assert.True(
            examined > 0,
            "no strings were examined on any surface, so this sweep proves nothing");
    }
}
