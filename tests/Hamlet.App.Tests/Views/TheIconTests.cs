using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **The icon is the amber quill on night, shipped as an icon file** (work instruction 508, HM-DEC-212).
/// Tim, 2026-09-30: *"Ours looks like an eight-year-old did it."*
/// </summary>
public sealed class TheIconTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the frames and windows are printed.</param>
    public TheIconTests(ITestOutputHelper output) => _output = output;

    /// <summary>The repository root: the folder holding `Hamlet.sln`.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above " + AppContext.BaseDirectory);
    }

    /// <summary>The square sizes an `.ico` file's directory lists, in order; 0 in a size byte means 256.</summary>
    internal static IReadOnlyList<int> Frames(byte[] ico)
    {
        if (ico.Length < 6 || BitConverter.ToUInt16(ico, 0) != 0 || BitConverter.ToUInt16(ico, 2) != 1)
        {
            return Array.Empty<int>();
        }

        var count = BitConverter.ToUInt16(ico, 4);

        return Enumerable.Range(0, count)
            .Select(i => ico[6 + (16 * i)] is var w && w == 0 ? 256 : (int)ico[6 + (16 * i)])
            .ToList();
    }

    /// <remarks>
    /// Task 1: `Hamlet.App.csproj` names an `ApplicationIcon`, so `Hamlet.exe` carries it in Explorer, on a
    /// shortcut and when pinned, and the file it names exists and is an icon of eight frames at 16, 20, 24,
    /// 32, 40, 48, 64 and 256.
    /// </remarks>
    [Fact]
    public void TheProgramCarriesTheIconFile()
    {
        var project = Path.Combine(Root(), "src", "Hamlet.App", "Hamlet.App.csproj");
        var named = XDocument.Load(project).Descendants("ApplicationIcon").Select(e => e.Value.Trim()).SingleOrDefault();

        _output.WriteLine("ApplicationIcon: " + (named ?? "none"));

        Assert.False(string.IsNullOrEmpty(named), "Hamlet.App.csproj names no ApplicationIcon");

        var file = Path.Combine(Path.GetDirectoryName(project)!, named!.Replace('\\', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(file), "the ApplicationIcon " + named + " does not exist");

        var frames = Frames(File.ReadAllBytes(file));

        _output.WriteLine("frames: " + string.Join(", ", frames));

        Assert.Equal(new[] { 16, 20, 24, 32, 40, 48, 64, 256 }, frames);
    }

    /// <remarks>
    /// Task 2: the icon every window carries is the icon file itself, handed to the platform whole so
    /// Windows picks the frame drawn for the size it wants, and never a raster of the old small mark.
    /// </remarks>
    [AvaloniaFact]
    public void TheWindowIconIsTheIconFile()
    {
        Assert.Equal("avares://Hamlet.App/Assets/hamlet.ico", AppIcon.Source);

        Assert.NotNull(AppIcon.Current);

        // The headless platform keeps no bytes of an icon it is handed (its Save writes nothing), so
        // what is read here is the file the icon is loaded from, through the same resource loader.
        using var file = Avalonia.Platform.AssetLoader.Open(new Uri(AppIcon.Source));
        using var bytes = new MemoryStream();
        file.CopyTo(bytes);

        var frames = Frames(bytes.ToArray());

        _output.WriteLine("loaded from " + AppIcon.Source + ", " + bytes.Length + " bytes, frames: " + string.Join(", ", frames));

        Assert.Equal(new[] { 16, 20, 24, 32, 40, 48, 64, 256 }, frames);
    }

    /// <remarks>
    /// Task 2: a missing icon file costs the icon and nothing else - the load answers null and throws
    /// nothing, so no window fails to open over a picture of itself (§8, never-throw).
    /// </remarks>
    [AvaloniaFact]
    public void AMissingIconFileIsNoIconAndNoThrow()
    {
        var icon = AppIcon.Load("avares://Hamlet.App/Assets/no-such-icon.ico");

        Assert.Null(icon);
    }

    /// <remarks>
    /// Task 2: every window the application opens carries the icon, shown on a rendering host - the
    /// main window, the achievements, About, and each dialog. Set in one place, `App.axaml`.
    /// </remarks>
    [AvaloniaFact]
    public void EveryWindowCarriesTheIcon()
    {
        var windows = typeof(MainWindow).Assembly.GetTypes()
            .Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) is not null)
            .OrderBy(t => t.Name)
            .ToList();

        _output.WriteLine("windows: " + windows.Count);

        Assert.Equal(10, windows.Count);

        var bare = new List<string>();

        foreach (var type in windows)
        {
            var window = (Window)Activator.CreateInstance(type)!;

            window.Show();

            var carries = window.Icon is not null && ReferenceEquals(window.Icon, AppIcon.Current);

            _output.WriteLine(type.Name + ": " + (carries ? "carries the icon" : "no icon"));

            if (!carries)
            {
                bare.Add(type.Name);
            }

            window.Close();
        }

        Assert.True(bare.Count == 0, "windows without the icon: " + string.Join(", ", bare));
    }
}
