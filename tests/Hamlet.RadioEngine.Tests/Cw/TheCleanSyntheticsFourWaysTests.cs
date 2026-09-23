using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Prints what the two clean synthetics give back, off disk and with a band under the tone, and
/// asserts nothing.
/// </summary>
/// <remarks>
/// <para>Work instruction 399, decision 2, the record of the measurement taken before the two
/// fixtures were touched. `clean-12wpm` and `clean-18wpm` were generated with no noise at all, so
/// the gaps between elements are exact digital silence, which the decoder refuses by design
/// (HM-OPEN-018). Way 1 decodes each file off disk exactly as
/// `CwFixtureTests.TheCleanRecordingsDecodeExactly` does. Ways 2 and 3 decode the same request
/// in memory with `NoiseAmplitude` at 0.02, the band `fading-18wpm` already carries, and at 0.01
/// and 0.04 to bracket it. Way 4 is the files off disk against a decoder changed in the working
/// tree and never committed, so it is run through the floor test and not here.</para>
/// <para>`TEXT`, the speed and the counts come from `CwDecodeHarness.Decode`, the floor test's own
/// path. `SETTLED` is `CwDecoder.CharacterSettled` from a second pass over the same audio the same
/// way. `EXACT` is the floor test's three assertions: the text is the sent text, the speed is
/// within one, every letter is high. Never on a carry-forward line.</para>
/// </remarks>
public sealed class TheCleanSyntheticsFourWaysTests
{
    private static readonly string[] Names = { "clean-12wpm", "clean-18wpm" };

    private readonly ITestOutputHelper _output;

    /// <summary>Takes the output the numbers are printed to.</summary>
    /// <param name="output">The test output.</param>
    public TheCleanSyntheticsFourWaysTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>Way 1: each file off disk, as the floor test reads it.</summary>
    [Fact]
    public void WayOneOffDisk()
    {
        foreach (var name in Names)
        {
            var fixture = CwFixtures.All.Single(f => f.Name == name);
            Print("way 1", fixture, "disk", CwFixtures.Read(fixture));
        }
    }

    /// <summary>Way 2: the same request with the band `fading-18wpm` carries, in memory.</summary>
    [Fact]
    public void WayTwoWithABandOf002()
        => InMemory("way 2", 0.02);

    /// <summary>Way 3: the same request at 0.01 and 0.04, so the band is bracketed.</summary>
    [Fact]
    public void WayThreeWithBandsOf001And004()
    {
        InMemory("way 3", 0.01);
        InMemory("way 3", 0.04);
    }

    private void InMemory(string way, double band)
    {
        foreach (var name in Names)
        {
            var fixture = CwFixtures.All.Single(f => f.Name == name);
            var request = fixture.Request with { NoiseAmplitude = band };
            Print(way, fixture, band.ToString("0.00"), CwSignal.Generate(request));
        }
    }

    private void Print(string way, CwFixture fixture, string band, MonoAudio audio)
    {
        var result = CwDecodeHarness.Decode(audio);
        var settled = Settled(audio);

        var letters = result.Letters;
        var high = letters.Count(c => c.Confidence == CwConfidence.High);
        var low = letters.Count(c => c.Confidence == CwConfidence.Low);
        var unreadable = letters.Count(c => c.IsUnreadable);
        var exact = result.Text == fixture.Sent
            && Math.Abs(result.WordsPerMinute - fixture.WordsPerMinute) <= 1
            && high == letters.Count;

        _output.WriteLine($"ROW | {way} | {fixture.Name} | {band} | [{result.Text}] | [{settled}] | "
            + $"{result.WordsPerMinute} | {letters.Count} | {high} | {low} | {unreadable} | "
            + $"{result.Report.SnrDb:0.0} | {result.Report.ToneHz:0} | "
            + $"EXACT {(exact ? "yes" : "no")}");
    }

    private static string Settled(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, CwSignal.DefaultToneHz);
        var settled = new StringBuilder();
        decoder.CharacterSettled += c => settled.Append(c.Text);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return settled.ToString().Trim();
    }
}
