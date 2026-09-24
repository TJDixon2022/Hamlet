using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Hamlet.App.Controls;

/// <summary>
/// **The top band's first row: the neighborhood card, the sun map and the rig face, left to
/// right - and the band governs the map** (PHASE_PLAN.md 10.3, rev7; work instruction 388 task 3,
/// stage A).
/// </summary>
/// <remarks>
/// <para>**THREE CHILDREN, IN THIS ORDER**: the card, the map, the rig face. Anything else is a
/// fault in the markup and is not laid out.</para>
/// <para>**WHY A PANEL AND NOT A GRID.** Tim, 2026-09-22: *vertical space is the concern, not
/// horizontal; the band governs the map.* The map's width follows its height (HM-DEC-092), and its
/// height has to follow the band - but in an `Auto` grid row the map's own appetite becomes the
/// row's, which is how unit 387 measured the band at 460 px. So the order of questions is fixed
/// here: **the rig face says how tall the row is** (its height does not depend on its width), the
/// map is made that tall, and the card takes the width that is left. The card's header, padding and
/// caption no longer stand over or under the map, because the map is no longer inside the card.</para>
/// <para>**THE CARD MUST STILL FIT.** Narrow windows leave the card too little width for the green
/// block's words at the row's height, and a taller card would make a taller band. So where the card
/// would outgrow the row, or be made narrower than <see cref="CardFloor"/>, the map gives the width
/// back - down to <see cref="MapFloor"/>, the
/// 246 x 134 map units 337 and 376 kept, and never below it - and where even that is not enough the
/// card governs exactly as it did before this unit. **The map never takes width from the rig face.**</para>
/// <para>**AND WHERE THE WINDOW ALLOWS IT, THE MAP STANDS AT THE BAND'S LEFT EDGE** (PHASE_PLAN.md
/// 10.3; work instruction 389 section 6 ruling 2, author's and overrulable). The band is the pills
/// row and this row together, 214 px at 1920 and at 1400, and the map beside the card alone could
/// only ever be this row's 178 of it. So where a map as tall as the whole band still leaves the
/// pills their one row to its right and the card at least <see cref="CardFloor"/>, the map comes
/// first, reaches up beside the pills by <see cref="PillsReach"/>, and the pills row is moved to
/// start where the map ends; the card stands between the map and the rig face, and the rig face
/// keeps its width and its place at the right. **Where either would not fit, it is exactly the
/// shape above** - decided by measured widths each layout, never by a window size written here.
/// The row's own height does not change by one pixel either way: the map reaches into the pills
/// row's height, which the band already had.</para>
/// </remarks>
public sealed class BandGovernsTheMapPanel : Panel
{
    /// <summary>The map is never shorter than this - the floor unit 387's guard holds.</summary>
    public const double MapFloor = 134;

    /// <summary>
    /// **The narrowest the card is made to give the map room** - the card's own width, its
    /// scroller's gap to the map not counted.
    /// </summary>
    /// <remarks>
    /// **THE UNIT'S OWN NUMBER, MARKED AS SUCH AND OVERRULABLE** (work instruction 388 task 3).
    /// Height alone is not a safe test: measured at 1100 x 780, a card squeezed to 167 px still
    /// fitted the row, because it did so by giving <c>GreenZoneLeft</c> - the band, the frequency
    /// and the verdict - 0 px of width and the caption 0, which is hiding information (§0.5). At
    /// 1400, the width 10.3 names, stage A leaves the card 467 px; 400 keeps every word of the
    /// green block on the card at every size where the map grows, and below it the map stays at
    /// its floor and the card is laid out exactly as it was before this unit.
    /// </remarks>
    public const double CardFloor = 400;

