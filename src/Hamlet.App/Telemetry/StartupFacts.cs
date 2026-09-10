using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Telemetry;

/// <summary>
/// **Gathers what the startup snapshot says, on this machine, right now.**
/// </summary>
/// <remarks>
/// <para>**THE SHAPE IS THE ENGINE'S AND THE GATHERING IS THIS SIDE'S** (§0.1).
/// `StartupSnapshot` knows what a diagnostic has to contain; only the application
/// knows how to enumerate a sound card or where the settings file lives.</para>
/// <para>**EVERY READER IS WRAPPED SEPARATELY, AND THAT IS THE POINT.** A machine
/// broken enough to throw while being described is exactly the machine somebody needs
/// the description of, so one failing reader records its own failure and the rest of
/// the snapshot still goes out. **A snapshot that fails to write on a broken machine
/// is worthless.**</para>
/// <para>**NOTHING PERSONAL** (§2.1): no callsign, no name, no grid, no location. The
/// operator's profile is never read here at all, which is stronger than remembering
/// not to write it.</para>
/// </remarks>
public static class StartupFacts
{
    /// <summary>Gather everything and write one event.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="settings">The settings as loaded.</param>
    /// <param name="audioDevices">The input devices, or null to enumerate.</param>
    /// <param name="transmitEndpoints">The output devices, or null to enumerate.</param>
    /// <param name="categoriesOn">Which telemetry categories are switched on.</param>
    /// <remarks>
    /// **IT IS WRITTEN EVEN WHEN IT IS BAD NEWS, ESPECIALLY THEN**, and it is written
    /// under `Diagnostics`, which is the category documented as the application's own
    /// record.
    /// </remarks>
    public static void Write(
        ITelemetry? telemetry,
        AppSettings? settings,
        IAudioDevices? audioDevices = null,
        Func<IReadOnlyList<RenderEndpoint>>? transmitEndpoints = null,
        Func<TelemetryCategory, bool>? categoriesOn = null)
    {
        if (telemetry is null)
        {
            return;
        }

        try
        {
            var parts = Gather(
                settings, audioDevices, transmitEndpoints, categoriesOn, telemetry);

            telemetry.Write(
                TelemetryCategory.Diagnostics,
                StartupSnapshot.EventName,
                StartupSnapshot.Compose(parts));
        }
        catch (Exception error)
        {
            // **EVEN THE GATHERING FAILING IS A DIAGNOSTIC**, and it is the one a
            // reader would most want. Never-throw discipline (§8) is not silence.
            try
            {
                telemetry.Write(
                    TelemetryCategory.Diagnostics,
                    StartupSnapshot.EventName,
                    new Dictionary<string, object?>
                    {
                        ["gathered"] = "failed",
                        ["gatheredWhy"] = error.GetType().Name + ": " + error.Message,
                    },
                    TelemetryLevel.Error);
            }
            catch (Exception)
            {
                // Logging that can crash the app is worse than no logging (§8).
            }
        }
    }

