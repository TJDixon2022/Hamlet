namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// A temporary working copy of upstream's generator with a stack it can live on, together with the
/// proof that it is otherwise the same program.
/// </summary>
/// <remarks>
/// <para>
/// <b>The original is read-only here and is never opened for writing.</b> The pinned clone at
/// <see cref="ReferenceClone.Location"/> is the provenance of everything in this port; a modified
/// pin would make every provenance test in this tree a lie. What this does is copy the executable
/// out to <see cref="Path.GetTempPath"/>, write a larger <c>SizeOfStackReserve</c> into the copy,
/// and delete the copy when the run ends. Nothing patched enters the tree and nothing patched is
/// committed.
/// </para>
/// <para>
/// <b>Four proofs, every run, before the copy is used for anything.</b> They are the entire licence
/// for using a modified binary as an oracle, and a comparison run against a copy that failed any of
/// them may not be reported as bit-identity with <c>ft8_lib</c>:
/// </para>
/// <list type="number">
/// <item>the copy is byte-for-byte the original except at the offsets written, and the number of
/// differing bytes is exactly the width of that one field;</item>
/// <item>the <c>.text</c> sections of the two images hash the same, so no instruction moved;</item>
/// <item>run with no arguments, where the original already worked, the copy exits the same way and
/// prints the same bytes — a patched image printing its usage unchanged is direct evidence that its
/// code path is untouched;</item>
/// <item>and it now survives a real message, which is the thing the original could not do.</item>
/// </list>
/// <para>
/// <b>What would make this unnecessary.</b> A stack-size flag on the link line in
/// <c>tools\build-ft8-oracle.bat</c>, which is the owner's file. If the image is ever found already
/// asking for 8 MB or more, <see cref="Ensure"/> makes no copy and the original is used directly.
/// </para>
/// </remarks>
internal static class OracleStackPatch
{
    /// <summary>
    /// What to ask Windows for. Generous on purpose: this is an address-space reservation rather
    /// than committed memory, and 8 MB is what the platforms <c>ft8_lib</c> targets would give the
    /// main thread anyway.
    /// </summary>
    public const ulong RequestedReserve = 16UL * 1024 * 1024;

    /// <summary>
    /// The reserve at or above which no patch is needed, because the owner's own link line has
    /// already provided it.
    /// </summary>
    public const ulong SufficientReserve = 8UL * 1024 * 1024;

    /// <summary>Everything one attempt at the copy established, values and verdicts alike.</summary>
    internal sealed record Attempt
    {
        /// <summary>Whether a copy was made at all.</summary>
        public bool CopyMade { get; init; }

        /// <summary>Why no copy was made, when none was.</summary>
        public string Reason { get; init; } = string.Empty;

        /// <summary>The copy, or null when there is none.</summary>
        public string? CopyPath { get; init; }

        /// <summary>What the original image asks Windows for.</summary>
        public ulong OriginalReserve { get; init; }

        /// <summary>What was read back out of the copy after writing.</summary>
        public ulong CopyReserve { get; init; }

        /// <summary>How the field was located, in words.</summary>
        public string FieldDetail { get; init; } = string.Empty;

        /// <summary>The offsets written to.</summary>
        public IReadOnlyList<long> WrittenOffsets { get; init; } = [];

        /// <summary>Proof 1: the size of each file and every offset at which they differ.</summary>
        public long OriginalBytes { get; init; }

        public long CopyBytes { get; init; }

        public IReadOnlyList<long> DifferingOffsets { get; init; } = [];

        /// <summary>Proof 2: whether the two <c>.text</c> sections hash the same.</summary>
        public bool? TextHashesEqual { get; init; }

        public string TextHashDetail { get; init; } = string.Empty;

        /// <summary>Proof 3: the no-argument behaviour of each image.</summary>
        public int OriginalNoArgumentExit { get; init; }

        public int CopyNoArgumentExit { get; init; }

        public bool NoArgumentOutputIdentical { get; init; }

        public string NoArgumentShape { get; init; } = string.Empty;

        /// <summary>Proof 4: what the copy does with a real message.</summary>
        public int CopyMessageExit { get; init; }

        public long CopyWavBytes { get; init; }

