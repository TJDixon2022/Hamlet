using System.Buffers.Binary;
using System.Text;

namespace Hamlet.RadioEngine.Audio;

/// <summary>Mono audio with its sample rate.</summary>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="Samples">Samples in [-1, 1].</param>
public sealed record MonoAudio(int SampleRate, float[] Samples)
{
    /// <summary>How long the audio runs.</summary>
    public TimeSpan Duration
        => SampleRate <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((double)Samples.Length / SampleRate);
}

/// <summary>
/// Reads and writes the one WAV flavor Hamlet needs: mono, 16-bit PCM.
/// </summary>
/// <remarks>
/// <para>HM-DEC-007 makes this load-bearing rather than a utility. Decoders
/// are built and tested against WAV fixtures, every decoder bug becomes a
/// replayable case, and §0.0.1 requires that raw audio can be captured to WAV
/// on demand. A wrong decode with its input attached is a regression test; a
/// wrong decode without one is an argument.</para>
/// <para>Deliberately narrow. A general WAV reader is a pile of chunk handling
/// and format conversion for formats Hamlet will never meet, and every branch
/// of it is a place to be subtly wrong about somebody's evidence. Sixteen-bit
/// mono PCM is what this writes, and anything else it is handed is refused out
/// loud rather than guessed at.</para>
/// </remarks>
public static class WavAudio
{
    private const int HeaderBytes = 44;
    private const short PcmFormat = 1;
    private const short FloatFormat = 3;

    /// <summary>Write mono audio as 16-bit PCM.</summary>
    /// <param name="path">Destination file.</param>
    /// <param name="audio">The audio to write.</param>
    public static void Write(string path, MonoAudio audio)
    {
        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        using var stream = File.Create(path);
        Write(stream, audio);
    }

    /// <summary>Write mono audio as 16-bit PCM to a stream.</summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="audio">The audio to write.</param>
    public static void Write(Stream stream, MonoAudio audio)
    {
        var samples = audio.Samples;
        var dataBytes = samples.Length * 2;
        var header = new byte[HeaderBytes];

        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4), 36 + dataBytes);
        "WAVE"u8.CopyTo(header.AsSpan(8));
        "fmt "u8.CopyTo(header.AsSpan(12));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), 16);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(20), PcmFormat);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(22), 1);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(24), audio.SampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(28), audio.SampleRate * 2);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(32), 2);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(34), 16);
        "data"u8.CopyTo(header.AsSpan(36));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40), dataBytes);

        stream.Write(header);

        var buffer = new byte[dataBytes];
        for (var i = 0; i < samples.Length; i++)
        {
            // Clamped before scaling, so a sample that overshot cannot wrap
            // around into the opposite polarity and turn a loud signal into a
            // burst of noise in the evidence.
            var clamped = Math.Clamp(samples[i], -1f, 1f);
            var value = (short)Math.Round(clamped * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(i * 2), value);
        }

        stream.Write(buffer);
    }

    /// <summary>Read a mono WAV file.</summary>
    /// <param name="path">Source file.</param>
    /// <returns>The audio.</returns>
    /// <exception cref="InvalidDataException">The file is not mono 16-bit or 32-bit float PCM.</exception>
    public static MonoAudio Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    /// <summary>Read mono audio from a stream.</summary>
    /// <param name="stream">Source stream.</param>
    /// <returns>The audio.</returns>
    /// <exception cref="InvalidDataException">The stream is not a mono PCM WAV.</exception>
    public static MonoAudio Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        if (new string(reader.ReadChars(4)) != "RIFF")
        {
            throw new InvalidDataException("not a RIFF file");
        }

        reader.ReadInt32();

        if (new string(reader.ReadChars(4)) != "WAVE")
        {
            throw new InvalidDataException("not a WAVE file");
        }

        short format = 0;
        short channels = 0;
        var sampleRate = 0;
        short bitsPerSample = 0;

        while (stream.Position < stream.Length)
        {
            var id = new string(reader.ReadChars(4));
            var size = reader.ReadInt32();

            if (id == "fmt ")
            {
                format = reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();

                // Skip any extension bytes this chunk carries.
                for (var i = 16; i < size; i++)
                {
                    reader.ReadByte();
                }

                continue;
            }

            if (id == "data")
            {
                if (channels != 1)
                {
                    throw new InvalidDataException(
                        $"expected mono, found {channels} channels");
                }

                return format switch
                {
                    PcmFormat when bitsPerSample == 16 => ReadPcm16(reader, size, sampleRate),
                    FloatFormat when bitsPerSample == 32 => ReadFloat32(reader, size, sampleRate),
                    _ => throw new InvalidDataException(
                        $"expected 16-bit PCM or 32-bit float, found format {format} at {bitsPerSample} bits"),
                };
            }

            // Some other chunk. Chunks are word-aligned, so an odd size is
            // followed by a pad byte that is not counted in the size.
            stream.Seek(size + (size & 1), SeekOrigin.Current);
        }

        throw new InvalidDataException("no data chunk");
    }

    private static MonoAudio ReadPcm16(BinaryReader reader, int byteCount, int sampleRate)
    {
        var count = byteCount / 2;
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = reader.ReadInt16() / (float)short.MaxValue;
        }

        return new MonoAudio(sampleRate, samples);
    }

    private static MonoAudio ReadFloat32(BinaryReader reader, int byteCount, int sampleRate)
    {
        var count = byteCount / 4;
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = reader.ReadSingle();
        }

        return new MonoAudio(sampleRate, samples);
    }
}
