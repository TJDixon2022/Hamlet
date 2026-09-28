using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// There is one layout, and it is the CW layout (work instruction 487, R101, HM-DEC-192).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"Here's the rule. There's only one layout. The layout that we use for
/// CW is the layout we use everywhere. It doesn't change. That's a rule."* His three screenshots
/// showed 14.070.0, the first hertz of the PSK31 block, alone rearranging.</para>
/// <para>**THE REAL WINDOW, HEADLESS, TUNED BY THE RIG FACE'S OWN WRITE**
/// (<see cref="TheTopRowTuned"/>), at two widths and three dials: 14.069.2 on the CW tab, and
/// 14.070.0 and 14.076.0 on the Digital tab. At each width every panel's place and size are the same
/// at all three dials; across the two widths the arrangement is the same - band row, card, map, rig
/// face, left to right - and the map keeps one size, while the card takes the width the window
/// gives it, because a column cannot be one size in two different windows.</para>
/// </remarks>
public sealed class TheLayoutIsOneLayoutTests
{
    private static readonly (long Hz, string Mode)[] Dials =
    {
        (14_069_200, "CW"), (14_070_000, "Digital"), (14_076_000, "Digital"),
    };

    private static readonly (double Width, double Height)[] Widths = { (1920, 1040), (1100, 780) };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rectangles are printed.</param>
    public TheLayoutIsOneLayoutTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves R101: at each width the card, the map, the rig face and the band pills stand at the same
    /// place and size at 14.069.2 CW, 14.070.0 and 14.076.0; and at both widths the map is right of
    /// the card, the rig face right of the map, and the map one size.
    /// </remarks>
    [AvaloniaFact]
    public void TheArrangementIsTheSameAtEveryDialModeAndWidth()
    {
        var differs = new List<string>();
        var maps = new List<PanelBounds>();

        foreach (var (width, height) in Widths)
        {
            var seen = new List<(string Case, IReadOnlyDictionary<string, PanelBounds> Panels)>();

            foreach (var (hz, mode) in Dials)
            {
                var (window, panel) = TheTopRowTuned.Open(width, height, LicenseClass.General, mode);

                try
                {
                    TheTopRowTuned.Tune(window, hz);

                    var panels = Panels(window);
                    var name = $"{width:0} wide, {hz / 1e6:0.0000} MHz {mode}";

                    _output.WriteLine(name + ": " + string.Join("; ", panels.Select(p => $"{p.Key} {p.Value}")));
                    seen.Add((name, panels));
                }
                finally
                {
                    window.Close();
                }
            }

            var first = seen[0];

            foreach (var (name, panels) in seen.Skip(1))
            {
                foreach (var (key, bounds) in panels)
                {
                    if (!Same(bounds, first.Panels[key]))
                    {
                        differs.Add($"{key} at {name} is {bounds}, at {first.Case} {first.Panels[key]}");
                    }
                }
            }

            foreach (var (name, panels) in seen)
            {
                if (!(panels["card"].Left < panels["map"].Left && panels["map"].Left < panels["rig"].Left))
                {
                    differs.Add($"{name}: not card, map, rig from the left");
                }
            }

            maps.Add(first.Panels["map"]);
        }

        if (Math.Abs(maps[0].Width - maps[1].Width) > 0.5 || Math.Abs(maps[0].Height - maps[1].Height) > 0.5)
        {
            differs.Add($"the map is {maps[0]} wide and {maps[1]} narrow");
        }

        foreach (var d in differs)
        {
            _output.WriteLine("DIFFERS " + d);
        }

        Assert.Empty(differs);
    }

    private static IReadOnlyDictionary<string, PanelBounds> Panels(Window window)
    {
        var row = window.GetVisualDescendants().OfType<BandGovernsTheMapPanel>().First(p => p.Name == "BandRow");
        var pills = window.GetVisualDescendants().OfType<Control>().First(c => c.Name == "BandPills");

        return new Dictionary<string, PanelBounds>
        {
            ["pills"] = TheTopRowTuned.Where(pills, window),
            ["card"] = TheTopRowTuned.Where((Control)row.Children[0], window),
            ["map"] = TheTopRowTuned.Where((Control)row.Children[1], window),
            ["rig"] = TheTopRowTuned.Where((Control)row.Children[2], window),
        };
    }

    private static bool Same(PanelBounds a, PanelBounds b)
        => Math.Abs(a.Left - b.Left) <= 0.5 && Math.Abs(a.Top - b.Top) <= 0.5
           && Math.Abs(a.Width - b.Width) <= 0.5 && Math.Abs(a.Height - b.Height) <= 0.5;
}
