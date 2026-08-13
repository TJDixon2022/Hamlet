namespace Hamlet.App.Controls;

/// <summary>
/// The waterfall's color ramp: 256 pre-mixed colors from noise floor to
/// full scale.
/// </summary>
/// <remarks>
/// <para>A dark instrument surface on warm paper, which is how HM-DEC-012 was
/// already applied to the rig's LCD. The app is a light theme with color,
/// but a waterfall is a screen you read faint detail off, and faint detail on
/// white is unreadable. Making it dark is consistent with the ruling rather
/// than an exception to it — the panel around it stays white.</para>
/// <para>Built once into a lookup table. At twenty-five frames a second times
/// five hundred bins, mixing a color per pixel would be twelve thousand
/// interpolations a second for a picture that only has 256 possible values
/// (HM-DEC-006).</para>
/// </remarks>
public static class WaterfallPalette
{
    /// <summary>Number of entries; one per possible bin amplitude.</summary>
    public const int Size = 256;

    private static readonly (double Stop, byte R, byte G, byte B)[] Ramp =
    {
        // Noise floor: nearly black, faintly blue, so an empty band still
        // reads as a receiver that is switched on.
        (0.00, 0x0B, 0x0F, 0x16),
        (0.22, 0x13, 0x2C, 0x4A),
        (0.42, 0x1B, 0x62, 0x74),
        (0.60, 0x35, 0x9E, 0x6B),
        (0.78, 0xC9, 0xA0, 0x3A),
        (0.92, 0xE8, 0x6A, 0x1F),
        (1.00, 0xFF, 0xF4, 0xDC),
    };

    private static readonly int[] Table = Build();

    /// <summary>
    /// The color for one amplitude, as premultiplied BGRA packed into an
    /// int.
    /// </summary>
    /// <param name="amplitude">Bin amplitude, 0 to 255.</param>
    /// <returns>Packed BGRA suitable for a Bgra8888 bitmap.</returns>
    public static int Color(byte amplitude) => Table[amplitude];

    /// <summary>The whole table, for the renderer to index directly.</summary>
    /// <returns>256 packed BGRA colors.</returns>
    public static int[] Lookup() => Table;

    private static int[] Build()
    {
        var table = new int[Size];

        for (var i = 0; i < Size; i++)
        {
            var t = i / (double)(Size - 1);
            var (r, g, b) = Sample(t);

            // Bgra8888, opaque.
            table[i] = (0xFF << 24) | (r << 16) | (g << 8) | b;
        }

        return table;
    }

    private static (byte R, byte G, byte B) Sample(double t)
    {
        for (var i = 0; i < Ramp.Length - 1; i++)
        {
            var lo = Ramp[i];
            var hi = Ramp[i + 1];

            if (t > hi.Stop && i + 2 < Ramp.Length)
            {
                continue;
            }

            var span = hi.Stop - lo.Stop;
            var k = span <= 0 ? 0 : Math.Clamp((t - lo.Stop) / span, 0, 1);

            return (
                (byte)(lo.R + ((hi.R - lo.R) * k)),
                (byte)(lo.G + ((hi.G - lo.G) * k)),
                (byte)(lo.B + ((hi.B - lo.B) * k)));
        }

        var last = Ramp[^1];
        return (last.R, last.G, last.B);
    }
}
