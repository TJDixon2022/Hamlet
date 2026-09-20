using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Settings;

/// <summary>
/// **A settings file written by an older Hamlet still loads whole.**
/// </summary>
/// <remarks>
/// <para>**WHY THIS EXISTS.** Between 1.13.30 (2026-09-14, PSK31 transmitting) and
/// 1.13.48 (2026-09-19) the transmit audio device went missing from the operator's
/// settings and FT8 sends stopped after read-back. He found the cause himself -
/// *"the settings lost the listing setting"* - re-chose the USB Audio CODEC, and
/// FT8 transmitted. **Nothing in the tree tested that an old settings file still
/// loads whole**, which is the hole this class fills (PHASE_PLAN.md R33, criterion
/// 0.2).</para>
/// <para>**THE THREE FIXTURES ARE RECONSTRUCTED FROM THE TREE, NOT INVENTED.** The
/// persisted shape of <see cref="AppSettings"/> at 681d45c8 (the commit that set
/// 1.13.30), at d66a6ace (1.13.40) and at HEAD is one and the same - `git diff`
/// over `src/Hamlet.App/Settings/` between those commits is empty - so the three
/// files below carry the same key set with different values, and the only key that
/// separates them is `Psk31AlcReference`, which this unit adds. **That identity is
/// itself the answer to criterion 0.1**: no commit in that window changed the
/// settings model, its loader, its migrations or the transmit-device picker, so the
/// loader is not where the device was lost.</para>
/// <para>**THE FILES ARE JSON TEXT AND NOT A ROUND-TRIP OF TODAY'S CLASS.** A
/// fixture built by serializing the current type would agree with the current type
/// by construction and could never catch a renamed key, which is the whole class of
/// fault being guarded.</para>
/// </remarks>
public sealed class TheSettingsSurviveAnUpgradeTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

    public TheSettingsSurviveAnUpgradeTests(ITestOutputHelper output)
        => _output = output;

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

    private string Write(string name, string json)
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, name);
        File.WriteAllText(path, json);
        return path;
    }

    // ------------------------------------------------------------------
    // The fixtures.
    // ------------------------------------------------------------------

    /// <summary>
    /// A settings file as 1.13.30 wrote one - every key the model carried at
    /// 681d45c8, and none it did not.
    /// </summary>
    private const string At_1_13_30 = """
        {
          "WindowX": 120,
          "WindowY": 64,
          "WindowWidth": 1100,
          "WindowHeight": 780,
          "WindowMaximized": false,
          "LastPort": "COM4",
          "ReconnectOnStartup": true,
          "ModeFollowsTheMap": true,
          "Favorites": [
            {
              "FrequencyHz": 14070150,
              "Name": "14.070, PSK31 corner",
              "Mode": "USB-D",
              "BandName": "20 m",
              "Neighborhood": "PSK31 corner",
              "SavedUtc": "2026-09-10T18:00:00Z",
              "Note": "where the Belgians were"
            }
          ],
          "Recent": [
            {
              "FrequencyHz": 14074000,
              "Station": "W1AW",
              "Mode": "USB-D",
              "BandName": "20 m",
              "Neighborhood": "FT8 city",
              "VisitedUtc": "2026-09-13T22:15:00Z",
              "Visits": 4,
              "StationSource": "decode"
            }
          ],
          "HasMeasuredSwr": true,
          "LastBand": "20 m",
          "Operator": {
            "Callsign": "KC3QIS",
            "CallsignSource": "EnteredByOperator",
            "CallsignSourceName": "the operator",
            "CallsignSetOn": "2026-08-13",
            "CallsignVerifiedAs": "KC3QIS",
            "OperatorName": "Tim",
            "Location": "Pittsburgh, PA",
            "GridSquare": "EN90xk",
            "Latitude": 40.44,
            "Longitude": -79.99,
            "GridSquareSource": "EnteredByOperator",
            "GridSquareSourceName": "the operator",
            "GridSquareSetOn": "2026-08-13",
            "GridSquareVerifiedAs": "EN90xk",
            "LicenseClass": "General",
            "LicenseClassSource": "LookedUp",
            "LicenseClassSourceName": "callook.info",
            "LicenseClassSetOn": "2026-08-13",
            "LicenseClassVerifiedAs": "General"
          },
          "SpotLens": "WhatsNew",
          "SpotsLastLookedUtc": "2026-09-13T22:00:00Z",
          "SpotFamilies": [ "Digital", "Morse" ],
          "SpotRefreshMinutes": 10,
          "PanelExpanded": { "waterfall": false, "spots": true, "guide": true },
          "SourceEnabled": { "POTA": true, "SOTA": false },
          "TelemetryCategories": { "Decode": false, "Transmit": true },
          "TelemetryMaxMegabytes": 75,
          "RestrictTransmitToPrivileges": true,
          "DistanceUnits": "Miles",
          "LastBylineIndex": 2,
          "AudioInputDeviceId": "{0.0.1.00000000}.{usb-audio-codec-capture}",
          "AudioOutputDeviceId": "{0.0.0.00000000}.{usb-audio-codec-render}",
          "TransmitDrivePeak": 0.42,
          "ContactBadgeAnnounced": 3,
          "ContactModeFirstsAnnounced": [ "FT8", "PSK31" ],
          "AchievementGroupsAnnounced": [ "Modes" ],
          "AchievementsUnseen": true,
          "HasTunedByWheel": true,
          "ShowKeyingSweep": false,
          "UseJointDecoder": true,
          "UseReferenceDecoder": false,
          "CwPitchHz": 650,
          "CopySpeedWpm": 18,
          "ActivationLifetimeMinutes": 45,
          "SkimmerLifetimeMinutes": 25,
          "ContestLifetimeMinutes": 150,
          "LastOperatingMode": "Digital",
          "LastDigitalSubMode": "PSK31",
          "DecodedFilter": "CqOnly",
          "DecodedShowCq": true,
          "DecodedShowMine": false,
          "DecodedNewestFirst": true,
          "CompareWithThePort": false
        }
        """;

    /// <summary>
    /// A settings file as 1.13.40 wrote one. Same key set as 1.13.30 - the model
    /// did not move between them - with different values throughout, so a test
    /// that passed by reading the other file's numbers cannot pass by accident.
    /// </summary>
    private const string At_1_13_40 = """
        {
          "WindowX": 300,
          "WindowY": 180,
          "WindowWidth": 1440,
          "WindowHeight": 900,
          "WindowMaximized": true,
          "LastPort": "COM7",
          "ReconnectOnStartup": false,
          "ModeFollowsTheMap": false,
          "Favorites": [],
          "Recent": [],
          "HasMeasuredSwr": false,
          "LastBand": "40 m",
          "Operator": {
            "Callsign": "KC3QIS",
            "CallsignSource": "LookedUp",
            "CallsignSourceName": "callook.info",
            "CallsignSetOn": "2026-09-01",
            "CallsignVerifiedAs": "KC3QIS",
            "OperatorName": "Tim",
            "Location": "Pittsburgh, Pennsylvania",
            "GridSquare": "EN90xj",
            "Latitude": 40.43,
            "Longitude": -79.98,
            "GridSquareSource": "LookedUp",
            "GridSquareSourceName": "callook.info",
            "GridSquareSetOn": "2026-09-01",
            "GridSquareVerifiedAs": "EN90xj",
            "LicenseClass": "Extra",
            "LicenseClassSource": "EnteredByOperator",
            "LicenseClassSourceName": "the operator",
            "LicenseClassSetOn": "2026-09-01",
            "LicenseClassVerifiedAs": "Extra"
          },
          "SpotLens": "Nearby",
          "SpotsLastLookedUtc": "2026-09-16T01:30:00Z",
          "SpotFamilies": [ "Voice" ],
          "SpotRefreshMinutes": 0,
          "PanelExpanded": { "waterfall": true, "spots": false, "story": false },
          "SourceEnabled": { "POTA": false, "SOTA": true },
          "TelemetryCategories": { "Decode": true, "Transmit": false },
          "TelemetryMaxMegabytes": 20,
          "RestrictTransmitToPrivileges": false,
          "DistanceUnits": "Kilometers",
          "LastBylineIndex": 5,
          "AudioInputDeviceId": "{0.0.1.00000000}.{another-capture-endpoint}",
          "AudioOutputDeviceId": "{0.0.0.00000000}.{another-render-endpoint}",
          "TransmitDrivePeak": 0.18,
          "ContactBadgeAnnounced": 7,
          "ContactModeFirstsAnnounced": [ "FT8", "FT4", "PSK31", "CW" ],
          "AchievementGroupsAnnounced": [ "Modes", "Distance" ],
          "AchievementsUnseen": false,
          "HasTunedByWheel": false,
          "ShowKeyingSweep": true,
          "UseJointDecoder": false,
          "UseReferenceDecoder": true,
          "CwPitchHz": 700,
          "CopySpeedWpm": 25,
          "ActivationLifetimeMinutes": 90,
          "SkimmerLifetimeMinutes": 10,
          "ContestLifetimeMinutes": 240,
          "LastOperatingMode": "CW",
          "LastDigitalSubMode": "FT4",
          "DecodedFilter": "Mine",
          "DecodedShowCq": false,
          "DecodedShowMine": true,
          "DecodedNewestFirst": false,
          "CompareWithThePort": true
        }
        """;

    /// <summary>
    /// A settings file as HEAD writes one: 1.13.40's key set plus
    /// `Psk31AlcReference`, the field this unit adds.
    /// </summary>
    private const string AtHead = """
        {
          "WindowX": 0,
          "WindowY": 0,
          "WindowWidth": 1280,
          "WindowHeight": 820,
          "WindowMaximized": false,
          "LastPort": "COM3",
          "ReconnectOnStartup": true,
          "ModeFollowsTheMap": true,
          "Favorites": [],
          "Recent": [],
          "HasMeasuredSwr": true,
          "LastBand": "20 m",
          "Operator": {
            "Callsign": "KC3QIS",
            "CallsignSource": "EnteredByOperator",
            "CallsignSourceName": "the operator",
            "CallsignSetOn": "2026-09-19",
            "CallsignVerifiedAs": "KC3QIS",
            "OperatorName": "Tim",
            "Location": "Pittsburgh, PA",
            "GridSquare": "EN90xk",
            "Latitude": 40.44,
            "Longitude": -79.99,
            "GridSquareSource": "EnteredByOperator",
            "GridSquareSourceName": "the operator",
            "GridSquareSetOn": "2026-09-19",
            "GridSquareVerifiedAs": "EN90xk",
            "LicenseClass": "General",
            "LicenseClassSource": "LookedUp",
            "LicenseClassSourceName": "callook.info",
            "LicenseClassSetOn": "2026-09-19",
            "LicenseClassVerifiedAs": "General"
          },
          "SpotLens": "WhatsNew",
          "SpotsLastLookedUtc": "2026-09-19T23:45:00Z",
          "SpotFamilies": [ "Digital" ],
          "SpotRefreshMinutes": 5,
          "PanelExpanded": { "psk31": true, "olivia": true },
          "SourceEnabled": { "POTA": true, "SOTA": true },
          "TelemetryCategories": { "Decode": true, "Transmit": true },
          "TelemetryMaxMegabytes": 50,
          "RestrictTransmitToPrivileges": true,
          "DistanceUnits": "Miles",
          "LastBylineIndex": 0,
          "AudioInputDeviceId": "{0.0.1.00000000}.{usb-audio-codec-capture}",
          "AudioOutputDeviceId": "{0.0.0.00000000}.{usb-audio-codec-render}",
          "TransmitDrivePeak": 0.35,
          "ContactBadgeAnnounced": 9,
          "ContactModeFirstsAnnounced": [ "FT8", "FT4", "PSK31", "Olivia" ],
          "AchievementGroupsAnnounced": [ "Modes" ],
          "AchievementsUnseen": false,
          "HasTunedByWheel": true,
          "ShowKeyingSweep": false,
          "UseJointDecoder": true,
          "UseReferenceDecoder": false,
          "CwPitchHz": 600,
          "CopySpeedWpm": 13,
          "ActivationLifetimeMinutes": 60,
          "SkimmerLifetimeMinutes": 20,
          "ContestLifetimeMinutes": 180,
          "LastOperatingMode": "Digital",
          "LastDigitalSubMode": "Olivia",
          "DecodedFilter": "Everything",
          "DecodedShowCq": false,
          "DecodedShowMine": false,
          "DecodedNewestFirst": true,
          "CompareWithThePort": false,
          "Psk31AlcReference": {
            "Reading": 94.5,
            "TakenUtc": "2026-09-19T23:40:12Z",
            "Mode": "FT8"
          }
        }
        """;

    public static TheoryData<string, string> EveryFixture => new()
    {
        { "1.13.30", At_1_13_30 },
        { "1.13.40", At_1_13_40 },
        { "HEAD", AtHead },
    };

    // ------------------------------------------------------------------
    // 0.2 - nothing the file carries is dropped.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every key the file carries is still there after a load and a save.**
    /// </summary>
    /// <remarks>
    /// **IT WALKS THE FILE RATHER THAN A LIST OF SEVEN NAMES.** The seven named in
    /// criterion 0.2 are asserted by value below; this one asserts the whole file,
    /// so a field dropped years from now by a rewrite that missed one fails here
    /// without anybody having thought to add it.
    /// </remarks>
    [Theory]
    [MemberData(nameof(EveryFixture))]
    public void NothingTheFileCarriesIsDropped(string version, string json)
    {
        var path = Write("settings-" + version + ".json", json);

        var loaded = SettingsStore.LoadFrom(path);

        var saved = Path.Combine(_folder, "written-back-" + version + ".json");
        SettingsStore.SaveTo(loaded, saved);

        using var before = JsonDocument.Parse(json);
        using var after = JsonDocument.Parse(File.ReadAllText(saved));

        var lost = new List<string>();
        Compare(before.RootElement, after.RootElement, "", lost);

        _output.WriteLine(version + ": " + lost.Count + " value(s) lost");
        foreach (var name in lost)
        {
            _output.WriteLine("  lost: " + name);
        }

        Assert.Empty(lost);
    }

    /// <summary>Record every leaf in <paramref name="before"/> that is absent
    /// from or changed in <paramref name="after"/>.</summary>
    private static void Compare(
        JsonElement before, JsonElement after, string path, List<string> lost)
    {
        if (before.ValueKind == JsonValueKind.Object)
        {
            if (after.ValueKind != JsonValueKind.Object)
            {
                lost.Add(path + " (was an object, came back " + after.ValueKind + ")");
                return;
            }

            foreach (var property in before.EnumerateObject())
            {
                var here = path.Length == 0
                    ? property.Name
                    : path + "." + property.Name;

                if (!after.TryGetProperty(property.Name, out var mirror))
                {
                    lost.Add(here);
                    continue;
                }

                Compare(property.Value, mirror, here, lost);
            }

            return;
        }

        if (before.ValueKind == JsonValueKind.Array)
        {
            if (after.ValueKind != JsonValueKind.Array
                || after.GetArrayLength() != before.GetArrayLength())
            {
                lost.Add(path + " (array length changed)");
                return;
            }

            var index = 0;
            foreach (var item in before.EnumerateArray())
            {
                Compare(item, after[index], path + "[" + index + "]", lost);
                index++;
            }

            return;
        }

        // A leaf. Numbers are compared as numbers so 0.42 and 0.4200 agree;
        // everything else on its text.
        var same = before.ValueKind == JsonValueKind.Number
                   && after.ValueKind == JsonValueKind.Number
            ? Math.Abs(before.GetDouble() - after.GetDouble()) < 1e-9
            : string.Equals(
                before.ToString(), after.ToString(), StringComparison.Ordinal);

        if (!same)
        {
            lost.Add(path + " (" + before + " -> " + after + ")");
        }
    }

    /// <summary>
    /// **The seven values criterion 0.2 names, read back from each fixture.**
    /// </summary>
    [Fact]
    public void TheSevenNamedValuesLoadFromTheFileWrittenAt_1_13_30()
    {
        var loaded = SettingsStore.LoadFrom(
            Write("settings-1.13.30.json", At_1_13_30));

        Assert.Equal(
            "{0.0.0.00000000}.{usb-audio-codec-render}", loaded.AudioOutputDeviceId);
        Assert.Equal(
            "{0.0.1.00000000}.{usb-audio-codec-capture}", loaded.AudioInputDeviceId);
        Assert.Equal("EN90xk", loaded.Operator.GridSquare);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Equal(LicenseClass.General, loaded.Operator.LicenseClass);
        Assert.Equal(0.42f, loaded.TransmitDrivePeak, 5);

        // The ALC reference is the one field a 1.13.30 file cannot carry,
        // because it did not exist. Absent, not invented (0.0).
        Assert.Null(loaded.Psk31AlcReference);
    }

    /// <summary>
    /// **The seven values criterion 0.2 names, read back from the 1.13.40 file.**
    /// </summary>
    [Fact]
    public void TheSevenNamedValuesLoadFromTheFileWrittenAt_1_13_40()
    {
        var loaded = SettingsStore.LoadFrom(
            Write("settings-1.13.40.json", At_1_13_40));

        Assert.Equal(
            "{0.0.0.00000000}.{another-render-endpoint}", loaded.AudioOutputDeviceId);
        Assert.Equal(
            "{0.0.1.00000000}.{another-capture-endpoint}", loaded.AudioInputDeviceId);
        Assert.Equal("EN90xj", loaded.Operator.GridSquare);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Equal(LicenseClass.Extra, loaded.Operator.LicenseClass);
        Assert.Equal(0.18f, loaded.TransmitDrivePeak, 5);
        Assert.Null(loaded.Psk31AlcReference);
    }

    /// <summary>
    /// **The seven values criterion 0.2 names, read back from a HEAD file -
    /// the ALC reference among them.**
    /// </summary>
    [Fact]
    public void TheSevenNamedValuesLoadFromTheFileWrittenAtHead()
    {
        var loaded = SettingsStore.LoadFrom(Write("settings-head.json", AtHead));

        Assert.Equal(
            "{0.0.0.00000000}.{usb-audio-codec-render}", loaded.AudioOutputDeviceId);
        Assert.Equal(
            "{0.0.1.00000000}.{usb-audio-codec-capture}", loaded.AudioInputDeviceId);
        Assert.Equal("EN90xk", loaded.Operator.GridSquare);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Equal(LicenseClass.General, loaded.Operator.LicenseClass);
        Assert.Equal(0.35f, loaded.TransmitDrivePeak, 5);

        var reference = Assert.IsType<LearnedAlcReference>(loaded.Psk31AlcReference);

        Assert.Equal(94.5, reference.Reading, 5);
        Assert.Equal("FT8", reference.Mode);
        Assert.Equal(
            new DateTime(2026, 9, 19, 23, 40, 12, DateTimeKind.Utc),
            reference.TakenUtc.ToUniversalTime());
    }

    // ------------------------------------------------------------------
    // A field the file lacks takes its default, and is written back.
    // ------------------------------------------------------------------

    /// <summary>
    /// **A field the file has never heard of takes its default and is written
    /// back, and the values around it are untouched.**
    /// </summary>
    /// <remarks>
    /// The 1.13.30 file has no `Psk31AlcReference`, because the field did not
    /// exist when it was written. **That is the upgrade this class is named for**:
    /// the missing field is the new one, everything else is the operator's.
    /// </remarks>
    [Fact]
    public void AFieldTheFileLacksTakesItsDefaultAndTheRestIsUntouched()
    {
        var path = Write("settings-1.13.30.json", At_1_13_30);

        var loaded = SettingsStore.LoadFrom(path);

        // The new field is absent, which for this one is its default.
        Assert.Null(loaded.Psk31AlcReference);

        // And the file it carried is still whole.
        Assert.Equal(
            "{0.0.0.00000000}.{usb-audio-codec-render}", loaded.AudioOutputDeviceId);
        Assert.Equal(650, loaded.CwPitchHz);
        Assert.Equal(18, loaded.CopySpeedWpm);
        Assert.Equal(DistanceUnits.Miles, loaded.DistanceUnits);
        Assert.Equal("20 m", loaded.LastBand);
        Assert.Single(loaded.Favorites);
        Assert.Single(loaded.Recent);

        var written = Path.Combine(_folder, "after-upgrade.json");
        SettingsStore.SaveTo(loaded, written);

        using var document = JsonDocument.Parse(File.ReadAllText(written));

        // **THE DEFAULTS ARE WRITTEN BACK**, so the next load of this file reads
        // them rather than defaulting a second time.
        foreach (var key in new[]
                 {
                     nameof(AppSettings.AudioOutputDeviceId),
                     nameof(AppSettings.TransmitDrivePeak),
                     nameof(AppSettings.CwPitchHz),
                     nameof(AppSettings.CopySpeedWpm),
                     nameof(AppSettings.DistanceUnits),
                     nameof(AppSettings.SpotRefreshMinutes),
                 })
        {
            Assert.True(
                document.RootElement.TryGetProperty(key, out _),
                key + " was not written back");
        }
    }

    // ------------------------------------------------------------------
    // The loaded file round-trips.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Load, save, load again, and the second reading equals the first.**
    /// </summary>
    [Theory]
    [MemberData(nameof(EveryFixture))]
    public void TheLoadedFileRoundTrips(string version, string json)
    {
        var path = Write("settings-" + version + ".json", json);

        var once = SettingsStore.LoadFrom(path);

        var written = Path.Combine(_folder, "round-" + version + ".json");
        SettingsStore.SaveTo(once, written);
        var twice = SettingsStore.LoadFrom(written);

        var again = Path.Combine(_folder, "round-again-" + version + ".json");
        SettingsStore.SaveTo(twice, again);

        Assert.Equal(File.ReadAllText(written), File.ReadAllText(again));

        Assert.Equal(once.AudioOutputDeviceId, twice.AudioOutputDeviceId);
        Assert.Equal(once.AudioInputDeviceId, twice.AudioInputDeviceId);
        Assert.Equal(once.Operator.GridSquare, twice.Operator.GridSquare);
        Assert.Equal(once.Operator.Callsign, twice.Operator.Callsign);
        Assert.Equal(once.Operator.LicenseClass, twice.Operator.LicenseClass);
        Assert.Equal(once.TransmitDrivePeak, twice.TransmitDrivePeak);
        Assert.Equal(once.Psk31AlcReference, twice.Psk31AlcReference);
    }

    // ------------------------------------------------------------------
    // One value that cannot be read costs that value and nothing else.
    // ------------------------------------------------------------------

    /// <summary>
    /// **A single unreadable value costs that value alone. It does not cost the
    /// file.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE FAULT THAT LOST THE OPERATOR'S TRANSMIT DEVICE** (work
    /// instruction 369 task 1). `LoadFrom` deserialized the whole file in one call
    /// and caught every exception with `return new AppSettings()`, so **one value
    /// anywhere in `settings.json` that the current build cannot parse discarded
    /// every other value in it** - the transmit device, the receive device, the
    /// grid, the callsign, the class, the drive, the lot. `App.axaml.cs` then saved
    /// those defaults over his file on exit, which is what made the loss permanent
    /// and what *"the settings lost the listing setting"* looked like from his
    /// chair.</para>
    /// <para>**THE CONVERTER'S OWN REMARKS SAW HALF OF IT.** `SettingsStore`
    /// already carries a `JsonStringEnumConverter` because without it a hand-edited
    /// `"General"` threw and *"EVERY setting silently reverts to defaults, which is
    /// a spectacular punishment for a reasonable guess"*. That fixed the one value
    /// it named. **Every other unparseable value still cost the file**, and a
    /// retired enum member, a renamed one or a number where a string is now wanted
    /// all reach the same catch.</para>
    /// <para>**THE FIXTURE IS A REAL SHAPE, NOT A CORRUPTION.** The file below is
    /// well-formed JSON with one enum member that no longer exists - which is
    /// precisely what a settings file becomes when a build retires a name. A
    /// genuinely unparseable file is still defaults, and
    /// `SettingsRoundTripTests.CorruptSettingsFile_YieldsDefaults` still says
    /// so.</para>
    /// </remarks>
    [Fact]
    public void OneUnreadableValueCostsThatValueAndNotTheFile()
    {
        var path = Write("settings.json", """
            {
              "AudioOutputDeviceId": "{0.0.0.00000000}.{usb-audio-codec-render}",
              "AudioInputDeviceId": "{0.0.1.00000000}.{usb-audio-codec-capture}",
              "TransmitDrivePeak": 0.42,
              "CwPitchHz": 650,
              "CopySpeedWpm": 18,
              "LastBand": "20 m",
              "DistanceUnits": "Furlongs",
              "Operator": {
                "Callsign": "KC3QIS",
                "OperatorName": "Tim",
                "GridSquare": "EN90xk",
                "LicenseClass": "General"
              }
            }
            """);

        var loaded = SettingsStore.LoadFrom(path);

        // **THE ONE VALUE NOBODY CAN READ TAKES ITS DEFAULT.**
        Assert.Equal(DistanceUnits.Miles, loaded.DistanceUnits);

        // **AND EVERYTHING ELSE IN THE FILE IS STILL THE OPERATOR'S.**
        Assert.Equal(
            "{0.0.0.00000000}.{usb-audio-codec-render}", loaded.AudioOutputDeviceId);
        Assert.Equal(
            "{0.0.1.00000000}.{usb-audio-codec-capture}", loaded.AudioInputDeviceId);
        Assert.Equal(0.42f, loaded.TransmitDrivePeak, 5);
        Assert.Equal(650, loaded.CwPitchHz);
        Assert.Equal(18, loaded.CopySpeedWpm);
        Assert.Equal("20 m", loaded.LastBand);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Equal("Tim", loaded.Operator.OperatorName);
        Assert.Equal("EN90xk", loaded.Operator.GridSquare);
        Assert.Equal(LicenseClass.General, loaded.Operator.LicenseClass);
    }

    /// <summary>
    /// **An unreadable value inside the profile costs that field alone, and the
    /// transmit device outside it is untouched.**
    /// </summary>
    /// <remarks>
    /// The nested case is its own test because the salvage has to descend into
    /// `Operator` rather than writing the whole object off - the profile holds
    /// four of the seven values criterion 0.2 names.
    /// </remarks>
    [Fact]
    public void AnUnreadableValueInsideTheProfileCostsThatFieldAlone()
    {
        var path = Write("settings.json", """
            {
              "AudioOutputDeviceId": "{0.0.0.00000000}.{usb-audio-codec-render}",
              "Operator": {
                "Callsign": "KC3QIS",
                "OperatorName": "Tim",
                "GridSquare": "EN90xk",
                "LicenseClass": "General",
                "LicenseClassSource": "RetiredMemberName"
              }
            }
            """);

        var loaded = SettingsStore.LoadFrom(path);

        Assert.Equal(LicenseClassSource.Unset, loaded.Operator.LicenseClassSource);

        Assert.Equal(
            "{0.0.0.00000000}.{usb-audio-codec-render}", loaded.AudioOutputDeviceId);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Equal("Tim", loaded.Operator.OperatorName);
        Assert.Equal("EN90xk", loaded.Operator.GridSquare);
        Assert.Equal(LicenseClass.General, loaded.Operator.LicenseClass);
    }

    /// <summary>
    /// **A file that is not JSON at all is still defaults, and still does not
    /// throw** (HM-DEC-018).
    /// </summary>
    /// <remarks>
    /// The salvage above must not turn into an excuse to half-read rubbish. There
    /// is nothing to salvage from a file with no structure, and the promise that
    /// losing preferences beats refusing to start is unchanged.
    /// </remarks>
    [Fact]
    public void AFileThatIsNotJsonIsStillDefaults()
    {
        var path = Write("settings.json", "{ this is not json");

        var loaded = SettingsStore.LoadFrom(path);

        Assert.Equal(AppSettings.DefaultSpotRefreshMinutes, loaded.SpotRefreshMinutes);
        Assert.Equal("KC3QIS", loaded.Operator.Callsign);
        Assert.Null(loaded.AudioOutputDeviceId);
    }

    // ------------------------------------------------------------------
    // The ALC reference outlives the evening it was learned in.
    // ------------------------------------------------------------------

    /// <summary>
    /// **A learned ALC reference survives a restart, with the time it was taken
    /// at rather than the time it was read back.**
    /// </summary>
    /// <remarks>
    /// **THE AGE IS THE POINT** (HM-DEC-111). A reference is weaker the older it
    /// is, and the operator is shown which he has; stamping a reload with *now*
    /// would make a reference from last Tuesday look like one from this send.
    /// </remarks>
    [Fact]
    public void TheAlcReferenceSurvivesARestartCarryingItsOwnAge()
    {
        var taken = new DateTime(2026, 9, 19, 23, 40, 12, DateTimeKind.Utc);

        var written = new AppSettings
        {
            Psk31AlcReference = new LearnedAlcReference(94.5, taken, "FT8"),
        };

        var path = Path.Combine(_folder, "settings.json");
        Directory.CreateDirectory(_folder);
        SettingsStore.SaveTo(written, path);

        var read = SettingsStore.LoadFrom(path).Psk31AlcReference;

        Assert.NotNull(read);
        Assert.Equal(94.5, read!.Reading, 5);
        Assert.Equal("FT8", read.Mode);
        Assert.Equal(taken, read.TakenUtc.ToUniversalTime());
    }

    /// <summary>
    /// **A reference learned in one session is on the panel in the next, and a
    /// send learns it into the file rather than into memory alone.**
    /// </summary>
    /// <remarks>
    /// <para>**A FIELD NOTHING READS IS NOT A SETTING THAT SURVIVES.** The two
    /// halves are asserted separately here because either one alone leaves the
    /// operator exactly where he was: learning into the file with nothing reading
    /// it back is a write nobody benefits from, and reading it back when nothing
    /// ever writes it is a property that is always null.</para>
    /// <para>**NO RADIO AND NOTHING KEYED** (FACT-004, FACT-006). The reading is
    /// handed in through the seam unit 324 built for exactly this, which runs the
    /// shipped method and nothing else.</para>
    /// </remarks>
    [Fact]
    public void AReferenceLearnedInOneSessionIsOnThePanelInTheNext()
    {
        var taken = new DateTime(2026, 9, 19, 23, 40, 12, DateTimeKind.Utc);
        var settings = new AppSettings { ReconnectOnStartup = false };

        // The evening it is learned.
        var learning = new MainWindowViewModel(settings, null);

        Assert.Null(learning.Psk31AlcReference);

        Assert.True(
            learning.LearnTheAlcForTests(
                RigValue.Known(RigField.Alc, 94.5, "94", taken, "a handed-in reading"),
                "FT8"));

        // **IT IS IN THE SETTINGS, NOT ONLY IN THE VIEW MODEL.**
        Assert.NotNull(settings.Psk31AlcReference);
        Assert.Equal(94.5, settings.Psk31AlcReference!.Reading, 5);

        // The next evening: the same file, a new window.
        var path = Path.Combine(_folder, "settings.json");
        Directory.CreateDirectory(_folder);
        SettingsStore.SaveTo(settings, path);

        var next = new MainWindowViewModel(SettingsStore.LoadFrom(path), null);

        Assert.True(next.HasPsk31AlcReference);
        Assert.Equal(94.5, next.Psk31AlcReference!.Reading, 5);
        Assert.Equal("FT8", next.Psk31AlcReference.Mode);

        // **AND IT CARRIES THE AGE IT WAS TAKEN AT, NOT THIS MOMENT**
        // (HM-DEC-111).
        Assert.Equal(taken, next.Psk31AlcReference.TakenUtc.ToUniversalTime());

        _output.WriteLine("next session reads: " + next.Psk31AlcReferenceLine);
    }
}
