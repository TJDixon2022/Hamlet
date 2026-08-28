using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How a window scoring minus sixty-eight thousand emitted sixty-nine
/// characters.
/// </summary>
/// <remarks>
/// <para>**TASK 1 OF WORK INSTRUCTION 027, AND IT DECIDES THE SHAPE OF TASK 2.**
/// `cw-2026-08-28-005158.txt` records `reading … -68562.4 better than silence per
/// hop against a gate of 1` beside `inThis 69 characters emitted`.</para>
/// <para>**THE FOUR CANDIDATES THE ORDER NAMES**: the gate is applied to a
/// different quantity than the one printed; it is applied per window but
/// characters settle from a window that already passed; the score is computed
/// after emission; or the streaming path does not consult it at all. **This
/// measures which**, by streaming the capture through the real decoder and
/// recording every window's ratio beside the characters that settled out of
/// it.</para>
/// <para>Nothing here changes behaviour. It is the trace.</para>
/// </remarks>
public sealed class WhyTheGateDidNotFireTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhyTheGateDidNotFireTests(ITestOutputHelper output)
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

    /// <summary>Tonight's four phantom captures and three good ones.</summary>
    private static (string Name, string What)[] Tonight { get; } =
    {
        ("cw-2026-08-28-004844", "GOOD - reads TUES AUG 25"),
        ("cw-2026-08-28-004902", "GOOD"),
        ("cw-2026-08-28-004915", "GOOD"),
        ("cw-2026-08-28-005051", "PHANTOM - 252 characters, unkeyed YES"),
        ("cw-2026-08-28-005158", "PHANTOM - 69 characters at -68562.4"),
        ("cw-2026-08-28-005218", "PHANTOM"),
        ("cw-2026-08-28-005243", "PHANTOM"),
    };

    /// <remarks>
    /// <para>**THE TRACE.** Each capture is streamed through a real
    /// <see cref="CwDecoder"/>, and every settled character is recorded with the
    /// window ratio that was standing when it settled.</para>
    /// <para>**IF ANY CHARACTER SETTLES OUT OF A WINDOW BELOW THE GATE, THE GATE
    /// IS NOT REACHING THE EMIT DECISION.** If none does, the gate is working and
    /// the sheet's single figure is a snapshot of the last window rather than a
    /// summary of the recording, which is a different fault with a different
    /// fix.</para>
    /// </remarks>
    [Fact]
    public void WhereTheCharactersCameFrom()
    {
        _output.WriteLine(
            $"  gate {CwProbabilisticDecoder.Gate:0.00} per hop");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                | chars | under gate | lowest ratio | "
            + "final ratio | what");
        _output.WriteLine(
            "  -----------------------|-------|------------|--------------|"
            + "-------------|-----");

        var offenders = new List<string>();

        foreach (var (name, what) in Tonight)
        {
            var audio = Capture(name);
            var decoder = new CwDecoder(audio.SampleRate, 600);

            var settled = 0;
            var underGate = 0;
            var lowest = double.PositiveInfinity;

            decoder.CharacterDecoded += _ =>
            {
                settled++;

                var ratio = decoder.Reading.LikelihoodRatio;

                lowest = Math.Min(lowest, ratio);

                if (ratio < CwProbabilisticDecoder.Gate)
                {
                    underGate++;
                }
            };

            using (var source = new BufferedAudioSource(audio))
            {
                decoder.Listen(source);
                source.PumpAll();
            }

            decoder.Flush();

            if (underGate > 0)
            {
                offenders.Add($"{name} ({underGate} of {settled})");
            }

            _output.WriteLine(
                $"  {name,-22} | {settled,5} | {underGate,10} | "
                + $"{(double.IsPositiveInfinity(lowest) ? 0 : lowest),12:0.00} | "
                + $"{decoder.Reading.LikelihoodRatio,11:0.00} | {what}");
        }

        _output.WriteLine("");

        if (offenders.Count > 0)
        {
            _output.WriteLine(
                "  characters settled while the standing window was below the "
                + "gate: " + string.Join(", ", offenders));
        }
        else
        {
            _output.WriteLine(
                "  no character settled while the standing window was below the "
                + "gate");
        }

        // **NOTHING IS ASSERTED ABOUT THE OUTCOME.** This is the trace that
        // decides task 2's shape, and a test demanding either answer would be
        // deciding it in advance.
        Assert.Equal(7, Tonight.Length);
    }
}
