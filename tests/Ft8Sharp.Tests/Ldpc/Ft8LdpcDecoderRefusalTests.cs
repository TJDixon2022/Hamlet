using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Every refusal <see cref="LdpcDecoder"/> defines, watched refusing -- and, where a refusal is
/// a bound rather than a length, by how much the legal case missed it.
/// </summary>
/// <remarks>
/// <b>A guard that has never been seen to fire is a comment.</b> Each of these is exercised
/// against the thing it is supposed to catch, and the two that are bounds rather than shapes
/// are exercised against the largest legal value as well, so the bound is shown to be where it
/// says it is rather than somewhere convenient.
/// </remarks>
public class Ft8LdpcDecoderRefusalTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcDecoderRefusalTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// A ratio array that is not exactly <see cref="Ft8Tables.LdpcN"/> long is refused, and the
    /// message says why rather than naming a parameter.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(Ft8Tables.LdpcN - 1)]
    [InlineData(Ft8Tables.LdpcN + 1)]
    [InlineData(Ft8Tables.LdpcN * 2)]
    public void ARatioArrayOfTheWrongLengthIsRefusedWithTheReason(int length)
    {
        var ratios = new float[length];
        var bits = new byte[Ft8Tables.LdpcN];

        var error = Assert.Throws<ArgumentException>(() => LdpcDecoder.Decode(ratios, bits));

        _output.WriteLine($"{length,5} ratios -> {error.Message.Split('.')[0]}.");
        Assert.Contains(Ft8Tables.LdpcN.ToString(), error.Message);
        Assert.Contains(length.ToString(), error.Message);
    }

    /// <summary>An output buffer of the wrong size is refused on its own terms.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(Ft8Tables.LdpcN - 1)]
    [InlineData(Ft8Tables.LdpcN + 1)]
    public void AnOutputBufferOfTheWrongLengthIsRefusedWithTheReason(int length)
    {
        var ratios = new float[Ft8Tables.LdpcN];
        var bits = new byte[length];

        var error = Assert.Throws<ArgumentException>(() => LdpcDecoder.Decode(ratios, bits));

        _output.WriteLine($"{length,5} output bytes -> {error.Message.Split('.')[0]}.");
        Assert.Contains(length.ToString(), error.Message);
    }

    /// <summary>
    /// A maximum iteration count of zero <b>returns cleanly rather than throwing</b>, having
    /// decoded nothing -- and one is enough for a clean codeword, so the bound is exactly where
    /// it says it is.
    /// </summary>
    /// <remarks>
    /// <b>The pair is the point.</b> Zero returning "no decode" would prove nothing on its own,
    /// because a decoder that always returned no decode would pass it. One iteration recovering
    /// a clean codeword is the other half: upstream takes its hard decision at the <em>top</em>
    /// of the loop, so the first pass judges the raw ratios with no message passed at all, and
    /// an undamaged codeword needs exactly that and no more.
    /// </remarks>
    [Fact]
    public void ZeroIterationsReturnsNothingDecodedAndOneIsEnoughForACleanCodeword()
    {
        var entry = EncodeCorpus.Build()[0];
        var ratios = SoftCodeword.RatiosFor(SoftCodeword.CodewordBitsFor(entry.Message));

        var atZero = new byte[Ft8Tables.LdpcN];
        var zero = LdpcDecoder.Decode(ratios, atZero, maxIterations: 0);

        var atOne = new byte[Ft8Tables.LdpcN];
        var one = LdpcDecoder.Decode(ratios, atOne, maxIterations: 1);

        _output.WriteLine($"maxIterations 0 : unsatisfied {zero.UnsatisfiedChecks,3}, iterations "
            + $"{zero.Iterations}, paritySatisfied {zero.ParitySatisfied}, output all zero "
            + $"{atZero.All(b => b == 0)}");
        _output.WriteLine($"maxIterations 1 : unsatisfied {one.UnsatisfiedChecks,3}, iterations "
            + $"{one.Iterations}, paritySatisfied {one.ParitySatisfied}");

        Assert.False(zero.ParitySatisfied);
        Assert.Equal(Ft8Tables.LdpcM, zero.UnsatisfiedChecks);
        Assert.Equal(0, zero.Iterations);
        Assert.All(atZero, b => Assert.Equal(0, b));

        Assert.True(one.ParitySatisfied);
        Assert.Equal(1, one.Iterations);
    }

    /// <summary>A negative maximum iteration count is a caller's arithmetic going wrong and is refused.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ANegativeIterationCountIsRefusedRatherThanTreatedAsZero(int maxIterations)
    {
        var ratios = new float[Ft8Tables.LdpcN];
        var bits = new byte[Ft8Tables.LdpcN];

        var error = Assert.Throws<ArgumentException>(
            () => LdpcDecoder.Decode(ratios, bits, maxIterations));

        _output.WriteLine($"{maxIterations,12} -> {error.Message.Split('.')[0]}.");
        Assert.Equal("maxIterations", error.ParamName);
    }

    /// <summary>
    /// An array of ratios all exactly zero -- no information at all about any bit -- returns
    /// <b>no decode</b> rather than a guess.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is the refusal that matters most of the four</b>, because the wrong answer here is
    /// not an exception, it is a message. With every ratio zero the hard decision is zero
    /// everywhere, and <b>the all-zero word satisfies every parity check of any linear code</b> --
    /// so a decoder that only counted unsatisfied checks would report a perfect decode on a
    /// signal that was never there. Upstream refuses it explicitly and this port follows.
    /// </para>
    /// <para>
    /// The count is reported as <see cref="Ft8Tables.LdpcM"/> rather than 0, and the bits go back
    /// all zero, which is the honest report of having decided nothing at all.
    /// </para>
    /// </remarks>
    [Fact]
    public void AnAllZeroRatioArrayReturnsNoDecodeRatherThanTheAllZeroCodeword()
    {
        var ratios = new float[Ft8Tables.LdpcN];
        var bits = new byte[Ft8Tables.LdpcN];

        var result = LdpcDecoder.Decode(ratios, bits);

        var syndrome = LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows);
        var independentlyFailing = LdpcCheck.FailingCount(syndrome);

        _output.WriteLine($"unsatisfied checks reported            : {result.UnsatisfiedChecks}");
        _output.WriteLine($"iterations                             : {result.Iterations}");
        _output.WriteLine($"paritySatisfied                        : {result.ParitySatisfied}");
        _output.WriteLine($"bits returned, all zero                : {bits.All(b => b == 0)}");
        _output.WriteLine($"and the all-zero word's TRUE syndrome  : {independentlyFailing} checks failing");
        _output.WriteLine("  -- so the refusal is a decision and not an arithmetic accident: the word");
        _output.WriteLine("     the decoder is refusing genuinely satisfies every check.");

        Assert.False(result.ParitySatisfied);
        Assert.Equal(Ft8Tables.LdpcM, result.UnsatisfiedChecks);
        Assert.Equal(0, independentlyFailing);
        Assert.All(bits, b => Assert.Equal(0, b));
    }

    /// <summary>
    /// Ratios that are all negative -- every bit confidently zero -- reach the same all-zero
    /// word and are refused the same way.
    /// </summary>
    [Fact]
    public void RatiosSayingEveryBitIsZeroAreRefusedForTheSameReason()
    {
        var ratios = new float[Ft8Tables.LdpcN];
        Array.Fill(ratios, -SoftCodeword.ConfidentMagnitude);
        var bits = new byte[Ft8Tables.LdpcN];

        var result = LdpcDecoder.Decode(ratios, bits);

        _output.WriteLine($"every bit confidently 0 -> unsatisfied {result.UnsatisfiedChecks}, "
            + $"iterations {result.Iterations}, all-zero output {bits.All(b => b == 0)}");

        Assert.False(result.ParitySatisfied);
        Assert.Equal(Ft8Tables.LdpcM, result.UnsatisfiedChecks);
    }

    /// <summary>
    /// The decoder leaves the caller's ratio array exactly as it found it.
    /// </summary>
    /// <remarks>
    /// A decoder that scaled or normalised its input in place would work perfectly once and give
    /// a different answer the second time, which is exactly the failure the determinism tests
    /// next door are looking for from the other end.
    /// </remarks>
    [Fact]
    public void TheCallersRatiosComeBackUntouched()
    {
        var entry = EncodeCorpus.Build()[0];
        var ratios = SoftCodeword.RatiosFor(SoftCodeword.CodewordBitsFor(entry.Message));
        var before = (float[])ratios.Clone();

        LdpcDecoder.Decode(ratios, new byte[Ft8Tables.LdpcN]);

        var differing = ratios.Where((value, i) => !value.Equals(before[i])).Count();
        _output.WriteLine($"ratios differing after the decode : {differing} of {ratios.Length}");
        Assert.Equal(0, differing);
    }
}
