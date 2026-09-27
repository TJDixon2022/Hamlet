using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-011 on one sent letter split in two: no piece of it is printed sure (work
/// instruction 472, task 2; PHASE_PLAN.md 3.2).
/// </summary>
/// <remarks>
/// <para>**A GENERATED CASE WHOSE KEY IS EXACT BY CONSTRUCTION** (CLAUDE.md 12.5):
/// `cq-18wpm-5db-char5`, the character gap unit 413 measured on the 7.052 sender, at
/// 5 dB. Its `D` of `DE` (-..) lost its middle dit in the noise, so the decoder heard
/// a dah, three units of key-up and a dit, and printed `T` then `E`, both sure, where
/// this sender's own character gap is five units (unit 472's trace).</para>
/// <para>**THE SENT LETTER'S TIME IS THE GENERATOR'S OWN**
/// (<see cref="CwFixtureGenerator.KeyEdges"/>) and the key is read by the test only,
/// never by the decoder (R72). Added against substituted is <see cref="CwMetrics"/>'s
/// alignment, the same one MET-INVENTED is counted from.</para>
/// </remarks>
public sealed class TheInventedLettersAreNotPrintedSureTests
{
    /// <summary>
    /// HM-REQ-011: over the sent `D` of `DE` in `cq-18wpm-5db-char5`, no letter the
    /// alignment counts invented, added or substituted, is printed sure.
    /// </summary>
    [Fact]
    public void ASentLetterSplitInTwoIsNotPrintedSure()
    {
        var recipe = SyntheticCq.All.Single(r => r.Name == "cq-18wpm-5db-char5");
        var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        // The `D` of `DE`: the seventh sent letter, its three marks the 19th to 21st.
        var edges = CwFixtureGenerator.KeyEdges(recipe, out _);
        var marksBefore = "CQCQCQ".Sum(c => MorseCode.Spell(c)!.Length);
        var from = edges[2 * marksBefore];
        var to = edges[(2 * (marksBefore + 3)) - 1];

        var reading = CwReading.Of(settled);
        var score = SyntheticCq.Whole(reading) with { Start = reading.Text.TakeWhile(char.IsWhiteSpace).Count() };
        var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
        var characters = covered.Where(c => !c.IsWordGap).ToList();
        var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, CwKeyKind.Exact);
        var invented = new List<string>();
        var d = 0;

        foreach (var step in alignment.Steps)
        {
            if (step.Decoded is not { } decoded)
            {
                continue;
            }

            var c = characters[d++];
            var end = c.At.TotalSeconds;
            var start = end - (Math.Max(1, c.SpanHops) * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

            if (decoded.Class == CwSymbolClass.Sure
                && !string.Equals(step.Key, decoded.Text, StringComparison.Ordinal)
                && start < to + 0.1 && end > from)
            {
                invented.Add($"`{decoded.Text}` sure at {start:0.000} to {end:0.000} s where the key has `{step.Key ?? "nothing"}`");
            }
        }

        Assert.True(invented.Count == 0,
            $"HM-REQ-011: over the sent D at {from:0.000} to {to:0.000} s, {invented.Count} invented letter(s) printed sure: {string.Join("; ", invented)}");
    }
}
