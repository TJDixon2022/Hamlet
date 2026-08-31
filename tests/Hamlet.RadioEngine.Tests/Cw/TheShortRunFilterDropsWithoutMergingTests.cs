using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How long a dropout has to be before it breaks a mark in two.
/// </summary>
/// <remarks>
/// <para>**TASK 1 ASKS WHETHER ANYTHING ALREADY BRIDGES A DROPOUT, AND TWO THINGS
/// DO** (work instruction 054). `CwUnitEstimator.Runs` at
/// `CwUnitEstimator.cs:543` runs a Schmitt trigger of ±6 dB about an Otsu cut, and
/// refuses to record any run shorter than `ShortestRunHops`, which is two hops of
/// five milliseconds. **Task 3 changes what is there rather than adding a
/// third.**</para>
/// <para>**THE FIRST VERSION OF THIS TEST ASSERTED THE WRONG THING, AND IT IS
/// RECORDED RATHER THAN DELETED.** It claimed a one-hop notch splits a mark, on
/// the reasoning that `Runs` drops a short run without merging the two it
/// separated — which it does do, in the code. **Measured, a one-hop notch never
/// reaches that line**: the hysteresis absorbs it first, and clean and notched
/// both read one mark of 220 ms. The drop-without-merge is real and, at one hop,
/// unreachable.</para>
/// <para>**SO THE USEFUL QUESTION IS WHERE THE HYSTERESIS STOPS ABSORBING**, and
/// that is what this measures: the notch length at which one mark becomes two.
/// That number sizes a hold-over, and it is measured here rather than taken from
/// the ripple figures.</para>
/// </remarks>
public sealed class TheShortRunFilterDropsWithoutMergingTests
{
    private readonly ITestOutputHelper _output;

    public TheShortRunFilterDropsWithoutMergingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>How long a dropout has to be before the mark comes apart.</summary>
    /// <remarks>
    /// **THE DROPOUT GOES TO THE NOISE FLOOR, WHICH IS THE DEEP CASE.** A shallow
    /// ripple is what the ±6 dB hysteresis was built for and it handles it; the
    /// operator's audio shows 49 to 61 per cent peak to peak, which is about six
    /// decibels, so the shallow case is already covered and the deep one is the
    /// question.
    /// </remarks>
    [Fact]
    public void TheNotchLengthAtWhichAMarkComesApart()
    {
        var clean = CwUnitEstimator.Elements(
            Envelope(40, -1, 0), CwProbabilisticDecoder.HopMilliseconds);

        _output.WriteLine(
            $"clean: {clean.Marks.Count} mark(s), {clean.Marks[0]:0} ms");
        _output.WriteLine("");
        _output.WriteLine("notchHops  notchMs  marks  lengths");

        int? splitsAt = null;

        foreach (var hops in new[] { 1, 2, 3, 4, 6, 8, 10, 12 })
        {
            var (marks, _) = CwUnitEstimator.Elements(
                Envelope(40, 20, hops), CwProbabilisticDecoder.HopMilliseconds);

            _output.WriteLine(
                $"{hops}  {hops * CwProbabilisticDecoder.HopMilliseconds:0}  "
                + $"{marks.Count}  "
                + string.Join(" + ", marks.Select(m => $"{m:0}")));

            if (marks.Count > 1 && splitsAt is null)
            {
                splitsAt = hops;
            }
        }

        _output.WriteLine("");
        _output.WriteLine(splitsAt is { } at
            ? $"a mark comes apart at {at} hops, which is "
              + $"{at * CwProbabilisticDecoder.HopMilliseconds:0} ms"
            : "no notch up to 60 ms split the mark");

        Assert.Single(clean.Marks);
    }

    /// <summary>
    /// What already bridges a dropout, so task 3 changes it rather than adding.
    /// </summary>
    /// <remarks>
    /// Two mechanisms, both in `CwUnitEstimator.Runs`: a Schmitt trigger of ±6 dB
    /// about an Otsu cut, and a refusal to record any run under two hops. The
    /// numbers are asserted rather than remembered, because task 3 is about to
    /// change one of them.
    /// </remarks>
    [Fact]
    public void WhatAlreadyBridgesADropout()
    {
        _output.WriteLine(
            $"Schmitt hysteresis: {CwUnitEstimator.HysteresisDb:0} dB "
            + "either side of an Otsu cut");
        _output.WriteLine(
            "shortest recorded run: 2 hops = "
            + $"{2 * CwProbabilisticDecoder.HopMilliseconds:0} ms");
        _output.WriteLine(
            $"detector hop: {CwProbabilisticDecoder.HopMilliseconds:0} ms, "
            + $"integrator {CwProbabilisticDecoder.IntegratorBandwidthHz:0} Hz");

        Assert.Equal(6.0, CwUnitEstimator.HysteresisDb);
        Assert.Equal(5.0, CwProbabilisticDecoder.HopMilliseconds);
        Assert.Equal(45.0, CwProbabilisticDecoder.IntegratorBandwidthHz);
    }

    /// <summary>A mark of a stated length, with a notch of a stated length in it.</summary>
    /// <remarks>
    /// **THE NOISE IS NOT DECORATION.** A two-level envelope gives Otsu a plateau —
    /// every split between the two levels scores identically — and the cut then
    /// lands at the top of the empty range, above the mark, so nothing is ever
    /// key-down. An earlier version of this measured nought marks on a clean dah
    /// for exactly that reason. A little spread gives the two classes a shape,
    /// which is what real audio has.
    /// </remarks>
    private static double[] Envelope(int markHops, int notchAt, int notchHops)
    {
        var random = new Random(20260830);
        var envelope = new double[200];

        for (var i = 0; i < envelope.Length; i++)
        {
            envelope[i] = 0.01 * (0.5 + random.NextDouble());
        }

        for (var i = 60; i < 60 + markHops; i++)
        {
            envelope[i] = 1.0 * (0.9 + (random.NextDouble() * 0.2));
        }

        for (var i = 0; notchAt >= 0 && i < notchHops; i++)
        {
            envelope[60 + notchAt + i] = 0.01 * (0.5 + random.NextDouble());
        }

        return envelope;
    }
}
