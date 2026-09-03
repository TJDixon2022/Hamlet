using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 239, task 1.3: does a reader of the tap delay the writer?
/// </summary>
/// <remarks>
/// <para>**THIS IS THE MEASUREMENT THE BUILD TASKS REST ON.** If a reader does
/// not delay `Take`, the suspect is refuted and nothing below it is worth
/// building. It is measured rather than reasoned about, because the reasoning —
/// a shared lock, a large allocation, a modulo per sample — is exactly the kind
/// that is obviously right and sometimes false.</para>
/// <para>**THE WRITER IS PACED AT 48 kHz** in 4,800-sample chunks, which is one
/// chunk per 100 ms — the device's own buffer period, read off NAudio 2.2.1 in
/// `WhatBufferPeriodIsInForceTests` as 100 ms. What is measured is how long
/// `Take` itself takes, not how long the loop takes: the callback's cost is the
/// call, and everything else in this harness is the harness.</para>
/// <para>**BOTH ARMS ARE MEASURED IN THE SAME RUN AND THE SAME PROCESS**, so the
/// difference between them is the reader and not the machine's mood.</para>
/// </remarks>
public sealed class DoesAReaderDelayTheWriterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public DoesAReaderDelayTheWriterTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>The device buffer period in force, measured separately.</summary>
    private const double BufferMicroseconds = 100_000;

    /// <summary>One device buffer at 48 kHz and a 100 ms period.</summary>
    private const int Chunk = 4_800;

    /// <summary>How many chunks each arm writes.</summary>
    /// <remarks>
    /// **TWO HUNDRED, SO THE 99TH PERCENTILE IS NOT THE MAXIMUM.** At sixty
    /// samples the p99 index is the last one, so the two figures were identical
    /// and the percentile said nothing the maximum had not already said. Two
    /// hundred chunks is twenty seconds per arm and puts two samples above the
    /// p99, which is the least that makes it a different number.
    /// </remarks>
    private const int Chunks = 200;

    /// <summary>What a reader costs the writer, with it running and stopped.</summary>
    [Fact]
    public void TheWritersWorstTakeWithAndWithoutAReader()
    {
        var quiet = Measure(readers: 0, out var quietCalls);
        var loud = Measure(readers: 1, out var loudCalls);

        _output.WriteLine("writer: " + Chunk + "-sample chunks at "
            + Rate + " Hz, paced to one per " + (Chunk * 1000.0 / Rate) + " ms");
        _output.WriteLine("reader: Tail(6 s) at 1 Hz and Window(15 s) every 15 s,");
        _output.WriteLine("        the two repeating readers found in task 1.2");
        _output.WriteLine("");
        _output.WriteLine("            no reader      reader running");
        _output.WriteLine("worst  Take " + Us(quiet.Worst) + "   " + Us(loud.Worst));
        _output.WriteLine("p99    Take " + Us(quiet.P99) + "   " + Us(loud.P99));
        _output.WriteLine("median Take " + Us(quiet.Median) + "   " + Us(loud.Median));
        _output.WriteLine("");
        _output.WriteLine("reader calls made: " + loudCalls + " (quiet arm: " + quietCalls + ")");
        _output.WriteLine("buffer period    : " + Us(BufferMicroseconds));
        _output.WriteLine("worst as a share : "
            + (loud.Worst / BufferMicroseconds).ToString("P1") + " of the period");

        // **THE HARNESS MUST HAVE ACTUALLY RUN A READER**, or the comparison is
        // between two identical arms and means nothing.
        Assert.True(loudCalls > 0, "the reader arm never read the tap");

        // Nothing else is asserted. This is the measurement the build tasks are
        // aimed by, and a threshold here would be a threshold about this
        // machine (SHACK_FACTS.md FACT-004).
    }

    private static string Us(double micros) => micros.ToString("0").PadLeft(9) + " us";

    private (double Worst, double P99, double Median) Measure(int readers, out long calls)
    {
        var tap = new AudioTap();
        var chunk = new float[Chunk];

        // Fill the ring first, so every reader copies a full 30 seconds - which
        // is what a reader on a running radio does.
        for (var i = 0; i < Rate * AudioTap.SecondsKept / Chunk; i++)
        {
            tap.Take(chunk, Rate);
        }

        var stop = false;
        var reads = 0L;
        var threads = new List<Thread>();

        for (var r = 0; r < readers; r++)
        {
            var thread = new Thread(() =>
            {
                var lastWindow = Stopwatch.GetTimestamp();

                while (!Volatile.Read(ref stop))
                {
                    // The keying meter, at its real 1 Hz cadence compressed to
                    // the harness's pace: it is the repeating reader that holds
                    // the lock longest per call.
                    tap.Tail(TimeSpan.FromSeconds(6));
                    Interlocked.Increment(ref reads);

                    // The slot watch's 15-second window, at its own cadence.
                    var since = (Stopwatch.GetTimestamp() - lastWindow)
                        / (double)Stopwatch.Frequency;

                    if (since >= 0.25)
                    {
                        tap.Window(Math.Max(0, tap.SamplesSeen - 15 * Rate), 15 * Rate);
                        Interlocked.Increment(ref reads);
                        lastWindow = Stopwatch.GetTimestamp();
                    }

                    Thread.Sleep(1);
                }
            })
            { IsBackground = true, Name = "tap-reader" };

            threads.Add(thread);
            thread.Start();
        }

        var costs = new double[Chunks];
        var pace = Stopwatch.Frequency * Chunk / Rate;
        var next = Stopwatch.GetTimestamp();

        for (var i = 0; i < Chunks; i++)
        {
            // Paced like a device: one chunk every buffer period, so the reader
            // has real gaps to get the lock in, exactly as it does in the app.
            while (Stopwatch.GetTimestamp() < next)
            {
                Thread.SpinWait(50);
            }

            next += pace;

            var started = Stopwatch.GetTimestamp();
            tap.Take(chunk, Rate);
            costs[i] = (Stopwatch.GetTimestamp() - started)
                * 1_000_000.0 / Stopwatch.Frequency;
        }

        Volatile.Write(ref stop, true);

        foreach (var thread in threads)
        {
            thread.Join(TimeSpan.FromSeconds(2));
        }

        calls = Interlocked.Read(ref reads);

        Array.Sort(costs);

        return (
            costs[^1],
            costs[(int)(costs.Length * 0.99)],
            costs[costs.Length / 2]);
    }
}
