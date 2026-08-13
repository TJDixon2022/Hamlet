using Hamlet.App.Settings;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Settings;

/// <summary>
/// The upgrade must not look like the app forgetting who the operator is
/// (HM-DEC-035).
/// </summary>
public sealed class SettingsMigrationTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-migration-" + Guid.NewGuid().ToString("N"));

    /// <summary>
    /// A settings.json exactly as the build before the US-spelling rename wrote
    /// it, carrying Tim's real profile.
    /// </summary>
    private const string LegacyFile = """
        {
          "Operator": {
            "Callsign": "KC3QIS",
            "OperatorName": "Tim",
            "Location": "Trafford, PA",
            "GridSquare": "FN00",
            "LicenceClass": "General",
            "LicenceClassSource": "LookedUp",
            "LicenceClassSourceName": "callook.info",
            "LicenceClassSetOn": "2026-08-13"
          },
          "LastPort": "COM4",
          "LastBand": "40 m"
        }
        """;

    private string Write(string json)
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, "settings.json");
        File.WriteAllText(path, json);
        return path;
    }

    /// <inheritdoc/>
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
            // A leftover temp folder is not worth failing a test over.
        }
    }

    /// <remarks>
    /// THE ONE THAT MATTERS. Proves the exact file on Tim's machine survives
    /// the rename: callsign, class, the service that answered, and the date it
    /// answered. A silent reset here would be indistinguishable from the app
    /// losing his identity, and he would have no reason to suspect a spelling
    /// change caused it.
    /// </remarks>
    [Fact]
    public void LegacyProfile_SurvivesTheRename()
    {
        var settings = SettingsStore.LoadFrom(Write(LegacyFile));

        Assert.Equal("KC3QIS", settings.Operator.Callsign);
        Assert.Equal("Tim", settings.Operator.OperatorName);
        Assert.Equal(LicenseClass.General, settings.Operator.LicenseClass);
        Assert.Equal(LicenseClassSource.LookedUp, settings.Operator.LicenseClassSource);
        Assert.Equal("callook.info", settings.Operator.LicenseClassSourceName);
        Assert.Equal("2026-08-13", settings.Operator.LicenseClassSetOn);
    }

    /// <remarks>
    /// Proves provenance survives intact, not just the class. "General, from
    /// callook.info, on 13 August" and "General, because you said so" are
    /// different claims, and a migration that kept the value and dropped where
    /// it came from would turn the first into the second.
    /// </remarks>
    [Fact]
    public void LegacyProfile_KeepsItsProvenanceNotJustItsClass()
    {
        var settings = SettingsStore.LoadFrom(Write(LegacyFile));

        Assert.False(settings.Operator.LicenseClassWasSetByHand);
        Assert.NotEqual("", settings.Operator.LicenseClassSourceName);
        Assert.NotEqual("", settings.Operator.LicenseClassSetOn);
    }

    /// <remarks>
    /// Proves a hand-set class stays hand-set through the migration. That flag
    /// is the one thing standing between the operator's own answer and a lookup
    /// overwriting it (HM-DEC-028), and losing it in an upgrade would quietly
    /// re-arm the overwrite.
    /// </remarks>
    [Fact]
    public void HandSetClass_StaysHandSet()
    {
        var settings = SettingsStore.LoadFrom(Write("""
            {
              "Operator": {
                "Callsign": "KC3QIS",
                "LicenceClass": "Extra",
                "LicenceClassSource": "EnteredByOperator",
                "LicenceClassSetOn": "2019-04-02"
              }
            }
            """));

        Assert.Equal(LicenseClass.Extra, settings.Operator.LicenseClass);
        Assert.True(settings.Operator.LicenseClassWasSetByHand);
    }

    /// <remarks>
    /// Proves the numeric form older builds wrote still migrates. Those files
    /// exist: the string converter arrived after the first release of the
    /// profile.
    /// </remarks>
    [Fact]
    public void NumericLegacyValues_AlsoMigrate()
    {
        var settings = SettingsStore.LoadFrom(Write($$"""
            {
              "Operator": {
                "Callsign": "KC3QIS",
                "LicenceClass": {{(int)LicenseClass.General}},
                "LicenceClassSource": {{(int)LicenseClassSource.LookedUp}}
              }
            }
            """));

        Assert.Equal(LicenseClass.General, settings.Operator.LicenseClass);
        Assert.Equal(LicenseClassSource.LookedUp, settings.Operator.LicenseClassSource);
    }

    /// <remarks>
    /// Proves the new key wins when both are present. A file that has already
    /// been migrated and then edited must not be dragged back to a stale value
    /// left beside it.
    /// </remarks>
    [Fact]
    public void NewKeyWinsOverAStaleLegacyKey()
    {
        var settings = SettingsStore.LoadFrom(Write("""
            {
              "Operator": {
                "Callsign": "KC3QIS",
                "LicenseClass": "Extra",
                "LicenceClass": "Technician"
              }
            }
            """));

        Assert.Equal(LicenseClass.Extra, settings.Operator.LicenseClass);
    }

    /// <remarks>
    /// Proves a file already written by the new build needs no migration and is
    /// not touched by one.
    /// </remarks>
    [Fact]
    public void CurrentFile_LoadsUnchanged()
    {
        var settings = SettingsStore.LoadFrom(Write("""
            {
              "Operator": {
                "Callsign": "KC3QIS",
                "LicenseClass": "General",
                "LicenseClassSource": "LookedUp",
                "LicenseClassSourceName": "callook.info",
                "LicenseClassSetOn": "2026-08-13"
              }
            }
            """));

        Assert.Equal(LicenseClass.General, settings.Operator.LicenseClass);
        Assert.Equal("callook.info", settings.Operator.LicenseClassSourceName);
    }

    /// <remarks>
    /// Proves a migrated profile then round-trips under the new key, so the
    /// carry-forward happens once rather than on every launch forever.
    /// </remarks>
    [Fact]
    public void MigratedProfile_SavesUnderTheNewKey()
    {
        var path = Write(LegacyFile);
        var settings = SettingsStore.LoadFrom(path);

        SettingsStore.SaveTo(settings, path);

        var written = File.ReadAllText(path);

        Assert.Contains("\"LicenseClass\": \"General\"", written, StringComparison.Ordinal);
        Assert.DoesNotContain("\"LicenceClass\"", written, StringComparison.Ordinal);

        Assert.Equal(LicenseClass.General, SettingsStore.LoadFrom(path).Operator.LicenseClass);
    }

    /// <remarks>
    /// Proves the migration never throws on rubbish. It runs on every launch,
    /// before the window exists, and a crash there would be worse than the
    /// reset it prevents (§8).
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[1,2,3]")]
    [InlineData("{\"Operator\": \"not an object\"}")]
    [InlineData("{\"Operator\": {\"LicenceClass\": {\"nested\": true}}}")]
    [InlineData("{\"Operator\": {\"LicenceClass\": \"Archduke\"}}")]
    public void MalformedFiles_DoNotThrow(string json)
    {
        var settings = SettingsStore.LoadFrom(Write(json));

        Assert.NotNull(settings);
        Assert.NotNull(settings.Operator);
    }

    /// <remarks>
    /// Proves a fresh install is unaffected: no file, no legacy keys, and the
    /// class stays honestly unknown rather than being defaulted to the
    /// commonest one (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void FreshInstall_HasNoClassAtAll()
    {
        var settings = SettingsStore.LoadFrom(
            Path.Combine(_folder, "does-not-exist.json"));

        Assert.Equal(LicenseClass.Unknown, settings.Operator.LicenseClass);
        Assert.Equal(LicenseClassSource.Unset, settings.Operator.LicenseClassSource);
    }
}