    /// <summary>Everything the snapshot can say about this machine.</summary>
    /// <param name="settings">The settings as loaded.</param>
    /// <param name="audioDevices">The input devices, or null to enumerate.</param>
    /// <param name="transmitEndpoints">The output devices, or null to enumerate.</param>
    /// <param name="categoriesOn">Which categories are switched on.</param>
    /// <param name="telemetry">The sink, for its own dropped count.</param>
    /// <returns>The parts, with every failure recorded rather than thrown.</returns>
    public static SnapshotParts Gather(
        AppSettings? settings,
        IAudioDevices? audioDevices = null,
        Func<IReadOnlyList<RenderEndpoint>>? transmitEndpoints = null,
        Func<TelemetryCategory, bool>? categoriesOn = null,
        ITelemetry? telemetry = null)
    {
        var notKnown = new Dictionary<string, string>(StringComparer.Ordinal);

        var inputs = Try(
            () => (audioDevices ?? new WasapiAudioDevices()).List()
                .Select(d => d.Id + " " + d.Name)
                .ToList() as IReadOnlyList<string>,
            notKnown, "audioInputs");

        var outputs = Try(
            () => (transmitEndpoints is null
                    ? WasapiTransmitSink.Endpoints()
                    : transmitEndpoints())
                .Select(e => e.Id + " " + e.Name)
                .ToList() as IReadOnlyList<string>,
            notKnown, "audioOutputs");

        var wantedInput = settings?.AudioInputDeviceId;
        var wantedOutput = settings?.AudioOutputDeviceId;

        var absent = new List<string>();

        var inputPresent = Present(inputs, wantedInput);
        var outputPresent = Present(outputs, wantedOutput);

        if (inputPresent == false)
        {
            absent.Add("audio input " + wantedInput);
        }

        if (outputPresent == false)
        {
            absent.Add("transmit device " + wantedOutput);
        }

        var parts = new SnapshotParts
        {
            AppVersion = Try(Version, notKnown, "appVersion"),
            Ft8SharpVersion = Try(
                () => VersionOf("Ft8Sharp"), notKnown, "ft8SharpVersion"),
            Ft8SharpDeepVersion = Try(
                () => VersionOf("Ft8Sharp.Deep"), notKnown, "ft8SharpDeepVersion"),
            Framework = Try(
                () => RuntimeInformation.FrameworkDescription, notKnown, "framework"),
            OsBuild = Try(
                () => RuntimeInformation.OSDescription, notKnown, "osBuild"),
            ProcessBits = Try(
                () => (int?)(Environment.Is64BitProcess ? 64 : 32),
                notKnown, "processBits"),

            AudioInputs = inputs,
            AudioInputSelected = wantedInput is { Length: > 0 }
                ? wantedInput
                : "(none named, so Hamlet preselects and never claims)",
            AudioInputPresent = inputPresent,
            AudioInputChosenBy = wantedInput is { Length: > 0 }
                ? "remembered from settings"
                : "not remembered",

            AudioOutputs = outputs,
            TransmitDeviceSelected = wantedOutput is { Length: > 0 }
                ? wantedOutput
                : "(none named, so a send refuses)",
            TransmitDevicePresent = outputPresent,

            SettingsLoaded = settings is not null,
            SettingsPathExists = Try(
                () => (bool?)File.Exists(SettingsStore.SettingsPath),
                notKnown, "settingsPathExists"),
            SettingsNamedButAbsent = absent,

            TelemetryCategoriesOn = Categories(categoriesOn, on: true),
            TelemetryCategoriesOff = Categories(categoriesOn, on: false),
            TelemetryEventsDropped = telemetry?.DroppedEventCount,
        };

        foreach (var (key, why) in notKnown)
        {
            parts.NotKnown[key] = why;
        }

        // **THE RADIO, THE CLOCK AND READINESS ARE NOT REACHABLE FROM HERE.** They
        // live on the view model, which is built after this runs; the caller fills
        // them in. **Saying so is the point** - a reader gets `unknown` and a reason
        // rather than a field that quietly is not there.
        return parts;
    }

    /// <summary>Whether a wanted device id is among the ones present.</summary>
    /// <remarks>
    /// **NULL WHERE THE LIST COULD NOT BE READ**, because *the enumeration threw* and
    /// *the device is missing* are different facts and the second is the serious one.
    /// </remarks>
    private static bool? Present(IReadOnlyList<string>? devices, string? wanted)
    {
        if (devices is null)
        {
            return null;
        }

        if (wanted is not { Length: > 0 })
        {
            return null;
        }

        return devices.Any(
            d => d.StartsWith(wanted, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The categories that are on, or off.</summary>
    private static IReadOnlyList<string> Categories(
        Func<TelemetryCategory, bool>? isOn, bool on)
    {
        if (isOn is null)
        {
            return Array.Empty<string>();
        }

        return Enum.GetValues<TelemetryCategory>()
            .Where(c => Safe(isOn, c) == on)
            .Select(c => c.ToString())
            .ToList();
    }

    private static bool Safe(Func<TelemetryCategory, bool> isOn, TelemetryCategory c)
    {
        try
        {
            return isOn(c);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Read one fact, or record why it could not be read.</summary>
    /// <remarks>
    /// **ONE READER FAILING MUST NOT COST THE OTHERS.** A snapshot that throws while
    /// describing a broken machine describes nothing, and the broken machine is the
    /// one somebody needs described.
    /// </remarks>
    private static T? Try<T>(
        Func<T?> read, Dictionary<string, string> notKnown, string key)
    {
        try
        {
            return read();
        }
        catch (Exception error)
        {
            notKnown[key] = error.GetType().Name + ": " + error.Message;

            return default;
        }
    }

    private static string Version()
        => typeof(StartupFacts).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? typeof(StartupFacts).Assembly.GetName().Version?.ToString()
        ?? StartupSnapshot.Unknown;

    private static string VersionOf(string assemblyName)
    {
        var found = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(
                a.GetName().Name, assemblyName, StringComparison.Ordinal));

        if (found is null)
        {
            return StartupSnapshot.Unknown + " (not loaded yet)";
        }

        return found.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? found.GetName().Version?.ToString()
            ?? StartupSnapshot.Unknown;
    }
}
