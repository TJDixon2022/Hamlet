using System.Diagnostics;
using System.Text;
using Xunit;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Upstream's own generator, run as a black box, and the one source of truth in this phase that
/// agreeing with ourselves cannot fake.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing this returns is ever committed.</b> The tones, the payload and the codeword upstream
/// emits are read at run time, compared, and dropped. What is recorded anywhere in this repository
/// is <em>whether it matched</em> and never <em>what it was</em>.
/// </para>
/// <para>
/// <b>The binary is not built here and is not ours.</b> It is produced outside the tree by
/// <c>tools\build-ft8-oracle.bat</c>, which is the owner's file, from the pinned clone at
/// <see cref="ReferenceClone.Location"/>. This class only locates it and runs it. No compiler is
/// invoked from inside a test process — that route was considered by unit 209 and judged a
/// workaround rather than a sanctioned one, and that judgment stands here.
/// </para>
/// <para>
/// <b>It skips rather than fails when the binary is absent</b>, in the same way and for the same
/// reason as <see cref="RequiresReferenceCloneFactAttribute"/>: a fresh clone on another machine
/// has neither the pin nor anything built from it, and must still come back green.
/// </para>
/// </remarks>
internal static class Ft8Oracle
{
    /// <summary>
    /// Where <c>tools\build-ft8-oracle.bat</c> puts the executable. One clearly named constant, so
    /// a re-pin or a relocation is a single edit rather than a search.
    /// </summary>
    private const string RelativeExecutablePath = @"build\gen_ft8.exe";

    /// <summary>The generator as the owner's script built it, wherever the clone is.</summary>
    /// <remarks>
    /// <b>This is the pin and it is read-only.</b> It is never opened for writing, never patched and
    /// never deleted. Where it cannot survive its own waveform, a copy of it is patched instead —
    /// see <see cref="ResolvedExecutablePath"/>.
    /// </remarks>
    public static string ExecutablePath =>
        Path.Combine(ReferenceClone.Location, RelativeExecutablePath);

    /// <summary>The image questions are actually put to.</summary>
    /// <remarks>
    /// <para>
    /// The original where it can answer, and a temporary copy with a wider
    /// <c>SizeOfStackReserve</c> where it cannot — but <b>only a copy that
    /// <see cref="OracleStackPatch.Attempt.Proven"/> has shown to be the same program in every byte
    /// that does any work</b>. An unproven copy is never offered here, so no comparison in this
    /// project can be run against one by accident.
    /// </para>
    /// <para>
    /// Resolved once per run and visible in <see cref="ProbeUsability"/>'s detail string, so a skip
    /// reason and a report both say which binary answered.
    /// </para>
    /// </remarks>
    public static string ResolvedExecutablePath => OracleStackPatch.ProvenCopyPath ?? ExecutablePath;

