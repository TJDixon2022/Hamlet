using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Solar;

namespace Hamlet.App.Controls;

/// <summary>
/// **The world with its night side drawn and one marker at the operator's grid** (work
/// instruction 332 task 2).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"We ought to use something like this with a dark/light
/// indicator… Looking for contacts in eastern Europe at 2:00 pm EST is not likely."* Then
/// *"without the orphan dots, maybe just a dot at my grid"*, and *"a little darker in the dead
/// zone"*. `assets/grayline-1400edt.png` is the look at 2 pm EDT; it is for reading and nothing
/// here loads it.</para>
/// <para>**THE PICTURE IS THE CARD'S OWN MAP** - `FlatWorldMap.Relief`, loaded once and only
/// where its hash matches, by <see cref="Ft8GlobeControl"/> - and every place on it is placed
/// by that record's own projection. There is not a second copy of either here.</para>
/// <para>**ONE MARKER AND NOTHING ELSE**: no station dots and no paths, by construction - this
/// control has no property that could carry one.</para>
/// <para>**THE NIGHT IS THE SUN AND THE CLOCK** (§0.0), from <see cref="SolarTerminator"/>.
/// Nothing drawn here says a band is open.</para>
/// </remarks>
public sealed class GrayLineMapControl : Control
{
    /// <summary>The instant the night side is drawn for.</summary>
    public static readonly StyledProperty<DateTime> UtcProperty =
        AvaloniaProperty.Register<GrayLineMapControl, DateTime>(nameof(Utc));

    /// <summary>The operator's grid, or null.</summary>
    public static readonly StyledProperty<string?> OperatorGridProperty =
        AvaloniaProperty.Register<GrayLineMapControl, string?>(nameof(OperatorGrid));

    /// <summary>Told once, when the panel first has a size.</summary>
    public static readonly StyledProperty<ICommand?> RenderedCommandProperty =
        AvaloniaProperty.Register<GrayLineMapControl, ICommand?>(nameof(RenderedCommand));

    /// <summary>
    /// **How dark full night is drawn: sixty per cent** (Tim, on the mockup: *"a little
    /// darker in the dead zone"*).
    /// </summary>
    public const double Darkest = 0.6;

    /// <summary>The side of one shaded cell, in the control's own units.</summary>
    /// <remarks>
    /// **THREE UNITS**, which at the panel's height is about a degree and a half of longitude
    /// - finer than the six-degree fade, so the gray edge reads as a gradient and not steps.
    /// </remarks>
    public const double CellSize = 3.0;

    private const int ShadeLevels = 32;

    private static readonly IBrush Paper = new SolidColorBrush(Color.Parse("#DDE6EC"));

    /// <summary>The night's shades, from clear to <see cref="Darkest"/>, made once.</summary>
    private static readonly IBrush[] Shades = Enumerable.Range(0, ShadeLevels + 1)
        .Select(i => (IBrush)new SolidColorBrush(Color.FromArgb(
            (byte)Math.Round(255 * Darkest * i / ShadeLevels), 0x00, 0x08, 0x1A)))
        .ToArray();

    private bool _reported;

    static GrayLineMapControl()
    {
        AffectsRender<GrayLineMapControl>(UtcProperty, OperatorGridProperty);
    }

    /// <summary>The instant the night side is drawn for.</summary>
    public DateTime Utc
    {
        get => GetValue(UtcProperty);
        set => SetValue(UtcProperty, value);
    }

    /// <summary>The operator's grid, or null.</summary>
    public string? OperatorGrid
    {
        get => GetValue(OperatorGridProperty);
        set => SetValue(OperatorGridProperty, value);
    }

    /// <summary>Told once, when the panel first has a size.</summary>
    public ICommand? RenderedCommand
    {
        get => GetValue(RenderedCommandProperty);
        set => SetValue(RenderedCommandProperty, value);
    }

    /// <summary>What a render would put on the surface, without a surface.</summary>
    /// <param name="SubsolarLongitude">Where the sun is overhead, degrees east.</param>
    /// <param name="SubsolarLatitude">The sun's declination.</param>
    /// <param name="Markers">How many markers are drawn: one, or none without a grid.</param>
    /// <param name="MarkerAt">Where the one marker lands on the control.</param>
    /// <param name="NightCells">Cells drawn fully dark.</param>
    /// <param name="TwilightCells">Cells drawn part dark: the gray edge.</param>
    /// <remarks>
    /// **NOTHING IN THIS REPOSITORY CAN LOOK AT A PICTURE**, so what the render decides is
    /// separated from the drawing of it, the way <see cref="Ft8GlobeControl.WhatWouldBeDrawn"/>
    /// does for the card's map. It still says nothing about whether the result is legible.
    /// </remarks>
    public sealed record Drawn(
        double SubsolarLongitude,
        double SubsolarLatitude,
        int Markers,
        Point MarkerAt,
        int NightCells,
        int TwilightCells);