        /// <summary>Proof 1 on its own terms: every byte that moved lies inside the field written.</summary>
        /// <remarks>
        /// <b>The test is containment, not equality of counts, and the difference matters.</b> The
        /// obvious expectation is that a copy differs from its original by exactly the width of the
        /// field — 8 bytes for PE32+. It does not, and it should not: 1 MB and 16 MB share six of
        /// their eight little-endian bytes, so only two of them move. Demanding all eight would fail
        /// a perfectly good patch, and demanding only a count of eight would accept six stray bytes
        /// elsewhere. What actually proves the point is that <em>no byte outside the field moved at
        /// all</em>, and that the file is the same length.
        /// </remarks>
        public bool OnlyTheFieldDiffers =>
            OriginalBytes == CopyBytes
            && DifferingOffsets.Count > 0
            && DifferingOffsets.Count <= WrittenOffsets.Count
            && DifferingOffsets.All(WrittenOffsets.Contains);

        /// <summary>Proof 4 on its own terms.</summary>
        public bool SurvivesAMessage => CopyMessageExit == 0;

        /// <summary>
        /// Whether all four proofs came out. <b>Only this licenses reporting a comparison against
        /// the copy as bit-identity with <c>ft8_lib</c>.</b>
        /// </summary>
        /// <remarks>
        /// The <c>.text</c> hash is allowed to be unavailable rather than unequal — if the section
        /// walk cannot find the sections at all, the whole-file comparison is the stronger of the two
        /// and stands alone. It is not allowed to be <em>unequal</em>.
        /// </remarks>
        public bool Proven =>
            CopyMade
            && OnlyTheFieldDiffers
            && TextHashesEqual != false
            && OriginalNoArgumentExit == CopyNoArgumentExit
            && NoArgumentOutputIdentical
            && SurvivesAMessage;

        /// <summary>Which of the four did not come out, for a skip reason or a report.</summary>
        public string ProofSummary =>
            $"whole-file: {(OnlyTheFieldDiffers ? $"{DifferingOffsets.Count} bytes differ and all of them are inside the field written" : $"{DifferingOffsets.Count} bytes differ, and not all of them are inside the field written")}; "
            + $".text hashes: {(TextHashesEqual switch { true => "equal", false => "DIFFERENT", _ => "unavailable" })}; "
            + $"no-argument behaviour: {(OriginalNoArgumentExit == CopyNoArgumentExit && NoArgumentOutputIdentical ? "identical" : "differs")}; "
            + $"real message: exit 0x{CopyMessageExit:X8}";
    }

    private static Attempt? _attempt;
    private static bool _cleanupRegistered;

    /// <summary>
    /// Makes the copy if one is needed and not already made, and returns what it established.
    /// </summary>
    /// <remarks>
    /// Once per test run. The copy costs a file copy and two process launches, and the answer cannot
    /// change inside a run.
    /// </remarks>
    public static Attempt Ensure()
    {
        if (_attempt is { } cached)
        {
            return cached;
        }

        _attempt = Make();
        return _attempt;
    }

    /// <summary>The patched copy where one was made and proven, and null otherwise.</summary>
    /// <remarks>
    /// <b>An unproven copy is not offered.</b> A copy that failed a proof is exactly the weakened
    /// oracle there was good reason to fear, and using it would produce a comparison nobody could
    /// stand behind.
    /// </remarks>
    public static string? ProvenCopyPath
    {
        get
        {
            var attempt = Ensure();
            return attempt.Proven ? attempt.CopyPath : null;
        }
    }

