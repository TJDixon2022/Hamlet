using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;

namespace Hamlet.App.Controls;

/// <summary>
/// The application's marks, drawn from the SVG files themselves.
/// </summary>
/// <remarks>
/// <para>**THE FILE IS THE MARK AND NOTHING TRANSCRIBES IT** (work instruction 285).
/// A logo copied into XAML by hand is a second drawing, and the first time somebody
/// edits the SVG the two disagree with nothing to say so. This reads
/// `Assets/hamlet-logo.svg` at run time, so the file Tim approved is the file that
/// renders.</para>
/// <para>**IT IS NOT AN SVG RENDERER AND DOES NOT PRETEND TO BE.** It handles the
/// seven things these two files use — a viewBox, a translate on a group, rect,
/// circle, line, path and their fill and stroke — and **throws on anything else**
/// rather than drawing an approximation. A mark that silently loses an element is
/// worse than one that fails to load, because nobody would know (§0.0).</para>
/// <para>**AND IT ADDS NO DEPENDENCY.** Rendering SVG in Avalonia otherwise wants a
/// package, and a NuGet reference is a decision with a licence and a supply chain
/// attached — Tim's under §0.4, and this order did not ask for one. Avalonia's own
/// geometry parser already speaks the SVG path mini-language, which is the only hard
/// part.</para>
/// </remarks>
public static class SvgMark
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    /// <summary>Where the full mark lives.</summary>
    public const string FullMarkUri = "avares://Hamlet.App/Assets/hamlet-logo.svg";

    /// <summary>Where the small mark lives.</summary>
    public const string SmallMarkUri = "avares://Hamlet.App/Assets/hamlet-mark-small.svg";

    private static readonly Lazy<DrawingImage> FullImage =
        new(() => new DrawingImage(Load(FullMarkUri)));

    private static readonly Lazy<DrawingImage> SmallImage =
        new(() => new DrawingImage(Load(SmallMarkUri)));

    /// <summary>The full mark, for an `Image` to bind to.</summary>
    /// <remarks>
    /// **LAZY, BECAUSE LOADING REACHES THE ASSET LOADER** and a static that ran at
    /// type-load would throw anywhere an Avalonia application had not been set up
    /// yet — a designer, a unit test of something else entirely.
    /// </remarks>
    public static DrawingImage Full => FullImage.Value;

    /// <summary>The small mark, for an `Image` to bind to.</summary>
    public static DrawingImage Small => SmallImage.Value;

    /// <summary>Load one of the marks as something Avalonia can draw.</summary>
    /// <param name="uri">An `avares://` URI naming an SVG in this assembly.</param>
    /// <returns>The drawing, with its own viewBox as its bounds.</returns>
    /// <exception cref="ArgumentNullException">The URI is null.</exception>
    /// <exception cref="NotSupportedException">The file uses something unhandled.</exception>
    public static DrawingGroup Load(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        using var stream = AssetLoader.Open(new Uri(uri));

        var root = XDocument.Load(stream).Root
            ?? throw new NotSupportedException(uri + " has no root element");

        var box = ViewBox(root);

        var group = new DrawingGroup();

        // **THE VIEWBOX IS THE PICTURE, WHICH IS WHAT A BROWSER SCALES.** Without
        // this, the drawing's bounds are its ink, and an `Image` would scale the ink
        // to the control instead of the box — so the mark would sit differently here
        // from everywhere else, and the clipping below would eat a different part of
        // it. An invisible rectangle over the box makes the two agree. It draws
        // nothing: no brush, no pen.
        group.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(box),
            Brush = null,
            Pen = null,
        });

        foreach (var drawing in Children(root, Matrix.Identity))
        {
            group.Children.Add(drawing);
        }

        // **THE VIEWBOX IS THE BOUNDS, WHICH IS WHAT MAKES CLIPPING VISIBLE.**
        // Anything the file draws outside its own viewBox is outside these bounds
        // too, exactly as a browser would clip it — so a mark that does not fit its
        // box looks wrong here in the same way it looks wrong anywhere else, rather
        // than being quietly rescued.
        group.ClipGeometry = new RectangleGeometry(box);

        return group;
    }

    /// <summary>The file's own viewBox.</summary>
    /// <param name="root">The `svg` element.</param>
    /// <returns>The rectangle.</returns>
    public static Rect ViewBox(XElement root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var raw = (string?)root.Attribute("viewBox")
            ?? throw new NotSupportedException("the mark has no viewBox");

        var n = Numbers(raw);

        return n.Count == 4
            ? new Rect(n[0], n[1], n[2], n[3])
            : throw new NotSupportedException("viewBox is not four numbers: " + raw);
    }

    /// <summary>
    /// **What the file actually draws, in its own coordinates.**
    /// </summary>
    /// <param name="uri">An `avares://` URI naming an SVG in this assembly.</param>
    /// <returns>The union of every element's bounds, ignoring the viewBox.</returns>
    /// <remarks>
    /// **THIS IS HOW CLIPPING IS MEASURED RATHER THAN ARGUED.** A mark whose ink
    /// runs outside its viewBox loses that ink wherever it is drawn, and the only
    /// honest way to say so is to compare the two rectangles.
    /// </remarks>
    public static Rect Extent(string uri)
    {
        Rect? bounds = null;

        foreach (var drawing in Shapes(uri))
        {
            var here = drawing.GetBounds();

            bounds = bounds is { } grown ? grown.Union(here) : here;
        }

        return bounds ?? default;
    }

    /// <summary>
    /// **Only the shapes the file draws**, without the viewBox spacer.
    /// </summary>
    /// <param name="uri">An `avares://` URI naming an SVG in this assembly.</param>
    /// <returns>One drawing per element in the file.</returns>
    /// <remarks>
    /// <see cref="Load"/> puts an invisible rectangle over the viewBox in front of
    /// these so an `Image` scales the box rather than the ink. That rectangle is not
    /// part of the mark, and anything asking what the file draws wants this.
    /// </remarks>
    public static IReadOnlyList<GeometryDrawing> Shapes(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        using var stream = AssetLoader.Open(new Uri(uri));

        var root = XDocument.Load(stream).Root
            ?? throw new NotSupportedException(uri + " has no root element");

        return Children(root, Matrix.Identity).ToList();
    }

    /// <summary>Every drawable under one element, with the transform applied.</summary>
    /// <param name="parent">The element to walk.</param>
    /// <param name="soFar">The transform inherited from above.</param>
    /// <returns>The drawings.</returns>
    private static IEnumerable<GeometryDrawing> Children(XElement parent, Matrix soFar)
    {
        foreach (var child in parent.Elements())
        {
            var name = child.Name.LocalName;

            if (name is "title" or "desc" or "metadata")
            {
                continue;
            }

            if (name == "g")
            {
                var here = Transform(child) * soFar;

                foreach (var inner in Children(child, here))
                {
                    yield return inner;
                }

                continue;
            }

            yield return Shape(child, soFar);
        }
    }

    /// <summary>One shape, as a drawing.</summary>
    /// <param name="element">The element.</param>
    /// <param name="transform">The transform in force.</param>
    /// <returns>The drawing.</returns>
    private static GeometryDrawing Shape(XElement element, Matrix transform)
    {
        var geometry = Geometry(element);

        geometry.Transform = new MatrixTransform(transform);

        var stroke = Paint(element, "stroke");

        return new GeometryDrawing
        {
            Geometry = geometry,
            Brush = Paint(element, "fill"),
            Pen = stroke is null
                ? null
                : new Pen(
                    stroke,
                    Number(element, "stroke-width", 1),
                    lineCap: Cap(element),
                    lineJoin: Join(element)),
        };
    }

    /// <summary>The geometry of one shape.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The geometry.</returns>
    /// <exception cref="NotSupportedException">The element is not one of the four.</exception>
    private static Geometry Geometry(XElement element)
        => element.Name.LocalName switch
        {
            "rect" => new RectangleGeometry(
                new Rect(
                    Number(element, "x", 0), Number(element, "y", 0),
                    Number(element, "width", 0), Number(element, "height", 0)),
                Number(element, "rx", 0),
                Number(element, "ry", Number(element, "rx", 0))),

            "circle" => new EllipseGeometry(
                new Rect(
                    Number(element, "cx", 0) - Number(element, "r", 0),
                    Number(element, "cy", 0) - Number(element, "r", 0),
                    Number(element, "r", 0) * 2,
                    Number(element, "r", 0) * 2)),

            "line" => new LineGeometry(
                new Point(Number(element, "x1", 0), Number(element, "y1", 0)),
                new Point(Number(element, "x2", 0), Number(element, "y2", 0))),

            // **AVALONIA'S PARSER SPEAKS THE SVG PATH MINI-LANGUAGE**, which is the
            // one part of this that would have been worth a dependency.
            "path" => Avalonia.Media.Geometry.Parse(
                (string?)element.Attribute("d")
                ?? throw new NotSupportedException("a path with no d")),

            "text" => Text(element),

            _ => throw new NotSupportedException(
                "the mark uses <" + element.Name.LocalName + ">, which this loader "
                + "does not handle. It is deliberately narrow: an element it drew "
                + "wrongly would be worse than one it refused."),
        };

    /// <summary>
    /// **A `text` element, as geometry.**
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The outlines of its glyphs.</returns>
    /// <remarks>
    /// <para>**ADDED FOR THE MARK OF WORK INSTRUCTION 286, WHICH PUTS `USB-D`, `FIL1`,
    /// `RX`, `UTC` AND THE FREQUENCY ON THE DISPLAY.** The loader refused it exactly as
    /// unit 285 built it to — *the mark uses &lt;text&gt;, which this loader does not
    /// handle* — and the order's rule is that **the mark is approved and the loader is
    /// not**, so the loader is what changes.</para>
    /// <para>**GLYPH OUTLINES RATHER THAN A TEXT DRAWING**, because a `DrawingGroup`
    /// holds geometry and Avalonia has no text drawing to put in one. It also means
    /// the letters scale with everything else and the mark stays one object.</para>
    /// <para>**SVG PUTS THE BASELINE AT `y` AND `FormattedText` PUTS THE TOP THERE.**
    /// The difference is the baseline distance, and it is subtracted rather than
    /// eyeballed — without it every label on the display sits a line too low.</para>
    /// <para>**A FONT THIS MACHINE DOES NOT HAVE FALLS BACK RATHER THAN FAILING.**
    /// `Consolas, monospace` is what the file asks for and what the application's own
    /// readouts use; where it is missing the platform substitutes, which changes the
    /// letterforms and not whether the mark draws.</para>
    /// </remarks>
    private static Geometry Text(XElement element)
    {
        var body = element.Value;

        if (string.IsNullOrEmpty(body))
        {
            throw new NotSupportedException("a text element with nothing in it");
        }

        var weight = (string?)element.Attribute("font-weight") switch
        {
            "bold" or "700" => FontWeight.Bold,
            "600" => FontWeight.SemiBold,
            "500" => FontWeight.Medium,
            _ => FontWeight.Normal,
        };

        var typeface = new Typeface(
            new FontFamily((string?)element.Attribute("font-family") ?? "monospace"),
            FontStyle.Normal,
            weight);

        var text = new FormattedText(
            body,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            Number(element, "font-size", 12),
            Brushes.Black);

        var x = Number(element, "x", 0);
        var y = Number(element, "y", 0);

        return text.BuildGeometry(new Point(x, y - text.Baseline))
            ?? throw new NotSupportedException(
                "the text \"" + body + "\" produced no geometry");
    }

    /// <summary>A `translate` on a group, or the identity.</summary>
    /// <param name="element">The group.</param>
    /// <returns>The matrix.</returns>
    private static Matrix Transform(XElement element)
    {
        var raw = (string?)element.Attribute("transform");

        if (string.IsNullOrWhiteSpace(raw))
        {
            return Matrix.Identity;
        }

        if (!raw.TrimStart().StartsWith("translate", StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                "the mark uses the transform \"" + raw + "\"; this loader handles "
                + "translate only");
        }

        var n = Numbers(raw);

        return Matrix.CreateTranslation(n[0], n.Count > 1 ? n[1] : 0);
    }

    /// <summary>A fill or stroke, or null where the file says none.</summary>
    /// <param name="element">The element.</param>
    /// <param name="attribute">`fill` or `stroke`.</param>
    /// <returns>The brush, or null.</returns>
    private static IBrush? Paint(XElement element, string attribute)
    {
        var raw = (string?)element.Attribute(attribute);

        if (attribute == "fill" && raw is null)
        {
            // SVG fills black by default, and every shape in these two files that
            // wants a fill says so. Following the spec here rather than guessing.
            return Brushes.Black;
        }

        return raw is null or "none"
            ? null
            : new SolidColorBrush(Color.Parse(raw));
    }

    /// <summary>The line cap, defaulting to butt as SVG does.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The cap.</returns>
    private static PenLineCap Cap(XElement element)
        => (string?)element.Attribute("stroke-linecap") switch
        {
            "round" => PenLineCap.Round,
            "square" => PenLineCap.Square,
            _ => PenLineCap.Flat,
        };

    /// <summary>The line join, defaulting to miter as SVG does.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The join.</returns>
    private static PenLineJoin Join(XElement element)
        => (string?)element.Attribute("stroke-linejoin") switch
        {
            "round" => PenLineJoin.Round,
            "bevel" => PenLineJoin.Bevel,
            _ => PenLineJoin.Miter,
        };

    /// <summary>One numeric attribute, or a default.</summary>
    /// <param name="element">The element.</param>
    /// <param name="attribute">Its name.</param>
    /// <param name="fallback">What to use where it is absent.</param>
    /// <returns>The number.</returns>
    private static double Number(XElement element, string attribute, double fallback)
        => (string?)element.Attribute(attribute) is { } raw
           && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
            ? v
            : fallback;

    /// <summary>Every number in a string, in order.</summary>
    /// <param name="raw">The text.</param>
    /// <returns>The numbers.</returns>
    private static List<double> Numbers(string raw)
        => System.Text.RegularExpressions.Regex
            .Matches(raw, @"-?\d+(\.\d+)?")
            .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture))
            .ToList();
}