    /// <summary>Whether the image being questioned is a patched copy rather than the original.</summary>
    public static bool AnsweringImageIsAPatchedCopy =>
        !string.Equals(ResolvedExecutablePath, ExecutablePath, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether the oracle is on this machine and can be run.</summary>
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

    /// <summary>What one run of the generator produced.</summary>
    /// <param name="ExitCode">The process's own exit code.</param>
    /// <param name="StandardOutput">Everything it wrote to stdout, verbatim and never committed.</param>
    /// <param name="StandardError">Everything it wrote to stderr.</param>
    /// <param name="WavBytes">The size of the WAV it wrote, or -1 if it wrote none.</param>
    internal sealed record Run(int ExitCode, string StandardOutput, string StandardError, long WavBytes);

    /// <summary>
    /// Runs the generator on one message, writing its WAV to a fresh temporary file which is
    /// deleted before this returns.
    /// </summary>
    /// <remarks>
    /// The WAVs are roughly 300 KB apiece — a corpus of two hundred is 60 MB — so they are written
    /// under <see cref="Path.GetTempPath"/>, never under the tree, and removed as we go rather than
    /// at the end, so a run that throws part way through does not leave a pile behind.
    /// </remarks>
    public static Run Generate(string messageText)
    {
        var wav = Path.Combine(
            Path.GetTempPath(),
            $"ft8-oracle-{Guid.NewGuid():N}.wav");

        try
        {
            return Invoke(wav, messageText, wav);
        }
        finally
        {
            try
            {
                if (File.Exists(wav))
                {
                    File.Delete(wav);
                }
            }
            catch
            {
                // A WAV left in the system temp folder is untidy, never wrong, and never in the tree.
            }
        }
    }

    /// <summary>
    /// Runs the generator with whatever arguments are given, so its own usage text and its
    /// behaviour on a message it will not encode can be read rather than guessed at.
    /// </summary>
    /// <param name="wavToMeasure">The file whose size is reported, or null when none is expected.</param>
    public static Run Invoke(string? wavToMeasure, params string[] arguments) =>
        InvokeImage(ResolvedExecutablePath, wavToMeasure, arguments);

    /// <summary>
    /// Runs one named image, so the original and a patched copy of it can be held against each other
    /// rather than one of them standing in for the other.
    /// </summary>
    /// <remarks>
    /// <see cref="OracleStackPatch"/> is the only caller that names an image: everything else goes
    /// through <see cref="Invoke"/> and gets whichever image can answer.
    /// </remarks>
    public static Run InvokeImage(string imagePath, string? wavToMeasure, params string[] arguments)
    {
        {
            var start = new ProcessStartInfo(imagePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,

                // The clone is the working directory rather than the tree, so that anything the
                // generator chooses to drop beside itself lands outside this repository.
                WorkingDirectory = ReferenceClone.Location,
            };
            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            using var process = Process.Start(start)
                ?? throw new InvalidOperationException($"could not start {imagePath}");

            // Read both pipes before waiting. A generator that fills one of them while we block on
            // Exit would deadlock, and this one writes to both.
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30_000))
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

                throw new TimeoutException(
                    $"{imagePath} did not exit within 30 s for arguments "
                    + $"'{string.Join(' ', arguments)}'.");
            }

