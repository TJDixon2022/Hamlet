using System.Text.Json;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// The moment listening starts now says how the capture device was chosen — never
/// which one it is (unit 236).
/// </summary>
/// <remarks>
/// <para>**THE SECOND HALF OF THE FORK THE BENCH CHECK OF 2026-09-03 DIED ON.** A
/// slot census that said nothing about the audio was one hole; the other was that
/// Hamlet picks the sound card for the operator on every launch and wrote down
/// neither the pick nor the reason for it. Unit 235 measured that his settings hold
/// no remembered device at all, so the pick has always been a guess.</para>
/// <para>**THE BRANCH AND NEVER THE DEVICE** (HM-DEC-018). A device name can carry
/// a computer's name, a person's name or the model of somebody's headset, and
/// <c>AppEvents.AudioDeviceChosen</c> has recorded a boolean rather than a name
/// since it was written. This follows it.</para>
/// <para>**NO HARDWARE IS TOUCHED** (`ARBITER.md` §6). Nothing here opens a capture
/// device or asks this machine what it has.</para>
/// </remarks>
public sealed class ListenStartSaysWhyThisDeviceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the written line is printed.</param>
    public ListenStartSaysWhyThisDeviceTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** The branch, the codec boolean and how many
    /// devices the machine offered, in the line that already says when listening
    /// started.
    /// </summary>
    [Fact]
    public void TheBranchThatChoseTheDeviceReachesTheRecord()
    {
        var sink = new CapturingTelemetry();

        AppEvents.DecoderStarted(
            sink,
            simulated: false,
            sampleRate: 48_000,
            pitchHz: 600,
            AudioDeviceChoiceReason.SystemDefault,
            looksLikeRadio: false,
            captureDevicesOffered: 3);

        var written = Assert.Single(sink.Events);

        _output.WriteLine("  " + JsonSerializer.Serialize(written.Data));

        Assert.Equal("decoder_started", written.Name);
        Assert.Equal("SystemDefault", written.Data["deviceChoice"]);
        Assert.Equal(false, written.Data["looksLikeRadio"]);
        Assert.Equal(3, written.Data["captureDevicesOffered"]);

        // The three that were always there keep their meaning.
        Assert.Equal(false, written.Data["simulated"]);
        Assert.Equal(48_000, written.Data["sampleRate"]);
        Assert.Equal(600, written.Data["pitchHz"]);
    }

    /// <summary>
    /// **EVERY BRANCH IS WRITABLE**, so none of the five is a state the record
    /// cannot hold.
    /// </summary>
    [Fact]
    public void EveryOneOfTheFiveBranchesCanBeWritten()
    {
        var sink = new CapturingTelemetry();

        foreach (var reason in Enum.GetValues<AudioDeviceChoiceReason>())
        {
            AppEvents.DecoderStarted(
                sink, false, 48_000, 600, reason, false, 2);
        }

        var written = sink.Events
            .Select(e => e.Data["deviceChoice"] as string)
            .ToArray();

        foreach (var line in written)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Equal(
            Enum.GetNames<AudioDeviceChoiceReason>(),
            written);
    }

    /// <summary>
    /// **THE TRAINING RADIO OPENS NO DEVICE, AND THE LINE SAYS NOTHING RATHER THAN
    /// GUESSING** (`CLAUDE.md` §0.0). A branch written beside a source that never
    /// looked at a sound card would be a measurement of a choice nobody made.
    /// </summary>
    [Fact]
    public void ASimulatedSourceRecordsNoBranchAtAll()
    {
        var sink = new CapturingTelemetry();

        AppEvents.DecoderStarted(sink, simulated: true, sampleRate: 8_000, pitchHz: 600);

        var data = Assert.Single(sink.Events).Data;

        _output.WriteLine("  " + JsonSerializer.Serialize(data));

        Assert.Null(data["deviceChoice"]);
        Assert.Null(data["looksLikeRadio"]);
        Assert.Null(data["captureDevicesOffered"]);
        Assert.Equal(true, data["simulated"]);
    }

    /// <summary>
    /// **AND NO DEVICE NAME OR ID CAN REACH THE LINE** (HM-DEC-018), which the
    /// signature enforces rather than this test remembering: there is no parameter
    /// that could carry one.
    /// </summary>
    [Fact]
    public void NothingOnTheLineCanHoldADeviceNameOrId()
    {
        var method = typeof(AppEvents).GetMethod(nameof(AppEvents.DecoderStarted));

        Assert.NotNull(method);

        // Not one string parameter, so a name has no way in.
        Assert.DoesNotContain(
            method!.GetParameters(), p => p.ParameterType == typeof(string));

        Assert.DoesNotContain(
            method.GetParameters(), p => p.ParameterType == typeof(AudioDevice));
    }

    private sealed record Written(
        TelemetryCategory Category,
        string Name,
        IReadOnlyDictionary<string, object?> Data,
        TelemetryLevel Level);

    private sealed class CapturingTelemetry : ITelemetry
    {
        private readonly List<Written> _events = new();

        public IReadOnlyList<Written> Events => _events;

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => _events.Add(new Written(
                category,
                eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal),
                level));
    }
}
