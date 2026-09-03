using System.Diagnostics;
using Xunit;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// <b>Upstream's own decoder, run as a black box.</b> The one instrument in this phase that is
/// outside this receiver rather than inside it.
/// </summary>
/// <remarks>
/// <para>
/// <b>It is the sibling of <see cref="Ft8Oracle"/> and follows every one of its rules.</b> The binary
/// is built outside the tree by <c>tools\build-ft8-oracle.bat</c>, which is the owner's file and is
/// never edited, committed or deleted here. This class locates it and runs it. <b>No compiler is
/// invoked from inside a test process</b> — unit 209 judged that a workaround rather than a
/// sanctioned route and that judgment stands.
/// </para>
/// <para>
/// <b>Nothing it returns is ever committed.</b> Upstream's stdout is read at run time, counted,
/// compared against the text that was transmitted, and dropped.
/// </para>
/// <para>
/// <b>It skips rather than fails when the binary is absent</b>, for the same reason as
/// <see cref="RequiresOracleFactAttribute"/>: a fresh clone on another machine has neither the pin
/// nor anything built from it and must still come back green.
/// </para>
/// </remarks>
internal static class Ft8Decoder
{
    /// <summary>Where <c>tools\build-ft8-oracle.bat</c> puts the decoder.</summary>
    private const string RelativeExecutablePath = @"build\decode_ft8.exe";

    /// <summary>The decoder as the owner's script built it, wherever the clone is.</summary>
    public static string ExecutablePath =>
        Path.Combine(ReferenceClone.Location, RelativeExecutablePath);

