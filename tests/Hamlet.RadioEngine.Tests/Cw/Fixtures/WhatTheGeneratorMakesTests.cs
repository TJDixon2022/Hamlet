using System.Globalization;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The trace before the set: what the generator actually renders at the two
/// corners of the grid (work instruction 414, task 1).
/// </summary>
/// <remarks>
/// <para>**MEASURED FROM THE AUDIO, NOT READ BACK FROM THE RECIPE.** The gaps come
/// off the rendered key edges and the signal-to-noise off the rendered samples
/// (<see cref="RenderedKeying"/>); the recipe is printed beside them only so a
/// disagreement shows. Each corner is measured twice: directly, and taken apart
/// into its tone and its noise, which is the only way to find the edges at the
/// weak end, where the envelope finds the noise's as well. A rendering that disagrees with its recipe is a generator
/// defect, HM-DEC-101's pattern, and the set is not built on it.</para>
/// <para>A printer. It asserts nothing, and above all no edit count: a number
/// nobody chose does not become a floor in the unit that first measures it.</para>
/// </remarks>
public sealed class WhatTheGeneratorMakesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhatTheGeneratorMakesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The corners: slowest at strongest, fastest at weakest.</summary>
    public static TheoryData<string> Corners { get; } = new()
    {
        "cq-12wpm-15db",
        "cq-25wpm-0db",
    };

    /// <remarks>
    /// Prints, per corner, the gaps as rendered by class with the recipe's value
    /// and the value the five-millisecond edges predict, the signal-to-noise as
    /// rendered against the recipe's, and the decode at HEAD against the exact
    /// key.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Corners))]
    public void TheCornerIsMeasured(string name)
    {
        var i = CultureInfo.InvariantCulture;
        var recipe = SyntheticCq.All.Single(r => r.Name == name);
        var (audio, _) = CwFixtureGenerator.Generate(recipe);
        var (noise, _) = CwFixtureGenerator.Generate(recipe with { SignalToNoiseDb = -400 });
        var direct = RenderedKeying.Measure(audio);
        var measured = RenderedKeying.MeasureApart(audio, noise);
        var dit = recipe.DitMilliseconds;
        var (marks, element, character, word) = SyntheticCq.Counts(recipe.Text);

        _output.WriteLine(string.Create(i,
            $"case | {name} | {recipe.WordsPerMinute:0} wpm | recipe dit {dit:0.###} ms, snr {recipe.SignalToNoiseDb:0.0} dB, tone {recipe.ToneHz:0} Hz, seed {recipe.Seed}"));

        foreach (var (how, m) in new[] { ("direct", direct), ("apart", measured) })
        {
            _output.WriteLine(string.Create(i,
                $"{how} pitch | measured {m.PitchHz:0} Hz | recipe {recipe.ToneHz:0} Hz drifting +/- {recipe.DriftHz:0}"));
            _output.WriteLine(string.Create(i,
                $"{how} snr | measured {m.SnrDb:0.00} dB in the passband | recipe {recipe.SignalToNoiseDb:0.0} dB | tone rms {20 * Math.Log10(m.ToneRms):0.00} dBFS, noise rms {20 * Math.Log10(m.NoiseRms):0.00} dBFS"));
            _output.WriteLine(
                $"{how} count | marks {m.Marks.Count} of {marks} | spaces {m.Spaces.Count} of {element + character + word}");
        }

        // The sequence the recipe keys, from the Morse table, so each rendered
        // run can be set against the one it should be.
        var wantMarks = new List<(double Ms, string Kind)>();
        var wantSpaces = new List<(double Ms, string Kind)>();
        var words = recipe.Text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            if (w > 0)
            {
                wantSpaces.Add((recipe.WordGapMilliseconds, "word"));
            }

            for (var c = 0; c < words[w].Length; c++)
            {
                if (c > 0)
                {
                    wantSpaces.Add((recipe.CharacterGapMilliseconds, "character"));
                }

                var pattern = MorseCode.Spell(words[w][c]) ?? "";

                for (var e = 0; e < pattern.Length; e++)
                {
                    if (e > 0)
                    {
                        wantSpaces.Add((recipe.ElementGapMilliseconds, "element"));
                    }

                    wantMarks.Add(pattern[e] == '.'
                        ? (recipe.DitMilliseconds, "dit")
                        : (recipe.DahMilliseconds, "dah"));
                }
            }
        }

        var aligned = measured.Marks.Count == wantMarks.Count
            && measured.Spaces.Count == wantSpaces.Count;

        _output.WriteLine(aligned
            ? "aligned | every rendered run stands against the run the recipe keys there"
            : "aligned | no - the run counts differ, so the classes below are by length and not by position");

        void Class(string label, IEnumerable<double> values, double recipeMs, double edgeMs)
        {
            var v = values.OrderBy(x => x).ToList();

            if (v.Count == 0)
            {
                _output.WriteLine($"{label} | none measured");
                return;
            }

            var median = v[v.Count / 2];

            _output.WriteLine(string.Create(i,
                $"{label} | n {v.Count} | median {median:0.0} ms, {median / dit:0.00} units | range {v[0]:0.0} to {v[^1]:0.0} ms | recipe {recipeMs:0.0} ms | the edges predict {edgeMs:0.0} ms | median off that {median - edgeMs:+0.0;-0.0} ms"));
        }

        if (aligned)
        {
            foreach (var kind in new[] { "dit", "dah" })
            {
                var want = wantMarks.First(m => m.Kind == kind).Ms;

                Class($"mark {kind}", measured.Marks.Where((_, k) => wantMarks[k].Kind == kind), want, want - 5);
            }

            foreach (var kind in new[] { "element", "character", "word" })
            {
                var want = wantSpaces.First(s => s.Kind == kind).Ms;

                Class($"gap {kind}", measured.Spaces.Where((_, k) => wantSpaces[k].Kind == kind), want, want + 5);
            }

            var worstMark = measured.Marks.Select((m, k) => Math.Abs(m - (wantMarks[k].Ms - 5))).Max();
            var worstGap = measured.Spaces.Select((s, k) => Math.Abs(s - (wantSpaces[k].Ms + 5))).Max();

            _output.WriteLine(string.Create(i,
                $"worst | a mark {worstMark:0.0} ms, a gap {worstGap:0.0} ms from what the recipe and the edges predict"));
        }
        else
        {
            var unit = measured.Marks.OrderBy(x => x).Take(Math.Max(1, measured.Marks.Count / 2)).Last();

            Class("mark short", measured.Marks.Where(m => m < 2 * unit), dit, dit - 5);
            Class("mark long", measured.Marks.Where(m => m >= 2 * unit), recipe.DahMilliseconds, recipe.DahMilliseconds - 5);
            Class("gap under 2 units", measured.Spaces.Where(s => s < 2 * dit), recipe.ElementGapMilliseconds, recipe.ElementGapMilliseconds + 5);
            Class("gap 2 to 5 units", measured.Spaces.Where(s => s >= 2 * dit && s < 5 * dit), recipe.CharacterGapMilliseconds, recipe.CharacterGapMilliseconds + 5);
            Class("gap 5 units up", measured.Spaces.Where(s => s >= 5 * dit), recipe.WordGapMilliseconds, recipe.WordGapMilliseconds + 5);
        }

        var reading = SyntheticCq.Read(audio);
        var whole = SyntheticCq.Whole(reading);
        var within = SyntheticCq.Within(reading);

        _output.WriteLine($"whole | {whole}");
        _output.WriteLine($"within | {within}");
        _output.WriteLine($"decode | `{reading.Text}`");
        _output.WriteLine($"key | `{SyntheticCq.Text}`");
    }
}
