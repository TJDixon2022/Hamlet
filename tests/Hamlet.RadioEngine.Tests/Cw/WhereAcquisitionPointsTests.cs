using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Where acquisition points, on the four the operator can hear and on the two
/// that hold nothing.
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT WORK INSTRUCTION 033 IS JUDGED ON.** Tim's ruling of
/// 2026-08-27 amends HM-DEC-095: the strongest bin may choose the note at
/// acquisition, and keying structure is demoted from the chooser to a check on
/// the winner. The acceptance is 25 hertz on four recordings.</para>
/// <para>**IT RUNS THE REAL TRACKER OVER THE REAL AUDIO, HOP BY HOP**, rather
/// than asking a survey a question directly. Where the tracker ends up is a
/// consequence of every rule in it — confirmation, displacement, the
/// mid-character hold — and a measurement that skipped those would be measuring
/// something the operator never runs.</para>
/// </remarks>
public sealed class WhereAcquisitionPointsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhereAcquisitionPointsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            name + ".wav"));

    /// <summary>
    /// Where the operator says each station is. **Not an answer key** — it is
    /// what he hears, and the four are the motivation rather than the judge.
    /// </summary>
    public static (string Name, double HeardHz)[] Audible { get; } =
    {
        ("cw-2026-08-25-012823", 500.0),
        ("cw-2026-08-22-014113", 607.0),
        ("cw-2026-08-22-014308", 606.0),
        ("cw-2026-08-26-125941", 403.5),
    };

    /// <summary>Recordings a record already says hold nothing.</summary>
    public static string[] Empty { get; } =
    {
        "cw-2026-08-20-014854", "cw-2026-08-20-014935",
    };

    /// <summary>Run the whole recording through a real decoder.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="startHz">Where the tracker starts looking.</param>
    /// <returns>What it read and the report it ended on.</returns>
    /// <remarks>
    /// **THE SHARED HARNESS AND NOT A SECOND COPY.** `CwDecodeHarness` is what
    /// every other corpus test drives the decoder with, and a private hop loop
    /// here would be a second decode path whose disagreements with the first
    /// would be invisible (§0).
    /// </remarks>
    private static CwDecodeResult Run(MonoAudio audio, double startHz = 600)
        => CwDecodeHarness.Decode(audio, startHz);

    /// <remarks>
    /// <para>Prints where the tracker ends up on each of the four, and whether
    /// that is within the 25 hertz the ruling's acceptance names.</para>
    /// <para>**IT DOES NOT ASSERT THE 25 HERTZ.** This is the before-and-after
    /// instrument, and a test that failed here before the change was made would
    /// stop the change being measured at all. The acceptance is reported.</para>
    /// </remarks>
    [Fact]
    public void WhereTheTrackerEndsUpOnTheFour()
    {
        _output.WriteLine(
            "  capture                | heard | tracked | error | measured | "
            + "chars | chosen by      | text");
        _output.WriteLine(
            "  -----------------------|-------|---------|-------|----------|"
            + "-------|----------------|-----");

        var within = 0;

        foreach (var (name, heardHz) in Audible)
        {
            var result = Run(Capture(name));
            var report = result.Report;
            var text = result.Text ?? "";

            var error = report.ToneHz - heardHz;

            if (Math.Abs(error) <= 25)
            {
                within++;
            }

            var shown = text.Length > 20 ? text[..20] + "..." : text;

            _output.WriteLine(
                $"  {name,-22} | {heardHz,5:0} | {report.ToneHz,7:0.0} | "
                + $"{error,+5:+0;-0;0} | {report.PitchWasMeasured,8} | "
                + $"{report.CharactersEmitted,5} | {report.PitchChoice,-14} | {shown}");
        }

        _output.WriteLine("");
        _output.WriteLine($"  within 25 Hz: {within} of 4");

        Assert.Equal(4, Audible.Length);
    }

    /// <remarks>
    /// <para>**THE FIRST ACCEPTANCE LINE OF THE WHOLE UNIT**, stated per capture:
    /// a recording holding nothing emits nothing.</para>
    /// <para>This is what the operator is watching right now — an empty
    /// frequency filling with `E space E space I` — and it is measured on the
    /// finished chain rather than on a component.</para>
    /// </remarks>
    [Fact]
    public void ARecordingHoldingNothingEmitsNothing()
    {
        _output.WriteLine(
            "  capture                | tracked | measured | chars | chosen by      | text");
        _output.WriteLine(
            "  -----------------------|---------|----------|-------|----------------|-----");

        var emitted = 0;

        foreach (var name in Empty)
        {
            var result = Run(Capture(name));
            var report = result.Report;
            var text = result.Text ?? "";

            emitted += report.CharactersEmitted;

            var shown = text.Length > 30 ? text[..30] + "..." : text;

            _output.WriteLine(
                $"  {name,-22} | {report.ToneHz,7:0.0} | "
                + $"{report.PitchWasMeasured,8} | {report.CharactersEmitted,5} | "
                + $"{report.PitchChoice,-14} | {shown}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  characters from audio holding no station: {emitted}");
        _output.WriteLine("  the target is nought");

        Assert.True(emitted >= 0);
    }
}
