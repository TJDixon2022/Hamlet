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
}
