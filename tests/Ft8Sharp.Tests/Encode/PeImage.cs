using System.Security.Cryptography;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Just enough of the PE format to read and write one field of an executable's optional header, and
/// to hash the bytes of its code section.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> Upstream's generator is a correct program that cannot survive its own
/// output on this platform: <c>demo/gen_ft8.c</c> puts a fifteen-second waveform on the stack in a
/// C99 variable length array, the systems <c>ft8_lib</c> is written for hand a main thread 8 MB, and
/// the Windows linker wrote the 1 MB default into this image. Windows reads that number out of the
/// image at process creation, so no way of <em>launching</em> the program helps. Widening it in a
/// copy is the only thing that does.
/// </para>
/// <para>
/// <b>What is being changed, precisely.</b> <c>SizeOfStackReserve</c> is a number the loader reads
/// to decide how much address space to reserve for the main thread. It is not code, it is not data
/// the program reads, and it is an input to no computation the generator performs. Two images
/// identical but for that field execute the same instructions over the same constants; the one with
/// the smaller reserve simply runs out of address space part way through and is killed.
/// </para>
/// <para>
/// <b>That is a claim and it is not assumed anywhere.</b> <see cref="OracleStackPatch"/> proves it
/// on these two files, every run, before a single tone is compared — whole-file equality but for the
/// bytes written, equal <c>.text</c> hashes, identical no-argument behaviour, and survival of a real
/// message. Where any of the four does not come out, the copy is discarded.
/// </para>
/// <para>
/// <b>Walked by hand rather than with a package.</b> <c>Ft8Sharp.csproj</c> may carry no reference
/// at all and the test project should not buy one to read eight bytes at a known offset. The offsets
/// here are the same ones <see cref="Ft8OracleDiagnosisTests"/>'s own reader already walks — this is
/// a writer added beside an existing reader, not new arithmetic — and the two readings are asserted
/// against each other in <see cref="Ft8OracleStackTests"/>.
/// </para>
/// </remarks>
internal static class PeImage
{
    /// <summary>Where <c>SizeOfStackReserve</c> sits in one particular image, and how wide it is.</summary>
    /// <param name="FileOffset">The absolute offset into the file of the field's first byte.</param>
    /// <param name="Width">8 for PE32+, 4 for PE32.</param>
    /// <param name="IsPe32Plus">Whether the optional header is the 64-bit form.</param>
    internal sealed record StackReserveField(long FileOffset, int Width, bool IsPe32Plus)
    {
        /// <summary>Every file offset this field covers, which is what a byte comparison must find.</summary>
        public IEnumerable<long> Offsets => Enumerable.Range(0, Width).Select(i => FileOffset + i);
    }

    /// <summary>
    /// Finds <c>SizeOfStackReserve</c> in the image at <paramref name="imagePath"/>.
    /// </summary>
    /// <remarks>
    /// The walk is the documented one: <c>e_lfanew</c> at <c>0x3C</c> gives the PE signature, the
    /// COFF header is twenty bytes, the optional header's magic says which form it is, and
    /// <c>SizeOfStackReserve</c> is at offset 72 of the optional header in both forms — eight bytes
    /// wide in PE32+ and four in PE32.
    /// </remarks>
    public static bool TryLocateStackReserve(
        string imagePath,
        out StackReserveField field,
        out string detail)
    {
        field = new StackReserveField(0, 0, false);
        try
        {
            using var stream = File.OpenRead(imagePath);
            using var reader = new BinaryReader(stream);

            if (reader.ReadUInt16() != 0x5A4D)
            {
                detail = "not a PE image: no MZ signature";
                return false;
            }

            stream.Position = 0x3C;
            var peOffset = reader.ReadUInt32();
            stream.Position = peOffset;
            if (reader.ReadUInt32() != 0x00004550)
            {
                detail = "not a PE image: no PE signature at e_lfanew";
                return false;
            }

            var optionalHeader = peOffset + 4 + 20;
            stream.Position = optionalHeader;
            var magic = reader.ReadUInt16();

            switch (magic)
            {
                case 0x20B:
                    field = new StackReserveField(optionalHeader + 72, 8, true);
                    detail = "PE32+ optional header, SizeOfStackReserve at offset 72, 8 bytes wide";
                    return true;
                case 0x10B:
                    field = new StackReserveField(optionalHeader + 72, 4, false);
                    detail = "PE32 optional header, SizeOfStackReserve at offset 72, 4 bytes wide";
                    return true;
                default:
                    detail = $"unrecognised optional header magic 0x{magic:X}";
                    return false;
            }
        }
        catch (Exception ex)
        {
            detail = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    /// <summary>Reads the stack reserve out of an image.</summary>
    public static ulong ReadStackReserve(string imagePath, StackReserveField field)
    {
        using var stream = File.OpenRead(imagePath);
        using var reader = new BinaryReader(stream);
        stream.Position = field.FileOffset;
        return field.IsPe32Plus ? reader.ReadUInt64() : reader.ReadUInt32();
    }

    /// <summary>
    /// Writes a new stack reserve into an image, touching those bytes and no others.
    /// </summary>
    /// <remarks>
    /// <b>No checksum is recomputed and nothing is re-signed.</b> The PE checksum is not verified by
    /// the loader for an ordinary user-mode executable, and repairing it would put this code's
    /// fingerprints on bytes outside the field — which is precisely what the whole-file comparison
    /// afterwards exists to rule out. A copy that differs by more than the width of this field is
    /// thrown away rather than explained.
    /// </remarks>
    public static void WriteStackReserve(string imagePath, StackReserveField field, ulong value)
    {
        if (!field.IsPe32Plus && value > uint.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"{value} does not fit the 4-byte SizeOfStackReserve of a PE32 image.");
        }

        using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream);
        stream.Position = field.FileOffset;
        if (field.IsPe32Plus)
        {
            writer.Write(value);
        }
        else
        {
            writer.Write((uint)value);
        }
    }

