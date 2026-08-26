using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The per-character span ratio reaches the capture sheet, and says nothing where
/// nothing was measured.
/// </summary>
/// <remarks>
/// <para>**THE FAILURE THIS CATCHES WOULD BE SILENT** (§0.0.1, HM-DEC-007). The
/// field is computed in the engine and written nowhere else, so a change that
/// dropped it on the way to the sheet would leave every test in the engine green
/// and quietly take away the only per-character evidence a capture carries. What
/// makes it worth a test is that nobody would notice for months: the sheet would
/// still be full of numbers.</para>
/// <para>**AND THE UNMEASURED CASE IS THE ONE THAT MATTERS MORE.** A pass that
/// does not compute the ratio must say so rather than print a nought, because
/// nought is a real reading — it is a character the key having been up explains
/// exactly as well (§12.4).</para>
/// </remarks>
public sealed class TheSpanRatioReachesTheSidecarTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheSpanRatioReachesTheSidecarTests(ITestOutputHelper output)
        => _output = output;

    private static CwCharacter Character(string text, double ratio)
        => new(
            text,
            CwConfidence.High,
            21.0,
            ".-",
            double.NaN,
            18,
            TimeSpan.FromSeconds(1))
        {
            SpanLogLikelihoodRatio = ratio,
        };

    /// <remarks>
    /// <para>Proves the quotient reaches the sheet, and reaches it because the
    /// two figures beside it cannot always be divided by a reader. Both are
    /// clamped at a million before they are printed, and unit 1.11.14 measured a
    /// raw margin of 2.98 × 10⁸ on `cw-2026-08-17-013347` — so on the recordings
    /// where the scale problem is worst, the printed line says `>1000000` and
    /// the one comparable form of the quantity is gone.</para>
    /// <para>**IT IS NOT A THRESHOLD AND NOTHING READS IT.** Measured across
    /// 1,580 characters it does not separate a good character from a bad one:
    /// split by whether the recording carries an adjudicated anchor, the medians
    /// are 0.004 and 0.005. It is on the sheet so the next question can be asked
    /// from a record rather than from a rebuild.</para>
    /// </remarks>
    [Fact]
    public void TheMarginsShareOfTheSpanSurvivesTheClamp()
    {
        var line = MainWindowViewModel.SpanRatioLine(
            new[]
            {
                // A margin the clamp destroys, over a span the clamp destroys.
                // Their quotient is 0.298 and neither printed figure shows it.
                new CwCharacter(
                    "C", CwConfidence.High, 1, "-.-.", 20, 18,
                    TimeSpan.FromSeconds(1))
                {
                    SpanLogLikelihoodRatio = 1e9,
                    MarginLlr = 2.98e8,
                },
            },
            "since listening started");

        _output.WriteLine(line);

        Assert.Contains(">1000000", line, StringComparison.Ordinal);
        Assert.Contains("0.298", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a character with no span to measure against says so rather than
    /// printing a quotient of nought, which is a different claim (§0.0).
    /// </remarks>
    [Fact]
    public void AQuotientWithNothingUnderItSaysSo()
    {
        var line = MainWindowViewModel.SpanRatioLine(
            new[]
            {
                new CwCharacter(
                    "E", CwConfidence.High, 1, ".", 20, 18,
                    TimeSpan.FromSeconds(1))
                {
                    SpanLogLikelihoodRatio = 4.0,
                    MarginLlr = double.NaN,
                },
            },
            "since listening started");

        _output.WriteLine(line);

        Assert.Contains("unmeasured", line, StringComparison.Ordinal);
        Assert.DoesNotContain("0.000", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the whole path from the field to the line a person reads: each
    /// character appears with its own number beside it, and the line says what
    /// the number is measured against rather than leaving a reader to guess.
    /// </remarks>
    [Fact]
    public void EachCharacterAppearsWithItsOwnEvidence()
    {
        var line = MainWindowViewModel.SpanRatioLine(
            new[]
            {
                Character("C", 142.5),
                Character("Q", 98.25),
                Character(MorseAlphabet.WordGap, double.NaN),
                Character("K", 3.4),
            },
            "since listening started");

        _output.WriteLine(line);

        Assert.Contains("C:142.5", line, StringComparison.Ordinal);
        Assert.Contains("Q:98.3", line, StringComparison.Ordinal);
        Assert.Contains("K:3.4", line, StringComparison.Ordinal);
        Assert.Contains("3 of the last 4 characters", line, StringComparison.Ordinal);
        Assert.Contains(
            "against the key having been up", line, StringComparison.Ordinal);
        Assert.Contains("since listening started", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the word gap is left out rather than printed as a nought. Nothing
    /// was keyed in it, so there are no marks for the quantity to be measured
    /// over, and a nought there is a number somebody would later reason from.
    /// </remarks>
    [Fact]
    public void AWordGapCarriesNoNumberBecauseNothingWasKeyedInIt()
    {
        var line = MainWindowViewModel.SpanRatioLine(
            new[] { Character(MorseAlphabet.WordGap, double.NaN) },
            "since listening started");

        _output.WriteLine(line);

        Assert.Equal("unmeasured (no character carried a span ratio)", line);
    }

    /// <remarks>
    /// Proves a window that emitted nothing writes nothing: the line says nothing
    /// was read rather than printing an empty list, which reads as a measurement
    /// that came back empty.
    /// </remarks>
    [Fact]
    public void AWindowThatEmittedNothingSaysSoRatherThanPrintingAnEmptyList()
    {
        var line = MainWindowViewModel.SpanRatioLine(
            Array.Empty<CwCharacter>(), "since listening started");

        _output.WriteLine(line);

        Assert.Equal("nothing read yet", line);
    }
}
