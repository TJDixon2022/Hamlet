using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// The oracle as it actually stands on this machine, and the working copy that lets it be asked a
/// question at all.
/// </summary>
/// <remarks>
/// <para>
/// <b>Measured here rather than inherited.</b> Everything previously written down about upstream's
/// generator — its stack reserve, its exit code, what it prints — came from one session's reading.
/// These tests take each of those afresh, because the owner may have rebuilt it since, and because a
/// diagnosis carried forward without being re-checked is how a phase spends a night on a problem
/// that no longer exists.
/// </para>
/// <para>
/// <b>Numbers and shapes only.</b> No tone, no payload, no codeword and no line of upstream's source
/// appears here. A stack size, an exit code and a file length are properties of a build, not data of
/// the algorithm.
/// </para>
/// </remarks>
public class Ft8OracleStackTests
{
    private readonly ITestOutputHelper _output;

    public Ft8OracleStackTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Whether the pin is on this machine and whether anything is built from it.
    /// </summary>
    /// <remarks>
    /// Deliberately not merged into the tests below. When this one skips, nothing else in the file
    /// could have run, and a reader should be able to see that in one line rather than deduce it
    /// from five identical skip reasons.
    /// </remarks>
    [Fact]
    public void WhetherTheCloneAndItsGeneratorAreOnThisMachine()
    {
        var reach = ReferenceClone.Probe(out var detail);
        _output.WriteLine($"pinned clone at {ReferenceClone.Location}: {reach} — {detail}");

        if (reach == ReferenceClone.Reach.Absent)
        {
            _output.WriteLine(
                "The clone is absent, so every comparison against upstream's own output skips rather "
                + "than fails. That is the standing rule for reference material: it is never "
                + "committed, so a fresh clone must still come back green.");
            return;
        }

        var built = File.Exists(Ft8Oracle.ExecutablePath);
        _output.WriteLine($"generator at {Ft8Oracle.ExecutablePath}: {(built ? "present" : "ABSENT")}");
        if (built)
        {
            _output.WriteLine($"generator size: {new FileInfo(Ft8Oracle.ExecutablePath).Length} bytes");
        }
    }

    /// <summary>
    /// What the built image asks Windows for, and therefore whether it needs a working copy at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Windows takes the stack reserve from the image at process creation, not from the parent, so
    /// no way of launching the generator can hand it more. If this reads the linker's 1 MB default
    /// and the generator wants 8 MB of waveform, the program is sound and the number is wrong.
    /// </para>
    /// <para>
    /// <b>If it reads 8 MB or more, the owner has put a stack-size flag on his own link line and no
    /// patching is needed.</b> That is the outcome this test most wants to find, and it is checked
    /// before anything is copied.
    /// </para>
    /// </remarks>
    [RequiresOracleFact]
    public void WhatTheGeneratorAsksWindowsForAndWhetherThatIsEnough()
    {
        Assert.True(
            PeImage.TryLocateStackReserve(Ft8Oracle.ExecutablePath, out var field, out var detail),
            $"could not find SizeOfStackReserve in the image: {detail}");

        var reserve = PeImage.ReadStackReserve(Ft8Oracle.ExecutablePath, field);

        _output.WriteLine($"image          : {Ft8Oracle.ExecutablePath}");
        _output.WriteLine($"optional header: {detail}");
        _output.WriteLine($"field offset   : {field.FileOffset} (0x{field.FileOffset:X}), {field.Width} bytes");
        _output.WriteLine($"stack reserve  : {reserve} bytes ({reserve / 1024.0 / 1024.0:F2} MB)");
        _output.WriteLine(
            reserve >= OracleStackPatch.SufficientReserve
                ? $"That is at or above the {OracleStackPatch.SufficientReserve} bytes the generator's "
                  + "waveform needs, so NO PATCH IS NEEDED — the owner's own link line has provided it."
                : $"That is below the {OracleStackPatch.SufficientReserve} bytes the generator's "
                  + "waveform needs. The image is sound and the number is too small.");

        Assert.True(reserve > 0, $"the image states no stack reserve: {detail}");
    }

    /// <summary>
    /// What the original image does with a real message, and what it does with no arguments at all.
    /// </summary>
    /// <remarks>
    /// The second half is the reference behaviour the patched copy is held against. A program that
    /// prints its own usage cleanly is a sound build; one that dies on a message it should encode is
    /// a sound build meeting a platform limit, and telling those two apart is the whole point of
    /// running it twice.
    /// </remarks>
    [RequiresOracleFact]
    public void WhatTheOriginalImageDoesWithAMessageAndWithNothing()
    {
        var wav = Path.Combine(Path.GetTempPath(), $"ft8-oracle-original-{Guid.NewGuid():N}.wav");
        Ft8Oracle.Run message;
        try
        {
            message = Ft8Oracle.InvokeImage(Ft8Oracle.ExecutablePath, wav, "CQ K1ABC FN42", wav);
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
                // Outside the tree either way.
            }
        }

