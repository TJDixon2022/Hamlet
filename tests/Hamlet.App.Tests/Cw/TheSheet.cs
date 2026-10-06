using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// The capture sheet as the press composes it, and the tree's root, for the tests that read a sheet. Kept from the sheet
/// fact that came out with the keying meter (work instruction 545).
/// </summary>
internal static class TheSheet
{
    /// <summary>The sheet exactly as the press composes it.</summary>
    internal static string Compose(
        CwDecoder decoder,
        MonoAudio audio,
        CwDecodeReport report,
        string tonePeak,
        DateTime? clearedUtc = null)
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        Field("_decoder").SetValue(model, decoder);

        if (clearedUtc is { } cleared)
        {
            Field("_clearedUtc").SetValue(model, cleared);
        }

        var writer = typeof(MainWindowViewModel).GetMethod(
            "CaptureNotes", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(writer);

        return (string)writer!.Invoke(
            model, [audio, decoder.Tap.SamplesSeen, report, decoder.ShapeSide, tonePeak])!;
    }

    private static FieldInfo Field(string name)
    {
        var field = typeof(MainWindowViewModel).GetField(
            name, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);

        return field!;
    }

    /// <summary>The tree's root: the folder holding <c>tests\fixtures</c>.</summary>
    internal static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
