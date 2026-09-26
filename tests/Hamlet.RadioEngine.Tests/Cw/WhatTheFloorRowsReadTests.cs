using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every capture row and named floor, with the count it was banked at, the count
/// it reads now and the text it reads (work instruction 442, tasks 1 and 2;
/// PHASE_PLAN.md 2.5).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** R78 has a row's character count
/// falling reported rather than rejected, so a row is re-banked with what it read
/// before and after printed beside it for the owner to read.</para>
/// <para>**A DIM LETTER IS PRINTED IN BRACKETS**, `(E)`, so what the operator
/// reads dimmed can be told from what he reads as sure. A placeholder is `■` and
/// a word gap a space, as the terminal draws them.</para>
/// </remarks>
public sealed class WhatTheFloorRowsReadTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public WhatTheFloorRowsReadTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What settled, as the operator reads it, dim letters in brackets.</summary>
    /// <param name="settled">What settled.</param>
    /// <returns>The text.</returns>
    internal static string Text(IEnumerable<CwCharacter> settled)
        => string.Concat(settled.Select(c =>
            c.IsWordGap || c.IsUnreadable || c.Confidence == CwConfidence.High ? c.Text : $"({c.Text})"));

    /// <remarks>
    /// Proves nothing about the decoder. For every capture row and every named
    /// floor: the row, its banked named count, its named count at or above the
    /// span bar now, its elements banked and now, and the whole text.
    /// </remarks>
    [Fact]
    public void EachRowWithItsBankedCountItsCountNowAndItsText()
    {
        _output.WriteLine("floor | table | recording | banked named | named now | banked elements | elements now | text");

        foreach (var row in TheCapturesThatDecodeKeepDecodingTests.Floors)
        {
            var name = (string)row[0];
            var settled = TheSeventeenThirtySevenCaptureTests.Settle(name);
            var counted = settled.Where(TheCapturesThatDecodeKeepDecodingTests.Counts).ToList();

            _output.WriteLine(
                $"floor | capture | {name} | {row[1]} | {counted.Count} | {row[2]} | "
                + $"{counted.Sum(c => Math.Max(1, c.Pattern.Length))} | `{Text(settled)}`");
        }

        foreach (var row in TheNumberCannotBeGamedTests.NamedFloors)
        {
            var name = (string)row[0];
            var settled = TheSeventeenThirtySevenCaptureTests.Settle(name);
            var counted = settled.Where(TheCapturesThatDecodeKeepDecodingTests.Counts).ToList();

            _output.WriteLine(
                $"floor | named | {name} | {row[1]} | {counted.Count} | - | "
                + $"{counted.Sum(c => Math.Max(1, c.Pattern.Length))} | `{Text(settled)}`");
        }
    }
}