    /// <summary>
    /// **A LINE OF THE CARD THAT IS DRAWN BUT DOES NOT DECIDE WHERE THE PANELS STAND** (work
    /// instruction 423, step 6 criterion 6.3).
    /// </summary>
    /// <remarks>
    /// <para>**SET ON THE PRIVILEGE PANEL'S OUTSIDE-PRIVILEGES LINES**: the reassurance, the upgrade
    /// row and its ladder, which are drawn only where his license does not reach. Measured at unit 423
    /// task 1, a General operator tuned from 14.050 to 14.010 MHz: those lines made the card 22 px
    /// taller at 1400 x 1040, both fit questions below answered no, and the map left the band's left
    /// edge - the card jumped 407 px left and the map 562 px right - and at 1920 x 1040 the layout
    /// never settled. Tim saw it at the radio (R62).</para>
    /// <para>**NOTHING IS HIDDEN.** A line marked here is still measured, arranged and drawn in the
    /// card, which grows the row by its height as before, up to the top row's 300 px cap and scrolling
    /// inside it (§0.5). What it no longer does is move the map: the fit questions ask about the card
    /// as the operator's own tune cannot change it. Every line drawn inside his privileges still
    /// counts, so every arrangement inside them is unchanged.</para>
    /// </remarks>
    public static readonly AttachedProperty<bool> OutsideTheFitProperty =
        AvaloniaProperty.RegisterAttached<BandGovernsTheMapPanel, Control, bool>("OutsideTheFit");

    /// <summary>
    /// **THE ARRANGEMENT IS HELD ACROSS A CHANGE OF THIS VALUE** - the window binds the privilege
    /// panel's tone (work instruction 423, step 6 criterion 6.3).
    /// </summary>
    /// <remarks>
    /// <para>**WHY A HOLD AS WELL AS <see cref="OutsideTheFitProperty"/>.** The verdict itself is
    /// longer outside his privileges - *listen all you like, but don't transmit* against *yours to
    /// use* - and it is a line drawn inside them too, so it cannot leave the fit without moving every
    /// arrangement inside them. Measured at unit 423 task 3 with the three lines left out: at 1400 x
    /// 1040 the verdict took a third line in the 170 px beside the best bet, the card stood 10 px over
    /// the row, and the map still left the band's left edge for 327 x 178 beside the card.</para>
    /// <para>**SO WHERE ONLY THIS VALUE HAS CHANGED, THE PANELS STAY WHERE THEY STOOD**: the same
    /// available width, the same rig face and the same pills row as when the arrangement was decided,
    /// and a different tone, is his tune across a privilege edge, and the map's height, its place and
    /// the card's slot are the ones decided a moment before. The card's words wrap in that slot and
    /// grow the row as they always have, up to the top row's 300 px cap. **Any other change decides
    /// afresh**, exactly as before - a resize, the rig face, the pills, or the card's words under an
    /// unchanged tone, such as a mode pressed - so every arrangement inside his privileges is the one
    /// it was.</para>
    /// </remarks>
    public static readonly StyledProperty<object?> HeldAcrossProperty =
        AvaloniaProperty.Register<BandGovernsTheMapPanel, object?>(nameof(HeldAcross));

    private double _mapHeight = MapFloor;

    private double _pillsReach;

    private bool _atTheLeftEdge;

    private bool _decided;

    private object? _decidedUnder;

    private (double Width, double Row, double Rig, double Need, double Reach) _decidedFor;

    /// <summary>The value the arrangement is held across; see <see cref="HeldAcrossProperty"/>.</summary>
    /// <remarks>
    /// **IT DOES NOT ASK FOR A MEASURE OF ITS OWN** (unit 423 task 3, measured). Registered as
    /// affecting measure, this panel was measured before the card's changed lines were, the card
    /// answered with its height from before the tune, and at 1400 x 1040 the map came back from
    /// 14.010 MHz to 327 x 178 beside the card instead of 393 x 214 at the band's left edge. The
    /// card's own lines ask for the measure when the words change, and by then they are measured.
    /// </remarks>
    public object? HeldAcross
    {
        get => GetValue(HeldAcrossProperty);
        set => SetValue(HeldAcrossProperty, value);
    }

