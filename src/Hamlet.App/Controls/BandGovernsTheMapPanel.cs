using System;
using Avalonia;
using Avalonia.Controls;

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

    private double _mapHeight = MapFloor;

    /// <summary>The height the map was last given, for a test and the report to read.</summary>
    public double MapHeight => _mapHeight;

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

            return card.DesiredSize.Height <= rowHeight + 0.5;
        }
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

        card.Arrange(new Rect(0, 0, cardWidth, finalSize.Height));
        map.Arrange(new Rect(cardWidth, 0, mapWidth, Math.Min(finalSize.Height, map.DesiredSize.Height)));
        rig.Arrange(new Rect(cardWidth + mapWidth, 0, rigWidth, finalSize.Height));

        return finalSize;
    }

    private static double CardWidth(double width, Control map, Control rig)
        => Math.Max(0, width - map.DesiredSize.Width - rig.DesiredSize.Width);
}
