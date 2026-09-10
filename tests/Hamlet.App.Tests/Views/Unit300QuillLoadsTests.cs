using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 300: **what `SvgMark` makes of the shipped quill.**
/// </summary>
/// <remarks>
/// **THE INSTRUCTION ASKS WHAT IT REFUSES AND THIS MEASURES IT** rather than
/// reasoning about it. The file uses one `path` and one `line`, both of which that
/// loader has always handled, so whatever it refuses is not the elements.
/// </remarks>
public sealed class Unit300QuillLoadsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the finding is printed.</param>
    public Unit300QuillLoadsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Load it and say what happens.**</summary>
    [AvaloniaFact]
    public void TheQuillIsLoaded()
    {
        try
        {
            var shapes = SvgMark.Shapes(AchievementQuill.Uri);

            _output.WriteLine("shapes: " + shapes.Count);

            foreach (var shape in shapes)
            {
                _output.WriteLine("  geometry=" + shape.Geometry?.GetType().Name
                    + "  fill=" + (shape.Brush?.ToString() ?? "(none)")
                    + "  pen=" + (shape.Pen is null
                        ? "(none)"
                        : (shape.Pen.Brush?.ToString() ?? "(deferred)")
                          + " at " + shape.Pen.Thickness));
            }
        }
        catch (Exception e)
        {
            _output.WriteLine("REFUSED: " + e.GetType().Name + ": " + e.Message);

            throw;
        }
    }
}
