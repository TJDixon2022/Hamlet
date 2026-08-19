using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// What the operator actually watches arrive, measured on real off-air audio.
/// </summary>
/// <remarks>
/// <para>**EVERY ACCURACY FIGURE THIS PROJECT HAS PUBLISHED WAS THE SETTLED
/// PASS'S.** The settled pass is the record kept afterwards; the leading edge is
/// the text that appears character by character while he is listening, and it is
/// what he reads at the radio. Its accuracy on the two real captures had never
/// been measured, so nobody could say whether a week of work on the transcript
/// had moved the thing he looks at.</para>
/// <para>These print rather than assert a bar. A number nobody has seen before
/// does not get a ratchet on its first sitting (§12.5) — what it gets is a place
/// where the next session can see whether it moved.</para>
/// </remarks>
public sealed class CwLeadingEdgeAccuracyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public CwLeadingEdgeAccuracyTests(ITestOutputHelper output) => _output = output;

    private (string Tip, string Settled) Read(string capture)
    {
        var path = Path.Combine(
            CwFixtureCatalogue.Folder, "..", "captured", capture + ".wav");

        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var tip = new List<CwCharacter>();
        var settled = new List<CwCharacter>();

        decoder.CharacterDecoded += c => tip.Add(c);
        decoder.CharacterSettled += c => settled.Add(c);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return (
            string.Concat(tip.Select(c => c.Text)).Trim(),
            string.Concat(settled.Select(c => c.Text)).Trim());
    }

    /// <summary>
    /// The longest run of the key that appears in the reading, in order.
    /// </summary>
    /// <remarks>
    /// **A GREEDY MATCH IS NOT GOOD ENOUGH HERE AND THE FIRST VERSION OF THIS WAS
    /// ONE.** The settled pass opens with two letters the key does not have, and a
    /// greedy walk then mis-anchors and reports three of forty-three for a reading
    /// that plainly carries `DOT NET`, `STATION` and `MESSAGE`. A longest common
    /// subsequence does not care where the reading starts, which is the whole
    /// difficulty with a decode that begins mid-acquisition.
    /// </remarks>
    private static int InOrder(string got, string key)
    {
        var table = new int[got.Length + 1, key.Length + 1];

        for (var i = 1; i <= got.Length; i++)
        {
            for (var j = 1; j <= key.Length; j++)
            {
                table[i, j] = got[i - 1] == key[j - 1]
                    ? table[i - 1, j - 1] + 1
                    : Math.Max(table[i - 1, j], table[i, j - 1]);
            }
        }

        return table[got.Length, key.Length];
    }

    /// <remarks>
    /// **THE TWO NUMBERS THE OPERATOR CARES ABOUT.** The ARRL bulletin at twenty
    /// words a minute, read by the leading edge and by the settled pass, against
    /// the answer key `CwFarnsworthTests` already carries.
    /// </remarks>
    [Fact]
    public void TheLeadingEdgeIsMeasuredOnTheBulletin()
    {
        var (tip, settled) = Read(CwFarnsworthTests.Bulletin);

        var key = CwFarnsworthTests.BulletinKey
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("<BT>", "", StringComparison.Ordinal);

        var tipFlat = tip.Replace(" ", "", StringComparison.Ordinal);
        var settledFlat = settled.Replace(" ", "", StringComparison.Ordinal);

        _output.WriteLine($"key      '{key}' ({key.Length})");
        _output.WriteLine(
            $"tip      '{tip}' — {InOrder(tipFlat, key)} of {key.Length} in order, "
            + $"{tipFlat.Length} emitted");
        _output.WriteLine(
            $"settled  '{settled}' — {InOrder(settledFlat, key)} of {key.Length} in "
            + $"order, {settledFlat.Length} emitted");

        Assert.True(tipFlat.Length > 0, "the leading edge read nothing at all");
    }

    /// <remarks>
    /// The second real capture, the one carrying `VA3VRR`. A callsign is the
    /// hardest thing on the air to read and the most costly to get wrong, so it is
    /// worth its own line.
    /// </remarks>
    [Fact]
    public void TheLeadingEdgeIsMeasuredOnTheCallsignCapture()
    {
        var (tip, settled) = Read("cw-2026-08-17-013347");

        _output.WriteLine($"tip      '{tip}'");
        _output.WriteLine($"settled  '{settled}'");

        _output.WriteLine(
            $"callsign in tip: {tip.Contains("VA3VRR", StringComparison.Ordinal)}, "
            + $"in settled: {settled.Contains("VA3VRR", StringComparison.Ordinal)}");

        Assert.True(tip.Length > 0, "the leading edge read nothing at all");
    }
}
