using System.Diagnostics;
using System.Globalization;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>The shack command: one capture in, one committed fixture out, no editing afterwards.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SPLIT, STATED HONESTLY, BECAUSE IT DECIDES WHAT THIS UNIT COULD CLOSE.</b>
/// </para>
/// <list type="bullet">
/// <item>
/// <b>Reachable on the development machine and tested here:</b> the hashing
/// (<see cref="Ft8CaptureFixture.HashOf"/>), the row parsing
/// (<see cref="WsjtxDecodeLines.Parse"/>), the fixture writing
/// (<see cref="WriteFixture"/>), the loud refusal when the decoder is not found
/// (<see cref="LocateDecoder"/>), and the loud refusal when it produced nothing
/// (<see cref="RowsFrom"/>). All of these are unit-tested against decode text committed as a test
/// input under <c>tests/fixtures/ft8/parser-inputs/</c>.
/// </item>
/// <item>
/// <b>NOT reachable here and therefore UNEXERCISED:</b> <see cref="Run"/>'s actual invocation of the
/// decoder and the real rows coming back from it. <b>There is no WSJT-X on this machine and no unit
/// may assume one</b> (the phase ruling of 2026-09-04), and <c>decode_ft8.exe</c> is not a
/// substitute - it is <c>ft8_lib</c>, which is the thing being improved on. <b>Tim's first run at
/// the shack is what exercises this half</b>, and the unit that reports it says so.
/// </item>
/// </list>
/// <para>
/// <b>It refuses to write a half-fixture.</b> The file is built whole in memory, written to a
/// temporary name in the destination folder, read back through <see cref="Ft8CaptureFixture.Read"/>
/// and checked against the capture before it is moved into place. A file that exists but is
/// incomplete is worse than no file, because the reader will read it happily.
/// </para>
/// </remarks>
internal static class Ft8FixtureGenerator
{
    /// <summary>The environment variable that names the decoder, checked second.</summary>
    internal const string DecoderVariable = "WSJTX_DECODER";

    /// <summary>
    /// <b>Where the decoder is looked for, in order, and this list is the documentation.</b>
    /// </summary>
    /// <remarks>
    /// <b>These paths are the standard WSJT-X install layout on Windows and are not verified on this
    /// machine</b> - there is nothing here to verify them against. They are a convenience; the
    /// explicit argument and <see cref="DecoderVariable"/> are the routes that are guaranteed to
    /// work, and the refusal names every place it looked so a wrong guess costs one line of output
    /// rather than an investigation.
    /// </remarks>
    internal static readonly string[] CandidatePaths =
    [
        @"C:\WSJT\wsjtx\bin\jt9.exe",
        @"C:\Program Files\WSJT\wsjtx\bin\jt9.exe",
        @"C:\Program Files (x86)\WSJT\wsjtx\bin\jt9.exe",
        @"C:\Program Files\WSJT-X\bin\jt9.exe",
    ];

    /// <summary>
    /// <b>Finds the decoder, or refuses and says everywhere it looked.</b>
    /// </summary>
    /// <param name="explicitPath">What the caller passed on the command line, if anything.</param>
    /// <param name="lookup">
    /// How the environment is read and how existence is tested, so the refusal can be watched on a
    /// machine where nothing is installed - which is every machine this will ever be tested on.
    /// </param>
    internal static string LocateDecoder(string? explicitPath, IDecoderLookup lookup)
    {
        var looked = new List<string>();

        if (explicitPath is { Length: > 0 })
        {
            if (lookup.Exists(explicitPath))
            {
                return explicitPath;
            }

            throw new Ft8FixtureException(
                "(no fixture yet)",
                "(no capture yet)",
                $"the decoder was given as \"{explicitPath}\" and there is nothing there. An explicit "
                + "path is not searched for elsewhere: if it is wrong, saying so is more useful than "
                + "quietly running something else.");
        }

        if (lookup.Variable(DecoderVariable) is { Length: > 0 } configured)
        {
            if (lookup.Exists(configured))
            {
                return configured;
            }

            looked.Add($"{DecoderVariable}={configured}");
        }

        foreach (var candidate in CandidatePaths)
        {
            if (lookup.Exists(candidate))
            {
                return candidate;
            }

            looked.Add(candidate);
        }

        throw new Ft8FixtureException(
            "(no fixture yet)",
            "(no capture yet)",
            "WSJT-X's decoder was not found, so no fixture was written. Looked in order at: "
            + string.Join("; ", looked)
            + $". Pass it with --decoder <path> or set {DecoderVariable}. NOTHING IS SUBSTITUTED FOR "
            + "IT - not decode_ft8.exe, which is ft8_lib and is the thing this project is measuring "
            + "itself against, and not this project's own decoder, which would make the fixture a "
            + "measurement of Hamlet against Hamlet.");
    }