    /// <summary>The height the map was last given, for a test and the report to read.</summary>
    public double MapHeight => _mapHeight;

    /// <summary>
    /// **True where the map was last laid out at the band's left edge**, reaching up beside the
    /// pills; false where it stands between the card and the rig face at this row's height.
    /// </summary>
    public bool MapIsAtTheLeftEdge => _atTheLeftEdge;

    /// <summary>
    /// How far above this row the map reaches when it is at the left edge: the pills row's own
    /// height and the gap under it, measured, and 0 where it is not.
    /// </summary>
    public double PillsReach => _atTheLeftEdge ? _pillsReach : 0;

    /// <summary>
    /// **The band-pills row above this one**, handed in by the window. Null leaves the map where
    /// stage A put it, between the card and the rig face.
    /// </summary>
    /// <remarks>
    /// **ITS LEFT MARGIN IS THE ONE THING OF IT THIS PANEL WRITES**, so the pills start where the
    /// map ends; its top, its bottom and its order are left exactly as the markup has them.
    /// </remarks>
    public Control? Pills { get; set; }

    /// <summary>Reads <see cref="OutsideTheFitProperty"/>.</summary>
    /// <param name="control">The line.</param>
    /// <returns>True where the line does not decide where the panels stand.</returns>
    public static bool GetOutsideTheFit(Control control) => control.GetValue(OutsideTheFitProperty);

