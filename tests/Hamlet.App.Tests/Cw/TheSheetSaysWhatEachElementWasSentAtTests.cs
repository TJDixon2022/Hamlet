using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 056, task 5: the capture sheet carries what pitch each
/// element was sent at.
/// </summary>
/// <remarks>
/// <para>**THE LINE REPORTS AND DOES NOT CONCLUDE, AND THAT IS WHAT IS TESTED
/// HERE.** Task 5 asked for per-stream lines where task 3 split streams. Task 3
/// splits nothing — no criterion measured across this corpus divides the
/// two-sender case from the clean ones — so what the sheet carries is the
/// measurement underneath the question rather than an answer to it. A line that
/// named two operators on the strength of an untested criterion would be exactly
/// the guess dressed as an answer §0.0 forbids, and it would be on the one
/// document written to be read months later as evidence.</para>
/// <para>**STATIC AND SEPARATE FROM THE VIEW MODEL**, for the reason
/// `KeyingLine` and `SpanRatioLine` are: what a record a person reads months
/// later says is worth a test of its own, and a test that has to build a window
/// to read one line will not be written.</para>
/// </remarks>
public sealed class TheSheetSaysWhatEachElementWasSentAtTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the line is printed.</param>
    public TheSheetSaysWhatEachElementWasSentAtTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// An unmeasured pitch produces no element pitches, and says so rather than
    /// printing numbers taken against the middle of a bank.
    /// </summary>
    [Fact]
    public void AnUnmeasuredPitchSaysSoRatherThanPrintingNumbers()
    {
        var line = MainWindowViewModel.ElementPitchLine(
            Silence(), new CwDecodeReport { ToneHz = double.NaN });

        _output.WriteLine(line);

        Assert.Contains("not measured", line, StringComparison.Ordinal);
        Assert.DoesNotContain("Hz apart", line, StringComparison.Ordinal);
    }

    /// <summary>Silence gives nothing to measure, and the line says that too.</summary>
    /// <remarks>
    /// **HM-DEC-120, ON THE SHEET RATHER THAN ON THE SCREEN.** A recording holding
    /// no keying must not produce a confident spread figure, because a spread is a
    /// claim that there were elements to spread.
    /// </remarks>
    [Fact]
    public void SilenceProducesNoSpread()
    {
        var line = MainWindowViewModel.ElementPitchLine(
            Silence(), new CwDecodeReport { ToneHz = 600 });

        _output.WriteLine(line);

        Assert.Contains("too few", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// The line never asserts that two people were sending, whatever it measured.
    /// </summary>
    /// <remarks>
    /// **THE LOAD-BEARING ASSERTION IN THIS FILE.** It fails the day somebody
    /// wires a split verdict into the sheet without the criterion behind it, which
    /// is the failure task 3 refused to commit and this is the guard on that
    /// refusal.
    /// </remarks>
    [Theory]
    [InlineData(600.0)]
    [InlineData(500.0)]
    public void TheLineNeverSaysTwoOperators(double toneHz)
    {
        var line = MainWindowViewModel.ElementPitchLine(
            Keying(toneHz), new CwDecodeReport { ToneHz = toneHz });

        _output.WriteLine(line);

        foreach (var forbidden in new[]
                 {
                     "two operators", "two senders", "two stations",
                     "second operator", "second sender", "two people are",
                 })
        {
            Assert.DoesNotContain(
                forbidden, line, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Thirty seconds of nothing at all.</summary>
    private static MonoAudio Silence()
        => new(8_000, new float[8_000 * 30]);

    /// <summary>Real Morse from the engine's own synthesizer.</summary>
    /// <remarks>
    /// **THE FIRST VERSION KEYED A BARE 180-ON 180-OFF SQUARE WAVE AND THE
    /// MEASURING BRANCH NEVER RAN** — the decoder emitted nothing from it, so the
    /// line reported nought elements and the test passed without testing
    /// anything. Generated Morse at a slow speed gives marks long enough to
    /// resolve and a structure the decoder will actually read.
    /// </remarks>
    private static MonoAudio Keying(double toneHz)
        => CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K",
            WordsPerMinute: 12,
            ToneHz: toneHz));
}
