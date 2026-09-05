using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Given the same audio, <c>Ft8Sharp.Deep</c> returns exactly what <c>Ft8Sharp</c> returns.</b>
/// <c>PHASE_PLAN.md</c> step 1's second must-pass exit, and <em>exactly</em> is the operative word.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS TRIVIALLY TRUE AND IT IS RUN ANYWAY. It is not dressed as a discovery.</b>
/// <c>Ft8DeepSlotDecoder</c> holds an <see cref="Ft8SlotDecoder"/> and hands every call to it, so of
/// course the two agree; nobody needs an experiment to believe that a delegating type delegates. The
/// step's own point is that <em>a step that changes no behaviour is the point</em>. What the run
/// proves, which reasoning does not, is that the SEAM and the HARNESS WIRING cost nothing: that a
/// whole <see cref="Ft8SlotResult"/> crosses a project boundary and an <c>Available()</c> seat with
/// no count dropped, no message reordered and no frequency or dt shifted.
/// </para>
/// <para>
/// <b>And it is emphatically not a claim that the sibling was "verified against" the port</b>, as
/// though the two had been written independently. They have not. There is one decoder here and it is
/// called twice.
/// </para>
/// <para>
/// <b>THE WHOLE RESULT, NEVER JUST <see cref="Ft8SlotResult.Texts"/>.</b> A comparison on text alone
/// passes while the five counts differ, and the counts are what steps 2, 3 and 4 will be read on.
/// <see cref="AssertIdentical"/> compares all five counts and every message's text, candidate,
/// frequency and dt, in order.
/// </para>
/// </remarks>
public class Ft8DeepIdentityTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// <b>The comparison, whole.</b> Five counts, then every message's text, candidate, frequency and
    /// dt, in order.
    /// </summary>
    private static void AssertIdentical(
        Ft8SlotResult port,
        Ft8SlotResult deep,
        Ft8WaterfallGeometry geometry,
        string what)
    {
        Assert.True(port.CandidateCount == deep.CandidateCount, $"{what}: candidate count");
        Assert.True(port.ParitySatisfiedCount == deep.ParitySatisfiedCount, $"{what}: parity satisfied");
        Assert.True(port.ChecksumPassedCount == deep.ChecksumPassedCount, $"{what}: checksum passed");
        Assert.True(port.BecameTextCount == deep.BecameTextCount, $"{what}: became text");
        Assert.True(port.DuplicateCount == deep.DuplicateCount, $"{what}: duplicate count");
        Assert.True(port.Messages.Count == deep.Messages.Count, $"{what}: message count");

        for (var i = 0; i < port.Messages.Count; i++)
        {
            var a = port.Messages[i];
            var b = deep.Messages[i];

            Assert.True(
                string.Equals(a.Text, b.Text, StringComparison.Ordinal),
                $"{what}: message {i} text, \"{a.Text}\" against \"{b.Text}\"");
            Assert.True(a.Candidate == b.Candidate, $"{what}: message {i} candidate");
            Assert.True(
                a.FrequencyHz(geometry) == b.FrequencyHz(geometry),
                $"{what}: message {i} frequency");
            Assert.True(
                a.TimeSeconds(geometry) == b.TimeSeconds(geometry),
                $"{what}: message {i} dt");
        }
    }

    /// <summary>
    /// <b>Set one of three: the ladder.</b> One whole block of 51 trials at a rung where decodes
    /// actually happen, and one whole block at -21 dB, which is the rung <c>HM-OPEN-067</c> carries.
    /// </summary>
    /// <remarks>
    /// The mixing below is <see cref="Ft8LadderHarness.Run"/>'s own inner loop, calling the same
    /// helpers with the same seed arithmetic - block <c>s</c> draws from
    /// <c>seed + s + round(rung * 10)</c>. It is written out here rather than gone through the
    /// harness because the harness returns counts and this test needs the raw
    /// <see cref="Ft8SlotResult"/> from each decoder on each trial. Nothing in the harness is changed
    /// to serve this test.
    /// </remarks>
    [Theory]
    [InlineData(-19.0)]
    [InlineData(-21.0)]
    public void OverAWholeBlockOfTheLadderTheTwoResultsAreIdentical(double rung)
    {
        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();
        var population = Ft8Step6Ladder.Population();
        var offset = Ft8LadderHarness.DefaultOffsetSamples;
        var blockSeed = Ft8LadderHarness.DefaultSeed + (int)Math.Round(rung * 10.0);
        var noise = new GaussianNoise(blockSeed);

        var clock = Stopwatch.StartNew();
        var trials = 0;
        var decodesSeen = 0;

        foreach (var entry in population)
        {
            var (clean, _) = SearchFixture.OneSignal(
                Rate, entry, Ft8LadderHarness.DefaultFrequencyHz, offset);
            var signalPower = SearchFixture.TransmissionPower(
                Rate, entry, Ft8LadderHarness.DefaultFrequencyHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

            var fromPort = port.Decode(mixed);
            var fromDeep = deep.Decode(mixed);

            AssertIdentical(fromPort, fromDeep, port.Geometry, $"{rung:F1} dB trial {trials}");

            decodesSeen += fromPort.Messages.Count;
            trials++;
        }

        clock.Stop();

        output.WriteLine(
            $"{rung:F1} dB, seed {blockSeed}, {trials} trials - one whole block of the "
            + $"{population.Count}-message population.");
        output.WriteLine(
            $"Every trial compared WHOLE: five counts and every message's text, candidate, "
            + "frequency and dt, in order.");
        output.WriteLine($"Messages returned by the port over the block: {decodesSeen}.");
        output.WriteLine($"Wall clock for both decoders over the block: {clock.Elapsed.TotalSeconds:F1} s.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "IDENTITY HERE IS TRIVIALLY TRUE - the sibling delegates to the port, so there is one");
        output.WriteLine(
            "decoder called twice. What the run proves is that the seam costs nothing, not that two");
        output.WriteLine("independently written decoders agree. They were not independently written.");

        Assert.Equal(population.Count, trials);

        // A rung where nothing decoded would compare two empty results and prove nothing about the
        // seam carrying a message across it. -19 dB is unit 221's 81 per cent rung.
        if (rung >= -19.0)
        {
            Assert.True(
                decodesSeen > 0,
                $"the port returned no messages at all over {trials} trials at {rung:F1} dB, so this "
                    + "comparison had no message to carry across the seam and is not evidence for "
                    + "exit 2. That is a finding about the decoder, not about the sibling.");
        }
    }

    /// <summary>
    /// <b>Set two of three: the committed example capture.</b>
    /// <c>tests/fixtures/ft8/example/ft8-example-244.wav</c>, unit 244's worked example.
    /// </summary>
    [Fact]
    public void OverTheCommittedExampleCaptureTheTwoResultsAreIdentical()
    {
        var fixture = Ft8CaptureFixture.Read(Ft8ExampleFixture.CommittedFixturePath);
        var contents = WavFile.Read(fixture.RequireCapture());

        var samples = new float[contents.Samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = contents.Samples[i] / 32768.0f;
        }

        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();

        var fromPort = port.Decode(samples);
        var fromDeep = deep.Decode(samples);

        AssertIdentical(fromPort, fromDeep, port.Geometry, fixture.CaptureName);

        output.WriteLine($"capture   {fixture.CaptureName} ({fixture.SampleRate} Hz, {fixture.Utc})");
        output.WriteLine($"sha256    {fixture.Sha256}");
        output.WriteLine(
            $"counts    candidates {fromPort.CandidateCount}, parity {fromPort.ParitySatisfiedCount}, "
            + $"checksum {fromPort.ChecksumPassedCount}, text {fromPort.BecameTextCount}, "
            + $"duplicates {fromPort.DuplicateCount}");
        output.WriteLine($"messages  {fromPort.Messages.Count}, identical in both columns");

        foreach (var message in fromDeep.Messages)
        {
            output.WriteLine(
                $"  \"{message.Text}\" at {message.FrequencyHz(port.Geometry):F1} Hz, "
                + $"dt {message.TimeSeconds(port.Geometry):F2} s");
        }

        Assert.NotEmpty(fromPort.Messages);
    }

    /// <summary>
    /// <b>Set three of three: every reference recording the pinned clone carries.</b> Real off-air
    /// audio from somebody else's antenna, which is the strongest instrument this phase has on the
    /// receive side.
    /// </summary>
    /// <remarks>
    /// <b>Absent is a skip and a reported count, never a failure.</b> The clone is roughly 21 MB of
    /// somebody else's recordings and never enters this repository, so a fresh clone has to stay
    /// green without it. When it is absent, exit 2 closes on the ladder and the committed capture,
    /// which is the plan's own named alternative.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void OverEveryReferenceRecordingTheTwoResultsAreIdentical()
    {
        var recordings = ReferenceRecordings.All().ToArray();

        output.WriteLine($"ReferenceRecordings.All() returned {recordings.Length} recordings from "
            + $"{ReferenceClone.Location}.");

        Assert.NotEmpty(recordings);

        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();
        var clock = Stopwatch.StartNew();
        var messages = 0;

        foreach (var recording in recordings)
        {
            var samples = recording.ReadSamples();

            var fromPort = port.Decode(samples);
            var fromDeep = deep.Decode(samples);

            AssertIdentical(fromPort, fromDeep, port.Geometry, recording.Name);
            messages += fromPort.Messages.Count;
        }

        clock.Stop();

        output.WriteLine(
            $"Every one compared WHOLE: five counts and every message's text, candidate, frequency "
            + "and dt, in order.");
        output.WriteLine($"Messages returned across all {recordings.Length} recordings: {messages}, "
            + "identical in both columns.");
        output.WriteLine($"Wall clock for both decoders over the set: {clock.Elapsed.TotalSeconds:F1} s.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "Again: trivially true, because the sibling delegates. The point is the seam, not a");
        output.WriteLine("comparison between two independent decoders.");

        Assert.True(
            messages > 0,
            "the port returned no messages at all across the reference set, so nothing crossed the "
                + "seam and this is not evidence for exit 2.");
    }
}