    /// <summary>What a render at this size and instant would draw.</summary>
    /// <param name="utc">The instant.</param>
    /// <param name="grid">The operator's grid, or null.</param>
    /// <param name="width">The control's width.</param>
    /// <param name="height">The control's height.</param>
    /// <returns>The decisions.</returns>
    public static Drawn WhatWouldBeDrawn(DateTime utc, string? grid, double width, double height)
    {
        var sun = SolarTerminator.At(utc);
        var scale = ScaleFor(width, height);

        if (scale <= 0)
        {
            return new Drawn(sun.Longitude, sun.Latitude, 0, default, 0, 0);
        }

        var night = 0;
        var twilight = 0;

        foreach (var (_, darkness) in Cells(sun, scale))
        {
            if (darkness >= 1)
            {
                night++;
            }
            else
            {
                twilight++;
            }
        }

        var marker = MarkerFor(grid, scale);

        return new Drawn(
            sun.Longitude, sun.Latitude, marker is null ? 0 : 1, marker ?? default,
            night, twilight);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// **THE PICTURE'S OWN PROPORTIONS, AT WHATEVER HEIGHT THE PANEL GIVES IT** - stretching
    /// would move every place off the pixel the projection puts it on (HM-DEC-092).
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;
        var height = availableSize.Height;

        if (double.IsInfinity(width) && double.IsInfinity(height))
        {
            return new Size(FlatWorldMap.Relief.WidthPixels, FlatWorldMap.Relief.HeightPixels);
        }

        if (double.IsInfinity(width))
        {
            width = Ft8GlobeControl.WidthFor(height);
        }

        if (double.IsInfinity(height))
        {
            height = Ft8GlobeControl.HeightFor(width);
        }

        var scale = ScaleFor(width, height);

        return new Size(
            FlatWorldMap.Relief.WidthPixels * scale, FlatWorldMap.Relief.HeightPixels * scale);
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_reported || RenderedCommand is not { } report || e.NewSize.Width <= 0)
        {
            return;
        }

        // **THE PANEL'S SIZE AND WHICH REGIONS DREW, ONCE.** The panel is the bordered box
        // around this map; the regions are found by name inside it.
        var panel = this.GetVisualAncestors().OfType<Border>()
            .FirstOrDefault(b => b.BorderThickness.Left > 0);

        if (panel is null)
        {
            return;
        }

        var regions = new[] { ("GreenZoneLeft", "left"), ("GreenZoneMap", "map"), ("GreenZoneRight", "right") }
            .Where(r => panel.GetVisualDescendants().OfType<Control>()
                .Any(c => c.Name == r.Item1 && c.IsVisible))
            .Select(r => r.Item2);

        var layout = new GreenZoneLayout(
            panel.Bounds.Width, panel.Bounds.Height, string.Join(",", regions));

        if (report.CanExecute(layout))
        {
            _reported = true;
            report.Execute(layout);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// **NO MAP MEANS NO NIGHT AND NO MARKER.** Where the bitmap is missing or its bytes are
    /// not the ones the projection describes, this draws the plain ground and stops - a dot
    /// without the picture it was placed against is a claim with nothing behind it.
    /// </remarks>
    public override void Render(DrawingContext context)
    {
        var scale = ScaleFor(Bounds.Width, Bounds.Height);

        if (scale <= 0)
        {
            return;
        }

        var drawn = new Rect(
            0, 0,
            FlatWorldMap.Relief.WidthPixels * scale,
            FlatWorldMap.Relief.HeightPixels * scale);

        context.FillRectangle(Paper, drawn);

        if (Ft8GlobeControl.ReliefBitmap is not { } map)
        {
            return;
        }

        context.DrawImage(
            map,
            new Rect(0, 0, FlatWorldMap.Relief.WidthPixels, FlatWorldMap.Relief.HeightPixels),
            drawn);

        foreach (var (cell, darkness) in Cells(SolarTerminator.At(Utc), scale))
        {
            context.FillRectangle(Shades[(int)Math.Round(darkness * ShadeLevels)], cell);
        }

        if (MarkerFor(OperatorGrid, scale) is { } at)
        {
            Ft8GlobeControl.OperatorMarker(context, at);
        }
    }

    private static double ScaleFor(double width, double height)
        => width <= 0 || height <= 0
            ? 0
            : Math.Min(
                width / FlatWorldMap.Relief.WidthPixels,
                height / FlatWorldMap.Relief.HeightPixels);

    /// <summary>Every cell with any night in it, and how dark.</summary>
    private static IEnumerable<(Rect Cell, double Darkness)> Cells(SubsolarPoint sun, double scale)
    {
        var width = FlatWorldMap.Relief.WidthPixels * scale;
        var height = FlatWorldMap.Relief.HeightPixels * scale;

        for (var y = 0.0; y < height; y += CellSize)
        {
            for (var x = 0.0; x < width; x += CellSize)
            {
                var w = Math.Min(CellSize, width - x);
                var h = Math.Min(CellSize, height - y);

                var (latitude, longitude) = FlatWorldMap.Relief.Coordinate(
                    (x + (w / 2)) / scale, (y + (h / 2)) / scale);

                var darkness = SolarTerminator.Darkness(
                    sun, Math.Clamp(latitude, -90.0, 90.0), longitude);

                if (darkness > 0)
                {
                    yield return (new Rect(x, y, w, h), darkness);
                }
            }
        }
    }

    /// <summary>Where the operator's marker lands, or null with no grid or no room.</summary>
    private static Point? MarkerFor(string? grid, double scale)
    {
        if (OperatorLocation.FromGrid(grid) is not { } here
            || FlatWorldMap.Relief.Place(here.Latitude, here.Longitude) is not { } pixel)
        {
            return null;
        }

        return new Point(pixel.X * scale, pixel.Y * scale);
    }
}
