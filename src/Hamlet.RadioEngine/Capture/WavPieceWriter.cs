using System.Buffers.Binary;

namespace Hamlet.RadioEngine.Capture;

/// <summary>
/// A 16-bit mono WAV written as the audio arrives, its sizes put in the header when it is closed (work instruction 549).
/// </summary>
/// <remarks>
/// <para>**THE SAME FILE <c>WavAudio.Write</c> MAKES**, 16-bit PCM with a 44-byte header, so a piece of an automatic capture
/// opens in everything a Record press does. What differs is that nothing is held: a five-minute piece at 48 kHz is 28.8 MB
/// on disk and a few kilobytes in memory.</para>
/// <para>**A PIECE CUT SHORT IS STILL A WAV.** The header is written first with sizes of nought and patched on
/// <see cref="Close"/>; a writer that dies before that leaves a file whose header understates it, which every reader
/// tolerates, rather than one with no header at all.</para>
/// </remarks>
public sealed class WavPieceWriter : IDisposable
{
    private const int HeaderBytes = 44;

    private readonly FileStream _stream;
    private byte[] _buffer = new byte[4096];
    private bool _closed;

    /// <summary>Open a piece for writing.</summary>
    /// <param name="path">The file.</param>
    /// <param name="sampleRate">Samples per second.</param>
    public WavPieceWriter(string path, int sampleRate)
    {
        var folder = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        FilePath = path;
        SampleRate = sampleRate;
        _stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        _stream.Write(Header(sampleRate, 0));
    }

    /// <summary>The file.</summary>
    public string FilePath { get; }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>How many samples are in the file.</summary>
    public long Samples { get; private set; }

    /// <summary>Append samples, clamped to full scale as <c>WavAudio.Write</c> clamps them.</summary>
    /// <param name="samples">The samples.</param>
    public void Write(ReadOnlySpan<float> samples)
    {
        if (_closed || samples.IsEmpty)
        {
            return;
        }

        if (_buffer.Length < samples.Length * 2)
        {
            _buffer = new byte[samples.Length * 2];
        }

        for (var i = 0; i < samples.Length; i++)
        {
            var value = (short)Math.Round(Math.Clamp(samples[i], -1f, 1f) * short.MaxValue);

            BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(i * 2), value);
        }

        _stream.Write(_buffer, 0, samples.Length * 2);
        Samples += samples.Length;
    }

    /// <summary>Put the sizes in the header and close the file.</summary>
    public void Close()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
        _stream.Position = 0;
        _stream.Write(Header(SampleRate, (int)Math.Min(int.MaxValue - 36, Samples * 2)));
        _stream.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose() => Close();

    private static byte[] Header(int sampleRate, int dataBytes)
    {
        var header = new byte[HeaderBytes];

        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4), 36 + dataBytes);
        "WAVE"u8.CopyTo(header.AsSpan(8));
        "fmt "u8.CopyTo(header.AsSpan(12));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), 16);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(20), 1);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(22), 1);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(24), sampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(28), sampleRate * 2);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(32), 2);
        BinaryPrimitives.WriteInt16LittleEndian(header.AsSpan(34), 16);
        "data"u8.CopyTo(header.AsSpan(36));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40), dataBytes);

        return header;
    }
}