    private static Attempt Make()
    {
        if (ReferenceClone.Probe(out var reach) == ReferenceClone.Reach.Absent)
        {
            return new Attempt { Reason = $"the pinned clone is not on this machine: {reach}" };
        }

        var original = Ft8Oracle.ExecutablePath;
        if (!File.Exists(original))
        {
            return new Attempt { Reason = $"nothing is built at {original}" };
        }

        if (!PeImage.TryLocateStackReserve(original, out var field, out var fieldDetail))
        {
            return new Attempt { Reason = $"could not find SizeOfStackReserve: {fieldDetail}" };
        }

        var originalReserve = PeImage.ReadStackReserve(original, field);
        if (originalReserve >= SufficientReserve)
        {
            return new Attempt
            {
                OriginalReserve = originalReserve,
                FieldDetail = fieldDetail,
                Reason =
                    $"no patch is needed: the image already asks for {originalReserve} bytes, which is "
                    + $"at or above the {SufficientReserve} the generator's waveform needs. The owner's "
                    + "own link line has provided it.",
            };
        }

        // A directory of its own, so anything the run drops beside the executable lands somewhere
        // this can delete whole, and so the loader's first search path holds only what was put there.
        var folder = Path.Combine(
            Path.GetTempPath(),
            $"ft8-oracle-stack-{Guid.NewGuid():N}");

        string copy;
        try
        {
            Directory.CreateDirectory(folder);
            copy = Path.Combine(folder, Path.GetFileName(original));
            File.Copy(original, copy, overwrite: false);

            // Anything the loader would find beside the original has to be beside the copy too, or
            // the copy fails to start for a reason that has nothing to do with the patch.
            var sourceFolder = Path.GetDirectoryName(original);
            if (sourceFolder is not null)
            {
                foreach (var dll in Directory.EnumerateFiles(sourceFolder, "*.dll"))
                {
                    File.Copy(dll, Path.Combine(folder, Path.GetFileName(dll)), overwrite: true);
                }
            }
        }
        catch (Exception ex)
        {
            // Reported and not retried, and never retried against the original.
            return new Attempt
            {
                OriginalReserve = originalReserve,
                FieldDetail = fieldDetail,
                Reason = $"the copy could not be made: {ex.GetType().Name}: {ex.Message}",
            };
        }

        RegisterCleanup(folder);

        try
        {
            PeImage.WriteStackReserve(copy, field, RequestedReserve);
        }
        catch (Exception ex)
        {
            return new Attempt
            {
                OriginalReserve = originalReserve,
                FieldDetail = fieldDetail,
                CopyPath = copy,
                Reason = $"the reserve could not be written: {ex.GetType().Name}: {ex.Message}",
            };
        }

        var copyReserve = PeImage.ReadStackReserve(copy, field);
        var (originalBytes, copyBytes, differing) = PeImage.Compare(original, copy);

        var originalHashRead = PeImage.TryHashSection(original, ".text", out var originalHash, out var hashDetailA);
        var copyHashRead = PeImage.TryHashSection(copy, ".text", out var copyHash, out var hashDetailB);
        bool? textEqual = originalHashRead && copyHashRead
            ? string.Equals(originalHash, copyHash, StringComparison.Ordinal)
            : null;
        var hashDetail = originalHashRead && copyHashRead
            ? $"original {hashDetailA}; copy {hashDetailB}"
            : $"the section walk did not find .text in both images (original: {hashDetailA}; copy: "
              + $"{hashDetailB}), so this rests on the whole-file comparison, which is the stronger "
              + "of the two anyway";

        // Proof 3: the behaviour that already worked, run on both images.
        var originalUsage = Ft8Oracle.InvokeImage(original, null);
        var copyUsage = Ft8Oracle.InvokeImage(copy, null);
        var usageIdentical =
            string.Equals(originalUsage.StandardOutput, copyUsage.StandardOutput, StringComparison.Ordinal)
            && string.Equals(originalUsage.StandardError, copyUsage.StandardError, StringComparison.Ordinal);

        // Proof 4: the thing the original could not do.
        var wav = Path.Combine(folder, "canary.wav");
        Ft8Oracle.Run message;
        try
        {
            message = Ft8Oracle.InvokeImage(copy, wav, "CQ K1ABC FN42", wav);
        }
        catch (Exception ex)
        {
            message = new Ft8Oracle.Run(-1, string.Empty, $"{ex.GetType().Name}: {ex.Message}", -1);
        }

        try
        {
            if (File.Exists(wav))
            {
                File.Delete(wav);
            }
        }
        catch
        {
            // Untidy in the temp folder, never wrong, and never in the tree.
        }

        return new Attempt
        {
            CopyMade = true,
            CopyPath = copy,
            OriginalReserve = originalReserve,
            CopyReserve = copyReserve,
            FieldDetail = fieldDetail,
            WrittenOffsets = [.. field.Offsets],
            OriginalBytes = originalBytes,
            CopyBytes = copyBytes,
            DifferingOffsets = differing,
            TextHashesEqual = textEqual,
            TextHashDetail = hashDetail,
            OriginalNoArgumentExit = originalUsage.ExitCode,
            CopyNoArgumentExit = copyUsage.ExitCode,
            NoArgumentOutputIdentical = usageIdentical,
            NoArgumentShape = Ft8Oracle.Shape(copyUsage.StandardOutput.Replace('\n', ' ')),
            CopyMessageExit = message.ExitCode,
            CopyWavBytes = message.WavBytes,
            Reason = string.Empty,
        };
    }

    /// <summary>Deletes the copy and its folder when the test run ends.</summary>
    private static void RegisterCleanup(string folder)
    {
        if (_cleanupRegistered)
        {
            return;
        }

        _cleanupRegistered = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch
            {
                // A patched copy left in the system temp folder is untidy. It is outside the tree,
                // it is never committed, and it is not worth failing a run over.
            }
        };
    }
}
