using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Hamlet.App.Controls;

/// <summary>
/// The window and taskbar icon: the amber quill on night, loaded from the icon file.
/// </summary>
/// <remarks>
/// <para>**THE FILE IS HANDED OVER WHOLE** (work instruction 508, HM-DEC-212).
/// `Assets/hamlet.ico` carries eight frames, 16 to 256, each drawn for its size, and
/// the platform is given the file rather than one picture, so Windows picks the frame
/// drawn for the size it wants. Nothing here scales one frame to make another.</para>
/// <para>**EVERY WINDOW TAKES IT FROM ONE PLACE**: a style in `App.axaml` sets
/// `Window.Icon` to <see cref="Current"/> for every window the application opens.</para>
/// <para>**IT REPLACES THE SMALL MARK** that work instruction 285 rasterised here at run
/// time from `Assets/hamlet-mark-small.svg`, one 256-pixel render scaled down to every
/// size, with no `.ico` in the tree. Tim, 2026-09-30: *"It looks terrible."*</para>
/// </remarks>
public static class AppIcon
{
    /// <summary>Where the icon file lives among the shell's resources.</summary>
    public const string Source = "avares://Hamlet.App/Assets/hamlet.ico";

    private static readonly Lazy<WindowIcon?> Shared = new(() => Load(Source));

    /// <summary>The icon, or null where the file could not be loaded.</summary>
    /// <remarks>
    /// **NULL IS A REAL ANSWER AND THE WINDOW HANDLES IT.** A window that refused to
    /// open because it could not load a picture of itself would be the worst possible
    /// trade (§8, never-throw).
    /// </remarks>
    public static WindowIcon? Current => Shared.Value;

    /// <summary>Load an icon file among the shell's resources.</summary>
    /// <param name="uri">The `avares://` address of the file.</param>
    /// <returns>The icon, or null where the file is missing or unreadable.</returns>
    /// <remarks>
    /// Never-throw (§8): the icon is decoration, and nothing about it is worth taking a
    /// window down for. The bytes are copied first, so the icon never holds a resource
    /// stream open.
    /// </remarks>
    internal static WindowIcon? Load(string uri)
    {
        try
        {
            using var file = AssetLoader.Open(new Uri(uri));
            var bytes = new MemoryStream();

            file.CopyTo(bytes);
            bytes.Position = 0;

            return new WindowIcon(bytes);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
