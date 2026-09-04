using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Audio recorded off a real antenna, arriving as rows on the Digital tab's own
/// collection.
/// </summary>
/// <remarks>
/// <para>**THIS IS WHERE THE UNIT'S NUMBER IS COUNTED, BECAUSE THIS IS WHERE THE
/// OPERATOR'S EYES ARE.** The engine-side measurement in
/// <c>RealOffAirAudioReachesTheTabTests</c> ends at <see cref="Ft8Reader"/>; this
/// one ends at <c>MainWindowViewModel.DigitalDecodes</c>, which is what the markup
/// binds to. Both are worth reporting and this is the stronger reading.</para>
/// <para>**EVERY ROW THE DIGITAL TAB HAS EVER SHOWN CAME FROM AUDIO THIS PROJECT
/// SYNTHESIZED FOR ITSELF** — until this file. Unit 224 put one message on the
/// table from a capture press over generated audio; unit 225 filled it slot after
/// slot, also over generated audio, and said in its own words that nothing in it
/// had heard a radio. These are Karlis Goba's off-air recordings: real stations, a
/// real antenna, somebody else's ionosphere.</para>
/// <para>**AND IT IS STILL NOT A RADIO.** There is no sound card, no USB-D path,
/// no rig and no clock error in this run, and it is not a sensitivity measurement
/// of anything.</para>
/// <para>**NOTHING HERE OPENS A WINDOW OR READS THE MACHINE'S CLOCK** (§5), and
/// nothing reaches a transmitter (§0.2). Nothing is copied out of the clone, and
/// its absence is a skip.</para>
/// </remarks>
public sealed class TheTabHearsARealBandTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheTabHearsARealBandTests(ITestOutputHelper output) => _output = output;

    /// <summary>The rate a USB codec delivers.</summary>
    private const int DeviceRate = 48000;

    /// <summary>Ten milliseconds, which is WASAPI's shared-mode period.</summary>
    private const int ChunkSamples = DeviceRate / 100;

    /// <summary>How often the Digital tab looks, in seconds.</summary>
    private const double LookSeconds = 0.25;

    /// <summary>The quarter minute upstream's recording is played from.</summary>
    private static readonly DateTime FirstSampleAt =
        new(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc);

    /// <summary>A clock measured two minutes earlier and found to match UTC.</summary>
    private static ClockOffset Measured =>
        new(0, FirstSampleAt.AddMinutes(-2));

    /// <summary>
    /// **THE ONE THIS UNIT WAS COMMISSIONED FOR.** Real off-air audio, through a
    /// real ring buffer at a real device rate, cut by the real watch, onto the
    /// view model's own rows.
    /// </summary>
    [RequiresOffAirRecordingsFact]
    public void RealOffAirAudioPutsRowsOnTheTabsOwnCollection()
    {
        var recording = Assert.Single(OffAirRecordings.Busiest(1));

        var model = new MainWindowViewModel(new AppSettings(), null);

        var slots = Play(recording, model);

        _output.WriteLine(
            $"  {recording.Name}, played at {DeviceRate} Hz in "
            + $"{ChunkSamples}-sample chunks through a real AudioTap");
        _output.WriteLine(
            $"  {slots} slot(s) handed over by the watch, "
            + $"{model.DigitalDecodes.Count} rows on the tab");
        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");
        _output.WriteLine($"  strip   [{model.DigitalModeStripLine}]");
        _output.WriteLine("");

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                $"    {row.Utc}  {row.Snr}  {row.Dt}  {row.Hz}  {row.Message}");
        }

        Assert.Equal(1, slots);

        // **THE COUNT IS NOT ASSERTED AT A NUMBER** (§0.0). How many messages this
        // port reads out of somebody else's recording is a measurement, and an
        // assertion at a figure would be a threshold this unit tuned to sixty
        // recordings. What is asserted is that real off-air audio reached the tab
        // at all, which is the thing that had never happened.
        Assert.NotEmpty(model.DigitalDecodes);
        Assert.True(model.HasDigitalDecodes);

        // Every row is about the slot it came out of and no other.
        Assert.All(
            model.DigitalDecodes, r => Assert.Equal("142230", r.Utc));

        // **THE SNR CELL STAYS AN EM DASH** (§0.0). This decoder produces a Costas
        // sync score and no decibels, and HM-OPEN-068 is Tim's.
        Assert.All(
            model.DigitalDecodes,
            r => Assert.Equal(DigitalDecodeRow.NoMeasurement, r.Snr));

        // The summary names the slot the rows came from and how many there are,
        // which is the only claim on that panel and is true of this recording.
        Assert.Equal(
            $"142230 UTC · {model.DigitalDecodes.Count} shown · newest first",
            model.DigitalDecodedSummary);

        // **A WITNESS, NEVER A GATE.** Step 5's third criterion was rewritten by
        // the owner on 2026-09-02 and this unit does not re-litigate it, does not
        // chase this count and adjusts nothing to move it.
        var expected = recording.ExpectedMessages().ToHashSet(StringComparer.Ordinal);
        var known = model.DigitalDecodes.Count(r => expected.Contains(r.Message));

        _output.WriteLine("");
        _output.WriteLine(
            $"  {known} of {model.DigitalDecodes.Count} rows appear in upstream's "
            + $"own list of {expected.Count} for this recording — a witness, not a "
            + "gate.");
        _output.WriteLine("");
        _output.WriteLine(
            "  This is not a radio. There is no sound card, no USB-D path, no rig");
        _output.WriteLine(
            "  and no clock error in it, and it is not a sensitivity measurement of");
        _output.WriteLine("  anything.");
    }

    /// <summary>
    /// Play one recording into a real tap and let the real watch feed the model.
    /// </summary>
    /// <param name="recording">The off-air audio.</param>
    /// <param name="model">The view model whose rows are being filled.</param>
    /// <returns>How many whole slots the watch handed over.</returns>
    /// <remarks>
    /// **THIS IS <c>OnSlotTick</c> AND <c>DecodeTheSlotAsync</c> WITH THE TIMER
    /// TAKEN OUT.** The view model's own tick reads <c>DateTime.UtcNow</c> and hangs
    /// off a live CW decoder, neither of which exists in a test; everything after
    /// the look — the reader, <c>NoteSlot</c>, the de-duplication and the rows — is
    /// the shipped code called the way the shipped code calls it.
    /// </remarks>
    private static int Play(OffAirRecording recording, MainWindowViewModel model)
    {
        var source = recording.Read();

        var samples = Ft8Resample.Resample(
            source.Samples, source.SampleRate, DeviceRate);

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        // A second of the stream before the recording and a second after it,
        // because a sound card is already running when the tab starts watching and
        // the ring has to hold the whole slot when the boundary arrives.
        var roll = DeviceRate;
        var silence = new float[ChunkSamples];

        var now = FirstSampleAt.AddSeconds(-1);

        // The watch arms inside the recording's own first slot. Arming in the
        // pre-roll would ask for fifteen seconds that were never on the wire.
        var nextLook = FirstSampleAt.AddSeconds(LookSeconds);

        var slots = 0;

        void Deliver(ReadOnlySpan<float> block)
        {
            tap.Take(block, DeviceRate);
            now = now.AddSeconds((double)block.Length / DeviceRate);

            while (now >= nextLook)
            {
                var look = watch.Look(tap, nextLook, Measured);

                if (look.Ready is { } ready)
                {
                    slots++;
                    model.NoteSlot(
                        Ft8Reader.Read(ready.Audio, ready.EndedAtPcUtc, Measured));
                }

                nextLook = nextLook.AddSeconds(LookSeconds);
            }
        }

        for (var at = 0; at < roll; at += ChunkSamples)
        {
            Deliver(silence.AsSpan(0, Math.Min(ChunkSamples, roll - at)));
        }

        for (var at = 0; at < samples.Length; at += ChunkSamples)
        {
            Deliver(samples.AsSpan(at, Math.Min(ChunkSamples, samples.Length - at)));
        }

        for (var at = 0; at < roll; at += ChunkSamples)
        {
            Deliver(silence.AsSpan(0, Math.Min(ChunkSamples, roll - at)));
        }

        return slots;
    }
}
