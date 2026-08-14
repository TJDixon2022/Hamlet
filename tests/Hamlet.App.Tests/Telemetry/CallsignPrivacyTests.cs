using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// The privacy guarantee of HM-DEC-018, re-proved now that HM-DEC-019 puts
/// the callsign in the same file as the telemetry switches.
/// </summary>
public sealed class CallsignPrivacyTests : IDisposable
{
    /// <summary>Every public event-writing method on <see cref="AppEvents"/>.
    /// If this number moves, a new event was added and the walk below has to
    /// grow with it — that is the point.</summary>
    private const int ExpectedEventMethodCount = 38;

    private const string Callsign = "KC3QIS";
    // "Timothy", not "Tim": a three-letter needle matches "timer", which is a
    // legitimate trigger value, and the test would fail on its own search
    // string rather than on a leak.
    private const string OperatorName = "Timothy";
    private const string Location = "Pittsburgh, PA";
    private const string Grid = "EN90";

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    /// <remarks>
    /// Proves HM-DEC-018 and HM-DEC-019: with a full operator profile loaded,
    /// every telemetry event the shell can emit is written, and not one line
    /// contains the callsign, the operator's name, their location or their
    /// grid square. This is the rule that a profile sharing settings.json with
    /// the telemetry switches makes easy to break by accident.
    /// </remarks>
    [Fact]
    public void NoTelemetryLine_ContainsAnyProfileValue()
    {
        var settings = new AppSettings();
        settings.Operator.Callsign = Callsign;
        settings.Operator.OperatorName = OperatorName;
        settings.Operator.Location = Location;
        settings.Operator.GridSquare = Grid;

        var telemetry = new JsonlTelemetry(
            _folder, "test", settings.IsTelemetryEnabled, 50L * 1024 * 1024);
        WriteEveryEvent(telemetry);
        telemetry.Dispose();

        var lines = ReadAllLines();

        Assert.NotEmpty(lines);
        Assert.All(lines, line =>
        {
            Assert.DoesNotContain(Callsign, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(OperatorName, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Location, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Grid, line, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <remarks>
    /// Proves the walk above is complete: every public method on AppEvents is
    /// exercised. A new event added without a line here fails this test rather
    /// than quietly escaping the privacy check.
    /// </remarks>
    [Fact]
    public void EveryAppEvent_IsCoveredByThePrivacyWalk()
    {
        var methods = typeof(AppEvents).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(ExpectedEventMethodCount, methods.Length);
    }

    /// <remarks>
    /// Proves the guarantee structurally as well as behaviorally: the
    /// telemetry payload builders cannot read the profile, because they are
    /// never handed one. No AppEvents method takes an AppSettings or an
    /// OperatorProfile, and the class holds no state to reach it through.
    /// </remarks>
    [Fact]
    public void AppEvents_CannotSeeTheProfile()
    {
        var methods = typeof(AppEvents).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.All(methods, m => Assert.All(m.GetParameters(), p =>
        {
            Assert.NotEqual(typeof(AppSettings), p.ParameterType);
            Assert.NotEqual(typeof(OperatorProfile), p.ParameterType);
        }));

        Assert.Empty(typeof(AppEvents).GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
    }

    /// <remarks>
    /// Proves HM-DEC-019: the About box's copyable diagnostics block — which
    /// is written to end up in a public issue tracker — carries the build,
    /// the runtime and the session id, and none of the operator's identity.
    /// </remarks>
    [Fact]
    public void CopiedDiagnostics_CarryNoProfileValues()
    {
        var settings = new AppSettings();
        settings.Operator.Callsign = Callsign;
        settings.Operator.OperatorName = OperatorName;
        settings.Operator.Location = Location;
        settings.Operator.GridSquare = Grid;

        using var telemetry = new JsonlTelemetry(_folder, "test", _ => true);
        var about = new AboutViewModel(settings, telemetry);

        Assert.Contains(telemetry.SessionId, about.DiagnosticsText, StringComparison.Ordinal);
        Assert.DoesNotContain(Callsign, about.DiagnosticsText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(OperatorName, about.DiagnosticsText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Location, about.DiagnosticsText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Grid, about.DiagnosticsText, StringComparison.OrdinalIgnoreCase);

        // The byline is displayed in the window and must NOT be in the paste.
        Assert.Equal("by Timothy, KC3QIS", about.Byline);
    }

    /// <summary>Every event the shell can emit, once each.</summary>
    private static void WriteEveryEvent(ITelemetry telemetry)
    {
        AppEvents.AppStart(telemetry);
        AppEvents.AppStop(telemetry);
        AppEvents.AboutOpened(telemetry);
        AppEvents.DiagnosticsCopied(telemetry);
        AppEvents.SettingsOpened(telemetry);
        AppEvents.ProfileEdited(telemetry, "callsign");
        AppEvents.ConnectOk(telemetry, "COM3", "IC-7300");
        AppEvents.ConnectFailed(telemetry, "COM3", "IC-7300", "no_response");
        AppEvents.BandChanged(telemetry, "40 m");
        AppEvents.TuneRequested(telemetry, 7_030_000, "story_or_spot");
        AppEvents.NeighborhoodClicked(telemetry, "The QRP corner");
        AppEvents.ModeCardOpened(telemetry, "FT8");
        AppEvents.SpotsRefreshed(telemetry, "timer", 7, 1);
        AppEvents.SpotTimerChanged(telemetry, running: true, intervalMinutes: 5);
        AppEvents.PanelToggled(telemetry, PanelKeys.Spots, expanded: false);

        // HM-DEC-024 sends the callsign to POTA, SOTA and RBN over the wire.
        // These four events exist because of that work, and they are walked
        // here for exactly that reason: the rule did not change, so the proof
        // has to grow to cover the new ground.
        AppEvents.SourceToggled(telemetry, "POTA", enabled: true);
        AppEvents.SourceUnhealthy(telemetry, "RBN", "Degraded");
        AppEvents.LeadCardBuilt(telemetry, hasSuggestion: true, score: 88);
        AppEvents.MapDotTuned(telemetry, 7_032_000);

        // HM-DEC-047 puts the same spots on the dial tape, and clicking one is
        // its own gesture with its own event. A spot names a station, so a new
        // way of clicking a spot is new ground this walk has to cover.
        AppEvents.TapeMarkerTuned(telemetry, 7_032_000);

        // HM-DEC-048 adds audio input, and a capture device's NAME routinely
        // carries a computer's name or a person's. The event records only
        // whether Hamlet's guess was kept, and this walk is what keeps that
        // provable rather than asserted.
        AppEvents.AudioDeviceChosen(telemetry, looksLikeRadio: true);

        // The decoder's own run parameters, which §0.0.1 asks for by name. The
        // walk covering it is what keeps "never message content" provable: a
        // transcript names two stations who did not agree to be in this file.
        AppEvents.DecoderStarted(
            telemetry, simulated: true, sampleRate: 8_000, pitchHz: 600);

        // HM-DEC-050 puts the radio's whole state on a screen. What it is tuned
        // to and how its filters are set are the operator's business, so the
        // event carries a count and nothing else, and this walk is what keeps
        // that provable.
        AppEvents.RigDiagnosticsOpened(telemetry, knownCount: 18);

        // HM-DEC-026 and HM-DEC-027 added the training radio and the field
        // guide's audio. Neither touches the profile, and both join the walk
        // so that stays provable rather than assumed.
        AppEvents.SpectrumSourceChanged(telemetry, "training", "40 m", simulated: true);
        AppEvents.ModeSamplePlayed(telemetry, "Cw", 18);

        // HM-DEC-028 sends the callsign to a lookup service to learn the
        // license class. That is the same bargain as HM-DEC-024's spot feeds:
        // the callsign goes over the wire and still never into this file, so
        // these five events join the walk that proves it.
        AppEvents.LicenseClassResolved(telemetry, "General", "callook.info");
        AppEvents.LicenseClassMismatch(telemetry, "Extra", "General");
        AppEvents.LicenseClassLookupFailed(telemetry, "Unavailable");
        AppEvents.UpgradeLadderOpened(telemetry, "Technician");
        AppEvents.TransmitGuardToggled(telemetry, enabled: true);

        // HM-DEC-045 keeps a local history of every spot Hamlet sees, which is
        // the first time the app has written spot data to disk at all. Both
        // events carry counts only. A spot names a station, and the store is
        // one folder away from this file, so this walk covering them is the
        // thing that keeps the two apart provable rather than assumed.
        AppEvents.SpotHistoryPruned(telemetry, 120, 400);
        AppEvents.SpotHistoryUnavailable(telemetry);
        AppEvents.ModeFollowed(telemetry, "Usb", dataMode: true, "Confirmed");
        AppEvents.SpotLensChosen(telemetry, "WhatsNew");
        AppEvents.SpotFamilyToggled(telemetry, "Cw", isOn: true);
        AppEvents.FavoriteSaved(telemetry, "40 m");
        AppEvents.FavoriteRemoved(telemetry, "40 m");
        AppEvents.FavoriteTuned(telemetry, "40 m");
    }

    private string[] ReadAllLines()
        => Directory.Exists(_folder)
            ? Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToArray()
            : Array.Empty<string>();
}