    /// <summary>
    /// <b>The decoder's output as fixture rows, refusing loudly when it produced none.</b>
    /// </summary>
    internal static IReadOnlyList<Ft8FixtureRow> RowsFrom(string output, string capture, string decoder)
    {
        var rows = WsjtxDecodeLines.Parse(output, $"{decoder} over {capture}");

        if (rows.Count == 0)
        {
            throw new Ft8FixtureException(
                "(no fixture written)",
                capture,
                $"{decoder} produced no decode lines at all for this capture, so there is nothing to "
                + "write. A zero-row fixture is not a measurement that found nothing - it would score "
                + "every decoder as having missed nothing, forever. If the capture really is empty it "
                + "is not a scoreboard and no fixture belongs beside it.");
        }

        return rows;
    }

    /// <summary>
    /// <b>Builds the whole fixture from a capture and a decoder's output. Nothing is written.</b>
    /// </summary>
    /// <param name="capturePath">The audio. Hashed here, from its bytes.</param>
    /// <param name="decoderOutput">Whatever the decoder wrote.</param>
    /// <param name="decoderName">Named in the generator field, so provenance has a subject.</param>
    /// <param name="utc">When the capture was taken, ISO 8601 with a Z.</param>
    /// <param name="sampleRate">The capture's rate, read from its own header by the caller.</param>
    internal static Ft8CaptureFixture Build(
        string capturePath,
        string decoderOutput,
        string decoderName,
        string utc,
        int sampleRate)
    {
        if (!File.Exists(capturePath))
        {
            throw new Ft8FixtureException(
                "(no fixture written)",
                capturePath,
                "the capture is not there, so nothing can be hashed and no fixture can be written.");
        }

        var capture = Path.GetFileName(capturePath);
        var rows = RowsFrom(decoderOutput, capture, decoderName);

        // provenance is wsjtx ONLY here, and only after a real decoder run returned real rows.
        // Nothing else in this repository writes that token.
        return new Ft8CaptureFixture(
            Ft8CaptureFixture.CurrentFormat,
            capture,
            utc,
            Ft8CaptureFixture.HashOf(capturePath),
            sampleRate,
            Ft8CaptureFixture.ProvenanceWsjtx,
            decoderName,
            rows,
            Path.ChangeExtension(capturePath, null) + Ft8CaptureFixture.Extension);
    }

