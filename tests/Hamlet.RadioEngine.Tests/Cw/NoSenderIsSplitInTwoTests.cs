using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Work instruction 056, task 3: an admitted station's elements are divided by
/// their own pitch only where the division is proved, and today it is not.
/// </summary>
/// <remarks>
/// <para>**THIS SUITE LOCKS A REFUSAL, WHICH IS THE UNUSUAL SHAPE AND THE RIGHT
/// ONE HERE.** The measurement exists, the statistics are real, and the verdict is
/// withheld because four criteria were surveyed across every capture in the tree
/// and each either missed `cw-2026-08-31-002829` or fired on a recording known to
/// hold one operator. `CwStreamSplit`'s own remarks carry that survey.</para>
/// <para>**WHAT IT WOULD COST TO BE WRONG.** A collision reads badly and looks
/// like it reads badly. A sender split in two reads cleanly and is wrong, which is
/// exactly the guess dressed as an answer §0.0 forbids — so the test that matters
/// is the one that fails if a later unit ships a criterion without proving it.
/// </para>
/// </remarks>
public sealed class NoSenderIsSplitInTwoTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the statistics are printed.</param>
    public NoSenderIsSplitInTwoTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The captures Hamlet reads best, which hold one operator each, are never
    /// divided.
    /// </summary>
    /// <remarks>
    /// **THESE FOUR READ AT A PRECISION OF 1.000 OR NEAR IT**, so a division here
    /// would be a division of a sender Hamlet is already reading correctly, which
    /// is the worst case this refusal exists to prevent.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-08-18-003758", 498.6)]
    [InlineData("cw-2026-08-18-004507", 500.8)]
    [InlineData("cw-2026-08-22-031838", 500.1)]
    [InlineData("cw-2026-08-25-013637", 535.7)]
    public void ASenderHamletAlreadyReadsIsNeverDivided(string capture, double toneHz)
    {
        var division = DivideOf(capture, toneHz);

        _output.WriteLine(
            $"{capture}: {division.Trusted} trusted, "
            + $"{division.LowerHz:0.0} and {division.UpperHz:0.0} Hz, "
            + $"apart {division.ApartHz:0.0}, scatter {division.ScatterHz:0.00}, "
            + $"separation {division.Separation:0.0}, {division.Handovers} handovers");

        Assert.False(
            division.Split,
            $"{capture} holds one operator and was divided in two");
    }

    /// <summary>
    /// The two-sender capture is not divided either, and that is the finding
    /// rather than the goal.
    /// </summary>
    /// <remarks>
    /// **THE ASSERTION IS DELIBERATELY THE ONE THE WORK ORDER DID NOT WANT.**
    /// Its acceptance was two streams near 602 and 615 Hz. What the survey found
    /// is that no threshold reaching them leaves `cw-2026-08-18-003758` alone, so
    /// the honest state is a refusal with its evidence attached. **A unit that
    /// proves a criterion changes this line and says so** — it is here so that the
    /// change is deliberate rather than incidental.
    /// </remarks>
    [Fact]
    public void TheTwoSenderCaptureIsNotYetDividedEither()
    {
        var division = DivideOf("cw-2026-08-31-002829", 609.4);

        _output.WriteLine(
            $"002829: {division.Trusted} trusted, "
            + $"{division.LowerHz:0.0} and {division.UpperHz:0.0} Hz, "
            + $"apart {division.ApartHz:0.0}, scatter {division.ScatterHz:0.00}, "
            + $"separation {division.Separation:0.0}, {division.Handovers} handovers");

        Assert.False(division.Split);

        // The measurement is real even where the verdict is withheld, and this is
        // the half a later unit builds on.
        Assert.True(
            division.ApartHz > 5.0,
            $"the two heaps stand {division.ApartHz:0.0} Hz apart, which is less "
            + "than the survey measured");
    }

    /// <summary>Anything with too few long marks says so rather than dividing.</summary>
    [Fact]
    public void TooFewLongMarksIsReportedAndNotResolved()
    {
        var division = CwStreamSplit.Divide(Array.Empty<CwElement>());

        Assert.False(division.Split);
        Assert.Equal(0, division.Trusted);
        Assert.True(double.IsNaN(division.ApartHz));
    }

    private static CwStreamSplit.CwStreamDivision DivideOf(
        string capture, double toneHz)
    {
        var audio = WavAudio.Read(CapturePath(capture));

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, toneHz);

        var read = CwProbabilisticDecoder.Decode(envelope, toneHz, null, null, false);

        var measured = CwElementPitch.MeasureAll(
            read.Elements, audio.Samples, audio.SampleRate, toneHz,
            CwProbabilisticDecoder.HopMilliseconds);

        return CwStreamSplit.Divide(measured);
    }

    private static string CapturePath(string capture)
    {
        var here = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(here))
        {
            var folder = Path.Combine(here, "tests", "fixtures", "cw", "captured");

            if (Directory.Exists(folder))
            {
                return Directory
                    .GetFiles(folder, capture + ".wav", SearchOption.AllDirectories)
                    .Single();
            }

            here = Path.GetDirectoryName(here.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        throw new DirectoryNotFoundException("no captured fixtures folder above " + AppContext.BaseDirectory);
    }
}
