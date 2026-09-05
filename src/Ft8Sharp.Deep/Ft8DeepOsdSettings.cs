using System;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How hard the ordered statistics stage tries, or nothing at all.</b> A null settings reference
/// on <see cref="Ft8DeepSlotDecoder"/> means the stage is off and the sibling does exactly what the
/// port does.
/// </summary>
/// <remarks>
/// <para>
/// <b>OFF IS THE DEFAULT AND OFF IS AN EXACT REPRODUCTION.</b> With no settings the sibling's
/// per-candidate loop runs the port's stages, in the port's order, with the port's counts and the
/// port's de-duplication key - and <c>Ft8DeepIdentityTests</c> holds it to returning the whole
/// <c>Ft8SlotResult</c> the port returns over two ladder blocks and the committed capture. That test
/// is what makes a difference between the OSD-off and OSD-on columns of the scoreboard attributable
/// to this setting and to nothing else.
/// </para>
/// <para>
/// <b><see cref="Order"/> is the number of basis positions the search is allowed to flip</b>, which
/// is Fossorier and Lin's λ (M. P. C. Fossorier and S. Lin, "Soft-decision decoding of linear block
/// codes based on ordered statistics", IEEE Transactions on Information Theory 41(5), September 1995,
/// pages 1379-1396). Order 0 re-encodes the most reliable basis as it stands; order λ additionally
/// tries every subset of the basis of size 1 to λ. The cost is
/// <c>sum over i of C(91, i)</c> re-encodings per candidate, which is 1, 92, 4187 and 125672 for
/// orders 0, 1, 2 and 3.
/// </para>
/// <para>
/// <b>There is no threshold, no confidence and no acceptance rule here</b>, because
/// <c>Ft8Sharp.Deep</c> never decides that a message is real. Whatever this stage produces is handed
/// to the port's <c>Ft8CodewordDecoder</c> and accepted or refused by the port's own parity and
/// CRC-14 gates. See <c>src/Ft8Sharp.Deep/porting-notes.md</c>.
/// </para>
/// </remarks>
public sealed class Ft8DeepOsdSettings
{
    /// <summary>The largest order this library will run. Beyond it the search stops being tractable.</summary>
    /// <remarks>
    /// A bound rather than a tuning: order 4 is about two and a half million re-encodings per
    /// candidate, and a slot carrying a hundred candidates would not finish inside FT8's fifteen
    /// seconds. The bound is refused loudly rather than clamped silently, because a caller who asked
    /// for order 5 and got order 3 would be reading a measurement of something it did not ask for.
    /// </remarks>
    public const int MaximumOrder = 4;

    /// <summary>Builds settings for one order.</summary>
    /// <param name="order">
    /// How many basis positions may be flipped, 0 to <see cref="MaximumOrder"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The order is negative or above <see cref="MaximumOrder"/>.
    /// </exception>
    public Ft8DeepOsdSettings(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                "An order is how many of the most reliable positions may be flipped and cannot be "
                + "negative. Zero is allowed and means re-encode the basis as it stands.");
        }

        if (order > MaximumOrder)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                $"Order {order} is beyond this library's bound of {MaximumOrder}. Order 4 is already "
                + "about two and a half million re-encodings per candidate; clamping instead of "
                + "refusing would report a measurement of an order nobody asked for.");
        }

        Order = order;
    }

    /// <summary>How many of the most reliable basis positions may be flipped.</summary>
    public int Order { get; }
}
