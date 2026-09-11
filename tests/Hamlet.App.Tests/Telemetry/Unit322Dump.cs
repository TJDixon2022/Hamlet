using System;
using System.Collections.Generic;
using System.IO;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 322 task 5: **play the fixtures and leave the file behind.**
/// </summary>
/// <remarks>
/// **THIS IS A TOOL, NOT AN ASSERTION.** It writes the two `.jsonl` files the task's
/// reading is written from, into a folder the environment names, so the diagnosis in
/// `docs/psk31-telemetry-reading.md` is made from a real record rather than from a
/// description of one. It asserts nothing and is skipped unless the folder is set.
/// </remarks>
public sealed class Unit322Dump
{
    /// <summary>Write the two files, where a folder is named.</summary>
    [Fact]
    public void WriteTheTwoFiles()
    {
        var into = Environment.GetEnvironmentVariable("HAMLET_PSK31_DUMP");

        if (string.IsNullOrWhiteSpace(into))
        {
            return;
        }

        Directory.CreateDirectory(into);

        Play(Path.Combine(into, "four"), "psk31-four-signals.wav");
        Play(Path.Combine(into, "noise"), "psk31-noise-only-30s.wav");
    }

    private static void Play(string folder, string fixture)
    {
        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "1.13.8", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";
            settings.Operator.GridSquare = "FN00DJ";

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                TapForTests = new AudioTap(),
                ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
            };

            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

            model.ChooseDigitalModeCommand.Execute("PSK31");

            var audio = WavAudio.Read(Path.Combine(
                Root(), "assets", "fixtures", fixture));

            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                var count = Math.Min(chunk, audio.Samples.Length - at);

                model.TapForTests!.Take(
                    audio.Samples.AsSpan(at, count), audio.SampleRate);

                model.LookForASlotForTests();
            }

            model.ChooseDigitalModeCommand.Execute("FT8");
        }
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
