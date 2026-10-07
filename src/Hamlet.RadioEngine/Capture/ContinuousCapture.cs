using System.Globalization;

namespace Hamlet.RadioEngine.Capture;

/// <summary>One finished piece of a continuous capture (work instruction 549, task 1).</summary>
/// <param name="Number">Its number, from one.</param>
/// <param name="WavPath">The WAV.</param>
/// <param name="SheetPath">Its sheet.</param>
/// <param name="FirstSampleIndex">The audio clock's index of its first sample.</param>
/// <param name="Samples">How many samples it holds.</param>
/// <param name="MissingSamples">How many samples the audio clock skipped inside it, which were not written.</param>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="StartUtc">When its first sample was written.</param>
/// <param name="EndUtc">When it was closed.</param>
/// <param name="EndReason">Why the capture ended, on its last piece; null on every other.</param>
/// <param name="Notes">What was noted while it was written: triggers that fired, holes in the audio.</param>
public sealed record CapturePiece(
    int Number,
    string WavPath,
    string SheetPath,
    long FirstSampleIndex,
    long Samples,
    long MissingSamples,
    int SampleRate,
    DateTime StartUtc,
    DateTime EndUtc,
    string? EndReason,
    IReadOnlyList<string> Notes)
{
    /// <summary>How long it is.</summary>
    public double Seconds => SampleRate > 0 ? Samples / (double)SampleRate : 0;
}

/// <summary>
/// **A LONG RECORDING IN BACK-TO-BACK PIECES, NOT ONE SAMPLE LOST BETWEEN THEM** (work instruction 549, task 1).
/// </summary>
/// <remarks>
/// <para>**WRITTEN FROM THE AUDIO AS IT ARRIVES, NEVER FROM A RING.** A ring is a window that slides; reading it every five
/// minutes would cut where the reads fell, and a read that came late would lose what slid out. Here every chunk the capture
/// is handed is written, and a piece ends exactly at its sample count: a chunk that straddles the boundary is split, its
/// first part closing one piece and the rest opening the next. So the next piece's first sample is the audio clock's very
/// next index, and a test can add the pieces up.</para>
/// <para>**A HOLE IS SAID, NOT FILLED.** A chunk whose index is past where the last one ended follows audio the capture never
/// got. Nothing is written in its place, since silence written there would read as a key-up; the piece's sheet says how
/// many samples were missing and where.</para>
/// <para>**ONE WRITER.** <see cref="Append"/> is called from one thread at a time; <see cref="Note"/> and <see cref="End"/>
/// may come from another, and all three take the same lock.</para>
/// </remarks>
public sealed class ContinuousCapture
{
    /// <summary>How long a piece is: five minutes, the owner's figure in work instruction 549.</summary>
    public const int PieceSeconds = 300;

    private readonly object _gate = new();
    private readonly Func<DateTime> _clock;
    private readonly Func<CapturePiece, string> _sheet;
    private readonly Func<long, string?>? _refusal;
    private readonly int _pieceSeconds;
    private readonly List<CapturePiece> _pieces = new();

    private WavPieceWriter? _writer;
    private List<string> _notes = new();
    private long _pieceFirst;
    private long _pieceMissing;
    private DateTime _pieceStart;
    private long _expectedNext = -1;

    /// <summary>A capture into a folder.</summary>
    /// <param name="folder">Where its pieces go; made when the first is written.</param>
    /// <param name="clock">The clock, in UTC.</param>
    /// <param name="sheet">Composes a piece's sheet when it closes.</param>
    /// <param name="refusal">
    /// Asked before each piece is opened with the bytes it will take; a reason ends the capture with it (task 4's disk keeper). Null
    /// asks nobody.
    /// </param>
    /// <param name="pieceSeconds">How long a piece is.</param>
    public ContinuousCapture(
        string folder,
        Func<DateTime> clock,
        Func<CapturePiece, string> sheet,
        Func<long, string?>? refusal = null,
        int pieceSeconds = PieceSeconds)
    {
        Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        _refusal = refusal;
        _pieceSeconds = Math.Max(1, pieceSeconds);
        StartedUtc = clock();
    }

    /// <summary>Where the pieces go.</summary>
    public string Folder { get; }

    /// <summary>When the capture began.</summary>
    public DateTime StartedUtc { get; }

    /// <summary>The piece being written, from one; the next one's number before any audio arrives.</summary>
    public int PieceNumber
    {
        get { lock (_gate) { return _pieces.Count + 1; } }
    }

