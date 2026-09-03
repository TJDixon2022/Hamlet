using System.Reflection;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// The digital capture press is driven the way the button drives it, into a
/// temporary folder, and the two files it is supposed to leave are read back.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE WRITER THAT HAS NEVER SUCCEEDED ON THE OWNER'S MACHINE.**
/// Unit 234's trace found that `%AppData%\Hamlet\captures` does not exist —
/// not empty, absent — and `CaptureDigital` creates it on its first successful
/// write. So on that machine no press has ever got as far as
/// `Directory.CreateDirectory`. Nothing in this repository had ever driven the
/// path, either, so *the write path had never been observed working anywhere*.
/// That is what this class fixes.</para>
/// <para>**THE OPERATOR'S CAPTURES ARE NOT A TEST'S TO WRITE INTO.**
/// <see cref="MainWindowViewModel.CaptureFolder"/> is settable expressly for
/// this, and every test here restores it in a `finally` and then asserts it is
/// back — a leaked static in a shared test host would point a later test at the
/// operator's real folder.</para>
/// <para>**THE SEAM.** `CaptureDigital` reads `_decoder?.Tap`, and `_decoder` is
/// a private field set only when the view model opens a sound card. Rather than
/// open one, the field is set by reflection to a real
/// <see cref="CwDecoder"/> whose real <see cref="AudioTap"/> has been fed real
/// samples. Everything downstream of the tap — the decode, the WAV, the sheet —
/// is production code running unmodified. Nothing here opens a window and
/// nothing reaches a transmitter (§0.2).</para>
/// </remarks>
public sealed class ThePressActuallyWritesItsCaptureTests
{
    private const int Rate = 12000;

    private const float PlacedAtHz = 1240;

    private readonly ITestOutputHelper _output;

    public ThePressActuallyWritesItsCaptureTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **THE ONE THE TASK EXISTS FOR.** A press over audio leaves a folder, a
    /// WAV that reads back, and a sheet with unit 233's blocks in it.
    /// </summary>
    [Fact]
    public void APressOverAudioLeavesAWavAndASheetOnDisk()
    {
        var was = MainWindowViewModel.CaptureFolder;
        var temporary = Path.Combine(
            Path.GetTempPath(), "hamlet-unit234-cap-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            MainWindowViewModel.CaptureFolder = temporary;

            var model = new MainWindowViewModel(new AppSettings(), null);
            var kept = OneSlotInThirtySeconds();
            GiveItATapHolding(model, kept);

            var digital = Path.Combine(temporary, "digital");
            Assert.False(Directory.Exists(digital));

            model.CaptureDigitalCommand.Execute(null);

            // **THE FOLDER IS CREATED BY THE PRESS**, which is the fact the
            // owner's machine has never demonstrated.
            Assert.True(Directory.Exists(digital),
                "the press did not create captures\\digital. StatusText was: "
                + model.StatusText);

            var wav = Assert.Single(Directory.GetFiles(digital, "ft8-*.wav"));
            var sheet = Assert.Single(Directory.GetFiles(digital, "ft8-*.txt"));

            _output.WriteLine("MEASURED wav   " + Path.GetFileName(wav)
                + "  " + new FileInfo(wav).Length + " bytes");
            _output.WriteLine("MEASURED sheet " + Path.GetFileName(sheet)
                + "  " + new FileInfo(sheet).Length + " bytes");
            _output.WriteLine("MEASURED status " + model.StatusText);

            // The two names are one press and pair up by stamp.
            Assert.Equal(
                Path.GetFileNameWithoutExtension(wav),
                Path.GetFileNameWithoutExtension(sheet));

            // **THE AUDIO SURVIVES THE ROUND TRIP.** A WAV that is written but
            // unreadable is the same failure as no WAV, one folder listing later.
            var readBack = WavAudio.Read(wav);
            Assert.Equal(kept.SampleRate, readBack.SampleRate);
            Assert.Equal(kept.Samples.Length, readBack.Samples.Length);

            _output.WriteLine("MEASURED wav reads back "
                + readBack.Samples.Length + " samples at " + readBack.SampleRate + " Hz");

            var text = File.ReadAllText(sheet);
            Assert.NotEmpty(text);

            // Unit 226's blocks, still there.
            Assert.Contains("windowFrom", text);
            Assert.Contains("sampleRate", text);
            Assert.Contains("trimmed", text);

            // **UNIT 233'S THREE BLOCKS**: the path the audio came through, the
            // slot geometry, and the per-slot census.
            Assert.Contains("device", text);
            Assert.Contains("audioIsReal", text);
            Assert.Contains("windowsMuted", text);
            Assert.Contains("slotGrid", text);
            Assert.Contains("wholeSlots", text);
            Assert.Contains("census", text);
            Assert.Contains("refusal", text);

            _output.WriteLine("MEASURED sheet lines " + text.Split('\n').Length);
        }
        finally
        {
            MainWindowViewModel.CaptureFolder = was;

            try
            {
                Directory.Delete(temporary, recursive: true);
            }
            catch (Exception)
            {
                // Test cleanup only.
            }
        }

        // **AND THE STATIC IS BACK**, asserted rather than assumed.
        Assert.Equal(was, MainWindowViewModel.CaptureFolder);
    }

