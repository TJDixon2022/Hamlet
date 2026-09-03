using System.Diagnostics;
using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Audio recorded off a real antenna, pushed through Hamlet's own ring buffer and
/// its own slot watch at the rate a sound card actually delivers.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE FIRST TIME ANYTHING IN THIS APPLICATION HAS HEARD A REAL
/// BAND.** Twenty-five units built a decoder, a slot watch and a table that fills
/// on its own, and every row that ever appeared on the Digital tab came from audio
/// this project synthesized for itself. A port that is self-consistently wrong at
/// both ends passes every such test. <c>Ft8Sharp</c> has read upstream's off-air
/// recordings since step 5 — but inside the library, with no tap, no watch and no
/// view model between the file and the decoder.</para>
/// <para>**WHAT IS UNDER TEST IS THE JOIN AND NOT THE DECODER.** Unit 224 proved
/// <see cref="Ft8Reader"/> at four sample rates; that is the reader. Here the
/// samples go into a real <see cref="AudioTap"/> in the chunk sizes a capture
/// delivers, at 48 000 and 44 100 rather than the library's 12 000, and the slots
/// are found by <see cref="Ft8SlotWatch"/> crossing real boundaries against a clock
/// that is a variable. The ring, the resampler, the sample-index arithmetic and the
/// boundary crossing are all exercised the way they will be at the radio.</para>
/// <para>**WHAT IT IS NOT, AND THE REPORT SAYS THIS IN THESE WORDS.** It is not a
/// radio. There is no sound card, no USB-D path, no rig and no clock error in it,
/// and it is not a sensitivity measurement of anything.</para>
/// <para>**NOTHING IS COPIED OUT OF THE CLONE** — not a WAV, not a fragment, not a
/// pasted expected list — and absence of the clone is a skip. See
/// <see cref="OffAirRecordings"/>.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2). Audio moves out
/// of a file and into a ring buffer.</para>
/// </remarks>
public sealed class RealOffAirAudioReachesTheTabTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public RealOffAirAudioReachesTheTabTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>How many of the busiest recordings the played runs take.</summary>
    /// <remarks>
    /// **BOUNDED ON PURPOSE.** Every one of these plays a whole recording through a
    /// ring buffer sample by sample at a device rate and decodes each slot on the
    /// way past, so the cost is real. Twelve is enough to see whether the join
    /// works and cheap enough to run twice, once at each rate.
    /// </remarks>
    private const int Played = 12;

    /// <summary>
    /// The moment the first sample of a recording is treated as having arrived.
    /// </summary>
    /// <remarks>
    /// **A QUARTER MINUTE, BECAUSE THAT IS WHERE UPSTREAM'S RECORDINGS BEGIN.**
    /// These are slot captures: the audio starts at a boundary and the transmission
    /// starts a fraction of a second into it. Placing the first sample anywhere else
    /// would cut every slot across two of upstream's, which would be a test of
    /// arithmetic this unit made up rather than of the path.
    /// </remarks>
    private static readonly DateTime FirstSampleAt =
        new(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc);

    /// <summary>A clock measured two minutes earlier and found to match UTC.</summary>
    /// <remarks>
    /// **MEASURED AT ZERO IS NOT UNKNOWN** (§0.0). The offset has to be a real
    /// measurement or the watch refuses, which is its correct behaviour and not the
    /// thing being tested here. There is no clock error in this run and the report
    /// says so.
    /// </remarks>
    private static ClockOffset Measured =>
        new(0, FirstSampleAt.AddMinutes(-2));

    /// <summary>How often the Digital tab looks, in seconds.</summary>
    private const double LookSeconds = 0.25;

    /// <summary>Formats a diagnostic line the same way whatever the machine's locale.</summary>
    /// <param name="parts">The pieces of the line.</param>
    /// <returns>Them, joined and formatted invariantly.</returns>
    /// <remarks>
    /// A printed measurement that reads <c>0,5</c> on one machine and <c>0.5</c> on
    /// another is a measurement somebody has to ask about. The pieces are taken
    /// separately because joining interpolated strings with <c>+</c> produces a
    /// <see cref="string"/> before anything can choose a culture for it.
    /// </remarks>
    private static string Inv(params FormattableString[] parts)
        => string.Concat(parts.Select(FormattableString.Invariant));

    /// <summary>
    /// How much silence is played before and after the recording.
    /// </summary>
    /// <remarks>
    /// **THE RING HAS TO HOLD THE WHOLE SLOT WHEN THE BOUNDARY ARRIVES.** The watch
    /// asks for the fifteen seconds that ended, so a stream that starts exactly at
    /// the boundary has nothing behind it and the watch refuses — correctly. A
    /// second each side is a stream that was already running, which is what a sound
    /// card is.
    /// </remarks>
    private const double RollSeconds = 1.0;

    /// <summary>
    /// **THE QUESTION NO UNIT HAS ASKED: HOW LONG DOES A SLOT ACTUALLY TAKE.**
    /// <c>DecodeTheSlotAsync</c> guards with a boolean on the stated assumption that
    /// a decode takes far less than the fifteen seconds until the next slot, and
    /// nobody has measured that against a real band. A slot arriving while a decode
    /// runs is discarded and never comes back, so the margin is not a performance
    /// question — it is whether the loss path is reachable at the radio.
    /// </summary>
    /// <remarks>
    /// **MACHINE-DEPENDENT, AND IT IS ONE MACHINE.** The figure below is this
    /// machine on this evening and is quoted as such.
    /// </remarks>
    [RequiresOffAirRecordingsFact]
    public void HowLongOneSlotTakesToDecodeOnABusyBand()
    {
        var recordings = OffAirRecordings.Busiest(Played);

        Assert.NotEmpty(recordings);

        var perSlot = new List<double>();

        _output.WriteLine(
            "  the busiest recordings upstream's own lists know about,");
        _output.WriteLine(
            "  decoded by Ft8Reader.Read exactly as DecodeTheSlotAsync calls it:");
        _output.WriteLine("");

        foreach (var recording in recordings)
        {
            var audio = recording.Read();
            var endedAt = FirstSampleAt.AddSeconds(audio.Duration.TotalSeconds);

            var watch = Stopwatch.StartNew();
            var heard = Ft8Reader.Read(audio, endedAt, Measured);
            watch.Stop();

            var slots = Math.Max(1, heard.SlotsDecoded);
            var each = watch.Elapsed.TotalMilliseconds / slots;

            perSlot.Add(each);

            _output.WriteLine(
                Inv(
                    $"    {recording.Name,-34} {audio.SampleRate} Hz  ",
                    $"{audio.Duration.TotalSeconds,5:0.0} s  ",
                    $"{heard.SlotsDecoded} slot(s)  ",
                    $"{heard.CandidatesFound,4} candidates  ",
                    $"{heard.Decodes.Count,3} msg  ",
                    $"{each,7:0.0} ms/slot  ",
                    $"(upstream lists {recording.ExpectedCount})"));
        }

        var sorted = perSlot.OrderBy(x => x).ToArray();
        var median = sorted.Length % 2 == 1
            ? sorted[sorted.Length / 2]
            : (sorted[(sorted.Length / 2) - 1] + sorted[sorted.Length / 2]) / 2;
        var worst = sorted[^1];

        _output.WriteLine("");
        _output.WriteLine(
            Inv(
                $"  median {median:0.0} ms, worst {worst:0.0} ms, ",
                $"worst as a fraction of the fifteen-second slot ",
                $"{worst / 15000.0:0.0000} — one machine, this evening"));

        // **NOT A THRESHOLD ANYBODY TUNES TO.** The assertion is only that the
        // measurement was taken and is a real number; what it means for the guard
        // in `DecodeTheSlotAsync` is reasoned about in the report, not here.
        Assert.All(perSlot, each => Assert.True(each > 0));
    }

    /// <summary>
    /// **THE UNIT'S NUMBER, AT 48 000** — the rate a USB codec delivers.
    /// </summary>
    [RequiresOffAirRecordingsFact]
    public void OffAirAudioAt48000ProducesRowsThroughTheTapAndTheWatch()
        => PlayEveryRecording(48000);

    /// <summary>
    /// **AND AT 44 100**, which is deliberately not a whole ratio to 12 000, so the
    /// resampler's fractional phase and the sample-index arithmetic are both
    /// exercised on audio nobody here generated.
    /// </summary>
    [RequiresOffAirRecordingsFact]
    public void OffAirAudioAt44100ProducesRowsThroughTheTapAndTheWatch()
        => PlayEveryRecording(44100);

    /// <summary>Play the busiest recordings through the tap at one device rate.</summary>
    /// <param name="deviceRate">What the sound card is pretending to be.</param>
    private void PlayEveryRecording(int deviceRate)
    {
        var recordings = OffAirRecordings.Busiest(Played);

        Assert.NotEmpty(recordings);

        var chunk = ChunkFor(deviceRate);

        _output.WriteLine(
            Inv(
                $"  device rate {deviceRate} Hz, delivered in {chunk}-sample chunks ",
                $"({1000.0 * chunk / deviceRate:0.0} ms), through a real AudioTap ",
                $"and a real Ft8SlotWatch"));
        _output.WriteLine("");

        var totalRows = 0;
        var totalSlots = 0;
        var totalWitness = 0;
        var totalExpected = 0;
        var slowest = 0.0;
        var slowestWhere = "";

        foreach (var recording in recordings)
        {
            var run = Play(recording, deviceRate, chunk);

            totalRows += run.Decodes.Count;
            totalSlots += run.SlotsReady;
            totalExpected += recording.ExpectedCount;

            var expected = recording.ExpectedMessages().ToHashSet(StringComparer.Ordinal);
            var witness = run.Decodes.Count(d => expected.Contains(d.Message));
            totalWitness += witness;

            if (run.SlowestSlotMs > slowest)
            {
                slowest = run.SlowestSlotMs;
                slowestWhere = recording.Name;
            }

            _output.WriteLine(
                Inv(
                    $"    {recording.Name,-34} {run.SlotsReady} slot(s) handed over, ",
                    $"{run.Decodes.Count,3} rows, slowest slot ",
                    $"{run.SlowestSlotMs,7:0.0} ms, {witness} of them in ",
                    $"upstream's list of {recording.ExpectedCount} (a witness, ",
                    $"not a gate)"));

            foreach (var decode in run.Decodes)
            {
                _output.WriteLine(
                    Inv(
                        $"      {decode.SlotStartUtc:HHmmss}  dt {decode.OffsetSeconds,5:0.00}  ",
                        $"{decode.FrequencyHz,5:0} Hz  {decode.Message}"));
            }

            if (run.Skipped > 0 || run.LastRefusal.Length > 0)
            {
                _output.WriteLine(
                    $"      skipped {run.Skipped}, last refusal [{run.LastRefusal}]");
            }

            // **THE WATCH MUST NOT REFUSE HERE AND MUST NOT SKIP HERE.** The stream
            // never stalls, the clock is measured and the ring is thirty seconds, so
            // a refusal or a skipped slot in this run would be a real fault in the
            // join rather than a fact about the band.
            Assert.Equal(string.Empty, run.LastRefusal);
            Assert.Equal(0, run.Skipped);
        }

        _output.WriteLine("");
        _output.WriteLine(
            Inv(
                $"  {totalRows} rows out of {totalSlots} slots at {deviceRate} Hz. ",
                $"{totalWitness} of them appear in upstream's lists, which total ",
                $"{totalExpected} across these recordings — reported as a witness ",
                $"and never as a gate."));
        _output.WriteLine(
            Inv(
                $"  slowest slot anywhere in this run {slowest:0.0} ms in ",
                $"{slowestWhere} — {slowest / 15000.0:0.0000} of a slot."));
        _output.WriteLine("");
        _output.WriteLine(
            "  This is not a radio. There is no sound card, no USB-D path, no rig");
        _output.WriteLine(
            "  and no clock error in it, and it is not a sensitivity measurement of");
        _output.WriteLine("  anything.");

        // **THE ONE THAT MATTERS.** Audio recorded off a real antenna produced rows
        // through Hamlet's own tap and Hamlet's own watch.
        Assert.True(
            totalRows > 0,
            $"no row came out of {totalSlots} slots of real off-air audio at "
            + $"{deviceRate} Hz");
    }

    /// <summary>What one recording gave up on the way through the tap.</summary>
    /// <param name="SlotsReady">Whole slots the watch handed over.</param>
    /// <param name="Skipped">Whole slots it counted as missed.</param>
    /// <param name="LastRefusal">The last thing it refused with, or "".</param>
    /// <param name="Decodes">The messages, oldest slot first.</param>
    /// <param name="SlowestSlotMs">The wall time of the slowest slot decode.</param>
    internal sealed record Run(
        int SlotsReady,
        int Skipped,
        string LastRefusal,
        IReadOnlyList<Ft8Decode> Decodes,
        double SlowestSlotMs);

    /// <summary>
    /// How big a chunk the capture path delivers, in samples.
    /// </summary>
    /// <param name="deviceRate">The device's rate.</param>
    /// <returns>Ten milliseconds' worth.</returns>
    /// <remarks>
    /// **TEN MILLISECONDS IS WASAPI'S SHARED-MODE PERIOD**, which is what
    /// <c>WasapiAudioSource.OnDataAvailable</c> is handed and hands straight on to
    /// the tap. At 48 000 that is a round 480 and at 44 100 it is 441 — neither
    /// divides fifteen seconds' worth of samples evenly, which is the point: the
    /// boundary the watch is looking for falls in the middle of a chunk, exactly as
    /// it will at the radio.
    /// </remarks>
    internal static int ChunkFor(int deviceRate)
        => (int)Math.Round(deviceRate * 0.010);

    /// <summary>
    /// A chunk of samples, as it arrives after whatever the device did to it.
    /// </summary>
    /// <param name="block">What was on the wire.</param>
    /// <returns>What the tap is handed.</returns>
    /// <remarks>
    /// **THE ONE SEAM BETWEEN THE FLOAT PATH AND THE BYTE PATH** (unit 237).
    /// Everything else about a run is identical, so a difference between the two
    /// has exactly one possible home.
    /// </remarks>
    internal delegate ReadOnlySpan<float> DeviceArrival(ReadOnlySpan<float> block);

    /// <summary>
    /// Play one recording into a real tap and let a real watch find its slots.
    /// </summary>
    /// <param name="recording">The off-air audio.</param>
    /// <param name="deviceRate">What the sound card is pretending to be.</param>
    /// <param name="chunk">How many samples arrive at once.</param>
    /// <param name="through">
    /// What the samples pass through on the way in, or null for the float path
    /// this file has always used.
    /// </param>
    /// <returns>What came out.</returns>
    /// <remarks>
    /// **<paramref name="through"/> IS WHY THIS IS ONE HELPER AND NOT TWO** (unit
    /// 237, task 4). Writing a second harness for the byte path would put two
    /// differences between the two runs - the conversion, and whatever the second
    /// harness got wrong - and only one of them would be the thing under test.
    /// </remarks>
    internal static Run Play(
        OffAirRecording recording,
        int deviceRate,
        int chunk,
        DeviceArrival? through = null)
    {
        var source = recording.Read();

        // **UP TO THE DEVICE RATE FIRST, AND THE PATH TAKES IT BACK DOWN.** The
        // recordings are 12 kHz and a sound card is not, so the audio has to arrive
        // the way it will arrive at the radio. The round trip is deliberate: the
        // resampler is in the path twice and its cost is part of what is measured.
        var samples = source.SampleRate == deviceRate
            ? source.Samples
            : Ft8Resample.Resample(source.Samples, source.SampleRate, deviceRate);

        var roll = (int)Math.Round(RollSeconds * deviceRate);
        var silence = new float[chunk];

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        var now = FirstSampleAt.AddSeconds(-RollSeconds);

        // **THE WATCH ARMS INSIDE THE RECORDING'S OWN FIRST SLOT, NOT IN THE
        // PRE-ROLL.** The tab starts watching while audio is already flowing, and
        // the first look only ever arms. A look during the second of silence before
        // the recording would arm one slot earlier and the boundary would then ask
        // for fifteen seconds that were never on the wire — which the watch answers
        // with `AudioAgedOut`, correctly, because the ring really does not hold
        // them. That is the start-up case unit 225 already asserts, and measuring it
        // again here would only measure the pre-roll this file invented.
        var nextLook = FirstSampleAt.AddSeconds(LookSeconds);

        var decodes = new List<Ft8Decode>();
        var slotsReady = 0;
        var skipped = 0;
        var refusal = "";
        var slowest = 0.0;

        void Deliver(ReadOnlySpan<float> block)
        {
            var arrived = through is null ? block : through(block);

            tap.Take(arrived, deviceRate);
            now = now.AddSeconds((double)arrived.Length / deviceRate);

            while (now >= nextLook)
            {
                var look = watch.Look(tap, nextLook, Measured);

                skipped += look.Skipped;

                if (look.Refusal.Length > 0)
                {
                    refusal = look.Refusal;
                }

                if (look.Ready is { } ready)
                {
                    slotsReady++;

                    var timer = Stopwatch.StartNew();
                    var heard = Ft8Reader.Read(
                        ready.Audio, ready.EndedAtPcUtc, Measured);
                    timer.Stop();

                    slowest = Math.Max(slowest, timer.Elapsed.TotalMilliseconds);

                    if (heard.Refusal.Length > 0)
                    {
                        refusal = heard.Refusal;
                    }

                    decodes.AddRange(heard.Decodes);
                }

                nextLook = nextLook.AddSeconds(LookSeconds);
            }
        }

        for (var delivered = 0; delivered < roll; delivered += chunk)
        {
            Deliver(silence.AsSpan(0, Math.Min(chunk, roll - delivered)));
        }

        for (var at = 0; at < samples.Length; at += chunk)
        {
            Deliver(samples.AsSpan(at, Math.Min(chunk, samples.Length - at)));
        }

        for (var delivered = 0; delivered < roll; delivered += chunk)
        {
            Deliver(silence.AsSpan(0, Math.Min(chunk, roll - delivered)));
        }

        return new Run(slotsReady, skipped, refusal, decodes, slowest);
    }
}
