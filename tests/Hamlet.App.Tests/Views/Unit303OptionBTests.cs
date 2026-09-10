using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 303 task 5: **the achievement mark, option B.**
/// </summary>
/// <remarks>
/// <para>**TIM WAS SHOWN THREE AND CHOSE B, 2026-09-10**: no pill, about 27 px,
/// filled green at rest rather than outlined, with the orbit ring when something is
/// new. **At rest it had been a grey sliver** and his word for it was *not
/// noticeable*.</para>
/// <para>**THE §0.6 QUESTION THIS RAISES IS WORTH BEING EXPLICIT ABOUT.** Unit 300
/// gave the mark two carriers - a ring that is present or absent, and a vane that is
/// filled or outlined - so the two states survived greyscale twice over. **Option B
/// spends one of them**: the vane is filled in both states now. **The ring is still a
/// shape**, so a greyscale printer still separates them, and that is what this class
/// holds.</para>
/// </remarks>
public sealed class Unit303OptionBTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drawn mark is printed.</param>
    public Unit303OptionBTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Filled green at rest, and filled green when something is new.**</summary>
    [AvaloniaFact]
    public void TheQuillIsFilledInBothStates()
    {
        foreach (var lit in new[] { false, true })
        {
            var drawings = Drawn(27, lit);

            var filled = drawings.Where(d => d.Brush is not null).ToList();

            _output.WriteLine(
                (lit ? "something new" : "at rest      ") + " : "
                + drawings.Count + " drawings, " + filled.Count + " filled");

            foreach (var d in drawings)
            {
                _output.WriteLine(
                    "      " + (d.Geometry?.Bounds.Width ?? 0).ToString("0.0")
                    + " x " + (d.Geometry?.Bounds.Height ?? 0).ToString("0.0")
                    + "   fill " + Describe(d.Brush)
                    + "   pen " + (d.Pen is null ? "(none)" : "yes"));
            }

            Assert.True(
                filled.Count > 0,
                (lit ? "the lit" : "the resting")
                + " mark filled nothing, so it is still a sliver");
        }
    }

    /// <summary>**The two states still differ without colour.**</summary>
    /// <remarks>
    /// **THE RING IS NOW THE ONLY CARRIER AND IT IS STILL A SHAPE** (§0.6). Option B
    /// spends the filled-against-outlined carrier, so this is the one that has to
    /// hold: the lit mark draws strictly more shapes than the resting one, and a
    /// greyscale printer keeps every one of them.
    /// </remarks>
    [AvaloniaFact]
    public void TheTwoStatesStillDifferWithoutColour()
    {
        var rest = Drawn(27, lit: false);
        var lit = Drawn(27, lit: true);

        _output.WriteLine("at rest       : " + rest.Count + " shapes");
        _output.WriteLine("something new : " + lit.Count + " shapes");

        Assert.True(
            lit.Count > rest.Count,
            "the two states draw the same number of shapes, so nothing about the "
            + "difference survives greyscale");
    }

    /// <summary>**About 27 px, and it fills the bar.**</summary>
    [AvaloniaFact]
    public void TheMarkIsAboutTwentySevenAndFillsTheBar()
    {
        var window = new MainWindow { DataContext = Model() };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        try
        {
            var bar = window.FindControl<Border>("StatusBar");
            var mark = window.FindControl<AchievementMarkControl>("AchievementMark");
            var belt = window.FindControl<Border>("ContactBeltRing");

            Assert.True(bar is not null && mark is not null && belt is not null);

            var inside = bar!.Bounds.Height
                - bar.Padding.Top - bar.Padding.Bottom
                - bar.BorderThickness.Top - bar.BorderThickness.Bottom;

            _output.WriteLine("status bar, outside : " + bar.Bounds.Height);
            _output.WriteLine("status bar, inside  : " + inside);
            _output.WriteLine("the mark            : " + mark!.Bounds.Height);
            _output.WriteLine("the belt asks for   : " + belt!.Height);

            Assert.Equal(27, mark.Bounds.Height, 1);

            // **IT STILL FITS**, with a little headroom, and the bar did not grow.
            Assert.True(
                mark.Bounds.Height <= inside,
                "the mark is taller than the inside of the bar");

            // **THE BELT PILL BESIDE IT IS UNCHANGED**, which the instruction says
            // outright - so the two still read as a pair off one resource.
            Assert.Equal(mark.Height, belt.Height);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>What a brush is, in a word.</summary>
    private static string Describe(IBrush? brush)
        => brush switch
        {
            null => "(none)",
            ISolidColorBrush solid => solid.Color.ToString(),
            _ => brush.GetType().Name,
        };

    /// <summary>What the control's own `Render` emitted at one size.</summary>
    private static IReadOnlyList<GeometryDrawing> Drawn(double side, bool lit)
    {
        var mark = new AchievementMarkControl
        {
            Width = side,
            Height = side,
            IsNew = lit,
        };

        mark.Measure(new Size(side, side));
        mark.Arrange(new Rect(0, 0, side, side));

        var group = new DrawingGroup();

        using (var context = group.Open())
        {
            mark.Render(context);
        }

        var flat = new List<GeometryDrawing>();

        Flatten(group, flat);

        return flat;
    }

    private static void Flatten(DrawingGroup group, List<GeometryDrawing> into)
    {
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case GeometryDrawing drawing:
                    into.Add(drawing);
                    break;

                case DrawingGroup nested:
                    Flatten(nested, into);
                    break;

                default:
                    break;
            }
        }
    }

    private static MainWindowViewModel Model()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        return new MainWindowViewModel(settings, null);
    }
}