    /// <summary>
    /// <b>Writes the fixture, whole or not at all.</b>
    /// </summary>
    /// <remarks>
    /// Built in memory, written under a temporary name in the destination folder, <b>read back and
    /// checked against the capture</b>, and only then moved into place. If anything fails the
    /// temporary file is deleted and the destination is untouched. A half-written fixture is worse
    /// than none: the reader would read it happily and every count taken from it would be short by an
    /// unknown number of rows.
    /// </remarks>
    internal static string WriteFixture(Ft8CaptureFixture fixture, IReadOnlyList<string>? preamble = null)
    {
        var destination = fixture.FixturePath;
        var folder = Path.GetDirectoryName(Path.GetFullPath(destination))
            ?? throw new Ft8FixtureException(
                destination, fixture.CaptureName, "the fixture path has no folder to write into.");

        Directory.CreateDirectory(folder);
        var temporary = Path.Combine(folder, $".{Guid.NewGuid():N}.fixture.partial");

        try
        {
            File.WriteAllText(temporary, fixture.ToFileText(preamble));

            // Read back through the SAME reader every later session uses. A fixture that this
            // generator can write and that reader cannot read is a fixture that fails six units from
            // now instead of now.
            var readBack = Ft8CaptureFixture.Read(temporary);
            if (!string.Equals(readBack.Sha256, fixture.Sha256, StringComparison.Ordinal)
                || readBack.Rows.Count != fixture.Rows.Count)
            {
                throw new Ft8FixtureException(
                    destination,
                    fixture.CaptureName,
                    $"what was written did not read back as what was built - {readBack.Rows.Count} "
                    + $"rows against {fixture.Rows.Count}, digest {readBack.Sha256} against "
                    + $"{fixture.Sha256}. Nothing was moved into place.");
            }

            File.Move(temporary, destination, overwrite: true);
            return destination;
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    /// <summary>
    /// <b>The whole command. THE INVOCATION HALF OF THIS IS UNEXERCISED ON THIS MACHINE.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Everything this calls is tested except <see cref="Process"/> itself and what comes back from
    /// it. There is no WSJT-X here to run and none may be assumed, so <b>Tim's first run at the shack
    /// is what exercises this path</b>. If the decoder prints something this parser refuses, the
    /// refusal carries the line verbatim and nothing is written.
    /// </para>
    /// <para>
    /// <b>The arguments are the shape jt9 is invoked with for an FT8 file.</b> They are not verified
    /// on this machine and <c>--arguments</c> overrides them for exactly that reason: a wrong guess
    /// about a switch must be correctable at the shack without an edit to this file, because Tim does
    /// not edit files.
    /// </para>
    /// </remarks>
    internal static string Run(
        string capturePath,
        string utc,
        int sampleRate,
        string? decoderPath,
        string? decoderArguments,
        IDecoderLookup lookup,
        Action<string> log)
    {
        var decoder = LocateDecoder(decoderPath, lookup);
        var arguments = decoderArguments ?? $"-8 \"{capturePath}\"";

        log($"decoder   {decoder}");
        log($"arguments {arguments}");
        log($"capture   {capturePath}");

        var start = new ProcessStartInfo(decoder, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(capturePath)) ?? ".",
        };

        using var process = Process.Start(start)
            ?? throw new Ft8FixtureException(
                "(no fixture written)",
                Path.GetFileName(capturePath),
                $"{decoder} could not be started at all. Nothing was written.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Ft8FixtureException(
                "(no fixture written)",
                Path.GetFileName(capturePath),
                $"{decoder} exited {process.ExitCode.ToString(CultureInfo.InvariantCulture)}. Nothing "
                + $"was written. It said on stderr: {error.Trim()}");
        }

        var fixture = Build(capturePath, output, $"{Path.GetFileName(decoder)} {arguments}", utc, sampleRate);

        return WriteFixture(
            fixture,
            [
                string.Empty,
                "Generated at the shack by tools/Ft8FixtureMaker. The rows are a real WSJT-X run's",
                "output over the capture beside this file, and provenance is \"wsjtx\" for that",
                "reason and no other. See docs/ft8-capture-fixture-format.md.",
            ]);
    }

    /// <summary>How the generator asks about the world, so the refusals can be watched without one.</summary>
    internal interface IDecoderLookup
    {
        /// <summary>Whether a file is there.</summary>
        bool Exists(string path);

        /// <summary>An environment variable, or null.</summary>
        string? Variable(string name);
    }

    /// <summary>The real one: the file system and the process environment.</summary>
    internal sealed class RealLookup : IDecoderLookup
    {
        /// <inheritdoc />
        public bool Exists(string path) => File.Exists(path);

        /// <inheritdoc />
        public string? Variable(string name) => Environment.GetEnvironmentVariable(name);
    }
}