    /// <summary>Whether the decoder is on this machine and can be run.</summary>
    public static bool IsPresent
    {
        get
        {
            try
            {
                return File.Exists(ExecutablePath);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>One message upstream said it heard.</summary>
    /// <param name="Text">The message text, everything after upstream's <c>~</c> marker.</param>
    /// <param name="Raw">The whole line, kept so a report can quote a shape without inventing one.</param>
    /// <param name="SnrDecibels">The signal-to-noise ratio upstream printed, or null if unreadable.</param>
    /// <param name="TimeSeconds">The time offset upstream printed, or null if unreadable.</param>
    /// <param name="FrequencyHz">The frequency upstream printed, or null if unreadable.</param>
    internal sealed record Line(
        string Text,
        string Raw,
        double? SnrDecibels,
        double? TimeSeconds,
        double? FrequencyHz);

    /// <summary>What one run of the decoder produced.</summary>
    /// <param name="ExitCode">The process's own exit code.</param>
    /// <param name="StandardOutput">Everything it wrote to stdout, verbatim and never committed.</param>
    /// <param name="StandardError">Everything it wrote to stderr.</param>
    /// <param name="Lines">The decodes parsed out of stdout.</param>
    internal sealed record Run(
        int ExitCode,
        string StandardOutput,
        string StandardError,
        IReadOnlyList<Line> Lines);

    /// <summary>Runs upstream's decoder over one WAV file and reads back what it heard.</summary>
    public static Run Decode(string wavPath)
    {
        var start = new ProcessStartInfo(ExecutablePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,

            // The clone is the working directory rather than the tree, so anything the decoder
            // chooses to drop beside itself lands outside this repository.
            WorkingDirectory = ReferenceClone.Location,
        };
        start.ArgumentList.Add(wavPath);

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException($"could not start {ExecutablePath}");

        // Read both pipes before waiting, or a decoder that fills one of them while we block on
        // Exit deadlocks.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60_000))
        {
            try
            {
                // The one termination permitted: a process this test started.
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Already gone; nothing to do.
            }

            throw new TimeoutException($"{ExecutablePath} did not exit within 60 s for {wavPath}.");
        }

        var text = stdout.Result;
        return new Run(process.ExitCode, text, stderr.Result, ReadLines(text));
    }

    /// <summary>
    /// The decodes upstream printed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Deliberately anchored on upstream's own marker rather than scavenging.</b> Every decode
    /// line <c>demo/decode_ft8.c</c> prints carries a <c>~</c> field between its numbers and its
    /// message, and a parser that took any line with words on it would read a banner, a usage
    /// string or an error as a decode and then report a rate it never measured.
    /// </para>
    /// <para>
    /// <b>The numeric fields are read where they parse and left null where they do not</b>, because
    /// the count is what the comparison turns on and a report that dropped a decode over an
    /// unreadable frequency would understate upstream.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<Line> ReadLines(string standardOutput)
    {
        var found = new List<Line>();
        foreach (var raw in standardOutput.Split('\n'))
        {
            var line = raw.TrimEnd('\r').TrimEnd();
            var marker = line.IndexOf('~');
            if (marker < 0)
            {
                continue;
            }

            var text = line[(marker + 1)..].Trim();
            if (text.Length == 0)
            {
                continue;
            }

            // The fields before the marker, in upstream's own order: a timestamp, an SNR, a time
            // offset and a frequency. Read positionally from the end so a timestamp that is present
            // on the live-capture path and absent on the file path does not shift the rest.
            var head = line[..marker]
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            double? snr = null, dt = null, hz = null;
            if (head.Length >= 3)
            {
                hz = ParseOrNull(head[^1]);
                dt = ParseOrNull(head[^2]);
                snr = ParseOrNull(head[^3]);
            }

            found.Add(new Line(text, line, snr, dt, hz));
        }

        return found;
    }

    private static double? ParseOrNull(string field) =>
        double.TryParse(
            field,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    /// <summary>Whether the decoder can actually answer a question, and if not, why not.</summary>
    internal enum Usability
    {
        /// <summary>It read a WAV upstream's own generator wrote and returned the message in it.</summary>
        Usable,

        /// <summary>The pinned clone is not on this machine.</summary>
        CloneAbsent,

        /// <summary>The generator is not built, so there is nothing to prove the decoder against.</summary>
        GeneratorNotBuilt,

        /// <summary>The clone is here and no decoder has been built from it.</summary>
        NotBuilt,

        /// <summary>The executable exists and does not survive a file.</summary>
        BuiltButWillNotRun,

        /// <summary>It ran, and did not return the message upstream's own generator had just written.</summary>
        RanButHeardNothing,
    }

    private static (Usability State, string Detail)? _usability;

    /// <summary>
    /// Asks the decoder one canary question, once per test run, and remembers the answer.
    /// </summary>
    /// <remarks>
    /// <b>The canary is the round trip and not <c>File.Exists</c>.</b> Unit 210 found upstream's
    /// generator present, sound enough to print its own usage, and dying on every real message —
    /// so a gate that asked only whether a file was there would have reported a comparison it never
    /// made. The same trap is available to a decoder and this walks around it: generate a slot with
    /// upstream's own generator, decode it with upstream's own decoder, and require the message back.
    /// </remarks>
    public static (Usability State, string Detail) ProbeUsability()
    {
        if (_usability is { } cached)
        {
            return cached;
        }

        _usability = Measure();
        return _usability.Value;

        static (Usability, string) Measure()
        {
            if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.Absent)
            {
                return (Usability.CloneAbsent,
                    $"the pinned clone is not at {ReferenceClone.Location}: {detail}");
            }

            if (!IsPresent)
            {
                return (Usability.NotBuilt,
                    $"nothing is built at {ExecutablePath}; run tools\\build-ft8-oracle.bat");
            }

            var (generator, generatorDetail) = Ft8Oracle.ProbeUsability();
            if (generator != Ft8Oracle.Usability.Usable)
            {
                return (Usability.GeneratorNotBuilt,
                    $"upstream's generator cannot write a slot to prove the decoder against — "
                    + $"{generator}: {generatorDetail}");
            }

            const string canary = "CQ K1ABC FN42";
            var written = Ft8Oracle.GenerateKeepingWav(canary);
            try
            {
                Run run;
                try
                {
                    run = Decode(written.WavPath);
                }
                catch (Exception ex)
                {
                    return (Usability.BuiltButWillNotRun,
                        $"{ExecutablePath}: {ex.GetType().Name}: {ex.Message}");
                }

                if (run.ExitCode != 0)
                {
                    return (Usability.BuiltButWillNotRun,
                        $"{ExecutablePath} exited {run.ExitCode} (0x{run.ExitCode:X8}) on a slot "
                        + "upstream's own generator had just written");
                }

                return run.Lines.Any(l => string.Equals(l.Text, canary, StringComparison.Ordinal))
                    ? (Usability.Usable,
                        $"read a slot upstream's generator wrote and returned '{canary}' — "
                        + $"{run.Lines.Count} line(s)")
                    : (Usability.RanButHeardNothing,
                        $"ran and exited zero over upstream's own generator output and did not "
                        + $"return '{canary}'; it printed {run.Lines.Count} decode line(s)");
            }
            finally
            {
                WavFile.DeleteQuietly(written.WavPath);
            }
        }
    }
}

/// <summary>
/// A fact that skips itself when the pinned clone is absent <em>or</em> when upstream's decoder has
/// not been built on this machine.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresDecoderFactAttribute : FactAttribute
{
    public RequiresDecoderFactAttribute()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.Absent)
        {
            Skip = $"The pinned ft8_lib clone is not on this machine at {ReferenceClone.Location}. "
                + $"It is never committed, so this is expected on a fresh clone. {detail}";
            return;
        }

        if (!Ft8Decoder.IsPresent)
        {
            Skip = "The clone is here but upstream's decoder is not built at "
                + $"{Ft8Decoder.ExecutablePath}. Nothing built from the pin is committed either, so "
                + "this is expected on a fresh machine; build it with tools\\build-ft8-oracle.bat.";
        }
    }
}

/// <summary>
/// A fact that needs upstream's decoder to <em>work</em>, proven by a round trip through upstream's
/// own generator, not merely to exist.
/// </summary>
/// <remarks>
/// <b>A skip from this attribute is not a clean bill of health.</b> When it fires on a machine that
/// has both binaries, something is wrong with the instrument, and the reason says what.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresWorkingDecoderFactAttribute : FactAttribute
{
    public RequiresWorkingDecoderFactAttribute()
    {
        var (state, detail) = Ft8Decoder.ProbeUsability();
        if (state != Ft8Decoder.Usability.Usable)
        {
            Skip = $"upstream's decoder is not usable here — {state}: {detail}";
        }
    }
}