    /// <summary>
    /// The sheet's census block carries the slot the press was holding, so a file
    /// opened the next morning says how far the candidates got.
    /// </summary>
    [Fact]
    public void TheSheetCarriesWhatTheSlotActuallyDid()
    {
        var was = MainWindowViewModel.CaptureFolder;
        var temporary = Path.Combine(
            Path.GetTempPath(), "hamlet-unit234-cap-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            MainWindowViewModel.CaptureFolder = temporary;

            var model = new MainWindowViewModel(new AppSettings(), null);
            GiveItATapHolding(model, OneSlotInThirtySeconds());

            model.CaptureDigitalCommand.Execute(null);

            var sheet = Assert.Single(
                Directory.GetFiles(Path.Combine(temporary, "digital"), "ft8-*.txt"));

            var text = File.ReadAllText(sheet);

            foreach (var line in text.Split('\n')
                .Where(l => l.Contains("census") || l.TrimStart().StartsWith("slot ")))
            {
                _output.WriteLine("MEASURED " + line.TrimEnd());
            }

            // The reader ran, the clock was measured, so slots were cut and the
            // sheet says how far each one's candidates got. **What is not
            // permitted is a blank.**
            var refusalLine = Assert.Single(
                text.Split('\n'), l => l.StartsWith("refusal"));

            Assert.EndsWith("none", refusalLine.TrimEnd());
            Assert.Contains("slots, counts below", text);
            Assert.Contains("candidates", text);
            Assert.Contains("top Costas match counts", text);
        }
        finally
        {
            MainWindowViewModel.CaptureFolder = was;

            try
            {
                Directory.Delete(temporary, recursive: true);
            }
            catch (Exception)
            {
                // Test cleanup only.
            }
        }

        Assert.Equal(was, MainWindowViewModel.CaptureFolder);
    }

    /// <summary>Thirty seconds of ring with one whole transmission inside it.</summary>
    private static MonoAudio OneSlotInThirtySeconds()
    {
        var packed = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", packed));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(packed), Rate, PlacedAtHz);

        var samples = new float[Rate * 30];
        slot.CopyTo(samples.AsSpan(13 * Rate));

        return new MonoAudio(Rate, samples);
    }

    /// <summary>
    /// Put a real decoder with a real tap holding real samples behind the view
    /// model, the way an open sound card would.
    /// </summary>
    /// <remarks>
    /// **THE CLOCK IS SET HERE ON PURPOSE.** The view model measures its offset
    /// with an SNTP query on a background task, and until that returns
    /// `ClockOffset` is Unknown — which makes `Ft8SlotCutter` refuse and cut
    /// nothing. Left to the race, this class's first run refused and its second
    /// decoded six candidates. Fixing the offset makes the branch a decision
    /// rather than a coin toss, and it is the branch worth watching: the one
    /// where the press has slots to write a census about.
    /// </remarks>
    private static void GiveItATapHolding(MainWindowViewModel model, MonoAudio audio)
    {
        model.ClockOffset = new ClockOffset(0, DateTime.UtcNow);

        var decoder = new CwDecoder(audio.SampleRate);
        decoder.Tap.Take(audio.Samples, audio.SampleRate);

        var field = typeof(MainWindowViewModel).GetField(
            "_decoder", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);
        field!.SetValue(model, decoder);

        // The seam is only worth using if it actually reached the tap.
        Assert.NotNull(decoder.Tap.Snapshot());
    }
}
