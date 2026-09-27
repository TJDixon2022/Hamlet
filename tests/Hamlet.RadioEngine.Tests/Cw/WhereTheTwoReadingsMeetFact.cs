using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Where the two decoders' readings meet on one clock, over every keyed
/// recording and every synthetic case (work instruction 465, task 1; HM-REQ-120,
/// 125, 126, 127; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** Nothing under
/// `src` changes for it.</para>
/// <para>**BOTH DECODERS COME FROM ONE HARNESS** (V-11):
/// <see cref="BothDecodersAreScoredAlikeTests.Rows"/>, and the port's p from
/// <see cref="FldigiConfidence.For(Hamlet.RadioEngine.Cw.Second.FldigiCwDecoder)"/>
/// on <see cref="WhatEachDecoderKnowsAboutEachCharacterFact.RunPort"/>'s second
/// run, checked there to print exactly what the harness's run printed.</para>
/// </remarks>
public sealed class WhereTheTwoReadingsMeetFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public WhereTheTwoReadingsMeetFact(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Work instruction 465 task 0, item 6: every character each decoder put out
    /// on all 35, word boundaries included, with its class and p, in order. Every
    /// byte-identical check of the unit is made against this print.
    /// </remarks>
    [Fact]
    public void EachDecodersCharactersWithClassAndP()
    {
        foreach (var r in BothDecodersAreScoredAlikeTests.Rows)
        {
            for (var i = 0; i < r.Ours.Settled.Count; i++)
            {
                var c = r.Ours.Settled[i];

                _output.WriteLine(string.Create(Invariant,
                    $"save | ours | {r.Name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(c.Probability)}"));
            }

            if (r.Port is null)
            {
                _output.WriteLine($"save | port | {r.Name} | not run: {r.PortNotRun}");
                continue;
            }

            var p = FldigiConfidence.For(WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(r));

            for (var i = 0; i < r.Port.Settled.Count; i++)
            {
                var c = r.Port.Settled[i];

                _output.WriteLine(string.Create(Invariant,
                    $"save | port | {r.Name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(p[i])}"));
            }
        }
    }

    /// <summary>sure, dim, placeholder or gap, as <see cref="CwSymbol"/> classes it.</summary>
    internal static string ClassOf(CwCharacter c) => c.IsWordGap ? "gap" : CwSymbol.Of(c).Class switch
    {
        CwSymbolClass.Sure => "sure",
        CwSymbolClass.NotSure => "dim",
        _ => "placeholder",
    };

    /// <summary>A word boundary printed as a word, so a line never ends in a space.</summary>
    internal static string Visible(string text) => text == MorseAlphabet.WordGap ? "(space)" : text;

    /// <summary>A p printed round-trip, or NaN.</summary>
    internal static string P(double p) => double.IsNaN(p) ? "NaN" : p.ToString("R", Invariant);
}
