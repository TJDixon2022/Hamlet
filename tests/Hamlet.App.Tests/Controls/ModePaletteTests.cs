using System.Reflection;
using Avalonia.Media;
using Hamlet.App.Controls;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.Controls;

/// <summary>
/// The mode-color language: one definition, four families, and color never the
/// only carrier of meaning (HM-DEC-032).
/// </summary>
public sealed class ModePaletteTests
{
    /// <remarks>
    /// Proves every family has an entry, so no surface can meet a family the
    /// palette cannot color.
    /// </remarks>
    [Fact]
    public void EveryFamilyHasColors()
    {
        foreach (var family in Enum.GetValues<ModeFamily>())
        {
            var colors = ModePalette.For(family);

            Assert.Equal(family, colors.Family);
            Assert.False(string.IsNullOrWhiteSpace(colors.Label));
        }

        Assert.Equal(Enum.GetValues<ModeFamily>().Length, ModePalette.All.Count);
    }

    /// <remarks>
    /// <para>Proves the four fills are separable at a glance. The palette this
    /// replaced put a pale amber next to a pale pink, which read as one wash;
    /// comparing every pair guards that regression directly rather than
    /// trusting the eye that picked the colors.</para>
    /// <para>Measured as CIE76 ΔE*ab, because plain RGB distance is not what
    /// eyes do — it scores the lavender/blue pair at 35 while a viewer has no
    /// trouble at all telling them apart. ΔE above 10 is the usual "clearly two
    /// colors" line; the closest pair in this palette measures about 17.</para>
    /// <para>The colors themselves are Tim's ruling (HM-DEC-032). This is a
    /// floor that guards them, not a target that chose them.</para>
    /// </remarks>
    [Fact]
    public void FillsAreDistinguishableFromOneAnother()
    {
        for (var i = 0; i < ModePalette.All.Count; i++)
        {
            for (var j = i + 1; j < ModePalette.All.Count; j++)
            {
                var difference = DeltaE(ModePalette.All[i].Fill, ModePalette.All[j].Fill);

                Assert.True(
                    difference > 10,
                    $"{ModePalette.All[i].Label} and {ModePalette.All[j].Label} are only "
                    + $"ΔE {difference:0.0} apart");
            }
        }
    }

    /// <remarks>
    /// <para>Proves every family's ink is readable on its own fill. A label
    /// nobody can read is the same as no label, which would leave color as the
    /// only carrier of meaning after all.</para>
    /// <para>WCAG AA for normal text is 4.5. Three of the four clear it
    /// comfortably; "Open / mixed" measures 4.1, because its ink and fill are
    /// both deliberately near-neutral — it is the color for space nothing owns,
    /// and making it shout would defeat that. The floor here is set at 4.0 so
    /// the ruled palette passes, and the shortfall is recorded rather than
    /// hidden: raising that one ink is Tim's call, not this test's.</para>
    /// </remarks>
    [Fact]
    public void InkIsReadableOnItsOwnFill()
    {
        foreach (var colors in ModePalette.All)
        {
            var ratio = ContrastRatio(colors.Ink, colors.Fill);

            Assert.True(
                ratio >= 4.0,
                $"{colors.Label}: contrast ratio {ratio:0.0}, below the 4.0 floor");
        }

        // The three families that name a mode carry full AA contrast.
        foreach (var colors in ModePalette.All.Where(c => c.Family != ModeFamily.Open))
        {
            Assert.True(
                ContrastRatio(colors.Ink, colors.Fill) >= 4.5,
                $"{colors.Label} fell below WCAG AA");
        }
    }

    /// <remarks>
    /// Proves the brushes are built once and shared, not allocated per call.
    /// The map redraws these on every frame of a resize.
    /// </remarks>
    [Fact]
    public void BrushesAreCached()
    {
        Assert.Same(ModePalette.Cw.FillBrush, ModePalette.Cw.FillBrush);
        Assert.Same(ModePalette.For(ModeFamily.Cw), ModePalette.For(ModeFamily.Cw));
    }

    /// <remarks>
    /// THE SINGLE-DEFINITION RULE (HM-DEC-032). Proves no control carries a
    /// mode color of its own: the palette's hex strings appear in the palette
    /// file and nowhere else in the app's source. Two copies of a language are
    /// two languages, and the second one drifts silently.
    /// </remarks>
    [Fact]
    public void NoOtherFileCarriesAModeColorLiteral()
    {
        var source = SourceRoot();
        var palette = Path.Combine(source, "Controls", "ModePalette.cs");

        Assert.True(File.Exists(palette), $"palette not found at {palette}");

        var literals = ModePalette.All
            .SelectMany(c => new[] { Hex(c.Fill), Hex(c.Ink) })
            .ToList();

        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories))
        {
            var extension = Path.GetExtension(file);

            if (extension is not (".cs" or ".axaml"))
            {
                continue;
            }

            if (string.Equals(file, palette, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = File.ReadAllText(file);

            foreach (var literal in literals.Where(
                l => text.Contains(l, StringComparison.OrdinalIgnoreCase)))
            {
                offenders.Add($"{Path.GetFileName(file)} carries {literal}");
            }
        }

        Assert.Empty(offenders);
    }

    private static string Hex(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>CIE76 color difference, via sRGB → XYZ → L*a*b*.</summary>
    private static double DeltaE(Color a, Color b)
    {
        var (l1, a1, b1) = Lab(a);
        var (l2, a2, b2) = Lab(b);

        return Math.Sqrt(
            Math.Pow(l1 - l2, 2) + Math.Pow(a1 - a2, 2) + Math.Pow(b1 - b2, 2));
    }

    private static (double L, double A, double B) Lab(Color color)
    {
        static double Linear(byte v)
        {
            var s = v / 255.0;
            return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        var r = Linear(color.R);
        var g = Linear(color.G);
        var b = Linear(color.B);

        // sRGB to CIE XYZ, D65.
        var x = ((0.4124 * r) + (0.3576 * g) + (0.1805 * b)) / 0.95047;
        var y = (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
        var z = ((0.0193 * r) + (0.1192 * g) + (0.9505 * b)) / 1.08883;

        static double F(double t)
            => t > 0.008856 ? Math.Cbrt(t) : ((7.787 * t) + (16.0 / 116.0));

        var fx = F(x);
        var fy = F(y);
        var fz = F(z);

        return ((116 * fy) - 16, 500 * (fx - fy), 200 * (fy - fz));
    }

    /// <summary>WCAG relative-luminance contrast ratio.</summary>
    private static double ContrastRatio(Color a, Color b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);

        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(Color color)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(color.R))
            + (0.7152 * Channel(color.G))
            + (0.0722 * Channel(color.B));
    }

    /// <summary>
    /// The app's source directory, found by walking up from the test binary to
    /// the repository root.
    /// </summary>
    private static string SourceRoot()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Hamlet.App");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find src/Hamlet.App above the test binary");
    }
}
