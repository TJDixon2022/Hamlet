using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE OWNER'S RECORDING READS** (work instruction 528, HM-DEC-232): thirty seconds of a real QSO on 7.0549 MHz at
/// 20:01 UTC on 2026-10-02, which Hamlet read as `FER CHET&lt;BT&gt; BESE7V E ■ &lt;SK&gt; KC4 Z GP DEWA`.
/// </summary>
/// <remarks>
/// <para>**THE ONE TEST THAT READS A RECORDING.** R88 bans reading recordings; the owner lifted it for this one file,
/// 2026-10-02: he made the recording for this and said yes. No other recording is read.</para>
/// <para>Read through the live path as the app wires it - the envelope detector, its pattern gate and the run reader -
/// at the radio's state on the sheet: CW, FIL2 500 Hz, pitch 600, AGC FAST.</para>
/// </remarks>
public sealed class TheOwnersRecordingReadsTests
{
    private const string Sent = "FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading and its marks are printed.</param>
    public TheOwnersRecordingReadsTests(ITestOutputHelper output) => _output = output;

    private static string Wav([System.Runtime.CompilerServices.CallerFilePath] string here = "")
        => Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "fixtures", "cw", "captured", "cw-2026-10-02-200157.wav");

    /// <summary>The recording read as the app reads it: the text, the characters, and every mark that stood.</summary>
    internal static (string Text, IReadOnlyList<CwCharacter> Characters, IReadOnlyList<CwMark> Marks) Read()
    {
        var audio = WavAudio.Read(Wav());
        var detector = new CwEnvelopeDetector(audio.SampleRate);

        detector.SetPassband(600, 500);

        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var sequence = 0L;
        var chunk = audio.SampleRate / 100;

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            detector.Process(audio.Samples.AsSpan(at, chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);
        }

        reader.Flush();

        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return (text, characters, detector.MarksSince(0).Marks);
    }

    /// <remarks>
    /// The letters read as sent, spaces ignored, and no space inside KC4ZGP. The text, the characters and every mark
    /// that stood are printed.
    /// </remarks>
    [Fact]
    public void TheOwnersRecordingReads()
    {
        var (text, characters, marks) = Read();

        _output.WriteLine($"sent `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`");
        _output.WriteLine($"read `{text}`");
        _output.WriteLine("characters: " + string.Concat(characters.Select(c => $"{c.Text}[{c.Pattern}]@{c.At.TotalSeconds:0.00} ")));

        CwMark? last = null;

        foreach (var m in marks.OrderBy(m => m.FromSeconds))
        {
            var gap = last is { } l ? (m.FromSeconds - l.ToSeconds) * 1000 : double.NaN;

            _output.WriteLine($"  mark {m.FromSeconds:0.000} s, {(m.ToSeconds - m.FromSeconds) * 1000:0} ms, gap before {gap:0} ms, {m.PitchHz:0} Hz, level {m.LevelDb:0.0} dB{(m.Fitted ? ", fitted" : string.Empty)}");
            last = m;
        }

        Assert.Equal(Sent, text.Replace(" ", string.Empty, StringComparison.Ordinal));
        Assert.Contains("KC4ZGP", text, StringComparison.Ordinal);
    }
}
