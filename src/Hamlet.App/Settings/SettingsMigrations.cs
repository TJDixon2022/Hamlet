using System.Text.Json;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Settings;

/// <summary>
/// Carries an older settings.json forward when a stored key changes name.
/// </summary>
/// <remarks>
/// <para>WHY THIS EXISTS (HM-DEC-035). Renaming a property renames the key it
/// is written under. The four licence-class fields were spelled the British
/// way, and the US-spelling standard renamed them — which on its own would mean
/// that the first launch after the upgrade finds no <c>LicenseClass</c> in the
/// file, falls back to the default, and quietly forgets that the operator is
/// General and where that was established. Nothing would crash and nothing
/// would say a word, which is exactly what makes it bad: it would look like the
/// app forgetting who he is.</para>
/// <para>Old keys are read only when the new key is absent, so a file that has
/// already been migrated is never overwritten by a stale value left beside it.
/// The migrated settings are written back on the next save in the normal way;
/// nothing here writes to disk.</para>
/// <para>Never throws. A settings file is a convenience, and a migration that
/// crashed the app on launch would be far worse than the reset it prevents
/// (§8).</para>
/// </remarks>
public static class SettingsMigrations
{
    /// <summary>
    /// Fill in anything the current key names did not find, from the names an
    /// older file would have used.
    /// </summary>
    /// <param name="settings">Settings as deserialized. Modified in place.</param>
    /// <param name="json">The raw file text it came from.</param>
    /// <returns>True when something was carried forward.</returns>
    public static bool Apply(AppSettings settings, string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("Operator", out var op)
                || op.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            return CarryLicenseClass(settings.Operator, op);
        }
        catch (Exception)
        {
            // A file we cannot parse is a file the loader already handled.
            return false;
        }
    }

    /// <summary>
    /// Pre-2026-08-13 files spell the four license-class keys the British way.
    /// </summary>
    private static bool CarryLicenseClass(OperatorProfile profile, JsonElement op)
    {
        var carried = false;

        if (profile.LicenseClass == LicenseClass.Unknown
            && !op.TryGetProperty("LicenseClass", out _)
            && op.TryGetProperty("LicenceClass", out var legacyClass)
            && TryReadClass(legacyClass, out var parsed))
        {
            profile.LicenseClass = parsed;
            carried = true;
        }

        if (profile.LicenseClassSource == LicenseClassSource.Unset
            && !op.TryGetProperty("LicenseClassSource", out _)
            && op.TryGetProperty("LicenceClassSource", out var legacySource)
            && TryReadSource(legacySource, out var source))
        {
            profile.LicenseClassSource = source;
            carried = true;
        }

        if (profile.LicenseClassSourceName.Length == 0
            && !op.TryGetProperty("LicenseClassSourceName", out _)
            && op.TryGetProperty("LicenceClassSourceName", out var legacyName)
            && legacyName.ValueKind == JsonValueKind.String)
        {
            profile.LicenseClassSourceName = legacyName.GetString() ?? "";
            carried = true;
        }

        if (profile.LicenseClassSetOn.Length == 0
            && !op.TryGetProperty("LicenseClassSetOn", out _)
            && op.TryGetProperty("LicenceClassSetOn", out var legacyDate)
            && legacyDate.ValueKind == JsonValueKind.String)
        {
            profile.LicenseClassSetOn = legacyDate.GetString() ?? "";
            carried = true;
        }

        return carried;
    }

    /// <summary>
    /// Read a class written as a name or, from older builds still, as a number.
    /// </summary>
    private static bool TryReadClass(JsonElement element, out LicenseClass value)
    {
        if (element.ValueKind == JsonValueKind.String
            && Enum.TryParse(element.GetString(), ignoreCase: true, out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number
            && element.TryGetInt32(out var number)
            && Enum.IsDefined(typeof(LicenseClass), number))
        {
            value = (LicenseClass)number;
            return true;
        }

        value = LicenseClass.Unknown;
        return false;
    }

    private static bool TryReadSource(JsonElement element, out LicenseClassSource value)
    {
        if (element.ValueKind == JsonValueKind.String
            && Enum.TryParse(element.GetString(), ignoreCase: true, out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number
            && element.TryGetInt32(out var number)
            && Enum.IsDefined(typeof(LicenseClassSource), number))
        {
            value = (LicenseClassSource)number;
            return true;
        }

        value = LicenseClassSource.Unset;
        return false;
    }
}
