using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Copy puts the terminal's text on the clipboard** (work instruction numbered 509, run as unit
/// 510, task 2, HM-DEC-214).
/// </summary>
/// <remarks>
/// The real window on the CW tab; a call decoded from synthetic audio written here, nothing read
/// from disk (R96), settled into the window's own transcript as the tab wires it.
/// </remarks>
public sealed class TheTerminalCopiesTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the clipboard is printed.</param>
    public TheTerminalCopiesTests(ITestOutputHelper output) => _output = output;

    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>The call decoded into a transcript, as the tab settles it.</summary>
    internal static void Decode(CwTranscript transcript)
    {
        var samples = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 5090)).Samples;
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate) { DetectorMarks = detector.MarksSince };

        decoder.CharacterSettled += transcript.Settle;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, samples.AsSpan(at, Chunk)));
            detector.Process(samples.AsSpan(at, Chunk));
        }

        decoder.Flush();
    }

    /// <remarks>
    /// Task 2: after a call, pressing `Copy` beside `Clear` leaves the terminal's text on the
    /// clipboard, and the button says on hover what it copies.
    /// </remarks>
    [AvaloniaFact]
    public async Task CopyPutsTheTerminalOnTheClipboard()
    {
        var panel = new MainWindowViewModel(new AppSettings(), null) { OperatingMode = "CW", TerminalExpanded = true };
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        Decode(panel.Transcript);
        Pump(window);

        var copy = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Content as string == "Copy");

        Assert.True(copy is not null, "there is no Copy button on the CW tab");
        Assert.False(string.IsNullOrWhiteSpace(ToolTip.GetTip(copy!) as string), "Copy says nothing on hover");

        copy!.Command!.Execute(copy.CommandParameter);
        Pump(window);

        var held = await window.Clipboard!.GetTextAsync();

        _output.WriteLine($"terminal `{panel.Transcript.PlainText}`");
        _output.WriteLine($"clipboard `{held}`");

        Assert.Equal(Call, held?.Trim());
        window.Close();
    }
}