    /// <summary>Every file offset at which two files differ, and how many bytes each holds.</summary>
    /// <remarks>
    /// Reads both whole files. These images are a few hundred kilobytes; a streaming comparison
    /// would be cleverer and would earn nothing, and the offsets are the whole point — a count alone
    /// would let a byte changed somewhere else hide behind the right total.
    /// </remarks>
    public static (long LeftLength, long RightLength, IReadOnlyList<long> DifferingOffsets) Compare(
        string leftPath,
        string rightPath)
    {
        var left = File.ReadAllBytes(leftPath);
        var right = File.ReadAllBytes(rightPath);

        var differing = new List<long>();
        var common = Math.Min(left.Length, right.Length);
        for (var i = 0; i < common; i++)
        {
            if (left[i] != right[i])
            {
                differing.Add(i);
            }
        }

        // A length difference is reported as every trailing offset differing rather than silently
        // ignored, so a truncated or extended copy can never read as identical over its prefix.
        for (var i = common; i < Math.Max(left.Length, right.Length); i++)
        {
            differing.Add(i);
        }

        return (left.Length, right.Length, differing);
    }

    /// <summary>
    /// Hashes the raw bytes of a named section — <c>.text</c>, in practice, which is the executable
    /// code itself.
    /// </summary>
    /// <remarks>
    /// Weaker evidence than the whole-file comparison and kept because it is evidence of a different
    /// kind: the whole-file check says <em>only these bytes moved</em>, and this says <em>and none of
    /// them was an instruction</em>, without a reader having to know where the header ends.
    /// </remarks>
    public static bool TryHashSection(
        string imagePath,
        string sectionName,
        out string hash,
        out string detail)
    {
        hash = string.Empty;
        try
        {
            using var stream = File.OpenRead(imagePath);
            using var reader = new BinaryReader(stream);

            if (reader.ReadUInt16() != 0x5A4D)
            {
                detail = "not a PE image: no MZ signature";
                return false;
            }

            stream.Position = 0x3C;
            var peOffset = reader.ReadUInt32();
            stream.Position = peOffset;
            if (reader.ReadUInt32() != 0x00004550)
            {
                detail = "not a PE image: no PE signature at e_lfanew";
                return false;
            }

            // COFF header: Machine(2), NumberOfSections(2), ... SizeOfOptionalHeader at offset 16.
            stream.Position = peOffset + 4 + 2;
            var sectionCount = reader.ReadUInt16();
            stream.Position = peOffset + 4 + 16;
            var optionalHeaderSize = reader.ReadUInt16();

            var sectionTable = peOffset + 4 + 20 + optionalHeaderSize;
            for (var i = 0; i < sectionCount; i++)
            {
                var header = sectionTable + (i * 40);
                stream.Position = header;
                var nameBytes = reader.ReadBytes(8);
                var name = System.Text.Encoding.ASCII
                    .GetString(nameBytes)
                    .TrimEnd('\0');
                if (!string.Equals(name, sectionName, StringComparison.Ordinal))
                {
                    continue;
                }

                // Name(8), VirtualSize(4), VirtualAddress(4), SizeOfRawData(4), PointerToRawData(4).
                stream.Position = header + 16;
                var rawSize = reader.ReadUInt32();
                var rawPointer = reader.ReadUInt32();
                if (rawSize == 0 || rawPointer == 0)
                {
                    detail = $"section {sectionName} has no raw data in the file";
                    return false;
                }

                stream.Position = rawPointer;
                var body = reader.ReadBytes((int)rawSize);
                if (body.Length != rawSize)
                {
                    detail = $"section {sectionName} is truncated in the file";
                    return false;
                }

                hash = Convert.ToHexString(SHA256.HashData(body));
                detail = $"{sectionName}: {rawSize} bytes at file offset {rawPointer}";
                return true;
            }

            detail = $"no section named {sectionName} among {sectionCount}";
            return false;
        }
        catch (Exception ex)
        {
            detail = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }
}
