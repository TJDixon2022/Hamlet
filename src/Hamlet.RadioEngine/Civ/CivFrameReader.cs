namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// Incremental CI-V frame parser. Serial data arrives in arbitrary chunks —
/// half a frame, three frames, noise between frames — so this accumulates
/// bytes and yields only complete, well-formed frames. Malformed runs are
/// skipped, never guessed at (§0.0), and counted so the record can say so
/// (§0.0.1).
/// </summary>
public sealed class CivFrameReader
{
    private readonly List<byte> _buffer = new(64);

    /// <summary>Bytes discarded while hunting for a preamble — noise, or a
    /// partial frame from before the reader attached. Diagnosable, not silent.</summary>
    public long DiscardedByteCount { get; private set; }

    /// <summary>
    /// Feed a chunk of received bytes; returns every frame completed by it,
    /// in arrival order. Returns an empty list when no frame completed.
    /// </summary>
    public IReadOnlyList<CivFrame> Feed(ReadOnlySpan<byte> chunk)
    {
        foreach (var b in chunk)
        {
            _buffer.Add(b);
        }

        List<CivFrame>? frames = null;

        while (true)
        {
            // Hunt for the FE FE preamble, discarding what precedes it.
            var start = IndexOfPreamble();
            if (start < 0)
            {
                // Keep at most one trailing 0xFE (it may be half a preamble).
                var keep = _buffer.Count > 0 && _buffer[^1] == CivConstants.Preamble ? 1 : 0;
                DiscardedByteCount += _buffer.Count - keep;
                _buffer.RemoveRange(0, _buffer.Count - keep);
                break;
            }

            if (start > 0)
            {
                DiscardedByteCount += start;
                _buffer.RemoveRange(0, start);
            }

            // Frame is complete only when the terminator has arrived.
            var end = _buffer.IndexOf(CivConstants.EndOfMessage);
            if (end < 0)
            {
                break;
            }

            // FE FE to from cmd ... FD → minimum length 6.
            if (end < 5)
            {
                // Terminator arrived before a full header: malformed. Drop
                // through the terminator and keep hunting.
                DiscardedByteCount += end + 1;
                _buffer.RemoveRange(0, end + 1);
                continue;
            }

            var data = new byte[end - 5];
            _buffer.CopyTo(5, data, 0, data.Length);
            var frame = new CivFrame(_buffer[2], _buffer[3], _buffer[4], data);
            _buffer.RemoveRange(0, end + 1);

            frames ??= new List<CivFrame>(1);
            frames.Add(frame);
        }

        return (IReadOnlyList<CivFrame>?)frames ?? Array.Empty<CivFrame>();
    }

    private int IndexOfPreamble()
    {
        for (var i = 0; i + 1 < _buffer.Count; i++)
        {
            if (_buffer[i] == CivConstants.Preamble && _buffer[i + 1] == CivConstants.Preamble)
            {
                return i;
            }
        }

        return -1;
    }
}