            var bytes = wavToMeasure is not null && File.Exists(wavToMeasure)
                ? new FileInfo(wavToMeasure).Length
                : -1L;
            return new Run(process.ExitCode, stdout.Result, stderr.Result, bytes);
        }
    }

    /// <summary>
    /// The tone sequence upstream printed, or an empty span when it printed none.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Deliberately strict about what a tone line is.</b> A parser that scavenges digits out of
    /// any line would happily read a frequency, a duration or a checksum as tones and then report
    /// agreement it never measured.
    /// </para>
    /// <para>
    /// <b>Two forms are accepted and both are exact.</b> Either the line is entirely
    /// whitespace-separated single digits in the FT8 alphabet, or one of its fields is an unbroken
    /// run of characters every one of which is in the alphabet — and in both cases there must be
    /// exactly as many of them as the modulation has channel symbols. The second form is the one
    /// upstream's generator actually uses: a short label, a colon, then the tones run together with
    /// no separator at all. Unit 210 could not know that, because the generator could not be made to
    /// print anything; the run-together form was added when it could.
    /// </para>
    /// <para>
    /// <b>The second form is no looser than the first.</b> A run of exactly
    /// <paramref name="expectedCount"/> characters, every one of them a digit between 0 and 7, is
    /// not something a frequency, a duration, a byte count or a checksum produces. A run one
    /// character short, or one carrying an 8, is refused — and both refusals are watched.
    /// </para>
    /// </remarks>
    public static bool TryReadTones(string standardOutput, int expectedCount, out byte[] tones)
    {
        foreach (var raw in standardOutput.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            // Form one: one field per tone.
            if (fields.Length == expectedCount
                && TryReadAlphabet(string.Concat(fields), expectedCount, out var spaced)
                && fields.All(f => f.Length == 1))
            {
                tones = spaced;
                return true;
            }

            // Form two: one field carrying every tone, run together.
            foreach (var field in fields)
            {
                if (TryReadAlphabet(field, expectedCount, out var packed))
                {
                    tones = packed;
                    return true;
                }
            }
        }

        tones = [];
        return false;
    }

    /// <summary>
    /// Reads a run of characters as tones, and only when there are exactly
    /// <paramref name="expectedCount"/> of them and every one is inside the eight-tone alphabet.
    /// </summary>
    private static bool TryReadAlphabet(string run, int expectedCount, out byte[] tones)
    {
        tones = [];
        if (run.Length != expectedCount)
        {
            return false;
        }

        var candidate = new byte[expectedCount];
        for (var i = 0; i < run.Length; i++)
        {
            if (run[i] < '0' || run[i] > '7')
            {
                return false;
            }

            candidate[i] = (byte)(run[i] - '0');
        }

        tones = candidate;
        return true;
    }

    /// <summary>
    /// A run of hex bytes upstream printed on a line carrying <paramref name="label"/>, or false
    /// when that line is not there.
    /// </summary>
    /// <remarks>
    /// Used for the payload and the codeword, which is what upgrades criterion 1 from a syndrome
    /// check against our own tables to a byte-for-byte comparison against upstream's own bits.
    /// </remarks>
    public static bool TryReadHexAfterLabel(string standardOutput, string label, out byte[] bytes)
    {
        foreach (var raw in standardOutput.Split('\n'))
        {
            var line = raw.Trim();
            var at = line.IndexOf(label, StringComparison.OrdinalIgnoreCase);
            if (at < 0)
            {
                continue;
            }

            // Upstream separates its label from its bytes with a colon, so the punctuation is
            // stepped over rather than read as the first field and mistaken for a non-hex run.
            var tail = line[(at + label.Length)..].TrimStart().TrimStart(':').TrimStart();
            var fields = tail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var collected = new List<byte>();
            foreach (var field in fields)
            {
                if (field.Length == 2
                    && Uri.IsHexDigit(field[0])
                    && Uri.IsHexDigit(field[1]))
                {
                    collected.Add(Convert.ToByte(field, 16));
                    continue;
                }

                break;
            }

            if (collected.Count > 0)
            {
                bytes = [.. collected];
                return true;
            }
        }

        bytes = [];
        return false;
    }

    /// <summary>Whether the oracle can actually answer a question, and if not, why not.</summary>
    internal enum Usability
    {
        /// <summary>It ran and produced a tone sequence.</summary>
        Usable,

        /// <summary>The pinned clone is not on this machine.</summary>
        CloneAbsent,

        /// <summary>The clone is here and nothing has been built from it.</summary>
        NotBuilt,

        /// <summary>The executable exists and does not survive a message.</summary>
        BuiltButWillNotRun,

        /// <summary>It ran, and printed no tone sequence we could read.</summary>
        RanButPrintedNoTones,
    }

    private static (Usability State, string Detail)? _usability;

    /// <summary>
    /// Asks the oracle one canary question, once per test run, and remembers the answer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why this is a probe and not an assumption.</b> A binary being present says nothing about
    /// whether it works. This one is present, is a sound build — with no arguments it prints its own
    /// usage — and dies on any real message, so a suite that gated only on <c>File.Exists</c> would
    /// have reported a comparison it never made.
    /// </para>
    /// <para>
    /// The result is cached because the gate is read once per test method and a process launch per
    /// attribute construction would be paid many times over for an answer that cannot change inside
    /// one run.
    /// </para>
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

            // Which image is going to answer, and why. A patch attempt that failed one of its four
            // proofs says so here, so a skip reason carries the diagnosis rather than only the
            // symptom.
            var patch = OracleStackPatch.Ensure();
            var which = AnsweringImageIsAPatchedCopy
                ? "a temporary copy of upstream's generator with a wider SizeOfStackReserve, proven "
                  + $"identical to the original but for that field ({patch.ProofSummary})"
                : $"upstream's generator as built, at {ExecutablePath}"
                  + (patch.CopyMade
                      ? $" — a patched copy was made and NOT used, because {patch.ProofSummary}"
                      : patch.Reason.Length > 0 ? $" — no patch was made: {patch.Reason}" : string.Empty);

            Run run;
            try
            {
                run = Generate("CQ K1ABC FN42");
            }
            catch (Exception ex)
            {
                return (Usability.BuiltButWillNotRun, $"{which}; {ex.GetType().Name}: {ex.Message}");
            }

            if (run.ExitCode != 0)
            {
                var named = run.ExitCode == unchecked((int)0xC00000FD)
                    ? " (STATUS_STACK_OVERFLOW)"
                    : string.Empty;
                return (Usability.BuiltButWillNotRun,
                    $"{ResolvedExecutablePath} exited {run.ExitCode} (0x{run.ExitCode:X8}){named} on "
                    + $"a message it should encode; the image asked was {which}");
            }

            return TryReadTones(run.StandardOutput, ToneSequenceLength, out _)
                ? (Usability.Usable, $"ran, and printed a tone sequence — the image asked was {which}")
                : (Usability.RanButPrintedNoTones,
                    "ran and exited zero, but printed no line this parser recognised as tones; the "
                    + $"image asked was {which}");
        }
    }

    /// <summary>
    /// How many channel symbols a tone line must carry to be one. Held here rather than reached for
    /// through the library so the parser cannot be made to agree with the port by construction.
    /// </summary>
    public const int ToneSequenceLength = 79;

    /// <summary>Describes a line without reproducing it, for a report that may not carry values.</summary>
    public static string Shape(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0)
        {
            return "(blank)";
        }

        var digits = trimmed.Count(char.IsAsciiDigit);
        var letters = trimmed.Count(char.IsAsciiLetter);
        var fields = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return $"{trimmed.Length} chars, {fields} fields, {letters} letters, {digits} digits";
    }
}

