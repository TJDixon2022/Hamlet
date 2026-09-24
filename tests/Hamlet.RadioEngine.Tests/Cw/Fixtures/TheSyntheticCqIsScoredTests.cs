using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Every synthetic CQ call scored at HEAD against its exact key (work instruction
/// 414, task 2; PHASE_PLAN.md 1.1).
/// </summary>
/// <remarks>
/// <para>**THE KEY IS EXACT BY CONSTRUCTION** (R61): the generator knows what it
/// sent. The scored region is the whole decode against the whole key
/// (<see cref="CwScorer.Whole"/>), so whatever the decoder prints from the noise
/// before the first `C` is its own error; <see cref="CwScorer.Within"/> is printed
/// beside it for comparison.</para>
/// <para>**A PRINTER. IT ASSERTS NO EDIT COUNT.** A number nobody chose does not
/// become a floor in the unit that first measures it. It asserts only that each
/// row's kinds add up to its edits.</para>
/// <para>**OUTSIDE EVERY TOTAL**, including the one 3.2 judges (P7), and never the
/// sole evidence for keeping a change (1.4).</para>
/// </remarks>
public sealed class TheSyntheticCqIsScoredTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheSyntheticCqIsScoredTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves 1.1: each committed case read from its WAV through `CwDecoder` hop by
    /// hop from 600 Hz, scored in three parts with unsure per named beside it.
    /// </remarks>
    [Fact]
    public void EachCaseIsScoredAgainstItsExactKey()
    {
        var i = CultureInfo.InvariantCulture;

        _output.WriteLine(
            "row | case | wpm | char gap units | snr dB | edits | scored length | key | unsure per named | within edits | wrong | missing | added | space added | space missing | decode");

        var grid = new List<CwScore>();
        var wide = new List<CwScore>();

        foreach (var recipe in SyntheticCq.All)
        {
            var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));
            var reading = SyntheticCq.Read(audio);
            var whole = SyntheticCq.Whole(reading);
            var within = SyntheticCq.Within(reading);
            var kinds = CwScorer.Kinds(whole);
            var units = recipe.CharacterGapMilliseconds / recipe.DitMilliseconds;

            _output.WriteLine(string.Create(i,
                $"row | {recipe.Name} | {recipe.WordsPerMinute:0} | {units:0} | {recipe.SignalToNoiseDb:0} | "
                + $"{whole.Edits} | {whole.ScoredLength} | exact | {whole.Guard} | {within.Edits} | "
                + $"{kinds.Wrong} | {kinds.Missing} | {kinds.Added} | {kinds.SpaceAdded} | {kinds.SpaceMissing} | "
                + $"`{whole.Region}`"));

            (units == 3 ? grid : wide).Add(whole);

            Assert.Equal(whole.Edits, kinds.Edits);
        }

        Total("grid", grid);
        Total("wide row", wide);
    }

    private void Total(string label, IReadOnlyCollection<CwScore> scores)
    {
        if (scores.Count == 0)
        {
            return;
        }

        var unsure = scores.Sum(s => s.Unsure);
        var named = scores.Sum(s => s.Named);

        _output.WriteLine(
            $"total | {label} | {scores.Sum(s => s.Edits)} edits over {scores.Sum(s => s.ScoredLength)} "
            + $"characters against exact keys, {scores.Count} cases, {unsure} unsure per {named} named, "
            + "outside every total 3.2 judges");
    }
}
