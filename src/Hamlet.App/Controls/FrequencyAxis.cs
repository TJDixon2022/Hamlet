namespace Hamlet.App.Controls;

/// <summary>
/// The stretch of the band a control's width covers, and the mapping between a
/// frequency and an x position on it.
/// </summary>
/// <param name="LowHz">Frequency at x = 0.</param>
/// <param name="HighHz">Frequency at x = <paramref name="Width"/>.</param>
/// <param name="Width">The control's width in pixels.</param>
/// <remarks>
/// <para>ONE AXIS, THREE SURFACES. The neighborhood map, the dial tape and the
/// waterfall all answer the same question — where on my width does this
/// frequency sit — and they used to answer it with three copies of the same
/// arithmetic. Three copies of a mapping is three mappings, and the moment one
/// of them rounds differently the operator is looking at a spot marker that
/// says one thing on the map and another an inch below it.</para>
/// <para>The map and the waterfall lay the whole band across their width, which
/// is <see cref="Across"/>. The tape lays a few kilohertz across its width and
/// slides that window under a fixed hairline, which is <see cref="Zoomed"/>.
/// Different windows onto the same axis, and that is the whole difference
/// between them.</para>
/// <para>Kept in doubles rather than hertz. The tape's window edges land on
/// fractional hertz for almost every width, and rounding them to whole hertz
/// before dividing would put a slow wobble into the scale as the operator
/// tunes. Frequencies handed back out are whole hertz, because that is what a
/// radio takes.</para>
/// </remarks>
public readonly record struct FrequencyAxis(double LowHz, double HighHz, double Width)
{
    /// <summary>
    /// The whole span laid across the width, edge to edge.
    /// </summary>
    /// <param name="lowHz">Band lower edge.</param>
    /// <param name="highHz">Band upper edge.</param>
    /// <param name="width">The control's width in pixels.</param>
    /// <returns>The axis.</returns>
    public static FrequencyAxis Across(long lowHz, long highHz, double width)
        => new(lowHz, highHz, width);

    /// <summary>
    /// A window of fixed scale, centered on a frequency.
    /// </summary>
    /// <param name="centerHz">The frequency at the middle of the width.</param>
    /// <param name="pixelsPerHz">The zoom, in pixels per hertz.</param>
    /// <param name="width">The control's width in pixels.</param>
    /// <returns>The axis.</returns>
    /// <remarks>
    /// This is the slide-rule case: the scale never changes, the window slides.
    /// The waterfall wants this one the day it stops drawing a whole band and
    /// starts drawing a span the radio actually resolves.
    /// </remarks>
    public static FrequencyAxis Zoomed(long centerHz, double pixelsPerHz, double width)
    {
        var span = width / pixelsPerHz;
        return new(centerHz - (span / 2), centerHz + (span / 2), width);
    }

    /// <summary>How many hertz the width covers.</summary>
    public double SpanHz => HighHz - LowHz;

    /// <summary>
    /// Whether this axis can map anything. A control with no width yet, or a
    /// band whose edges have not arrived, has an axis that answers nothing.
    /// </summary>
    public bool IsUsable => Width > 0 && HighHz > LowHz;

    /// <summary>Where a frequency falls, in pixels from the left edge.</summary>
    /// <param name="hz">The frequency.</param>
    /// <returns>An x position, which may be off either end.</returns>
    public double XOf(double hz) => (hz - LowHz) / SpanHz * Width;

    /// <summary>What frequency an x position stands for.</summary>
    /// <param name="x">Pixels from the left edge.</param>
    /// <returns>The frequency in whole hertz.</returns>
    public long HzAt(double x) => (long)Math.Round(LowHz + (x / Width * SpanHz));

    /// <summary>
    /// What frequency an x position stands for, held inside the window.
    /// </summary>
    /// <param name="x">Pixels from the left edge.</param>
    /// <returns>The frequency in whole hertz, never outside the axis.</returns>
    public long HzAtClamped(double x)
        => HzAt(Math.Clamp(x, 0, Width));

    /// <summary>Whether a frequency falls inside the window at all.</summary>
    /// <param name="hz">The frequency.</param>
    /// <returns>True when it is on screen.</returns>
    public bool Covers(double hz) => hz >= LowHz && hz <= HighHz;
}
