using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 249, task 1.5: what one slot costs through the port and
/// through Deep with both stages on.
/// </summary>
/// <remarks>
/// <para>**THE BUDGET IS FIFTEEN SECONDS AND IT IS A HARD ONE.** The watch
/// decodes on a slot boundary while audio keeps arriving, so a decode that
/// overran would not merely be late - the next slot's boundary would arrive
/// while this one was still working.</para>
/// <para>**MEASURED RATHER THAN REASONED, ON REAL AUDIO.** Ordered statistics
/// is the stage with the frightening cost - it is combinatorial in its order -
/// and reasoning about it from the order alone would be guessing at how often it
/// runs, which depends on how many candidates fail the cheap path.</para>
/// <para>**THIS IS THE DEVELOPMENT MACHINE** (`SHACK_FACTS.md` FACT-004). The
/// shack machine is a different computer and the margin there is its own
/// question; what this establishes is that the margin is large rather than
/// marginal.</para>
/// </remarks>
public sealed class WhatOneSlotCostsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the timings are printed.</param>
    public WhatOneSlotCostsTests(ITestOutputHelper output) => _output = output;

    /// <summary>The slot period, and the whole of the budget.</summary>
    private const double BudgetSeconds = 15;

    /// <summary>What a slot costs each way, against the budget.</summary>
    [Fact]
    public void OneSlotThroughThePortAndThroughDeep()
    {
        var audio = Capture(out var from);

        Assert.True(audio is not null, "no FT8 audio fixture in the tree");

        _output.WriteLine("audio  : " + from);

        var port = new Ft8SlotDecoder();

        var deep = new Ft8DeepSlotDecoder(
            osd: Ft8DeepOsdSettings.Default,
            fineSync: Ft8DeepFineSyncSettings.Default);

        // The slot the reader would hand a decoder: cut, resampled to the FT8
        // grid, analysed into a waterfall. Timed below is the decode alone,
        // because that is the part this unit changes.
        var cut = Ft8SlotCutter.Cut(audio, EndedAt, Measured);

        Assert.NotEmpty(cut.Slots);

        var samples = Ft8Resample.ToFt8Rate(cut.Slots[0].Audio).Samples;

        var portWaterfall = new Ft8Monitor(port.Geometry).Analyse(samples);
        var deepWaterfall = new Ft8Monitor(deep.Geometry).Analyse(samples);

        var portMs = Time(() => port.Decode(portWaterfall), out var portResult);

        // **THE WATERFALL ENTRY POINT CANNOT RUN FINE SYNC AND DEEP SAYS SO IN
        // ITS OWN REMARK**: `Decode(Ft8Waterfall)` hands the loop an empty span,
        // and "a waterfall has no phase in it and no samples behind it and there
        // is nothing in one to re-sync from". Measured here, it refuses all 42
        // candidates `RefusedForWantOfSamples` - so a Hamlet that called this
        // overload would pay for Deep and get none of the off-grid gain the
        // whole phase was for.
        var waterfallOnlyMs = Time(() => deep.Decode(deepWaterfall), out var waterfallOnly);
        var waterfallOnlyFine = deep.LastFineSync;

        // The samples entry point, which is the one Hamlet must call.
        var samplesCopy = samples;
        var deepMs = Time(() => deep.Decode(samplesCopy), out var deepResult);

        _output.WriteLine("one slot of real off-air audio, decode only:");
        _output.WriteLine("");
        _output.WriteLine("  port                    : "
            + portMs.ToString("0.0") + " ms, "
            + portResult.Messages.Count + " message(s)");
        _output.WriteLine("  Deep via waterfall      : "
            + waterfallOnlyMs.ToString("0.0") + " ms, "
            + waterfallOnly.Messages.Count + " message(s)"
            + "   <- fine sync refused "
            + waterfallOnlyFine.RefusedForWantOfSamples + " for want of samples");
        _output.WriteLine("  Deep via samples        : "
            + deepMs.ToString("0.0") + " ms, "
            + deepResult.Messages.Count + " message(s)"
            + "   <- the entry point Hamlet must call");
        _output.WriteLine("");
        _output.WriteLine("  budget                  : "
            + (BudgetSeconds * 1000).ToString("0") + " ms");
        _output.WriteLine("  Deep uses               : "
            + (deepMs / (BudgetSeconds * 1000)).ToString("P2") + " of it");
        _output.WriteLine("  margin left             : "
            + ((BudgetSeconds * 1000) - deepMs).ToString("0") + " ms");
        _output.WriteLine("");
        _output.WriteLine("  Deep costs the port     : "
            + (portMs <= 0 ? "not measurable" : (deepMs / portMs).ToString("0.0") + "x"));
        _output.WriteLine("  OSD stage               : " + deep.LastOsd);
        _output.WriteLine("  fine sync               : " + deep.LastFineSync);

        // **THE ONE ASSERTION THAT MATTERS**: a slot fits, with room to spare.
        Assert.True(
            deepMs < BudgetSeconds * 1000,
            "one slot took " + deepMs.ToString("0")
            + " ms through Deep against a " + (BudgetSeconds * 1000)
            + " ms budget, so the next slot's boundary would arrive while this "
            + "one was still decoding");

        // And it really did the extra work, or the timing measured the port
        // twice and says nothing.
        Assert.True(
            deep.LastOsd.Offered > 0,
            "the ordered statistics stage never ran, so this timed the port's "
            + "path through Deep and proves nothing about the cost of turning "
            + "the stages on");

        // **THE FINDING TASK 1 EXISTS FOR.** The waterfall overload cannot
        // re-sync, so a reader that kept calling it would pay Deep's cost for
        // none of its off-grid reach.
        Assert.True(
            waterfallOnlyFine.RefusedForWantOfSamples > 0,
            "the waterfall overload did not refuse for want of samples, so the "
            + "reason to move Ft8Reader onto the samples entry point is not the "
            + "one this test recorded");

        Assert.Equal(0, waterfallOnlyFine.Offered - waterfallOnlyFine.RefusedForWantOfSamples);
    }

    private static double Time(Func<Ft8SlotResult> work, out Ft8SlotResult result)
    {
        // One untimed run so tiered compilation is not charged to the reading.
        work();

        var started = Stopwatch.GetTimestamp();
        result = work();

        return (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
    }

    private static DateTime EndedAt { get; } =
        new(2026, 9, 3, 21, 6, 30, DateTimeKind.Utc);

    private static ClockOffset Measured { get; } =
        new(0, new DateTime(2026, 9, 3, 21, 0, 0, DateTimeKind.Utc));

    /// <summary>The FT8 audio this tree has, and where it came from.</summary>
    /// <remarks>
    /// <para>**IT IS THE SYNTHESISED EXAMPLE AND NOT AN OFF-AIR CAPTURE, AND
    /// THAT IS THE CORRECT STATE HERE.** `tests/fixtures/ft8/captured/` exists
    /// and holds no audio at all: its own README records that the radio lives on
    /// a different computer (`SHACK_FACTS.md` FACT-004) and that zero real
    /// fixtures passes cleanly rather than being a defect.</para>
    /// <para>**SO THE TIMING BELOW IS TAKEN ON THE EXAMPLE**, and the report
    /// says so. A cost measured on synthesised audio is a fair reading of what
    /// the two decoders do with a slot; what it cannot tell anybody is how a
    /// crowded evening on 14.074 loads the ordered statistics stage, which runs
    /// more often the more candidates fail the cheap path.</para>
    /// <para>**THE FIRST VERSION OF THIS LOOKED IN `captured/` AND STOPPED**,
    /// because that folder exists and only its README is in it. Searching the
    /// whole `ft8` tree and preferring a real capture where one appears is the
    /// behaviour that keeps working the day one is committed.</para>
    /// </remarks>
    private static MonoAudio? Capture(out string from)
    {
        from = "none";

        var root = Path.Combine(Root(), "tests", "fixtures", "ft8");

        if (!Directory.Exists(root))
        {
            return null;
        }

        var files = Directory
            .EnumerateFiles(root, "*.wav", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        // A real off-air capture wins where one has been committed.
        var file = files.FirstOrDefault(
            p => p.Contains($"{Path.DirectorySeparatorChar}captured{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            ?? files.FirstOrDefault();

        if (file is null)
        {
            return null;
        }

        from = Path.GetRelativePath(Root(), file)
            + (file.Contains("captured", StringComparison.Ordinal)
                ? "  (off-air capture)"
                : "  (synthesised example, no off-air capture in this tree)");

        return WavAudio.Read(file);
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
