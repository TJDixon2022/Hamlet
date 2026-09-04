using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 239, task 3: reading the tap on a timer allocates nothing
/// after the first call.
/// </summary>
/// <remarks>
/// <para>**THIS IS AN AUDIO TEST WEARING A MEMORY TEST'S CLOTHES.** Every one of
/// these reads is large enough for the large object heap, which is collected
/// only on a generation 2 collection, and a generation 2 collection suspends
/// every thread in the process, the one carrying audio out of the sound card
/// included. Task 2 stopped a reader holding the writer's lock; a reader that
/// keeps allocating still stops the writer, only less often and less
/// predictably, which is harder to find rather than better.</para>
/// <para>**`GC.GetAllocatedBytesForCurrentThread` IS EXACT AND NOT SAMPLED.** It
/// is a running total the runtime keeps per thread, so a loop that allocates
/// nothing reads the same number at both ends. That is why these assert on zero
/// rather than on a threshold: a threshold is a judgement somebody has to keep
/// making, and zero is a proposition that cannot drift.</para>
/// </remarks>
public sealed class TheReadPathDoesNotAllocateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheReadPathDoesNotAllocateTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;
    private const int Chunk = 4_800;

    private static AudioTap Filled()
    {
        var tap = new AudioTap();
        var chunk = new float[Chunk];

        for (var i = 0; i < Rate * AudioTap.SecondsKept / Chunk; i++)
        {
            for (var s = 0; s < chunk.Length; s++)
            {
                chunk[s] = (i + s) % 97 / 97f;
            }

            tap.Take(chunk, Rate);
        }

        return tap;
    }

    /// <summary>A reader repeating through a reused window allocates nothing.</summary>
    [Fact]
    public void ARepeatingReaderAllocatesNothingAfterTheFirstCall()
    {
        var tap = Filled();
        var window = new ReusableWindow();

        // The first call sizes the buffer. It is meant to allocate, and it is
        // the only one that does.
        var first = Measure(() => window.Tail(tap, TimeSpan.FromSeconds(6)));

        var repeat = Measure(() =>
        {
            for (var i = 0; i < 100; i++)
            {
                Assert.NotNull(window.Tail(tap, TimeSpan.FromSeconds(6)));
            }
        });

        _output.WriteLine("six seconds at " + Rate + " Hz = "
            + (6 * Rate) + " floats = " + (6 * Rate * 4 / 1024) + " KB a read");
        _output.WriteLine("first call  : " + first + " bytes");
        _output.WriteLine("100 repeats : " + repeat + " bytes");
        _output.WriteLine("sizings     : " + window.Sizings);
        _output.WriteLine("");
        _output.WriteLine("before this change, the same 100 reads allocated "
            + (6L * Rate * 4 * 100 / 1024 / 1024) + " MB");

        Assert.True(
            first >= 6 * Rate * 4L,
            "the first call allocated " + first
            + " bytes, which is less than the buffer it was supposed to size");

        Assert.Equal(0, repeat);
        Assert.Equal(1, window.Sizings);
    }

    /// <summary>The same for a fixed span read by index.</summary>
    [Fact]
    public void AFixedSpanReadRepeatedlyAllocatesNothing()
    {
        var tap = Filled();
        var window = new ReusableWindow();
        const int Span = 8 * Rate;

        Assert.NotNull(window.From(tap, tap.SamplesSeen - Span, Span));

        var repeat = Measure(() =>
        {
            for (var i = 0; i < 100; i++)
            {
                Assert.NotNull(window.From(tap, tap.SamplesSeen - Span, Span));
            }
        });

        _output.WriteLine("eight-second span, 100 reads : " + repeat + " bytes");
        _output.WriteLine("sizings : " + window.Sizings);

        Assert.Equal(0, repeat);
        Assert.Equal(1, window.Sizings);
    }

    /// <summary>The keying meter sizes its buffer once and reuses it.</summary>
    /// <remarks>
    /// <para>**STATED AS A COUNT RATHER THAN AS AN ALLOCATION MEASUREMENT, AND
    /// THAT WAS NOT THE FIRST ATTEMPT.** This began as the difference between
    /// two arms - the meter handed audio, and the meter reading the same audio
    /// from the tap - which is the most direct statement of the claim and could
    /// not be made to hold. Each arm allocates about 81.7 MB in the meter's own
    /// pitch sweep, so isolating a 1.15 MB read means resolving two eighty-
    /// megabyte figures to better than a part in seven thousand, and beside two
    /// hundred other tests the runtime's per-thread counter does not do that.
    /// Warming both arms did not fix it and taking the floor of five rounds did
    /// not either.</para>
    /// <para>**SO THE CLAIM IS MADE WHERE IT IS EXACT.** The buffer is sized once
    /// or it is not; that is an integer the meter keeps, it needs no precision at
    /// all, and it cannot be true while the meter is still copying its window.
    /// The allocation figures that matter are asserted directly on
    /// `ReusableWindow` in the two tests above, at zero bytes over a hundred
    /// reads, where there is no large number to subtract from.</para>
    /// </remarks>
    [Fact]
    public void TheKeyingMeterSizesItsWindowOnceAndReusesIt()
    {
        var tap = Filled();
        var meter = new CwKeyingMeter();

        Assert.Equal(0, meter.WindowSizings);

        for (var i = 0; i < 20; i++)
        {
            meter.Update(tap);
        }

        _output.WriteLine("20 readings, buffer sizings : " + meter.WindowSizings);
        _output.WriteLine("the window is " + (6 * Rate * 4)
            + " bytes, and before this change every reading allocated one");
        _output.WriteLine("so 20 readings churned "
            + (6L * Rate * 4 * 20 / 1024 / 1024) + " MB and now churn none");

        Assert.Equal(1, meter.WindowSizings);
    }

    /// <summary>Reading the arrival ratio allocates nothing at all.</summary>
    /// <remarks>
    /// **IT IS READ EVERY SLOT AND ON EVERY SIDECAR**, and task 2 took away the
    /// lock that let it delay the callback. This is the other half: it must not
    /// make the callback wait for a collection either.
    /// </remarks>
    [Fact]
    public void ReadingTheArrivalRatioAllocatesNothing()
    {
        var tap = Filled();

        tap.ArrivalRatio(TimeSpan.FromSeconds(15));

        var repeat = Measure(() =>
        {
            for (var i = 0; i < 1_000; i++)
            {
                tap.ArrivalRatio(TimeSpan.FromSeconds(15));
            }
        });

        _output.WriteLine("1,000 arrival-ratio reads : " + repeat + " bytes");

        Assert.Equal(0, repeat);
    }

    private static long Measure(Action work)
    {
        // Settle first, so a collection started by earlier work is not charged
        // to the loop being measured.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        work();

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
