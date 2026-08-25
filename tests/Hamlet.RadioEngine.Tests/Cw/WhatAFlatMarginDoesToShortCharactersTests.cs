using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What a flat character margin does to short characters, and whether
/// normalising fixes it.
/// </summary>
/// <remarks>
/// <para>**THE GATE IS RUNNING AND `■` IS IT WORKING.** A field report read
/// `■:-93.4` off a capture sheet as a character that had passed a margin ruled
/// at nought. It is the opposite: `■` is the unresolved placeholder, so that
/// line is a character the gate caught and marked, which is exactly what
/// HM-DEC-048's third confidence state is for.</para>
/// <para>**AND THE BIAS IS REAL BUT ONLY ABOVE NOUGHT.** An `E` is one dit and
/// can never accumulate the evidence a five-element digit does, so a flat
/// *positive* threshold structurally punishes the shortest characters. At the
/// ruled margin of nought there is nothing to punish: a character is marked when
/// silence explains its span better than the letter does, which is a statement
/// about that character rather than about its length.</para>
/// <para>**NEITHER NORMALISATION MAKES ONE-ELEMENT CHARACTERS COMPARABLE**, which
/// is the finding. Per keying unit is the better of the two — a dah is three dits
/// and carries three dits' evidence — but both leave `E` and `T` about a tenth of
/// everything else. If a positive margin is ever ruled, neither divisor on its
/// own is enough.</para>
/// <para>Measurement only. Nothing here asserts a figure, because the constant
/// it would assert against is Tim's (§12.1).</para>
/// </remarks>
public sealed class WhatAFlatMarginDoesToShortCharactersTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the distributions are printed.</param>
    public WhatAFlatMarginDoesToShortCharactersTests(ITestOutputHelper output)
        => _output = output;

    private static IEnumerable<(string Name, double Tone)> Corpus()
    {
        yield return ("cw-2026-08-17-013347", 600);
        yield return ("cw-2026-08-17-013622", 600);
        yield return ("cw-2026-08-17-134712", 600);
        yield return ("cw-2026-08-18-004507", 501);
        yield return ("unadjudicated/cw-2026-08-18-003016", 669);
        yield return ("unadjudicated/cw-2026-08-18-003126", 675);
        yield return ("unadjudicated/cw-2026-08-18-003758", 501);
        yield return ("unadjudicated/cw-2026-08-22-031905", 499.79);
        yield return ("unadjudicated/cw-2026-08-24-012403", 440.09);
        yield return ("unadjudicated/cw-2026-08-20-014854", 600);
        yield return ("unadjudicated/cw-2026-08-20-014935", 825);
    }

    /// <remarks>
    /// Prints the distributions the decision would be made from. Read ungated so
    /// characters the window guard refused are counted too, because a margin has
    /// to be judged on everything the path spelled rather than on what survived
    /// a different gate.
    /// </remarks>
    [Fact]
    public void WhatTheMarginDoesAndWhatNormalisingWouldDo()
    {
        // Element count -> raw span LLRs, and keying-time-normalised ones.
        var byElements = new Dictionary<int, List<double>>();
        var perElement = new List<(int Elements, double Value, bool Marked)>();
        var perUnit = new List<(int Elements, double Value, bool Marked)>();

        var marked = 0;
        var total = 0;

        foreach (var (name, tone) in Corpus())
        {
            var audio = WavAudio.Read(
                Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

            var env = CwProbabilisticDecoder.Envelope(
                audio.Samples, audio.SampleRate, tone);

            // Ungated, so characters the window guard refused are visible too.
            var r = CwProbabilisticDecoder.DecodeForMeasurement(
                env, tone, ungated: true, CwProbabilisticDecoder.NoiseSpanSeconds);

            foreach (var c in r.Characters)
            {
                if (c.Pattern.Length == 0)
                {
                    continue;
                }

                total++;

                var isMarked = c.SpanLogLikelihoodRatio < CwProbabilisticDecoder.CharacterMargin;

                if (isMarked)
                {
                    marked++;
                }

                var elements = c.Pattern.Length;

                // A dah is three dits, so keying time is dits, not elements.
                var units = c.Pattern.Sum(ch => ch == '-' ? 3 : 1);

                if (!byElements.TryGetValue(elements, out var list))
                {
                    byElements[elements] = list = new List<double>();
                }

                list.Add(c.SpanLogLikelihoodRatio);

                perElement.Add((elements, c.SpanLogLikelihoodRatio / elements, isMarked));
                perUnit.Add((elements, c.SpanLogLikelihoodRatio / units, isMarked));
            }
        }

        _output.WriteLine(
            $"{total} characters across the corpus, {marked} below the ruled "
            + $"margin of {CwProbabilisticDecoder.CharacterMargin:0} "
            + $"({100.0 * marked / Math.Max(total, 1):0.0} %)");
        _output.WriteLine("");
        _output.WriteLine("median raw span LLR by element count:");

        foreach (var k in byElements.Keys.OrderBy(x => x))
        {
            var v = byElements[k].OrderBy(x => x).ToArray();

            _output.WriteLine(
                $"  {k} element{(k == 1 ? "" : "s"),-2} n={v.Length,4}  "
                + $"median {v[v.Length / 2],12:0.0}");
        }

        _output.WriteLine("");
        _output.WriteLine("after normalising, median by element count:");

        foreach (var k in byElements.Keys.OrderBy(x => x))
        {
            var e = perElement.Where(x => x.Elements == k)
                .Select(x => x.Value).OrderBy(x => x).ToArray();

            var u = perUnit.Where(x => x.Elements == k)
                .Select(x => x.Value).OrderBy(x => x).ToArray();

            _output.WriteLine(
                $"  {k} element{(k == 1 ? "" : "s"),-2}  per element "
                + $"{e[e.Length / 2],12:0.0}   per keying unit {u[u.Length / 2],12:0.0}");
        }

        // Which normalisation flattens the spread across element counts best?
        // A flat gate is fair only if the medians are comparable.
        double Spread(IEnumerable<(int Elements, double Value, bool Marked)> set)
        {
            var medians = set.GroupBy(x => x.Elements)
                .Where(g => g.Count() >= 5)
                .Select(g =>
                {
                    var v = g.Select(x => x.Value).OrderBy(x => x).ToArray();

                    return v[v.Length / 2];
                })
                .ToArray();

            return medians.Length < 2
                ? double.NaN
                : medians.Max() / Math.Max(medians.Min(), 1e-9);
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"spread of medians, highest over lowest: raw "
            + $"{Spread(byElements.SelectMany(kv => kv.Value.Select(v => (kv.Key, v, false)))):0.0}, "
            + $"per element {Spread(perElement):0.0}, "
            + $"per keying unit {Spread(perUnit):0.0}");
    }
}