    /// <summary>Writes <see cref="OutsideTheFitProperty"/>.</summary>
    /// <param name="control">The line.</param>
    /// <param name="value">True where the line does not decide where the panels stand.</param>
    public static void SetOutsideTheFit(Control control, bool value) => control.SetValue(OutsideTheFitProperty, value);

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count != 3)
        {
            return default;
        }

        var card = Children[0];
        var map = Children[1];
        var rig = Children[2];

        rig.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var rowHeight = rig.DesiredSize.Height;
        var width = availableSize.Width;

        // The privilege panel's outside-privileges lines, which the fit questions leave out.
        var outside = card.GetVisualDescendants().OfType<Control>().Where(GetOutsideTheFit).ToList();

        // **THE CARD AS IT IS NOW, NOT AS IT WAS LAST MEASURED** (unit 423 task 3, measured). The rig
        // face's new digits can have this panel measured before the card's changed lines are, and a
        // card measured again at the width it last had answers from its cache: tuned from 14.010 back
        // to 14.050 MHz at 1400 x 1040 it answered with the outside-privileges height, the map left
        // the band's left edge on a card that fitted, and nothing asked again. So every line waiting
        // to be measured marks the way up to the card as waiting too.
        foreach (var waiting in card.GetVisualDescendants().OfType<Layoutable>().Where(l => !l.IsMeasureValid).ToList())
        {
            for (var up = waiting.GetVisualParent(); up is Layoutable above && !ReferenceEquals(above, this); up = up.GetVisualParent())
            {
                above.InvalidateMeasure();
            }
        }

        var room = Room(width, rig, rowHeight);

        if (!double.IsInfinity(width) && _decided && !Equals(HeldAcross, _decidedUnder) && room == _decidedFor)
        {
            return Held(width, card, map, rig, rowHeight);
        }

        if (!double.IsInfinity(width) && Pills is { } pills && AtTheLeftEdge(pills))
        {
            Decided(room);

            // A card taller than the row here stands over it by its outside-privileges lines alone,
            // and the row grows by them rather than scrolling them away.
            return new Size(width, card.DesiredSize.Height > rowHeight + 0.5 ? card.DesiredSize.Height : rowHeight);
        }

        PlacePills(0);
        _atTheLeftEdge = false;

        if (double.IsInfinity(width))
        {
            // Nothing to trade against: the map at the rig's height, the card at its natural width.
            _mapHeight = Math.Max(MapFloor, rowHeight);
            map.Measure(new Size(double.PositiveInfinity, _mapHeight));
            card.Measure(availableSize);

            return new Size(
                card.DesiredSize.Width + map.DesiredSize.Width + rig.DesiredSize.Width,
                Math.Max(rowHeight, Math.Max(card.DesiredSize.Height, map.DesiredSize.Height)));
        }

        // **AS TALL AS THE ROW, IF THE CARD STILL FITS.** Otherwise the largest height, down to
        // the floor, at which it does - found by halving, because the card's height only ever
        // falls as its width grows.
        var high = Math.Max(MapFloor, rowHeight);

        if (!CardFits(high))
        {
            var low = MapFloor;

            if (CardFits(low))
            {
                for (var i = 0; i < 12 && high - low > 0.5; i++)
                {
                    var mid = (low + high) / 2;

                    if (CardFits(mid))
                    {
                        low = mid;
                    }
                    else
                    {
                        high = mid;
                    }
                }
            }

            high = Math.Floor(low);
        }

        _mapHeight = high;
        Decided(room);

        // The last measure of each child is the one it is arranged at.
        map.Measure(new Size(double.PositiveInfinity, _mapHeight));
        card.Measure(new Size(CardWidth(width, map, rig), double.PositiveInfinity));

        return new Size(
            width,
            Math.Max(rowHeight, Math.Max(card.DesiredSize.Height, map.DesiredSize.Height)));

        bool CardFits(double mapHeight)
        {
            map.Measure(new Size(double.PositiveInfinity, mapHeight));

            var slot = CardWidth(width, map, rig);

            if (mapHeight > MapFloor && slot - card.Margin.Left - card.Margin.Right < CardFloor)
            {
                return false;
            }

            card.Measure(new Size(slot, double.PositiveInfinity));

            return FitHeight(card, outside) <= rowHeight + 0.5;
        }

        // **THE MAP AT THE BAND'S LEFT EDGE, IF BOTH NEIGHBOURS STILL FIT** (work instruction 389
        // ruling 2 items 1 to 3). The pills are asked for their one row with nothing beside them;
        // the map is made as tall as this row and the pills' reach together, which is the band;
        // then the pills must fit to its right and the card between it and the rig must keep its
        // floor and this row's height. Either failing is the stage A shape below, unchanged.
        bool AtTheLeftEdge(Control pills)
        {
            pills.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var need = pills.DesiredSize.Width - pills.Margin.Left - pills.Margin.Right;
            var reach = pills.DesiredSize.Height - pills.Margin.Top;
            var tall = rowHeight + reach;

            map.Measure(new Size(double.PositiveInfinity, tall));

            var mapWidth = map.DesiredSize.Width;
            var slot = width - mapWidth - rig.DesiredSize.Width;

            if (reach <= 0
                || width - mapWidth < need
                || slot - card.Margin.Left - card.Margin.Right < CardFloor)
            {
                return false;
            }

            card.Measure(new Size(slot, double.PositiveInfinity));

            if (FitHeight(card, outside) > rowHeight + 0.5)
            {
                return false;
            }

            _pillsReach = reach;
            _mapHeight = tall;
            _atTheLeftEdge = true;
            PlacePills(mapWidth);

            return true;
        }
    }

    /// <summary>
    /// What the arrangement is decided for: the width, the rig face and the pills row, each to a
    /// tenth of a pixel. The card is not in it, because the card is what is being placed.
    /// </summary>
    private (double Width, double Row, double Rig, double Need, double Reach) Room(double width, Control rig, double rowHeight)
    {
        var need = 0.0;
        var reach = 0.0;

        if (Pills is { } pills)
        {
            pills.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            need = pills.DesiredSize.Width - pills.Margin.Left - pills.Margin.Right;
            reach = pills.DesiredSize.Height - pills.Margin.Top;
        }

        return (Tenth(width), Tenth(rowHeight), Tenth(rig.DesiredSize.Width), Tenth(need), Tenth(reach));

        static double Tenth(double value) => double.IsInfinity(value) ? value : Math.Round(value, 1);
    }

    /// <summary>Records that the arrangement was decided afresh, for what room and under what value.</summary>
    private void Decided((double Width, double Row, double Rig, double Need, double Reach) room)
    {
        _decided = true;
        _decidedUnder = HeldAcross;
        _decidedFor = room;
    }

    /// <summary>
    /// **THE ARRANGEMENT DECIDED A MOMENT BEFORE**, measured again as it stood: the map at the height
    /// it was given, the card in the slot that leaves, the pills where they were.
    /// </summary>
    private Size Held(double width, Control card, Control map, Control rig, double rowHeight)
    {
        map.Measure(new Size(double.PositiveInfinity, _mapHeight));

        if (_atTheLeftEdge)
        {
            card.Measure(new Size(Math.Max(0, width - map.DesiredSize.Width - rig.DesiredSize.Width), double.PositiveInfinity));
            PlacePills(map.DesiredSize.Width);

            return new Size(width, card.DesiredSize.Height > rowHeight + 0.5 ? card.DesiredSize.Height : rowHeight);
        }

        PlacePills(0);
        card.Measure(new Size(CardWidth(width, map, rig), double.PositiveInfinity));

        return new Size(
            width,
            Math.Max(rowHeight, Math.Max(card.DesiredSize.Height, map.DesiredSize.Height)));
    }

    /// <summary>Moves the pills row's left edge, and nothing else about it.</summary>
    private void PlacePills(double left)
    {
        if (Pills is not { } pills || Math.Abs(pills.Margin.Left - left) < 0.01)
        {
            return;
        }

        var margin = pills.Margin;

        pills.Margin = new Thickness(left, margin.Top, margin.Right, margin.Bottom);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count != 3)
        {
            return finalSize;
        }

        var card = Children[0];
        var map = Children[1];
        var rig = Children[2];

        var rigWidth = rig.DesiredSize.Width;
        var mapWidth = map.DesiredSize.Width;
        var cardWidth = Math.Max(0, finalSize.Width - rigWidth - mapWidth);

        if (_atTheLeftEdge)
        {
            // The map first, from the pills row's top to this row's bottom; then the card; then
            // the rig face at the same x and width it has in the shape below.
            map.Arrange(new Rect(0, -_pillsReach, mapWidth, map.DesiredSize.Height));
            card.Arrange(new Rect(mapWidth, 0, cardWidth, finalSize.Height));
            rig.Arrange(new Rect(mapWidth + cardWidth, 0, rigWidth, finalSize.Height));

            return finalSize;
        }

        card.Arrange(new Rect(0, 0, cardWidth, finalSize.Height));
        map.Arrange(new Rect(cardWidth, 0, mapWidth, Math.Min(finalSize.Height, map.DesiredSize.Height)));
        rig.Arrange(new Rect(cardWidth + mapWidth, 0, rigWidth, finalSize.Height));

        return finalSize;
    }

    /// <summary>
    /// The card's height as the fit questions ask it: as measured, less each drawn line marked
    /// <see cref="OutsideTheFitProperty"/> and the spacing its stack gives it.
    /// </summary>
    private static double FitHeight(Control card, IReadOnlyList<Control> outside)
    {
        var height = card.DesiredSize.Height;

        foreach (var line in outside)
        {
            if (!line.IsEffectivelyVisible)
            {
                continue;
            }

            var spacing = line.GetVisualParent() is StackPanel stack ? stack.Spacing : 0;

            height -= line.DesiredSize.Height + spacing;
        }

        return height;
    }

    private static double CardWidth(double width, Control map, Control rig)
        => Math.Max(0, width - map.DesiredSize.Width - rig.DesiredSize.Width);
}