/// <summary>
/// A fact that skips itself when the pinned clone is absent <em>or</em> when upstream's generator
/// has not been built on this machine.
/// </summary>
/// <remarks>
/// <para>
/// Two runtime conditions, not one. The clone can be present with nothing built from it — which is
/// the normal state of a fresh machine, and was the state of this one until the toolchain arrived —
/// and the two want telling apart in the skip reason, because one of them is a missing download and
/// the other is a missing build step with a script that performs it.
/// </para>
/// <para>
/// <c>[Fact(Skip=...)]</c> is not the mechanism: a hard-coded skip is a test that never runs
/// anywhere, and the whole value of this comparison is that it <em>ran here</em>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresOracleFactAttribute : FactAttribute
{
    public RequiresOracleFactAttribute()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.Absent)
        {
            Skip = $"The pinned ft8_lib clone is not on this machine at {ReferenceClone.Location}. "
                + $"It is never committed, so this is expected on a fresh clone. {detail}";
            return;
        }

        if (!Ft8Oracle.IsPresent)
        {
            Skip = $"The clone is here but upstream's generator is not built at "
                + $"{Ft8Oracle.ExecutablePath}. Nothing built from the pin is committed either, so "
                + "this is expected on a fresh machine; build it with tools\\build-ft8-oracle.bat.";
        }
    }
}

/// <summary>
/// A fact that needs the oracle to <em>work</em>, not merely to exist.
/// </summary>
/// <remarks>
/// <para>
/// The distinction is not academic and this machine is why. Unit 210 found the generator built,
/// sound enough to print its own usage, and dying with a stack overflow on every real message —
/// so a gate that asked only whether the file was there would have skipped nothing and failed
/// everything, or worse, reported a comparison that never happened.
/// </para>
/// <para>
/// <b>A skip from this attribute is not a clean bill of health.</b> When it fires on a machine
/// that has the oracle, something is wrong with the oracle, and the reason says what.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresWorkingOracleFactAttribute : FactAttribute
{
    public RequiresWorkingOracleFactAttribute()
    {
        var (state, detail) = Ft8Oracle.ProbeUsability();
        if (state != Ft8Oracle.Usability.Usable)
        {
            Skip = $"upstream's generator is not usable here — {state}: {detail}";
        }
    }
}