        _output.WriteLine("--- the original, given a real message ---");
        _output.WriteLine($"exit code : {message.ExitCode} (0x{message.ExitCode:X8})"
            + (message.ExitCode == unchecked((int)0xC00000FD) ? " — STATUS_STACK_OVERFLOW" : string.Empty));
        _output.WriteLine($"wav       : {(message.WavBytes < 0 ? "none written" : $"{message.WavBytes} bytes")}");
        _output.WriteLine($"stdout    : {Ft8Oracle.Shape(message.StandardOutput.Replace('\n', ' '))}");

        var usage = Ft8Oracle.InvokeImage(Ft8Oracle.ExecutablePath, null);
        _output.WriteLine(string.Empty);
        _output.WriteLine("--- the original, given no arguments (the reference behaviour) ---");
        _output.WriteLine($"exit code : {usage.ExitCode} (0x{usage.ExitCode:X8})");
        _output.WriteLine($"stdout    : {Ft8Oracle.Shape(usage.StandardOutput.Replace('\n', ' '))}");
        _output.WriteLine($"stderr    : {Ft8Oracle.Shape(usage.StandardError.Replace('\n', ' '))}");
        _output.WriteLine($"stdout lines: {usage.StandardOutput.Split('\n').Length}");

        Assert.True(
            usage.StandardOutput.Length + usage.StandardError.Length > 0,
            "the generator printed nothing at all with no arguments, which contradicts its own usage "
            + "text and would mean this is not the build the diagnosis was taken from.");
    }

    /// <summary>
    /// The working copy, and the four proofs that it is the same program.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This test is the licence for everything downstream of it.</b> A comparison run against a
    /// copy that failed one of these may not be reported as bit-identity with <c>ft8_lib</c> — an
    /// oracle shown to differ from the original in exactly its stack reservation and nothing else is
    /// not a modified oracle in any sense that bears on the comparison, and an oracle merely
    /// believed to be that is.
    /// </para>
    /// <para>
    /// It asserts rather than merely reports, because <see cref="Ft8Oracle.ResolvedExecutablePath"/>
    /// hands the copy to every other test in the project, and a proof that only printed would let a
    /// weakened oracle answer a criterion.
    /// </para>
    /// </remarks>
    [RequiresOracleFact]
    public void TheWorkingCopyIsTheSameProgramWithAStackItCanLiveOn()
    {
        var patch = OracleStackPatch.Ensure();

        if (!patch.CopyMade)
        {
            _output.WriteLine($"No copy was made. {patch.Reason}");
            _output.WriteLine(
                patch.OriginalReserve >= OracleStackPatch.SufficientReserve
                    ? "This is the good outcome: the original is used directly."
                    : "This is a failure to reach the oracle and criterion 2 stays open.");

            // A patch that was not needed is not a failure; a patch that was needed and did not
            // happen is, and the difference is the reserve rather than the absence of a file.
            Assert.True(
                patch.OriginalReserve >= OracleStackPatch.SufficientReserve
                || ReferenceClone.Probe(out _) == ReferenceClone.Reach.Absent
                || !File.Exists(Ft8Oracle.ExecutablePath),
                $"the image needs a wider stack and no copy was made: {patch.Reason}");
            return;
        }

        _output.WriteLine($"original : {Ft8Oracle.ExecutablePath}");
        _output.WriteLine($"copy     : {patch.CopyPath}");
        _output.WriteLine($"field    : {patch.FieldDetail}");
        _output.WriteLine(
            $"reserve  : {patch.OriginalReserve} bytes in the original, {patch.CopyReserve} in the "
            + $"copy ({patch.CopyReserve / 1024.0 / 1024.0:F2} MB)");
        _output.WriteLine(string.Empty);

        _output.WriteLine("PROOF 1 — byte-for-byte identical except at the patched offsets");
        _output.WriteLine($"  original: {patch.OriginalBytes} bytes; copy: {patch.CopyBytes} bytes");
        _output.WriteLine($"  bytes differing : {patch.DifferingOffsets.Count}, out of a field {patch.WrittenOffsets.Count} bytes wide");
        _output.WriteLine($"  offsets written : {string.Join(", ", patch.WrittenOffsets)}");
        _output.WriteLine($"  offsets differing: {string.Join(", ", patch.DifferingOffsets.Take(32))}"
            + (patch.DifferingOffsets.Count > 32 ? " …" : string.Empty));
        _output.WriteLine(
            "  Fewer bytes moved than the field is wide, and that is right rather than suspicious: "
            + "the old and new reserves share six of their eight little-endian bytes. What proves "
            + "the point is that every byte that moved is inside the field and none outside it did.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("PROOF 2 — the executable code is untouched");
        _output.WriteLine($"  .text hashes: {patch.TextHashesEqual switch
        {
            true => "EQUAL",
            false => "DIFFERENT",
            _ => "unavailable",
        }}");
        _output.WriteLine($"  {patch.TextHashDetail}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("PROOF 3 — identical behaviour where the original already worked");
        _output.WriteLine($"  no-argument exit: original {patch.OriginalNoArgumentExit}, copy {patch.CopyNoArgumentExit}");
        _output.WriteLine($"  no-argument output identical: {patch.NoArgumentOutputIdentical}");
        _output.WriteLine($"  what the copy printed: {patch.NoArgumentShape}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("PROOF 4 — it now survives a real message");
        _output.WriteLine($"  exit code: {patch.CopyMessageExit} (0x{patch.CopyMessageExit:X8})"
            + (patch.CopyMessageExit == unchecked((int)0xC00000FD) ? " — STATUS_STACK_OVERFLOW, the patch did not take" : string.Empty));
        _output.WriteLine($"  wav written: {(patch.CopyWavBytes < 0 ? "none" : $"{patch.CopyWavBytes} bytes")}");

        _output.WriteLine(string.Empty);
        _output.WriteLine($"ALL FOUR: {(patch.Proven ? "PROVEN" : "NOT PROVEN")} — {patch.ProofSummary}");

        Assert.Equal(patch.OriginalBytes, patch.CopyBytes);
        Assert.NotEmpty(patch.DifferingOffsets);
        Assert.All(
            patch.DifferingOffsets,
            offset => Assert.Contains(offset, patch.WrittenOffsets));
        Assert.True(
            patch.DifferingOffsets.Count <= patch.WrittenOffsets.Count,
            "more bytes moved than the field is wide, which cannot happen if only the field was written.");
        Assert.NotEqual((bool?)false, patch.TextHashesEqual);
        Assert.Equal(patch.OriginalNoArgumentExit, patch.CopyNoArgumentExit);
        Assert.True(
            patch.NoArgumentOutputIdentical,
            "the patched copy printed something different from the original with no arguments, which "
            + "would mean the patch reached a code path rather than a loader field.");
        Assert.Equal(OracleStackPatch.RequestedReserve, patch.CopyReserve);
        Assert.True(
            patch.SurvivesAMessage,
            $"the copy still exits 0x{patch.CopyMessageExit:X8} on a real message; the reserve read "
            + $"back out of its header is {patch.CopyReserve} bytes, so the patch "
            + $"{(patch.CopyReserve == OracleStackPatch.RequestedReserve ? "took and something else is killing it" : "did not take")}.");
    }

    /// <summary>
    /// Which image the rest of the project is going to question, stated once and in one place.
    /// </summary>
    /// <remarks>
    /// A report that says <em>the comparison ran</em> without saying <em>against what</em> is worth
    /// very little, and a reader should not have to infer it from a skip reason.
    /// </remarks>
    [RequiresOracleFact]
    public void WhichImageAnswersTheComparison()
    {
        var (state, detail) = Ft8Oracle.ProbeUsability();

        _output.WriteLine($"resolved image : {Ft8Oracle.ResolvedExecutablePath}");
        _output.WriteLine($"is a patched copy: {Ft8Oracle.AnsweringImageIsAPatchedCopy}");
        _output.WriteLine($"usability      : {state}");
        _output.WriteLine($"detail         : {detail}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "Nothing this image produces is committed. The tones, the payload, the codeword and the "
            + "WAVs are read at run time, compared, and dropped; the patched copy is deleted when the "
            + "run ends. What is recorded is whether it matched and never what it was.");
    }

    /// <summary>
    /// The two independent readings of the same header field agree.
    /// </summary>
    /// <remarks>
    /// <see cref="Ft8OracleDiagnosisTests"/> walks the optional header with its own reader written
    /// for a different unit. <see cref="PeImage"/> walks it again to find the offset to write to. If
    /// those two ever disagreed, the writer would be writing somewhere the reader is not looking, and
    /// the whole-file comparison would be checking the wrong bytes.
    /// </remarks>
    [RequiresOracleFact]
    public void TheOffsetWrittenToIsTheOffsetTheOtherReaderReadsFrom()
    {
        Assert.True(
            PeImage.TryLocateStackReserve(Ft8Oracle.ExecutablePath, out var field, out var detail),
            detail);

        // The independent reader's own arithmetic, restated rather than called, because it is
        // private to its own test class: e_lfanew at 0x3C, PE signature, twenty-byte COFF header,
        // SizeOfStackReserve at offset 72 of the optional header.
        using var stream = File.OpenRead(Ft8Oracle.ExecutablePath);
        using var reader = new BinaryReader(stream);
        stream.Position = 0x3C;
        var peOffset = reader.ReadUInt32();
        var expected = (long)peOffset + 4 + 20 + 72;

        _output.WriteLine($"e_lfanew                : {peOffset}");
        _output.WriteLine($"offset from the reader  : {expected}");
        _output.WriteLine($"offset the writer targets: {field.FileOffset}");

        Assert.Equal(expected, field.FileOffset);
    }
}
