using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Hamlet.App.Controls;

/// <summary>
/// The window and taskbar icon, rendered from the small mark.
/// </summary>
/// <remarks>
/// <para>**PRODUCED FROM THE FILE, NOT DRAWN AGAIN** (work instruction 285 task 3).
/// `Assets/hamlet-mark-small.svg` is the source; this rasterises it at run time, so
/// there is no `.ico` in the tree to fall out of step with the drawing and no second
/// place anybody has to remember to update.</para>
/// <para>**AND NOTHING HERE RESCUES THE MARK.** The small mark draws 15.5 of its 68
/// units above its own viewBox, so an icon made from it is missing a little over
/// half the feather — the part that makes it a quill rather than a whip. That is
/// measured in `TheMarksRenderTests` and reported rather than corrected, because the
/// mark is Tim's (work instruction 285: report, do not redesign).</para>
/// <para>**ONE SIZE, RENDERED LARGE AND SCALED DOWN.** Windows asks for the icon at
/// several sizes and scaling a 256-pixel render down to 16 is kinder than scaling a
/// 32 up. **It costs one render at startup**, which is cheaper than the alternative
/// of shipping five bitmaps that can disagree.</para>
/// </remarks>
public static class AppIcon
{
    /// <summary>How large the icon is rendered before the platform scales it.</summary>
    /// <remarks>
    /// 256 is the largest size Windows asks for, so every smaller one is a
    /// downscale rather than an upscale.
    /// </remarks>
    public const int RenderedAt = 256;

    private static readonly Lazy<WindowIcon?> Shared = new(() => Build(RenderedAt));

    /// <summary>The icon, or null where this platform could not raster it.</summary>
    /// <remarks>
    /// **NULL IS A REAL ANSWER AND THE WINDOW HANDLES IT.** A headless run has no
    /// rasteriser, and a window that refused to open because it could not draw a
    /// picture of itself would be the worst possible trade (§8, never-throw).
    /// </remarks>
    public static WindowIcon? Small => Shared.Value;

    /// <summary>Render the small mark to an icon at one size.</summary>
    /// <param name="side">The square, in pixels.</param>
    /// <returns>The icon, or null where rasterising is not available.</returns>
    /// <remarks>
    /// Never-throw (§8): the icon is decoration, and nothing about it is worth
    /// taking the window down for.
    /// </remarks>
    public static WindowIcon? Build(int side)
    {
        try
        {
            return new WindowIcon(Raster(side));
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>The small mark as a bitmap.</summary>
    /// <param name="side">The square, in pixels.</param>
    /// <returns>The bitmap.</returns>
    public static RenderTargetBitmap Raster(int side)
    {
        var image = new Image
        {
            Source = SvgMark.Small,
            Width = side,
            Height = side,
            Stretch = Stretch.Uniform,
        };

        image.Measure(new Size(side, side));
        image.Arrange(new Rect(0, 0, side, side));

        var target = new RenderTargetBitmap(
            new PixelSize(side, side), new Vector(96, 96));

        target.Render(image);

        return target;
    }
}