    /// <summary>The pieces finished so far.</summary>
    public IReadOnlyList<CapturePiece> Pieces
    {
        get { lock (_gate) { return _pieces.ToList(); } }
    }

    /// <summary>Why it ended, or null while it runs.</summary>
    public string? EndReason { get; private set; }

    /// <summary>Whether it has ended.</summary>
    public bool Ended => EndReason is not null;

    /// <summary>Bytes a piece of <paramref name="seconds"/> at <paramref name="sampleRate"/> takes: 16 bits a sample and a header.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="seconds">Its length.</param>
    /// <returns>Bytes.</returns>
    public static long PieceBytes(int sampleRate, int seconds) => 44 + (2L * sampleRate * seconds);

    /// <summary>Write a chunk the audio clock placed at <paramref name="firstSampleIndex"/>.</summary>
    /// <param name="firstSampleIndex">The audio clock's index of its first sample.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="samples">The samples.</param>
    public void Append(long firstSampleIndex, int sampleRate, ReadOnlySpan<float> samples)
    {
        lock (_gate)
        {
            if (Ended || samples.IsEmpty || sampleRate <= 0)
            {
                return;
            }

            if (_expectedNext >= 0 && firstSampleIndex != _expectedNext)
            {
                var gap = firstSampleIndex - _expectedNext;

                if (_writer is not null && gap > 0)
                {
                    _pieceMissing += gap;
                    _notes.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "hole       {0} samples ({1:0} ms) never arrived before sample {2}; nothing was written in their place",
                        gap,
                        gap * 1000.0 / sampleRate,
                        firstSampleIndex));
                }
            }

            _expectedNext = firstSampleIndex + samples.Length;

            var index = firstSampleIndex;
            var pieceSamples = (long)sampleRate * _pieceSeconds;

            while (!samples.IsEmpty && !Ended)
            {
                if (_writer is null && !Open(index, sampleRate))
                {
                    return;
                }

                var room = (int)Math.Min(samples.Length, pieceSamples - _writer!.Samples);

                _writer.Write(samples[..room]);
                samples = samples[room..];
                index += room;

                if (_writer.Samples >= pieceSamples)
                {
                    Close(null);
                }
            }
        }
    }

    /// <summary>Note a line on the piece being written, or the next one if none is open.</summary>
    /// <param name="line">The line.</param>
    public void Note(string line)
    {
        lock (_gate)
        {
            if (!Ended)
            {
                _notes.Add(line);
            }
        }
    }

    /// <summary>End the capture, saying why on its last sheet.</summary>
    /// <param name="reason">Why.</param>
    public void End(string reason)
    {
        lock (_gate)
        {
            if (Ended)
            {
                return;
            }

            if (_writer is not null)
            {
                Close(reason);
            }
            else if (_pieces.Count > 0)
            {
                // The last piece closed exactly on its boundary; its sheet is rewritten to carry why it was the last.
                var last = _pieces[^1] with { EndReason = reason, Notes = _pieces[^1].Notes.Concat(_notes).ToList() };

                _pieces[^1] = last;
                WriteSheet(last);
            }

            EndReason = reason;
        }
    }

    private bool Open(long index, int sampleRate)
    {
        if (_refusal?.Invoke(PieceBytes(sampleRate, _pieceSeconds)) is { } refused)
        {
            EndReason = refused;

            if (_pieces.Count > 0)
            {
                var last = _pieces[^1] with { EndReason = EndReason };

                _pieces[^1] = last;
                WriteSheet(last);
            }

            return false;
        }

        var number = _pieces.Count + 1;

        _writer = new WavPieceWriter(Path.Combine(Folder, $"piece-{number:00}.wav"), sampleRate);
        _pieceFirst = index;
        _pieceMissing = 0;
        _pieceStart = _clock();

        return true;
    }

    private void Close(string? reason)
    {
        var writer = _writer!;

        writer.Close();
        _writer = null;

        var number = _pieces.Count + 1;
        var piece = new CapturePiece(
            number,
            writer.FilePath,
            Path.Combine(Folder, $"piece-{number:00}.txt"),
            _pieceFirst,
            writer.Samples,
            _pieceMissing,
            writer.SampleRate,
            _pieceStart,
            _clock(),
            reason,
            _notes);

        _notes = new List<string>();
        _pieces.Add(piece);
        WriteSheet(piece);
    }

    private void WriteSheet(CapturePiece piece)
    {
        try
        {
            File.WriteAllText(piece.SheetPath, _sheet(piece));
        }
        catch (Exception)
        {
            // Never-throw (§8): a sheet that cannot be written loses the sheet, and the audio is still on disk.
        }
    }
}
