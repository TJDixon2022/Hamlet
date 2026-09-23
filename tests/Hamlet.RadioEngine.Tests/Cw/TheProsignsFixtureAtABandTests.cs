using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Prints what the prosigns fixture gives back, off disk and with a band under the tone, and
/// asserts nothing.
/// </summary>
/// <remarks>
/// Work instruction 400, decision 10, the sibling of <see cref="TheCleanSyntheticsFourWaysTests"/>:
/// the measurement taken before `prosigns-18wpm` is touched. `prosigns-18wpm` was generated with no
/// noise at all, like the two clean synthetics before unit 400 gave them a band. Each row is the
/// three things `TheProsignRecordingDecodesItsProsigns` and the share theory ask of it: `<BT>` and
/// `<SK>` in the text, no confident mistake, and the share given back. Never on a carry-forward
/// line.
/// </remarks>
public sealed class TheProsignsFixtureAtABandTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Takes the output the numbers are printed to.</summary>
    /// <param name="output">The test output.</param>
    public TheProsignsFixtureAtABandTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>Off disk as the fixture tests read it, and in memory at a band of 0.02.</summary>
    [Fact]
    public void OffDiskAndAtABandOf002()
    {
        var fixture = CwFixtures.All.Single(f => f.Name == "prosigns-18wpm");
        Print(fixture, "disk", CwFixtures.Read(fixture));
        Print(fixture, "0.02", CwSignal.Generate(fixture.Request with { NoiseAmplitude = 0.02 }));
    }

    private void Print(CwFixture fixture, string band, MonoAudio audio)
    {
        var result = CwDecodeHarness.Decode(audio);
        var mistakes = CwAlignment.ConfidentMistakes(result.Characters, fixture.Sent);
        var wanted = CwAlignment.SymbolCount(fixture.Sent);
        var got = result.Letters.Count(c => !c.IsUnreadable);
        var high = result.Letters.Count(c => c.Confidence == CwConfidence.High);

        _output.WriteLine($"ROW | {fixture.Name} | {band} | [{result.Text}] | "
            + $"BT {result.Text.Contains("<BT>", StringComparison.Ordinal)} | "
            + $"SK {result.Text.Contains("<SK>", StringComparison.Ordinal)} | "
            + $"mistakes {mistakes.Count} | share {got} of {wanted} | high {high} of {result.Letters.Count} | "
            + $"{result.WordsPerMinute} wpm");
    }
}
