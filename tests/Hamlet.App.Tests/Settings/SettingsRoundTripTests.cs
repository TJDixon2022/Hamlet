using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.Settings;

public sealed class SettingsRoundTripTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_folder, "settings.json");

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

    /// <remarks>Proves HM-DEC-019: the operator profile survives a write and
    /// a read of settings.json intact — all four fields, in the one file, not
    /// a second one.</remarks>
    [Fact]
    public void OperatorProfile_RoundTripsThroughSettingsJson()
    {
        var written = new AppSettings();
        written.Operator.Callsign = "KC3QIS";
        written.Operator.OperatorName = "Tim";
        written.Operator.Location = "Pittsburgh, PA";
        written.Operator.GridSquare = "EN90";

        SettingsStore.SaveTo(written, SettingsPath);
        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.Equal("KC3QIS", read.Operator.Callsign);
        Assert.Equal("Tim", read.Operator.OperatorName);
        Assert.Equal("Pittsburgh, PA", read.Operator.Location);
        Assert.Equal("EN90", read.Operator.GridSquare);
    }

    /// <remarks>Proves HM-DEC-019: a fresh install carries Tim's callsign and
    /// nothing else, so the About byline has something true to say without
    /// inventing a name.</remarks>
    [Fact]
    public void DefaultProfile_HasCallsignOnly()
    {
        var settings = new AppSettings();

        Assert.Equal("KC3QIS", settings.Operator.Callsign);
        Assert.Equal("", settings.Operator.OperatorName);
        Assert.Equal("", settings.Operator.Location);
        Assert.Equal("", settings.Operator.GridSquare);
        Assert.Equal("by KC3QIS", settings.Operator.Byline);
    }

    /// <remarks>Proves HM-DEC-019: the byline degrades honestly — name plus
    /// call, either alone, or empty so the About box shows just the app name
    /// rather than a byline with a hole in it.</remarks>
    [Theory]
    [InlineData("Tim", "KC3QIS", "by Tim, KC3QIS")]
    [InlineData("", "KC3QIS", "by KC3QIS")]
    [InlineData("Tim", "", "by Tim")]
    [InlineData("", "", "")]
    public void Byline_DegradesToWhatIsKnown(string name, string call, string expected)
    {
        var profile = new OperatorProfile { OperatorName = name, Callsign = call };

        Assert.Equal(expected, profile.Byline);
    }

    /// <remarks>Proves HM-DEC-021: a collapsed panel comes back collapsed
    /// after a restart, and a panel nobody has ever touched comes back
    /// open.</remarks>
    [Fact]
    public void PanelCollapseState_PersistsAcrossLoad()
    {
        var written = new AppSettings();
        written.SetPanelExpanded(PanelKeys.Waterfall, false);
        written.SetPanelExpanded(PanelKeys.Spots, true);

        SettingsStore.SaveTo(written, SettingsPath);
        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.False(read.IsPanelExpanded(PanelKeys.Waterfall));
        Assert.True(read.IsPanelExpanded(PanelKeys.Spots));
        Assert.True(read.IsPanelExpanded(PanelKeys.Guide));
    }

    /// <remarks>
    /// Proves HM-DEC-066: the stated Morse speed survives a restart, and a
    /// fresh install starts at the pace the ranking calls relaxed rather than
    /// where the contest operators live. The default is read from the ranking's
    /// own scale, so this also catches the two of them drifting apart.
    /// </remarks>
    [Fact]
    public void CopySpeed_PersistsAndStartsGentle()
    {
        Assert.Equal(13, new AppSettings().CopySpeedWpm);
        Assert.Equal(
            Hamlet.RadioEngine.Explore.SpotRankWeights.RelaxedWpm,
            AppSettings.DefaultCopySpeedWpm);

        SettingsStore.SaveTo(new AppSettings { CopySpeedWpm = 22 }, SettingsPath);

        Assert.Equal(22, SettingsStore.LoadFrom(SettingsPath).CopySpeedWpm);
    }

    /// <remarks>Proves HM-DEC-064: reordering the Explorer's panels costs
    /// nobody the preferences they set. A settings file written before the move
    /// still opens and closes exactly the panels it named, because the state is
    /// keyed by the panel and never by where it sits in the column. The JSON is
    /// written by hand here on purpose, so the test would still catch it if
    /// somebody rewrote the storage as a positional list.</remarks>
    [Fact]
    public void PanelCollapseState_SurvivesTheExplorerReorder()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, """
            {
              "PanelExpanded": {
                "lead": true,
                "spots": false,
                "story": false,
                "guide": true,
                "contact": false
              }
            }
            """);

        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.True(read.IsPanelExpanded(PanelKeys.Lead));
        Assert.False(read.IsPanelExpanded(PanelKeys.Spots));
        Assert.False(read.IsPanelExpanded(PanelKeys.Story));
        Assert.True(read.IsPanelExpanded(PanelKeys.Guide));
        Assert.False(read.IsPanelExpanded(PanelKeys.Contact));
    }

    /// <remarks>
    /// Proves HM-DEC-072: where the operator has been survives a restart. The
    /// moment this list matters most is the following evening, thinking "where
    /// was that station", and a list that emptied on exit would fail exactly
    /// then. The entry with no callsign has to come back with none, because a
    /// blank that reloaded as something would be worse than not saving it.
    /// </remarks>
    [Fact]
    public void RecentStations_SurviveARestart()
    {
        var written = new AppSettings
        {
            Recent = new List<SavedRecentStation>
            {
                new()
                {
                    FrequencyHz = 7_030_000, Station = "W1AW", Mode = "CW",
                    BandName = "40 m", Neighborhood = "QRP watering hole",
                    VisitedUtc = new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc),
                },
                new()
                {
                    FrequencyHz = 14_074_000, Station = "", Mode = "USB-D",
                    BandName = "20 m", Neighborhood = "FT8 city",
                    VisitedUtc = new DateTime(2026, 8, 15, 19, 0, 0, DateTimeKind.Utc),
                },
            },
        };

        SettingsStore.SaveTo(written, SettingsPath);
        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.Equal(2, read.Recent.Count);
        Assert.Equal("W1AW", read.Recent[0].Station);
        Assert.Equal("QRP watering hole", read.Recent[0].Neighborhood);
        Assert.Equal(7_030_000, read.Recent[0].FrequencyHz);

        // The one nobody identified comes back unidentified.
        Assert.Equal("", read.Recent[1].Station);
    }

    /// <remarks>Proves HM-DEC-072: a profile written before this feature loads
    /// with an empty list rather than throwing, which is the same promise every
    /// other added setting makes (§6.1).</remarks>
    [Fact]
    public void RecentStations_AreEmptyInAProfileWrittenBeforeThem()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, """{ "LastBand": "40 m" }""");

        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.Empty(read.Recent);
        Assert.Equal("40 m", read.LastBand);
    }

    /// <remarks>Proves HM-DEC-020: the chosen refresh interval survives a
    /// restart, and "Off" (0) survives as Off rather than being rounded back
    /// up to the default by a null-ish check.</remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(15)]
    public void SpotRefreshInterval_Persists(int minutes)
    {
        var written = new AppSettings { SpotRefreshMinutes = minutes };

        SettingsStore.SaveTo(written, SettingsPath);

        Assert.Equal(minutes, SettingsStore.LoadFrom(SettingsPath).SpotRefreshMinutes);
    }

    /// <remarks>Proves HM-DEC-052: startup reconnect is on out of the box, and
    /// an operator who turns it off finds it still off next time. A switch that
    /// silently reverts to on would reopen a COM port somebody deliberately left
    /// alone.</remarks>
    [Fact]
    public void StartupReconnect_IsOnByDefaultAndSurvivesBeingTurnedOff()
    {
        Assert.True(new AppSettings().ReconnectOnStartup);

        SettingsStore.SaveTo(new AppSettings { ReconnectOnStartup = false }, SettingsPath);

        Assert.False(SettingsStore.LoadFrom(SettingsPath).ReconnectOnStartup);
    }

    /// <remarks>Proves HM-DEC-056: the first thing Hamlet does to somebody's
    /// radio without being asked ships on, and an operator who turns it off
    /// finds it still off next time. A switch that silently reverted to on would
    /// start moving a control they deliberately took back.</remarks>
    [Fact]
    public void ModeFollowingTheMap_IsOnByDefaultAndSurvivesBeingTurnedOff()
    {
        Assert.True(new AppSettings().ModeFollowsTheMap);

        SettingsStore.SaveTo(new AppSettings { ModeFollowsTheMap = false }, SettingsPath);

        Assert.False(SettingsStore.LoadFrom(SettingsPath).ModeFollowsTheMap);
    }

    /// <remarks>Proves HM-DEC-057: a fresh profile has chosen no lens, which is
    /// the state that lets Hamlet guess one. Once the operator picks, the choice
    /// and the watermark both survive a restart, because guessing again after
    /// they have answered is the app arguing with them.</remarks>
    [Fact]
    public void TheChosenLensAndTheWatermarkSurviveARestart()
    {
        Assert.Null(new AppSettings().SpotLens);
        Assert.Null(new AppSettings().SpotsLastLookedUtc);

        var looked = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

        SettingsStore.SaveTo(
            new AppSettings { SpotLens = "WhatsNew", SpotsLastLookedUtc = looked },
            SettingsPath);

        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.Equal("WhatsNew", read.SpotLens);
        Assert.Equal(looked, read.SpotsLastLookedUtc!.Value.ToUniversalTime());
    }

    /// <remarks>Proves HM-DEC-060: favorites survive a restart with everything
    /// that answers "what was this for" intact. A favorite that came back as a
    /// bare number would be the radio's own memory channels again, which is the
    /// problem rather than the answer.</remarks>
    [Fact]
    public void FavoritesSurviveARestartWithTheirReasonAttached()
    {
        Assert.Empty(new AppSettings().Favorites);

        var written = new AppSettings();
        written.Favorites.Add(new SavedFavorite
        {
            FrequencyHz = 14_074_000,
            Name = "14.074, FT8 city",
            Mode = "USB",
            BandName = "20 m",
            Neighborhood = "FT8 city",
            SavedUtc = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc),
            Note = "where I heard Japan that once",
        });

        SettingsStore.SaveTo(written, SettingsPath);
        var read = SettingsStore.LoadFrom(SettingsPath).Favorites;

        var back = Assert.Single(read);

        Assert.Equal(14_074_000, back.FrequencyHz);
        Assert.Equal("14.074, FT8 city", back.Name);
        Assert.Equal("USB", back.Mode);
        Assert.Equal("20 m", back.BandName);
        Assert.Equal("FT8 city", back.Neighborhood);
        Assert.Equal("where I heard Japan that once", back.Note);
    }

    /// <remarks>
    /// <para>Proves a favorite written before notes existed still loads, and has
    /// none. Nothing about the file changed for anybody who never writes one, so
    /// there is nothing to migrate and this is the check that says so (§6.1).
    /// </para>
    /// </remarks>
    [Fact]
    public void AFavoriteWrittenBeforeNotesExistedStillLoads()
    {
        Directory.CreateDirectory(_folder);

        File.WriteAllText(
            SettingsPath,
            "{ \"Favorites\": [ { \"FrequencyHz\": 7030000, "
            + "\"Name\": \"7.030, Morse main street\", "
            + "\"Mode\": \"CW\", \"BandName\": \"40 m\", "
            + "\"Neighborhood\": \"Morse main street\", "
            + "\"SavedUtc\": \"2026-08-15T12:00:00Z\" } ] }");

        var back = Assert.Single(SettingsStore.LoadFrom(SettingsPath).Favorites);

        Assert.Equal("7.030, Morse main street", back.Name);
        Assert.Equal("", back.Note);
    }

    /// <remarks>Proves HM-DEC-018 still holds with the profile added: a
    /// corrupt settings file yields defaults rather than a crash, and the
    /// defaults include a usable profile.</remarks>
    [Fact]
    public void CorruptSettingsFile_YieldsDefaults()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, "{ this is not json");

        var read = SettingsStore.LoadFrom(SettingsPath);

        Assert.Equal(AppSettings.DefaultSpotRefreshMinutes, read.SpotRefreshMinutes);
        Assert.Equal("KC3QIS", read.Operator.Callsign);
        Assert.Equal(
            Enum.GetValues<TelemetryCategory>().Length,
            read.EnabledTelemetryCategoryCount);
    }
}
