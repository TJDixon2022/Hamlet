using System.Reflection;
using System.Text.RegularExpressions;
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
    /// <para>The floor is WCAG AA for normal text, 4.5:1, with no exceptions
    /// (HM-DEC-036). "Open / mixed" used to sit at 4.09 and was carved out; Tim
    /// ruled the carve-out away rather than the shortfall, so the ink darkened
    /// and the floor now applies to all four.</para>
    /// </remarks>
    [Fact]
    public void InkIsReadableOnItsOwnFill()
    {
        foreach (var colors in ModePalette.All)
        {
            var ratio = ContrastRatio(colors.Ink, colors.Fill);

            Assert.True(
                ratio >= 4.5,
                $"{colors.Label}: contrast ratio {ratio:0.00}, below WCAG AA's 4.5");
        }
    }

    /// <remarks>
    /// Names the measurement the ruling records, so a later change to the
    /// near-neutral ink cannot quietly slide back under the bar it was
    /// darkened to clear (HM-DEC-036).
    /// </remarks>
    [Fact]
    public void OpenMixedInkClearsTheBarItWasDarkenedFor()
    {
        var ratio = ContrastRatio(ModePalette.Open.Ink, ModePalette.Open.Fill);

        Assert.True(ratio >= 4.5, $"open/mixed fell back to {ratio:0.00}");
        Assert.True(
            ratio >= 5.0,
            $"open/mixed is at {ratio:0.00}, with none of the headroom it was given");
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

    /// <remarks>
    /// Proves the panel families obey the same contrast rule the mode families
    /// do (§0.6): every ink clears WCAG AA against the surface it is drawn on.
    /// This is the check that forced the darker amber, since the header color
    /// used on warm paper reaches only 3.84:1 on its own tinted fill.
    /// </remarks>
    [Fact]
    public void EveryPanelInkClearsAaOnItsOwnSurface()
    {
        foreach (var colors in PanelPalette.All)
        {
            var header = ContrastRatio(colors.HeaderInk, colors.Fill);
            var pill = ContrastRatio(colors.PillInk, colors.PillFill);

            Assert.True(
                header >= 4.5,
                $"{colors.Family} header: {header:0.00} on its own fill");
            Assert.True(
                pill >= 4.5,
                $"{colors.Family} badge: {pill:0.00} on its own pill");
        }
    }

    /// <remarks>
    /// Proves every family is present and distinct, so a section cannot land
    /// on a family the palette has no answer for.
    /// </remarks>
    [Fact]
    public void EveryPanelFamilyHasColors()
    {
        foreach (var family in Enum.GetValues<PanelFamily>())
        {
            Assert.Equal(family, PanelPalette.For(family).Family);
        }

        Assert.Equal(Enum.GetValues<PanelFamily>().Length, PanelPalette.All.Count);
        Assert.Equal(
            PanelPalette.All.Count,
            PanelPalette.All.Select(c => c.Fill).Distinct().Count());
    }

    /// <remarks>
    /// ONE DEFINITION, IN TWO NECESSARY FORMS (HM-DEC-044). The theme
    /// dictionary in App.axaml has to hold these values as XAML resources and
    /// C# has to hold them as typed colors, so there are two representations
    /// and no way around it. What there is a way around is drift, so this
    /// asserts they agree key by key. Anywhere else carrying one of these
    /// literals is a third copy and is a failure.
    /// </remarks>
    [Fact]
    public void ThePanelColorsAgreeWithTheThemeDictionary()
    {
        var source = SourceRoot();
        var theme = File.ReadAllText(Path.Combine(source, "App.axaml"));

        void Same(string key, Color expected)
        {
            var match = Regex.Match(
                theme, $"x:Key=\"{Regex.Escape(key)}\">(#[0-9A-Fa-f]{{6}})<");

            Assert.True(match.Success, $"App.axaml has no {key}");
            Assert.Equal(Hex(expected), match.Groups[1].Value.ToUpperInvariant());
        }

        Same("HmAmber", PanelPalette.Amber.Title);
        Same("HmAmberDeep", PanelPalette.Amber.HeaderInk);
        Same("HmAmberEdge", PanelPalette.Amber.Edge);
        Same("HmAmberTint", PanelPalette.Amber.Fill);

        Same("HmBlue", PanelPalette.Blue.Title);
        Same("HmBlueEdge", PanelPalette.Blue.Edge);
        Same("HmBlueTint", PanelPalette.Blue.Fill);

        Same("HmGreen", PanelPalette.Green.Title);
        Same("HmGreenEdge", PanelPalette.Green.Edge);
        Same("HmGreenTint", PanelPalette.Green.Fill);

        Same("HmSlateEdge", PanelPalette.Slate.Edge);
        Same("HmTextPrimary", PanelPalette.Slate.Title);
    }

    /// <remarks>
    /// Proves no THIRD copy exists. The theme dictionary is the one file
    /// allowed to repeat these values, because XAML cannot read the C# ones;
    /// every other file has to go through the palette or a resource key.
    /// </remarks>
    [Fact]
    public void NoOtherFileCarriesAPanelColorLiteral()
    {
        var source = SourceRoot();

        var allowed = new[]
        {
            Path.Combine(source, "Controls", "ModePalette.cs"),

            // The theme dictionary. XAML cannot read the C# values, so these
            // two representations both have to exist; the test above holds
            // them to agreeing.
            Path.Combine(source, "App.axaml"),

            // The CollapsiblePanel control theme keeps its hover tint as a
            // literal on purpose, so it depends on no application resource and
            // cannot load before the thing it would need. That reason is
            // written at the line itself.
            Path.Combine(source, "Controls", "CollapsiblePanel.axaml"),
        };

        var literals = PanelPalette.All
            .SelectMany(c => new[]
            {
                Hex(c.Title), Hex(c.Edge), Hex(c.HeaderInk), Hex(c.PillFill), Hex(c.PillInk),
            })
            .Where(h => h != "#FFFFFF")
            .Distinct()
            .ToList();

        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(file) is not (".cs" or ".axaml")
                || allowed.Any(a => string.Equals(a, file, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var text = File.ReadAllText(file);

            offenders.AddRange(
                literals
                    .Where(l => text.Contains(l, StringComparison.OrdinalIgnoreCase))
                    .Select(l => $"{Path.GetFileName(file)} carries {l}"));
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
