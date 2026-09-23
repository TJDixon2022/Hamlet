using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: the six `CwDisplacementFloorTests` cases decoded the
/// ways unit 401 measured them, and the speed properties behind
/// `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom`.
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NOTHING** and is on no carry-forward line. Way 1 is the
/// displacement type as it stood at unit 401's entry, the leading edge
/// (`CharacterDecoded`) at the noise the test gives; way 2 the settled
/// transcript (`CharacterSettled`) at the same noise; ways 3 to 6 the settled
/// transcript at bands of 0.002, 0.005, 0.01 and 0.02 (R47, HM-DEC-091,
/// HM-DEC-127).</para>
/// </remarks>
public sealed class TheDisplacementFloorFourWaysTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheDisplacementFloorFourWaysTests(ITestOutputHelper output) => _output = output;

    private const string Message = "VVV VVV VVV CQ DE W1AW K";

    private static readonly (string Name, double Tone, double Start, double Noise)[] Cases =
    [
        ("image", 400, 600, 0),
        ("elsewhere-400", 400, 600, 0),
        ("elsewhere-500", 500, 600, 0),
        ("elsewhere-750", 750, 600, 0),
        ("elsewhere-875", 875, 600, 0),
        ("refused-before", 400, 600, 0.06),
    ];

    private static (int Moves, string Text) Decode(double toneHz, double startHz, double noise, bool settled)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            Message, WordsPerMinute: 18, ToneHz: toneHz, NoiseAmplitude: noise));

        var decoder = new CwDecoder(audio.SampleRate, startHz);
        var read = new System.Text.StringBuilder();

        if (settled)
        {
            decoder.CharacterSettled += c => read.Append(c.Text);
        }
        else
        {
            decoder.CharacterDecoded += c => read.Append(c.Text);
        }

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return (decoder.Tracker.Retunes, read.ToString().Trim());
    }

    private void Row(string name, double tone, double start, int way, double band, bool settled)
    {
        var run = Decode(tone, start, band, settled);

        var ends = run.Text.EndsWith("CQ DE W1AW K", StringComparison.Ordinal);
        var image = name == "image" ? $" retunes-is-1 {run.Moves == 1}" : string.Empty;

        _output.WriteLine(
            $"ROW | {name} | way {way} | band {band} | retunes {run.Moves} | ends {ends}{image} | '{run.Text}'");
    }

    /// <summary>Prints every row. Asserts nothing.</summary>
    [Fact]
    public void PrintTheRows()
    {
        foreach (var c in Cases)
        {
            Row(c.Name, c.Tone, c.Start, 1, c.Noise, settled: false);
            Row(c.Name, c.Tone, c.Start, 2, c.Noise, settled: true);

            var bands = c.Noise == 0 ? new[] { 0.002, 0.005, 0.01, 0.02 } : new[] { 0.02 };
            var way = 3;

            foreach (var band in bands)
            {
                Row(c.Name, c.Tone, c.Start, c.Noise == 0 ? way++ : 6, band, settled: true);
            }
        }

        // #24: the request CwEmissionGateTests builds for its real signal.
        var real = CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW K", WordsPerMinute: 18, ToneHz: 620,
            Amplitude: 0.5, NoiseAmplitude: 0.02, Seed: 5));

        var decoder = new CwDecoder(real.SampleRate, 600);
        var settledText = new System.Text.StringBuilder();
        decoder.CharacterSettled += ch => settledText.Append(ch.Text);

        using (var source = new BufferedAudioSource(real))
        {
            decoder.Listen(source);
            source.PumpAll();
            decoder.Flush();
        }

        _output.WriteLine(
            $"ROW24 | characters {decoder.Report.CharactersEmitted} | reading-wpm {decoder.Reading.WordsPerMinute:0.00} "
            + $"| reading-text '{decoder.Reading.Text}' | wpm {decoder.WordsPerMinute?.ToString() ?? "null"} "
            + $"| reacquiring {decoder.SpeedIsReacquiring} | settled '{settledText.ToString().Trim()}'");
    }
}
